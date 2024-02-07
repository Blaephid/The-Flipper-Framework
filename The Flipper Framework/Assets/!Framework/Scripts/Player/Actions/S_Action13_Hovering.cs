using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Action13_Hovering : MonoBehaviour
{
    S_CharacterTools Tools;
    S_PlayerPhysics PlayerPhys;
    S_ActionManager Actions;
    Animator CharacterAnimator;
    Transform PlayerSkin;
    S_Control_SoundsPlayer Sounds;

    float floatSpeed = 15;
    public AnimationCurve forceFromSource;

    [HideInInspector] public bool inWind;
    float exitWindTimer;
    float exitWind = 0.6f;
    Vector3 forward;

    [HideInInspector] public float _skiddingStartPoint_;
    float _airSkiddingIntensity_;

    S_Trigger_Updraft hoverForce;

    private void Awake()
    {
        if (PlayerPhys == null)
        {
            Tools = GetComponent<S_CharacterTools>();
            AssignTools();

            AssignStats();
        }

    }

    private void AssignTools()
    {
        PlayerPhys = GetComponent<S_PlayerPhysics>();
        Actions = GetComponent<S_ActionManager>();
        CharacterAnimator = Tools.CharacterAnimator;
        PlayerSkin = Tools.PlayerSkinTransform;

        Sounds = Tools.SoundControl;
    }

    private void AssignStats()
    {
        _skiddingStartPoint_ = Tools.Stats.SkiddingStats.skiddingStartPoint;
        _airSkiddingIntensity_ = Tools.Stats.WhenInAir.airSkiddingForce;
    }

    public void InitialEvents(S_Trigger_Updraft up)
    {
        PlayerPhys.GravityAffects = false;
        inWind = true;
        forward = PlayerSkin.forward;

        hoverForce = up;
    }

    public void updateHover(S_Trigger_Updraft up)
    {
        inWind = true;
        hoverForce = up;
    }

    private void Update()
    {
        CharacterAnimator.SetInteger("Action", 13);

        //Do a homing attack
        if (Actions.Action02.HomingAvailable && Actions.Action02Control.HasTarget && Actions.HomingPressed)
        {

            //Do a homing attack
            if (Actions.Action02 != null && PlayerPhys._homingDelay_ <= 0)
            {
                if (Actions.Action02Control.HomingAvailable)
                {
                    Sounds.HomingAttackSound();
                    Actions.ChangeAction(S_Enums.PlayerStates.Homing);
                    Actions.Action02.InitialEvents();
                }
            }

        }
    }

   

    private void FixedUpdate()
    {
        updateModel();
        PlayerPhys.Grounded = false;

        getForce();

        if (inWind)
        {
            exitWindTimer = 0;

            if(PlayerPhys.rb.velocity.y < floatSpeed)
            {
                PlayerPhys.AddVelocity(hoverForce.transform.up * floatSpeed);
            }

        }
        else
        {
            exitWindTimer += Time.deltaTime;

            if (PlayerPhys.rb.velocity.y < floatSpeed)
            {
                PlayerPhys.AddVelocity(hoverForce.transform.up * (floatSpeed * 0.35f));
            }

            if (exitWindTimer >= exitWind)
            {
                Actions.ChangeAction(S_Enums.PlayerStates.Regular);
            }
        }

        //Skidding
        if ((PlayerPhys.b_normalSpeed < -_skiddingStartPoint_) && !PlayerPhys.Grounded)
        {
            if (PlayerPhys.SpeedMagnitude >= -(_airSkiddingIntensity_ * 0.8f)) PlayerPhys.AddVelocity(PlayerPhys.rb.velocity.normalized * (_airSkiddingIntensity_ * 0.8f) * (PlayerPhys.isRolling ? 0.5f : 1));


            if (PlayerPhys.SpeedMagnitude < 4)
            {
                PlayerPhys.isRolling = false;
                PlayerPhys.b_normalSpeed = 0;

            }
        }


    }
    void updateModel()
    {
        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(PlayerPhys.rb.velocity.x, 0, PlayerPhys.rb.velocity.z);
        if (VelocityMod != Vector3.zero)
        {
            Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
            CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * Actions.Action00.skinRotationSpeed);
        }
        PlayerSkin.forward = forward;
    }

    void getForce()
    {
        float distance = transform.position.y - hoverForce.bottom.position.y;
        float difference = distance / (hoverForce.top.position.y - hoverForce.bottom.position.y);
        floatSpeed = forceFromSource.Evaluate(difference) * hoverForce.power;
        Debug.Log(difference);

        if(difference > 0.98)
        {
            floatSpeed = -Mathf.Clamp(PlayerPhys.rb.velocity.y, -100, 0);
        }
        else if (PlayerPhys.rb.velocity.y > 0)
        {
            floatSpeed = Mathf.Clamp(floatSpeed, 0.5f, PlayerPhys.rb.velocity.y);
        }
    }

    private void OnDisable()
    {
        CharacterAnimator.SetInteger("Action", 1);
        PlayerSkin.forward = CharacterAnimator.transform.forward;
        PlayerPhys.GravityAffects = true;
        inWind = false;
    }
}
