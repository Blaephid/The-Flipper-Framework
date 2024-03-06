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
	private S_Handler_Camera      _CamHandler;
	private S_Control_PlayerSound _Sounds;

	private Animator              _CharacterAnimator;
	private GameObject            _JumpBall;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	//Main jump
	private float       _maxJumpTime_;
	private float       _minJumpTime_;
	private float       _startSlopedJumpDuration_;
	private float       _startJumpSpeed_;
	private float       _jumpSlopeConversion_;
	private float       _stopYSpeedOnRelease_;

	//Additional jumps
	private int         _maxJumps_;
	private float       _doubleJumpSpeed_;
	private float       _doubleJumpDuration_;
	private float       _speedLossOnDoubleJump_;
	#endregion

	// Trackers
	#region trackers

	private int         _positionInActionList;

	public float        _skinRotationSpeed;
	[HideInInspector]
	public Vector3      _upwardsDirection;
	[HideInInspector]
	public float        _counter;
	[HideInInspector]
	public int          _jumpCount;
	[HideInInspector]
	public bool         _isJumping;
	private bool        _isJumpingFromGround;

	private float       _thisJumpDuration;
	private float       _slopedJumpDuration;
	private float       _thisJumpSpeed;
	private float       _jumpSlopeSpeed;


	private float       _jumpSpeedModifier = 1f;
	private float       _jumpDurationModifier = 1f;

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
		ReadyAction();
		_JumpBall.SetActive(true);
	}

	private void OnDisable () {
		_JumpBall.SetActive(false);
	}


	// Update is called once per frame
	void Update () {
		//Set Animator Parameters
		_Actions.Action00.HandleAnimator(1);
		_Actions.Action00.SetSkinRotationToVelocity(_skinRotationSpeed);

		//Actions
		if (!_Actions.isPaused)
		{
			HandleInputs();
		}
	}

	private void FixedUpdate () {
		//Tracking length of jump
		_counter += Time.fixedDeltaTime;
		_Actions._actionTimeCounter += Time.fixedDeltaTime;

		//Ending Jump Early
		if (!_Input.JumpPressed && _counter > _minJumpTime_ && _isJumping)
		{
			_jumpCount++;
			_counter = _thisJumpDuration;
			_isJumping = false;
		}
		//Ending jump after max duration
		else if (_counter > _thisJumpDuration && _isJumping && _Input.JumpPressed)
		{
			_jumpCount++;
			_counter = _thisJumpDuration;
			_isJumping = false;
			_Input.JumpPressed = false;
		}
		//Add Jump Speed
		else if (_isJumping)
		{
			//Jump move at angle
			if (_counter < _slopedJumpDuration)
			{
				_PlayerPhys.AddCoreVelocity(_upwardsDirection * (_thisJumpSpeed * 0.75f), false);
				_PlayerPhys.AddCoreVelocity(Vector3.up * (_jumpSlopeSpeed * 0.25f), false); //Extra speed to ballance out direction
			}
			//Move straight up in world.
			else
			{
				_PlayerPhys.AddCoreVelocity(Vector3.up * (_thisJumpSpeed), false);
			}
		}

		//End Action on landing. Has to have been in the air for some time first though to prevent immediately becoming grounded.
		if (_PlayerPhys._isGrounded && _counter > _slopedJumpDuration)
		{
			_jumpCount = 0;

			//Prevents holding jump to keep doing so forever.
			_Input.JumpPressed = false;

			_Actions.Action00.StartAction();
		}
	}

	//Called when checking if this action is to be performed, including inputs.
	public bool AttemptAction () {
		if (_Input.JumpPressed)
		{
			switch (_Actions.whatAction)
			{
				default:
					//Normal grounded Jump
					if (_PlayerPhys._isGrounded)
					{
						AssignStartValues(_PlayerPhys._groundNormal, true);
						StartAction();
					}
					//Jump from regular action due to coyote time
					else if (_Actions.Action00.enabled && _Actions.Action00._isCoyoteInEffect)
					{
						AssignStartValues(_Actions.Action00._coyoteRememberDirection, true);
						StartAction();
					}
					//Jump when in the air
					else if (_jumpCount < _maxJumps_ && !_Actions.lockDoubleJump)
					{
						AssignStartValues(Vector3.up, false);
						StartAction();
					}
					return true;

				case S_Enums.PrimaryPlayerStates.Jump:
					if (!_isJumping && _jumpCount < _maxJumps_ && !_Actions.lockDoubleJump)
					{
						AssignStartValues(Vector3.up, false);
						StartAction();
					}
					return true;

				case S_Enums.PrimaryPlayerStates.Rail:
					AssignStartValues(transform.up, true);
					StartAction();
					return true;
				case S_Enums.PrimaryPlayerStates.WallRunning:
					AssignStartValues(_Actions.Action12._jumpAngle, true);
					StartAction();
					return true;
			}
		}
		return false;
	}

	public void StartAction () {

		ReadyAction();

		//Setting private
		_isJumping = true;
		_counter = 0;

		//Setting public
		_Input.RollPressed = false;
		_Actions._actionTimeCounter = 0;
		_PlayerPhys.SetIsGrounded(false);

		//Effects
		_CharacterAnimator.SetInteger("Action", 1);
		_CharacterAnimator.SetTrigger("ChangedState");
		_Sounds.JumpSound();

		//Snap off of ground to make sure you do jump
		transform.position += (_upwardsDirection * 0.3f);

		//If performing a grounded jump. JumpCount may be changed externally to allow for this.
		if (_isJumpingFromGround)
		{
			if (_Actions.eventMan != null) _Actions.eventMan.JumpsPerformed += 1;

			//Sets jump stats for this specific jump.
			_thisJumpSpeed = _startJumpSpeed_ * _jumpSpeedModifier;
			_thisJumpDuration = _maxJumpTime_ * _jumpDurationModifier;
			_slopedJumpDuration = _startSlopedJumpDuration_ * _jumpDurationModifier;

			//Number of jumps set to zero, allowing for double jumps.
			_jumpCount = 0;

		}
		else
		{
			if (_Actions.eventMan != null) _Actions.eventMan.DoubleJumpsPerformed += 1;

			//Sets jump stats for this specific jump.
			_thisJumpSpeed = _doubleJumpSpeed_ * _jumpSpeedModifier;
			_thisJumpDuration = _doubleJumpDuration_ * _jumpDurationModifier;
			_slopedJumpDuration = _doubleJumpDuration_ * _jumpDurationModifier;

			JumpInAir();
		}

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Jump);
	}

	public void StopAction () {
		this.enabled = false;
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Called when entering the action, to ready any variables needed for performing it.
	private void AssignStartValues ( Vector3 normaltoJump, bool fromGround = false ) {
		if (1 - Mathf.Abs(normaltoJump.y) < 0.1f)
			normaltoJump = Vector3.up;


		//Sets jump direction
		_upwardsDirection = normaltoJump;

		//Jump higher depending on the speed and the slope you're in
		if (fromGround && _PlayerPhys._RB.velocity.y > 0 && normaltoJump.y > 0)
		{
			_jumpSlopeSpeed = _PlayerPhys._RB.velocity.y * _jumpSlopeConversion_;
		}

		_isJumpingFromGround = fromGround;
	}

	public void HandleInputs () {
		if (!_Actions.isPaused)
		{
			//Moving camera behind
			_CamHandler.AttemptCameraReset();

			//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
			_Actions.HandleInputs(_positionInActionList);
		}
	}

	//Additional effects if a jump is being made from in the air.
	private void JumpInAir () {

		//Take some horizontal speed on jump and remove vertical speed to ensure jump has an upwards force.
		Vector3 newVec;
		if (_PlayerPhys._RB.velocity.y > 10)
			newVec = new Vector3(_PlayerPhys._RB.velocity.x * _speedLossOnDoubleJump_, _PlayerPhys._RB.velocity.y, _PlayerPhys._RB.velocity.z * _speedLossOnDoubleJump_);
		else
			newVec = new Vector3(_PlayerPhys._RB.velocity.x * _speedLossOnDoubleJump_, Mathf.Clamp(_PlayerPhys._RB.velocity.y * 0.1f, 0.1f, 5), _PlayerPhys._RB.velocity.z * _speedLossOnDoubleJump_);
		_PlayerPhys.SetCoreVelocity(newVec, true);

		//Add particle effect during jump
		GameObject JumpDashParticleClone = Instantiate(_Tools.JumpDashParticle, _Tools.FeetPoint.position, Quaternion.identity) as GameObject;
		JumpDashParticleClone.transform.position = _Tools.FeetPoint.position;
		JumpDashParticleClone.transform.rotation = Quaternion.LookRotation(Vector3.up);

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

	public void ReadyAction () {
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
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Actions = GetComponent<S_ActionManager>();
		_CamHandler = GetComponent<S_Handler_Camera>();
		_Input = GetComponent<S_PlayerInput>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
		_JumpBall = _Tools.JumpBall;

	}

	//Responsible for assigning stats from the stats script.
	private void AssignStats () {
		_maxJumpTime_ = _Tools.Stats.JumpStats.startJumpDuration.y;
		_minJumpTime_ = _Tools.Stats.JumpStats.startJumpDuration.x;
		_startJumpSpeed_ = _Tools.Stats.JumpStats.startJumpSpeed;
		_startSlopedJumpDuration_ = _Tools.Stats.JumpStats.startSlopedJumpDuration;
		_jumpSlopeConversion_ = _Tools.Stats.JumpStats.jumpSlopeConversion;
		_stopYSpeedOnRelease_ = _Tools.Stats.JumpStats.stopYSpeedOnRelease;

		_maxJumps_ = _Tools.Stats.MultipleJumpStats.maxJumpCount;
		_doubleJumpDuration_ = _Tools.Stats.MultipleJumpStats.doubleJumpDuration;
		_doubleJumpSpeed_ = _Tools.Stats.MultipleJumpStats.doubleJumpSpeed;

		_speedLossOnDoubleJump_ = _Tools.Stats.MultipleJumpStats.speedLossOnDoubleJump;
	}
	#endregion
}
