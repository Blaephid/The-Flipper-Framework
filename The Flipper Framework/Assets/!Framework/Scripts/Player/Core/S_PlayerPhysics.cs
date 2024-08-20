using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Assertions.Must;

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
	private S_ActionManager       _Actions;
	private S_CharacterTools      _Tools;
	static public S_PlayerPhysics s_MasterPlayer;
	private S_PlayerInput         _Input;
	private S_PlayerEvents        _Events;

	[HideInInspector]
	public Rigidbody              _RB;
	private CapsuleCollider       _CharacterCapsule;
	private Transform             _FeetTransform;
	private Transform             _MainSkin;
	#endregion

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
	private bool                  _shouldStopAirMovementIfNoInput_ = false;
	private float                 _keepNormalForThis_ = 0.083f;
	private float                 _maxFallingSpeed_;
	[HideInInspector]
	public Vector3               _gravityWhenMovingUp_;
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
	private float                 _forceTowardsGround_;
	private Vector2               _stickingLerps_ = new Vector2(0.885f, 1.5f);
	private float                 _stickingNormalLimit_ = 0.4f;
	private float                 _stickCastAhead_ = 1.9f;
	private AnimationCurve        _upwardsLimitByCurrentSlope_;
	[HideInInspector]
	public float                  _placeAboveGroundBuffer_ = 0.6115f;
	private Vector2                _rayToGroundDistance_ ;
	private float                 _raytoGroundSpeedRatio_ = 0.01f;
	private float                 _raytoGroundSpeedMax_ = 2.4f;
	private float                 _rotationResetThreshold_ = -0.1f;
	private float                 _stepHeight_ = 0.6f;
	private Vector3                 _groundDifferenceLimit_;
	private float                 _landingConversionFactor_ = 2;

	[HideInInspector]
	public LayerMask              _Groundmask_;


	#endregion
	// Trackers
	#region trackers

	private bool        _isPositiveUpdate;  //Alternates between on and off every update, so can be used universally for anything that should only happen every other frame.
	[HideInInspector]
	public int         _frameCount;         //Used for Debugging, can be set to increase here every frame, and referenced in other scripts.

	//Methods
	public delegate Vector3 DelegateAccelerationAndTurning ( Vector3 vector, Vector3 input, Vector2 modifier );        //A delegate for deciding methods to calculate acceleration and turning 
	public DelegateAccelerationAndTurning   CallAccelerationAndTurning; //This delegate will be called in controlled velocity to return changes to acceleration and turning. This will usually be the base one in this script, but may be changed externally depending on the action.

	[HideInInspector]
	public bool                   _arePhysicsOn = true;         //If false, no changes to velocity will be calculated or applied. This script will be inactive.

	[HideInInspector]
	public Vector3                _coreVelocity;                //Core velocity is the velocity under the player's control. Whether it be through movement, actions or more. It cannot exceed maximum speed. Most calculations are based on this
	[HideInInspector]
	public Vector3                _environmentalVelocity;       //Environmental velocity is the velocity applied by external forces, such as springs, fans and more.
	[HideInInspector]
	public Vector3                _totalVelocity;               //The combination of core and environmetal velocity determening actual movement direction and speed in game.
	[HideInInspector]
	public Vector3                _worldVelocity;               //This is set at the start of a frame as a temporary total velocity, based on the actual velocity in physics. So Total Velocity is set, then affected by collision after the FixedUpdate, then adjusted by TrackAndChangeVelocity, then set here.
	[HideInInspector]
	public List<Vector3>          _previousVelocities = new List<Vector3>() {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };           //The total velocity at the end of the previous TWO frames, compared to Unity physics at the start of a frame to see if anything major like collision has changed movement.

	private List<Vector3>         _listOfVelocityToAddThisUpdate = new List<Vector3>(); //Rather than applied across all scripts, added forces are stored here and applied at the end of the frame.
	private List<Vector3>         _listOfCoreVelocityToAdd= new List<Vector3>();
	private Vector3               _externalCoreVelocity;        //Replaces core velocity this frame instead of just add to it.
	private float                 _externalRunningSpeed;                  //Replaces core velocity magnitude this frame, but keeps direction and applied forces.
	private bool                  _isOverwritingCoreVelocity;   //Set to true if core velocity should be completely replaced, including any aditions that would be made. If false, added forces will still be applied.

	private Vector3                 _velocityToNotCountWhenCheckingForAChange; //Will be set to invert ground sticking force. So if the above is applied while stationary, this will be set to the opposite and used during comparisons between frames.


	//Environmental velocity can be reset based on a variety of factors. These will be set when environmental velocity is set, and then set environmental when checked and true.
	[HideInInspector]
	public bool                  _resetEnvironmentalOnGrounded;
	[HideInInspector]
	public bool                  _resetEnvironmentalOnAirAction;

	[HideInInspector]
	public float                  _speedMagnitude;              //The speed of the player at the end of the frame.
	[HideInInspector]
	public float                  _horizontalSpeedMagnitude;    //The speed of the player relative to the character transform, so only shows running speed.
	[HideInInspector]
	public float                  _currentRunningSpeed;         //Similar to horizontalSpedMagnitde, but only core velocity, therefore the actual running velocity applied through this script.
	[HideInInspector]
	public List<float>            _previousHorizontalSpeeds = new List<float>() {1f, 2f, 3f, 4 }; //The horizontal speeds across the last few frames. Useful for collision checks.
	[HideInInspector]
	public List<float>            _previousRunningSpeeds = new List<float>() {1f, 2f, 3f}; //The horizontal speeds across the last few frames. Useful for checking if core speed has been changed in external scripts.

	[HideInInspector]
	public Vector3                _moveInput;         //Assigned by the input script, the direction the player is trying to go.
	[HideInInspector]
	public Vector3                _trackMoveInput;    //Follows the input direction, and it is has changed but the controller input hasn't, that means the camera was moved to change direction.

	[HideInInspector]
	public float                  _currentTopSpeed;   //Player cannot exceed this speed by just running on flat ground. May be changed across gameplay.
	[HideInInspector]
	public float                  _currentMaxSpeed;   //Player's core velocity can not exceed this by any means.
	[HideInInspector]
	public float                  _currentMinSpeed;   //Player's running velocity can not go below this. Should be 0, and only temporarily set for certain actions.

	//Updated each frame to get current place on animation curves relevant to movement.
	private float                 _currentRunAccell;
	private float                 _currentRollAccell;
	private float                  _curvePosAcell;
	private float                 _curvePosDecell;
	[HideInInspector]
	public float                  _curvePosDrag;
	[HideInInspector]
	public float                  _curvePosSlopePower;


	[HideInInspector]
	public float                  _inputVelocityDifference = 1; //Referenced by other scripts to get the angle between player input and movement direction
	[HideInInspector]
	public Vector3                _playerPos;         //A quick reference to the players current location
	[HideInInspector]
	public Vector3                _feetOffsetFromCentre;

	private float                 _timeUpHill;        //Tracks how long a player has been running up hill. Decreases when going down hill or on flat ground.

	//Ground tracking
	[HideInInspector]
	public bool                   _isGrounded = false;        //Used to check if the player is currently grounded. _isGrounded
	private Vector3               _groundCheckDirection;	//Set as transform.down at the start of the update, but because the player rotation is changed, this is saved to be used when deciding where to set towards the ground.
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
	private bool                  _isUpsideDown;                //Rotating to face up when upside down has a unique approach.
	private bool                  _isRotatingLeft;              //Which way to rotate around to face up from upside down.
	private Vector3               _rotateSidewaysTowards;       //The angle of rotation to follow when rotating from upside down
	private float                 _keepNormalCounter;           //Tracks how before rotating can begin

	//In air
	private bool _wasInAirBeforeSlope;
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

		//Set delegates
		CallAccelerationAndTurning = DefaultAccelerateAndTurn; //Whenever this delegate is called, it will call the default acceleration and turning present in this script, but the delegate may be changed by actions.
	}

	//On FixedUpdate,  call HandleGeneralPhysics if relevant.
	void FixedUpdate () {
		_isCurrentlyOnSlope = false; //Set to false at the end of a frame but will be set to true if slope physics are called next frame.

		HandleGeneralPhysics();

		_isPositiveUpdate = !_isPositiveUpdate; //Alternates at the end of an update, so will be the oppositve value enxt call.
	}

	//Sets public variables relevant to other calculations 
	void LateUpdate () {
		_playerPos = transform.position;

		_frameCount++;
	}

	///----Collision Trackers
	private void OnTriggerEnter ( Collider other ) {
		_Events._OnTriggerEnter.Invoke(other);
	}
	private void OnTriggerExit ( Collider other ) {
		_Events._OnTriggerExit.Invoke(other);
	}

	private void OnCollisionEnter ( Collision collision ) {
		_Events._OnCollisionEnter.Invoke(collision);
	}

	private void OnTriggerStay ( Collider other ) {
		_Events._OnTriggerStay.Invoke(other);
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Manages the character's physics, calling the relevant functions.
	public void HandleGeneralPhysics () {

		//Get curve positions, which will be used in calculations for this frame.
		_curvePosAcell = _AccelBySpeed_.Evaluate(_currentRunningSpeed / _currentTopSpeed);
		_curvePosDecell = _DecelBySpeed_.Evaluate(_currentRunningSpeed / _currentMaxSpeed);
		_curvePosDrag = _DragBySpeed_.Evaluate(_currentRunningSpeed / _currentMaxSpeed);
		_curvePosSlopePower = _slopePowerBySpeed_.Evaluate(_currentRunningSpeed / _currentMaxSpeed);

		if (!_arePhysicsOn) { return; }

		//Set if the player is grounded based on current situation.
		CheckForGround();

		AlignToGround(_groundNormal, _isGrounded);

		//Handle any changes to the velocity between updates.
		CheckAndApplyVelocityChanges();

		//Calls the appropriate movement handler.
		if (_isGrounded)
			GroundMovement();
		else
			_coreVelocity = HandleAirMovement(_coreVelocity);

		//After all other calculations are made across the scripts, the new velocities are applied to the rigidbody.
		SetTotalVelocity();
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
			groundCheckerDistance = _rayToGroundDistance_.x + (_horizontalSpeedMagnitude * _raytoGroundSpeedRatio_);
			groundCheckerDistance = Mathf.Min(groundCheckerDistance, _raytoGroundSpeedMax_);
		}

		_groundCheckDirection = -transform.up;

		//Uses the ray to check for ground, if found, sets grounded to true and takes the normal.
		Vector3 castStartPosition = transform.position - (_groundCheckDirection * 0.0f);
		Vector3 castEndPosition = transform.position + (_groundCheckDirection * groundCheckerDistance);

		Debug.DrawLine(castStartPosition, castEndPosition, Color.black, 10f);

		if (Physics.Linecast(castStartPosition, castEndPosition, out RaycastHit hitGroundTemp, _Groundmask_))
		{
			Vector3 tempNormal = hitGroundTemp.normal;

			if (_isGrounded)
			{
				//castEndPosition = transform.position - transform.up * (0.5f + 1.f);

				//Because terrain can be bumpy, find an average normal between multiple around the same area.
				float[] checksAtRotations = new float[]{0,70,110,110 }; //Each element is a check, and the value is how much to rotate (relative to player up), before checking.
				Vector3 offSetForCheck = _horizontalSpeedMagnitude > 30 ? _worldVelocity * Time.fixedDeltaTime : _MainSkin.forward * 0.5f; //The offset from the main check that will rotate

				for (int i = 0 ; i < checksAtRotations.Length ; i++)
				{
					//Gets a new position for this check instance, by rotating to the right from the last.
					Quaternion thisRotation = Quaternion.AngleAxis(checksAtRotations[i], transform.up);
					offSetForCheck = thisRotation * offSetForCheck;

					Vector3 thisEndPosition = castEndPosition + offSetForCheck;

					Debug.DrawLine(castStartPosition, thisEndPosition, Color.white, 10f);
					if (Physics.Linecast(castStartPosition, thisEndPosition, out RaycastHit hitSecondTemp, _Groundmask_))
					{
						//If this instance is too much of an outlier, ignore it because it is probably a wall.
						if (Vector3.Angle(tempNormal.normalized, hitSecondTemp.normal) < 75)
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
				Vector3 lateralDirection = new Vector3(_worldVelocity.x, 0 , _worldVelocity.z);
				Vector3 lateralTempNormal = new Vector3(tempNormal.x, 0, tempNormal.z);
				//If the directions without vertical lead to the normal facing away from move direction.
				useGroundDifferentLimit = Vector3.Angle(lateralDirection, lateralTempNormal) > 85f ? _groundDifferenceLimit_.z : useGroundDifferentLimit;
			}

			Debug.Log(tempNormal);
			Debug.DrawRay(hitGroundTemp.point, tempNormal, Color.green, 10f);
			Debug.Log(Vector3.Angle(_groundNormal, tempNormal));

			//After getting average normal, check if it's not too different, then set ground.
			if (Vector3.Angle(_groundNormal, tempNormal) < useGroundDifferentLimit)
			{
				_HitGround = hitGroundTemp;
				SetIsGrounded(true);
				_groundNormal = tempNormal;
				return;
			}
			else
			{
				Debug.Log("OUT OF ANGLE BECAUSE  "  +useGroundDifferentLimit);
				Debug.DrawRay(_HitGround.point, -tempNormal * 2f, Color.red, 10f);
			}
		}
		//If return is not called yet, then sets grounded to false.
		_groundNormal = Vector3.up;
		SetIsGrounded(false, 0.01f);
	}

	//If the rigidbody velocity is smaller than it was last frame (such as from hitting a wall),
	//Then apply the difference to the _corevelocity as well so it knows there's been a change and can make calculations based on it.
	private void CheckAndApplyVelocityChanges () {

		Vector3 velocityThisFrame = _RB.velocity;
		Vector3 velocityLastFrame = _previousVelocities[0];

		bool fromAirToGround = _isGrounded && _wasInAirBeforeSlope;

		if (fromAirToGround)
		{
			//If the last time environmental velocity was set, it was set to reset here, then remove environmnetal velocity.
			if (_resetEnvironmentalOnGrounded)
			{
				//Must remove from the velocity change calculations otherwise coreVelocity wont be updated accurately.
				velocityThisFrame -= _environmentalVelocity;
				velocityLastFrame -= _environmentalVelocity;
				SetEnvironmentalVelocity(Vector3.zero, false, false, S_Enums.ChangeLockState.Unlock);
			}
		}

		//General velocities applied just for last frame (like an anti offset set when groundsticking) are removed later on in this script so should not be factored in here.
		if (_velocityToNotCountWhenCheckingForAChange != Vector3.zero)
		{
			velocityLastFrame -= _velocityToNotCountWhenCheckingForAChange;
			velocityThisFrame -= _velocityToNotCountWhenCheckingForAChange;
		}

		//The magnitudes of the old and current total velocities
		float speedThisFrame = velocityThisFrame.sqrMagnitude;
		float speedLastFrame = velocityLastFrame.sqrMagnitude;

		Debug.DrawRay(transform.position, velocityLastFrame.normalized, Color.cyan, 10f);
		Debug.DrawRay(transform.position, velocityThisFrame.normalized, Color.cyan, 10f);

		//Only apply the changes if physics decreased the speed.
		if (speedThisFrame < speedLastFrame)
		{
			speedThisFrame = velocityThisFrame.magnitude;
			speedLastFrame = velocityLastFrame.magnitude;

			float angleChange = Vector3.Angle(velocityThisFrame, velocityLastFrame);
			if (speedThisFrame < 0.01f) { angleChange = 0; } //Because angle would still be calculated even if a one vector is zero.

			//float speedDifference = Mathf.Abs(currentSpeed - previousSpeed);
			float speedDifference = Mathf.Max(speedLastFrame - speedThisFrame, 0);

			//----Undoing Changes----

			//Converting speed from landing onto running downhill
			if (fromAirToGround)
			{
				Vector4 newVelocityAndSpeed = LandOnSlope(velocityThisFrame, velocityLastFrame, speedThisFrame, speedLastFrame);
				velocityThisFrame = newVelocityAndSpeed;
				speedThisFrame = newVelocityAndSpeed.w;
			}

			// If already moving and the change is just making the player bounce off upwards slightly, then ignore velocity change
			else if (angleChange > 1 && angleChange < 15  //If a slight angle change
				&& Vector3.Angle(velocityThisFrame, transform.up) - 5 < Vector3.Angle(velocityLastFrame, transform.up) //If new velocity is taking the player noticeably more upwards
				&& speedDifference < Mathf.Min(5f, speedLastFrame * 0.1f)) //If not too much speed was lost
			{
				//While this undoes changes, if running into a wall and the velocity keeps resetting, then the player would slide up the wall slowly.
				velocityThisFrame = velocityLastFrame;
				speedThisFrame = speedLastFrame;
			}

			//If the difference in speed is minor(such as lightly colliding with a slope when going up), then ignore the change.
			else if (speedDifference < Mathf.Max(15, speedLastFrame * 0.3f) && speedDifference > 0.01f)
			{
				//These sudden changes will almost always be caused by collision, but running into a wall at an angle redirects the player, while running into a floor or ceiling should be ignored.
				//If only having horizontal velocity changed, don't change direction by increase speed slightly to what it was before for smoothness.
				if (Mathf.Abs(velocityThisFrame.normalized.y - velocityLastFrame.normalized.y) < 0.2f)
				{
					speedThisFrame = Mathf.Lerp(speedThisFrame, speedLastFrame, 0.1f);
					velocityThisFrame = velocityThisFrame.normalized * speedThisFrame;
				}
				//If changing vertically, this will either be an issue with bumping into the ground while running, or landing and converting fall speed to run speed.
				else if (_isGrounded)
				{
					velocityThisFrame = velocityThisFrame.normalized * speedLastFrame;
					//currentVelocity = previousVelocity;
					speedThisFrame = speedLastFrame;
				}
			}

			//----Confirming Changes-----

			//Apply to local velocities what happened to the physics one
			Vector3 vectorDifference =  velocityThisFrame - velocityLastFrame;

			//Since collisions will very rarely put velocity to 0 exactly, add some wiggle room to end player movement if currentVelocity has not been reverted. This will only trigger if player was already moving.
			if (speedThisFrame < 1f && speedLastFrame > speedThisFrame + 0.05)
			{
				_coreVelocity = Vector3.zero;
			}
			//Set to zero if the loss was subsantial and not almost entirely just in vertical difference (to avoid losing lateral speed when landing and losing vertical speed)
			else if (speedThisFrame < speedLastFrame * 0.15f && (speedThisFrame + Mathf.Abs(velocityLastFrame.y)) < speedLastFrame)
			{
				_coreVelocity = Vector3.zero;
			}
			//To ensure core Velocity isn't inverted or even increased if it loses more than itself.
			else if (Mathf.Pow(speedDifference, 2) > _coreVelocity.sqrMagnitude + 0.1f)
			{
				_coreVelocity = Vector3.zero;
			}
			//Otherwise, apply changes so coreVelocity is aware.
			else
			{
				_coreVelocity += vectorDifference;
			}

			//If environmental velocity is in use, decrease as well to track the collisions.
			if (_environmentalVelocity.sqrMagnitude > 3)
			{
				if (_environmentalVelocity.sqrMagnitude > vectorDifference.sqrMagnitude)
				{
					_environmentalVelocity += vectorDifference;
				}
				else
				{
					_environmentalVelocity = Vector3.zero;
				}
			}
		}
		//World velocity is the actual rigidbody velocity found at the start of the frame, edited here if needed, with the removed velocity reapplied.
		_worldVelocity = velocityThisFrame + _velocityToNotCountWhenCheckingForAChange;

		_velocityToNotCountWhenCheckingForAChange = Vector3.zero; //So this can be increased over this update, then checked again at the start of this method.
	}

	//After every other calculation has been made, all of the new velocities and combined and set to the rigidbody.
	//This includes the core and environmental velocities, but also the others that have been added into lists using the addvelocity methods.
	public void SetTotalVelocity () {

		//Core velocity that's been calculated across this script. Either assigns what it should be, or adds the stored force pushes.
		if (_externalCoreVelocity != default(Vector3))
		{
			_coreVelocity = _externalCoreVelocity;
			_externalCoreVelocity = default(Vector3);
		}
		if (!_isOverwritingCoreVelocity)
		{
			//Using a for loop instead of a foreach makes it longer to read, but creates less garbage so improves performance
			for (int i = 0 ; i < _listOfCoreVelocityToAdd.Count ; i++)
			{
				_coreVelocity += _listOfCoreVelocityToAdd[i];
			}
		}

		//Calculate total velocity this frame.
		_totalVelocity = _coreVelocity + _environmentalVelocity;
		for (int i = 0 ; i < _listOfVelocityToAddThisUpdate.Count ; i++)
		{
			_totalVelocity += _listOfVelocityToAddThisUpdate[i];
		}

		//Clear the lists to prevent forces carrying over multiple frames.
		_listOfCoreVelocityToAdd.Clear();
		_listOfVelocityToAddThisUpdate.Clear();
		_isOverwritingCoreVelocity = false;

		//Sets rigidbody velocity, this should be the only line in the player scripts to do so.
		_RB.velocity = _totalVelocity;

		//Debug.DrawRay(transform.position, _totalVelocity * Time.fixedDeltaTime, Color.grey, 10f);

		//Adds this new velocity to a list of 2, tracking the last 2 frames.
		_previousVelocities.Insert(0, _totalVelocity);
		_previousVelocities.RemoveAt(4);

		//Assigns the global variables for the current movement, since it's assigned at the end of a frame, changes between frames won't be counted when using this,
		_speedMagnitude = _totalVelocity.magnitude;
		Vector3 releVec = GetRelevantVector(_totalVelocity, false);
		_horizontalSpeedMagnitude = releVec.magnitude;

		releVec = GetRelevantVector(_coreVelocity, false);
		_currentRunningSpeed = releVec.magnitude;

		//Adds this new speed to a list of 3
		_previousHorizontalSpeeds.Insert(0, _horizontalSpeedMagnitude);
		_previousHorizontalSpeeds.RemoveAt(4);

		_previousRunningSpeeds.Insert(0, _currentRunningSpeed);
		_previousRunningSpeeds.RemoveAt(3);
	}

	//Calls all the methods involved in managing coreVelocity on the ground, such as normal control (with normal modifiers), sticking to the ground, and effects from slopes.
	private void GroundMovement () {

		_timeOnGround += Time.deltaTime;

		_coreVelocity = HandleControlledVelocity(_coreVelocity, new Vector2(1, 1));
		_coreVelocity = StickToGround(_coreVelocity);
		//_coreVelocity = HandleSlopePhysics(_coreVelocity);
	}

	//Calls methods relevant to general control and gravity, while applying the turn and accelleration modifiers depending on a number of factors while in the air.
	public Vector3 HandleAirMovement ( Vector3 coreVelocity ) {

		//In order to change horizontal movement in the air, the player must not be inputting into a wall. Because moving into a slanted wall can lead to the player sliding up it while still not being grounded.
		Vector3 spherePosition = transform.position - transform.up;
		Vector3 direction = GetRelevantVector(_moveInput, false);

		if (!Physics.SphereCast(spherePosition, _CharacterCapsule.radius, direction, out RaycastHit hit, 5, _Groundmask_))
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
			if (_horizontalSpeedMagnitude < 20)
			{
				airAccelMod += 0.5f;
			}

			//Handles lateral velocity.
			coreVelocity = HandleControlledVelocity(_coreVelocity, new Vector2(airTurnMod, airAccelMod));
		}

		//Apply Gravity (vertical velocity)
		if (_listOfIsGravityOn.Count == 0)
			coreVelocity = ApplyGravityToIncreaseFallSpeed(coreVelocity, _currentFallGravity, _currentUpwardsFallGravity, _maxFallingSpeed_, _totalVelocity);

		return coreVelocity;
	}

	//Handles core velocity, which is the velocity directly under the player's control (seperate from environmental velocity which is placed on the character by other things).
	//This turns, decreases and/or increases the velocity based on input.
	public Vector3 HandleControlledVelocity ( Vector3 startVelocity, Vector2 modifier, float decelerationModifier = 1 ) {

		//Certain actions control velocity in their own way, so if the list is greater than 0, end the method (ensuring anything that shouldn't carry over frames won't.)
		if (_listOfCanControl.Count != 0)
		{
			_externalRunningSpeed = -1;
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
		lateralVelocity = CallAccelerationAndTurning(lateralVelocity, _moveInput, modifier); //Because this is a delegate, the method it is calling may change, but by default it will be the method in this script called Default.

		lateralVelocity = Decelerate(lateralVelocity, _moveInput * decelerationModifier, _curvePosDecell);

		//If external core speed has been set to a positive value this frame, overwrite running speed without losing direction.
		if (_externalRunningSpeed >= 0 && lateralVelocity.magnitude > -1)
		{
			if (lateralVelocity.sqrMagnitude < 0.1f) { lateralVelocity = _MainSkin.forward; } //Ensures speed will always be applied, even if there's currently no velocity.
			lateralVelocity = lateralVelocity.normalized * _externalRunningSpeed;
			_externalRunningSpeed = -1; //Set to a negative value so core speeds of 0 can be set externally.
		}
		//Enforces the min speed if there is one, but only checks if close to it.
		else if (_currentMinSpeed > 0 && _currentRunningSpeed < _currentMinSpeed + 5)
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
		if (_currentRunningSpeed > _currentMaxSpeed)
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
		else if (_listOfCanTurns.Count == 0)
		{
			// Step 2) Rotate lateral velocity towards the same velocity under the desired rotation.
			//         The ammount rotated is determined by turn speed multiplied by turn rate (defined by the difference in angles, and current speed).
			//	Turn speed will also increase if the difference in pure input (ignoring camera) is different, allowing precise movement with the camera.

			float turnRate = (_isRolling ? _rollingTurningModifier_ : 1.0f);
			turnRate *= _TurnRateByAngle_.Evaluate(deviationFromInput);
			turnRate *= _TurnRateBySpeed_.Evaluate((_coreVelocity.sqrMagnitude / _currentMaxSpeed) / _currentMaxSpeed);

			if (_Input.IsTurningBecauseOfCamera(inputDirection))
			{
				turnRate *= _TurnRateByInputChange_.Evaluate(Vector3.Angle(_Input._inputOnController, _Input._prevInputWithoutCamera) / 180);
			}

			dragRate = _DragByAngle_.Evaluate(deviationFromInput) * _curvePosDrag; //If turning, may lose speed.

			lateralVelocity = Vector3.RotateTowards(lateralVelocity, lateralToInput * lateralVelocity, _turnSpeed_ * turnRate * Mathf.Deg2Rad * modifier.x, 0.0f); //Apply turn by calculate speed
		}

		// Step 3) Get current velocity (if it's zero then use input)
		//         Increase or decrease the size by using movetowards zero (a minus value increases size in the velocity direction)
		//         The total change is decided by acceleration based on input and speed, then drag from the turn.

		Vector3 setVelocity = lateralVelocity.sqrMagnitude > 0 ? lateralVelocity : inputDirection;
		float accelRate = 0;

		if (deviationFromInput < _angleToAccelerate_ || _currentRunningSpeed < 10) //Will only accelerate if inputing in direction enough, unless under certain speed.
		{
			accelRate = (_isRolling && _isGrounded ? _currentRollAccell : _currentRunAccell) * inputMagnitude;
			accelRate *= _curvePosAcell;
			if (_isGrounded) accelRate *= _AccelBySlope_.Evaluate(_groundNormal.y);
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
		if (_listOfCanDecelerates.Count == 0)
		{         //If there is no input, ready conventional deceleration.
			if (input.sqrMagnitude < 0.1)
			{
				if (_isGrounded)
				{
					decelAmount = _moveDeceleration_ * modifier;
				}
				else if (_shouldStopAirMovementIfNoInput_)
				{
					decelAmount = _airDecel_ * modifier;
				}
			}
			//If grounded and rolling but not on a slope, even with input, ready deceleration. 
			else if (_isRolling && _groundNormal.y > _slopeEffectLimit_ && _currentRunningSpeed > 10)
			{
				decelAmount = _rollingDecel_ * modifier;
			}
		}
		//If in air, a constant deceleration is applied in addition to any others.
		if (!_isGrounded && _currentRunningSpeed > 14)
		{
			decelAmount += _constantAirDecel_;
		}

		//Apply calculated deceleration
		return Vector3.MoveTowards(lateralVelocity, Vector3.zero, decelAmount);
	}

	//If just landed, apply additional speed dependant on slope angle.
	public Vector4 LandOnSlope ( Vector4 currentVelocity, Vector3 previousVelocity, float physicsCalculatedSpeed, float previousSpeed ) {

		float newSpeed = Mathf.Max(_previousHorizontalSpeeds[1], _previousRunningSpeeds[1]);
		Vector3 horizontalDirection = _totalVelocity.normalized;
		horizontalDirection.y = 0;

		//If was falling down faster last frame, but still going downhill and not uphill.
		if (previousVelocity.y < currentVelocity.y && currentVelocity.y < -10 && Vector3.Dot(_groundNormal, horizontalDirection) > 0f)
		{
			//Get magnitude,higher if rolling.
			float lerpValue = _isRolling ?  _landingConversionFactor_ * _rollingLandingBoost_ :  _landingConversionFactor_;

			newSpeed = Mathf.Lerp(newSpeed, physicsCalculatedSpeed, lerpValue);
		}
		_wasInAirBeforeSlope = false;

		
		currentVelocity = currentVelocity.normalized * newSpeed;
		currentVelocity.w = newSpeed;
		return currentVelocity;
	}

	//Handles interactions with slopes (non flat ground), both positive and negative, relative to the player's current rotation.
	//This includes adding force downhill, aiding or hampering running, as well as falling off when too slow.
	public Vector3 HandleSlopePhysics ( Vector3 worldVelocity, bool canChangeDirectionOfVelocity = true ) {
		if (!_isUsingSlopePhysics_) { return worldVelocity; }


		Vector3 slopeVelocity = Vector3.zero;

		if (canChangeDirectionOfVelocity)
		{
			//If moving too slow compared to the limit
			float speedRequirement = _SlopeSpeedLimitByAngle_.Evaluate(_groundNormal.y);
			if (_horizontalSpeedMagnitude < speedRequirement)
			{
				//Then fall off and away from the slope.
				SetIsGrounded(false, 0.5f);
				AddCoreVelocity(_groundNormal * 5f);
				_keepNormalCounter = _keepNormalForThis_ - 0.1f;
			}
		}

		//Slope power
		//If slope angle is less than limit, meaning on a slope
		if (_groundNormal.y < _slopeEffectLimit_ && _horizontalSpeedMagnitude > 3)
		{
			_isCurrentlyOnSlope = true;

			//Get force to always apply whether up or down hill
			float force =  -_curvePosSlopePower;
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
				decreaseTimeUpHillBy *= 1 + (_totalVelocity.normalized.y); //Decrease more depending on how downwards is moving. If going straight downwards, then this becomes x2, making it equal to any uphill.

				_timeUpHill -= Mathf.Clamp(_timeUpHill - decreaseTimeUpHillBy, 0, _timeUpHill); //Apply, but can't go under 0
				force *= _downhillMultiplier_; //Affect by unique stat for downhill
				force = _isRolling ? force * _rollingDownhillBoost_ : force; //Add more force if rolling.
			}

			//This force is then added to the current velocity. but aimed towards down the slope, leading to a more realistic and natural effect than just changing speed.
			Vector3 downSlopeForce = AlignWithNormal(new Vector3(_groundNormal.x, 0, _groundNormal.z), _groundNormal, -force);

			float amountToRotate = Vector3.Angle(Vector3.down, downSlopeForce) * Mathf.Deg2Rad;
			downSlopeForce = Vector3.RotateTowards(Vector3.down, downSlopeForce, amountToRotate * 0.7f, 0);

			slopeVelocity += downSlopeForce;

		}
		else { _timeUpHill = 0; }

		return worldVelocity + slopeVelocity;
	}

	//Handles the player's velocity following the path of the ground. This does not set the rotation to match it, but does prevent them from flying off or colliding with slopes.
	//This also handles stepping up over small ledges.
	public Vector3 StickToGround ( Vector3 velocity ) {

		if (!_canStickToGround) { return velocity; }

		Debug.DrawRay(transform.position, velocity * Time.deltaTime * 0.8f, Color.red, 10f);

		//If moving and has been grounded for long enough. The time on ground is to prevent gravity force before landing being carried over to shoot player forwards on landing.
		//Then ready a raycast to check for slopes.
		if (_timeOnGround > 0.12f && _horizontalSpeedMagnitude > 3)
		{

			Vector3 currentGroundNormal = _groundNormal;
			Vector3 raycastStartPosition = _FeetTransform.position + (transform.up * 0.1f);
			Vector3 rayCastDirection = _worldVelocity.normalized;

			//If the Raycast Hits something, then there is a wall in front, that could be a negative slope (ground is higher and will tilt backwards to go up).
			if (Physics.Raycast(raycastStartPosition, rayCastDirection, out RaycastHit hitSticking,
				_horizontalSpeedMagnitude * _stickCastAhead_ * Time.fixedDeltaTime, _Groundmask_))
			{
				float upwardsDirectionDifference = Vector3.Angle(currentGroundNormal, hitSticking.normal);
				float limit = _upwardsLimitByCurrentSlope_.Evaluate(currentGroundNormal.y);

				//If the angle difference between current slope and encountered one is under the limit	
				if (upwardsDirectionDifference / 180 < 70)
				{
					//Then it creates a velocity aligned to that new normal, then interpolates from the current to this new one.	
					currentGroundNormal = hitSticking.normal.normalized;
					Vector3 Dir = AlignWithNormal(velocity, currentGroundNormal, velocity.magnitude);
					velocity = Vector3.LerpUnclamped(velocity, Dir, _stickingLerps_.x);

					//If player is too far from ground, set back to buffer position.
					if (_placeAboveGroundBuffer_ > 0 && (_FeetTransform.position - _HitGround.point).sqrMagnitude > _placeAboveGroundBuffer_ * _placeAboveGroundBuffer_)
					{
						Vector3 directionFromGroundToPlayer = transform.position - _HitGround.point;
						Debug.DrawRay(hitSticking.point, -_groundCheckDirection * 2, Color.yellow, 10f);
						Vector3 newPos = hitSticking.point  -(_groundCheckDirection * _placeAboveGroundBuffer_) - _feetOffsetFromCentre;
						//Vector3 newPos = _HitGround.point  -(_groundCheckDirection * 1.0f);
						SetPlayerPosition(newPos);
					}
				}
				//If the difference is too large, then it's not a slope, and is likely facing towards the player, so see if it's a step to step over/onto.
				else
				{
					StepOver(raycastStartPosition, rayCastDirection, currentGroundNormal);
				}
			}

			// If there is no wall, then we may be dealing with a positive slope (like the outside of a loop, where the ground is relatively lower).
			else
			{
				float upwardsDirectionDifference = Vector3.Angle(transform.up, _groundNormal);

				//If the difference between current movement and ground normal is less than the limit to stick (lower limits prevent super sticking). 
				if (upwardsDirectionDifference / 180 < _stickingNormalLimit_)
				{
					if (upwardsDirectionDifference < 2)
					{
						//Shoots a raycast from infront, but downwards to check for lower ground.
						raycastStartPosition = raycastStartPosition + (rayCastDirection * (_horizontalSpeedMagnitude * 0.7f) * _stickCastAhead_ * Time.deltaTime);
						//Then create a velocity relative to the current groundNormal, then lerp from one to the other.
						if (Physics.Raycast(raycastStartPosition, -_groundNormal, out hitSticking, 2.5f, _Groundmask_))
						{
							currentGroundNormal = hitSticking.normal;
						}
					}
					Vector3 Dir = AlignWithNormal(velocity, currentGroundNormal, velocity.magnitude);
					velocity = Vector3.LerpUnclamped(velocity, Dir, _stickingLerps_.y);


					// Adds velocity downwards to remain on the slope. This is general so it won't be involved in the next coreVelocity calculations, which needs to be relevant to the ground surface.
					AddGeneralVelocity(-currentGroundNormal * _forceTowardsGround_, false);
				}
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
			AddGeneralVelocity(-_groundNormal * _forceTowardsGround_ * 1.2f, false);
		}
		Debug.DrawRay(transform.position, velocity * Time.deltaTime * 0.6f, Color.blue, 10f);
		return velocity;
	}


	//Handles stepping up onto slightly raised surfaces without losing momentum, rather than bouncing off them. Requires multiple checks into the situation to avoid clipping or unnecesary stepping.
	private void StepOver ( Vector3 raycastStartPosition, Vector3 rayCastDirection, Vector3 newGroundNormal ) {
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
		AddGeneralVelocity(-newGroundNormal * _forceTowardsGround_, false); //If doesn't step up, push more towards the ground, to prevent slowly sliding up a step 

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

	//Makes a vector relative to a normal. Such as a forward direction being affected by the ground.
	public Vector3 AlignWithNormal ( Vector3 vector, Vector3 normal, float magnitude ) {
		//return Vector3.ProjectOnPlane(vector.normalized, normal) * magnitude;

		Vector3 tangent = Vector3.Cross(normal, vector);
		Vector3 newVector = -Vector3.Cross(normal, tangent);
		vector = newVector.normalized * magnitude;
		return vector;
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
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.6f);
			}
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

						Quaternion targetRot = Quaternion.FromToRotation(transform.up, cross) * transform.rotation;
						transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 11f);
					}
					//When flipped over, set y value of the right side to zero to ensure not tilted anymore, and ready main rotation to sort any remaining rotation differences.
					else
					{
						transform.right = new Vector3(transform.right.x, 0, transform.right.z).normalized;
						_keepNormal = Vector3.up;
					}
				}
				//General rotation to face up again.
				else
				{
					Quaternion targetRot = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
					transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 5f);
					if (transform.rotation == targetRot && _isUpsideDown)
					{
						_isUpsideDown = false;
						_listOfCanTurns.Remove(false);
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
				_wasInAirBeforeSlope = true;
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

	//the following methods are called by other scripts when they want to affect the velocity. The changes are stored and applied in the SetTotalVelocity method.
	public void AddCoreVelocity ( Vector3 force, bool shouldPrintForce = false ) {

		_listOfCoreVelocityToAdd.Add(force);
		if (shouldPrintForce) Debug.Log("ADD Core FORCE  ");
	}
	public void SetCoreVelocity ( Vector3 force, string willOverwrite = "", bool shouldPrintForce = false ) {
		if (_isOverwritingCoreVelocity && willOverwrite == "")
		{ return; } //If a previous call set isoverwriting to true, then if this isn't doing the same it will be ignored.

		_isOverwritingCoreVelocity = willOverwrite == "Overwrite"; //If true, core velocity will be fully replaced, including additions. Sets to true rather than same bool, because setting to false would overwrite this.

		_externalCoreVelocity = force == Vector3.zero ? new Vector3(0, 0.02f, 0) : force; //Will not use Vector3.zero because that upsets the check seeing if this is not default(Vector3). To avoid that, use a tiny velocity.
		if (shouldPrintForce) Debug.Log("Set Core FORCE");
	}
	//This will change the magnitude of the local lateral velocity vector in ControlledVelocity but will not change the direction.
	public void SetLateralSpeed ( float speed, bool shouldPrintForce = true ) {
		_externalRunningSpeed = speed; //This will be set to negative at the end of the frame, but if changed here will be applied in HandleControlledVelocity (NOT SetTotalVelocity). This is because this should only change running speed.
		if (shouldPrintForce) Debug.Log("Set Core SPEED");
	}
	public void SetBothVelocities ( Vector3 force, Vector2 split, string willOverwrite = "", bool shouldPrintForce = false ) {
		_environmentalVelocity = force * split.y;

		SetCoreVelocity(force * split.x, willOverwrite, shouldPrintForce);

		if (shouldPrintForce) Debug.Log("Set Total FORCE To " + force);
	}

	//Bear in mind velocity added in this method will only last this frame, as the velocity will be recalclated without it next fixedUpdate.
	public void AddGeneralVelocity ( Vector3 force, bool shouldIncludeThisNextCheck = true, bool shouldPrintForce = false ) {
		_listOfVelocityToAddThisUpdate.Add(force);

		if (!shouldIncludeThisNextCheck)
		{
			_velocityToNotCountWhenCheckingForAChange += force;
		}

		if (shouldPrintForce) Debug.Log("ADD Total FORCE  " + force);
	}

	//Environmental. Caused by objects in the world, but can b removed by others.
	public void SetEnvironmentalVelocity ( Vector3 force, bool willRemoveOnGrounded, bool willRemoveOnAirAction,
		S_Enums.ChangeLockState whatToDoWithDeceleration = S_Enums.ChangeLockState.Ignore, bool shouldPrintForce = false ) {

		_environmentalVelocity = force;

		//Because HandleDeceleration can be called to lock multiple times before being called to unlock (because Unlock should only be called when removing environmnetal velocity),
		//only add a new lock if it hasn't locked this way already.
		if ((_resetEnvironmentalOnAirAction && willRemoveOnAirAction) || (_resetEnvironmentalOnGrounded && willRemoveOnGrounded))
		{
			//Intentionally empty, so the else only happens if the above is false.
		}
		else
		{
			//This will apply or remove constraints on deceleration, as certain calls will prevent manual deceleration, while calls that remove this velocity will allow it again. But will usually be ignored.
			if (willRemoveOnAirAction && willRemoveOnGrounded) { whatToDoWithDeceleration = S_Enums.ChangeLockState.Lock; }
			HandleDecelerationWhenEnvironmentalForce(whatToDoWithDeceleration);
		}

		_resetEnvironmentalOnGrounded = willRemoveOnGrounded;
		_resetEnvironmentalOnAirAction = willRemoveOnAirAction;

		if (shouldPrintForce) Debug.Log("Set Environmental FORCE  " + force);
	}

	private void HandleDecelerationWhenEnvironmentalForce ( S_Enums.ChangeLockState whatCase ) {

		//Due to deceleration working with core velocity at different speeds. Sometimes when environmental velocity is set, it will prevent deceleration because that would make the movement path inconsistent
		//(as core velocity won't always be the same before environmental is added).
		switch (whatCase)
		{
			//This should always be called before Unlock. As such, whenever an environmental velocity is setting willRemoveOnGrounded to true, it should do this. Because the check will call unlock if true, then stop checking.
			case S_Enums.ChangeLockState.Lock:
				_listOfCanDecelerates.Add(false); break;
			//This should only be called when environmental velocity is being removed.
			case S_Enums.ChangeLockState.Unlock:
				_listOfCanDecelerates.RemoveAt(0); break;
				//Ignore is the default state, which means this call won't change the deceleration ability.
		}
	}


	public void AddEnvironmentalVelocity ( Vector3 force ) {
		_environmentalVelocity += force;
	}
	//Called by air actions to check if environmental velocity should be removed.
	public void RemoveEnvironmentalVelocityAirAction () {
		//If The last time environmental velocity was set, it was set to reset here, then remove environmental velocity. This will be called in other air actions as well.
		if (_resetEnvironmentalOnGrounded)
		{
			SetEnvironmentalVelocity(Vector3.zero, false, false, S_Enums.ChangeLockState.Unlock);
		}
	}

	public void SetPlayerPosition ( Vector3 newPosition, bool shouldPrintLocation = true ) {

		Debug.DrawLine(transform.position, newPosition, Color.magenta, 10f);
		Debug.DrawRay(newPosition, Vector3.down * 0.3f, Color.magenta, 10f);
		transform.position = newPosition;

		if (shouldPrintLocation) Debug.Log("Change Position to  ");
	}
	public void AddToPlayerPosition (Vector3 Add) {
		transform.Translate(Add);
	}

	public void SetPlayerRotation ( Quaternion newRotation, bool shouldPrintRotation = false ) {
		transform.rotation = newRotation;

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
		_currentRunAccell = _startAcceleration_;
		_currentRollAccell = _startRollAcceleration_;
		_currentTopSpeed = _startTopSpeed_;
		_currentMaxSpeed = _startMaxSpeed_;
		_maxFallingSpeed_ = _startMaxFallingSpeed_;
		_currentFallGravity = _startFallGravity_;
		_currentUpwardsFallGravity = _gravityWhenMovingUp_;

		_keepNormal = Vector3.up;


	}

	private void AssignTools () {
		s_MasterPlayer = this;
		_RB = GetComponent<Rigidbody>();
		_Actions = _Tools._ActionManager;
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_Events = _Tools.PlayerEvents;

		_CharacterCapsule = _Tools.CharacterCapsule.GetComponent<CapsuleCollider>();
		_FeetTransform = _Tools.FeetPoint;
		_MainSkin = _Tools.MainSkin;
	}
	#endregion
}
