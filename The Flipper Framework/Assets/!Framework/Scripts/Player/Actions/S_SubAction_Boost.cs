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

	private GameObject            _BoostCone;
	#endregion

	//Stats
	#region Stats
	private float       _maxBoostEnergy_;

	private float       _boostSpeed_ = 120;
	private float       _startBoostSpeed_ = 70;

	private int         _framesToReachBoostSpeed_ = 10;
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
		ReadyAction();
		StartCoroutine(CheckState());
	}

	// Update is called once per frame
	void Update () {

	}

	//Only called when enabled, but tracks the time of the quickstep and performs it until its up.
	private void FixedUpdate () {
		Debug.Log("CHECK AND  "  +_Actions._whatAction);
		ApplyBoost();
	}


	//Called when attempting to perform an action, checking and preparing inputs.
	public bool AttemptAction () {
		_inAStateThatCanBoost = true; //This will lead to a back and forth with it being set to false every frame. This means as soon as this method stops being called, this will be false.
		Debug.Log("YES");

		if (_Input.BoostPressed)
		{
			if (!_PlayerPhys._isBoosting) { StartAction(); }
			return true; //This is called as long as the input is held because it will prevent other sub actions from being performed, even if not starting this action from scratch.
		}
		return false;
	}

	//Called when the action is enabled and readies all variables for it to be performed.
	public void StartAction () {
		_PlayerPhys._isBoosting = true;
		Debug.Log("Start Boost");

		//Get speeds to boost at and reach
		_currentSpeed = _Actions._listOfSpeedOnPaths.Count > 0 ? _Actions._listOfSpeedOnPaths[0] : _PlayerPhys._horizontalSpeedMagnitude; //The boost speed will be set to and increase from either the running speed, or path speed if currently in use.
		_currentSpeed = Mathf.Max(_currentSpeed, _startBoostSpeed_); //Ensures will start from a noticeable speed, then increase to full boost speed.
		_goalSpeed = Mathf.Max(_boostSpeed_, _currentSpeed); //This is how fast the boost will move, and speed will lerp towards it.

		StopCoroutine(LerpToGoalSpeed(0)); //If already in motion for a boost just before, this ends that calculation, before starting a new one.
		StartCoroutine(LerpToGoalSpeed(_framesToReachBoostSpeed_));

		//Effects
		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraPause(10));
		_BoostCone.SetActive(true);
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
			if (!_Input.BoostPressed || !_inAStateThatCanBoost) 
			{ 
				EndBoost(); 
			} //Will end boost if released button or entered a state where boosting doesn't happen.

			_PlayerPhys.SetLateralSpeed(_currentSpeed, false); //Applies boost speed to movement.
			if (_Actions._listOfSpeedOnPaths.Count > 0) _Actions._listOfSpeedOnPaths[0] = _currentSpeed; //Sets speed on rails, or other actions following paths.

			if(_Input._move.sqrMagnitude < 0.1f) { _Input._move = _MainSkin.forward; } //Ensures there will always be an input forwards if nothing else.

			_Input.RollPressed = false; //This will ensure the player won't crouch or roll and instead stay boosting.
		}
	}

	private void EndBoost () {
		Debug.Log("END Boost");
		_PlayerPhys._isBoosting = false;
		_Input.BoostPressed = false;

		_BoostCone.SetActive(false);
	}

	//The following boolean is set to true whenever AttemptAction is called, which means the current action can boost. Every x seconds this will be set to false so states that don't call attemptAction are states that can't boost.
	private IEnumerator CheckState () {
		while (true)
		{
			yield return new WaitForSeconds(0.2f);
			yield return new WaitForFixedUpdate();
			Debug.Log("NO");
			_inAStateThatCanBoost = false;
		}
	}

	//Lerps towards boost speed rather than change speed instantly. maxFrames is how many frames it will take to reach this from a speed less than the startBoostSpees stat.
	private IEnumerator LerpToGoalSpeed ( float maxFrames ) {
		float segments = 1 / maxFrames; //Gets the value to increase lerp by each frame.
		float lerpValue = segments; //The start lerp value from the first point along segments.
		float startSpeed = _startBoostSpeed_; //How fast was moving at.

		if (_currentSpeed > startSpeed)
		{
			lerpValue = (_currentSpeed - _startBoostSpeed_) / (_goalSpeed - _startBoostSpeed_); //If started boosting from over start boost speed, increase the lerp value to match how far along the lerp is. This will decrease how many frames it will take.
		}

		//Will lerp towards the boost speed and end the loop as soon as it reaches it.
		while (_currentSpeed < _goalSpeed)
		{
			_currentSpeed = Mathf.Lerp(startSpeed, _goalSpeed, lerpValue);
			_currentSpeed = Mathf.Min(_goalSpeed, _currentSpeed); //Ensures that no matter the lerp value, current speed will not exceed speed set for this boost.
			lerpValue += segments; //Goes up according to number of frames.
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
		}
	}

	private void AssignStats () {
		
	}

	private void AssignTools () {
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools.GetComponent<S_ActionManager>();
		_CamHandler = _Tools.CamHandler;
		_Input = _Tools.GetComponent<S_PlayerInput>();

		_CharacterCapsule = _Tools.CharacterCapsule.GetComponent<CapsuleCollider>();
		_MainSkin = _Tools.MainSkin;
		_BoostCone = _Tools.BoostCone;
		_BoostCone.SetActive(false);

		_Tools.GetComponent<S_Handler_HealthAndHurt>().onRingGet += EventGainEnergy;
	}
	#endregion
}
