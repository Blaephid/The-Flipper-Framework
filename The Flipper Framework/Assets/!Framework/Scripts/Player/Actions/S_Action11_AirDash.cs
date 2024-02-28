using UnityEngine;
using System.Collections;

public class S_Action11_AirDash : MonoBehaviour, IMainAction
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
	private S_Action02_Homing	_Action02;
	private S_Handler_Camera	_CamHandler;
	private S_Control_PlayerSound	_Sounds;

	private Animator	_CharacterAnimator;
	private GameObject	_HomingTrailContainer;
	private GameObject	_JumpDashParticle;
	private GameObject	_JumpBall;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	[HideInInspector]
	public bool _isAdditive_;
	[HideInInspector]
	public float _AirDashSpeed_;
	[HideInInspector]
	public float _AirDashDuration_;
	#endregion

	// Trackers
	#region trackers
	public float	_skinRotationSpeed;

	private float	_XZspeed;
	private float	_timer;
	private float	_aSpeed;
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
		_Actions.Action00.HandleAnimator(11);

		_Actions.Action00.SetSkinRotation(_skinRotationSpeed);

		//Set Animation Angle
		//Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
		//if (VelocityMod != Vector3.zero)
		//{
		//	Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
		//	_CharacterAnimator.transform.rotation = Quaternion.Lerp(_CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * _skinRotationSpeed);
		//}
	}

	private void FixedUpdate () {
		_timer += Time.deltaTime;

		if (_Input._inputWithoutCamera != Vector3.zero)
		{
			if (_timer > 0.03)
			{

				Vector3 FaceDir = _CharacterAnimator.transform.position - _CamHandler._HedgeCam.transform.position;
				bool Facing = Vector3.Dot(_CharacterAnimator.transform.forward, FaceDir.normalized) < 0f;
				if (Facing)
				{
					_Input._inputWithoutCamera.x = -_Input._inputWithoutCamera.x;
				}


				//Direction = CharacterAnimator.transform.forward;
				_direction = Vector3.RotateTowards(new Vector3(_direction.x, 0, _direction.z), _CharacterAnimator.transform.right, Mathf.Clamp(_Input._inputWithoutCamera.x * 4, -2.5f, 2.5f) * Time.deltaTime, 0f);
			}

			//Direction.y = Player.fallGravity.y * 0.1f;

		}
		else
		{
			//Direction = (transform.TransformDirection(Player.PreviousRawInput).normalized + (Player.rb.velocity).normalized * 2);
			//Direction.y = Player.fallGravity.y * 0.1f;

		}

		Vector3 newVec = _direction.normalized * _aSpeed;
		if (_PlayerPhys._RB.velocity.y < 0)
			newVec.y = _PlayerPhys._currentFallGravity.y * 0.5f;

		_PlayerPhys.SetCoreVelocity(newVec);

		//End homing attck if in air for too long
		if (_timer > _AirDashDuration_)
		{
			_JumpBall.SetActive(true);
			_Actions.ChangeAction(S_Enums.PlayerStates.Jump);
		}
		else if (_PlayerPhys._isGrounded)
		{
			_CharacterAnimator.SetInteger("Action", 0);
			_CharacterAnimator.SetBool("Grounded", _PlayerPhys._isGrounded);
			_Actions.Action00.StartAction();
			_Actions.ChangeAction(S_Enums.PlayerStates.Regular);
		}
	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		if (!_Actions.Action02._isHomingAvailable && _Input.SpecialPressed)
		{
			if (!_Actions.Action02Control._HasTarget)
			{
				_Sounds.AirDashSound();
				_Actions.ChangeAction(S_Enums.PlayerStates.JumpDash);
				InitialEvents();
				willChangeAction = true;
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
		_Action02 = GetComponent<S_Action02_Homing>();
		_CamHandler = GetComponent<S_Handler_Camera>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_HomingTrailScript = _Tools.HomingTrailScript;
		_HomingTrailContainer = _Tools.HomingTrailContainer;
		_JumpBall = _Tools.JumpBall;
		_JumpDashParticle = _Tools.JumpDashParticle;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_isAdditive_ = _Tools.Stats.JumpDashStats.shouldUseCurrentSpeedAsMinimum;
		_AirDashSpeed_ = _Tools.Stats.JumpDashStats.dashSpeed;
		_AirDashDuration_ = _Tools.Stats.JumpDashStats.duration;
	}
	#endregion

	void Awake () {
		if (_PlayerPhys == null)
		{
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();

			AssignStats();
		}

		_Action02._isHomingAvailable = true;
	}

	public void InitialEvents () {
		if (!_Actions.lockJumpDash)
		{
			if (_Action02._isHomingAvailable)
			{

				_HomingTrailScript.emitTime = _AirDashDuration_ + 0.5f;
				_HomingTrailScript.emit = true;

				_JumpBall.SetActive(false);
				_Input.SpecialPressed = false;
				_Input.HomingPressed = false;

				_timer = 0;
				_Action02._isHomingAvailable = false;

				_XZspeed = _PlayerPhys._horizontalSpeedMagnitude;

				AirDashParticle();

				if (_XZspeed < _AirDashSpeed_)
				{
					_aSpeed = _AirDashSpeed_;
				}
				else
				{
					_aSpeed = _XZspeed;
				}

				_direction = _CharacterAnimator.transform.forward;
				//Direction = Vector3.RotateTowards(Direction, lateralToInput * Direction, turnRate * 40f, 0f);


			}
			else
			{
				//Action.ChangeAction(Action.PreviousAction);
			}
		}

	}

	public void AirDashParticle () {
		GameObject JumpDashParticleClone = Instantiate(_JumpDashParticle, _HomingTrailContainer.transform.position, Quaternion.identity) as GameObject;
		//if (Player.SpeedMagnitude > 60)
		//    JumpDashParticleClone.GetComponent<ParticleSystem>().startSize = Player.SpeedMagnitude / 60f;
		//else
		//    JumpDashParticleClone.GetComponent<ParticleSystem>().startSize = 1f;

		if (_PlayerPhys._speedMagnitude > 60)
			JumpDashParticleClone.transform.localScale = new Vector3(_PlayerPhys._speedMagnitude / 60f, _PlayerPhys._speedMagnitude / 60f, _PlayerPhys._speedMagnitude / 60f);
		//else
		//    JumpDashParticleClone.GetComponent<ParticleSystem>().startSize = 1f;

		JumpDashParticleClone.transform.position = _HomingTrailContainer.transform.position;
		JumpDashParticleClone.transform.rotation = _HomingTrailContainer.transform.rotation;
		//JumpDashParticleClone.transform.parent = HomingTrailContainer.transform;
	}

}
