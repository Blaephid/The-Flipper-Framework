
using UnityEngine;
using System.Collections;
using Cinemachine;
using SplineMesh;


//[RequireComponent(typeof(Spline))]
public class S_Interaction_Pathers : MonoBehaviour
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
	private S_Action05_Rail       _RailAction;

	[HideInInspector]
	public S_Trigger_CineCamera   _currentExternalCamera;

	//Character
	private Collider    _characterCapsule;
	private Animator    _CharacterAnimator;
	private Transform   _MainSkin;

	//Path
	[HideInInspector]
	public Spline                 _PathSpline;
	[HideInInspector]
	public CurveSample            _RailSample;

	//Upreel
	[HideInInspector]
	public S_Upreel               _currentUpreel;
	private Transform             _HandGripTransform;
	[HideInInspector]
	public Transform              _FeetTransform;
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private float                 _offsetUpreel_ = 1.5f;
	private float                 _UpreelSpeedKeptAfter_;
	#endregion

	// Trackers
	#region trackers

	private bool                  _canEnterAutoPath = false;    //Set to false when enterring a path trigger, and true when leaving it. Prevents multiple collisions with the same trigger, and allows entering a trigger to start OR end.
	[HideInInspector]
	public bool                   _canGrindOnRail;              //Set to false every frame, but then to true if in the AttemptAction method of the grind action.

	[HideInInspector]
	public bool                   _isFollowingPath ;            //If currently on a rail, zipline or automated path. Even if in the grind script, this won't always be true.

	//Upreel
	private float                 _speedBeforeUpreel;           //Gets running speed before an upreel and returns partly to it after finishing the action.
	#endregion

	public enum PathTypes {
		rail,
		zipline,
		upreel,
		automation,
	}

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {
		StartCoroutine(DisablingGrindingOnRailAtIntervals());
	}

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyScript();
	}

	private void FixedUpdate () {
			MoveOnUpreel();		
	}

	public void OnTriggerEnter ( Collider col ) {

		switch (col.gameObject.tag)
		{
			case "Rail":
				//Can only enter a rail if not already on one and in an action with grinding set as a situational action in the action manager.
				if (!_canGrindOnRail) { return; }

				//The different sizes of the rail collider have different minimum speeds. This is to allow less accuracy required at high speed.
				switch (col.GetComponent<CapsuleCollider>().radius)
				{
					case 4:
						if (_PlayerPhys._speedMagnitude > 120 || Mathf.Abs(_PlayerPhys._RB.velocity.y) > 30) { SetOnRail(null, true, col); }
						break;
					case 3:
						if (_PlayerPhys._speedMagnitude > 80 || Mathf.Abs(_PlayerPhys._RB.velocity.y) > 20) { SetOnRail(null, true, col); }
						break;
					case 2:
						if (_PlayerPhys._speedMagnitude > 40 || Mathf.Abs(_PlayerPhys._RB.velocity.y) > 10) { SetOnRail(null, true, col); }
						break;
					case 1:
						if (col.GetComponent<CapsuleCollider>().radius == 1) { SetOnRail(null, true, col); }
						break;
					default:
						SetOnRail(null, true, col);
						break;

				}
				break;

			case "ZipLine":
				//Can only enter a zipline if not already on one and in an action with grinding set as a situational action in the action manager.
				if (!_canGrindOnRail) { return; }

				if (col.transform.GetComponent<S_Control_Zipline>())
				{
					SetOnZipline(col);
				}
				break;

			case "Upreel":
				//If not already on an upreel
				if(_currentUpreel == null)
				{
					SetOnUpreel(col);
				}
				break;

			case "PathTrigger":
				if (!_canEnterAutoPath)
					StartCoroutine(SetOnPath(col));
				break;
		}
	}

	private void OnTriggerExit ( Collider col ) {
		//Automatic Paths
		switch (col.gameObject.tag)
		{
			case "PathTrigger":
				_canEnterAutoPath = false; //Since collision with trigger layers isn't disabled, this prevents being set on a path multiple times in one collider.
				break;
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private


	//This will constantly run in the background, disabling canGrindOnRail, which is enabled in the grinding script AttemptAction method. These two counterballance, meaning as long as AttemptAction is being called, this will be true, but when it stops, this will be false
	private IEnumerator DisablingGrindingOnRailAtIntervals () {
		while (true)
		{
			yield return new WaitForSeconds(0.04f);
			yield return new WaitForFixedUpdate();
			yield return new WaitForEndOfFrame();
			_canGrindOnRail = false; //Set true in the grinding action script whenever attempt action is called. This will mean currently in an action that can enter grind rails. 
		}
	}

	//Readies all the stats needed to move up an upreel over the next few updates.
	private void SetOnUpreel(Collider col) {
		//Activates the upreel to start retracting. See PulleyActor class for more.
		//Sets currentUpreel. See FixedUpdate() above for more.
		_currentUpreel = col.gameObject.GetComponentInParent<S_Upreel>();

		//If the object has the necessary scripts
		if (_currentUpreel != null)
		{
			//Set same animation as when on a zipline.
			_CharacterAnimator.SetInteger("Action", 9);
			_CharacterAnimator.SetTrigger("ChangedState");

			_speedBeforeUpreel = _PlayerPhys._previousHorizontalSpeeds[1];

			_PlayerPhys._listOfCanControl.Add(false); //Removes ability to control velocity until empty
			_PlayerPhys._isGravityOn = false;

			_currentUpreel.RetractPulley(); //This method is in a script on the upreel rather than the player
			_Actions._ActionDefault.StartAction();
		}
	}

	//Handles player movement up an upreel when on it.
	private void MoveOnUpreel () {
		//Updates if the player is currently on an Upreel
		if (_currentUpreel != null)
		{
			//If the upreel is moving
			if (_currentUpreel.Moving)
			{
				//Deactives player control and freezes movemnt to keep them in line with the upreel.

				_PlayerPhys._RB.velocity = Vector3.zero;
				_Input.LockInputForAWhile(0f, false, Vector3.zero);

				//Moves the player to the position of the Upreel
				Vector3 HandPos = transform.position - _HandGripTransform.position;
				HandPos += (_currentUpreel.transform.forward * _offsetUpreel_);
				transform.position = _currentUpreel.HandleGripPos.position + HandPos;
				_MainSkin.rotation = Quaternion.LookRotation(-_currentUpreel.transform.forward, _currentUpreel.transform.up);
			}
			//On finished
			else
			{
				//Restores control but prevents input for a moment
				_Input.LockInputForAWhile(20f, false, _MainSkin.forward);
				_PlayerPhys._listOfCanControl.RemoveAt(0);
				_PlayerPhys._isGravityOn = true;

				_Actions._isAirDashAvailables = true;

				//Enter standard animation
				_CharacterAnimator.SetInteger("Action", 0);

				StartCoroutine(ExitPulley(_currentUpreel.transform));

				//Ends updates on this until a new upreel is set.
				_currentUpreel = null;
			}
		}
	}

	//When leaving pulley, player is bounced up and forwards after a momment, allowing them to clear the wall without issue.
	IEnumerator ExitPulley (Transform Upreel) {
		_PlayerPhys.SetTotalVelocity(Upreel.up * 60, new Vector2(1, 0)); //Apply force up relative to upreel (therefore the direction the player was moving).

		yield return new WaitForSeconds(.2f);

		//Apply new force once past the wall to keep movement going.
		Vector3 forwardDirection = -Upreel.forward;
		forwardDirection.y = 0;
		//The speed forwards is a minimum of 15, but will increase to previous speed based on percentage set as an external stat.
		_PlayerPhys.AddCoreVelocity(forwardDirection * Mathf.Max(_UpreelSpeedKeptAfter_ * _speedBeforeUpreel, 15));
	}

	//Readies stats and activates the grinding action when on a rail. Can be called by OnTriggerEnter and OnCollisionEnter, assigning parameters appropriately.
	public void SetOnRail ( Collision collision, bool isTrigger, Collider collider ) {
		//Rail must have a spline to follow.
		if (collider.gameObject.GetComponentInParent<Spline>())
		{
			_PathSpline = collider.gameObject.GetComponentInParent<Spline>();

			//Depending on what collision type called this (trigger or collision), assign the position to get rail position from.
			Transform ColPos = isTrigger ? transform : GetCollisionPoint(collision);

			float Range = GetClosestPos(ColPos.position, _PathSpline); //Returns the closest point on the rail by position.

			Vector3 offSet = Vector3.zero;

			//Trigger Collisions on rails are organised by the PlaceOnSpline script, so if that had an offset to move the rail collision away from the spline, then follow that (this allows for multiple rails next to each other all following the same spline).
			if (isTrigger)
			{
				if (collider.gameObject.GetComponentInParent<S_PlaceOnSpline>())
				{
					offSet.x = collider.gameObject.GetComponentInParent<S_PlaceOnSpline>().Offset3d.x;
				}
			}
			//Physical collisions on rails use colliders generated by SplineMeshTiling, so check that for an offset.
			else
			{
				if (collider.gameObject.GetComponentInParent<S_SplineMeshTiling>())
				{
					offSet.x = collider.gameObject.GetComponentInParent<S_SplineMeshTiling>().translation.x;
				}
			}

			//If the rail has addOnRail scripts, then these contain info on rails following or preceeding this one.
			S_AddOnRail addOn = null;
			if (collider.gameObject.GetComponentInParent<S_AddOnRail>())
			{
				addOn = collider.gameObject.GetComponentInParent<S_AddOnRail>();

				//Check there is a potential rail to go to either way, otherwise don't add it.
				if(addOn.nextRail == null && addOn.PrevRail == null) { addOn = null; }
			}

			//Sets the player to the rail grind action, and sets their position and what spline to follow.
			_RailAction.AssignForThisGrind(Range, _PathSpline.transform, PathTypes.rail, offSet, addOn);
			_RailAction.StartAction();		
		}
	}

	//Readies stats for movemement on zipline and the zipline itself.
	private void SetOnZipline (Collider col) {
		_PathSpline = col.transform.GetComponent<S_Control_Zipline>().Rail;
		if(_PathSpline == null) { return; }

		//Assigns what is the Zipline and handle, and sets it to not be kinematic to avoid gravity.
		Rigidbody zipbody = col.GetComponent<Rigidbody>();
		_RailAction._ZipHandle = col.transform;
		_RailAction._ZipBody = zipbody;
		zipbody.isKinematic = false;

		float Range = GetClosestPos(zipbody.position, _PathSpline); //Gets place on rail closest to collision point.

		//Disables the homing target so it isn't a presence if homing attack can be performed in the grind action
		GameObject target = col.transform.GetComponent<S_Control_Zipline>().homingtgt;
		target.SetActive(false);

		//Sets the player to the rail grind action, and sets their position and what spline to follow.
		_RailAction.AssignForThisGrind(Range, _PathSpline.transform, PathTypes.zipline, Vector3.zero, null);
		_RailAction.StartAction();
	}

	IEnumerator SetOnPath ( Collider col ) {
		_canEnterAutoPath = true;

		//If the player is already on a path, then hitting this trigger will end it.
		if (_Actions._whatAction == S_Enums.PrimaryPlayerStates.Path || col.gameObject.name == "End")
		{
			//See MoveAlongPath for more
			_Actions.Action10.ExitPath();
		}

		else
		{
			float speedGo = 0f;

			//If the path is being started by a path speed pad
			if (col.gameObject.GetComponent<S_Data_SpeedPad>())
			{
				_PathSpline = col.gameObject.GetComponent<S_Data_SpeedPad>().path;
				speedGo = Mathf.Max(col.gameObject.GetComponent<S_Data_SpeedPad>().Speed, _PlayerPhys._horizontalSpeedMagnitude);
			}

			//If the path is being started by a normal trigger
			else if (col.gameObject.GetComponentInParent<Spline>() && col.gameObject.CompareTag("PathTrigger"))
				_PathSpline = col.gameObject.GetComponentInParent<Spline>();
			else
				_PathSpline = null;

			//If the player has been given a path to follow. This cuts out speed pads that don't have attached paths.
			if (_PathSpline != null)
			{
				//noDelay, the coroutine and otherCol. enabled are used to prevent the player colliding with the trigger multiple times for all of their attached colliders

				//Sets the player to start at the start and move forwards
				bool back = false;
				float range = 0f;

				//If entering an Exit trigger, the player will be set to move backwards and start at the end.
				if (col.gameObject.name == "Exit")
				{
					back = true;
					range = _PathSpline.Length - 1f;
				}

				//If the paths has a set camera angle.
				if (col.gameObject.GetComponent<S_Trigger_CineCamera>())
				{
					_currentExternalCamera = col.gameObject.GetComponent<S_Trigger_CineCamera>();
					_currentExternalCamera.ActivateCam(8f);
				}

				//Starts the player moving along the path using the path follow action
				_Actions.Action10.InitialEvents(range, _PathSpline.transform, back, speedGo);
				_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Path);
			}
		}
		yield return new WaitForEndOfFrame();

	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//Returns the average point in 3D space of all the different collision points
	public Transform GetCollisionPoint ( Collision col ) {
		Transform CollisionPointTransform = transform;
		foreach (ContactPoint contact in col.contacts)
		{
			//Set Middle Point
			Vector3 pointSum = Vector3.zero;
			//Add all collision points together
			for (int i = 0 ; i < col.contacts.Length ; i++)
			{
				pointSum = pointSum + col.contacts[i].point;
			}
			//Divide by amount of points to get average.
			pointSum = pointSum / col.contacts.Length;
			CollisionPointTransform.position = pointSum;
		}
		return CollisionPointTransform;
	}


	//Goes through whole spline and returns the point closests to the given position
	public float GetClosestPos ( Vector3 colliderPosition, Spline thisSpline ) {
		float CurrentDist = 9999999f;
		float closestSample = 0;
		for (float n = 0 ; n < thisSpline.Length ; n += 5)
		{
			//The distance between the point at distance n along the spline, and the current collider position.
			float dist = Vector3.Distance(thisSpline.transform.position + (thisSpline.transform.rotation * thisSpline.GetSampleAtDistance(n).location), colliderPosition);

			//Every time the distance is lower, the closest sample is set as that, so by the end of the loop, this will be set to the closest point.
			if (dist <= CurrentDist)
			{
				CurrentDist = dist;
				closestSample = n;
			}
		}
		return closestSample;
	}

	//Called when leaving a pulley to prevent player attaching to it immediately.
	public IEnumerator JumpFromZipLine ( Transform zipHandle, float time ) {

		//Disables the zipline handles collisions and homing target until after the delay
		zipHandle.GetComponent<CapsuleCollider>().enabled = false;
		GameObject target = zipHandle.transform.GetComponent<S_Control_Zipline>().homingtgt;
		target.SetActive(false);

		yield return new WaitForSeconds(time);

		//Reenables
		zipHandle.GetComponent<CapsuleCollider>().enabled = true;
		target.SetActive(true);
		zipHandle.GetComponentInChildren<MeshCollider>().enabled = true;

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
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Input = GetComponent<S_PlayerInput>();
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Actions = GetComponent<S_ActionManager>();
		_RailAction = GetComponent<S_Action05_Rail>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_characterCapsule = _Tools.characterCapsule.GetComponent<Collider>();
		_HandGripTransform = _Tools.HandGripPoint;
		_FeetTransform = _Tools.FeetPoint;
		_MainSkin = _Tools.mainSkin;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_offsetUpreel_ = _Tools.Stats.RailPosition.upreel;
		_UpreelSpeedKeptAfter_ = _Tools.Stats.ObjectInteractions.UpreelSpeedKeptAfter;
	}
	#endregion

}



