using UnityEngine;
using System.Collections;
using UnityEngine.Windows;
using SplineMesh;

[RequireComponent(typeof(S_RailFollow_Base))]
public class S_Action05_Rail : S_Action_Base, IMainAction
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	//Scripts

	[HideInInspector]
	public S_Interaction_Pathers  _Rail_int;

	[HideInInspector] public S_RailFollow_Base _RF;

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
	private AnimationCurve          _forceBySlopeAngle_;
	private float                 _upHillMultiplier_ = 0.25f;
	private float                 _downHillMultiplier_ = 0.35f;
	private float                 _upHillMultiplierCrouching_ = 0.4f;
	private float                 _downHillMultiplierCrouching_ = 0.6f;

	private float                 _boostDecayTime_;
	private float                 _boostDecaySpeed_;

	private float                 _hopSpeed_ = 3.5f;
	private float                 _hopDelay_;
	private float                 _hopDistance_ = 12;
	private AnimationCurve          _HopSpeedByTime_;
	#endregion

	// Trackers
	#region trackers

	[HideInInspector]
	public bool         _canEnterRail = true;            //Prevents the start action method being called multiple times when on a rail. Must be set to false when leaving or starting a hop.
	private float                 _pulleyRotate;      //Set by inputs and incorperated into position and rotation on spline when using a zipline. Decides how much to tilt the handle and player.

	private float                 _pushTimer = 0f;    //Constantly goes up, is set to zzero after pushing forward. Implements the delay to prevent constant pushing.
	[HideInInspector]
	private bool                  _isCrouching;       //Set by input, will change slope calculations
	private bool                   _isBraking;        //Set by input, if true will decrease speed.


	//Stepping
	private bool        _canInput = true;   //Set true when entering a rail, but set false when rail hopping. Must be two to perform any actions.
	private bool        _canHop = false;    //Set false when entering a rail, but true after a moment.

	private float       _hopThisFrame;
	private float       _distanceToHop;    //Set when starting a hop and will go down by distance traveled every frame, ending action when zero.
	private float       _timeHopping;
	private float       _timeToCompleteHop;
	private bool        _isHoppingRight;   //Hopping to a rail on the right or on the left.

	private bool        _isFacingRight = true;        //Used by the animator, changed on push forward.

	//Boosters
	[HideInInspector]
	public bool         _isBoosted;
	[HideInInspector]
	public float        _boostTime;
	private bool            _firstBoosterApply;
	private float        _boostSpeedToSettle;
	private float        _boostSpeedAddOn;
	private float           _boostToDecay;

	[HideInInspector]
	public bool         _isGrinding; //USed to ensure no calculations are made from this still being active for possibly one frame called by Update when ending action.
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	public S_Action05_Rail () {
		_canEnterStateFromSelf = true;
	}

	// Start is called before the first frame update
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		if (!enabled || !_isGrinding || !_RF._RailTransform) { return; }
		ApplyHopUpdate();

		SoundControl();
		//Handle animations
		switch (_RF._whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.rail:
				_Actions._ActionDefault.HandleAnimator(10);
				_CharacterAnimator.SetBool("GrindRight", _isFacingRight);
				break;
			case S_Interaction_Pathers.PathTypes.zipline:
				_Actions._ActionDefault.HandleAnimator(9);
				break;
		}

		_RF.CustomUpdate();
		_RF.PlaceOnRail(SetRotation, SetPosition);

	}

	new private void FixedUpdate () {
		base.FixedUpdate();
		if (!enabled || !_isGrinding) { return; }

		_PlayerPhys._timeOnGround = 0;

		ApplyHopFixedUpdate();

		//This is to make the code easier to read, as a single variable name is easier than an element in a public list.
		if (_Actions._listOfSpeedOnPaths.Count > 0) { _RF._grindingSpeed = _Actions._listOfSpeedOnPaths[0]; }

		_RF.CustomFixedUpdate();
		_RF.PlaceOnRail(SetRotation, SetPosition);
		MoveOnRail();

		if (_canInput) { HandleInputs(); }

		if (_Actions._listOfSpeedOnPaths.Count > 0) { _Actions._listOfSpeedOnPaths[0] = _RF._grindingSpeed; }//Apples all changes to grind speed.
	}

	new public void StartAction ( bool overwrite = false ) {
		if (!_Actions._canChangeActions && !overwrite) { return; }

		_Sounds.RailGrindSound(true);

		_canEnterRail = false;

		//ignore further rail collisions
		Physics.IgnoreLayerCollision(this.gameObject.layer, 23, true);

		//Prevents rail hopping temporarily
		StartCoroutine(DelayHopOnLanding());

		//Set private 
		_isGrinding = true;
		_RF._isRailLost = false;
		_canInput = true;
		_pushTimer = _pushFowardDelay_;
		_distanceToHop = 0; //Ensure not immediately stepping when called

		_Rail_int._canGrindOnRail = false; //Prevents calling this multiple times in one update

		//Get how much the character is facing the same way as the point.
		_RF._PathSpline = _Rail_int._PathSpline;
		_RF.StartOnRail();
		_RF.GetNewSampleOnRail(0);

		//Use biased so horizontal velociity has slightly more say than vertical velocity. This 
		Vector3 biasedPlayerDirection = _PlayerVel._worldVelocity.normalized;
		float facingDot = 1;

		//The following won't be performed if already in the rail action, as this can be called when rail hopping as the action doesn't change.
		if (_Actions._whatCurrentAction != S_S_ActionHandling.PrimaryPlayerStates.Rail)
		{
			//Effects
			_Sounds.RailLandSound();

			biasedPlayerDirection.y = 0;
			biasedPlayerDirection.Normalize();
			biasedPlayerDirection = Vector3.Lerp(biasedPlayerDirection, _PlayerVel._worldVelocity.normalized.y * Vector3.up, 0.4f);


			facingDot = Vector3.Dot(biasedPlayerDirection, _RF._sampleTransforms.forwards); //Use sampleTransforms because _sampleForwards is affected by previous move direction.

			_RF._grindingSpeed = _PlayerVel._horizontalSpeedMagnitude;
			//What action before this one.
			switch (_Actions._whatCurrentAction)
			{
				// If it was a homing attack, the difference in facing should be by the direction moving BEFORE the attack was performed.
				case S_S_ActionHandling.PrimaryPlayerStates.Homing:
					facingDot = Vector3.Dot(GetComponent<S_Action02_Homing>()._directionBeforeAttack.normalized, _RF._sampleForwards);
					_RF._grindingSpeed = GetComponent<S_Action02_Homing>()._speedBeforeAttack;
					break;
				//If it was a drop charge, add speed from the charge to the grind speed.
				case S_S_ActionHandling.PrimaryPlayerStates.DropCharge:
					float charge = GetComponent<S_Action08_DropCharge>().GetCharge();
					_RF._grindingSpeed = Mathf.Clamp(charge, _RF._grindingSpeed + (charge / 6), 160);
					break;
				default:
					//If any other action, then check if speed on rail is gained from falling onto, or being launched up into.
					if (Mathf.Abs(_PlayerVel._worldVelocity.y) > _RF._grindingSpeed)
					{
						//If direction to grind is upwards, and player is being launched up.
						if (Mathf.Sign(_PlayerVel._worldVelocity.y) == 1 && (_RF._sampleUpwards * _RF._movingDirection).y > 0.5f)
						{
							_RF._grindingSpeed = Mathf.Abs(_PlayerVel._worldVelocity.y);
						}
						//If direction to grind is downwards enough, and player is falling down.
						else if (Mathf.Sign(_PlayerVel._worldVelocity.y) == -1 && (_RF._sampleUpwards * _RF._movingDirection).y < -0.5)
						{
							_RF._grindingSpeed = Mathf.Abs(_PlayerVel._worldVelocity.y);
						}
					}
					break;
			}

			_isCrouching = false;
			_pulleyRotate = 0f;

			_isBoosted = false;
			_boostTime = 0;

			//Set controls
			S_S_Logic.AddLockToList(ref _PlayerPhys._locksForIsGravityOn, "Rail");
			S_S_Logic.AddLockToList(ref _PlayerPhys._locksForCanControl, "Rail");
			_PlayerPhys._canChangeGrounded = false;

			_Input._JumpPressed = false;

			//Animator
			_Actions._ActionDefault.SwitchSkin(true);
			_CharacterAnimator.SetTrigger("ChangedState");
			switch (_RF._whatKindOfRail)
			{
				case S_Interaction_Pathers.PathTypes.rail:
					_CharacterAnimator.SetBool("GrindRight", _isFacingRight);   //Sets which direction the character animation is facing. Tracked between rails to hopping doesn't change it.
					_CharacterAnimator.SetInteger("Action", 10);
					break;
				case S_Interaction_Pathers.PathTypes.zipline:
					_RF._ZipHandle.GetComponentInChildren<MeshCollider>().enabled = false; //Ensures there won't be weird collisions along the zipline.
					_CharacterAnimator.SetInteger("Action", 9);
					break;
			}

			//If got onto this rail from anything except a rail hop, set speed to physics.

			// Apply minimum speed
			_RF._grindingSpeed = Mathf.Max(_RF._grindingSpeed, _minStartSpeed_);
			_Actions._listOfSpeedOnPaths.Add(_RF._grindingSpeed);

			_Actions.ChangeAction(S_S_ActionHandling.PrimaryPlayerStates.Rail);
			enabled = true;

			SetDirection();
		}
		else
		{
			facingDot = Vector3.Dot(biasedPlayerDirection, _RF._sampleTransforms.forwards); //Use sampleTransforms because _sampleForwards is affected by previous move direction.
			_RF._grindingSpeed = Mathf.Max(_RF._grindingSpeed, _minStartSpeed_);
			SetDirection();
		}

		return;
		void SetDirection () {
			// Get Direction for the Rail
			_RF._isGoingBackwards = facingDot < 0;
			_RF._movingDirection = facingDot < 0 ? -1 : 1;

			_PlayerVel.SetBothVelocities(_RF._sampleTransforms.forwards * _RF._movingDirection * _RF._grindingSpeed, new Vector2(1, 0));
			_RF.PlaceOnRail(SetRotation, SetPosition);
		}
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { ReadyAction(); return; } //First time is called on ActionManager Awake() to ensure this starts disabled and has a single opportunity to assign tools and stats.

		//Effects
		_Sounds.FeetSource.Stop();

		_isGrinding = false;
		_RF._RailTransform = null; //Set this to null so there's nothing to compare to on next rail.

		//If left this action to perform a jump,
		if (_Actions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.Jump)
		{
			switch (_RF._whatKindOfRail)
			{
				case S_Interaction_Pathers.PathTypes.zipline:
					_PlayerVel.SetCoreVelocity(_RF._sampleForwards * _RF._grindingSpeed, "Overwrite");  //Ensure player carries on momentum
					_PlayerPhys._groundNormal = Vector3.up; // Fix rotation

					//After a delay, restore zipline collisions and physics
					StartCoroutine(_Rail_int.JumpFromZipLine(_RF._ZipHandle, 1));
					_RF._ZipBody.isKinematic = true;
					break;
			}
		}

		S_S_Logic.RemoveLockFromList(ref _PlayerPhys._locksForIsGravityOn, "Rail");
		S_S_Logic.RemoveLockFromList(ref _PlayerPhys._locksForCanControl, "Rail");
		_PlayerPhys._canChangeGrounded = true;

		//To prevent instant actions
		_Input._RollPressed = false;
		_Input._SpecialPressed = false;
		_Input._BouncePressed = false;

		//If left, they would still be called if the player went from a zipline onto a rail as they wouldn't be overwritten.
		_RF._ZipBody = null;
		_RF._ZipHandle = null;

		_Actions._listOfSpeedOnPaths.RemoveAt(0); //Remove the speed that was used for this action. As a list because this stop action might be called after the other action's StartAction.

		StartCoroutine(DelayCollision());
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private


	public void SetRotation ( S_Interaction_Pathers.PathTypes pathType ) {

		switch (pathType)
		{
			case S_Interaction_Pathers.PathTypes.rail:
				_PlayerVel.transform.up = _RF._sampleUpwards;
				if (!_RF._isRailLost)
					_Actions._ActionDefault.SetSkinRotationToVelocity(_skinRotationSpeed, _RF._sampleForwards, default(Vector3), _RF._sampleUpwards);
				else
					_Actions._ActionDefault.SetSkinRotationToVelocity(0, _RF._sampleForwards, default(Vector3), _RF._sampleUpwards); //Turn is complete and isntant so player always ends facing the right direction.

				break;

			case S_Interaction_Pathers.PathTypes.zipline:
				//Set ziphandle rotation to follow sample
				_RF._ZipHandle.rotation = _RF._RailTransform.rotation * _RF._Sample.Rotation;
				//Since the handle and by extent the player can be tilted up to the sides (not changing forward direction), adjust the eueler angles to reflect this.
				//_pulleyRotate is handled in input, but applied here.
				_RF._ZipHandle.eulerAngles = new Vector3(_RF._ZipHandle.eulerAngles.x, _RF._ZipHandle.eulerAngles.y, _RF._ZipHandle.eulerAngles.z + _pulleyRotate * 70f * _RF._movingDirection);

				_Actions._ActionDefault.SetSkinRotationToVelocity(_skinRotationSpeed, _RF._sampleForwards);
				_MainSkin.eulerAngles = new Vector3(_MainSkin.eulerAngles.x, _MainSkin.eulerAngles.y, _pulleyRotate * 70f);

				break;
		}
	}

	public void SetPosition ( Vector3 position ) {

		_PlayerPhys.SetPlayerPosition(position);
	}

	//Takes the data from the previous method but handles physics for smoothing and applying if lost rail.
	public void MoveOnRail () {

		if (!_isGrinding) { return; }
		_PlayerPhys.SetIsGrounded(true, 0.5f);

		HandleRailSpeed(); //Make changes to player speed based on angle

		//Set Player Speed correctly so that it becomes smooth grinding
		_PlayerVel.SetBothVelocities(_RF._sampleForwards * _RF._grindingSpeed, new Vector2(1, 0));
		if (_RF._ZipBody) { _RF._ZipBody.velocity = _RF._sampleForwards * _RF._grindingSpeed; }

		if (_RF._isRailLost)
			LoseRail();

	}

	//Called when the player is at the end of a rail and being launched off.
	private void LoseRail () {

		_Input.LockInputForAWhile(5f, false, _RF._sampleForwards); //Prevent instant turning off the end of the rail
		StartCoroutine(_PlayerPhys.LockFunctionForTime(S_PlayerPhysics.EnumControlLimitations.canDecelerate, 0, "RailLost", 10));
		_distanceToHop = 0; //Stop a step that might be happening

		_isGrinding = false;

		switch (_RF._whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.zipline:

				//If the end of a zipline, then the handle must go flying off the end, so disable trigger for player and homing target, but renable collider with world.
				_RF._ZipHandle.GetComponent<CapsuleCollider>().enabled = false;
				if (_RF._ZipHandle.GetComponentInChildren<MeshCollider>()) { _RF._ZipHandle.GetComponentInChildren<MeshCollider>().enabled = false; }
				GameObject target = _RF._ZipHandle.transform.GetComponent<S_Control_Zipline>()._HomingTarget;
				target.SetActive(false);

				//_PlayerPhys.SetCoreVelocity(_ZipBody.velocity); //Make sure zip handle flies off

				_PlayerVel.SetCoreVelocity(_RF._sampleForwards * _RF._grindingSpeed); //Make sure player flies off the end of the rail consitantly.
				break;

			case S_Interaction_Pathers.PathTypes.rail:
				_PlayerVel.SetBothVelocities(_RF._sampleForwards * _RF._grindingSpeed, new Vector2(1, 0)); //Make sure player flies off the end of the rail consitantly.
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
		yield return new WaitForSeconds(0.55f);
		Physics.IgnoreLayerCollision(this.gameObject.layer, 23, false);
		_canEnterRail = true;
	}

	void HandleRailSpeed () {
		if (_isBraking && _RF._grindingSpeed > _minStartSpeed_)
		{
			_RF._grindingSpeed *= _playerBrakePower_;
		}

		if (IsOnSlope())
		{
			if (_RF._grindingSpeed < 5)
			{
				_RF._movingDirection *= -1;
				_RF._isGoingBackwards = !_RF._isGoingBackwards;
				_RF._grindingSpeed = 6;
			}
		}
		else if (_RF._grindingSpeed > _railTopSpeed_)
		{
			_RF._grindingSpeed -= _decaySpeed_;

		}

		HandleBoosterLogic();
	}

	//Set to true outside of this script. But when boosted on a rail will gain a bunch of speed at once before having some of it quickly drop off.
	void HandleBoosterLogic () {
		//If currently being boosted
		if (_isBoosted)
		{
			//When boost starts, boost time is set to positive, so when it goes below 0, decay starts setting in
			_boostTime -= Time.fixedDeltaTime;
			float boostSpeedTotal = _boostSpeedToSettle + _boostSpeedAddOn;

			if (!_firstBoosterApply && _RF._grindingSpeed != boostSpeedTotal)
			{
				float difference = _RF._grindingSpeed - boostSpeedTotal;
				_boostSpeedToSettle += difference;
				_boostSpeedToSettle = Mathf.Clamp(_boostSpeedToSettle, 0, Mathf.Min(_railmaxSpeed_, _PlayerPhys._PlayerMovement._currentMaxSpeed));

				if (difference > 0 && _RF._grindingSpeed < _railmaxSpeed_) //Booster add on speed won't be lost if speed is gained in its place from other sources.
					_boostToDecay = Mathf.Max(_boostToDecay - difference, 0);
			}

			//Start decayingSpeed
			if (_boostTime < 0)
			{
				//Boost to settle speed can't go over rail max speed, but the boostAdd on can, so that is what decays away. This allows the gained speed being lost over time.
				float loseThisFrame = _boostToDecay * Time.deltaTime;
				if (_boostSpeedToSettle > 60) { _boostSpeedAddOn -= loseThisFrame; } 

				if (_boostTime <= -_boostDecayTime_ || _boostSpeedAddOn <= 0)
				{
					_isBoosted = false;
					_boostTime = 0;
				}
			}

			boostSpeedTotal = _boostSpeedToSettle + _boostSpeedAddOn;
			_RF._grindingSpeed = boostSpeedTotal;

			_firstBoosterApply = false;
		}
		else
			//Decrease speed if over max or top speed on the rail.
			_RF._grindingSpeed = Mathf.Clamp(_RF._grindingSpeed, 0, Mathf.Min(_railmaxSpeed_, _PlayerPhys._PlayerMovement._currentMaxSpeed));
	}

	private bool IsOnSlope () {
		bool onSlope = false;

		//Start a force to apply based on the curve position and general modifier for all slopes handled in physics script 
		float force = _generalHillModifier;

		//Force affected by steepness of slope. The closer to 0 (completely horizontal), the greater the force.
		float slopeEffect = (1 - (Mathf.Abs(_RF._sampleUpwards.y) / 10)) + 1;
		slopeEffect = 1 - Mathf.Abs(_RF._sampleUpwards.y);
		slopeEffect = _forceBySlopeAngle_.Evaluate(Mathf.Abs(_RF._sampleUpwards.y));
		force *= slopeEffect;

		//use player vertical speed to find if player is going up or down
		//if going uphill on rail
		if (_PlayerVel._worldVelocity.y > 0.05f)
		{
			//Get main modifier and multiply by position on curve and general hill modifer used for other slope physics.
			force *= _isCrouching ? _upHillMultiplierCrouching_ : _upHillMultiplier_;
			force *= -1;
			onSlope = force < -Mathf.Min(0.3f, _RF._grindingSpeed / 100f);
		}
		else if (_PlayerVel._worldVelocity.y < -0.05f)
		{
			//Downhill
			force *= _isCrouching ? _downHillMultiplierCrouching_ : _downHillMultiplier_;
			onSlope = force > Mathf.Min(0.3f, _RF._grindingSpeed / 100f);
		}
		//Apply to moving speed (if uphill will be a negative/
		_RF._grindingSpeed += force;
		return onSlope;
	}

	public override void HandleInputs () {
		base.HandleInputs();
		if (!_Actions._isPaused) HandleUniqueInputs();
	}

	private void HandleUniqueInputs () {
		_pushTimer += Time.deltaTime;

		//Certain types of rail have unique controls / subactions to them.
		switch (_RF._whatKindOfRail)
		{
			case S_Interaction_Pathers.PathTypes.rail:

				//Crouching, relevant to slope physics.
				_isCrouching = _Input._RollPressed;
				_CharacterAnimator.SetBool("isRolling", _isCrouching);

				RailTrick();

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
		_isBraking = _Input._PowerPressed;
	}

	private void RailTrick () {
		//RailTrick to accelerate, but only after delay
		if (_Input._SpecialPressed && _pushTimer > _pushFowardDelay_)
		{
			//Will only increase speed if under the max trick speed.
			if (_RF._grindingSpeed < _pushFowardmaxSpeed_)
			{
				_RF._grindingSpeed += _pushFowardIncrements_ * _accelBySpeed_.Evaluate(_RF._grindingSpeed / _pushFowardmaxSpeed_); //Increae by flat increment, affected by current speed
			}
			SwapFacingSide();
		}
	}

	private void SwapFacingSide () {
		_isFacingRight = !_isFacingRight; //This will cause the animator to perform a small hop and face the other way.
		_Input._SpecialPressed = false; //Prevents it being spammed by holding	
		_pushTimer = 0f; //Resets timer so delay must be exceeded again.
	}

	private void CheckHopping () {
		if (_canInput && _canHop)
		{
			//Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
			Vector3 Direction = _MainSkin.position - _CamHandler._HedgeCam.transform.position;
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

			//If there is still an input, set the distance to step, which will be taken and handled in ApplyHop();
			if (_Input._RightStepPressed || _Input._LeftStepPressed)
			{
				StartHop();
			}
		}
	}

	private void StartHop () {
		_Sounds.FeetSource.Stop();
		_Sounds.QuickStepSound();

		_distanceToHop = _hopDistance_;
		_isHoppingRight = _Input._RightStepPressed; //Right step has priority over left
		_timeHopping = 0;
		_timeToCompleteHop = _hopDistance_ / _hopSpeed_;

		//Disable inputs until the hop is over
		_canInput = false;
		_Input._RightStepPressed = false;
		_Input._LeftStepPressed = false;

		SwapFacingSide();
	}

	private void ApplyHopUpdate () {
		//If this is set to over zero in checkHopping, then the player should be moved off the rail accordingly.
		if (_distanceToHop > 0)
		{
			_timeHopping += Time.deltaTime;

			//Get how far to move this frame and in which direction.
			_hopThisFrame = _hopSpeed_;
			_hopThisFrame *= _HopSpeedByTime_.Evaluate(_timeHopping / _timeToCompleteHop);

			_hopThisFrame *= Time.deltaTime;
			if (_isHoppingRight)
				_hopThisFrame = -_hopThisFrame;
			if (_RF._isGoingBackwards)
				_hopThisFrame = -_hopThisFrame;

			_hopThisFrame = Mathf.Clamp(_hopThisFrame, -_distanceToHop, _distanceToHop);

			//To show hopping off a rail, change the offset, this means the player will still follow the rail during the hop.
			_RF._setOffSet.Set(_RF._setOffSet.x + _hopThisFrame, _RF._setOffSet.y, _RF._setOffSet.z);

			//Decrease how far to move by how far has moved.
			_distanceToHop -= Mathf.Abs(_hopThisFrame);
			_distanceToHop = Mathf.Max(_distanceToHop, 0.1f);

			//Near the end of a step, renable collision so can collide again with grind on them instead.
			if (_distanceToHop < _hopDistance_ / 2 && !_canEnterRail)
			{
				Physics.IgnoreLayerCollision(this.gameObject.layer, 23, false);
				_canEnterRail = true;
			}
		}
	}

	private void ApplyHopFixedUpdate () {
		if (_distanceToHop > 0)
		{
			_PlayerVel.AddGeneralVelocity(_RF._sampleRight * ((_hopThisFrame * -1) / Time.deltaTime));

			//If moving, check for walls, and if there's a collision, end state.
			if (_hopThisFrame > 0)
			{
				if (Physics.BoxCast(transform.position, new Vector3(1.3f, 3f, 1.3f), -_MainSkin.right, Quaternion.identity, 4, _Tools.Stats.QuickstepStats.StepLayerMask))
				{
					_Actions._ActionDefault.StartAction();
				}
				else if (Physics.BoxCast(transform.position, new Vector3(1.3f, 3f, 1.3f), _MainSkin.right, Quaternion.identity, 4, _Tools.Stats.QuickstepStats.StepLayerMask))
				{
					_Actions._ActionDefault.StartAction();
				}
			}

			//Once a step is over and the player hasn't started this action through collisions, exit state.
			if (_distanceToHop <= 0.1f)
			{
				LoseRail();
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
		if (_distanceToHop == 0)
			_Sounds.RailGrindSound();
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public
	//Called by the pathers interaction script to ready important stats gained from the collision. This is seperate to startAction because startAction is inherited from an interface.
	public void AssignForThisGrind ( float range, Transform Rail, S_Interaction_Pathers.PathTypes type, Vector3 thisOffset, S_AddOnRail AddOn ) {


		_RF._setOffSet = -thisOffset; //Offset is obtianed from the offset on the collider, and will be followed consitantly to allow folowing different rails on the same spline.

		_RF._whatKindOfRail = type; //Zipline or rail

		//Setting up Rails
		//If same spline, landing from a hop, pre-existing PointOnSpline already works.
		if (!_RF._RailTransform || _RF._RailTransform != Rail)
		{
			_RF._pointOnSpline = range; //Starts at this position along the spline.
		}
		_RF._RailTransform = Rail; //Player position must add this as spline positions are in local space.

		_RF._ConnectedRails = AddOn; //Will be used to go onto subsequent rails without recalculating collisions.
	}

	//Called externally when entering a booster on a rail. Changes speed. 
	public IEnumerator ApplyBoosters ( S_Data_RailBooster BoosterLogic ) {
		//Rather than apply boost immediately, add a slight delay to give time to be set on rail before boost. If already on rail, no delay.
		for (int i = 0 ; i < 3 ; i++)
		{
			yield return new WaitForFixedUpdate();

			if (_Actions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.Rail)
			{

				_isBoosted = true;
				_boostTime = BoosterLogic._timeBeforeDecay; //How long the boost lasts before decaying.
				_firstBoosterApply = true;

				float newSpeed = BoosterLogic._NewSpeedByCurrentSpeed.Evaluate(_RF._grindingSpeed);
				_boostSpeedToSettle = newSpeed = Mathf.Clamp(newSpeed, 0, Mathf.Min(_railmaxSpeed_, _PlayerPhys._PlayerMovement._currentMaxSpeed));

				_boostSpeedAddOn = BoosterLogic._BoostAddOnByCurrentSpeed.Evaluate(_RF._grindingSpeed);

				_boostToDecay = _boostSpeedAddOn;

				//Changes which direction to grind in.
				_RF._isGoingBackwards = BoosterLogic._willSetBackwards_;

				StartCoroutine(_CamHandler._HedgeCam.ApplyCameraFallBack(BoosterLogic._cameraFallBack, BoosterLogic._cameraFallBack.z,
					_RF._grindingSpeed,_boostSpeedAddOn + _boostSpeedToSettle, 0.5f)); //The camera will fall back before catching up.

				break; //Since speed has now been applied, can end checking for if on rail.

			}
		}
	}
	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//Responsible for assigning objects and components from the tools script.
	public override void AssignTools () {
		base.AssignTools();

		_Rail_int = _Tools.PathInteraction;
		_RF = GetComponent<S_RailFollow_Base>();
	}

	//Reponsible for assigning stats from the stats script.
	public override void AssignStats () {
		_railTopSpeed_ = _Tools.Stats.RailStats.railTopSpeed;
		_railmaxSpeed_ = _Tools.Stats.RailStats.railMaxSpeed;
		_decaySpeed_ = _Tools.Stats.RailStats.railDecaySpeed;
		_minStartSpeed_ = _Tools.Stats.RailStats.minimumStartSpeed;
		_pushFowardmaxSpeed_ = _Tools.Stats.RailStats.RailPushFowardmaxSpeed;
		_pushFowardIncrements_ = _Tools.Stats.RailStats.RailPushFowardIncrements;
		_pushFowardDelay_ = _Tools.Stats.RailStats.RailPushFowardDelay;

		_forceBySlopeAngle_ = _Tools.Stats.SlopeStats.SlopePowerByAngle;
		_generalHillModifier = _Tools.Stats.RailStats.RailSlopePower;
		_upHillMultiplier_ = _Tools.Stats.RailStats.RailUpHillMultiplier.x;
		_downHillMultiplier_ = _Tools.Stats.RailStats.RailDownHillMultiplier.x;
		_upHillMultiplierCrouching_ = _Tools.Stats.RailStats.RailUpHillMultiplier.y;
		_downHillMultiplierCrouching_ = _Tools.Stats.RailStats.RailDownHillMultiplier.y;
		_playerBrakePower_ = _Tools.Stats.RailStats.RailPlayerBrakePower;
		_hopDelay_ = _Tools.Stats.RailStats.hopDelay;
		_hopSpeed_ = _Tools.Stats.RailStats.hopSpeed;
		_hopDistance_ = _Tools.Stats.RailStats.hopDistance;
		_HopSpeedByTime_ = _Tools.Stats.RailStats.HopSpeedByTime;
		_accelBySpeed_ = _Tools.Stats.RailStats.PushBySpeed;

		_offsetRail_ = _Tools.Stats.RailPosition.offsetRail;
		_offsetZip_ = _Tools.Stats.RailPosition.offsetZip;
		_RF._upOffsetRail_ = _offsetRail_;
		_RF._upOffsetZip_ = _offsetZip_;
		_boostDecaySpeed_ = _Tools.Stats.RailStats.railBoostDecaySpeed;
		_boostDecayTime_ = _Tools.Stats.RailStats.railBoostDecayTime;
	}
	#endregion

}


