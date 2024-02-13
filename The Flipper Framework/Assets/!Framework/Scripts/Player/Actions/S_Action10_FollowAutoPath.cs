using UnityEngine;
using System.Collections;
//using Luminosity.IO;

namespace SplineMesh
{
    //[RequireComponent(typeof(Spline))]
    public class S_Action10_FollowAutoPath : MonoBehaviour
    {
        S_CharacterTools Tools;

        S_PlayerPhysics Player;
        S_PlayerInput Input;
        Animator CharacterAnimator;
        S_Control_SoundsPlayer Sounds;
        Quaternion CharRot;
        S_Interaction_Pathers Path_Int;
        S_ActionManager Actions;
        S_HedgeCamera Cam;

   

        [Header("Skin Params")]

        Transform Skin;
        Vector3 OGSkinLocPos;

        public float skinRotationSpeed;
        public Vector3 SkinOffsetPos = new Vector3(0, -0.4f, 0);
        public float Offset = 2.05f;
        
    

        // Setting up Values
        private float range = 0f;
        Transform PathTransform;
        [HideInInspector] public float PlayerSpeed;
        float PathTopSpeed;
        bool backwards;

        CurveSample sample;
        
        //float rotYFix;
        //Quaternion rot;
        Quaternion InitialRot;

        //Camera testing
        public float TargetDistance = 10;
        public float CameraLerp = 10;

        [HideInInspector] public float OriginalRayToGroundRot;
        [HideInInspector] public float OriginalRayToGround;

        [HideInInspector] public Collider patherStarter;

        private void Awake()
        {
            if (Player == null)
            {
                Tools = GetComponent<S_CharacterTools>();
                AssignTools();

                AssignStats();
            }

            OGSkinLocPos = Skin.transform.localPosition;

        }

        private void OnDisable()
        {  
            if (Skin != null)
            {
                Skin.transform.localPosition = OGSkinLocPos;
                Skin.localRotation = Quaternion.identity;
            }
        }


        public void InitialEvents(float Range, Transform PathPos, bool back, float speed, float pathspeed = 0f)
        {

            //Disable colliders to prevent jankiness
            //Path_Int.playerCol.enabled = false;


            Skin.transform.localPosition = Skin.transform.localPosition;


            //fix for camera jumping
            //rotYFix = transform.rotation.eulerAngles.y;
            //transform.rotation = Quaternion.identity;

            if (transform.eulerAngles.y < -89)
            {
                Player.transform.eulerAngles = new Vector3(0, -89, 0);
            }


            //Setting up the path to follow
            range = Range;
            PathTransform = PathPos;

            PathTopSpeed = pathspeed;

            PlayerSpeed = Player._RB.velocity.magnitude;

            if (PlayerSpeed < speed)
                PlayerSpeed = speed;
            
            backwards = back;

            InitialRot = transform.rotation;
            CharacterAnimator.SetBool("Grounded", true);

        }



        void FixedUpdate()
        {
            Input.LockInputForAWhile(1f, true);
            PathMove();
        }

        void Update()
        {
            //CameraFocus();

            //Set Animator Parameters as player is always running

            CharacterAnimator.SetInteger("Action", 0);
            CharacterAnimator.SetFloat("YSpeed", Player._RB.velocity.y);
            CharacterAnimator.SetFloat("GroundSpeed", Player._RB.velocity.magnitude);
            CharacterAnimator.SetBool("Grounded", Player._isGrounded);
            

            //Set Animation Angle
            Vector3 VelocityMod = new Vector3(Player._RB.velocity.x, Player._RB.velocity.y, Player._RB.velocity.z);
            Quaternion CharRot = Quaternion.LookRotation(VelocityMod, sample.up);
            CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);



        }


        public void PathMove()
        {
            //Increase the Amount of distance through the Spline by DeltaTime
            float ammount = (Time.deltaTime * PlayerSpeed);

            Player.AlignWithGround();

            //Slowly increases player speed.
            if (PlayerSpeed < Player._currentTopSpeed || PlayerSpeed < PathTopSpeed)
            {
                if (PlayerSpeed < Player._currentTopSpeed * 0.7f)
                    PlayerSpeed += .14f;
                else
                    PlayerSpeed += .07f;

            }

            //Simple Slope effects
            if (Player._groundNormal.y < 1 && Player._groundNormal != Vector3.zero)
            {
                //UpHill
                if (Player._RB.velocity.y > 0f)
                {
                    PlayerSpeed -= (1f - Player._groundNormal.y) * 0.1f;
                }

                //DownHill
                if (Player._RB.velocity.y < 0f)
                {
                    PlayerSpeed += (1f - Player._groundNormal.y) * 0.1f;
                }
            }

            //Leave path at low speed
            if (PlayerSpeed < 10f)
            {
                ExitPath();
            }

            // Increase/Decrease Range depending on direction

            if (!backwards)
            {
                //range += ammount / dist;
                range += ammount;
            }
            else
            {
                //range -= ammount / dist;
                range -= ammount;
            }

            //Check so for the size of the Spline
            if (range < Path_Int.RailSpline.Length && range > 0)
            {
                //Get Sample of the Path to put player
                sample = Path_Int.RailSpline.GetSampleAtDistance(range);

                //Set player Position and rotation on Path
                //Quaternion rot = (Quaternion.FromToRotation(Skin.transform.up, sample.Rotation * Vector3.up) * Skin.rotation);
                //Skin.rotation = rot;
                //CharacterAnimator.transform.rotation = rot;


                if ((Physics.Raycast(sample.location + (transform.up * 2), -transform.up, out RaycastHit hitRot, 2.2f + Tools.Stats.GreedysStickToGround.rayToGroundDistance, Player._Groundmask_)))
                {
                    //Vector3 FootPos = transform.position - Path_Int.feetPoint.position;
                    //transform.position = (hitRot.point + PathTransform.position) + FootPos;

                    Vector3 FootPos = transform.position - Path_Int.feetPoint.position;
                    transform.position = ((sample.location) + PathTransform.position) + FootPos;
                }
                else
                {
                    Vector3 FootPos = transform.position - Path_Int.feetPoint.position;
                    transform.position = ((sample.location) + PathTransform.position) + FootPos;
                }

            

                //Moves the player to the position of the Upreel
                //Vector3 HandPos = transform.position - HandGripPoint.position;
                //transform.position = currentUpreel.HandleGripPos.position + HandPos;


                //Set Player Speed correctly for smoothness
                if (!backwards)
                {
                    Player._RB.velocity = sample.tangent * (PlayerSpeed);


                    //remove camera tracking at the end of the path to be safe from strange turns
                    //if (range > Rail_int.RailSpline.Length * 0.9f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f;}
                }
                else
                {
                    Player._RB.velocity = -sample.tangent * (PlayerSpeed);

                    //remove camera tracking at the end of the path to be safe from strange turns
                    //if (range < 0.1f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f; }
                }

            }
            else
            {
                //Check if the Spline is loop and resets position
                if (Path_Int.RailSpline.IsLoop)
                {
                    if (!backwards)
                    {
                        range = range - Path_Int.RailSpline.Length;
                        PathMove();
                    }
                    else
                    {
                        range = range + Path_Int.RailSpline.Length;
                        PathMove();
                    }
                }
                else
                {
                    ExitPath();
                }
            }

        }

        public void ExitPath()
        {
            

            Player.AlignWithGround();

            //Set Player Speed correctly for smoothness
            if (!backwards)
            {
                sample = Path_Int.RailSpline.GetSampleAtDistance(Path_Int.RailSpline.Length);
                Player._RB.velocity = sample.tangent * (PlayerSpeed);

            }
            else
            {
                sample = Path_Int.RailSpline.GetSampleAtDistance(0);
                Player._RB.velocity = -sample.tangent * (PlayerSpeed);

            }

            CharacterAnimator.transform.rotation = Quaternion.LookRotation(Player._RB.velocity, Player.hitGround.normal);

            Player.GetComponent<S_Handler_Camera>().Cam.setBehind();
            Input.LockInputForAWhile(30f, true);

            //Reenables Colliders
            //Path_Int.playerCol.enabled = true;


            //Deactivates any cinemachine that might be attached.
            if (Path_Int.currentCam != null)
            {
                Path_Int.currentCam.DeactivateCam(18);
                Path_Int.currentCam = null;
            }

            Actions.ChangeAction(S_Enums.PlayerStates.Regular);
        }

        void AssignStats()
        {

        }

        void AssignTools()
        {
            Actions = GetComponent<S_ActionManager>();
            Input = GetComponent<S_PlayerInput>();
            Path_Int = GetComponent<S_Interaction_Pathers>();
            Player = GetComponent<S_PlayerPhysics>();

            CharacterAnimator = Tools.CharacterAnimator;
            Sounds = Tools.SoundControl;
            Cam = GetComponent<S_Handler_Camera>().Cam;
            Skin = Tools.mainSkin;
        }

    }
    
}
