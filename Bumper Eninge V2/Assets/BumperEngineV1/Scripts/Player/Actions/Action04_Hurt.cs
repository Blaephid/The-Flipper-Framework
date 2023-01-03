using UnityEngine;
using System.Collections;

public class Action04_Hurt : MonoBehaviour {

    Animator CharacterAnimator;
    PlayerBhysics Player;
    CharacterTools Tools;
    ActionManager Actions;
    SonicSoundsControl sounds;
    PlayerBinput Inp;

    LayerMask RecoilFrom;

    [HideInInspector] public float KnockbackUpwardsForce = 10;
    int counter;
    public float deadCounter { get; set; }

    [HideInInspector] public bool ResetSpeedOnHit = false;
    [HideInInspector] public float KnockbackForce = 10;

    float BonkBackForce;
    float BonkUpForce;

    float RecoilGround ;
    float RecoilAir ;

    float BonkLock ;
    float BonkLockAir ;

    float lockedForGround;
    float lockedForAir;

    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<CharacterTools>();
            AssignTools();

            AssignStats();

        }
        
    }

    public void InitialEvents(bool bonk = false)
    {
        Tools.JumpBall.SetActive(false);

        //Change Velocity
        if(bonk)
        {
            Vector3 newSpeed = -CharacterAnimator.transform.forward * BonkBackForce;
            newSpeed.y = BonkUpForce;
            if (Player.Grounded)
                newSpeed.y *= 2;
            Player.rb.velocity = newSpeed;

            lockedForAir = BonkLockAir;
            lockedForGround = BonkLock;
            sounds.PainVoicePlay();
        }
        else if (!ResetSpeedOnHit && !Physics.Raycast(transform.position, CharacterAnimator.transform.forward, 6, RecoilFrom))
        {
            Vector3 newSpeed = new Vector3((Player.rb.velocity.x / 2), KnockbackUpwardsForce, (Player.rb.velocity.z / 2));
            newSpeed.y = KnockbackUpwardsForce;
            Player.rb.velocity = newSpeed;
            lockedForAir = RecoilAir;
            lockedForGround = RecoilGround;
        }
        else
        {
            Vector3 newSpeed = -CharacterAnimator.transform.forward * KnockbackForce;
            newSpeed.y = KnockbackUpwardsForce;
            Player.rb.velocity = newSpeed;
            lockedForAir = RecoilAir * 1.4f;
            lockedForGround = RecoilGround * 1.4f;
        }

        Inp.LockInputForAWhile(lockedForGround * 0.85f, false);

    }

    void FixedUpdate () {

        //Get out of Action
        counter += 1;

        if ((Player.Grounded && counter > lockedForGround) || counter > lockedForAir)
        {
            if (!Actions.Action04Control.isDead)
            {

                Actions.Action02Control.HomingAvailable = true;
                Actions.Action01.jumpCount = 0;

                CharacterAnimator.SetInteger("Action", 0);
                Actions.ChangeAction(0);
                //Debug.Log("What");
                counter = 0;
            }
        }

    }

    void Update()
    {
        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 4);

        //Dead
        if (Actions.Action04Control.isDead)
        {
            deadCounter += Time.deltaTime;
            if (Player.Grounded && deadCounter > 0.3f)
            {
                CharacterAnimator.SetBool("Dead", true);
            }
        }

    }

    private void AssignStats ()
    {
        KnockbackForce = Tools.coreStats.KnockbackForce;
        KnockbackUpwardsForce = Tools.coreStats.KnockbackUpwardsForce;
        ResetSpeedOnHit = Tools.coreStats.ResetSpeedOnHit;
        RecoilFrom = Tools.coreStats.RecoilFrom;

        BonkBackForce = Tools.coreStats.BonkBackwardsForce;
        BonkUpForce = Tools.coreStats.BonkUpwardsForce;

        RecoilAir = Tools.coreStats.HurtControlLockAir;
        RecoilGround = Tools.coreStats.HurtControlLock;
        BonkLock = Tools.coreStats.BonkControlLock;
        BonkLockAir = Tools.coreStats.BonkControlLockAir;
    }

    private void AssignTools()
    {
        Player = GetComponent<PlayerBhysics>();
        Actions = GetComponent<ActionManager>();
        Inp = GetComponent<PlayerBinput>();
        CharacterAnimator = Tools.CharacterAnimator;
        sounds = Tools.SoundControl;
    }
}
