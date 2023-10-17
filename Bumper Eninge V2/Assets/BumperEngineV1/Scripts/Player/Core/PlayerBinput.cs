using UnityEngine;
using System.Collections;

public class PlayerBinput : MonoBehaviour
{

    private PlayerBhysics Player; // Reference to the ball controller.
    CameraControl Cam;
    ActionManager Actions;

    public Vector3 moveAcc { get; set; }
    private Vector3 move;
    // the world-relative desired move direction, calculated from the camForward and user input.

    private Transform cam; // A reference to the main camera in the scenes transform
    private Vector3 camForward; // The current forward direction of the camera

    private bool PreviousInputWasNull;

    public AnimationCurve InputLerpingRateOverSpeed;
    public bool UtopiaTurning;
    public AnimationCurve UtopiaInputLerpingRateOverSpeed;
    public float InputLerpSpeed { get; set; }
    public Vector3 UtopiaInput { get; set; }
    public float UtopiaIntensity;
    public float UtopiaInitialInputLerpSpeed;
    public float UtopiaLerpingSpeed { get; set; }
    float InitialInputMag;
    float InitialLerpedInput;

    bool LockInput { get; set; }
    float LockedTime;
    Vector3 LockedInput;
    float LockedCounter = 0;
    bool LockCam { get; set; }
    public float prevDecel { get; set; }
    private bool HittingWall;

    private void Awake()
    {
        // Set up the reference.
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();
        prevDecel = Player.MoveDecell;

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

        // Get the axis and jump input.

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // calculate move direction
        if (cam != null)
        {

            //Vector3 moveInp = new Vector3(h, 0, v);
            Vector3 moveInp = new Vector3(h, 0, v);

            InitialInputMag = moveInp.sqrMagnitude;
            InitialLerpedInput = Mathf.Lerp(InitialLerpedInput, InitialInputMag, Time.deltaTime * UtopiaInitialInputLerpSpeed);

            float currentInputSpeed = (!UtopiaTurning) ? InputLerpSpeed : UtopiaLerpingSpeed;

            if (moveInp != Vector3.zero)
            {
                Vector3 transformedInput;
                transformedInput = Quaternion.FromToRotation(cam.up, Player.GroundNormal) * (cam.rotation * moveInp);
                transformedInput = transform.InverseTransformDirection(transformedInput);
                transformedInput.y = 0.0f;


                Player.RawInput = transformedInput;
                moveInp = Vector3.Lerp(move, transformedInput, Time.deltaTime * currentInputSpeed);
            }
            else
            {
                //Debug.Log ("InputNull");
                Vector3 transformedInput = Quaternion.FromToRotation(cam.up, Player.GroundNormal) * (cam.rotation * moveInp);
                transformedInput = transform.InverseTransformDirection(transformedInput);
                transformedInput.y = 0.0f;
                Player.RawInput = transformedInput;
                moveInp = Vector3.Lerp(move, transformedInput, Time.deltaTime * (UtopiaLerpingSpeed * UtopiaIntensity));
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
            LockedInputFunction();
        }

    }



    void FixedUpdate()
    {

        Debug.DrawRay(transform.position, move, Color.cyan);
        Player.MoveInput = (move);

    }

    void LockedInputFunction()
    {
        move = Vector3.zero;
        LockedCounter += 1;
        Player.MoveDecell = 1;
        Player.b_normalSpeed = 0;

        if (LockCam)
        {
            Cam.Cam.FollowDirection(3, 14, -10, 0);
        }

        if (Actions.Action != 0)
        {
            LockedCounter = LockedTime;
        }

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

}
