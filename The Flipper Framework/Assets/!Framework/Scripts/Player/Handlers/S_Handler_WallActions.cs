using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class S_Handler_WallActions : MonoBehaviour
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

	private S_Action12_WallRunning	_WallRunning;
	private S_Action15_WallClimbing	_WallClimbing;

	[HideInInspector]
	public GameObject _BannedWall;

	#endregion



	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private Vector2 _wallCheckDistance_;
	private LayerMask _WallLayerMask_;
	#endregion

	// Trackers
	#region trackers

	[HideInInspector]
	public bool         _isScanningForRun;
	[HideInInspector]
	public bool         _isScanningForClimb;

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

			//Set these to false so they're considered that way if not scanning, or later scans fail.
			_isWallFront = false;
			_isWallRight = false;
			_isWallLeft = false;

			//If High enough above ground and in an action calling WallRunning's AttemptAction()
			if (IsEnoughAboveGround())
			{
				_currentSpeed = _PlayerPhys._horizontalSpeedMagnitude;
				_saveVelocity = _PlayerPhys._RB.velocity;

				//Has to be inputting at all, check if inputting towards a wall later.
				if (_Input._constantInputRelevantToCharacter.sqrMagnitude > 0.8f)
				{
					//Checks for nearby walls using raycasts
					CheckForWall();
				}
			}
			_isScanningForRun = false; //Set to false every frame but will be counteracted in Action WallRunning's AttemptAction()
			_isScanningForClimb = false;

		}
	}


	private void CheckForWall () {
		Vector3 origin = transform.position - _MainSkin.up * 0.4f;

		if (_isScanningForClimb)
		{
			if (IsInputtingInCharacterAngle(_MainSkin.forward) && IsRunningFastEnough(40))
			{
				float distance = Mathf.Max(_wallCheckDistance_.x, _currentSpeed * Time.fixedDeltaTime + 2);

				//Checks for wall in front using raycasts, outputing hits and booleans
				_isWallFront = Physics.SphereCast(origin, 2f, _MainSkin.forward, out _FrontWallHit, distance, _WallLayerMask_);

				//Checks if the wall can be used. Banned walls are set when the player jumps off the wall.
				_isWallFront = IsWallNotBanned(_FrontWallHit);
			}
		}

		if (_isScanningForRun)
		{
			origin -= _MainSkin.forward * 0.2f;
			origin -= _MainSkin.right * 0.4f;

			float distance = Mathf.Max(_wallCheckDistance_.y, GetSpeedToTheSide()) + 0.4f;

			if (IsInputtingInCharacterAngle(_MainSkin.right) && IsRunningFastEnough(50))
			{
				Debug.DrawRay(origin, _MainSkin.right * distance, Color.red, 5f);
				//Checks for nearby walls using raycasts, outputing hits and booleans
				_isWallRight = Physics.SphereCast(origin, 2f, _MainSkin.right, out _RightWallHit, distance, _WallLayerMask_);
				_isWallRight = IsWallNotBanned(_RightWallHit);
			}

			else if (IsInputtingInCharacterAngle(-_MainSkin.right) && IsRunningFastEnough(50))
			{
				origin += _MainSkin.right * 0.8f;

				Debug.DrawRay(origin, -_MainSkin.right * distance, Color.red, 5f);

				_isWallLeft = Physics.SphereCast(origin, 2f, -_MainSkin.right, out _LeftWallHit, distance, _WallLayerMask_);
				_isWallLeft = IsWallNotBanned(_LeftWallHit);
			}
		}
	}

	private bool IsEnoughAboveGround () {
		//If raycast does not detect ground
		if (!_PlayerPhys._isGrounded)
			return !Physics.Raycast(_MainSkin.position, -Vector3.up, 6f, _WallLayerMask_);
		return false;
	}

	private float GetSpeedToTheSide () {
		Vector3 releventVelocity = _PlayerPhys.GetRelevantVector(_PlayerPhys._totalVelocity, false);
		return Mathf.Abs(releventVelocity.x * Time.deltaTime * 1.5f);
	}

	private bool IsWallNotBanned ( RaycastHit HitWall ) {
		if (!HitWall.collider) { return false; }

		return HitWall.collider.gameObject != _BannedWall;
	}

	private bool IsInputtingInCharacterAngle ( Vector3 characterAngle ) {
		
		Debug.DrawRay(transform.position, _Input._constantInputRelevantToCharacter, Color.green, 10f);
		Debug.DrawRay(transform.position, _Input._camMoveInput, Color.blue, 10f);
		return Vector3.Angle(_Input._constantInputRelevantToCharacter, characterAngle) < 90;
	}

	private bool IsRunningFastEnough ( float minSpeed = 30 ) {
		return _currentSpeed > minSpeed;
	}

	public bool IsWallVerticalEnough ( Vector3 normal, float limit = 0.3f ) {
		return Mathf.Abs(normal.y) < limit;
	}

	private bool IsInputtingTowardsWall ( Vector3 hitPoint, float angleLimit = 20 ) {
		if(_Input._constantInputRelevantToCharacter.sqrMagnitude < 0.5) { return false; }

		Vector3 directionToWall = hitPoint - transform.position;
		return Vector3.Angle(directionToWall.normalized, _Input._constantInputRelevantToCharacter) < angleLimit;
	}

	private bool IsFacingWallEnough ( Vector3 wallNormal ) {
		return Vector3.Angle(_MainSkin.forward, -wallNormal) < 20;
	}

	#endregion
	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

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
					_WallClimbing.SetupClimbing(_FrontWallHit);
					return true;
				}
			}
		}
		return false;
	}

	public bool TryWallRun () {
		//If has detected a wall on one of the sides
		if(_isWallLeft || _isWallRight)
		{
			//For less lines, set the hit to be used, prioritising the right side than the left
			RaycastHit RelevantHit = _isWallRight ? _RightWallHit : _LeftWallHit;

  			if (IsWallVerticalEnough(RelevantHit.normal, 0.4f))
			{
 				if (IsInputtingTowardsWall(RelevantHit.point, 75))
				{
					_WallRunning.SetupRunning(RelevantHit, _isWallRight);
					return true;
				}
			}
		}

		////If detecting a wall on left with correct angle.
		//if (_isWallLeft && IsWallVerticalEnough(_LeftWallHit.normal, 0.4f))
		//{
		//	float dis = Vector3.Distance(transform.position, _LeftWallHit.point);
		//	if (Physics.Raycast(transform.position, _Input._camMoveInput, dis + 0.1f, _WallLayerMask_))
		//	{
		//		//Debug.Log("Trigger Wall Left");
		//		//Enter a wallrun with wall on left.
		//		_WallAction.InitialEvents(false, _LeftWallHit, false);
		//		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.WallRunning);
		//		return true;

		//	}
		//}
		return false;
	}

	public bool IsInputtingToWall ( Vector3 directionToWall ) {
		Debug.DrawRay(transform.position, _Input._constantInputRelevantToCharacter, Color.cyan, 5f);

		if(_Input._constantInputRelevantToCharacter.sqrMagnitude > 0.5f)
		{
			directionToWall.y = 0;
			directionToWall.Normalize();

			return Vector3.Angle(_Input._constantInputRelevantToCharacter, directionToWall) < 90;
		}
		return false;
	}

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

		_WallRunning = _Actions._ObjectForActions.GetComponent<S_Action12_WallRunning>();
		_WallClimbing = _Actions._ObjectForActions.GetComponent<S_Action15_WallClimbing>();

		_MainSkin = _Tools.MainSkin;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
		_JumpBall = _Tools.JumpBall;

	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_WallLayerMask_ = _Tools.Stats.WallActionsStats.WallLayerMask;
		_wallCheckDistance_ = _Tools.Stats.WallActionsStats.wallCheckDistance;
	}
	#endregion


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
