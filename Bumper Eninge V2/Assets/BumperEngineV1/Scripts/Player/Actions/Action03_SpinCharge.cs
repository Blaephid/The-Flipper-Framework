using UnityEngine;
using System.Collections;

public class Action03_SpinCharge : MonoBehaviour
{
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

    Quaternion CharRot;

    bool pressedCurrently = true;


    public float ReleaseShakeAmmount;

    private void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();
        }
    }

    public void InitialEvents()
    {
        sounds.SpinDashSound();
        charge = 0;
        pressedCurrently = true;
    }

    void FixedUpdate()
    {
        charge += SpinDashChargingSpeed;

        //Lock camera on behind
        //Cam.Cam.FollowDirection(3, 14f, -10, 0);

       


        effects.DoSpindash(1, SpinDashChargedEffectAmm * charge, charge,
        effects.GetSpinDashDust(), MaximunCharge);

        Player.rb.velocity /= SpinDashStillForce;

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
                charge += (SpinDashChargingSpeed * 2.5f);
            }

            pressedCurrently = true;
        }

        //Prevents going over the maximum
        if (charge > MaximunCharge)
        {
            charge = MaximunCharge;
        }

        //Stop if not grounded
        if (!Player.Grounded) 
        {
            effects.EndSpinDash();
            if (Actions.RollPressed)
            {
                Actions.Action08.InitialEvents(charge);
                Actions.ChangeAction(8);
            }
            else
            {
                Actions.ChangeAction(0);
            }
           
        }
    }

    IEnumerator delayRelease()
    {

        for (int s = 0; s < 18; s++)
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
            Actions.ChangeAction(0);
        }
        else
        {
            
            sounds.SpinDashReleaseSound();
            Player.rb.velocity = charge * (PlayerSkinTransform.forward);
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
        CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
        CharacterAnimator.SetFloat("GroundSpeed", 0);
        CharacterAnimator.SetBool("Grounded", true);
        CharacterAnimator.SetFloat("NormalSpeed", 0);
        BallAnimator.SetFloat("SpinCharge", charge);
        BallAnimator.speed = charge * BallAnimationSpeedMultiplier;

        //Check if rolling
        //if (Player.Grounded && Player.isRolling) { CharacterAnimator.SetInteger("Action", 1); }
        //CharacterAnimator.SetBool("isRolling", Player.isRolling);

        //Rotation

        if (Player.RawInput.sqrMagnitude < 0.2f)
        {
            CharRot = Quaternion.LookRotation(Player.MainCamera.transform.forward - Player.GroundNormal * Vector3.Dot(Player.MainCamera.transform.forward, Player.GroundNormal), Vector3.up);
        }
        else if (Player.rb.velocity != Vector3.zero)
        {
            CharRot = Quaternion.LookRotation(Player.rb.velocity, Vector3.up);
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