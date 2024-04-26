using UnityEngine;
using System.Collections;
using SplineMesh;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;

[RequireComponent(typeof(S_Handler_RingRoad))]
public class S_Action07_RingRoad : MonoBehaviour, IMainAction
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
	private S_Handler_RingRoad    _RoadHandler;

	private Transform   _MainSkin;
	private Animator    _CharacterAnimator;
	private GameObject  _HomingTrailContainer;
	private GameObject  _HomingTrail;
	private GameObject  _JumpBall;

	private Spline      _CreatedSpline;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	private bool        _willCarrySpeed_;
	private float       _dashSpeed_;
	private float       _minimumEndingSpeed_;
	private float       _speedGain_;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;        //In every action script, takes note of where in the Action Managers Main action list this script is.  This is used for transitioning to other actions, by input or interaction.

	public Vector3      _trailOffSet = new Vector3(0,-3,0);	//The trail effect will be away from the player by this.


	private float       _speedBeforeAction;		//The speed moving at before the action. Will return to it when action ends.
	private Vector3     _directionToGo;		//The direction the player will move towards the next ring or along the created path.
	private float       _positionAlongPath;		//How far the player has moved this dash.

	private int         _counter;

	private List<Transform> _ListOfRingsInRoad = new List<Transform>();
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
		ReadyAction();
	}

	// Update is called once per frame
	void Update () {
		//Set Animator Parameters
		_Actions._ActionDefault.HandleAnimator(7);
		PlaceOnCreatedPath();
	}

	private void FixedUpdate () {
		CreatePath();
		_counter++;

		HandleInputs();
	}

	public bool AttemptAction () {
		_RoadHandler._isScanning = true; //This makes it so the scanner will only happen if this method is called by another action (decided in the action manager).
		if (_Input.InteractPressed && _RoadHandler._TargetRing != null && !enabled)
		{
			StartAction();
			return true;
		}

		return false;
	}

	public void StartAction () {

		//Physics
		_PlayerPhys.SetBothVelocities(Vector3.zero, Vector2.right); //Prevent character moving outside of the path.
		_PlayerPhys._listOfCanControl.Add(false); //Prevent controlled movement until end of action.
		_PlayerPhys._canChangeGrounded = false;
		_PlayerPhys.SetIsGrounded(false);

		//Effects
		_CharacterAnimator.SetTrigger("ChangedState");
		_JumpBall.SetActive(false);

		//Trail that follows behind player.
		if (_HomingTrailContainer.transform.childCount < 1)
		{
			GameObject HomingTrailClone = Instantiate (_HomingTrail, _HomingTrailContainer.transform.position, Quaternion.identity) as GameObject;
			HomingTrailClone.transform.parent = _HomingTrailContainer.transform;
			HomingTrailClone.transform.localPosition = _trailOffSet;
		}

		//Ready path. The way this action works is it creates a spline going through each ring target. This created an object with the component to store this data in.
		GameObject GO = new GameObject("TEMPORARY SPLINE");
		_CreatedSpline = GO.AddComponent<Spline>();

		//Set private
		_speedBeforeAction = _PlayerPhys._horizontalSpeedMagnitude; //Gets speed to return to later
		_ListOfRingsInRoad.Clear(); //Used to track if a target has been used as a node for the spline.

		_positionAlongPath = 0; //The spline is created from player location, so start at 0.
		_counter = 0;

		_speedBeforeAction = _PlayerPhys._horizontalSpeedMagnitude;
		_Actions._listOfSpeedOnPaths.Add(Mathf.Max(_dashSpeed_, _speedBeforeAction * 1.2f)); //Speed to move at, always faster than was moving before.

		_directionToGo = _RoadHandler._TargetRing.position - transform.position; //This will be changed to reflect the spline later, but this allows checking and movement before that.

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.RingRoad);
		this.enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		//Destroy(_CreatedSpline.gameObject); //Since its purpose is fulfiled, remove it to save space.

		//Here to ensure this is always called no matter why the action ends.
		_Input.LockInputForAWhile(10, false, _MainSkin.forward); //Lock for a moment.
		StartCoroutine(_PlayerPhys.LockFunctionForTime(S_PlayerPhysics.EnumControlLimitations.canDecelerate, 0, 15));

		_PlayerPhys._canChangeGrounded = true;

		_Actions._listOfSpeedOnPaths.RemoveAt(0); //Remove the speed that was used for this action. As a list because this stop action might be called after the other action's StartAction.

		_PlayerPhys._listOfCanControl.RemoveAt(0); //Remove lock on control before this, but add a new delay before control returns.
		StartCoroutine(_PlayerPhys.LockFunctionForTime(S_PlayerPhysics.EnumControlLimitations.canControl, 0.2f));

		//End effects
		for (int i = _HomingTrailContainer.transform.childCount - 1 ; i >= 0 ; i--)
			Destroy(_HomingTrailContainer.transform.GetChild(i).gameObject);
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

	private void CreatePath () {

		//Goes through the list of targets from closest to furthers (see Handler_RingRoad).
		foreach (Transform Target in _RoadHandler._ListOfCloseTargets)
		{
			if (Target != null) //If still a valid object (because rings could be picked up).
			{
				//If not used yet.
				if (!_ListOfRingsInRoad.Contains(Target))
				{
					//Create a new node in the spline, expanding it so it can be followed.
					_CreatedSpline.AddNode(new SplineNode(Target.position, Target.position));

					//This ensures the player won't miss the ring when moving along spline at high speed.
					Target.GetComponent<SphereCollider>().radius = 8f;

					_ListOfRingsInRoad.Add(Target);
				}
			}
		}

		//Spline is created by looking for more valid targets through each target. So this makes a scan for rings which wouldn't happen otherwise, but from the most recent target, in the direction from penultimate node to last.
		if (_CreatedSpline.nodes.Count > 1)
			_RoadHandler.ScanForRings(new Vector2(0.4f, 1.8f), _CreatedSpline.nodes[_CreatedSpline.nodes.Count - 1].Position - _CreatedSpline.nodes[_CreatedSpline.nodes.Count - 2].Position, _CreatedSpline.nodes[_CreatedSpline.nodes.Count - 1].Position); //Called now because when being performed there needs to be constant updates to targets.
		else if (_CreatedSpline.nodes.Count > 0)
			_RoadHandler.ScanForRings(new Vector2(2.5f, 0.1f), _directionToGo.normalized, _CreatedSpline.nodes[0].Position);

	}

	private void PlaceOnCreatedPath () {
		if (_counter < 3) { return; } //Gives time to create a spline before moving along it.

		_positionAlongPath += Time.deltaTime * _Actions._listOfSpeedOnPaths[0]; //Increase distance on spline by speed.

		//If still on the spline.
		if (_positionAlongPath < _CreatedSpline.Length)
		{
			//Get the world transform point of that point on the spline.
			CurveSample Sample = _CreatedSpline.GetSampleAtDistance(_positionAlongPath);

			//Place player on it (since called in update, not fixed update, won't be too jittery).
			_PlayerPhys.SetPlayerPosition( Sample.location);

			//Rotate towards the next ring, according to the created spline.
			_directionToGo = Sample.tangent;
			Quaternion targetRotation = Quaternion.LookRotation(Sample.tangent);
			_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, targetRotation, 0.6f);
		}
		else
		{
			EndRingRoad();
		}
	}

	//Handles player physics when at the end of a chain of rings.
	private void EndRingRoad () {


		//End at the speed started at (with a slight change), but with a minimum.
		float endingSpeedResult = Mathf.Max(_minimumEndingSpeed_, _speedBeforeAction * _speedGain_);

		if (!_willCarrySpeed_) endingSpeedResult = _minimumEndingSpeed_; //Speed unaffected by how it was before the action.

		//Sends the player in the direction of the end of the spline.
		CurveSample Sample = _CreatedSpline.GetSampleAtDistance(_CreatedSpline.Length - 1);
		_directionToGo = Sample.tangent;

		_Actions._ActionDefault.SetSkinRotationToVelocity(0, _directionToGo);

		_PlayerPhys.SetPlayerPosition( Sample.location);
		_PlayerPhys.SetBothVelocities(_directionToGo.normalized * endingSpeedResult, new Vector2(1, 0));

		//If the speed the player is at now is lower than the speed they were dashing at, lerp the difference rather than make it instant.
		float differentSpeedOnExit = _Actions._listOfSpeedOnPaths[0] - endingSpeedResult;
		if(differentSpeedOnExit > 0) { StartCoroutine(LoseTemporarySpeedOverTime(differentSpeedOnExit)); }

		_Actions._ActionDefault.HandleAnimator(0);
		_Actions._ActionDefault.StartAction();
	}

	//For a period of frames, adds a force (that doesn't carry over to the next frame) to fake there still being speed from the dash, but decrease it down to the actual speed.
	private IEnumerator LoseTemporarySpeedOverTime (float tempSpeed) {
		int frames = 15;
		float increments = tempSpeed / frames;
		for (int i = 0 ; i < frames ; i++)
		{
			yield return new WaitForFixedUpdate ();
			_PlayerPhys.AddGeneralVelocity(_PlayerPhys._coreVelocity.normalized * tempSpeed);
			tempSpeed -= increments; //Go down in increments. When at zero, the player will have smoothly gone down to the actual speed rather than jump.
		}
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
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.RingRoad)
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
		_Actions = _Tools._ActionManager;
		_Actions = _Tools._ActionManager;
		_RoadHandler = GetComponent<S_Handler_RingRoad>();

		_MainSkin = _Tools.MainSkin;
		_HomingTrailContainer = _Tools.HomingTrailContainer;
		_JumpBall = _Tools.JumpBall;
		_HomingTrail = _Tools.HomingTrail;
		_CharacterAnimator = _Tools.CharacterAnimator;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_willCarrySpeed_ = _Tools.Stats.RingRoadStats.willCarrySpeed;
		_dashSpeed_= _Tools.Stats.RingRoadStats.dashSpeed;
		_minimumEndingSpeed_ = _Tools.Stats.RingRoadStats.minimumEndingSpeed;
		_speedGain_ = _Tools.Stats.RingRoadStats.speedGained;
	}
	#endregion

}
