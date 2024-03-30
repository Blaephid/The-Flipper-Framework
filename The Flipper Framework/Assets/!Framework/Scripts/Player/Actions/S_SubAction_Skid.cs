using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class S_SubAction_Skid : MonoBehaviour, ISubAction
{



	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_PlayerPhysics       _PlayerPhys;
	private S_CharacterTools      _Tools;
	private S_PlayerInput         _Input;
	private S_Control_SoundsPlayer _Sounds;
	private S_ActionManager       _Actions;

	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	[HideInInspector]
	public float        _regularSkidAngleStartPoint_;
	[HideInInspector]
	public float        _regularSkiddingIntensity_;
	private float       _airSkiddingIntensity_;
	private float       _spinSkidAngleStartPoint_;
	private bool        _canSkidInAir_;
	private int         _speedToStopAt_;
	private bool        _shouldSkiddingDisableTurning_;
	#endregion

	// Trackers
	#region trackers
	public bool         _isSkidding;
	private S_Enums.PrimaryPlayerStates _whatCurrentAction;
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
		AssignTools();
		AssignStats();
	}

	// Update is called once per frame
	void Update () {

	}

	private void FixedUpdate () {
		if(_whatCurrentAction != _Actions._whatAction) 
		{ 
			StopAction();
			enabled = false;
		}

	}

	//Called when attempting to perform an action, checking and preparing inputs.
	public bool AttemptAction () {
		bool willStartAction = false;
		_whatCurrentAction = _Actions._whatAction;

		//Different actions require different skids, even though they all call this function.
		switch (_Actions._whatAction)
		{
			case S_Enums.PrimaryPlayerStates.Default:
				if (_PlayerPhys._isGrounded)
				{
					willStartAction = TryRegularSkid();
				}
				else
				{
					willStartAction = TryJumpSkid();
				}
				break;

			case S_Enums.PrimaryPlayerStates.Jump:
				willStartAction = TryJumpSkid();
				break;

			case S_Enums.PrimaryPlayerStates.SpinCharge:
				willStartAction = TrySpinSkid();
				break;
			case S_Enums.PrimaryPlayerStates.Homing:
				willStartAction = _Actions._ObjectForActions.GetComponent<S_Action02_Homing>().TryHomingSkid();
				break;

		}
		return willStartAction;
	}

	//Called when skidding is started and _isSkiddin is used to prevent the sound being played multiple times.
	public void StartAction() {
		if (!_isSkidding)
		{
			_Actions._whatSubAction = S_Enums.SubPlayerStates.Skidding;
			if (!enabled) { enabled = true; }

			_Sounds.SkiddingSound();

			_Tools.CharacterAnimator.SetBool("Skidding", true);
			_isSkidding = true;
			if (_shouldSkiddingDisableTurning_) { _PlayerPhys._listOfCanTurns.Add(false); }
		}
	}

	//Called when skidding has to stop. If they were skidding when this was called, undo any changes skidding did.
	public void StopAction() {
		if (_isSkidding)
		{
			_Tools.CharacterAnimator.SetBool("Skidding", false);
			_isSkidding = false;
			if (_shouldSkiddingDisableTurning_) { _PlayerPhys._listOfCanTurns.Remove(false); }
		}
	}


	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Skidding when in the regular state and on the ground.
	private bool TryRegularSkid () {

		//If the input direction is different enough to the current movement direction, then a skid should be performed.
		if (_PlayerPhys._inputVelocityDifference > _regularSkidAngleStartPoint_ && !_Input._isInputLocked)
		{

			if(_PlayerPhys._horizontalSpeedMagnitude > -_regularSkiddingIntensity_ || _isSkidding)
			{
				StartAction();

				//If under a certain speed, stop immediately.
				if (_PlayerPhys._horizontalSpeedMagnitude < _speedToStopAt_)
				{
					_PlayerPhys.AddCoreVelocity(-_PlayerPhys._RB.velocity.normalized * _PlayerPhys._horizontalSpeedMagnitude * 1.5f);
					StopAction();
				}
				//Add force against the character to slow them down.
				else
				{
					_PlayerPhys.AddCoreVelocity(_PlayerPhys._RB.velocity.normalized * _regularSkiddingIntensity_ * (_PlayerPhys._isRolling ? 0.5f : 1));
				}
				return true;
			}	
		}
		StopAction();
		return false;
	}

	//Skidding when in the air.
	private bool TryJumpSkid () {

		if(!_canSkidInAir_) { return false; }

		//If the input direction is different enough to the current movement direction, then a skid should be performed. 
		if ((_PlayerPhys._inputVelocityDifference > _regularSkidAngleStartPoint_) && !_Input._isInputLocked)
		{
			//Uses relevant velocity rather whan world in order to not skid against vertical speed from jumping or falling.
			Vector3 releVel = _PlayerPhys.GetRelevantVel(_PlayerPhys._RB.velocity);
			
				if (_PlayerPhys._horizontalSpeedMagnitude < _speedToStopAt_)
				{
					_PlayerPhys.AddCoreVelocity(new Vector3(releVel.x, 0f, releVel.z).normalized * _PlayerPhys._horizontalSpeedMagnitude * 0.95f);
				}
				else
				{
					_PlayerPhys.AddCoreVelocity(new Vector3(releVel.x, 0f, releVel.z).normalized * _airSkiddingIntensity_ * (_PlayerPhys._isRolling ? 0.5f : 1));
				}
			return true;
		}
		//In case the player lost ground but was skidding before doing so.
		StopAction();
		return false;
	}

	//Skidding when charging a spin charge.
	private bool TrySpinSkid () {
		
			//Different start point from the other two skid types.
		if (_PlayerPhys._inputVelocityDifference > _spinSkidAngleStartPoint_ && !_Input._isInputLocked)
		{
			_PlayerPhys.AddCoreVelocity(_PlayerPhys._RB.velocity.normalized * _regularSkiddingIntensity_ * 0.6f);
			return true;
		}
		return false;
	}
	#endregion


	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	private void AssignTools() {
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_Sounds = _Tools.SoundControl;
		_Actions = _Tools.GetComponent<S_ActionManager>();
	}

	private void AssignStats() {
		_regularSkiddingIntensity_ = _Tools.Stats.SkiddingStats.skiddingIntensity;
		_airSkiddingIntensity_ = _Tools.Stats.SkiddingStats.skiddingIntensity;
		_canSkidInAir_ = _Tools.Stats.SkiddingStats.canSkidInAir;
		_regularSkidAngleStartPoint_ = _Tools.Stats.SkiddingStats.angleToPerformSkid;
		_spinSkidAngleStartPoint_ = _Tools.Stats.SkiddingStats.angleToPerformSpinSkid;
		_speedToStopAt_ = (int)_Tools.Stats.SkiddingStats.speedToStopAt;
		_shouldSkiddingDisableTurning_ = _Tools.Stats.SkiddingStats.shouldSkiddingDisableTurning;
	}
	#endregion
}
