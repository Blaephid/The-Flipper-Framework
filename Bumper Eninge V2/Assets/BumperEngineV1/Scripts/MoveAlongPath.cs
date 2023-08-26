using UnityEngine;
using System.Collections;
//using Luminosity.IO;

namespace SplineMesh
{
    //[RequireComponent(typeof(Spline))]
    public class MoveAlongPath : MonoBehaviour
    {
        CharacterTools Tools;

        PlayerBhysics Player;
        PlayerBinput Input;
        Animator CharacterAnimator;
        SonicSoundsControl Sounds;
        Quaternion CharRot;
        Pathers_Interaction Path_Int;
        ActionManager Actions;
        HedgeCamera Cam;

   

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

        private void Awake()
        {
            if (Player == null)
            {
                Tools = GetComponent<CharacterTools>();
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

            PlayerSpeed = Player.rb.velocity.magnitude;

            if (PlayerSpeed < speed)
                PlayerSpeed = speed;
            
            backwards = back;

            InitialRot = transform.rotation;
            CharacterAnimator.SetBool("Grounded", true);

            OriginalRayToGroundRot = Player.RayToGroundRotDistancecor;
            OriginalRayToGround = Player.RayToGroundDistancecor;

            Player.RayToGroundDistancecor = Player.RayToGroundDistancecor * 4f;
            Player.RayToGroundRotDistancecor = Player.RayToGroundRotDistancecor * 4f;

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
            CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
            CharacterAnimator.SetFloat("GroundSpeed", Player.rb.velocity.magnitude);
            CharacterAnimator.SetBool("Grounded", Player.Grounded);
            

            //Set Animation Angle
            Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, Player.rb.velocity.y, Player.rb.velocity.z);
            Quaternion CharRot = Quaternion.LookRotation(VelocityMod, sample.up);
            CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);



        }


        public void PathMove()
        {
            //Increase the Amount of distance through the Spline by DeltaTime
            float ammount = (Time.deltaTime * PlayerSpeed);

            //Slowly increases player speed.
            if (PlayerSpeed < Player.TopSpeed || PlayerSpeed < PathTopSpeed)
            {
                if (PlayerSpeed < Player.TopSpeed * 0.7f)
                    PlayerSpeed += .14f;
                else
                    PlayerSpeed += .07f;

            }

            //Simple Slope effects
            if (Player.GroundNormal.y < 1 && Player.GroundNormal != Vector3.zero)
            {
                //UpHill
                if (Player.rb.velocity.y > 0f)
                {
                    PlayerSpeed -= (1f - Player.GroundNormal.y) * 0.1f;
                }

                //DownHill
                if (Player.rb.velocity.y < 0f)
                {
                    PlayerSpeed += (1f - Player.GroundNormal.y) * 0.1f;
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
                Quaternion rot = (Quaternion.FromToRotation(Skin.transform.up, sample.Rotation * Vector3.up) * Skin.rotation);
                Skin.rotation = rot;
                CharacterAnimator.transform.rotation = rot;

                Vector3 FootPos = transform.position - Path_Int.feetPoint.position;
                transform.position = ((sample.location) + PathTransform.position) + FootPos;

                //Moves the player to the position of the Upreel
                //Vector3 HandPos = transform.position - HandGripPoint.position;
                //transform.position = currentUpreel.HandleGripPos.position + HandPos;


                //Set Player Speed correctly for smoothness
                if (!backwards)
                {
                    Player.rb.velocity = sample.tangent * (PlayerSpeed);


                    //remove camera tracking at the end of the path to be safe from strange turns
                    //if (range > Rail_int.RailSpline.Length * 0.9f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f;}
                }
                else
                {
                    Player.rb.velocity = -sample.tangent * (PlayerSpeed);

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
            }

        }

        public void ExitPath()
        {
            CharacterAnimator.transform.rotation = Quaternion.LookRotation(Player.rb.velocity);

            Player.RayToGroundDistancecor = OriginalRayToGround;
            Player.RayToGroundRotDistancecor = OriginalRayToGroundRot;
            Input.LockInputForAWhile(30f, true);

            //Reenables Colliders
            //Path_Int.playerCol.enabled = true;


            //Deactivates any cinemachine that might be attached.
            if (Path_Int.currentCam != null)
            {
                Path_Int.currentCam.DeactivateCam(18);
                Path_Int.currentCam = null;
            }

            Actions.ChangeAction(0);
        }

        void AssignStats()
        {

        }

        void AssignTools()
        {
            Actions = GetComponent<ActionManager>();
            Input = GetComponent<PlayerBinput>();
            Path_Int = GetComponent<Pathers_Interaction>();
            Player = GetComponent<PlayerBhysics>();

            CharacterAnimator = Tools.CharacterAnimator;
            Sounds = Tools.SoundControl;
            Cam = GetComponent<CameraControl>().Cam;
            Skin = Tools.mainSkin;
        }

    }
    
}
