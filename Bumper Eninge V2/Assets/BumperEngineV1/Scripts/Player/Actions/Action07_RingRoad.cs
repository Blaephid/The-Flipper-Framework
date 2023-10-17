using UnityEngine;
using System.Collections;

public class Action07_LightDash : MonoBehaviour
{

    [SerializeField] Animator CharacterAnimator;
    Quaternion CharRot;
    public ActionManager Action;
    public GameObject HomingTrailContainer;
    public GameObject HomingTrail;
    PlayerBhysics Player;
    public float skinRotationSpeed = 1;
    public Transform Target { get; set; }
    private float InitialVelocityMagnitude;
    float Timer;
    float Speed;
    float Aspeed;

    //[SerializeField] float DashingTimerLimit;
    [SerializeField] float DashSpeed;
    [SerializeField] float EndingSpeedFactor;
    [SerializeField] float MinimumEndingSpeed;
    Vector3 direction;

    void Awake()
    {
        Player = GetComponent<PlayerBhysics>();
    }

    public void InitialEvents()
    {
        InitialVelocityMagnitude = Player.p_rigidbody.velocity.magnitude;
        Player.p_rigidbody.velocity = Vector3.zero;

        Action.Action01.JumpBall.SetActive(false);
        if (HomingTrailContainer.transform.childCount < 1)
        {
            GameObject HomingTrailClone = Instantiate(HomingTrail, HomingTrailContainer.transform.position, Quaternion.identity) as GameObject;
            HomingTrailClone.transform.parent = HomingTrailContainer.transform;
        }

        if (Action.Action07Control.HasTarget && Target != null)
        {
            Target = LightDashControl.TargetObject.transform;
        }

    }

    void Update()
    {

        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 7);
        CharacterAnimator.SetFloat("YSpeed", Player.p_rigidbody.velocity.y);
        CharacterAnimator.SetFloat("GroundSpeed", Player.p_rigidbody.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);

        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, Player.p_rigidbody.velocity.y, Player.p_rigidbody.velocity.z);
        Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);

    }

    void FixedUpdate()
    {
        //Timer += 1;

        //CharacterAnimator.SetInteger("Action", 1);
        if (Action.Action07Control.HasTarget)
        {
            Target = LightDashControl.TargetObject.transform;
            direction = Target.position - transform.position;
            Player.p_rigidbody.velocity = direction.normalized * DashSpeed;

            GetComponent<CameraControl>().Cam.FollowDirection(2, 14f, -10, 0);
        }
        //End homing attck if on air for too long
        if (!Action.Action07Control.HasTarget)
        {
            float EndingSpeedResult = 0;
            ////Debug.Log (InitialVelocityMagnitude);
            EndingSpeedResult = Mathf.Max(MinimumEndingSpeed, InitialVelocityMagnitude);
            ////Debug.Log (InitialVelocityMagnitude);
            ////Debug.Log (EndingSpeedResult);
            Player.p_rigidbody.velocity = Vector3.zero;
            Player.p_rigidbody.velocity = direction.normalized * EndingSpeedResult * EndingSpeedFactor;

            GetComponent<CameraControl>().Cam.SetCamera(direction.normalized, 2.5f, 20, 5f, 10);

            for (int i = HomingTrailContainer.transform.childCount - 1; i >= 0; i--)
                Destroy(HomingTrailContainer.transform.GetChild(i).gameObject);

            GetComponent<PlayerBinput>().LockInputForAWhile(10, true);

            CharacterAnimator.SetInteger("Action", 0);
            Action.ChangeAction(0);
        }
    }

}
