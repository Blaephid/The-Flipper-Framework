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
	private Transform             _SkinOffset;
	private CapsuleCollider       _CharacterCapsule;

	private GameObject			_BoostCone;
	private MeshRenderer[]		_ListOfSubCones;
	#endregion

	//Stats
	#region Stats
	private bool        _hasAirBoost_;

	private float       _maxBoostEnergy_;
	private float       _energyDrainedEachFrame_;
	private float       _energyDrainedOnStart_;

	private float       _boostSpeed_ = 120;
	private float       _startBoostSpeed_ = 70;

	private int         _framesToReachBoostSpeed_ = 10;

	private float       _turnCharacterThreshold_ = 48;
	private float        _boostTurnSpeed_ = 2;
	private float        _faceTurnSpeed_ = 6;
	private float       _maximumTurnAngle_ = 90;

	private float       _boostCooldown_ = 0.5f;
	#endregion

	// Trackers
	#region trackers
	private S_Enums.PrimaryPlayerStates _whatActionWasOn;
	private float		_currentBoostEnergy;

	private float                 _currentSpeed;
	private float                 _goalSpeed;

	private bool                  _inAStateThatCanBoost;
	private bool                  _canStartBoost = true;

	private Vector3               _faceDirection;
	private Vector3               _faceDirectionWithOffset;
	private Vector3               _savedSkinDirection;          //Stores which way the character is facing according to this script, and if it isn't equal to the actual main skin, it means that's been changed externally, so should respond. This is used to change face direction if physics or paths happen.

	private bool                  _isStrafing;

	private Vector3               _trackMoveInput;
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Awake () {
		ReadyAction();

		// Get all renderer components representing sub boost cones.
		_ListOfSubCones = _BoostCone.GetComponentsInChildren<MeshRenderer>();

		ChangeAlphaOfCones(0.1f);
	}

	// Update is called once per frame
	void Update () {
		if (_PlayerPhys._isBoosting)
		{
			_Actions._ActionDefault.HandleAnimator(_Actions._ActionDefault._animationAction);
			
			_Actions._ActionDefault.SetSkinRotationToVelocity(10, _faceDirection, _faceDirectionWithOffset);
			_savedSkinDirection = _MainSkin.forward;
		}
	}

	//Only called when enabled, but tracks the time of the quickstep and performs it until its up.
	private void FixedUpdate () {
		ApplyBoost();
		_inAStateThatCanBoost = false; //Set to false at the end of every fixed frame, and if this is not set to true in the AttemptAction method, the boost will end.
	}


	//Called when attempting to perform an action, checking and preparing inputs.
	public bool AttemptAction () {
		_inAStateThatCanBoost = true; //This will lead to a back and forth with it being set to false every frame. This means as soon as this method stops being called, this will be false.

		if (_Input.BoostPressed)
		{
			if (!_PlayerPhys._isBoosting) {
				if(_canStartBoost) //Will still return true even if not applying the boost, this will prevent entering the wrong action when this is in cooldown.
				{
					StartAction();
				}
				return true; 
			}		
		}
		return false;
	}

	//Called when the action is enabled and readies all variables for it to be performed.
	public void StartAction () {
		_PlayerPhys._isBoosting = true;
		_canStartBoost = false;

		//Get speeds to boost at and reach
		_currentSpeed = _Actions._listOfSpeedOnPaths.Count > 0 ? _Actions._listOfSpeedOnPaths[0] : _PlayerPhys._horizontalSpeedMagnitude; //The boost speed will be set to and increase from either the running speed, or path speed if currently in use.
		_currentSpeed = Mathf.Max(_currentSpeed, _startBoostSpeed_); //Ensures will start from a noticeable speed, then increase to full boost speed.
		_goalSpeed = Mathf.Max(_boostSpeed_, Mathf.Min(_currentSpeed + 10, _PlayerPhys._currentMaxSpeed)); //This is how fast the boost will move, and speed will lerp towards it. It will either be boost speed, of if over that, a slight increase, not exceeding max speed.

		StopCoroutine(LerpToGoalSpeed(0)); //If already in motion for a boost just before, this ends that calculation, before starting a new one.
		StartCoroutine(LerpToGoalSpeed(_framesToReachBoostSpeed_));

		_faceDirection = _MainSkin.forward; //The direction to keep the player facing towards, will be changed in the turning script.
		_faceDirectionWithOffset = _faceDirection;

		//Control
		_PlayerPhys.CallAccelerationAndTurning = CustomTurningAndAcceleration; //Changing this delegate will change what method to call to handle turning from the default to the custom one in this script.

		_Actions._ActionDefault._isAnimatorControlledExternally = true; //This script will point the character manually.
		_isStrafing = true; //Will start by strafing, though this might be changed immediately depending on player input.

		_savedSkinDirection = _MainSkin.forward; //Keeps track of which way was facing when started, and if these stop being equal it means something external has affected the character's direction.

		//Effects
		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraPause(new Vector2 (12, 45))); //The camera will fall back before catching up.
		//Will make the boost effects fade in rather than appear instantly.
		StopCoroutine(SetBoostEffectVisibility(0, 0, 0));
		StartCoroutine(SetBoostEffectVisibility(0, 1, 8));
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
				EndBoost();  //Will end boost if released button or entered a state where boosting doesn't happen.
			}

			_currentSpeed = Mathf.Max(_currentSpeed, _PlayerPhys._horizontalSpeedMagnitude); //If running speed has been increased beyond boost speed (such as through slope physics) then factor that in so it isn't set over.
			//If running speed has been decreased by an external force AFTER boost speed was set last frame (such as by slope physics), then apply the difference to the boost speed.
			if(_PlayerPhys._horizontalSpeedMagnitude < _PlayerPhys._previousHorizontalSpeeds[1])
			{
				float difference = _PlayerPhys._horizontalSpeedMagnitude - _PlayerPhys._previousHorizontalSpeeds[1];
				_currentSpeed += difference;
			}

			//If speed is decreased beyond boost speed, then if on flat ground return to it.
			if(!_PlayerPhys._isCurrentlyOnSlope) { _currentSpeed = Mathf.Max(_currentSpeed, _goalSpeed); }

			//Apply speed
			_PlayerPhys.SetLateralSpeed(_currentSpeed, false); //Applies boost speed to movement.
			if (_Actions._listOfSpeedOnPaths.Count > 0) _Actions._listOfSpeedOnPaths[0] = _currentSpeed; //Sets speed on rails, or other actions following paths.

			//Remember that the turning method will be called by the delegate in PlayerPhysics, not here. 

			_Input.RollPressed = false; //This will ensure the player won't crouch or roll and instead stay boosting.
		}
	}

	private void EndBoost () {
		_PlayerPhys._isBoosting = false;

		//Controls
		_Input.BoostPressed = false;
		StartCoroutine(DelayBoostStart());

		//Physics
		_PlayerPhys.CallAccelerationAndTurning = _PlayerPhys.DefaultAccelerateAndTurn; //Changes the method delegated for calculating acceleration and turning back to the correct one.

		_Actions._ActionDefault._isAnimatorControlledExternally = false;

		//Effects
		StopCoroutine(SetBoostEffectVisibility(0, 0, 0));
		StartCoroutine(SetBoostEffectVisibility(1, 0.1f, 12));
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

	private IEnumerator DelayBoostStart () {
		yield return new WaitForSeconds(_boostCooldown_);
		_canStartBoost = true;
	}

	private IEnumerator SetBoostEffectVisibility(float startAlpha, float setAlpha, float frames) {
		if(setAlpha > 0.5f) { _BoostCone.SetActive(true); }

		float segments = 1 / frames;
		float lerpValue = startAlpha; 

		for (int i = 0; i < frames; i++)
		{
			lerpValue = Mathf.MoveTowards(lerpValue, setAlpha, segments);
			ChangeAlphaOfCones(lerpValue);
			yield return new WaitForFixedUpdate();
		}

		if (setAlpha <= 0.1f) { _BoostCone.SetActive(false); }
	}

	private void ChangeAlphaOfCones(float alpha ) {
		foreach (MeshRenderer r in _ListOfSubCones)
		{
			MaterialPropertyBlock TempBlock = new MaterialPropertyBlock();
			TempBlock.SetFloat("_Alpha_Modifier", alpha);
			r.SetPropertyBlock(TempBlock);
		}
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	public Vector3 CustomTurningAndAcceleration ( Vector3 lateralVelocity, Vector3 input, Vector2 modifier ) {
		if (_PlayerPhys._moveInput.sqrMagnitude < 0.1f) { _PlayerPhys._moveInput = _faceDirection; } //Ensures there will always be an input forwards if nothing else.

		// Normalize to get input direction and magnitude seperately. For efficency and to prevent larger values at angles, the magnitude is based on the higher input.
		Vector3 inputDirection = input.normalized;
		float inputMagnitude = Mathf.Max(Mathf.Abs(input.x), Mathf.Abs(input.z));

		_PlayerPhys._inputVelocityDifference = lateralVelocity.sqrMagnitude < 1 ? 0 : Vector3.Angle(_faceDirection, inputDirection); //The change in input in degrees, this will be used by the skid script to calculate whether should skid.
		float inputDifference = _PlayerPhys._inputVelocityDifference;

		//If inputting backwards, ignore turning, this gives the chance to perform a skid.
		if(_PlayerPhys._inputVelocityDifference > 150) 
		{ 
			inputDirection = _faceDirection;
			inputDifference = 0;
		}

		float turnModifier = HandleStrafeOrFullTurn(inputDirection, inputDifference, lateralVelocity);

		Vector3 useInput = Vector3.RotateTowards(_faceDirection, inputDirection, _maximumTurnAngle_ * Mathf.Deg2Rad, 0); //Limits how far can be turned.
		useInput = inputDirection;

		lateralVelocity = Vector3.RotateTowards(lateralVelocity, useInput, _boostTurnSpeed_  * turnModifier * Mathf.Deg2Rad, 0); //Applies turn onto velocity.

		Debug.DrawRay(transform.position, _faceDirection * 2, Color.red);
		//Debug.DrawRay(transform.position, _PlayerPhys._moveInput * 2, Color.magenta);

		//If the mainSkid direction has been changed when the saved hasn't, it means it's been done externally, such as by a path the player is moving along. So adjust the saved direction and facing direction used to align to.
		if(_savedSkinDirection != _MainSkin.forward)
		{
			_faceDirection = _MainSkin.forward;
			_savedSkinDirection = _MainSkin.forward;
		}
		return lateralVelocity;
	}

	private float HandleStrafeOrFullTurn(Vector3 inputDirection, float inputDifference, Vector3 lateralVelocity) {
		//Strafing will only happen when not turning with the camera, with a small enough angle.
		if (_Input.IsTurningBecauseOfCamera(inputDirection) || inputDifference > _turnCharacterThreshold_)
		{
			EndStrafing();
		}

		//Proper turning. If not strafing, then the face direction that controls player skin should rotate towards movement direction.
		if (!_isStrafing)
		{
			//Rotate face direction towards movement and remove offset. The mainskin will Rotate to these due to SetSkinRotation being called in Update.
			_faceDirection = Vector3.RotateTowards(_faceDirection, lateralVelocity, _faceTurnSpeed_ * Mathf.Deg2Rad, 0);
			_faceDirectionWithOffset = _faceDirection;

			Debug.Log(Vector3.Angle(_faceDirection, lateralVelocity.normalized) + "     " + Vector3.Angle(_MainSkin.forward, _faceDirection));

			//Strafing will only be possible once the turn has completed so velocity has reached input and the character has reached input.
			if (_faceDirection == lateralVelocity.normalized && Vector3.Angle(_MainSkin.forward, _faceDirection) < 2f)
			{
				_isStrafing = true;
			}

			return 1;
		}
		//Strafing 
		else
		{
			//Even if not turning, make the player skin slightly rotate right or left to show some turning(this could ideally be done with unique animations but we don't have access to that.
			Vector3 directionToTheRight = Vector3.Cross(_faceDirection, transform.up); //Get direction to right
			float rotateValue = Vector3.Angle(inputDirection, directionToTheRight) < 90 ? 1 : -1f; //If inputting to the right, rotate will rotate to the right, otherwise, will rotate to the opposite.
			rotateValue *= Mathf.Min(inputDifference, 40); //Prevents the offset from being more than x degrees.

			_faceDirectionWithOffset = Vector3.RotateTowards(_faceDirection, directionToTheRight, rotateValue * Mathf.Deg2Rad, 0); //Apply offset this frame by difference to main direction. This will be used when calling SetSkinRotation

			return 0.6f;
		}

		void EndStrafing() {
			if (_isStrafing)
			{
				//To prevent rotating inwards before outwards as the offset goes away. Set main rotation to offset, leading to no difference to the player's eye.
				_MainSkin.forward = _SkinOffset.forward;
				_SkinOffset.localRotation = Quaternion.identity;
			}

			_isStrafing = false;
		}
	}

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
		_Tools =		GetComponentInParent<S_CharacterTools>();
		_PlayerPhys =	_Tools.GetComponent<S_PlayerPhysics>();
		_Actions =	_Tools.GetComponent<S_ActionManager>();
		_CamHandler =	_Tools.CamHandler;
		_Input =		_Tools.GetComponent<S_PlayerInput>();

		_CharacterCapsule = _Tools.CharacterCapsule.GetComponent<CapsuleCollider>();
		_MainSkin =	_Tools.MainSkin;
		_SkinOffset =	_Tools.CharacterModelOffset;

		_BoostCone =	_Tools.BoostCone;
		_BoostCone.SetActive(false);

		_Tools.GetComponent<S_Handler_HealthAndHurt>().onRingGet += EventGainEnergy;
	}
	#endregion
}
