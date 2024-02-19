using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Windows;
using UnityEditor;

public class S_PlayerPhysics : MonoBehaviour
{

	/// <summary>
	/// Members ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region members

	//Unity
	#region Unity Specific Members
	[HideInInspector] public S_ActionManager _Action;
	S_CharacterTools _Tools;
	Transform _MainSkin;

	static public S_PlayerPhysics s_MasterPlayer;
	public Rigidbody _RB { get; set; }

	[HideInInspector] public RaycastHit hitGround;

	S_Control_SoundsPlayer _SoundController;

	CapsuleCollider _CharacterCapsule;
	private Transform               _FeetTransform;
	#endregion


	//General
	#region General Members

	//Stats
	#region Stats

	[Header("Grounded Movement")]
	private float                 _startAcceleration_ = 12f;
	private float                 _startRollAcceleration_ = 4f;
	private float                 _runAccell;
	private float                 _rollAccell;
	private AnimationCurve        _accelOverSpeed_;
	private float                 _accelShiftOverSpeed_;
	private float                 _decelShiftOverSpeed_;

	[HideInInspector]
	public float                  _moveDeceleration_ = 1.3f;
	AnimationCurve                _decelBySpeed_;
	private float                 _airDecell_ = 1.05f;
	private float                 _naturalAirDecel_ = 1.01f;

	private float                 _tangentialDrag_;
	private float                 _tangentialDragShiftSpeed_;
	private float                 _turnSpeed_ = 16f;
	private AnimationCurve        _TurnRateByAngle_;
	private AnimationCurve        _TurnRateBySpeed_;
	private AnimationCurve        _tangDragOverAngle_;
	private AnimationCurve        _tangDragOverSpeed_;

	private float                 _startTopSpeed_ = 65f;
	private float                 _startMaxSpeed_ = 230f;
	private float                 _startMaxFallingSpeed_ = -500f;

	[Header("Slopes")]
	private float                 _slopeEffectLimit_ = 0.9f;
	private float                  _standOnSlopeLimit_ = 0.8f;
	private float                 _slopePower_ = 0.5f;
	private float                 _slopeRunningAngleLimit_ = 0.5f;
	private AnimationCurve        _slopeSpeedLimit_;

	private float                 _generalHillMultiplier_ = 1;
	private float                 _uphillMultiplier_ = 0.5f;
	private float                 _downhillMultiplier_ = 2;
	private float                 _downHillThreshold_ = -7;
	private float                 _upHillThreshold = -7;

	private AnimationCurve        _slopePowerOverSpeed_;
	private AnimationCurve        _UpHillByTime_;

	[Header("Air Movement Extras")]
	Vector2                         _airControlAmmount_;
	private bool                  _shouldStopAirMovementIfNoInput_ = false;
	private float                 _keepNormalForThis_ = 0.083f;
	private float                 _maxFallingSpeed_;
	[HideInInspector]
	public float                  _homingDelay_;
	private Vector3               _upGravity_;
	[HideInInspector]
	public Vector3                _startFallGravity_;
	[HideInInspector]
	public Vector3                _fallGravity_;

	private float                 _jumpExtraControlThreshold_;
	private Vector2               _jumpAirControl_;
	private Vector2               _bounceAirControl_;

	[Header("Rolling Values")]
	float                         _rollingLandingBoost_;
	private float                 _rollingDownhillBoost_;
	private float                 _rollingUphillBoost_;
	private float                 _rollingStartSpeed_;
	private float                 _rollingTurningDecrease_;
	private float                 _rollingDecel_;
	private float                 _slopeTakeoverAmount_; // This is the normalized slope angle that the player has to be in order to register the land as "flat"

	[Header("Stick To Ground")]
	private Vector2               _stickingLerps_ = new Vector2(0.885f, 1.5f);
	private float                 _stickingNormalLimit_ = 0.4f;
	private float                 _stickCastAhead_ = 1.9f;
	private AnimationCurve                _upwardsLimitByCurrentSlope_;
	[HideInInspector]
	public float                  _negativeGHoverHeight_ = 0.6115f;
	private float                 _rayToGroundDistance_ = 0.55f;
	private float                 _raytoGroundSpeedRatio_ = 0.01f;
	private float                 _raytoGroundSpeedMax_ = 2.4f;
	private float                 _rayToGroundRotDistance_ = 1.1f;
	private float                 _raytoGroundRotSpeedMax_ = 2.6f;
	private float                 _rotationResetThreshold_ = -0.1f;
	private float                 _stepHeight_ = 0.6f;
	private float                 _groundDifferenceLimit_ = 0.3f;

	[HideInInspector]
	public LayerMask              _Groundmask_;

	#endregion

	// Trackers
	#region trackers
	[HideInInspector]
	public bool                   _arePhysicsOn = true;

	[HideInInspector]
	public Vector3                _coreVelocity;
	[HideInInspector]
	public Vector3                _environmentalVelocity;
	[HideInInspector]
	public Vector3                _totalVelocity;
	public Vector3                _prevTotalVelocity;
	private List<Vector3>         _listOfVelocityToAddNextUpdate = new List<Vector3>();
	private List<Vector3>         _listOfCoreVelocityToAdd= new List<Vector3>();
	private Vector3               _externalSetVelocity;
	private Vector3               _externalCoreVelocity;

	private Vector3               _rayDebugPosition;

	public float _speedMagnitude { get; set; }
	public float _horizontalSpeedMagnitude { get; set; }

	public Vector3 _moveInput { get; set; }
	public Vector3 PreviousInput { get; set; }
	public Vector3 RawInput { get; set; }
	public Vector3 PreviousRawInput { get; set; }
	[HideInInspector]
	public float                  _currentTopSpeed;
	[HideInInspector]
	public float                  _currentMaxSpeed;
	public float curvePosAcell { get; set; }
	private float                 curvePosDecell = 1f;
	public float curvePosTang { get; set; }
	public float curveSlopePower { get; set; }
	[HideInInspector]
	public float                  _inputVelocityDifference = 1;
	public Vector3 b_normalVelocity { get; set; }
	public Vector3 b_tangentVelocity { get; set; }
	[Tooltip("A quick reference to the players current location")]
	public Vector3 _playerPos { get; set; }

	private float                 _timeUpHill;
	private float                 _slopePowerShiftSpeed;
	private float                 _landingConversionFactor = 2;

	[Tooltip("Used to check if the player is currently grounded. _isGrounded")]
	public bool _isGrounded { get; set; }
	[HideInInspector]
	public bool                   _canBeGrounded = true;
	public Vector3 _groundNormal { get; set; }
	public Vector3 _collisionPointsNormal { get; set; }
	private Vector3               _keepNormal;
	private float                 _KeepNormalCounter;
	public bool _wasInAir { get; set; }
	[HideInInspector]
	public bool                   _isGravityOn = true;

	public bool _isRolling { get; set; }

	[Header("Greedy Stick Fix")]
	public bool                   _EnableDebug;
	public float _TimeOnGround { get; set; }
	private bool                  _isRotatingSideways;
	[HideInInspector]

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
		_MainSkin = _Tools.mainSkin;

		SetIsGrounded(false);
	}

	//On FixedUpdate,  call HandleGeneralPhysics if relevant.
	void FixedUpdate () {
		HandleGeneralPhysics();

		if (_homingDelay_ > 0)
		{
			_homingDelay_ -= Time.deltaTime;
		}
	}

	//Sets public variables relevant to other calculations 
	void Update () {

		_playerPos = transform.position;

		CheckForGround();
	}

	//Involed in sticking calculations
	#region TRYSCRAP
	//public void OnCollisionStay ( Collision col ) {
	//	Vector3 prevNormal = _groundNormal;
	//	foreach (ContactPoint contact in col.contacts)
	//	{

	//		//Set Middle Point
	//		Vector3 pointSum = Vector3.zero;
	//		Vector3 normalSum = Vector3.zero;
	//		for (int i = 0 ; i < col.contacts.Length ; i++)
	//		{
	//			pointSum = pointSum + col.contacts[i].point;
	//			normalSum = normalSum + col.contacts[i].normal;
	//		}

	//		pointSum = pointSum / col.contacts.Length;
	//		_collisionPointsNormal = normalSum / col.contacts.Length;

	//		if (rb.velocity.normalized != Vector3.zero)
	//		{
	//			CollisionPoint.position = pointSum;
	//		}
	//	}
	//}
	#endregion
	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Manages the character's physics, calling the relevant functions.
	void HandleGeneralPhysics () {
		if (!_arePhysicsOn) { return; }
		//Set Curve thingies
		//curvePosAcell = Mathf.Lerp(curvePosAcell, _accelOverSpeed_.Evaluate((rb.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _accelShiftOverSpeed_);
		//curvePosDecell = Mathf.Lerp(curvePosDecell, _decelBySpeed_.Evaluate((rb.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _decelShiftOverSpeed_);
		//curvePosTang = Mathf.Lerp(curvePosTang, _tangDragOverSpeed_.Evaluate((rb.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _tangentialDragShiftSpeed_);
		//curvePosSlope = Mathf.Lerp(curvePosSlope, _slopePowerOverSpeed_.Evaluate((rb.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _slopePowerShiftSpeed);

		//Get curve positions, which will be used in calculations for this frame.
		curvePosAcell = _accelOverSpeed_.Evaluate(_horizontalSpeedMagnitude / _currentMaxSpeed);
		curvePosDecell = _decelBySpeed_.Evaluate(_horizontalSpeedMagnitude / _currentMaxSpeed);
		curvePosTang = _tangDragOverSpeed_.Evaluate(_horizontalSpeedMagnitude / _currentMaxSpeed);
		curveSlopePower = _slopePowerOverSpeed_.Evaluate(_horizontalSpeedMagnitude / _currentMaxSpeed);

		//// Clamp horizontal running speed.
		//if (_horizontalSpeedMagnitude > _currentMaxSpeed)
		//{
		//	Vector3 ReducedSpeed = _RB.velocity;
		//	float keepY = _RB.velocity.y;
		//	ReducedSpeed = Vector3.ClampMagnitude(ReducedSpeed, _currentMaxSpeed);
		//	ReducedSpeed.y = keepY;
		//	_RB.velocity = ReducedSpeed;
		//}

		//Do it for Y
		//if (Mathf.Abs(rb.velocity.y) > MaxFallingSpeed)
		//{
		//    Vector3 ReducedSpeed = rb.velocity;
		//    float keepX = rb.velocity.x;
		//    float keepZ = rb.velocity.z;
		//    ReducedSpeed = Vector3.ClampMagnitude(ReducedSpeed, MaxSpeed);
		//    ReducedSpeed.x = keepX;
		//    ReducedSpeed.z = keepZ;
		//    rb.velocity = ReducedSpeed;
		//}

		CheckForGround();

		_rayDebugPosition = transform.position;

		//If the rigidbody velocity is smaller than it was last frame (such as from hitting a wall),
		//Then apply the difference to the _corevelocity as well so it knows there's been a change.
		//But if the difference is minor (such as lightly colliding with a slope when going up), then ignore change.
		Vector3 newVelocity = _RB.velocity;
		if (newVelocity.sqrMagnitude <= _prevTotalVelocity.sqrMagnitude && _prevTotalVelocity.sqrMagnitude > 1)
		{
			float angleChange = Vector3.Angle(newVelocity, _prevTotalVelocity) / 180;
			float sizeDifference = Mathf.Abs(_RB.velocity.magnitude - _speedMagnitude);
			float newSpeed = _RB.velocity.sqrMagnitude;

			if (angleChange > 0.45 || (angleChange > 0.1 && Vector3.Angle(newVelocity, transform.up) < Vector3.Angle(_prevTotalVelocity, transform.up)))
			{
				_RB.velocity = Vector3.zero;
			}
			else if(sizeDifference < 10 && newSpeed > Mathf.Pow(15, 2))
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
		AlignToGround(_groundNormal);

		//StartCoroutine(setVelocityAtEndOfUpdate());
		SetTotalVelocity();
	}

	//Determines if the player is on the ground and sets _isGrounded to the answer.
	void CheckForGround () {

		//If currently moving upwards in the air, then cannot be grounded
		if (!_canBeGrounded) { return; }

		//Sets the size of the ray to check for ground. 
		float _rayToGroundDistancecor = _rayToGroundDistance_;
		if (_Action.whatAction == S_Enums.PlayerStates.Regular && _isGrounded)
		{
			//grounder line
			_rayToGroundDistancecor = Mathf.Max(_rayToGroundDistance_ + (_horizontalSpeedMagnitude * _raytoGroundSpeedRatio_), _rayToGroundDistance_);
			_rayToGroundDistancecor = Mathf.Min(_rayToGroundDistancecor, _raytoGroundSpeedMax_);

		}

		//Uses the ray to check for ground, if found, sets grounded to true and takes the normal.
		if (Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out RaycastHit hitGroundTemp, 2f + _rayToGroundDistancecor, _Groundmask_))
		{
			if (Vector3.Angle(_groundNormal, hitGroundTemp.normal) / 180 < _groundDifferenceLimit_)
			{
				hitGround = hitGroundTemp;
				SetIsGrounded(true);
				_groundNormal = hitGround.normal;
				return;
			}
		}

		//If return is not called yet, then sets grounded to false.
		_groundNormal = Vector3.up;
		SetIsGrounded(false);

	}

	IEnumerator SetVelocityAtEndOfUpdate () {
		yield return new WaitForEndOfFrame();
		SetTotalVelocity();
	}

	//After every other calculation has been made, all of the new velocities and combined and set to the rigidbody.
	//This includes the core and environmental velocities, but also the others that have been added into a list using the addvelocity method.
	private void SetTotalVelocity () {

		//Core velocity. Either assigns what it should be, or adds the stored force pushes.
		if (_externalCoreVelocity != Vector3.zero)
		{
			_coreVelocity = _externalCoreVelocity;
			_externalCoreVelocity = Vector3.zero;
		}
		else
		{
			foreach (Vector3 force in _listOfCoreVelocityToAdd)
			{
				_coreVelocity += force;
			}
		}
		_listOfCoreVelocityToAdd.Clear();

		//Total velocity.
		if (_externalSetVelocity != Vector3.zero)
		{
			_totalVelocity = _externalSetVelocity;
			_externalSetVelocity = Vector3.zero;
		}
		else
		{
			_totalVelocity = _coreVelocity + _environmentalVelocity;

			foreach (Vector3 force in _listOfVelocityToAddNextUpdate)
			{
				_totalVelocity += force;
			}

		}
		_listOfVelocityToAddNextUpdate.Clear();

		//Sets rigidbody, this should be the only line in the player scripts to do so.
		_RB.velocity = _totalVelocity;
		_prevTotalVelocity = _totalVelocity;

		//Assigns the global variables the current movement, since it's assigned at the end of a frame, changes between frames won't be counted when using this,
		//But the issues here should be minor (E.G. running into a wall leads to RB.velocity 0, but this won't be, until the end of the frame.
		_speedMagnitude = _totalVelocity.magnitude;
		Vector3 releVec = GetRelevantVec(_RB.velocity);
		_horizontalSpeedMagnitude = new Vector3(releVec.x, 0f, releVec.z).magnitude;
	}

	//Calls all the functions involved in managing coreVelocity on the ground, such as normal control (with normal modifiers), sticking to the ground, and effects from slopes.
	private void GroundMovement () {

		_TimeOnGround += Time.deltaTime;

		_coreVelocity = HandleControlledVelocity(_moveInput, new Vector2(1, 1));
		_coreVelocity = StickToGround(_coreVelocity);
		_coreVelocity = HandleSlopePhysics(_coreVelocity);
	}

	//Handles core velocity, which is the velocity directly under the player's control (seperate from environmental velocity which is placed on the character by other things).
	//This decreases or increases the velocity based on input.
	Vector3 HandleControlledVelocity ( Vector3 input, Vector2 modifier ) {

		if (_Action.whatAction == S_Enums.PlayerStates.JumpDash || _Action.whatAction == S_Enums.PlayerStates.WallRunning) { return _coreVelocity; }

		//Original by Damizean, edited by Blaephid

		//Gets current running velocity, then splits it into horizontal and vertical velocity relative to the character.
		//This means running up a wall will have zero vertical velocity because the character isn't moveing up relative to their rotation.
		Vector3 velocity = _coreVelocity;

		Vector3 localVelocity = transform.InverseTransformDirection(velocity);
		Vector3 lateralVelocity = new Vector3(localVelocity.x, 0.0f, localVelocity.z);
		Vector3 verticalVelocity = new Vector3(0.0f, localVelocity.y, 0.0f);

		lateralVelocity = HandleTurningAndAccel(lateralVelocity, input, modifier);
		lateralVelocity = HandleDecel(lateralVelocity, input, _isGrounded);


		// Compose local velocity back and compute velocity back into the Global frame.
		localVelocity = lateralVelocity + verticalVelocity;
		velocity = transform.TransformDirection(localVelocity);

		// Clamp horizontal running speed.
		if (_horizontalSpeedMagnitude > _currentMaxSpeed)
		{
			Vector3 ReducedSpeed = velocity;
			float keepY = velocity.y;
			ReducedSpeed = Vector3.ClampMagnitude(ReducedSpeed, _currentMaxSpeed);
			ReducedSpeed.y = keepY;
			velocity = ReducedSpeed;
		}

		return velocity;
	}

	//This handles increasing the speed while changing the direction of the player's controlled velocity.
	//It will not allow speed to increase if over topSpeed, but will only decrease if there is enough drag from the turn.
	Vector3 HandleTurningAndAccel ( Vector3 lateralVelocity, Vector3 input, Vector2 modifier ) {
		// If there is some input...

		// Normalize to get input direction and magnitude seperately
		Vector3 inputDirection = input.normalized;
		float inputMagnitude = input.magnitude;

		// Step 1) Determine angle between current lateral velocity and desired direction.
		//         Creates a quarternion which rotates to the direction, which will be zero if velocity is too slow.

		float deviationFromInput = Vector3.Angle(lateralVelocity, inputDirection) / 180.0f;
		Quaternion lateralToInput = Mathf.Approximately(lateralVelocity.sqrMagnitude, 0.0f)
			? Quaternion.identity
			: Quaternion.FromToRotation(lateralVelocity.normalized, inputDirection);

		// Step 2) Rotate lateral velocity towards the same velocity under the desired rotation.
		//         The ammount rotated is determined by turn speed multiplied by turn rate (defined by the difference in angles, and current speed).


		float turnRate = _TurnRateByAngle_.Evaluate(deviationFromInput);
		turnRate *= _TurnRateBySpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);
		lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, _turnSpeed_ * turnRate * Time.deltaTime * modifier.x, 0.0f);


		// Step 3) Further lateral velocity into normal (in the input direction) and tangential
		//         components. Note: normalSpeed is the magnitude of normalVelocity, with the added
		//         bonus that it's signed. If positive, the speed goes towards the same
		//         direction than the input :)

		//float normalSpeed = Vector3.Dot(lateralVelocity, inputDirection);
		//Vector3 normalVelocity = inputDirection * normalSpeed;
		//Vector3 tangentVelocity = lateralVelocity - normalVelocity;
		//float tangentSpeed = tangentVelocity.magnitude;

		//Debug.DrawRay(transform.position, tangentVelocity, Color.red);

		// Step 4) Apply user control in this direction.

		//if (normalSpeed < _currentTopSpeed)
		//{
		//	// Accelerate towards the input direction.
		//	normalSpeed += (_isRolling ? 0 : _moveAccell) * inputMagnitude;

		//	normalSpeed = Mathf.Min(normalSpeed, _currentTopSpeed);

		//	// Rebuild back the normal velocity with the correct modulus.

		//	normalVelocity = inputDirection * normalSpeed;
		//}
		Vector3 setVelocity = lateralVelocity.sqrMagnitude > 0 ? lateralVelocity : inputDirection;
		float accelRate = (_isRolling ? _rollAccell : _runAccell) * inputMagnitude;
		float dragRate = _tangDragOverAngle_.Evaluate(deviationFromInput) * _tangDragOverSpeed_.Evaluate((_currentMaxSpeed * _currentMaxSpeed));
		float speedChange = -(accelRate - (dragRate * _tangentialDrag_) * modifier.y);

		//setVelocity = Vector3.MoveTowards(setVelocity, setVelocity.normalized * 10000, accelRate);
		setVelocity = Vector3.MoveTowards(setVelocity, Vector3.zero, speedChange);

		if (setVelocity.sqrMagnitude < _currentTopSpeed * _currentTopSpeed)
		{
			lateralVelocity = setVelocity;
		}
		else if (setVelocity.sqrMagnitude < lateralVelocity.sqrMagnitude)
		{
			lateralVelocity = setVelocity;
		}

		return lateralVelocity;
	}

	//Handles decreasing the magnitude of the player's controlled velocity, usually only if there is no input, but other circumstances may decrease speed as well.
	// Deceleration is calculated, then applied at the end of the method.
	private Vector3 HandleDecel ( Vector3 lateralVelocity, Vector3 input, bool isGrounded ) {
		float DecellAmount = 0;
		//If there is no input, apply conventional deceleration.
		if (Mathf.Approximately(input.sqrMagnitude, 0))
		{
			if (isGrounded)
			{
				DecellAmount = _moveDeceleration_ * curvePosDecell;
			}
			else
			{
				DecellAmount = _airDecell_ * curvePosDecell;
			}
		}
		//If grounded and rolling but not on a slope, even with input, ready deceleration. 
		else if (_isRolling && _groundNormal.y > _slopeTakeoverAmount_ && _horizontalSpeedMagnitude > 10)
		{
			DecellAmount = _rollingDecel_ * curvePosDecell;
		}
		//If in air, a constant deceleration is applied in addition to any others.
		if (!isGrounded && _horizontalSpeedMagnitude > 14)
		{
			DecellAmount += _naturalAirDecel_;
		}
		return Vector3.MoveTowards(lateralVelocity, Vector3.zero, DecellAmount);
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
				addVelocity = _groundNormal * _landingConversionFactor;
				addVelocity = AlignWithNormal(addVelocity, _groundNormal, _landingConversionFactor);
			}
			else
			{
				addVelocity = (_groundNormal * _landingConversionFactor) * _rollingLandingBoost_;
				addVelocity = AlignWithNormal(addVelocity, _groundNormal, _rollingLandingBoost_);
				_SoundController.SpinningSound();
			}
			//addVelocity.y = 0;
			slopeVelocity += addVelocity;
			_wasInAir = false;
		}

		//If moving too slow compared to the limit
		//Then fall off and away from the slope.
		if (_horizontalSpeedMagnitude < _slopeSpeedLimit_.Evaluate(_groundNormal.y))
		{
			SetIsGrounded(false);
			AddTotalVelocity(_groundNormal * 2f);
			_KeepNormalCounter = _keepNormalForThis_ - 0.1f;
		}

		//Slope power
		//If slope angle is less than limit, meaning on a slope
		//Then add a force directly downwards. This force is acquired through a number of calculations based on rolling, uphill or downhill, current speed, steepness, and more.
		//This force is then added to the current velocity, leading to a more realistic and natural effect than just changing speed.
		if (_groundNormal.y < _slopeEffectLimit_ && _horizontalSpeedMagnitude > 5)
		{
			Vector3 force = new Vector3(0, -curveSlopePower, 0);
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
				if(_horizontalSpeedMagnitude < _currentTopSpeed)
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
			//If not upside down, then force turns slightly towards down the slope (rather than just straight down). 
			//if (_groundNormal.y < 0) {
			Vector3 downSlopeForce = AlignWithNormal(new Vector3(_groundNormal.x, 0, _groundNormal.y), _groundNormal, force.y);
			Debug.DrawRay(transform.position, downSlopeForce.normalized * 2, Color.black, 5f);
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
		if (_TimeOnGround > 0.06f && _horizontalSpeedMagnitude > 1)
		{
			float DirectionDot = Vector3.Dot(_RB.velocity.normalized, hitGround.normal);
			Vector3 newGroundNormal = hitGround.normal;
			Vector3 raycastStartPosition = hitGround.point + (hitGround.normal * 0.2f);
			Vector3 rayCastDirection = _RB.velocity.normalized;

			//If the Raycast Hits something, it adds it's normal to the ground normal making an inbetween value the interpolates the direction;
			//if (Physics.Raycast(raycastStartPosition, _RB.velocity.normalized, out RaycastHit hitSticking, _speedMagnitude * _stickCastAhead_ * Time.deltaTime, _Groundmask_))
			if (Physics.Raycast(raycastStartPosition, rayCastDirection, out RaycastHit hitSticking,
				_horizontalSpeedMagnitude * _stickCastAhead_ * Time.fixedDeltaTime, _Groundmask_))
			{
				float dif = Vector3.Angle(newGroundNormal, hitSticking.normal) / 180;
				float limit = _upwardsLimitByCurrentSlope_.Evaluate(newGroundNormal.y);

				//If the difference between current slope and encountered one is under the limit specific to that current slope
				//(if slope is flat ground, lower limit, this is to make this happen more when heading downhill then to sudden flatground)
				//Then it creates a velocity aligned to that new normal, then interpolates from the current to this new one.			
				if (dif < limit)
				{
					newGroundNormal = hitSticking.normal.normalized;
					Vector3 Dir = AlignWithNormal(velocity, newGroundNormal.normalized, velocity.magnitude);
					velocity = Vector3.Lerp(velocity, Dir, _stickingLerps_.x);
					transform.position = hitGround.point + newGroundNormal * _negativeGHoverHeight_;
				}
				//If the difference is too large, see it its a step to step over/onto.
				else
				{
					
					StepOver(raycastStartPosition, rayCastDirection, newGroundNormal);
				}
			}

			//Positive slopes (like the outside of a loop, where the player starts facing more downwards).
			else if (_TimeOnGround > 0.1f)
			{
				//If the difference between current movement and ground normal is less than the limit to stick (lower limits prevent super sticking).
				if (Mathf.Abs(DirectionDot) < _stickingNormalLimit_)
				{
					raycastStartPosition = raycastStartPosition + (rayCastDirection * (_horizontalSpeedMagnitude * 0.7f) * _stickCastAhead_ * Time.deltaTime);
					//Shoots a raycast from infront, but downwards to check for lower ground.
					//Then create a velocity relative to the current groundNormal, then lerp from one to the other.
					//Also apply force downwards to stick.
					if (Physics.Raycast(raycastStartPosition, -hitGround.normal, out hitSticking, 2.5f, _Groundmask_))
					{
						newGroundNormal = hitSticking.normal;
						Vector3 Dir = AlignWithNormal(velocity, newGroundNormal, velocity.magnitude);
						float lerp = _stickingLerps_.y;

						if (hitSticking.distance > 1.5f)
						{
							lerp = 1.05f;
							//transform.position = hitGround.point + groundNormal * (_CharacterCapsule.height / 2f + _negativeGHoverHeight_);
							_RB.position = hitGround.point + newGroundNormal * _negativeGHoverHeight_;
						}
						velocity = Vector3.LerpUnclamped(velocity, Dir, lerp);
						//_groundNormal = Vector3.Lerp(_groundNormal, newGroundNormal, _stickingLerps_.x);
						AddCoreVelocity(-newGroundNormal * 2, false); // Adds velocity downwards to remain on the slope.
						return velocity;

					}

				}
			}
		}
		return velocity;
	}

	//Makes a vector relative to a normal. Such as a forward direction being affected by the ground.
	Vector3 AlignWithNormal ( Vector3 vector, Vector3 normal, float magnitude ) {

		//typically used to rotate a movement vector by a surface normal
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
			//Shoot a boxcast down from above and slightly continuing on from the impact point. If there is a lip, the wall infront is very short-
			Vector3 rayStartPosition = hitSticking.point + (rayCastDirection * 0.15f) + (newGroundNormal * 1.25f) + (newGroundNormal * _CharacterCapsule.radius);
			if (Physics.SphereCast(rayStartPosition, _CharacterCapsule.radius, -newGroundNormal, out RaycastHit hitLip, 1.2f, _Groundmask_))
			{
				//if the ledge is within step height and a similar angle to the current one-
				float stepHeight = 1.5f - (hitLip.distance);
				float floorToStepDot = Vector3.Dot(hitLip.normal, newGroundNormal);
				if (stepHeight < _stepHeight_ && stepHeight > 0.05f && _horizontalSpeedMagnitude > 5f && floorToStepDot > 0.93f)
				{
					//Gets a position to place the player ontop of the step, then performs a box cast to check if there is enough empty space for the player to fit. 
					//Then move them to that position.
					Vector3 castPositionAtHit = rayStartPosition - (newGroundNormal * hitLip.distance);
					Vector3 newPosition = castPositionAtHit - (_groundNormal * _FeetTransform.localPosition.y);
					Vector3 boxSizeOfPlayerCollider = new Vector3 (_CharacterCapsule.radius, _CharacterCapsule.height + (_CharacterCapsule.radius * 2), _CharacterCapsule.radius);
					if(!Physics.BoxCast(newPosition, boxSizeOfPlayerCollider, rayCastDirection, Quaternion.LookRotation(rayCastDirection, transform.up), 0.4f))
					{
						transform.position = newPosition;
					}
				}
			}

		}

	}

	//Calls methods relevant to general control and gravity, while applying the turn and accelleration modifiers depending on a number of factors while in the air.
	void HandleAirMovement () {

		//Gets the air control modifiers.
		float airAccelMod = _airControlAmmount_.y;
		float airTurnMod = _airControlAmmount_.x;
		switch (_Action.whatAction)
		{
			case S_Enums.PlayerStates.Jump:
				if (_Action.Action01.ControlCounter < _jumpExtraControlThreshold_)
				{
					airAccelMod = _jumpAirControl_.y;
					airTurnMod = _jumpAirControl_.x;
				}
				break;
			case S_Enums.PlayerStates.Bounce:
				airAccelMod = _bounceAirControl_.y;
				airTurnMod = _bounceAirControl_.x;
				break;
		}
		if (_horizontalSpeedMagnitude < 20)
		{
			airAccelMod += 0.5f;
		}

		_coreVelocity = HandleControlledVelocity(_moveInput, new Vector2(airTurnMod, airAccelMod));

		//Apply Gravity
		if (_isGravityOn)
			_coreVelocity = SetGravity(_coreVelocity, _coreVelocity.y);
		//Max Falling Speed
		_coreVelocity = new Vector3(_coreVelocity.x, Mathf.Clamp(_coreVelocity.y, _maxFallingSpeed_, _coreVelocity.y), _coreVelocity.z);
	}

	//Returns appropriate downwards force when in the air
	Vector3 SetGravity ( Vector3 velocity, float vertSpeed ) {

		//If falling down, return normal fall gravity.
		if (vertSpeed <= 0)
		{
			return velocity + _fallGravity_;
		}
		//If currently moving up while in the air, apply a different (typically higher) gravity force with an slight increase dependant on upwards speed.
		else
		{
			float applyMod = Mathf.Clamp(1 + ((vertSpeed / 10) * 0.1f), 1, 3);
			Vector3 newGrav = new Vector3(_upGravity_.x, _upGravity_.y * applyMod, _upGravity_.z);
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
	public void AlignToGround ( Vector3 normal ) {

		//If on ground, then rotates to match the normal of the floor.
		//if ((Physics.Raycast(transform.position + (transform.up), -transform.up, 1.5f + _rayToGroundDistance_, _Groundmask_)))
		if (_isGrounded)
		{
			_keepNormal = normal;
			transform.rotation = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;

		}
		//If in the air, then stick to previous normal for a moment before rotating towards legs towards gravity. This avoids collision issues
		else
		{

			_KeepNormalCounter += Time.deltaTime;
			if (_KeepNormalCounter < _keepNormalForThis_)
			//if (KeepNormalCounter < 1f)
			{
				transform.rotation = Quaternion.FromToRotation(transform.up, _keepNormal) * transform.rotation;
				//transform.rotation = Quaternion.LookRotation(transform.forward, _keepNormal);
			}
			else
			{
				//EditorApplication.isPaused = true;

				//If upside down, then the player must rotate sideways, and not forwards. This keeps them facing the same way while pointing down to the ground again.
				if (_keepNormal.y < _rotationResetThreshold_)
				{

					//If the left side is under 0 y normal, then it is lower, so it is quicker to rotate so the lower left reaches normal
					//then rotate in the direction of th left going up, and when it becomes higher, set it to 0 and you've rotated around on the local sides.
					if (-transform.right.y < 0)
					{
						transform.rotation = Quaternion.RotateTowards(transform.rotation,
							Quaternion.FromToRotation(-transform.right, transform.up) * transform.rotation, 10f);

						if (-transform.right.y >= 0)
						{
							transform.right = new Vector3(transform.right.x, 0, transform.right.z);
							//_keepNormal = Vector3.up;
						}
					}
					else
					{
						transform.rotation = Quaternion.RotateTowards(transform.rotation,
							Quaternion.FromToRotation(transform.right, transform.up) * transform.rotation, 10f);

						if (transform.right.y >= 0)
						{
							transform.right = new Vector3(transform.right.x, 0, transform.right.z);
							//_keepNormal = Vector3.up;
						}
					}

					//Debug.Log("Side rotate");
				}
				else
				{
					Quaternion targetRot = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
					transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 10f);
					//Debug.Log("General rotate");
				}
			}
		}
	}

	public Vector3 GetRelevantVec ( Vector3 vec ) {
		return transform.InverseTransformDirection(vec);
	}

	public void SetIsGrounded ( bool value ) {
		if (_isGrounded != value)
		{
			_isGrounded = value;
			if (!_isGrounded)
			{
				_TimeOnGround = 0;
			}
			else
			{
				_KeepNormalCounter = 0;
			}
		}

	}

	public void AddTotalVelocity ( Vector3 force, bool shouldPrintForce = true ) {
		_listOfVelocityToAddNextUpdate.Add(force);
		if (shouldPrintForce) Debug.Log("Add Total FORCE");
	}
	public void AddCoreVelocity ( Vector3 force, bool shouldPrintForce = false ) {
		_listOfCoreVelocityToAdd.Add(force);
		if (shouldPrintForce) Debug.Log("ADD Core FORCE");
	}
	public void setCoreVelocity ( Vector3 force, bool shouldPrintForce = false ) {
		_externalCoreVelocity = force;
		if (shouldPrintForce) Debug.Log("Set Core FORCE");
	}
	public void setTotalVelocity ( Vector3 force, bool shouldPrintForce = true ) {
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
		_accelOverSpeed_ = _Tools.Stats.AccelerationStats.AccelBySpeed;
		_accelShiftOverSpeed_ = _Tools.Stats.AccelerationStats.accelShiftOverSpeed;
		_tangentialDrag_ = _Tools.Stats.TurningStats.tangentialDrag;
		_tangentialDragShiftSpeed_ = _Tools.Stats.TurningStats.tangentialDragShiftSpeed;
		_turnSpeed_ = _Tools.Stats.TurningStats.turnSpeed;

		_TurnRateByAngle_ = _Tools.Stats.TurningStats.TurnRateByAngle;
		_TurnRateBySpeed_ = _Tools.Stats.TurningStats.TurnRateBySpeed;
		_tangDragOverAngle_ = _Tools.Stats.TurningStats.TangDragByAngle;
		_tangDragOverSpeed_ = _Tools.Stats.TurningStats.TangDragBySpeed;
		_startTopSpeed_ = _Tools.Stats.SpeedStats.topSpeed;
		_startMaxSpeed_ = _Tools.Stats.SpeedStats.maxSpeed;
		_startMaxFallingSpeed_ = _Tools.Stats.WhenInAir.startMaxFallingSpeed;
		_moveDeceleration_ = _Tools.Stats.DecelerationStats.moveDeceleration;
		_decelBySpeed_ = _Tools.Stats.DecelerationStats.DecelBySpeed;
		_decelShiftOverSpeed_ = _Tools.Stats.DecelerationStats.decelShiftOverSpeed;
		_naturalAirDecel_ = _Tools.Stats.DecelerationStats.naturalAirDecel;
		_airDecell_ = _Tools.Stats.DecelerationStats.airDecel;
		_slopeEffectLimit_ = _Tools.Stats.SlopeStats.slopeEffectLimit;
		_standOnSlopeLimit_ = _Tools.Stats.SlopeStats.standOnSlopeLimit;
		_slopePower_ = _Tools.Stats.SlopeStats.slopePower;
		_slopeRunningAngleLimit_ = _Tools.Stats.SlopeStats.slopeRunningAngleLimit;
		_slopeSpeedLimit_ = _Tools.Stats.SlopeStats.SlopeLimitBySpeed;
		_generalHillMultiplier_ = _Tools.Stats.SlopeStats.generalHillMultiplier;
		_uphillMultiplier_ = _Tools.Stats.SlopeStats.uphillMultiplier;
		_downhillMultiplier_ = _Tools.Stats.SlopeStats.downhillMultiplier;
		_downHillThreshold_ = _Tools.Stats.SlopeStats.downhillThreshold;
		_upHillThreshold = _Tools.Stats.SlopeStats.uphillThreshold;
		_slopePowerOverSpeed_ = _Tools.Stats.SlopeStats.SlopePowerByCurrentSpeed;
		_airControlAmmount_ = _Tools.Stats.WhenInAir.controlAmmount;

		_jumpExtraControlThreshold_ = _Tools.Stats.JumpStats.jumpExtraControlThreshold;
		_jumpAirControl_ = _Tools.Stats.JumpStats.jumpAirControl;
		_bounceAirControl_ = _Tools.Stats.BounceStats.bounceAirControl;

		_shouldStopAirMovementIfNoInput_ = _Tools.Stats.WhenInAir.shouldStopAirMovementWhenNoInput;
		_rollingLandingBoost_ = _Tools.Stats.RollingStats.rollingLandingBoost;
		_rollingDownhillBoost_ = _Tools.Stats.RollingStats.rollingDownhillBoost;
		_rollingUphillBoost_ = _Tools.Stats.RollingStats.rollingUphillBoost;
		_rollingStartSpeed_ = _Tools.Stats.RollingStats.rollingStartSpeed;
		_rollingTurningDecrease_ = _Tools.Stats.RollingStats.rollingTurningDecrease;
		_rollingDecel_ = _Tools.Stats.RollingStats.rollingFlatDecell;
		_slopeTakeoverAmount_ = _Tools.Stats.RollingStats.slopeTakeoverAmount;
		_UpHillByTime_ = _Tools.Stats.SlopeStats.UpHillEffectByTime;
		_startFallGravity_ = _Tools.Stats.WhenInAir.fallGravity;
		_upGravity_ = _Tools.Stats.WhenInAir.upGravity;
		_keepNormalForThis_ = _Tools.Stats.WhenInAir.keepNormalForThis;


		_stickingLerps_ = _Tools.Stats.GreedysStickToGround.stickingLerps;
		_stickingNormalLimit_ = _Tools.Stats.GreedysStickToGround.stickingNormalLimit;
		_stickCastAhead_ = _Tools.Stats.GreedysStickToGround.stickCastAhead;
		_negativeGHoverHeight_ = _Tools.Stats.GreedysStickToGround.negativeGHoverHeight;
		_rayToGroundDistance_ = _Tools.Stats.GreedysStickToGround.rayToGroundDistance;
		_raytoGroundSpeedRatio_ = _Tools.Stats.GreedysStickToGround.raytoGroundSpeedRatio;
		_raytoGroundSpeedMax_ = _Tools.Stats.GreedysStickToGround.raytoGroundSpeedMax;
		_rayToGroundRotDistance_ = _Tools.Stats.GreedysStickToGround.rayToGroundRotDistance;
		_raytoGroundRotSpeedMax_ = _Tools.Stats.GreedysStickToGround.raytoGroundRotSpeedMax;
		_rotationResetThreshold_ = _Tools.Stats.GreedysStickToGround.rotationResetThreshold;
		_Groundmask_ = _Tools.Stats.GreedysStickToGround.GroundMask;
		_upwardsLimitByCurrentSlope_ = _Tools.Stats.GreedysStickToGround.upwardsLimitByCurrentSlope;
		_stepHeight_ = _Tools.Stats.GreedysStickToGround.stepHeight;
		_groundDifferenceLimit_ = _Tools.Stats.GreedysStickToGround.groundDifferenceLimit;

		//Sets all changeable core values to how they are set to start in the editor.
		_runAccell = _startAcceleration_;
		_rollAccell = _startRollAcceleration_;
		_currentTopSpeed = _startTopSpeed_;
		_currentMaxSpeed = _startMaxSpeed_;
		_maxFallingSpeed_ = _startMaxFallingSpeed_;
		_fallGravity_ = _startFallGravity_;

		_keepNormal = Vector3.up;


	}

	private void AssignTools () {
		s_MasterPlayer = this;
		_RB = GetComponent<Rigidbody>();
		PreviousInput = transform.forward;
		_Action = GetComponent<S_ActionManager>();
		_SoundController = _Tools.SoundControl;
		_CharacterCapsule = _Tools.characterCapsule.GetComponent<CapsuleCollider>();
		_FeetTransform = _Tools.FeetPoint;
	}

	#endregion
}
