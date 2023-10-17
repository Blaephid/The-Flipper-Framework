using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Action06_Bounce : MonoBehaviour
{

    public ActionManager Action;
    public Animator CharacterAnimator;
    PlayerBhysics Player;
    [SerializeField] SonicSoundsControl sounds;

    public bool BounceAvailable;
    private bool HasBounced;
    private float OriginalBounceFactor;
    private float CurrentBounceAmount;
    public int BounceCount;

    //[SerializeField] public bool ShouldStomp;

    [SerializeField] float DropSpeed;
    [SerializeField] private List<float> BounceUpSpeeds;
    [SerializeField] float BounceUpMaxSpeed;
    [SerializeField] float BounceConsecutiveFactor;
    [SerializeField] float BounceHaltFactor;

    [SerializeField] GameObject HomingTrailContainer;
    [SerializeField] GameObject HomingTrail;

    Vector3 direction;
    RaycastHit hit;

    void Awake()
    {
        Player = GetComponent<PlayerBhysics>();
        OriginalBounceFactor = BounceConsecutiveFactor;
    }

    public void InitialEvents()
    {

        ////Debug.Log ("BounceDrop");
        sounds.BounceStartSound();
        BounceAvailable = false;
        Player.p_rigidbody.velocity = new Vector3(Player.p_rigidbody.velocity.x / BounceHaltFactor, 0, Player.p_rigidbody.velocity.z / BounceHaltFactor);
        Player.AddVelocity(new Vector3(0, -DropSpeed, 0));

        if (HomingTrailContainer.transform.childCount < 1)
        {
            GameObject HomingTrailClone = Instantiate(HomingTrail, HomingTrailContainer.transform.position, Quaternion.identity) as GameObject;
            HomingTrailClone.transform.parent = HomingTrailContainer.transform;
        }

        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 1);
        CharacterAnimator.SetBool("isRolling", false);
        Action.Action01.JumpBall.SetActive(true);
    }


    private void ResetValues()
    {

        BounceCount = 0;
    }

    private void Bounce(Vector3 normal)
    {
        HasBounced = true;
        CurrentBounceAmount = BounceUpSpeeds[BounceCount];

        CurrentBounceAmount = Mathf.Clamp(CurrentBounceAmount, BounceUpSpeeds[BounceCount], BounceUpMaxSpeed);
        ////Debug.Log (CurrentBounceAmount);
        Player.p_rigidbody.velocity = new Vector3(Player.p_rigidbody.velocity.x, CurrentBounceAmount, Player.p_rigidbody.velocity.z);
        Player.AddVelocity(Player.GroundNormal);

        sounds.BounceImpactSound();

        HomingTrailContainer.transform.DetachChildren();

        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 1);
        CharacterAnimator.SetBool("isRolling", false);
        Action.Action01.JumpBall.SetActive(false);
        if (BounceCount < BounceUpSpeeds.Count - 1)
        {
            BounceCount++;
        }

    }

    private void Stomp()
    {
        Player.p_rigidbody.velocity = new Vector3(0, 0, 0);
        ResetValues();
        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 6);
        CharacterAnimator.SetBool("isRolling", false);
        Action.Action01.JumpBall.SetActive(false);

        sounds.StompImpactSound();

        StartCoroutine(AfterStomp());
    }

    private IEnumerator AfterStomp()
    {
        yield return new WaitForSeconds(2);
        //	ShouldStomp = false;
        Action.ChangeAction(0);

    }

    void Update()
    {
        bool raycasthit = Physics.Raycast(transform.position, Vector3.down, out hit, (Player.SpeedMagnitude * Time.deltaTime * 1.1f) + Player.negativeGHoverHeight, Player.Playermask);
        bool groundhit = Player.Grounded || raycasthit;

        //End Action
        if (!groundhit && HasBounced)
        {

            HasBounced = false;

            ////Debug.Log ("BackToIdleJump");

            Action.ChangeAction(0);
        }
        if (groundhit && !HasBounced)
        {
            if (!raycasthit)
            {
                Debug.Log("Ground Bounce " + Player.GroundNormal);
                Bounce(Player.GroundNormal);
            }
            else
            {
                Debug.Log("RaycastHitBounce " + hit.normal);
                transform.position = hit.point;
                Bounce(hit.normal);
            }
        }

    }

}
