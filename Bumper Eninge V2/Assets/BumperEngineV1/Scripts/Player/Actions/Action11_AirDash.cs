using UnityEngine;
using System.Collections;

public class Action11_AirDash : MonoBehaviour
{
    CharacterTools Tools;
    CharacterStats Stats;

    ActionManager Action;
    VolumeTrailRenderer HomingTrailScript;
    Animator CharacterAnimator;
    Action02_Homing Action02;
    PlayerBhysics Player;

    [HideInInspector] public bool isAdditive;
    [HideInInspector] public float AirDashSpeed;
    [HideInInspector] public float AirDashDuration;

    float XZmag;
    GameObject HomingTrailContainer;
    GameObject JumpDashParticle;
    GameObject JumpBall;

    float Timer;
    float Aspeed;


    public float skinRotationSpeed;


    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            Stats = GetComponent<CharacterStats>();
            AssignStats();
        }

        Action02.HomingAvailable = true;
    }


    public void InitialEvents()
    {

        if (Action02.HomingAvailable)
        {

            HomingTrailScript.emitTime = AirDashDuration + 0.5f;
            HomingTrailScript.emit = true;

            JumpBall.SetActive(false);
            Action.SpecialPressed = false;
            Action.HomingPressed = false;

            Timer = 0;
            Action02.HomingAvailable = false;

            XZmag = Player.HorizontalSpeedMagnitude;

            AirDashParticle();

            if (XZmag < AirDashSpeed)
            {
                Aspeed = AirDashSpeed;
            }
            else
            {
                Aspeed = XZmag;
            }

        


        }
        else
        {
            Action.ChangeAction(Action.PreviousAction);
        }
    }

    void Update()
    {

        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 11);
        CharacterAnimator.SetFloat("YSpeed", Player.p_rigidbody.velocity.y);
        CharacterAnimator.SetFloat("GroundSpeed", Player.p_rigidbody.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);

        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z);
        Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);



    }

    void FixedUpdate()
    {
        //Debug.Log(Timer);

        Timer += Time.deltaTime;

        if (Player.RawInput != Vector3.zero)
        {
            Vector3 Direction = (transform.TransformDirection(Player.RawInput).normalized + (Player.p_rigidbody.velocity).normalized * 2);
            Direction.y = Player.Gravity.y * 0.2f;
            Player.p_rigidbody.velocity = Direction.normalized * Aspeed;
        }
        else
        {
            Vector3 Direction = (transform.TransformDirection(Player.PreviousRawInput).normalized + (Player.p_rigidbody.velocity).normalized * 2);
            Direction.y = Player.Gravity.y * 0.2f;
            Player.p_rigidbody.velocity = Direction.normalized * Aspeed;
        }


        //End homing attck if on air for too long
        if (Timer > AirDashDuration)
        {
            JumpBall.SetActive(true);
            Action.ChangeAction(1);
        }
        else if (Player.Grounded)
        {
            CharacterAnimator.SetInteger("Action", 0);
            CharacterAnimator.SetBool("Grounded", Player.Grounded);
            Action.ChangeAction(0);
        }
    }

    public void AirDashParticle()
    {
        GameObject JumpDashParticleClone = Instantiate(JumpDashParticle, HomingTrailContainer.transform.position, Quaternion.identity) as GameObject;
        if (Player.SpeedMagnitude > 60)
            JumpDashParticleClone.GetComponent<ParticleSystem>().startSize = Player.SpeedMagnitude / 60f;
        else
            JumpDashParticleClone.GetComponent<ParticleSystem>().startSize = 1f;

        JumpDashParticleClone.transform.position = HomingTrailContainer.transform.position;
        JumpDashParticleClone.transform.rotation = HomingTrailContainer.transform.rotation;
        //JumpDashParticleClone.transform.parent = HomingTrailContainer.transform;
    }

    private void AssignStats()
    {
        isAdditive = Stats.isAdditive;
        AirDashSpeed = Stats.AirDashSpeed;
        AirDashDuration = Stats.AirDashDuration;
    }

    private void AssignTools()
    {
        //HomingAttackControl.TargetObject = null;
        Player = GetComponent<PlayerBhysics>();
        Action = GetComponent<ActionManager>();
        Action02 = GetComponent<Action02_Homing>();


        CharacterAnimator = Tools.CharacterAnimator;
        HomingTrailScript = Tools.HomingTrailScript;
        HomingTrailContainer = Tools.HomingTrailContainer;
        JumpBall = Tools.JumpBall;
        JumpDashParticle = Tools.JumpDashParticle;
    }

}
