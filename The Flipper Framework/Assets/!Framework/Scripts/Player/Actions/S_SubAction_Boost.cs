using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

public class S_SubAction_Boost : MonoBehaviour, ISubAction
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
	private S_ActionManager       _Actions;
	private S_Handler_Camera      _CamHandler;
	private S_PlayerInput         _Input;
	private Transform             _MainSkin;
	private CapsuleCollider       _CharacterCapsule;
	#endregion

	//Stats
	#region Stats
	private float       _maxBoostEnergy_;

	private float       _boostSpeed_ = 120;
	#endregion

	// Trackers
	#region trackers
	private S_Enums.PrimaryPlayerStates _whatActionWasOn;
	private float       _currentBoostEnergy;

	private float                 _currentSpeed;
	private float                 _goalSpeed;

	private bool                  _inAStateThatCanBoost;
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools.GetComponent<S_ActionManager>();
		_MainSkin = _Tools.MainSkin;
		_CamHandler = _Tools.CamHandler;
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_CharacterCapsule = _Tools.CharacterCapsule.GetComponent<CapsuleCollider>();

		_Tools.GetComponent<S_Handler_HealthAndHurt>().onRingGet += EventGainEnergy;
	}

	// Update is called once per frame
	void Update () {

	}

	//Only called when enabled, but tracks the time of the quickstep and performs it until its up.
	private void FixedUpdate () {
		_inAStateThatCanBoost = false;
		ApplyBoost();
	}


	//Called when attempting to perform an action, checking and preparing inputs.
	public bool AttemptAction () {
		_inAStateThatCanBoost = true;

		if (_Input.BoostPressed)
		{
			if(!_PlayerPhys._isBoosting) { StartAction(); }
			return true; //This is called as long as the input is held because it will prevent other sub actions from being performed, even if not starting this action from scratch.
		}
		return false;
	}

	//Called when the action is enabled and readies all variables for it to be performed.
	public void StartAction () {
		_currentSpeed = _Actions._listOfSpeedOnPaths.Count > 0 ? _Actions._listOfSpeedOnPaths[0] : _PlayerPhys._horizontalSpeedMagnitude; //The boost speed will be set to and increase from either the running speed, or path speed if currently in use.
		_goalSpeed = _boostSpeed_; //This is how fast the boost will move, and speed will lerp towards it.

		StopCoroutine(LerpToGoalSpeed(0)); //If already in motion for a boost just before, this ends that calculation.
		StartCoroutine(LerpToGoalSpeed(20));

		_PlayerPhys._isBoosting = true;
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Called every frame and applies physics accordingly
	private void ApplyBoost () {
		if (_PlayerPhys._isBoosting)
		{
			if (!_Input.BoostPressed || !_inAStateThatCanBoost) { EndBoost(); } //Will end boost if released button or entered a state where boosting doesn't happen.

			_PlayerPhys.SetCoreVelocity(_MainSkin.forward * _currentSpeed, false);
			if (_Actions._listOfSpeedOnPaths.Count > 0) _Actions._listOfSpeedOnPaths[0] = _currentSpeed;

			_Input.RollPressed = false; //This will ensure the player won't crouch or roll and instead stay boosting.
		}
	}

	private void EndBoost ()
	{
		_PlayerPhys._isBoosting = false;
	}

	private IEnumerator LerpToGoalSpeed(float frames) {
		float segments = 1 / frames;
		float lerpValue = segments;
		float startSpeed = _currentSpeed;
		Debug.Log("STARTED THE BOOST COROUTINE");

		while (_currentSpeed < _goalSpeed)
		{
			_currentSpeed = Mathf.Lerp(startSpeed, _goalSpeed, lerpValue);
			lerpValue += segments;
			yield return new WaitForFixedUpdate();
		}
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	void EventGainEnergy ( object sender, float source ) {
		_currentBoostEnergy = Mathf.Min(_currentBoostEnergy + source, _maxBoostEnergy_);
	}

	#endregion
}
