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
	[HideInInspector] public  float       _wallCheckDistance_;
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
	public RaycastHit  _wallHit;
	[HideInInspector]
	public float        _runningSpeed;
	private bool        _isWallOnRight;


	[Header("Wall Rules")]
	[HideInInspector]	public float       _currentClimbingSpeed;
	[HideInInspector]	public  bool        _isHoldingWall;
	[HideInInspector]	public  float       _counter;
	[HideInInspector]	public  float       _distanceFromWall;

	private int         _switchToJump;

	[HideInInspector]	public bool         _isWall;
	[HideInInspector]	public  Vector3     _previDir;
	private Vector3     _previLoc;

	[HideInInspector]	public Vector3      _jumpAngle;
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
		}
		else if (_switchToJump > 0)
		{
			JumpfromWall();
		}
	}

	public bool AttemptAction () {
		if (enabled) return false;

		_WallHandler._isScanningForRun = true;
		if (_WallHandler.TryWallRun()) { return true; }

		if (this is S_Action15_WallClimbing)
		{
			_WallHandler._isScanningForClimb = true;
			if (_WallHandler.TryWallClimb()) { return true; }
		}

		return false;
	}

	//Due to requiring additional data for the wall, this is called in the SetUp methods, unlike AttemptAction.
	public void StartAction () {
		_isWall = true;
		_counter = 0;

		_distanceFromWall = _CoreCollider.radius * 1.15f;

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

		Debug.Log("STOP WALL ACTION");
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
		//Prevents normal movement in input and physics
		_Input.LockInputForAWhile(3f, false, Vector3.zero);

		_CharacterAnimator.SetFloat("GroundSpeed", _runningSpeed);
		_CharacterAnimator.SetBool("WallRight", _isWallOnRight);

		//Detect current wall
		if (_isWallOnRight)
		{
			if (_counter < 0.3f)
				_isWall = Physics.Raycast(transform.position, _MainSkin.right, out _wallHit, _wallCheckDistance_ * 2.5f, _wallLayerMask_);
			else
			{
				_isWall = Physics.Raycast(transform.position, _MainSkin.right, out _wallHit, _wallCheckDistance_ * 1.6f, _wallLayerMask_);

				if (!_isWall)
				{
					Vector3 backPos = Vector3.Lerp(transform.position, _previLoc, 0.7f);
					_isWall = Physics.Raycast(backPos, _MainSkin.right, out _wallHit, _wallCheckDistance_ * 2.1f, _wallLayerMask_);
				}
			}
		}
		else
		{
			if (_counter < 0.3f)
				_isWall = Physics.Raycast(transform.position, -_MainSkin.right, out _wallHit, _wallCheckDistance_ * 2.5f, _wallLayerMask_);
			else
			{
				_isWall = Physics.Raycast(transform.position, -_MainSkin.right, out _wallHit, _wallCheckDistance_ * 1.6f, _wallLayerMask_);
				if (!_isWall)
				{
					Vector3 backPos = Vector3.Lerp(transform.position, _previLoc, 0.8f);
					_isWall = Physics.Raycast(backPos, -_MainSkin.right, out _wallHit, _wallCheckDistance_ * 2.1f, _wallLayerMask_);
				}
			}

		}

		if (!_isWall)
		{
			_CharacterAnimator.SetInteger("Action", 0);
			_CharacterAnimator.SetBool("Grounded", false);

			StartCoroutine(loseWall());

		}
		else
		{
			_CamHandler._HedgeCam.GoBehindCharacter(3, 0, false);
			_isHoldingWall = _WallHandler.IsInputtingToWall(_wallHit.point - transform.position);
			_CurrentWall = _wallHit.collider.gameObject;
		}


		//If jumping off wall
		if (_Input.JumpPressed)
		{
			_isWall = false;
			_PlayerPhys.transform.position = new Vector3(_wallHit.point.x + _wallHit.normal.x * 0.9f, _wallHit.point.y + _wallHit.normal.y * 0.5f, _wallHit.point.z + _wallHit.normal.z * 0.9f);
			//CharacterAnimator.transform.forward = Vector3.Lerp(CharacterAnimator.transform.forward, wallToRun.normal, 0.3f);

			//This bool causes the jump physics to be done next frame, making things much smoother. 2 Represents jumping from a wallrun
			_switchToJump = 2;
		}
	}

	private void RunningPhysics () {
		Vector3 wallNormal = _wallHit.normal;
		Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);


		if ((_MainSkin.forward - wallForward).sqrMagnitude > (_MainSkin.forward - -wallForward).sqrMagnitude)
			wallForward = -wallForward;

		_previDir = wallForward;
		_previLoc = transform.position;

		//Set direction facing
		_MainSkin.rotation = Quaternion.LookRotation(wallForward, transform.up);
		//characterTransform.rotation = Quaternion.LookRotation(wallForward, Vector3.Lerp(transform.up, wallNormal, 0.2f));


		//Decide speed to slide down wall.
		if (_currentClimbingSpeed > 10 && _currentClimbingSpeed < 20)
		{
			_currentClimbingSpeed *= (1.001f * _scrapeModi_);
		}
		else if (_currentClimbingSpeed > 29)
		{
			_currentClimbingSpeed *= (1.0015f * _scrapeModi_);
		}
		else if (_currentClimbingSpeed > 2)
		{
			_currentClimbingSpeed += (1.0018f * _scrapeModi_);
		}
		else
		{
			_currentClimbingSpeed += (1.002f * _scrapeModi_);
		}

		//Apply scraping speed
		Vector3 newVec = wallForward * _runningSpeed;
		newVec = new Vector3(newVec.x, -_currentClimbingSpeed, newVec.z);




		//Applying force against wall for when going round curves on the outside.
		float forceToWall = 1f;
		if (_runningSpeed > 100)
			forceToWall += _runningSpeed / 7;
		else if (_runningSpeed > 150)
			forceToWall += _runningSpeed / 8;
		else if (_runningSpeed > 200)
			forceToWall += _runningSpeed / 9;
		else
			forceToWall += _runningSpeed / 10;

		//
		newVec += forceToWall * -wallNormal;
		if (_counter < 0.3f)
			newVec += -wallNormal * 3;

		_PlayerPhys.SetCoreVelocity(newVec);

		//Debug.Log(scrapingSpeed);
		//Debug.Log(Player.p_rigidbody.velocity.y);
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	public void CheckCanceling () {

		Debug.Log(_PlayerPhys._canChangeGrounded);

		_isHoldingWall = _WallHandler.IsInputtingToWall(_wallHit.point - transform.position);

		//Cancel action by letting go of skid after .5 seconds
		if (!_isHoldingWall && _counter > 0.5f || _PlayerPhys._isGrounded)
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


	public void RunningSetup ( RaycastHit wallHit, bool wallRight ) {
		Vector3 wallDirection = wallHit.point - transform.position;
		_PlayerPhys._RB.AddForce(wallDirection * 10f);

		_PlayerPhys.SetPlayerPosition(wallHit.point + (wallHit.normal * _distanceFromWall));


		_wallHit = wallHit;

		_CharacterAnimator.SetInteger("Action", 14);
		//CharacterAnimator.SetBool("Grounded", true);

		_runningSpeed = _PlayerPhys._horizontalSpeedMagnitude;
		_currentClimbingSpeed = _PlayerPhys._RB.velocity.y * 0.7f;

		//If running with the wall on the right
		if (wallRight)
		{
			_isWallOnRight = true;
			//CharacterAnimator.transform.right = wallDirection.normalized;

			Vector3 wallForward = Vector3.Cross(wallHit.normal, transform.up);
			if ((_MainSkin.forward - wallForward).sqrMagnitude > (_MainSkin.forward - -wallForward).sqrMagnitude)
				wallForward = -wallForward;

			//Set direction facing
			_MainSkin.rotation = Quaternion.LookRotation(wallForward, transform.up);
			//characterTransform.rotation = Quaternion.LookRotation(wallForward, Vector3.Lerp(transform.up, wallHit.normal, 0.2f));
		}
		//If running with the wall on the left
		else
		{
			_isWallOnRight = false;
			//CharacterAnimator.transform.right = wallDirection.normalized;
			Vector3 wallForward = Vector3.Cross(wallHit.normal, transform.up);
			if ((_MainSkin.forward - wallForward).sqrMagnitude > (_MainSkin.forward - -wallForward).sqrMagnitude)
				wallForward = -wallForward;

			//Set direction facing
			_MainSkin.rotation = Quaternion.LookRotation(wallForward, transform.up);
			//characterTransform.rotation = Quaternion.LookRotation(wallForward, Vector3.Lerp(transform.up, wallHit.normal, 0.2f));
		}

		//Camera
		Vector3 newCamPos = _CamTarget.position + (wallHit.normal.normalized * 1.8f);
		newCamPos.y += 3f;
		_CamTarget.position = newCamPos;
		_CamHandler._HedgeCam.SetCamera(_MainSkin.forward, 2f, 0, 0.001f, 1.1f);
		//Cam._HedgeCam._cameraMaxDistance_ = Cam._initialDistance - 2f;

	}


	/// <summary>
	/// Other
	/// </summary>
	/// 

	private IEnumerator loseWall () {
		Vector3 newVec = _previDir * _runningSpeed;
		yield return null;

		_MainSkin.forward = newVec.normalized;
		_PlayerPhys.SetCoreVelocity(newVec);
	}

	void JumpfromWall () {
		Vector3 faceDir;

		if (_switchToJump == 2)
		{

			_jumpAngle = Vector3.Lerp(_wallHit.normal, transform.up, 0.8f);

			Vector3 wallNormal = _wallHit.normal;
			Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);


			if ((_MainSkin.forward - wallForward).sqrMagnitude > (_MainSkin.forward - -wallForward).sqrMagnitude)
				wallForward = -wallForward;


			Vector3 newVec = wallForward;


			//Debug.Log(jumpAngle);
			if (_isWallOnRight)
			{
				newVec = Vector3.Lerp(newVec, -_MainSkin.right, 0.25f);
				faceDir = Vector3.Lerp(newVec, -_MainSkin.right, 0.1f);
				newVec *= _runningSpeed;
			}
			else
			{
				newVec = Vector3.Lerp(newVec, _MainSkin.right, 0.25f);
				faceDir = Vector3.Lerp(newVec, _MainSkin.right, 0.1f);
				newVec *= _runningSpeed;
				//newVec += (CharacterAnimator.transform.right * 0.3f);
			}

			//CharacterAnimator.transform.forward = newVec.normalized;
			_PlayerPhys.SetCoreVelocity(newVec);

		}

		_switchToJump = 0;

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Jump);
	}
}
