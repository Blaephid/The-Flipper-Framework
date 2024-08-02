using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_WallRunning : MonoBehaviour
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

	private GameObject            _JumpBall;
	private S_Control_SoundsPlayer _Sounds;
	private Animator              _CharacterAnimator;
	private Transform             _MainSkin;

	private S_Action12_WallRunning _WallAction;

	[HideInInspector]
	public GameObject _BannedWall;

	#endregion



	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private float _wallCheckDistance_;
	private LayerMask _WallLayerMask_;
	#endregion

	// Trackers
	#region trackers

	[HideInInspector]
	public bool         _isScanning;
	private float       _checkModifier = 1;

	[HideInInspector]
	public float        _currentSpeed;
	private Vector3     _saveVelocity;

	private RaycastHit  _LeftWallHit;
	private bool        _isWallLeft;
	private RaycastHit  _RightWallHit;
	private bool        _isWallRight;
	private RaycastHit _FrontWallHit;
	private bool        _isWallFront;


	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {
		StartCoroutine(CheckForWalls());
	}

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyScript();
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	//Responsible for swtiching to wall run if specifications are met.
	private IEnumerator CheckForWalls () {
		while (true)
		{
			yield return new WaitForFixedUpdate();

			//If High enough above ground and in an action calling WallRunning's AttemptAction()
			if (_isScanning && IsEnoughAboveGround())
			{
				_currentSpeed = _PlayerPhys._horizontalSpeedMagnitude;
				_saveVelocity = _PlayerPhys._RB.velocity;

				//Has to be inputting at all, check if inputting towards a wall later.
				if (_Input._camMoveInput.sqrMagnitude > 0.8f)
				{
					//Checks for nearby walls using raycasts
					CheckForWall();

					//If detecting a wall in front with a near horizontal normal
					if (_isWallFront && IsWallVerticalEnough(_FrontWallHit.normal, 0.3f))
					{
						yield return new WaitForFixedUpdate();
						TryWallClimb();
					}

					//If detecting a wall to the side

					//If detecting a wall on left with correct angle.
					else if (_isWallLeft && _LeftWallHit.normal.y <= 0.4 && _currentSpeed > 38f &&
					    _LeftWallHit.normal.y >= -0.4)
					{
						yield return new WaitForFixedUpdate();
						tryWallRunLeft();

					}

					//If detecting a wall on right with correct angle.
					else if (_isWallRight && _RightWallHit.normal.y <= 0.4 && _currentSpeed > 38f &&
					    _RightWallHit.normal.y >= -0.4)
					{
						yield return new WaitForFixedUpdate();
						tryWallRunRight();
					}
				}
			}
			_isScanning = false; //Set to false every frame but will be counteracted in Action homing's AttemptAction()
		}
	}


	private void CheckForWall () {

		if (IsInputtingInCharacterAngle(_MainSkin.forward) && IsRunningFastEnough(30))
		{
			float distance = Mathf.Max(_wallCheckDistance_, _currentSpeed * Time.fixedDeltaTime + 2);
			//Checks for wall in front using raycasts, outputing hits and booleans
			_isWallFront = Physics.SphereCast(transform.position - _MainSkin.up * 0.4f, 2f, _MainSkin.forward, out _FrontWallHit, distance, _WallLayerMask_);
			Debug.DrawRay(transform.position - _MainSkin.up * 0.4f, _MainSkin.forward * distance, Color.red);

			//Checks if the wall can be used. Banned walls are set when the player jumps off the wall.
			_isWallFront = IsWallNotBanned(_FrontWallHit);
			Debug.Log(_isWallFront);
		}

		if (IsInputtingInCharacterAngle(_MainSkin.right) && IsRunningFastEnough(50))
		{
			//Checks for nearby walls using raycasts, outputing hits and booleans
			_isWallRight = Physics.SphereCast(_MainSkin.position, 3f, _MainSkin.right, out _RightWallHit, Mathf.Max(_wallCheckDistance_, _currentSpeed * Time.fixedDeltaTime + 1), _WallLayerMask_);
			_isWallRight = IsWallNotBanned(_RightWallHit);
		}

		if (IsInputtingInCharacterAngle(-_MainSkin.right) && IsRunningFastEnough(50))
		{
			_isWallLeft = Physics.SphereCast(_MainSkin.position, 3f, -_MainSkin.right, out _LeftWallHit, Mathf.Max(_wallCheckDistance_, _currentSpeed * Time.fixedDeltaTime + 1), _WallLayerMask_);
			_isWallLeft = IsWallNotBanned(_LeftWallHit);
		}

		////If no walls directily on sides, checks at angles with greater range.
		//if (!_isWallRight && !_isWallLeft && !_isWallFront)
		//{
		//	//Checks for wall on right first. Sets angle between right and forward and uses it.
		//	Vector3 direction = Vector3.Lerp(_MainSkin.right, _MainSkin.forward, 0.4f);
		//	_isWallRight = Physics.Raycast(_MainSkin.position, direction, out _RightWallHit, _wallCheckDistance_ * 2, _WallLayerMask_);

		//	//If no wall on right, checks left.
		//	if (!_isWallRight)
		//	{
		//		//Same as before but left
		//		direction = Vector3.Lerp(-_MainSkin.right, _MainSkin.forward, 0.4f);
		//		_isWallLeft = Physics.Raycast(_MainSkin.position, direction, out _LeftWallHit, _wallCheckDistance_ * 2, _WallLayerMask_);


		//		//If there isn't a wall and moving fast enough
		//		if (!_isWallLeft && !_isWallRight)
		//		{
		//			//Increases check range based on speed
		//			_checkModifier = (_PlayerPhys._horizontalSpeedMagnitude * 0.035f) + .5f;
		//			_isWallFront = Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), _MainSkin.forward, out _FrontWallHit,
		//			_wallCheckDistance_ * _checkModifier, _WallLayerMask_);

		//		}

		//	}

		//}


		//if (_isWallRight)
		//{
		//	if (_RightWallHit.collider.gameObject == _BannedWall)
		//		_isWallRight = false;
		//	else
		//	{
		//		Vector3 wallDirection = _RightWallHit.point - transform.position;
		//		//Debug.Log(Vector3.Dot(wallDirection.normalized, Inp.trueMoveInput.normalized));

		//		if (Vector3.Dot(wallDirection.normalized, _Input._camMoveInput.normalized) < 0.2f)
		//		{
		//			_isWallFront = false;
		//		}
		//	}
		//}
		//if (_isWallLeft)
		//{
		//	if (_LeftWallHit.collider.gameObject == _BannedWall)
		//		_isWallLeft = false;
		//	Vector3 wallDirection = _LeftWallHit.point - transform.position;

		//	if (Vector3.Dot(wallDirection.normalized, _Input._camMoveInput.normalized) < 0.2f)
		//	{
		//		_isWallFront = false;
		//	}
		//}

	}

	private bool IsEnoughAboveGround () {
		//If raycast does not detect ground
		if (!_PlayerPhys._isGrounded)
			return !Physics.Raycast(_MainSkin.position, -Vector3.up, 6f, _WallLayerMask_);
		else
			return false;
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 


	//This has to be set up in Editor. The invoker is in the PlayerPhysics script component, adding this event to it will mean this is called whenever the player lands.
	public void EventOnGrounded () {
		_BannedWall = null;
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//If not assigned already, sets the tools and stats and gets placement in Action Manager's action list.
	public void ReadyScript () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponentInParent<S_CharacterTools>();
			AssignTools();
			AssignStats();
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools._ActionManager;

		_WallAction = _Actions._ObjectForActions.GetComponent<S_Action12_WallRunning>();

		_MainSkin = _Tools.MainSkin;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
		_JumpBall = _Tools.JumpBall;

	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_WallLayerMask_ = _Tools.Stats.WallRunningStats.WallLayerMask;
		_wallCheckDistance_ = _Tools.Stats.WallRunningStats.wallCheckDistance;
	}
	#endregion

	private bool IsWallNotBanned ( RaycastHit HitWall ) {
		if (!HitWall.collider) { return false; }

		return HitWall.collider.gameObject == _BannedWall;
	}

	private bool IsInputtingInCharacterAngle ( Vector3 characterAngle ) {
		return Vector3.Angle(_Input._camMoveInput, characterAngle) < 90;
	}

	private bool IsRunningFastEnough ( float minSpeed = 30 ) {
		return _currentSpeed > minSpeed;
	}

	private bool IsWallVerticalEnough ( Vector3 normal, float limit = 0.3f ) {
		return Mathf.Abs(normal.y) < limit;
	}

	private bool IsInputtingTowardsWall ( Vector3 hitPoint ) {
		Vector3 directionToWall = hitPoint - transform.position;
		return Vector3.Angle(directionToWall.normalized, _Input._camMoveInput.normalized) < 20;
	}

	private bool IsFacingWallEnough ( Vector3 wallNormal ) {
		return Vector3.Angle(_MainSkin.forward, -wallNormal) < 20;
	}

	//If a wall is found in front, go through a number of checks to see if applicable
	public bool TryWallClimb () {
		//If detecting a wall in front with a near horizontal normal
		if (_isWallFront && IsWallVerticalEnough(_FrontWallHit.normal, 0.3f))
		{
			if (IsInputtingTowardsWall(_FrontWallHit.point))
			{
				if (IsFacingWallEnough(_FrontWallHit.normal))
				{
					//Enter wall run as a climb
					_WallAction.InitialEvents(true, _FrontWallHit, false, _wallCheckDistance_ * _checkModifier);
					_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.WallRunning);
					return true;
				}
			}
		}
		return false;
	}

	public bool tryWallRunLeft () {
		//If detecting a wall on left with correct angle.
		if (_isWallLeft && IsWallVerticalEnough(_LeftWallHit.normal, 0.4f))
		{
			float dis = Vector3.Distance(transform.position, _LeftWallHit.point);
			if (Physics.Raycast(transform.position, _Input._camMoveInput, dis + 0.1f, _WallLayerMask_))
			{
				//Debug.Log("Trigger Wall Left");
				//Enter a wallrun with wall on left.
				_WallAction.InitialEvents(false, _LeftWallHit, false);
				_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.WallRunning);
				return true;

			}
		}
		return false;
	}

	public bool tryWallRunRight () {
		//If detecting a wall on right with correct angle.
		if (_isWallRight && IsWallVerticalEnough(_RightWallHit.normal, 0.4f))
		{
			float dis = Vector3.Distance(transform.position, _RightWallHit.point);
			if (Physics.Raycast(transform.position, _Input._camMoveInput, dis + 0.1f, _WallLayerMask_))
			{
				//Debug.Log("Trigger Wall Right");
				//Enter a wallrun with wall on right.
				_WallAction.InitialEvents(false, _RightWallHit, true);
				_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.WallRunning);
				return true;
			}
		}
		return false;
	}



	private void OnCollisionEnter ( Collision collision ) {
		if (collision.collider.gameObject.layer == 0)
		{
			StartCoroutine(buffering());
		}
	}

	IEnumerator buffering () {
		Vector3 theVec = _saveVelocity;
		float theSpeed = _currentSpeed;

		for (int i = 0 ; i < 8 ; i++)
		{
			yield return new WaitForFixedUpdate();
			_saveVelocity = theVec;
			_currentSpeed = theSpeed;
		}
	}



}
