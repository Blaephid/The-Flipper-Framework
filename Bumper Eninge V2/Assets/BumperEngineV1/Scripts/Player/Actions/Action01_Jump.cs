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

    SonicSoundsControl sounds;
    Animator CharacterAnimator;
    public float skinRotationSpeed;
    GameObject JumpBall;

    public Vector3 InitialNormal { get; set; }
    public float Counter { get; set; }
    [HideInInspector] public int jumpCount;
    bool Jumping;

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

    float jumpSpeedModifier = 1f;
    float jumpDurationModifier = 1f;

    [Header("Additional Jump Stats")]

    [HideInInspector] public bool canDoubleJump = true;
    bool canTripleJump = false;

    float doubleJumpSpeed;
    float doubleJumpDuration;


    [Header("QuickStepping")]
    [HideInInspector] public bool QuickStepping;
    bool canStep;
    float airStepSpeed;
    [HideInInspector] public bool StepRight;
    float StepCounter = 0f;
    [HideInInspector] public float StepDistance = 50f;
    [HideInInspector] public float DistanceToStep;
    RaycastHit stepHit;
    LayerMask StepPlayermask;

    float jumpSlopeSpeed;

    [Header("Detecting Wall Run")]
    Action12_WallRunning WallRun;
    float WallCheckDistance;
    LayerMask wallLayerMask;
    float CheckModifier;

    private RaycastHit leftWallDetect;
    private bool wallLeft;
    private RaycastHit rightWallDetect;
    private bool wallRight;
    private RaycastHit frontWallDetect;
    private bool wallFront;




    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();
            
            AssignStats();   
        }
    }

    public void InitialEvents(Vector3 normaltoJump)
    {

        Jumping = true;
        Counter = 0;
        jumpSlopeSpeed = 0;
        //Debug.Log(jumpCount);

        //If performing a grounded jump. JumpCount may be changed externally to allow for this.
        if (Player.Grounded || jumpCount == -1)
        {

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
            JumpAgain();

            //Increases jump count
            if (jumpCount == 0)
                jumpCount = 1;

            //Sets jump stats for this specific jump.
            JumpSpeed = doubleJumpSpeed * jumpSpeedModifier;
            JumpDuration = doubleJumpDuration * jumpDurationModifier;
            SlopedJumpDuration = JumpDuration;
        }

        //SetAnims
        CharacterAnimator.SetInteger("Action", 1);
        JumpBall.SetActive(true);

        //Sound
        sounds.JumpSound();
    }

    void Update()
    {
        //Debug.Log(jumpCount);
        //Debug.Log("Jumping is " + Jumping);
        //Debug.Log(JumpSpeed);

        //Set Animator Parameters
        if (Actions.Action == 1)
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
                            Actions.ChangeAction(2);
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
                        JumpBall.SetActive(false);
                        Actions.ChangeAction(11);
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
                    //Debug.Log("Do a double jump");
                    InitialEvents(Vector3.up);
                    Actions.ChangeAction(1);
                }

                //Do a triple jump
                else if (jumpCount == 2 && canTripleJump)
                {
                    //Debug.Log("Do a triple jump");
                    InitialEvents(Vector3.up);
                    Actions.ChangeAction(1);
                }
            }
            

            //Do a Bounce Attack
            if (Actions.BouncePressed)
            {
                if(Actions.Action06.BounceAvailable && Player.rb.velocity.y < 35f)
                {
                    Actions.Action06.InitialEvents();
                    Actions.ChangeAction(6);
                    //	Actions.Action06.ShouldStomp = false;
                }

            }

            //Set Camera to back
            if (Actions.CamResetPressed)
            {
                if (Actions.moveX == 0 && Actions.moveY == 0 && Player.b_normalSpeed < 5f)
                    Cam.Cam.FollowDirection(6, 14f, -10, 0);
            }

            //Do a LightDash Attack
            if (Actions.InteractPressed)
            {
                if(Actions.Action07Control.HasTarget)
                {
                    Actions.CamResetPressed = false;
                    JumpBall.SetActive(false);
                    Actions.ChangeAction(7);
                    Actions.Action07.InitialEvents();
                }
            }

            //Do a DropDash 

            if (Actions.RollPressed)
            {
                if (Player.rb.velocity.y < 10f && Actions.Action08 != null)
                {
                    //Debug.Log("Enter DropDash");
                    Actions.ChangeAction(8);

                    Actions.Action08.InitialEvents();
                }
                
            }

            /////Enalbing Quickstepping
            ///
            //Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
            if (Actions.RightStepPressed)
            {
                Vector3 Direction = CharacterAnimator.transform.position - Cam.Cam.transform.position;
                bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < 0f;
                if (Facing)
                {
                    Actions.RightStepPressed = false;
                    Actions.LeftStepPressed = true;
                }
            }
            else if (Actions.LeftStepPressed)
            {
                Vector3 Direction = CharacterAnimator.transform.position - Cam.Cam.transform.position;
                bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < 0f;
                if (Facing)
                {
                    Actions.RightStepPressed = true;
                    Actions.LeftStepPressed = false;
                }
            }

            //Enable Quickstep right or left
            if (Actions.RightStepPressed && !QuickStepping)
            {
                Actions.RightStepPressed = false;
                Actions.LeftStepPressed = false;

                if (Player.HorizontalSpeedMagnitude > 10f)
                {
                    QuickStepping = true;
                    canStep = true;
                    StepRight = true;
                    DistanceToStep = StepDistance;

                }
            }

            else if (Actions.LeftStepPressed && !QuickStepping)
            {
                Actions.RightStepPressed = false;
                Actions.LeftStepPressed = false;

                if (Player.HorizontalSpeedMagnitude > 10f)
                {
                    QuickStepping = true;
                    canStep = true;
                    StepRight = false;
                    DistanceToStep = StepDistance;

                }
            }
        }

    }


    void FixedUpdate()
    {
        //Jump action
        Counter += Time.deltaTime;

        //if (!Actions.JumpPressed && Counter < JumpDuration && Counter > 0.1f && Jumping)
        if (!Actions.JumpPressed && Counter > 0.1f && Jumping)
        {
            jumpCount ++;
            Counter = JumpDuration;
            Jumping = false;
        }

        //Add Jump Speed
        if (Counter < JumpDuration)
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
        if (Player.rb.velocity.y > 0 && !Actions.JumpPressed && Counter > 0.1)
        {
            //jumpCount = 1;
            Vector3 Velocity = new Vector3(Player.rb.velocity.x, Player.rb.velocity.y, Player.rb.velocity.z);
            Velocity.y = Velocity.y - StopYSpeedOnRelease;
            Player.rb.velocity = Velocity;
        }

        //End Action
        if (Player.Grounded && Counter > SlopedJumpDuration)
        {
            jumpCount = 0;
            QuickStepping = false;

            Actions.JumpPressed = false;
            JumpBall.SetActive(false);

            Actions.ChangeAction(0);
            Actions.Action06.BounceCount = 0;
            //JumpBall.SetActive(false);
        }

        //Implementing Quickstep
        if (QuickStepping)
        {
            //If the step has a distance to go.
            if (DistanceToStep > 0)
            {        
                if (StepRight)
                {
                    //Finds position to head towards.
                    Vector3 positionTo = transform.position + (CharacterAnimator.transform.right * DistanceToStep);
                    float ToTravel = airStepSpeed * Time.deltaTime;

                    if (DistanceToStep - ToTravel <= 0)
                    {
                        ToTravel = DistanceToStep;
                        StepCounter = 0.4f;
                    }

                    DistanceToStep -= ToTravel;

                    //Detects if wall is in the way both from high and low.
                    if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.35f, transform.position.z), CharacterAnimator.transform.right * 1, out hit, 1.5f, StepPlayermask) && canStep)
                        if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), CharacterAnimator.transform.right * 1, out hit, .8f, StepPlayermask))
                            transform.position = Vector3.MoveTowards(transform.position, positionTo, ToTravel);
                        else
                            canStep = false;

                }
                // !(Physics.Raycast(transform.position, CharacterAnimator.transform.right * -1, out hit, 4f, StepPlayermask)
                else if (!StepRight)
                {
                    //Finds position to head towards.
                    Vector3 positionTo = transform.position + (-CharacterAnimator.transform.right * DistanceToStep);
                    float ToTravel = airStepSpeed * Time.deltaTime;

                    if (DistanceToStep - ToTravel <= 0)
                    {
                        ToTravel = DistanceToStep;
                        StepCounter = 0.3f;
                    }

                    DistanceToStep -= ToTravel;

                    //Detects if wall is in the way both from high and low.
                    if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.35f, transform.position.z), CharacterAnimator.transform.right * -1, out hit, 1.5f, StepPlayermask) && canStep)
                        if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), CharacterAnimator.transform.right * -1, out hit, .8f, StepPlayermask))
                            transform.position = Vector3.MoveTowards(transform.position, positionTo, ToTravel);
                        else
                            canStep = false;
                }    
            }

            //If not, quickstep ends.
            else
            {
                StepCounter -= Time.deltaTime;
                if (StepCounter <= 0)
                {
                    QuickStepping = false;
                }
            }
        }

    }


    private void JumpAgain()
    {
        //jumpCount++;
        //InitialNormal = CharacterAnimator.transform.up;

        //SnapOutOfGround to make sure you do jump
        transform.position += (InitialNormal * 0.3f);

        Player.rb.velocity = new Vector3(Player.rb.velocity.x * 0.92f, Player.rb.velocity.y * 0.1f, Player.rb.velocity.z * 0.92f);

        GameObject JumpDashParticleClone = Instantiate(Tools.JumpDashParticle, Tools.FeetPoint.position, Quaternion.identity) as GameObject;

        JumpDashParticleClone.GetComponent<ParticleSystem>().startSize = 1f;

        JumpDashParticleClone.transform.position = Tools.FeetPoint.position;
        JumpDashParticleClone.transform.rotation = Quaternion.LookRotation(Vector3.up);

    }

    //Reponsible for assigning stats from the stats script.
    private void AssignStats()
    {
        StartJumpDuration = Tools.stats.StartJumpDuration;
        StartJumpSpeed = Tools.stats.StartJumpSpeed;
        StartSlopedJumpDuration = Tools.stats.StartSlopedJumpDuration;
        JumpSlopeConversion = Tools.stats.JumpSlopeConversion;
        RollingLandingBoost = Tools.stats.JumpRollingLandingBoost;
        StopYSpeedOnRelease = Tools.stats.StopYSpeedOnRelease;

        canDoubleJump = Tools.stats.canDoubleJump;
        canTripleJump = Tools.stats.canTripleJump;
        doubleJumpDuration = Tools.stats.doubleJumpDuration;
        doubleJumpSpeed = Tools.stats.doubleJumpSpeed;

        airStepSpeed = Tools.stats.AirStepSpeed;
        StepDistance = Tools.stats.AirStepDistance;
        StepPlayermask = Tools.stats.StepLayerMask;

        wallLayerMask = Tools.stats.wallLayerMask;
        WallCheckDistance = Tools.stats.WallCheckDistance;
    }

    //Responsible for assigning objects and components from the tools script.
    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();
        homingControl = GetComponent<HomingAttackControl>();
        WallRun = Actions.Action12;

        CharacterAnimator = Tools.CharacterAnimator;
        sounds = Tools.SoundControl;
        JumpBall = Tools.JumpBall;
    }
}
