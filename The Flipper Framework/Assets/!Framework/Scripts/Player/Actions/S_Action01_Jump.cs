using UnityEngine;
using System.Collections;

public class S_Action01_Jump : MonoBehaviour, IMainAction
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_CharacterTools      _Tools;
	private S_PlayerPhysics       _PlayerPhys;
	private S_PlayerInput         _Input;
	private S_ActionManager       _Actions;
	private S_Handler_Camera _CamHandler;
	private S_Handler_quickstep _QuickStepManager;
	private S_Control_PlayerSound _Sounds;

	private Animator CharacterAnimator;
	private GameObject JumpBall;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	[Header("Core Stats")]
	private float _startJumpDuration_;
	private float _startSlopedJumpDuration_;
	private float _startJumpSpeed_;
	private float _jumpSlopeConversion_;
	private float _stopYSpeedOnRelease_;

	[HideInInspector] 
	public bool _canDoubleJump_ = true;
	bool _canTripleJump_ = false;

	float _doubleJumpSpeed_;
	float _doubleJumpDuration_;
	float _speedLossOnDoubleJump_;
	#endregion

	// Trackers
	#region trackers

	public float skinRotationSpeed;
	public Vector3 InitialNormal { get; set; }
	public float Counter { get; set; }
	public float ControlCounter;
	[HideInInspector] public int jumpCount;
	[HideInInspector] public bool Jumping;

	private float JumpDuration;
	private float SlopedJumpDuration;
	private float JumpSpeed;


	float jumpSpeedModifier = 1f;
	float jumpDurationModifier = 1f;


	float jumpSlopeSpeed;


	[HideInInspector] public float timeJumping;
	bool cancelled;
	#endregion

	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {

	}

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		if (_PlayerPhys == null)
		{
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();
		}
		JumpBall.SetActive(true);
	}

	private void OnDisable () {
		JumpBall.SetActive(false);
	}


	// Update is called once per frame
	void Update () {
		//Set Animator Parameters
		_Actions.Action00.HandleAnimator(1);
		_Actions.Action00.SetSkinRotation(skinRotationSpeed);

		////Set Animation Angle
		//Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);

		//if (VelocityMod != Vector3.zero)
		//{
		//	Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
		//	CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
		//}

		//Actions
		if (!_Actions.isPaused)
		{
			//If can homing attack pressed.
			if (_Input.HomingPressed)
			{
				if (_Actions.Action02._isHomingAvailable && _Actions.Action02Control._HasTarget && !_PlayerPhys._isGrounded)
				{

					//Do a homing attack
					if (_Actions.Action02 != null && _PlayerPhys._homingDelay_ <= 0)
					{
						if (_Actions.Action02Control._isHomingAvailable)
						{
							_Sounds.HomingAttackSound();
							_Actions.ChangeAction(S_Enums.PlayerStates.Homing);
							_Actions.Action02.InitialEvents();
						}
					}

				}
			}


			//If no tgt, do air dash;
			if (_Input.SpecialPressed)
			{
				if (_Actions.Action02._isHomingAvailable)
				{
					if (!_Actions.Action02Control._HasTarget)
					{
						_Sounds.AirDashSound();
						_Actions.ChangeAction(S_Enums.PlayerStates.JumpDash);
						_Actions.Action11.InitialEvents();
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
					_Actions.ChangeAction(S_Enums.PlayerStates.Jump);
				}

				//Do a triple jump
				else if (jumpCount == 2 && _canTripleJump_)
				{
					//Debug.Log("Do a triple jump");
					InitialEvents(Vector3.up);
					_Actions.ChangeAction(S_Enums.PlayerStates.Jump);
				}
			}


			//Do a Bounce Attack
			if (_Input.BouncePressed)
			{
				if (_Actions.Action06.BounceAvailable && _PlayerPhys._RB.velocity.y < 35f)
				{
					_Actions.Action06.InitialEvents();
					_Actions.ChangeAction(S_Enums.PlayerStates.Bounce);
					//	Actions.Action06.ShouldStomp = false;
				}

			}

			//Set Camera to back
			if (_Input.CamResetPressed)
			{
				if (_Input.moveX == 0 && _Input.moveY == 0 && _PlayerPhys._horizontalSpeedMagnitude < 5f)
					_CamHandler._HedgeCam.GoBehindCharacter(6, 20, false);
			}



			//Do a DropDash 

			if (_Input.RollPressed)
			{
				_Actions.Action08.TryDropCharge();

			}

			_QuickStepManager.AttemptAction();
		}
	}

	private void FixedUpdate () {
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
			_Actions.Action00.SetIsRolling(false);
			if (Counter < SlopedJumpDuration)
			{
				_PlayerPhys.AddCoreVelocity(InitialNormal * (JumpSpeed), false);
				//Debug.Log(InitialNormal);
			}
			else
			{
				_PlayerPhys.AddCoreVelocity(new Vector3(0, 1, 0) * (JumpSpeed), false);
			}
			//Extra speed
			_PlayerPhys.AddCoreVelocity(new Vector3(0, 1, 0) * (jumpSlopeSpeed), false);
		}



		//Cancel Jump
		if (!cancelled && _PlayerPhys._RB.velocity.y > 0 && !Jumping && Counter > 0.1)
		{
			cancelled = true;
			//jumpCount = 1;
			Vector3 Velocity = new Vector3(_PlayerPhys._RB.velocity.x, _PlayerPhys._RB.velocity.y, _PlayerPhys._RB.velocity.z);
			Velocity.y = Velocity.y - _stopYSpeedOnRelease_;
			//Player._RB.velocity = Velocity;
			//Player.setTotalVelocity(Velocity);
		}

		//End Action
		if (_PlayerPhys._isGrounded && Counter > SlopedJumpDuration)
		{

			jumpCount = 0;


			_Input.JumpPressed = false;
			JumpBall.SetActive(false);

			_Actions.Action00.StartAction();
			_Actions.ChangeAction(S_Enums.PlayerStates.Regular);
			_Actions.Action06.BounceCount = 0;
			//JumpBall.SetActive(false);
		}

		//Skidding
		_Actions.skid.AttemptAction();
	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		if (_Input.JumpPressed)
		{
			switch(_Actions.whatAction)
			{
				default:
					//Normal grounded Jump
					if (_PlayerPhys._isGrounded)
					{
						InitialEvents(_PlayerPhys._groundNormal, true, _PlayerPhys._RB.velocity.y);
						_Actions.ChangeAction(S_Enums.PlayerStates.Jump);
					}
					//Jump from regular action due to coyote time
					else if (_Actions.Action00._coyoteInEffect)
					{
						InitialEvents(_Actions.Action00._coyoteRememberDir, true, _Actions.Action00._coyoteRememberSpeed);
						_Actions.ChangeAction(S_Enums.PlayerStates.Jump);
					}
					//Jump when in the air
					else
					{
						jumpCount = 0;
						InitialEvents(Vector3.up);
						_Actions.ChangeAction(S_Enums.PlayerStates.Jump);
					}
					willChangeAction = true;
					break;
			}
		}
		return willChangeAction;
	}

	public void StartAction () {

	}

	public void StopAction () {

	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	public void HandleInputs () {

	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {

		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Actions = GetComponent<S_ActionManager>();
		_CamHandler = GetComponent<S_Handler_Camera>();

		_QuickStepManager = GetComponent<S_Handler_quickstep>();
		_Input = GetComponent<S_PlayerInput>();

		CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
		JumpBall = _Tools.JumpBall;

	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_startJumpDuration_ = _Tools.Stats.JumpStats.startJumpDuration;
		_startJumpSpeed_ = _Tools.Stats.JumpStats.startJumpSpeed;
		_startSlopedJumpDuration_ = _Tools.Stats.JumpStats.startSlopedJumpDuration;
		_jumpSlopeConversion_ = _Tools.Stats.JumpStats.jumpSlopeConversion;
		_stopYSpeedOnRelease_ = _Tools.Stats.JumpStats.stopYSpeedOnRelease;

		_canDoubleJump_ = _Tools.Stats.MultipleJumpStats.canDoubleJump;
		_canTripleJump_ = _Tools.Stats.MultipleJumpStats.canTripleJump;
		_doubleJumpDuration_ = _Tools.Stats.MultipleJumpStats.doubleJumpDuration;
		_doubleJumpSpeed_ = _Tools.Stats.MultipleJumpStats.doubleJumpSpeed;

		_speedLossOnDoubleJump_ = _Tools.Stats.MultipleJumpStats.speedLossOnDoubleJump;
	}
	#endregion




	public void InitialEvents ( Vector3 normaltoJump, bool Grounded = false, float verticalSpeed = 0, float controlDelay = 0, float minJumpSpeed = 0 ) {
		if (!_Actions.lockDoubleJump)
		{
			_Input.RollPressed = false;
			cancelled = false;
			Jumping = true;
			Counter = 0;
			ControlCounter = controlDelay;
			jumpSlopeSpeed = 0;
			timeJumping = 0f;
			//Debug.Log(jumpCount);

			_PlayerPhys.SetIsGrounded(false);

			if (1 - Mathf.Abs(normaltoJump.y) < 0.1f)
				normaltoJump = Vector3.up;

			if (verticalSpeed < 0)
				verticalSpeed = 0;

			//If performing a grounded jump. JumpCount may be changed externally to allow for this.
			//if (Grounded || jumpCount == -1)
			if (Grounded)
			{
				//Player._RB.velocity = new Vector3(Player._RB.velocity.x * _speedLossOnJump_, verticalSpeed, Player._RB.velocity.z * _speedLossOnJump_);


				if (_Actions.eventMan != null) _Actions.eventMan.JumpsPerformed += 1;

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
				if (_PlayerPhys._RB.velocity.y > 0 && normaltoJump.y > 0)
				{
					jumpSlopeSpeed = _PlayerPhys._RB.velocity.y * _jumpSlopeConversion_;
				}

			}

			else
			{

				if (_Actions.eventMan != null) _Actions.eventMan.DoubleJumpsPerformed += 1;

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
			_Sounds.JumpSound();
		}

	}


	private void JumpAgain () {
		//jumpCount++;
		//InitialNormal = CharacterAnimator.transform.up;

		//SnapOutOfGround to make sure you do jump
		transform.position += (InitialNormal * 0.3f);

		Vector3 newVec;

		if (_PlayerPhys._RB.velocity.y > 10)
			newVec = new Vector3(_PlayerPhys._RB.velocity.x * _speedLossOnDoubleJump_, _PlayerPhys._RB.velocity.y, _PlayerPhys._RB.velocity.z * _speedLossOnDoubleJump_);
		else
			newVec = new Vector3(_PlayerPhys._RB.velocity.x * _speedLossOnDoubleJump_, Mathf.Clamp(_PlayerPhys._RB.velocity.y * 0.1f, 0.1f, 5), _PlayerPhys._RB.velocity.z * _speedLossOnDoubleJump_);

		//Player._RB.velocity = newVec;
		_PlayerPhys.SetTotalVelocity(newVec);

		GameObject JumpDashParticleClone = Instantiate(_Tools.JumpDashParticle, _Tools.FeetPoint.position, Quaternion.identity) as GameObject;

		//JumpDashParticleClone.GetComponent<ParticleSystem>().startSize = 1f;

		JumpDashParticleClone.transform.position = _Tools.FeetPoint.position;
		JumpDashParticleClone.transform.rotation = Quaternion.LookRotation(Vector3.up);

	}
}
