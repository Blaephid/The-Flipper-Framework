using UnityEngine;
using System.Collections;

public class Action08_DropCharge : MonoBehaviour
{
    CharacterTools Tools;

    Animator CharacterAnimator;
   
    Transform feetPoint;
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
 

    [HideInInspector] public float SpinDashChargingSpeed = 0.3f;
    [HideInInspector] public float MinimunCharge = 10;
    [HideInInspector] public float MaximunCharge = 100;
    bool Charging = true;

    [HideInInspector] public float charge;
    bool isSpinDashing;
    Vector3 RawPrevInput;
    Quaternion CharRot;
    RaycastHit floorHit;
    Vector3 newForward;

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


    public void TryDropCharge()
    {
        if (Player.rb.velocity.y < 40f && Actions.Action08 != null)
        {
            //Debug.Log("Enter DropDash");
            Actions.ChangeAction(ActionManager.States.DropCharge);

            Actions.Action08.InitialEvents();
        }
    }

    public void InitialEvents(float newCharge = 15)
    {
        ////Debug.Log ("startDropDash");
        CharacterAnimator.SetInteger("Action", 1);
        CharacterAnimator.SetBool("Grounded", false);

        sounds.SpinDashSound();
        charge =  newCharge;
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
                //if (DropEffect.isPlaying == true)
                //{
                //    DropEffect.Stop();
                //}

                sounds.Source2.Stop();


                Charging = false;
                StartCoroutine(exitAction());
            }

            if (charge > MaximunCharge)
            {
                charge = MaximunCharge;
            }
        }
        else
        {
            if (Actions.RollPressed)
            {
                Charging = true;
                StopCoroutine(exitAction());
            }
        }

        if (Physics.Raycast(feetPoint.position, -transform.up, out floorHit, 1.3f, Player.Playermask))
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
            JumpBall.SetActive(false);
            Actions.ChangeAction(ActionManager.States.Regular);
        }

        else if (Actions.SpecialPressed && charge > MinimunCharge)
        {
            AirRelease();
        }
    }

    IEnumerator exitAction()
    {
        yield return new WaitForSeconds(0.7f);
        if (DropEffect.isPlaying == true)
        {
            DropEffect.Stop();
        }
        if (Actions.Action == ActionManager.States.DropCharge)
        {
            JumpBall.SetActive(true);
            Actions.ChangeAction(ActionManager.States.Jump);
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
        if (GetComponent<Action11_JumpDash>() != null)
            GetComponent<Action11_JumpDash>().AirDashParticle();
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
        JumpBall.SetActive(false);
        Actions.ChangeAction(ActionManager.States.Jump);

    }

    void Release()
    {

        if (Actions.eventMan != null) Actions.eventMan.dropChargesPerformed += 1;

        JumpBall.SetActive(false);

        DropDashAvailable = false;

        //Vector3 newForward = Player.rb.velocity - transform.up * Vector3.Dot(Player.rb.velocity, transform.up);

        //if (newForward.magnitude < 0.1f)
        //{
        //    newForward = CharacterAnimator.transform.forward;
        //}

        //CharRot = Quaternion.LookRotation(newForward, transform.up);
        //CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * 200);

        newForward = Vector3.ProjectOnPlane(CharacterAnimator.transform.forward, floorHit.normal);

        if (charge < MinimunCharge)
        {
            charge = MinimunCharge;
        }
    

        if (DropEffect.isPlaying == true)
        {
            DropEffect.Stop();
        }

        StartCoroutine(delayForce(charge, 1));

    }


    void Launch(float charge)
    {
        HedgeCamera.Shakeforce = (ReleaseShakeAmmount * charge) / 100;
        sounds.SpinDashReleaseSound();

        Player.alignWithGround();

        Vector3 newVec = charge *  newForward;

        Actions.Action00.Curl();
        Player.isRolling = true;
        Actions.Action00.rollCounter = 0.005f;


        Vector3 releVec = Player.getRelevantVec(newVec);
        float newSpeedMagnitude = new Vector3(releVec.x, 0f, releVec.z).magnitude;

        Debug.DrawRay(transform.position, newVec.normalized * 30, Color.red * 2, 20f);

        if (newSpeedMagnitude > Player.HorizontalSpeedMagnitude)
        {
            Player.rb.velocity = newVec;

            Cam.Cam.FollowHeightDirection(18, 25f);
        }
        else
        {
            Player.rb.velocity = newVec.normalized * (Player.HorizontalSpeedMagnitude + (charge* 0.45f));
            Cam.Cam.FollowHeightDirection(20, 15f);
        }
    }

    IEnumerator delayForce(float charge, int delay)
    {
        for (int i = 1; i <= delay; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        Launch(charge);
    }

    public float externalDash()
    {
        HedgeCamera.Shakeforce = (ReleaseShakeAmmount * charge) / 100;
        sounds.SpinDashReleaseSound();
        return charge;
    }

    void Update()
    {
        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 1);
        CharacterAnimator.SetFloat("GroundSpeed", 100);

        //Check if rolling
        //if (Player.Grounded && Player.isRolling) { CharacterAnimator.SetInteger("Action", 1); }
        //CharacterAnimator.SetBool("isRolling", Player.isRolling);

        //Rotation

        //Set Animation Angle
        //if (!Player.Grounded)
        {
            Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
            if(VelocityMod != Vector3.zero)
            {
                Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
                CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * 200);
            }
        }

        if (Player.Grounded && DropEffect.isPlaying)
        {
            DropEffect.Stop();
        }

    }

    private void OnDisable()
    {
        DropEffect.Stop();
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
        sounds = Tools.SoundControl;
        DropEffect = Tools.DropEffect;

        feetPoint = Tools.FeetPoint;
        JumpBall = Tools.JumpBall;
        PlayerSkin = Tools.PlayerSkin;


    }
}
