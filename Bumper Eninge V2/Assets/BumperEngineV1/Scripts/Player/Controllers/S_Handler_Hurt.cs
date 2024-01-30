using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class S_Handler_Hurt : MonoBehaviour 
{
    S_CharacterTools Tools;

    S_ActionManager Actions;
    S_PlayerInput Inp;
    S_Manager_LevelProgress Level;
    S_PlayerPhysics Player;
    S_Interaction_Objects Objects;
    S_Handler_Camera Cam;
    

    S_Control_SoundsPlayer Sounds;
    GameObject JumpBall;
    Animator CharacterAnimator;
    Transform faceHitCollider;
    float faceHitSize;

    [HideInInspector] public int InvencibilityTime;
    int counter;
    public bool IsHurt { get; set; }
    public bool IsInvencible { get; set; }
    float flickerCount;
    float FlickerSpeed;

    LayerMask bonkWall;
    GameObject WallToBonk;
    Vector3 previDir;

    SkinnedMeshRenderer[] SonicSkins;

    GameObject MovingRing;
    GameObject releaseDirection;
    [HideInInspector] public int MaxRingLoss;
    [HideInInspector] public float RingReleaseSpeed;
    [HideInInspector] public float RingArcSpeed;
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
        counter = InvencibilityTime;
        releaseDirection = new GameObject();
        previDir = transform.forward;
        this.enabled = true;
    }



    void FixedUpdate () {

        counter += 1;
        if(counter < InvencibilityTime)
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

        if(Actions.killBindPressed)
        {
            if(Actions.Action != S_ActionManager.States.Hurt)
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

        if((Actions.Action == 0 && Player.HorizontalSpeedMagnitude > 50) || (Actions.Action == S_ActionManager.States.Jump && Player.HorizontalSpeedMagnitude > 40) || (Actions.Action == S_ActionManager.States.JumpDash
            && Player.HorizontalSpeedMagnitude > 30) || (Actions.Action == S_ActionManager.States.WallRunning && Actions.Action12.RunningSpeed > 5))
        {
            if(Physics.SphereCast(transform.position, 0.3f, CharacterAnimator.transform.forward, out RaycastHit tempHit, 10f, bonkWall))
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
        previousSpeed = Player.rb.velocity.sqrMagnitude;
    }

    void Death()
    {

        S_Interaction_Objects.RingAmount = 0;

        JumpBall.SetActive (false);


        Inp.enabled = false;
        Actions.ChangeAction(S_ActionManager.States.Hurt);
        Player.MoveInput = Vector3.zero;
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

            Inp.enabled = true;
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
        flickerCount += FlickerSpeed;
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
            movingRing.GetComponent<Rigidbody>().AddForce((releaseDirection.transform.forward * RingReleaseSpeed), ForceMode.Acceleration);
            releaseDirection.transform.Rotate(0, RingArcSpeed, 0);
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
            Cam.Cam.SetCamera(-99);
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
            if (!Physics.Raycast(transform.position + (CharacterAnimator.transform.up * 1.5f), previDir, 10f, bonkWall) && !Player.Grounded)
            {
                transform.position = transform.position + (CharacterAnimator.transform.up * 1.5f);
            }
            else if (!Physics.Raycast(transform.position + (-CharacterAnimator.transform.up * 1.5f), previDir, 10f, bonkWall) && !Player.Grounded)
            {
                transform.position = transform.position + (-CharacterAnimator.transform.up * 1.5f);
            }
            else if (previousSpeed / 1.6f > Player.rb.velocity.sqrMagnitude || !Player.Grounded)
            {
                StartCoroutine(giveChanceToWallClimb());
            }
            
            
        }
    }

    IEnumerator giveChanceToWallClimb()
    {
        Vector3 newDir = CharacterAnimator.transform.forward;
        if (Actions.Action != S_ActionManager.States.WallRunning)
        {
            if(!Player.Grounded)
            {
                for (int i = 0; i < 3; i++)
                {
                    yield return new WaitForFixedUpdate();
                    Player.rb.velocity = Vector3.zero;
                    CharacterAnimator.transform.forward = newDir;
                }
            }
            
            if (Actions.Action != S_ActionManager.States.WallRunning)
            {
                Actions.Action04.InitialEvents(true);
                Actions.ChangeAction(S_ActionManager.States.Hurt);
            }
        }
        else if(Actions.Action12.RunningSpeed > 0)
        {
            Actions.Action04.InitialEvents(true);
            Actions.ChangeAction(S_ActionManager.States.Hurt);
        }
    }

    private void AssignStats()
    {
        InvencibilityTime = Tools.stats.InvincibilityTime;
        MaxRingLoss = Tools.stats.MaxRingLoss;
        RingReleaseSpeed = Tools.coreStats.RingReleaseSpeed;
        RingArcSpeed = Tools.coreStats.RingArcSpeed;
        FlickerSpeed = Tools.coreStats.FlickerSpeed;
        bonkWall = Tools.coreStats.BonkOnWalls;
    }

    private void AssignTools()
    {
        Player = GetComponent<S_PlayerPhysics>();
        Level = GetComponent<S_Manager_LevelProgress>();
        Actions = GetComponent<S_ActionManager>();
        Objects = GetComponent<S_Interaction_Objects>();
        Cam = GetComponent<S_Handler_Camera>();
        Inp = GetComponent<S_PlayerInput>();

        faceHitCollider = Tools.faceHit.transform;
        JumpBall = Tools.JumpBall;
        Sounds = Tools.SoundControl;
        CharacterAnimator = Tools.CharacterAnimator;
        SonicSkins = Tools.PlayerSkin;
        MovingRing = Tools.movingRing;
        FadeOutImage = Tools.FadeOutImage;
    }
}
