using UnityEngine;
using System.Collections;

public class Action11_JumpDash : MonoBehaviour
{
    CharacterTools Tools;

    ActionManager Action;
    VolumeTrailRenderer HomingTrailScript;
    Animator CharacterAnimator;
    Action02_Homing Action02;
    PlayerBhysics Player;
    PlayerBinput Inp;

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

    Vector3 Direction;



    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();
        }

        Action02.HomingAvailable = true;
    }


    public void InitialEvents()
    {
        if(!Action.lockJumpDash)
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

                Direction = CharacterAnimator.transform.forward;
                //Direction = Vector3.RotateTowards(Direction, lateralToInput * Direction, turnRate * 40f, 0f);





            }
            else
            {
                //Action.ChangeAction(Action.PreviousAction);
            }
        }

    }

    void Update()
    {

        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 11);
        CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
        CharacterAnimator.SetFloat("GroundSpeed", Player.rb.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);

        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
        if(VelocityMod != Vector3.zero)
        {
            Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
            CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
        }    



    }

    void FixedUpdate()
    {
        //Debug.Log(Timer);

        Timer += Time.deltaTime;

        if (Inp.inputPreCamera != Vector3.zero)
        {
            if(Timer > 0.03)
            {
                
                Vector3 inputDirection = new Vector3(0, 0, Inp.inputPreCamera.z);
                Debug.Log(Inp.inputPreCamera);
                

                //Direction = CharacterAnimator.transform.forward;
                Direction = Vector3.RotateTowards(Direction, CharacterAnimator.transform.right, Mathf.Clamp(Inp.inputPreCamera.x * 4, -2.5f, 2.5f) * Time.deltaTime, 0f);
            }           

            Direction.y = Player.fallGravity.y * 0.18f;
          
        }
        else
        {
            //Direction = (transform.TransformDirection(Player.PreviousRawInput).normalized + (Player.rb.velocity).normalized * 2);
            Direction.y = Player.fallGravity.y * 0.19f;

        }
        Player.rb.velocity = Direction.normalized * Aspeed;

        //End homing attck if in air for too long
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
        isAdditive = Tools.coreStats.isAdditive;
        AirDashSpeed = Tools.stats.AirDashSpeed;
        AirDashDuration = Tools.stats.AirDashDuration;
    }

    private void AssignTools()
    {
        //HomingAttackControl.TargetObject = null;
        Player = GetComponent<PlayerBhysics>();
        Action = GetComponent<ActionManager>();
        Action02 = GetComponent<Action02_Homing>();
        Inp = GetComponent<PlayerBinput>();


        CharacterAnimator = Tools.CharacterAnimator;
        HomingTrailScript = Tools.HomingTrailScript;
        HomingTrailContainer = Tools.HomingTrailContainer;
        JumpBall = Tools.JumpBall;
        JumpDashParticle = Tools.JumpDashParticle;
    }

}
