using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;
using UnityEngine.Windows;
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
	private S_PlayerVelocity      _PlayerVel;
	private S_PlayerMovement	_PlayerMovement;
	private S_CharacterTools      _Tools;
	private S_ActionManager       _Actions;
	private S_Handler_Camera      _CamHandler;
	private S_PlayerInput         _Input;
	private S_Control_SoundsPlayer _Sounds;

	private Transform             _MainSkin;

	//Represent the effect
	private GameObject                      _BoostCone;
	private MeshRenderer[]                  _ListOfSubCones;

	private Animator    _CharacterAnimator;

	//Used to display boost energy to player.
	private S_UI_Boost            _BoostUI;
	#endregion

	//Stats - See CharacterStats for tooltips.
	#region Stats
	private bool        _hasAirBoost_;
	private float       _boostFramesInAir_ = 60;
	private float       _angleOfAligningToEndBoost_ = 85f;

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
	private int         _framesToLoseSpeed_ = 25;

	private float        _turnCharacterThreshold_ = 48;
	private float        _boostTurnSpeed_ = 2;
	private float        _faceTurnSpeed_ = 6;

	private float       _boostCooldown_ = 0.5f;

	private Vector2     _cameraPauseEffect_ = new Vector2(3, 40);
	#endregion

	// Trackers
	#region trackers
	private float                 _currentBoostEnergy = 10;

	//Running speed to apply / reach
	private float                 _currentSpeed;                //Sets movement speed to this every frame, and when boost started will lerp towards goal speed.
	private float                 _goalSpeed;                   //When a boost starts, this is the speed that will be used for the boost. It will either follow the stat, or an increase from current running speed. Whichevers higher.
	private float                 _currentSpeedLerpingTowardsGoal;        //Saved how much _currentSpeed increased towards goal speed, but ignores changes made to _currentSpeed externally, allowing _currentSpeed to lerp towards this when decreased after reaching _goalSpeed.

	//Boost checkers/
	private bool                  _inAStateThatCanBoost;        //Will be set false every frame, but true whenever AttemptAction is called. This means when false only, boost should end as not in a boostable state.
	private bool                  _canStartBoost = true;        //Set false when using boost, and only true after a cooldown. Must be true to start a boost.
	private bool                  _canBoostBecauseHasntBoostedInAir = true; //Turns false when starting a boost in the air, and true when grounded. Prevents starting multiple boosts in the air.

	//Strafing and character rotation
	private Vector3               _faceDirection;               //This decides which direction the character model will face when boosting. This overwrites the default handling of facing velocity, and instead allows strafing.
	private Vector2               _faceDirectionOffset;         //When applying character model direction, this will change the facing direction away from the proper direction, showing slight difference when strafing.
	private Vector3               _savedSkinDirection;          //Stores which way the character is facing according to this script, and if it isn't equal to the actual main skin, it means that's been changed externally, so should respond. This is used to change face direction if physics or paths happen.

	private Vector3               _coreVelocityLastFrame;

	private bool                  _isStrafing;
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
			switch (_Actions._whatCurrentAction)
			{
				case S_Enums.PrimaryPlayerStates.Default:
					if (_PlayerPhys._isGrounded)
						_Actions._ActionDefault.HandleAnimator(_Actions._ActionDefault._animationAction); // Means animation will reflect jumping or being grounded.;
					else
						_Actions._ActionDefault.HandleAnimator(11);
					_Actions._ActionDefault.SetSkinRotationToVelocity(10, _faceDirection, _faceDirectionOffset);
					break;
				case S_Enums.PrimaryPlayerStates.Jump:
					_Actions._ActionDefault.HandleAnimator(1);
					_Actions._ActionDefault.SetSkinRotationToVelocity(10, _faceDirection, _faceDirectionOffset);
					break;
			}

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

		if (_Input._BoostPressed)
		{
			_Input._RollPressed = false;
			if (!_PlayerPhys._isBoosting)//Will only trigger start action if not already boosting.
			{
				if (_canStartBoost && _currentBoostEnergy > _energyDrainedOnStart_ && _canBoostBecauseHasntBoostedInAir)
				{
					//Can always be performed on the ground, but if in the air, air actions must be available.
					if (_PlayerPhys._isGrounded || _Actions._areAirActionsAvailable)
						StartAction();
				}
				return true; //Will still return true even if not applying the boost, this will prevent entering the wrong action when this is in cooldown.
			}
		}
		return false;
	}

	//Called when the action is enabled and readies all variables for it to be performed.
	public void StartAction ( bool overwrite = false ) {
		if (!_Actions._canChangeActions && !overwrite) { return; }

		//Flow Control
		_PlayerPhys._isBoosting = true;
		_canStartBoost = false;

		//Energy management
		_currentBoostEnergy -= _energyDrainedOnStart_;

		//Get speeds to start boost at and reach after some time.
		_currentSpeed = _Actions._listOfSpeedOnPaths.Count > 0 ? _Actions._listOfSpeedOnPaths[0] : _PlayerVel._horizontalSpeedMagnitude; //The boost speed will be set to and increase from either the running speed, or path speed if currently in use.
		_currentSpeed = Mathf.Max(_currentSpeed, _startBoostSpeed_); //Ensures will start from a noticeable speed, then increase to full boost speed.
		_goalSpeed = Mathf.Max(_boostSpeed_, Mathf.Min(_currentSpeed + 10, _PlayerMovement._currentMaxSpeed)); //This is how fast the boost will move, and speed will lerp towards it. It will either be boost speed, of if over that, a slight increase, not exceeding max speed.

		StopCoroutine(LerpToGoalSpeed(0)); //If already in motion for a boost just before, this ends that calculation, before starting a new one.
		StartCoroutine(LerpToGoalSpeed(_framesToReachBoostSpeed_)); //Starts a coroutine to get to needed speed.

		_faceDirection = _MainSkin.forward; //The direction to keep the player facing towards, will be changed in the turning script.
		_faceDirectionOffset = Vector2.zero;

		//Physics
		_PlayerMovement._currentMaxSpeed = _maxSpeedWhileBoosting_;

		if (!_PlayerPhys._isGrounded)
		{
			_canBoostBecauseHasntBoostedInAir = false; //This will prevent the boost being used again until grounded.
			_CharacterAnimator.SetInteger("Action", 11);
			_CharacterAnimator.SetTrigger("ChangedState");
			
			StartCoroutine(CheckAirBoost(_boostFramesInAir_)); //Starts air boost parameters rather than normal boost.
			_PlayerVel.SetBothVelocities(_faceDirection * _currentSpeed, new Vector2(1, 0));
		}
		else
		{
			_PlayerVel.SetCoreVelocity(_faceDirection * _currentSpeed); //Immediately launch the player in the direction the character is facing.
		}

		//Control
		EnforceForwards();
		_PlayerMovement.CallAccelerationAndTurning = CustomTurningAndAcceleration; //Changing this delegate will change what method to call to handle turning from the default to the custom one in this script.

		_Actions._ActionDefault._isAnimatorControlledExternally = true; //This script will point the character manually.
		_isStrafing = true; //Will start by strafing, though this might be changed immediately depending on player input.

		_savedSkinDirection = _MainSkin.forward; //Keeps track of which way was facing when started, and if these stop being equal it means something external has affected the character's direction.

		//Effects
		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraPause(_cameraPauseEffect_, new Vector2(_PlayerVel._horizontalSpeedMagnitude, _goalSpeed + 2), 0.5f)); //The camera will fall back before catching up.

		_Sounds.BoostStartSound();

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
			//Energy management.
			_currentBoostEnergy = Mathf.Max(_currentBoostEnergy - (_energyDrainedPerSecond_ * Time.fixedDeltaTime), 0);

			Vector3 currentRunningPhysics = _PlayerPhys.GetRelevantVector(_PlayerVel._worldVelocity, false); //Get the running velocity in physics (seperate from script calculations) as this will factor in collision.

			// Will end boost if released button , entered a state where without boost attached,  ran out of energy , or movement speed was decreased externally (like from a collision)
			if (!_Input._BoostPressed || !_inAStateThatCanBoost || _currentBoostEnergy <= 0 
				|| (currentRunningPhysics.sqrMagnitude < Mathf.Pow(40,2) && _currentSpeedLerpingTowardsGoal == _goalSpeed)) //Remember that sqrMagnitude means what it's being compared to should be squared (10 -> 100)
			{
				EndBoost();
			}

			if (_Actions._listOfSpeedOnPaths.Count == 0)
			{
				_currentSpeed = Mathf.Max(_currentSpeed, _PlayerVel._currentRunningSpeed); //If running speed has been increased beyond boost speed (such as through slope physics) then factor that in so it isn't set over.
				
				//If running speed has been decreased by an external force AFTER boost speed was set last frame (such as by slope physics), then apply the difference to the boost speed.
				if (_PlayerVel._currentRunningSpeed < _PlayerVel._previousRunningSpeeds[1])
				{
					float difference = _PlayerVel._currentRunningSpeed - _PlayerVel._previousRunningSpeeds[1]; //Uses running speed instad of horizontal speed to only track core velocity (quickstep and other actions change total velocity only).
					_currentSpeed += difference;
				}
			}
			else
				_currentSpeed = Mathf.Max(_currentSpeed, _Actions._listOfSpeedOnPaths[0]);

			//If speed is decreased beyond boost speed, then if on flat ground return to it.
			if (!_PlayerPhys._isCurrentlyOnSlope && _PlayerPhys._isGrounded && _currentSpeed < _currentSpeedLerpingTowardsGoal)
			{
				_currentSpeed = Mathf.MoveTowards(_currentSpeed, _currentSpeedLerpingTowardsGoal, _regainBoostSpeed_); //The latter will only be changed during initial boost start, so this won't kick in until losing speed after finishing lerp.
			}

			//Apply speed
			_PlayerVel.SetLateralSpeed(_currentSpeed, false); //Applies boost speed to movement.
			if (_Actions._listOfSpeedOnPaths.Count > 0)
				_Actions._listOfSpeedOnPaths[0] = _currentSpeed; //Sets speed on rails, or other actions following paths.

			//Remember that the turning method will be called by the delegate in PlayerPhysics, not here. 

			_Input._RollPressed = false; //This will ensure the player won't crouch or roll and instead stay boosting.
		}
		//If not currently boosting, then check if should gain energy each frame.
		else if (_gainEnergyOverTime_) { GainEnergyFromTime(); }
	}

	//Called when a boost should come to an end. Applies trackers to tell the script the boost is over, and applie ending effects.
	private void EndBoost ( bool skipSlowing = false ) {
		//Flow cotntrol
		_PlayerPhys._isBoosting = false;

		//Controls
		_Input._BoostPressed = false;
		StartCoroutine(DelayBoostStart());

		//Physics
		_PlayerMovement.CallAccelerationAndTurning = _PlayerMovement.DefaultAccelerateAndTurn; //Changes the method delegated for calculating acceleration and turning back to the correct one.
		if (!skipSlowing) StartCoroutine(SlowSpeedOnEnd(_speedLostOnEndBoost_, _framesToLoseSpeed_)); //Player lose speed when ending a boost

		//Control
		_Input._BoostPressed = false; //Incase end boost was called not letting go of the button.
		_Actions._ActionDefault._isAnimatorControlledExternally = false; //The main action now takes over animations again.

		//Effects
		StopCoroutine(SetBoostEffectVisibility(0, 0, 0));
		StartCoroutine(SetBoostEffectVisibility(1, 0.1f, 12));
		_Sounds.BoostSource2.Stop();
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

	//Called at the end of another boost and prevents a boost from being started until over.
	private IEnumerator DelayBoostStart () {
		yield return new WaitForSeconds(_boostCooldown_);
		_canStartBoost = true;
	}

	//Called when starting or ending a boost, and will over time cause the boost effect to fade in. 
	private IEnumerator SetBoostEffectVisibility ( float startAlpha, float setAlpha, float frames ) {
		if (setAlpha > 0.5f) { _BoostCone.SetActive(true); } //If fading in, the effect should become active, enabling visibility, before increasing alpha.

		float segments = 1 / frames;
		float lerpValue = startAlpha;

		//Over the input frames, will linearly change alpha from start to end.
		for (int i = 0 ; i < frames ; i++)
		{
			lerpValue = Mathf.MoveTowards(lerpValue, setAlpha, segments); //Moves value
			ChangeAlphaOfCones(lerpValue); //Changes alpha to new value.
			yield return new WaitForFixedUpdate();
		}

		if (setAlpha <= 0.1f) { _BoostCone.SetActive(false); } //If fading out, should become inactive once no longer visible.
	}

	//Applies a new visibility to the objects saved as representting the boost effect using Material Property Blockers.
	private void ChangeAlphaOfCones ( float alpha ) {
		//Since the effect might be made up of several cones, each with their own materials.
		for (int i = 0 ; i < _ListOfSubCones.Length ; i++)
		{
			MeshRenderer R = _ListOfSubCones[i];
			// Takes the instance of this material on the mesh, and changes a property in its shader. Check the shader itself for the property, but currently, alpha modifier is multiplied with the opacity nodes.
			MaterialPropertyBlock TempBlock = new MaterialPropertyBlock();
			TempBlock.SetFloat("_Alpha_Modifier", alpha);
			R.SetPropertyBlock(TempBlock);
		}
	}

	//Called when boosting in the air, or entering the boost from the air.
	private IEnumerator CheckAirBoost ( float frames ) {

		if (_hasAirBoost_) { yield break; } //Air boost works the same as a normal boost, but in the air. And if it isn't in use, will end boost as now in the air.

		Vector3 startUpwards = transform.up; //Saves how the player was rotated at the start, and will compare against this.

		//A boost can last in the air for this many frames.
		for (int i = 0 ; i < frames ; i++)
		{
			yield return new WaitForFixedUpdate();

			if (_PlayerPhys._isGrounded) { yield break; }                         //If hits the ground, then stop this check and resume boost as normal.

			//If the player rotates enough (due to the align with ground function in PlayerPhysics), then boost should be ended prematurely. For instance, if running up a wall, that is then lost, boost should end once the player is facing forwards relative to gravity.
			else if (Vector3.Angle(startUpwards, transform.up) > _angleOfAligningToEndBoost_)
			{
				break;
			}
		}

		EndBoost();
	}

	//Over a set number of frames will apply force that removes an amount of speed. This will be against any existing acceleration.
	private IEnumerator SlowSpeedOnEnd ( float loseSpeed, int Frames ) {
		float speedPerFrame = loseSpeed / Frames;
		float previousFrameSpeed = _PlayerVel._horizontalSpeedMagnitude;
		Vector3 runningVelocity;

		for (int i = 0 ; i < Frames ; i++)
		{
			runningVelocity = _PlayerPhys.GetRelevantVector(_PlayerVel._coreVelocity, false).normalized; //Every update ensures its applying against players running speed, leaving gravity alone.

			if (_PlayerVel._horizontalSpeedMagnitude < 80) { yield break; } //Won't decrease speed if player is already running under a certain speed.

			else if (_PlayerVel._horizontalSpeedMagnitude >= previousFrameSpeed - 2) //Will not decrease speed if player has already decelerated from something else between frames.
			{
				_PlayerVel.AddCoreVelocity(-runningVelocity * speedPerFrame); //Applies force against player this frame to slow them down.
				if (_Actions._listOfSpeedOnPaths.Count > 0) _Actions._listOfSpeedOnPaths[0] -= speedPerFrame; //Will also slow speed on rails, or other actions following paths.
				previousFrameSpeed = _PlayerVel._horizontalSpeedMagnitude - speedPerFrame; //Use for checking if speed has slowed externally, between checks here.

			}
			yield return new WaitForFixedUpdate();
		}

		_PlayerMovement._currentMaxSpeed = _Tools.Stats.SpeedStats.maxSpeed; // When all extra speed is lost, this resets the max speed players can reach to what it should be.
	}

	//Ensures there will always be an input forwards if nothing else.
	private Vector3 EnforceForwards (Vector3 input = default(Vector3)) {
		Vector3 localFaceDirection = _PlayerPhys.GetRelevantVector(_faceDirection, true);
		if (_PlayerMovement._moveInput.sqrMagnitude < 0.1f) { _PlayerMovement._moveInput = localFaceDirection; }
		
		if (input.sqrMagnitude < 0.1f) { input = localFaceDirection; }
		return input;
	}


	#endregion
	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//This is called via a delegate and replaces the default turning and acceleration present in PlayerPhysics. It takes the same inputs, but will return a different output.
	public Vector3 CustomTurningAndAcceleration ( Vector3 lateralVelocity, Vector3 input, Vector2 modifier ) {
		input = EnforceForwards(input);

		// Normalize to get input direction and magnitude seperately. For efficency and to prevent larger values at angles, the magnitude is based on the higher input.
		Vector3 inputDirection = input.normalized;

		//Because input is relative to transform, temporarily make face directions operate in the same space. Without vertical value so it interacts properly with input direction.
		Vector3 localFaceDirection = _PlayerPhys.GetRelevantVector(_faceDirection, false);

		_PlayerMovement._inputVelocityDifference = lateralVelocity.sqrMagnitude < 1 ? 0 : Vector3.Angle(localFaceDirection, inputDirection); //The change in input in degrees, this will be used by the skid script to calculate whether should skid.
		float inputDifference = _PlayerMovement._inputVelocityDifference;

		//If inputting backwards, ignore turning, this gives the chance to perform a skid.
		if (_PlayerMovement._inputVelocityDifference > 150)
		{
			inputDirection = localFaceDirection;
			inputDifference = 0;
		}

		//This gets a difference in turn speed based on input causing a strafe or full turn. The method will also check strafing and apply additional effects.
		float turnModifier = HandleStrafeOrFullTurn(inputDirection, inputDifference, lateralVelocity);

		lateralVelocity = Vector3.RotateTowards(lateralVelocity, inputDirection, _boostTurnSpeed_ * turnModifier * Mathf.Deg2Rad, 0); //Applies turn onto velocity.

		//If the mainSkin direction has been changed when the saved hasn't, it means it's been done externally, such as by a path the player is moving along. So adjust the saved direction and facing direction used to align to.
		if (_savedSkinDirection != _MainSkin.forward)
		{
			_faceDirection = _MainSkin.forward;
			_savedSkinDirection = _MainSkin.forward;
		}
		//If the core velocity direction has been changed, like by running into and scraping along a wall
		else if (_coreVelocityLastFrame.normalized != _PlayerVel._coreVelocity.normalized)
		{
			//Then align to this new direction.
			Vector3 newForward = _PlayerVel._coreVelocity.normalized;
			newForward = newForward - transform.up * Vector3.Dot(newForward, transform.up);
			_faceDirection = newForward;
		}
		_coreVelocityLastFrame = _PlayerVel._coreVelocity;

		return lateralVelocity; //Just like normal turns, velocity input and output are relevant to local space of players horizontal movement.
	}

	private float HandleStrafeOrFullTurn ( Vector3 inputDirection, float inputDifference, Vector3 lateralVelocity ) {
		//Will not strafe if turning with the camera, if not inputting, or inputting too much.
		if (_Input.IsTurningBecauseOfCamera(inputDirection, 6))
		{
			_isStrafing = false;
		}
		else if (inputDifference > _turnCharacterThreshold_ || inputDifference == 0)
		{
			_isStrafing = false;
		}

		//Proper turning. If not strafing, then the face direction that controls player skin should rotate towards movement direction.
		if (!_isStrafing)
		{
			Vector3 newForward = _PlayerVel._coreVelocity.normalized;
			newForward = newForward - transform.up * Vector3.Dot(newForward, transform.up);

			//Rotate face direction towards movement and remove offset. The mainskin will Rotate to these due to SetSkinRotation being called in Update.
			_faceDirection = Vector3.RotateTowards(_faceDirection, newForward, _faceTurnSpeed_ * Mathf.Deg2Rad, 0);
			_faceDirectionOffset = Vector2.zero;

			//Strafing will only be possible once the turn has completed so velocity has reached input and the character has reached input.
			if (Vector3.Angle(_faceDirection, newForward.normalized) < 1 && Vector3.Angle(_MainSkin.forward, transform.TransformDirection(_faceDirection)) < 2f)
			{
				_isStrafing = true;
			}

			return 1;
		}
		//Strafing 
		else
		{
			//Even if not turning, make the player skin slightly rotate right or left to show strafing (this could ideally be done with unique animations but we don't have access to that).
			Vector3 directionToTheRight = Vector3.Cross(_faceDirection, transform.up); //Get direction to right
			float horizontalOffset = Vector3.Angle(inputDirection, directionToTheRight) < 90 ? -1 : 1f; //If inputting to the right,  euler angle should be negative, meaning facing right.
			horizontalOffset *= Mathf.Min(inputDifference, 30); //Prevents the offset from being more than x degrees.

			_faceDirectionOffset = new Vector2(horizontalOffset, 0); //Apply offset this frame by difference to main direction. This will be used when calling SetSkinRotation

			return 0.6f;
		}
	}

	//These functions will handle increasing boost energy from various sources. Some are events that will be attached to event Handlers.
	void EventGainEnergyFromRings ( object sender, float source ) {
		source *= _energyGainPerRing_; //The source is how many rings, so gain energy for each multiplied by amount per ring.
		_currentBoostEnergy = Mathf.Min(_currentBoostEnergy + source, _maxBoostEnergy_);
	}

	//Not an event, but depending on stats will be called every frame to increase energy.
	void GainEnergyFromTime () {
		float source = Time.fixedDeltaTime * _energyGainPerSecond_;
		_currentBoostEnergy = Mathf.Min(_currentBoostEnergy + source, _maxBoostEnergy_);
	}

	//These events must be set in the PlayerPhysics component, and will happen when the player goes from grounded to airborne, or vice versa.
	public void EventOnGrounded () {
		if (!_canBoostBecauseHasntBoostedInAir)
		{
			_CharacterAnimator.SetInteger("Action", 0);
			_CharacterAnimator.SetTrigger("ChangedState");
		}

		_canBoostBecauseHasntBoostedInAir = true; //This allows another boost to be performed in the air (because this started from the ground. }
	}
	public void EventOnLoseGround () {
		if (_PlayerPhys._isBoosting)
			StartCoroutine(CheckAirBoost(_boostFramesInAir_));
	}

	//Boost should end when going through springs or dash rings.This will not trigger the speed being lost over time.
	public void EventOnTriggerAirLauncher () {
		EndBoost(true);
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
		if (_gainEnergyFromRings_)
			_Tools.GetComponent<S_Handler_HealthAndHurt>().onRingGet += EventGainEnergyFromRings;

		_cameraPauseEffect_ = _Tools.Stats.BoostStats.cameraPauseEffect;

		_hasAirBoost_ = _Tools.Stats.BoostStats.hasAirBoost;
		_boostFramesInAir_ = _Tools.Stats.BoostStats.boostFramesInAir;
		_angleOfAligningToEndBoost_ = _Tools.Stats.BoostStats.AngleOfAligningToEndBoost;

		_gainEnergyFromRings_ = _Tools.Stats.BoostStats.gainEnergyFromRings;
		_gainEnergyOverTime_ = _Tools.Stats.BoostStats.gainEnergyOverTime;
		_energyGainPerSecond_ = _Tools.Stats.BoostStats.energyGainPerSecond;
		_energyGainPerRing_ = _Tools.Stats.BoostStats.energyGainPerRing;

		_maxBoostEnergy_ = _Tools.Stats.BoostStats.maxBoostEnergy;
		_energyDrainedPerSecond_ = _Tools.Stats.BoostStats.energyDrainedPerSecond;
		_energyDrainedOnStart_ = _Tools.Stats.BoostStats.energyDrainedOnStart;

		_startBoostSpeed_ = _Tools.Stats.BoostStats.startBoostSpeed;
		_framesToReachBoostSpeed_ = _Tools.Stats.BoostStats.framesToReachBoostSpeed;

		_boostSpeed_ = _Tools.Stats.BoostStats.boostSpeed;
		_maxSpeedWhileBoosting_ = _Tools.Stats.BoostStats.maxSpeedWhileBoosting;
		_regainBoostSpeed_ = _Tools.Stats.BoostStats.regainBoostSpeed;

		_speedLostOnEndBoost_ = _Tools.Stats.BoostStats.speedLostOnEndBoost;
		_framesToLoseSpeed_ = _Tools.Stats.BoostStats.framesToLoseSpeed;

		_turnCharacterThreshold_ = _Tools.Stats.BoostStats.turnCharacterThreshold;
		_boostTurnSpeed_ = _Tools.Stats.BoostStats.boostTurnSpeed;
		_faceTurnSpeed_ = _Tools.Stats.BoostStats.faceTurnSpeed;

		_boostCooldown_ = _Tools.Stats.BoostStats.cooldown;

	}

	private void AssignTools () {
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_PlayerVel = _Tools.GetComponent<S_PlayerVelocity>();
		_PlayerMovement = _Tools.GetComponent<S_PlayerMovement>();
		_Actions = _Tools._ActionManager;
		_CamHandler = _Tools.CamHandler;
		_Input = _Tools.GetComponent<S_PlayerInput>();

		_Sounds = _Tools.SoundControl;
		_MainSkin = _Tools.MainSkin;
		_CharacterAnimator = _Tools.CharacterAnimator;	

		_BoostCone = _Tools.BoostCone;
		_BoostCone.SetActive(false);

		_BoostUI = _Tools.UISpawner._BoostUI;
	}
	#endregion
}
