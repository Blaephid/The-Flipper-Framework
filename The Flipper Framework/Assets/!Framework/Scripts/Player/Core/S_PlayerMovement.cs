using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

[RequireComponent(typeof(S_PlayerPhysics))]
public class S_PlayerMovement : MonoBehaviour
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
	private S_PlayerVelocity	_PlayerVel;
	private S_PlayerInput         _Input;
	private S_ActionManager       _Actions;
	private S_Control_SoundsPlayer _Sounds;

	private Transform   _MainSkin;
	#endregion

	//Stats
	#region Stats
	[Header("Grounded Movement")]
	private float                 _startAcceleration_ = 12f;
	private float                 _startRollAcceleration_ = 4f;
	private AnimationCurve        _AccelBySpeed_;
	private AnimationCurve        _AccelBySlope_;
	private float                 _angleToAccelerate_;

	[HideInInspector]
	public float                  _moveDeceleration_ = 1.3f;
	AnimationCurve                _DecelBySpeed_;
	private float                 _airDecel_ = 1.05f;
	private float                 _constantAirDecel_ = 1.01f;

	private float                 _turnDrag_;
	private float                 _turnSpeed_ = 16f;
	private AnimationCurve        _TurnRateByAngle_;
	private AnimationCurve        _TurnRateBySpeed_;
	private AnimationCurve        _TurnRateByInputChange_;
	private AnimationCurve        _DragByAngle_;
	private AnimationCurve        _DragBySpeed_;

	private float                 _startTopSpeed_ = 65f;
	private float                 _startMaxSpeed_ = 230f;
	private float                 _startMaxFallingSpeed_ = -500f;

	[Header("Rolling Values")]
	private float                 _rollingTurningModifier_;
	private float                 _rollingDecel_;

	[Header("Air Values")]
	private bool                  _shouldStopAirMovementIfNoInput_ = false;

	private float                 _slopeEffectLimit_ = 0.9f;
	#endregion

	// Trackers
	#region trackers
	//Methods
	public delegate Vector3 DelegateAccelerationAndTurning ( Vector3 vector, Vector3 input, Vector2 modifier );        //A delegate for deciding methods to calculate acceleration and turning 
	public DelegateAccelerationAndTurning   CallAccelerationAndTurning; //This delegate will be called in controlled velocity to return changes to acceleration and turning. This will usually be the base one in this script, but may be changed externally depending on the action.
	[HideInInspector] public bool _lockAccelerationAndTurningToDefault; //If true, the delegate above will not be called, and instead the default one will be, preventing any overwriting while locked.	
	

	[HideInInspector]
	public Vector3                _moveInput;         //Assigned by the input script, the direction the player is trying to go.

	[HideInInspector]
	public Vector3                _trackMoveInput;    //Follows the input direction, and it is has changed but the controller input hasn't, that means the camera was moved to change direction.

	public float _externalAccelModi { private get; set; } = 1;
	public float _externalTurnModi { private get; set; } = 1;

	[HideInInspector]
	public float _useFlatTurnRate = 0;

	[HideInInspector]
	public float                  _currentTopSpeed;   //Player cannot exceed this speed by just running on flat ground. May be changed across gameplay.
	[HideInInspector]
	public float                  _currentMaxSpeed;   //Player's core velocity can not exceed this by any means.
	[HideInInspector]
	public float                  _currentMinSpeed;   //Player's running velocity can not go below this. Should be 0, and only temporarily set for certain actions.

	//Updated each frame to get current place on animation curves relevant to movement.
	private float                 _currentRunAccell;
	private float                 _currentRollAccell;
	[HideInInspector]
	public float                  _curvePosAcell;
	[HideInInspector]
	public float                 _curvePosDecell;
	[HideInInspector]
	public float                  _curvePosDrag;

	[HideInInspector]
	public float                  _inputVelocityDifference = 1; //Referenced by other scripts to get the angle between player input and movement direction

	#endregion
	#endregion

	//On start, assigns stats.
	private void Awake () {
		_Tools = GetComponent<S_CharacterTools>();
		AssignTools();
		AssignStats();

		//Set delegates
		CallAccelerationAndTurning = DefaultAccelerateAndTurn; //Whenever this delegate is called, it will call the default acceleration and turning present in this script, but the delegate may be changed by actions.

	}

	private void FixedUpdate () {
		//Get curve positions, which will be used in calculations for this frame.
		_curvePosAcell = _AccelBySpeed_.Evaluate(_PlayerVel._currentRunningSpeed / _currentTopSpeed);
		_curvePosDecell = _DecelBySpeed_.Evaluate(_PlayerVel._currentRunningSpeed / _currentMaxSpeed);
		_curvePosDrag = _DragBySpeed_.Evaluate(_PlayerVel._currentRunningSpeed / _currentMaxSpeed);
	}

	//Handles core velocity, which is the velocity directly under the player's control (seperate from environmental velocity which is placed on the character by other things).
	//This turns, decreases and/or increases the velocity based on input.
	public Vector3 HandleControlledVelocity ( Vector3 startVelocity, Vector2 modifier, float decelerationModifier = 1 ) {

		modifier.Set(modifier.x * _externalAccelModi, modifier.y * _externalTurnModi);

		//Certain actions control velocity in their own way, so if the list is greater than 0, end the method (ensuring anything that shouldn't carry over frames won't.)
		if (_PlayerPhys._locksForCanControl.Count != 0)
		{
			_PlayerVel._externalRunningSpeed = -1;
			return startVelocity;
		}

		//Original by Damizean, edited by Blaephid

		//Gets current running velocity, then splits it into horizontal and vertical velocity relative to the character.
		//This means running up a wall will have zero vertical velocity because the character isn't moveing up relative to their rotation.Only the lateral velocity will be changed in this method.
		Vector3 localVelocity = transform.InverseTransformDirection(startVelocity);
		Vector3 lateralVelocity = new Vector3(localVelocity.x, 0.0f, localVelocity.z);
		Vector3 lateralVelocityBeforeChanges = lateralVelocity;

		Vector3 verticalVelocity = new Vector3(0.0f, localVelocity.y, 0.0f);

		//Apply changes to the lateral velocity based on input.
		if(!_lockAccelerationAndTurningToDefault)
		{
			//Because this is a delegate, the method it is calling may change, but by default it will be the method in this script called Default.
			lateralVelocity = CallAccelerationAndTurning(lateralVelocity, _moveInput, modifier);
		}
		else
		{
			lateralVelocity = DefaultAccelerateAndTurn(lateralVelocity, _moveInput, modifier);
		}

		lateralVelocity = Decelerate(lateralVelocity, _moveInput * decelerationModifier, _curvePosDecell);

		//If external core speed has been set to a positive value this frame, overwrite running speed without losing direction.
		if (_PlayerVel._externalRunningSpeed >= 0 && lateralVelocity.sqrMagnitude > -1)
		{
			if (lateralVelocity.sqrMagnitude < 0.1f) { lateralVelocity = _MainSkin.forward; } //Ensures speed will always be applied, even if there's currently no velocity.
			lateralVelocity = lateralVelocity.normalized * _PlayerVel._externalRunningSpeed;
			_PlayerVel._externalRunningSpeed = -1; //Set to a negative value so core speeds of 0 can be set externally.
		}
		//Enforces the min speed if there is one, but only checks if close to it.
		else if (_currentMinSpeed > 0 && _PlayerVel._currentRunningSpeed < _currentMinSpeed + 5)
		{
			if (lateralVelocity.sqrMagnitude < Mathf.Pow(_currentMinSpeed, 2))
			{
				lateralVelocity = lateralVelocity.normalized * _currentMinSpeed;
			}
		}

		//Before taking off, no matter the acceleration, there will be one frame before the player starts gaining speed, this is to give an easy change to remove speed if trying to move into an obstacle.
		if (lateralVelocityBeforeChanges.sqrMagnitude < Mathf.Pow(0.0001f, 2))
		{
			lateralVelocity = lateralVelocity.normalized * 0.005f;
		}

		// Clamp horizontal running speed. coreVelocity can never exceed the player moving laterally faster than this.
		localVelocity = lateralVelocity + verticalVelocity;
		if (_PlayerVel._currentRunningSpeed > _currentMaxSpeed)
		{
			Vector3 ReducedSpeed = localVelocity;
			float keepY = localVelocity.y;
			ReducedSpeed = Vector3.ClampMagnitude(ReducedSpeed, _currentMaxSpeed);
			ReducedSpeed.y = keepY;
			localVelocity = ReducedSpeed;
		}

		//Bring local velocity back to world space.
		Vector3 newVelocity = transform.TransformDirection(localVelocity);
		return newVelocity;
	}

	//This handles increasing the speed while changing the direction of the player's controlled velocity.
	//It will not allow speed to increase if over topSpeed, but will only decrease if there is enough drag from the turn.
	//This will only be called by delegates, but is the default means of handling acceleration and turn. See CallAccelerationAndTurning for more.
	public Vector3 DefaultAccelerateAndTurn ( Vector3 lateralVelocity, Vector3 input, Vector2 modifier ) {

		Debug.DrawRay(transform.position, input * 10, Color.yellow);

		// Normalize to get input direction and magnitude seperately. For efficency and to prevent larger values at angles, the magnitude is based on the higher input.
		Vector3 inputDirection = input.normalized;
		float inputMagnitude = Mathf.Max(Mathf.Abs(_Input._inputOnController.x), Mathf.Abs(_Input._inputOnController.z));

		// Step 1) Determine angle between current lateral velocity and desired direction.
		//         Creates a quarternion which rotates to the direction, which will be identity if velocity is too slow.

		_inputVelocityDifference = lateralVelocity.sqrMagnitude < 1 ? 0 : Vector3.Angle(lateralVelocity, inputDirection); //The change in input in degrees, this will be used by the skid script to calculate whether should skid.
		float deviationFromInput = _inputVelocityDifference  / 180.0f;

		Quaternion lateralToInput = lateralVelocity.sqrMagnitude < 1
			? Quaternion.identity
			: Quaternion.FromToRotation(lateralVelocity.normalized, inputDirection);

		float dragRate = 0; //This will be applied when changing speed. But will only be greater than 0 if turning.

		//If standing still, should immediately move in required direction, rather than rotate velocity from zero towards it.
		if (lateralVelocity.sqrMagnitude < 1)
		{
			lateralVelocity = inputDirection * inputMagnitude;
		}

		//A list is used rather than a single boolean because if just one was used, anything that takes turning away would overlap. This way means all instances of turning being disabled must stop in order to regain control.
		else if (_PlayerPhys._locksForCanTurn.Count == 0)
		{
			// Step 2) Rotate lateral velocity towards the same velocity under the desired rotation.
			//         The ammount rotated is determined by turn speed multiplied by turn rate (defined by the difference in angles, and current speed).
			//	Turn speed will also increase if the difference in pure input (ignoring camera) is different, allowing precise movement with the camera.

			float turnRate = 1;
			dragRate = _DragByAngle_.Evaluate(deviationFromInput) * _curvePosDrag; //If turning, may lose speed.

			if (_useFlatTurnRate == 0)
			{
				turnRate = (_PlayerPhys._isRolling ? _rollingTurningModifier_ : 1.0f);
				turnRate *= _TurnRateByAngle_.Evaluate(deviationFromInput);
				turnRate *= _TurnRateBySpeed_.Evaluate((_PlayerVel._coreVelocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);

				if (_Input.IsTurningBecauseOfCamera(inputDirection))
				{
					turnRate *= _TurnRateByInputChange_.Evaluate(Vector3.Angle(_Input._inputOnController, _Input._prevInputWithoutCamera) / 180);
				}

				turnRate *= _turnSpeed_;
				turnRate *= modifier.x;
			}
			else
			{
				turnRate = _useFlatTurnRate;
				turnRate *= _TurnRateByAngle_.Evaluate(deviationFromInput);
			}

			lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, turnRate * Mathf.Deg2Rad, 0.0f); //Apply turn by calculate speed
		}

		// Step 3) Get current velocity (if it's zero then use input)
		//         Increase or decrease the size by using movetowards zero (a minus value increases size in the velocity direction)
		//         The total change is decided by acceleration based on input and speed, then drag from the turn.

		Vector3 setVelocity = lateralVelocity.sqrMagnitude > 0 ? lateralVelocity : inputDirection;
		float accelRate = 0;

		if (deviationFromInput < _angleToAccelerate_ || _PlayerVel._currentRunningSpeed < 10) //Will only accelerate if inputing in direction enough, unless under certain speed.
		{
			accelRate = (_PlayerPhys._isRolling && _PlayerPhys._isGrounded ? _currentRollAccell : _currentRunAccell) * inputMagnitude;
			accelRate *= _curvePosAcell;
			if (_PlayerPhys._isGrounded) accelRate *= _AccelBySlope_.Evaluate(_PlayerPhys._groundNormal.y);
		}

		float speedChange = accelRate - (dragRate * _turnDrag_) * modifier.y;

		setVelocity = Vector3.MoveTowards(setVelocity, Vector3.zero, -speedChange);

		//Step 4) If the change is still under the current top speed, or the change is a decrease in total, then apply it.
		//        Top speed can only be exceeded through other means like actions or slopes.
		if (setVelocity.sqrMagnitude < _currentTopSpeed * _currentTopSpeed || setVelocity.sqrMagnitude < lateralVelocity.sqrMagnitude)
		{
			lateralVelocity = setVelocity;
		}

		return lateralVelocity;
	}

	//Handles decreasing the magnitude of the player's controlled velocity, usually only if there is no input, but other circumstances may decrease speed as well.
	//Deceleration is calculated, then applied at the end of the method.
	//is static so it can be called by simulations.
	public Vector3 Decelerate ( Vector3 lateralVelocity, Vector3 input, float modifier ) {

		float decelAmount = 0;
		//Manual decelerations can only happen if nothing is denying them.
		if (_PlayerPhys._locksForCanDecelerate.Count == 0)
		{         //If there is no input, ready conventional deceleration.
			if (input.sqrMagnitude < 0.1)
			{
				if (_PlayerPhys._isGrounded)
				{
					decelAmount = _moveDeceleration_ * modifier;
				}
				else if (_shouldStopAirMovementIfNoInput_)
				{
					decelAmount = _airDecel_ * modifier;
				}
			}
			//If grounded and rolling but not on a slope, even with input, ready deceleration. 
			else if (_PlayerPhys._isRolling && _PlayerPhys._groundNormal.y > _slopeEffectLimit_ && _PlayerVel._currentRunningSpeed > 10)
			{
				decelAmount = _rollingDecel_ * modifier;
			}
		}
		//If in air, a constant deceleration is applied in addition to any others.
		if (!_PlayerPhys._isGrounded && _PlayerVel._currentRunningSpeed > 14)
		{
			decelAmount += _constantAirDecel_;
		}

		//Apply calculated deceleration
		return Vector3.MoveTowards(lateralVelocity, Vector3.zero, decelAmount);
	}

	private void AssignStats () {
		_startAcceleration_ = _Tools.Stats.AccelerationStats.runAcceleration;
		_startRollAcceleration_ = _Tools.Stats.AccelerationStats.rollAccel;
		_AccelBySpeed_ = _Tools.Stats.AccelerationStats.AccelBySpeed;
		_AccelBySlope_ = _Tools.Stats.AccelerationStats.AccelBySlopeAngle;
		_angleToAccelerate_ = _Tools.Stats.AccelerationStats.angleToAccelerate / 180;
		_turnDrag_ = _Tools.Stats.TurningStats.turnDrag;
		_turnSpeed_ = _Tools.Stats.TurningStats.turnSpeed;

		_TurnRateByAngle_ = _Tools.Stats.TurningStats.TurnRateByAngle;
		_TurnRateBySpeed_ = _Tools.Stats.TurningStats.TurnRateBySpeed;
		_TurnRateByInputChange_ = _Tools.Stats.TurningStats.TurnRateByInputChange;
		_DragByAngle_ = _Tools.Stats.TurningStats.DragByAngle;
		_DragBySpeed_ = _Tools.Stats.TurningStats.DragBySpeed;
		_startTopSpeed_ = _Tools.Stats.SpeedStats.topSpeed;
		_startMaxSpeed_ = _Tools.Stats.SpeedStats.maxSpeed;
		_startMaxFallingSpeed_ = _Tools.Stats.WhenInAir.startMaxFallingSpeed;
		_moveDeceleration_ = _Tools.Stats.DecelerationStats.moveDeceleration;
		_DecelBySpeed_ = _Tools.Stats.DecelerationStats.DecelBySpeed;
		_constantAirDecel_ = _Tools.Stats.DecelerationStats.airConstantDecel;
		_airDecel_ = _Tools.Stats.DecelerationStats.airManualDecel;

		_slopeEffectLimit_ = _Tools.Stats.SlopeStats.slopeEffectLimit;


		_shouldStopAirMovementIfNoInput_ = _Tools.Stats.WhenInAir.shouldStopAirMovementWhenNoInput;
		_rollingTurningModifier_ = _Tools.Stats.RollingStats.rollingTurningModifier;
		_rollingDecel_ = _Tools.Stats.DecelerationStats.rollingFlatDecell;

		//Sets all changeable core values to how they are set to start in the editor.
		_PlayerPhys._maxFallingSpeed_ = _startMaxFallingSpeed_;
		_currentRunAccell = _startAcceleration_;
		_currentRollAccell = _startRollAcceleration_;
		_currentTopSpeed = _startTopSpeed_;
		_currentMaxSpeed = _startMaxSpeed_;
	}

	private void AssignTools () {
		_Actions = _Tools._ActionManager;
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_PlayerVel = GetComponent<S_PlayerVelocity>();

		_MainSkin = _Tools.MainSkin;
	}
}
