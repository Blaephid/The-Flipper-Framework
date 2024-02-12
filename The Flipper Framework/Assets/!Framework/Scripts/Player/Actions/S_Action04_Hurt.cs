using UnityEngine;
using System.Collections;

public class S_Action04_Hurt : MonoBehaviour {

    Animator CharacterAnimator;
    S_PlayerPhysics Player;
    S_CharacterTools Tools;
    S_ActionManager Actions;
    S_Control_SoundsPlayer sounds;
    S_PlayerInput Inp;

    LayerMask _recoilFrom_;

    [HideInInspector] public float _knockbackUpwardsForce_ = 10;
    int counter;
    public float deadCounter { get; set; }

    [HideInInspector] public bool _resetSpeedOnHit_ = false;
    [HideInInspector] public float _knockbackForce_ = 10;

    float _bonkBackForce_;
    float _bonkUpForce_;

    float _recoilGround_ ;
    float _recoilAir_ ;

    float _bonkLock_ ;
    float _bonkLockAir_ ;

    float lockedForGround;
    float lockedForAir;

    void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<S_CharacterTools>();
            AssignTools();

            AssignStats();

        }
        
    }

    public void InitialEvents(bool bonk = false)
    {
        Tools.JumpBall.SetActive(false);
        CharacterAnimator.SetInteger("Action", 4);
        CharacterAnimator.SetTrigger("Damaged");

        //Change Velocity
        if (bonk)
        {
            Vector3 newSpeed = -CharacterAnimator.transform.forward * _bonkBackForce_;
            newSpeed.y = _bonkUpForce_;
            if (Player._isGrounded)
                newSpeed.y *= 2;
            Player._RB.velocity = newSpeed;

            lockedForAir = _bonkLockAir_;
            lockedForGround = _bonkLock_;
            sounds.PainVoicePlay();
        }
        else if (!_resetSpeedOnHit_ && !Physics.Raycast(transform.position, CharacterAnimator.transform.forward, 6, _recoilFrom_))
        {
            Vector3 newSpeed = new Vector3((Player._RB.velocity.x / 2), _knockbackUpwardsForce_, (Player._RB.velocity.z / 2));
            newSpeed.y = _knockbackUpwardsForce_;
            Player._RB.velocity = newSpeed;
            lockedForAir = _recoilAir_;
            lockedForGround = _recoilGround_;
        }
        else
        {
            Vector3 newSpeed = -CharacterAnimator.transform.forward * _knockbackForce_;
            newSpeed.y = _knockbackUpwardsForce_;
            Player._RB.velocity = newSpeed;
            lockedForAir = _recoilAir_ * 1.4f;
            lockedForGround = _recoilGround_ * 1.4f;
        }

        Inp.LockInputForAWhile(lockedForGround * 0.85f, false);

    }

    void FixedUpdate () {

        //Get out of Action
        counter += 1;

        if ((Player._isGrounded && counter > lockedForGround) || counter > lockedForAir)
        {
            if (!Actions.Action04Control.isDead)
            {

                Actions.Action02Control._isHomingAvailable = true;
                Actions.Action01.jumpCount = 0;

                CharacterAnimator.SetInteger("Action", 0);
                Actions.ChangeAction(S_Enums.PlayerStates.Regular);
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
            if (Player._isGrounded && deadCounter > 0.3f)
            {
                CharacterAnimator.SetBool("Dead", true);
            }
        }

    }

    private void AssignStats ()
    {
        _knockbackForce_ = Tools.Stats.WhenHurt.knockbackForce;
        _knockbackUpwardsForce_ = Tools.Stats.WhenHurt.knockbackUpwardsForce;
        _resetSpeedOnHit_ = Tools.Stats.WhenHurt.shouldResetSpeedOnHit;
        _recoilFrom_ = Tools.Stats.WhenHurt.recoilFrom;

        _bonkBackForce_ = Tools.Stats.WhenBonked.bonkBackwardsForce;
        _bonkUpForce_ = Tools.Stats.WhenBonked.bonkUpwardsForce;

        _recoilAir_ = Tools.Stats.WhenHurt.hurtControlLockAir;
        _recoilGround_ = Tools.Stats.WhenHurt.hurtControlLock;
        _bonkLock_ = Tools.Stats.WhenBonked.bonkControlLock;
        _bonkLockAir_ = Tools.Stats.WhenBonked.bonkControlLockAir;
    }

    private void AssignTools()
    {
        Player = GetComponent<S_PlayerPhysics>();
        Actions = GetComponent<S_ActionManager>();
        Inp = GetComponent<S_PlayerInput>();
        CharacterAnimator = Tools.CharacterAnimator;
        sounds = Tools.SoundControl;
    }
}
