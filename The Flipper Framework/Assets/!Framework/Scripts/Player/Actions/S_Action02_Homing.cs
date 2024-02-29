using UnityEngine;
using System.Collections;

public class S_Action02_Homing : MonoBehaviour, IMainAction
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
	private S_VolumeTrailRenderer _HomingTrailScript;
	private Animator		 _CharacterAnimator;
	private S_Handler_HomingAttack _HomingControl;
	private S_Control_PlayerSound	 _Sounds;

	private GameObject		_HomingTrailContainer;
	private GameObject		_JumpBall;
	[HideInInspector]
	public Transform		_Target;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	private bool	_isAdditive_;
	private float	_homingAttackSpeed_;
	private float	_airDashSpeed_;
	private float	_airDashDuration_;
	private float	_homingTimerLimit_;
	private float	_facingAmount;
	private bool        _CanBePerformedOnGround_;
	#endregion

	// Trackers
	#region trackers
	private float	_XZmag;
	public float	_lateSpeed;
	public Vector3      _targetDirection;
	private float	_timer;
	private float	_speed;
	private float	_storedSpeed;
	private Vector3	_direction;
	private Vector3	_newRotation;

	public float	_skinRotationSpeed;
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

	}

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		_isHomingAvailable = true;

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
		_PlayerPhys._isGravityOn = false;

		//Set Animator Parameters
		_Actions.Action00.HandleAnimator(1);
		//_CharacterAnimator.SetInteger("Action", 1);
		//_CharacterAnimator.SetFloat("YSpeed", _PlayerPhys._RB.velocity.y);
		//_CharacterAnimator.SetFloat("GroundSpeed", _PlayerPhys._RB.velocity.magnitude);
		//_CharacterAnimator.SetBool("Grounded", _PlayerPhys._isGrounded);

		//Set Animation Angle
		_Actions.Action00.SetSkinRotation(_skinRotationSpeed);
		//Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
		//Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
		//_CharacterAnimator.transform.rotation = Quaternion.Lerp(_CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * _skinRotationSpeed);
	}

	private void FixedUpdate () {
		_timer += Time.deltaTime;

		//Ends homing attack if in air for too long or target is lost
		if (_Target == null || _timer > _homingTimerLimit_)
		{
			_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Default);
		}

		//direction = Target.position - transform.position;
		//Player.p_rigidbody.velocity = direction.normalized * Speed;

		//Debug.Log (Player.SpeedMagnitude);

		_direction = _Target.position - transform.position;
		_newRotation = Vector3.RotateTowards(_newRotation, _direction, 1f, 0.0f);
		//Player._RB.velocity = newRotation * Speed;
		_PlayerPhys.SetCoreVelocity(_newRotation * _speed);


		//Set Player location when close enough, for precision.
		if (_Target != null && Vector3.Distance(_Target.transform.position, transform.position) < (_speed * Time.fixedDeltaTime) && _Target.gameObject.activeSelf)
		{
			transform.position = _Target.transform.position;
		}
		else
		{
			//LateSpeed = Mathf.Max(XZmag, HomingAttackSpeed);
			_lateSpeed = Mathf.Max(_XZmag, _homingAttackSpeed_);
		}
	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		if(_PlayerPhys._isGrounded || _CanBePerformedOnGround_)
		{
			if (_HomingControl._HasTarget && _Input.HomingPressed && _Actions.Action02._isHomingAvailable)
			{
				//Do a homing attack
				if (_Actions.Action02 != null && _PlayerPhys._homingDelay_ <= 0)
				{
					_Sounds.HomingAttackSound();
					_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Homing);
					InitialEvents();
					willChangeAction = true;
				}
			}
		}
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
		_HomingControl = GetComponent<S_Handler_HomingAttack>();
		_Sounds = GetComponent<S_Control_PlayerSound>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_HomingTrailScript = _Tools.HomingTrailScript;
		_HomingTrailContainer = _Tools.HomingTrailContainer;
		_JumpBall = _Tools.JumpBall;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_isAdditive_ = _Tools.Stats.JumpDashStats.shouldUseCurrentSpeedAsMinimum;
		_homingAttackSpeed_ = _Tools.Stats.HomingStats.attackSpeed;
		_airDashSpeed_ = _Tools.Stats.JumpDashStats.dashSpeed;
		_homingTimerLimit_ = _Tools.Stats.HomingStats.timerLimit;
		_airDashDuration_ = _Tools.Stats.JumpDashStats.duration;
		_CanBePerformedOnGround_ = _Tools.Stats.HomingStats.CanBePerformedOnGround;
	}
	#endregion


	public void InitialEvents () {
		if (!_Actions.lockHoming)
		{

			_HomingTrailScript.emitTime = _airDashDuration_;
			_HomingTrailScript.emit = true;


			_JumpBall.SetActive(false);


			if (_Actions.Action02Control._HasTarget)
			{
				_Target = _HomingControl._TargetObject.transform;
				_targetDirection = (_Target.transform.position - _PlayerPhys._playerPos).normalized;
			}
			else
			{
				_targetDirection = _PlayerPhys._RB.velocity.normalized;
			}

			_timer = 0;
			_isHomingAvailable = false;

			_XZmag = _PlayerPhys._horizontalSpeedMagnitude;




			//Action.actionDisable();

			//Vector3 TgyXY = HomingControl.TargetObject.transform.position.normalized;
			//TgyXY.y = 0;
			//float facingAmmount = Vector3.Dot(Player.PreviousRawInput.normalized, TgyXY);

			_direction = _Target.position - transform.position;
			_newRotation = Vector3.RotateTowards(transform.forward, _direction, 5f, 0.0f);

			// //Debug.Log(facingAmmount);
			// if (facingAmmount < FacingAmount) { IsAirDash = true; }

			if (_XZmag * 0.7f < _homingAttackSpeed_)
			{
				_speed = _homingAttackSpeed_;
				_storedSpeed = _speed;
			}
			else
			{
				_speed = _XZmag * 0.7f;
				_storedSpeed = _XZmag;
			}
		}

	}

	public void ResetHomingVariables () {
		_timer = 0;
		_HomingTrailContainer.transform.DetachChildren();
		//IsAirDash = false;
	}


}
