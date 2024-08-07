using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(S_Handler_WallActions))]
public class S_Action12_WallRunning : MonoBehaviour, IMainAction
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	[HideInInspector]	public  S_CharacterTools                _Tools;
	[HideInInspector]	public  S_PlayerPhysics                 _PlayerPhys;
	[HideInInspector]	public  S_PlayerInput                   _Input;
	[HideInInspector]	public  S_ActionManager                 _Actions;
	[HideInInspector]	public  S_Control_SoundsPlayer          _Sounds;
	[HideInInspector]	public  S_Handler_HomingAttack          _HomingControl;
	[HideInInspector]	public  S_Handler_Camera                _CamHandler;
	[HideInInspector]	public  S_Handler_WallActions           _WallHandler;

	[HideInInspector]	public  GameObject            _JumpBall;
	[HideInInspector]	public  GameObject            _DropShadow;
	[HideInInspector]	public  Transform             _CamTarget;
	[HideInInspector]	public  Transform             _ConstantTarget;
	[HideInInspector]	public  CapsuleCollider       _CoreCollider;
	[HideInInspector]	public  GameObject            _CurrentWall;
	[HideInInspector]	public  Animator              _CharacterAnimator;
	[HideInInspector]	public  Transform             _MainSkin;
	[HideInInspector]	public  Transform             _CharacterTransform;
	#endregion

	//Stats
	#region Stats
	[Header("Wall Stats")]
	[HideInInspector] public  float       _scrapeModi_ = 1f;
	[HideInInspector] public  Vector2       _wallCheckDistance_;
	[HideInInspector] public  LayerMask   _wallLayerMask_;
	[HideInInspector] public  float       _climbModi_;
	[HideInInspector] public float        _fallOffAtFallSpeed_;
	#endregion

	// Trackers
	#region trackers
	[HideInInspector]
	public  int         _positionInActionList;        //In every action script, takes note of where in the Action Managers Main action list this script is. 

	[HideInInspector]
	public Vector3      _originalVelocity;
	public float        _skinRotationSpeed;




	[Header("Wall Running")]
	[HideInInspector]
	public float        _runningSpeed;
	private bool        _isWallOnRight;


	[Header("Wall Rules")]
	[HideInInspector]
	public RaycastHit  _wallHit;
	[HideInInspector]   public Vector3      _raycastOrigin;
	[HideInInspector]	public  bool        _isHoldingWall;
	[HideInInspector]	public  float       _counter;
	[HideInInspector]	public  float       _distanceFromWall;
	[HideInInspector]   public float       _checkDistance;

	[HideInInspector]   public float       _currentClimbingSpeed;

	private Vector3     _wallForward;

	private int         _switchToJump;

	[HideInInspector]	public bool         _isWall;
	[HideInInspector]	public  Vector3     _previDirection;
	private Vector3     _previLocation;

	
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
		//Counter for how long on the wall
		_counter += Time.deltaTime;
	}

	private void FixedUpdate () {
		if (_isWall)
		{
			CheckCanceling();
			RunningInteraction();
			RunningPhysics();

			HandleInputs();
		}
	}

	public bool AttemptAction () {
		if (enabled) return false;

		if (this is S_Action15_WallClimbing)
		{
			_WallHandler._isScanningForClimb = true;
			if (_WallHandler.TryWallClimb()) { return true; }
		}
		else
		{
			_WallHandler._isScanningForRun = true;
			if (_WallHandler.TryWallRun()) { return true; }
		}

		return false;
	}

	//Due to requiring additional data for the wall, this is called in the SetUp methods, unlike AttemptAction.
	public void StartAction () {
		_isWall = true;
		_counter = 0;

		_distanceFromWall = _CoreCollider.radius * 1.25f;

		//Universal
		_DropShadow.SetActive(false);
		_JumpBall.SetActive(false);
		_Input.JumpPressed = false;
		_CamHandler._HedgeCam._shouldSetHeightWhenMoving_ = false;


		//Visual
		_Actions._ActionDefault.SwitchSkin(true);
		_DropShadow.SetActive(false);
		_CharacterAnimator.SetTrigger("ChangedState"); //This is the only animation change because if set to this in the air, should keep the apperance from other actions. The animator will only change when action is changed.
		
		//Physics
		_originalVelocity = _PlayerPhys._totalVelocity;
		_PlayerPhys.SetBothVelocities(Vector3.zero, Vector2.one);

		_PlayerPhys.SetIsGrounded(true); //This is to reset actions like JumpDash and Homing as if grounded
		_PlayerPhys.SetIsGrounded(false); //Will now be treated as not grounded until the action is over.
		_PlayerPhys._canChangeGrounded = false;

		//Control
		_PlayerPhys._listOfCanControl.Add(false);
		_PlayerPhys._isGravityOn = false;

		this.enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		//Wall fields
		_WallHandler._BannedWall = _CurrentWall;
		_isWall = false;

		//Universal
		_DropShadow.SetActive(true);
		_CamHandler._HedgeCam._shouldSetHeightWhenMoving_ = false;

		//Control
		_PlayerPhys._isGravityOn = true;
		_PlayerPhys._listOfCanControl.RemoveAt(0);
		_PlayerPhys._canChangeGrounded = true;
	}
	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	public void HandleInputs () {
		//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
		_Actions.HandleInputs(_positionInActionList);
	}

	private void RunningInteraction () {
		_Input.LockInputForAWhile(20f, false, Vector3.zero); //Locks input for half a second so any actions that end this don't have immediate control.

		//Get information of wall
		int wallDirection = _isWallOnRight ? 1: -1;
		Vector3 raycastOrigin = transform.position + _MainSkin.forward * 0.3f;
		_isWall = Physics.Raycast(raycastOrigin, _MainSkin.right * wallDirection, out _wallHit, _checkDistance, _wallLayerMask_);

		if (_counter > 0.3f)
		{
			_checkDistance = _wallCheckDistance_.y;
		}

		//Animator
		_PlayerPhys._currentRunningSpeed = _runningSpeed;
		_Actions._ActionDefault.HandleAnimator(12);
		_CharacterAnimator.SetBool("WallRight", _isWallOnRight);

		if (!_isWall || !_WallHandler.IsWallVerticalEnough(_wallHit.normal, 0.6f))
		{
			_Input.UnLockInput();
			_Actions._ActionDefault.StartAction();
		}
		else
		{
			_CamHandler._HedgeCam.GoBehindCharacter(3, 0, false);	
			_CurrentWall = _wallHit.collider.gameObject;
		}


		_Actions._ActionDefault.SetSkinRotationToVelocity(0, _wallForward, Vector2.zero, GetUpDirectionOfWall(_wallHit.normal));

		//For actions to exit the wall
		_Actions._jumpAngle = Vector3.Lerp(_wallHit.normal, Vector3.up, 0.4f);
		_Actions._dashAngle = Vector3.Lerp(_wallHit.normal, _wallForward, 0.5f);

		//If the wall stops being very steep
		//if (!_WallHandler.IsWallVerticalEnough(_wallHit.normal, 0.6f))
		//{
		//	_Input.UnLockInput();
		//	_Actions._ActionDefault.StartAction();
		//}
	}

	private void RunningPhysics () {
		Vector3 newVec = _wallForward * _runningSpeed;

		if (_isWall) //Incase RunningInteraction lost the wall
		{
			Vector3 wallNormal = _wallHit.normal;
			_wallForward = GetWallForward(_wallHit.normal);

			//Apply scraping speed
			newVec = new Vector3(newVec.x, -_currentClimbingSpeed, newVec.z);


			////Applying force against wall for when going round curves on the outside.
			//float forceToWall = 1f;
			//if (_runningSpeed > 100)
			//	forceToWall += _runningSpeed / 7;
			//else if (_runningSpeed > 150)
			//	forceToWall += _runningSpeed / 8;
			//else if (_runningSpeed > 200)
			//	forceToWall += _runningSpeed / 9;
			//else
			//	forceToWall += _runningSpeed / 10;

			////
			//newVec += forceToWall * -wallNormal;
			if (_counter < 0.3f)
				newVec += -wallNormal * Mathf.Max(5, _runningSpeed * 0.1f);
		}

		_PlayerPhys.SetCoreVelocity(newVec);
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 


	public void SetupRunning ( RaycastHit wallHit, bool wallRight ) {
		Vector3 wallDirection = wallHit.point - transform.position;
		_PlayerPhys.SetPlayerPosition(wallHit.point + (wallHit.normal * _distanceFromWall));

		//Wall values
		_wallHit = wallHit;
		_isWallOnRight = wallRight;

		_runningSpeed = _PlayerPhys._horizontalSpeedMagnitude;
		_currentClimbingSpeed = _PlayerPhys._totalVelocity.y * 0.6f;

		_checkDistance = Vector3.Distance(wallHit.point, transform.position) + 1; //Ensures first checks for x seconds will find the wall.

		//Visual
		_CharacterAnimator.SetInteger("Action", 14);
		_wallForward = GetWallForward(_wallHit.normal);

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.WallRunning); //Not part of startAction because other actions inherit that
		StartAction();

		////If running with the wall on the right
		//if (wallRight)
		//{
		//	_isWallOnRight = true;

		//	Vector3 wallForward = Vector3.Cross(wallHit.normal, transform.up);
		//	if ((_MainSkin.forward - wallForward).sqrMagnitude > (_MainSkin.forward - -wallForward).sqrMagnitude)
		//		wallForward = -wallForward;


		//	//Set direction facing
		//	_MainSkin.rotation = Quaternion.LookRotation(wallForward, transform.up);

		//}
		////If running with the wall on the left
		//else
		//{
		//	_isWallOnRight = false;
		//	Vector3 wallForward = Vector3.Cross(wallHit.normal, transform.up);
		//	if ((_MainSkin.forward - wallForward).sqrMagnitude > (_MainSkin.forward - -wallForward).sqrMagnitude)
		//		wallForward = -wallForward;

		//	//Set direction facing
		//	_MainSkin.rotation = Quaternion.LookRotation(wallForward, transform.up);
		//}
	}

	public void CheckCanceling () {

		_isHoldingWall = _WallHandler.IsInputtingToWall(_wallHit.point - _raycastOrigin);
		bool isOnGround = IsOnGround();

		//Cancel action by letting go of skid after .5 seconds
		if (!_isHoldingWall && _counter > 0.5f || isOnGround)
		{
			_Input.UnLockInput();
			_Actions._ActionDefault.StartAction();
		}
	}

	public Vector3 GetUpDirectionOfWall (Vector3 normal) {
		// Calculate direction upwards on the wall
		Vector3 right = Vector3.Cross(Vector3.up,normal).normalized;
		return Vector3.Cross(normal, right).normalized;
	}

	public Vector3 GetWallForward (Vector3 normal) {
		Vector3 forward = Vector3.Cross(normal, transform.up);

		if ((_MainSkin.forward - forward).sqrMagnitude > (_MainSkin.forward - -forward).sqrMagnitude)
			forward = -forward;
		return forward;
	}

	//Because canChangeGrounded is set to false on start, use own method when checking for ground, with own values to ensure doesn't count the current wall being climbed as ground.
	public bool IsOnGround () {
		if(_PlayerPhys.GetRelevantVector(_PlayerPhys._totalVelocity).y < -1) //Can only be grounded if going down wall (because wall climbing can transition to grounded seperately).
		{
			Vector3 rayCastStartPosition = transform.position + _wallHit.normal * 0.5f;
			return Physics.Raycast(rayCastStartPosition, -GetUpDirectionOfWall(_wallHit.normal), out RaycastHit hitGroundTemp, 7, _PlayerPhys._Groundmask_);
		}
		return false;
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
				if (this is S_Action15_WallClimbing)
				{
					if(_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.WallClimbing){
						_positionInActionList = i;
						break;
					}
				}
				else if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.WallRunning)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	//Responsible for assigning objects and components from the tools script.
	public void AssignTools () {
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools._ActionManager;
		_CamHandler = _Tools.CamHandler;
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_WallHandler = GetComponent<S_Handler_WallActions>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.MainSkin;
		_CharacterTransform = _Tools.CharacterModelOffset;
		_Sounds = _Tools.SoundControl;
		_JumpBall = _Tools.JumpBall;
		_DropShadow = _Tools.DropShadow;
		_CamTarget = _Tools.CameraTarget;
		_ConstantTarget = _Tools.ConstantTarget;
		_CoreCollider = _Tools.CharacterCapsule.GetComponent<CapsuleCollider>();
	}

	//Reponsible for assigning stats from the stats script.
	public void AssignStats () {

		_wallCheckDistance_ = _Tools.Stats.WallActionsStats.wallCheckDistance;
		_wallLayerMask_ = _Tools.Stats.WallActionsStats.WallLayerMask;
		_climbModi_ = _Tools.Stats.WallActionsStats.climbModifier;
		_scrapeModi_ = _Tools.Stats.WallActionsStats.scrapeModifier;
		_fallOffAtFallSpeed_ = _Tools.Stats.WallActionsStats.fallOffAtFallSpeed;
	}
	#endregion


	/// <summary>
	/// Other
	/// </summary>
	/// 

	private IEnumerator loseWall () {
		Vector3 newVec = _previDirection * _runningSpeed;
		yield return null;

		_MainSkin.forward = newVec.normalized;
		_PlayerPhys.SetCoreVelocity(newVec);
	}
}
