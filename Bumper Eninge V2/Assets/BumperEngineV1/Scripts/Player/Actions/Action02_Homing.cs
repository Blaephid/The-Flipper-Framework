using UnityEngine;
using System.Collections;

public class Action02_Homing : MonoBehaviour
{

    ActionManager Action;
    VolumeTrailRenderer HomingTrailScript;
    Animator CharacterAnimator;
    HomingAttackControl HomingControl;
    PlayerBhysics Player;
    CharacterTools Tools;

    GameObject JumpBall;

    [HideInInspector] public bool isAdditive;
    [HideInInspector] public float HomingAttackSpeed;
    [HideInInspector] public float AirDashSpeed;
    [HideInInspector] public float AirDashDuration;
    [HideInInspector] public float HomingTimerLimit;
    //[HideInInspector] public float FacingAmount;

    float XZmag;
    public float LateSpeed { get; set; }
    public Vector3 TargetDirection { get; set; }
    GameObject HomingTrailContainer;
    //public GameObject HomingTrail;
    float Timer;
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
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();          
        }
    }


    public void InitialEvents()
    {
        AssignStats();
        

        HomingTrailScript.emitTime = AirDashDuration;
        HomingTrailScript.emit = true;

        
        JumpBall.SetActive(false);
        

        if (Action.Action02Control.HasTarget)
        {
            Target = HomingControl.TargetObject.transform;
            TargetDirection = (Target.transform.position - Player.playerPos).normalized;
        }
        else
        {
            TargetDirection = Player.rb.velocity.normalized;
        }

        Timer = 0;
        HomingAvailable = false;

        XZmag = Player.HorizontalSpeedMagnitude;



        
        //Action.actionDisable();
            
        //Vector3 TgyXY = HomingControl.TargetObject.transform.position.normalized;
        //TgyXY.y = 0;
        //float facingAmmount = Vector3.Dot(Player.PreviousRawInput.normalized, TgyXY);

        direction = Target.position - transform.position;
        newRotation = Vector3.RotateTowards(transform.forward, direction, 5f, 0.0f);

        // //Debug.Log(facingAmmount);
        // if (facingAmmount < FacingAmount) { IsAirDash = true; }

        if (XZmag * 0.7f < HomingAttackSpeed)
        {
            Speed = HomingAttackSpeed;
            StoredSpeed = Speed;
        }
        else
        {
            Speed = XZmag * 0.7f;
            StoredSpeed = XZmag;
        }

        

    }

    void Update()
    {

        //Debug.Log(Player.p_rigidbody.velocity);


        //Player.Gravity = new Vector3(0f, 0f, 0f);
        Player.GravityAffects = false;

        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 1);
        CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
        CharacterAnimator.SetFloat("GroundSpeed", Player.rb.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);

        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
        Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);



    }

    void FixedUpdate()
    {
        

        Timer += Time.deltaTime;

        //Ends homing attack if in air for too long or target is lost
        if (Target == null || Timer > HomingTimerLimit)
        {
            Action.ChangeAction(0);
        }

        //direction = Target.position - transform.position;
        //Player.p_rigidbody.velocity = direction.normalized * Speed;

        //Debug.Log (Player.SpeedMagnitude);

        direction = Target.position - transform.position;
        newRotation = Vector3.RotateTowards(newRotation, direction, 1f, 0.0f);
        Player.rb.velocity = newRotation * Speed;
        

        //Set Player location when close enough, for precision.
        if (Target != null && Vector3.Distance(Target.transform.position, transform.position) < (Speed * Time.fixedDeltaTime) && Target.gameObject.activeSelf)
        {
           transform.position = Target.transform.position;
        }
        else
        {
           //LateSpeed = Mathf.Max(XZmag, HomingAttackSpeed);
           LateSpeed = Mathf.Max(XZmag, HomingAttackSpeed);
        }
        
    }

    public void ResetHomingVariables()
    {
        Timer = 0;
        HomingTrailContainer.transform.DetachChildren();
        //IsAirDash = false;
    }

    private void AssignTools()
    {
        //HomingAttackControl.TargetObject = null;
        Player = GetComponent<PlayerBhysics>();
        HomingControl = GetComponent<HomingAttackControl>();
        Action = GetComponent<ActionManager>();

        CharacterAnimator = Tools.CharacterAnimator;
        HomingTrailScript = Tools.HomingTrailScript;
        HomingTrailContainer = Tools.HomingTrailContainer;
        JumpBall = Tools.JumpBall;


    }
    private void AssignStats()
    {
        isAdditive = Tools.stats.isAdditive;
        HomingAttackSpeed = Tools.stats.HomingAttackSpeed;
        AirDashSpeed = Tools.stats.AirDashSpeed;
        HomingTimerLimit = Tools.stats.HomingTimerLimit;
        AirDashDuration = Tools.stats.AirDashDuration;
    }

}
