using UnityEngine;
using System.Collections;
using SplineMesh;
using UnityEngine.Windows;

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

	private Transform   _PathTransform;
	private CurveSample _Sample;

	private Animator	_CharacterAnimator;
	[HideInInspector]
	public Collider	_PatherStarter;
	private Transform	_MainSkin;
	#endregion



	//Stats
	#region Stats
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;


	// Values of spline
	private float	_pointOnSpline = 0f;

	private Vector3     _sampleUpwards;
	private Vector3     _sampleForwards;
	private Vector3     _sampleLocation;

	//Values for movement
	[HideInInspector]
	public float        _playerSpeed;
	private float       _pathMaxSpeed;
	private float       _pathMinSpeed;

	private bool        _isGoingBackwards;
	private int         _moveDirection;


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
		_Actions._ActionDefault.SetSkinRotationToVelocity(10);
		_Actions._ActionDefault.HandleAnimator(0);
	}

	private void FixedUpdate () {

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
		_Pathers._canExitAutoPath = true; //Will no longer cancel action when hitting a trigger.

		if (_CharacterAnimator.GetInteger("Action") != 0)
			_CharacterAnimator.SetTrigger("ChangedState"); //This is the only animation change because if set to this in the air, should keep the apperance from other actions. The animator will only change when action is changed.


		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Path);
		enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		_PlayerPhys._arePhysicsOn = true;
		_Input.LockInputForAWhile(15, true, Vector3.zero, S_Enums.LockControlDirection.CharacterForwards);

		_Pathers._canExitAutoPath = false; //Will no longer cancel action when hitting a trigger.
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	private void MoveAlongPath () {

		GetNewPointOnSpline();
		GetSampleOfSpline();

		_PlayerPhys.SetPlayerRotation(Quaternion.LookRotation(transform.forward, _sampleUpwards));
		_PlayerPhys.CheckForGround();
		_PlayerPhys.AlignToGround(_PlayerPhys._groundNormal, _PlayerPhys._isGrounded);

		SetVelocityAlongSpline(); 

		//If at the end of the path, all movement will still have been done, but will now cancel.
		if (_pointOnSpline == _Pathers._PathSpline.Length - 1 || _pointOnSpline == 0)
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
	}

	private void GetSampleOfSpline () {
		_pointOnSpline = Mathf.Clamp(_pointOnSpline, 0, _Pathers._PathSpline.Length - 1);

		//Get Sample of the Path to put player
		_Sample = _Pathers._PathSpline.GetSampleAtDistance(_pointOnSpline);
		_sampleForwards = _PathTransform.rotation * _Sample.tangent * _moveDirection;

		_sampleUpwards = _PathTransform.rotation * _Sample.up;
		_sampleLocation = (_PathTransform.rotation * _Sample.location) + _PathTransform.position;
	}

	private void PlaceOnSpline () {
		//Place at position so feet are on the spline.
		Vector3 FootPos = transform.position - _Pathers._FeetTransform.position;
		_PlayerPhys.SetPlayerPosition(_sampleLocation + FootPos);
		_PlayerPhys.SetPlayerRotation(Quaternion.LookRotation(transform.forward, _sampleUpwards));
	}

	private void SetRunningInDirectionOfSpline () {

		//Ensure starts going down the same direction every time.
		Vector3 relevantVelocity = _PlayerPhys.GetRelevantVector(_PlayerPhys._coreVelocity);
		Vector3 verticalVelocity = transform.up * relevantVelocity.y; //Seperates this so player can fall to the ground while still following the path.

		_playerSpeed = Mathf.Clamp(_PlayerPhys._currentRunningSpeed, _pathMinSpeed, _pathMaxSpeed); //Get new speed after changed according to primary inputs.

		_PlayerPhys.SetBothVelocities((_sampleForwards * _playerSpeed) + verticalVelocity, Vector2.right, "Overwrite");
		_PlayerPhys.SetTotalVelocity(); //Because arePhysicsEnabled was just disabled, enable here to ensure it goes through before overwritten by this script next update.
	}

	//Takes manual control of PlayerPhysics methods to move believably despite taking control of the input directions to ensure stays on the spline.
	private void SetVelocityAlongSpline () {
		Vector3 physCoreVelocity = _PlayerPhys._coreVelocity;

		//If inputting enough in direction of spline, go forwards
		Vector3 input = _Input._constantInputRelevantToCharacter;
		float dot = Vector3.Dot(_sampleForwards, input.normalized);
		if (input.sqrMagnitude > 0)
		{
			//If brought to a stop and inputting away from the spline, then turn around.
			if (dot < -0.5 && _PlayerPhys._currentRunningSpeed < 7) 
			{
				_sampleForwards = -_sampleForwards;
				physCoreVelocity = _sampleForwards * 1;
				_isGoingBackwards = !_isGoingBackwards;
				dot = 1;
			} 
			else { dot = dot > -0.7 ? 1 : 0; }
		}
		else
			dot = 0;
		_PlayerPhys._moveInput = _sampleForwards * dot; ;

		//Call methods after input is changed, acting as if mvoing normally just in the desired direction
		if (_PlayerPhys._isGrounded)
		{
			physCoreVelocity = _PlayerPhys.HandleControlledVelocity(physCoreVelocity,Vector2.one);
			physCoreVelocity = _PlayerPhys.HandleSlopePhysics(physCoreVelocity);
			physCoreVelocity = _PlayerPhys.StickToGround(physCoreVelocity);
		}
		else
		{
			physCoreVelocity = _PlayerPhys.HandleAirMovement(physCoreVelocity);
		}

		//Takes the changes to velocity and applies the lateral changes to the intended direction.
		Vector3 relevantVelocity = _PlayerPhys.GetRelevantVector(physCoreVelocity);
		Vector3 verticalVelocity = transform.up * relevantVelocity.y; //Seperates this so player can fall to the ground while still following the path.
		verticalVelocity = Vector3.zero;
		relevantVelocity.y = 0;
		_playerSpeed = Mathf.Clamp(relevantVelocity.magnitude, _pathMinSpeed, _pathMaxSpeed); //Get new speed after changed according to primary inputs.

		Debug.DrawRay(transform.position - transform.up * 0.5f, physCoreVelocity * Time.fixedDeltaTime, Color.yellow, 100f);

		//_PlayerPhys.SetBothVelocities((_sampleForwards * _playerSpeed) + verticalVelocity, Vector2.right, "Overwrite");
		_PlayerPhys.SetBothVelocities(physCoreVelocity, Vector2.right, "Overwrite");
		_PlayerPhys.SetTotalVelocity();
	}

	private void ExitPath () {

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
	public void AssignForThisAutoPath ( float range, Transform PathTransform, bool isGoingBack, float startSpeed, Vector2 speedClamps, bool willLockToStart ) {

		//Setting up the path to follow
		_pointOnSpline = range;
		_PathTransform = PathTransform;

		//Speed and direction to move this action
		_pathMinSpeed = speedClamps.x;
		_pathMaxSpeed = speedClamps.y;
		_playerSpeed = Mathf.Max(_PlayerPhys._currentRunningSpeed, startSpeed);

		_isGoingBackwards = isGoingBack;
		_moveDirection = _isGoingBackwards ? -1 : 1;

		GetSampleOfSpline();
		if (willLockToStart) { PlaceOnSpline(); }
		SetRunningInDirectionOfSpline();
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


