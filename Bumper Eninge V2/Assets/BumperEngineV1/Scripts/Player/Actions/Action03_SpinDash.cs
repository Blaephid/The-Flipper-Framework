using UnityEngine;
using System.Collections;

public class Action03_SpinDash : MonoBehaviour
{
    CharacterStats Stats;
    CharacterTools Tools;

    Animator CharacterAnimator;
    Animator BallAnimator;
    CameraControl Cam;
    public float BallAnimationSpeedMultiplier;

    ActionManager Actions;

    PlayerBhysics Player;
    SonicSoundsControl sounds;
    SonicEffectsControl effects;
    public float SpinDashChargedEffectAmm;


    SkinnedMeshRenderer[] PlayerSkin;
    SkinnedMeshRenderer SpinDashBall;
    Transform PlayerSkinTransform;


    [HideInInspector] public float SpinDashChargingSpeed = 0.3f;
    [HideInInspector] public float MinimunCharge = 10;
    [HideInInspector] public float MaximunCharge = 100;
    [HideInInspector] public float SpinDashStillForce = 20f;
    float charge;
    bool isSpinDashing;
    Vector3 RawPrevInput;
    Quaternion CharRot;

    bool pressedCurrently = true;
    float exitCounter;

    public float ReleaseShakeAmmount;

    private void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            Stats = GetComponent<CharacterStats>();
            AssignStats();
        }
    }

    public void InitialEvents()
    {
        sounds.SpinDashSound();
        charge = 0;
        pressedCurrently = true;
        exitCounter = 0;
    }

    void FixedUpdate()
    {
        charge += SpinDashChargingSpeed;

        //Lock camera on behind
        Cam.Cam.FollowDirection(3, 14f, -10, 0);

        if (Player.RawInput.sqrMagnitude > 0.9f)
        {
            RawPrevInput = Player.RawInput;
            //RawPrevInput = CharacterAnimator.transform.forward;
        }
        else
        {
            RawPrevInput = Vector3.Scale(PlayerSkinTransform.forward, Player.GroundNormal);
            // RawPrevInput = Player.PreviousRawInput;
            //RawPrevInput = CharacterAnimator.transform.forward;
        }

        float energyCharge = charge * 0.15f;
        if (energyCharge > 55f)
            energyCharge = 55f;

        effects.DoSpindash(1, SpinDashChargedEffectAmm * charge, energyCharge);

        Player.p_rigidbody.velocity /= SpinDashStillForce;

        //Counter to exit after not pressing button for a bit;
        if (!pressedCurrently)
        {
            exitCounter += Time.deltaTime;
            if (exitCounter > 0.2f)
                Release();
        }

        //If not pressed, sets the player as exiting
        if (!Actions.RollPressed) 
        {
            pressedCurrently = false; 
        }

        //If the button is pressed while exiting, charge more, means mashing the button is more effective.
        else
        {
            if (!pressedCurrently)
            {
                charge += (SpinDashChargingSpeed * 8);
                pressedCurrently = true;
                exitCounter = 0f;
            }
        }

        //Prevents going over the maximum
        if (charge > MaximunCharge)
        {
            charge = MaximunCharge;
        }

        //Stop if not grounded
        if (!Player.Grounded) 
        { 
            Actions.ChangeAction(0);
            effects.EndSpinDash();
        }
    }

    void Release()
    {
        effects.EndSpinDash();
        HedgeCamera.Shakeforce = (ReleaseShakeAmmount * charge) / 100;
        if (charge < MinimunCharge)
        {
            sounds.Source2.Stop();
            Actions.ChangeAction(0);
        }
        else
        {
            
            sounds.SpinDashReleaseSound();
            Player.p_rigidbody.velocity = charge * (PlayerSkinTransform.forward);
            //Player.MoveInput = new Vector3 (-1, 0, 0);

            Actions.Action00.rollCounter = 0.2f;
            Actions.Action00.Rolling = true;
            Player.isRolling = true;

            Actions.ChangeAction(0);
        }

    }

    void Update()
    {
        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 0);
        CharacterAnimator.SetFloat("YSpeed", Player.p_rigidbody.velocity.y);
        CharacterAnimator.SetFloat("GroundSpeed", 0);
        CharacterAnimator.SetBool("Grounded", true);
        CharacterAnimator.SetFloat("NormalSpeed", 0);
        BallAnimator.SetFloat("SpinCharge", charge);
        BallAnimator.speed = charge * BallAnimationSpeedMultiplier;

        //Check if rolling
        //if (Player.Grounded && Player.isRolling) { CharacterAnimator.SetInteger("Action", 1); }
        //CharacterAnimator.SetBool("isRolling", Player.isRolling);

        //Rotation

        if (Player.RawInput.sqrMagnitude < 0.9f)
        {
            CharRot = Quaternion.LookRotation(Player.MainCamera.transform.forward - Player.GroundNormal * Vector3.Dot(Player.MainCamera.transform.forward, Player.GroundNormal), Vector3.up);
        }
        else if (Player.p_rigidbody.velocity != Vector3.zero)
        {
            CharRot = Quaternion.LookRotation(Player.p_rigidbody.velocity, Vector3.up);
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
        SpinDashChargingSpeed = Stats.SpinDashChargingSpeed;
        MinimunCharge = Stats.MinimunCharge;
        MaximunCharge = Stats.MaximunCharge;
        SpinDashStillForce = Stats.SpinDashStillForce;
    }
    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();

        CharacterAnimator = Tools.CharacterAnimator;
        BallAnimator = Tools.BallAnimator;
        sounds = Tools.SoundControl;
        effects = Tools.EffectsControl;

        PlayerSkin = Tools.PlayerSkin;
        PlayerSkinTransform = Tools.PlayerSkinTransform;
        SpinDashBall = Tools.SpinDashBall.GetComponent<SkinnedMeshRenderer>();



    }
}

//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using Luminosity.IO;

//public class Action03_SpinDash : MonoBehaviour
//{

//    PlayerBhysics Player;
//    ActionManager Actions;


//    public bool isPeelout { get; set; }
//    float skinRotationSpeed;
//    Animator CharacterAnimator;
//    Animator BallAnimator;
//    HedgeCamera Cam;
//    Transform SkinOffset;
//    SonicSoundsControl sounds;
//    SonicEffectsControl effects;
//    SkinnedMeshRenderer[] PlayerSkin;
//    MeshRenderer[] PlayerExtras;

//    float BallAnimationSpeedMultiplier;
//    float SpinDashChargedEffectAmm;
//    float SpinDashChargingSpeed = 0.3f;
//    float MinimunCharge = 10;
//    float MaximunCharge = 100;
//    float Overcharge = 15;
//    float SpinDashStillForce = 20f;
//    float ReleaseCamLagAmmount;
//    float ForceIntoRollfor = 1;
//    float ReleaseShakeAmmount;

//    int buttonsPressed;
//    float OriginalSlopeStandingLimit;
//    bool isRevingUp;
//    float charge;
//    Quaternion CharRot;


//    Objects_Interaction Interaction;

//    [Header("Audio Sound For Spindash")]
//    [SerializeField]
//    Vector2 looptimeforSound = new Vector2(0.4f, 0.15f);
//    [SerializeField]
//    AnimationCurve PitchRate;
//    [SerializeField]
//    float inputPitch = 1.2f, pitchDecreaseLerp = 2f;

//    float soundTimer, pitch;


//    void Awake()
//    {
//        Player = GetComponent<PlayerBhysics>();
//        Actions = GetComponent<ActionManager>();
//        CharacterAnimator = Player.Resources.CharacterAnimator;
//        BallAnimator = Player.Resources.JumpBallAnimator;
//        Cam = Player.Resources.MainCamera;
//        SkinOffset = Player.Resources.SkinOffset;
//        sounds = Player.Resources.Sounds;
//        effects = Player.Resources.Effects;
//        PlayerSkin = Player.Resources.PlayerSkins;
//        fetchStats();
//        PlayerExtras = Player.Resources.PlayerExtraParts;
//        OriginalSlopeStandingLimit = Player.SlopeStandingLimit;
//        Interaction = GetComponent<Objects_Interaction>();
//    }

//    public void InitialEvents(bool Peelout = false)
//    {
//        isPeelout = Peelout;
//        sounds.SpinDashSound(PitchRate.Evaluate(0));
//        charge = MinimunCharge;
//        //Lock camera on behind
//        //Cam.Cam.FollowDirection(12, 5f, -10,12);


//    }

//    private void OnDisable()
//    {

//        Player.SlopeStandingLimit = OriginalSlopeStandingLimit;
//    }

//    void FixedUpdate()
//    {
//        charge += SpinDashChargingSpeed;

//        //Lock camera on behind
//        Cam.FollowDirection(2, 5f, -10, 12);


//        effects.DoSpindashDust(1, SpinDashChargedEffectAmm * charge);

//        Player.p_rigidbody.velocity /= SpinDashStillForce;


//        Player.SlopeStandingLimit = 0f;








//        if (charge > MaximunCharge)
//        {
//            charge = MaximunCharge;
//        }

//        //Stop if not grounded
//        if (!Player.Grounded) { Actions.ChangeAction(0); }
//    }

//    void Release()
//    {
//        if (charge < MinimunCharge)
//        {
//            sounds.Source2.Stop();
//            Actions.ChangeAction(0);
//        }
//        else
//        {
//            Cam.CamLagSet(ReleaseCamLagAmmount, ReleaseCamLagAmmount);
//            HedgeCamera.Shakeforce = (ReleaseShakeAmmount * charge) / 100;
//            sounds.SpinDashReleaseSound();
//            Player.p_rigidbody.velocity = charge * (SkinOffset.forward);
//            Actions.ChangeAction(0);

//            if (!isPeelout)
//            {
//                Player.isRolling = true;
//                Player.Resources.Actions.isRolling = true;
//                Actions.Action00.KeepRollingFor(ForceIntoRollfor);
//            }
//        }

//    }

//    void Update()
//    {

//        SoundLooping();
//        //Set Animator Parameters
//        CharacterAnimator.SetInteger("Action", 0);
//        if (isPeelout) CharacterAnimator.SetInteger("Action", 33);
//        CharacterAnimator.SetFloat("YSpeed", Player.p_rigidbody.velocity.y);
//        CharacterAnimator.SetFloat("GroundSpeed", charge);
//        CharacterAnimator.SetBool("Grounded", true);
//        CharacterAnimator.SetFloat("NormalSpeed", 0);
//        BallAnimator.SetFloat("SpinCharge", charge);

//        BallAnimator.speed = charge * BallAnimationSpeedMultiplier;
//        Player.Resources.Actions.isRolling = true;

//        //Check if rolling
//        if (!Actions.isPaused)
//        {
//            buttonsPressed = 0;

//            if (InputManager.GetButton("B-Bounce", Actions.Player_ID)) buttonsPressed++;
//            if (InputManager.GetButton("X-Stomp", Actions.Player_ID)) buttonsPressed++;
//            if (InputManager.GetButton("R1-Roll", Actions.Player_ID)) buttonsPressed++;
//            if (InputManager.GetButton("A-Jump", Actions.Player_ID)) buttonsPressed++;

//            if (buttonsPressed == 0)
//            {
//                Release();
//            }
//            else
//            {
//                if (!isPeelout)
//                {
//                    for (int i = 0; i < PlayerSkin.Length; i++)
//                    {
//                        PlayerSkin[i].enabled = false;
//                    }
//                    for (int i = 0; i < PlayerExtras.Length; i++)
//                    {
//                        PlayerExtras[i].enabled = false;
//                    }
//                }
//            }



//            if (buttonsPressed > 1 && !isRevingUp) { isRevingUp = true; charge += Overcharge; soundTimer = 99; pitch = inputPitch; effects.SpindashPart.Emit(1); }

//            if (buttonsPressed <= 1) { isRevingUp = false; }
//        }

//        //Rotation

//        if (Player.RawInput.sqrMagnitude < 0.1f)
//        {
//            CharRot = Quaternion.LookRotation(Player.Resources.MainCamera.transform.forward - Player.GroundNormal * Vector3.Dot(Player.Resources.MainCamera.transform.forward, Player.GroundNormal), Player.transform.up);
//        }
//        else
//        {
//            CharRot = Quaternion.LookRotation(Player.p_rigidbody.velocity, Player.transform.up);
//        }
//        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);



//    }
//    //0.15 max 0.40 min;
//    void SoundLooping()
//    {
//        float range = (charge - MinimunCharge) / (MaximunCharge - MinimunCharge);
//        float newPitch = PitchRate.Evaluate(range);
//        float tempo = Mathf.Lerp(looptimeforSound.x, looptimeforSound.y, range);
//        soundTimer += Time.deltaTime;
//        pitch = (newPitch > pitch) ? newPitch : Mathf.Lerp(pitch, newPitch, Time.deltaTime * pitchDecreaseLerp);

//        if (soundTimer > tempo)
//        {
//            soundTimer = 0;
//            sounds.SpinDashSound(pitch);
//        }
//    }


//    public void ResetSpinDashVariables()
//    {
//        if (Player != null)
//        {
//            for (int i = 0; i < PlayerSkin.Length; i++)
//            {
//                PlayerSkin[i].enabled = true;
//            }
//            for (int i = 0; i < PlayerExtras.Length; i++)
//            {
//                PlayerExtras[i].enabled = true;
//            }
//            sounds.Source2.pitch = 1;
//            charge = 60;
//        }
//    }

//    void fetchStats()
//    {
//        CharacterStatsHolder character = Player.Resources.CharacterStats;

//        skinRotationSpeed = Player.Resources.skinRotationSpeed;

//        BallAnimationSpeedMultiplier = character.BallAnimationSpeedMultiplier;
//        SpinDashChargedEffectAmm = character.SpinDashChargedEffectAmm;
//        SpinDashChargingSpeed = character.SpinDashChargingSpeed;
//        MinimunCharge = character.MinimunCharge;
//        MaximunCharge = character.MaximunCharge;
//        Overcharge = character.Overcharge;
//        SpinDashStillForce = character.SpinDashStillForce;
//        ReleaseCamLagAmmount = character.ReleaseCamLagAmmount;
//        ReleaseShakeAmmount = character.ReleaseShakeAmmount;
//        ForceIntoRollfor = character.ForceIntoRollfor;
//    }


//}

