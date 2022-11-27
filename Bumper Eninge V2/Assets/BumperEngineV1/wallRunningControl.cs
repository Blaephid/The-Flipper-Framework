using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wallRunningControl : MonoBehaviour
{
    CharacterTools Tools;

    Action01_Jump JumpAction;
    Action00_Regular RegularAction;
    PlayerBhysics Player;
    ActionManager Actions;
    HomingAttackControl homingControl;
    CameraControl Cam;

    SonicSoundsControl sounds;
    Animator CharacterAnimator;
    public float skinRotationSpeed;
    GameObject JumpBall;

    [HideInInspector] public GameObject bannedWall;

    [Header("Detecting Wall Run")]
    bool canCheck = false;
    Action12_WallRunning WallRun;
    float WallCheckDistance;
    LayerMask wallLayerMask;
    float CheckModifier;

    [HideInInspector] public float checkSpeed;
    Vector3 saveVec;

    private RaycastHit leftWallDetect;
    private bool wallLeft;
    private RaycastHit rightWallDetect;
    private bool wallRight;
    private RaycastHit frontWallDetect;
    private bool wallFront;

    [Header ("Quickstepping")]
    bool StepRight;
    float StepDistance = 50f;
    float DistanceToStep;


    // Start is called before the first frame update
    void Start()
    {
        //Assigns tool and stats for later use.
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //If jumping, step stats reflect jump versions.
        if (Actions.Action == 1)
        {
            //StepRight = JumpAction.StepRight;
            //StepDistance = JumpAction.StepDistance;
            //DistanceToStep = JumpAction.DistanceToStep;
            canCheck = true;
            //  Debug.Log("Can check");
        }
        //If moving normally, step stats refelct normal versions.
        else if (Actions.Action == 0)
        {
            //StepRight = RegularAction.StepRight;
            //StepDistance = RegularAction.airStepDistance;
            //DistanceToStep = RegularAction.DistanceToStep;
            canCheck = true;
        }
        //Manages what actions can and cannot traverse into a wall run.
        else if (Actions.Action == 6 || Actions.Action == 11)
            canCheck = true;

        else canCheck = false;
        

        if (Player.Grounded)
        {
            bannedWall = null;
        }
        
   
    }
    private void FixedUpdate()
    {
        //If able, check for wall run. Player must be in air, pressing skid, and able to.
        if (Actions.SkidPressed)
        {
            if (canCheck)
                checkWallRun();
        }
        checkSpeed = Player.HorizontalSpeedMagnitude;
        saveVec = Player.rb.velocity;
    }


    //Responsible for swtiching to wall run if specifications are met.
    private void checkWallRun()
    {

        //If High enough above ground and not at an odd rotation
        if (!Player.Grounded)
        {

            //Checks for nearby walls using raycasts
            CheckForWall();

            //If detecting a wall in front with a near horizontal normal
            if (wallFront && frontWallDetect.normal.y <= 0.3 && frontWallDetect.normal.y >= -0.2 && checkSpeed > 20f)
            {
                //If facing the wall enough
                if (Vector3.Dot(CharacterAnimator.transform.forward, frontWallDetect.normal) < -0.85f)
                {
                    //Enter wall run as a climb
                    if (Actions.eventMan != null) Actions.eventMan.wallClimbsPerformed += 1;

                    WallRun.InitialEvents(true, frontWallDetect, false, WallCheckDistance * CheckModifier);
                    Actions.ChangeAction(12);
                }

            }

            //If detecting a wall to the side

            //If detecting a wall on left with correct angle.
            else if (wallLeft && DistanceToStep < StepDistance / 2  && leftWallDetect.normal.y <= 0.4 && checkSpeed > 28f &&
                leftWallDetect.normal.y >= -0.4 && !(DistanceToStep > 0 && StepRight))
            {
                //Enter a wallrun with wall on left.
                WallRun.InitialEvents(false, leftWallDetect, false);
                if (Actions.eventMan != null) Actions.eventMan.wallRunsPerformed += 1;
                Actions.ChangeAction(12);
            }

            //If detecting a wall on right with correct angle.
            else if (wallRight && DistanceToStep < StepDistance / 2 && rightWallDetect.normal.y <= 0.4 && checkSpeed > 28f &&
                rightWallDetect.normal.y >= -0.4 && !(DistanceToStep > 0 && !StepRight))
            {
                //Enter a wallrun with wall on right.
                WallRun.InitialEvents(false, rightWallDetect, true);
                if (Actions.eventMan != null) Actions.eventMan.wallRunsPerformed += 1;
                Actions.ChangeAction(12);
            }
        }
    }


    private bool enoughAboveGround()
    {
        //If racycast does not detect ground
        return !Physics.Raycast(CharacterAnimator.transform.position, -Vector3.up, 5f, wallLayerMask);
    }

    private void CheckForWall()
    {
        //Checks for wall in front using raycasts, outputing hits and booleans
        wallFront = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), CharacterAnimator.transform.forward, out frontWallDetect,
            WallCheckDistance * 2.5f, wallLayerMask);

        //Checks for nearby walls using raycasts, outputing hits and booleans
        wallRight = Physics.Raycast(CharacterAnimator.transform.position, CharacterAnimator.transform.right, out rightWallDetect, WallCheckDistance, wallLayerMask);
        wallLeft = Physics.Raycast(CharacterAnimator.transform.position, -CharacterAnimator.transform.right, out leftWallDetect, WallCheckDistance, wallLayerMask);

        //If no walls directily on sides, checks at angles with greater range.
        if (!wallRight && !wallLeft && !wallFront)
        {
            //Checks for wall on right first. Sets angle between right and forward and uses it.
            Vector3 direction = Vector3.Lerp(CharacterAnimator.transform.right, CharacterAnimator.transform.forward, 0.4f);
            wallRight = Physics.Raycast(CharacterAnimator.transform.position, direction, out rightWallDetect, WallCheckDistance * 2, wallLayerMask);

            //If no wall on right, checks left.
            if (!wallRight)
            {
                //Same as before but left
                direction = Vector3.Lerp(-CharacterAnimator.transform.right, CharacterAnimator.transform.forward, 0.4f);
                wallLeft = Physics.Raycast(CharacterAnimator.transform.position, direction, out leftWallDetect, WallCheckDistance * 2, wallLayerMask);

                //If they find the wall, apply force towards it.
                if (wallLeft)
                {
                    //Player.p_rigidbody.AddForce(direction * 20f);
                }
                else
                {
                    //If there isn't a wall and moving fast enough
                    if (!wallFront)
                    {
                        //Increases check range based on speed
                        CheckModifier = (Player.HorizontalSpeedMagnitude * 0.035f) + .5f;
                        wallFront = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), CharacterAnimator.transform.forward, out frontWallDetect,
                        WallCheckDistance * CheckModifier, wallLayerMask);

                    }
                }
            }
            //If they find the wall, apply force towards it.
            else if (wallRight)
            {
                //Player.p_rigidbody.AddForce(direction * 20f);
            }
        }

        
        

        //Checks if the wall can be used. Banned walls are set when the player jumps off the wall.
        if (wallFront)
        {
            if (frontWallDetect.collider.gameObject == bannedWall)
                wallFront = false;
        }
        if (wallRight)
        {
            if (rightWallDetect.collider.gameObject == bannedWall)
                wallRight = false;
        }
        if (wallLeft)
        {
            if (leftWallDetect.collider.gameObject == bannedWall)
                wallLeft = false;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.layer == 0)
        {
            StartCoroutine(buffering());
        }
    }

    IEnumerator buffering()
    {
        Vector3 theVec = saveVec;
        float theSpeed = checkSpeed;

        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForFixedUpdate();
            saveVec = theVec;
            checkSpeed = theSpeed;
        }
    }


    //Reponsible for assigning stats from the stats script.
    private void AssignStats()
    {

        wallLayerMask = Tools.coreStats.wallLayerMask;
        WallCheckDistance = Tools.coreStats.WallCheckDistance;
    }

    //Responsible for assigning objects and components from the tools script.
    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();
        homingControl = GetComponent<HomingAttackControl>();
        WallRun = Actions.Action12;
        JumpAction = Actions.Action01;
        RegularAction = Actions.Action00;

        CharacterAnimator = Tools.CharacterAnimator;
        sounds = Tools.SoundControl;
        JumpBall = Tools.JumpBall;
    }
}
