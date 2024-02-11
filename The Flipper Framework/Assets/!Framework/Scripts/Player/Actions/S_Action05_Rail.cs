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

        GameObject JumpBall;

        S_CharacterTools Tools;
        S_ActionManager Actions;
        S_HedgeCamera Cam;
        public Transform ZipHandle { get; set; }
        public Rigidbody ZipBody { get; set; }

        [HideInInspector] public bool isZipLine;

        [Header("Skin Rail Params")]

        public float skinRotationSpeed;
        float _offsetRail_ = 2.05f;
        float _offsetZip_ = -2.05f;

        public LayerMask railMask;
        [HideInInspector] public float _railmaxSpeed_;
        float _railTopSpeed_;
        float _decaySpeedLow_;
        float _decaySpeedHigh_;
        float _minStartSpeed_ = 60f;
        float _pushFowardmaxSpeed_ = 80f;
        float _pushFowardIncrements_ = 15f;
        float _pushFowardDelay_ = 0.5f;
        float _slopePower_ = 2.5f;
        float _upHillMultiplier_ = 0.25f;
        float _downHillMultiplier_ = 0.35f;
        float _upHillMultiplierCrouching_ = 0.4f;
        float _downHillMultiplierCrouching_ = 0.6f;
        float _dragVal_ = 0.0001f;
        float _playerBrakePower_ = 0.95f;
        float _hopDelay_;
        float _hopDistance_ = 12;
        float _decayTime_;
        AnimationCurve _accelBySpeed_;
        float _decaySpeed_;

        float curvePosSlope { get; set; }

        // Setting up Values
        float timer = 0f;
        float PulleyRotate;
        [HideInInspector] public float range = 0f;
        Transform RailTransform;
        public bool OnRail { get; set; }
        public S_AddOnRail ConnectedRails;
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
        float _stepSpeed_ = 3.5f;
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
            if (!transform.parent.gameObject.activeSelf)
                return;

            Player._isGravityOn = true;

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

        public void InitialEvents(float Range, Transform RailPos, bool isZip, Vector3 thisOffset, S_AddOnRail addOn)
        {
            StartCoroutine(allowHop());

            //ignore further railcollisions
            Physics.IgnoreLayerCollision(this.gameObject.layer, 23, true);

            canInput = true;
            setOffSet = -thisOffset;

            ConnectedRails = addOn;
            JumpBall.SetActive(false);
            Actions.JumpPressed = false;

            isZipLine = isZip;
            timer = _pushFowardDelay_;
            RailContactSound = false;

            Boosted = false;
            boostTime = 0;

            Player._isGravityOn = false;
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
                PlayerSpeed = Player._speedMagnitude;

       

            CurveSample sample = Rail_int.RailSpline.GetSampleAtDistance(range);
            float dotdir = Vector3.Dot(Player.rb.velocity.normalized, sample.tangent);
            Crouching = false;
            PulleyRotate = 0f;

            InitialRot = transform.rotation;

            //Vector3 dir = sample.tangent;
            //Cam.SetCamera(dir, 2.5f, 20f, 1f);
            //Cam.Locked = false;


            // Check if was Homingattack
            if (Actions.whatAction == S_Enums.PlayerStates.Homing)
            {
                PlayerSpeed = Actions.Action02.LateSpeed;
                dotdir = Vector3.Dot(Actions.Action02.TargetDirection.normalized, sample.tangent);
            }
            else if (Actions.whatAction == S_Enums.PlayerStates.DropCharge)
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
            PlayerSpeed = Mathf.Max(PlayerSpeed, _minStartSpeed_);

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
            yield return new WaitForSeconds(_hopDelay_);
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
                CharacterAnimator.SetBool("Grounded", Player._isGrounded);

                Actions.ChangeAction(S_Enums.PlayerStates.Regular);
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
            CharacterAnimator.SetFloat("GroundSpeed", PlayerSpeed / Player._currentMaxSpeed);
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
                    Player._groundNormal = new Vector3(0f, 1f, 0f);

                    StartCoroutine(Rail_int.JumpFromZipLine(ZipHandle, 1));

                }

                transform.position += jumpCorrectedOffset;

                OnRail = false;

                isZipLine = false;


                //Player.transform.rotation = InitialRot;

                //Player.transform.eulerAngles = new Vector3(0,1,0);
                Actions.Action01.jumpCount = -1;
                Actions.Action01.InitialEvents(sample.up, true, Player.rb.velocity.y);
                Actions.ChangeAction(S_Enums.PlayerStates.Jump);

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

                if (timer > _pushFowardDelay_)
                {
                    //Sounds.RailSoundStop();
                    isSwitching = true;
                    if (PlayerSpeed < _pushFowardmaxSpeed_)
                    {
                        PlayerSpeed += _pushFowardIncrements_ + _accelBySpeed_.Evaluate(PlayerSpeed / _pushFowardmaxSpeed_);
                    }
                    faceRight = !faceRight;
                    timer = 0f;
                    Actions.SpecialPressed = false;
                }
            }
            isSwitching = (timer < _pushFowardDelay_);

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
                    transform.position = (sample.location + RailTransform.position + (sample.up * _offsetRail_)) + binormal;

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
                    transform.position = ZipHandle.transform.position + (ZipHandle.transform.up * _offsetZip_);


                }

                if (isBraking && PlayerSpeed > _minStartSpeed_) PlayerSpeed *= _playerBrakePower_;

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
                  
                    distanceToStep = _hopDistance_;
                    canInput = false;
                    steppingRight = true;
                    Actions.RightStepPressed = false;
                    performStep();
                    return;

                }
                else if (Actions.LeftStepPressed)
                {
                    

                    distanceToStep = _hopDistance_;
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
                float move = _stepSpeed_;

                if (steppingRight)
                    move = -move;
                if(backwards)
                    move = -move;

                move = Mathf.Clamp(move, -distanceToStep, distanceToStep);

                setOffSet.Set(setOffSet.x + move, setOffSet.y, setOffSet.z);
               
                if(move < 0)
                    if(Physics.BoxCast(CharacterAnimator.transform.position,new Vector3(1.3f, 3f, 1.3f), -CharacterAnimator.transform.right, Quaternion.identity, 4, Tools.Stats.QuickstepStats.StepLayerMask))
                    {
                        Actions.ChangeAction(S_Enums.PlayerStates.Regular);
                        CharacterAnimator.SetInteger("Action", 0);
                    }
                else
                    if (Physics.BoxCast(CharacterAnimator.transform.position, new Vector3(1.3f, 3f, 1.3f), CharacterAnimator.transform.right, Quaternion.identity, 4, Tools.Stats.QuickstepStats.StepLayerMask))
                    {
                        Actions.ChangeAction(S_Enums.PlayerStates.Regular);
                        CharacterAnimator.SetInteger("Action", 0);
                    }

                distanceToStep -= _stepSpeed_;

                if (distanceToStep < 6)
                {
                    Physics.IgnoreLayerCollision(8, 23, false);

                    if (distanceToStep <= 0)
                    {
                        Actions.ChangeAction(S_Enums.PlayerStates.Regular);
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
                    setOffSet.Set(-ConnectedRails.GetComponent<S_PlaceOnSpline>().Offset3d.x, 0, 0);

                    Rail_int.RailSpline = ConnectedRails.GetComponentInParent<Spline>();
                    RailTransform = Rail_int.RailSpline.transform.parent;

                    Debug.Log("Then With = " + range);
                }
                else if (backwards && ConnectedRails.PrevRail != null)
                {

                    Debug.Log("Back To Previous Rail");

                    S_AddOnRail temp = ConnectedRails;
                    ConnectedRails = ConnectedRails.PrevRail;
                    setOffSet.Set(-ConnectedRails.GetComponent<S_PlaceOnSpline>().Offset3d.x, 0, 0);

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
                        PlayerSpeed -= _decaySpeed_;
                        if (boostTime < -_decayTime_)
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
                float lean = _upHillMultiplier_;
                if (Crouching) { lean = _upHillMultiplierCrouching_; }
                //Debug.Log("UpHill : *" + lean);
                float force = (_slopePower_ * curvePosSlope) * lean;
                //Debug.Log(Mathf.Abs(Player.p_rigidbody.velocity.normalized.y - 1));
                float AbsYPow = Mathf.Abs(Player.rb.velocity.normalized.y * Player.rb.velocity.normalized.y);
                //Debug.Log( "Val" + Player.p_rigidbody.velocity.normalized.y + "Pow" + AbsYPow);
                force = (AbsYPow * force) + (_dragVal_ * PlayerSpeed);
                //Debug.Log(force);
                force = Mathf.Clamp(force, -0.3f, 0.3f);
                PlayerSpeed += force;

                //Enforce max Speed
                if (PlayerSpeed > Player._currentMaxSpeed)
                {
                    PlayerSpeed = Player._currentMaxSpeed;
                }
            }
            else if (Player.rb.velocity.y < -0.05f)
            {
                //Downhill
                float lean = _downHillMultiplier_;
                if (Crouching) { lean = _downHillMultiplierCrouching_; }
                //Debug.Log("DownHill : *" + lean);
                float force = (_slopePower_ * curvePosSlope) * lean;
                //Debug.Log(Mathf.Abs(Player.p_rigidbody.velocity.normalized.y));
                float AbsYPow = Mathf.Abs(Player.rb.velocity.normalized.y * Player.rb.velocity.normalized.y);
                //Debug.Log("Val" + Player.p_rigidbody.velocity.normalized.y + "Pow" + AbsYPow);
                force = (AbsYPow * force) - (_dragVal_ * PlayerSpeed);
                //Debug.Log(force);
                PlayerSpeed -= force;

                //Enforce max Speed
                if (PlayerSpeed > Player._currentMaxSpeed)
                {
                    PlayerSpeed = Player._currentMaxSpeed;
                }
            }
            else
            {
                //Decay
                if (PlayerSpeed > _railmaxSpeed_)
                    PlayerSpeed -= _decaySpeedHigh_;

                else if (PlayerSpeed > _railTopSpeed_)
                    PlayerSpeed -= _decaySpeedLow_;
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
            _railTopSpeed_ = Tools.Stats.RailStats.railTopSpeed;
            _railmaxSpeed_ = Tools.Stats.RailStats.railMaxSpeed;
            _decaySpeedHigh_ = Tools.Stats.RailStats.railDecaySpeedHigh;
            _decaySpeedLow_ = Tools.Stats.RailStats.railDecaySpeedLow;
            _minStartSpeed_ = Tools.Stats.RailStats.MinStartSpeed;
            _pushFowardmaxSpeed_ = Tools.Stats.RailStats.RailPushFowardmaxSpeed;
            _pushFowardIncrements_ = Tools.Stats.RailStats.RailPushFowardIncrements;
            _pushFowardDelay_ = Tools.Stats.RailStats.RailPushFowardDelay;
            _slopePower_ = Tools.Stats.SlopeStats.slopePower;
            _upHillMultiplier_ = Tools.Stats.RailStats.RailUpHillMultiplier;
            _downHillMultiplier_ = Tools.Stats.RailStats.RailDownHillMultiplier;
            _upHillMultiplierCrouching_ = Tools.Stats.RailStats.RailUpHillMultiplierCrouching;
            _downHillMultiplierCrouching_ = Tools.Stats.RailStats.RailDownHillMultiplierCrouching;
            _dragVal_ = Tools.Stats.RailStats.RailDragVal;
            _playerBrakePower_ = Tools.Stats.RailStats.RailPlayerBrakePower;
            _hopDelay_ = Tools.Stats.RailStats.hopDelay;
            _stepSpeed_ = Tools.Stats.RailStats.hopSpeed;
            _hopDistance_ = Tools.Stats.RailStats.hopDistance;
            _accelBySpeed_ = Tools.Stats.RailStats.RailAccelerationBySpeed;

            _offsetRail_ = Tools.Stats.RailPosition.offsetRail;
            _offsetZip_ = Tools.Stats.RailPosition.offsetZip;
            _decaySpeed_ = Tools.Stats.RailStats.railBoostDecaySpeed;
            _decayTime_ = Tools.Stats.RailStats.railBoostDecayTime;

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

            JumpBall = Tools.JumpBall;
        }

    }

}
