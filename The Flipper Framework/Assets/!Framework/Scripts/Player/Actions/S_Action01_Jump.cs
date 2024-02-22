using UnityEngine;
using System.Collections;

public class S_Action01_Jump : MonoBehaviour
{
	S_CharacterTools Tools;

	RaycastHit hit;
	Vector3 DirInput;

	[Header("Basic Resources")]

	S_PlayerPhysics Player;
	S_ActionManager Actions;
	S_Handler_HomingAttack homingControl;
	S_Handler_Camera Cam;
	S_Handler_quickstep stepManager;
	S_PlayerInput _Input;

	S_Control_PlayerSound sounds;
	Animator CharacterAnimator;
	public float skinRotationSpeed;
	GameObject JumpBall;

	public Vector3 InitialNormal { get; set; }
	public float Counter { get; set; }
	public float ControlCounter;
	[HideInInspector] public int jumpCount;
	[HideInInspector] public bool Jumping;

	[Header("Core Stats")]
	[HideInInspector] public float _startJumpDuration_;
	[HideInInspector] public float _startSlopedJumpDuration_;
	[HideInInspector] public float _startJumpSpeed_;
	[HideInInspector] public float _jumpSlopeConversion_;
	[HideInInspector] public float _stopYSpeedOnRelease_;
	[HideInInspector] public float _rollingLandingBoost_;

	[HideInInspector] public float JumpDuration;
	[HideInInspector] public float SlopedJumpDuration;
	[HideInInspector] public float JumpSpeed;
	float _speedLossOnJump_;
	float _speedLossOnDoubleJump_;


	float jumpSpeedModifier = 1f;
	float jumpDurationModifier = 1f;

	[Header("Additional Jump Stats")]

	[HideInInspector] public bool _canDoubleJump_ = true;
	bool _canTripleJump_ = false;

	float _doubleJumpSpeed_;
	float _doubleJumpDuration_;


	float jumpSlopeSpeed;


	[HideInInspector] public float timeJumping;
	bool cancelled;




	void Awake () {
		if (Player == null)
		{
			Tools = GetComponent<S_CharacterTools>();
			AssignTools();

			AssignStats();
		}
	}

	public void InitialEvents ( Vector3 normaltoJump, bool Grounded = false, float verticalSpeed = 0, float controlDelay = 0, float minJumpSpeed = 0 ) {
		if (!Actions.lockDoubleJump)
		{
			_Input.RollPressed = false;
			cancelled = false;
			Jumping = true;
			Counter = 0;
			ControlCounter = controlDelay;
			jumpSlopeSpeed = 0;
			timeJumping = 0f;
			//Debug.Log(jumpCount);

			Player.SetIsGrounded(false);

			if (1 - Mathf.Abs(normaltoJump.y) < 0.1f)
				normaltoJump = Vector3.up;

			if (verticalSpeed < 0)
				verticalSpeed = 0;

			//If performing a grounded jump. JumpCount may be changed externally to allow for this.
			//if (Grounded || jumpCount == -1)
			if (Grounded)
			{
				//Player._RB.velocity = new Vector3(Player._RB.velocity.x * _speedLossOnJump_, verticalSpeed, Player._RB.velocity.z * _speedLossOnJump_);


				if (Actions.eventMan != null) Actions.eventMan.JumpsPerformed += 1;

				//Sets jump stats for this specific jump.

				JumpSpeed = _startJumpSpeed_ * jumpSpeedModifier;
				JumpDuration = _startJumpDuration_ * jumpDurationModifier;
				SlopedJumpDuration = _startSlopedJumpDuration_ * jumpDurationModifier;

				//Number of jumps set to zero, allowing for double jumps.
				jumpCount = 0;

				//Sets jump direction
				InitialNormal = normaltoJump;

				//SnapOutOfGround to make sure you do jump
				transform.position += (InitialNormal * 0.3f);

				//Jump higher depending on the speed and the slope you're in
				if (Player._RB.velocity.y > 0 && normaltoJump.y > 0)
				{
					jumpSlopeSpeed = Player._RB.velocity.y * _jumpSlopeConversion_;
				}

			}

			else
			{

				if (Actions.eventMan != null) Actions.eventMan.DoubleJumpsPerformed += 1;

				//Increases jump count
				if (jumpCount == 0)
					jumpCount = 1;

				//Sets jump direction
				InitialNormal = normaltoJump;

				//Sets jump stats for this specific jump.
				JumpSpeed = _doubleJumpSpeed_ * jumpSpeedModifier;
				JumpDuration = _doubleJumpDuration_ * jumpDurationModifier;
				SlopedJumpDuration = _doubleJumpDuration_ * jumpDurationModifier;

				JumpAgain();
			}

			//SetAnims
			CharacterAnimator.SetInteger("Action", 1);


			//Sound
			sounds.JumpSound();
		}

	}

	void Update () {

		//Set Animator Parameters
		if (Actions.whatAction == S_Enums.PlayerStates.Jump)
		{
			CharacterAnimator.SetInteger("Action", 1);
		}

		if (!(Player._RB.velocity.y < 0.1f && Player._RB.velocity.y > -0.1f))
		{
			CharacterAnimator.SetFloat("YSpeed", Player._RB.velocity.y);
		}
		CharacterAnimator.SetFloat("GroundSpeed", Player._horizontalSpeedMagnitude);
		CharacterAnimator.SetBool("Grounded", Player._isGrounded);
		CharacterAnimator.SetBool("isRolling", false);

		//Set Animation Angle
		Vector3 VelocityMod = new Vector3(Player._RB.velocity.x, 0, Player._RB.velocity.z);

		if (VelocityMod != Vector3.zero)
		{
			Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
			CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
		}

		//if (Actions.Action02 != null)
		//{
		//    Actions.Action02.HomingAvailable = true;
		//}
		if (Actions.Action06 != null)
		{
			Actions.Action06.BounceAvailable = true;
		}

		//Actions
		if (!Actions.isPaused)
		{
			//If can homing attack pressed.
			if (_Input.HomingPressed)
			{
				if (Actions.Action02.HomingAvailable && Actions.Action02Control._HasTarget && !Player._isGrounded)
				{

					//Do a homing attack
					if (Actions.Action02 != null && Player._homingDelay_ <= 0)
					{
						if (Actions.Action02Control._isHomingAvailable)
						{
							sounds.HomingAttackSound();
							Actions.ChangeAction(S_Enums.PlayerStates.Homing);
							Actions.Action02.InitialEvents();
						}
					}

				}
			}


			//If no tgt, do air dash;
			if (_Input.SpecialPressed)
			{
				if (Actions.Action02.HomingAvailable)
				{
					if (!Actions.Action02Control._HasTarget)
					{
						sounds.AirDashSound();
						Actions.ChangeAction(S_Enums.PlayerStates.JumpDash);
						Actions.Action11.InitialEvents();
					}
				}
			}



			//Handle additional jumps. Can only be done if jump is over but jump button pressed.
			if (!Jumping && _Input.JumpPressed)
			{
				//Do a double jump
				if (jumpCount == 1 && _canDoubleJump_)
				{

					InitialEvents(Vector3.up);
					Actions.ChangeAction(S_Enums.PlayerStates.Jump);
				}

				//Do a triple jump
				else if (jumpCount == 2 && _canTripleJump_)
				{
					//Debug.Log("Do a triple jump");
					InitialEvents(Vector3.up);
					Actions.ChangeAction(S_Enums.PlayerStates.Jump);
				}
			}


			//Do a Bounce Attack
			if (_Input.BouncePressed)
			{
				if (Actions.Action06.BounceAvailable && Player._RB.velocity.y < 35f)
				{
					Actions.Action06.InitialEvents();
					Actions.ChangeAction(S_Enums.PlayerStates.Bounce);
					//	Actions.Action06.ShouldStomp = false;
				}

			}

			//Set Camera to back
			if (_Input.CamResetPressed)
			{
				if (_Input.moveX == 0 && _Input.moveY == 0 && Player._horizontalSpeedMagnitude < 5f)
					Cam._HedgeCam.FollowDirection(10, 14f, -10, 0);
			}



			//Do a DropDash 

			if (_Input.RollPressed)
			{
				Actions.Action08.TryDropCharge();

			}

			/////Enalbing Quickstepping
			///
			//Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
			if (_Input.RightStepPressed)
			{
				stepManager.pressRight();
			}
			else if (_Input.LeftStepPressed)
			{
				stepManager.pressLeft();
			}

			//Enable Quickstep right or left
			if (_Input.RightStepPressed && !stepManager.enabled)
			{
				if (Player._horizontalSpeedMagnitude > 15f)
				{
					Debug.Log("Step in Jump");
					stepManager.initialEvents(true);
					stepManager.enabled = true;
				}

			}

			else if (_Input.LeftStepPressed && !stepManager.enabled)
			{

				if (Player._horizontalSpeedMagnitude > 15f)
				{

					stepManager.initialEvents(false);
					stepManager.enabled = true;
				}

			}
		}

	}

	void FixedUpdate () {
		//Jump action
		Counter += Time.fixedDeltaTime;
		ControlCounter += Time.fixedDeltaTime;
		timeJumping += Time.fixedDeltaTime;

		//if (!Actions.JumpPressed && Counter < JumpDuration && Counter > 0.1f && Jumping)
		if (!_Input.JumpPressed && Counter > 0.1f && Jumping)
		{
			jumpCount++;
			Counter = JumpDuration;
			Jumping = false;
		}
		else if (Counter > JumpDuration && Jumping && _Input.JumpPressed)
		{
			jumpCount++;
			Counter = JumpDuration;
			Jumping = false;
			_Input.JumpPressed = false;
		}
		//Add Jump Speed
		else if (Counter < JumpDuration && Jumping)
		{
			Player._isRolling = false;
			if (Counter < SlopedJumpDuration)
			{
				Player.AddCoreVelocity(InitialNormal * (JumpSpeed), false);
				//Debug.Log(InitialNormal);
			}
			else
			{
				Player.AddCoreVelocity(new Vector3(0, 1, 0) * (JumpSpeed), false);
			}
			//Extra speed
			Player.AddCoreVelocity(new Vector3(0, 1, 0) * (jumpSlopeSpeed), false);
		}



		//Cancel Jump
		if (!cancelled && Player._RB.velocity.y > 0 && !Jumping && Counter > 0.1)
		{
			cancelled = true;
			//jumpCount = 1;
			Vector3 Velocity = new Vector3(Player._RB.velocity.x, Player._RB.velocity.y, Player._RB.velocity.z);
			Velocity.y = Velocity.y - _stopYSpeedOnRelease_;
			//Player._RB.velocity = Velocity;
			//Player.setTotalVelocity(Velocity);
		}

		//End Action
		if (Player._isGrounded && Counter > SlopedJumpDuration)
		{

			jumpCount = 0;


			_Input.JumpPressed = false;
			JumpBall.SetActive(false);

			Actions.ChangeAction(S_Enums.PlayerStates.Regular);
			Actions.Action06.BounceCount = 0;
			//JumpBall.SetActive(false);
		}

		//Skidding
		Actions.skid.jumpSkid();


	}


	private void JumpAgain () {
		//jumpCount++;
		//InitialNormal = CharacterAnimator.transform.up;

		//SnapOutOfGround to make sure you do jump
		transform.position += (InitialNormal * 0.3f);

		Vector3 newVec;

		if (Player._RB.velocity.y > 10)
			newVec = new Vector3(Player._RB.velocity.x * _speedLossOnDoubleJump_, Player._RB.velocity.y, Player._RB.velocity.z * _speedLossOnDoubleJump_);
		else
			newVec = new Vector3(Player._RB.velocity.x * _speedLossOnDoubleJump_, Mathf.Clamp(Player._RB.velocity.y * 0.1f, 0.1f, 5), Player._RB.velocity.z * _speedLossOnDoubleJump_);

		//Player._RB.velocity = newVec;
		Player.SetTotalVelocity(newVec);

		GameObject JumpDashParticleClone = Instantiate(Tools.JumpDashParticle, Tools.FeetPoint.position, Quaternion.identity) as GameObject;

		//JumpDashParticleClone.GetComponent<ParticleSystem>().startSize = 1f;

		JumpDashParticleClone.transform.position = Tools.FeetPoint.position;
		JumpDashParticleClone.transform.rotation = Quaternion.LookRotation(Vector3.up);

	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_startJumpDuration_ = Tools.Stats.JumpStats.startJumpDuration;
		_startJumpSpeed_ = Tools.Stats.JumpStats.startJumpSpeed;
		_startSlopedJumpDuration_ = Tools.Stats.JumpStats.startSlopedJumpDuration;
		_jumpSlopeConversion_ = Tools.Stats.JumpStats.jumpSlopeConversion;
		_rollingLandingBoost_ = Tools.Stats.JumpStats.jumpRollingLandingBoost;
		_stopYSpeedOnRelease_ = Tools.Stats.JumpStats.stopYSpeedOnRelease;

		_canDoubleJump_ = Tools.Stats.MultipleJumpStats.canDoubleJump;
		_canTripleJump_ = Tools.Stats.MultipleJumpStats.canTripleJump;
		_doubleJumpDuration_ = Tools.Stats.MultipleJumpStats.doubleJumpDuration;
		_doubleJumpSpeed_ = Tools.Stats.MultipleJumpStats.doubleJumpSpeed;

		_speedLossOnDoubleJump_ = Tools.Stats.MultipleJumpStats.speedLossOnDoubleJump;
		_speedLossOnJump_ = Tools.Stats.JumpStats.speedLossOnJump;

	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		Player = GetComponent<S_PlayerPhysics>();
		Actions = GetComponent<S_ActionManager>();
		Cam = GetComponent<S_Handler_Camera>();
		homingControl = GetComponent<S_Handler_HomingAttack>();

		stepManager = GetComponent<S_Handler_quickstep>();
		_Input = GetComponent<S_PlayerInput>();

		CharacterAnimator = Tools.CharacterAnimator;
		sounds = Tools.SoundControl;
		JumpBall = Tools.JumpBall;
	}


	private void OnDisable () {
		JumpBall.SetActive(false);
	}

	private void OnEnable () {
		JumpBall.SetActive(true);
	}
}
