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

	private AudioSource		_IconSound;
	private GameObject		_AlreadyPlayed;
	private Animator		_IconAnim;
	private Animator		_CharacterAnimator;

	private Transform	_MainCamera;

	private Transform	_IconTransform;
	private GameObject	_NormalIcon;
	private GameObject	_DamageIcon;

	[HideInInspector] 
	public GameObject		_TargetObject;
	private GameObject		_PreviousTarget;
	public static GameObject[]	_ListOfTargets;
	[HideInInspector] 
	public GameObject[]		_ListOfTgtDebugs;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	private float	_targetSearchDistance_ = 10;
	private float	_faceRange_ = 66;
	private LayerMask	_TargetLayer_;
	private LayerMask	_BlockingLayers_;
	private float	_facingAmount_;
	public float        _homingDelay_;



	private float	_iconScale_;
	private float	_iconDistanceScaling_;
	#endregion

	// Trackers
	#region trackers

	[HideInInspector]
	public bool	_HasTarget;
	private bool	_isScanning = true;

	private int	_homingCount;
	private float	_distance;
	public float	_delayCounter;
	[HideInInspector]
	public bool         _isHomingAvailable;
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

		_IconTransform.parent = null;
		StartCoroutine(ScanForTargets(.10f));
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

	private void FixedUpdate () {
		if (_delayCounter > 0)
		{
			_delayCounter -= Time.deltaTime;
		}

		//Prevent Homing attack spamming

		_homingCount += 1;

		if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Homing)
		{
			_isHomingAvailable = false;
			_homingCount = 0;
		}
		if (_homingCount > 3)
		{
			_isHomingAvailable = true;
		}


		if (_HasTarget && _TargetObject != null)
		{
			_IconTransform.position = _TargetObject.transform.position;
			float camDist = Vector3.Distance(transform.position, _MainCamera.position);
			_IconTransform.localScale = (Vector3.one * _iconScale_) + (Vector3.one * (camDist * _iconDistanceScaling_));

			if (_AlreadyPlayed != _TargetObject)
			{
				_AlreadyPlayed = _TargetObject;
				_IconSound.Play();
				_IconAnim.SetTrigger("NewTgt");
			}
		}
		else
		{
			_IconTransform.localScale = Vector3.zero;
		}
	}


	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	private IEnumerator ScanForTargets ( float secondsBetweenChecks ) {
		while (_isScanning)
		{

			while (!_PlayerPhys._isGrounded && _Actions.whatAction != S_Enums.PrimaryPlayerStates.Rail)
			{
				UpdateHomingTargets();
				if (!_HasTarget)
					yield return new WaitForSeconds(secondsBetweenChecks);
				else
				{
					//Debug.Log(Vector3.Distance(transform.position, TargetObject.transform.position));
					yield return new WaitForSeconds(secondsBetweenChecks * 1.5f);
				}
			}
			_PreviousTarget = null;
			_HasTarget = false;
			yield return new WaitForSeconds(.1f);
		}


	}

	//This function will look for every possible homing attack target in the whole level. 
	//And you can call it from other scritps via [ HomingAttackControl.UpdateHomingTargets() ]
	private void UpdateHomingTargets () {
		if (_Actions.Action02.enabled == true) { return; }

		_HasTarget = false;
		_TargetObject = null;
		_TargetObject = GetClosestTarget(_TargetLayer_, _targetSearchDistance_);
		_PreviousTarget = _TargetObject;

	}

	private GameObject GetClosestTarget ( LayerMask layer, float Radius ) {
		///First we use a spherecast to get every object with the given layer in range. Then we go through the
		///available targets from the spherecast to find which is the closest to Sonic.

		GameObject closestTarget = null;
		_distance = 0f;
		int checkLimit = 0;
		RaycastHit[] NewTargetsInRange = Physics.SphereCastAll(transform.position, 10f, Camera.main.transform.forward, _faceRange_, layer);
		foreach (RaycastHit t in NewTargetsInRange)
		{
			if (t.collider.gameObject.GetComponent<S_Data_HomingTarget>())
			{

				Transform target = t.collider.transform;
				closestTarget = CheckTarget(target, Radius, closestTarget, 1.5f);
			}

			checkLimit++;
			if (checkLimit > 3)
				break;
		}

		checkLimit = 0;
		if (closestTarget == null)
		{
			Collider[] TargetsInRange = Physics.OverlapSphere(transform.position, Radius, layer);
			foreach (Collider t in TargetsInRange)
			{

				if (t.gameObject.GetComponent<S_Data_HomingTarget>())
				{

					Transform target = t.gameObject.transform;
					closestTarget = CheckTarget(target, Radius, closestTarget, 1);
				}

				checkLimit++;
				if (checkLimit > 3)
					break;

			}

			if (_PreviousTarget != null)
			{

				closestTarget = CheckTarget(_PreviousTarget.transform, Radius, closestTarget, 1.3f);
			}
		}

		return closestTarget;
	}

	private GameObject CheckTarget ( Transform target, float Radius, GameObject closest, float maxDisMod ) {
		Vector3 Direction = _CharacterAnimator.transform.position - target.position;
		float TargetDistance = (Direction.sqrMagnitude / Radius) / Radius;

		if (TargetDistance < maxDisMod * Radius)
		{
			bool Facing = Vector3.Dot(_CharacterAnimator.transform.forward, Direction.normalized) < _facingAmount_; //Make sure Sonic is facing the target enough

			Vector3 screenPoint = Camera.main.WorldToViewportPoint(target.position); //Get the target's screen position
			bool onScreen = screenPoint.z > 0.3f && screenPoint.x > 0.08 && screenPoint.x < 0.92f && screenPoint.y > 0f && screenPoint.y < 0.95f; //Make sure the target is on screen

			if ((TargetDistance < _distance || _distance == 0f) && Facing && onScreen)
			{
				if (!Physics.Linecast(transform.position, target.position, _BlockingLayers_))
				{
					_HasTarget = true;
					//Debug.Log(closestTarget);
					_distance = TargetDistance;
					return target.gameObject;
				}
			}
		}

		return closest;
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

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

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainCamera = _Tools.MainCamera;
		_IconTransform = _Tools.homingIcons.transform;
		_NormalIcon = _Tools.normalIcon;
		_DamageIcon = _Tools.weakIcon;

		_IconSound = _IconTransform.gameObject.GetComponent<AudioSource>();
		_IconAnim = _IconTransform.gameObject.GetComponent<Animator>();
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_targetSearchDistance_ = _Tools.Stats.HomingSearch.targetSearchDistance;
		_faceRange_ = _Tools.Stats.HomingSearch.faceRange;
		_TargetLayer_ = _Tools.Stats.HomingSearch.TargetLayer;
		_BlockingLayers_ = _Tools.Stats.HomingSearch.blockingLayers;
		_facingAmount_ = _Tools.Stats.HomingSearch.facingAmount;
		_homingDelay_ = _Tools.Stats.HomingStats.successDelay;

		_iconScale_ = _Tools.Stats.HomingSearch.iconScale;
		_iconDistanceScaling_ = _Tools.Stats.HomingSearch.iconDistanceScaling;
	}
	#endregion

}

