using UnityEngine;
using System.Collections;
using SplineMesh;
using UnityEngine.Windows;

public class S_Action10_FollowAutoPath : S_Action_Base, IMainAction
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_Interaction_Pathers _Pathers;

	private Transform   _PathTransform;
	private CurveSample _Sample;

	[HideInInspector]
	public Collider	_PatherStarter;
	#endregion



	//Stats
	#region Stats
	#endregion

	// Trackers
	#region trackers
	


	// Values of spline
	private float	_pointOnSpline = 0f;

	private Vector3     _sampleUpwards;
	private Vector3     _sampleForwards;
	private Vector3     _sampleLocation;

	//Values for movement
	private Vector3     _physicsCoreVelocity;
	[HideInInspector]
	public float        _playerSpeed;
	private float       _pathMaxSpeed;
	private float       _pathMinSpeed;

	private bool        _isGoingBackwards;
	private int         _moveDirection;
	private bool        _canReverse;
	private bool        _canSlow;

	private int         _willLockFor;

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

	new public bool AttemptAction () {		
		return false;
	}

	new public void StartAction ( bool overwrite = false ) {
		if (enabled || (!_Actions._canChangeActions && !overwrite)) { return; }

		//Physics
		_PlayerPhys._arePhysicsOn = false;
		_PlayerPhys._canStickToGround = true;
		_PlayerPhys._rayToGroundDistance_ *= 2;

		_Pathers._canExitAutoPath = true; //Will no longer cancel action when hitting a trigger.
		_Actions._listOfSpeedOnPaths.Add(_playerSpeed);

		if (_CharacterAnimator.GetInteger("Action") != 0)
			_CharacterAnimator.SetTrigger("ChangedState"); //This is the only animation change because if set to this in the air, should keep the apperance from other actions. The animator will only change when action is changed.


		_Actions.ChangeAction(S_S_ActionHandling.PrimaryPlayerStates.Path);
		enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { ReadyAction(); return; } //First time is called on ActionManager Awake() to ensure this starts disabled and has a single opportunity to assign tools and stats.

		_PlayerPhys._arePhysicsOn = true;
		_PlayerPhys._rayToGroundDistance_ = _Tools.Stats.FindingGround.rayToGroundDistance;

		_Pathers._canExitAutoPath = false; //Will no longer cancel action when hitting a trigger.

		_Actions._listOfSpeedOnPaths.RemoveAt(0);

		_PlayerMovement._currentMinSpeed = 0;
		_PlayerMovement._currentMaxSpeed = _Tools.Stats.SpeedStats.maxSpeed;
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

		SetVelocityAlongSpline(); 
		MoveTowardsPathMiddle();

		_PlayerPhys.AlignToGround(_PlayerPhys._groundNormal, _PlayerPhys._isGrounded);
		//_PlayerPhys.RespondToCollisions();

		ApplyVelocity();

		//If at the end of the path, all movement will still have been done, but will now cancel.
		if (_pointOnSpline == _Pathers._PathSpline.Length - 1 || _pointOnSpline == 0)
		{
				ExitPath();
		}
	}

	//Take speed and return new distace on spline

	private void GetNewPointOnSpline () {
		//Increase the Amount of distance through the Spline by DeltaTime
		float travelAmount = (Time.deltaTime * _playerSpeed);
		_moveDirection = _isGoingBackwards ? -1 : 1;

		// Increase/Decrease Range depending on direction
		_pointOnSpline += travelAmount * _moveDirection;
	}

	//Take the distance and return the necessary data from that point, including parent transforms.
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

	//Rotate player to move in the spline direction immediately
	private void SetRunningInDirectionOfSpline () {

		//Ensure starts going down the same direction every time.
		Vector3 relevantVelocity = _PlayerPhys.GetRelevantVector(_PlayerVel._worldVelocity);
		Vector3 verticalVelocity = transform.up * relevantVelocity.y; //Seperates this so player can fall to the ground while still following the path.

		_PlayerVel.SetBothVelocities((_sampleForwards * _playerSpeed) + verticalVelocity, Vector2.right, "Overwrite");
		//Set total velocity in PlayerVelocity fixedUpdate is still called after every other script.
	}

	//Takes manual control of PlayerPhysics methods to move believably despite taking control of the input directions to ensure stays on the spline.
	private void SetVelocityAlongSpline () {
		_physicsCoreVelocity = _PlayerVel._coreVelocity;

		//If inputting enough in direction of spline, go forwards
		Vector3 input = _Input._constantInputRelevantToCharacter;
		float dot = Vector3.Dot(_sampleForwards, input.normalized);

		float direction = 0;
		float decelerationValue = 1;            //direction must be above 0 so turning still happens, so to apply deceleration, use a seperate modifer.
		if (input.sqrMagnitude > 0 || !_canSlow)
		{
			//If brought to a stop and inputting away from the spline, then turn around.
			if (_canReverse && dot < -0.5 && _PlayerVel._currentRunningSpeed < 7)
			{
				_sampleForwards = -_sampleForwards;
				_physicsCoreVelocity = _sampleForwards * 1;
				_isGoingBackwards = !_isGoingBackwards;
				direction = 1;
			}
			else  
			{
				_Input._inputOnController = Vector2.one;
				direction = dot > -0.7 || !_canReverse ? 1 : 0.2f;
			}
		}
		else      //Otherwise, not inputting. 	
		{
			direction = _pathMinSpeed > 0 ? 1 : 0.2f;
			decelerationValue = 0;
		}
	
		//Ensure player is either inputting alon or against the path, translated to current rotation.
		_PlayerMovement._moveInput = _PlayerPhys.GetRelevantVector(_sampleForwards * direction);
		_Input.LockInputForAWhile(0, false, _sampleForwards * direction, S_GeneralEnums.LockControlDirection.Change);

		//Call methods after input is changed, acting as if mvoing normally just in the desired direction
		if (_PlayerPhys._isGrounded)
		{
			_PlayerPhys._timeOnGround += Time.deltaTime;

			_physicsCoreVelocity = _PlayerMovement.HandleControlledVelocity(_physicsCoreVelocity,new Vector2 (3, 1), decelerationValue);
			_physicsCoreVelocity = _PlayerPhys.HandleSlopePhysics(_physicsCoreVelocity, false);
			_physicsCoreVelocity = _PlayerPhys.StickToGround(_physicsCoreVelocity);

			_Actions._ActionDefault.SwitchSkin(true);
		}
		else
		{
			//Doesnt call handle air velocity because it creates its own modifiers to turn speed.
			_physicsCoreVelocity = _PlayerMovement.HandleControlledVelocity(_physicsCoreVelocity, new Vector2(3, 0.8f));
			_physicsCoreVelocity = _PlayerPhys.CheckGravity(_physicsCoreVelocity);
		}
	}

	private void ApplyVelocity () {

		_PlayerVel.SetBothVelocities(_physicsCoreVelocity, Vector2.right);
		//_PlayerVel.SetTotalVelocity();
		//Set total velocity in PlayerVelocity fixedUpdate is still called after every other script.

		_playerSpeed = _PlayerVel._currentRunningSpeed;
	}

	//Apply an additional velocity this frame to slowly move to the path itself, rather than be at an offset.
	private void MoveTowardsPathMiddle () {
		if (_PlayerPhys._isGrounded && _playerSpeed > 20)
		{
			Vector3 FootPos = transform.position - _Pathers._FeetTransform.position;
			Vector3 direction = _sampleLocation - _Pathers._FeetTransform.position;

			//Don't apply any velocity upwards, so take relevant to player, remove veritcal, then return.
			direction = _PlayerPhys.GetRelevantVector(direction, false);
			direction = transform.TransformDirection(direction);

			_PlayerVel.AddGeneralVelocity(direction.normalized * 4, false, false);
		}
	}

	private void ExitPath () {
		_Input.LockInputForAWhile(_willLockFor, false, _sampleForwards, S_GeneralEnums.LockControlDirection.Change);

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
	public void AssignForThisAutoPath ( float range, Transform PathTransform, bool isGoingBack, float startSpeed, S_Trigger_Path.StrucAutoPathData PathData, bool willLockToStart ) {

		//Setting up the path to follow
		_pointOnSpline = range;
		_PathTransform = PathTransform;

		//Speed and direction to move this action
		_pathMinSpeed = PathData._speedLimits_.x;
		_PlayerMovement._currentMinSpeed = _pathMinSpeed;
		_pathMaxSpeed = PathData._speedLimits_.y;
		_PlayerMovement._currentMaxSpeed = _pathMaxSpeed;
		_playerSpeed = Mathf.Max(_PlayerVel._currentRunningSpeed, startSpeed);
		_playerSpeed = Mathf.Clamp(_playerSpeed, _pathMinSpeed, _pathMaxSpeed); //Get new speed after changed according to primary inputs.

		_isGoingBackwards = isGoingBack;
		_moveDirection = _isGoingBackwards ? -1 : 1;
		_canReverse = PathData._canPlayerReverse_;
		_canSlow = PathData._canPlayerSlow_;
		_willLockFor = PathData._lockPlayerFor_;

		GetSampleOfSpline();
		if (willLockToStart) { PlaceOnSpline(); }
		SetRunningInDirectionOfSpline();
	}
	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//Responsible for assigning objects and components from the tools script.
	public override void AssignTools () {
		base.AssignTools();
		_Pathers = _Tools.PathInteraction;
	}

	//Reponsible for assigning stats from the stats script.
	public override void AssignStats () {

	}
	#endregion
}


