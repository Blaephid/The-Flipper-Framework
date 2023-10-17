using UnityEngine;
using System.Collections;

public class Action02_Homing : MonoBehaviour
{

    public ActionManager Action;
    public Animator CharacterAnimator;
    PlayerBhysics Player;

    public bool isAdditive;
    public float HomingAttackSpeed;
    public float AirDashSpeed;
    public float HomingTimerLimit;
    public float FacingAmount;
    public GameObject HomingTrailContainer;
    public GameObject HomingTrail;
    public GameObject JumpDashParticle;
    float Timer;
    float Speed;
    float Aspeed;
    Vector3 direction;


    public Transform Target { get; set; }
    public float skinRotationSpeed;
    public bool HomingAvailable { get; set; }
    public bool IsAirDash { get; set; }

    void Awake()
    {
        HomingAvailable = true;
        Player = GetComponent<PlayerBhysics>();
    }

    public void InitialEvents()
    {

        Action.Action01.JumpBall.SetActive(false);
        if (HomingTrailContainer.transform.childCount < 1)
        {
            GameObject HomingTrailClone = Instantiate(HomingTrail, HomingTrailContainer.transform.position, Quaternion.identity) as GameObject;
            HomingTrailClone.transform.parent = HomingTrailContainer.transform;

            GameObject JumpDashParticleClone = Instantiate(JumpDashParticle, HomingTrailContainer.transform.position, Quaternion.identity) as GameObject;
            JumpDashParticleClone.transform.parent = HomingTrailContainer.transform;
        }


        if (Action.Action02Control.HasTarget)
        {
            Target = HomingAttackControl.TargetObject.transform;
        }

        Timer = 0;
        HomingAvailable = false;

        if (isAdditive)
        {

            // Apply Max Speed Limit
            float XZmag = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z).magnitude;

            if (XZmag < HomingAttackSpeed)
            {
                Speed = HomingAttackSpeed;
            }
            else
            {
                Speed = XZmag;
            }

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
            Aspeed = AirDashSpeed;
            Speed = HomingAttackSpeed;
        }

        //Check if not facing Object
        if (!IsAirDash)
        {
            Vector3 TgyXY = HomingAttackControl.TargetObject.transform.position.normalized;
            TgyXY.y = 0;
            float facingAmmount = Vector3.Dot(Player.PreviousRawInput.normalized, TgyXY);
            // //Debug.Log(facingAmmount);
            // if (facingAmmount < FacingAmount) { IsAirDash = true; }
        }

    }

    void Update()
    {

        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 1);
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
        Timer += 1;

        CharacterAnimator.SetInteger("Action", 1);

        if (IsAirDash)
        {
            if (Player.RawInput != Vector3.zero)
            {
                Player.p_rigidbody.velocity = transform.TransformDirection(Player.RawInput) * Aspeed;
            }
            else
            {
                // //Debug.Log("prev");
                Player.p_rigidbody.velocity = transform.TransformDirection(Player.PreviousRawInput) * Aspeed;
            }
            Timer = HomingTimerLimit + 10;
        }
        else
        {
            direction = Target.position - transform.position;
            Player.p_rigidbody.velocity = direction.normalized * Speed;
        }

        //Set Player location when close enough, for precision.
        if (Target != null && Vector3.Distance(Target.position, transform.position) < 5)
        {
            transform.position = Target.position;
            //Debug.Log ("CloseEnough");
        }

        //End homing attck if on air for too long
        if (Timer > HomingTimerLimit)
        {
            Action.ChangeAction(0);
        }
    }

    public void ResetHomingVariables()
    {
        Timer = 0;
        HomingTrailContainer.transform.DetachChildren();
        //IsAirDash = false;
    }



}
