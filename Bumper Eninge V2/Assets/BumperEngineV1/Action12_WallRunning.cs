using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action12_WallRunning : MonoBehaviour
{
    CharacterTools Tools;

    [Header("Basic Resources")]
    Animator CharacterAnimator;
    Transform characterTransform;
    PlayerBhysics Player;
    PlayerBinput Inp;
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
    float scrapingSpeed;
    bool SwitchToGround;
    float SwitchToJump = 0;

    [Header("Wall Running")]
    bool Running;
    RaycastHit wallToRun;
    [HideInInspector] public float RunningSpeed;
    bool wallOnRight;

    [Header("Wall Rules")]
    bool holdingWall;
    float WallCheckDistance;
    float minHeight;
    LayerMask wallLayerMask;
    float wallDuration;
    float Counter;
    float distanceFromWall;

    [Header("Wall Stats")]
    float scrapeModi = 1f;
    float climbModi = 1f;

    bool wall;
    Vector3 previDir;
    Vector3 previLoc;


    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();
            AssignStats();
            
        }
    }

    private void OnDisable()
    {
        ExitWall(false);
    }

    public void InitialEvents(bool Climb, RaycastHit wallHit, bool wallRight, float frontDistance = 1f)
    {
        wall = true;

        //Debug.Log("wallrunning");

        //Universal varaibles
        SwitchToGround = false;
        JumpBall.SetActive(false);

        OriginalVelocity = Player.rb.velocity;
        Player.rb.velocity = Vector3.zero;
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

        if(wall)
        {
            if (Running)
            {
                RunningInteraction();

            }

            else if (Climbing)
            {
                ClimbingInteraction();
            }
        }

        //Debug.Log(characterTransform.eulerAngles);



    }

    private void FixedUpdate()
    {
        //Cancel action by letting go of skid after .5 seconds
        if ((!holdingWall && Counter > 0.9f && (Climbing || Running)) || Player.Grounded)
        {
            if (Running && !Player.Grounded)
                StartCoroutine(loseWall());
            else
                ExitWall(true);
        }

        else if(wall)
        {
            //If Climbing
            if (Climbing)
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

        }
        else if (SwitchToJump > 0)
        {
            JumpfromWall();
        }

    }

    bool inputtingToWall(Vector3 wallDirection)
    {
        Vector3 transformedInput;
        transformedInput = (CharacterAnimator.transform.rotation * Inp.inputPreCamera);
        transformedInput = transform.InverseTransformDirection(transformedInput);
        transformedInput.y = 0.0f;
        //Debug.DrawRay(transform.position, transformedInput * 10, Color.red);

        if (Inp.trueMoveInput.sqrMagnitude > 0.4f)
        {
            //Debug.Log(Vector3.Dot(wallDirection, Inp.trueMoveInput));
            if(Vector3.Dot(wallDirection.normalized, Inp.trueMoveInput.normalized) > 0.05f)
            {
                return true;
            }
            else
            {
                if(Vector3.Dot(wallDirection.normalized, transformedInput.normalized) > 0.05f)
                {
                    return true;
                }
            }
        }
        return false;
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
        ClimbingSpeed = Player.HorizontalSpeedMagnitude * 0.8f;
        ClimbingSpeed *= climbModi;
        RunningSpeed = 0f;

        //If moving up, increases climbing speed
       
        //Cam.Cam.SetCamera(-wallHit.normal, 2f, -30, 0.001f, 30);
       // Cam.Cam.CameraMaxDistance = Cam.InitialDistance - 3f;
 
        scrapingSpeed = 5f;


        //Sets min and max climbing speed
        ClimbingSpeed = 8f * (int)(ClimbingSpeed / 8);
        ClimbingSpeed = Mathf.Clamp(ClimbingSpeed, 48, 176);

        climbWallDistance = frontDistance;

        //Set animations
        CharacterAnimator.SetInteger("Action", 1);
        //CharacterAnimator.SetBool("Grounded", true);
        CharacterAnimator.transform.rotation = Quaternion.LookRotation(-wallToClimb.normal, CharacterAnimator.transform.up);
    }

    void RunningSetup(RaycastHit wallHit, bool wallRight)
    {
        Vector3 wallDirection = wallHit.point - transform.position;
        Player.rb.AddForce(wallDirection * 10f);

        transform.position = wallHit.point + (wallHit.normal * distanceFromWall);

        Running = true;
        Climbing = false;
        wallToRun = wallHit;

        CharacterAnimator.SetInteger("Action", 14);
        //CharacterAnimator.SetBool("Grounded", true);

        ClimbingSpeed = 0f;
        RunningSpeed = Player.HorizontalSpeedMagnitude;
        scrapingSpeed = Player.rb.velocity.y * 0.7f;

        //If running with the wall on the right
        if (wallRight)
        {
            wallOnRight = true;
            //CharacterAnimator.transform.right = wallDirection.normalized;

            Vector3 wallForward = Vector3.Cross(wallHit.normal, transform.up);
            if ((CharacterAnimator.transform.forward - wallForward).sqrMagnitude > (CharacterAnimator.transform.forward - -wallForward).sqrMagnitude)
                wallForward = -wallForward;

            //Set direction facing
            CharacterAnimator.transform.rotation = Quaternion.LookRotation(wallForward, transform.up);
            //characterTransform.rotation = Quaternion.LookRotation(wallForward, Vector3.Lerp(transform.up, wallHit.normal, 0.2f));
        }
        //If running with the wall on the left
        else
        {
            wallOnRight = false;
            //CharacterAnimator.transform.right = wallDirection.normalized;
            Vector3 wallForward = Vector3.Cross(wallHit.normal, transform.up);
            if ((CharacterAnimator.transform.forward - wallForward).sqrMagnitude > (CharacterAnimator.transform.forward - -wallForward).sqrMagnitude)
                wallForward = -wallForward;

            //Set direction facing
            CharacterAnimator.transform.rotation = Quaternion.LookRotation(wallForward, transform.up);
            //characterTransform.rotation = Quaternion.LookRotation(wallForward, Vector3.Lerp(transform.up, wallHit.normal, 0.2f));
        }

        //Camera
        Vector3 newCamPos = camTarget.position + (wallHit.normal.normalized * 1.8f);
        newCamPos.y += 3f;
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
        Inp.LockInputForAWhile(0f, false);

        //Updates the status of the wall being climbed.
        if (Counter < 0.3f)
            wall = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), CharacterAnimator.transform.forward, out wallToClimb, climbWallDistance * 1.3f, wallLayerMask);
        else
            wall = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), CharacterAnimator.transform.forward, out wallToClimb, 3f, wallLayerMask);

        //If they reach the top of the wall
        if (!wall)
        {
            Debug.Log("Lost Wall");
            Debug.DrawRay(new Vector3(transform.position.x, transform.position.y -0.3f, transform.position.z), CharacterAnimator.transform.forward * climbWallDistance * 1.3f, Color.red, 20f);
            CharacterAnimator.SetInteger("Action", 0);
            CharacterAnimator.SetBool("Grounded", false);

            //Bounces the player up to keep momentum
            StartCoroutine(JumpOverWall(CharacterAnimator.transform.rotation));

            //Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z);
            //CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, -Player.Gravity.normalized);
        }
        else
        {
            
            holdingWall = inputtingToWall(wallToClimb.point - transform.position);
            //Esnures the player faces the wall
            CharacterAnimator.transform.rotation = Quaternion.LookRotation(-wallToClimb.normal, CharacterAnimator.transform.up);
            previDir = CharacterAnimator.transform.forward;
            currentWall = wallToClimb.collider.gameObject;
        }

        //If jumping off wall
        if (Actions.JumpPressed)
        {
            wall = false;

            transform.position = wallToClimb.point + (wallToClimb.normal * 4f);

            //This bool causes the jump physics to be done next frame, making things much smoother. 1 Represents jumping from a wallrun
            SwitchToJump = 1;
            Climbing = false;
            Running = false;
        }

    }

    void RunningInteraction()
    {
        //Prevents normal movement in input and physics
        Inp.LockInputForAWhile(0f, false);

        CharacterAnimator.SetFloat("GroundSpeed", RunningSpeed);
        CharacterAnimator.SetBool("WallRight", wallOnRight);

        //Detect current wall
        if (wallOnRight)
        {
            if (Counter < 0.3f)
                wall = Physics.Raycast(transform.position, CharacterAnimator.transform.right, out wallToRun, WallCheckDistance * 2.5f, wallLayerMask);
            else
            {
                wall = Physics.Raycast(transform.position, CharacterAnimator.transform.right, out wallToRun, WallCheckDistance * 1.6f, wallLayerMask);

                if(!wall)
                {
                    Vector3 backPos = Vector3.Lerp(transform.position, previLoc, 0.7f);
                    wall = Physics.Raycast(backPos, CharacterAnimator.transform.right, out wallToRun, WallCheckDistance * 2.1f, wallLayerMask);
                }
            }              
        }
        else
        {
            if (Counter < 0.3f)
                wall = Physics.Raycast(transform.position, -CharacterAnimator.transform.right, out wallToRun, WallCheckDistance * 2.5f, wallLayerMask);
            else
            {
                wall = Physics.Raycast(transform.position, -CharacterAnimator.transform.right, out wallToRun, WallCheckDistance * 1.6f, wallLayerMask);
                if (!wall)
                {
                    Vector3 backPos = Vector3.Lerp(transform.position, previLoc, 0.8f);
                    wall = Physics.Raycast(backPos, -CharacterAnimator.transform.right, out wallToRun, WallCheckDistance * 2.1f, wallLayerMask);
                }
            }
                
        }

        if (!wall)
        {
            CharacterAnimator.SetInteger("Action", 0);
            CharacterAnimator.SetBool("Grounded", false);

            StartCoroutine(loseWall());

            //Debug.Log("Lost the Wall");
            if(wallOnRight)
                Debug.DrawRay(transform.position, CharacterAnimator.transform.right * WallCheckDistance * 2f, Color.blue, 20f);
            else
                Debug.DrawRay(transform.position, -CharacterAnimator.transform.right * WallCheckDistance * 2f, Color.blue, 20f);

        }
        else
        {
            Cam.Cam.FollowDirection(15, 14f, 0, 0);
            holdingWall = inputtingToWall(wallToRun.point - transform.position);
            currentWall = wallToRun.collider.gameObject;
        }
            

        //If jumping off wall
        if (Actions.JumpPressed)
        {
            wall = false;
            transform.position = new Vector3(wallToRun.point.x + wallToRun.normal.x * 0.9f, wallToRun.point.y + wallToRun.normal.y * 0.5f, wallToRun.point.z + wallToRun.normal.z * 0.9f);
            //CharacterAnimator.transform.forward = Vector3.Lerp(CharacterAnimator.transform.forward, wallToRun.normal, 0.3f);

            //This bool causes the jump physics to be done next frame, making things much smoother. 2 Represents jumping from a wallrun
            SwitchToJump = 2;
            Climbing = false;
            Running = false;
        }
    }

    /// <summary>
    /// Physics for climing and runing on wall
    /// </summary>
    void ClimbingPhysics()
    {
        //After a short pause / when climbing
        if (Counter > 0.15f)
        {

            //After being on the wall for too long.
            if (ClimbingSpeed < -5f || Physics.Raycast(transform.position, CharacterAnimator.transform.up, 5, wallLayerMask))
            {
                CharacterAnimator.SetInteger("Action", 0);
                //Debug.Log("Out of Speed");

                //Drops and send the player back a bit.
                Vector3 newVec = new Vector3(0f, ClimbingSpeed, 0f);
                newVec += (-CharacterAnimator.transform.forward * 6f);
                Player.rb.velocity = newVec;

                CharacterAnimator.transform.rotation = Quaternion.LookRotation(-wallToClimb.normal, Vector3.up);
                //Input.LockInputForAWhile(10f, true);

                ExitWall(true);
            }

            else
            {
                Vector3 newVec = new Vector3(0f, ClimbingSpeed, 0f);
                newVec += (CharacterAnimator.transform.forward * 20f);
                Player.rb.velocity = newVec;
            }

            //Adds a changing deceleration
            if (Counter > 1.2)
                ClimbingSpeed -= 2.5f;
            else if (Counter > 0.9)
                ClimbingSpeed -= 2.0f;
            else if (Counter > 0.7)
                ClimbingSpeed -= 1.5f;
            else if (Counter > 0.4)
                ClimbingSpeed -= 1.0f;
            else
                ClimbingSpeed -= 0.5f;


            //if (ClimbingSpeed < 0f)
            //{
            //    //Cam.Cam.FollowDirection(10f, 6f);

            //    //Decreases climbing speed decrease if climbing down.
            //    if (ClimbingSpeed < -40f)
            //        ClimbingSpeed += 1.2f;
            //    else if (ClimbingSpeed < -1f)
            //        ClimbingSpeed += .6f;
            //}


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
                Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
                CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, wallToClimb.normal);
            }

        }

        //Adds a little delay before the climb, to attatch to wall more and add a flow
        else
        {
            Vector3 newVec = new Vector3(0f, scrapingSpeed, 0f);
            if (CharacterAnimator.transform.rotation == Quaternion.LookRotation(-wallToClimb.normal, Vector3.up))
                newVec += (-wallToClimb.normal * 45f);
            //else
            //    newVec = (wallToClimb.normal * 4f);

            //Decreases scraping Speed
            scrapingSpeed *= 0.95f * scrapeModi;
            //ClimbingSpeed -= 0.1f;


            //Sets velocity
            Player.rb.velocity = newVec;
        }
    }

    void FromWallToGround()
    {
        Player.GravityAffects = true;
        Debug.Log("Switching to ground running");

        //Set rotation to put feet on ground.
        Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z), -CharacterAnimator.transform.up, out wallToClimb, climbWallDistance, wallLayerMask);
        Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
        CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, wallToClimb.normal);

        //Set velocity to move along and push down to the ground
        Vector3 newVec = CharacterAnimator.transform.forward * (ClimbingSpeed);
        newVec += -wallToClimb.normal * 10f;

        Player.rb.velocity = newVec;

        //Actions.ChangeAction(0);
    }

    void RunningPhysics()
    {
        Vector3 wallNormal = wallToRun.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);


        if ((CharacterAnimator.transform.forward - wallForward).sqrMagnitude > (CharacterAnimator.transform.forward - -wallForward).sqrMagnitude)
            wallForward = -wallForward;

        previDir = wallForward;
        previLoc = transform.position;

        //Set direction facing
        CharacterAnimator.transform.rotation = Quaternion.LookRotation(wallForward, transform.up);
        //characterTransform.rotation = Quaternion.LookRotation(wallForward, Vector3.Lerp(transform.up, wallNormal, 0.2f));


        //Decide speed to slide down wall.
        if (scrapingSpeed > 10 && scrapingSpeed < 20)
        {
            scrapingSpeed *= (1.001f * scrapeModi);
        }
        else if (scrapingSpeed > 29)
        {
            scrapingSpeed *= (1.0015f * scrapeModi);
        }
        else if (scrapingSpeed > 2)
        {
            scrapingSpeed += (1.0018f * scrapeModi);
        }
        else
        {
            scrapingSpeed += (1.002f * scrapeModi);
        }

        //Apply scraping speed
        Vector3 newVec = wallForward * RunningSpeed;
        newVec = new Vector3(newVec.x, -scrapingSpeed, newVec.z);




        //Applying force against wall for when going round curves on the outside.
        float forceToWall = 1f;
        if (RunningSpeed > 100)
            forceToWall += RunningSpeed / 7;
        else if (RunningSpeed > 150)
            forceToWall += RunningSpeed / 8;
        else if (RunningSpeed > 200)
            forceToWall += RunningSpeed / 9;
        else
            forceToWall += RunningSpeed / 10;

        //
        newVec += forceToWall * -wallNormal;
        if (Counter < 0.3f)
            newVec += -wallNormal * 3;

        Player.rb.velocity = newVec;

        //Debug.Log(scrapingSpeed);
        //Debug.Log(Player.p_rigidbody.velocity.y);
    }


    /// <summary>
    /// Other
    /// </summary>
    /// 

    IEnumerator loseWall()
    {
        Vector3 newVec = previDir * RunningSpeed;
        yield return null;

        CharacterAnimator.transform.forward = newVec.normalized;
        Player.rb.velocity = newVec;
        ExitWall(true);
    }

    void ExitWall(bool immediately)
    {
        Control.bannedWall = currentWall;

        //Actions.SkidPressed = false;

        dropShadow.SetActive(true);
        Cam.Cam.CameraMaxDistance = Cam.InitialDistance;
        Player.GravityAffects = true;
        Cam.Cam.LockHeight = true;
        camTarget.position = constantTarget.position;
        CharacterAnimator.transform.rotation = Quaternion.identity;
        if(previDir != Vector3.zero)
            CharacterAnimator.transform.forward = previDir;
        //characterTransform.up = CharacterAnimator.transform.up;
       
        characterTransform.localEulerAngles = Vector3.zero;

       

        if (immediately && Actions.Action != ActionManager.States.Jump)
            Actions.ChangeAction(ActionManager.States.Regular);
    }

    void JumpfromWall()
    {
        Vector3 jumpAngle;
        Vector3 faceDir;

        if (SwitchToJump == 2)
        {

            jumpAngle = Vector3.Lerp(wallToRun.normal, transform.up, 0.8f);

            Vector3 wallNormal = wallToRun.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);


            if ((CharacterAnimator.transform.forward - wallForward).sqrMagnitude > (CharacterAnimator.transform.forward - -wallForward).sqrMagnitude)
                wallForward = -wallForward;

            
            Vector3 newVec = wallForward;
            

            //Debug.Log(jumpAngle);
            if (wallOnRight)
            {
                newVec = Vector3.Lerp(newVec, -CharacterAnimator.transform.right, 0.25f);
                faceDir = Vector3.Lerp(newVec, -CharacterAnimator.transform.right, 0.1f);
                newVec *= RunningSpeed;
                //newVec += (-CharacterAnimator.transform.right * 0.3f);
            }
            else
            {
                newVec = Vector3.Lerp(newVec, CharacterAnimator.transform.right, 0.25f);
                faceDir = Vector3.Lerp(newVec, CharacterAnimator.transform.right, 0.1f);
                newVec *= RunningSpeed;
                //newVec += (CharacterAnimator.transform.right * 0.3f);
            }

            //CharacterAnimator.transform.forward = newVec.normalized;
            Player.rb.velocity = newVec;

        }
        else
        {
            Debug.Log(Vector3.Dot(wallToClimb.normal, Inp.trueMoveInput));
            Debug.Log(ClimbingSpeed);

            jumpAngle = Vector3.Lerp(wallToClimb.normal, transform.up, 0.6f);
            faceDir = wallToClimb.normal;

            Debug.DrawRay(transform.position, faceDir, Color.red, 20);

            Player.rb.velocity = faceDir * 4f;
        }

        SwitchToJump = 0;
        ExitWall(false);

        CharacterAnimator.transform.forward = faceDir;

        //Actions.Action01.jumpCount = -1;
        Actions.Action01.InitialEvents(jumpAngle, true, 0, Mathf.Clamp(ClimbingSpeed, 5, ClimbingSpeed));
        Actions.ChangeAction(ActionManager.States.Jump);
    }

    IEnumerator JumpOverWall(Quaternion originalRotation, float jumpOverCounter = 0)
    {
        float jumpSpeed = Player.rb.velocity.y * 0.6f;
        if (jumpSpeed < 5) jumpSpeed = 5;

        Player.rb.velocity = CharacterAnimator.transform.up * jumpSpeed;

        ExitWall(false);
        Inp.LockInputForAWhile(25f, false);

        while (true)
        {
            jumpOverCounter += 1;
            CharacterAnimator.transform.rotation = originalRotation;
            yield return new WaitForSeconds(0.0f);
            if ((!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.6f, transform.position.z), CharacterAnimator.transform.forward, out wallToClimb, climbWallDistance * 1.3f, wallLayerMask)) || jumpOverCounter == 40)
            {
                //Vector3 newVec = Player.p_rigidbody.velocity + CharacterAnimator.transform.forward * (ClimbingSpeed * 0.1f);
                Player.rb.velocity += CharacterAnimator.transform.forward * 8;
                if (Actions.RollPressed)
                {
                    Actions.Action08.TryDropCharge();
                    break;
                }

                else
                {
                    if (Actions.Action != ActionManager.States.Jump)
                        Actions.ChangeAction(ActionManager.States.Regular);
                    break;
                }
            }

        }
    }


    //Reponsible for assigning stats from the stats script.
    void AssignStats()
    {
        WallCheckDistance = Tools.coreStats.WallCheckDistance;
        minHeight = Tools.coreStats.minHeight;
        wallLayerMask = Tools.coreStats.wallLayerMask;
        wallDuration = Tools.coreStats.wallDuration;

        scrapeModi = Tools.stats.scrapeModi;
        climbModi = Tools.stats.climbModi;
    }

    //Responsible for assigning objects and components from the tools script.
    void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();
        Inp = GetComponent<PlayerBinput>();
        Control = GetComponent<wallRunningControl>();

        CharacterAnimator = Tools.CharacterAnimator;
        characterTransform = Tools.PlayerSkinTransform;
        sounds = Tools.SoundControl;
        JumpBall = Tools.JumpBall;
        dropShadow = Tools.dropShadow;
        camTarget = Tools.cameraTarget;
        constantTarget = Tools.constantTarget;
        coreCollider = Tools.characterCapsule.GetComponent<CapsuleCollider>();
    }
}
