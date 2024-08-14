using UnityEngine;
using System.Collections;
using UnityEngine.Windows;
using SplineMesh;

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
	private S_Control_SoundsPlayer _Sounds;
	[HideInInspector]
	public S_Interaction_Pathers  _Rail_int;
	private S_AddOnRail            _ConnectedRails;
	private S_HedgeCamera         _CamHandler;

	//Current rail
	private Transform             _RailTransform;
	private CurveSample            _Sample;

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

	public float                  _skinRotationSpeed = 20;
	private float                 _offsetRail_ = 2.05f;
	private float                 _offsetZip_ = -2.05f;

	private float                 _minStartSpeed_ = 60f;
	[HideInInspector]
	public float                  _railmaxSpeed_;
	private float                 _railTopSpeed_;
	private float                 _playerBrakePower_ = 0.95f;
	private AnimationCurve        _accelBySpeed_;

	private float                 _decaySpeed_;

	private float                 _pushFowardmaxSpeed_ = 80f;
	private float                 _pushFowardIncrements_ = 15f;
	private float                 _pushFowardDelay_ = 0.5f;

	private float                 _generalHillModifier = 2.5f;
	private float                 _upHillMultiplier_ = 0.25f;
	private float                 _downHillMultiplier_ = 0.35f;
	private float                 _upHillMultiplierCrouching_ = 0.4f;
	private float                 _downHillMultiplierCrouching_ = 0.6f;

	private float                 _boostDecayTime_;
	private float                 _boostDecaySpeed_;

	private float                 _hopSpeed_ = 3.5f;
	private float                 _hopDelay_;
	private float                 _hopDistance_ = 12;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;        //In every action script, takes note of where in the Action Managers Main action list this script is. 

	private bool         _canEnterRail = true;            //Prevents the start action method being called multiple times when on a rail. Must be set to false when leaving or starting a hop.

	[HideInInspector]
	public S_Interaction_Pathers.PathTypes _whatKindOfRail;     //Set when entering the action, deciding if this is a zipline, rail or later added type

	private float                 _pulleyRotate;      //Set by inputs and incorperated into position and rotation on spline when using a zipline. Decides how much to tilt the handle and player.

	private float                 _pushTimer = 0f;    //Constantly goes up, is set to zzero after pushing forward. Implements the delay to prevent constant pushing.
	[HideInInspector]
	public float                  _pointOnSpline = 0f; //The actual place on the spline being travelled. The number is how many units along the length of the spline it is (not affected by spline length).
	[HideInInspector]
	public bool                   _isGoingBackwards;  //Is the player going up or down on the spline points.
	private int                   _movingDirection;   //A 1 or -1 based on going backwards or not. Used in calculations.
	private bool                  _isCrouching;       //Set by input, will change slope calculations
	private bool                   _isBraking;        //Set by input, if true will decrease speed.

	private Vector3               _sampleForwards;    //The sample is the world point of a spline at a distance along it. This if the relevant forwards direction of that point including spline transform.
	private Vector3               _sampleUpwards;    //The sample is the world point of a spline at a distance along it. This if the relevant forwards direction of that point including spline transform.

	//Quaternion rot;
	private Vector3               _setOffSet;         //Will follow a spline at this distance (relevant to sample forwards). Set when entering a spline and used to grind on rails offset of the spline. Hopping will change this value to move to the sides.

	//Stepping
	private bool        _canInput = true;   //Set true when entering a rail, but set false when rail hopping. Must be two to perform any actions.
	private bool        _canHop = false;    //Set false when entering a rail, but true after a moment.
	private float       _distanceToStep;    //Set when starting a hop and will go down by distance traveled every frame, ending action when zero.
	private bool        _isSteppingRight;   //Hopping to a rail on the right or on the left.

	private bool        _isFacingRight = true;        //Used by the animator, changed on push forward.

	private float       _grindingSpeed;     //Set by action pathSpeeds every frame. Used to check movement along rail.

	//Boosters
	[HideInInspector]
	public bool         _isBoosted;
	[HideInInspector]
	public float        _boostTime;

	[HideInInspector]
	public bool	_isGrinding; //USed to ensure no calculations are made from this still being active for possibly one frame called by Update when ending action.
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

	// Update is called once per frame
	void Update () {
		if(!enabled || !_isGrinding) { return; }
		PlaceOnRail();
		PerformHop();

		SoundControl();
		//Handle animations
		switch (_whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.rail:
				_Actions._ActionDefault.HandleAnimator(10);
				_CharacterAnimator.SetBool("GrindRight", _isFacingRight);
				break;
			case S_Interaction_Pathers.PathTypes.zipline:
				_Actions._ActionDefault.HandleAnimator(9);
				break;
		}

	}

	private void FixedUpdate () {

		if (!enabled || !_isGrinding) { return; }

		//This is to make the code easier to read, as a single variable name is easier than an element in a public list.
		if (_Actions._listOfSpeedOnPaths.Count > 0) { _grindingSpeed = _Actions._listOfSpeedOnPaths[0]; } 

		MoveOnRail();
		if (_canInput) { HandleInputs(); }

		if (_Actions._listOfSpeedOnPaths.Count > 0) { _Actions._listOfSpeedOnPaths[0] = _grindingSpeed; }//Apples all changes to grind speed.
	}

	public bool AttemptAction () {
		_Rail_int._canGrindOnRail = true; //The pathers interaction will check if this is true when hitting a rail. If it is, then enter this action, but it will usually be false.
		return false;
	}

	public void StartAction () {
		if (!_canEnterRail) { return; }
		_canEnterRail = false;

		//ignore further rail collisions
		Physics.IgnoreLayerCollision(this.gameObject.layer, 23, true);

		if (enabled) { _PlayerPhys._listOfCanControl.RemoveAt(0); } //Because this action can transfer into itself through rail hopping, undo the lock that would usually be undone in StopAction. This prevents multiple from stacking up.

		//Prevents raill hopping temporarily
		StartCoroutine(DelayHopOnLanding());

		//Set private 
		_isGrinding = true;
		_canInput = true;
		_pushTimer = _pushFowardDelay_;
		_distanceToStep = 0; //Ensure not immediately stepping when called

		_Rail_int._canGrindOnRail = false; //Prevents calling this multiple times in one update

		_isCrouching = false;
		_pulleyRotate = 0f;

		_isBoosted = false;
		_boostTime = 0;

		//Set controls
		_PlayerPhys._listOfIsGravityOn.Add(false);
		_PlayerPhys._listOfCanControl.Add(false);
		_PlayerPhys._canChangeGrounded = false;

		_Input._JumpPressed = false;

		//Animator
		_Actions._ActionDefault.SwitchSkin(true);
		_CharacterAnimator.SetTrigger("ChangedState");
		switch (_whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.rail:
				_CharacterAnimator.SetBool("GrindRight", _isFacingRight);   //Sets which direction the character animation is facing. Tracked between rails to hopping doesn't change it.
				_CharacterAnimator.SetInteger("Action", 10);
				break;
			case S_Interaction_Pathers.PathTypes.zipline:
				_ZipHandle.GetComponentInChildren<MeshCollider>().enabled = false; //Ensures there won't be weird collisions along the zipline.
				_CharacterAnimator.SetInteger("Action", 9);
				break;
		}

		//If got onto this rail from anything except a rail hop, set speed to physics.
		_Actions._listOfSpeedOnPaths.Add(_PlayerPhys._speedMagnitude);

		//Get how much the character is facing the same way as the point.
		CurveSample sample = _Rail_int._PathSpline.GetSampleAtDistance(_pointOnSpline);
		_sampleForwards = _RailTransform.rotation * sample.tangent;
		float facingDot = Vector3.Dot(_PlayerPhys._RB.velocity.normalized, _sampleForwards);

		_grindingSpeed = _PlayerPhys._horizontalSpeedMagnitude;
		//What action before this one.
		switch (_Actions._whatCurrentAction)
		{
			// If it was a homing attack, the difference in facing should be by the direction moving BEFORE the attack was performed.
			case S_Enums.PrimaryPlayerStates.Homing:
				facingDot = Vector3.Dot(GetComponent<S_Action02_Homing>()._directionBeforeAttack.normalized, _sampleForwards);
				_grindingSpeed = GetComponent<S_Action02_Homing>()._speedBeforeAttack;
				break;
			//If it was a drop charge, add speed from the charge to the grind speed.
			case S_Enums.PrimaryPlayerStates.DropCharge:
				float charge = GetComponent<S_Action08_DropCharge>().GetCharge();
				_grindingSpeed = Mathf.Clamp(charge, _grindingSpeed + (charge / 6), 160);
				break;
		}

		// Get Direction for the Rail
		_isGoingBackwards = facingDot < 0;

		// Apply minimum speed
		_grindingSpeed = Mathf.Max(_grindingSpeed, _minStartSpeed_);

		_PlayerPhys.SetBothVelocities(Vector3.zero, new Vector2(1, 0)); //Freeze player before gaining speed from the grind next frame.

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Rail);
		enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { ReadyAction(); return; } //First time is called on ActionManager Awake() to ensure this starts disabled and has a single opportunity to assign tools and stats.

		_isGrinding = false;

		//If left this action to perform a jump,
		if (_Actions._whatCurrentAction == S_Enums.PrimaryPlayerStates.Jump)
		{
			switch (_whatKindOfRail)
			{
				case S_Interaction_Pathers.PathTypes.zipline:
					_PlayerPhys.SetCoreVelocity(_sampleForwards * _grindingSpeed, "Overwrite");  //Ensure player carries on momentum
					_PlayerPhys._groundNormal = Vector3.up; // Fix rotation

					//After a delay, restore zipline collisions and physics
					StartCoroutine(_Rail_int.JumpFromZipLine(_ZipHandle, 1));
					_ZipBody.isKinematic = true;
					break;
			}
		}

		//Restore Control
		_PlayerPhys._listOfIsGravityOn.RemoveAt(0);
		_PlayerPhys._listOfCanControl.RemoveAt(0);
		_PlayerPhys._canChangeGrounded = true;

		//To prevent instant actions
		_Input._RollPressed = false;
		_Input._SpecialPressed = false;
		_Input._BouncePressed = false;

		_Actions._listOfSpeedOnPaths.RemoveAt(0); //Remove the speed that was used for this action. As a list because this stop action might be called after the other action's StartAction.

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
		float travelAmount = (Time.deltaTime * _grindingSpeed);
		_movingDirection = _isGoingBackwards ? -1 : 1;

		_pointOnSpline += travelAmount * _movingDirection;

		//If this point is on the spline.
		if (_pointOnSpline < _Rail_int._PathSpline.Length && _pointOnSpline > 0)
		{
			//Get the data of the spline at that point along it (rotation, location, etc)
			_Sample = _Rail_int._PathSpline.GetSampleAtDistance(_pointOnSpline);
			_sampleForwards = _RailTransform.rotation * _Sample.tangent * _movingDirection;
			_sampleUpwards = (_RailTransform.rotation * _Sample.up);

			//Set player Position and rotation on Rail
			switch (_whatKindOfRail)
			{
				//Place character in world space on point in rail
				case S_Interaction_Pathers.PathTypes.rail:

					_PlayerPhys.transform.up = _sampleUpwards;
					_Actions._ActionDefault.SetSkinRotationToVelocity(_skinRotationSpeed, _sampleForwards, default(Vector3), _sampleUpwards);

					Vector3 relativeOffset = _RailTransform.rotation * _Sample.Rotation * -_setOffSet; //Moves player to the left or right of the spline to be on the correct rail

					//Position is set to the local location of the spline point, the location of the spline object, the player offset relative to the up position (so they're actually on the rail) and the local offset.
					Vector3 newPos = _RailTransform.position + ( _RailTransform.rotation * _Sample.location);
					newPos += (_sampleUpwards * _offsetRail_) + relativeOffset;
					_PlayerPhys.SetPlayerPosition(newPos);
					break;

				case S_Interaction_Pathers.PathTypes.zipline:

					//Set ziphandle rotation to follow sample
					_ZipHandle.rotation = _RailTransform.rotation * _Sample.Rotation;
					//Since the handle and by extent the player can be tilted up to the sides (not changing forward direction), adjust the eueler angles to reflect this.
					//_pulleyRotate is handled in input, but applied here.
					_ZipHandle.eulerAngles = new Vector3(_ZipHandle.eulerAngles.x, _ZipHandle.eulerAngles.y, _ZipHandle.eulerAngles.z + _pulleyRotate * 70f * _movingDirection);

					//_PlayerPhys.transform.up = _PlayerPhys.transform.rotation * (_RailTransform.rotation * _Sample.up);
					_Actions._ActionDefault.SetSkinRotationToVelocity(_skinRotationSpeed, _sampleForwards);
					_MainSkin.eulerAngles = new Vector3(_MainSkin.eulerAngles.x, _MainSkin.eulerAngles.y, _pulleyRotate * 70f);

					//Similar to on rail, but place handle first, and player relevant to that.
					newPos = _RailTransform.position + (_RailTransform.rotation * _Sample.location);
					newPos += _setOffSet;
					_ZipHandle.transform.position = newPos;
					_PlayerPhys.SetPlayerPosition( newPos + (_ZipHandle.transform.up * _offsetZip_));
					break;
			}
		}

	}
	//Takes the data from the previous method but handles physics for smoothing and applying if lost rail.
	public void MoveOnRail () {

		if(!_isGrinding) { return; }
		_PlayerPhys.SetIsGrounded(true, 0.5f);

		HandleRailSpeed(); //Make changes to player speed based on angle

		//If this point is on the spline.
		if (_pointOnSpline < _Rail_int._PathSpline.Length && _pointOnSpline > 0)
		{
			//Set Player Speed correctly so that it becomes smooth grinding
			_PlayerPhys.SetBothVelocities(_sampleForwards * _grindingSpeed, new Vector2(1, 0));
			if (_ZipBody) { _ZipBody.velocity = _sampleForwards * _grindingSpeed; }
		}
		else
		{
			//Since has gone beyond the spline, treat the player as leaving on the very end to make consistent calculations.
			if (!_isGoingBackwards)
				_Sample = _Rail_int._PathSpline.GetSampleAtDistance(_Rail_int._PathSpline.Length - 1);
			else
				_Sample = _Rail_int._PathSpline.GetSampleAtDistance(0);

			_sampleForwards = _RailTransform.rotation * _Sample.tangent * _movingDirection;
			CheckLoseRail();
		}
	}

	//Checks the properties of the rail to see if should enter 
	void CheckLoseRail () {

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

		LoseRail();
	}

	//Called when the player is at the end of a rail and being launched off.
	private void LoseRail () {
		_Input.LockInputForAWhile(5f, false, _sampleForwards); //Prevent instant turning off the end of the rail
		StartCoroutine(_PlayerPhys.LockFunctionForTime(S_PlayerPhysics.EnumControlLimitations.canDecelerate, 0, 10));
		_distanceToStep = 0; //Stop a step that might be happening

		_isGrinding = false;

		switch (_whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.zipline:

				//If the end of a zipline, then the handle must go flying off the end, so disable trigger for player and homing target, but renable collider with world.
				_ZipHandle.GetComponent<CapsuleCollider>().enabled = false;
				if (_ZipHandle.GetComponentInChildren<MeshCollider>()) { _ZipHandle.GetComponentInChildren<MeshCollider>().enabled = false; }
				GameObject target = _ZipHandle.transform.GetComponent<S_Control_Zipline>()._HomingTarget;
				target.SetActive(false);

				//_PlayerPhys.SetCoreVelocity(_ZipBody.velocity); //Make sure zip handle flies off

				_PlayerPhys.SetCoreVelocity(_sampleForwards * _grindingSpeed); //Make sure player flies off the end of the rail consitantly.
				break;

			case S_Interaction_Pathers.PathTypes.rail:
				_PlayerPhys.SetBothVelocities(_sampleForwards * _grindingSpeed, new Vector2(1, 0)); //Make sure player flies off the end of the rail consitantly.
				break;
		}

		//Prevent instant stepping.
		_Input._LeftStepPressed = false;
		_Input._RightStepPressed = false;

		_PlayerPhys._canChangeGrounded = true;
		_PlayerPhys.CheckForGround();

		//End action
		StartCoroutine(_Actions._ActionDefault.CoyoteTime());
		_Actions._ActionDefault.StartAction();
	}

	//Prevent colliding with rails until slightly after losing the current rail.
	IEnumerator DelayCollision () {
		yield return new WaitForSeconds(0.45f);
		Physics.IgnoreLayerCollision(this.gameObject.layer, 23, false);
		_canEnterRail = true;
	}

	void HandleRailSpeed () {
		if (_isBraking && _grindingSpeed > _minStartSpeed_) _grindingSpeed *= _playerBrakePower_;

		HandleBoost();
		HandleSlopes();

		//Decrease speed if over max or top speed on the rail.
		_grindingSpeed = Mathf.Min(_grindingSpeed, _railmaxSpeed_);

		if (_grindingSpeed > _railTopSpeed_)
			_grindingSpeed -= _decaySpeed_;

		_grindingSpeed = Mathf.Clamp(_grindingSpeed, 10, _PlayerPhys._currentMaxSpeed);
	}

	//Set to true outside of this script. But when boosted on a rail will gain a bunch of speed at once before having some of it quickly drop off.
	void HandleBoost () {
		//If currently being boosted
		if (_isBoosted)
		{
			//When boost starts, boost time is set to positive, so when it goes below 0 it should start losing speed.
			_boostTime -= Time.fixedDeltaTime;
			if (_boostTime < 0)
			{
				if (_grindingSpeed > 60) { _grindingSpeed -= _boostDecaySpeed_; } //Speed can never decay to go under 60.
										      //Keep losing speed until _decayTime_ amount of time has passed.
				if (_boostTime < -_boostDecayTime_)
				{
					_isBoosted = false;
					_boostTime = 0;
				}
			}
		}
	}

	private void HandleSlopes () {
		//Start a force to apply based on the curve position and general modifier for all slopes handled in physics script 
		float force = _generalHillModifier;
		force *= (1 - (Mathf.Abs(_PlayerPhys.transform.up.y) / 10)) + 1; //Force affected by steepness of slope. The closer to 0 (completely horizontal), the greater the force, ranging from 1 - 2
		//float AbsYPow = Mathf.Abs(_PlayerPhys._RB.velocity.normalized.y * _PlayerPhys._RB.velocity.normalized.y);

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
			force *= _isCrouching ? _downHillMultiplierCrouching_ : _downHillMultiplier_;
		}
		force = (0.1f * force);
		//Apply to moving speed (if uphill will be a negative/
		_grindingSpeed += force;
	}

	//Inputs
	public void HandleInputs () {

		//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
		_Actions.HandleInputs(_positionInActionList);

		if (!_Actions._isPaused) HandleUniqueInputs();
	}

	private void HandleUniqueInputs () {
		_pushTimer += Time.deltaTime;

		//Certain types of rail have unique controls / subactions to them.
		switch (_whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.rail:

				//Crouching, relevant to slope physics.
				_isCrouching = _Input._RollPressed;
				_CharacterAnimator.SetBool("isRolling", _isCrouching);

				//RailTrick to accelerate, but only after delay
				if (_Input._SpecialPressed && _pushTimer > _pushFowardDelay_)
				{
					//Will only increase speed if under the max trick speed.
					if (_grindingSpeed < _pushFowardmaxSpeed_)
					{
						_grindingSpeed += _pushFowardIncrements_ * _accelBySpeed_.Evaluate(_grindingSpeed / _pushFowardmaxSpeed_); //Increae by flat increment, affected by current speed
					}
					_isFacingRight = !_isFacingRight; //This will cause the animator to perform a small hop and face the other way.
					_pushTimer = 0f; //Resets timer so delay must be exceeded again.
					_Input._SpecialPressed = false; //Prevents it being spammed by holding		
				}

				CheckHopping();
				break;

			case S_Interaction_Pathers.PathTypes.zipline:
				float aimForRotation = 0;
				if (_Input._RightStepPressed) { aimForRotation = 1; }
				else if (_Input._LeftStepPressed) { aimForRotation = -1; }

				//_pulleyRotate is used in the RailGrind method, so here lerp towards the new goal rather than make it instant.
				_pulleyRotate = Mathf.MoveTowards(_pulleyRotate, aimForRotation, 3.5f * Time.deltaTime);
				break;
		}
		//Breaking
		_isBraking = _Input._BouncePressed;
	}

	private void CheckHopping () {
		if (_canInput && _canHop)
		{
			//Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
			Vector3 Direction = _MainSkin.position - _CamHandler.transform.position;
			bool isFacing = Vector3.Dot(_MainSkin.forward, Direction.normalized) < -0.5f;
			if (_Input._RightStepPressed && isFacing)
			{
				_Input._RightStepPressed = false;
				_Input._LeftStepPressed = true;
			}
			else if (_Input._LeftStepPressed && isFacing)
			{
				_Input._RightStepPressed = true;
				_Input._LeftStepPressed = false;
			}

			//If there is still an input, set the distance to step, which will be taken and handled in PerformHop();
			if (_Input._RightStepPressed || _Input._LeftStepPressed)
			{
				_distanceToStep = _hopDistance_;
				_isSteppingRight = _Input._RightStepPressed; //Right step has priority over left

				//Disable inputs until the hop is over
				_canInput = false;
				_Input._RightStepPressed = false;
				_Input._LeftStepPressed = false;
			}
		}
	}

	private void PerformHop () {
		//If this is set to over zero in checkHopping, then the player should be moved off the rail accordingly.
		if (_distanceToStep > 0)
		{
			//Get how far to move this frame and in which direction.
			float move = _hopSpeed_ * Time.deltaTime;
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
					_Actions._ActionDefault.StartAction();
				}
				else if (Physics.BoxCast(_MainSkin.position, new Vector3(1.3f, 3f, 1.3f), _MainSkin.right, Quaternion.identity, 4, _Tools.Stats.QuickstepStats.StepLayerMask))
				{
					_Actions._ActionDefault.StartAction();
				}

			//Decrease how far to move by how far has moved.
			_distanceToStep -= Mathf.Abs(move);

			//Near the end of a step, renable collision so can collide again with grind on them instead.
			if (_distanceToStep < 6)
			{
				AttemptAction();
				Physics.IgnoreLayerCollision(this.gameObject.layer, 23, false);
				_canEnterRail = true;

				//Once a step is over and the player hasn't started this action through collisions, exit state.
				if (_distanceToStep <= 0)
				{
					_Actions._ActionDefault.StartAction();
				}
			}
		}
	}

	//Make it so can't rail hop until being on a rail for long enough. This includes hopping from one rail to another.
	private IEnumerator DelayHopOnLanding () {
		_canHop = false;
		yield return new WaitForSeconds(_hopDelay_);
		_canHop = true;
	}

	//Effects
	void SoundControl () {
		//Player Rail Sound

	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public
	//Called by the pathers interaction script to ready important stats gained from the collision. This is seperate to startAction because startAction is inherited from an interface.
	public void AssignForThisGrind ( float range, Transform Rail, S_Interaction_Pathers.PathTypes type, Vector3 thisOffset, S_AddOnRail AddOn ) {

		_setOffSet = -thisOffset; //Offset is obtianed from the offset on the collider, and will be followed consitantly to allow folowing different rails on the same spline.

		_whatKindOfRail = type; //Zipline or rail

		//Setting up Rails
		_pointOnSpline = range; //Starts at this position along the spline.
		_RailTransform = Rail; //Player position must add this as spline positions are in local space.

		_ConnectedRails = AddOn; //Will be used to go onto subsequent rails without recalculating collisions.
	}

	//Called externally when entering a booster on a rail. Changes speed. 
	public IEnumerator ApplyBoosters ( float speed, bool set, float addSpeed, bool backwards ) {
		//Rather than apply boost immediately, stretch it over three frames for smoothness and to ensure player proerly enters rail.
		for (int i = 0 ; i < 3 ; i++)
		{
			yield return new WaitForFixedUpdate();

			//Set means completely changing the speed to a specific value.
			if (set)
			{
				if (_grindingSpeed < speed)
				{
					_grindingSpeed = speed;
					_isBoosted = true;
					_boostTime = 0.9f; //How long the boost lasts before decaying.

				}
				else
					set = false; //If speed higher than what will be set, go through the other option instead.
			}
			//Keep checking if on a rail before applying this.
			if (_Actions._whatCurrentAction == S_Enums.PrimaryPlayerStates.Rail)
			{
				_grindingSpeed += addSpeed;
				_isBoosted = true;
				_boostTime = 0.7f; //How long the boost lasts before decaying.

				break; //Since speed has now been applied, can end checking for if on rail.

			}

			//Changes which direction to grind in.
			if (backwards)
				_isGoingBackwards = true;
			else
				_isGoingBackwards = false;
		}
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
		_Actions =	_Tools._ActionManager;
		_Input =		_Tools.GetComponent<S_PlayerInput>();
		_Rail_int =	_Tools.PathInteraction;
		_PlayerPhys =	_Tools.GetComponent<S_PlayerPhysics>();
		_CamHandler =	_Tools.CamHandler._HedgeCam;

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin =	_Tools.MainSkin;
		_Sounds =		_Tools.SoundControl;

		_JumpBall =	_Tools.JumpBall;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_railTopSpeed_ = _Tools.Stats.RailStats.railTopSpeed;
		_railmaxSpeed_ = _Tools.Stats.RailStats.railMaxSpeed;
		_decaySpeed_ = _Tools.Stats.RailStats.railDecaySpeed;
		_minStartSpeed_ = _Tools.Stats.RailStats.minimumStartSpeed;
		_pushFowardmaxSpeed_ = _Tools.Stats.RailStats.RailPushFowardmaxSpeed;
		_pushFowardIncrements_ = _Tools.Stats.RailStats.RailPushFowardIncrements;
		_pushFowardDelay_ = _Tools.Stats.RailStats.RailPushFowardDelay;
		_generalHillModifier = _Tools.Stats.SlopeStats.generalHillMultiplier;
		_upHillMultiplier_ = _Tools.Stats.RailStats.RailUpHillMultiplier.x;
		_downHillMultiplier_ = _Tools.Stats.RailStats.RailDownHillMultiplier.x;
		_upHillMultiplierCrouching_ = _Tools.Stats.RailStats.RailUpHillMultiplier.y;
		_downHillMultiplierCrouching_ = _Tools.Stats.RailStats.RailDownHillMultiplier.y;
		_playerBrakePower_ = _Tools.Stats.RailStats.RailPlayerBrakePower;
		_hopDelay_ = _Tools.Stats.RailStats.hopDelay;
		_hopSpeed_ = _Tools.Stats.RailStats.hopSpeed;
		_hopDistance_ = _Tools.Stats.RailStats.hopDistance;
		_accelBySpeed_ = _Tools.Stats.RailStats.PushBySpeed;

		_offsetRail_ = _Tools.Stats.RailPosition.offsetRail;
		_offsetZip_ = _Tools.Stats.RailPosition.offsetZip;
		_boostDecaySpeed_ = _Tools.Stats.RailStats.railBoostDecaySpeed;
		_boostDecayTime_ = _Tools.Stats.RailStats.railBoostDecayTime;
	}
	#endregion

}


