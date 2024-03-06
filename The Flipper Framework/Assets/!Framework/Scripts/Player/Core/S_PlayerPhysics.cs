using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Windows;
using UnityEditor;
using System.Linq;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class S_PlayerPhysics : MonoBehaviour
{

	/// <summary>
	/// Members ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region members

	//Unity
	#region Unity Specific Members
	private S_ActionManager       _Action;
	private S_CharacterTools      _Tools;
	private S_Control_PlayerSound _SoundController;
	static public S_PlayerPhysics s_MasterPlayer;
	private S_PlayerInput         _Input;
	private S_Handler_Camera      _camHandler;

	public UnityEvent             _OnGrounded;        //Event called when isGrounded is set to true from false, remember to assign what methods to call in the editor.

	[HideInInspector]
	public Rigidbody _RB;
	private CapsuleCollider       _CharacterCapsule;
	private Transform             _FeetTransform;
	private Transform             _MainSkin;
	#endregion

	//General
	#region General Members

	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
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

	[Header("Slopes")]
	private float                 _slopeEffectLimit_ = 0.9f;
	private AnimationCurve        _SlopeSpeedLimitByAngle_;

	private float                 _generalHillMultiplier_ = 1;
	private float                 _uphillMultiplier_ = 0.5f;
	private float                 _downhillMultiplier_ = 2;
	private float                 _downHillThreshold_ = -7;
	private float                 _upHillThreshold = -7;

	private AnimationCurve        _slopePowerBySpeed_;
	private AnimationCurve        _UpHillByTime_;

	[Header("Air Movement Extras")]
	Vector2                       _airControlAmmount_;
	private bool                  _shouldStopAirMovementIfNoInput_ = false;
	private float                 _keepNormalForThis_ = 0.083f;
	private float                 _maxFallingSpeed_;
	private Vector3               _gravityWhenMovingUp_;
	[HideInInspector]
	public Vector3                _startFallGravity_;

	private float                 _jumpExtraControlThreshold_;
	private Vector2               _jumpAirControl_;
	private Vector2               _bounceAirControl_;

	[Header("Rolling Values")]
	float                         _rollingLandingBoost_;
	private float                 _rollingDownhillBoost_;
	private float                 _rollingUphillBoost_;
	private float                 _rollingTurningModifier_;
	private float                 _rollingDecel_;

	[Header("Stick To Ground")]
	private Vector2               _stickingLerps_ = new Vector2(0.885f, 1.5f);
	private float                 _stickingNormalLimit_ = 0.4f;
	private float                 _stickCastAhead_ = 1.9f;
	private AnimationCurve        _upwardsLimitByCurrentSlope_;
	[HideInInspector]
	public float                  _negativeGHoverHeight_ = 0.6115f;
	private float                 _rayToGroundDistance_ = 0.55f;
	private float                 _raytoGroundSpeedRatio_ = 0.01f;
	private float                 _raytoGroundSpeedMax_ = 2.4f;
	private float                 _rotationResetThreshold_ = -0.1f;
	private float                 _stepHeight_ = 0.6f;
	private float                 _groundDifferenceLimit_ = 0.3f;
	private float                 _landingConversionFactor_ = 2;

	[HideInInspector]
	public LayerMask              _Groundmask_;

	#endregion

	// Trackers
	#region trackers
	[HideInInspector]
	public bool                   _arePhysicsOn = true;         //If false, no changes to velocity will be calculated or applied.

	[HideInInspector]
	public Vector3                _coreVelocity;                //Core velocity is the velocity under the player's control. Whether it be through movement, actions or more. It cannot exceed maximum speed. Most calculations are based on this
	[HideInInspector]
	public Vector3                _environmentalVelocity;       //Environmental velocity is the velocity applied by external forces, such as springs, fans and more.
	[HideInInspector]
	public Vector3                _totalVelocity;               //The combination of core and environmetal velocity determening actual movement direction and speed in game.
	public Vector3                _prevTotalVelocity;           //The total velocity at the end of the previous frame, compared to Unity physics at the start of a frame to see if anything major like collision has changed movement.

	private List<Vector3>         _listOfVelocityToAddNextUpdate = new List<Vector3>(); //Rather than applied across all scripts, added forces are stored here and applied at the end of the frame.
	private List<Vector3>         _listOfCoreVelocityToAdd= new List<Vector3>();

	private Vector3               _externalSetVelocity;         //Same as adding, but overwrites all other calculations.
	private Vector3               _externalCoreVelocity;

	[HideInInspector]
	public float                  _speedMagnitude;              //The speed of the player at the end of the frame.
	[HideInInspector]
	public float                  _horizontalSpeedMagnitude;    //The speed of the player relative to the character transform, so only shows running speed.

	[HideInInspector]
	public Vector3                _moveInput;         //Assigned by the input script, the direction the player is trying to go.
	[HideInInspector]
	public Vector3                _trackMoveInput;    //Follows the input direction, and it is has changed but the controller input hasn't, that means the camera was moved to change direction.

	[HideInInspector]
	public float                  _currentTopSpeed;   //Player cannot exceed this speed by just running on flat ground. May be changed across gameplay.
	[HideInInspector]
	public float                  _currentMaxSpeed;   //Player's core velocity can not exceed this by any means.

	//Updated each frame to get current place on animation curves relevant to movement.
	private float                 _currentRunAccell;
	private float                 _currentRollAccell;
	public float                  _curvePosAcell;
	private float                 _curvePosDecell;
	[HideInInspector]
	public float                  _curvePosDrag;
	[HideInInspector]
	public float                  _curvePosSlopePower;


	[HideInInspector]
	public float                  _inputVelocityDifference = 1; //Referenced by other scripts to get the angle between player input and movement direction
	[HideInInspector]
	public Vector3                _playerPos;         //A quick reference to the players current location

	private float                 _timeUpHill;        //Tracks how long a player has been running up hill. Decreases when going down hill or on flat ground.

	//Ground tracking
	[HideInInspector]
	public bool                   _isGrounded;        //Used to check if the player is currently grounded. _isGrounded
	[HideInInspector]
	public bool                   _canBeGrounded = true;        //Set externally to prevent player's entering a grounded state.
	[HideInInspector]
	public RaycastHit             _HitGround;         //Used to check if there is ground under the player's feet, and gets data on it like the normal.
	[HideInInspector]
	public Vector3                _groundNormal;
	private Vector3               _keepNormal;        //Used when in the air to remember up direction when the ground was lost.
	private float                 _groundingDelay;    //Set when ground is lost and can't enter grounded state again until it's over.
	[HideInInspector]
	public float                  _timeOnGround;

	//Rotating in air
	private bool                  _isUpsideDown;                //Rotating to face up when upside down has a unique approach.
	private bool                  _isRotatingLeft;              //Which way to rotate around to face up from upside down.
	private Vector3               _rotateSidewaysTowards;       //The angle of rotation to follow when rotating from upside down
	private float                 _keepNormalCounter;           //Tracks how before rotating can begin

	//In air
	[HideInInspector] public bool _wasInAir;
	[HideInInspector]
	public bool                   _isGravityOn = true;
	[HideInInspector]
	public Vector3                _currentFallGravity;

	[HideInInspector]
	public bool                   _isRolling;         //Set by the rolling subaction, certain controls are different when rolling.

	//Disabling aspects of control. These are used as lists because if multiple things disable control, they all have to end it before that control is restored. If they just used single bools, multiple aspects taking control would overlap.
	[HideInInspector]
	public List<bool>             _listOfCanTurns = new List<bool>();
	[HideInInspector]
	public List<bool>             _listOfCanControl = new List<bool>();

	#endregion
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	//On start, assigns stats.
	private void Start () {
		_Tools = GetComponent<S_CharacterTools>();
		AssignTools();
		AssignStats();

		SetIsGrounded(false);
	}

	//On FixedUpdate,  call HandleGeneralPhysics if relevant.
	void FixedUpdate () {
		HandleGeneralPhysics();
	}

	//Sets public variables relevant to other calculations 
	void Update () {

		_playerPos = transform.position;

		//CheckForGround();
	}
	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Manages the character's physics, calling the relevant functions.
	void HandleGeneralPhysics () {
		if (!_arePhysicsOn) { return; }

		//Get curve positions, which will be used in calculations for this frame.
		_curvePosAcell = _AccelBySpeed_.Evaluate(_horizontalSpeedMagnitude / _currentTopSpeed);
		_curvePosDecell = _DecelBySpeed_.Evaluate(_horizontalSpeedMagnitude / _currentMaxSpeed);
		_curvePosDrag = _DragBySpeed_.Evaluate(_horizontalSpeedMagnitude / _currentMaxSpeed);
		_curvePosSlopePower = _slopePowerBySpeed_.Evaluate(_horizontalSpeedMagnitude / _currentMaxSpeed);

		//Set if the player is grounded based on current situation.
		CheckForGround();

		//If the rigidbody velocity is smaller than it was last frame (such as from hitting a wall),
		//Then apply the difference to the _corevelocity as well so it knows there's been a change and can make calculations based on it.
		Vector3 newVelocity = _RB.velocity;
		if (newVelocity.sqrMagnitude <= _prevTotalVelocity.sqrMagnitude && _prevTotalVelocity.sqrMagnitude > 1)
		{
			float angleChange = Vector3.Angle(newVelocity, _prevTotalVelocity) / 180;
			float sizeDifference = Mathf.Abs(newVelocity.magnitude - _speedMagnitude);
			float newSpeed = _RB.velocity.sqrMagnitude;
			// If the change is just making the player bounce off upwards into the air, then set it to zero.
			if (angleChange > 0.45 || (angleChange > 0.1 && Vector3.Angle(newVelocity, transform.up) < Vector3.Angle(_prevTotalVelocity, transform.up)))
			{
				_RB.velocity = Vector3.zero;
			}
			//But if the difference in speed is minor(such as lightly colliding with a slope when going up), then ignore the change.
			else if (sizeDifference < 10 && newSpeed > Mathf.Pow(15, 2))
			{
				_RB.velocity = _prevTotalVelocity;
			}

			Vector3 vectorDifference = _prevTotalVelocity - newVelocity;
			_coreVelocity -= vectorDifference;
		}

		//Calls the appropriate movement handler.
		if (_isGrounded)
			GroundMovement();
		else
			HandleAirMovement();

		AlignToGround(_groundNormal, _isGrounded);

		//After all other calculations are made across the scripts, the new velocities are applied to the rigidbody.
		SetTotalVelocity();
	}

	//Determines if the player is on the ground and sets _isGrounded to the answer.
	void CheckForGround () {

		if (_groundingDelay > 0)
		{
			_groundingDelay -= Time.fixedDeltaTime;
			return;
		}
		//Certain actions will prevent being grounded.
		if (!_canBeGrounded) { return; }

		//Sets the size of the ray to check for ground. If running on the ground then it is typically to avoid flying off the ground.
		float _rayToGroundDistancecor = _rayToGroundDistance_;
		if (_Action.whatAction == S_Enums.PrimaryPlayerStates.Default && _isGrounded)
		{
			_rayToGroundDistancecor = Mathf.Max(_rayToGroundDistance_ + (_horizontalSpeedMagnitude * _raytoGroundSpeedRatio_), _rayToGroundDistance_);
			_rayToGroundDistancecor = Mathf.Min(_rayToGroundDistancecor, _raytoGroundSpeedMax_);

		}

		//Uses the ray to check for ground, if found, sets grounded to true and takes the normal.
		if (Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out RaycastHit hitGroundTemp, 2f + _rayToGroundDistancecor, _Groundmask_))
		{
			if (Vector3.Angle(_groundNormal, hitGroundTemp.normal) / 180 < _groundDifferenceLimit_)
			{
				_HitGround = hitGroundTemp;
				SetIsGrounded(true);
				_groundNormal = _HitGround.normal;
				return;
			}
		}

		//If return is not called yet, then sets grounded to false.
		_groundNormal = Vector3.up;
		SetIsGrounded(false, 0.1f);
	}

	//After every other calculation has been made, all of the new velocities and combined and set to the rigidbody.
	//This includes the core and environmental velocities, but also the others that have been added into lists using the addvelocity methods.
	private void SetTotalVelocity () {

		//Core velocity that's been calculated across this script. Either assigns what it should be, or adds the stored force pushes.
		if (_externalCoreVelocity != default(Vector3))
		{
			_coreVelocity = _externalCoreVelocity;
			_externalCoreVelocity = default(Vector3);
		}
		else
		{
			foreach (Vector3 force in _listOfCoreVelocityToAdd)
			{
				_coreVelocity += force;
			}
		}

		//If the total velocity has  been preset at a different point this frame, then apply that, if not, calculate it through core and enviornmental.
		if (_externalSetVelocity != default(Vector3))
		{
			_totalVelocity = _externalSetVelocity;
			_externalSetVelocity = default(Vector3);
			_coreVelocity = _totalVelocity;
		}
		else
		{
			//Calculate total velocity this frame.
			_totalVelocity = _coreVelocity + _environmentalVelocity;
			foreach (Vector3 force in _listOfVelocityToAddNextUpdate)
			{
				_totalVelocity += force;
			}
		}

		//Clear the lists to prevent forces carrying over multiple frames.
		_listOfCoreVelocityToAdd.Clear();
		_listOfVelocityToAddNextUpdate.Clear();

		//Sets rigidbody, this should be the only line in the player scripts to do so.
		_RB.velocity = _totalVelocity;
		_prevTotalVelocity = _totalVelocity;

		//Assigns the global variables for the current movement, since it's assigned at the end of a frame, changes between frames won't be counted when using this,
		_speedMagnitude = _totalVelocity.magnitude;
		Vector3 releVec = GetRelevantVel(_RB.velocity);
		_horizontalSpeedMagnitude = new Vector3(releVec.x, 0f, releVec.z).magnitude;
	}

	//Calls all the methods involved in managing coreVelocity on the ground, such as normal control (with normal modifiers), sticking to the ground, and effects from slopes.
	private void GroundMovement () {

		_timeOnGround += Time.deltaTime;

		_coreVelocity = HandleControlledVelocity(new Vector2(1, 1));
		_coreVelocity = StickToGround(_coreVelocity);
		_coreVelocity = HandleSlopePhysics(_coreVelocity);
	}

	//Calls methods relevant to general control and gravity, while applying the turn and accelleration modifiers depending on a number of factors while in the air.
	private void HandleAirMovement () {

		//Gets the air control modifiers.
		float airAccelMod = _airControlAmmount_.y;
		float airTurnMod = _airControlAmmount_.x;
		switch (_Action.whatAction)
		{
			case S_Enums.PrimaryPlayerStates.Jump:
				if (_Action._actionTimeCounter < _jumpExtraControlThreshold_)
				{
					airAccelMod = _jumpAirControl_.y;
					airTurnMod = _jumpAirControl_.x;
				}
				break;
			case S_Enums.PrimaryPlayerStates.Bounce:
				airAccelMod = _bounceAirControl_.y;
				airTurnMod = _bounceAirControl_.x;
				break;
		}
		if (_horizontalSpeedMagnitude < 20)
		{
			airAccelMod += 0.5f;
		}

		//Handles lateral velocity.
		_coreVelocity = HandleControlledVelocity(new Vector2(airTurnMod, airAccelMod));

		//Apply Gravity (vertical velocity)
		if (_isGravityOn)
			_coreVelocity = SetGravity(_coreVelocity, _coreVelocity.y);


		//Clamp to max falling speed, so can't fall faster than this.
		_coreVelocity = new Vector3(_coreVelocity.x, Mathf.Clamp(_coreVelocity.y, _maxFallingSpeed_, _coreVelocity.y), _coreVelocity.z);
	}

	//Handles core velocity, which is the velocity directly under the player's control (seperate from environmental velocity which is placed on the character by other things).
	//This turns, decreases and/or increases the velocity based on input.
	Vector3 HandleControlledVelocity ( Vector2 modifier ) {

		//Certain actions control velocity in their own way.
		if (_listOfCanControl.Count != 0) { return _coreVelocity; }

		//Original by Damizean, edited by Blaephid

		//Gets current running velocity, then splits it into horizontal and vertical velocity relative to the character.
		//This means running up a wall will have zero vertical velocity because the character isn't moveing up relative to their rotation.Only the lateral velocity will be changed in this method.
		Vector3 localVelocity = transform.InverseTransformDirection(_coreVelocity);
		Vector3 lateralVelocity = new Vector3(localVelocity.x, 0.0f, localVelocity.z);
		Vector3 verticalVelocity = new Vector3(0.0f, localVelocity.y, 0.0f);

		//Apply changes to the lateral velocity based on input.
		lateralVelocity = HandleTurningAndAcceleration(lateralVelocity, _moveInput, modifier);
		lateralVelocity = HandleDecel(lateralVelocity, _moveInput, _isGrounded);

		// Clamp horizontal running speed. coreVelocity can never exceed the player moving laterally faster than this.
		localVelocity = lateralVelocity + verticalVelocity;
		if (_horizontalSpeedMagnitude > _currentMaxSpeed)
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
	Vector3 HandleTurningAndAcceleration ( Vector3 lateralVelocity, Vector3 input, Vector2 modifier ) {

		// Normalize to get input direction and magnitude seperately. For efficency and to prevent larger values at angles, the magnitude is based on the higher input.
		Vector3 inputDirection = input.normalized;
		float inputMagnitude = Mathf.Max(Mathf.Abs(input.x), Mathf.Abs(input.z));


		// Step 1) Determine angle between current lateral velocity and desired direction.
		//         Creates a quarternion which rotates to the direction, which will be identity if velocity is too slow.

		_inputVelocityDifference = Vector3.Angle(lateralVelocity, inputDirection);
		float deviationFromInput = _inputVelocityDifference  / 180.0f;
		Quaternion lateralToInput = lateralVelocity.sqrMagnitude < 1
			? Quaternion.identity
			: Quaternion.FromToRotation(lateralVelocity.normalized, inputDirection);


		//A list is used rather than a single boolean because if just one was used, anything that takes turning away would overlap. This way means all instances of turning being disabled must stop in order to regain control.
		if (_listOfCanTurns.Count == 0)
		{
			// Step 2) Rotate lateral velocity towards the same velocity under the desired rotation.
			//         The ammount rotated is determined by turn speed multiplied by turn rate (defined by the difference in angles, and current speed).
			//	Turn speed will also increase if the difference in pure input (ignoring camera) is different, allowing precise movement with the camera.

			float turnRate = (_isRolling ? _rollingTurningModifier_ : 1.0f);
			turnRate *= _TurnRateByAngle_.Evaluate(deviationFromInput);
			turnRate *= _TurnRateBySpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);
			if (_trackMoveInput != inputDirection)
			{
				turnRate *= _TurnRateByInputChange_.Evaluate(Vector3.Angle(_Input._inputWithoutCamera, _Input._prevInputWithoutCamera) / 180);
				_Input._prevInputWithoutCamera = _Input._inputWithoutCamera;
				_trackMoveInput = inputDirection;
			}
			lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, _turnSpeed_ * turnRate * Mathf.Deg2Rad * modifier.x, 0.0f);
		}

		// Step 3) Get current velocity (if it's zero then use input)
		//         Increase or decrease the size by using movetowards zero (a minus value increases size in the velocity direction)
		//         The total change is decided by acceleration based on input and speed, then drag from the turn.

		Vector3 setVelocity = lateralVelocity.sqrMagnitude > 0 ? lateralVelocity : inputDirection;
		float accelRate = 0;
		if (deviationFromInput < _angleToAccelerate_ || _horizontalSpeedMagnitude < 10)
		{
			accelRate = (_isRolling && _isGrounded ? _currentRollAccell : _currentRunAccell) * inputMagnitude;
			accelRate *= _curvePosAcell;
			accelRate *= _AccelBySlope_.Evaluate(_groundNormal.y);
		}
		float dragRate = _DragByAngle_.Evaluate(deviationFromInput) * _curvePosDrag;
		float speedChange = -(accelRate - (dragRate * _turnDrag_) * modifier.y);
		setVelocity = Vector3.MoveTowards(setVelocity, Vector3.zero, speedChange);


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
	private Vector3 HandleDecel ( Vector3 lateralVelocity, Vector3 input, bool isGrounded ) {
		float decelAmount = 0;
		//If there is no input, ready conventional deceleration.
		if (Mathf.Approximately(input.sqrMagnitude, 0))
		{
			if (isGrounded)
			{
				decelAmount = _moveDeceleration_ * _curvePosDecell;
			}
			else if (_shouldStopAirMovementIfNoInput_)
			{
				decelAmount = _airDecel_ * _curvePosDecell;
			}
		}
		//If grounded and rolling but not on a slope, even with input, ready deceleration. 
		else if (_isRolling && _groundNormal.y > _slopeEffectLimit_ && _horizontalSpeedMagnitude > 10)
		{
			decelAmount = _rollingDecel_ * _curvePosDecell;
		}
		//If in air, a constant deceleration is applied in addition to any others.
		if (!isGrounded && _horizontalSpeedMagnitude > 14)
		{
			decelAmount += _constantAirDecel_;
		}

		//Apply calculated deceleration
		return Vector3.MoveTowards(lateralVelocity, Vector3.zero, decelAmount);
	}

	//Handles interactions with slopes (non flat ground), both positive and negative, relative to the player's current rotation.
	//This includes adding force downhill, aiding or hampering running, as well as falling off when too slow.
	private Vector3 HandleSlopePhysics ( Vector3 worldVelocity ) {
		Vector3 slopeVelocity = Vector3.zero;

		//If just landed
		//Then apply additional speed dependant on slope angle.
		if (_wasInAir)
		{
			Vector3 addVelocity;
			if (!_isRolling)
			{
				addVelocity = _groundNormal * _landingConversionFactor_;
				addVelocity = AlignWithNormal(addVelocity, _groundNormal, _landingConversionFactor_);
			}
			else
			{
				addVelocity = (_groundNormal * _landingConversionFactor_) * _rollingLandingBoost_;
				addVelocity = AlignWithNormal(addVelocity, _groundNormal, _rollingLandingBoost_ * _landingConversionFactor_);
				_SoundController.SpinningSound();
			}
			//addVelocity.y = 0;
			slopeVelocity += addVelocity;
			_wasInAir = false;
		}

		//If moving too slow compared to the limit
		//Then fall off and away from the slope.
		if (_horizontalSpeedMagnitude < _SlopeSpeedLimitByAngle_.Evaluate(_groundNormal.y))
		{
			SetIsGrounded(false, 1f);
			AddTotalVelocity(_groundNormal * 5f);
			_keepNormalCounter = _keepNormalForThis_ - 0.1f;
		}

		//Slope power
		//If slope angle is less than limit, meaning on a slope
		//Then add a force directly downwards. This force is acquired through a number of calculations based on rolling, uphill or downhill, current speed, steepness, and more.
		//This force is then added to the current velocity, leading to a more realistic and natural effect than just changing speed.
		if (_groundNormal.y < _slopeEffectLimit_ && _horizontalSpeedMagnitude > 5)
		{
			Vector3 force = new Vector3(0, -_curvePosSlopePower, 0);
			force *= _generalHillMultiplier_;
			force *= ((1 - (Mathf.Abs(_groundNormal.y) / 10)) + 1);

			if (worldVelocity.y > _upHillThreshold)
			{
				_timeUpHill += Time.fixedDeltaTime;
				force *= _uphillMultiplier_;
				force *= _UpHillByTime_.Evaluate(_timeUpHill);
				if (!_isRolling)
				{
					force *= _rollingUphillBoost_;
				}
				if (_horizontalSpeedMagnitude < _currentTopSpeed)
				{
					force *= 1.5f;
				}
			}
			else if (worldVelocity.y < _downHillThreshold_)
			{
				_timeUpHill -= Mathf.Clamp(_timeUpHill - (Time.fixedDeltaTime * 0.7f), 0, _timeUpHill);
				force *= _downhillMultiplier_;
				if (!_isRolling)
				{
					force *= _rollingDownhillBoost_;
				}
			}
			//Force aims slightly towards down the slope (rather than just straight down). 
			Vector3 downSlopeForce = AlignWithNormal(new Vector3(_groundNormal.x, 0, _groundNormal.y), _groundNormal, force.y);
			force = Vector3.Lerp(force, downSlopeForce, 0.35f);

			slopeVelocity += force;

		}
		else { _timeUpHill = 0; }

		return worldVelocity + slopeVelocity;
	}

	//Handles the player's velocity following the path of the ground. This does not set the rotation to match it, but does prevent them from flying off or colliding with slopes.
	//This also handles stepping up over small ledges.
	private Vector3 StickToGround ( Vector3 velocity ) {

		//If moving and has been grounded for long enough.
		//Then ready a raycast to check for slopes.
		if (_timeOnGround > 0.06f && _horizontalSpeedMagnitude > 1)
		{
			float DirectionDif = Vector3.Angle(_RB.velocity.normalized, _HitGround.normal) / 180;
			Vector3 newGroundNormal = _HitGround.normal;
			Vector3 raycastStartPosition = _HitGround.point + (_HitGround.normal * 0.2f);
			Vector3 rayCastDirection = _RB.velocity.normalized;

			//If the Raycast Hits something, then there is a wall in front, that could be a slope.
			if (Physics.Raycast(raycastStartPosition, rayCastDirection, out RaycastHit hitSticking,
				_horizontalSpeedMagnitude * _stickCastAhead_ * Time.fixedDeltaTime, _Groundmask_))
			{
				float dif = Vector3.Angle(newGroundNormal, hitSticking.normal) / 180;
				float limit = _upwardsLimitByCurrentSlope_.Evaluate(newGroundNormal.y);

				//If the difference between current slope and encountered one is under the limit
				//Then it creates a velocity aligned to that new normal, then interpolates from the current to this new one.			
				if (dif < limit)
				{
					newGroundNormal = hitSticking.normal.normalized;
					Vector3 Dir = AlignWithNormal(velocity, newGroundNormal.normalized, velocity.magnitude);
					velocity = Vector3.Lerp(velocity, Dir, _stickingLerps_.x);
					transform.position = _HitGround.point + newGroundNormal * _negativeGHoverHeight_;
				}
				//If the difference is too large, then it's not a slope, so see if its a step to step over/onto.
				else
				{
					StepOver(raycastStartPosition, rayCastDirection, newGroundNormal);
				}
			}
			// If there is no wall, then we may be dealing with a positive slope (like the outside of a loop, where the ground is relatively lower).
			else if (_timeOnGround > 0.1f)
			{
				//If the difference between current movement and ground normal is less than the limit to stick (lower limits prevent super sticking).
				if (Mathf.Abs(DirectionDif) < _stickingNormalLimit_)
				{
					raycastStartPosition = raycastStartPosition + (rayCastDirection * (_horizontalSpeedMagnitude * 0.7f) * _stickCastAhead_ * Time.deltaTime);
					//Shoots a raycast from infront, but downwards to check for lower ground.
					//Then create a velocity relative to the current groundNormal, then lerp from one to the other.
					if (Physics.Raycast(raycastStartPosition, -_HitGround.normal, out hitSticking, 2.5f, _Groundmask_))
					{
						newGroundNormal = hitSticking.normal;
						Vector3 Dir = AlignWithNormal(velocity, newGroundNormal, velocity.magnitude);
						velocity = Vector3.LerpUnclamped(velocity, Dir, _stickingLerps_.y);

					}

					// Adds velocity downwards to remain on the slope.
					AddCoreVelocity(-newGroundNormal * 2, false);

				}
			}
		}
		return velocity;
	}

	//Makes a vector relative to a normal. Such as a forward direction being affected by the ground.
	Vector3 AlignWithNormal ( Vector3 vector, Vector3 normal, float magnitude ) {
		Vector3 tangent = Vector3.Cross(normal, vector);
		Vector3 newVector = -Vector3.Cross(normal, tangent);
		vector = newVector.normalized * magnitude;
		return vector;
	}

	//Handles stepping up onto slightly raised surfaces without losing momentum, rather than bouncing off them. Requires multiple checks into the situation to avoid clipping or unnecesary stepping.
	void StepOver ( Vector3 raycastStartPosition, Vector3 rayCastDirection, Vector3 newGroundNormal ) {

		//Shoots a boxcast rather than a raycast to check for steps slightly to the side as well as directly infront.
		if (Physics.BoxCast(raycastStartPosition, new Vector3(0.15f, 0.05f, 0.01f), rayCastDirection, out RaycastHit hitSticking,
				Quaternion.LookRotation(rayCastDirection, newGroundNormal), _horizontalSpeedMagnitude * _stickCastAhead_ * Time.fixedDeltaTime, _Groundmask_))
		{
			//Shoot a boxcast down from above and slightly continuing on from the impact point. If there is a lip, the wall infront is very short
			Vector3 rayStartPosition = hitSticking.point + (rayCastDirection * 0.15f) + (newGroundNormal * 1.25f) + (newGroundNormal * _CharacterCapsule.radius);
			if (Physics.SphereCast(rayStartPosition, _CharacterCapsule.radius, -newGroundNormal, out RaycastHit hitLip, 1.2f, _Groundmask_))
			{
				//if the lip is within step height and a similar angle to the current one, then it is a step
				float stepHeight = 1.5f - (hitLip.distance);
				float floorToStepDot = Vector3.Dot(hitLip.normal, newGroundNormal);
				if (stepHeight < _stepHeight_ && stepHeight > 0.05f && _horizontalSpeedMagnitude > 5f && floorToStepDot > 0.93f)
				{
					//Gets a position to place the player ontop of the step, then performs a box cast to check if there is enough empty space for the player to fit. 
					//Then move them to that position.
					Vector3 castPositionAtHit = rayStartPosition - (newGroundNormal * hitLip.distance);
					Vector3 newPosition = castPositionAtHit - (_groundNormal * _FeetTransform.localPosition.y);
					Vector3 boxSizeOfPlayerCollider = new Vector3 (_CharacterCapsule.radius, _CharacterCapsule.height + (_CharacterCapsule.radius * 2), _CharacterCapsule.radius);
					if (!Physics.BoxCast(newPosition, boxSizeOfPlayerCollider, rayCastDirection, Quaternion.LookRotation(rayCastDirection, transform.up), 0.4f))
					{
						transform.position = newPosition;
					}
				}
			}

		}

	}

	//Returns appropriate downwards force when in the air
	Vector3 SetGravity ( Vector3 velocity, float vertSpeed ) {

		//If falling down, return normal fall gravity.
		if (vertSpeed <= 0)
		{
			return velocity + _currentFallGravity;
		}
		//If currently moving up while in the air, apply a different (typically higher) gravity force with a slight increase dependant on upwards speed.
		else
		{
			float applyMod = Mathf.Clamp(1 + ((vertSpeed / 10) * 0.1f), 1, 3);
			Vector3 newGrav = new Vector3(_gravityWhenMovingUp_.x, _gravityWhenMovingUp_.y * applyMod, _gravityWhenMovingUp_.z);
			return velocity + newGrav;
		}
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//Changes the character's rotation to match the current situation. This includes when on a slope, when in the air, or transitioning between the two.
	public void AlignToGround ( Vector3 normal, bool isGrounded ) {

		//If on ground, then rotates to match the normal of the floor.
		if (isGrounded)
		{
			_keepNormal = normal;
			transform.rotation = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;

		}
		//If in the air, then stick to previous normal for a moment before rotating legs towards gravity. This avoids collision issues
		else
		{
			Vector3 localRight = _MainSkin.right;

			if (_keepNormalCounter < _keepNormalForThis_)
			{
				_keepNormalCounter += Time.deltaTime;
				transform.rotation = Quaternion.FromToRotation(transform.up, _keepNormal) * transform.rotation;

				//Upon counter ending, prepare to rotate to ground.
				if (_keepNormalCounter >= _keepNormalForThis_)
				{
					if (_keepNormal.y < _rotationResetThreshold_)
					{
						//Disabled turning until all the way over to prevent velocity changing because of the unqiue camera movement.
						if (!_isUpsideDown)
						{
							_isUpsideDown = true;
							_listOfCanTurns.Add(false);
						}

						// Going off the current rotation, can tell if needs to rotate right or left (rotate right if right side is higher than left), and prepare the angle to rotate around. 
						if (localRight.y >= 0)
						{
							_isRotatingLeft = false;
							_rotateSidewaysTowards = new Vector3(_RB.velocity.x, 0, _RB.velocity.z).normalized * -1;
						}
						else
						{
							_isRotatingLeft = true;
							_rotateSidewaysTowards = new Vector3(_RB.velocity.x, 0, _RB.velocity.z).normalized;
						}
					}
				}
			}
			else
			{
				//If upside down, then the player must rotate sideways, and not forwards. This keeps them facing the same way while pointing down to the ground again.
				if (_keepNormal.y < _rotationResetThreshold_)
				{

					//If the player was set to rotating right, and their right side is still higher than their left, then they have not flipped over all the way yet. Same with rotating left and lower left. 
					//Then get a cross product from preset rotating angle and transform up, then move in that direction.
					if ((!_isRotatingLeft && localRight.y >= 0) || (_isRotatingLeft && localRight.y < 0))
					{

						Vector3 cross = Vector3.Cross(_rotateSidewaysTowards, transform.up);
						Debug.DrawRay(transform.position, cross, Color.red, 10f);

						Quaternion targetRot = Quaternion.FromToRotation(transform.up, cross) * transform.rotation;
						transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 10f);
					}
					//When flipped over, set y value of the right side to zero to ensure not tilted anymore, and ready main rotation to sort any remaining rotation differences.
					else
					{
						transform.right = new Vector3(transform.right.x, 0, transform.right.z).normalized;
						_keepNormal = Vector3.up;
						//_canTurn = true;			
					}
				}
				//General rotation to face up again.
				else
				{
					Quaternion targetRot = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
					transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 10f);
					if (transform.rotation == targetRot && _isUpsideDown)
					{
						_isUpsideDown = false;
						_listOfCanTurns.Remove(false);
					}
				}
			}
		}
	}

	//Called anywhere to get what the input velocity is in the player's local space.
	public Vector3 GetRelevantVel ( Vector3 vel, bool includeY = true ) {
		vel = transform.InverseTransformDirection(vel);
		if (!includeY)
		{
			vel.y = 0;
		}
		return vel;
	}

	//Since there's such a difference between being grounded and not, this is called whenever the value is changed to affect any other relevant variables at the same time.
	public void SetIsGrounded ( bool value, float timer = 0 ) {
		if (_isGrounded != value)
		{
			//If changed to be in the air when was on the ground
			if (!_isGrounded && _isGrounded != value)
			{
				_groundingDelay = timer;
				_timeOnGround = 0;
			}
			//If changed to be on the ground when was in the air
			else if (_isGrounded && _isGrounded != value)
			{
				//If hasn't completed aligning to face upwards when was upside down, then end that prematurely and retern turning.
				if (_isUpsideDown)
				{
					_isUpsideDown = false;
					_listOfCanTurns.Remove(false);
				}
				_keepNormalCounter = 0;

				_OnGrounded.Invoke(); // Any methods attatched to the Unity event in editor will be called. These should all be called "EventOnGrounded".
			}
			_isGrounded = value;
		}
	}

	//the following methods are called by other scripts when they want to affect the velocity. The changes are stored and applied in the SetTotalVelocity method.
	public void AddTotalVelocity ( Vector3 force, bool shouldPrintForce = false ) {
		_listOfVelocityToAddNextUpdate.Add(force);
		if (shouldPrintForce) Debug.Log("Add Total FORCE");
	}
	public void AddCoreVelocity ( Vector3 force, bool shouldPrintForce = false ) {
		_listOfCoreVelocityToAdd.Add(force);
		if (shouldPrintForce) Debug.Log("ADD Core FORCE");
	}
	public void SetCoreVelocity ( Vector3 force, bool shouldPrintForce = false ) {
		_externalCoreVelocity = force;
		if (shouldPrintForce) Debug.Log("Set Core FORCE");
	}
	public void SetTotalVelocity ( Vector3 force, bool shouldPrintForce = false ) {
		_externalSetVelocity = force;
		if (shouldPrintForce) Debug.Log("Set Total FORCE");
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	//Matches all changeable stats to how they are set in the character stats script.
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
		_constantAirDecel_ = _Tools.Stats.DecelerationStats.naturalAirDecel;
		_airDecel_ = _Tools.Stats.DecelerationStats.airDecel;
		_slopeEffectLimit_ = _Tools.Stats.SlopeStats.slopeEffectLimit;
		_SlopeSpeedLimitByAngle_ = _Tools.Stats.SlopeStats.SpeedLimitBySlopeAngle;
		_generalHillMultiplier_ = _Tools.Stats.SlopeStats.generalHillMultiplier;
		_uphillMultiplier_ = _Tools.Stats.SlopeStats.uphillMultiplier;
		_downhillMultiplier_ = _Tools.Stats.SlopeStats.downhillMultiplier;
		_downHillThreshold_ = _Tools.Stats.SlopeStats.downhillThreshold;
		_upHillThreshold = _Tools.Stats.SlopeStats.uphillThreshold;
		_slopePowerBySpeed_ = _Tools.Stats.SlopeStats.SlopePowerByCurrentSpeed;
		_airControlAmmount_ = _Tools.Stats.WhenInAir.controlAmmount;

		_jumpExtraControlThreshold_ = _Tools.Stats.JumpStats.jumpExtraControlThreshold;
		_jumpAirControl_ = _Tools.Stats.JumpStats.jumpAirControl;
		_bounceAirControl_ = _Tools.Stats.BounceStats.bounceAirControl;

		_shouldStopAirMovementIfNoInput_ = _Tools.Stats.WhenInAir.shouldStopAirMovementWhenNoInput;
		_rollingLandingBoost_ = _Tools.Stats.RollingStats.rollingLandingBoost;
		_landingConversionFactor_ = _Tools.Stats.SlopeStats.landingConversionFactor;
		_rollingDownhillBoost_ = _Tools.Stats.RollingStats.rollingDownhillBoost;
		_rollingUphillBoost_ = _Tools.Stats.RollingStats.rollingUphillBoost;
		_rollingTurningModifier_ = _Tools.Stats.RollingStats.rollingTurningModifier;
		_rollingDecel_ = _Tools.Stats.DecelerationStats.rollingFlatDecell;
		_UpHillByTime_ = _Tools.Stats.SlopeStats.UpHillEffectByTime;
		_startFallGravity_ = _Tools.Stats.WhenInAir.fallGravity;
		_gravityWhenMovingUp_ = _Tools.Stats.WhenInAir.upGravity;
		_keepNormalForThis_ = _Tools.Stats.WhenInAir.keepNormalForThis;


		_stickingLerps_ = _Tools.Stats.GreedysStickToGround.stickingLerps;
		_stickingNormalLimit_ = _Tools.Stats.GreedysStickToGround.stickingNormalLimit;
		_stickCastAhead_ = _Tools.Stats.GreedysStickToGround.stickCastAhead;
		_negativeGHoverHeight_ = _Tools.Stats.GreedysStickToGround.negativeGHoverHeight;
		_rayToGroundDistance_ = _Tools.Stats.FindingGround.rayToGroundDistance;
		_raytoGroundSpeedRatio_ = _Tools.Stats.FindingGround.raytoGroundSpeedRatio;
		_raytoGroundSpeedMax_ = _Tools.Stats.FindingGround.raytoGroundSpeedMax;
		_rotationResetThreshold_ = _Tools.Stats.GreedysStickToGround.rotationResetThreshold;
		_Groundmask_ = _Tools.Stats.FindingGround.GroundMask;
		_upwardsLimitByCurrentSlope_ = _Tools.Stats.GreedysStickToGround.upwardsLimitByCurrentSlope;
		_stepHeight_ = _Tools.Stats.GreedysStickToGround.stepHeight;
		_groundDifferenceLimit_ = _Tools.Stats.FindingGround.groundDifferenceLimit;

		//Sets all changeable core values to how they are set to start in the editor.
		_currentRunAccell = _startAcceleration_;
		_currentRollAccell = _startRollAcceleration_;
		_currentTopSpeed = _startTopSpeed_;
		_currentMaxSpeed = _startMaxSpeed_;
		_maxFallingSpeed_ = _startMaxFallingSpeed_;
		_currentFallGravity = _startFallGravity_;

		_keepNormal = Vector3.up;


	}

	private void AssignTools () {
		s_MasterPlayer = this;
		_RB = GetComponent<Rigidbody>();
		_Action = GetComponent<S_ActionManager>();
		_SoundController = _Tools.SoundControl;
		_CharacterCapsule = _Tools.characterCapsule.GetComponent<CapsuleCollider>();
		_FeetTransform = _Tools.FeetPoint;
		_Input = GetComponent<S_PlayerInput>();
		_MainSkin = _Tools.mainSkin;
		_camHandler = GetComponent<S_Handler_Camera>();
	}

	#endregion
}
