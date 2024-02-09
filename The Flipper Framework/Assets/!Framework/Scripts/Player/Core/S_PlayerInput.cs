using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class S_PlayerInput : MonoBehaviour {

    private S_PlayerPhysics Player; // Reference to the ball controller.
    S_Handler_Camera Cam;
    S_ActionManager Actions;
    S_CharacterTools Tools;

    public Vector3 moveAcc { get; set; }
    private Vector3 move;
    public Vector3 inputPreCamera;
    public Vector3 trueMoveInput;
    // the world-relative desired move direction, calculated from the camForward and user input.

    private Transform cam; // A reference to the main camera in the scenes transform
    private Vector3 camForward; // The current forward direction of the camera

	private bool PreviousInputWasNull;

    //[HideInInspector] public AnimationCurve InputLerpingRateOverSpeed;

    public float InputLerpSpeed { get; set; }

    [HideInInspector] public float UtopiaLerpingSpeed { get; set; }
    float InitialInputMag;
    float InitialLerpedInput;

    public bool LockInput { get; set; }
    float LockedTime;
    Vector3 LockedInput;
    float LockedCounter = 0;
    [HideInInspector] public bool LockCam { get; set; }
    public bool onPath { get; set; }
    public float prevDecel { get; set; }
	private bool HittingWall;

    [HideInInspector] public Vector3 moveInp;
    private float moveX;
    private float moveY;
    [HideInInspector] public Vector2 InputExporter = Vector2.zero;

    private void Awake()
    {
        // Set up the reference.
        Player = GetComponent<S_PlayerPhysics>();
        Actions = GetComponent<S_ActionManager>();
        Cam = GetComponent<S_Handler_Camera>();

        Tools = GetComponent<S_CharacterTools>();
        
        AssignStats();
        

        prevDecel = Player._moveDeceleration_;
        //newInput = new PlayerNewInput();

        // get the transform of the main camera
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }

    }

    private void Update()
    {
        AcquireMoveInput();

    }

    void AcquireMoveInput()
    {
        // calculate move direction
        if (cam != null)
        {
            moveX = Actions.moveX;
            moveY = Actions.moveY;
            moveInp = new Vector3(moveX, 0, moveY);

            InitialInputMag = moveInp.sqrMagnitude;
            InitialLerpedInput = Mathf.Lerp(InitialLerpedInput, InitialInputMag, Time.deltaTime);


            //Make movement relative to camera
            inputPreCamera = moveInp;
            trueMoveInput = GetTrueInput(moveInp);

            
            if (moveInp != Vector3.zero && !onPath)
            {
                Vector3 transformedInput;
                transformedInput = Quaternion.FromToRotation(cam.up, Player.GroundNormal) * (cam.rotation * moveInp);
                transformedInput = transform.InverseTransformDirection(transformedInput);
                transformedInput.y = 0.0f;

                Player.RawInput = transformedInput;
                moveInp = transformedInput;
            }
     

            if (moveInp.x < 0.02 && moveInp.z < 0.02 && moveInp.x > -0.02 && moveInp.z > -0.02)
            {
                moveInp = Vector3.zero;
            }

            move = moveInp;
            
        }

        //Lock Input Funcion
        if (LockInput)
        {
            //Debug.Log(LockedCounter);
            LockedInputFunction(move);
        }

        InputExporter.x = moveInp.x;
        InputExporter.y = moveInp.y;
    }

    public Vector3 GetTrueInput(Vector3 inputDirection)
    {
        if(inputDirection.sqrMagnitude > 0.3f)
        {
            float _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              Cam.Cam.transform.eulerAngles.y;

            //The direction the player is inputting to move
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            return targetDirection;
        }
        return inputDirection;
    }

    void FixedUpdate()
    {
        Player._moveInput = move;

    }

    void LockedInputFunction(Vector3 oldMove)
    {
        
        move = Vector3.zero;
        LockedCounter += 1;
        Player._moveDeceleration_ = 1;
        Player.b_normalSpeed = 0;

        if (LockCam)
        {
            Cam.Cam.FollowDirection(3, 14, -10,0, true);
        }

        //if (Actions.Action != 0)
        //{
        //    LockedCounter = LockedTime;
        //}

        if (LockedCounter > LockedTime)
        {
            Player._moveDeceleration_ = prevDecel;
            LockInput = false;
            move = oldMove;
        }
    }

    public void LockInputForAWhile(float duration, bool lockCam)
    {
        if (LockInput)
            LockedTime = Mathf.Max(duration, LockedTime);
        else
            LockedTime = duration;

        LockedCounter = 0;
        LockInput = true;
        LockCam = lockCam;
    }


    private void AssignStats()
    {

    }
}
