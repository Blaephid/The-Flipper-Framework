using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_Action14_Upreel : MonoBehaviour, IMainAction
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
	private Animator              _CharacterAnimator;
	private Transform             _MainSkin;


	[HideInInspector]
	public S_Upreel               _currentUpreel;
	private Transform             _HandGripTransform;
	#endregion



	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private float                 _upreelSpeedKeptAfter_;
	private float                 _minimumSpeedCarried_;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;

	private float       _speedBeforeUpreel;

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
	private void OnDisable () {

	}

	// Update is called once per frame
	void Update () {
	}

	private void FixedUpdate () {
		MoveOnUpreel();
	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		willChangeAction = true;
		return willChangeAction;

	}

	public void StartAction () {
		//Set same animation as when on a zipline.
		_CharacterAnimator.SetInteger("Action", 9);
		_CharacterAnimator.SetTrigger("ChangedState");

		_Actions._ActionDefault.SwitchSkin(true);
		_Actions._ActionDefault._isAnimatorControlledExternally = true;

		_speedBeforeUpreel = _PlayerPhys._previousHorizontalSpeeds[1];

		_PlayerPhys._listOfCanControl.Add(false); //Removes ability to control velocity until empty
		_PlayerPhys._isGravityOn = false;
		_PlayerPhys.SetIsGrounded(false);
		_PlayerPhys._canChangeGrounded = false;

		_currentUpreel.DeployOrRetractHandle(false); //This method is in a script on the upreel rather than the player

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Upreel);
		enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		_PlayerPhys._isGravityOn = true;
		_PlayerPhys._canChangeGrounded = true;

		_Actions._isAirDashAvailables = true;

		//Enter standard animation
		_CharacterAnimator.SetInteger("Action", 0);
		_Actions._ActionDefault._isAnimatorControlledExternally = false;

		//Ends updates on this until a new upreel is set.
		_currentUpreel = null;

		_Actions._ActionDefault.StartAction();
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


	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//Readies all the stats needed to move up an upreel over the next few updates.
	public void StartUpreel ( Collider col ) {
		//If not already on an upreel
		if (_currentUpreel != null) { return; }


		//Activates the upreel to start retracting. See PulleyActor class for more.
		//Sets currentUpreel. See FixedUpdate() above for more.
		_currentUpreel = col.gameObject.GetComponentInParent<S_Upreel>();

		//If the object has the necessary scripts
		if (_currentUpreel != null)
		{
			StartAction();
		}
	}

	//Handles player movement up an upreel when on it.
	public void MoveOnUpreel () {
		//Updates if the player is currently on an Upreel
		if (_currentUpreel != null)
		{
			//If the upreel is moving
			if (_currentUpreel._isMoving)
			{
				_Input.LockInputForAWhile(0f, false, Vector3.zero);
				_Actions._ActionDefault.HandleAnimator(9);

				PlaceOnHandle();
				_PlayerPhys.SetBothVelocities(_currentUpreel._velocity, Vector2.right);

				_Actions._ActionDefault.SetSkinRotationToVelocity(0, -_currentUpreel.transform.forward, Vector2.zero, _currentUpreel.transform.up);
			}
			//On finished
			else
			{
				PlaceOnHandle ();
				StartCoroutine(EndUpreel(_currentUpreel.transform));
			}
		}
	}

	//Sets the player to the position of the Upreel including offset
	private void PlaceOnHandle () {

		Vector3 handlePosition = _currentUpreel.MoveHandleToLength();
		Vector3 HandPos = transform.position - _HandGripTransform.position;
		_PlayerPhys.SetPlayerPosition(handlePosition + HandPos);
	}

	//When leaving pulley, player is bounced up and forwards after a momment, allowing them to clear the wall without issue.
	IEnumerator EndUpreel ( Transform Upreel ) {
		//Restores control but prevents input for a moment
		_Input.LockInputForAWhile(15f, false, _MainSkin.forward);
		_PlayerPhys._listOfCanControl.RemoveAt(0);

		//Launched over the lip the Upreel was on, then starts falling.
		_PlayerPhys.SetCoreVelocity(Vector3.zero);
		_PlayerPhys.SetEnvironmentalVelocity(new Vector3(0, Upreel.up.y * _currentUpreel._launchUpwardsForce, 0), true, true); //Launch straight upwards over any wall without affecting core velocity.

		StopAction();

		yield return new WaitForSeconds(0.12f);

		//Apply new force once past the wall to keep movement going.
		Vector3 forwardDirection = -Upreel.forward;
		forwardDirection.y = 0;

		float newSpeed = Mathf.Max(_upreelSpeedKeptAfter_ * _speedBeforeUpreel, _minimumSpeedCarried_);

		_PlayerPhys.SetCoreVelocity(forwardDirection * newSpeed, "Overwrite");
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//If not assigned already, sets the tools and stats and gets placement in Action Manager's action list.
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
		_Actions = _Tools._ActionManager;

		_MainSkin = _Tools.MainSkin;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_HandGripTransform = _Tools.HandGripPoint;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_upreelSpeedKeptAfter_ = _Tools.Stats.ObjectInteractions.upreelSpeedKeptAfter;
		_minimumSpeedCarried_ = _Tools.Stats.ObjectInteractions.minimumSpeedCarried;
	}
	#endregion
}
