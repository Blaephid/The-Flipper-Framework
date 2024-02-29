using UnityEngine;
using System.Collections;

[RequireComponent(typeof(S_ActionManager))]
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
	private S_SubAction_Quickstep _QuickStepManager;
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

	private int         _positionInActionList;

	public float	_skinRotationSpeed;
	public Vector3	_initialNormal { get; set; }
	public float	_counter { get; set; }
	public float	_controlCounter;
	[HideInInspector] 
	public int	_jumpCount;
	[HideInInspector] 
	public bool	_isJumping;

	private float	_thisJumpDuration;
	private float	_slopedJumpDuration;
	private float	_thisJumpSpeed;
	private float	_jumpSlopeSpeed;


	private float	_jumpSpeedModifier = 1f;
	private float	_jumpDurationModifier = 1f;


	[HideInInspector] 
	public float	_timeJumping;
	private bool	_isCancelled;
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
			//Assign all external values needed for gameplay.
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
			for (int i = 0 ; i < _Actions._MainActions.Count ; i++)
			{
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Default)
				{
					_positionInActionList = i;
					break;
				}
			}
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
		_Actions.Action00.SetSkinRotation(_skinRotationSpeed);

		//Actions
		if (!_Actions.isPaused)
		{
			HandleInputs();
		}
	}

	private void FixedUpdate () {
		//Jump action
		_counter += Time.fixedDeltaTime;
		_controlCounter += Time.fixedDeltaTime;
		_timeJumping += Time.fixedDeltaTime;

		//if (!Actions.JumpPressed && Counter < JumpDuration && Counter > 0.1f && Jumping)
		if (!_Input.JumpPressed && _counter > 0.1f && _isJumping)
		{
			_jumpCount++;
			_counter = _thisJumpDuration;
			_isJumping = false;
		}
		else if (_counter > _thisJumpDuration && _isJumping && _Input.JumpPressed)
		{
			_jumpCount++;
			_counter = _thisJumpDuration;
			_isJumping = false;
			_Input.JumpPressed = false;
		}
		//Add Jump Speed
		else if (_counter < _thisJumpDuration && _isJumping)
		{
			_Actions.Action00.SetIsRolling(false);
			if (_counter < _slopedJumpDuration)
			{
				_PlayerPhys.AddCoreVelocity(_initialNormal * (_thisJumpSpeed), false);
				//Debug.Log(InitialNormal);
			}
			else
			{
				_PlayerPhys.AddCoreVelocity(new Vector3(0, 1, 0) * (_thisJumpSpeed), false);
			}
			//Extra speed
			_PlayerPhys.AddCoreVelocity(new Vector3(0, 1, 0) * (_jumpSlopeSpeed), false);
		}



		//Cancel Jump
		if (!_isCancelled && _PlayerPhys._RB.velocity.y > 0 && !_isJumping && _counter > 0.1)
		{
			_isCancelled = true;
			//jumpCount = 1;
			Vector3 Velocity = new Vector3(_PlayerPhys._RB.velocity.x, _PlayerPhys._RB.velocity.y, _PlayerPhys._RB.velocity.z);
			Velocity.y = Velocity.y - _stopYSpeedOnRelease_;
			//Player._RB.velocity = Velocity;
			//Player.setTotalVelocity(Velocity);
		}

		//End Action
		if (_PlayerPhys._isGrounded && _counter > _slopedJumpDuration)
		{

			_jumpCount = 0;


			_Input.JumpPressed = false;
			JumpBall.SetActive(false);

			_Actions.Action00.StartAction();
			_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Default);
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
				case S_Enums.PrimaryPlayerStates.Default:
					//Normal grounded Jump
					if (_PlayerPhys._isGrounded)
					{
						InitialEvents(_PlayerPhys._groundNormal, true, _PlayerPhys._RB.velocity.y);
						_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Jump);
					}
					//Jump from regular action due to coyote time
					else if (_Actions.Action00._coyoteInEffect)
					{
						InitialEvents(_Actions.Action00._coyoteRememberDir, true, _Actions.Action00._coyoteRememberSpeed);
						_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Jump);
					}
					//Jump when in the air
					else
					{
						_jumpCount = 0;
						InitialEvents(Vector3.up);
						_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Jump);
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
		if(!_Actions.isPaused)
		{
			//Moving camera behind
			_CamHandler.AttemptCameraReset();

			//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
			_Actions.HandleInputs(_positionInActionList);
		}
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

		_QuickStepManager = GetComponent<S_SubAction_Quickstep>();
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
			_isCancelled = false;
			_isJumping = true;
			_counter = 0;
			_controlCounter = controlDelay;
			_jumpSlopeSpeed = 0;
			_timeJumping = 0f;
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

				_thisJumpSpeed = _startJumpSpeed_ * _jumpSpeedModifier;
				_thisJumpDuration = _startJumpDuration_ * _jumpDurationModifier;
				_slopedJumpDuration = _startSlopedJumpDuration_ * _jumpDurationModifier;

				//Number of jumps set to zero, allowing for double jumps.
				_jumpCount = 0;

				//Sets jump direction
				_initialNormal = normaltoJump;

				//SnapOutOfGround to make sure you do jump
				transform.position += (_initialNormal * 0.3f);

				//Jump higher depending on the speed and the slope you're in
				if (_PlayerPhys._RB.velocity.y > 0 && normaltoJump.y > 0)
				{
					_jumpSlopeSpeed = _PlayerPhys._RB.velocity.y * _jumpSlopeConversion_;
				}

			}

			else
			{

				if (_Actions.eventMan != null) _Actions.eventMan.DoubleJumpsPerformed += 1;

				//Increases jump count
				if (_jumpCount == 0)
					_jumpCount = 1;

				//Sets jump direction
				_initialNormal = normaltoJump;

				//Sets jump stats for this specific jump.
				_thisJumpSpeed = _doubleJumpSpeed_ * _jumpSpeedModifier;
				_thisJumpDuration = _doubleJumpDuration_ * _jumpDurationModifier;
				_slopedJumpDuration = _doubleJumpDuration_ * _jumpDurationModifier;

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
		transform.position += (_initialNormal * 0.3f);

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
