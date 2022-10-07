using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HurtControl : MonoBehaviour 
{
    CharacterTools Tools;
    CharacterStats Stats;

    ActionManager Actions;
    PlayerBinput Inp;
    LevelProgressControl Level;
    PlayerBhysics Player;
    Objects_Interaction Objects;
    CameraControl Cam;
    

    SonicSoundsControl Sounds;
    GameObject JumpBall;
    Animator CharacterAnimator;

    [HideInInspector] public int InvencibilityTime;
    int counter;
    public bool IsHurt { get; set; }
    public bool IsInvencible { get; set; }
    float flickerCount;
    float FlickerSpeed;

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

	void Awake () 
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            Stats = GetComponent<CharacterStats>();
            AssignStats();
        }
        InitialDir = transform.forward;
        counter = InvencibilityTime;
        releaseDirection = new GameObject();
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
	}

    void Death()
    {
		JumpBall.SetActive (false);


        Inp.enabled = false;
        Actions.ChangeAction(4);
        Player.MoveInput = Vector3.zero;
        deadCounter += 1;
        //Debug.Log("DeathGroup");

        if (deadCounter > 60)
        {
            Color alpha = Color.black;
            alpha.a = 1;
            FadeOutImage.color = Color.Lerp(FadeOutImage.color, alpha, 0.5f);
        }
        if(deadCounter  > 150)
        {
            if (Level.CurrentCheckPoint)
            {
                Cam.Cam.SetCamera(Level.CurrentCheckPoint.transform.forward, 2,10,10);
            }
            else
            {
                Cam.Cam.SetCamera(InitialDir, 5);
            }
            Inp.enabled = true;
            Level.ResetToCheckPoint();
            //Debug.Log("CallingReset");
            isDead = false;
            CharacterAnimator.SetBool("Dead", false);
            deadCounter = 0;
            counter = 0;
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
        Objects_Interaction.RingAmount = 0;

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

        if(Objects_Interaction.RingAmount > 0 && !releasingRings)
        {
            RingsToRelease = Objects_Interaction.RingAmount;
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
    }

    private void AssignStats()
    {
        InvencibilityTime = Stats.InvincibilityTime;
        MaxRingLoss = Stats.MaxRingLoss;
        RingReleaseSpeed = Stats.RingReleaseSpeed;
        RingArcSpeed = Stats.RingArcSpeed;
        FlickerSpeed = Stats.FlickerSpeed;
    }

    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Level = GetComponent<LevelProgressControl>();
        Actions = GetComponent<ActionManager>();
        Objects = GetComponent<Objects_Interaction>();
        Cam = GetComponent<CameraControl>();
        Inp = GetComponent<PlayerBinput>();

        JumpBall = Tools.JumpBall;
        Sounds = Tools.SoundControl;
        CharacterAnimator = Tools.CharacterAnimator;
        SonicSkins = Tools.PlayerSkin;
        MovingRing = Tools.movingRing;
        FadeOutImage = Tools.FadeOutImage;
    }
}
