
using UnityEngine;
using System.Collections;
using Cinemachine;
using SplineMesh;
using System.Drawing;


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
	private S_CharacterTools		_Tools;
	private S_PlayerPhysics		_PlayerPhys;
	private S_PlayerVelocity		_PlayerVel;
	private S_PlayerInput		_Input;
	private S_ActionManager		_Actions;
	private S_Action05_Rail		_RailAction;
	private S_Action10_FollowAutoPath	_PathAction;
	private S_Action14_Upreel		_UpreelAction;

	[HideInInspector]
	public S_Trigger_CineCamera   _currentExternalCamera;

	//Character
	private Collider    _characterCapsule;
	private Animator    _CharacterAnimator;
	private Transform   _MainSkin;
	[HideInInspector]
	public Transform   _FeetTransform;

	//Path
	[HideInInspector]
	public Spline                 _PathSpline;
	[HideInInspector]
	public CurveSample            _RailSample;

	#endregion

	// Trackers
	#region trackers

	[HideInInspector]
	public    bool                _canEnterAutoPath = false;
	[HideInInspector]
	public    bool                _canExitAutoPath = false;
	[HideInInspector]
	public bool                  _isCurrentlyInAutoTrigger = false;    //Set to true when enterring a path trigger, and false when leaving it. Prevents multiple collisions with the same trigger, and allows entering a trigger to start OR end.
	[HideInInspector]
	public bool                   _canGrindOnRail;              //Set to false every frame, but then to true if in the AttemptAction method of the grind action.

	//Upreel
	private float                 _speedBeforeUpreel;           //Gets running speed before an upreel and returns partly to it after finishing the action.
	#endregion

	public enum PathTypes
	{
		rail,
		zipline,
		automation,
	}

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited


	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyScript();
	}

	private void FixedUpdate () {
		_canGrindOnRail = _RailAction ? _RailAction._inAStateConnectedToThis && _RailAction._canEnterRail : false;
		_canEnterAutoPath = _PathAction ? _PathAction._inAStateConnectedToThis : false;
	}

	private void Update () {

	}

	public void EventTriggerEnter ( Collider col ) {

		switch (col.gameObject.tag)
		{
			case "ZipLine":
				//Can only enter a zipline if not already on one and in an action with grinding set as a situational action in the action manager.
				if (!_canGrindOnRail) { return; }

				if (col.transform.GetComponent<S_Control_Zipline>())
				{
					SetOnZipline(col);
				}
				break;

			case "Upreel":
				if(_UpreelAction != null)
					_UpreelAction.StartUpreel(col);
				break;

			case "PathTrigger":
				//If currently already on a path, exit it.
				if (_canExitAutoPath && !_isCurrentlyInAutoTrigger)
					_Actions._ActionDefault.StartAction();
				else if (!_isCurrentlyInAutoTrigger && _canEnterAutoPath)
					SetOnPath(col);
				break;
		}
	}

	//Because the variety of speeds means rails should be easier to land on at higher speeds, being in a rail trigger causes different checks to happen each frame, only setting if within range inside the trigger.
	public void EventTriggerStay ( Collider col ) {
		switch (col.gameObject.tag)
		{
			case "Rail":
				//Can only enter a rail if not already on one and in an action with grinding set as a situational action in the action manager.
				if (!_canGrindOnRail) { return; }

				CheckRail(col);
				break;
		}
	}

	public void EventTriggerExit ( Collider col ) {
		//Automatic Paths
		switch (col.gameObject.tag)
		{
			case "PathTrigger":
				_isCurrentlyInAutoTrigger = false; //Since collision with trigger layers isn't disabled, this prevents being set on a path multiple times in one collider.
				break;
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	private void CheckRail ( Collider Col ) {
		Spline ThisSpline = Col.gameObject.GetComponentInParent<Spline>(); //Create a temporary variable to check this rail before confirming it.

		//Can only get on a rail if players feet are towards the top of it (meaning upwards directions are similar.)
		float angleBetweenUpwardsDirections = Vector3.Angle(transform.up, Col.transform.up);
		if(angleBetweenUpwardsDirections > 90) { return;  }

		Vector3 offset = Vector3.zero;
		if (Col.GetComponentInParent<S_PlaceOnSpline>())
		{
			offset = Col.GetComponentInParent<S_PlaceOnSpline>()._offset3d_;
		}

		Vector2 rangeAndDistanceSquared = S_RailFollow_Base.GetClosestPointOfSpline(transform.position, ThisSpline, offset, 3); //Returns the closest point on the rail by position.

		//At higher speeds, it should be easier to get on the rail, so get the distance between player and point, and check if close enough based on speed.
		float maxDistanceNeededBasedOnSpeed = Mathf.Max(_PlayerVel._horizontalSpeedMagnitude, Mathf.Abs(_PlayerVel._coreVelocity.y));
		maxDistanceNeededBasedOnSpeed /= 13;

		//If rail hopping, movement is set horizontally, so ensure player can't get stuck on the same rail they just left.
		if (_Actions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.Rail) { maxDistanceNeededBasedOnSpeed = 5f; }

		maxDistanceNeededBasedOnSpeed = Mathf.Pow(Mathf.Clamp(maxDistanceNeededBasedOnSpeed, 2f, 11f), 2);
		//Compare distance squared, to distance needed depending on speed squared.
		if (rangeAndDistanceSquared.y < maxDistanceNeededBasedOnSpeed)
		{
			SetOnRail(true, Col, rangeAndDistanceSquared);
		}
	}

	//Readies stats and activates the grinding action when on a rail. Can be called by OnTriggerEnter and OnCollisionEnter, assigning parameters appropriately.
	public void SetOnRail ( bool isTrigger, Collider collider, Vector2 rangeAndDis ) {
		//Rail must have a spline to follow.
		if (collider.gameObject.GetComponentInParent<Spline>())
		{
			_PathSpline = collider.gameObject.GetComponentInParent<Spline>();

			Vector3 offSet = Vector3.zero;

			//Debug.DrawRay(collider.transform.position, Vector3.up * 10, UnityEngine.Color.red, 10f);
			//Debug.DrawRay(transform.position, Vector3.up * 10, UnityEngine.Color.blue, 10f);

			//Trigger Collisions on rails are organised by the PlaceOnSpline script, so if that had an offset to move the rail collision away from the spline, then follow that (this allows for multiple rails next to each other all following the same spline).
			if (isTrigger)
			{
				if (collider.gameObject.GetComponentInParent<S_PlaceOnSpline>())
				{
					offSet.x = collider.gameObject.GetComponentInParent<S_PlaceOnSpline>()._offset3d_.x;
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
				if (addOn.UseNextRail == null && addOn.UsePrevRail == null) { addOn = null; }
			}

			//Sets the player to the rail grind action, and sets their position and what spline to follow.
			_RailAction.AssignForThisGrind(rangeAndDis.x, _PathSpline.transform, PathTypes.rail, offSet, addOn);
			_RailAction.StartAction();
		}
	}

	//Readies stats for movemement on zipline and the zipline itself.
	private void SetOnZipline ( Collider col ) {
		_PathSpline = col.transform.GetComponent<S_Control_Zipline>()._Rail;
		if (_PathSpline == null) { return; }

		//Assigns what is the Zipline and handle, and sets it to not be kinematic to avoid gravity.
		Rigidbody zipbody = col.GetComponent<Rigidbody>();
		_RailAction._RF._ZipHandle = col.transform;
		_RailAction._RF._ZipBody = zipbody;
		zipbody.isKinematic = false;

		Vector2 rangeAndDistanceSquared = S_RailFollow_Base.GetClosestPointOfSpline(zipbody.position, _PathSpline, Vector3.zero); //Gets place on rail closest to collision point.

		//Disables the homing target so it isn't a presence if homing attack can be performed in the grind action
		GameObject target = col.transform.GetComponent<S_Control_Zipline>()._HomingTarget;
		target.SetActive(false);

		//Sets the player to the rail grind action, and sets their position and what spline to follow.
		_RailAction.AssignForThisGrind(rangeAndDistanceSquared.x, _PathSpline.transform, PathTypes.zipline, Vector3.zero, null);
		_RailAction.StartAction();
	}

	private void SetOnPath ( Collider Col ) {
		S_Trigger_Path PathTrigger = Col.GetComponent<S_Trigger_Path>();
		if (!PathTrigger) {return;}
		S_Trigger_Path.StrucAutoPathData PathData = PathTrigger._PathData_;

		//Sets the player to start at the start and move forwards
		bool back = false;

		//If entering an Exit trigger, the player will be set to move backwards and start at the end.
		if (PathTrigger._isExit_)
		{
			back = true;
		}
		//If the trigger entered is pointing towards elsewhere for path data (like an exit would have path data stored on the entrance.
		if(PathTrigger._ExternalPathData_ != null)
		{
			PathData = PathTrigger._ExternalPathData_._PathData_;
		}

		_isCurrentlyInAutoTrigger = true; //To prevent this being called multiple times with the same trigger.
		float speedGo = 0f;

		bool willPlaceOnSpline = false;


		//If the path is being started by a path speed pad
		if (Col.TryGetComponent(out S_Data_SpeedPad SpeedPadScript))
		{
			willPlaceOnSpline = true;
			_PathSpline = SpeedPadScript._Path;
			speedGo = Mathf.Max(SpeedPadScript._speedToSet_, _PlayerVel._horizontalSpeedMagnitude);
		}

		//If the path is being started by a normal trigger
		else
			_PathSpline = PathData.spline;

		//If the player has been given a path to follow. This cuts out speed pads that don't have attached paths.
		if (!_PathSpline) { return; }

		Vector2 rangeAndDistanceSquared = S_RailFollow_Base.GetClosestPointOfSpline(transform.position, _PathSpline, Vector2.zero);

		if (PathData._removeVerticalVelocityOnStart_)
		{
			_PlayerVel.SetCoreVelocity(new Vector3(_PlayerVel._coreVelocity.x, 0, _PlayerVel._coreVelocity.z));
		}

		//Starts the player moving along the path using the path follow action
		_PathAction.AssignForThisAutoPath(rangeAndDistanceSquared.x, _PathSpline.transform, back, speedGo, PathData, willPlaceOnSpline);
		_PathAction.StartAction();
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
		Vector3 pointSum = Vector3.zero;

		// Iterate over each ContactPoint
		for (int i = 0 ; i < col.contacts.Length ; i++)
		{
			ContactPoint contact = col.contacts[i];

			// Add all collision points together
			for (int i2 = 0 ; i2 < col.contacts.Length ; i2++)
			{
				pointSum += col.contacts[i2].point;
			}

			// Divide by the number of points to get the average
			pointSum /= col.contacts.Length;
			CollisionPointTransform.position = pointSum;
		}
		return CollisionPointTransform;
	}

	//Called when leaving a pulley to prevent player attaching to it immediately.
	public IEnumerator JumpFromZipLine ( Transform zipHandle, float time ) {

		//Disables the zipline handles collisions and homing target until after the delay
		zipHandle.GetComponent<CapsuleCollider>().enabled = false;
		GameObject target = zipHandle.transform.GetComponent<S_Control_Zipline>()._HomingTarget;
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
			_Tools = GetComponentInParent<S_CharacterTools>();
			AssignTools();
			AssignStats();
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_PlayerVel = _Tools.GetComponent<S_PlayerVelocity>();
		_Actions = _Tools._ActionManager;

		//Can afford to directly search for these actions as they will only be read if their AttemptAcions are called.
		_Actions._ObjectForActions.TryGetComponent(out _RailAction);
		_Actions._ObjectForActions.TryGetComponent(out _PathAction);
		_Actions._ObjectForActions.TryGetComponent(out _UpreelAction);

		_CharacterAnimator = _Tools.CharacterAnimator;
		_characterCapsule = _Tools.CharacterCapsule.GetComponent<Collider>();
		_FeetTransform = _Tools.FeetPoint;
		_MainSkin = _Tools.MainSkin;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {

	}
	#endregion

}



