using UnityEngine;
using System.Collections;
using UnityEngine.Windows;
using SplineMesh;

[RequireComponent(typeof(S_ActionManager))]
public class S_Action05_Rail : MonoBehaviour, IMainAction
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	//Scripts
	private S_CharacterTools      _Tools;
	private S_PlayerPhysics       _PlayerPhys;
	private S_PlayerInput         _Input;
	private S_ActionManager       _Actions;
	private S_Control_PlayerSound _Sounds;
	[HideInInspector]
	public S_Interaction_Pathers  _Rail_int;
	public S_AddOnRail            _ConnectedRails;
	private S_HedgeCamera         _CamHandler;

	//Current rail
	private Transform             _RailTransform;
	public CurveSample            _Sample;

	//ZipLine
	[HideInInspector]
	public Transform              _ZipHandle;
	[HideInInspector]
	public Rigidbody              _ZipBody;

	//Effects
	private GameObject            _JumpBall;
	private Animator              _CharacterAnimator;
	private Transform             _MainSkin;
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	[Header("Skin Rail Params")]

	public float                  _skinRotationSpeed;
	private float                 _offsetRail_ = 2.05f;
	private float                 _offsetZip_ = -2.05f;

	[HideInInspector]
	public float                  _railmaxSpeed_;
	private float                 _railTopSpeed_;
	private float                 _decaySpeedLow_;
	private float                 _decaySpeedHigh_;
	private float                 _minStartSpeed_ = 60f;
	private float                 _pushFowardmaxSpeed_ = 80f;
	private float                 _pushFowardIncrements_ = 15f;
	private float                 _pushFowardDelay_ = 0.5f;
	private float                 _generalHillModifier = 2.5f;
	private float                 _upHillMultiplier_ = 0.25f;
	private float                 _downHillMultiplier_ = 0.35f;
	private float                 _upHillMultiplierCrouching_ = 0.4f;
	private float                 _downHillMultiplierCrouching_ = 0.6f;
	private float                 _dragVal_ = 0.0001f;
	private float                 _playerBrakePower_ = 0.95f;
	private float                 _hopDelay_;
	private float                 _hopDistance_ = 12;
	private float                 _decayTime_;
	private AnimationCurve        _accelBySpeed_;
	private float                 _decaySpeed_;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;        //In every action script, takes note of where in the Action Managers Main action list this script is. 

	[HideInInspector]
	public S_Interaction_Pathers.PathTypes _whatKindOfRail;     //Set when entering the action, deciding if this is a zipline, rail or later added type

	private float                 _pulleyRotate;
	[HideInInspector]
	public float                  _curvePosSlope;


	private float                 _timer = 0f;
	[HideInInspector]
	public float                  _pointOnSpline = 0f;
	[HideInInspector]
	public float                  _playerSpeed;
	[HideInInspector]
	public bool                   _isGoingBackwards;
	private int                   _movingDirection;
	private bool                  _isCrouching;
	private bool                   isBraking;

	private Vector3               _sampleForwards;

	//Quaternion rot;
	private Vector3               _setOffSet;

	//Camera testing
	public float                  _targetDistance = 10;
	public float                  _cameraLerp = 10;

	//Stepping
	private bool        _canInput = true;
	private bool        _canHop = false;
	private float       _distanceToStep;
	private float       _stepSpeed_ = 3.5f;
	private bool        _isSteppingRight;
	private bool        _isFacingRight = true;

	//Boosters
	[HideInInspector]
	public bool         _isBoosted;
	[HideInInspector]
	public float        _boostTime;
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {

	}

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyAction();
	}

	// Update is called once per frame
	void Update () {
		if (_Rail_int._isFollowingPath)
		{
			PlaceOnRail();
			PerformHop();

			SoundControl();
			//Handle animations
			switch (_whatKindOfRail)
			{
				case S_Interaction_Pathers.PathTypes.rail:
					_Actions.ActionDefault.HandleAnimator(10);
					_CharacterAnimator.SetBool("GrindRight", _isFacingRight);
					break;
				case S_Interaction_Pathers.PathTypes.zipline:
					_Actions.ActionDefault.HandleAnimator(9);
					break;
			}
		}

		// Actions Go Here
		if (_canInput)
		{
			HandleInputs();
		}
	}

	private void FixedUpdate () {
		//If on a rail.
		if (_Rail_int._isFollowingPath)
		{
			MoveOnRail();
		}
		//If no longer on a path, then exit the action and return to regular state.
		else
		{
			//End action
			_Actions.ActionDefault.ReadyCoyote();
			_Actions.ActionDefault.StartAction();
		}
	}

	public bool AttemptAction () {
		_Rail_int._canGrindOnRail = true; //The pathers interaction will check if this is true when hitting a rail. If it is, then enter this action, but it will usually be false.
		return false;
	}

	public void StartAction () {
		if (enabled) { _PlayerPhys._listOfCanControl.RemoveAt(0); } //Because this action can transfer into itself through rail hopping, undo the lock that would usually be undone in StopAction.

		//Prevents raill hopping temporarily
		StartCoroutine(DelayHopOnLanding());

		//ignore further rail collisions
		Physics.IgnoreLayerCollision(this.gameObject.layer, 23, true);

		//Set private 
		_canInput = true;
		_timer = _pushFowardDelay_;
		_distanceToStep = 0; //Ensure not immediately stepping when called

		_Rail_int._canGrindOnRail = false; //Prevents calling this multiple times in one update

		_isCrouching = false;
		_pulleyRotate = 0f;

		_isBoosted = false;
		_boostTime = 0;

		//Set controls
		_PlayerPhys._isGravityOn = false;
		_PlayerPhys._listOfCanControl.Add(false);
		_Input.JumpPressed = false;

		//Animator
		_CharacterAnimator.SetTrigger("ChangedState");
		switch (_whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.rail:
				_CharacterAnimator.SetBool("GrindRight", _isFacingRight);
				_CharacterAnimator.SetInteger("Action", 10);
				break;
			case S_Interaction_Pathers.PathTypes.zipline:
				_ZipHandle.GetComponentInChildren<MeshCollider>().enabled = false;
				_CharacterAnimator.SetInteger("Action", 9);
				break;
		}

		//If got onto this rail from anything except a rail hop, set speed to physics.
		_playerSpeed = _PlayerPhys._speedMagnitude;

		//Get how much the character is facing the same way as the point.
		CurveSample sample = _Rail_int._PathSpline.GetSampleAtDistance(_pointOnSpline);
		_sampleForwards = _RailTransform.rotation * sample.tangent;
		float facingDot = Vector3.Dot(_PlayerPhys._RB.velocity.normalized, _sampleForwards);

		//What action before this one.
		switch (_Actions.whatAction)
		{
			// If it was a homing attack, the difference in facing should be by the direction moving BEFORE the attack was performed.
			case S_Enums.PrimaryPlayerStates.Homing:
				facingDot = Vector3.Dot(GetComponent<S_Action02_Homing>()._directionBeforeAttack.normalized, _sampleForwards);
				_playerSpeed = GetComponent<S_Action02_Homing>()._speedBeforeAttack;
				break;
			//If it was a drop charge, add speed from the charge to the grind speed.
			case S_Enums.PrimaryPlayerStates.DropCharge:
				float charge = _Actions.Action08.externalDash();
				_playerSpeed = Mathf.Clamp(charge, _playerSpeed + (charge / 6), 160);
				break;

		}

		// Get Direction for the Rail
		_isGoingBackwards = facingDot < 0;

		// Apply minimum speed
		_playerSpeed = Mathf.Max(_playerSpeed, _minStartSpeed_);

		_PlayerPhys.SetTotalVelocity(Vector3.zero, new Vector2(1, 0)); //Freeze player before gaining speed from the grind next frame.

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Rail);
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; }

		enabled = false;

		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		//If left this action to perform a jump,
		if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Jump)
		{
			switch (_whatKindOfRail)
			{
				case S_Interaction_Pathers.PathTypes.zipline:
					_PlayerPhys.SetCoreVelocity(_sampleForwards * _playerSpeed, true);  //Ensure player carries on momentum
					_PlayerPhys._groundNormal = Vector3.up; // Fix rotation

					//After a delay, restore zipline collisions and physics
					StartCoroutine(_Rail_int.JumpFromZipLine(_ZipHandle, 1));
					_ZipBody.isKinematic = true;
					break;
			}
		}

		//Restore Control
		_PlayerPhys._isGravityOn = true;
		_PlayerPhys._listOfCanControl.RemoveAt(0);

		//To prevent instant actions
		_Input.RollPressed = false;
		_Input.SpecialPressed = false;
		_Input.BouncePressed = false;

		//Local values
		_Rail_int._isFollowingPath = false;

		StartCoroutine(DelayCollision());
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Physics
	//Gets new location on rail, changing position and rotation to match. Called in update in order to ensure the character matches the rail in real time.
	public void PlaceOnRail () {

		//Increase/decrease the Amount of distance travelled on the Spline by DeltaTime and direction
		float travelAmount = (Time.deltaTime * _playerSpeed);
		_movingDirection = _isGoingBackwards ? -1 : 1;

		_pointOnSpline += travelAmount * _movingDirection;

		//If this point is on the spline.
		if (_pointOnSpline < _Rail_int._PathSpline.Length && _pointOnSpline > 0)
		{
			//Get the data of the spline at that point along it (rotation, location, etc)
			_Sample = _Rail_int._PathSpline.GetSampleAtDistance(_pointOnSpline);
			_sampleForwards = _RailTransform.rotation * _Sample.tangent * _movingDirection;

			//Set player Position and rotation on Rail
			switch (_whatKindOfRail)
			{
				//Place character in world space on point in rail
				case S_Interaction_Pathers.PathTypes.rail:

					transform.up = transform.rotation * (_RailTransform.rotation * _Sample.up);
					_MainSkin.rotation = Quaternion.LookRotation(_sampleForwards, transform.up);

					Vector3 relativeOffset = _RailTransform.rotation * _Sample.Rotation * -_setOffSet; //Moves player to the left or right of the spline to be on the correct rail

					//Position is set to the local location of the spline point, the location of the spline object, the player offset relative to the up position (so they're actually on the rail) and the local offset.
					Vector3 newPos = _RailTransform.position + ( _RailTransform.rotation * _Sample.location);
					newPos += (_Sample.up * _offsetRail_) + relativeOffset;
					transform.position = newPos;
					break;

				case S_Interaction_Pathers.PathTypes.zipline:

					//Set ziphandle rotation to follow sample
					_ZipHandle.rotation = _RailTransform.rotation * _Sample.Rotation;
					transform.up = transform.rotation * (_RailTransform.rotation * _Sample.up);
					_MainSkin.rotation = Quaternion.LookRotation(_sampleForwards, transform.up);

					//Since the handle and by extent the player can be tilted up to the sides (not changing forward direction), adjust the eueler angles to reflect this.
					//_pulleyRotate is handled in input, but applied here.
					_ZipHandle.eulerAngles = new Vector3 (_ZipHandle.eulerAngles.x, _ZipHandle.eulerAngles.y, _ZipHandle.eulerAngles.z + _pulleyRotate * 70f);
					_MainSkin.eulerAngles = new Vector3(_MainSkin.eulerAngles.x, _MainSkin.eulerAngles.y, _MainSkin.eulerAngles.z + _pulleyRotate * 70f);

					//Similar to on rail, but place handle first, and player relevant to that.
					newPos = _RailTransform.position + (_RailTransform.rotation * _Sample.location);
					newPos += _setOffSet;
					_ZipHandle.transform.position = newPos;
					transform.position = newPos + (_ZipHandle.transform.up * _offsetZip_);
					break;
			}
		}
		
	}
	//Takes the data from the previous method but handles physics for smoothing and applying if lost rail.
	public void MoveOnRail () {

		HandleRailSpeed(); //Make changes to player speed based on angle

		//If this point is on the spline.
		if (_pointOnSpline < _Rail_int._PathSpline.Length && _pointOnSpline > 0)
		{

			//Set Player Speed correctly so that it becomes smooth grinding
			_PlayerPhys.SetCoreVelocity(_sampleForwards * _playerSpeed);
			if (_ZipBody) { _ZipBody.velocity = _sampleForwards * _playerSpeed; }
		}
		else
		{
			//Since has gone beyond the spline, treat the player as leaving on the very end to make consistent calculations.
			if (!_isGoingBackwards)
				_Sample = _Rail_int._PathSpline.GetSampleAtDistance(_Rail_int._PathSpline.Length - 1);
			else
				_Sample = _Rail_int._PathSpline.GetSampleAtDistance(0);

			_sampleForwards = _RailTransform.rotation * _Sample.tangent * _movingDirection;
			LoseRail();
		}
	}

	void LoseRail () {
		Physics.IgnoreLayerCollision(this.gameObject.layer, 23, true);

		//If the spline loops around then just move place on length back to the start or end.
		if (_Rail_int._PathSpline.IsLoop)
		{
			_pointOnSpline = _pointOnSpline + (_Rail_int._PathSpline.Length * -_movingDirection);
			return;
		}

		//Or if this rail has either a next rail or previous rail attached.
		else if (_ConnectedRails != null)
		{
			//If going forwards, and the rail has a rail off the end, then go onto it.
			if (!_isGoingBackwards && _ConnectedRails.nextRail != null && _ConnectedRails.nextRail.isActiveAndEnabled)
			{
				//Set point on spline to be how much over this grind went over the current rail.
				_pointOnSpline = Mathf.Max(0, _pointOnSpline - _Rail_int._PathSpline.Length);

				//The data storing next and previous rails is changed to the one for the new rail, meaning this rail will now become PrevRail.
				_ConnectedRails = _ConnectedRails.nextRail;

				//Change the offset to match this rail (since may go from a rail offset from a spline, straight onto rail directily on a different spline)
				_setOffSet.Set(-_ConnectedRails.GetComponent<S_PlaceOnSpline>().Offset3d.x, 0, 0);

				//Set path and positions to follow.
				_Rail_int._PathSpline = _ConnectedRails.GetComponentInParent<Spline>();
				_RailTransform = _Rail_int._PathSpline.transform.parent;
				return;
			}
			//If going backwards, and the rail has a rail off the end, then go onto it.
			else if (_isGoingBackwards && _ConnectedRails.PrevRail != null && _ConnectedRails.PrevRail.isActiveAndEnabled)
			{
				//Set data first, because will need to affect point by new length.
				_ConnectedRails = _ConnectedRails.PrevRail;

				// Change offset to match the new rail.
				_setOffSet.Set(-_ConnectedRails.GetComponent<S_PlaceOnSpline>().Offset3d.x, 0, 0);

				//Set path and positions to follow.
				_Rail_int._PathSpline = _ConnectedRails.GetComponentInParent<Spline>();
				_RailTransform = _Rail_int._PathSpline.transform.parent;

				//Since coming onto this new rail from the end of it, must have a reference to its length. This is why the point is acquired at the end of this if flow, rather than the start.
				_pointOnSpline = _pointOnSpline + _Rail_int._PathSpline.Length;
				_pointOnSpline = _Rail_int._PathSpline.Length;
				return;
			}
		}
		//If hasn't returned yet, then there is nothing to follow, so actually leave the rail.

		_Input.LockInputForAWhile(5f, false); //Prevent instant turning off the end of the rail
		_distanceToStep = 0; //Stop a step that might be happening

		switch (_whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.zipline:

				//If the end of a zipline, then the handle must go flying off the end, so disable trigger for player and homing target, but renable collider with world.
				_ZipHandle.GetComponent<CapsuleCollider>().enabled = false;
				if (_ZipHandle.GetComponentInChildren<MeshCollider>()) { _ZipHandle.GetComponentInChildren<MeshCollider>().enabled = false; }
				GameObject target = _ZipHandle.transform.GetComponent<S_Control_Zipline>().homingtgt;
				target.SetActive(false);

				_PlayerPhys.SetCoreVelocity(_ZipBody.velocity); //Make sure zip handle flies off

				//Make player face upwards again rather than tilted to the side from rotating handle
				Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
				if (VelocityMod != Vector3.zero) { _MainSkin.rotation = Quaternion.LookRotation(VelocityMod, transform.up); }
				_PlayerPhys.SetCoreVelocity(_sampleForwards * _playerSpeed); //Make sure player flies off the end of the rail consitantly.
				break;

			case S_Interaction_Pathers.PathTypes.rail:
				_PlayerPhys.SetCoreVelocity(_sampleForwards * _playerSpeed); //Make sure player flies off the end of the rail consitantly.

				VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
				if (VelocityMod != Vector3.zero) { _MainSkin.rotation = Quaternion.LookRotation(VelocityMod, transform.up); }
				break;
		}

		//Prevent instant stepping.
		_Input.LeftStepPressed = false;
		_Input.RightStepPressed = false;

		//Next frame will return to default state
		_Rail_int._isFollowingPath = false;

	}

	//Prevent colliding with rails until slightly after losing the current rail.
	IEnumerator DelayCollision () {
		yield return new WaitForSeconds(0.3f);
		Physics.IgnoreLayerCollision(this.gameObject.layer, 23, false);
	}

	void HandleRailSpeed () {
		if (isBraking && _playerSpeed > _minStartSpeed_) _playerSpeed *= _playerBrakePower_;

		ApplyBoost();
		HandleSlopes();

		//Decrease speed if over max or top speed on the rail.
		if (_playerSpeed > _railmaxSpeed_)
			_playerSpeed -= _decaySpeedHigh_;
		else if (_playerSpeed > _railTopSpeed_)
			_playerSpeed -= _decaySpeedLow_;

		_playerSpeed = Mathf.Clamp(_playerSpeed, 10, _PlayerPhys._currentMaxSpeed);
	}

	//Set to true outside of this script. But when boosted on a rail will gain a bunch of speed at once before having some of it quickly drop off.
	void ApplyBoost () {
		//If currently being boosted
		if (_isBoosted)
		{
			//When boost starts, boost time is set to positive, so when it goes below 0 it should start losing speed.
			_boostTime -= Time.fixedDeltaTime;
			if (_boostTime < 0)
			{		
				if (_playerSpeed > 60) { _playerSpeed -= _decaySpeed_; } //Speed can never decay to go under 60.
				//Keep losing speed until _decayTime_ amount of time has passed.
				if (_boostTime < -_decayTime_)
				{
					_isBoosted = false;
					_boostTime = 0;
				}
			}
		}
	}

	void HandleSlopes () {
		//Start a force to apply based on the curve position and general modifier for all slopes handled in physics script 
		float force = _generalHillModifier;
		force *= ((1 - (Mathf.Abs(transform.up.y) / 10)) + 1); //Force affected by steepness of slope. The closer to 0 (completely horizontal), the greater the force, ranging from 1 - 2
		float AbsYPow = Mathf.Abs(_PlayerPhys._RB.velocity.normalized.y * _PlayerPhys._RB.velocity.normalized.y);

		//use player vertical speed to find if player is going up or down
		//if going uphill on rail
		if (_PlayerPhys._RB.velocity.y > 0.05f)
		{
			//Get main modifier and multiply by position on curve and general hill modifer used for other slope physics.
			force *= _isCrouching ? _upHillMultiplierCrouching_ : _upHillMultiplier_;
			force *= -1;
		}
		else if (_PlayerPhys._RB.velocity.y < -0.05f)
		{
			//Downhill
			force *=_isCrouching ? _downHillMultiplierCrouching_ : _downHillMultiplier_;
		}
		force = (AbsYPow * force) + (_dragVal_ * _playerSpeed);
		//Apply to moving speed (if uphill will be a negative/
		_playerSpeed += force;
	}

	//Inputs
	public void HandleInputs () {
		if (!_Actions.isPaused)
		{
			HandleUniqueInputs();

			//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
			_Actions.HandleInputs(_positionInActionList);
		}
	}

	void HandleUniqueInputs () {
		_timer += Time.deltaTime;

		//Certain types of rail have unique controls / subactions to them.
		switch (_whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.rail:

				//Crouching, relevant to slope physics.
				_isCrouching = _Input.RollPressed;
				_CharacterAnimator.SetBool("isRolling", _isCrouching);

				//RailTrick to accelerate, but only after delay
				if (_Input.SpecialPressed && _timer > _pushFowardDelay_)
				{	
					//Will only increase speed if under the max trick speed.
					if (_playerSpeed < _pushFowardmaxSpeed_)
					{
						_playerSpeed += _pushFowardIncrements_ + _accelBySpeed_.Evaluate(_playerSpeed / _pushFowardmaxSpeed_); //Increae by flat increment, affected by current speed
					}
					_isFacingRight = !_isFacingRight; //This will cause the animator to perform a small hop and face the other way.
					_timer = 0f; //Resets timer so delay must be exceeded again.
					_Input.SpecialPressed = false; //Prevents it being spammed by holding		
				}

				CheckHopping();
				break;

			case S_Interaction_Pathers.PathTypes.zipline:
				float aimForRotation = 0;
				if (_Input.RightStepPressed) { aimForRotation = 1; }
				else if (_Input.LeftStepPressed) { aimForRotation = -1; }

				//_pulleyRotate is used in the RailGrind method, so here lerp towards the new goal rather than make it instant.
				_pulleyRotate = Mathf.MoveTowards(_pulleyRotate, aimForRotation, 3.5f * Time.deltaTime);
				break;
		}
		//Breaking
		isBraking = _Input.BouncePressed;
	}

	void CheckHopping () {
		if (_canInput && _canHop)
		{
			//Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
			Vector3 Direction = _MainSkin.position - _CamHandler.transform.position;
			bool isFacing = Vector3.Dot(_MainSkin.forward, Direction.normalized) < -0.5f;
			if (_Input.RightStepPressed && isFacing)
			{	
				_Input.RightStepPressed = false;
				_Input.LeftStepPressed = true;	
			}
			else if (_Input.LeftStepPressed && isFacing)
			{
				_Input.RightStepPressed = true;
				_Input.LeftStepPressed = false;			
			}

			//If there is still an input, set the distance to step, which will be taken and handled in PerformHop();
			if (_Input.RightStepPressed || _Input.LeftStepPressed)
			{
				_distanceToStep = _hopDistance_;
				_isSteppingRight = _Input.RightStepPressed; //Right step has priority over left

				//Disable inputs until the hop is over
				_canInput = false;
				_Input.RightStepPressed = false;
				_Input.LeftStepPressed = false;
			}
		}
	}

	void PerformHop () {
		//If this is set to over zero in checkHopping, then the player should be moved off the rail accordingly.
		if (_distanceToStep > 0)
		{
			//Get how far to move this frame and in which direction.
			float move = _stepSpeed_ * Time.deltaTime;
			if (_isSteppingRight)
				move = -move;
			if (_isGoingBackwards)
				move = -move;

			move = Mathf.Clamp(move, -_distanceToStep, _distanceToStep);

			//To show hopping off a rail, change the offset, this means the player will still follow the rail during the hop.
			_setOffSet.Set(_setOffSet.x + move, _setOffSet.y, _setOffSet.z);

			//If moving, check for walls, and if there's a collision, end state.
			if (move < 0)
				if (Physics.BoxCast(_MainSkin.position, new Vector3(1.3f, 3f, 1.3f), -_MainSkin.right, Quaternion.identity, 4, _Tools.Stats.QuickstepStats.StepLayerMask))
				{
					_Actions.ActionDefault.StartAction();
				}
				else if (Physics.BoxCast(_MainSkin.position, new Vector3(1.3f, 3f, 1.3f), _MainSkin.right, Quaternion.identity, 4, _Tools.Stats.QuickstepStats.StepLayerMask))
				{
					_Actions.ActionDefault.StartAction();
				}

			//Decrease how far to move by how far has moved.
			_distanceToStep -= Mathf.Abs(move);

			//Near the end of a step, renable collision so can collide again with grind on them instead.
			if (_distanceToStep < 6)
			{
				AttemptAction();
				Physics.IgnoreLayerCollision(this.gameObject.layer, 23, false);

				//Once a step is over and the player hasn't started this action through collisions, exit state.
				if (_distanceToStep <= 0)
				{
					_Rail_int._isFollowingPath = false;
					_Actions.ActionDefault.StartAction();
				}
			}
		}
	}

	//Make it so can't rail hop until being on a rail for long enough. This includes hopping from one rail to another.
	IEnumerator DelayHopOnLanding () {
		_canHop = false;
		yield return new WaitForSeconds(_hopDelay_);
		_canHop = true;
	}

	//Effects
	void SoundControl () {
		//Player Rail Sound

	}
	#endregion

	//Called by the pathers interaction script to ready important stats gained from the collision. This is seperate to startAction because startAction is inherited from an interface.
	public void AssignForThisGrind ( float range, Transform Rail, S_Interaction_Pathers.PathTypes type, Vector3 thisOffset, S_AddOnRail AddOn ) {

		_setOffSet = -thisOffset; //Offset is obtianed from the offset on the collider, and will be followed consitantly to allow folowing different rails on the same spline.

		_whatKindOfRail = type; //Zipline or rail

		//Setting up Rails
		_pointOnSpline = range; //Starts at this position along the spline.
		_RailTransform = Rail; //Player position must add this as spline positions are in local space.
		_Rail_int._isFollowingPath = true;

		_ConnectedRails = AddOn; //Will be used to go onto subsequent rails without recalculating collisions.
	}

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//Assigns all external elements of the action.
	public void ReadyAction () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
			for (int i = 0 ; i < _Actions._MainActions.Count ; i++)
			{
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Rail)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Actions = GetComponent<S_ActionManager>();
		_Input = GetComponent<S_PlayerInput>();
		_Rail_int = GetComponent<S_Interaction_Pathers>();
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_CamHandler = GetComponent<S_Handler_Camera>()._HedgeCam;

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.mainSkin;
		_Sounds = _Tools.SoundControl;

		_JumpBall = _Tools.JumpBall;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_railTopSpeed_ = _Tools.Stats.RailStats.railTopSpeed;
		_railmaxSpeed_ = _Tools.Stats.RailStats.railMaxSpeed;
		_decaySpeedHigh_ = _Tools.Stats.RailStats.railDecaySpeedHigh;
		_decaySpeedLow_ = _Tools.Stats.RailStats.railDecaySpeedLow;
		_minStartSpeed_ = _Tools.Stats.RailStats.MinStartSpeed;
		_pushFowardmaxSpeed_ = _Tools.Stats.RailStats.RailPushFowardmaxSpeed;
		_pushFowardIncrements_ = _Tools.Stats.RailStats.RailPushFowardIncrements;
		_pushFowardDelay_ = _Tools.Stats.RailStats.RailPushFowardDelay;
		_generalHillModifier = _Tools.Stats.SlopeStats.generalHillMultiplier;
		_upHillMultiplier_ = _Tools.Stats.RailStats.RailUpHillMultiplier;
		_downHillMultiplier_ = _Tools.Stats.RailStats.RailDownHillMultiplier;
		_upHillMultiplierCrouching_ = _Tools.Stats.RailStats.RailUpHillMultiplierCrouching;
		_downHillMultiplierCrouching_ = _Tools.Stats.RailStats.RailDownHillMultiplierCrouching;
		_dragVal_ = _Tools.Stats.RailStats.RailDragVal;
		_playerBrakePower_ = _Tools.Stats.RailStats.RailPlayerBrakePower;
		_hopDelay_ = _Tools.Stats.RailStats.hopDelay;
		_stepSpeed_ = _Tools.Stats.RailStats.hopSpeed;
		_hopDistance_ = _Tools.Stats.RailStats.hopDistance;
		_accelBySpeed_ = _Tools.Stats.RailStats.RailAccelerationBySpeed;

		_offsetRail_ = _Tools.Stats.RailPosition.offsetRail;
		_offsetZip_ = _Tools.Stats.RailPosition.offsetZip;
		_decaySpeed_ = _Tools.Stats.RailStats.railBoostDecaySpeed;
		_decayTime_ = _Tools.Stats.RailStats.railBoostDecayTime;
	}
	#endregion

}


