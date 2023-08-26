using UnityEngine;
using System.Collections;

public class Action03_SpinCharge : MonoBehaviour
{
    CharacterTools Tools;
    PlayerBinput Inp;

    Animator CharacterAnimator;
    Animator BallAnimator;
    CameraControl Cam;
    public float BallAnimationSpeedMultiplier;
    GameObject lowerCapsule;
    GameObject characterCapsule;

    ActionManager Actions;
    quickstepHandler quickstepManager;

    PlayerBhysics Player;
    SonicSoundsControl sounds;
    SonicEffectsControl effects;
    public float SpinDashChargedEffectAmm;


    SkinnedMeshRenderer[] PlayerSkin;
    SkinnedMeshRenderer SpinDashBall;
    Transform PlayerSkinTransform;

    float time = 0;
    [HideInInspector] public float SpinDashChargingSpeed = 0.3f;
    [HideInInspector] public float MinimunCharge = 10;
    [HideInInspector] public float MaximunCharge = 100;
    [HideInInspector] public float SpinDashStillForce = 20f;
    AnimationCurve SpeedLossByTime;
    AnimationCurve ForceGainByAngle;
    AnimationCurve gainBySpeed;

    bool tapped = false;
    float charge;

    Quaternion CharRot;

    bool pressedCurrently = true;


    float ReleaseShakeAmmount;

    private void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();

            quickstepManager = GetComponent<quickstepHandler>();
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

        lowerCapsule.SetActive(true);
        characterCapsule.SetActive(false);
    }

    void FixedUpdate()
    {
        charge += SpinDashChargingSpeed;
        time += Time.deltaTime;

        //Lock camera on behind
        //Cam.Cam.FollowDirection(3, 14f, -10, 0);
        Inp.LockCam = false;
        Cam.Cam.lookTimer = 0;


        effects.DoSpindash(1, SpinDashChargedEffectAmm * charge, charge,
        effects.GetSpinDashDust(), MaximunCharge);

        Player.MoveInput *= 0.4f;
        float stillForce = (SpinDashStillForce * SpeedLossByTime.Evaluate(time)) + 1;
        Player.rb.velocity /= stillForce;

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
                charge += (SpinDashChargingSpeed * 2.5f);
            }

            pressedCurrently = true;
        }

        //Prevents going over the maximum
        if (charge > MaximunCharge)
        {
            charge = MaximunCharge;
        }

        startFall();

        Actions.skid.spinSkid();

        handleInput();
    }

    void startFall()
    {
        //Stop if not grounded
        if (!Player.Grounded)
        {
            Actions.SpecialPressed = false;
            effects.EndSpinDash();
            if (Actions.RollPressed)
            {
                Actions.Action08.InitialEvents(charge);
                Actions.ChangeAction(ActionManager.States.DropCharge);
            }
            else
            {
                Actions.ChangeAction(ActionManager.States.Regular);
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
            if (Player.HorizontalSpeedMagnitude > 10f)
            {

                quickstepManager.initialEvents(true);
                quickstepManager.enabled = true;
            }
        }

        else if (Actions.LeftStepPressed && !quickstepManager.enabled)
        {
            if (Player.HorizontalSpeedMagnitude > 10f)
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
        HedgeCamera.Shakeforce = (ReleaseShakeAmmount * charge) / 100;
        if (charge < MinimunCharge || Actions.JumpPressed)
        {
            sounds.Source2.Stop();
            Actions.ChangeAction(ActionManager.States.Regular);
        }
        else
        {
            
            sounds.SpinDashReleaseSound();

            Vector3 newForce = charge * (PlayerSkinTransform.forward);
            float dif = Vector3.Dot(newForce.normalized, Player.rb.velocity.normalized);

            if(Player.HorizontalSpeedMagnitude > 20)
                newForce *= ForceGainByAngle.Evaluate(dif);
            newForce *= gainBySpeed.Evaluate(Player.HorizontalSpeedMagnitude / Player.MaxSpeed);

            Player.rb.velocity += newForce;

            CharacterAnimator.SetFloat("XZSpeed", Mathf.Abs((Player.rb.velocity.x + Player.rb.velocity.z) / 2));
            CharacterAnimator.SetFloat("GroundSpeed", Player.rb.velocity.magnitude);

            
            Actions.Action00.Rolling = true;
            Player.isRolling = true;
            Actions.Action00.rollCounter = 0.3f;

            Inp.LockInputForAWhile(0, false);

            Actions.ChangeAction(ActionManager.States.Regular);
        }

    }

    void Update()
    {
        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 0);
        CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
        //CharacterAnimator.SetFloat("GroundSpeed", 0);
        CharacterAnimator.SetBool("Grounded", true);
        CharacterAnimator.SetFloat("NormalSpeed", 0);
        BallAnimator.SetFloat("SpinCharge", charge);
        BallAnimator.speed = charge * BallAnimationSpeedMultiplier;

        //Check if rolling
        //if (Player.Grounded && Player.isRolling) { CharacterAnimator.SetInteger("Action", 1); }
        //CharacterAnimator.SetBool("isRolling", Player.isRolling);

        //Rotation

        if (Player.RawInput.sqrMagnitude > 0.1f)
        {
            
            CharRot = Quaternion.LookRotation(Player.MainCamera.transform.forward - Player.GroundNormal * Vector3.Dot(Player.MainCamera.transform.forward, Player.GroundNormal), transform.up);
        }
        else if (Player.rb.velocity != Vector3.zero)
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
        SpinDashChargingSpeed = Tools.stats.SpinDashChargingSpeed;
        MinimunCharge = Tools.stats.MinimunCharge;
        MaximunCharge = Tools.stats.MaximunCharge;
        SpinDashStillForce = Tools.stats.SpinDashStillForce;
        SpeedLossByTime = Tools.coreStats.SpeedLossByTime;
        ForceGainByAngle = Tools.coreStats.ForceGainByAngle;
        gainBySpeed = Tools.coreStats.gainBySpeed;
        ReleaseShakeAmmount = Tools.coreStats.ReleaseShakeAmmount;
    }
    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();
        Inp = GetComponent<PlayerBinput>();

        CharacterAnimator = Tools.CharacterAnimator;
        BallAnimator = Tools.BallAnimator;
        sounds = Tools.SoundControl;
        effects = Tools.EffectsControl;

        PlayerSkin = Tools.PlayerSkin;
        PlayerSkinTransform = Tools.PlayerSkinTransform;
        SpinDashBall = Tools.SpinDashBall.GetComponent<SkinnedMeshRenderer>();
        lowerCapsule = Tools.crouchCapsule;
        characterCapsule = Tools.characterCapsule;
    }
}