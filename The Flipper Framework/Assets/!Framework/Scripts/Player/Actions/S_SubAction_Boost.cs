using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;

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

	private S_UI_Boost		_BoostUI;
	#endregion

	//Stats
	#region Stats
	private bool        _hasAirBoost_;
	private float	_boostFramesInAir_ = 60;
	private float       _AngleOfAligningToEndBoost_ = 85f;

	private bool        _gainEnergyFromRings_ = true;
	private bool        _gainEnergyOverTime_ = false;
	private float       _energyGainPerSecond_ = 5;
	private float       _energyGainPerRing_ = 5;

	private float       _maxBoostEnergy_ = 100;
	private float       _energyDrainedPerSecond_ = 0;
	private float       _energyDrainedOnStart_ = 0;

	private float       _startBoostSpeed_ = 70;
	private int         _framesToReachBoostSpeed_ = 5;

	private float       _boostSpeed_ = 120;
	private float       _maxSpeedWhileBoosting_ = 180;
	private float       _regainBoostSpeed_ = 10;

	private float       _speedLostOnEndBoost_ = 15;
	private int	_framesToLoseSpeed_ = 25;

	private float        _turnCharacterThreshold_ = 48;
	private float        _boostTurnSpeed_ = 2;
	private float        _faceTurnSpeed_ = 6;

	private float       _boostCooldown_ = 0.5f;

	private Vector2     _cameraPauseEffect_ = new Vector2(3, 40);
	#endregion

	// Trackers
	#region trackers
	private S_Enums.PrimaryPlayerStates _whatActionWasOn;
	private float		_currentBoostEnergy = 10;

	private float                 _currentSpeed;
	private float                 _currentSpeedLerpingTowardsGoal;
	private float                 _goalSpeed;

	private bool                  _inAStateThatCanBoost;
	private bool                  _canStartBoost = true;
	private bool                  _canBoostBecauseGround = true;

	private Vector3               _faceDirection;
	private Vector2               _faceDirectionOffset;
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
		_BoostUI.EnergyTracker.text = _currentBoostEnergy.ToString();

		//If currently enabled, then apply rotation and animations to character.
		if (_PlayerPhys._isBoosting)
		{
			_Actions._ActionDefault.HandleAnimator(_Actions._ActionDefault._animationAction); // Means animation will reflect jumping or being grounded.;
			
			_Actions._ActionDefault.SetSkinRotationToVelocity(10, _faceDirection, _faceDirectionOffset);
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
				if(_canStartBoost && _currentBoostEnergy > _energyDrainedOnStart_ && _canBoostBecauseGround)
				{
					StartAction();
				}
				return true; //Will still return true even if not applying the boost, this will prevent entering the wrong action when this is in cooldown.
			}		
		}
		return false;
	}

	//Called when the action is enabled and readies all variables for it to be performed.
	public void StartAction () {
		_PlayerPhys._isBoosting = true;
		_canStartBoost = false;

		_currentBoostEnergy -= _energyDrainedOnStart_;

		//Get speeds to boost at and reach
		_currentSpeed = _Actions._listOfSpeedOnPaths.Count > 0 ? _Actions._listOfSpeedOnPaths[0] : _PlayerPhys._horizontalSpeedMagnitude; //The boost speed will be set to and increase from either the running speed, or path speed if currently in use.
		_currentSpeed = Mathf.Max(_currentSpeed, _startBoostSpeed_); //Ensures will start from a noticeable speed, then increase to full boost speed.
		_goalSpeed = Mathf.Max(_boostSpeed_, Mathf.Min(_currentSpeed + 10, _PlayerPhys._currentMaxSpeed)); //This is how fast the boost will move, and speed will lerp towards it. It will either be boost speed, of if over that, a slight increase, not exceeding max speed.

		StopCoroutine(LerpToGoalSpeed(0)); //If already in motion for a boost just before, this ends that calculation, before starting a new one.
		StartCoroutine(LerpToGoalSpeed(_framesToReachBoostSpeed_));

		_faceDirection = _MainSkin.forward; //The direction to keep the player facing towards, will be changed in the turning script.
		_faceDirectionOffset = Vector2.zero;

		//Physics
		_PlayerPhys.SetCoreVelocity(_faceDirection * _currentSpeed); //Immediately launch the player in the direction the character is facing.
		_PlayerPhys._currentMaxSpeed = _maxSpeedWhileBoosting_;

		if (!_PlayerPhys._isGrounded) 
		{ 
			StartCoroutine(CheckAirBoost(_boostFramesInAir_));
			_canBoostBecauseGround = false; //This prevents another boost from being performed if in the air.
		}

		//Control
		_PlayerPhys.CallAccelerationAndTurning = CustomTurningAndAcceleration; //Changing this delegate will change what method to call to handle turning from the default to the custom one in this script.

		_Actions._ActionDefault._isAnimatorControlledExternally = true; //This script will point the character manually.
		_isStrafing = false; //Will start by not strafing, though this might be changed immediately depending on player input.

		_savedSkinDirection = _MainSkin.forward; //Keeps track of which way was facing when started, and if these stop being equal it means something external has affected the character's direction.

		//Effects
		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraPause(_cameraPauseEffect_, new Vector2 (_PlayerPhys._horizontalSpeedMagnitude,_goalSpeed + 2), 0.5f)); //The camera will fall back before catching up.
		//Make the boost effects fade in rather than appear instantly.
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
			_currentBoostEnergy -= _energyDrainedPerSecond_ * Time.fixedDeltaTime;

			Vector3 currentRunningPhysics = _PlayerPhys.GetRelevantVel(_PlayerPhys._RB.velocity, false); //Get the running velocity in physics (seperate from script calculations) as this will factor in collision.

			// Will end boost if released button , entered a state where without boost attached,  ran out of energy , or movement speed was decreased externally (like from a collision)
			if (!_Input.BoostPressed || !_inAStateThatCanBoost || _currentBoostEnergy <= 0 || currentRunningPhysics.sqrMagnitude < 100) //Remember that sqrMagnitude means what it's being compared to should be squared (10 -> 100)
			{ 
				EndBoost();
			}

			_currentSpeed = Mathf.Max(_currentSpeed, _PlayerPhys._horizontalSpeedMagnitude); //If running speed has been increased beyond boost speed (such as through slope physics) then factor that in so it isn't set over.
			//If running speed has been decreased by an external force AFTER boost speed was set last frame (such as by slope physics), then apply the difference to the boost speed.
			if(_PlayerPhys._horizontalSpeedMagnitude < _PlayerPhys._previousHorizontalSpeeds[1])
			{
				float difference = _PlayerPhys._horizontalSpeedMagnitude - _PlayerPhys._previousHorizontalSpeeds[1];
				_currentSpeed += difference;
			}

			//If speed is decreased beyond boost speed, then if on flat ground return to it.
			if(!_PlayerPhys._isCurrentlyOnSlope && _PlayerPhys._isGrounded) 
			{
				_currentSpeed = Mathf.MoveTowards(_currentSpeed, _currentSpeedLerpingTowardsGoal, _regainBoostSpeed_); //The latter will only be changed during initial boost start, so this won't kick in until losing speed after finishing lerp.
			}

			//Apply speed
			_PlayerPhys.SetLateralSpeed(_currentSpeed, false); //Applies boost speed to movement.
			if (_Actions._listOfSpeedOnPaths.Count > 0) _Actions._listOfSpeedOnPaths[0] = _currentSpeed; //Sets speed on rails, or other actions following paths.

			//Remember that the turning method will be called by the delegate in PlayerPhysics, not here. 

			_Input.RollPressed = false; //This will ensure the player won't crouch or roll and instead stay boosting.
		}

		else if (_gainEnergyOverTime_) { GainEnergyFromTime(); }
	}

	private void EndBoost () {
		_PlayerPhys._isBoosting = false;

		//Controls
		_Input.BoostPressed = false;
		StartCoroutine(DelayBoostStart());

		//Physics
		_PlayerPhys.CallAccelerationAndTurning = _PlayerPhys.DefaultAccelerateAndTurn; //Changes the method delegated for calculating acceleration and turning back to the correct one.
		_PlayerPhys._currentMaxSpeed = _Tools.Stats.SpeedStats.maxSpeed;
		StartCoroutine(SlowSpeedOnEnd(_speedLostOnEndBoost_, _framesToLoseSpeed_));

		//Control
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
			_currentSpeedLerpingTowardsGoal = _currentSpeed; //This will be used to track how far the lerp has come even if running speed is changed.

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

	private IEnumerator CheckAirBoost (float frames) {
		Vector3 startUpwards = transform.up;

		if(_hasAirBoost_) { yield break; }

		for (int i = 0; i < frames; i++)
		{
			yield return new WaitForFixedUpdate();

			if (_PlayerPhys._isGrounded) { yield break; }
			else if(Vector3.Angle(startUpwards, transform.up) > _AngleOfAligningToEndBoost_)
			{
				break;
			}
		}

		EndBoost();
	}

	//Over a set number of frames will apply force that removes an amount of speed. This will be against any existing acceleration.
	private IEnumerator SlowSpeedOnEnd ( float loseSpeed, int Frames) {
		float speedPerFrame = loseSpeed / Frames;
		float previousFrameSpeed = _PlayerPhys._horizontalSpeedMagnitude;
		Vector3 runningVelocity;

		for (int i = 0;i < Frames;i++)
		{
			runningVelocity = _PlayerPhys.GetRelevantVel(_PlayerPhys._coreVelocity, false).normalized; //Every update ensures its applying against players running speed, leaving gravity alone.

			if (_PlayerPhys._horizontalSpeedMagnitude < 80) { yield break; } //Won't decrease speed if player is already running under a certain speed.
			else if(_PlayerPhys._horizontalSpeedMagnitude >= previousFrameSpeed - 2) //Will not decrease speed if player has already decelerated from something else between frames.
			{
				_PlayerPhys.AddCoreVelocity(-runningVelocity * speedPerFrame); //Applies force against player this frame to slow them down.
				previousFrameSpeed = _PlayerPhys._horizontalSpeedMagnitude - speedPerFrame;
			}
			yield return new WaitForFixedUpdate();
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

		//Because input is relative to transform, temporarily make face directions operate in the same space. Without vertical value so it interacts properly with input direction.
		_faceDirection = _PlayerPhys.GetRelevantVel(_faceDirection, false);

		_PlayerPhys._inputVelocityDifference = lateralVelocity.sqrMagnitude < 1 ? 0 : Vector3.Angle(_faceDirection, inputDirection); //The change in input in degrees, this will be used by the skid script to calculate whether should skid.
		float inputDifference = _PlayerPhys._inputVelocityDifference;

		//If inputting backwards, ignore turning, this gives the chance to perform a skid.
		if(_PlayerPhys._inputVelocityDifference > 150) 
		{ 
			inputDirection = _faceDirection;
			inputDifference = 0;
		}

		float turnModifier = HandleStrafeOrFullTurn(inputDirection, inputDifference, lateralVelocity);

		lateralVelocity = Vector3.RotateTowards(lateralVelocity, inputDirection, _boostTurnSpeed_  * turnModifier * Mathf.Deg2Rad, 0); //Applies turn onto velocity.

		//Return face direction to world space so they can be applied in the SetSkinRotation method in default.
		_faceDirection = transform.TransformDirection(_faceDirection);

		//If the mainSkid direction has been changed when the saved hasn't, it means it's been done externally, such as by a path the player is moving along. So adjust the saved direction and facing direction used to align to.
		if(_savedSkinDirection != _MainSkin.forward)
		{
			_faceDirection = _MainSkin.forward;
			_savedSkinDirection = _MainSkin.forward;
		}
		return lateralVelocity;
	}

	private float HandleStrafeOrFullTurn(Vector3 inputDirection, float inputDifference, Vector3 lateralVelocity) {
		//Will not strafe if turning with the camera, if not inputting, or inputting too much.
		if (_Input.IsTurningBecauseOfCamera(inputDirection, 5))
		{
			_isStrafing = false;
		}
		else if(inputDifference > _turnCharacterThreshold_ || inputDifference == 0)
		{
			_isStrafing = false;
		}

		//Proper turning. If not strafing, then the face direction that controls player skin should rotate towards movement direction.
		if (!_isStrafing)
		{
			//Rotate face direction towards movement and remove offset. The mainskin will Rotate to these due to SetSkinRotation being called in Update.
			_faceDirection = Vector3.RotateTowards(_faceDirection, lateralVelocity, _faceTurnSpeed_ * Mathf.Deg2Rad, 0);
			_faceDirectionOffset = Vector2.zero;

			//Strafing will only be possible once the turn has completed so velocity has reached input and the character has reached input.
			if (Vector3.Angle(_faceDirection, lateralVelocity.normalized) < 1 && Vector3.Angle(_MainSkin.forward, transform.TransformDirection(_faceDirection)) < 2f)
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
			float horizontalOffset = Vector3.Angle(inputDirection, directionToTheRight) < 90 ? -1 : 1f; //If inputting to the right,  euler angle should be negative, meaning facing right.
			horizontalOffset *= Mathf.Min(inputDifference, 30); //Prevents the offset from being more than x degrees.

			_faceDirectionOffset = new Vector2( horizontalOffset, 0); //Apply offset this frame by difference to main direction. This will be used when calling SetSkinRotation

			return 0.6f;
		}
	}

	//These functions will handle increasing boost energy from various sources. Some are events that will be attached to event Handlers.
	void EventGainEnergyFromRings ( object sender, float source ) {
		source *= _energyGainPerRing_; //The source is how many rings, so gain energy for each multiplied by amount per ring.
		_currentBoostEnergy = Mathf.Min(_currentBoostEnergy + source, _maxBoostEnergy_);
	}

	void GainEnergyFromTime () {
		float source = Time.fixedDeltaTime * _energyGainPerSecond_;
		_currentBoostEnergy = Mathf.Min(_currentBoostEnergy + source, _maxBoostEnergy_);
	}

	//These events must be set in the PlayerPhysics component, and will happen when the player goes from grounded to airborne, or vice versa.
	public void EventOnGrounded () {			
 		_canBoostBecauseGround = true; //This allows another boost to be performed in the air (because this started from the ground. }
	}
	public void EventOnLoseGround() {
		StartCoroutine(CheckAirBoost(_boostFramesInAir_ * 0.75f));
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
		if(_gainEnergyFromRings_)
			_Tools.GetComponent<S_Handler_HealthAndHurt>().onRingGet += EventGainEnergyFromRings;

		_cameraPauseEffect_ = _Tools.Stats.BoostStats.cameraPauseEffect;
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

		_BoostUI =	_Tools.UISpawner._BoostUI;
	}
	#endregion
}
