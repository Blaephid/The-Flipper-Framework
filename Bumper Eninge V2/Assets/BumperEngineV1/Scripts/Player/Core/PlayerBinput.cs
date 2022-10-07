using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerBinput : MonoBehaviour {

    private PlayerBhysics Player; // Reference to the ball controller.
    CameraControl Cam;
    ActionManager Actions;
    CharacterStats Stats;

    public Vector3 moveAcc { get; set; }
    private Vector3 move;
    // the world-relative desired move direction, calculated from the camForward and user input.

    private Transform cam; // A reference to the main camera in the scenes transform
    private Vector3 camForward; // The current forward direction of the camera

	private bool PreviousInputWasNull;

    [HideInInspector] public AnimationCurve InputLerpingRateOverSpeed;
    [HideInInspector] public bool UtopiaTurning;
    [HideInInspector] public AnimationCurve UtopiaInputLerpingRateOverSpeed;
    public float InputLerpSpeed { get; set; }
    public Vector3 UtopiaInput { get; set; }
    [HideInInspector] public float UtopiaIntensity;
    [HideInInspector] public float UtopiaInitialInputLerpSpeed;
    [HideInInspector] public float UtopiaLerpingSpeed { get; set; }
    float InitialInputMag;
    float InitialLerpedInput;

    bool LockInput { get; set; }
    float LockedTime;
    Vector3 LockedInput;
    float LockedCounter = 0;
    bool LockCam { get; set; }
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
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();

        Stats = GetComponent<CharacterStats>();
        if (Stats != null)
        {
            AssignStats();
        }

        prevDecel = Player.MoveDecell;
        //newInput = new PlayerNewInput();

        // get the transform of the main camera
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }

    }

    private void Update()
    {
        // Get curve position

        InputLerpSpeed = InputLerpingRateOverSpeed.Evaluate((Player.p_rigidbody.velocity.sqrMagnitude / Player.MaxSpeed) / Player.MaxSpeed);
        UtopiaLerpingSpeed = UtopiaInputLerpingRateOverSpeed.Evaluate((Player.p_rigidbody.velocity.sqrMagnitude / Player.MaxSpeed) / Player.MaxSpeed);

        

		// calculate move direction
		if (cam != null)
		{
            moveX = Actions.moveX;
            moveY = Actions.moveY;
			moveInp = new Vector3(moveX, 0, moveY);

			InitialInputMag = moveInp.sqrMagnitude;
			InitialLerpedInput = Mathf.Lerp(InitialLerpedInput, InitialInputMag, Time.deltaTime * UtopiaInitialInputLerpSpeed);

			float currentInputSpeed = (!UtopiaTurning) ? InputLerpSpeed : UtopiaLerpingSpeed;

            //Make movement relative to camera

			if (moveInp != Vector3.zero && !onPath)
			{
				Vector3 transformedInput;
				transformedInput = Quaternion.FromToRotation(cam.up, Player.GroundNormal) * (cam.rotation * moveInp);    
				transformedInput = transform.InverseTransformDirection (transformedInput);
				transformedInput.y = 0.0f;
				
				Player.RawInput = transformedInput;
				moveInp = Vector3.Lerp(move, transformedInput, Time.deltaTime * currentInputSpeed);
			}
			else if (!onPath)
			{
				//Debug.Log ("InputNull");
				Vector3 transformedInput = Quaternion.FromToRotation(cam.up, Player.GroundNormal) * (cam.rotation * moveInp);
				transformedInput = transform.InverseTransformDirection(transformedInput);
				transformedInput.y = 0.0f;
				Player.RawInput = transformedInput;
				moveInp = Vector3.Lerp(move, transformedInput, Time.deltaTime * (UtopiaLerpingSpeed*UtopiaIntensity));
			}
				
			if (moveInp.x < 0.01 && moveInp.z < 0.01 && moveInp.x > -0.01 && moveInp.z > -0.01) 
			{
				moveInp = Vector3.zero;
			}

			move = moveInp;
		}

        //Lock Input Funcion
        if (LockInput)
        {
            //Debug.Log(LockedCounter);
            LockedInputFunction();
        }

        InputExporter.x = moveInp.x;
        InputExporter.y = moveInp.y;

    }



    void FixedUpdate()
    {

        Debug.DrawRay(transform.position, move, Color.cyan);
        Player.MoveInput = move;

    }

    void LockedInputFunction()
    {
        move = Vector3.zero;
        LockedCounter += 1;
        Player.MoveDecell = 1;
        Player.b_normalSpeed = 0;

        if (LockCam)
        {
            Cam.Cam.FollowDirection(3, 14, -10,0);
        }

        //if (Actions.Action != 0)
        //{
        //    LockedCounter = LockedTime;
        //}

        if (LockedCounter > LockedTime)
        {
            Player.MoveDecell = prevDecel;
            LockInput = false;
        }
    }

    public void LockInputForAWhile(float duration, bool lockCam)
    {
        LockedTime = duration;
        LockedCounter = 0;
        LockInput = true;
        LockCam = lockCam;
    }


    private void AssignStats()
    {
        InputLerpingRateOverSpeed = Stats.InputLerpingRateOverSpeed;
        UtopiaTurning = Stats.UtopiaTurning;
        UtopiaInputLerpingRateOverSpeed = Stats.UtopiaInputLerpingRateOverSpeed;
        UtopiaIntensity = Stats.UtopiaIntensity;
        UtopiaInitialInputLerpSpeed = Stats.UtopiaInitialInputLerpSpeed;

    }
}
