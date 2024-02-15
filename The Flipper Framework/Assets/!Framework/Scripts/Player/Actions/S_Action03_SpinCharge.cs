using UnityEngine;
using System.Collections;

public class S_Action03_SpinCharge : MonoBehaviour
{
    S_CharacterTools Tools;
    S_PlayerInput Inp;

    Animator CharacterAnimator;
    Animator BallAnimator;
    S_Handler_Camera Cam;
    public float BallAnimationSpeedMultiplier;
    GameObject LowerCapsule;
    GameObject CharacterCapsule;

    S_ActionManager Actions;
    S_Handler_quickstep quickstepManager;

    S_PlayerPhysics Player;
    S_Control_SoundsPlayer sounds;
    S_Control_EffectsPlayer effects;
    public float SpinDashChargedEffectAmm;


    SkinnedMeshRenderer[] PlayerSkin;
    SkinnedMeshRenderer SpinDashBall;
    Transform PlayerSkinTransform;

    float time = 0;
    [HideInInspector] public float _spinDashChargingSpeed_ = 0.3f;
    [HideInInspector] public float _minimunCharge_ = 10;
    [HideInInspector] public float _maximunCharge_ = 100;
    [HideInInspector] public float _spinDashStillForce_ = 20f;
    AnimationCurve _speedLossByTime_;
    AnimationCurve _forceGainByAngle_;
    AnimationCurve _gainBySpeed_;

    bool tapped = false;
    float charge;

    Quaternion CharRot;

    bool pressedCurrently = true;


    float _releaseShakeAmmount_;

    private void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<S_CharacterTools>();
            AssignTools();

            AssignStats();

            quickstepManager = GetComponent<S_Handler_quickstep>();
            quickstepManager.enabled = false;
        }
    }

    public void InitialEvents()
    {
        sounds.SpinDashSound();
        charge = 20;
        time = 0;
        pressedCurrently = true;
        tapped = true;

        LowerCapsule.SetActive(true);
        CharacterCapsule.SetActive(false);
    }

    void FixedUpdate()
    {
        charge += _spinDashChargingSpeed_;
        time += Time.deltaTime;

        //Lock camera on behind
        //Cam.Cam.FollowDirection(3, 14f, -10, 0);
        Inp.LockCam = false;
        Cam.Cam.lookTimer = 0;


        effects.DoSpindash(1, SpinDashChargedEffectAmm * charge, charge,
        effects.GetSpinDashDust(), _maximunCharge_);

        Player._moveInput *= 0.4f;
        float stillForce = (_spinDashStillForce_ * _speedLossByTime_.Evaluate(time)) + 1;
		if (stillForce * 4 < Player._horizontalSpeedMagnitude)
		{
			Player.AddCoreVelocity(Player._RB.velocity.normalized * -stillForce);
		}

        //Counter to exit after not pressing button for a bit;
        

        //If not pressed, sets the player as exiting
        if (!Actions.spinChargePressed) 
        {
            if (pressedCurrently)
                StartCoroutine(delayRelease());
            pressedCurrently = false; 
        }

        //If the button is pressed while exiting, charge more, means mashing the button is more effective.
        else
        {
            if (!pressedCurrently)
            {
                tapped = false;
                charge += (_spinDashChargingSpeed_ * 2.5f);
            }

            pressedCurrently = true;
        }

        //Prevents going over the maximum
        if (charge > _maximunCharge_)
        {
            charge = _maximunCharge_;
        }

        startFall();

        Actions.skid.spinSkid();

        handleInput();
    }

    void startFall()
    {
        //Stop if not grounded
        if (!Player._isGrounded)
        {
            Actions.SpecialPressed = false;
            effects.EndSpinDash();
            if (Actions.RollPressed)
            {
                Actions.Action08.TryDropCharge();
            }
            else
            {
                Actions.ChangeAction(S_Enums.PlayerStates.Regular);
            }

        }
    }

    void handleInput()
    {
        /////Quickstepping
        ///
        //Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
        if (Actions.RightStepPressed)
        {
            quickstepManager.pressRight();
        }
        else if (Actions.LeftStepPressed)
        {
            quickstepManager.pressLeft();
        }

        //Enable Quickstep right or left
        if (Actions.RightStepPressed && !quickstepManager.enabled)
        {
            if (Player._horizontalSpeedMagnitude > 10f)
            {

                quickstepManager.initialEvents(true);
                quickstepManager.enabled = true;
            }
        }

        else if (Actions.LeftStepPressed && !quickstepManager.enabled)
        {
            if (Player._horizontalSpeedMagnitude > 10f)
            {
                quickstepManager.initialEvents(false);
                quickstepManager.enabled = true;
            }
        }
    }


    IEnumerator delayRelease()
    {
        int waitFor = 14;
        if (tapped)
            waitFor = 8;

        for (int s = 0; s < waitFor; s++)
        {
            yield return new WaitForFixedUpdate();
            if(pressedCurrently)
            {
                yield break;
            }
        }
        Release();
    }

    void Release()
    {
        if (Actions.eventMan != null) Actions.eventMan.SpinChargesPeformed += 1;

        effects.EndSpinDash();
        S_HedgeCamera.Shakeforce = (_releaseShakeAmmount_ * charge) / 100;
        if (charge < _minimunCharge_)
        {
            sounds.Source2.Stop();
            Actions.ChangeAction(S_Enums.PlayerStates.Regular);
        }
        else
        {            
            sounds.SpinDashReleaseSound();

            Vector3 newForce = charge * (PlayerSkinTransform.forward);
            float dif = Vector3.Dot(newForce.normalized, Player._RB.velocity.normalized);

            if(Player._horizontalSpeedMagnitude > 20)
                newForce *= _forceGainByAngle_.Evaluate(dif);
            newForce *= _gainBySpeed_.Evaluate(Player._horizontalSpeedMagnitude / Player._currentMaxSpeed);

            Player.AddCoreVelocity(newForce);

            CharacterAnimator.SetFloat("GroundSpeed", Player._RB.velocity.magnitude);

            
            Actions.Action00.Rolling = true;
            Player._isRolling = true;
            Actions.Action00.rollCounter = 0.3f;

            Inp.LockInputForAWhile(0, false);

            Actions.ChangeAction(S_Enums.PlayerStates.Regular);
        }

    }

    void Update()
    {
        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 0);
        CharacterAnimator.SetFloat("YSpeed", Player._RB.velocity.y);
        //CharacterAnimator.SetFloat("GroundSpeed", 0);
        CharacterAnimator.SetBool("Grounded", true);
        BallAnimator.SetFloat("SpinCharge", charge);
        BallAnimator.speed = charge * BallAnimationSpeedMultiplier;

        //Check if rolling
        //if (Player.Grounded && Player.isRolling) { CharacterAnimator.SetInteger("Action", 1); }
        //CharacterAnimator.SetBool("isRolling", Player.isRolling);

        //Rotation

        if (Player.RawInput.sqrMagnitude > 0.1f)
        {
            
            CharRot = Quaternion.LookRotation(Tools.MainCamera.transform.forward - Player._groundNormal * Vector3.Dot(Tools.MainCamera.transform.forward, Player._groundNormal), transform.up);
        }
        else if (Player._RB.velocity != Vector3.zero)
        {
            //CharRot = Quaternion.LookRotation(Player.rb.velocity, Vector3.up);
        }
        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * Actions.Action00.skinRotationSpeed);


        for (int i = 0; i < PlayerSkin.Length; i++)
        {
            PlayerSkin[i].enabled = false;
        }
        SpinDashBall.enabled = true;
    }

    public void ResetSpinDashVariables()
    {
        for (int i = 0; i < PlayerSkin.Length; i++)
        {
            PlayerSkin[i].enabled = true;
        }
        SpinDashBall.enabled = false;
        charge = 0;
    }

    private void AssignStats()
    {
        _spinDashChargingSpeed_ = Tools.Stats.SpinChargeStats.chargingSpeed;
        _minimunCharge_ = Tools.Stats.SpinChargeStats.minimunCharge;
        _maximunCharge_ = Tools.Stats.SpinChargeStats.maximunCharge;
        _spinDashStillForce_ = Tools.Stats.SpinChargeStats.forceAgainstMovement;
        _speedLossByTime_ = Tools.Stats.SpinChargeStats.SpeedLossByTime;
        _forceGainByAngle_ = Tools.Stats.SpinChargeStats.ForceGainByAngle;
        _gainBySpeed_ = Tools.Stats.SpinChargeStats.ForceGainByCurrentSpeed;
        _releaseShakeAmmount_ = Tools.Stats.SpinChargeStats.releaseShakeAmmount;
    }
    private void AssignTools()
    {
        Player = GetComponent<S_PlayerPhysics>();
        Actions = GetComponent<S_ActionManager>();
        Cam = GetComponent<S_Handler_Camera>();
        Inp = GetComponent<S_PlayerInput>();

        CharacterAnimator = Tools.CharacterAnimator;
        BallAnimator = Tools.BallAnimator;
        sounds = Tools.SoundControl;
        effects = Tools.EffectsControl;

        PlayerSkin = Tools.PlayerSkin;
        PlayerSkinTransform = Tools.PlayerSkinTransform;
        SpinDashBall = Tools.SpinDashBall.GetComponent<SkinnedMeshRenderer>();
        LowerCapsule = Tools.crouchCapsule;
        CharacterCapsule = Tools.characterCapsule;
    }
}