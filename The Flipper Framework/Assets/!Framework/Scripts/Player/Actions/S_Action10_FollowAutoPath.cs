using UnityEngine;
using System.Collections;
using SplineMesh;

public class S_Action10_FollowAutoPath : MonoBehaviour, IMainAction
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
	private S_Control_SoundsPlayer _Sounds;
	private S_Interaction_Pathers _Pathers;
	private S_HedgeCamera	_CamHandler;

	private CurveSample _Sample;
	private Animator	_CharacterAnimator;
	[HideInInspector]
	public Collider	_PatherStarter;
	private Transform	_MainSkin;
	private Transform	_PathTransform;
	#endregion



	//Stats
	#region Stats
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;

	private Vector3	_OGSkinLocPos;

	public float	_skinRotationSpeed;
	public Vector3	_skinOffsetPos = new Vector3(0, -0.4f, 0);
	public float	_offset = 2.05f;

	private Quaternion	_charRot;

	// Setting up Values
	private float	_pointOnSpline = 0f;
	[HideInInspector]
	public float	_playerSpeed;
	private float	_pathTopSpeed;
	private bool	_isGoingBackwards;
	private int         _moveDirection;

	//Camera testing
	public float	_targetDistance = 10;
	public float	_cameraLerp = 10;

	[HideInInspector] 
	public float	_originalRayToGroundRot;
	[HideInInspector] 
	public float	_originalRayToGround;

	private Vector3     _sampleUpwards;
	private Vector3     _sampleForwards;
	private Vector3     _sampleLocation;
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
		_OGSkinLocPos = _MainSkin.transform.localPosition;
	}

	// Update is called once per frame
	void Update () {

	}

	private void FixedUpdate () {
		_Input.LockInputForAWhile(1f, true, Vector3.zero);

		//This is to make the code easier to read, as a single variable name is easier than an element in a public list.
		if (_Actions._listOfSpeedOnPaths.Count > 0) { _playerSpeed = _Actions._listOfSpeedOnPaths[0]; } 
		MoveAlongPath();
		if (_Actions._listOfSpeedOnPaths.Count > 0) { _Actions._listOfSpeedOnPaths[0] = _playerSpeed; }//Apples all changes to grind speed.
	}

	public bool AttemptAction () {		
		_Pathers._canEnterAutoPath = true;
		return false;
	}

	public void StartAction () {
		_PlayerPhys._arePhysicsOn = false;

		_MainSkin.transform.localPosition = _MainSkin.transform.localPosition;

		if (transform.eulerAngles.y < -89)
		{
			_PlayerPhys.transform.eulerAngles = new Vector3(0, -89, 0);
		}

		_CharacterAnimator.SetBool("Grounded", true);

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Path);
		enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		_PlayerPhys._arePhysicsOn = true;

		if (_MainSkin != null)
		{
			_MainSkin.transform.localPosition = _OGSkinLocPos;
			_MainSkin.localRotation = Quaternion.identity;
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	private void SetToPath () {

		_PlayerPhys.AlignToGround(_PlayerPhys._groundNormal, _PlayerPhys._isGrounded);
	}

	private void MoveAlongPath () {

		GetNewPointOnSpline();

		PlaceOnSpline();
		//if ((Physics.Raycast(splinePoint + (_sampleUpwards * 2), -transform.up, out RaycastHit hitRot, 2.2f + _Tools.Stats.FindingGround.rayToGroundDistance, _PlayerPhys._Groundmask_)))
		//{
		//	Vector3 FootPos = transform.position - _Pathers._FeetTransform.position;
		//	_PlayerPhys.SetPlayerPosition(((_Sample.location) + _PathTransform.position) + FootPos);
		//}
		//else
		//{
		//	Vector3 FootPos = transform.position - _Pathers._FeetTransform.position;
		//	_PlayerPhys.SetPlayerPosition(((_Sample.location) + _PathTransform.position) + FootPos);
		//}
		_PlayerPhys.CheckForGround();
		_PlayerPhys.AlignToGround(_PlayerPhys._groundNormal, _PlayerPhys._isGrounded);

		float inputDirection = Mathf.Sign(Vector3.Dot(_sampleForwards, _PlayerPhys._moveInput));
		_PlayerPhys._moveInput = _sampleForwards * inputDirection;

		Vector3 physCoreVelocity = _PlayerPhys.HandleControlledVelocity(Vector2.one);
		if (_PlayerPhys._isGrounded)
		{
			physCoreVelocity = _PlayerPhys.StickToGround(physCoreVelocity);
			physCoreVelocity = _PlayerPhys.HandleSlopePhysics(physCoreVelocity);
		}
		_playerSpeed = physCoreVelocity.magnitude;

		_PlayerPhys.SetBothVelocities(_sampleForwards * _moveDirection * _playerSpeed, Vector2.right);

		if (_pointOnSpline == _Pathers._PathSpline.Length || _pointOnSpline == 0)
		{
				ExitPath();
		}
	}

	private void GetNewPointOnSpline () {
		//Increase the Amount of distance through the Spline by DeltaTime
		float travelAmount = (Time.deltaTime * _playerSpeed);
		_moveDirection = _isGoingBackwards ? -1 : 1;

		// Increase/Decrease Range depending on direction
		_pointOnSpline += travelAmount * _moveDirection;
		_pointOnSpline = Mathf.Clamp(_pointOnSpline, 0, _Pathers._PathSpline.Length);

		//Get Sample of the Path to put player
		_Sample = _Pathers._PathSpline.GetSampleAtDistance(_pointOnSpline);
		_sampleForwards = _Sample.tangent;
		_sampleUpwards = _Sample.up;
		_sampleLocation = _Sample.location + _Pathers._PathSpline.transform.position;
	}

	private void PlaceOnSpline () {
		//Place at position so feet are on the spline, setting to ground if close to sample.
		Vector3 FootPos = transform.position - _Pathers._FeetTransform.position;
		_PlayerPhys.SetPlayerPosition(_sampleLocation + FootPos);
		_PlayerPhys.SetPlayerRotation(Quaternion.LookRotation(_sampleForwards, _sampleUpwards));
	}

	private void ExitPath () {


		_PlayerPhys.AlignToGround(_PlayerPhys._groundNormal, _PlayerPhys._isGrounded);

		//Set Player Speed correctly for smoothness
		if (!_isGoingBackwards)
		{
			_Sample = _Pathers._PathSpline.GetSampleAtDistance(_Pathers._PathSpline.Length);
			_PlayerPhys._RB.velocity = _Sample.tangent * (_playerSpeed);

		}
		else
		{
			_Sample = _Pathers._PathSpline.GetSampleAtDistance(0);
			_PlayerPhys._RB.velocity = -_Sample.tangent * (_playerSpeed);

		}

		_MainSkin.rotation = Quaternion.LookRotation(_PlayerPhys._RB.velocity, _PlayerPhys._HitGround.normal);

		_CamHandler.SetBehind(0);
		_Input.LockInputForAWhile(30f, true, _MainSkin.forward);

		//Deactivates any cinemachine that might be attached.
		if (_Pathers._currentExternalCamera != null)
		{
			_Pathers._currentExternalCamera.DeactivateCam(18);
			_Pathers._currentExternalCamera = null;
		}

		_Actions._ActionDefault.StartAction();
	}

	public void HandleInputs () {

	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	public void AssignForThisAutoPath ( float Range, Transform PathPos, bool back, float speed, float pathspeed = 0f ) {


		//Setting up the path to follow
		_pointOnSpline = Range;
		_PathTransform = PathPos;

		_pathTopSpeed = pathspeed;

		_playerSpeed = _PlayerPhys._RB.velocity.magnitude;

		if (_playerSpeed < speed)
			_playerSpeed = speed;

		_isGoingBackwards = back;
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
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Path)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Actions = _Tools._ActionManager;
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_Pathers = _Tools.PathInteraction;
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.MainSkin;
		_Sounds = _Tools.SoundControl;
		_CamHandler = _Tools.CamHandler._HedgeCam;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {

	}
	#endregion
}


