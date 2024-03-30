using UnityEngine;
using System.Collections;

[RequireComponent(typeof(S_Handler_HomingAttack))]
public class S_Action02_Homing : MonoBehaviour, IMainAction
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_CharacterTools      _Tools;
	private S_PlayerPhysics       _PlayerPhys;
	private S_PlayerInput         _Input;
	private S_ActionManager       _Actions;
	private S_VolumeTrailRenderer  _HomingTrailScript;
	private S_Handler_HomingAttack _HomingHandler;
	private S_Control_SoundsPlayer  _Sounds;

	private GameObject            _HomingTrailContainer;
	private GameObject            _JumpBall;
	private Animator              _CharacterAnimator;
	private Transform             _Skin;
	[HideInInspector]
	public Transform              _Target;
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private bool        _CanBePerformedOnGround_;

	private float       _homingAttackSpeed_;
	private float       _homingTimerLimit_;
	private float       _homingTurnSpeed_;

	private bool        _canBeControlled_;
	private int         _homingSkidAngleStartPoint_;
	private float         _homingDeceleration_;
	private float         _homingAcceleration_;

	private int         _maxHomingSpeed_;
	private int         _minHomingSpeed_;

	private float       _homingBouncingPower_;
	private int         _minSpeedGainOnHit_;
	private float       _lerpToPreviousDirection_;
	private float       _lerpToNewInput_;

	private int         _homingCountLimit_;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;        //In every action script, takes note of where in the Action Managers Main action list this script is. 

	public float        _skinRotationSpeed = 7;

	private bool        _isHoming;                    //If currently homing. The action has unique interactions that will turn this off, disabling actual homing in on targets.

	[HideInInspector]
	public float       _speedBeforeAttack;           //The movement speed before performing this action.
	[HideInInspector]
	public Vector3     _directionBeforeAttack;       //The direction the player was moving before performing this action.
	private float       _currentSpeed;                //The speed the homing attack moves at this frame. If skid is a possible action, this can be changed.
	private float       _speedAtStart;                //The speed the homing attack happens at when performed, accelerating after decelerating will not exceed this.

	[HideInInspector]
	public Vector3      _targetDirection;             //Set at the start of the action to be used by other scripts on hit.
	private float       _distanceFromTarget;          //Updated each frame as certain movements will be edited when close to the target
	private Vector3     _currentDirection;            //Updated each frame to get the current direction
	private Vector3     _horizontalDirection;         //Same as above but without vertical
	private Vector3     _currentInput;
	private float       _inputAngle;                  //The angle difference between input and moving direction

	private float       _timer;
	private int         _homingCount;                 //Keeps track of how many homing attacks have been used before landing (or some more specific resets).
	#endregion

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyAction();
		_JumpBall.SetActive(false);
	}

	private void OnDisable () {
		_timer = 0;
		_HomingTrailContainer.transform.DetachChildren();

		//Removes one count of control being locked to counteract one being added when this action is start. This can also be done elsewhere but that will disable _isHoming
		if (_PlayerPhys._listOfCanControl.Count > 0)
		{
			_PlayerPhys._listOfCanControl.RemoveAt(0);
		}

		//if ended prematurely
		if(_isHoming)
		{
			_Actions.AddDashDelay(_HomingHandler._homingDelay_);
		}
	}

	// Update is called once per frame
	void Update () {

		if (_isHoming)
		{
			//Set Animator Parameters
			_Actions._ActionDefault.HandleAnimator(1);

			//Set Animation Angle
			_Actions._ActionDefault.SetSkinRotationToVelocity(_skinRotationSpeed);

			HandleInputs();
		}
	}

	private void FixedUpdate () {
		if (_isHoming)
		{
			HomeInOnTarget();
		}
	}

	//Called when checking if this action is to be performed, including inputs.
	public bool AttemptAction () {
		//Depending on stats, this can only be performed when grounded.
		if (!_PlayerPhys._isGrounded || _CanBePerformedOnGround_)
		{
			_HomingHandler._isScanning = true;

			//Must have a valid target when pressed
			if (_HomingHandler._TargetObject && _Input.HomingPressed)
			{
				//Homing attack must be currently allowed
				if (_Actions._isAirDashAvailables && (_homingCountLimit_ == 0 || _homingCountLimit_ > _homingCount))
				{
					StartAction();
					return true;
				}
			}
		}
		return false;
	}

	public void StartAction () {
		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Homing);
		this.enabled = true;

		ReadyAction();

		//Setting private
		_isHoming = true;
		_inputAngle = 0; //The difference between movement direction and input
		_homingCount++;

		_timer = 0;
		_speedBeforeAttack = _PlayerPhys._horizontalSpeedMagnitude; //Saved so it can be called back to on hit or end of action.
		_directionBeforeAttack = _PlayerPhys._RB.velocity.normalized;

		//Gets the direction to move in, rotate a lot faster than normal for the first frame.
		_Target = _HomingHandler._TargetObject.transform;
		_targetDirection = _Target.position - transform.position;
		_currentDirection = Vector3.RotateTowards(_Skin.forward, _targetDirection, Mathf.Deg2Rad * _homingTurnSpeed_ * 8, 0.0f);

		//Setting public
		_PlayerPhys._isGravityOn = false;
		_PlayerPhys._canBeGrounded = false;
		_PlayerPhys._canBeGrounded = false;
		_PlayerPhys.SetIsGrounded(false);
		_JumpBall.SetActive(false);
		_PlayerPhys._listOfCanControl.Add(false);
		_Input.JumpPressed = false;

		//Effects
		_CharacterAnimator.SetInteger("Action", 1);
		_CharacterAnimator.SetTrigger("ChangedState");
		_Sounds.HomingAttackSound();
		_HomingTrailScript.emitTime = _homingTimerLimit_ + 0.06f;
		_HomingTrailScript.emit = true;
		_Actions._ActionDefault.SwitchSkin(false);

		//Get speed of attack and speed to return to on hit.		
		_speedAtStart = Mathf.Max(_speedBeforeAttack * 0.9f, _homingAttackSpeed_);
		_speedAtStart = Mathf.Min(_speedAtStart, _maxHomingSpeed_);
		_currentSpeed = _speedAtStart;


		_speedBeforeAttack = Mathf.Max(_speedBeforeAttack, _minSpeedGainOnHit_);

	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.

		enabled = false;

		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		_PlayerPhys._canBeGrounded = true;
		_PlayerPhys._isGravityOn = true;
	}

	public void EventCollisionEnter ( Collision collision ) {
		if(!enabled) { return; }
		Debug.Log("Collision in homing");

		//If something is blocking the way, bounce off it.
		if (Physics.Linecast(transform.position, collision.contacts[0].point, out RaycastHit hit, _PlayerPhys._Groundmask_))
		{
			Debug.Log("Rebound");
			StartCoroutine(HittingObstacle(hit.normal));
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Handles player movement towards the target. Exits state if appropriate.
	private void HomeInOnTarget () {
		_timer += Time.deltaTime;

		//Ends homing attack if in air for too long or target is lost
		if ((_Target == null || _timer > _homingTimerLimit_))
		{
			_Actions._ActionDefault._animationAction = 0;
			_Actions._ActionDefault.StartAction();
			return;
		}

		//Get direction to move in.
		Vector3 newDirection = _Target.position - transform.position;
		_distanceFromTarget = Vector3.Distance(_Target.position, transform.position);
		float thisTurn =  _homingTurnSpeed_;

		//Set Player location when close enough, for precision.
		if (_distanceFromTarget < (_currentSpeed * Time.fixedDeltaTime))
		{
			_PlayerPhys.transform.position = _Target.transform.position;
			return;
		}
		//Turn faster when close to target and fast to make missing very hard.
		else
		{
			if (_distanceFromTarget < 40)
				thisTurn *= 2f;
			if (_currentSpeed > 90)
				thisTurn *= 1.3f;
		}


		//If there is input, then alter direction slightly to left or right.
		if (_PlayerPhys._moveInput.sqrMagnitude > 0.2f && _canBeControlled_ && _timer > 0.02f)
		{
			//Get horizontal input
			_currentInput =  transform.TransformDirection(_PlayerPhys._moveInput);
			_currentInput.y = 0;

			//Get current horizontal direction
			_horizontalDirection = newDirection;
			float rememberY = _horizontalDirection.y;
			_horizontalDirection.y = 0;

			_inputAngle = Vector3.Angle(_horizontalDirection, _currentInput);

			//Will only add control if input is not pointing directily behind character as that will lead to zigzagging
			if (_inputAngle < 130)
			{
				//Limit how different the input can be to the move direction (no more than x degrees).
				Vector3 useInput = Vector3.RotateTowards(_horizontalDirection, _currentInput, Mathf.Deg2Rad * 80, 0);

				//Get a horizontal direction between the two but don't change vertical.
				float percentageRelevantDif = Vector3.Angle(_horizontalDirection, useInput) * 0.8f;
				if (_distanceFromTarget < 40)
				{
					percentageRelevantDif *= 0.3f;
				}

				//A lerp would go through 0, while rotating by difference means it goes outwards without losing magnitude.
				Vector3 temp = Vector3.RotateTowards(_horizontalDirection, useInput, Mathf.Deg2Rad * percentageRelevantDif, 0);

				temp.y = rememberY;
				newDirection = temp.normalized;
			}
		}
		_currentDirection = Vector3.RotateTowards(_currentDirection, newDirection, Mathf.Deg2Rad * thisTurn, 0.0f);
		_PlayerPhys.SetCoreVelocity(_currentDirection * _currentSpeed);
	}

	public void HandleInputs () {

		//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
		_Actions.HandleInputs(_positionInActionList);	
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//What happens to the character after they hit a target, the directions they bounce based on input, stats and target.
	public void HittingTarget ( S_Enums.HomingRebounding whatRebound ) {
		_isHoming = false; //Prevents the rest of the code in Update and FixedUpdate from happening.
		_HomingHandler._TargetObject = null;
		_HomingHandler._PreviousTarget = null;

		//Effects
		_HomingTrailScript.emitTime = 0.1f;

		if (_Actions._jumpCount > 0) 
			_Actions._jumpCount = Mathf.Clamp(_Actions._jumpCount - 1, 1, _Actions._jumpCount); //Allows double jumping again after a hit

		_CharacterAnimator.SetInteger("Action", 1);

		_Actions.AddDashDelay(_HomingHandler._homingDelay_); //Add delay before it can be used again.
		Vector3 newSpeed = Vector3.zero;

		switch (whatRebound)
		{
			case S_Enums.HomingRebounding.BounceThrough:
				if (_Input.HomingPressed) { additiveHit(); }
				else { bounceUpHit(); }

				//Restore control and switch to default action
				_PlayerPhys.SetCoreVelocity(newSpeed);
				_PlayerPhys._isGravityOn = true;
				_Actions._ActionDefault._animationAction = 1;
				_Actions._ActionDefault.StartAction();

				break;
			case S_Enums.HomingRebounding.Rebound:
				StartCoroutine(HittingObstacle());
				return;
			case S_Enums.HomingRebounding.bounceOff:
				bounceUpHit();
				//Restore control and switch to default action
				_PlayerPhys.SetCoreVelocity(newSpeed);
				_PlayerPhys._isGravityOn = true;
				_Actions._ActionDefault._animationAction = 1;
				_Actions._ActionDefault.StartAction();
				break;
		}

		//An additive hit that keeps momentum
		void additiveHit () {
			//Disable inputs so actions don't happen immediately after hitting
			_Input.SpecialPressed = false;
			_Input.HomingPressed = false;

			GetDirectionPostHit();

			//Send player in new horizontal direction by speed before attack, but vertical speed is determined by bounce power.
			newSpeed.y = 0;
			newSpeed.Normalize();
			newSpeed *= Mathf.Min(_speedBeforeAttack, _currentSpeed);
			newSpeed.y = _homingBouncingPower_;

		}

		//Basic hit that only moves upwards, allowing time to aim at the next target.
		void bounceUpHit () {
			GetDirectionPostHit();
			newSpeed.y = 0;
			newSpeed.Normalize();
			newSpeed *= 3;
			newSpeed.y = _homingBouncingPower_; ;
		}

		void GetDirectionPostHit () {
			//Get current movement direction

			if (_PlayerPhys._moveInput.sqrMagnitude < 0.1)
			{
				newSpeed = _PlayerPhys._coreVelocity.normalized;
			}
			//If trying to move in the direction taken by the attack at the end, then will move that way
			else if (Vector3.Angle(_PlayerPhys._coreVelocity.normalized, _PlayerPhys._moveInput) / 180 < _lerpToNewInput_)
			{
				newSpeed = _PlayerPhys._coreVelocity.normalized;
			}
			//otherwise will move in previous direction.
			else
			{
				//Rotate towards previous direction by percentage
				float partDifference = Vector3.Angle(newSpeed, _directionBeforeAttack) * _lerpToPreviousDirection_;
				newSpeed = Vector3.RotateTowards(newSpeed, _directionBeforeAttack, partDifference * Mathf.Deg2Rad, 0);
			}
		}
	}

	//Applies knockback and a temporary locked state
	public IEnumerator HittingObstacle ( Vector3 wallNormal = default(Vector3), float force = 25 ) {

		float duration = 0.6f * 55;

		_PlayerPhys._canBeGrounded = true;

		//Gets a direction to make the player face and rebound away from. This is either the way they were already going, or slightly affected by what they hit.
		Vector3 faceDirection = _PlayerPhys._RB.velocity.normalized;
		if (wallNormal != default(Vector3))
		{
			faceDirection = Vector3.Lerp(faceDirection, wallNormal, 0.5f);
		}

		_PlayerPhys.SetTotalVelocity(Vector3.up * 2, new Vector2(1, 0), true);
		yield return new WaitForFixedUpdate();//For optimisation, freezes movement for a bit before applying the new physics.
		yield return new WaitForFixedUpdate();//For optimisation, freezes movement for a bit before applying the new physics.

		//Bounce backwards and upwards (halfway between the two).
		Vector3 ReboundDirection = new Vector3(-faceDirection.x, 0.8f, -faceDirection.z);
		_PlayerPhys.AddCoreVelocity(ReboundDirection * force, true);

		for (int i = 0 ; i < duration * 0.2f && !_PlayerPhys._isGrounded ; i++)
		{
			//Rotation
			_Skin.rotation = Quaternion.LookRotation(faceDirection, transform.up);

			yield return new WaitForFixedUpdate();
		}

		//Returns control partway through the rebound.
		_PlayerPhys._isGravityOn = true;
		this.enabled = false;

		for (int i = 0 ; i < duration * 0.8f && !_PlayerPhys._isGrounded ; i++)
		{
			//Rotation
			_Skin.rotation = Quaternion.LookRotation(faceDirection, transform.up);

			yield return new WaitForFixedUpdate();
		}

		_Actions._ActionDefault._animationAction = 1;
		_Actions._ActionDefault.StartAction();
	}

	//Called only by the skid subaction script, and only if this state is stet to have skidding as a subaction.
	public bool TryHomingSkid () {
		//Different start point from the other two skid types.
		if (_inputAngle > _homingSkidAngleStartPoint_ && !_Input._isInputLocked)
		{
			_currentSpeed -= _homingDeceleration_;
			_currentSpeed = Mathf.Clamp(_currentSpeed, Mathf.Max(_minHomingSpeed_, 20), _speedAtStart);
			return true;
		}
		else if (_inputAngle < 40 && !_Input._isInputLocked)
		{
			_currentSpeed += _homingAcceleration_;
			_currentSpeed = Mathf.Clamp(_currentSpeed, 0, _speedAtStart);
		}
		return false;
	}

	//This has to be set up in Editor. The invoker is in the PlayerPhysics script component, adding this event to it will mean this is called whenever the player lands.
	public void EventOnGrounded () {		
			_Actions._isAirDashAvailables = true;
			_homingCount = 0;		
	}


	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

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
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Homing)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools.GetComponent<S_ActionManager>();
		_HomingHandler = GetComponent<S_Handler_HomingAttack>();
		_Sounds = _Tools.SoundControl;
		_Skin = _Tools.MainSkin;

		_CharacterAnimator = _Tools.CharacterAnimator;
		_HomingTrailScript = _Tools.HomingTrailScript;
		_HomingTrailContainer = _Tools.HomingTrailContainer;
		_JumpBall = _Tools.JumpBall;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_homingAttackSpeed_ = _Tools.Stats.HomingStats.attackSpeed;
		_homingTimerLimit_ = _Tools.Stats.HomingStats.timerLimit;
		_CanBePerformedOnGround_ = _Tools.Stats.HomingStats.canBePerformedOnGround;
		_homingTurnSpeed_ = _Tools.Stats.HomingStats.turnSpeed;
		_homingSkidAngleStartPoint_ = _Tools.Stats.SkiddingStats.angleToPerformHomingSkid;
		_canBeControlled_ = _Tools.Stats.HomingStats.canBeControlled;
		_homingBouncingPower_ = _Tools.Stats.EnemyInteraction.homingBouncingPower;
		_minSpeedGainOnHit_ = _Tools.Stats.HomingStats.minimumSpeedOnHit;
		_lerpToPreviousDirection_ = _Tools.Stats.HomingStats.lerpToPreviousDirectionOnHit;
		_lerpToNewInput_ = _Tools.Stats.HomingStats.lerpToNewInputOnHit;
		_maxHomingSpeed_ = _Tools.Stats.HomingStats.maximumSpeed;
		_homingDeceleration_ = _Tools.Stats.HomingStats.deceleration;
		_homingAcceleration_ = _Tools.Stats.HomingStats.acceleration;
		_minHomingSpeed_ = _Tools.Stats.HomingStats.minimumSpeed;
		_homingCountLimit_ = _Tools.Stats.HomingStats.homingCountLimit;
	}
	#endregion


}
