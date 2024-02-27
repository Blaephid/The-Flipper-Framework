using UnityEngine;
using System.Collections;

public class S_Action00_Regular : MonoBehaviour
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties

	private Animator		_CharacterAnimator;
	private S_CharacterTools	_Tools;
	private S_PlayerPhysics	_PlayerPhys;
	private S_PlayerInput	_Input;
	private S_ActionManager	_Actions;
	private S_Handler_Camera	_CamHandler;
	private S_Handler_quickstep	_QuickstepManager;
	private S_Control_PlayerSound _Sounds;
	private GameObject            _CharacterCapsule;
	private GameObject            _RollingCapsule;
	private S_Action01_Jump       _JumpAction;


	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	public float	_skinRotationSpeed;
	private float	_MaximumSpeedForSpinDash_;
	private float	_MaximumSlopeForSpinDash_;
	private bool	_CanDashDuringFall_;
	private AnimationCurve _CoyoteTimeBySpeed_;

	#endregion

	// Trackers
	#region trackers

	private Quaternion CharRot;
	private RaycastHit hit;

	//Coyote
	private bool coyoteInEffect = false;
	private Vector3 coyoteRememberDir;
	private float coyoteRememberSpeed;
	private bool _isInCoyote = false;

	//Used to prevent rolling sound from constantly playing.
	[HideInInspector] public bool _isRolling = false;
	[HideInInspector] public float rollCounter;
	private float minRollTime = 0.3f;
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
		_Tools = GetComponent<S_CharacterTools>();
		AssignTools();
		AssignStats();
	}

	// Update is called once per frame
	void Update () {

		HandleAnimator();
		SetSkinRotation();
		handleInputs();
	}

	private void FixedUpdate () {
		if (_PlayerPhys._speedMagnitude < 15 && _Input._move == Vector3.zero && _PlayerPhys._isGrounded)
		{
			_Actions.skid._hasSked = false;
		}

		//Skidding

		if (_PlayerPhys._isGrounded)
			_Actions.skid.RegularSkid();
		else
			_Actions.skid.jumpSkid();

		//Set Homing attack to true
		if (_PlayerPhys._isGrounded)
		{
			readyCoyote();

			if (_Actions.Action02 != null)
			{
				_Actions.Action02.HomingAvailable = true;
			}

			if (_Actions.Action06.BounceCount > 0)
			{
				_Actions.Action06.BounceCount = 0;
			}

		}
		else if (!_isInCoyote)
		{
			StartCoroutine(CoyoteTime());
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	private void HandleAnimator() {
		//Set Animator Parameters
		if (_PlayerPhys._isGrounded) { _CharacterAnimator.SetInteger("Action", 0); }
		_CharacterAnimator.SetFloat("YSpeed", _PlayerPhys._RB.velocity.y);
		_CharacterAnimator.SetFloat("GroundSpeed", _PlayerPhys._RB.velocity.magnitude);
		_CharacterAnimator.SetFloat("HorizontalInput", _Input.moveX * _PlayerPhys._RB.velocity.magnitude);
		_CharacterAnimator.SetBool("Grounded", _PlayerPhys._isGrounded);
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	public void SetSkinRotation() {
		//Set Skin Rotation
		if (_PlayerPhys._isGrounded)
		{
			Vector3 releVec = _PlayerPhys.GetRelevantVec(_PlayerPhys._RB.velocity);
			Vector3 newForward = _PlayerPhys._RB.velocity - transform.up * Vector3.Dot(_PlayerPhys._RB.velocity, transform.up);


			if (newForward.magnitude < 0.1f)
			{
				newForward = _CharacterAnimator.transform.forward;
			}

			CharRot = Quaternion.LookRotation(newForward, transform.up);
			_CharacterAnimator.transform.rotation = Quaternion.Lerp(_CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * _skinRotationSpeed);
		}
		else
		{
			Vector3 releVec = _PlayerPhys.GetRelevantVec(_PlayerPhys._RB.velocity);
			Vector3 VelocityMod = new Vector3(releVec.x, 0, releVec.z);

			Vector3 newForward = _PlayerPhys._RB.velocity - transform.up * Vector3.Dot(_PlayerPhys._RB.velocity, transform.up);

			if (VelocityMod != Vector3.zero)
			{
				Quaternion CharRot = Quaternion.LookRotation(newForward, transform.up);
				_CharacterAnimator.transform.rotation = Quaternion.Lerp(_CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * _skinRotationSpeed);
			}

		}
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	#endregion



	public void readyCoyote () {
		_isInCoyote = false;
		coyoteInEffect = true;
		if (_PlayerPhys._isGrounded)
			coyoteRememberDir = _PlayerPhys._groundNormal;
		else
			coyoteRememberDir = transform.up;
		coyoteRememberSpeed = _PlayerPhys._RB.velocity.y;
	}


	public void Curl () {
		if (!_isRolling)
		{
			if (_Actions.eventMan != null) _Actions.eventMan.RollsPerformed += 1;
			_Sounds.SpinningSound();
			_RollingCapsule.SetActive(true);
			_CharacterCapsule.SetActive(false);
			//rollCounter = 0f;
		}
		_PlayerPhys._isRolling = true;
		_isRolling = true;
	}

	void unCurl () {
		CapsuleCollider col = _RollingCapsule.GetComponent<CapsuleCollider>();


		if (!Physics.Raycast(col.transform.position + col.center, _CharacterAnimator.transform.up, 4))
		{
			_CharacterCapsule.SetActive(true);
			_RollingCapsule.SetActive(false);
			rollCounter = 0f;
			_isRolling = false;
			_PlayerPhys._isRolling = false;

		}

	}

	void handleInputs () {

		//Jump
		if (_Input.JumpPressed && (_PlayerPhys._isGrounded || coyoteInEffect))
		{

			if (_PlayerPhys._isGrounded)
				_JumpAction.InitialEvents(_PlayerPhys._groundNormal, true, _PlayerPhys._RB.velocity.y);
			else
				_JumpAction.InitialEvents(coyoteRememberDir, true, coyoteRememberSpeed);

			_Actions.ChangeAction(S_Enums.PlayerStates.Jump);
		}

		//Set Camera to back
		if (_Input.CamResetPressed)
		{
			if (_Input.moveVec == Vector2.zero && _PlayerPhys._horizontalSpeedMagnitude < 5f)
				_CamHandler._HedgeCam.GoBehindCharacter(6, 20f, false);
		}



		//Do Spindash
		if (_Input.spinChargePressed && _PlayerPhys._isGrounded && _PlayerPhys._groundNormal.y > _MaximumSlopeForSpinDash_ && _PlayerPhys._horizontalSpeedMagnitude < _MaximumSpeedForSpinDash_)
		{
			_Actions.ChangeAction(S_Enums.PlayerStates.SpinCharge);
			_Actions.Action03.InitialEvents();
		}

		//Check if rolling
		if (_PlayerPhys._isGrounded && _PlayerPhys._isRolling)
		{
			_CharacterAnimator.SetInteger("Action", 1);
		}
		_CharacterAnimator.SetBool("isRolling", _PlayerPhys._isRolling);

		//Change to rolling state
		if (_Input.RollPressed && _PlayerPhys._isGrounded)
		{
			Curl();
		}

		//Exit rolling state
		if ((!_Input.RollPressed && rollCounter > minRollTime) | !_PlayerPhys._isGrounded)
		{
			unCurl();
		}

		if (_isRolling)
			rollCounter += Time.deltaTime;

		_QuickstepManager.ReadyAction();

		//The actions the player can take while the air		
		if (!_PlayerPhys._isGrounded && !coyoteInEffect)
		{
			//Do a homing attack
			if (_Actions.Action02Control._HasTarget && _Input.HomingPressed && _Actions.Action02.HomingAvailable)
			{

				//Do a homing attack
				if (_Actions.Action02 != null && _PlayerPhys._homingDelay_ <= 0)
				{
					if (_Actions.Action02Control._isHomingAvailable)
					{
						_Sounds.HomingAttackSound();
						_Actions.ChangeAction(S_Enums.PlayerStates.Homing);
						_Actions.Action02.InitialEvents();
					}
				}
			}
			//Do an air dash;
			else if (_Actions.Action02.HomingAvailable && _Input.SpecialPressed)
			{
				if (!_Actions.Action02Control._HasTarget && _CanDashDuringFall_)
				{
					_Sounds.AirDashSound();
					_Actions.ChangeAction(S_Enums.PlayerStates.JumpDash);
					_Actions.Action11.InitialEvents();
				}
			}

			//Do a Double Jump
			else if (_Input.JumpPressed && _Actions.Action01._canDoubleJump_)
			{

				_Actions.Action01.jumpCount = 0;
				_Actions.Action01.InitialEvents(Vector3.up);
				_Actions.ChangeAction(S_Enums.PlayerStates.Jump);
			}


			//Do a Bounce Attack
			if (_Input.BouncePressed && _PlayerPhys._RB.velocity.y < 35f)
			{
				_Actions.Action06.InitialEvents();
				_Actions.ChangeAction(S_Enums.PlayerStates.Bounce);
				//Actions.Action06.ShouldStomp = false;

			}

			//Do a DropDash Attack
			if (_Actions.Action08 != null)
			{

				if (!_PlayerPhys._isGrounded && _Input.RollPressed)
				{
					_Actions.Action08.TryDropCharge();
				}

				if (_PlayerPhys._isGrounded && _Actions.Action08.DropEffect.isPlaying)
				{
					_Actions.Action08.DropEffect.Stop();
				}
			}
		}
	}

	IEnumerator CoyoteTime () {
		_isInCoyote = true;
		coyoteInEffect = true;
		float waitFor = _CoyoteTimeBySpeed_.Evaluate(_PlayerPhys._horizontalSpeedMagnitude / 100);

		yield return new WaitForSeconds(waitFor);

		coyoteInEffect = false;
	}

	public void cancelCoyote () {
		_isInCoyote = false;
		coyoteInEffect = false;
		StopCoroutine(CoyoteTime());
	}

	private void AssignStats () {
		_CoyoteTimeBySpeed_ = _Tools.Stats.JumpStats.CoyoteTimeBySpeed;
		_MaximumSlopeForSpinDash_ = _Tools.Stats.SpinChargeStats.maximumSlopePerformedAt;
		_MaximumSpeedForSpinDash_ = _Tools.Stats.SpinChargeStats.maximumSpeedPerformedAt;
		_CanDashDuringFall_ = _Tools.Stats.HomingStats.canDashWhenFalling;
	}

	private void AssignTools () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Input = GetComponent<S_PlayerInput>();
		_Actions = GetComponent<S_ActionManager>();
		_JumpAction = GetComponent<S_Action01_Jump>();
		_CamHandler = GetComponent<S_Handler_Camera>();
		_QuickstepManager = GetComponent<S_Handler_quickstep>();
		_Sounds = _Tools.SoundControl;
		_QuickstepManager.enabled = false;
		_CharacterCapsule = _Tools.characterCapsule;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_RollingCapsule = _Tools.crouchCapsule;
	}

}
