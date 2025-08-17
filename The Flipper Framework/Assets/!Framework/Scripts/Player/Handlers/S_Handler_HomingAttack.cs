using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

public class S_Handler_HomingAttack : S_Player_Base
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties


	private S_Action02_Homing     _HomingAction;

	private AudioSource           _IconSound;

	private Transform             _MainCamera;

	private GameObject      _IconHUDObject;
	private RectTransform     _IconRectTransform;
	private Animator        _IconHUDAnimator;

	[HideInInspector]
	public Transform            _TargetObject;                //The target set at the end of an update
	[HideInInspector]
	public Transform                _PreviousTarget;
	private Transform            _targetPlayedAnimationOn;     //If different to the current target, then play animation and set to current target so it doesn't happen again until new target.
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private float       _targetSearchDistance_ = 10;
	private float       _faceRange_ = 66;
	private float         _minTargetDistanceSquared_;
	private float         _maxTargetDistanceSquared_;
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

	private float       _currentTargetDistanceSquared;
	[HideInInspector]
	public bool         _isHomingAvailable;//Must be true for scan to be active.
	[HideInInspector]
	public bool         _isHomingLocked; //Can only perform if true, but will still scan and show icons.

	[HideInInspector]
	public float       _timeOnThisTarget;       //Increases when there is a target, and is reset when target changes. Used to make sure targets can't change until they've been targets for long enough.
	#endregion

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	public override void Awake () {
		base.Awake();

		StartCoroutine(ScanForTargets(_timeBetweenScans_)); //For efficiency, this is not done every frame, instead being every x seconds.
	}

	private void Update () {
		if (_TargetObject) { _timeOnThisTarget += Time.deltaTime; }
		else _timeOnThisTarget = 0;
		
		UpdateHomingReticle();
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
			yield return new WaitForEndOfFrame();

			//Determined in the homing action script, based on if attempt action is called, which means this only updates if the current action can perform homing attacks.
			if (_HomingAction._inAStateConnectedToThis && _isHomingAvailable)
			{
				UpdateHomingTargets();

				//Wait until next check, taking longer if no object yet as it needs to be quicker if multiple are around.
				if (_TargetObject)
				{
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
				_TargetObject = null;
				_PreviousTarget = null;
			}
			else
				yield return new WaitForSeconds(secondsBetweenChecks);
		}
	}

	//Handles
	private void UpdateHomingTargets () {

		_TargetObject = GetClosestTarget(_TargetLayer_, _targetSearchDistance_);
		DelayingTargetSwitch();
	}

	//Checks for potential target then finds the closest.
	private Transform GetClosestTarget ( LayerMask TargetMask, float radius ) {

		Transform closestTarget = null;
		_currentTargetDistanceSquared = 0;

		//First, send a spherecast in direction camera is facing, this has more range than normal checks. This takes priority as it allows for precision.
		RaycastHit[] NewTargetsInRange = Physics.SphereCastAll(transform.position, _radiusOfCameraTargetCheck_, _MainCamera.forward, _faceRange_ * radius, TargetMask);
		Collider[] TargetsInRange = new Collider[NewTargetsInRange.Length];
		for (int i = 0 ; i < NewTargetsInRange.Length ; i++)
		{
			TargetsInRange[i] = NewTargetsInRange[i].collider;
		}

		CheckArrayOfTargets(ref TargetsInRange, _cameraDirectionPriority_);

		if (!closestTarget)
		{
			//If nothing found yet, check for all potential targets around the player.
			TargetsInRange = Physics.OverlapSphere(transform.position, radius, TargetMask);

			CheckArrayOfTargets(ref TargetsInRange, 1);
		}

		//If there is currently already a target, compare it to the new closest, with a modification to distance that makes it seem closer, and therefore higher priority.
		if (_PreviousTarget != null)
		{
			float distanceSquared = S_S_MoreMaths.GetDistanceSqrOfVectors(transform.position, _PreviousTarget.transform.position);
			closestTarget = CheckTarget(_PreviousTarget.transform, distanceSquared * _currentTargetPriority_, closestTarget, _facingAmount_);
		}

		return closestTarget;

		void CheckArrayOfTargets ( ref Collider[] targets, float extraPriority ) {
			int checkLimit = 0;

			for (int i = 0 ; i < targets.Length ; i++)
			{
				Collider hit = TargetsInRange[i];
				float distanceSquared = S_S_MoreMaths.GetDistanceSqrOfVectors(transform.position, hit.transform.position);

				//If has the homing target component and is far enough away, then compare to current closest.
				if (hit.gameObject.TryGetComponent(out S_Data_HomingTarget TargetData) && distanceSquared > _minTargetDistanceSquared_)
				{
					Transform checkTarget = hit.gameObject.transform;
					float distanceToUse = (distanceSquared / TargetData._distanceModifier) * extraPriority; //Some targets may need to be closer.
					closestTarget = CheckTarget(checkTarget, distanceToUse, closestTarget, _facingAmount_);
					//Compare 
				}

				//For efficiency, cannot check more than 4 objects
				checkLimit++;
				if (checkLimit == 4) { break; }
			}
		}
	}

	//Takes in a target and return the closer of it or the current one.
	private Transform CheckTarget ( Transform newTarget, float distanceSquared, Transform closest, float facingAmount, bool skipIsOnScreen = false ) {

		//If this new target is out of the maximum range, then ignore it, no matter the check. Gets its own distance because the distance parameter won't always be the exact distance.
		if (S_S_MoreMaths.GetDistanceSqrOfVectors(transform.position, newTarget.position) > _maxTargetDistanceSquared_) { return closest; }


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
		if ((distanceSquared < _currentTargetDistanceSquared || _currentTargetDistanceSquared == 0f) && isFacing && isOnScreen)
		{
			SetTarget();
		}

		return closest;

		//Makes final checks and sets the new target and its distance
		void SetTarget () {
			//Checks if the target is accessible.
			if (!Physics.Linecast(transform.position, newTarget.position, _BlockingLayers_) && distanceSquared < _maxTargetDistanceSquared_)
			{
				_currentTargetDistanceSquared = distanceSquared;
				closest = newTarget;
			}
		}
	}

	//Prevents targets from changing too quickly.
	private void DelayingTargetSwitch () {
		if (_PreviousTarget)
		{
			//If there is no current target but there is still a previous target
			if (!_TargetObject)
			{
				//Then check the timer and keep the target the same if still under. But the target must still be on screen and within face range.
				if (_timeOnThisTarget < _timeToKeepTarget_.y) { _TargetObject = CheckTarget(_PreviousTarget, 0, null, 160, true); }
			}
			//If the target has changed, then once time has expired, reset the counter.
			else if (_TargetObject != _PreviousTarget)
			{
				if (_timeOnThisTarget < _timeToKeepTarget_.x)
				{
					_TargetObject = CheckTarget(_PreviousTarget, 0, _TargetObject, 160, false);
				}
				else
				{
					_PreviousTarget = _TargetObject;
					_timeOnThisTarget = 0;
				}
			}
		}
		else
		{
			_PreviousTarget = _TargetObject;
			_timeOnThisTarget = 0;
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
			Vector3 screenPos = Camera.main.WorldToScreenPoint(_TargetObject.transform.position);
			_IconRectTransform.position = new Vector3(screenPos.x, screenPos.y, 0); //Places icon on target in UI space

			float camDist = screenPos.z;
			float newSize = camDist * _iconDistanceScaling_;
			newSize += _iconScale_;
			newSize = Mathf.Clamp(newSize, 0.1f, _iconScale_ * 3f);

			_IconRectTransform.localScale = Vector3.one * newSize / 20;

			//If this is a new target, then play sound and animation.
			if (_targetPlayedAnimationOn != _TargetObject)
			{
				_targetPlayedAnimationOn = _TargetObject;
				_IconSound.Play();
				_IconHUDAnimator.SetTrigger("EnterNormal");

				//Depending on the target, will show a different icon 
				switch (_TargetObject.GetComponent<S_Data_HomingTarget>().type)
				{
					case S_Data_HomingTarget.TargetType.normal:
						_IconHUDAnimator.SetTrigger("EnterNormal");
						break;
					case S_Data_HomingTarget.TargetType.destroy:
						_IconHUDAnimator.SetTrigger("EnterDestroy");
						break;
				}
			}

			_IconHUDObject.SetActive(true); //Makes sure the icon is visible
		}

		//Hide Icon if no target
		else
		{
			_IconRectTransform.localScale = Vector3.zero; //Set scale to 0, rather than deactivate, to ensure the animator can still be called.
			_targetPlayedAnimationOn = null;
		}
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//Responsible for assigning objects and components from the tools script.
	public override void AssignTools () {
		base.AssignTools();

		_MainCamera = Camera.main.transform;

		_IconHUDObject = _CoreUIElements._HomingIcon;
		_IconSound = _IconHUDObject.GetComponent<AudioSource>();
		_IconHUDAnimator = _IconHUDObject.GetComponent<Animator>();
		_IconRectTransform = _IconHUDObject.GetComponent<RectTransform>();

		_HomingAction = GetComponent<S_Action02_Homing>();
		if (!_HomingAction) { enabled = false; }
	}

	//Reponsible for assigning stats from the stats script.
	public override void AssignStats () {
		base.AssignStats();
		_targetSearchDistance_ = _Tools.Stats.HomingSearch.targetSearchDistance;
		_faceRange_ = _Tools.Stats.HomingSearch.distanceModifierInCameraDirection;
		_minTargetDistanceSquared_ = Mathf.Pow(_Tools.Stats.HomingSearch.minimumTargetDistance, 2);
		_maxTargetDistanceSquared_ = Mathf.Pow(_Tools.Stats.HomingSearch.maximumTargetDistance, 2);
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

