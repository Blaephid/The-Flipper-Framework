using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

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

	private S_Action07_RingRoad  _RingRoadAction;

	[HideInInspector]
	public Transform              _TargetRing;
	[HideInInspector]
	public GameObject[]           _ListOfHitTargets;
	[HideInInspector]
	public List<Transform>        _ListOfCloseTargets = new List<Transform>();

	private Transform             _MainSkin;
	#endregion

	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats

	private float       _targetSearchDistance_ = 10;
	private LayerMask   _Layer_;
	#endregion

	// Trackers
	#region trackers

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
			//For efficiency, there must be a gap between scans. This will only be a frame if currently performing the action (as this needs to be smoothly updated).
			//Ring road is currently not here because it will call ScanForRings seperately.
			switch (_Actions._whatCurrentAction)
			{
				default:
					yield return new WaitForSeconds(0.05f);
					break;
			}
			//Determined in the road action script, based on if attempt action is called, which means this only updates if the current action can enter a ring road
			//if (_Actions.IsActionConnectedToCurrentAction(S_S_ActionHandling.PlayerControlledStates.None,S_S_ActionHandling.PlayerSituationalStates.RingRoad))//When active, ring road scans for rings on its own, meaning this won't need to scan seperately.
			if(_RingRoadAction._inAStateConnectedToThis)
			{
				ScanForRings(new Vector2 (1, 0.1f), _MainSkin.forward, _PlayerPhys._CharacterCenterPosition);
			}
		}

	}

	//Called by this script at intervals, and by the ring road action when a ring has been picked up. Gets nereby rings, then calls a method to get a target from them.
	public void ScanForRings ( Vector2 modifier, Vector3 direction, Vector3 position ) {
		direction.Normalize();

		//Modifier x increase raidus, modifer y increase range sphere is cast along, direction and position will usually based on the character, but when creating a path will go from target to target.
		List<Transform> TargetsInRange = GetTargetsInRange(_targetSearchDistance_ * modifier.x, _targetSearchDistance_ * modifier.y, direction, position);
		_TargetRing = OrderTargets(TargetsInRange, position, direction);
	}


	//Returns any triggers of the correct layers (rings or ring roads).
	List<Transform> GetTargetsInRange ( float radius, float range, Vector3 scannerDirection, Vector3 scannerPosition ) {

		RaycastHit[] HitsInRange = Physics.SphereCastAll(scannerPosition, radius, scannerDirection, range, _Layer_, QueryTriggerInteraction.Collide);

		//Since a cast returns hits, convert those to a list of transforms.
		List<Transform>  TargetsInRange = new List<Transform>();
		for (int i = 0 ; i < HitsInRange.Length ; i++)
		{
			RaycastHit Target = HitsInRange[i];
			//If the transform of this hit isn't in the new list, add it.
			if (!TargetsInRange.Contains(Target.collider.transform))
			{
				TargetsInRange.Add(Target.collider.transform);
			}
		}
		return TargetsInRange;
	}

	//Go through each target found, and ready a list of them to be ordered in.
	Transform OrderTargets ( List<Transform> TargetsInRange, Vector3 scannerPosition, Vector3 scannerDirection ) {

		int checkLimit = 0; //Used to prevent too many checks in one frame, no matter how many rings in range at once.
		_ListOfCloseTargets.Clear(); //This new empty list will be used for the ordered targets.
		_ListOfCloseTargets.Add(null); //If none are found, will return null

		//Go through each collider and check it. If list is empty, then this will be skipped and null wull be returned.
		for (int i = 0 ; i < TargetsInRange.Count ; i++)
		{
			Transform Target = TargetsInRange[i];
			if (Target != null) //Called in case the collider was lost since scanned (like if the ring was picked up).
			{
				//Only add to list if in the direction the scan was going, so ones found behind the intended direction are not included
				Vector3 directionToTarget = Target.position - scannerPosition;
				if (Vector3.Angle(directionToTarget.normalized, scannerDirection) < 110)
				{
					PlaceTargetInOrder(Target, scannerPosition); //Compare this one to what's already been set as closest this scan (will return the new one if closest is null) 

					//As said above, limits checks per scan.
					checkLimit++;
					if (checkLimit > 4)
						break;
				}
			}
		}
		return _ListOfCloseTargets[0];
	}

	//Take the current target and place it where it fits into the list, by closest to furthest.
	void PlaceTargetInOrder ( Transform thisTarget, Vector3 scannerCentre ) {

		//Get the distance and direction of this target from the scanning centre
		float thisDistanceFromScannerSquared = S_S_MoreMaths.GetDistanceOfVectors(scannerCentre, thisTarget.position);

		//Go through the new ordered list so far.
		for (int i = 0 ; i < _ListOfCloseTargets.Count ; i++)
		{
			//If the first target checked this scan, then set immediately.
			if(_ListOfCloseTargets[i] == null)
			{
				_ListOfCloseTargets[i] = thisTarget;
				return; //Going through loop again after editing it would cause issues.
			}

			//If our checking target is closer than the target in this array space, then it goes before it. If not, check next array element.
			float tempDistanceSquared = S_S_MoreMaths.GetDistanceOfVectors(scannerCentre, _ListOfCloseTargets[i].position);
			if(thisDistanceFromScannerSquared < tempDistanceSquared)
			{
				_ListOfCloseTargets.Insert(i, thisTarget);
				return;
			}
		}
		//If hasn't returned yet, then this target is furthest, so add it at the end.
		_ListOfCloseTargets.Add(thisTarget);
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

		_MainSkin = _Tools.MainSkin;

		_RingRoadAction = GetComponent<S_Action07_RingRoad>();
		if (!_RingRoadAction) { enabled = false; }
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_targetSearchDistance_ = _Tools.Stats.RingRoadStats.searchDistance;
		_Layer_ = _Tools.Stats.RingRoadStats.RingRoadLayer;
	}
	#endregion
}
