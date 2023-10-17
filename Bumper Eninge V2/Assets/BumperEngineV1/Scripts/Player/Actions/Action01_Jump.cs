using UnityEngine;
using System.Collections;

public class Action01_Jump : MonoBehaviour
{
    CharacterTools Tools;

    RaycastHit hit;
    Vector3 DirInput;

    [Header("Basic Resources")]
    
    PlayerBhysics Player;
    ActionManager Actions;
    HomingAttackControl homingControl;
    CameraControl Cam;
    quickstepHandler stepManager;

    SonicSoundsControl sounds;
    Animator CharacterAnimator;
    public float skinRotationSpeed;
    GameObject JumpBall;

    public Vector3 InitialNormal { get; set; }
    public float Counter { get; set; }
    public float ControlCounter;
    [HideInInspector] public int jumpCount;
    [HideInInspector] public bool Jumping;

    [Header("Core Stats")]
    [HideInInspector] public float StartJumpDuration;
    [HideInInspector] public float StartSlopedJumpDuration;
    [HideInInspector] public float StartJumpSpeed;
    [HideInInspector] public float JumpSlopeConversion;
    [HideInInspector] public float StopYSpeedOnRelease;
    [HideInInspector] public float RollingLandingBoost;

    [HideInInspector] public float JumpDuration;
    [HideInInspector] public float SlopedJumpDuration;
    [HideInInspector] public float JumpSpeed;
    float speedLossOnJump;
    float speedLossOnDoubleJump;using UnityEngine;
using System.Collections;

public class Action01_Jump : MonoBehaviour
{

    public Animator CharacterAnimator;
    PlayerBhysics Player;
    ActionManager Actions;
    public SonicSoundsControl sounds;

    public float skinRotationSpeed;
    public GameObject JumpBall;

    public Vector3 InitialNormal { get; set; }
    public float Counter { get; set; }
    public float JumpDuration;
    public float SlopedJumpDuration;
    public float JumpSpeed;
    public float JumpSlopeConversion;
    public float StopYSpeedOnRelease;
    public float RollingLandingBoost;

    float jumpSlopeSpeed;

    void Awake()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
    }

    public void InitialEvents()
    {
        //Set Initial Variables
        JumpBall.SetActive(true);
        Counter = 0;
        jumpSlopeSpeed = 0;
        InitialNormal = Player.GroundNormal;
        Player.TimeOnGround = 0;
        //Debug.Log(Player.GroundNormal);

        //SnapOutOfGround to make sure you do jump
        transform.position += (InitialNormal * 0.3f);

        //Jump higher depending on the speed and the slope you're in
        if (Player.p_rigidbody.velocity.y > 0 && Player.GroundNormal.y > 0)
        {
            jumpSlopeSpeed = Player.p_rigidbody.velocity.y * JumpSlopeConversion;
        }
        //Sound
        sounds.JumpSound();
    }

    void Update()
    {

        //Set Animator Parameters
        if (Actions.Action == 1)
        {
            CharacterAnimator.SetInteger("Action", 1);
        }
        CharacterAnimator.SetFloat("YSpeed", Player.p_rigidbody.velocity.y);
        CharacterAnimator.SetFloat("GroundSpeed", Player.p_rigidbody.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);
        CharacterAnimator.SetBool("isRolling", false);

        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z);
        Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);

        if (Actions.Action02 != null)
        {
            Actions.Action02.HomingAvailable = true;
        }
        if (Actions.Action06 != null)
        {
            Actions.Action06.BounceAvailable = true;
        }
        if (Actions.Action08 != null)
        {
            Actions.Action08.DropDashAvailable = true;
        }

        //Do a homing attack
        if (Actions.Action02 != null)
        {

            if (Counter > 0.08f && Input.GetButtonDown("A") && Actions.Action02Control.HasTarget && Actions.Action02.HomingAvailable)
            {
                if (Actions.Action02Control.HomingAvailable)
                {
                    sounds.HomingAttackSound();
                    Actions.Action02.IsAirDash = false;
                    Actions.ChangeAction(2);
                    Actions.Action02.InitialEvents();
                }
            }
            //If no tgt, do air dash;
            if (Counter > 0.08f && Input.GetButtonDown("A") && !Actions.Action02Control.HasTarget && Actions.Action02.HomingAvailable && Actions.Action08 == null)
            {
                if (Actions.Action02Control.HomingAvailable)
                {
                    sounds.AirDashSound();
                    Actions.Action02.IsAirDash = true;
                    Actions.ChangeAction(2);
                    Actions.Action02.InitialEvents();
                }
            }
        }

        //Do a Bounce Attack
        if (Input.GetButtonDown("X") && Actions.Action06.BounceAvailable)
        {
            Actions.ChangeAction(6);
            //	Actions.Action06.ShouldStomp = false;
            Actions.Action06.InitialEvents();
        }
        //Do a LightDash Attack
        if (Input.GetButtonDown("Y") && Actions.Action07Control.HasTarget)
        {
            Actions.ChangeAction(7);
            Actions.Action07.InitialEvents();
        }
        //Do a DropDash Attack
        if (Actions.Action08 != null)
        {
            if (Counter > 0.08f && Input.GetButtonDown("A") && Actions.Action08.DropDashAvailable && Actions.Action08 != null && !Actions.Action02Control.HasTarget)
            {
                Actions.ChangeAction(8);

                Actions.Action08.InitialEvents();
            }
        }



    }

    void FixedUpdate()
    {

        //Jump action
        Counter += Time.deltaTime;

        if (!Input.GetButton("A") && Counter < JumpDuration)
        {
            Counter = JumpDuration;
        }

        //Keep Colliders Rotation to avoid collision Issues
        if (Counter < 0.2f)
        {
            //transform.rotation = Quaternion.FromToRotation(transform.up, InitialNormal) * transform.rotation;
        }

        //Add Jump Speed
        if (Counter < JumpDuration)
        {
            Player.isRolling = false;
            if (Counter < SlopedJumpDuration)
            {
                Player.AddVelocity(InitialNormal * (JumpSpeed));
            }
            else
            {
                Player.AddVelocity(new Vector3(0, 1, 0) * (JumpSpeed));
            }
            //Extra speed
            Player.AddVelocity(new Vector3(0, 1, 0) * (jumpSlopeSpeed));
        }

        //Cancel Jump
        if (Player.p_rigidbody.velocity.y > 0 && !Input.GetButton("A"))
        {
            Vector3 Velocity = new Vector3(Player.p_rigidbody.velocity.x, Player.p_rigidbody.velocity.y, Player.p_rigidbody.velocity.z);
            Velocity.y = Velocity.y - StopYSpeedOnRelease;
            Player.p_rigidbody.velocity = Velocity;
        }


        //End Action
        if (Player.Grounded && Counter > SlopedJumpDuration)
        {
            Actions.ChangeAction(0);

            Actions.Action06.BounceCount = 0;
            JumpBall.SetActive(false);
        }

    }
}



float jumpSpeedModifier = 1f;
    float jumpDurationModifier = 1f;

    [Header("Additional Jump Stats")]

    [HideInInspector] public bool canDoubleJump = true;
    bool canTripleJump = false;

    float doubleJumpSpeed;
    float doubleJumpDuration;


    float jumpSlopeSpeed;


    [HideInInspector] public float timeJumping;
    bool cancelled;




    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();
            
            AssignStats();   
        }
    }

    public void InitialEvents(Vector3 normaltoJump, bool Grounded = false, float verticalSpeed = 0, float controlDelay = 0, float minJumpSpeed = 0)
    {
        if(!Actions.lockDoubleJump)
        {
            Actions.RollPressed = false;
            cancelled = false;
            Jumping = true;
            Counter = 0;
            ControlCounter = controlDelay;
            jumpSlopeSpeed = 0;
            timeJumping = 0f;
            //Debug.Log(jumpCount);

            if (1 - Mathf.Abs(normaltoJump.y) < 0.1f)
                normaltoJump = Vector3.up;

            if (verticalSpeed < 0)
                verticalSpeed = 0;

            //If performing a grounded jump. JumpCount may be changed externally to allow for this.
            //if (Grounded || jumpCount == -1)
            if (Grounded)
            {
                Player.rb.velocity = new Vector3(Player.rb.velocity.x * speedLossOnJump, verticalSpeed, Player.rb.velocity.z * speedLossOnJump);

                if (Actions.eventMan != null) Actions.eventMan.JumpsPerformed += 1;

                //Sets jump stats for this specific jump.

                JumpSpeed = StartJumpSpeed * jumpSpeedModifier;
                JumpDuration = StartJumpDuration * jumpDurationModifier;
                SlopedJumpDuration = StartSlopedJumpDuration * jumpDurationModifier;

                //Number of jumps set to zero, allowing for double jumps.
                jumpCount = 0;

                //Sets jump direction
                InitialNormal = normaltoJump;

                Player.TimeOnGround = 0;

                //SnapOutOfGround to make sure you do jump
                transform.position += (InitialNormal * 0.3f);

                //Jump higher depending on the speed and the slope you're in
                if (Player.rb.velocity.y > 0 && normaltoJump.y > 0)
                {
                    jumpSlopeSpeed = Player.rb.velocity.y * JumpSlopeConversion;
                }

            }

            else
            {

                if (Actions.eventMan != null) Actions.eventMan.DoubleJumpsPerformed += 1;

                //Increases jump count
                if (jumpCount == 0)
                    jumpCount = 1;

                //Sets jump direction
                InitialNormal = normaltoJump;

                //Sets jump stats for this specific jump.
                JumpSpeed = doubleJumpSpeed * jumpSpeedModifier;
                JumpDuration = doubleJumpDuration * jumpDurationModifier;
                SlopedJumpDuration = doubleJumpDuration * jumpDurationModifier;

                JumpAgain();
            }

            //SetAnims
            CharacterAnimator.SetInteger("Action", 1);
            

            //Sound
            sounds.JumpSound();
        }
        
    }

    void Update()
    {

        //Set Animator Parameters
        if (Actions.Action == ActionManager.States.Jump)
        {
            CharacterAnimator.SetInteger("Action", 1);
        }

        if (!(Player.rb.velocity.y < 0.1f && Player.rb.velocity.y > -0.1f))
        {
            CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
        }
        CharacterAnimator.SetFloat("GroundSpeed", Player.HorizontalSpeedMagnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);
        CharacterAnimator.SetBool("isRolling", false);

        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);

        if (VelocityMod != Vector3.zero)
        {
            Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
            CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
        }

        //if (Actions.Action02 != null)
        //{
        //    Actions.Action02.HomingAvailable = true;
        //}
        if (Actions.Action06 != null)
        {
            Actions.Action06.BounceAvailable = true;
        }

        //Actions
        if (!Actions.isPaused)
        {
            //If can homing attack pressed.
            if(Actions.HomingPressed)
            {
                if (Actions.Action02.HomingAvailable && Actions.Action02Control.HasTarget && !Player.Grounded)
                {

                    //Do a homing attack
                    if (Actions.Action02 != null && Player.HomingDelay <= 0)
                    {
                        if (Actions.Action02Control.HomingAvailable)
                        {
                            sounds.HomingAttackSound();
                            Actions.ChangeAction(ActionManager.States.Homing);
                            Actions.Action02.InitialEvents();
                        }
                    }

                }
            }
            

            //If no tgt, do air dash;
            if (Actions.SpecialPressed)
            {
                if (Actions.Action02.HomingAvailable)
                {
                    if (!Actions.Action02Control.HasTarget)
                    {
                        sounds.AirDashSound();
                        Actions.ChangeAction(ActionManager.States.JumpDash);
                        Actions.Action11.InitialEvents();
                    }
                }
            }



            //Handle additional jumps. Can only be done if jump is over but jump button pressed.
            if (!Jumping && Actions.JumpPressed)
            {
                //Do a double jump
                if (jumpCount == 1 && canDoubleJump)
                {
                    
                    InitialEvents(Vector3.up);
                    Actions.ChangeAction(ActionManager.States.Jump);
                }

                //Do a triple jump
                else if (jumpCount == 2 && canTripleJump)
                {
                    //Debug.Log("Do a triple jump");
                    InitialEvents(Vector3.up);
                    Actions.ChangeAction(ActionManager.States.Jump);
                }
            }
            

            //Do a Bounce Attack
            if (Actions.BouncePressed)
            {
                if(Actions.Action06.BounceAvailable && Player.rb.velocity.y < 35f)
                {
                    Actions.Action06.InitialEvents();
                    Actions.ChangeAction(ActionManager.States.Bounce);
                    //	Actions.Action06.ShouldStomp = false;
                }

            }

            //Set Camera to back
            if (Actions.CamResetPressed)
            {
                if (Actions.moveX == 0 && Actions.moveY == 0 && Player.b_normalSpeed < 5f)
                    Cam.Cam.FollowDirection(10, 14f, -10, 0);
            }

         

            //Do a DropDash 

            if (Actions.RollPressed)
            {
                Actions.Action08.TryDropCharge();
                          
            }

            /////Enalbing Quickstepping
            ///
            //Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
            if (Actions.RightStepPressed)
            {
                stepManager.pressRight();
            }
            else if (Actions.LeftStepPressed)
            {
                stepManager.pressLeft();
            }

            //Enable Quickstep right or left
            if (Actions.RightStepPressed && !stepManager.enabled)
            {
                if (Player.HorizontalSpeedMagnitude > 15f)
                {
                    Debug.Log("Step in Jump");
                    stepManager.initialEvents(true);
                    stepManager.enabled = true;
                }
                    
            }

            else if (Actions.LeftStepPressed && !stepManager.enabled)
            {

                if (Player.HorizontalSpeedMagnitude > 15f)
                {

                    stepManager.initialEvents(false);
                    stepManager.enabled = true;
                }
                  
            }
        }

    }

    void FixedUpdate()
    {
        //Jump action
        Counter += Time.fixedDeltaTime;
        ControlCounter += Time.fixedDeltaTime;
        timeJumping += Time.fixedDeltaTime;

        //if (!Actions.JumpPressed && Counter < JumpDuration && Counter > 0.1f && Jumping)
        if (!Actions.JumpPressed && Counter > 0.1f && Jumping)
        {
            jumpCount ++;
            Counter = JumpDuration;
            Jumping = false;
        }
        else if (Counter > JumpDuration && Jumping && Actions.JumpPressed)
        {
            jumpCount++;
            Counter = JumpDuration;
            Jumping = false;
            Actions.JumpPressed = false;
        }
        //Add Jump Speed
        else if (Counter < JumpDuration && Jumping)
        {
            Player.isRolling = false;
            if (Counter < SlopedJumpDuration)
            {
                Player.AddVelocity(InitialNormal * (JumpSpeed));
                //Debug.Log(InitialNormal);
            }
            else
            {
                Player.AddVelocity(new Vector3(0, 1, 0) * (JumpSpeed));
            }
            //Extra speed
            Player.AddVelocity(new Vector3(0, 1, 0) * (jumpSlopeSpeed));
        }
        


        //Cancel Jump
        if (!cancelled && Player.rb.velocity.y > 0 && !Jumping && Counter > 0.1)
        {
            cancelled = true;
            //jumpCount = 1;
            Vector3 Velocity = new Vector3(Player.rb.velocity.x, Player.rb.velocity.y, Player.rb.velocity.z);
            Velocity.y = Velocity.y - StopYSpeedOnRelease;
            Player.rb.velocity = Velocity;
        }

        //End Action
        if (Player.Grounded && Counter > SlopedJumpDuration)
        {

            jumpCount = 0;


            Actions.JumpPressed = false;
            JumpBall.SetActive(false);

            Actions.ChangeAction(ActionManager.States.Regular);
            Actions.Action06.BounceCount = 0;
            //JumpBall.SetActive(false);
        }

        //Skidding
        Actions.skid.jumpSkid();


    }


    private void JumpAgain()
    {
        //jumpCount++;
        //InitialNormal = CharacterAnimator.transform.up;

        //SnapOutOfGround to make sure you do jump
        transform.position += (InitialNormal * 0.3f);

        Vector3 newVec;

        if (Player.rb.velocity.y > 10)
            newVec = new Vector3(Player.rb.velocity.x * speedLossOnDoubleJump, Player.rb.velocity.y, Player.rb.velocity.z * speedLossOnDoubleJump);
        else
            newVec = new Vector3(Player.rb.velocity.x * speedLossOnDoubleJump, Mathf.Clamp(Player.rb.velocity.y * 0.1f, 0.1f, 5), Player.rb.velocity.z * speedLossOnDoubleJump);

        Player.rb.velocity = newVec;

        GameObject JumpDashParticleClone = Instantiate(Tools.JumpDashParticle, Tools.FeetPoint.position, Quaternion.identity) as GameObject;

        //JumpDashParticleClone.GetComponent<ParticleSystem>().startSize = 1f;

        JumpDashParticleClone.transform.position = Tools.FeetPoint.position;
        JumpDashParticleClone.transform.rotation = Quaternion.LookRotation(Vector3.up);

    }

    //Reponsible for assigning stats from the stats script.
    private void AssignStats()
    {
        StartJumpDuration = Tools.stats.StartJumpDuration;
        StartJumpSpeed = Tools.stats.StartJumpSpeed;
        StartSlopedJumpDuration = Tools.stats.StartSlopedJumpDuration;
        JumpSlopeConversion = Tools.coreStats.JumpSlopeConversion;
        RollingLandingBoost = Tools.coreStats.JumpRollingLandingBoost;
        StopYSpeedOnRelease = Tools.coreStats.StopYSpeedOnRelease;

        canDoubleJump = Tools.stats.canDoubleJump;
        canTripleJump = Tools.stats.canTripleJump;
        doubleJumpDuration = Tools.stats.doubleJumpDuration;
        doubleJumpSpeed = Tools.stats.doubleJumpSpeed;

        speedLossOnDoubleJump = Tools.stats.speedLossOnDoubleJump;
        speedLossOnJump = Tools.stats.speedLossOnJump;


    }

    //Responsible for assigning objects and components from the tools script.
    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();
        homingControl = GetComponent<HomingAttackControl>();

        stepManager = GetComponent<quickstepHandler>();

        CharacterAnimator = Tools.CharacterAnimator;
        sounds = Tools.SoundControl;
        JumpBall = Tools.JumpBall;
    }


    private void OnDisable()
    {
        JumpBall.SetActive(false);
    }

    private void OnEnable()
    {
        JumpBall.SetActive(true);
    }
}
