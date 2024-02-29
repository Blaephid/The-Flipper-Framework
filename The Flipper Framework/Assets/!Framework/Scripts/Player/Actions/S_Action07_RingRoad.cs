using UnityEngine;
using System.Collections;

[RequireComponent(typeof(S_ActionManager))]
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

	public Transform	_Target;
	private Animator	_CharacterAnimator;
	private GameObject	_HomingTrailContainer;
	public GameObject	_HomingTrail;
	private GameObject	_JumpBall;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	private float	_dashSpeed_;
	private float	_endingSpeedFactor_;
	private float	_minimumEndingSpeed_;
	#endregion

	// Trackers
	#region trackers
	public float	_skinRotationSpeed = 1;
	public Vector3	_trailOffSet = new Vector3(0,-3,0);
	private float	_initialVelocityMagnitude;
	private Vector3	_direction;
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
		if (_PlayerPhys == null)
		{
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();
		}
	}
	private void OnDisable () {

	}

	// Update is called once per frame
	void Update () {
		//Set Animator Parameters
		_CharacterAnimator.SetInteger("Action", 7);
		_CharacterAnimator.SetFloat("YSpeed", _PlayerPhys._RB.velocity.y);
		_CharacterAnimator.SetFloat("GroundSpeed", _PlayerPhys._RB.velocity.magnitude);
		_CharacterAnimator.SetBool("Grounded", _PlayerPhys._isGrounded);

		//Set Animation Angle
		Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, _PlayerPhys._RB.velocity.y, _PlayerPhys._RB.velocity.z);
		if (VelocityMod != Vector3.zero)
		{
			Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
			_CharacterAnimator.transform.rotation = Quaternion.Lerp(_CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * _skinRotationSpeed);
		}
	}

	private void FixedUpdate () {
		//Timer += 1;

		_Input.LockInputForAWhile(1f, true);

		//CharacterAnimator.SetInteger("Action", 1);
		if (_Actions.Action07Control.TargetObject != null)
		{
			_Target = _Actions.Action07Control.TargetObject.transform;
			_direction = _Target.position - transform.position;
			_PlayerPhys._RB.velocity = _direction.normalized * _dashSpeed_;

			GetComponent<S_Handler_Camera>()._HedgeCam.GoBehindCharacter(7, 30f, true);
		}

		else
		{
			float EndingSpeedResult = 0;

			EndingSpeedResult = Mathf.Max(_minimumEndingSpeed_, _initialVelocityMagnitude);

			_PlayerPhys._RB.velocity = Vector3.zero;
			_PlayerPhys._RB.velocity = _direction.normalized * EndingSpeedResult * _endingSpeedFactor_;

			//GetComponent<CameraControl>().Cam.SetCamera(direction.normalized, 2.5f, 20, 5f,10);

			for (int i = _HomingTrailContainer.transform.childCount - 1 ; i >= 0 ; i--)
				Destroy(_HomingTrailContainer.transform.GetChild(i).gameObject);

			GetComponent<S_PlayerInput>().LockInputForAWhile(10, true);

			_CharacterAnimator.SetInteger("Action", 0);
			_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Default);
		}
	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		willChangeAction = true;
		return willChangeAction;
	}

	public void StartAction () {

	}

	public void StopAction () {

	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	public void HandleInputs () {

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

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Input = GetComponent<S_PlayerInput>();
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Actions = GetComponent<S_ActionManager>();
		_Actions = GetComponent<S_ActionManager>();

		_HomingTrailContainer = _Tools.HomingTrailContainer;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_JumpBall = _Tools.JumpBall;
		_HomingTrail = _Tools.HomingTrail;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_dashSpeed_ = _Tools.Stats.RingRoadStats.dashSpeed;
		_endingSpeedFactor_ = _Tools.Stats.RingRoadStats.endingSpeedFactor;
		_minimumEndingSpeed_ = _Tools.Stats.RingRoadStats.minimumEndingSpeed;
	}
	#endregion

	public void InitialEvents () {
		_initialVelocityMagnitude = _PlayerPhys._RB.velocity.magnitude;
		_PlayerPhys._RB.velocity = Vector3.zero;

		_JumpBall.SetActive(false);
		if (_HomingTrailContainer.transform.childCount < 1)
		{
			GameObject HomingTrailClone = Instantiate (_HomingTrail, _HomingTrailContainer.transform.position, Quaternion.identity) as GameObject;
			HomingTrailClone.transform.parent = _HomingTrailContainer.transform;
			HomingTrailClone.transform.localPosition = _trailOffSet;
		}

		if (_Actions.Action07Control.HasTarget && _Target != null)
		{
			_Target = _Actions.Action07Control.TargetObject.transform;
		}

	}

}
