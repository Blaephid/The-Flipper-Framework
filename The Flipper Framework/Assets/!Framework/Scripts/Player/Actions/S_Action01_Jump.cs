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
	private S_Handler_Camera      _CamHandler;
	private S_Control_SoundsPlayer _Sounds;

	private Animator              _CharacterAnimator;
	private GameObject            _JumpBall;
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	//Main jump
	private float		_maxJumpTime_;
	private float		_minJumpTime_;
	private float		_startSlopedJumpDuration_;

	private float		_startJumpSpeed_;
	private AnimationCurve	_JumpForceByTime_;
	private float		_jumpSlopeConversion_;

	//Additional jumps
	private int         _maxJumps_;
	private float       _doubleJumpSpeed_;
	private Vector2       _doubleJumpDuration_;
	private float       _speedLossOnDoubleJump_;
	#endregion

	// Trackers
	#region trackers

	private int         _positionInActionList;         //In every action script, takes note of where in the Action Managers Main action list this script is. 

	public float        _skinRotationSpeed = 8;

	[HideInInspector]
	public Vector3      _upwardsDirection;	//The direction the jump will move in. If on the ground, follows the normal of the floor, otherwise is upwards.
	[HideInInspector]
	public float        _counter;		//Tracks how long is jumping for
	[HideInInspector]
	public bool         _isJumping;	//Will only apply force if this is true, when false, it means another jump can now be performed
	private bool        _isJumpingFromGround;	//Seperates grounded and air jumps

	private float       _thisMinDuration;		//After this is exceeded, can end jump by releasing button.
	private float       _thisMaxDuration;		//Cancels jump when counter exceeds this. Affected by stats and situation
	private float       _slopedJumpDuration;	//When exceeded, jump will only move upwards, even if started on a slope.	
	private float       _thisJumpSpeed;		
	private float       _jumpSlopeSpeed;		//Jump speed is different on slopes, determined by upwards speed and conversion stat

	//Edit the relevant values when jump starts, currently set to 1 but can be changed externally depending on other interacitons.
	private float       _jumpSpeedModifier = 1f;
	private float       _jumpDurationModifier = 1f;

	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited
	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyAction();
	}


	// Update is called once per frame
	void Update () {
		if(!_Actions._ActionDefault._isAnimatorControlledExternally)
		{
			_Actions._ActionDefault._animationAction = 1;

			//Set Animator Parameters
			_Actions._ActionDefault.HandleAnimator(_Actions._ActionDefault._animationAction);
			_Actions._ActionDefault.SetSkinRotationToVelocity(_skinRotationSpeed);
		}
	}

	private void FixedUpdate () {
		HandleInputs();

		//Tracking length of jump
		_counter += Time.fixedDeltaTime;
		_Actions._actionTimeCounter += Time.fixedDeltaTime;

		ApplyForce();

		CheckShouldEndAction();
	}

	//Called when checking if this action is to be performed, including inputs.
	public bool AttemptAction () {
		if (_Input.JumpPressed)
		{
			switch (_Actions._whatAction)
			{
				case S_Enums.PrimaryPlayerStates.Default:
					//Normal grounded Jump
					if (_PlayerPhys._isGrounded)
					{
						AssignStartValues(_PlayerPhys._groundNormal, true);
						StartAction();
					}
					//Jump from regular action due to coyote time
					else if (_Actions._ActionDefault.enabled && _Actions._ActionDefault._isCoyoteInEffect)
					{
						AssignStartValues(_Actions._ActionDefault._coyoteRememberDirection, true);
						StartAction();
					}
					//Jump when in the air
					else if (_Actions._jumpCount < _maxJumps_ && _Actions._areAirActionsAvailable)
					{
						AssignStartValues(Vector3.up, false);
						StartAction();
					}
					return true;
				case S_Enums.PrimaryPlayerStates.Jump:
					if (!_isJumping && _Actions._jumpCount < _maxJumps_ && _Actions._areAirActionsAvailable)
					{
						AssignStartValues(Vector3.up, false);
						StartAction();
					}
					return true;
				case S_Enums.PrimaryPlayerStates.WallRunning:
					AssignStartValues(_Actions.Action12._jumpAngle, true);
					StartAction();
					return true;
				case S_Enums.PrimaryPlayerStates.Rail:
					AssignStartValues(transform.up, true);
					StartAction();
					return true;
				default:
					AssignStartValues(transform.up, _PlayerPhys._isGrounded);
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

		//Physics
		_PlayerPhys.SetIsGrounded(false);
		_PlayerPhys._canBeGrounded = false; //Prevents being set to grounded if jumping because it would lead to weird interactions going up through platforms or triggering on grounded events
		_PlayerPhys.RemoveEnvironmentalVelocityAirAction();

		//Effects
		_CharacterAnimator.SetInteger("Action", 1);
		_CharacterAnimator.SetTrigger("ChangedState");
		_Sounds.JumpSound();
		_JumpBall.SetActive(true);
		_Actions._ActionDefault.SwitchSkin(false);

		//Snap off of ground to make sure player jumps
		_PlayerPhys.transform.position += (_upwardsDirection * 0.3f);

		//If performing a grounded jump. JumpCount may be changed externally to allow for this.
		if (_isJumpingFromGround)
		{
			//Sets jump stats for this specific jump.
			_thisJumpSpeed = _startJumpSpeed_ * _jumpSpeedModifier;
			_thisMinDuration = _minJumpTime_ * _jumpDurationModifier;
			_thisMaxDuration = _maxJumpTime_ * _jumpDurationModifier;

			//Jump higher depending based on speed, if jumping upwards off a slope the players running up.
			if (_PlayerPhys._RB.velocity.y > 5 && _upwardsDirection.y > 1)
			{
				_jumpSlopeSpeed = Mathf.Max( _PlayerPhys._RB.velocity.y * _jumpSlopeConversion_, _thisJumpSpeed);
				_slopedJumpDuration = _startSlopedJumpDuration_ * _jumpDurationModifier;
			}
			else
			{
				_jumpSlopeSpeed = 0; //Means slope jump force won't be applied this jump
			}
			_Actions._jumpCount = 1; //Number of jumps set to 1, allowing for double jumps.
		}
		else
		{
			//Sets jump stats for this specific jump.
			_thisJumpSpeed = _doubleJumpSpeed_ * _jumpSpeedModifier;
			_thisMaxDuration = _doubleJumpDuration_.y * _jumpDurationModifier;
			_thisMinDuration = _doubleJumpDuration_.x * _jumpDurationModifier;
			_slopedJumpDuration = 0;

			_jumpSlopeSpeed = 0;

			_Actions._jumpCount = Mathf.Clamp(_Actions._jumpCount + 1, 2, _Actions._jumpCount + 1); //Track this new jump, and it must be 2 or higher to track grounded jump as being skipped.

			JumpInAir();
		}

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Jump);
		this.enabled = true;
	}

	public void StopAction (bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.

		enabled = false;

		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		_Actions._ActionDefault._animationAction = 0; //Ensures player will land properly in the correct animation when entering default action.
		_PlayerPhys._canBeGrounded = true;
		_JumpBall.SetActive(false);
	}

	//This has to be set up in Editor. The invoker is in the PlayerPhysics script component, adding this event to it will mean this is called whenever the player lands.
	public void EventOnGrounded() {
		_Actions._jumpCount = 0;
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
		_isJumpingFromGround = fromGround;
	}

	public void HandleInputs () {
		//Moving camera behind
		if (!_Actions._isPaused) _CamHandler.AttemptCameraReset();

		//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
		_Actions.HandleInputs(_positionInActionList);
	}

	//Additional effects if a jump is being made from in the air.
	private void JumpInAir () {

		//Take some horizontal speed on jump and remove vertical speed to ensure jump is an upwards force.
		Vector3 newVel = new Vector3(_PlayerPhys._coreVelocity.x * _speedLossOnDoubleJump_, Mathf.Max(_PlayerPhys._RB.velocity.y, 2), _PlayerPhys._coreVelocity.z * _speedLossOnDoubleJump_);
		_PlayerPhys.SetCoreVelocity(newVel, true, false);

		//Add particle effect during jump
		GameObject JumpDashParticleClone = Instantiate(_Tools.JumpDashParticle, _Tools.FeetPoint.position, Quaternion.identity) as GameObject;
		JumpDashParticleClone.transform.position = _Tools.FeetPoint.position;
		JumpDashParticleClone.transform.rotation = Quaternion.LookRotation(Vector3.up);

	}

	private void ApplyForce() {
		//Ending Jump Early
		if (!_Input.JumpPressed && _counter > _thisMinDuration && _isJumping)
		{
			EndJumpForce();
		}
		//Ending jump after max duration
		else if (_counter > _thisMaxDuration && _isJumping && _Input.JumpPressed)
		{
   			EndJumpForce();
		}
		//If no longer moving upwards, then there is probably something blocking the jump, so end it early.
		else if(_isJumping && _PlayerPhys.GetRelevantVel(_PlayerPhys._coreVelocity).y <= 0 && _counter > 0.2f)
		{
			EndJumpForce();
		}
		//If there are no interuptions, apply jump force.
		else if (_isJumping)
		{
			float modifierThisFrame = _JumpForceByTime_.Evaluate(_counter / _thisMaxDuration ); //Get a modifier to adjust jump force this frame based on how long has been jumping for out of maximum time.
			//Jump move at angle
			if (_counter < _slopedJumpDuration && _jumpSlopeSpeed > 0)
			{
				_PlayerPhys.AddCoreVelocity(_upwardsDirection * (_jumpSlopeSpeed * 0.95f * modifierThisFrame), false);
				_PlayerPhys.AddCoreVelocity(Vector3.up * (_jumpSlopeSpeed * 0.05f * modifierThisFrame), false); //Extra speed to ballance out direction
			}
			//Move straight up in world.
			else
			{
				_PlayerPhys.AddCoreVelocity(_upwardsDirection * (_thisJumpSpeed) * modifierThisFrame, false);
			}
		}
		//If jumping is over, the player can be grounded again, which will set them back to the default action.
		else
		{
			_PlayerPhys._canBeGrounded = true;
		}
	}

	//Called when the jump should stop applying force, but before exiting the state.
	private void EndJumpForce () {
		_counter = _thisMaxDuration;
		_isJumping = false;
		_Input.JumpPressed = false;
	}

	private void CheckShouldEndAction() {
		//End Action on landing. Has to have been in the air for some time first though to prevent immediately becoming grounded.
		if (_PlayerPhys._isGrounded && _counter > _slopedJumpDuration)
		{ 
			//Prevents holding jump to keep doing so forever.
			_Input.JumpPressed = false;

			_Actions._ActionDefault.StartAction();
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

	public void ReadyAction () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponentInParent<S_CharacterTools>();
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
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools.GetComponent<S_ActionManager>();
		_CamHandler = _Tools.CamHandler;
		_Input = _Tools.GetComponent<S_PlayerInput>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
		_JumpBall = _Tools.JumpBall;

	}

	//Responsible for assigning stats from the stats script.
	private void AssignStats () {
		_maxJumpTime_ = _Tools.Stats.JumpStats.jumpDuration.y;
		_minJumpTime_ = _Tools.Stats.JumpStats.jumpDuration.x;
		_startJumpSpeed_ = _Tools.Stats.JumpStats.jumpSpeed;
		_startSlopedJumpDuration_ = _Tools.Stats.JumpStats.startSlopedJumpDuration;
		_jumpSlopeConversion_ = _Tools.Stats.JumpStats.jumpSlopeConversion;

		_JumpForceByTime_ = _Tools.Stats.JumpStats.JumpForceByTime;

		_maxJumps_ = _Tools.Stats.MultipleJumpStats.maxJumpCount;
		_doubleJumpDuration_ = _Tools.Stats.MultipleJumpStats.doubleJumpDuration;
		_doubleJumpSpeed_ = _Tools.Stats.MultipleJumpStats.doubleJumpSpeed;

		_speedLossOnDoubleJump_ = _Tools.Stats.MultipleJumpStats.speedLossOnDoubleJump;
	}
	#endregion
}
