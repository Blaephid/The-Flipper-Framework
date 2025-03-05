using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_Action14_Upreel : S_Action_Base, IMainAction
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	[HideInInspector]
	public S_Upreel               _CurrentUpreel;
	private Transform             _HandGripTransform;
	#endregion



	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private float                 _upreelSpeedKeptAfter_;
	private float                 _minimumSpeedCarried_;
	#endregion

	// Trackers
	#region trackers
	

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

	new private void FixedUpdate () {
		base.FixedUpdate();
		MoveOnUpreel();
	}

	new public bool AttemptAction () {
		return false;
	}

	new public void StartAction ( bool overwrite = false ) {
		if (!base.AttemptAction()) return;

		if (enabled || (!_Actions._canChangeActions && !overwrite)) { return; }

		_Actions.ChangeAction(S_S_ActionHandling.PrimaryPlayerStates.Upreel);
		enabled = true;

		//Set same animation as when on a zipline.
		_CharacterAnimator.SetInteger("Action", 9);
		_CharacterAnimator.SetTrigger("ChangedState");

		_Actions._ActionDefault.SwitchSkin(true);
		_Actions._ActionDefault._isAnimatorControlledExternally = true;

		_speedBeforeUpreel = _PlayerVel._previousHorizontalSpeeds[1];

		S_S_Logic.AddLockToList(ref _PlayerPhys._locksForCanControl, "Upreel");
		S_S_Logic.AddLockToList(ref _PlayerPhys._locksForIsGravityOn, "Upreel");
		//_PlayerPhys._locksForCanControl.Add(false); //Removes ability to control velocity until empty
		//_PlayerPhys._locksForIsGravityOn.Add(false);
		_PlayerPhys.SetIsGrounded(false);
		_PlayerPhys._canChangeGrounded = false;

		_CurrentUpreel.DeployOrRetractHandle(false); //This method is in a script on the upreel rather than the player

	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { ReadyAction();  return; } //If first time, then return after setting to disabled.

		S_S_Logic.RemoveLockFromList(ref _PlayerPhys._locksForIsGravityOn, "Upreel");
		_PlayerPhys._canChangeGrounded = true;

		_Actions._isAirDashAvailable = true;

		//Enter standard animation
		_CharacterAnimator.SetInteger("Action", 0);
		_Actions._ActionDefault._isAnimatorControlledExternally = false;

		//Ends updates on this until a new upreel is set.
		_CurrentUpreel = null;

		_Actions._ActionDefault.StartAction();
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private


	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//Readies all the stats needed to move up an upreel over the next few updates.
	public void StartUpreel ( Collider col ) {
		//If not already on an upreel
		if (_CurrentUpreel != null) { return; }


		//Activates the upreel to start retracting. See PulleyActor class for more.
		//Sets currentUpreel. See FixedUpdate() above for more.
		_CurrentUpreel = col.gameObject.GetComponentInParent<S_Upreel>();

		//If the object has the necessary scripts
		if (_CurrentUpreel != null)
		{
			StartAction();
		}
	}

	//Handles player movement up an upreel when on it.
	public void MoveOnUpreel () {
		//Updates if the player is currently on an Upreel
		if (_CurrentUpreel != null)
		{
			//If the upreel is moving
			if (_CurrentUpreel._isMoving)
			{
				_Input.LockInputForAWhile(0f, false, Vector3.zero);
				_Actions._ActionDefault.HandleAnimator(9);

				PlaceOnHandle();
				_PlayerVel.SetBothVelocities(_CurrentUpreel._velocity, Vector2.right);

				_Actions._ActionDefault.SetSkinRotationToVelocity(0, -_CurrentUpreel.transform.forward, Vector2.zero, _CurrentUpreel.transform.up);
			}
			//On finished
			else
			{
				PlaceOnHandle ();
				StartCoroutine(EndUpreel(_CurrentUpreel.transform));
			}
		}
	}

	//Sets the player to the position of the Upreel including offset
	private void PlaceOnHandle () {

		Vector3 handlePosition = _CurrentUpreel.MoveHandleToLength();
		Vector3 HandPos = _PlayerPhys._CharacterPivotPosition - _HandGripTransform.position;
		_PlayerPhys.SetPlayerPosition(handlePosition + HandPos);
	}

	//When leaving pulley, player is bounced up and forwards after a momment, allowing them to clear the wall without issue.
	IEnumerator EndUpreel ( Transform Upreel ) {
		//Restores control but prevents input for a moment
		_Input.LockInputForAWhile(15f, false, _MainSkin.forward);
		//_PlayerPhys._locksForCanControl.RemoveAt(0);
		S_S_Logic.RemoveLockFromList(ref _PlayerPhys._locksForCanControl, "Upreel");

		//Launched over the lip the Upreel was on, then starts falling.
		_PlayerVel.SetCoreVelocity(Vector3.zero);
		_PlayerVel.SetEnvironmentalVelocity(new Vector3(0, Upreel.up.y * _CurrentUpreel._launchUpwardsForce, 0), true, true); //Launch straight upwards over any wall without affecting core velocity.

		StopAction();

		yield return new WaitForSeconds(0.12f);

		//Apply new force once past the wall to keep movement going.
		Vector3 forwardDirection = -Upreel.forward;
		forwardDirection.y = 0;

		float newSpeed = Mathf.Max(_upreelSpeedKeptAfter_ * _speedBeforeUpreel, _minimumSpeedCarried_);

		_PlayerVel.SetCoreVelocity(forwardDirection * newSpeed, "Overwrite");
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning


	//Responsible for assigning objects and components from the tools script.
	public override void AssignTools () {
		base.AssignTools();
		_HandGripTransform = _Tools.HandGripPoint;
	}

	//Reponsible for assigning stats from the stats script.
	public override void AssignStats () {
		_upreelSpeedKeptAfter_ = _Tools.Stats.ObjectInteractions.upreelSpeedKeptAfter;
		_minimumSpeedCarried_ = _Tools.Stats.ObjectInteractions.minimumSpeedCarried;
	}
	#endregion
}
