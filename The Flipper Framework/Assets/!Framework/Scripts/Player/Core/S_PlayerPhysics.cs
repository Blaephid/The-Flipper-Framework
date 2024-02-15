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
	private AnimationCurve        _turnRateOverAngle_;
	private AnimationCurve        _turnRateOverSpeed_;
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
	float                         _airControlAmmount_ = 2;
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
	private Vector2		_jumpAirControl_;
	private Vector2		_bounceAirControl_;

	[Header("Rolling Values")]
	float                         _rollingLandingBoost_;
	private float                 _rollingDownhillBoost_;
	private float                 _rollingUphillBoost_;
	private float                 _rollingStartSpeed_;
	private float                 _rollingTurningDecrease_;
	private float                 _rollingFlatDecell_;
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
	public float _inputVelocityDifference = 1;
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
	public bool _canBeGrounded = true;
	public Vector3 _groundNormal { get; set; }
	public Vector3 _collisionPointsNormal { get; set; }
	private Vector3               _KeepNormal;
	private float                 _KeepNormalCounter;
	public bool _wasInAir { get; set; }
	[HideInInspector]
	public bool                   _isGravityOn = true;

	public bool _isRolling { get; set; }

	[Header("Greedy Stick Fix")]
	public bool                   _EnableDebug;
	public float _TimeOnGround { get; set; }
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
		curvePosDecell = _decelBySpeed_.Evaluate(_horizontalSpeedMagnitude / _currentMaxSpeed );
		curvePosTang = _tangDragOverSpeed_.Evaluate(_horizontalSpeedMagnitude / _currentMaxSpeed );
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

		//If the rigidbody velocity is smaller than it was last frame (such as from hitting a wall),
		//Then apply the difference to the corevelocity as well so it knows there's been a change.
		//But if the difference is minor (such as lightly colliding with a slope when going up), then ignore change.
		Debug.Log("This Frame = " + _RB.velocity.magnitude + "  Last Frame = " + _prevTotalVelocity.magnitude);
		if (_RB.velocity.sqrMagnitude < _prevTotalVelocity.sqrMagnitude)
		{
			Debug.Log("Lower");
			float dif = Mathf.Abs(_RB.velocity.magnitude - _speedMagnitude);
			float sped = _RB.velocity.sqrMagnitude;

			if (dif < 10 && sped > Mathf.Pow(15, 2))
			{
				Debug.Log("Change");
				_RB.velocity = _RB.velocity * _speedMagnitude;
			}
			else
			{
				//Debug.Log("ABS = " + Mathf.Abs(_RB.velocity.sqrMagnitude - _prevTotalVelocity.sqrMagnitude) + "  VEC = " +(_prevTotalVelocity - _RB.velocity).sqrMagnitude);

				Vector3 Difference = _prevTotalVelocity - _RB.velocity;
				_coreVelocity -= Difference;
			}
		}

		//Calls the appropriate movement handler.
		if (_isGrounded)
			GroundMovement();
		else
			HandleAirMovement();
		AlignWithGround();

		//StartCoroutine(setVelocityAtEndOfUpdate());
		SetTotalVelocity();	
	}

	//Determines if the player is on the ground and sets _isGrounded to the answer.
	void CheckForGround () {

		//If currently moving upwards in the air, then cannot be grounded
		if(!_canBeGrounded) { return; }

		//Sets the size of the ray to check for ground. 
		float _rayToGroundDistancecor = _rayToGroundDistance_;
		if (_Action.whatAction == S_Enums.PlayerStates.Regular && _isGrounded)
		{
			//grounder line
			_rayToGroundDistancecor = Mathf.Max(_rayToGroundDistance_ + (_speedMagnitude * _raytoGroundSpeedRatio_), _rayToGroundDistance_);
			_rayToGroundDistancecor = Mathf.Min(_rayToGroundDistancecor, _raytoGroundSpeedMax_);

		}

		//Uses the ray to check for ground, if found, sets grounded to true and takes the normal.
		if (Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out RaycastHit hitGroundTemp, 2f + _rayToGroundDistancecor, _Groundmask_))
		{
			if(Vector3.Angle(_groundNormal, hitGroundTemp.normal) / 180 < _groundDifferenceLimit_)
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

	IEnumerator SetVelocityAtEndOfUpdate() {
		yield return new WaitForEndOfFrame();
		SetTotalVelocity();
	}

	//After every other calculation has been made, all of the new velocities and combined and set to the rigidbody.
	//This includes the core and environmental velocities, but also the others that have been added into a list using the addvelocity method.
	private void SetTotalVelocity () {
		
		if(_externalCoreVelocity != Vector3.zero)
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


		_RB.velocity = _totalVelocity;
		_prevTotalVelocity = _totalVelocity;

		Debug.DrawRay(hitGround.point + (_groundNormal * 0.25f), _totalVelocity.normalized * 1f, Color.green, 900);

		_speedMagnitude = _totalVelocity.magnitude;
		Vector3 releVec = GetRelevantVec(_RB.velocity);
		_horizontalSpeedMagnitude = new Vector3(releVec.x, 0f, releVec.z).magnitude;
	}

	//Calls all the functions involved in managing coreVelocity on the ground, such as normal control (with normal modifiers), sticking to the ground, and effects from slopes.
	private void GroundMovement () {

		Debug.DrawRay(_FeetTransform.position, -hitGround.normal * 0.4f, Color.gray, 900, true);
		 _TimeOnGround += Time.deltaTime;

		_coreVelocity = HandleControlledVelocity(1, _moveInput, new Vector2 (1,1));
		_coreVelocity = StickToGround(_coreVelocity);
		Debug.DrawRay(hitGround.point + (_groundNormal * 0.25f), _coreVelocity.normalized * 1.2f, Color.blue, 900);
		//_coreVelocity = HandleSlopePhysics(_coreVelocity);
	}

	//Handles core velocity, which is the velocity directly under the player's control (seperate from environmental velocity which is placed on the character by other things).
	//This decreases or increases the velocity based on input.
	Vector3 HandleControlledVelocity ( float del, Vector3 input, Vector2 modifier ) {

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
	Vector3 HandleTurningAndAccel(Vector3 lateralVelocity, Vector3 input, Vector2 modifier) {
		// If there is some input...

		// Normalize to get input direction and magnitude seperately
		Vector3 inputDirection = input.normalized;
		float inputMagnitude = input.magnitude;

		Debug.DrawRay(transform.position, inputDirection , Color.yellow);

		// Step 1) Determine angle between current lateral velocity and desired direction.
		//         Creates a quarternion which rotates to the direction, which will be zero if velocity is too slow.

		float deviationFromInput = Vector3.Angle(lateralVelocity, inputDirection) / 180.0f;
		Quaternion lateralToInput = Mathf.Approximately(lateralVelocity.sqrMagnitude, 0.0f)
			? Quaternion.identity
			: Quaternion.FromToRotation(lateralVelocity.normalized, inputDirection);

		// Step 2) Rotate lateral velocity towards the same velocity under the desired rotation.
		//         The ammount rotated is determined by turn speed multiplied by turn rate (defined by the difference in angles, and current speed).

		float turnRate = _turnRateOverAngle_.Evaluate(deviationFromInput);
		turnRate *= _turnRateOverSpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);
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
		else if(setVelocity.sqrMagnitude < lateralVelocity.sqrMagnitude)
		{
			lateralVelocity = setVelocity;
		}
		
		return lateralVelocity;
	}

	//Handles decreasing the magnitude of the player's controlled velocity, usually only if there is no input, but other circumstances may decrease speed as well.
	// Deceleration is calculated, then applied at the end of the method.
	private Vector3 HandleDecel(Vector3 lateralVelocity, Vector3 input, bool isGrounded) {
		float DecellAmount = 0;
		//If there is no input, apply conventional deceleration.
		if (Mathf.Approximately(input.sqrMagnitude, 0))
		{
			if(isGrounded)
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
			DecellAmount = _rollingFlatDecell_ * curvePosDecell;
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
	private Vector3 HandleSlopePhysics (Vector3 worldVelocity) {
		Vector3 slopeVelocity = Vector3.zero;

		//If just landed
		//Then apply additional speed dependant on slope angle.
		if (_wasInAir)
		{
			Vector3 addVelocity;
			if (!_isRolling)
			{
				addVelocity = _groundNormal * _landingConversionFactor;
				addVelocity = AlignWithNormal(addVelocity, _groundNormal);
			}
			else
			{
				addVelocity = (_groundNormal * _landingConversionFactor) * _rollingLandingBoost_;
				addVelocity = AlignWithNormal(addVelocity, _groundNormal);
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
			AddCoreVelocity(_groundNormal * 2f);
		}

		//Slope power
		//If slope angle is less than limit, meaning on a slope
		//Then add a force directly downwards. This force is acquired through a number of calculations based on rolling, uphill or downhill, current speed, steepness, and more.
		//This force is then added to the current velocity, leading to a more realistic and natural effect than just changing speed.
		if (_groundNormal.y < _slopeEffectLimit_)
		{
			Vector3 force = new Vector3(0, -curveSlopePower, 0);
			force *= _generalHillMultiplier_;
			force *= ((1 - (_groundNormal.y / 10)) + 1);

			if (worldVelocity.y > _upHillThreshold)
			{
				_timeUpHill += Time.fixedDeltaTime;
				force *= _uphillMultiplier_;
				force *= _UpHillByTime_.Evaluate(_timeUpHill);
				if (!_isRolling)
				{
					force *= _rollingUphillBoost_;
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
			//If not upside down, then force is applied down along slope rather than straight down.
			if (_groundNormal.y > 0)
				force = AlignWithNormal(force, _groundNormal);
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
			Vector3 raycastStartPosition = hitGround.point + (_groundNormal * 0.25f);
			Vector3 rayCastDirection = _RB.velocity.normalized;

			Debug.DrawRay(raycastStartPosition, _RB.velocity.normalized * 1.8f, Color.white, 900);

			//Negative slopes (like loops, where the player starts facing more upwards).
			//Shoots a raycast forwards from just above the ground, meaning when theres a wall or sudden slope relative to the ground, it will hit.		
			if (Physics.BoxCast(raycastStartPosition, new Vector3(0.1f, 0.04f, 0.4f), rayCastDirection, out RaycastHit hitSticking, 
				Quaternion.LookRotation(velocity, newGroundNormal), _horizontalSpeedMagnitude * _stickCastAhead_ * Time.fixedDeltaTime, _Groundmask_))
			{
				float dif = Vector3.Angle(newGroundNormal, hitSticking.normal) / 180;
				float limit = _upwardsLimitByCurrentSlope_.Evaluate(newGroundNormal.y);

				//If the difference between current slope and encountered one is under the limit specific to that current slope
				//(if slope is flat ground, lower limit, this is to make this happen more when heading downhill then to sudden flatground)
				//Then it creates a velocity aligned to that new normal, then interpolates from the current to this new one.			
				if (dif < limit) 
				{
					Debug.DrawRay(raycastStartPosition, rayCastDirection * 1.7f, Color.black, 900);

					newGroundNormal = hitSticking.normal;
					Vector3 Dir = AlignWithNormal(velocity, newGroundNormal);
					velocity = Vector3.LerpUnclamped(velocity, Dir, _stickingLerps_.x);

					Debug.DrawRay(raycastStartPosition, velocity.normalized * 1.5f, Color.red, 900);

					//_groundNormal = Vector3.Lerp(_groundNormal, newGroundNormal, _stickingLerps_.x);
					transform.position = hitGround.point + (newGroundNormal * _negativeGHoverHeight_) + (newGroundNormal * _FeetTransform.localPosition.y);
					//transform.position += newGroundNormal * _negativeGHoverHeight_;

					Debug.DrawRay(hitGround.point + newGroundNormal * _negativeGHoverHeight_, newGroundNormal * 0.3f, Color.cyan, 900);

				}

				//If not, then shoot a spherecast down from above and slightly continuing on from the impact point.
				//If there is a lip, if the wall infront is very short-
				//Then if the ledge is within step height and a similar angle to the current one-
				//the player will be moved to the position of the player, automatically stepping up over the lip.
				else if (Physics.SphereCast(hitSticking.point + rayCastDirection + newGroundNormal * 1.5f, _CharacterCapsule.radius,
					-newGroundNormal, out RaycastHit hitLip, 1.45f - _CharacterCapsule.radius, _Groundmask_))
				{
					//float dis = Vector3.Distance(hitSticking.point + _RB.velocity.normalized, hitLip.point);
					//float dot = Vector3.Dot(hitLip.normal, newGroundNormal);
					//if (dis < _stepHeight_ && dis > 0.015f && _horizontalSpeedMagnitude > 20f && dot > 0.98f)
					//{
					//	Vector3 castPosition = (hitSticking.point + _RB.velocity.normalized + newGroundNormal) - (newGroundNormal * hitLip.distance);
					//	_RB.position = castPosition + _feetPoint;
					//}
				}

				return velocity;
			}
			//Positive slopes (like the outside of a loop, where the player starts facing more downwards).
			else if (_TimeOnGround > 0.1f)
			{
				//If the difference between current movement and ground normal is less than the limit to stick (lower limits prevent super sticking).
				if (Mathf.Abs(DirectionDot) < _stickingNormalLimit_)
				{
					raycastStartPosition = raycastStartPosition + (_RB.velocity * _stickCastAhead_ * Time.deltaTime);
					//Shoots a raycast from infront, but downwards to check for lower ground.
					if (Physics.Raycast(raycastStartPosition, -hitGround.normal, out hitSticking, 2.5f, _Groundmask_))
					{
						//If the gound hit is further than the distance to the feet, then the ground is lower, so the slope is positive
						//Then create a velocity relative to the current groundNormal, then lerp from one to the other.
						//Also apply force downwards to stick.
						float hitDis = Vector3.Distance(raycastStartPosition, hitSticking.point);
						float feetDis = Vector3.Distance(raycastStartPosition, raycastStartPosition + (hitGround.normal * _FeetTransform.localPosition.y)); ;
						if (hitDis > feetDis + 0.15f)
						{
							Vector3 Dir = AlignWithNormal(velocity, newGroundNormal);
							float lerp = _stickingLerps_.y;

							if (hitSticking.distance > 1.5f)
							{
								lerp = 3;
								//transform.position = hitGround.point + groundNormal * (_CharacterCapsule.height / 2f + _negativeGHoverHeight_);
								_RB.position = hitGround.point + newGroundNormal * _negativeGHoverHeight_;
							}
							velocity = Vector3.LerpUnclamped(velocity, Dir, lerp);
							//_groundNormal = Vector3.Lerp(_groundNormal, newGroundNormal, _stickingLerps_.x);
							AddCoreVelocity(-newGroundNormal * 2); // Adds velocity downwards to remain on the slope.

							return velocity;
						}
					}
					
				}
			}
		}
		return AlignWithNormal(velocity, _groundNormal);
	}

	//Makes a vector relative to a normal. Such as a forward direction being affected by the ground.
	Vector3 AlignWithNormal ( Vector3 vector, Vector3 normal ) {
		//typically used to rotate a movement vector by a surface normal
		Vector3 vec = Vector3.ProjectOnPlane(vector, normal);
		return vec;
	}

	//Calls methods relevant to general control and gravity, while applying the turn and accelleration modifiers depending on a number of factors while in the air.
	void HandleAirMovement () {

		float airAccelMod = 1;
		float airTurnMod = 1;
		switch (_Action.whatAction)
		{
			case S_Enums.PlayerStates.Jump:
				if(_Action.Action01.ControlCounter < _jumpExtraControlThreshold_)
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
		if(_horizontalSpeedMagnitude < 20)
		{
			airAccelMod += 0.5f;
		}

		_coreVelocity = HandleControlledVelocity(1, _moveInput, new Vector2(airTurnMod, airAccelMod));
		
		//Apply Gravity
		if (_isGravityOn)
			_coreVelocity += GetGravity(_coreVelocity.y);
		//Max Falling Speed
		_coreVelocity = new Vector3(_coreVelocity.x, Mathf.Clamp(_coreVelocity.y, _maxFallingSpeed_, _coreVelocity.y), _coreVelocity.z);
	}

	//Returns appropriate downwards force when in the air
	Vector3 GetGravity ( float vertSpeed ) {

		//If falling down, return normal fall gravity.
		if (vertSpeed <= 0)
		{
			return _fallGravity_;
		}
		//If currently moving up while in the air, apply a different (typically higher) gravity force with an slight increase dependant on upwards speed.
		else
		{
	
			float applyMod = Mathf.Clamp(1 + ((vertSpeed / 10) * 0.1f), 1, 3);
			Vector3 newGrav = new Vector3(_upGravity_.x, _upGravity_.y * applyMod, _upGravity_.z);
			return newGrav;
		}
	}


	#endregion
	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//
	public void AlignWithGround () {


		//if ((Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out hitRot, 2f + RayToGroundRotDistancecor, Playermask)))
		if (_isGrounded)
		{
			_KeepNormal = _groundNormal;

			transform.rotation = Quaternion.FromToRotation(transform.up, _groundNormal) * transform.rotation;
			//transform.rotation = Quaternion.LookRotation(Vector3.forward, GroundNormal);
			//transform.up = GroundNormal;

			_KeepNormalCounter = 0;

		}
		else
		{
			//Keep the rotation after exiting the ground for a while, to avoid collision issues.

			_KeepNormalCounter += Time.deltaTime;
			if (_KeepNormalCounter < _keepNormalForThis_)
			//if (KeepNormalCounter < 1f)
			{
				transform.rotation = Quaternion.FromToRotation(transform.up, _KeepNormal) * transform.rotation;

			}
			else
			{
				//Debug.Log(KeepNormal.y);

				//if (transform.up.y < RotationResetThreshold)
				if (_KeepNormal.y < _rotationResetThreshold_)
				{
					_KeepNormal = Vector3.up;

					if (_MainSkin.right.y >= -_MainSkin.right.y)
						transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(transform.up, _MainSkin.right) * transform.rotation, 10f);
					else
						transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(transform.up, -_MainSkin.right) * transform.rotation, 10f);

					if (Vector3.Dot(transform.up, Vector3.up) > 0.99)
						_KeepNormal = Vector3.up;

				}
				else
				{
					Quaternion targetRot = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
					transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 10f);
				}
			}
		}
	}
	public Vector3 GetRelevantVec ( Vector3 vec ) {
		return transform.InverseTransformDirection(vec);
		//if (!Grounded)
		//{
		//    return transform.InverseTransformDirection(vec);
		//    //Vector3 releVec = transform.InverseTransformDirection(rb.velocity.normalized);
		//}
		//else
		//{
		//    return transform.InverseTransformDirection(vec);
		//    return Vector3.ProjectOnPlane(vec, groundHit.normal);
		//}
	}

	public void SetIsGrounded ( bool value ) {
		if (_isGrounded != value)
		{
			_isGrounded = value;
			if (!_isGrounded) { _TimeOnGround = 0; }
		}

	}

	public void AddTotalVelocity ( Vector3 force ) {
		_listOfVelocityToAddNextUpdate.Add(force);
		//_RB.velocity += force;
	}public void AddCoreVelocity ( Vector3 force ) {
		_listOfCoreVelocityToAdd.Add(force);
		//_RB.velocity += force;
	}
	public void setCoreVelocity(Vector3 force ) {
		_externalCoreVelocity = force;
	}public void setTotalVelocity(Vector3 force ) {
		_externalSetVelocity = force;
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

		_turnRateOverAngle_ = _Tools.Stats.TurningStats.TurnRateByAngle;
		_turnRateOverSpeed_ = _Tools.Stats.TurningStats.TurnRateBySpeed;
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
		_rollingFlatDecell_ = _Tools.Stats.RollingStats.rollingFlatDecell;
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

		_KeepNormal = Vector3.up;


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
