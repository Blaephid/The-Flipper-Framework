using UnityEngine;
using System.Collections;

public class S_Action07_RingRoad : MonoBehaviour {

	S_CharacterTools Tools;


	Animator CharacterAnimator;
    Quaternion CharRot;
	S_ActionManager Action;
	GameObject HomingTrailContainer;
	public GameObject HomingTrail;
	public Vector3 trailOffSet = new Vector3(0,-3,0);
	S_PlayerPhysics Player;
	S_PlayerInput Inp;


	GameObject JumpBall;
	
    public float skinRotationSpeed = 1;
	public Transform Target { get; set; }
	private float InitialVelocityMagnitude;
	float Timer;
	float Speed;
	float Aspeed;

	//[SerializeField] float DashingTimerLimit;
	float _dashSpeed_;
	float _endingSpeedFactor_;
	float _minimumEndingSpeed_;
	Vector3 direction;

	void Awake()
	{
		if (Player == null)
        {
			Tools = GetComponent<S_CharacterTools>();
			AssignTools();

			AssignStats();
        }
	}


    public void InitialEvents()
	{
		InitialVelocityMagnitude = Player._RB.velocity.magnitude;
		Player._RB.velocity = Vector3.zero;

		JumpBall.SetActive(false);
		if (HomingTrailContainer.transform.childCount < 1) 
		{
			GameObject HomingTrailClone = Instantiate (HomingTrail, HomingTrailContainer.transform.position, Quaternion.identity) as GameObject;
			HomingTrailClone.transform.parent = HomingTrailContainer.transform;
			HomingTrailClone.transform.localPosition = trailOffSet;
		}
			
		if (Action.Action07Control.HasTarget && Target != null)
		{
			Target = Action.Action07Control.TargetObject.transform;
		}
			
	}

	void Update()
	{

		//Set Animator Parameters
		CharacterAnimator.SetInteger("Action", 7);
		CharacterAnimator.SetFloat("YSpeed", Player._RB.velocity.y);
		CharacterAnimator.SetFloat("GroundSpeed", Player._RB.velocity.magnitude);
		CharacterAnimator.SetBool("Grounded", Player._isGrounded);

		//Set Animation Angle
		Vector3 VelocityMod = new Vector3(Player._RB.velocity.x, Player._RB.velocity.y, Player._RB.velocity.z);
		if (VelocityMod != Vector3.zero)
		{
			Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
			CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
		}
	

	}

    void FixedUpdate()
    {
		
		//Timer += 1;

		Inp.LockInputForAWhile(1f, true);

		//CharacterAnimator.SetInteger("Action", 1);
		if (Action.Action07Control.TargetObject  != null) 
		{
			Target = Action.Action07Control.TargetObject.transform;
			direction = Target.position - transform.position;
			Player._RB.velocity = direction.normalized * _dashSpeed_;

			GetComponent<S_Handler_Camera>().Cam.FollowDirection(4, 14f, -10,0);
		}

		else
		{
			float EndingSpeedResult = 0;

			EndingSpeedResult = Mathf.Max (_minimumEndingSpeed_, InitialVelocityMagnitude);

			Player._RB.velocity = Vector3.zero;
			Player._RB.velocity = direction.normalized*EndingSpeedResult*_endingSpeedFactor_;
		
			//GetComponent<CameraControl>().Cam.SetCamera(direction.normalized, 2.5f, 20, 5f,10);

			for(int i = HomingTrailContainer.transform.childCount-1; i>=0; i--)
				Destroy(HomingTrailContainer.transform.GetChild(i).gameObject);

			GetComponent<S_PlayerInput>().LockInputForAWhile(10, true);

			CharacterAnimator.SetInteger("Action", 0);
			Action.ChangeAction(S_Enums.PlayerStates.Regular);
		}
    }

	private void AssignStats()
    {
		_dashSpeed_ = Tools.Stats.RingRoadStats.dashSpeed;
		_endingSpeedFactor_ = Tools.Stats.RingRoadStats.endingSpeedFactor;
		_minimumEndingSpeed_ = Tools.Stats.RingRoadStats.minimumEndingSpeed;
    }

	private void AssignTools()
    {
		Player = GetComponent<S_PlayerPhysics>();
		Action = GetComponent<S_ActionManager>();
		Inp = GetComponent<S_PlayerInput>();

		HomingTrailContainer = Tools.HomingTrailContainer;
		CharacterAnimator = Tools.CharacterAnimator;
		JumpBall = Tools.JumpBall;
		HomingTrail = Tools.HomingTrail;
	}

}
