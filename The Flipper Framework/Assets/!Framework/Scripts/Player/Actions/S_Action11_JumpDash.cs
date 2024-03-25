using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

[RequireComponent(typeof(S_ActionManager))]
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
	private S_Handler_Camera	_CamHandler;
	private S_Control_SoundsPlayer	_Sounds;
	private S_Control_EffectsPlayer	_Effects;

	private Animator	_CharacterAnimator;
	private Transform   _MainSkin;
	private GameObject	_JumpBall;
	#endregion


	//Stats
	#region Stats
	private S_Enums.JumpDashType _WhatType_;

	private float	_airDashSpeed_;
	private float       _airDashIncrease_;
	private float       _turnSpeed_;

	private float	_maxDuration_;
	private float	_minDuration_;

	private float       _verticalAngle_;
	private float	_horizontalAngle_;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;         //In every action script, takes note of where in the Action Managers Main action list this script is. 

	public float	_skinRotationSpeed;

	private float	_timer;		//Tracks how long has been in this action
	private float	_dashSpeed;	//Generated at start of action, based on player speed and stats.
	private Vector3	_dashDirection;	//Generated at start of action, based on input, stats and movement.
	private float	_downwardsDirection;	//Generated at start of action, based on input, stats and movement.
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
	}

	// Update is called once per frame
	void Update () {
		//Set Animator Parameters
		_Actions.ActionDefault.HandleAnimator(11);

		_Actions.ActionDefault.SetSkinRotationToVelocity(_skinRotationSpeed);
	}

	private void FixedUpdate () {
		_timer += Time.deltaTime;

		HandleMovement();
		CheckTimer();
	}

	public bool AttemptAction () {
		bool willChangeAction = false;

		switch(_Actions.whatAction)
		{
			//Regular requires a seperate check in addition to other actions.
			case S_Enums.PrimaryPlayerStates.Default:
				if (_Actions.ActionDefault._canDashDuringFall_)
				{
					CheckDash();
				}
				break;
			default:
				CheckDash();
				break;
		}
		return willChangeAction;

		//This is called no matter the action, so it used as function to check the always relevant data.
		void CheckDash() {
			//Can't be grounded or have the action locked by external means.
			if (!_PlayerPhys._isGrounded && !_Actions.lockJumpDash && _Actions._isAirDashAvailables && _Input.SpecialPressed)
			{
				StartAction();
				willChangeAction = true; //Used because returning here would just return to the main method, not the caller.
			}
		}
	}

	public void StartAction () {

		//Effects
		_Sounds.AirDashSound();
		_HomingTrailScript.emitTime = _maxDuration_ + 0.5f;
		_HomingTrailScript.emit = true;

		_JumpBall.SetActive(false);
		_Effects.AirDashParticle();

		_CharacterAnimator.SetTrigger("ChangedState");
		_Actions.ActionDefault.SwitchSkin(true);

		//Control
		_Input.HomingPressed = false;
		_Actions._isAirDashAvailables = false; //Can't be used again until this is true

		//Disable normal control so it's taken care of here
		_PlayerPhys._listOfCanControl.Add(false);
		_PlayerPhys._isGravityOn = false;

		//Set private
		_timer = 0;

		//Creative vector to move in
		_dashSpeed = Mathf.Max(_PlayerPhys._horizontalSpeedMagnitude +  _airDashIncrease_, _airDashSpeed_);

		MakeFullTurn();

		switch (_WhatType_)
		{
			case S_Enums.JumpDashType.Push:
				_timer = _maxDuration_;
				break;
		}

		Vector3 newVel = _dashDirection * _dashSpeed;
		newVel.y = 0;
		_PlayerPhys.SetCoreVelocity(newVel, true);

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.JumpDash);
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.

		enabled = false;

		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		//Inputs
		_Input.SpecialPressed = false;

		//Physics
		_PlayerPhys._listOfCanControl.RemoveAt(0);
		_PlayerPhys._isGravityOn = true;
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
		if(_timer < 0.01f)
		{
			MakeFullTurn() ;
		}
		//Rotate to the right or left based on horizontal input
		else if(_timer > 0.03f)
		{
			float inputMag = Mathf.Max(Mathf.Abs(_PlayerPhys._moveInput.x), Mathf.Abs(_PlayerPhys._moveInput.y));

			Vector3 directionToRotate = _PlayerPhys._moveInput;
			if(Vector3.Angle(_PlayerPhys._moveInput, _dashDirection) > 80)
			{
				directionToRotate = Vector3.Angle(_PlayerPhys._moveInput, _MainSkin.right) < 90 ? _MainSkin.right : -_MainSkin.right;
			}
			_dashDirection = Vector3.RotateTowards(_dashDirection, directionToRotate, inputMag * _turnSpeed_ * Time.deltaTime, 0f);
		}
		//Slowly rotate to face downwards, though gravity is not being applied.
		_downwardsDirection = Mathf.MoveTowards(_downwardsDirection, -1, 0.02f);

		Vector3 newVec = _dashDirection.normalized * _dashSpeed;
		newVec += _MainSkin.up * _downwardsDirection;

		_PlayerPhys.SetCoreVelocity(newVec);
	}

	private void CheckTimer () {
		//End dash if at max time, min time but let go of button, or grounded.
		if (_timer > _maxDuration_)
		{
			_Actions.ActionDefault.StartAction();
		}
		else if(_timer > _minDuration_ && _Input.SpecialPressed)
		{
			_Actions.ActionDefault.StartAction();
		}
		else if (_PlayerPhys._isGrounded)
		{
			_Actions.ActionDefault.StartAction();
		}
	}

	//Change _dashDirection on a large scale, rather than small turns adding up over time.
	private void MakeFullTurn () {

		_dashDirection = _MainSkin.forward;
		_dashDirection.y = 0;
		_downwardsDirection = 0;

		return;

		//Aiming Vertically
		_downwardsDirection = Mathf.MoveTowards(_downwardsDirection, 1, _verticalAngle_ / 180);

		//Aiming Horizontally
		Vector3 input = _PlayerPhys._moveInput;
		_dashDirection = Vector3.RotateTowards(_dashDirection, _PlayerPhys.AlignWithNormal(input, _MainSkin.up, 1), _horizontalAngle_, 0); //This will cause the player to dash in input direction (relevant to current character up), with a determined max angle

		Debug.DrawRay(transform.position, _PlayerPhys.AlignWithNormal(input, _MainSkin.up, 10), Color.red, 20);
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
			_Tools = GetComponent<S_CharacterTools>();
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
		_Input =		GetComponent<S_PlayerInput>();
		_PlayerPhys =	GetComponent<S_PlayerPhysics>();
		_Actions =	GetComponent<S_ActionManager>();
		_CamHandler =	GetComponent<S_Handler_Camera>();
		_Sounds =			_Tools.SoundControl;
		_Effects =		_Tools.EffectsControl;

		_CharacterAnimator =	_Tools.CharacterAnimator;
		_MainSkin =		_Tools.mainSkin;
		_HomingTrailScript =	_Tools.HomingTrailScript;
		_JumpBall =		_Tools.JumpBall;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_airDashSpeed_ =		_Tools.Stats.JumpDashStats.dashSpeed;
		_airDashIncrease_ =		_Tools.Stats.JumpDashStats.dashIncrease;
		_turnSpeed_ =		_Tools.Stats.JumpDashStats.turnSpeed;

		_maxDuration_ =		_Tools.Stats.JumpDashStats.maxDuration;
		_minDuration_ =		_Tools.Stats.JumpDashStats.minDuration;

		_WhatType_ =		_Tools.Stats.JumpDashStats.behaviour;

		_verticalAngle_ =		_Tools.Stats.JumpDashStats.verticalAngle;
		_horizontalAngle_ =		_Tools.Stats.JumpDashStats.horizontalAngle;
	}
	#endregion

}
