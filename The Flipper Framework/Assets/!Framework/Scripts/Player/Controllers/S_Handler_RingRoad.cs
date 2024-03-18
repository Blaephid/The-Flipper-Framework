using UnityEngine;
using System.Collections;

public class S_Handler_RingRoad : MonoBehaviour
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

	[HideInInspector]
	public GameObject		_TargetRing;
	[HideInInspector] 
	public GameObject[]		_Targets;
	#endregion

	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats

	private float	_targetSearchDistance_ = 10;
	private LayerMask	_Layer_;
	#endregion

	// Trackers
	#region trackers
	[HideInInspector]
	public bool	_hasTarget;
	[HideInInspector]
	public bool         _isScanning;
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {
		ReadyScript();
		StartCoroutine(ScanForTargets());
	}

	// Update is called once per frame
	void Update () {

	}

	private void FixedUpdate () {
		if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.RingRoad)
		{
			Collider[] TargetsInRange = GetCloseTargets(_targetSearchDistance_);

			if (TargetsInRange.Length > 0)
			{
				//yield return new WaitForFixedUpdate();
				_TargetRing = GetClosestTarget(TargetsInRange);
			}
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	private IEnumerator ScanForTargets () {
		while (true)
		{
			yield return new WaitForFixedUpdate();
			//Determined in the road action script, based on if attempt action is called, which means this only updates if the current action can enter a ring road
			if (_isScanning)
			{
				Collider[] TargetsInRange = GetCloseTargets(_targetSearchDistance_);

				//If any are found, wait another frame for efficiency, then sort them.
				if (TargetsInRange.Length > 0)
				{
					yield return new WaitForFixedUpdate();
					_TargetRing = GetClosestTarget(TargetsInRange);

				}
			}
			_isScanning = false; //Set to false every frame but will be counteracted in Action RingRoad's AttemptAction()
		}

	}

	//Returns any triggers of the correct layers (rings or ring roads), if in the given range.
	Collider[] GetCloseTargets ( float maxDistance ) {
		Collider[] TargetsInRange = Physics.OverlapSphere(transform.position, maxDistance, _Layer_, QueryTriggerInteraction.Collide);
		return TargetsInRange;
	}

	GameObject GetClosestTarget ( Collider[] TargetsInRange ) {
		_hasTarget = false;

		int checkLimit = 0;
		Transform closestTarget = null;
		foreach (Collider t in TargetsInRange)
		{
			if (t != null) //Called in case the collider was lost since scanned (like if the ring was picked up).
			{
				Transform target = t.transform;
				closestTarget = CheckTarget(target, closestTarget);

				checkLimit++;
				if (checkLimit > 3)
					break;
			}

		}
		if (closestTarget != null)
			return closestTarget.gameObject;
		else
			return null;
	}

	Transform CheckTarget ( Transform thisTarget, Transform current ) {
		float dis = Vector3.Distance(transform.position, thisTarget.position); 

		if (current == null)
			return thisTarget;
		else
		{
			float closDis = Vector3.Distance(transform.position, current.position);
			if (closDis > dis)
			{
				_hasTarget = true;
				return thisTarget;
			}

		}

		return current;
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
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_targetSearchDistance_ = _Tools.Stats.RingRoadStats.SearchDistance;
		_Layer_ = _Tools.Stats.RingRoadStats.RingRoadLayer;
	}
	#endregion
}
