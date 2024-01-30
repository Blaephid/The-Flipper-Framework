using UnityEngine;
using System.Collections;
//using Luminosity.IO;

namespace SplineMesh
{
    //[RequireComponent(typeof(Spline))]
    public class S_Action05_Rail : MonoBehaviour
    {
        S_PlayerPhysics Player;
        S_PlayerInput Input;
        Animator CharacterAnimator;
        S_Control_SoundsPlayer Sounds;
        Quaternion CharRot;
        [HideInInspector] public S_Interaction_Pathers Rail_int;

        GameObject jumpBall;

        S_CharacterTools Tools;
        S_ActionManager Actions;
        S_HedgeCamera Cam;
        public Transform ZipHandle { get; set; }
        public Rigidbody ZipBody { get; set; }

        [HideInInspector] public bool isZipLine;

        [Header("Skin Rail Params")]

        public float skinRotationSpeed;
        float OffsetRail = 2.05f;
        float OffsetZip = -2.05f;

        public LayerMask railMask;
        [HideInInspector] public float railmaxSpeed;
        float railTopSpeed;
        float decaySpeedLow;
        float decaySpeedHigh;
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
        float HopDelay;
        float hopDistance = 12;
        float decayTime;
        AnimationCurve accelBySpeed;
        float decaySpeed;

        float curvePosSlope { get; set; }

        // Setting up Values
        float timer = 0f;
        float PulleyRotate;
        [HideInInspector] public float range = 0f;
        Transform RailTransform;
        public bool OnRail { get; set; }
        public AddOnRail ConnectedRails;
        [HideInInspector] public float PlayerSpeed;
        [HideInInspector] public bool backwards;
        int RailSound = 1;
        bool RailContactSound, isBraking, isSwitching;
        CurveSample sample;
        bool Crouching;
        //float rotYFix;
        //Quaternion rot;
        Quaternion InitialRot;
        Vector3 setOffSet;

        //Camera testing
        public float TargetDistance = 10;
        public float CameraLerp = 10;


        //Stepping
        bool canInput = true;
        bool canHop = false;
        float distanceToStep;
        float stepSpeed = 3.5f;
        bool steppingRight;

        bool faceRight = true;

        //Boosters
        [HideInInspector] public bool Boosted;
        [HideInInspector] public float boostTime;

        private void Awake()
        {
            if (Player == null)
            {
                Tools = GetComponent<S_CharacterTools>();
                AssignTools();

                AssignStats();
            }

            RailContactSound = false;
            //OGSkinLocPos = Skin.transform.localPosition;

        }

        IEnumerator DelayCollision()
        {
            yield return new WaitForSeconds(0.1f);
            Physics.IgnoreLayerCollision(8, 23, false);
        }

        private void OnDisable()
        {

            Player.GravityAffects = true;

            OnRail = false;
            isZipLine = false;
            ZipBody = null;
            isBraking = false;
            RailContactSound = false;
            StartCoroutine(DelayCollision());
            ////////Sounds.RailSoundStop();
            ///

            Actions.RollPressed = false;
            Actions.SpecialPressed = false;
            Actions.BouncePressed = false;

            transform.rotation = Quaternion.identity;

            if(Player.rb.velocity != Vector3.zero)
                CharacterAnimator.transform.rotation = Quaternion.LookRotation(Player.rb.velocity, Vector3.up);

            //if (Skin != null)
            //{
            //    Skin.transform.localPosition = OGSkinLocPos;
            //    Skin.localRotation = Quaternion.identity;
            //}

        }

        public void InitialEvents(float Range, Transform RailPos, bool isZip, Vector3 thisOffset, AddOnRail addOn)
        {
            StartCoroutine(allowHop());

            //ignore further railcollisions
            Physics.IgnoreLayerCollision(this.gameObject.layer, 23, true);

            canInput = true;
            setOffSet = -thisOffset;

            ConnectedRails = addOn;
            jumpBall.SetActive(false);
            Actions.JumpPressed = false;

            isZipLine = isZip;
            timer = PushFowardDelay;
            RailContactSound = false;

            Boosted = false;
            boostTime = 0;

            Player.GravityAffects = false;
            // Player.p_rigidbody.useGravity = false;

            //Animations and Skin Changes
            //CharacterAnimator.SetTrigger("GenericT");

            if (!isZipLine)
            {
                CharacterAnimator.SetBool("GrindRight", faceRight);
                CharacterAnimator.SetInteger("Action", 10);
                

            }
            else
            {
                ZipHandle.GetComponentInChildren<MeshCollider>().enabled = false;
                CharacterAnimator.SetInteger("Action", 9);

            }

            CharacterAnimator.SetTrigger("HitRail");

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

            if(distanceToStep <= 0)
                PlayerSpeed = Player.SpeedMagnitude;

       

            CurveSample sample = Rail_int.RailSpline.GetSampleAtDistance(range);
            float dotdir = Vector3.Dot(Player.rb.velocity.normalized, sample.tangent);
            Crouching = false;
            PulleyRotate = 0f;

            InitialRot = transform.rotation;

            //Vector3 dir = sample.tangent;
            //Cam.SetCamera(dir, 2.5f, 20f, 1f);
            //Cam.Locked = false;


            // Check if was Homingattack
            if (Actions.Action == S_ActionManager.States.Homing)
            {
                PlayerSpeed = Actions.Action02.LateSpeed;
                dotdir = Vector3.Dot(Actions.Action02.TargetDirection.normalized, sample.tangent);
            }
            else if (Actions.Action == S_ActionManager.States.DropCharge)
            {
                //dotdir = Vector3.Dot(Player.rb.velocity.normalized, sample.tangent);

                float charge = Actions.Action08.externalDash();


                PlayerSpeed = Mathf.Clamp(charge, PlayerSpeed + (charge / 6), 160);
            }

            //Cam.CamLagSet(4);
            ////////Cam.OverrideTarget.position = Skin.position;

            // make sure that Player wont have a shitty Speed...
            if ((dotdir > 0.85f || dotdir < -.85f) && distanceToStep > 0)
            {
                PlayerSpeed = Mathf.Abs(PlayerSpeed * 1);
            }
            else if (dotdir < 0.5 && dotdir > -.5f)
            {
                PlayerSpeed = Mathf.Abs(PlayerSpeed * 0.8f);
            }
            PlayerSpeed = Mathf.Max(PlayerSpeed, MinStartSpeed);

            // Get Direction for the Rail
            if (dotdir > 0)
            {
                backwards = false;

                if (isZipLine && range > Rail_int.RailSpline.Length - 5)
                    backwards = true;
            }
            else
            {
                backwards = true;
                if (isZipLine && range < 5)
                    backwards = false;
            }


            Player.rb.velocity = Vector3.zero;

        }


        IEnumerator allowHop()
        {
            canHop = false;
            yield return new WaitForSeconds(HopDelay);
            canHop = true;
        }
        void FixedUpdate()
        {

            if (OnRail)
            {
                CharacterAnimator.SetBool("GrindRight", faceRight);
                RailGrind();
            }
            else
            {


                Actions.Action00.readyCoyote();
                CharacterAnimator.SetInteger("Action", 0);
                CharacterAnimator.SetBool("Grounded", Player.Grounded);

                Actions.ChangeAction(S_ActionManager.States.Regular);
                if (Actions.Action02 != null)
                {
                    Actions.Action02.HomingAvailable = true;
                }
            }

        }

        void Update()
        {
            SoundControl();
            //CameraFocus();
            //Set Animator Parameters
            //CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
            CharacterAnimator.SetFloat("GroundSpeed", PlayerSpeed / Player.MaxSpeed);
            CharacterAnimator.SetBool("Grounded", false);

            // Actions Go Here
            if (!Actions.isPaused && canInput)
            {
                InputHandling();

            }
        }

        void InputHandling()
        {
            timer += Time.deltaTime;

            if (Actions.JumpPressed)
            {
                //Cam.CamLagSet(0.8f, 0f);

                Vector3 jumpCorrectedOffset = (CharacterAnimator.transform.up * 1.5f); //Quaternion.LookRotation(Player.p_rigidbody.velocity, transform.up) * (transform.forward * 3.5f);


                if (isZipLine)
                {
                    if (!backwards)
                        Player.rb.velocity = sample.tangent * PlayerSpeed;
                    else
                        Player.rb.velocity = -sample.tangent * PlayerSpeed;

                    jumpCorrectedOffset = -jumpCorrectedOffset;
                    ZipBody.isKinematic = true;
                    Player.GroundNormal = new Vector3(0f, 1f, 0f);

                    StartCoroutine(Rail_int.JumpFromZipLine(ZipHandle, 1));

                }

                transform.position += jumpCorrectedOffset;

                OnRail = false;

                isZipLine = false;


                //Player.transform.rotation = InitialRot;

                //Player.transform.eulerAngles = new Vector3(0,1,0);
                Actions.Action01.jumpCount = -1;
                Actions.Action01.InitialEvents(sample.up, true, Player.rb.velocity.y);
                Actions.ChangeAction(S_ActionManager.States.Jump);

                if (Actions.Action02 != null)
                {
                    Actions.Action02.HomingAvailable = true;
                }

            }


            if (Actions.RollPressed && !isZipLine)
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

            if (Actions.SpecialPressed && !isZipLine)
            {
                //ChangeSide

                if (timer > PushFowardDelay)
                {
                    //Sounds.RailSoundStop();
                    isSwitching = true;
                    if (PlayerSpeed < PushFowardmaxSpeed)
                    {
                        PlayerSpeed += PushFowardIncrements + accelBySpeed.Evaluate(PlayerSpeed / PushFowardmaxSpeed);
                    }
                    faceRight = !faceRight;
                    timer = 0f;
                    Actions.SpecialPressed = false;
                }
            }
            isSwitching = (timer < PushFowardDelay);

            //If above a certain speed, the player breaks depending it they're presseing the skid button.
            if (Time.timeScale != 0 && !isZipLine)
            {
                isBraking = Actions.BouncePressed;

            }
            else
            {
                isBraking = false;
            }

        }

        public void RailGrind()
        {

            //Increase the Amount of distance trought the Spline by DeltaTime
            float ammount = (Time.deltaTime * PlayerSpeed);
            // Increase/Decrease Range depending on direction

            SlopePhys();

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
                    if (backwards)
                    {
                        CharacterAnimator.transform.rotation = Quaternion.LookRotation(-sample.tangent, sample.up);
                    }
                    else
                    {
                        CharacterAnimator.transform.rotation = Quaternion.LookRotation(sample.tangent, sample.up);
                    }

                    Vector3 binormal = Vector3.zero;

                    if (setOffSet != Vector3.zero)
                    {
                        //binormal = sample.tangent;
                        //binormal = Quaternion.LookRotation(Vector3.right, Vector3.up) * binormal;
                        binormal += sample.Rotation * -setOffSet;
                    }
                    transform.position = (sample.location + RailTransform.position + (sample.up * OffsetRail)) + binormal;

                    if (canHop)
                    {
                        railHopping();
                    }

                }
                else
                {
                    float rotatePoint = 0;
                    if (Actions.RightStepPressed)
                    {
                        Actions.LeftStepPressed = false;
                        rotatePoint = 1;
                    }
                    else if (Actions.LeftStepPressed)
                    {
                        Actions.RightStepPressed = false;
                        rotatePoint = -1;
                    }

                    if (!backwards)
                    {
                        PulleyRotate = Mathf.MoveTowards(PulleyRotate, rotatePoint, 3.5f * Time.deltaTime);
                        CharacterAnimator.transform.rotation = Quaternion.LookRotation(sample.tangent, sample.up);
                    }
                    else
                    { 
                        PulleyRotate = Mathf.MoveTowards(PulleyRotate, rotatePoint, 3.5f * Time.deltaTime);
                        CharacterAnimator.transform.rotation = Quaternion.LookRotation(-sample.tangent, sample.up);
                    }

                    ZipHandle.rotation = sample.Rotation;
                    ZipHandle.eulerAngles = new Vector3(ZipHandle.eulerAngles.x, ZipHandle.eulerAngles.y, ZipHandle.eulerAngles.z + PulleyRotate * 70f);

                    CharacterAnimator.transform.eulerAngles = new Vector3(CharacterAnimator.transform.eulerAngles.x, CharacterAnimator.transform.eulerAngles.y, CharacterAnimator.transform.eulerAngles.z + PulleyRotate * 70f);


                    

                   // Cam.FollowDirection(0.8f, 14, -5, 0.1f, true);
                    //CameraTarget.position = sample.location + RailTransform.position;
                   // CameraTarget.localRotation = Quaternion.LookRotation(CharacterAnimator.transform.forward, Vector3.up);



                    ZipHandle.transform.position = (sample.location + RailTransform.position) + setOffSet;
                    transform.position = ZipHandle.transform.position + (ZipHandle.transform.up * OffsetZip);


                }

                if (isBraking && PlayerSpeed > MinStartSpeed) PlayerSpeed *= PlayerBrakePower;

                //Set Player Speed correctly so that it becomes smooth grinding
                if (!backwards)
                {

                    if (isZipLine && ZipBody != null)
                    {
                        ZipBody.velocity = sample.tangent * (PlayerSpeed);
                        Player.rb.velocity = sample.tangent;
                    }
                    else
                        Player.rb.velocity = sample.tangent * (PlayerSpeed);

                    //remove camera tracking at the end of the rail to be safe from strange turns
                    //if (range > Rail_int.RailSpline.Length * 0.9f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f;}
                    if (range > Rail_int.RailSpline.Length * 0.9f)
                    {
                       // Cam.lockCamFor(0.5f);
                    }
                }
                else
                {

                    if (isZipLine && ZipBody != null)
                    {
                        ZipBody.velocity = -sample.tangent * (PlayerSpeed);
                        Player.rb.velocity = -sample.tangent;
                    }
                    else
                        Player.rb.velocity = -sample.tangent * (PlayerSpeed);
                    //remove camera tracking at the end of the rail to be safe from strange turns
                    //if (range < 0.1f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f; }
                    if (range > Rail_int.RailSpline.Length * 0.9f)
                    {
                      //  Cam.lockCamFor(0.5f);
                    }
                }

            }
            else
            {
                if(!backwards)
                    sample = Rail_int.RailSpline.GetSampleAtDistance(Rail_int.RailSpline.Length - 1);
                else
                    sample = Rail_int.RailSpline.GetSampleAtDistance(0);

                LoseRail();
            }

        }

        void railHopping()
        {
            if(canInput)
            {
                //Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
                if (Actions.RightStepPressed)
                {
                    Vector3 Direction = CharacterAnimator.transform.position - Cam.transform.position;
                    bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < -0.5f;
                    if (Facing)
                    {
                        Actions.RightStepPressed = false;
                        Actions.LeftStepPressed = true;
                    }
                }
                else if (Actions.LeftStepPressed)
                {
                    Vector3 Direction = CharacterAnimator.transform.position - Cam.transform.position;
                    bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < -0.5f;
                    if (Facing)
                    {
                        Actions.RightStepPressed = true;
                        Actions.LeftStepPressed = false;
                    }
                }

                Debug.DrawRay(transform.position - (sample.up * 2) + (CharacterAnimator.transform.right * 3), CharacterAnimator.transform.right * 10, Color.red);
                Debug.DrawRay(transform.position - (sample.up * 2) + (CharacterAnimator.transform.right * 3), -CharacterAnimator.transform.right * 10, Color.red);

                if (Actions.RightStepPressed)
                {
                  
                    distanceToStep = hopDistance;
                    canInput = false;
                    steppingRight = true;
                    Actions.RightStepPressed = false;
                    performStep();
                    return;

                }
                else if (Actions.LeftStepPressed)
                {
                    

                    distanceToStep = hopDistance;
                    canInput = false;
                    steppingRight = false;
                    Actions.LeftStepPressed = false;
                    performStep();
                    return;
                }
            }
 
            performStep();
        }

        void performStep()
        {
            if (distanceToStep > 0)
            {
                float move = stepSpeed;

                if (steppingRight)
                    move = -move;
                if(backwards)
                    move = -move;

                move = Mathf.Clamp(move, -distanceToStep, distanceToStep);

                setOffSet.Set(setOffSet.x + move, setOffSet.y, setOffSet.z);
               
                if(move < 0)
                    if(Physics.BoxCast(CharacterAnimator.transform.position,new Vector3(1.3f, 3f, 1.3f), -CharacterAnimator.transform.right, Quaternion.identity, 4, Tools.coreStats.StepLayerMask))
                    {
                        Actions.ChangeAction(S_ActionManager.States.Regular);
                        CharacterAnimator.SetInteger("Action", 0);
                    }
                else
                    if (Physics.BoxCast(CharacterAnimator.transform.position, new Vector3(1.3f, 3f, 1.3f), CharacterAnimator.transform.right, Quaternion.identity, 4, Tools.coreStats.StepLayerMask))
                    {
                        Actions.ChangeAction(S_ActionManager.States.Regular);
                        CharacterAnimator.SetInteger("Action", 0);
                    }

                distanceToStep -= stepSpeed;

                if (distanceToStep < 6)
                {
                    Physics.IgnoreLayerCollision(8, 23, false);

                    if (distanceToStep <= 0)
                    {
                        Actions.ChangeAction(S_ActionManager.States.Regular);
                        OnRail = false;
                        CharacterAnimator.SetInteger("Action", 0);
                    }

                }
            }
        }

        void LoseRail()
        {
            distanceToStep = 0;
            Physics.IgnoreLayerCollision(8, 23, true);

            Debug.Log("The Rail Is Over");

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
            else if (ConnectedRails != null && ((!backwards && ConnectedRails.nextRail != null && ConnectedRails.nextRail.isActiveAndEnabled) || (backwards && ConnectedRails.PrevRail != null && ConnectedRails.PrevRail.isActiveAndEnabled)))
            {
                if (!backwards && ConnectedRails.nextRail != null)
                {
                    Debug.Log("On to Next Rail With = " +range);
                    //Debug.Log("Set by " + ConnectedRails.nextRail);

                    range = range - Rail_int.RailSpline.Length;
                    range = 0;

                    ConnectedRails.Announce();

                    ConnectedRails = ConnectedRails.nextRail;
                    setOffSet.Set(-ConnectedRails.GetComponent<ExampleSower>().Offset3d.x, 0, 0);

                    Rail_int.RailSpline = ConnectedRails.GetComponentInParent<Spline>();
                    RailTransform = Rail_int.RailSpline.transform.parent;

                    Debug.Log("Then With = " + range);
                }
                else if (backwards && ConnectedRails.PrevRail != null)
                {

                    Debug.Log("Back To Previous Rail");

                    AddOnRail temp = ConnectedRails;
                    ConnectedRails = ConnectedRails.PrevRail;
                    setOffSet.Set(-ConnectedRails.GetComponent<ExampleSower>().Offset3d.x, 0, 0);

                    Rail_int.RailSpline = ConnectedRails.GetComponentInParent<Spline>();
                    RailTransform = Rail_int.RailSpline.transform.parent;

                    range = range + Rail_int.RailSpline.Length;
                    range = Rail_int.RailSpline.Length; 

                }

               
                //RailGrind();
            }
            else
            {
                Input.LockInputForAWhile(5f, false);

                if (isZipLine)
                {
                    ZipHandle.GetComponent<CapsuleCollider>().enabled = false;
                    GameObject target = ZipHandle.transform.GetComponent<S_Control_PulleyObject>().homingtgt;
                    target.SetActive(false);

                    if (backwards)
                    {
                        Player.rb.velocity = ZipBody.velocity;
                    }
                    else
                    {
                        Player.rb.velocity = ZipBody.velocity;
                    }

                    Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
                    if (VelocityMod != Vector3.zero)
                    {
                        CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, transform.up);
                    }
                
                }
                else
                {

                    if (backwards)
                        Player.rb.velocity = -sample.tangent * PlayerSpeed;
                    else
                        Player.rb.velocity = sample.tangent * PlayerSpeed;

                    Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
                    if (VelocityMod != Vector3.zero)
                    {
                        CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, transform.up);
                    }
                }

                Actions.LeftStepPressed = false;
                Actions.RightStepPressed = false;

                OnRail = false;
            }
        }
        void SlopePhys()
        {
            if(Boosted)
            {
                if (PlayerSpeed > 60)
                {
                    boostTime -= Time.fixedDeltaTime;
                    if (boostTime < 0)
                    {
                        PlayerSpeed -= decaySpeed;
                        if (boostTime < -decayTime)
                        { 
                            Boosted = false;
                            boostTime = 0;
                        }
                    }
                }
                else
                    Boosted = false;
            }

            //slope curve from Bhys
            curvePosSlope = Player.curvePosSlope;
            float v = Input.InputExporter.y;
            v = (v + 1) / 2;
            //use player vertical speed to find if player is going up or down
            //Debug.Log(Player.p_rigidbody.velocity.normalized.y);

            //if (Player.rb.velocity.y >= -3f)
            if (Player.rb.velocity.y > 0.05f)
            {
                //uphill and straight
                float lean = UpHillMultiplier;
                if (Crouching) { lean = UpHillMultiplierCrouching; }
                //Debug.Log("UpHill : *" + lean);
                float force = (SlopePower * curvePosSlope) * lean;
                //Debug.Log(Mathf.Abs(Player.p_rigidbody.velocity.normalized.y - 1));
                float AbsYPow = Mathf.Abs(Player.rb.velocity.normalized.y * Player.rb.velocity.normalized.y);
                //Debug.Log( "Val" + Player.p_rigidbody.velocity.normalized.y + "Pow" + AbsYPow);
                force = (AbsYPow * force) + (DragVal * PlayerSpeed);
                //Debug.Log(force);
                force = Mathf.Clamp(force, -0.3f, 0.3f);
                PlayerSpeed += force;

                //Enforce max Speed
                if (PlayerSpeed > Player.MaxSpeed)
                {
                    PlayerSpeed = Player.MaxSpeed;
                }
            }
            else if (Player.rb.velocity.y < -0.05f)
            {
                //Downhill
                float lean = DownHillMultiplier;
                if (Crouching) { lean = DownHillMultiplierCrouching; }
                //Debug.Log("DownHill : *" + lean);
                float force = (SlopePower * curvePosSlope) * lean;
                //Debug.Log(Mathf.Abs(Player.p_rigidbody.velocity.normalized.y));
                float AbsYPow = Mathf.Abs(Player.rb.velocity.normalized.y * Player.rb.velocity.normalized.y);
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
            else
            {
                //Decay
                if (PlayerSpeed > railmaxSpeed)
                    PlayerSpeed -= decaySpeedHigh;

                else if (PlayerSpeed > railTopSpeed)
                    PlayerSpeed -= decaySpeedLow;
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
                //Sounds.RailSound(RailSound);
            }
        }

        //void CameraFocus()
        //{
        //    Cam.OverrideTarget.position = Vector3.Lerp(Cam.OverrideTarget.position, transform.position + (sample.up * TargetDistance), Time.deltaTime * CameraLerp);
        //    Cam.TargetOverriden = true;
        //}

        void AssignStats()
        {
            railTopSpeed = Tools.coreStats.railTopSpeed;
            railmaxSpeed = Tools.coreStats.railMaxSpeed;
            decaySpeedHigh = Tools.coreStats.railDecaySpeedHigh;
            decaySpeedLow = Tools.coreStats.railDecaySpeedLow;
            MinStartSpeed = Tools.coreStats.MinStartSpeed;
            PushFowardmaxSpeed = Tools.coreStats.RailPushFowardmaxSpeed;
            PushFowardIncrements = Tools.coreStats.RailPushFowardIncrements;
            PushFowardDelay = Tools.coreStats.RailPushFowardDelay;
            SlopePower = Tools.coreStats.SlopePower;
            UpHillMultiplier = Tools.coreStats.RailUpHillMultiplier;
            DownHillMultiplier = Tools.coreStats.RailDownHillMultiplier;
            UpHillMultiplierCrouching = Tools.coreStats.RailUpHillMultiplierCrouching;
            DownHillMultiplierCrouching = Tools.coreStats.RailDownHillMultiplierCrouching;
            DragVal = Tools.coreStats.RailDragVal;
            PlayerBrakePower = Tools.coreStats.RailPlayerBrakePower;
            HopDelay = Tools.coreStats.hopDelay;
            stepSpeed = Tools.coreStats.hopSpeed;
            hopDistance = Tools.coreStats.hopDistance;
            accelBySpeed = Tools.coreStats.railAccelBySpeed;

            OffsetRail = Tools.coreStats.OffsetRail;
            OffsetZip = Tools.coreStats.OffsetZip;
            decaySpeed = Tools.coreStats.railBoostDecaySpeed;
            decayTime = Tools.coreStats.railBoostDecayTime;

        }

        void AssignTools()
        {
            Actions = GetComponent<S_ActionManager>();
            Input = GetComponent<S_PlayerInput>();
            Rail_int = GetComponent<S_Interaction_Pathers>();
            Player = GetComponent<S_PlayerPhysics>();
            Cam = GetComponent<S_Handler_Camera>().Cam;

            CharacterAnimator = Tools.CharacterAnimator;
            Sounds = Tools.SoundControl;

            jumpBall = Tools.JumpBall;
        }

    }

}
