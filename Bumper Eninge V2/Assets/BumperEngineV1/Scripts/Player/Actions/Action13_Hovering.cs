using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action13_Hovering : MonoBehaviour
{
    CharacterTools Tools;
    PlayerBhysics player;
    ActionManager Actions;
    Animator CharacterAnimator;
    Transform playerSkin;
    SonicSoundsControl sounds;

    float floatSpeed = 15;
    public AnimationCurve forceFromSource;

    [HideInInspector] public bool inWind;
    float exitWindTimer;
    float exitWind = 0.6f;
    Vector3 forward;

    [HideInInspector] public float SkiddingStartPoint;
    float AirSkiddingIntensity;

    updraft hoverForce;

    private void Awake()
    {
        if (player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();
        }

    }

    private void AssignTools()
    {
        player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        CharacterAnimator = Tools.CharacterAnimator;
        playerSkin = Tools.PlayerSkinTransform;

        sounds = Tools.SoundControl;
    }

    private void AssignStats()
    {
        SkiddingStartPoint = Tools.stats.SkiddingStartPoint;
        AirSkiddingIntensity = Tools.stats.AirSkiddingForce;
    }

    public void InitialEvents(updraft up)
    {
        player.GravityAffects = false;
        inWind = true;
        forward = playerSkin.forward;

        hoverForce = up;
    }

    public void updateHover(updraft up)
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
            if (Actions.Action02 != null && player.HomingDelay <= 0)
            {
                if (Actions.Action02Control.HomingAvailable)
                {
                    sounds.HomingAttackSound();
                    Actions.ChangeAction(ActionManager.States.Homing);
                    Actions.Action02.InitialEvents();
                }
            }

        }
    }

   

    private void FixedUpdate()
    {
        updateModel();
        player.Grounded = false;

        getForce();

        if (inWind)
        {
            exitWindTimer = 0;

            if(player.rb.velocity.y < floatSpeed)
            {
                player.AddVelocity(hoverForce.transform.up * floatSpeed);
            }

        }
        else
        {
            exitWindTimer += Time.deltaTime;

            if (player.rb.velocity.y < floatSpeed)
            {
                player.AddVelocity(hoverForce.transform.up * (floatSpeed * 0.35f));
            }

            if (exitWindTimer >= exitWind)
            {
                Actions.ChangeAction(0);
            }
        }

        //Skidding
        if ((player.b_normalSpeed < -SkiddingStartPoint) && !player.Grounded)
        {
            if (player.SpeedMagnitude >= -(AirSkiddingIntensity * 0.8f)) player.AddVelocity(player.rb.velocity.normalized * (AirSkiddingIntensity * 0.8f) * (player.isRolling ? 0.5f : 1));


            if (player.SpeedMagnitude < 4)
            {
                player.isRolling = false;
                player.b_normalSpeed = 0;

            }
        }


    }
    void updateModel()
    {
        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(player.rb.velocity.x, 0, player.rb.velocity.z);
        if (VelocityMod != Vector3.zero)
        {
            Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
            CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * Actions.Action00.skinRotationSpeed);
        }
        playerSkin.forward = forward;
    }

    void getForce()
    {
        float distance = transform.position.y - hoverForce.bottom.position.y;
        float difference = distance / (hoverForce.top.position.y - hoverForce.bottom.position.y);
        floatSpeed = forceFromSource.Evaluate(difference) * hoverForce.power;
        Debug.Log(difference);

        if(difference > 0.98)
        {
            floatSpeed = -Mathf.Clamp(player.rb.velocity.y, -100, 0);
        }
        else if (player.rb.velocity.y > 0)
        {
            floatSpeed = Mathf.Clamp(floatSpeed, 0.5f, player.rb.velocity.y);
        }
    }

    private void OnDisable()
    {
        CharacterAnimator.SetInteger("Action", 1);
        playerSkin.forward = CharacterAnimator.transform.forward;
        player.GravityAffects = true;
        inWind = false;
    }
}
