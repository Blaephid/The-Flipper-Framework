using UnityEngine;
using System.Collections;

public class Action08_DropDash : MonoBehaviour
{
    CharacterTools Tools;

    Animator CharacterAnimator;
    Animator BallAnimator;
    public float BallAnimationSpeedMultiplier;

    CameraControl Cam;
    ActionManager Actions;
    PlayerBhysics Player;
    SonicSoundsControl sounds;
    [HideInInspector] public ParticleSystem DropEffect;
    GameObject JumpBall;
    public float SpinDashChargedEffectAmm;

    public bool DropDashAvailable { get; set; }

    SkinnedMeshRenderer[] PlayerSkin;
    GameObject SpinDashBall;
    Transform PlayerSkinTransform;
    Transform DirectionReference;

    [HideInInspector] public float SpinDashChargingSpeed = 0.3f;
    [HideInInspector] public float MinimunCharge = 10;
    [HideInInspector] public float MaximunCharge = 100;
    bool Charging = true;

    float charge;
    bool isSpinDashing;
    Vector3 RawPrevInput;
    Quaternion CharRot;

    public float ReleaseShakeAmmount;

    void Awake()
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
        ////Debug.Log ("startDropDash");
        CharacterAnimator.SetInteger("Action", 1);
        CharacterAnimator.SetBool("Grounded", false);

        sounds.SpinDashSound();
        charge = 15f;
        Charging = true;
    }

    void FixedUpdate()
    {
        if (Charging)
        {
            charge += SpinDashChargingSpeed;

            //Lock camera on behind
            // Cam.Cam.FollowDirection(3, 14f, -10,0);

            if (Player.RawInput.sqrMagnitude > 0.9f)
            {
                //RawPrevInput = Player.RawInput;
                //RawPrevInput = Vector3.Scale(CharacterAnimator.transform.forward, Player.GroundNormal);
                RawPrevInput = CharacterAnimator.transform.forward;
            }
            else
            {
                //RawPrevInput = Vector3.Scale(CharacterAnimator.transform.forward, Player.GroundNormal);
                //RawPrevInput = Player.PreviousRawInput;
                RawPrevInput = CharacterAnimator.transform.forward;
            }

            if (DropEffect.isPlaying == false)
            {
                DropEffect.Play();
            }

            // Player.rigidbody.velocity /= SpinDashStillForce;

            if (!Actions.RollPressed)
            {
                if (DropEffect.isPlaying == true)
                {
                    DropEffect.Stop();
                }

                sounds.Source2.Stop();


                Actions.ChangeAction(0);
            }

            if (charge > MaximunCharge)
            {
                charge = MaximunCharge;
            }

            //Stop if grounded
            if (Player.Grounded)
            {
                if (!Actions.JumpPressed)
                    Release();
                else
                {
                    Player.isRolling = true;
                    JumpBall.SetActive(false);
                    if (DropEffect.isPlaying == true)
                    {
                        DropEffect.Stop();
                    }
                }

                Actions.JumpPressed = false;
                Actions.ChangeAction(0);
            }

            else if (Actions.SpecialPressed && charge > MinimunCharge)
            {
                AirRelease();
            }
        }

        if (Player.Grounded)
        {
            Actions.ChangeAction(0);
        }
    }

    void AirRelease()
    {
        Actions.JumpPressed = false;
        Actions.SpecialPressed = false;
        Actions.HomingPressed = false;
        charge *= 0.6f;

        StartCoroutine(airDash());

        Release();
        if (GetComponent<Action11_AirDash>() != null)
            GetComponent<Action11_AirDash>().AirDashParticle();
        Charging = false;
    }

    IEnumerator airDash()
    {
        float time = Mathf.Round(charge / 30);
        time /= 10;

        Debug.Log(time);

        Player.GravityAffects = false;
        yield return new WaitForSeconds(time);
        Player.GravityAffects = true;
        Actions.ChangeAction(1);

    }

    void Release()
    {
        JumpBall.SetActive(false);

        DropDashAvailable = false;
        Actions.RollPressed = false;
        Player.isRolling = false;

        Vector3 newForward = Player.rb.velocity - transform.up * Vector3.Dot(Player.rb.velocity, transform.up);

        if (newForward.magnitude < 0.1f)
        {
            newForward = CharacterAnimator.transform.forward;
        }

        CharRot = Quaternion.LookRotation(newForward, transform.up);
        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * 200);

        HedgeCamera.Shakeforce = (ReleaseShakeAmmount * charge) / 100;
        if (charge < MinimunCharge)
        {
            //sounds.Source2.Stop();
            charge = MinimunCharge;
        }

        Player.isRolling = true;
        sounds.SpinDashReleaseSound();
        ////Debug.Log (charge);

        Vector3 newVec = charge * (CharacterAnimator.transform.forward);

        if (new Vector3(newVec.x, 0f, newVec.z).magnitude > Player.HorizontalSpeedMagnitude)
        {
            Player.rb.velocity = newVec;
            Cam.Cam.FollowDirection(18, 25f);
        }
        else
        {
            Player.rb.velocity += newVec * 0.3f;
            Cam.Cam.FollowDirection(20, 15f);
        }


        if (DropEffect.isPlaying == true)
        {
            DropEffect.Stop();
        }


    }



    void Update()
    {
        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 1);
        //CharacterAnimator.SetFloat("YSpeed", 1000);
        CharacterAnimator.SetFloat("GroundSpeed", 100);
        // CharacterAnimator.SetFloat("GroundSpeed", 0);
        // CharacterAnimator.SetBool("Grounded", true);
        // CharacterAnimator.SetFloat("NormalSpeed", 0);
        //  BallAnimator.SetFloat("SpinCharge", charge);
        //  BallAnimator.speed = charge * BallAnimationSpeedMultiplier;

        //Check if rolling
        //if (Player.Grounded && Player.isRolling) { CharacterAnimator.SetInteger("Action", 1); }
        //CharacterAnimator.SetBool("isRolling", Player.isRolling);

        //Rotation

        //Set Animation Angle
        if (!Player.Grounded)
        {
            Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
            Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
            CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * 200);
        }
        //GetComponent<CameraControl>().Cam.FollowDirection(2, 14f, -10,0);

        if (Player.Grounded && DropEffect.isPlaying)
        {
            DropEffect.Stop();
        }

        /*
        for (int i = 0; i < PlayerSkin.Length; i++)
        {
            PlayerSkin[i].enabled = false;
        }

        */
        //SpinDashBall.SetActive(true);
    }

    public void ResetSpinDashVariables()
    {
        if (DropEffect.isPlaying == true)
        {
            DropEffect.Stop();
        }
        for (int i = 0; i < PlayerSkin.Length; i++)
        {
            PlayerSkin[i].enabled = true;
        }
        //SpinDashBall.SetActive(false);
        charge = 0;
    }

    private void AssignStats()
    {
        SpinDashChargingSpeed = Tools.stats.DropDashChargingSpeed;
        MinimunCharge = Tools.stats.DropMinimunCharge;
        MaximunCharge = Tools.stats.DropMaximunCharge;
    }

    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();

        CharacterAnimator = Tools.CharacterAnimator;
        BallAnimator = Tools.BallAnimator;
        sounds = Tools.SoundControl;
        DropEffect = Tools.DropEffect;

        JumpBall = Tools.JumpBall;
        PlayerSkin = Tools.PlayerSkin;
        SpinDashBall = Tools.DropSpinBall;
        PlayerSkinTransform = Tools.PlayerSkinTransform;
        DirectionReference = Tools.DirectionReference;

    }
}
