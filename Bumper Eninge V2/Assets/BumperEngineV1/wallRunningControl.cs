using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wallRunningControl : MonoBehaviour
{
    CharacterTools Tools;

    Action01_Jump JumpAction;
    Action00_Regular RegularAction;
    PlayerBhysics Player;
    PlayerBinput Inp;
    ActionManager Actions;
    HomingAttackControl homingControl;
    CameraControl Cam;

    SonicSoundsControl sounds;
    Animator CharacterAnimator;
    public float skinRotationSpeed;
    GameObject JumpBall;

    [HideInInspector] public GameObject bannedWall;

    [Header("Detecting Wall Run")]
    Action12_WallRunning WallRun;
    float WallCheckDistance;
    LayerMask wallLayerMask;
    float CheckModifier = 1;

    [HideInInspector] public float checkSpeed;
    Vector3 saveVec;

    private RaycastHit leftWallDetect;
    private bool wallLeft;
    private RaycastHit rightWallDetect;
    private bool wallRight;
    private RaycastHit frontWallDetect;
    private bool wallFront;



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

        StartCoroutine(checkWallRun());
    }



    //Responsible for swtiching to wall run if specifications are met.
    private IEnumerator checkWallRun()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            //Debug.DrawRay(transform.position, Player.MoveInput, Color.red);


            checkSpeed = Player.HorizontalSpeedMagnitude;
            saveVec = Player.rb.velocity;

            //If High enough above ground and not at an odd rotation
            if (enoughAboveGround() && (Actions.Action == ActionManager.States.Regular || Actions.Action == ActionManager.States.JumpDash || (Actions.Action == ActionManager.States.Jump && GetComponent<Pathers_Interaction>().currentUpreel == null)))
            {
                
                if(Inp.trueMoveInput.sqrMagnitude > 0.8f)
                {
                    //Checks for nearby walls using raycasts
                    CheckForWall();

                    //If detecting a wall in front with a near horizontal normal
                    if (wallFront && frontWallDetect.normal.y <= 0.3 && frontWallDetect.normal.y >= -0.2 && checkSpeed > 30f)
                    {
                        yield return new WaitForFixedUpdate();
                        tryWallClimb();

                    }

                    //If detecting a wall to the side

                    //If detecting a wall on left with correct angle.
                    else if (wallLeft && leftWallDetect.normal.y <= 0.4 && checkSpeed > 38f &&
                        leftWallDetect.normal.y >= -0.4)
                    {
                        yield return new WaitForFixedUpdate();
                        tryWallRunLeft();

                    }

                    //If detecting a wall on right with correct angle.
                    else if (wallRight && rightWallDetect.normal.y <= 0.4 && checkSpeed > 38f &&
                        rightWallDetect.normal.y >= -0.4 )
                    {
                        yield return new WaitForFixedUpdate();
                        tryWallRunRight();
                    }
                }

                
            }
            else if (Player.Grounded)
            {
                bannedWall = null;
            }

        }
        
    }

    void tryWallClimb()
    {
        //If facing the wall enough
        //Debug.Log(Vector3.Dot(CharacterAnimator.transform.forward, frontWallDetect.normal));
        if (Vector3.Dot(CharacterAnimator.transform.forward, frontWallDetect.normal) < -0.95f)
        {
            //Debug.Log("Trigger Wall Climb");

            //Enter wall run as a climb
            if (Actions.eventMan != null) Actions.eventMan.wallClimbsPerformed += 1;
            WallRun.InitialEvents(true, frontWallDetect, false, WallCheckDistance * CheckModifier);
            Actions.ChangeAction(ActionManager.States.WallRunning);        

        }
    }

    void tryWallRunLeft()
    {
        float dis = Vector3.Distance(transform.position, leftWallDetect.point);
        if (Physics.Raycast(transform.position, Inp.trueMoveInput, dis + 0.1f, wallLayerMask))
        {
            //Debug.Log("Trigger Wall Left");
            //Enter a wallrun with wall on left.
            WallRun.InitialEvents(false, leftWallDetect, false);
            if (Actions.eventMan != null) Actions.eventMan.wallRunsPerformed += 1;
            Actions.ChangeAction(ActionManager.States.WallRunning);
            
        }
    }

    void tryWallRunRight()
    {
        float dis = Vector3.Distance(transform.position, rightWallDetect.point);
        if (Physics.Raycast(transform.position, Inp.trueMoveInput, dis + 0.1f, wallLayerMask))
        {
            //Debug.Log("Trigger Wall Right");
            //Enter a wallrun with wall on right.
            WallRun.InitialEvents(false, rightWallDetect, true);
            if (Actions.eventMan != null) Actions.eventMan.wallRunsPerformed += 1;
            Actions.ChangeAction(ActionManager.States.WallRunning);
            
        }
    }

    private bool enoughAboveGround()
    {
        //If racycast does not detect ground
        if (!Player.Grounded)
            return !Physics.Raycast(CharacterAnimator.transform.position, -Vector3.up, 6f, wallLayerMask);
        else
            return false;
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

               
                //If there isn't a wall and moving fast enough
                if (!wallLeft && !wallRight)
                {
                    //Increases check range based on speed
                    CheckModifier = (Player.HorizontalSpeedMagnitude * 0.035f) + .5f;
                    wallFront = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), CharacterAnimator.transform.forward, out frontWallDetect,
                    WallCheckDistance * CheckModifier, wallLayerMask);

                }
                
            }
   
        }




        //Checks if the wall can be used. Banned walls are set when the player jumps off the wall.
        if (wallFront)
        {
            if (frontWallDetect.collider.gameObject == bannedWall)
                wallFront = false;
            else
            {
                Vector3 wallDirection = frontWallDetect.point - transform.position;
                //Debug.Log(Vector3.Dot(wallDirection.normalized, Inp.trueMoveInput.normalized));

                if (Vector3.Dot(wallDirection.normalized, Inp.trueMoveInput.normalized) < 0.2f)
                {
                    wallFront = false;
                }
            }
        }
        if (wallRight)
        {
            if (rightWallDetect.collider.gameObject == bannedWall)
                wallRight = false;
            else
            {
                Vector3 wallDirection = rightWallDetect.point - transform.position;
                //Debug.Log(Vector3.Dot(wallDirection.normalized, Inp.trueMoveInput.normalized));

                if (Vector3.Dot(wallDirection.normalized, Inp.trueMoveInput.normalized) < 0.2f)
                {
                    wallFront = false;
                }
            }
        }
        if (wallLeft)
        {
            if (leftWallDetect.collider.gameObject == bannedWall)
                wallLeft = false;
            Vector3 wallDirection = leftWallDetect.point - transform.position;
            //Debug.Log(Vector3.Dot(wallDirection.normalized, Inp.trueMoveInput.normalized));
            //Debug.DrawRay(transform.position, wallDirection, Color.red, 60f);
            //Debug.DrawRay(transform.position, Inp.trueMoveInput, Color.green, 60f);

            if (Vector3.Dot(wallDirection.normalized, Inp.trueMoveInput.normalized) < 0.2f)
            {
                wallFront = false;
            }
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
        Inp = GetComponent<PlayerBinput>();

        CharacterAnimator = Tools.CharacterAnimator;
        sounds = Tools.SoundControl;
        JumpBall = Tools.JumpBall;
    }
}
