using UnityEngine;
using System.Collections;
using Luminosity.IO;

namespace SplineMesh
{
    [RequireComponent(typeof(Spline))]
    public class Action05_Rail : MonoBehaviour
    {
        PlayerBhysics Player;
        PlayerBinput Input;
        Animator CharacterAnimator;
        SonicSoundsControl Sounds;
        Quaternion CharRot;
        Rail_Interaction Rail_int;
        ActionManager Actions;
        HedgeCamera Cam;
        public Transform pulley { get; set; }
        public Rigidbody ZipBody { get; set; }

        bool isZipLine;

        [Header("Skin Rail Params")]

        Transform Skin;
        Vector3 OGSkinLocPos;

        float skinRotationSpeed;
        public Vector3 SkinOffsetPosRail = new Vector3(0, -0.4f, 0);
        public Vector3 SkinOffsetPosZip = new Vector3(0, -0.4f, 0);
        public float OffsetRail = 2.05f;
        public float OffsetZip = -2.05f;

        float MinStartSpeed = 60f;
        float PushFowardmaxSpeed = 80f;
        float PushFowardIncrements = 15f;
        float PushFowardDelay = 0.5f;
        float SlopePower = 2.5f;
        float UpHillMultiplier = 0.25f;
        float DownHillMultiplier = 0.35f;
        float UpHillMultiplierCrouching = 0.4f;
        float DownHillMultiplierCrouching = 0.6f;
        float DragVal = 0.0001f;
        float PlayerBrakePower = 0.95f;

        float curvePosSlope { get; set; }

        // Setting up Values
        float timer = 0f;
        private float range = 0f;
        Transform RailTransform;
        public bool OnRail { get; set; }
        float PlayerSpeed;
        bool backwards;
        int RailSound = 1;
        bool RailContactSound, isBraking, isSwitching;
        CurveSample sample;
        bool Crouching;
        //float rotYFix;
        //Quaternion rot;
        Quaternion InitialRot;

        //Camera testing
        public float TargetDistance = 10;
        public float CameraLerp = 10;


        private void Awake()
        {
            Actions = GetComponent<ActionManager>();
            Input = GetComponent<PlayerBinput>();
            Rail_int = GetComponent<Rail_Interaction>();
            Player = GetComponent<PlayerBhysics>();
            CharacterAnimator = Player.Resources.CharacterAnimator;
            Sounds = Player.Resources.Sounds;
            Skin = Player.Resources.SkinOffset;
            Cam = Player.Resources.MainCamera;

            FetchValues();
            RailContactSound = false;
            OGSkinLocPos = Skin.transform.localPosition;

        }

        private void OnDisable()
        {
            OnRail = false;
            isZipLine = false;
            ZipBody = null;
            isBraking = false;
            RailContactSound = false;
            Physics.IgnoreLayerCollision(8, 23, false);
            Sounds.RailSoundStop();
            if (Skin != null)
            {
                Skin.transform.localPosition = OGSkinLocPos;
                Skin.localRotation = Quaternion.identity;
            }
            if (Actions.Action08.selectedAction == SpecialAction.Metal_Boost)
            {
                Actions.Action08.OnDisable();
            }
        }


        public void InitialEvents(float Range, Transform RailPos, bool isZip)
        {
            //ignore further railcollisions
            Physics.IgnoreLayerCollision(8, 23, true);


            isZipLine = isZip;
            timer = PushFowardDelay;
            RailContactSound = false;

            //Animations and Skin Changes
            CharacterAnimator.SetTrigger("GenericT");
            if (!isZipLine)
            {
                Skin.transform.localPosition = Skin.transform.localPosition + SkinOffsetPosRail;
            }
            else
            {

                Skin.transform.localPosition = Skin.transform.localPosition + SkinOffsetPosZip;
            }

            //fix for camera jumping
            //rotYFix = transform.rotation.eulerAngles.y;
            //transform.rotation = Quaternion.identity;
            if (transform.eulerAngles.y < -89)
            {
                Player.transform.eulerAngles = new Vector3(0, -89, 0);
            }


            //Setting up Rails
            range = Range;
            RailTransform = RailPos;
            OnRail = true;
            PlayerSpeed = Player.p_rigidbody.velocity.magnitude;
            CurveSample sample = Rail_int.RailSpline.GetSampleAtDistance(range);
            float dotdir = Vector3.Dot(Player.p_rigidbody.velocity.normalized, sample.tangent);
            Crouching = false;

            InitialRot = transform.rotation;



            // Check if was Homingattack
            if (Actions.Action == 2)
            {
                PlayerSpeed = Actions.Action02.LateSpeed;
                dotdir = Vector3.Dot(Actions.Action02.TargetDirection.normalized, sample.tangent);
                //Debug.Log(dotdir);
            }

            //Cam.CamLagSet(4);
            Cam.OverrideTarget.position = Skin.position;

            // make sure that Player wont have a shitty Speed...
            if (dotdir > 0.85f || dotdir < -.85f)
            {
                PlayerSpeed = Mathf.Abs(PlayerSpeed * 1);
            }
            else if (dotdir < 0.5 && dotdir > -.5f)
            {
                PlayerSpeed = Mathf.Abs(PlayerSpeed * 0.5f);
            }
            PlayerSpeed = Mathf.Max(PlayerSpeed, MinStartSpeed);

            // Get Direction for the Rail
            if (dotdir > 0)
            {
                backwards = false;
            }
            else
            {
                backwards = true;
            }


        }



        void FixedUpdate()
        {
            if (OnRail)
            {

                RailGrind();
            }
            else
            {

                Player.Resources.MainCamera.CamLagSet(0.8f, 0f);

                //Change Into Action 0
                CharacterAnimator.SetInteger("Action", 0);
                CharacterAnimator.SetBool("Grounded", Player.Grounded);
                Actions.ChangeAction(0);
                if (Actions.Action02 != null)
                {
                    Actions.Action02.HomingAvailable = true;
                }
                if (Actions.Action10 != null)
                {
                    Actions.Action10.IsWallJumping = false;
                }
                if (Actions.Action11 != null)
                {
                    Actions.Action11.CanPerform = true;
                }
            }

        }

        void Update()
        {
            SoundControl();
            //CameraFocus();
            //Set Animator Parameters
            if (!isZipLine)
            {
                CharacterAnimator.SetInteger("Action", 5);
            }
            else
            {
                CharacterAnimator.SetInteger("Action", 51);
            }
            CharacterAnimator.SetFloat("YSpeed", Player.p_rigidbody.velocity.y);
            CharacterAnimator.SetFloat("GroundSpeed", Player.p_rigidbody.velocity.magnitude);
            CharacterAnimator.SetBool("Grounded", Player.Grounded);

            //Set Animation Angle
            Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, Player.p_rigidbody.velocity.y, Player.p_rigidbody.velocity.z);
            Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
            CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
            if (isZipLine) pulley.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);



            // Actions Goes Here
            if (!Actions.isPaused)
            {
                timer += Time.deltaTime;

                if (InputManager.GetButtonDown("A-Jump", Actions.Player_ID))
                {
                    Player.Resources.MainCamera.CamLagSet(0.8f, 0f);

                    Player.GroundNormal = sample.up;
                    Vector3 jumpCorrectedOffset = (Skin.up * 3f); //Quaternion.LookRotation(Player.p_rigidbody.velocity, transform.up) * (transform.forward * 3.5f);
                    if (isZipLine) jumpCorrectedOffset = -jumpCorrectedOffset;
                    Player.transform.position += jumpCorrectedOffset;
                    Actions.Action01.InitialEvents();
                    OnRail = false;
                    if (isZipLine) ZipBody.isKinematic = true;
                    isZipLine = false;


                    //Player.transform.rotation = InitialRot;

                    //Player.transform.eulerAngles = new Vector3(0,1,0);
                    Actions.ChangeAction(1);
                    if (Actions.Action02 != null)
                    {
                        Actions.Action02.HomingAvailable = true;
                    }
                    if (Actions.Action10 != null)
                    {
                        Actions.Action10.IsWallJumping = false;
                    }
                    if (Actions.Action11 != null)
                    {
                        Actions.Action11.CanPerform = true;
                    }

                }


                if (InputManager.GetButton("R1-Roll", Actions.Player_ID) && !isZipLine)
                {
                    //Crouch
                    Crouching = true;
                    CharacterAnimator.SetBool("isRolling", true);
                }
                else
                {
                    Crouching = false;
                    CharacterAnimator.SetBool("isRolling", false);
                }

                if (InputManager.GetButtonDown("B-Bounce", Actions.Player_ID) && !isZipLine)
                {
                    //ChangeSide

                    if (timer > PushFowardDelay)
                    {
                        Sounds.RailSoundStop();
                        isSwitching = true;
                        if (PlayerSpeed < PushFowardmaxSpeed)
                        {
                            PlayerSpeed += PushFowardIncrements;
                        }
                        CharacterAnimator.SetTrigger("GenericT3");
                        timer = 0f;
                    }
                }
                isSwitching = (timer < PushFowardDelay);

                if (InputManager.GetButton("X-Stomp", Actions.Player_ID) && Time.timeScale != 0 && !isZipLine)
                {
                    if (Actions.Action08.selectedAction == SpecialAction.Metal_Boost)
                    {
                        if (Objects_Interaction.RingAmount > 0)
                        {
                            if (InputManager.GetButtonDown("X-Stomp", Actions.Player_ID)) Actions.Action08.Boost_InitialEvents();
                            Actions.Action08.Boost_Update(true);
                            PlayerSpeed += (Actions.Action08.BoostRail * Time.deltaTime);
                        }
                    }
                    else
                    {
                        isBraking = true;
                    }

                }
                else
                {
                    isBraking = false;
                }
                if (InputManager.GetButtonUp("X-Stomp", Actions.Player_ID) && Time.timeScale != 0 && !isZipLine)
                {
                    if (Actions.Action08.selectedAction == SpecialAction.Metal_Boost)
                    {
                        if (Objects_Interaction.RingAmount > 0)
                        {
                            Actions.Action08.OnDisable();
                        }
                    }
                }
            }
        }


        public void RailGrind()
        {



            //Increase the Amount of distance trought the Spline by DeltaTime
            float ammount = (Time.deltaTime * PlayerSpeed);

            //Check for Low Speed to change direction so player dont get stuck
            if (PlayerSpeed < 10)
            {
                if (!backwards)
                {
                    backwards = true;
                    PlayerSpeed = 12;
                    ammount = (Time.deltaTime * PlayerSpeed);
                }
                else
                {
                    backwards = false;
                    PlayerSpeed = 12;
                    ammount = (Time.deltaTime * PlayerSpeed);

                }

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
            if (range < Rail_int.RailSpline.Length && range > 0)
            {
                //Get Sample of the Rail to put player
                sample = Rail_int.RailSpline.GetSampleAtDistance(range);

                //Set player Position and rotation on Rail
                if (!isZipLine)
                {
                    Quaternion rot = (Quaternion.FromToRotation(Skin.transform.up, sample.Rotation * Vector3.up) * Skin.rotation);
                    Skin.rotation = rot;
                    transform.position = (sample.location) + RailTransform.position + ((sample.Rotation * transform.up * OffsetRail));

                }
                else
                {
                    transform.rotation = (Quaternion.FromToRotation(transform.up, sample.up) * transform.rotation);
                    pulley.transform.rotation = (Quaternion.FromToRotation(transform.up, sample.up) * transform.rotation);
                    Vector3 correctedOffset = Quaternion.LookRotation(Player.p_rigidbody.velocity, transform.up) * (transform.up * OffsetZip);
                    transform.position = sample.location + RailTransform.position + correctedOffset;
                    pulley.transform.position = sample.location + RailTransform.position;
                }

                //Add Physics
                SlopePhys();

                if (isBraking && PlayerSpeed > MinStartSpeed) PlayerSpeed *= PlayerBrakePower;

                //Debug.DrawRay(transform.position, sample.tangent * 10f,Color.black);

                //Set Player Speed correctly so that it becomes smooth grinding
                if (!backwards)
                {
                    Player.p_rigidbody.velocity = sample.tangent * (PlayerSpeed);
                    if (isZipLine && ZipBody != null) ZipBody.velocity = sample.tangent * (PlayerSpeed);

                    //remove camera tracking at the end of the rail to be safe from strange turns
                    //if (range > Rail_int.RailSpline.Length * 0.9f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f;}
                }
                else
                {
                    Player.p_rigidbody.velocity = -sample.tangent * (PlayerSpeed);
                    if (isZipLine && ZipBody != null) ZipBody.velocity = -sample.tangent * (PlayerSpeed);
                    //remove camera tracking at the end of the rail to be safe from strange turns
                    //if (range < 0.1f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f; }
                }

            }
            else
            {

                //Check if the Spline is loop and resets position
                if (Rail_int.RailSpline.IsLoop)
                {
                    if (!backwards)
                    {
                        range = range - Rail_int.RailSpline.Length;
                        RailGrind();
                    }
                    else
                    {
                        range = range + Rail_int.RailSpline.Length;
                        RailGrind();
                    }
                }
                else
                {
                    if (isZipLine) pulley.GetComponent<CapsuleCollider>().enabled = false;
                    OnRail = false;
                    isZipLine = false;
                    ZipBody = null;


                }
            }

        }
        void SlopePhys()
        {

            //slope curve from Bhys
            curvePosSlope = Player.curvePosSlope;
            float v = Input.InputExporter.y;
            v = (v + 1) / 2;
            //use player vertical speed to find if player is going up or down
            //Debug.Log(Player.p_rigidbody.velocity.normalized.y);
            if (Player.p_rigidbody.velocity.y >= -3f)
            {
                //uphill and straight
                float lean = UpHillMultiplier;
                if (Crouching) { lean = UpHillMultiplierCrouching; }
                //Debug.Log("UpHill : *" + lean);
                float force = (SlopePower * curvePosSlope) * lean;
                //Debug.Log(Mathf.Abs(Player.p_rigidbody.velocity.normalized.y - 1));
                float AbsYPow = Mathf.Abs(Player.p_rigidbody.velocity.normalized.y * Player.p_rigidbody.velocity.normalized.y);
                //Debug.Log( "Val" + Player.p_rigidbody.velocity.normalized.y + "Pow" + AbsYPow);
                force = (AbsYPow * force) + (DragVal * PlayerSpeed);
                //Debug.Log(force);
                PlayerSpeed += force;

                //Enforce max Speed
                if (PlayerSpeed > Player.MaxSpeed)
                {
                    PlayerSpeed = Player.MaxSpeed;
                }
            }
            else
            {
                //Downhill
                float lean = DownHillMultiplier;
                if (Crouching) { lean = DownHillMultiplierCrouching; }
                //Debug.Log("DownHill : *" + lean);
                float force = (SlopePower * curvePosSlope) * lean;
                //Debug.Log(Mathf.Abs(Player.p_rigidbody.velocity.normalized.y));
                float AbsYPow = Mathf.Abs(Player.p_rigidbody.velocity.normalized.y * Player.p_rigidbody.velocity.normalized.y);
                //Debug.Log("Val" + Player.p_rigidbody.velocity.normalized.y + "Pow" + AbsYPow);
                force = (AbsYPow * force) - (DragVal * PlayerSpeed);
                //Debug.Log(force);
                PlayerSpeed -= force;

                //Enforce max Speed
                if (PlayerSpeed > Player.MaxSpeed)
                {
                    PlayerSpeed = Player.MaxSpeed;
                }
            }

        }

        void SoundControl()
        {
            //Player Rail Sound

            //If Entring Rail
            if (!RailContactSound)
            {
                RailSound = !(isZipLine) ? 0 : 10;
                RailContactSound = true;
            }
            else
            {
                RailSound = !(isZipLine) ? 1 : 11;
            }

            if (!isSwitching)
            {
                Sounds.RailSound(RailSound);
            }
        }

        void CameraFocus()
        {
            Cam.OverrideTarget.position = Vector3.Lerp(Cam.OverrideTarget.position, transform.position + (sample.up * TargetDistance), Time.deltaTime * CameraLerp);
            Cam.TargetOverriden = true;
        }


        void FetchValues()
        {
            CharacterStatsHolder chr = Player.Resources.CharacterStats;

            skinRotationSpeed = Player.Resources.skinRotationSpeed;

            MinStartSpeed = chr.RailMinStartSpeed;
            PushFowardmaxSpeed = chr.RailPushFowardmaxSpeed;
            PushFowardIncrements = chr.RailPushFowardIncrements;
            PushFowardDelay = chr.RailPushFowardDelay;
            SlopePower = chr.SlopePower;
            UpHillMultiplier = chr.RailUpHillMultiplier;
            DownHillMultiplier = chr.RailDownHillMultiplier;
            UpHillMultiplierCrouching = chr.RailUpHillMultiplierCrouching;
            DownHillMultiplierCrouching = chr.RailDownHillMultiplierCrouching;
            DragVal = chr.RailDragVal;
            PlayerBrakePower = chr.railPlayerBrakePower;
        }

    }

}
