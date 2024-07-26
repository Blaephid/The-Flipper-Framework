using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class S_Handler_HomingAttack : MonoBehaviour
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

	private AudioSource           _IconSound;
	private Animator              _IconAnim;
	private Animator              _CharacterAnimator;
	private Transform             _MainSkin;

	private Transform             _MainCamera;

	private Transform             _IconTransform;
	private GameObject            _NormalIcon;
	private GameObject            _DamageIcon;

	[HideInInspector]
	public Transform            _TargetObject;                //The target set at the end of an update
	[HideInInspector]
	public Transform		_PreviousTarget;
	private Transform            _targetPlayedAnimationOn;     //If different to the current target, then play animation and set to current target so it doesn't happen again until new target.
	#endregion

	//General
	#region General Properties

	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private float       _targetSearchDistance_ = 10;
	private float       _faceRange_ = 66;
	private int         _minTargetDistance_;
	private int         _maxTargetDistance_;
	private float       _currentTargetPriority_;
	private float       _cameraDirectionPriority_;
	private LayerMask   _TargetLayer_;
	private LayerMask   _BlockingLayers_;
	private float       _facingAmount_;
	[HideInInspector]
	public float        _homingDelay_;
	private float       _iconScale_;
	private float       _iconDistanceScaling_;
	private Vector2     _timeToKeepTarget_;
	private float       _timeBetweenScans_;
	private int         _radiusOfCameraTargetCheck_;
	#endregion

	// Trackers
	#region trackers

	[HideInInspector]
	public bool         _isScanning = false;          //Will only go through the target searching and calculations when this is true

	private float       _currentTargetDistance;
	[HideInInspector]
	public bool         _isHomingAvailable;

	private float       _counterWithThisTarget;       //Increases when there is a target, and is reset when target changes. Used to make sure targets can't change until they've been targets for long enough.
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

		_IconTransform.parent = null; //Makes it seperate to player object so it doesn't move with them.

		StartCoroutine(ScanForTargets(_timeBetweenScans_)); //For efficiency, this is not done every frame, instead being every x seconds.
	}

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyScript();
	}
	private void OnDisable () {

	}

	// Update is called once per frame
	void Update () {
	}


	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	private IEnumerator ScanForTargets ( float secondsBetweenChecks ) {
		//Will constantly be checking, but only performing calculations if isScanning
		while (true)
		{
			yield return new WaitForSeconds(.02f);
			yield return new WaitForEndOfFrame();
			//Determined in the homing action script, based on if attempt action is called, which means this only updates if the current action can perform homing attacks.
			if (_isScanning)
			{
				UpdateHomingTargets();

				//Wait until next check, taking longer if no object yet as it needs to be quicker if multiple are around.
				if (_TargetObject)
				{
					_counterWithThisTarget += secondsBetweenChecks;
					yield return new WaitForSeconds(secondsBetweenChecks);
				}
				else
				{
					yield return new WaitForSeconds(secondsBetweenChecks * 1.5f);
				}
			}
			//If not scanning then there can't be a target
			else if (_TargetObject)
			{
				_counterWithThisTarget = 0;
				_TargetObject = null;
				_PreviousTarget = null;
				UpdateHomingReticle();
			}

			_isScanning = false; //Set to false every frame but will be counteracted in Action homing's AttemptAction()
		}
	}

	//Handles
	private void UpdateHomingTargets () {

		_TargetObject = GetClosestTarget(_TargetLayer_, _targetSearchDistance_);

		DelayingTargetSwitch();

		UpdateHomingReticle();

	}

	//Checks for potential target then finds the closest.
	private Transform GetClosestTarget ( LayerMask TargetMask, float radius ) {

		Transform closestTarget = null;
		_currentTargetDistance = 0;
		int checkLimit = 0;

		//First, send a spherecast in direction camera is facing, this has more range than normal checks. This takes pririty as it allows for precision.
		RaycastHit[] NewTargetsInRange = Physics.SphereCastAll(transform.position, _radiusOfCameraTargetCheck_, _MainCamera.forward, _faceRange_ * radius, TargetMask);
		if (NewTargetsInRange.Length > 0)
		{
			foreach (RaycastHit hit in NewTargetsInRange)
			{
				float distance = hit.distance;
				//If hit has the homing target component and is far enough away, then compare to current closest.
				if (hit.collider.gameObject.GetComponent<S_Data_HomingTarget>() && distance > _minTargetDistance_)
				{
					Transform newTarget = hit.collider.transform;
					closestTarget = CheckTarget(newTarget, distance * _cameraDirectionPriority_, closestTarget, _facingAmount_);
				}

				//For efficiency, cannot check more than 3 objects
				checkLimit++;
				if (checkLimit > 3) { break; }
			}
		}
		checkLimit = 0;

		//If nothing found yet, check fir all potential targets around the player.
		Collider[] TargetsInRange = Physics.OverlapSphere(transform.position, radius, TargetMask);
		if(TargetsInRange.Length > 0)
		{
			foreach (Collider hit in TargetsInRange)
			{
				float distance = Vector3.Distance(transform.position, hit.transform.position);

				//If has the homing target component and is far enough away, then compare to current closest.
				if (hit.gameObject.GetComponent<S_Data_HomingTarget>() && distance > _minTargetDistance_)
				{
					Transform newTarget = hit.gameObject.transform;
					closestTarget = CheckTarget(newTarget, distance, closestTarget, _facingAmount_);
				}

				//For efficiency, cannot check more than 3 objects
				checkLimit++;
				if (checkLimit > 3) { break; }

			}
		}

		//If there is currently already a target, compare it to the new closest, with a modification to distance that makes it seem closer, and therefore higher priority.
		if (_PreviousTarget != null)
		{
			float distance = Vector3.Distance(transform.position, _PreviousTarget.transform.position);
			closestTarget = CheckTarget(_PreviousTarget.transform, distance * _currentTargetPriority_, closestTarget, _facingAmount_);
		}

		return closestTarget;
	}

	//Takes in a target and return the closer of it or the current one.
	private Transform CheckTarget ( Transform newTarget, float distance, Transform closest, float facingAmount, bool skipIsOnScreen = false ) {

		//If this new target is out of the maximum range, then ignore it, no matter the check. Gets its own distance because the distance parameter won't always be the exact distance.
		if (Vector3.Distance(transform.position, newTarget.position) > _maxTargetDistance_ ) { return closest; } 
		//Make sure Sonic is facing the target enough
		Vector3 direction = (newTarget.position - transform.position).normalized;
		float angle = Vector3.Angle(new Vector3(_MainSkin.forward.x, 0, _MainSkin.forward.z), new Vector3 (direction.x, 0, direction.z));
		bool isFacing = angle < facingAmount;
		
		bool isOnScreen = true;
		if (!skipIsOnScreen)
		{
			//Make sure the target is on screen
			Vector3 screenPoint = _MainCamera.GetComponent<Camera>().WorldToViewportPoint(newTarget.position);
			isOnScreen = screenPoint.z > 0.3f && screenPoint.x > 0.08 && screenPoint.x < 0.92f && screenPoint.y > 0f && screenPoint.y < 0.95f;
		}

		//If the above are true, and the distance to this new target is less than the one to the closest, this becomes the target.
		if ((distance < _currentTargetDistance || _currentTargetDistance == 0f) && isFacing && isOnScreen)
		{
			SetTarget();
		}

		return closest;

		//Makes final checks and sets the new target and its distance
		void SetTarget () {
			//Checks if the target is accessible.
			if (!Physics.Linecast(transform.position, newTarget.position, _BlockingLayers_) && distance < _maxTargetDistance_)
			{
				_currentTargetDistance = distance;
				closest = newTarget;
			}
		}
	}

	//Prevents targets from changing too quickly.
	private void DelayingTargetSwitch() {
		if (_PreviousTarget)
		{
			//If there is no current target but there is still a previous target
			if (!_TargetObject)
			{
				//Then check the timer and keep the target the same if still under. But the target must still be on screen and within face range.
				if (_counterWithThisTarget < _timeToKeepTarget_.y) { _TargetObject = CheckTarget(_PreviousTarget, 0, null, 160, true); }
			}
			//If the target has changed, then once time has expired, reset the counter.
			else if (_TargetObject != _PreviousTarget)
			{
				if (_counterWithThisTarget < _timeToKeepTarget_.x)
				{
					_TargetObject = CheckTarget(_PreviousTarget, 0, _TargetObject, 160, false);
				}
				else
				{
					_PreviousTarget = _TargetObject;
					_counterWithThisTarget = 0;
				}
			}
		}
		else
		{
			_PreviousTarget = _TargetObject;
			_counterWithThisTarget = 0;
		}
	}


	#endregion


	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//Handles the location, animations and effects of the homing reticle, based on whether or not there is a current target
	public void UpdateHomingReticle () {
		if (_TargetObject != null)
		{
			_IconTransform.position = _TargetObject.transform.position; //Places icon on target

			//Effects icon size by camera distance
			float camDist = Vector3.Distance(transform.position, _MainCamera.position);
			_IconTransform.localScale = (Vector3.one * _iconScale_) + (Vector3.one * (camDist * _iconDistanceScaling_));

			//If this is a new target, then play sound and animation.
			if (_targetPlayedAnimationOn != _TargetObject)
			{
				_targetPlayedAnimationOn = _TargetObject;
				_IconSound.Play();
				_IconAnim.SetTrigger("NewTgt");
			}

			//Depending on the target, will show a different icon (all are children of the proper target being placed)
			switch (_TargetObject.GetComponent<S_Data_HomingTarget>().type)
			{
				case S_Data_HomingTarget.TargetType.normal:
					_DamageIcon.SetActive(false);
					_NormalIcon.SetActive(true);
					break;
				case S_Data_HomingTarget.TargetType.destroy:
					_DamageIcon.SetActive(true);
					_NormalIcon.SetActive(false);
					break;
			}
		}

		//Hide Icon if no target
		else
		{
			_IconTransform.localScale = Vector3.zero;
		}
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//If stats and tools are not assigned yet, assign them now.
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
		_Actions = _Tools.GetComponent<S_ActionManager>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.MainSkin;
		_MainCamera = _Tools.MainCamera;
		_IconTransform = _Tools.homingIcons.transform;
		_NormalIcon = _Tools.NormalIcon;
		_DamageIcon = _Tools.DamageIcon;

		_IconSound = _IconTransform.gameObject.GetComponent<AudioSource>();
		_IconAnim = _IconTransform.gameObject.GetComponent<Animator>();
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_targetSearchDistance_ = _Tools.Stats.HomingSearch.targetSearchDistance;
		_faceRange_ = _Tools.Stats.HomingSearch.distanceModifierInCameraDirection;
		_minTargetDistance_ = _Tools.Stats.HomingSearch.minimumTargetDistance;
		_maxTargetDistance_ = _Tools.Stats.HomingSearch.maximumTargetDistance;
		_TargetLayer_ = _Tools.Stats.HomingSearch.TargetLayer;
		_BlockingLayers_ = _Tools.Stats.HomingSearch.blockingLayers;
		_facingAmount_ = _Tools.Stats.HomingSearch.facingAmount;
		_homingDelay_ = _Tools.Stats.HomingStats.successDelay;
		_currentTargetPriority_ = 1 - _Tools.Stats.HomingSearch.currentTargetPriority;
		_cameraDirectionPriority_ = 1 - _Tools.Stats.HomingSearch.cameraDirectionPriority;
		_timeToKeepTarget_ = _Tools.Stats.HomingSearch.timeToKeepTarget;
		_timeBetweenScans_ = _Tools.Stats.HomingSearch.timeBetweenScans;
		_radiusOfCameraTargetCheck_ = _Tools.Stats.HomingSearch.radiusOfCameraTargetCheck;

		_iconScale_ = _Tools.Stats.HomingSearch.iconScale;
		_iconDistanceScaling_ = _Tools.Stats.HomingSearch.iconDistanceScaling;
	}
	#endregion

}

