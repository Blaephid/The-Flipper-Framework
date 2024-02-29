using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class S_Handler_Hurt : MonoBehaviour 
{
    S_CharacterTools Tools;

    S_ActionManager Actions;
    S_PlayerInput _Input;
    S_Manager_LevelProgress Level;
    S_PlayerPhysics Player;
    S_Interaction_Objects Objects;
    S_Handler_Camera Cam;
    

    S_Control_PlayerSound Sounds;
    GameObject JumpBall;
    Animator CharacterAnimator;
    Transform faceHitCollider;
    float faceHitSize;

    [HideInInspector] public int _invincibilityTime_;
    int counter;
    public bool IsHurt { get; set; }
    public bool IsInvencible { get; set; }
    float flickerCount;
    float _flickerSpeed_;

    LayerMask _BonkWall_;
    GameObject WallToBonk;
    Vector3 previDir;

    SkinnedMeshRenderer[] SonicSkins;

    GameObject MovingRing;
    GameObject releaseDirection;
    [HideInInspector] public int _maxRingLoss_;
    [HideInInspector] public float _ringReleaseSpeed_;
    [HideInInspector] public float _ringArcSpeed_;
    bool releasingRings = false;
    int RingsToRelease;

    public bool isDead { get; set; }
    int deadCounter = 0;

    [HideInInspector] public Image FadeOutImage;
    Vector3 InitialDir;
    float previousSpeed;

	void Awake () 
    {
        if (Player == null)
        {
            Tools = GetComponent<S_CharacterTools>();
            AssignTools();

            AssignStats();
        }
        InitialDir = transform.forward;
        counter = _invincibilityTime_;
        releaseDirection = new GameObject();
        previDir = transform.forward;
        this.enabled = true;
    }



    void FixedUpdate () {

        counter += 1;
        if(counter < _invincibilityTime_)
        {
            IsInvencible = true;
            SkinFlicker();
        }
        else
        {
            IsInvencible = false;
            IsHurt = false;
            ToggleSkin(true);
        }

        if (releasingRings)
        {
            if(RingsToRelease > 30) { RingsToRelease = 30; }
            RingLoss();
        }

        if(_Input.killBindPressed)
        {
            if(Actions.whatAction != S_Enums.PrimaryPlayerStates.Hurt)
                CharacterAnimator.SetTrigger("Damaged");
            isDead = true;
        }

        //IsDead things
        if(isDead == true)
        {
            Death();
        }
        else if(counter > 30)
        {
            Color alpha = Color.black;
            alpha.a = 0;
            FadeOutImage.color = Color.Lerp(FadeOutImage.color, alpha, 0.5f);
        }

        Bonk();

    }

    void Bonk()
    {
        faceHitCollider.transform.rotation = Quaternion.LookRotation(CharacterAnimator.transform.forward, transform.up); ;

        if((Actions.whatAction == 0 && Player._horizontalSpeedMagnitude > 50) || (Actions.whatAction == S_Enums.PrimaryPlayerStates.Jump && Player._horizontalSpeedMagnitude > 40) || (Actions.whatAction == S_Enums.PrimaryPlayerStates.JumpDash
            && Player._horizontalSpeedMagnitude > 30) || (Actions.whatAction == S_Enums.PrimaryPlayerStates.WallRunning && Actions.Action12._runningSpeed > 5))
        {
            if(Physics.SphereCast(transform.position, 0.3f, CharacterAnimator.transform.forward, out RaycastHit tempHit, 10f, _BonkWall_))
            {
                
                if (Vector3.Dot(CharacterAnimator.transform.forward, tempHit.normal) < -0.7f)
                {
                    WallToBonk = tempHit.collider.gameObject;
                    previDir = CharacterAnimator.transform.forward;
                    return;
                }
            }
        }
        WallToBonk = null;
        previousSpeed = Player._RB.velocity.sqrMagnitude;
    }

    void Death()
    {

        S_Interaction_Objects.RingAmount = 0;

        JumpBall.SetActive (false);


        _Input.enabled = false;
        Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Hurt);
		_Input._move = Vector3.zero;
        deadCounter += 1;
        //Debug.Log("DeathGroup");

        if (deadCounter > 70)
        {
            Color alpha = Color.black;
            alpha.a = 1;
            FadeOutImage.color = Color.Lerp(FadeOutImage.color, alpha, 0.51f);
        }
        if(deadCounter == 120)
        {
            Level.RespawnObjects();
        }
        else if(deadCounter  == 170)
        {
            CharacterAnimator.SetBool("Dead", false);

            if (Level.CurrentCheckPoint)
            {
                //Cam.Cam.SetCamera(Level.CurrentCheckPoint.transform.forward, 2,10,10);
            }
            else
            {
                //Cam.Cam.SetCamera(InitialDir, 5);
            }

            _Input.enabled = true;
            Level.ResetToCheckPoint();
            //Debug.Log("CallingReset");
            isDead = false;
            deadCounter = 0;
            counter = 0;

            if (Actions.eventMan != null) Actions.eventMan.Death();
        }
    }

    void SkinFlicker()
    {
        flickerCount += _flickerSpeed_;
        if(flickerCount < 0)
        {
            ToggleSkin(false);
        }
        else
        {
            ToggleSkin(true);
        }
        if(flickerCount > 10)
        {
            flickerCount = -10;
        }
    }

    void RingLoss()
    {
        S_Interaction_Objects.RingAmount = 0;

        if(RingsToRelease > 0)
        {
            Vector3 pos = transform.position;
            pos.y += 1;
            GameObject movingRing;
            movingRing = Instantiate(MovingRing, pos, Quaternion.identity);
            movingRing.transform.parent = null;
            movingRing.GetComponent<Rigidbody>().velocity = Vector3.zero;
            movingRing.GetComponent<Rigidbody>().AddForce((releaseDirection.transform.forward * _ringReleaseSpeed_), ForceMode.Acceleration);
            releaseDirection.transform.Rotate(0, _ringArcSpeed_, 0);
            RingsToRelease -= 1;

	//		Player.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        }
        else
        {
            releasingRings = false;
		//	Player.GetComponent<Rigidbody> ().freezeRotation = false;
        }
    }

    public void ToggleSkin(bool on)
    {
        for (int i = 0; i < SonicSkins.Length; i++)
        {
            SonicSkins[i].enabled = on;
        }
    }

    public void GetHurt()
    {
        IsHurt = true;
        counter = 0;

        if(S_Interaction_Objects.RingAmount > 0 && !releasingRings)
        {
            RingsToRelease = S_Interaction_Objects.RingAmount;
            releasingRings = true;
        }
    }

    public void OnTriggerStay(Collider col)
    {
        if (col.tag == "Pit")
        {
            Cam._HedgeCam.SetCameraNoLook(100);
        }
    }
    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Pit")
        {
            Sounds.DieSound();
            isDead = true;
        }

        //Debug.Log(Player.HorizontalSpeedMagnitude);
        //Debug.Log(WallToBonk);
        if (col.gameObject == WallToBonk)
        {
           
            //Debug.Log("Attempt Bonk");
            if (!Physics.Raycast(transform.position + (CharacterAnimator.transform.up * 1.5f), previDir, 10f, _BonkWall_) && !Player._isGrounded)
            {
                transform.position = transform.position + (CharacterAnimator.transform.up * 1.5f);
            }
            else if (!Physics.Raycast(transform.position + (-CharacterAnimator.transform.up * 1.5f), previDir, 10f, _BonkWall_) && !Player._isGrounded)
            {
                transform.position = transform.position + (-CharacterAnimator.transform.up * 1.5f);
            }
            else if (previousSpeed / 1.6f > Player._RB.velocity.sqrMagnitude || !Player._isGrounded)
            {
                StartCoroutine(giveChanceToWallClimb());
            }
            
            
        }
    }

    IEnumerator giveChanceToWallClimb()
    {
        Vector3 newDir = CharacterAnimator.transform.forward;
        if (Actions.whatAction != S_Enums.PrimaryPlayerStates.WallRunning)
        {
            if(!Player._isGrounded)
            {
                for (int i = 0; i < 3; i++)
                {
                    yield return new WaitForFixedUpdate();
                    Player.SetTotalVelocity(Vector3.zero);
                    CharacterAnimator.transform.forward = newDir;
                }
            }
            
            if (Actions.whatAction != S_Enums.PrimaryPlayerStates.WallRunning)
            {
                Actions.Action04.InitialEvents(true);
                Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Hurt);
            }
        }
        else if(Actions.Action12._runningSpeed > 0)
        {
            Actions.Action04.InitialEvents(true);
            Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Hurt);
        }
    }

    private void AssignStats()
    {
        _invincibilityTime_ = Tools.Stats.WhenHurt.invincibilityTime;
        _maxRingLoss_ = Tools.Stats.WhenHurt.maxRingLoss;
        _ringReleaseSpeed_ = Tools.Stats.WhenHurt.ringReleaseSpeed;
        _ringArcSpeed_ = Tools.Stats.WhenHurt.ringArcSpeed;
        _flickerSpeed_ = Tools.Stats.WhenHurt.flickerSpeed;
        _BonkWall_ = Tools.Stats.WhenBonked.BonkOnWalls;
    }

    private void AssignTools()
    {
        Player = GetComponent<S_PlayerPhysics>();
        Level = GetComponent<S_Manager_LevelProgress>();
        Actions = GetComponent<S_ActionManager>();
        Objects = GetComponent<S_Interaction_Objects>();
        Cam = GetComponent<S_Handler_Camera>();
        _Input = GetComponent<S_PlayerInput>();

        faceHitCollider = Tools.faceHit.transform;
        JumpBall = Tools.JumpBall;
        Sounds = Tools.SoundControl;
        CharacterAnimator = Tools.CharacterAnimator;
        SonicSkins = Tools.PlayerSkin;
        MovingRing = Tools.movingRing;
        FadeOutImage = Tools.FadeOutImage;
    }
}
