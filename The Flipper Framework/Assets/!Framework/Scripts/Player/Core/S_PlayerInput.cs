using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class S_PlayerInput : MonoBehaviour {

    private S_PlayerPhysics Player; // Reference to the ball controller.
    S_Handler_Camera Cam;
    S_ActionManager Actions;
    S_CharacterTools _Tools;
	Transform _MainSkin;

	public Vector3 moveAcc { get; set; }
    private Vector3 move;
    public Vector3 inputPreCamera;
    public Vector3 camMoveInput;
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

    [HideInInspector] public Vector3 finalMoveInput;
    private float moveX;
    private float moveY;
    [HideInInspector] public Vector2 InputExporter = Vector2.zero;

    private void Awake()
    {
        // Set up the reference.
        Player = GetComponent<S_PlayerPhysics>();
        Actions = GetComponent<S_ActionManager>();
        Cam = GetComponent<S_Handler_Camera>();

        _Tools = GetComponent<S_CharacterTools>();
		_MainSkin = _Tools.mainSkin;

		AssignStats();
        

        //prevDecel = Player._moveDeceleration_;
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
            finalMoveInput = new Vector3(moveX, 0, moveY);

            InitialInputMag = finalMoveInput.sqrMagnitude;
            InitialLerpedInput = Mathf.Lerp(InitialLerpedInput, InitialInputMag, Time.deltaTime);


            //Make movement relative to camera
            inputPreCamera = finalMoveInput;
            camMoveInput = GetInputByCameraDirection(finalMoveInput);
			finalMoveInput = GetInputByLocalTransform(finalMoveInput);

			Vector3 debugRay = transform.TransformDirection(finalMoveInput);
			//Debug.DrawRay(transform.position, debugRay * 1.5f, Color.black, 200f);
			//Debug.DrawRay(transform.position + debugRay * 1.5f, debugRay * 0.5f, Color.red, 200f);

			move = finalMoveInput;
			

		}

        //Lock Input Funcion
        if (LockInput)
        {
            //Debug.Log(LockedCounter);
            LockedInputFunction(move);
        }

  
    }

    public Vector3 GetInputByCameraDirection(Vector3 inputDirection)
    {
        
            //float _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
            //                  Cam.Cam.transform.eulerAngles.y;

            ////The direction the player is inputting to move
            //Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            //return targetDirection;

			float x = inputDirection.x;
			float z = inputDirection.z;

			Vector3 camForward = Cam.Cam.transform.forward;
			Vector3 camRight = Cam.Cam.transform.right;

			camForward.y = 0;
			camRight.y = 0;
			camForward.Normalize();
			camRight.Normalize();
			Vector3 targetDirection = (z * camForward) + (x * camRight);
			return targetDirection;
    }

	Vector3 GetInputByLocalTransform(Vector3 inputDirection) {

		if (inputDirection != Vector3.zero)
		{
			Vector3 transformedInput = inputDirection;
			Vector3 upDirection = Player._isGrounded ? Player._groundNormal : transform.up;
			transformedInput = Quaternion.FromToRotation(cam.up, upDirection) *  (cam.rotation * inputDirection);
			//transformedInput = _MainSkin.transform.rotation * inputDirection;
			transformedInput = transform.InverseTransformDirection(transformedInput);
			transformedInput.y = 0.0f;

			Player._rawInput = transformedInput;
			return transformedInput;	
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
        //Player._moveDeceleration_ = 1;
        Player._inputVelocityDifference = 0;

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
            //Player._moveDeceleration_ = prevDecel;
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
