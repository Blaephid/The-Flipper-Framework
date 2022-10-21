using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action12_WallRunning : MonoBehaviour
{
    CharacterTools Tools;
    CharacterStats stats;

    [Header("Basic Resources")]
    Animator CharacterAnimator;
    PlayerBhysics Player;
    PlayerBinput Input;
    ActionManager Actions;
    SonicSoundsControl sounds;
    HomingAttackControl homingControl;
    CameraControl Cam;
    wallRunningControl Control;

    public float skinRotationSpeed;
    GameObject JumpBall;
    GameObject dropShadow;
    Transform camTarget;
    Transform constantTarget;
    CapsuleCollider coreCollider;

    Vector3 OriginalVelocity;

    GameObject currentWall;

    [Header("Wall Climbing")]
    bool Climbing;
    RaycastHit wallToClimb;
    [HideInInspector] public float ClimbingSpeed;
    float climbWallDistance;
    bool upwards;
    float scrapingSpeed;
    bool SwitchToGround;
    float SwitchToJump = 0;

    [Header("Wall Running")]
    bool Running;
    RaycastHit wallToRun;
    [HideInInspector] public float RunningSpeed;
    bool wallOnRight;

    [Header("Wall Rules")]
    float WallCheckDistance;
    float minHeight;
    LayerMask wallLayerMask;
    float wallDuration;
    float Counter;
    float distanceFromWall;

    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            stats = GetComponent<CharacterStats>();
            AssignStats();
            
        }
    }

    public void InitialEvents(bool Climb, RaycastHit wallHit, bool wallRight, float frontDistance = 1f)
    {

        //Debug.Log("wallrunning");

        //Universal varaibles
        SwitchToGround = false;
        JumpBall.SetActive(false);

        OriginalVelocity = Player.p_rigidbody.velocity;
        Player.p_rigidbody.velocity = Vector3.zero;
        distanceFromWall = coreCollider.radius * 1.15f;


        Counter = 0;
        Actions.JumpPressed = false;
        Player.GravityAffects = false;
        Cam.Cam.LockHeight = false ;

        //If entering a wallclimb
        if (Climb)
        {
            ClimbingSetup(wallHit,frontDistance);
        }

        //If wallrunning
        else
        {
            RunningSetup(wallHit, wallRight);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //Counter for how long on the wall
        Counter += Time.deltaTime;

        //Debug.Log(ClimbingSpeed);

        //If the player touches the ground by scraping down too much
        if (Player.Grounded || Physics.Raycast(CharacterAnimator.transform.position, -transform.up, .1f, wallLayerMask))
        {
            ExitWall(true);
        }

        if (Running)
        {
            RunningInteraction();
            
        }

        else if (Climbing)
        {
            ClimbingInteraction();
        }
        
    }

    private void FixedUpdate()
    {
        //Cancel action by letting go of skid after .5 seconds
        if (!Actions.SkidPressed && Counter > 0.9f && (Climbing || Running))
        {
            ExitWall(true);
        }

        //If Climbing
        else if (Climbing)
        {
            ClimbingPhysics();

        
        }

        else if (Running)
        {
            RunningPhysics();

        }

        //If going from climbing wall to running on flat ground normally.
        else if (SwitchToGround)
        {
            FromWallToGround();
        }

        else if (SwitchToJump > 0)
        {
            JumpfromWall();
        }
    }


    ///
    /// Setup on wall
    /// 

    void ClimbingSetup(RaycastHit wallHit, float frontDistance)
    {
        //Set wall and type of movement
        Climbing = true;
        Running = false;
        wallToClimb = wallHit;
        dropShadow.SetActive(false);

        //Set the climbing speed based on player's speed
        ClimbingSpeed = Player.HorizontalSpeedMagnitude * 0.5f;
        RunningSpeed = 0f;

        //If moving up, increases climbing speed
        if (OriginalVelocity.y > 0)
        {
            Cam.Cam.SetCamera(-wallHit.normal, 2f, -30, 0.001f, 30);
            Cam.Cam.CameraMaxDistance = Cam.InitialDistance - 3f;

            ClimbingSpeed += OriginalVelocity.y * 0.4f;
            scrapingSpeed = 5f;
            upwards = true;
        }
        //If falling, sets the player to scrape down first before climbing.
        else
        {
            Cam.Cam.SetCamera(-wallHit.normal, 2f, 20, 0.002f, 30);
            Cam.Cam.CameraMaxDistance = Cam.InitialDistance - 3f;

            //Debug.Log("Begin Scraping");
            scrapingSpeed = OriginalVelocity.y * 0.9f;
            upwards = false;
        }

        //Sets min and max climbing speed
        if (ClimbingSpeed < 30f)
            ClimbingSpeed = 30f;
        else if (ClimbingSpeed > 130f)
            ClimbingSpeed = 150f;

        climbWallDistance = frontDistance;

        //Set animations
        CharacterAnimator.SetInteger("Action", 0);
        //CharacterAnimator.SetBool("Grounded", true);
        CharacterAnimator.transform.rotation = Quaternion.LookRotation(-wallToClimb.normal, CharacterAnimator.transform.up);
    }

    void RunningSetup(RaycastHit wallHit, bool wallRight)
    {
        Vector3 wallDirection = wallHit.point - transform.position;
        Player.p_rigidbody.AddForce(wallDirection * 20f);

        transform.position = wallHit.point + -wallDirection.normalized * distanceFromWall;

        Running = true;
        Climbing = false;
        wallToRun = wallHit;

        CharacterAnimator.SetInteger("Action", 0);
        CharacterAnimator.SetBool("Grounded", true);

        ClimbingSpeed = 0f;
        RunningSpeed = Player.HorizontalSpeedMagnitude;
        scrapingSpeed = Player.p_rigidbody.velocity.y * 0.7f;

        //If running with the wall on the right
        if (wallRight)
        {
            wallOnRight = true;
            CharacterAnimator.transform.right = wallDirection.normalized;
        }
        //If running with the wall on the left
        else
        {
            wallOnRight = false;
            CharacterAnimator.transform.right = -wallDirection.normalized;
        }

        //Camera
        Vector3 newCamPos = transform.position + (wallHit.normal.normalized * 1.1f);
        newCamPos.y = transform.position.y + 2f;
        camTarget.position = newCamPos;
        Cam.Cam.SetCamera(CharacterAnimator.transform.forward, 2f, 0, 0.001f, 30);
        Cam.Cam.CameraMaxDistance = Cam.InitialDistance - 2f;

    }




    ///
    /// Interacting with wall. Update.
    ///

    void ClimbingInteraction()
    {
        //Prevents normal movement in input and physics
        Input.LockInputForAWhile(0f, false);

        //Updates the status of the wall being climbed.
        bool wall;
        if (Counter < 0.3f)
            wall = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), CharacterAnimator.transform.forward, out wallToClimb, climbWallDistance * 1.3f, wallLayerMask);
        else
            wall = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), CharacterAnimator.transform.forward, out wallToClimb, 3f, wallLayerMask);

        //If jumping off wall
        if (Actions.JumpPressed)
        {
            transform.position = new Vector3(wallToClimb.point.x + wallToClimb.normal.x * 2.5f, wallToClimb.point.y + wallToClimb.normal.y * 1.5f, wallToClimb.point.z + wallToClimb.normal.z * 2.5f);

            //This bool causes the jump physics to be done next frame, making things much smoother. 1 Represents jumping from a wallrun
            SwitchToJump = 1;
            Climbing = false;
            Running = false;
        }

        //If they reach the top of the wall
        if (!wall)
        {
            Debug.Log("Lost Wall");
            CharacterAnimator.SetInteger("Action", 0);
            CharacterAnimator.SetBool("Grounded", false);

            //Bounces the player up to keep momentum
            StartCoroutine(JumpOverWall(CharacterAnimator.transform.rotation));

            //Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z);
            //CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, -Player.Gravity.normalized);
        }
        else
        {
            //Esnures the player faces the wall
            CharacterAnimator.transform.rotation = Quaternion.LookRotation(-wallToClimb.normal, CharacterAnimator.transform.up);
            currentWall = wallToClimb.collider.gameObject;
        }
    }

    void RunningInteraction()
    {
        //Prevents normal movement in input and physics
        Input.LockInputForAWhile(0f, false);
        bool wall;

        //If jumping off wall
        if (Actions.JumpPressed)
        {
            transform.position = new Vector3(wallToRun.point.x + wallToRun.normal.x * 2.5f, wallToRun.point.y + wallToRun.normal.y * 1.5f, wallToRun.point.z + wallToRun.normal.z * 2.5f);

            //This bool causes the jump physics to be done next frame, making things much smoother. 2 Represents jumping from a wallrun
            SwitchToJump = 2;
            Climbing = false;
            Running = false;
        }

        //Detect current wall
        if (wallOnRight)
        {
            if (Counter < 0.3f)
                wall = Physics.Raycast(transform.position, CharacterAnimator.transform.right, out wallToRun, WallCheckDistance, wallLayerMask);
            else
                wall = Physics.Raycast(transform.position, CharacterAnimator.transform.right, out wallToRun, WallCheckDistance * 2f, wallLayerMask);
        }
        else
        {
            if (Counter < 0.3f)
                wall = Physics.Raycast(transform.position, -CharacterAnimator.transform.right, out wallToRun, WallCheckDistance, wallLayerMask);
            else
                wall = Physics.Raycast(transform.position, -CharacterAnimator.transform.right, out wallToRun, WallCheckDistance * 2f, wallLayerMask);
        }

        if (!wall)
        {
            CharacterAnimator.SetInteger("Action", 0);
            CharacterAnimator.SetBool("Grounded", false);

            ExitWall(true);
        }
        else
            currentWall = wallToRun.collider.gameObject;
    }

    /// <summary>
    /// Physics for climing and runing on wall
    /// </summary>
    void ClimbingPhysics()
    {
        //After a short pause / when climbing
        if (Counter > 0.3f && upwards)
        {

            //After being on the wall for too long.
            if (ClimbingSpeed < -100f)
            {
                CharacterAnimator.SetInteger("Action", 0);
                //Debug.Log("Out of Speed");

                //Drops and send the player back a bit.
                Vector3 newVec = new Vector3(0f, ClimbingSpeed, 0f);
                newVec += (-CharacterAnimator.transform.forward * 6f);
                Player.p_rigidbody.velocity = newVec;

                CharacterAnimator.transform.rotation = Quaternion.LookRotation(-wallToClimb.normal, Vector3.up);
                //Input.LockInputForAWhile(10f, true);

                ExitWall(true);
            }

            else
            {
                //Debug.Log(ClimbingSpeed);

                //Ready the player velocity
                //Vector3 newVec = Player.p_rigidbody.velocity;
                Vector3 newVec = new Vector3(0f, ClimbingSpeed, 0f);
                newVec += (CharacterAnimator.transform.forward * 20f);
                Player.p_rigidbody.velocity = newVec;
            }

            //Adds a changing deceleration
            if (Counter > 1.2)
                ClimbingSpeed -= 1.8f + (Counter / 1.5f);
            else if (Counter > 0.9)
                ClimbingSpeed -= 2.3f;
            else if (Counter > 0.7)
                ClimbingSpeed -= 1.7f;
            else if (Counter > 0.4)
                ClimbingSpeed -= 1.2f;
            else
                ClimbingSpeed -= .7f;



            //Apply camera or animation based on direction
            if (ClimbingSpeed > 10f)
            {
                //Debug.Log("Change Cam");
                //Cam.Cam.FollowDirection(-Cam.Cam.y, Cam.Cam.HeightFollowSpeed);
                //Cam.Cam.FollowDirection(50f, 30f, -20f, 20f);#

            }

            else if (ClimbingSpeed < 0f)
            {
                Cam.Cam.FollowDirection(10f, 5f);

                //Decreases climbing speed decrease if climbing down.
                if (ClimbingSpeed < -40f)
                    ClimbingSpeed += 1.2f;
                else if (ClimbingSpeed < -1f)
                    ClimbingSpeed += .6f;
            }



            //If the wall stops being very steep
            if (wallToClimb.normal.y > 0.6 || wallToClimb.normal.y < -0.3)
            {
                CharacterAnimator.SetInteger("Action", 0);
                //Sets variables to go to swtich to ground option in FixedUpdate
                Climbing = false;
                Running = false;
                SwitchToGround = true;

                //Set rotation to put feet on ground.
                Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z), CharacterAnimator.transform.forward, out wallToClimb, climbWallDistance, wallLayerMask);
                Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z);
                CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, wallToClimb.normal);
            }

        }

        //If scraping down the wall
        else if (Counter > 0.3f)
        {
            Vector3 newVec = new Vector3(0f, scrapingSpeed, 0f);
            newVec += (CharacterAnimator.transform.forward * 8f);

            //Decreases scraping Speed
            scrapingSpeed *= 0.8f;
            if (ClimbingSpeed > 30f)
                ClimbingSpeed -= 0.2f;

            //After scraping enough, switch to climbing upwards
            if (scrapingSpeed > -8f)
            {
                Cam.Cam.SetCamera(CharacterAnimator.transform.forward, 2f, -20, 0.5f);
                Cam.Cam.CameraMaxDistance = Cam.InitialDistance - 3f;
                upwards = true;
            }

            else
                //Sets velocity
                Player.p_rigidbody.velocity = newVec;

            //Debug.Log("Scraping Speed is " + scrapingSpeed);
        }

        //Adds a little delay before the climb, to attatch to wall more and add a flow
        else
        {
            Vector3 newVec = new Vector3(0f, scrapingSpeed, 0f);
            if (CharacterAnimator.transform.rotation == Quaternion.LookRotation(-wallToClimb.normal, Vector3.up))
                newVec += (-wallToClimb.normal * 45f);
            else
                newVec = (wallToClimb.normal * 4f);

            //Decreases scraping Speed
            scrapingSpeed *= 0.95f;
            ClimbingSpeed -= 0.1f;


            //Sets velocity
            Player.p_rigidbody.velocity = newVec;
        }
    }

    void FromWallToGround()
    {
        Player.GravityAffects = true;
        Debug.Log("Switching to ground running");

        //Set rotation to put feet on ground.
        Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z), -CharacterAnimator.transform.up, out wallToClimb, climbWallDistance, wallLayerMask);
        Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z);
        CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, wallToClimb.normal);

        //Set velocity to move along and push down to the ground
        Vector3 newVec = CharacterAnimator.transform.forward * (ClimbingSpeed);
        newVec += -wallToClimb.normal * 10f;

        Player.p_rigidbody.velocity = newVec;

        //Actions.ChangeAction(0);
    }

    void RunningPhysics()
    {
        Vector3 wallNormal = wallToRun.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);


        if ((CharacterAnimator.transform.forward - wallForward).sqrMagnitude > (CharacterAnimator.transform.forward - -wallForward).sqrMagnitude)
            wallForward = -wallForward;

        //Set direction facing
        CharacterAnimator.transform.rotation = Quaternion.LookRotation(wallForward, CharacterAnimator.transform.up);

        //Decide speed to slide down wall.
        if (scrapingSpeed > 10 && scrapingSpeed < 20)
        {
            scrapingSpeed *= 1.011f;
        }
        else if (scrapingSpeed > 29)
        {
            scrapingSpeed *= 1.018f;
        }
        else if (scrapingSpeed > 2)
        {
            scrapingSpeed += (Time.deltaTime * 10f);
        }
        else
        {
            scrapingSpeed += (Time.deltaTime * 5f);
        }

        //Apply scraping speed
        Vector3 newVec = wallForward * RunningSpeed;
        newVec = new Vector3(newVec.x, -scrapingSpeed, newVec.z);

        
        Player.p_rigidbody.velocity = newVec;

        //Applying force against wall for when going round curves on the outside.
        float forceToWall = RunningSpeed;
        if (forceToWall > 100)
            forceToWall *= 1.2f;
        else if (forceToWall > 150)
            forceToWall *= 1.4f;
        else if (forceToWall > 200)
            forceToWall *= 1.6f;

        //
        Player.p_rigidbody.AddForce(-wallNormal * (RunningSpeed * 1.3f));
        if (Counter < 0.3f)
            Player.p_rigidbody.AddForce(-wallNormal * 2f);

        //Debug.Log(scrapingSpeed);
        //Debug.Log(Player.p_rigidbody.velocity.y);
    }


    /// <summary>
    /// Other
    /// </summary>
    /// 
    void ExitWall(bool immediately)
    {
        Control.bannedWall = currentWall;

        Actions.SkidPressed = false;

        dropShadow.SetActive(true);
        Cam.Cam.CameraMaxDistance = Cam.InitialDistance;
        Player.GravityAffects = true;
        Cam.Cam.LockHeight = true;
        camTarget.position = constantTarget.position;

        if (immediately && Actions.Action != 1)
            Actions.ChangeAction(0);
    }

    void JumpfromWall()
    {
        Vector3 jumpAngle;
     
        

        if (SwitchToJump == 2)
        {
            jumpAngle = Vector3.Lerp(wallToRun.normal, transform.up, 0.8f);
            //Debug.Log(jumpAngle);
            if (wallOnRight)
                Player.p_rigidbody.AddForce(transform.right * 2f);
            else
                Player.p_rigidbody.AddForce(-transform.right * 2f);
        }
        else
        {
            jumpAngle = Vector3.Lerp(wallToRun.normal, transform.up, 0.6f);
            //Debug.Log(jumpAngle);
            Player.p_rigidbody.AddForce(-transform.forward * 2f);
        }

        SwitchToJump = 0;
        ExitWall(false);

        Actions.Action01.jumpCount = -1;
        Actions.Action01.InitialEvents(jumpAngle);
        Actions.ChangeAction(1);
    }

    IEnumerator JumpOverWall(Quaternion originalRotation, float jumpOverCounter = 0)
    {
        float jumpSpeed = Player.p_rigidbody.velocity.y * 0.6f;
        if (jumpSpeed < 5) jumpSpeed = 5;

        Player.p_rigidbody.velocity = CharacterAnimator.transform.up * jumpSpeed;

        ExitWall(false);
        Input.LockInputForAWhile(25f, false);

        while (true)
        {
            jumpOverCounter += 1;
            CharacterAnimator.transform.rotation = originalRotation;
            yield return new WaitForSeconds(0.0f);
            if ((!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.6f, transform.position.z), CharacterAnimator.transform.forward, out wallToClimb, climbWallDistance * 1.3f, wallLayerMask)) || jumpOverCounter == 40)
            {
                //Vector3 newVec = Player.p_rigidbody.velocity + CharacterAnimator.transform.forward * (ClimbingSpeed * 0.1f);
                Player.p_rigidbody.velocity += CharacterAnimator.transform.forward * 8;
                if (Actions.RollPressed)
                {
                    Actions.ChangeAction(8);
                    Actions.Action08.InitialEvents();
                    break;
                }

                else
                {
                    if (Actions.Action != 1)
                        Actions.ChangeAction(0);
                    break;
                }
            }

        }
    }


    //Reponsible for assigning stats from the stats script.
    void AssignStats()
    {
        WallCheckDistance = stats.WallCheckDistance;
        minHeight = stats.minHeight;
        wallLayerMask = stats.wallLayerMask;
        wallDuration = stats.wallDuration;
    }

    //Responsible for assigning objects and components from the tools script.
    void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();
        Input = GetComponent<PlayerBinput>();
        Control = GetComponent<wallRunningControl>();

        CharacterAnimator = Tools.CharacterAnimator;
        sounds = Tools.SoundControl;
        JumpBall = Tools.JumpBall;
        dropShadow = Tools.dropShadow;
        camTarget = Tools.cameraTarget;
        constantTarget = Tools.constantTarget;
        coreCollider = Tools.characterCapsule.GetComponent<CapsuleCollider>();
    }
}
