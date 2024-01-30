using UnityEngine;
using System.Collections;

public class S_Action07_RingRoad : MonoBehaviour {

	S_CharacterTools Tools;


	Animator CharacterAnimator;
    Quaternion CharRot;
	S_ActionManager Action;
	GameObject HomingTrailContainer;
	public GameObject HomingTrail;
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
	float DashSpeed;
	float EndingSpeedFactor;
	float MinimumEndingSpeed;
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
		InitialVelocityMagnitude = Player.rb.velocity.magnitude;
		Player.rb.velocity = Vector3.zero;

		JumpBall.SetActive(false);
		if (HomingTrailContainer.transform.childCount < 1) 
		{
			GameObject HomingTrailClone = Instantiate (HomingTrail, HomingTrailContainer.transform.position, Quaternion.identity) as GameObject;
			HomingTrailClone.transform.parent = HomingTrailContainer.transform;
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
		CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
		CharacterAnimator.SetFloat("GroundSpeed", Player.rb.velocity.magnitude);
		CharacterAnimator.SetBool("Grounded", Player.Grounded);

		//Set Animation Angle
		Vector3 VelocityMod = new Vector3(Player.rb.velocity.x, Player.rb.velocity.y, Player.rb.velocity.z);
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
			Player.rb.velocity = direction.normalized * DashSpeed;

			GetComponent<S_Handler_Camera>().Cam.FollowDirection(4, 14f, -10,0);
		}

		else
		{
			float EndingSpeedResult = 0;

			EndingSpeedResult = Mathf.Max (MinimumEndingSpeed, InitialVelocityMagnitude);

			Player.rb.velocity = Vector3.zero;
			Player.rb.velocity = direction.normalized*EndingSpeedResult*EndingSpeedFactor;
		
			//GetComponent<CameraControl>().Cam.SetCamera(direction.normalized, 2.5f, 20, 5f,10);

			for(int i = HomingTrailContainer.transform.childCount-1; i>=0; i--)
				Destroy(HomingTrailContainer.transform.GetChild(i).gameObject);

			GetComponent<S_PlayerInput>().LockInputForAWhile(10, true);

			CharacterAnimator.SetInteger("Action", 0);
			Action.ChangeAction(0);
		}
    }

	private void AssignStats()
    {
		DashSpeed = Tools.stats.DashSpeed;
		EndingSpeedFactor = Tools.stats.EndingSpeedFactor;
		MinimumEndingSpeed = Tools.stats.MinimumEndingSpeed;
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
