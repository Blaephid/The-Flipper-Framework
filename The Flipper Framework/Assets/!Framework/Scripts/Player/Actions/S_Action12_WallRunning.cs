using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Action12_WallRunning : MonoBehaviour, IMainAction
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_CharacterTools		_Tools;
	private S_PlayerPhysics		_PlayerPhys;
	private S_PlayerInput		_Input;
	private S_ActionManager		_Actions;
	private S_Control_PlayerSound		_Sounds;
	private S_Handler_HomingAttack	_HomingControl;
	private S_Handler_Camera		_CamHandler;
	private S_Handler_WallRunning		_Control;

	private GameObject		_JumpBall;
	private GameObject		_DropShadow;
	private Transform		_CamTarget;
	private Transform		_ConstantTarget;
	private CapsuleCollider	_CoreCollider;
	private GameObject		_CurrentWall;
	private Animator		_CharacterAnimator;
	private Transform		_CharacterTransform;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	[Header("Wall Stats")]
	private float	_scrapeModi_ = 1f;
	private float	_climbModi_ = 1f;
	private float	_wallCheckDistance_;
	private LayerMask	_wallLayerMask_;
	#endregion

	// Trackers
	#region trackers
	private Vector3	_originalVelocity;
	public float	_skinRotationSpeed;

	[Header("Wall Climbing")]
	private bool	_isClimbing;
	private RaycastHit	_wallToClimb;
	[HideInInspector] 
	public float	_climbingSpeed;
	private float	_climbWallDistance;
	private float	_scrapingSpeed;
	private bool	_isSwitchingToGround;
	private float	_switchToJump = 0;

	[Header("Wall Running")]
	private bool	_isRunning;
	private RaycastHit	_wallToRun;
	[HideInInspector] 
	public float	_runningSpeed;
	private bool	_isWallOnRight;

	[Header("Wall Rules")]
	private bool	_isHoldingWall;
	private float	_counter;
	private float	_distanceFromWall;

	private bool	_isWall;
	private Vector3	_previDir;
	private Vector3	_previLoc;

	[HideInInspector]
	public Vector3      _jumpAngle;
	#endregion

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
		if (_PlayerPhys == null)
		{
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();
		}
	}
	private void OnDisable () {
		ExitWall(false);
	}

	// Update is called once per frame
	void Update () {
		//Counter for how long on the wall
		_counter += Time.deltaTime;

		//Debug.Log(ClimbingSpeed);

		if (_isWall)
		{
			if (_isRunning)
			{
				RunningInteraction();

			}

			else if (_isClimbing)
			{
				ClimbingInteraction();
			}
		}

	}

	private void FixedUpdate () {
		//Cancel action by letting go of skid after .5 seconds
		if ((!_isHoldingWall && _counter > 0.9f && (_isClimbing || _isRunning)) || _PlayerPhys._isGrounded)
		{
			if (_isRunning && !_PlayerPhys._isGrounded)
				StartCoroutine(loseWall());
			else
				ExitWall(true);
		}

		else if (_isWall)
		{
			//If Climbing
			if (_isClimbing)
			{
				ClimbingPhysics();
			}

			else if (_isRunning)
			{
				RunningPhysics();

			}

			//If going from climbing wall to running on flat ground normally.
			else if (_isSwitchingToGround)
			{
				FromWallToGround();
			}

		}
		else if (_switchToJump > 0)
		{
			JumpfromWall();
		}
	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		willChangeAction = true;
		return willChangeAction;
	}

	public void StartAction () {

	}

	public void StopAction () {

	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	public void HandleInputs () {

	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Actions = GetComponent<S_ActionManager>();
		_CamHandler = GetComponent<S_Handler_Camera>();
		_Input = GetComponent<S_PlayerInput>();
		_Control = GetComponent<S_Handler_WallRunning>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_CharacterTransform = _Tools.PlayerSkinTransform;
		_Sounds = _Tools.SoundControl;
		_JumpBall = _Tools.JumpBall;
		_DropShadow = _Tools.dropShadow;
		_CamTarget = _Tools.cameraTarget;
		_ConstantTarget = _Tools.constantTarget;
		_CoreCollider = _Tools.characterCapsule.GetComponent<CapsuleCollider>();
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_wallCheckDistance_ = _Tools.Stats.WallRunningStats.wallCheckDistance;
		_wallLayerMask_ = _Tools.Stats.WallRunningStats.WallLayerMask;

		_scrapeModi_ = _Tools.Stats.WallRunningStats.scrapeModifier;
		_climbModi_ = _Tools.Stats.WallRunningStats.climbModifier;
	}
	#endregion

	

	public void InitialEvents ( bool Climb, RaycastHit wallHit, bool wallRight, float frontDistance = 1f ) {
		_isWall = true;

		//Debug.Log("wallrunning");

		//Universal varaibles
		_isSwitchingToGround = false;
		_JumpBall.SetActive(false);

		_originalVelocity = _PlayerPhys._RB.velocity;
		_PlayerPhys.SetCoreVelocity(Vector3.zero);
		_distanceFromWall = _CoreCollider.radius * 1.15f;


		_counter = 0;
		_Input.JumpPressed = false;
		_PlayerPhys._isGravityOn = false;
		_CamHandler._HedgeCam._shouldSetHeightWhenMoving_ = false;

		//If entering a wallclimb
		if (Climb)
		{
			ClimbingSetup(wallHit, frontDistance);
		}

		//If wallrunning
		else
		{
			RunningSetup(wallHit, wallRight);
		}

	}

	bool inputtingToWall ( Vector3 wallDirection ) {
		Vector3 transformedInput;
		transformedInput = (_CharacterAnimator.transform.rotation * _Input._inputWithoutCamera);
		transformedInput = transform.InverseTransformDirection(transformedInput);
		transformedInput.y = 0.0f;
		//Debug.DrawRay(transform.position, transformedInput * 10, Color.red);

		if (_Input._camMoveInput.sqrMagnitude > 0.4f)
		{
			//Debug.Log(Vector3.Dot(wallDirection, Inp.trueMoveInput));
			if (Vector3.Dot(wallDirection.normalized, _Input._camMoveInput.normalized) > 0.05f)
			{
				return true;
			}
			else
			{
				if (Vector3.Dot(wallDirection.normalized, transformedInput.normalized) > 0.05f)
				{
					return true;
				}
			}
		}
		return false;
	}

	///
	/// Setup on wall
	/// 

	void ClimbingSetup ( RaycastHit wallHit, float frontDistance ) {
		//Set wall and type of movement
		_isClimbing = true;
		_isRunning = false;
		_wallToClimb = wallHit;
		_DropShadow.SetActive(false);

		//Set the climbing speed based on player's speed
		_climbingSpeed = _PlayerPhys._horizontalSpeedMagnitude * 0.8f;
		_climbingSpeed *= _climbModi_;
		_runningSpeed = 0f;

		//If moving up, increases climbing speed

		//Cam.Cam.SetCamera(-wallHit.normal, 2f, -30, 0.001f, 30);
		// Cam.Cam.CameraMaxDistance = Cam.InitialDistance - 3f;

		_scrapingSpeed = 5f;


		//Sets min and max climbing speed
		_climbingSpeed = 8f * (int)(_climbingSpeed / 8);
		_climbingSpeed = Mathf.Clamp(_climbingSpeed, 48, 176);

		_climbWallDistance = frontDistance;

		//Set animations
		_CharacterAnimator.SetInteger("Action", 1);
		//CharacterAnimator.SetBool("Grounded", true);
		_CharacterAnimator.transform.rotation = Quaternion.LookRotation(-_wallToClimb.normal, _CharacterAnimator.transform.up);
	}

	void RunningSetup ( RaycastHit wallHit, bool wallRight ) {
		Vector3 wallDirection = wallHit.point - transform.position;
		_PlayerPhys._RB.AddForce(wallDirection * 10f);

		transform.position = wallHit.point + (wallHit.normal * _distanceFromWall);

		_isRunning = true;
		_isClimbing = false;
		_wallToRun = wallHit;

		_CharacterAnimator.SetInteger("Action", 14);
		//CharacterAnimator.SetBool("Grounded", true);

		_climbingSpeed = 0f;
		_runningSpeed = _PlayerPhys._horizontalSpeedMagnitude;
		_scrapingSpeed = _PlayerPhys._RB.velocity.y * 0.7f;

		//If running with the wall on the right
		if (wallRight)
		{
			_isWallOnRight = true;
			//CharacterAnimator.transform.right = wallDirection.normalized;

			Vector3 wallForward = Vector3.Cross(wallHit.normal, transform.up);
			if ((_CharacterAnimator.transform.forward - wallForward).sqrMagnitude > (_CharacterAnimator.transform.forward - -wallForward).sqrMagnitude)
				wallForward = -wallForward;

			//Set direction facing
			_CharacterAnimator.transform.rotation = Quaternion.LookRotation(wallForward, transform.up);
			//characterTransform.rotation = Quaternion.LookRotation(wallForward, Vector3.Lerp(transform.up, wallHit.normal, 0.2f));
		}
		//If running with the wall on the left
		else
		{
			_isWallOnRight = false;
			//CharacterAnimator.transform.right = wallDirection.normalized;
			Vector3 wallForward = Vector3.Cross(wallHit.normal, transform.up);
			if ((_CharacterAnimator.transform.forward - wallForward).sqrMagnitude > (_CharacterAnimator.transform.forward - -wallForward).sqrMagnitude)
				wallForward = -wallForward;

			//Set direction facing
			_CharacterAnimator.transform.rotation = Quaternion.LookRotation(wallForward, transform.up);
			//characterTransform.rotation = Quaternion.LookRotation(wallForward, Vector3.Lerp(transform.up, wallHit.normal, 0.2f));
		}

		//Camera
		Vector3 newCamPos = _CamTarget.position + (wallHit.normal.normalized * 1.8f);
		newCamPos.y += 3f;
		_CamTarget.position = newCamPos;
		_CamHandler._HedgeCam.SetCamera(_CharacterAnimator.transform.forward, 2f, 0, 0.001f, 1.1f);
		//Cam._HedgeCam._cameraMaxDistance_ = Cam._initialDistance - 2f;

	}


	///
	/// Interacting with wall. Update.
	///

	void ClimbingInteraction () {
		//Prevents normal movement in input and physics
		_Input.LockInputForAWhile(0f, false);

		//Updates the status of the wall being climbed.
		if (_counter < 0.3f)
			_isWall = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), _CharacterAnimator.transform.forward, out _wallToClimb, _climbWallDistance * 1.3f, _wallLayerMask_);
		else
			_isWall = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), _CharacterAnimator.transform.forward, out _wallToClimb, 3f, _wallLayerMask_);

		//If they reach the top of the wall
		if (!_isWall)
		{
			Debug.Log("Lost Wall");
			Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), _CharacterAnimator.transform.forward * _climbWallDistance * 1.3f, Color.red, 20f);
			_CharacterAnimator.SetInteger("Action", 0);
			_CharacterAnimator.SetBool("Grounded", false);

			//Bounces the player up to keep momentum
			StartCoroutine(JumpOverWall(_CharacterAnimator.transform.rotation));

			//Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z);
			//CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, -Player.Gravity.normalized);
		}
		else
		{

			_isHoldingWall = inputtingToWall(_wallToClimb.point - transform.position);
			//Esnures the player faces the wall
			_CharacterAnimator.transform.rotation = Quaternion.LookRotation(-_wallToClimb.normal, _CharacterAnimator.transform.up);
			_previDir = _CharacterAnimator.transform.forward;
			_CurrentWall = _wallToClimb.collider.gameObject;
		}

		//If jumping off wall
		if (_Input.JumpPressed)
		{
			_isWall = false;

			transform.position = _wallToClimb.point + (_wallToClimb.normal * 4f);

			//This bool causes the jump physics to be done next frame, making things much smoother. 1 Represents jumping from a wallrun
			_switchToJump = 1;
			_isClimbing = false;
			_isRunning = false;
		}

	}

	void RunningInteraction () {
		//Prevents normal movement in input and physics
		_Input.LockInputForAWhile(0f, false);

		_CharacterAnimator.SetFloat("GroundSpeed", _runningSpeed);
		_CharacterAnimator.SetBool("WallRight", _isWallOnRight);

		//Detect current wall
		if (_isWallOnRight)
		{
			if (_counter < 0.3f)
				_isWall = Physics.Raycast(transform.position, _CharacterAnimator.transform.right, out _wallToRun, _wallCheckDistance_ * 2.5f, _wallLayerMask_);
			else
			{
				_isWall = Physics.Raycast(transform.position, _CharacterAnimator.transform.right, out _wallToRun, _wallCheckDistance_ * 1.6f, _wallLayerMask_);

				if (!_isWall)
				{
					Vector3 backPos = Vector3.Lerp(transform.position, _previLoc, 0.7f);
					_isWall = Physics.Raycast(backPos, _CharacterAnimator.transform.right, out _wallToRun, _wallCheckDistance_ * 2.1f, _wallLayerMask_);
				}
			}
		}
		else
		{
			if (_counter < 0.3f)
				_isWall = Physics.Raycast(transform.position, -_CharacterAnimator.transform.right, out _wallToRun, _wallCheckDistance_ * 2.5f, _wallLayerMask_);
			else
			{
				_isWall = Physics.Raycast(transform.position, -_CharacterAnimator.transform.right, out _wallToRun, _wallCheckDistance_ * 1.6f, _wallLayerMask_);
				if (!_isWall)
				{
					Vector3 backPos = Vector3.Lerp(transform.position, _previLoc, 0.8f);
					_isWall = Physics.Raycast(backPos, -_CharacterAnimator.transform.right, out _wallToRun, _wallCheckDistance_ * 2.1f, _wallLayerMask_);
				}
			}

		}

		if (!_isWall)
		{
			_CharacterAnimator.SetInteger("Action", 0);
			_CharacterAnimator.SetBool("Grounded", false);

			StartCoroutine(loseWall());

			//Debug.Log("Lost the Wall");
			if (_isWallOnRight)
				Debug.DrawRay(transform.position, _CharacterAnimator.transform.right * _wallCheckDistance_ * 2f, Color.blue, 20f);
			else
				Debug.DrawRay(transform.position, -_CharacterAnimator.transform.right * _wallCheckDistance_ * 2f, Color.blue, 20f);

		}
		else
		{
			_CamHandler._HedgeCam.GoBehindCharacter(3, 0, false);
			_isHoldingWall = inputtingToWall(_wallToRun.point - transform.position);
			_CurrentWall = _wallToRun.collider.gameObject;
		}


		//If jumping off wall
		if (_Input.JumpPressed)
		{
			_isWall = false;
			transform.position = new Vector3(_wallToRun.point.x + _wallToRun.normal.x * 0.9f, _wallToRun.point.y + _wallToRun.normal.y * 0.5f, _wallToRun.point.z + _wallToRun.normal.z * 0.9f);
			//CharacterAnimator.transform.forward = Vector3.Lerp(CharacterAnimator.transform.forward, wallToRun.normal, 0.3f);

			//This bool causes the jump physics to be done next frame, making things much smoother. 2 Represents jumping from a wallrun
			_switchToJump = 2;
			_isClimbing = false;
			_isRunning = false;
		}
	}

	/// <summary>
	/// Physics for climing and runing on wall
	/// </summary>
	void ClimbingPhysics () {
		//After a short pause / when climbing
		if (_counter > 0.15f)
		{

			//After being on the wall for too long.
			if (_climbingSpeed < -5f || Physics.Raycast(transform.position, _CharacterAnimator.transform.up, 5, _wallLayerMask_))
			{
				_CharacterAnimator.SetInteger("Action", 0);
				//Debug.Log("Out of Speed");

				//Drops and send the player back a bit.
				Vector3 newVec = new Vector3(0f, _climbingSpeed, 0f);
				newVec += (-_CharacterAnimator.transform.forward * 6f);
				_PlayerPhys.SetCoreVelocity(newVec);

				_CharacterAnimator.transform.rotation = Quaternion.LookRotation(-_wallToClimb.normal, Vector3.up);
				//Input.LockInputForAWhile(10f, true);

				ExitWall(true);
			}

			else
			{
				Vector3 newVec = new Vector3(0f, _climbingSpeed, 0f);
				newVec += (_CharacterAnimator.transform.forward * 20f);
				_PlayerPhys.SetCoreVelocity(newVec);
			}

			//Adds a changing deceleration
			if (_counter > 1.2)
				_climbingSpeed -= 2.5f;
			else if (_counter > 0.9)
				_climbingSpeed -= 2.0f;
			else if (_counter > 0.7)
				_climbingSpeed -= 1.5f;
			else if (_counter > 0.4)
				_climbingSpeed -= 1.0f;
			else
				_climbingSpeed -= 0.5f;


			//if (ClimbingSpeed < 0f)
			//{
			//    //Cam.Cam.FollowDirection(10f, 6f);

			//    //Decreases climbing speed decrease if climbing down.
			//    if (ClimbingSpeed < -40f)
			//        ClimbingSpeed += 1.2f;
			//    else if (ClimbingSpeed < -1f)
			//        ClimbingSpeed += .6f;
			//}


			//If the wall stops being very steep
			if (_wallToClimb.normal.y > 0.6 || _wallToClimb.normal.y < -0.3)
			{
				_CharacterAnimator.SetInteger("Action", 0);
				//Sets variables to go to swtich to ground option in FixedUpdate
				_isClimbing = false;
				_isRunning = false;
				_isSwitchingToGround = true;

				//Set rotation to put feet on ground.
				Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z), _CharacterAnimator.transform.forward, out _wallToClimb, _climbWallDistance, _wallLayerMask_);
				Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
				_CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, _wallToClimb.normal);
			}

		}

		//Adds a little delay before the climb, to attatch to wall more and add a flow
		else
		{
			Vector3 newVec = new Vector3(0f, _scrapingSpeed, 0f);
			if (_CharacterAnimator.transform.rotation == Quaternion.LookRotation(-_wallToClimb.normal, Vector3.up))
				newVec += (-_wallToClimb.normal * 45f);
			//else
			//    newVec = (wallToClimb.normal * 4f);

			//Decreases scraping Speed
			_scrapingSpeed *= 0.95f * _scrapeModi_;
			//ClimbingSpeed -= 0.1f;


			//Sets velocity
			_PlayerPhys.SetCoreVelocity(newVec);
		}
	}

	void FromWallToGround () {
		_PlayerPhys._isGravityOn = true;


		//Set rotation to put feet on ground.
		Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z), -_CharacterAnimator.transform.up, out _wallToClimb, _climbWallDistance, _wallLayerMask_);
		Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
		_CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, _wallToClimb.normal);

		//Set velocity to move along and push down to the ground
		Vector3 newVec = _CharacterAnimator.transform.forward * (_climbingSpeed);
		newVec += -_wallToClimb.normal * 10f;

		_PlayerPhys.SetCoreVelocity(newVec);

		//Actions.ChangeAction(0);
	}

	void RunningPhysics () {
		Vector3 wallNormal = _wallToRun.normal;
		Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);


		if ((_CharacterAnimator.transform.forward - wallForward).sqrMagnitude > (_CharacterAnimator.transform.forward - -wallForward).sqrMagnitude)
			wallForward = -wallForward;

		_previDir = wallForward;
		_previLoc = transform.position;

		//Set direction facing
		_CharacterAnimator.transform.rotation = Quaternion.LookRotation(wallForward, transform.up);
		//characterTransform.rotation = Quaternion.LookRotation(wallForward, Vector3.Lerp(transform.up, wallNormal, 0.2f));


		//Decide speed to slide down wall.
		if (_scrapingSpeed > 10 && _scrapingSpeed < 20)
		{
			_scrapingSpeed *= (1.001f * _scrapeModi_);
		}
		else if (_scrapingSpeed > 29)
		{
			_scrapingSpeed *= (1.0015f * _scrapeModi_);
		}
		else if (_scrapingSpeed > 2)
		{
			_scrapingSpeed += (1.0018f * _scrapeModi_);
		}
		else
		{
			_scrapingSpeed += (1.002f * _scrapeModi_);
		}

		//Apply scraping speed
		Vector3 newVec = wallForward * _runningSpeed;
		newVec = new Vector3(newVec.x, -_scrapingSpeed, newVec.z);




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


	/// <summary>
	/// Other
	/// </summary>
	/// 

	IEnumerator loseWall () {
		Vector3 newVec = _previDir * _runningSpeed;
		yield return null;

		_CharacterAnimator.transform.forward = newVec.normalized;
		_PlayerPhys.SetCoreVelocity(newVec);
		ExitWall(true);
	}

	void ExitWall ( bool immediately ) {
		_Control.bannedWall = _CurrentWall;

		//Actions.SkidPressed = false;

		_DropShadow.SetActive(true);
		//Cam._HedgeCam._cameraMaxDistance_ = Cam._initialDistance;
		_PlayerPhys._isGravityOn = true;
		_CamHandler._HedgeCam._shouldSetHeightWhenMoving_ = true;
		//camTarget.position = constantTarget.position;
		_CharacterAnimator.transform.rotation = Quaternion.identity;
		if (_previDir != Vector3.zero)
			_CharacterAnimator.transform.forward = _previDir;
		//characterTransform.up = CharacterAnimator.transform.up;

		_CharacterTransform.localEulerAngles = Vector3.zero;



		if (immediately && _Actions.whatAction != S_Enums.PrimaryPlayerStates.Jump)
			_Actions.ActionDefault.StartAction();
	}

	void JumpfromWall () {
		Vector3 faceDir;

		if (_switchToJump == 2)
		{

			_jumpAngle = Vector3.Lerp(_wallToRun.normal, transform.up, 0.8f);

			Vector3 wallNormal = _wallToRun.normal;
			Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);


			if ((_CharacterAnimator.transform.forward - wallForward).sqrMagnitude > (_CharacterAnimator.transform.forward - -wallForward).sqrMagnitude)
				wallForward = -wallForward;


			Vector3 newVec = wallForward;


			//Debug.Log(jumpAngle);
			if (_isWallOnRight)
			{
				newVec = Vector3.Lerp(newVec, -_CharacterAnimator.transform.right, 0.25f);
				faceDir = Vector3.Lerp(newVec, -_CharacterAnimator.transform.right, 0.1f);
				newVec *= _runningSpeed;
				//newVec += (-CharacterAnimator.transform.right * 0.3f);
			}
			else
			{
				newVec = Vector3.Lerp(newVec, _CharacterAnimator.transform.right, 0.25f);
				faceDir = Vector3.Lerp(newVec, _CharacterAnimator.transform.right, 0.1f);
				newVec *= _runningSpeed;
				//newVec += (CharacterAnimator.transform.right * 0.3f);
			}

			//CharacterAnimator.transform.forward = newVec.normalized;
			_PlayerPhys.SetCoreVelocity(newVec);

		}
		else
		{
			Debug.Log(Vector3.Dot(_wallToClimb.normal, _Input._camMoveInput));
			Debug.Log(_climbingSpeed);

			_jumpAngle = Vector3.Lerp(_wallToClimb.normal, transform.up, 0.6f);
			faceDir = _wallToClimb.normal;

			Debug.DrawRay(transform.position, faceDir, Color.red, 20);

			_PlayerPhys.SetCoreVelocity(faceDir * 4f);
		}

		_switchToJump = 0;
		ExitWall(false);

		_CharacterAnimator.transform.forward = faceDir;

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Jump);
	}

	IEnumerator JumpOverWall ( Quaternion originalRotation, float jumpOverCounter = 0 ) {
		float jumpSpeed = _PlayerPhys._RB.velocity.y * 0.6f;
		if (jumpSpeed < 5) jumpSpeed = 5;

		_PlayerPhys.SetCoreVelocity(_CharacterAnimator.transform.up * jumpSpeed);

		ExitWall(false);
		_Input.LockInputForAWhile(25f, false);

		while (true)
		{
			jumpOverCounter += 1;
			_CharacterAnimator.transform.rotation = originalRotation;
			yield return new WaitForSeconds(0.0f);
			if ((!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.6f, transform.position.z), _CharacterAnimator.transform.forward, out _wallToClimb, _climbWallDistance * 1.3f, _wallLayerMask_)) || jumpOverCounter == 40)
			{
				//Vector3 newVec = Player.p_rigidbody.velocity + CharacterAnimator.transform.forward * (ClimbingSpeed * 0.1f);
				_PlayerPhys.AddCoreVelocity(_CharacterAnimator.transform.forward * 8);
				if (_Input.RollPressed)
				{
					_Actions.Action08.TryDropCharge();
					break;
				}

				else
				{
					if (_Actions.whatAction != S_Enums.PrimaryPlayerStates.Jump)
						_Actions.ActionDefault.StartAction();
					break;
				}
			}

		}
	}
}
