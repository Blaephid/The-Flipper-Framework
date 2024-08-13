using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

public class S_Action11_JumpDash : MonoBehaviour, IMainAction
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
	private S_VolumeTrailRenderer _HomingTrailScript;
	private S_Handler_Camera      _CamHandler;
	private S_Control_SoundsPlayer          _Sounds;
	private S_Control_EffectsPlayer         _Effects;

	private Animator    _CharacterAnimator;
	private Transform   _MainSkin;
	private GameObject  _JumpBall;
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private S_Enums.JumpDashTypes _WhatType_;

	private float       _airDashSpeed_;
	private float       _airDashIncrease_;
	private float       _turnSpeed_;

	private float       _speedAfterDash_;
	private float       _framesToSpendChangingSpeed_;

	private float       _maxDuration_;
	private float       _minDuration_;

	private float       _verticalAngle_;
	private float       _horizontalAngle_;

	private float       _faceDownwardsSpeed_ = 0.02f;
	private float       _maxDownwardsSpeed_  = -5f;

	private int         _lockMoveInputOnStart_ = 0;
	private int         _lockMoveInputOnEnd_ = 10;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;         //In every action script, takes note of where in the Action Managers Main action list this script is. 

	public float        _skinRotationSpeed;

	private float       _timer;             //Tracks how long has been in this action
	private float       _dashSpeed;         //Generated at start of action, based on player speed and stats.
	private Vector3     _dashDirection;     //Generated at start of action, based on input, stats and movement.
	private float       _upwardsSpeed;          //Generated at start of action, based on input, stats and movement.

	private Vector3     _input;
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
		//Set Animator Parameters and rotation
		_Actions._ActionDefault.HandleAnimator(11);
		_Actions._ActionDefault.SetSkinRotationToVelocity(_skinRotationSpeed);
	}

	private void FixedUpdate () {
		_timer += Time.deltaTime;

		HandleMovement();
		CheckTimer();
	}

	public bool AttemptAction () {
		bool willChangeAction = false;

		switch (_Actions._whatAction)
		{
			//Regular requires a seperate check in addition to other actions.
			case S_Enums.PrimaryPlayerStates.Default:
				if (_Actions._ActionDefault._canDashDuringFall_)
				{
					if (CheckDash())
					{
						SetStartDirection(_MainSkin.forward);
						StartAction();
					}
				}
				break;
			case S_Enums.PrimaryPlayerStates.WallClimbing:
				if (CheckDash())
				{
					StartCoroutine(_CamHandler._HedgeCam.KeepGoingBehindCharacterForFrames(30, 8, 0, true));
					SetStartDirection(_Actions._dashAngle);
					StartAction();
				}
				break;
			case S_Enums.PrimaryPlayerStates.WallRunning:
				if (CheckDash())
				{
					SetStartDirection(_Actions._dashAngle);
					StartAction();
				}
				break;
			default:
				if (CheckDash())
				{
					SetStartDirection(_MainSkin.forward);
					StartAction();
				}
				break;
		}
		return willChangeAction;

		//This is called no matter the action, so it used as function to check the always relevant data.
		bool CheckDash () {
			//Can't be grounded or have the action locked by external means.
			willChangeAction = !_PlayerPhys._isGrounded && _Actions._areAirActionsAvailable && _Actions._isAirDashAvailables && _Input._SpecialPressed;
			return willChangeAction;
		}
	}

	public void StartAction () {

		//Effects
		_Sounds.AirDashSound();
		_HomingTrailScript.emitTime = _maxDuration_ + 0.5f;
		_HomingTrailScript.emit = true;

		_JumpBall.SetActive(false);
		_Effects.AirDashParticle();

		_CharacterAnimator.SetInteger("Action", 11);
		_CharacterAnimator.SetTrigger("ChangedState");
		_Actions._ActionDefault.SwitchSkin(true);

		//Control
		_Input._HomingPressed = false;
		_Actions._isAirDashAvailables = false; //Can't be used again until this is true

		_PlayerPhys._canStickToGround = false; //Prevents the  landing following the ground direction, converting fall speed to running speed.

		//Disable normal control so it's taken care of here
		_PlayerPhys._listOfCanControl.Add(false);
		_PlayerPhys._listOfIsGravityOn.Add(false);

		//Set private
		_timer = 0;

		//Create vector to move in
		_dashSpeed = Mathf.Max(_PlayerPhys._currentRunningSpeed + _airDashIncrease_, _airDashSpeed_); //Speed increased with a minimum.

		//Rotate right or left on a large scale based on input
		MakeFullTurn();

		switch (_WhatType_)
		{
			case S_Enums.JumpDashTypes.Push:
				_timer = _maxDuration_;
				break;
		}

		_Input.LockInputForAWhile(_lockMoveInputOnStart_, false, _dashDirection);

		Vector3 newVec = _dashDirection * _dashSpeed; //Dash forwards
		newVec += _MainSkin.up * _upwardsSpeed; //If has upwards force, add that as well.
		_PlayerPhys.SetCoreVelocity(newVec, "Overwrite"); //Move in dash direction
		_PlayerPhys.RemoveEnvironmentalVelocityAirAction(); //If environmental action set to be removed on air action, then remove.

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.JumpDash);
		this.enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		//Inputs
		_Input._SpecialPressed = false;

		//Physics
		_PlayerPhys._listOfCanControl.RemoveAt(0);
		_PlayerPhys._listOfIsGravityOn.RemoveAt(0);
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	public void HandleInputs () {
		//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
		_Actions.HandleInputs(_positionInActionList);
	}

	private void HandleMovement () {

		//To make the big turn performable by humans, adds a time it can still be performed.
		if (_timer < 0.01f)
		{
			MakeFullTurn();
		}
		else if (_timer > 0.03f)
		{
			//Input based on if stick is pushed more vertically or horizontally
			float inputMag = Mathf.Max(Mathf.Abs(_PlayerPhys._moveInput.x), Mathf.Abs(_PlayerPhys._moveInput.z));

			//Get direction to rotate towards. To avoid rotating down and under, then if over ninety then go right or left.
			Vector3 _input = transform.TransformDirection(_PlayerPhys._moveInput);
			if (Vector3.Angle(_input, _dashDirection) > 80)
			{
				_input = Vector3.Angle(_PlayerPhys._moveInput, _MainSkin.right) < 90 ? _MainSkin.right : -_MainSkin.right;
			}

			//Rotate from current direction to new one, based on input and stats
			_dashDirection = Vector3.RotateTowards(_dashDirection, _input, inputMag * _turnSpeed_ * Time.deltaTime, 0f);
		}
		//Since gravity is not being applied, use this to slowly aim more downwards.
		_upwardsSpeed = Mathf.MoveTowards(_upwardsSpeed, _maxDownwardsSpeed_, _faceDownwardsSpeed_);

		//Build and set velocity.
		Vector3 newVec = _dashDirection.normalized * _dashSpeed;
		newVec += _MainSkin.up * _upwardsSpeed;
		_PlayerPhys.SetCoreVelocity(newVec);
	}

	private void CheckTimer () {
		//End dash if at max time, min time but let go of button, or grounded.
		if (_timer > _maxDuration_)
		{
			EndDashManually();
		}
		else if (_timer > _minDuration_ && !_Input._SpecialPressed)
		{
			EndDashManually();
		}
		else if (_PlayerPhys._isGrounded)
		{
			EndDashManually();
		}
	}

	//Called when the dash has finished (seperate from stop action because this won't be called on interuptions like hitting a rail)
	private void EndDashManually () {
		_Input.LockInputForAWhile(_lockMoveInputOnEnd_, false, _MainSkin.forward);

		StartCoroutine(ChangeSpeedSmoothly(_speedAfterDash_));

		_Actions._ActionDefault.StartAction();
	}

	//Over several frames will add velocity in increments adding up to the total change.
	private IEnumerator ChangeSpeedSmoothly ( float newSpeed ) {
		float increments = newSpeed / _framesToSpendChangingSpeed_; //Gets the increments to change speed in.

		//For the number of frames the stat has split it over.
		for (int i = 0 ; i < _framesToSpendChangingSpeed_ ; i++)
		{
			yield return new WaitForFixedUpdate();

			if (_PlayerPhys._horizontalSpeedMagnitude > newSpeed) { break; } //If something has changed (like hitting a wall), then ignore this.
			else
			{
				_PlayerPhys.AddCoreVelocity(_PlayerPhys._coreVelocity.normalized * increments); //Depending on increments, will either increase speed or decrease it as it goes.
			}
		}
	}

	private void SetStartDirection ( Vector3 forward ) {
		_dashDirection = forward;
		_dashDirection.y = 0;
	}

	//Change _dashDirection on a large scale, rather than small turns adding up over time.
	private void MakeFullTurn () {

		//Aiming Vertically
		_upwardsSpeed = _verticalAngle_;

		//Aiming Horizontally
		_input = transform.TransformDirection(_PlayerPhys._moveInput);
		_dashDirection = Vector3.RotateTowards(_dashDirection, _PlayerPhys.AlignWithNormal(_input, _MainSkin.up, 1), _horizontalAngle_ * Mathf.Deg2Rad, 0); //This will cause the player to dash in input direction (relevant to current character up), with a determined max angle

		_MainSkin.forward = _dashDirection;
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//This has to be set up in Editor. The invoker is in the PlayerPhysics script component, adding this event to it will mean this is called whenever the player lands.
	public void EventOnGrounded () {
		_Actions._isAirDashAvailables = true;
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//Assigns all external elements of the action.
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
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.JumpDash)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools._ActionManager;
		_CamHandler = _Tools.CamHandler;
		_Sounds = _Tools.SoundControl;
		_Effects = _Tools.EffectsControl;

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.MainSkin;
		_HomingTrailScript = _Tools.HomingTrailScript;
		_JumpBall = _Tools.JumpBall;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_airDashSpeed_ = _Tools.Stats.JumpDashStats.dashSpeed;
		_airDashIncrease_ = _Tools.Stats.JumpDashStats.dashIncrease;
		_turnSpeed_ = _Tools.Stats.JumpDashStats.turnSpeed;

		_maxDuration_ = _Tools.Stats.JumpDashStats.maxDuration;
		_minDuration_ = _Tools.Stats.JumpDashStats.minDuration;

		_WhatType_ = _Tools.Stats.JumpDashStats.behaviour;

		_verticalAngle_ = _Tools.Stats.JumpDashStats.forceUpwards;
		_horizontalAngle_ = _Tools.Stats.JumpDashStats.horizontalAngle;

		_faceDownwardsSpeed_ = _Tools.Stats.JumpDashStats.faceDownwardsSpeed;
		_maxDownwardsSpeed_ = _Tools.Stats.JumpDashStats.maxDownwardsSpeed;

		_lockMoveInputOnStart_ = _Tools.Stats.JumpDashStats.lockMoveInputOnStart;
		_lockMoveInputOnEnd_ = _Tools.Stats.JumpDashStats.lockMoveInputOnEnd;
		_speedAfterDash_ = _Tools.Stats.JumpDashStats.speedAfterDash;
		_framesToSpendChangingSpeed_ = _Tools.Stats.JumpDashStats.framesToChangeSpeed;
	}
	#endregion

}
