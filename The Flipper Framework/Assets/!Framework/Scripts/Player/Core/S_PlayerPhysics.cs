using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;

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

	[HideInInspector] public RaycastHit groundHit;
	Transform CollisionPoint;

	S_Control_SoundsPlayer sounds;

	#endregion


	//General
	#region General Members

	//Stats
	#region Stats

	[Header("Grounded Movement")]
	private float		_startAcceleration_ = 0.5f;
	private float		_moveAccell;
	private AnimationCurve	_accelOverSpeed_;
	private float		_accelShiftOverSpeed_;
	private float		_decelShiftOverSpeed_;

	[HideInInspector] 
	public float		_moveDeceleration_ = 1.3f;
	AnimationCurve		_decelBySpeed_;
	private float		_airDecell_ = 1.05f;
	private float		_naturalAirDecel_ = 1.01f;

	private float		_tangentialDrag_;
	private float		_tangentialDragShiftSpeed_;
	private float		_turnSpeed_ = 16f;
	private AnimationCurve	_turnRateOverAngle_;
	private AnimationCurve	_turnRateOverSpeed_;
	private AnimationCurve	_tangDragOverAngle_;
	private AnimationCurve	_tangDragOverSpeed_;

	private float		_startTopSpeed_ = 65f;
	private float		_startMaxSpeed_ = 230f;
	private float		_startMaxFallingSpeed_ = -500f;

	[Header("Slopes")]
	private float		_slopeEffectLimit_ = 0.9f;
	private float		 _standOnSlopeLimit_ = 0.8f;
	private float		_slopePower_ = 0.5f;
	private float		_slopeRunningAngleLimit_ = 0.5f;
	private AnimationCurve	_slopeSpeedLimit_;

	private float		_generalHillMultiplier_ = 1;
	private float		_uphillMultiplier_ = 0.5f;
	private float		_downhillMultiplier_ = 2;
	private float		_startDownhillMultiplier_ = -7;

	private AnimationCurve	_slopePowerOverSpeed_;
	private AnimationCurve	_UpHillByTime_;

	[Header("Air Movement Extras")]
	float			_airControlAmmount_ = 2;
	private bool		_shouldStopAirMovementIfNoInput_ = false;
	private float		_keepNormalForThis_ = 0.083f;
	private float		_maxFallingSpeed_;
	[HideInInspector] 
	public float		_homingDelay_;
	private Vector3               _upGravity_;
	[HideInInspector] 
	public Vector3		_startFallGravity_;
	[HideInInspector] 
	public Vector3		_fallGravity_;

	[Header("Rolling Values")]
	float			_rollingLandingBoost_;
	private float		_rollingDownhillBoost_;
	private float		_rollingUphillBoost_;
	private float		_rollingStartSpeed_;
	private float		_rollingTurningDecrease_;
	private float		_rollingFlatDecell_;
	private float		_slopeTakeoverAmount_; // This is the normalized slope angle that the player has to be in order to register the land as "flat"

	[Header("Stick To Ground")]
	private Vector2		_stickingLerps_ = new Vector2(0.885f, 1.5f);
	private float		_stickingNormalLimit_ = 0.4f;
	private float		_stickCastAhead_ = 1.9f;
	[HideInInspector]
	public float		_negativeGHoverHeight_ = 0.6115f;
	private float		_rayToGroundDistance_ = 0.55f;
	private float		_raytoGroundSpeedRatio_ = 0.01f;
	private float		_raytoGroundSpeedMax_ = 2.4f;
	private float		_rayToGroundRotDistance_ = 1.1f;
	private float		_raytoGroundRotSpeedMax_ = 2.6f;
	private float		_rotationResetThreshold_ = -0.1f;

	[HideInInspector] 
	public LayerMask		_Groundmask_;
	#endregion

	// Trackers
	#region trackers
	public float		_speedMagnitude { get; set; }
	public float		_horizontalSpeedMagnitude { get; set; }

	public Vector3		_moveInput { get; set; }
	public Vector3		PreviousInput { get; set; }
	public Vector3		RawInput { get; set; }
	public Vector3		PreviousRawInput { get; set; }
	public Vector3		PreviousRawInputForAnim { get; set; }
	[HideInInspector] 
	public float		_currentTopSpeed;
	[HideInInspector] 
	public float		_currentMaxSpeed;
	public float		curvePosAcell { get; set; }
	private float		curvePosDecell = 1f;
	public float		curvePosTang { get; set; }
	public float		curvePosSlope { get; set; }
	public float		b_normalSpeed { get; set; }
	public Vector3		b_normalVelocity { get; set; }
	public Vector3		b_tangentVelocity { get; set; }
	[Tooltip("A quick reference to the players current location")]
	public Vector3		_playerPos { get; set; }

	private float		_timeUpHill;
	private float		_slopePowerShiftSpeed;
	private float		_landingConversionFactor = 2;

	[Tooltip("Used to check if the player is currently grounded. _isGrounded")]
	public bool		_isGrounded { get; set; }
	public Vector3		_groundNormal { get; set; }
	public Vector3		_collisionPointsNormal { get; set; }
	private Vector3		_KeepNormal;
	private float		_KeepNormalCounter;
	public bool		_wasInAir { get; set; }
	[HideInInspector] 
	public bool		_isGravityOn = true;

	public bool		_isRolling { get; set; }

	[Header("Greedy Stick Fix")]
	public bool		_EnableDebug;
	public float		_TimeOnGround { get; set; }
	private RaycastHit		_hitSticking, hitRot;
	[HideInInspector] 
	public float		_RayToGroundDistancecor, _RayToGroundRotDistancecor;

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
	}

	//On FixedUpdate, increases time on ground and calls HandleGeneralPhysics if relevant.
	void FixedUpdate () {
		

		if (_isGrounded) { _TimeOnGround += Time.deltaTime; };
		if (_Action.whatAction != S_Enums.PlayerStates.Path)
			HandleGeneralPhysics();

		if (_homingDelay_ > 0)
		{
			_homingDelay_ -= Time.deltaTime;
		}
	}

	//Sets public variables relevant to other calculations 
	void Update () {
		_speedMagnitude = _RB.velocity.magnitude;
		Vector3 releVec = GetRelevantVec(_RB.velocity);
		_horizontalSpeedMagnitude = new Vector3(releVec.x, 0f, releVec.z).magnitude;

		_playerPos = transform.position;
	}

	//Involed in sticking calculations
	#region TRYSCRAP
	public void OnCollisionStay ( Collision col ) {
		Vector3 prevNormal = _groundNormal;
		foreach (ContactPoint contact in col.contacts)
		{

			//Set Middle Point
			Vector3 pointSum = Vector3.zero;
			Vector3 normalSum = Vector3.zero;
			for (int i = 0 ; i < col.contacts.Length ; i++)
			{
				pointSum = pointSum + col.contacts[i].point;
				normalSum = normalSum + col.contacts[i].normal;
			}

			pointSum = pointSum / col.contacts.Length;
			_collisionPointsNormal = normalSum / col.contacts.Length;

			if (_RB.velocity.normalized != Vector3.zero)
			{
				CollisionPoint.position = pointSum;
			}
		}
	}
	#endregion
	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	void HandleGeneralPhysics () {
		//Set Previous input
		if (RawInput.sqrMagnitude >= 0.03f)
		{
			PreviousRawInputForAnim = RawInput * 90;
			PreviousRawInputForAnim = PreviousRawInputForAnim.normalized;
		}

		if (_moveInput.sqrMagnitude >= 0.9f)
		{
			PreviousInput = _moveInput;
		}
		if (RawInput.sqrMagnitude >= 0.9f)
		{
			PreviousRawInput = RawInput;
		}

		//Set Curve thingies
		curvePosAcell = Mathf.Lerp(curvePosAcell, _accelOverSpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _accelShiftOverSpeed_);
		curvePosDecell = Mathf.Lerp(curvePosDecell, _decelBySpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _decelShiftOverSpeed_);
		curvePosTang = Mathf.Lerp(curvePosTang, _tangDragOverSpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _tangentialDragShiftSpeed_);
		curvePosSlope = Mathf.Lerp(curvePosSlope, _slopePowerOverSpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _slopePowerShiftSpeed);

		// Do it for X and Z
		if (_horizontalSpeedMagnitude > _currentMaxSpeed)
		{
			Vector3 ReducedSpeed = _RB.velocity;
			float keepY = _RB.velocity.y;
			ReducedSpeed = Vector3.ClampMagnitude(ReducedSpeed, _currentMaxSpeed);
			ReducedSpeed.y = keepY;
			_RB.velocity = ReducedSpeed;
		}

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

		//Rotate Colliders     
		if (_EnableDebug)
		{
			Debug.DrawRay(transform.position + (transform.up * 2) + transform.right, -transform.up * (2f + _RayToGroundRotDistancecor), Color.red);
		}

		AlignWithGround();

		CheckForGround();
	}


	Vector3 HandleGroundControl ( float deltaTime, Vector3 input ) {
		if (_Action.whatAction != S_Enums.PlayerStates.JumpDash && _Action.whatAction != S_Enums.PlayerStates.WallRunning)
		{

			//By Damizean

			// We assume input is already in the Player's local frame...
			// Fetch velocity in the Player's local frame, decompose into lateral and vertical
			// components.

			Vector3 velocity = _RB.velocity;
			Vector3 localVelocity = transform.InverseTransformDirection(velocity);

			Vector3 lateralVelocity = new Vector3(localVelocity.x, 0.0f, localVelocity.z);
			Vector3 verticalVelocity = new Vector3(0.0f, localVelocity.y, 0.0f);

			// If there is some input...

			if (input.sqrMagnitude != 0.0f)
			{

				// Normalize to get input direction.



				Vector3 inputDirection = input.normalized;
				float inputMagnitude = input.magnitude;

				// Step 1) Determine angle and rotation between current lateral velocity and desired direction.
				//         Prevent invalid rotations if no lateral velocity component exists.

				float deviationFromInput = Vector3.Angle(lateralVelocity, inputDirection) / 180.0f;
				Quaternion lateralToInput = Mathf.Approximately(lateralVelocity.sqrMagnitude, 0.0f)
				    ? Quaternion.identity
				    : Quaternion.FromToRotation(lateralVelocity.normalized, inputDirection);

				// Step 2) Let the user retain some component of the velocity if it's trying to move in
				//         nearby directions from the current one. This should improve controlability.

				float turnRate = _turnRateOverAngle_.Evaluate(deviationFromInput);
				turnRate *= _turnRateOverSpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);
				//lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, Mathf.Deg2Rad * TurnSpeed * turnRate * Time.deltaTime, 0.0f);

				lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, _turnSpeed_ * turnRate * Time.deltaTime, 0.0f);



				// Step 3) Further lateral velocity into normal (in the input direction) and tangential
				//         components. Note: normalSpeed is the magnitude of normalVelocity, with the added
				//         bonus that it's signed. If positive, the speed goes towards the same
				//         direction than the input :)

				var normalDot = Vector3.Dot(lateralVelocity.normalized, inputDirection.normalized);

				//if (Mathf.Abs(normalDot) <= 0.6f && normalDot > -0.6f)
				//{
				//    inputDirection = Vector3.Slerp(lateralVelocity.normalized, inputDirection, 0.005f);
				//}

				float normalSpeed = Vector3.Dot(lateralVelocity, inputDirection);
				Vector3 normalVelocity = inputDirection * normalSpeed;
				Vector3 tangentVelocity = lateralVelocity - normalVelocity;
				float tangentSpeed = tangentVelocity.magnitude;

				// Step 4) Apply user control in this direction.

				if (normalSpeed < _currentTopSpeed)
				{


					// Accelerate towards the input direction.
					normalSpeed += (_isRolling ? 0 : _moveAccell) * deltaTime * inputMagnitude;

					normalSpeed = Mathf.Min(normalSpeed, _currentTopSpeed);

					// Rebuild back the normal velocity with the correct modulus.

					normalVelocity = inputDirection * normalSpeed;
				}

				// Step 5) Dampen tangential components.

				float dragRate = _tangDragOverAngle_.Evaluate(deviationFromInput)
			  * _tangDragOverSpeed_.Evaluate((tangentSpeed * tangentSpeed) / (_currentMaxSpeed * _currentMaxSpeed));

				tangentVelocity = Vector3.MoveTowards(tangentVelocity, Vector3.zero, _tangentialDrag_ * dragRate * deltaTime);

				lateralVelocity = normalVelocity + tangentVelocity;

				//Export nescessary variables

				b_normalSpeed = normalSpeed;
				b_normalVelocity = normalVelocity;
				b_tangentVelocity = tangentVelocity;

			}

			// Otherwise, apply some damping as to decelerate Sonic.
			if (_isGrounded)
			{
				float DecellAmount = 1;
				if (_isRolling && _groundNormal.y > _slopeTakeoverAmount_ && _horizontalSpeedMagnitude > 10)
				{
					DecellAmount = _rollingFlatDecell_ * curvePosDecell;
					if (input.sqrMagnitude == 0)
						DecellAmount *= _moveDeceleration_;
				}

				else if (input.sqrMagnitude == 0)
				{
					DecellAmount = _moveDeceleration_ * curvePosDecell;
				}
				lateralVelocity /= DecellAmount;
			}


			// Compose local velocity back and compute velocity back into the Global frame.

			localVelocity = lateralVelocity + verticalVelocity;

			//new line for the stick to ground from GREEDY


			velocity = transform.TransformDirection(localVelocity);

			if (_isGrounded)
				velocity = StickToGround(velocity);

			return velocity;
		}
		return _RB.velocity;

	}

	private void GroundMovement () {
		//Stop Rolling
		//if (HorizontalSpeedMagnitude < 3)
		//{
		//    isRolling = false;
		//}

		//Slope Physics
		SlopePlysics();

		// Call Ground Control



		Vector3 setVelocity = HandleGroundControl(1, _moveInput * curvePosAcell);
		_RB.velocity = setVelocity;


	}

	private void SlopePlysics () {
		//ApplyLandingSpeed
		if (_wasInAir)
		{
			Vector3 Addsped;

			if (!_isRolling)
			{
				Addsped = _groundNormal * _landingConversionFactor;
				//StickToGround(GroundStickingPower);
			}
			else
			{
				Addsped = (_groundNormal * _landingConversionFactor) * _rollingLandingBoost_;
				//StickToGround(GroundStickingPower * RollingLandingBoost);
				sounds.SpinningSound();
			}

			Addsped.y = 0;
			AddVelocity(Addsped);
			_wasInAir = false;
		}

		//Get out of slope if speed is too low
		if (_horizontalSpeedMagnitude < _slopeSpeedLimit_.Evaluate(_groundNormal.y))
		{
			if (_slopeRunningAngleLimit_ > _groundNormal.y)
			{
				//transform.rotation = Quaternion.identity;
				SetIsGrounded(false);
				AddVelocity(_groundNormal * 1.5f);
			}

		}



		//Apply slope power
		if (_groundNormal.y < _slopeEffectLimit_)
		{

			if (_timeUpHill < 0)
				_timeUpHill = 0;

			if (_RB.velocity.y > _startDownhillMultiplier_)
			{
				_timeUpHill += Time.deltaTime;
				//Debug.Log(p_rigidbody.velocity.y);
				if (!_isRolling)
				{
					Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _uphillMultiplier_ * _generalHillMultiplier_, 0);
					force *= _UpHillByTime_.Evaluate(_timeUpHill);
					AddVelocity(force);
				}
				else
				{
					Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _uphillMultiplier_ * _generalHillMultiplier_, 0) * _rollingUphillBoost_;
					force *= _UpHillByTime_.Evaluate(_timeUpHill);
					AddVelocity(force);
				}
			}

			else
			{
				_timeUpHill -= Time.deltaTime * 0.8f;
				if (_moveInput != Vector3.zero && b_normalSpeed > 0)
				{
					if (!_isRolling)
					{
						Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _downhillMultiplier_ * _generalHillMultiplier_, 0);
						AddVelocity(force);
					}
					else
					{
						Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _downhillMultiplier_ * _generalHillMultiplier_, 0) * _rollingDownhillBoost_;
						AddVelocity(force);
					}

				}
				else if (_groundNormal.y < _standOnSlopeLimit_)
				{
					Vector3 force = new Vector3(0, _slopePower_ * curvePosSlope, 0);
					AddVelocity(force);
				}
			}
		}
		else
			_timeUpHill = 0;

	}

	private Vector3 StickToGround ( Vector3 Velocity ) {
		Vector3 result = Velocity;
		if (_EnableDebug)
		{
			Debug.Log("Before: " + result + "speed " + result.magnitude);
		}
		if (_TimeOnGround > 0.1f && _speedMagnitude > 1)
		{
			float DirectionDot = Vector3.Dot(_RB.velocity.normalized, groundHit.normal);
			Vector3 normal = groundHit.normal;
			Vector3 Raycasterpos = _RB.position + (groundHit.normal * -0.12f);

			if (_EnableDebug)
			{
				Debug.Log("Speed: " + _speedMagnitude + "\n Direction DOT: " + DirectionDot + " \n Velocity Normal:" + _RB.velocity.normalized + " \n  Ground normal : " + groundHit.normal);
				Debug.DrawRay(groundHit.point + (transform.right * 0.2F), groundHit.normal * 3, Color.yellow, 1);
			}

			//If the Raycast Hits something, it adds it's normal to the ground normal making an inbetween value the interpolates the direction;
			if (Physics.Raycast(Raycasterpos, _RB.velocity.normalized, out _hitSticking, _speedMagnitude * _stickCastAhead_ * Time.deltaTime, _Groundmask_))
			{
				if (_EnableDebug) Debug.Log("AvoidingGroundCollision");

				if (Vector3.Dot(normal, _hitSticking.normal) > 0.15f) //avoid flying off Walls
				{
					normal = _hitSticking.normal.normalized;
					Vector3 Dir = Align(Velocity, normal.normalized);
					result = Vector3.Lerp(Velocity, Dir, _stickingLerps_.x);
					transform.position = groundHit.point + normal * _negativeGHoverHeight_;
					if (_EnableDebug)
					{
						Debug.DrawRay(groundHit.point, normal * 3, Color.red, 1);
						Debug.DrawRay(transform.position, Dir.normalized * 3, Color.yellow, 1);
						Debug.DrawRay(transform.position + transform.right, Dir.normalized * 3, Color.cyan + Color.black, 1);
					}
				}
			}
			else
			{
				if (Mathf.Abs(DirectionDot) < _stickingNormalLimit_) //avoid SuperSticking
				{
					Vector3 Dir = Align(Velocity, normal.normalized);
					float lerp = _stickingLerps_.y;
					if (Physics.Raycast(Raycasterpos + (_RB.velocity * _stickCastAhead_ * Time.deltaTime), -groundHit.normal, out _hitSticking, 2.5f, _Groundmask_))
					{
						float dist = _hitSticking.distance;
						if (_EnableDebug)
						{
							Debug.Log("PlacedDown" + dist);
							Debug.DrawRay(Raycasterpos + (_RB.velocity * _stickCastAhead_ * Time.deltaTime), -groundHit.normal * 3, Color.cyan, 2);
						}
						if (dist > 1.5f)
						{
							if (_EnableDebug) Debug.Log("ForceDown");
							lerp = 5;
							result += (-groundHit.normal * 10);
							transform.position = groundHit.point + normal * _negativeGHoverHeight_;
						}
					}

					result = Vector3.LerpUnclamped(Velocity, Dir, lerp);

					if (_EnableDebug)
					{
						Debug.Log("Lerp " + lerp + " Result " + result);
						Debug.DrawRay(groundHit.point, normal * 3, Color.green, 0.6f);
						Debug.DrawRay(transform.position, result.normalized * 3, Color.grey, 0.6f);
						Debug.DrawRay(transform.position + transform.right, result.normalized * 3, Color.cyan + Color.black, 0.6f);
					}
				}

			}

			result += (-groundHit.normal * 2); // traction addition
		}
		if (_EnableDebug)
		{
			Debug.Log("After: " + result + "speed " + result.magnitude);
		}
		return result;

	}



	Vector3 Align ( Vector3 vector, Vector3 normal ) {
		//typically used to rotate a movement vector by a surface normal
		Vector3 tangent = Vector3.Cross(normal, vector);
		Vector3 newVector = -Vector3.Cross(normal, tangent);
		vector = newVector.normalized * vector.magnitude;
		return vector;
	}

	void AirMovement () {
		Vector3 setVelocity;
		//AddSpeed
		//Air Skidding  
		//if (b_normalSpeed < 0 && (Action.Action  == ActionManager.States.Regular || Action.Action == ActionManager.States.Jump || Action.Action == ActionManager.States.Hovering))
		//{
		//    Debug.Log(MoveInput * AirSkiddingForce * MoveAccell);
		//    setVelocity = HandleGroundControl(1, (MoveInput * AirSkiddingForce) * MoveAccell);
		//}

		if (_moveInput.sqrMagnitude > 0.1f)
		{
			float airMod = 1;
			float airMoveMod = 1;
			if (_horizontalSpeedMagnitude < 15)
			{
				airMod += 2f;
				airMoveMod += 3f;
			}
			if (_Action.whatAction == S_Enums.PlayerStates.Jump)
			{
				//Debug.Log(Action.Action01.timeJumping);
				if (_Action.Action01.ControlCounter < 0.5)
				{
					airMod += 1f;
					airMoveMod += 2f;
				}
				else if (_Action.Action01.ControlCounter > 5)
				{
					airMod -= 1f;
					airMoveMod -= 4f;
				}

			}
			else if (_Action.whatAction == S_Enums.PlayerStates.Bounce)
			{
				airMod += 1f;
				airMoveMod += 2.5f;
			}
			airMoveMod = Mathf.Clamp(airMoveMod, 0.8f, 10);
			airMod = Mathf.Clamp(airMod, 0.8f, 10);

			setVelocity = HandleGroundControl(_airControlAmmount_ * airMod, _moveInput * _moveAccell * airMoveMod);
		}
		else
		{
			setVelocity = HandleGroundControl(_airControlAmmount_, _moveInput * _moveAccell);

			if (_moveInput == Vector3.zero && _shouldStopAirMovementIfNoInput_)
			{
				Vector3 ReducedSpeed = setVelocity;
				ReducedSpeed.x = ReducedSpeed.x / _airDecell_;
				ReducedSpeed.z = ReducedSpeed.z / _airDecell_;
				//setVelocity = ReducedSpeed;
			}

		}
		//Get out of roll
		_isRolling = false;


		if (_horizontalSpeedMagnitude > 14)
		{
			Vector3 ReducedSpeed = setVelocity;
			ReducedSpeed.x = ReducedSpeed.x / _naturalAirDecel_;
			ReducedSpeed.z = ReducedSpeed.z / _naturalAirDecel_;
			//setVelocity = ReducedSpeed;
		}

		//Get set for landing
		_wasInAir = true;



		//Apply Gravity
		if (_isGravityOn)
			setVelocity += Gravity((int)setVelocity.y);

		//if(setVelocity.y > rb.velocity.y)
		//    Debug.Log("Gravity is = " +Gravity((int)setVelocity.y).y);

		//Max Falling Speed
		if (_RB.velocity.y < _maxFallingSpeed_)
		{
			setVelocity = new Vector3(setVelocity.x, _maxFallingSpeed_, setVelocity.z);
		}

		_RB.velocity = setVelocity;


	}
	Vector3 Gravity ( int vertSpeed ) {

		if (vertSpeed < 0)
		{
			return _fallGravity_;
		}
		else
		{
			int gravMod;
			if (vertSpeed > 70)
				gravMod = vertSpeed / 12;
			else
				gravMod = vertSpeed / 8;
			float applyMod = 1 + (gravMod * 0.1f);

			Vector3 newGrav = new Vector3(0f, _upGravity_.y * applyMod, 0f);

			return newGrav;
		}

	}

	void CheckForGround () {
		_RayToGroundDistancecor = _rayToGroundDistance_;
		_RayToGroundRotDistancecor = _rayToGroundRotDistance_;
		if (_Action.whatAction == 0 && _isGrounded)
		{
			//grounder line
			_RayToGroundDistancecor = Mathf.Max(_rayToGroundDistance_ + (_speedMagnitude * _raytoGroundSpeedRatio_), _rayToGroundDistance_);
			_RayToGroundDistancecor = Mathf.Min(_RayToGroundDistancecor, _raytoGroundSpeedMax_);

			//rotorline
			_RayToGroundRotDistancecor = Mathf.Max(_rayToGroundRotDistance_ + (_speedMagnitude * _raytoGroundSpeedRatio_), _rayToGroundRotDistance_);
			_RayToGroundRotDistancecor = Mathf.Min(_RayToGroundRotDistancecor, _raytoGroundRotSpeedMax_);

		}
		if (_EnableDebug)
		{
			Debug.DrawRay(transform.position + (transform.up * 2) + -transform.right, -transform.up * (2f + _RayToGroundDistancecor), Color.yellow);
		}
		//Debug.Log(GravityAffects);

		if (Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out groundHit, 2f + _RayToGroundDistancecor, _Groundmask_))
		{
			_groundNormal = groundHit.normal;
			SetIsGrounded(true);
			GroundMovement();
		}
		else if (_Action.whatAction != S_Enums.PlayerStates.Bounce && _Action.whatAction != S_Enums.PlayerStates.WallRunning && _Action.whatAction != S_Enums.PlayerStates.Rail)
		{
			SetIsGrounded(false);
			_groundNormal = Vector3.zero;
			AirMovement();
		}
	}


	#endregion
	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	public void AlignWithGround () {


		//if ((Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out hitRot, 2f + RayToGroundRotDistancecor, Playermask)))
		if (_isGrounded)
		{
			_groundNormal = groundHit.normal;

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

	public void SetIsGrounded( bool value ) {
		if (_isGrounded != value)
		{
			_isGrounded = value;
			if (!_isGrounded) { _TimeOnGround = 0; }
		}
		
	}

	public void AddVelocity ( Vector3 force ) {
		_RB.velocity += force;
	}

	#endregion
	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	//Matches all changeable stats to how they are set in the character stats script.
	private void AssignStats () {
		_startAcceleration_ = _Tools.Stats.AccelerationStats.acceleration;
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
		_startDownhillMultiplier_ = _Tools.Stats.SlopeStats.startDownhillMultiplier;
		_slopePowerOverSpeed_ = _Tools.Stats.SlopeStats.SlopePowerByCurrentSpeed;
		_airControlAmmount_ = _Tools.Stats.WhenInAir.controlAmmount;

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

		//Sets all changeable core values to how they are set to start in the editor.
		_moveAccell = _startAcceleration_;
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

		CollisionPoint = _Tools.CollisionPoint;
		sounds = _Tools.SoundControl;
	}

	#endregion
}


//using UnityEngine;
//using System.Collections;
//using TMPro;
//using System.Collections.Generic;
//using static UnityEngine.Rendering.DebugUI;

//public class S_PlayerPhysics : MonoBehaviour
//{

//	/// <summary>
//	/// Members ----------------------------------------------------------------------------------
//	/// </summary>
//	/// 
//	#region members

//	//Unity
//	#region Unity Specific Members
//	[HideInInspector] public S_ActionManager _Action;
//	S_CharacterTools _Tools;
//	Transform _MainSkin;

//	static public S_PlayerPhysics s_MasterPlayer;
//	public Rigidbody _RB { get; set; }

//	[HideInInspector] public RaycastHit groundHit;

//	S_Control_SoundsPlayer _SoundController;

//	#endregion


//	//General
//	#region General Members

//	//Stats
//	#region Stats

//	[Header("Grounded Movement")]
//	private float                 _startAcceleration_ = 0.5f;
//	private float                 _moveAccell;
//	private AnimationCurve        _accelOverSpeed_;
//	private float                 _accelShiftOverSpeed_;
//	private float                 _decelShiftOverSpeed_;

//	[HideInInspector]
//	public float                  _moveDeceleration_ = 1.3f;
//	AnimationCurve                _decelBySpeed_;
//	private float                 _airDecell_ = 1.05f;
//	private float                 _naturalAirDecel_ = 1.01f;

//	private float                 _tangentialDrag_;
//	private float                 _tangentialDragShiftSpeed_;
//	private float                 _turnSpeed_ = 16f;
//	private AnimationCurve        _turnRateOverAngle_;
//	private AnimationCurve        _turnRateOverSpeed_;
//	private AnimationCurve        _tangDragOverAngle_;
//	private AnimationCurve        _tangDragOverSpeed_;

//	private float                 _startTopSpeed_ = 65f;
//	private float                 _startMaxSpeed_ = 230f;
//	private float                 _startMaxFallingSpeed_ = -500f;

//	[Header("Slopes")]
//	private float                 _slopeEffectLimit_ = 0.9f;
//	private float                  _standOnSlopeLimit_ = 0.8f;
//	private float                 _slopePower_ = 0.5f;
//	private float                 _slopeRunningAngleLimit_ = 0.5f;
//	private AnimationCurve        _slopeSpeedLimit_;

//	private float                 _generalHillMultiplier_ = 1;
//	private float                 _uphillMultiplier_ = 0.5f;
//	private float                 _downhillMultiplier_ = 2;
//	private float                 _startDownhillMultiplier_ = -7;

//	private AnimationCurve        _slopePowerOverSpeed_;
//	private AnimationCurve        _UpHillByTime_;

//	[Header("Air Movement Extras")]
//	float                         _airControlAmmount_ = 2;
//	private bool                  _shouldStopAirMovementIfNoInput_ = false;
//	private float                 _keepNormalForThis_ = 0.083f;
//	private float                 _maxFallingSpeed_;
//	[HideInInspector]
//	public float                  _homingDelay_;
//	private Vector3               _upGravity_;
//	[HideInInspector]
//	public Vector3                _startFallGravity_;
//	[HideInInspector]
//	public Vector3                _fallGravity_;

//	[Header("Rolling Values")]
//	float                         _rollingLandingBoost_;
//	private float                 _rollingDownhillBoost_;
//	private float                 _rollingUphillBoost_;
//	private float                 _rollingStartSpeed_;
//	private float                 _rollingTurningDecrease_;
//	private float                 _rollingFlatDecell_;
//	private float                 _slopeTakeoverAmount_; // This is the normalized slope angle that the player has to be in order to register the land as "flat"

//	[Header("Stick To Ground")]
//	private Vector2               _stickingLerps_ = new Vector2(0.885f, 1.5f);
//	private float                 _stickingNormalLimit_ = 0.4f;
//	private float                 _stickCastAhead_ = 1.9f;
//	[HideInInspector]
//	public float                  _negativeGHoverHeight_ = 0.6115f;
//	private float                 _rayToGroundDistance_ = 0.55f;
//	private float                 _raytoGroundSpeedRatio_ = 0.01f;
//	private float                 _raytoGroundSpeedMax_ = 2.4f;
//	private float                 _rayToGroundRotDistance_ = 1.1f;
//	private float                 _raytoGroundRotSpeedMax_ = 2.6f;
//	private float                 _rotationResetThreshold_ = -0.1f;

//	[HideInInspector]
//	public LayerMask              _Groundmask_;
//	#endregion

//	// Trackers
//	#region trackers
//	[HideInInspector]
//	public bool                   _arePhysicsOn = true;

//	[HideInInspector]
//	public Vector3                _coreVelocity;
//	[HideInInspector]
//	public Vector3                _environmentalVelocity;
//	[HideInInspector]
//	public Vector3                _totalVelocity;

//	public float _speedMagnitude { get; set; }
//	public float _horizontalSpeedMagnitude { get; set; }

//	public Vector3 _moveInput { get; set; }
//	public Vector3 PreviousInput { get; set; }
//	public Vector3 RawInput { get; set; }
//	public Vector3 PreviousRawInput { get; set; }
//	[HideInInspector]
//	public float                  _currentTopSpeed;
//	[HideInInspector]
//	public float                  _currentMaxSpeed;
//	public float curvePosAcell { get; set; }
//	private float                 curvePosDecell = 1f;
//	public float curvePosTang { get; set; }
//	public float curvePosSlope { get; set; }
//	public float b_normalSpeed { get; set; }
//	public Vector3 b_normalVelocity { get; set; }
//	public Vector3 b_tangentVelocity { get; set; }
//	[Tooltip("A quick reference to the players current location")]
//	public Vector3 _playerPos { get; set; }

//	private float                 _timeUpHill;
//	private float                 _slopePowerShiftSpeed;
//	private float                 _landingConversionFactor = 2;

//	[Tooltip("Used to check if the player is currently grounded. _isGrounded")]
//	public bool _isGrounded { get; set; }
//	public Vector3 _groundNormal { get; set; }
//	public Vector3 _collisionPointsNormal { get; set; }
//	private Vector3               _KeepNormal;
//	private float                 _KeepNormalCounter;
//	public bool _wasInAir { get; set; }
//	[HideInInspector]
//	public bool                   _isGravityOn = true;

//	public bool _isRolling { get; set; }

//	[Header("Greedy Stick Fix")]
//	public bool                   _EnableDebug;
//	public float _TimeOnGround { get; set; }
//	private RaycastHit            _hitSticking, hitRot;
//	[HideInInspector]
//	//public float		_RayToGroundDistancecor, _RayToGroundRotDistancecor;

//	#endregion
//	#endregion
//	#endregion


//	/// <summary>
//	/// Inherited ----------------------------------------------------------------------------------
//	/// </summary>
//	/// 
//	#region Inherited

//	//On start, assigns stats.
//	private void Start () {
//		_Tools = GetComponent<S_CharacterTools>();
//		AssignTools();
//		AssignStats();
//		_MainSkin = _Tools.mainSkin;
//	}

//	//On FixedUpdate, increases time on ground and call HandleGeneralPhysics if relevant.
//	void FixedUpdate () {


//		if (_isGrounded) { _TimeOnGround += Time.deltaTime; };
//		if (_Action.whatAction != S_Enums.PlayerStates.Path)
//			HandleGeneralPhysics();

//		if (_homingDelay_ > 0)
//		{
//			_homingDelay_ -= Time.deltaTime;
//		}
//	}

//	//Sets public variables relevant to other calculations 
//	void Update () {
//		_speedMagnitude = _RB.velocity.magnitude;
//		Vector3 releVec = GetRelevantVec(_RB.velocity);
//		_horizontalSpeedMagnitude = new Vector3(releVec.x, 0f, releVec.z).magnitude;

//		_playerPos = transform.position;
//	}

//	//Involed in sticking calculations
//	#region TRYSCRAP
//	//public void OnCollisionStay ( Collision col ) {
//	//	Vector3 prevNormal = _groundNormal;
//	//	foreach (ContactPoint contact in col.contacts)
//	//	{

//	//		//Set Middle Point
//	//		Vector3 pointSum = Vector3.zero;
//	//		Vector3 normalSum = Vector3.zero;
//	//		for (int i = 0 ; i < col.contacts.Length ; i++)
//	//		{
//	//			pointSum = pointSum + col.contacts[i].point;
//	//			normalSum = normalSum + col.contacts[i].normal;
//	//		}

//	//		pointSum = pointSum / col.contacts.Length;
//	//		_collisionPointsNormal = normalSum / col.contacts.Length;

//	//		if (rb.velocity.normalized != Vector3.zero)
//	//		{
//	//			CollisionPoint.position = pointSum;
//	//		}
//	//	}
//	//}
//	#endregion
//	#endregion

//	/// <summary>
//	/// Private ----------------------------------------------------------------------------------
//	/// </summary>
//	/// 
//	#region private

//	//Manages the character's physics, calling the relevant functions.
//	void HandleGeneralPhysics () {
//		if (!_arePhysicsOn) { return; }
//		//Set Curve thingies
//		//curvePosAcell = Mathf.Lerp(curvePosAcell, _accelOverSpeed_.Evaluate((rb.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _accelShiftOverSpeed_);
//		//curvePosDecell = Mathf.Lerp(curvePosDecell, _decelBySpeed_.Evaluate((rb.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _decelShiftOverSpeed_);
//		//curvePosTang = Mathf.Lerp(curvePosTang, _tangDragOverSpeed_.Evaluate((rb.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _tangentialDragShiftSpeed_);
//		//curvePosSlope = Mathf.Lerp(curvePosSlope, _slopePowerOverSpeed_.Evaluate((rb.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed), Time.fixedDeltaTime * _slopePowerShiftSpeed);

//		//Get curve positions, which will be used in calculations for this frame.
//		curvePosAcell = _accelOverSpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);
//		curvePosDecell = _decelBySpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);
//		curvePosTang = _tangDragOverSpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);
//		curvePosSlope = _slopePowerOverSpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);

//		// Clamp horizontal running speed.
//		if (_horizontalSpeedMagnitude > _currentMaxSpeed)
//		{
//			Vector3 ReducedSpeed = _RB.velocity;
//			float keepY = _RB.velocity.y;
//			ReducedSpeed = Vector3.ClampMagnitude(ReducedSpeed, _currentMaxSpeed);
//			ReducedSpeed.y = keepY;
//			_RB.velocity = ReducedSpeed;
//		}

//		//Do it for Y
//		//if (Mathf.Abs(rb.velocity.y) > MaxFallingSpeed)
//		//{
//		//    Vector3 ReducedSpeed = rb.velocity;
//		//    float keepX = rb.velocity.x;
//		//    float keepZ = rb.velocity.z;
//		//    ReducedSpeed = Vector3.ClampMagnitude(ReducedSpeed, MaxSpeed);
//		//    ReducedSpeed.x = keepX;
//		//    ReducedSpeed.z = keepZ;
//		//    rb.velocity = ReducedSpeed;
//		//}

//		//Finds out if grounded, then calls the appropriate movement handler.
//		CheckForGround();
//		if (_isGrounded)
//			GroundMovement();
//		else
//			AirMovement();
//		setTotalVelocity();
//		AlignWithGround();
//	}

//	//Determines if the player is on the ground and sets _isGrounded to the answer.
//	void CheckForGround () {

//		//Sets the size of the ray to check for ground. 
//		float _rayToGroundDistancecor = _rayToGroundDistance_;
//		if (_Action.whatAction == 0 && _isGrounded)
//		{
//			//grounder line
//			_rayToGroundDistancecor = Mathf.Max(_rayToGroundDistance_ + (_speedMagnitude * _raytoGroundSpeedRatio_), _rayToGroundDistance_);
//			_rayToGroundDistancecor = Mathf.Min(_rayToGroundDistancecor, _raytoGroundSpeedMax_);

//		}

//		//Uses the ray to check for ground, if found, sets grounded to true and takes the normal.
//		if (Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out groundHit, 2f + _rayToGroundDistancecor, _Groundmask_))
//		{
//			_groundNormal = groundHit.normal;

//			SetIsGrounded(true);
//		}
//		//If not found, then sets grounded to false.
//		else
//		{
//			_groundNormal = Vector3.zero;

//			SetIsGrounded(false);
//		}
//	}

//	private void setTotalVelocity () {
//		_totalVelocity = _coreVelocity + _environmentalVelocity;
//		_RB.velocity = _totalVelocity;
//	}

//	private void GroundMovement () {

//		//Slope Physics
//		HandleSlopePhysics();

//		// Call Control with unaltered input.
//		_coreVelocity = HandleGroundControl(1, _moveInput * curvePosAcell);

//	}

//	//Handles core velocity, which is the velocity directly under the player's control (seperate from environmental velocity which is placed on the character by other things).
//	//This decreases or increases the velocity based on input.
//	Vector3 HandleGroundControl ( float deltaTime, Vector3 input ) {

//		if (_Action.whatAction == S_Enums.PlayerStates.JumpDash || _Action.whatAction == S_Enums.PlayerStates.WallRunning) { return _coreVelocity; }

//		//Original by Damizean, edited by Blaephid

//		//Gets current running velocity, then splits it into horizontal and vertical velocity relative to the character.
//		//This means running up a wall will have zero vertical velocity because the character isn't moveing up relative to their rotation.
//		Vector3 velocity = _coreVelocity;
//		Vector3 localVelocity = transform.InverseTransformDirection(velocity);
//		Vector3 lateralVelocity = new Vector3(localVelocity.x, 0.0f, localVelocity.z);
//		Vector3 verticalVelocity = new Vector3(0.0f, localVelocity.y, 0.0f);

//		// If there is some input...
//		if (input.sqrMagnitude != 0.0f)
//		{

//			// Normalize to get input direction and magnitude seperately
//			Vector3 inputDirection = input.normalized;
//			float inputMagnitude = input.magnitude;

//			// Step 1) Determine angle between current lateral velocity and desired direction.
//			//         Creates a quarternion which rotates to the direction, which will be zero if velocity is too slow.

//			float deviationFromInput = Vector3.Angle(lateralVelocity, inputDirection) / 180.0f;
//			Quaternion lateralToInput = Mathf.Approximately(lateralVelocity.sqrMagnitude, 0.0f)
//				? Quaternion.identity
//				: Quaternion.FromToRotation(lateralVelocity.normalized, inputDirection);

//			// Step 2) Rotate lateral velocity towards the same velocity under the desired rotation.
//			//         The ammount rotated is determined by turn speed multiplied by turn rate (defined by the difference in angles, and current speed).

//			float turnRate = _turnRateOverAngle_.Evaluate(deviationFromInput);
//			turnRate *= _turnRateOverSpeed_.Evaluate((_RB.velocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);
//			lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, _turnSpeed_ * turnRate * Time.deltaTime, 0.0f);


//			// Step 3) Further lateral velocity into normal (in the input direction) and tangential
//			//         components. Note: normalSpeed is the magnitude of normalVelocity, with the added
//			//         bonus that it's signed. If positive, the speed goes towards the same
//			//         direction than the input :)

//			var normalDot = Vector3.Dot(lateralVelocity.normalized, inputDirection.normalized);

//			float normalSpeed = Vector3.Dot(lateralVelocity, inputDirection);
//			Vector3 normalVelocity = inputDirection * normalSpeed;
//			Vector3 tangentVelocity = lateralVelocity - normalVelocity;
//			float tangentSpeed = tangentVelocity.magnitude;

//			// Step 4) Apply user control in this direction.

//			if (normalSpeed < _currentTopSpeed)
//			{
//				// Accelerate towards the input direction.
//				normalSpeed += (_isRolling ? 0 : _moveAccell) * deltaTime * inputMagnitude;

//				normalSpeed = Mathf.Min(normalSpeed, _currentTopSpeed);

//				// Rebuild back the normal velocity with the correct modulus.

//				normalVelocity = inputDirection * normalSpeed;
//			}

//			// Step 5) Dampen tangential components.

//			float dragRate = _tangDragOverAngle_.Evaluate(deviationFromInput)
//			* _tangDragOverSpeed_.Evaluate((tangentSpeed * tangentSpeed) / (_currentMaxSpeed * _currentMaxSpeed));

//			tangentVelocity = Vector3.MoveTowards(tangentVelocity, Vector3.zero, _tangentialDrag_ * dragRate * deltaTime);

//			lateralVelocity = normalVelocity + tangentVelocity;

//			//Export nescessary variables

//			b_normalSpeed = normalSpeed;
//			b_normalVelocity = normalVelocity;
//			b_tangentVelocity = tangentVelocity;

//		}

//		// Otherwise, apply some damping as to decelerate Sonic.
//		if (_isGrounded)
//		{
//			float DecellAmount = 1;
//			if (_isRolling && _groundNormal.y > _slopeTakeoverAmount_ && _horizontalSpeedMagnitude > 10)
//			{
//				DecellAmount = _rollingFlatDecell_ * curvePosDecell;
//				if (input.sqrMagnitude == 0)
//					DecellAmount *= _moveDeceleration_;
//			}

//			else if (input.sqrMagnitude == 0)
//			{
//				DecellAmount = _moveDeceleration_ * curvePosDecell;
//			}
//			lateralVelocity /= DecellAmount;
//		}


//		// Compose local velocity back and compute velocity back into the Global frame.

//		localVelocity = lateralVelocity + verticalVelocity;

//		//new line for the stick to ground from GREEDY


//		velocity = transform.TransformDirection(localVelocity);

//		if (_isGrounded)
//			velocity = StickToGround(velocity);

//		return velocity;


//	}

//	private void HandleSlopePhysics () {
//		//ApplyLandingSpeed
//		if (_wasInAir)
//		{
//			Vector3 Addsped;

//			if (!_isRolling)
//			{
//				Addsped = _groundNormal * _landingConversionFactor;
//				//StickToGround(GroundStickingPower);
//			}
//			else
//			{
//				Addsped = (_groundNormal * _landingConversionFactor) * _rollingLandingBoost_;
//				//StickToGround(GroundStickingPower * RollingLandingBoost);
//				_SoundController.SpinningSound();
//			}

//			Addsped.y = 0;
//			AddVelocity(Addsped);
//			_wasInAir = false;
//		}

//		//Get out of slope if speed is too low
//		if (_horizontalSpeedMagnitude < _slopeSpeedLimit_.Evaluate(_groundNormal.y))
//		{
//			if (_slopeRunningAngleLimit_ > _groundNormal.y)
//			{
//				//transform.rotation = Quaternion.identity;
//				SetIsGrounded(false);
//				AddVelocity(_groundNormal * 1.5f);
//			}

//		}



//		//Apply slope power
//		if (_groundNormal.y < _slopeEffectLimit_)
//		{

//			if (_timeUpHill < 0)
//				_timeUpHill = 0;

//			if (_RB.velocity.y > _startDownhillMultiplier_)
//			{
//				_timeUpHill += Time.deltaTime;
//				//Debug.Log(p_rigidbody.velocity.y);
//				if (!_isRolling)
//				{
//					Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _uphillMultiplier_ * _generalHillMultiplier_, 0);
//					force *= _UpHillByTime_.Evaluate(_timeUpHill);
//					AddVelocity(force);
//				}
//				else
//				{
//					Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _uphillMultiplier_ * _generalHillMultiplier_, 0) * _rollingUphillBoost_;
//					force *= _UpHillByTime_.Evaluate(_timeUpHill);
//					AddVelocity(force);
//				}
//			}

//			else
//			{
//				_timeUpHill -= Time.deltaTime * 0.8f;
//				if (_moveInput != Vector3.zero && b_normalSpeed > 0)
//				{
//					if (!_isRolling)
//					{
//						Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _downhillMultiplier_ * _generalHillMultiplier_, 0);
//						AddVelocity(force);
//					}
//					else
//					{
//						Vector3 force = new Vector3(0, (_slopePower_ * curvePosSlope) * _downhillMultiplier_ * _generalHillMultiplier_, 0) * _rollingDownhillBoost_;
//						AddVelocity(force);
//					}

//				}
//				else if (_groundNormal.y < _standOnSlopeLimit_)
//				{
//					Vector3 force = new Vector3(0, _slopePower_ * curvePosSlope, 0);
//					AddVelocity(force);
//				}
//			}
//		}
//		else
//			_timeUpHill = 0;

//	}

//	private Vector3 StickToGround ( Vector3 Velocity ) {
//		Vector3 result = Velocity;
//		if (_EnableDebug)
//		{
//			Debug.Log("Before: " + result + "speed " + result.magnitude);
//		}
//		if (_TimeOnGround > 0.1f && _speedMagnitude > 1)
//		{
//			float DirectionDot = Vector3.Dot(_RB.velocity.normalized, groundHit.normal);
//			Vector3 normal = groundHit.normal;
//			Vector3 Raycasterpos = _RB.position + (groundHit.normal * -0.12f);

//			if (_EnableDebug)
//			{
//				Debug.Log("Speed: " + _speedMagnitude + "\n Direction DOT: " + DirectionDot + " \n Velocity Normal:" + _RB.velocity.normalized + " \n  Ground normal : " + groundHit.normal);
//				Debug.DrawRay(groundHit.point + (transform.right * 0.2F), groundHit.normal * 3, Color.yellow, 1);
//			}

//			//If the Raycast Hits something, it adds it's normal to the ground normal making an inbetween value the interpolates the direction;
//			if (Physics.Raycast(Raycasterpos, _RB.velocity.normalized, out _hitSticking, _speedMagnitude * _stickCastAhead_ * Time.deltaTime, _Groundmask_))
//			{
//				if (_EnableDebug) Debug.Log("AvoidingGroundCollision");

//				if (Vector3.Dot(normal, _hitSticking.normal) > 0.15f) //avoid flying off Walls
//				{
//					normal = _hitSticking.normal.normalized;
//					Vector3 Dir = Align(Velocity, normal.normalized);
//					result = Vector3.Lerp(Velocity, Dir, _stickingLerps_.x);
//					transform.position = groundHit.point + normal * _negativeGHoverHeight_;
//					if (_EnableDebug)
//					{
//						Debug.DrawRay(groundHit.point, normal * 3, Color.red, 1);
//						Debug.DrawRay(transform.position, Dir.normalized * 3, Color.yellow, 1);
//						Debug.DrawRay(transform.position + transform.right, Dir.normalized * 3, Color.cyan + Color.black, 1);
//					}
//				}
//			}
//			else
//			{
//				if (Mathf.Abs(DirectionDot) < _stickingNormalLimit_) //avoid SuperSticking
//				{
//					Vector3 Dir = Align(Velocity, normal.normalized);
//					float lerp = _stickingLerps_.y;
//					if (Physics.Raycast(Raycasterpos + (_RB.velocity * _stickCastAhead_ * Time.deltaTime), -groundHit.normal, out _hitSticking, 2.5f, _Groundmask_))
//					{
//						float dist = _hitSticking.distance;
//						if (_EnableDebug)
//						{
//							Debug.Log("PlacedDown" + dist);
//							Debug.DrawRay(Raycasterpos + (_RB.velocity * _stickCastAhead_ * Time.deltaTime), -groundHit.normal * 3, Color.cyan, 2);
//						}
//						if (dist > 1.5f)
//						{
//							if (_EnableDebug) Debug.Log("ForceDown");
//							lerp = 5;
//							result += (-groundHit.normal * 10);
//							transform.position = groundHit.point + normal * _negativeGHoverHeight_;
//						}
//					}

//					result = Vector3.LerpUnclamped(Velocity, Dir, lerp);

//					if (_EnableDebug)
//					{
//						Debug.Log("Lerp " + lerp + " Result " + result);
//						Debug.DrawRay(groundHit.point, normal * 3, Color.green, 0.6f);
//						Debug.DrawRay(transform.position, result.normalized * 3, Color.grey, 0.6f);
//						Debug.DrawRay(transform.position + transform.right, result.normalized * 3, Color.cyan + Color.black, 0.6f);
//					}
//				}

//			}

//			result += (-groundHit.normal * 2); // traction addition
//		}
//		if (_EnableDebug)
//		{
//			Debug.Log("After: " + result + "speed " + result.magnitude);
//		}
//		return result;

//	}


//	Vector3 Align ( Vector3 vector, Vector3 normal ) {
//		//typically used to rotate a movement vector by a surface normal
//		Vector3 tangent = Vector3.Cross(normal, vector);
//		Vector3 newVector = -Vector3.Cross(normal, tangent);
//		vector = newVector.normalized * vector.magnitude;
//		return vector;
//	}

//	void AirMovement () {

//		//AddSpeed
//		if (_moveInput.sqrMagnitude > 0.1f)
//		{
//			float airMod = 1;
//			float airMoveMod = 1;
//			if (_horizontalSpeedMagnitude < 15)
//			{
//				airMod += 2f;
//				airMoveMod += 3f;
//			}
//			if (_Action.whatAction == S_Enums.PlayerStates.Jump)
//			{
//				//Debug.Log(Action.Action01.timeJumping);
//				if (_Action.Action01.ControlCounter < 0.5)
//				{
//					airMod += 1f;
//					airMoveMod += 2f;
//				}
//				else if (_Action.Action01.ControlCounter > 5)
//				{
//					airMod -= 1f;
//					airMoveMod -= 4f;
//				}

//			}
//			else if (_Action.whatAction == S_Enums.PlayerStates.Bounce)
//			{
//				airMod += 1f;
//				airMoveMod += 2.5f;
//			}
//			airMoveMod = Mathf.Clamp(airMoveMod, 0.8f, 10);
//			airMod = Mathf.Clamp(airMod, 0.8f, 10);

//			_coreVelocity = HandleGroundControl(_airControlAmmount_ * airMod, _moveInput * _moveAccell * airMoveMod);
//		}
//		else
//		{
//			_coreVelocity = HandleGroundControl(_airControlAmmount_, _moveInput * _moveAccell);

//			if (_moveInput == Vector3.zero && _shouldStopAirMovementIfNoInput_)
//			{
//				Vector3 ReducedSpeed = _coreVelocity;
//				ReducedSpeed.x = ReducedSpeed.x / _airDecell_;
//				ReducedSpeed.z = ReducedSpeed.z / _airDecell_;
//				//setVelocity = ReducedSpeed;
//			}

//		}
//		//Get out of roll
//		_isRolling = false;


//		if (_horizontalSpeedMagnitude > 14)
//		{
//			Vector3 ReducedSpeed = _coreVelocity;
//			ReducedSpeed.x = ReducedSpeed.x / _naturalAirDecel_;
//			ReducedSpeed.z = ReducedSpeed.z / _naturalAirDecel_;
//			//setVelocity = ReducedSpeed;
//		}

//		//Get set for landing
//		_wasInAir = true;



//		//Apply Gravity
//		if (_isGravityOn)
//			_coreVelocity += Gravity((int)_coreVelocity.y);

//		//if(setVelocity.y > rb.velocity.y)
//		//    Debug.Log("Gravity is = " +Gravity((int)setVelocity.y).y);

//		//Max Falling Speed
//		if (_RB.velocity.y < _maxFallingSpeed_)
//		{
//			_coreVelocity = new Vector3(_coreVelocity.x, _maxFallingSpeed_, _coreVelocity.z);
//		}

//		_RB.velocity = _coreVelocity;


//	}
//	Vector3 Gravity ( int vertSpeed ) {

//		if (vertSpeed < 0)
//		{
//			return _fallGravity_;
//		}
//		else
//		{
//			int gravMod;
//			if (vertSpeed > 70)
//				gravMod = vertSpeed / 12;
//			else
//				gravMod = vertSpeed / 8;
//			float applyMod = 1 + (gravMod * 0.1f);

//			Vector3 newGrav = new Vector3(0f, _upGravity_.y * applyMod, 0f);

//			return newGrav;
//		}

//	}


//	#endregion
//	/// <summary>
//	/// Public ----------------------------------------------------------------------------------
//	/// </summary>
//	/// 
//	#region public 
//	public void AlignWithGround () {


//		//if ((Physics.Raycast(transform.position + (transform.up * 2), -transform.up, out hitRot, 2f + RayToGroundRotDistancecor, Playermask)))
//		if (_isGrounded)
//		{
//			_groundNormal = groundHit.normal;

//			_KeepNormal = _groundNormal;

//			transform.rotation = Quaternion.FromToRotation(transform.up, _groundNormal) * transform.rotation;
//			//transform.rotation = Quaternion.LookRotation(Vector3.forward, GroundNormal);
//			//transform.up = GroundNormal;

//			_KeepNormalCounter = 0;

//		}
//		else
//		{
//			//Keep the rotation after exiting the ground for a while, to avoid collision issues.

//			_KeepNormalCounter += Time.deltaTime;
//			if (_KeepNormalCounter < _keepNormalForThis_)
//			//if (KeepNormalCounter < 1f)
//			{
//				transform.rotation = Quaternion.FromToRotation(transform.up, _KeepNormal) * transform.rotation;

//			}
//			else
//			{
//				//Debug.Log(KeepNormal.y);

//				//if (transform.up.y < RotationResetThreshold)
//				if (_KeepNormal.y < _rotationResetThreshold_)
//				{
//					_KeepNormal = Vector3.up;

//					if (_MainSkin.right.y >= -_MainSkin.right.y)
//						transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(transform.up, _MainSkin.right) * transform.rotation, 10f);
//					else
//						transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(transform.up, -_MainSkin.right) * transform.rotation, 10f);

//					if (Vector3.Dot(transform.up, Vector3.up) > 0.99)
//						_KeepNormal = Vector3.up;

//				}
//				else
//				{
//					Quaternion targetRot = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
//					transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 10f);
//				}
//			}
//		}
//	}
//	public Vector3 GetRelevantVec ( Vector3 vec ) {
//		return transform.InverseTransformDirection(vec);
//		//if (!Grounded)
//		//{
//		//    return transform.InverseTransformDirection(vec);
//		//    //Vector3 releVec = transform.InverseTransformDirection(rb.velocity.normalized);
//		//}
//		//else
//		//{
//		//    return transform.InverseTransformDirection(vec);
//		//    return Vector3.ProjectOnPlane(vec, groundHit.normal);
//		//}
//	}

//	public void SetIsGrounded ( bool value ) {
//		if (_isGrounded != value)
//		{
//			_isGrounded = value;
//			if (!_isGrounded) { _TimeOnGround = 0; }
//		}

//	}

//	public void AddVelocity ( Vector3 force ) {
//		_RB.velocity += force;
//	}

//	#endregion
//	/// <summary>
//	/// Assigning ----------------------------------------------------------------------------------
//	/// </summary>
//	#region Assigning
//	//Matches all changeable stats to how they are set in the character stats script.
//	private void AssignStats () {
//		_startAcceleration_ = _Tools.Stats.AccelerationStats.acceleration;
//		_accelOverSpeed_ = _Tools.Stats.AccelerationStats.AccelBySpeed;
//		_accelShiftOverSpeed_ = _Tools.Stats.AccelerationStats.accelShiftOverSpeed;
//		_tangentialDrag_ = _Tools.Stats.TurningStats.tangentialDrag;
//		_tangentialDragShiftSpeed_ = _Tools.Stats.TurningStats.tangentialDragShiftSpeed;
//		_turnSpeed_ = _Tools.Stats.TurningStats.turnSpeed;

//		_turnRateOverAngle_ = _Tools.Stats.TurningStats.TurnRateByAngle;
//		_turnRateOverSpeed_ = _Tools.Stats.TurningStats.TurnRateBySpeed;
//		_tangDragOverAngle_ = _Tools.Stats.TurningStats.TangDragByAngle;
//		_tangDragOverSpeed_ = _Tools.Stats.TurningStats.TangDragBySpeed;
//		_startTopSpeed_ = _Tools.Stats.SpeedStats.topSpeed;
//		_startMaxSpeed_ = _Tools.Stats.SpeedStats.maxSpeed;
//		_startMaxFallingSpeed_ = _Tools.Stats.WhenInAir.startMaxFallingSpeed;
//		_moveDeceleration_ = _Tools.Stats.DecelerationStats.moveDeceleration;
//		_decelBySpeed_ = _Tools.Stats.DecelerationStats.DecelBySpeed;
//		_decelShiftOverSpeed_ = _Tools.Stats.DecelerationStats.decelShiftOverSpeed;
//		_naturalAirDecel_ = _Tools.Stats.DecelerationStats.naturalAirDecel;
//		_airDecell_ = _Tools.Stats.DecelerationStats.airDecel;
//		_slopeEffectLimit_ = _Tools.Stats.SlopeStats.slopeEffectLimit;
//		_standOnSlopeLimit_ = _Tools.Stats.SlopeStats.standOnSlopeLimit;
//		_slopePower_ = _Tools.Stats.SlopeStats.slopePower;
//		_slopeRunningAngleLimit_ = _Tools.Stats.SlopeStats.slopeRunningAngleLimit;
//		_slopeSpeedLimit_ = _Tools.Stats.SlopeStats.SlopeLimitBySpeed;
//		_generalHillMultiplier_ = _Tools.Stats.SlopeStats.generalHillMultiplier;
//		_uphillMultiplier_ = _Tools.Stats.SlopeStats.uphillMultiplier;
//		_downhillMultiplier_ = _Tools.Stats.SlopeStats.downhillMultiplier;
//		_startDownhillMultiplier_ = _Tools.Stats.SlopeStats.startDownhillMultiplier;
//		_slopePowerOverSpeed_ = _Tools.Stats.SlopeStats.SlopePowerByCurrentSpeed;
//		_airControlAmmount_ = _Tools.Stats.WhenInAir.controlAmmount;

//		_shouldStopAirMovementIfNoInput_ = _Tools.Stats.WhenInAir.shouldStopAirMovementWhenNoInput;
//		_rollingLandingBoost_ = _Tools.Stats.RollingStats.rollingLandingBoost;
//		_rollingDownhillBoost_ = _Tools.Stats.RollingStats.rollingDownhillBoost;
//		_rollingUphillBoost_ = _Tools.Stats.RollingStats.rollingUphillBoost;
//		_rollingStartSpeed_ = _Tools.Stats.RollingStats.rollingStartSpeed;
//		_rollingTurningDecrease_ = _Tools.Stats.RollingStats.rollingTurningDecrease;
//		_rollingFlatDecell_ = _Tools.Stats.RollingStats.rollingFlatDecell;
//		_slopeTakeoverAmount_ = _Tools.Stats.RollingStats.slopeTakeoverAmount;
//		_UpHillByTime_ = _Tools.Stats.SlopeStats.UpHillEffectByTime;
//		_startFallGravity_ = _Tools.Stats.WhenInAir.fallGravity;
//		_upGravity_ = _Tools.Stats.WhenInAir.upGravity;
//		_keepNormalForThis_ = _Tools.Stats.WhenInAir.keepNormalForThis;


//		_stickingLerps_ = _Tools.Stats.GreedysStickToGround.stickingLerps;
//		_stickingNormalLimit_ = _Tools.Stats.GreedysStickToGround.stickingNormalLimit;
//		_stickCastAhead_ = _Tools.Stats.GreedysStickToGround.stickCastAhead;
//		_negativeGHoverHeight_ = _Tools.Stats.GreedysStickToGround.negativeGHoverHeight;
//		_rayToGroundDistance_ = _Tools.Stats.GreedysStickToGround.rayToGroundDistance;
//		_raytoGroundSpeedRatio_ = _Tools.Stats.GreedysStickToGround.raytoGroundSpeedRatio;
//		_raytoGroundSpeedMax_ = _Tools.Stats.GreedysStickToGround.raytoGroundSpeedMax;
//		_rayToGroundRotDistance_ = _Tools.Stats.GreedysStickToGround.rayToGroundRotDistance;
//		_raytoGroundRotSpeedMax_ = _Tools.Stats.GreedysStickToGround.raytoGroundRotSpeedMax;
//		_rotationResetThreshold_ = _Tools.Stats.GreedysStickToGround.rotationResetThreshold;
//		_Groundmask_ = _Tools.Stats.GreedysStickToGround.GroundMask;

//		//Sets all changeable core values to how they are set to start in the editor.
//		_moveAccell = _startAcceleration_;
//		_currentTopSpeed = _startTopSpeed_;
//		_currentMaxSpeed = _startMaxSpeed_;
//		_maxFallingSpeed_ = _startMaxFallingSpeed_;
//		_fallGravity_ = _startFallGravity_;

//		_KeepNormal = Vector3.up;


//	}

//	private void AssignTools () {
//		s_MasterPlayer = this;
//		_RB = GetComponent<Rigidbody>();
//		PreviousInput = transform.forward;
//		_Action = GetComponent<S_ActionManager>();
//		_SoundController = _Tools.SoundControl;
//	}

//	#endregion
//}
