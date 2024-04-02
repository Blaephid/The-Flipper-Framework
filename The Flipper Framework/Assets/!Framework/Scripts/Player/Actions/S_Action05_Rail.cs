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

	public float                  _skinRotationSpeed = 20;
	private float                 _offsetRail_ = 2.05f;
	private float                 _offsetZip_ = -2.05f;

	[HideInInspector]
	private float                 _minStartSpeed_ = 60f;
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

	private float		_hopSpeed_ = 3.5f;
	private float                 _hopDelay_;
	private float                 _hopDistance_ = 12;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;        //In every action script, takes note of where in the Action Managers Main action list this script is. 

	[HideInInspector]
	public S_Interaction_Pathers.PathTypes _whatKindOfRail;     //Set when entering the action, deciding if this is a zipline, rail or later added type

	private float                 _pulleyRotate;	//Set by inputs and incorperated into position and rotation on spline when using a zipline. Decides how much to tilt the handle and player.

	private float                 _pushTimer = 0f;	//Constantly goes up, is set to zzero after pushing forward. Implements the delay to prevent constant pushing.
	[HideInInspector]
	public float                  _pointOnSpline = 0f; //The actual place on the spline being travelled. The number is how many units along the length of the spline it is (not affected by spline length).
	[HideInInspector]
	public bool                   _isGoingBackwards;	//Is the player going up or down on the spline points.
	private int                   _movingDirection;	//A 1 or -1 based on going backwards or not. Used in calculations.
	private bool                  _isCrouching;	//Set by input, will change slope calculations
	private bool                   _isBraking;	//Set by input, if true will decrease speed.

	private Vector3               _sampleForwards;	//The sample is the world point of a spline at a distance along it. This if the relevant forwards direction of that point including spline transform.

	//Quaternion rot;
	private Vector3               _setOffSet;	//Will follow a spline at this distance (relevant to sample forwards). Set when entering a spline and used to grind on rails offset of the spline. Hopping will change this value to move to the sides.

	//Stepping
	private bool        _canInput = true;	//Set true when entering a rail, but set false when rail hopping. Must be two to perform any actions.
	private bool        _canHop = false;	//Set false when entering a rail, but true after a moment.
	private float       _distanceToStep;	//Set when starting a hop and will go down by distance traveled every frame, ending action when zero.
	private bool        _isSteppingRight;	//Hopping to a rail on the right or on the left.

	private bool        _isFacingRight = true;	//Used by the animator, changed on push forward.

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
					_Actions._ActionDefault.HandleAnimator(10);
					_CharacterAnimator.SetBool("GrindRight", _isFacingRight);
					break;
				case S_Interaction_Pathers.PathTypes.zipline:
					_Actions._ActionDefault.HandleAnimator(9);
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
			StartCoroutine(_Actions._ActionDefault.CoyoteTime());
			_Actions._ActionDefault.StartAction();
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
		_pushTimer = _pushFowardDelay_;
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
		_Actions._ActionDefault.SwitchSkin(true);
		_CharacterAnimator.SetTrigger("ChangedState");
		switch (_whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.rail:
				_CharacterAnimator.SetBool("GrindRight", _isFacingRight);	//Sets which direction the character animation is facing. Tracked between rails to hopping doesn't change it.
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

		//What action before this one.
		switch (_Actions._whatAction)
		{
			// If it was a homing attack, the difference in facing should be by the direction moving BEFORE the attack was performed.
			case S_Enums.PrimaryPlayerStates.Homing:
				facingDot = Vector3.Dot(GetComponent<S_Action02_Homing>()._directionBeforeAttack.normalized, _sampleForwards);
				_Actions._listOfSpeedOnPaths[0] = GetComponent<S_Action02_Homing>()._speedBeforeAttack;
				break;
			//If it was a drop charge, add speed from the charge to the grind speed.
			case S_Enums.PrimaryPlayerStates.DropCharge:
				float charge = GetComponent<S_Action08_DropCharge>().GetCharge();
				_Actions._listOfSpeedOnPaths[0] = Mathf.Clamp(charge, _Actions._listOfSpeedOnPaths[0] + (charge / 6), 160);
				break;
		}

		// Get Direction for the Rail
		_isGoingBackwards = facingDot < 0;

		// Apply minimum speed
		_Actions._listOfSpeedOnPaths[0] = Mathf.Max(_Actions._listOfSpeedOnPaths[0], _minStartSpeed_);

		_PlayerPhys.SetTotalVelocity(Vector3.zero, new Vector2(1, 0)); //Freeze player before gaining speed from the grind next frame.

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Rail);
		this.enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; }

		enabled = false;

		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		//If left this action to perform a jump,
		if (_Actions._whatAction == S_Enums.PrimaryPlayerStates.Jump)
		{
			switch (_whatKindOfRail)
			{
				case S_Interaction_Pathers.PathTypes.zipline:
					_PlayerPhys.SetCoreVelocity(_sampleForwards * _Actions._listOfSpeedOnPaths[0], true);  //Ensure player carries on momentum
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
		float travelAmount = (Time.deltaTime * _Actions._listOfSpeedOnPaths[0]);
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

					_PlayerPhys.transform.up = _PlayerPhys.transform.rotation * (_RailTransform.rotation * _Sample.up);
					_MainSkin.rotation = Quaternion.LookRotation(_sampleForwards, _PlayerPhys.transform.up);

					Vector3 relativeOffset = _RailTransform.rotation * _Sample.Rotation * -_setOffSet; //Moves player to the left or right of the spline to be on the correct rail

					//Position is set to the local location of the spline point, the location of the spline object, the player offset relative to the up position (so they're actually on the rail) and the local offset.
					Vector3 newPos = _RailTransform.position + ( _RailTransform.rotation * _Sample.location);
					newPos += (_Sample.up * _offsetRail_) + relativeOffset;
					_PlayerPhys.transform.position = newPos;
					break;

				case S_Interaction_Pathers.PathTypes.zipline:

					//Set ziphandle rotation to follow sample
					_ZipHandle.rotation = _RailTransform.rotation * _Sample.Rotation;
					_PlayerPhys.transform.up = _PlayerPhys.transform.rotation * (_RailTransform.rotation * _Sample.up);
					_MainSkin.rotation = Quaternion.LookRotation(_sampleForwards, _PlayerPhys.transform.up);

					//Since the handle and by extent the player can be tilted up to the sides (not changing forward direction), adjust the eueler angles to reflect this.
					//_pulleyRotate is handled in input, but applied here.
					_ZipHandle.eulerAngles = new Vector3 (_ZipHandle.eulerAngles.x, _ZipHandle.eulerAngles.y, _ZipHandle.eulerAngles.z + _pulleyRotate * 70f * _movingDirection);
					_MainSkin.eulerAngles = new Vector3(_MainSkin.eulerAngles.x, _MainSkin.eulerAngles.y, _MainSkin.eulerAngles.z + _pulleyRotate * 70f);

					//Similar to on rail, but place handle first, and player relevant to that.
					newPos = _RailTransform.position + (_RailTransform.rotation * _Sample.location);
					newPos += _setOffSet;
					_ZipHandle.transform.position = newPos;
					_PlayerPhys.transform.position = newPos + (_ZipHandle.transform.up * _offsetZip_);
					break;
			}
		}
		
	}
	//Takes the data from the previous method but handles physics for smoothing and applying if lost rail.
	public void MoveOnRail () {

		_PlayerPhys._isGrounded = true;

		HandleRailSpeed(); //Make changes to player speed based on angle

		//If this point is on the spline.
		if (_pointOnSpline < _Rail_int._PathSpline.Length && _pointOnSpline > 0)
		{
			//Set Player Speed correctly so that it becomes smooth grinding
			_PlayerPhys.SetCoreVelocity(_sampleForwards * _Actions._listOfSpeedOnPaths[0]);
			if (_ZipBody) { _ZipBody.velocity = _sampleForwards * _Actions._listOfSpeedOnPaths[0]; }
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

		_Input.LockInputForAWhile(5f, false, _sampleForwards); //Prevent instant turning off the end of the rail
		StartCoroutine(_PlayerPhys.LockFunctionForTime(S_PlayerPhysics.EnumControlLimitations.canDecelerate, 0, 10));
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
				_PlayerPhys.SetCoreVelocity(_sampleForwards * _Actions._listOfSpeedOnPaths[0]); //Make sure player flies off the end of the rail consitantly.
				break;

			case S_Interaction_Pathers.PathTypes.rail:
				_PlayerPhys.SetCoreVelocity(_sampleForwards * _Actions._listOfSpeedOnPaths[0]); //Make sure player flies off the end of the rail consitantly.

				VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
				if (VelocityMod != Vector3.zero) { _MainSkin.rotation = Quaternion.LookRotation(VelocityMod, _PlayerPhys.transform.up); }
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
		if (_isBraking && _Actions._listOfSpeedOnPaths[0] > _minStartSpeed_) _Actions._listOfSpeedOnPaths[0] *= _playerBrakePower_;

		HandleBoost();
		HandleSlopes();

		//Decrease speed if over max or top speed on the rail.
		_Actions._listOfSpeedOnPaths[0] = Mathf.Min(_Actions._listOfSpeedOnPaths[0], _railmaxSpeed_);

		if (_Actions._listOfSpeedOnPaths[0] > _railTopSpeed_)
			_Actions._listOfSpeedOnPaths[0] -= _decaySpeed_;

		_Actions._listOfSpeedOnPaths[0] = Mathf.Clamp(_Actions._listOfSpeedOnPaths[0], 10, _PlayerPhys._currentMaxSpeed);
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
				if (_Actions._listOfSpeedOnPaths[0] > 60) { _Actions._listOfSpeedOnPaths[0] -= _boostDecaySpeed_; } //Speed can never decay to go under 60.
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
		force *= ((1 - (Mathf.Abs(_PlayerPhys.transform.up.y) / 10)) + 1); //Force affected by steepness of slope. The closer to 0 (completely horizontal), the greater the force, ranging from 1 - 2
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
		force = (AbsYPow * force) ;
		//Apply to moving speed (if uphill will be a negative/
		_Actions._listOfSpeedOnPaths[0] += force;
	}

	//Inputs
	public void HandleInputs () {
		if (!_Actions.isPaused) HandleUniqueInputs();

			//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
			_Actions.HandleInputs(_positionInActionList);
	}

	private void HandleUniqueInputs () {
		_pushTimer += Time.deltaTime;

		//Certain types of rail have unique controls / subactions to them.
		switch (_whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.rail:

				//Crouching, relevant to slope physics.
				_isCrouching = _Input.RollPressed;
				_CharacterAnimator.SetBool("isRolling", _isCrouching);

				//RailTrick to accelerate, but only after delay
				if (_Input.SpecialPressed && _pushTimer > _pushFowardDelay_)
				{	
					//Will only increase speed if under the max trick speed.
					if (_Actions._listOfSpeedOnPaths[0] < _pushFowardmaxSpeed_)
					{
						_Actions._listOfSpeedOnPaths[0] += _pushFowardIncrements_ * _accelBySpeed_.Evaluate(_Actions._listOfSpeedOnPaths[0] / _pushFowardmaxSpeed_); //Increae by flat increment, affected by current speed
					}
					_isFacingRight = !_isFacingRight; //This will cause the animator to perform a small hop and face the other way.
					_pushTimer = 0f; //Resets timer so delay must be exceeded again.
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
		_isBraking = _Input.BouncePressed;
	}

	private void CheckHopping () {
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

				//Once a step is over and the player hasn't started this action through collisions, exit state.
				if (_distanceToStep <= 0)
				{
					_Rail_int._isFollowingPath = false;
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
		_Rail_int._isFollowingPath = true;

		_ConnectedRails = AddOn; //Will be used to go onto subsequent rails without recalculating collisions.
	}

	//Called externally when entering a booster on a rail. Changes speed. 
	public IEnumerator ApplyBoost ( float speed, bool set, float addSpeed, bool backwards ) {
		//Rather than apply boost immediately, stretch it over three frames for smoothness and to ensure player proerly enters rail.
		for (int i = 0 ; i < 3 ; i++)
		{
			yield return new WaitForFixedUpdate();

			//Set means completely changing the speed to a specific value.
			if (set)
			{
				if (_Actions._listOfSpeedOnPaths[0] < speed)
				{
					_Actions._listOfSpeedOnPaths[0] = speed;
					_isBoosted = true;
					_boostTime = 0.9f; //How long the boost lasts before decaying.

				}
				else
					set = false; //If speed higher than what will be set, go through the other option instead.
			}
			//Keep checking if on a rail before applying this.
			if (_Actions._whatAction == S_Enums.PrimaryPlayerStates.Rail)
			{
				_Actions._listOfSpeedOnPaths[0] += addSpeed;
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
		_Actions = _Tools.GetComponent<S_ActionManager>();
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_Rail_int = _Tools.PathInteraction;
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_CamHandler = _Tools.CamHandler._HedgeCam;

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.MainSkin;
		_Sounds = _Tools.SoundControl;

		_JumpBall = _Tools.JumpBall;
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


