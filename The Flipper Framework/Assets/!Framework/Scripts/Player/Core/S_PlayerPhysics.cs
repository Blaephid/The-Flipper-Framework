using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

using UnityEngine.Profiling;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(S_PlayerMovement))]
[RequireComponent(typeof(S_PlayerVelocity))]
public class S_PlayerPhysics : MonoBehaviour
{

	/// <summary>
	/// Members ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region members

	//Unity
	#region Unity Specific Members
	private S_ActionManager       _Actions;
	[HideInInspector]
	public S_PlayerMovement       _PlayerMovement;
	[HideInInspector]
	public S_PlayerVelocity       _PlayerVelocity;
	private S_CharacterTools      _Tools;
	private S_PlayerInput         _Input;
	private S_PlayerEvents        _Events;

	private S_Handler_Camera      _CamHandler;

	static public S_PlayerPhysics s_MasterPlayer;

	[HideInInspector]
	public Rigidbody              _RB;
	private CapsuleCollider       _CharacterCapsule;
	private Transform             _FeetTransform;
	private Transform             _MainSkin;
	#endregion

	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats 



	[Header("Slopes")]
	public bool                   _isUsingSlopePhysics_ = true;

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
	private float                 _keepNormalForThis_ = 0.083f;
	[HideInInspector]
	public float                 _maxFallingSpeed_;
	[HideInInspector]
	public Vector3               _gravityWhenMovingUp_;
	[HideInInspector]
	public Vector3                _startFallGravity_;

	private float                 _jumpExtraControlThreshold_;
	private Vector2               _jumpAirControl_;
	private Vector2               _bounceAirControl_;

	[Header("Rolling Values")]
	private float                 _rollingDownhillBoost_;
	private float                 _rollingUphillBoost_;

	[Header("Stick To Ground")]
	private float                 _forceTowardsGround_;
	private Vector2               _stickingLerps_ = new Vector2(0.885f, 1.5f);
	private float                 _stickingNormalLimit_ = 0.4f;
	private float                 _stickCastAhead_ = 1.9f;
	private AnimationCurve        _upwardsLimitByCurrentSlope_;
	[HideInInspector]
	public float                  _placeAboveGroundBuffer_ = 0.6115f;
	[HideInInspector]
	public Vector2                _rayToGroundDistance_ ;
	private float                 _raytoGroundSpeedRatio_ = 0.01f;
	private float                 _raytoGroundSpeedMax_ = 2.4f;
	private float                 _rotationResetThreshold_ = -0.1f;
	private float                 _stepHeight_ = 0.6f;
	private Vector3                 _groundDifferenceLimit_;

	[HideInInspector]
	public LayerMask              _Groundmask_;


	#endregion
	// Trackers
	#region trackers

	private bool        _isPositiveUpdate;  //Alternates between on and off every update, so can be used universally for anything that should only happen every other frame.
	[HideInInspector]
	public int         _frameCount;         //Used for Debugging, can be set to increase here every frame, and referenced in other scripts.

	[HideInInspector]
	public bool                   _arePhysicsOn = true;         //If false, no changes to velocity will be calculated or applied. This script will be inactive.

	//Updated each frame to get current place on animation curves relevant to movement.
	[HideInInspector]
	public float                  _curvePosSlopePower;


	[HideInInspector]
	public Vector3                _playerPos;         //A quick reference to the players current location
	[HideInInspector]
	public Vector3                _feetOffsetFromCentre;

	private float                 _timeUpHill;        //Tracks how long a player has been running up hill. Decreases when going down hill or on flat ground.

	//Ground tracking
	[HideInInspector]
	public bool                   _isGrounded = false;        //Used to check if the player is currently grounded. _isGrounded
	private Vector3               _groundCheckDirection;        //Set as transform.down at the start of the update, but because the player rotation is changed, this is saved to be used when deciding where to set towards the ground.
	[HideInInspector]
	public bool                   _isCurrentlyOnSlope;        //Used so external scripts can interact differently knowing if player is on a slope being affected by slope physics.
	[HideInInspector]
	public bool                   _canChangeGrounded = true;        //Set externally to prevent player's entering a grounded state.
	[HideInInspector]
	public bool                   _canStickToGround = true;        //Set externally to allow following the ground. Only actions focussed on being grounded will enable this.
	[HideInInspector]
	public RaycastHit             _HitGround;         //Used to check if there is ground under the player's feet, and gets data on it like the normal.
	[HideInInspector]
	public Vector3                _groundNormal;
	private List<Vector3>              _listOfPreviousGroundNormals = new List<Vector3>() { new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3 (0,0,0)};        //Used to prevent the player jittering between two different upwards direction due to matching the rotation of one making the other be detected.
	private Vector3               _keepNormal;        //Used when in the air to remember up direction when the ground was lost.
	private float                 _groundingDelay;    //Set when ground is lost and can't enter grounded state again until it's over.
	[HideInInspector]
	public float                  _timeOnGround;

	//Rotating in air
	private float                 _amountToRotate;
	private bool                  _isUpsideDown;                //Rotating to face up when upside down has a unique approach.
	private bool                  _isRotatingLeft;              //Which way to rotate around to face up from upside down.
	private Vector3               _rotateSidewaysTowards;       //The angle of rotation to follow when rotating from upside down
	private float                 _keepNormalCounter;           //Tracks how before rotating can begin

	//In air
	[HideInInspector]
	public bool                   _wasInAirLastFrame;
	[HideInInspector]
	public Vector3                _currentFallGravity;          //The actual gravity used in calculations, set to start gravity at start, and will return to that after temporary changes expire.
	[HideInInspector]
	public Vector3                _currentUpwardsFallGravity;

	[HideInInspector]
	public bool                   _isRolling;         //Set by the rolling subaction, certain controls are different when rolling.
	[HideInInspector]
	public bool                   _isBoosting = false;         //Set by the boost subaction. This will be used in attacks and changes calculations.

	//Disabling options
	[HideInInspector]
	public List<bool>                   _listOfIsGravityOn = new List<bool>();

	//Disabling aspects of control. These are used as lists because if multiple things disable control, they all have to end it before that control is restored. If they just used single bools, multiple aspects taking control would overlap.
	[HideInInspector]
	public List<bool>             _listOfCanTurns = new List<bool>();
	[HideInInspector]
	public List<bool>             _listOfCanControl = new List<bool>();
	[HideInInspector]
	public List<bool>             _listOfCanDecelerates = new List<bool>();

	public enum EnumControlLimitations
	{
		canTurn,
		canControl,
		canDecelerate,
	}

	//COLLISION TRACKERS
	private List<Collider> _ListOfTriggersEnteredThisFrame = new List<Collider>();
	private List<Collider> _ListOfTriggersExitedThisFrame= new List<Collider>();
	private List<Collision> _ListOfCollisionsStartedThisFrame= new List<Collision>();
	private List<Collider> _ListOfTriggersStayedinThisFrame= new List<Collider>();

	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	//On start, assigns stats.
	private void Awake () {
		_Tools = GetComponent<S_CharacterTools>();
		AssignTools();
		AssignStats();

		SetIsGrounded(false);
	}

	//On FixedUpdate,  call HandleGeneralPhysics if relevant.
	void FixedUpdate () {
		_isCurrentlyOnSlope = false; //Set to false at the end of a frame but will be set to true if slope physics are called next frame.

		HandleGeneralPhysics();

		_isPositiveUpdate = !_isPositiveUpdate; //Alternates at the end of an update, so will be the oppositve value next call.
	}

	private void Update () {
		//if (!_isGrounded)
		//	AlignToGround(_groundNormal, _isGrounded);
	}

	//Sets public variables relevant to other calculations 
	void LateUpdate () {
		_playerPos = transform.position;

		_frameCount++;

	}

	///----Collision Trackers
	//To allow actions with OnTriggerEnter options to be in children of the RigidBody, we use an event to pass down the info.
	//HOWEVER, because S_PlayerVelocity.CheckAndApplyVelocityChanges is called AFTER these (because it has to happen after collision calculations),
	//We don't invoke these events until after said method is called, as onTrigger can lead to velocity changes that go haywire against the calculations made there.
	private void OnTriggerEnter ( Collider other ) {
		_ListOfTriggersEnteredThisFrame.Add(other);
		//_Events._OnTriggerEnter.Invoke(other);
	}
	private void OnTriggerExit ( Collider other ) {
		_ListOfTriggersExitedThisFrame.Add(other);
		//_Events._OnTriggerExit.Invoke(other);
	}

	private void OnCollisionEnter ( Collision collision ) {
		_ListOfCollisionsStartedThisFrame.Add(collision);
		//_Events._OnCollisionEnter.Invoke(collision);
	}

	private void OnTriggerStay ( Collider other ) {
		_ListOfTriggersStayedinThisFrame.Add(other);
		//_Events._OnTriggerStay.Invoke(other);
	}

	//Because we set velocity seperately, the numbers we have aren't always accurate to the actual velocity in world.
	//So we have to check what has happened since they were set and factor in those changes.
	//This must be done first, even before any ontrigger or on collision events, because those can lead to action or velocity changes that mess up the controller.
	public void RespondToCollisions () {
		_PlayerVelocity.CheckAndApplyVelocityChanges();

		for (int i = 0 ; i < _ListOfTriggersEnteredThisFrame.Count ; i++)
		{
			if(_ListOfTriggersEnteredThisFrame[i] != null) //Check, because the object might handle its own responses, which happen after its added but before this.
				_Events._OnTriggerEnter.Invoke(_ListOfTriggersEnteredThisFrame[i]);
		}
		for (int i = 0 ; i < _ListOfTriggersExitedThisFrame.Count ; i++)
		{
			if (_ListOfTriggersExitedThisFrame[i] != null)
				_Events._OnTriggerExit.Invoke(_ListOfTriggersExitedThisFrame[i]);
		}
		for (int i = 0 ; i < _ListOfTriggersStayedinThisFrame.Count ; i++)
		{
			if(_ListOfTriggersStayedinThisFrame[i] != null)
				_Events._OnTriggerStay.Invoke(_ListOfTriggersStayedinThisFrame[i]);
		}
		for (int i = 0 ; i < _ListOfCollisionsStartedThisFrame.Count ; i++)
		{
			if(_ListOfCollisionsStartedThisFrame[i] != null);
				_Events._OnCollisionEnter.Invoke(_ListOfCollisionsStartedThisFrame[i]);
		}
	}

	//This must happen every fixedUpdate, no matter the options, so is called in S_PlayerVelocity because it is always the last class called.
	public void ClearListsOfCollisions () {
		_ListOfCollisionsStartedThisFrame.Clear();
		_ListOfTriggersEnteredThisFrame.Clear();
		_ListOfTriggersExitedThisFrame.Clear();
		_ListOfTriggersStayedinThisFrame.Clear();
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Manages the character's physics, calling the relevant functions.
	//IMPORTANT. Due to the order and the Script Execution Order project settings, this will always be the first Method called every FixedUpdate, before any other script in the game.
	public void HandleGeneralPhysics () {

		Profiler.BeginSample("PlayerPhysics");

		//Get curve positions, which will be used in calculations for this frame.
		_curvePosSlopePower = _slopePowerBySpeed_.Evaluate(_PlayerVelocity._currentRunningSpeed / _PlayerMovement._currentMaxSpeed);

		if (!_arePhysicsOn) { RespondToCollisions(); return; }

		//Set if the player is grounded based on current situation.
		CheckForGround();

		AlignToGround(_groundNormal, _isGrounded);

		//Handle any changes to the velocity between updates. This is why this must be the first method called.
		RespondToCollisions();

		//Calls the appropriate movement handler.
		if (_isGrounded)
			GroundMovement();
		else
			_PlayerVelocity._coreVelocity = HandleAirMovement(_PlayerVelocity._coreVelocity);

		//After all other calculations are made across the scripts, the new velocities are applied to the rigidbody in the PlayerVelocity Script.
		//That is called there because it should only be called at the end of a fixed update, after everything else has been applied.
		Profiler.EndSample();
	}

	//Determines if the player is on the ground and sets _isGrounded to the answer.
	public void CheckForGround () {

		if (_groundingDelay > 0)
		{
			_groundingDelay -= Time.fixedDeltaTime;
			return;
		}
		//Certain actions will prevent being grounded.
		if (!_canChangeGrounded) { return; }

		//Sets the size of the ray to check for ground. If running on the ground then it is typically to avoid flying off the ground.
		float groundCheckerDistance = _rayToGroundDistance_.y;
		if (_isGrounded && _canStickToGround)
		{
			groundCheckerDistance = _rayToGroundDistance_.x + (_PlayerVelocity._horizontalSpeedMagnitude * _raytoGroundSpeedRatio_);
			groundCheckerDistance = Mathf.Min(groundCheckerDistance, _raytoGroundSpeedMax_);
		}

		_groundCheckDirection = -transform.up;

		//Uses the ray to check for ground, if found, sets grounded to true and takes the normal.
		Vector3 castStartPosition = transform.position - (_groundCheckDirection * 0.0f);
		Vector3 castEndPosition = transform.position + (_groundCheckDirection * groundCheckerDistance);

		if (Physics.Linecast(castStartPosition, castEndPosition, out RaycastHit hitGroundTemp, _Groundmask_))
		{
			Vector3 tempNormal = hitGroundTemp.normal;

			if (_isGrounded)
			{
				//castEndPosition = transform.position - transform.up * (0.5f + 1.f);

				//Because terrain can be bumpy, find an average normal between multiple around the same area.
				float[] checksAtRotations = new float[]{60,120,120 }; //Each element is a check, and the value is how much to rotate (relative to player up), before checking.
				Vector3 offSetForCheck = _PlayerVelocity._horizontalSpeedMagnitude > 30 ? _PlayerVelocity._worldVelocity * Time.fixedDeltaTime : _MainSkin.forward * 0.5f; //The offset from the main check that will rotate

				for (int i = 0 ; i < checksAtRotations.Length ; i++)
				{
					//Gets a new position for this check instance, by rotating to the right from the last.
					Quaternion thisRotation = Quaternion.AngleAxis(checksAtRotations[i], transform.up);
					offSetForCheck = thisRotation * offSetForCheck;

					Vector3 thisEndPosition = castEndPosition + offSetForCheck;

					if (Physics.Linecast(castStartPosition, thisEndPosition, out RaycastHit hitSecondTemp, _Groundmask_))
					{
						//If this instance is too much of an outlier, ignore it because it is probably a wall.
						if (Mathf.Abs(tempNormal.normalized.y - hitSecondTemp.normal.y) < 0.75f)
							tempNormal += hitSecondTemp.normal;
					}
				}
				tempNormal = tempNormal.normalized; //Gets the average upwards direction by adding them all together then normalizing.
			}

			//Depending on situation, can allow for greater difference in floor, like if in the air should be easier to find ground as normal to compare is always straight up
			float useGroundDifferentLimit = _groundDifferenceLimit_.x;
			if (!_isGrounded)
				useGroundDifferentLimit = _groundDifferenceLimit_.y;
			else //or should be a higher limit if going uphill, calculated if new normal is pointing away moving direction
			{
				//If the directions without vertical lead to the normal facing away from move direction.
				//useGroundDifferentLimit = Vector3.Angle(lateralDirection, lateralTempNormal) > 85f ? _groundDifferenceLimit_.z : useGroundDifferentLimit;
				if (Vector3.Angle(tempNormal, -_PlayerVelocity._worldVelocity) < Vector3.Angle(_HitGround.normal, -_PlayerVelocity._worldVelocity))
				{
					useGroundDifferentLimit = _groundDifferenceLimit_.z;
				}
			}

			//After getting average normal, check if it's not too different, then set ground.
			if (Vector3.Angle(_groundNormal, tempNormal) < useGroundDifferentLimit)
			{
				//If looknig for ground from the air, ensuring not latching onto ground would be forced off imeddiately. 
				if (_isGrounded || !IsTooSlowOnSlope(tempNormal))
				{
					_HitGround = hitGroundTemp;
					SetIsGrounded(true);
					_groundNormal = tempNormal;
					return;
				}
			}
		}
		SetIsGrounded(false, 0.01f);
	}


	//Calls all the methods involved in managing coreVelocity on the ground, such as normal control (with normal modifiers), sticking to the ground, and effects from slopes.
	private void GroundMovement () {

		_timeOnGround += Time.deltaTime;

		//To avoid jittering against a wall if going to and from 0 to even some speed, only call this if there isn't a wall SUPER close in the input direction.
		if (!(_PlayerVelocity._horizontalSpeedMagnitude < 10 && Physics.SphereCast(transform.position, _CharacterCapsule.radius * 0.99f,
			_Input._constantInputRelevantToCharacter, out RaycastHit hit, 0.02f + (_CharacterCapsule.radius * 0.01f), _Groundmask_)))
		{
			_PlayerVelocity._coreVelocity = _PlayerMovement.HandleControlledVelocity(_PlayerVelocity._coreVelocity, new Vector2(1, 1));
		}

		_PlayerVelocity._coreVelocity = HandleSlopePhysics(_PlayerVelocity._coreVelocity);
		_PlayerVelocity._coreVelocity = StickToGround(_PlayerVelocity._coreVelocity);	}

	//Calls methods relevant to general control and gravity, while applying the turn and accelleration modifiers depending on a number of factors while in the air.
	public Vector3 HandleAirMovement ( Vector3 coreVelocity ) {

		//In order to change horizontal movement in the air, the player must not be inputting into a wall.
		//Because moving into a slanted wall can lead to the player sliding up it while still not being grounded.
		Vector3 spherePosition = _FeetTransform.position + (transform.up * (_CharacterCapsule.radius * 0.45f)) ;
		Vector3 direction = _Input._constantInputRelevantToCharacter;

		if (!Physics.SphereCast(spherePosition, _CharacterCapsule.radius * 0.95f, direction, out RaycastHit hit, 2, _Groundmask_))
		{
			//Gets the air control modifiers.
			float airAccelMod = _airControlAmmount_.y;
			float airTurnMod = _airControlAmmount_.x;
			switch (_Actions._whatCurrentAction)
			{
				case S_Enums.PrimaryPlayerStates.Jump:
					if (_Actions._actionTimeCounter < _jumpExtraControlThreshold_)
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
			if (_PlayerVelocity._horizontalSpeedMagnitude < 20)
			{
				airAccelMod += 0.5f;
			}

			//Handles lateral velocity.
			coreVelocity = _PlayerMovement.HandleControlledVelocity(_PlayerVelocity._coreVelocity, new Vector2(airTurnMod, airAccelMod));
		}
		coreVelocity = CheckGravity(coreVelocity);

		return coreVelocity;
	}

	//A seperate public method so it can be called without HandleAirMovement or needing to call all of its used fields.
	public Vector3 CheckGravity ( Vector3 coreVelocity, bool overwrite = false ) {
		//Apply Gravity (vertical velocity)
		if (_listOfIsGravityOn.Count == 0 || overwrite)
			coreVelocity = ApplyGravityToIncreaseFallSpeed(coreVelocity, _currentFallGravity, _currentUpwardsFallGravity, _maxFallingSpeed_, _PlayerVelocity._worldVelocity);
		return coreVelocity;
	}

	//Handles interactions with slopes (non flat ground), both positive and negative, relative to the player's current rotation.
	//This includes adding force downhill, aiding or hampering running, as well as falling off when too slow.
	public Vector3 HandleSlopePhysics ( Vector3 worldVelocity, bool canFallOff = true ) {
		if (!_isUsingSlopePhysics_) { return worldVelocity; }


		Vector3 slopeVelocity = Vector3.zero;

		if (canFallOff && IsTooSlowOnSlope(_groundNormal))
		{
			//Then fall off and away from the slope.
			_PlayerVelocity.AddGeneralVelocity(Vector3.Lerp(_groundNormal, Vector3.down, 0.2f) * 15f, true, true);
			SetIsGrounded(false, 0.3f); //Wont be able to find ground again for x seconds.

			_keepNormalCounter = _keepNormalForThis_ - 0.03f; //Ensures will immediately start to rotate to ground being down.
			return worldVelocity;
		}

		//Slope power
		//If slope angle is less than limit, meaning on a slope
		if (_groundNormal.y < _slopeEffectLimit_ && _PlayerVelocity._horizontalSpeedMagnitude > 3)
		{
			_isCurrentlyOnSlope = true;

			//Get force to always apply whether up or down hill
			float force =  _curvePosSlopePower;
			force *= _generalHillMultiplier_;
			float steepForce = 0.8f - (Mathf.Abs(_groundNormal.y) / 2) + 1;
			force *= steepForce; //Force affected by steepness of slope. The closer to 0 (completely horizontal), the greater the force, ranging from 1 - 2

			//If moving uphill
			if (worldVelocity.y > _upHillThreshold)
			{
				//Increase time uphill so after force can be more after a while.
				_timeUpHill += Time.fixedDeltaTime;
				force *= _UpHillByTime_.Evaluate(_timeUpHill);

				force *= _uphillMultiplier_; //Affect by unique stat for uphill, and ensure the force is going the other way 
				force = _isRolling ? force * _rollingUphillBoost_ : force; //Add more force if rolling.

			}
			//If moving downhill
			else if (worldVelocity.y < _downHillThreshold_)
			{
				//Decrease timeUpHill.
				float decreaseTimeUpHillBy = Time.fixedDeltaTime * 0.5f; //not as quickly as how it increases so zigzagging down and up won't work.
				decreaseTimeUpHillBy *= 1 + (_PlayerVelocity._totalVelocity.normalized.y); //Decrease more depending on how downwards is moving. If going straight downwards, then this becomes x2, making it equal to any uphill.

				_timeUpHill -= Mathf.Clamp(_timeUpHill - decreaseTimeUpHillBy, 0, _timeUpHill); //Apply, but can't go under 0
				force *= _downhillMultiplier_; //Affect by unique stat for downhill
				force = _isRolling ? force * _rollingDownhillBoost_ : force; //Add more force if rolling.
			}

			//This force is then added to the current velocity. but aimed towards down the slope, leading to a more realistic and natural effect than just changing speed.
			//Vector3 downSlopeForce = AlignWithNormal(new Vector3(_groundNormal.x, 0, _groundNormal.z), _groundNormal, -force);
			// Calculate direction upwards on the wall
			Vector3 right = Vector3.Cross(Vector3.up,_groundNormal).normalized;
			Vector3 upOnWall = Vector3.Cross(_groundNormal, right).normalized;
			Vector3 downSlopeForce = -upOnWall ;
			downSlopeForce = downSlopeForce.normalized * force;

			slopeVelocity += downSlopeForce;


		}
		else { _timeUpHill = 0; }

		return worldVelocity + slopeVelocity;
	}

	private bool IsTooSlowOnSlope ( Vector3 normal ) {
		//If moving too slow compared to the limit
		float speedRequirement = _SlopeSpeedLimitByAngle_.Evaluate(normal.y);
		return (_PlayerVelocity._horizontalSpeedMagnitude < speedRequirement);
	}

	//Handles the player's velocity following the path of the ground. This does not set the rotation to match it, but does prevent them from flying off or colliding with slopes.
	//This also handles stepping up over small ledges.
	public Vector3 StickToGround ( Vector3 velocity ) {

		if (!_canStickToGround) { return velocity; }

		//If moving and has been grounded for long enough. The time on ground is to prevent gravity force before landing being carried over to shoot player forwards on landing.
		//Then ready a raycast to check for slopes.
		if (_timeOnGround > 0.12f && _PlayerVelocity._horizontalSpeedMagnitude > 3)
		{

			Vector3 currentGroundNormal = _groundNormal;
			Vector3 raycastStartPosition = _HitGround.point + (_groundNormal * 0.08f);
			Vector3 rayCastDirection = AlignWithNormal(_PlayerVelocity._worldVelocity.normalized, _groundNormal, 1);

			//If the Raycast Hits something, then there is a wall in front, that could be a negative slope (ground is higher and will tilt backwards to go up).
			//if (Physics.Raycast(raycastStartPosition, rayCastDirection, out RaycastHit hitSticking,
			//	_PlayerVelocity._speedMagnitude * _stickCastAhead_ * Time.fixedDeltaTime, _Groundmask_))
			//Shoots a boxcast rather than a raycast to check for steps slightly to the side as well as directly infront.
			if (Physics.BoxCast(raycastStartPosition, new Vector3(0.12f, 0.05f, 0.01f), rayCastDirection, out RaycastHit hitSticking,
					Quaternion.LookRotation(rayCastDirection, currentGroundNormal),
					_PlayerVelocity._horizontalSpeedMagnitude * _stickCastAhead_ * Time.fixedDeltaTime, _Groundmask_))
			{
				float upwardsDirectionDifference =  Vector3.Angle(currentGroundNormal, hitSticking.normal);

				//If the angle difference between current slope and encountered one is under the limit	
				if (upwardsDirectionDifference < 70f)
					velocity = AlignToUpwardsSlope(currentGroundNormal, hitSticking, velocity);
				//If the difference is too large, then it's not a slope, and is likely facing towards the player, so see if it's a step to step over/onto.
				else
					StepOver(raycastStartPosition, rayCastDirection, currentGroundNormal, hitSticking);

			}
			// If there is no wall, then we may be dealing with a positive slope (like the outside of a loop, where the ground is relatively lower).
			else
			{
				velocity = AlignToDownwardsOrCurrentSlope(raycastStartPosition, rayCastDirection, currentGroundNormal, velocity);
			}
		}
		//Even if not adjusting velocity to ground, still try to stick on it. This will avoid liting off the ground when slowing down to a stop.
		else if (_isGrounded)
		{
			//Gives a small chance to convert fall speed to run speed based on slopes.
			if (_timeOnGround > 0.08)
			{
				//Since stationary, remove any relative upwards force in core that might push the player off the ground.
				velocity = GetRelevantVector(velocity);
				velocity.y = 0;
				velocity = transform.TransformDirection(velocity);
			}
			_PlayerVelocity.AddGeneralVelocity(-_groundNormal * _forceTowardsGround_ * 1.2f, false, false);
		}
		return velocity;
	}

	private Vector3 AlignToUpwardsSlope ( Vector3 currentGroundNormal, RaycastHit hitSticking, Vector3 velocity ) {

		//Then it creates a velocity aligned to that new normal, then interpolates from the current to this new one.	
		currentGroundNormal = hitSticking.normal.normalized;
		Vector3 Dir = AlignWithNormal(velocity, currentGroundNormal, velocity.magnitude);
		velocity = Vector3.LerpUnclamped(velocity, Dir, _stickingLerps_.x);

		//If player is too far from ground, set back to buffer position.
		//if (_placeAboveGroundBuffer_ > 0 && (_FeetTransform.position - _HitGround.point).sqrMagnitude > _placeAboveGroundBuffer_ * _placeAboveGroundBuffer_)
		if (_placeAboveGroundBuffer_ > 0)
		{
			Vector3 directionFromGroundToPlayer = transform.position - _HitGround.point;
			Vector3 newPos = _HitGround.point  -(_groundCheckDirection * _placeAboveGroundBuffer_) - _feetOffsetFromCentre;
			SetPlayerPosition(newPos);
		}

		return velocity;
	}

	private Vector3 AlignToDownwardsOrCurrentSlope ( Vector3 raycastStartPosition, Vector3 rayCastDirection, Vector3 currentGroundNormal, Vector3 velocity ) {
		float lerpAmount = _stickingLerps_.x;

		//Shoots a raycast under the ground where the player would be next frame, meaning it will only be true if the ground next frame will be lower than this frame.
		raycastStartPosition = _HitGround.point - (_groundNormal * 0.02f);
		raycastStartPosition += (rayCastDirection * (_PlayerVelocity._horizontalSpeedMagnitude * Time.deltaTime));
		if (Physics.Raycast(raycastStartPosition, -_groundNormal, out RaycastHit hitSticking, 1f, _Groundmask_))
		{
			//Check if this ground isn't too different to warrent lerping to it.
			float upwardsDirectionDifference = Vector3.Angle(transform.up, hitSticking.normal);
			if (upwardsDirectionDifference < _stickingNormalLimit_)
			{
				currentGroundNormal = hitSticking.normal;
				lerpAmount = _stickingLerps_.y;
			}
		}
		Vector3 Dir = AlignWithNormal(velocity, currentGroundNormal, velocity.magnitude);
		velocity = Vector3.LerpUnclamped(velocity, Dir, lerpAmount);

		// Adds velocity downwards to remain on the slope. This is general so it won't be involved in the next coreVelocity calculations, which needs to be relevant to the ground surface.
		_PlayerVelocity.AddGeneralVelocity(-currentGroundNormal * _forceTowardsGround_, false, false);

		return velocity;
	}


	//Handles stepping up onto slightly raised surfaces without losing momentum, rather than bouncing off them. Requires multiple checks into the situation to avoid clipping or unnecesary stepping.
	private void StepOver ( Vector3 raycastStartPosition, Vector3 rayCastDirection, Vector3 newGroundNormal, RaycastHit hitSticking ) {

		//Find a point above and slightly continuing on from the impact point.
		Vector3 rayStartPosition = hitSticking.point + (rayCastDirection * 0.15f) + (newGroundNormal * 1.25f) + (newGroundNormal * _CharacterCapsule.radius);
		//If enough space to be placed there or slightly below.
		if (!Physics.CheckSphere(rayStartPosition, _CharacterCapsule.radius))
		{
			// then shoot a sphere down for a lip to see the walls height
			if (Physics.SphereCast(rayStartPosition, _CharacterCapsule.radius, -newGroundNormal, out RaycastHit hitLip, 1.2f, _Groundmask_))
			{

				//if the lip is within step height and a similar angle to the current one, then it is a step
				float stepHeight = 1.5f - (hitLip.distance);
				float floorToStepDot = Vector3.Dot(hitLip.normal, newGroundNormal);

				if (stepHeight < _stepHeight_ && stepHeight > 0.05f && _PlayerVelocity._horizontalSpeedMagnitude > 5f && floorToStepDot > 0.93f)
				{
					//Gets a position to place the player ontop of the step, then performs a box cast to check if there is enough empty space for the player to fit. 
					//Then move them to that position.
					Vector3 castPositionAtHit = rayStartPosition - (newGroundNormal * hitLip.distance);
					Vector3 newPosition = castPositionAtHit - (_groundNormal * _FeetTransform.localPosition.y);
					newPosition = castPositionAtHit - _feetOffsetFromCentre;
					Vector3 boxSizeOfPlayerCollider = new Vector3 (_CharacterCapsule.radius, _CharacterCapsule.height + (_CharacterCapsule.radius * 2), _CharacterCapsule.radius);

					if (!Physics.BoxCast(newPosition, boxSizeOfPlayerCollider, rayCastDirection, Quaternion.LookRotation(rayCastDirection, transform.up), 0.4f))
					{
						SetPlayerPosition(newPosition);
						return;
					}
				}
			}
		}
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//Returns appropriate downwards force when in the air. This is public and static so it can be called in simulations that want to represent these calculations.
	public static Vector3 ApplyGravityToIncreaseFallSpeed ( Vector3 coreVelocity, Vector3 normalGravity, Vector3 upGravity, float maxFall, Vector3 totalVelocity ) {

		float vertSpeed = coreVelocity.y;
		//If falling down, return normal fall gravity.
		if (vertSpeed <= 0)
		{
			coreVelocity += normalGravity;
		}
		//If currently moving up while in the air, apply a different (typically higher) gravity force with a slight increase dependant on upwards speed.
		else
		{
			float applyMod = Mathf.Clamp(1 + ((vertSpeed / 10) * 0.1f), 1, 3);
			Vector3 newGrav = new Vector3( upGravity.x,  upGravity.y * applyMod,  upGravity.z);
			coreVelocity += newGrav;
		}

		//If the core velocity combined with environmental velocity has not gone beyond max fall speed, apply the gravity downwards. Doing this rather than just checking against core velocity downwards speed means the physics velocity can always reach this speed, even if environmental is pushing up.
		if (totalVelocity.y > maxFall)
		{
			return coreVelocity;
		}
		//If the gravity would push downwards speed below maxFall (remember that max fall is negative), then don't change it.
		else
		{
			//Clamp to max falling speed, so can't fall faster than this.
			return new Vector3(coreVelocity.x, Mathf.Clamp(coreVelocity.y, vertSpeed, coreVelocity.y), coreVelocity.z);
		}
	}


	//Makes a world space vector relative to a normal. Such as a forward direction being affected by the ground.
	public Vector3 AlignWithNormal ( Vector3 vector, Vector3 normal, float magnitude = 0 ) {

		Vector3 tangent = Vector3.Cross(normal.normalized, vector);
		Vector3 newVector = -Vector3.Cross(normal.normalized, tangent);

		//Using this cross method leads to magnitude changes, so must overide the size.
		newVector = newVector.normalized * magnitude;
		return newVector;
	}

	//Changes the character's rotation to match the current situation. This includes when on a slope, when in the air, or transitioning between the two.
	public void AlignToGround ( Vector3 normal, bool isGrounded ) {

		//If on ground, then rotates to match the normal of the floor.
		if (isGrounded)
		{
			//Change rotation if
			//- this new normal is not the same as the normal two frames ago (so there won't be flickering between two different normals at all times).
			//- this normal is the same as 3 frames ago (to prevent character not rotating to goal unless normal changes constantly.)
			if (_listOfPreviousGroundNormals[1] != normal || _listOfPreviousGroundNormals[2] == normal)
			{
				_keepNormal = normal;
				Quaternion targetRotation = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;
				SetPlayerRotation(Quaternion.Lerp(transform.rotation, targetRotation, 0.8f));
			}
		}
		//If in the air, then stick to previous normal for a moment before rotating legs towards gravity. This avoids collision issues
		else
		{
			Vector3 localRight = _MainSkin.right;

			if (_keepNormalCounter < _keepNormalForThis_)
			{
				_keepNormalCounter += Time.deltaTime;
				SetPlayerRotation(Quaternion.FromToRotation(transform.up, _keepNormal) * transform.rotation);

				//Upon counter ending, prepare to rotate to ground.
				if (_keepNormalCounter >= _keepNormalForThis_)
				{
					_amountToRotate = Vector3.Angle(Vector3.up, _keepNormal);

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
			else if (transform.up.y != 1)
			{
				//If upside down, then the player must rotate sideways, and not forwards. This keeps them facing the same way while pointing down to the ground again.
				if (_keepNormal.y < _rotationResetThreshold_)
				{

					//If the player was set to rotating right, and their right side is still higher than their left,
					//then they have not flipped over all the way yet. Same with rotating left and lower left. 
					if ((!_isRotatingLeft && localRight.y >= 0) || (_isRotatingLeft && localRight.y < 0))
					{
						//Then get a cross product from preset rotating angle and transform up, then rotate in that direction.
						Vector3 cross = Vector3.Cross(_rotateSidewaysTowards, transform.up);

						Quaternion targetRot = Quaternion.FromToRotation(transform.up, cross) * transform.rotation;
						SetPlayerRotation(Quaternion.RotateTowards(transform.rotation, targetRot, 300f * Time.deltaTime));
					}
					//When flipped over, set y value of the right side to zero to ensure not tilted anymore, and ready main rotation to sort any remaining rotation differences.
					else
					{
						transform.right = new Vector3(transform.right.x, 0, transform.right.z).normalized;
						_keepNormal = Vector3.up;
					}
				}
				//
				//General rotation to face up again.
				//
				else
				{
					Quaternion targetRot = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
					SetPlayerRotation(Quaternion.RotateTowards(transform.rotation, targetRot, 180f * Time.deltaTime));
					//If the rotation amoung is less than the difference, then the rotation will complete to face upwards.
					if (Quaternion.Angle(targetRot, transform.rotation) < 180 * Time.deltaTime)
					{
						if (_isUpsideDown)
						{
							_listOfCanTurns.Remove(false);
						}
						else if (_amountToRotate > 60)
							StartCoroutine(_CamHandler._HedgeCam.KeepGoingToHeightForFrames(30, 50, 60));

						_amountToRotate = 0;
						_isUpsideDown = false;
					}
				}
			}
		}

		_feetOffsetFromCentre = _FeetTransform.position - transform.position;

		_listOfPreviousGroundNormals.Insert(0, normal);
		_listOfPreviousGroundNormals.RemoveAt(3);
	}

	//Called anywhere to get what the input velocity is in the player's local space.
	public Vector3 GetRelevantVector ( Vector3 vel, bool includeY = true ) {
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
			_isGrounded = value;

			//If changed to be in the air when was on the ground
			if (!_isGrounded)
			{
				_groundNormal = Vector3.up;
				_wasInAirLastFrame = true;
				_groundingDelay = timer;
				_timeOnGround = 0;
				_Events._OnLoseGround.Invoke();
			}
			//If changed to be on the ground when was in the air
			else if (_isGrounded)
			{
				_timeOnGround = 0;
				_timeUpHill = 0;

				//If hasn't completed aligning to face upwards when was upside down, then end that prematurely and retern turning.
				if (_isUpsideDown)
				{
					_isUpsideDown = false;
					_listOfCanTurns.Remove(false);
				}
				_keepNormalCounter = 0;

				_Events._OnGrounded.Invoke(); // Any methods attatched to the Unity event in editor will be called. These should all be called "EventOnGrounded".
			}
		}
	}



	public void SetPlayerPosition ( Vector3 newPosition, bool shouldPrintLocation = false ) {
		Debug.DrawLine(transform.position, newPosition, Color.magenta, 10f);

		transform.position = newPosition;
		if (shouldPrintLocation) Debug.Log("Change Position to  ");
	}
	public void AddToPlayerPosition ( Vector3 Add ) {
		transform.Translate(Add);
	}

	public void SetPlayerRotation ( Quaternion newRotation, bool immediately = false, bool shouldPrintRotation = false ) {
		if(immediately)
			transform.rotation = newRotation;
		//Using rigidBody is smoother but wont take effect this frame, so if you need to rotate for specific calculations, change the transform.
		else
			_RB.MoveRotation(newRotation);

		if (shouldPrintRotation) Debug.Log("Change Position to  " + newRotation);
	}

	//Called at any point when one wants to lock one of the basic functions like turning or controlling for a set ammount of time. Must input the function first though.
	public IEnumerator LockFunctionForTime ( EnumControlLimitations whatToLimit, float seconds, int frames = 0 ) {
		//Add lock to a list based on enum input
		switch (whatToLimit)
		{
			case EnumControlLimitations.canControl:
				_listOfCanControl.Add(false);
				break;
			case EnumControlLimitations.canTurn:
				_listOfCanTurns.Add(false);
				break;
			case EnumControlLimitations.canDecelerate:
				_listOfCanDecelerates.Add(false);
				break;
		}

		//Add a delay, either based on real time or by number of frames(55 = 1 second ideally)
		if (seconds > 0)
			yield return new WaitForSeconds(seconds);
		else
			for (int i = 0 ; i < frames ; i++) { yield return new WaitForFixedUpdate(); }

		//Remove lock from the list
		switch (whatToLimit)
		{
			case EnumControlLimitations.canControl:
				_listOfCanControl.RemoveAt(0);
				break;
			case EnumControlLimitations.canTurn:
				_listOfCanTurns.RemoveAt(0);
				break;
			case EnumControlLimitations.canDecelerate:
				_listOfCanDecelerates.RemoveAt(0);
				break;
		}
	}


	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	//Matches all changeable stats to how they are set in the character stats script.
	private void AssignStats () {
		_isUsingSlopePhysics_ = _Tools.Stats.SlopeStats.isUsingSlopePhysics;
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

		_rollingDownhillBoost_ = _Tools.Stats.RollingStats.rollingDownhillBoost;
		_rollingUphillBoost_ = _Tools.Stats.RollingStats.rollingUphillBoost;
		_UpHillByTime_ = _Tools.Stats.SlopeStats.UpHillEffectByTime;
		_startFallGravity_ = _Tools.Stats.WhenInAir.fallGravity;
		_gravityWhenMovingUp_ = _Tools.Stats.WhenInAir.upGravity;
		_keepNormalForThis_ = _Tools.Stats.WhenInAir.keepNormalForThis;

		_forceTowardsGround_ = _Tools.Stats.GreedysStickToGround.forceTowardsGround;
		_stickingLerps_ = _Tools.Stats.GreedysStickToGround.stickingLerps;
		_stickingNormalLimit_ = _Tools.Stats.GreedysStickToGround.stickingNormalLimit;
		_stickCastAhead_ = _Tools.Stats.GreedysStickToGround.stickCastAhead;
		_placeAboveGroundBuffer_ = _Tools.Stats.GreedysStickToGround.groundBuffer;
		_rayToGroundDistance_ = _Tools.Stats.FindingGround.rayToGroundDistance;
		_raytoGroundSpeedRatio_ = _Tools.Stats.FindingGround.raytoGroundSpeedRatio;
		_raytoGroundSpeedMax_ = _Tools.Stats.FindingGround.raytoGroundSpeedMax;
		_rotationResetThreshold_ = _Tools.Stats.GreedysStickToGround.rotationResetThreshold;
		_Groundmask_ = _Tools.Stats.FindingGround.GroundMask;
		_upwardsLimitByCurrentSlope_ = _Tools.Stats.GreedysStickToGround.upwardsLimitByCurrentSlope;
		_stepHeight_ = _Tools.Stats.GreedysStickToGround.stepHeight;
		_groundDifferenceLimit_ = _Tools.Stats.FindingGround.groundAngleDifferenceLimit;

		//Sets all changeable core values to how they are set to start in the editor.
		_currentFallGravity = _startFallGravity_;
		_currentUpwardsFallGravity = _gravityWhenMovingUp_;

		_keepNormal = Vector3.up;


	}

	private void AssignTools () {
		s_MasterPlayer = this;
		_RB = GetComponent<Rigidbody>();
		_Actions = _Tools._ActionManager;
		_PlayerMovement = GetComponent<S_PlayerMovement>();
		_PlayerVelocity = GetComponent<S_PlayerVelocity>();
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_Events = _Tools.PlayerEvents;
		_CamHandler = _Tools.CamHandler;

		_CharacterCapsule = _Tools.CharacterCapsule.GetComponent<CapsuleCollider>();
		_FeetTransform = _Tools.FeetPoint;
		_MainSkin = _Tools.MainSkin;
	}
	#endregion
}
