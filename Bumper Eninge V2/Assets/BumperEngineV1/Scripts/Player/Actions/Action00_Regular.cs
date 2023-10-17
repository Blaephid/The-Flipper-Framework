using UnityEngine;
using System.Collections;

public class Action00_Regular : MonoBehaviour
{

    public Animator CharacterAnimator;
    PlayerBhysics Player;
    ActionManager Actions;
    CameraControl Cam;
    public SonicSoundsControl sounds;

    public float skinRotationSpeed;
    Action01_Jump JumpAction;
    Quaternion CharRot;

    public float MaximumSpeed; //The max amount of speed you can be at to perform a Spin Dash
    public float MaximumSlope; //The highest slope you can be on to Spin Dash

    public float SpeedToStopAt;

    public float SkiddingStartPoint;
    public float SkiddingIntensity;

    public bool hasSked;
    [SerializeField] bool CanDashDuringFall;
    void Awake()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        JumpAction = GetComponent<Action01_Jump>();
        Cam = GetComponent<CameraControl>();
    }

    void FixedUpdate()
    {
        if (Player.SpeedMagnitude < 15 && Player.MoveInput == Vector3.zero && Player.Grounded)
        {
            Player.b_normalSpeed = 0;
            Player.p_rigidbody.velocity *= 0.90f;
            hasSked = false;
        }

        //Skidding
        if (Player.b_normalSpeed < -SkiddingStartPoint && Player.Grounded)
        {
            if (Player.SpeedMagnitude >= -SkiddingIntensity) Player.AddVelocity(Player.p_rigidbody.velocity.normalized * SkiddingIntensity * (Player.isRolling ? 0.5f : 1));
            if (!hasSked && Player.Grounded && !Player.isRolling)
            {
                sounds.SkiddingSound();
                hasSked = true;
            }
            if (Player.SpeedMagnitude < 4)
            {
                Player.isRolling = false;
                Player.b_normalSpeed = 0;
                hasSked = false;
            }
        }
        else
        {
            hasSked = false;
        }


        //Set Homing attack to true
        if (Player.Grounded)
        {

            if (Actions.Action02 != null)
            {
                Actions.Action02.HomingAvailable = true;
            }

            if (Actions.Action06.BounceCount > 0)
            {
                Actions.Action06.BounceCount = 0;
            }

        }

    }

    void Update()
    {


        //Set Animator Parameters
        if (Player.Grounded) { CharacterAnimator.SetInteger("Action", 0); }
        CharacterAnimator.SetFloat("YSpeed", Player.p_rigidbody.velocity.y);
        CharacterAnimator.SetFloat("XZSpeed", Mathf.Abs((Player.p_rigidbody.velocity.x + Player.p_rigidbody.velocity.z) / 2));
        CharacterAnimator.SetFloat("GroundSpeed", Player.p_rigidbody.velocity.magnitude);
        CharacterAnimator.SetFloat("HorizontalInput", Input.GetAxis("Horizontal") * Player.p_rigidbody.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);
        CharacterAnimator.SetFloat("NormalSpeed", Player.b_normalSpeed + SkiddingStartPoint);


        if (Input.GetButtonDown("A") && Player.Grounded)
        {
            JumpAction.InitialEvents();
            Actions.ChangeAction(1);
        }

        //Set Camera to back
        if (Input.GetButton("RightStickIn"))
        {
            //Lock camera on behind
            Cam.Cam.FollowDirection(6, 14f, -10, 0);
            Cam.Cam.FollowDirection(15, 6);
        }

        //Do Spindash
        if (Input.GetButton("B") && Player.Grounded && Player.GroundNormal.y > MaximumSlope && Player.p_rigidbody.velocity.sqrMagnitude < MaximumSpeed)
        { Actions.ChangeAction(3); Actions.Action03.InitialEvents(); }

        //Check if rolling
        if (Player.Grounded && Player.isRolling) { CharacterAnimator.SetInteger("Action", 1); }
        CharacterAnimator.SetBool("isRolling", Player.isRolling);

        //Play Rolling Sound
        if (Input.GetButtonDown("R1") && Player.Grounded && (GetComponent<Rigidbody>().velocity.sqrMagnitude > Player.RollingStartSpeed))
        {
            sounds.SpinningSound();
        }

        //Set Character Animations and position1
        CharacterAnimator.transform.parent = null;

        //Set Skin Rotation
        if (Player.Grounded)
        {
            Vector3 newForward = Player.p_rigidbody.velocity - transform.up * Vector3.Dot(Player.p_rigidbody.velocity, transform.up);

            if (newForward.magnitude < 0.1f)
            {
                newForward = CharacterAnimator.transform.forward;
            }

            CharRot = Quaternion.LookRotation(newForward, transform.up);
            CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);

            // CharRot = Quaternion.LookRotation( Player.rigidbody.velocity, transform.up);
            // CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
        }
        else
        {
            Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z);
            Quaternion CharRot = Quaternion.LookRotation(VelocityMod, -Player.Gravity.normalized);
            CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
        }

        if (Actions.Action02 != null)
        {

            //Do a homing attack
            if (!Player.Grounded && Input.GetButtonDown("A") && Actions.Action02Control.HasTarget && Actions.Action02.HomingAvailable)
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
            if (!Player.Grounded && Input.GetButtonDown("A") && !Actions.Action02Control.HasTarget && Actions.Action02.HomingAvailable && CanDashDuringFall && Actions.Action08 == null)
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
        if (!Player.Grounded && Input.GetButtonDown("X"))
        {
            Actions.ChangeAction(6);
            //Actions.Action06.ShouldStomp = false;
            Actions.Action06.InitialEvents();
        }

        //Do a DropDash Attack
        if (Actions.Action08 != null)
        {

            if (!Player.Grounded && Input.GetButtonDown("A") && Actions.Action08 != null && !Actions.Action02Control.HasTarget)
            {
                Actions.Action08.DropDashAvailable = false;
                Actions.ChangeAction(8);
                Actions.Action08.InitialEvents();
            }

            if (Player.Grounded && Actions.Action08.DropEffect.isPlaying)
            {
                Actions.Action08.DropEffect.Stop();
            }
        }

        //Do a LightDash Attack
        if (Input.GetButtonDown("Y") && Actions.Action07Control.HasTarget)
        {
            Actions.ChangeAction(7);
            Actions.Action07.InitialEvents();
        }

    }

}
