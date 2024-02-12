using UnityEngine;
using System.Collections;

public class S_Action02_Homing : MonoBehaviour
{

    S_ActionManager Action;
    S_VolumeTrailRenderer HomingTrailScript;
    Animator CharacterAnimator;
    S_Handler_HomingAttack HomingControl;
    S_PlayerPhysics Player;
    S_CharacterTools Tools;

    GameObject JumpBall;

    [HideInInspector] public bool _isAdditive_;
    [HideInInspector] public float _homingAttackSpeed_;
    [HideInInspector] public float _airDashSpeed_;
    [HideInInspector] public float _airDashDuration_;
    [HideInInspector] public float _homingTimerLimit_;
    //[HideInInspector] public float FacingAmount;

    float XZmag;
    public float LateSpeed { get; set; }
    public Vector3 TargetDirection { get; set; }
    GameObject HomingTrailContainer;
    //public GameObject HomingTrail;
    float timer;
    float Speed;
    float StoredSpeed;
    Vector3 direction;
    Vector3 newRotation;


    public Transform Target { get; set; }
    public float skinRotationSpeed;
    public bool HomingAvailable { get; set; }


    void Awake()
    {
        HomingAvailable = true;

        if (Player == null)
        {
            Tools = GetComponent<S_CharacterTools>();
            AssignTools();

            AssignStats();          
        }
    }


    public void InitialEvents()
    {
        if(!Action.lockHoming)
        {

            AssignStats();


            HomingTrailScript.emitTime = _airDashDuration_;
            HomingTrailScript.emit = true;


            JumpBall.SetActive(false);


            if (Action.Action02Control._HasTarget)
            {
                Target = HomingControl._TargetObject.transform;
                TargetDirection = (Target.transform.position - Player._playerPos).normalized;
            }
            else
            {
                TargetDirection = Player._RB.velocity.normalized;
            }

            timer = 0;
            HomingAvailable = false;

            XZmag = Player._horizontalSpeedMagnitude;




            //Action.actionDisable();

            //Vector3 TgyXY = HomingControl.TargetObject.transform.position.normalized;
            //TgyXY.y = 0;
            //float facingAmmount = Vector3.Dot(Player.PreviousRawInput.normalized, TgyXY);

            direction = Target.position - transform.position;
            newRotation = Vector3.RotateTowards(transform.forward, direction, 5f, 0.0f);

            // //Debug.Log(facingAmmount);
            // if (facingAmmount < FacingAmount) { IsAirDash = true; }

            if (XZmag * 0.7f < _homingAttackSpeed_)
            {
                Speed = _homingAttackSpeed_;
                StoredSpeed = Speed;
            }
            else
            {
                Speed = XZmag * 0.7f;
                StoredSpeed = XZmag;
            }
        }    

    }

    void Update()
    {

        //Debug.Log(Player.p_rigidbody.velocity);


        //Player.Gravity = new Vector3(0f, 0f, 0f);
        Player._isGravityOn = false;

        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 1);
        CharacterAnimator.SetFloat("YSpeed", Player._RB.velocity.y);
        CharacterAnimator.SetFloat("GroundSpeed", Player._RB.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player._isGrounded);

        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(Player._RB.velocity.x, 0, Player._RB.velocity.z);
        Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);



    }

    void FixedUpdate()
    {
        

        timer += Time.deltaTime;

        //Ends homing attack if in air for too long or target is lost
        if (Target == null || timer > _homingTimerLimit_)
        {
            Action.ChangeAction(S_Enums.PlayerStates.Regular);
        }

        //direction = Target.position - transform.position;
        //Player.p_rigidbody.velocity = direction.normalized * Speed;

        //Debug.Log (Player.SpeedMagnitude);

        direction = Target.position - transform.position;
        newRotation = Vector3.RotateTowards(newRotation, direction, 1f, 0.0f);
        Player._RB.velocity = newRotation * Speed;
        

        //Set Player location when close enough, for precision.
        if (Target != null && Vector3.Distance(Target.transform.position, transform.position) < (Speed * Time.fixedDeltaTime) && Target.gameObject.activeSelf)
        {
           transform.position = Target.transform.position;
        }
        else
        {
           //LateSpeed = Mathf.Max(XZmag, HomingAttackSpeed);
           LateSpeed = Mathf.Max(XZmag, _homingAttackSpeed_);
        }
        
    }

    public void ResetHomingVariables()
    {
        timer = 0;
        HomingTrailContainer.transform.DetachChildren();
        //IsAirDash = false;
    }

    private void AssignTools()
    {
        //HomingAttackControl.TargetObject = null;
        Player = GetComponent<S_PlayerPhysics>();
        HomingControl = GetComponent<S_Handler_HomingAttack>();
        Action = GetComponent<S_ActionManager>();

        CharacterAnimator = Tools.CharacterAnimator;
        HomingTrailScript = Tools.HomingTrailScript;
        HomingTrailContainer = Tools.HomingTrailContainer;
        JumpBall = Tools.JumpBall;


    }
    private void AssignStats()
    {
        _isAdditive_ = Tools.Stats.JumpDashStats.shouldUseCurrentSpeedAsMinimum;
        _homingAttackSpeed_ = Tools.Stats.HomingStats.attackSpeed;
        _airDashSpeed_ = Tools.Stats.JumpDashStats.dashSpeed;
        _homingTimerLimit_ = Tools.Stats.HomingStats.timerLimit;
        _airDashDuration_ = Tools.Stats.JumpDashStats.duration;
    }

}
