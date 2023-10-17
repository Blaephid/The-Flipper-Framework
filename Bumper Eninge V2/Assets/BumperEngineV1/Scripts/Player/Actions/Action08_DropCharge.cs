using UnityEngine;
using System.Collections;

public class Action08_DropDash : MonoBehaviour
{

    public Animator CharacterAnimator;
    public Animator BallAnimator;
    CameraControl Cam;
    public float BallAnimationSpeedMultiplier;

    ActionManager Actions;
    PlayerBhysics Player;
    public SonicSoundsControl sounds;
    public ParticleSystem DropEffect;
    public float SpinDashChargedEffectAmm;

    public bool DropDashAvailable { get; set; }

    public SkinnedMeshRenderer[] PlayerSkin;
    public GameObject SpinDashBall;
    public Transform PlayerSkinTransform;
    public Transform DirectionReference;

    public float SpinDashChargingSpeed = 0.3f;
    public float MinimunCharge = 10;
    public float MaximunCharge = 100;
    // public float SpinDashStillForce = 1.05f;
    float charge;
    bool isSpinDashing;
    Vector3 RawPrevInput;
    Quaternion CharRot;

    public float ReleaseShakeAmmount;

    void Start()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Cam = GetComponent<CameraControl>();
    }

    public void InitialEvents()
    {
        ////Debug.Log ("startDropDash");

        sounds.SpinDashSound();
        charge = 0;
    }

    void FixedUpdate()
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

        if (!Input.GetButton("A"))
        {
            if (DropEffect.isPlaying == true)
            {
                DropEffect.Stop();
            }
            Actions.ChangeAction(0);
        }

        if (charge > MaximunCharge)
        {
            charge = MaximunCharge;
        }

        //Stop if not grounded
        if (Player.Grounded) { Release(); }
    }

    void Release()
    {
        DropDashAvailable = false;

        Vector3 newForward = Player.p_rigidbody.velocity - transform.up * Vector3.Dot(Player.p_rigidbody.velocity, transform.up);

        if (newForward.magnitude < 0.1f)
        {
            newForward = CharacterAnimator.transform.forward;
        }

        CharRot = Quaternion.LookRotation(newForward, transform.up);
        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * 200);

        HedgeCamera.Shakeforce = (ReleaseShakeAmmount * charge) / 100;
        if (charge < MinimunCharge)
        {
            sounds.Source2.Stop();
            Actions.ChangeAction(0);
        }
        else
        {
            Player.isRolling = true;
            sounds.SpinDashReleaseSound();
            ////Debug.Log (charge);
            Player.p_rigidbody.velocity = charge * (CharacterAnimator.transform.forward);

            if (DropEffect.isPlaying == true)
            {
                DropEffect.Stop();
            }

            Actions.ChangeAction(0);
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
            Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z);
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
}
