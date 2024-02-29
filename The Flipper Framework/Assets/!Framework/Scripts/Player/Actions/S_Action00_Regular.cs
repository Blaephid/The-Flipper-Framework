using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using UnityEditor;

[RequireComponent(typeof(S_ActionManager))]
public class S_Action00_Regular : MonoBehaviour, IMainAction
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties

	private Animator		_CharacterAnimator;
	private Transform             _MainSkin;
	private S_CharacterTools	_Tools;
	private S_PlayerPhysics	_PlayerPhys;
	private S_PlayerInput	_Input;
	private S_ActionManager	_Actions;
	private S_Handler_Camera	_CamHandler;
	private S_SubAction_Quickstep	_QuickstepManager;
	private S_Control_PlayerSound _Sounds;
	private GameObject            _CharacterCapsule;
	private GameObject            _RollingCapsule;
	private S_Action01_Jump       _JumpAction;


	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	public float	_skinRotationSpeed = 2;
	[HideInInspector] 
	public bool	_CanDashDuringFall_;
	private AnimationCurve _CoyoteTimeBySpeed_;
	private float       _rollingStartSpeed_;

	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;

	private Quaternion	_CharRot;

	//Coyote
	[HideInInspector] 
	public bool	_coyoteInEffect = false;
	[HideInInspector] 
	public Vector3	_coyoteRememberDir;
	[HideInInspector]
	public float	_coyoteRememberSpeed;

	[HideInInspector] 
	public float	_rollCounter;
	private float	_minRollTime = 0.3f;
	#endregion
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
			for(int i = 0 ; i < _Actions._MainActions.Count ; i++)
			{
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Default)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {

		HandleAnimator(0);
		SetSkinRotation(_skinRotationSpeed);
		HandleInputs();
	}

	private void FixedUpdate () {

		//Skidding
		_Actions.skid.AttemptAction();

		//Coyote time refers to the short time after losing ground where a player can still jump as if they were still on it.
		if (_PlayerPhys._isGrounded)
		{
			ReadyCoyote();

		}
		else if (!_coyoteInEffect)
		{
			StartCoroutine(CoyoteTime());
		}
	}

	//Called when the current action should be set to this.
	public void StartAction() {

		StartCoroutine(ApplyWhenGrounded());
	}

	public bool AttemptAction() {
		return true;
	}
	public void StopAction() {

	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	public void HandleInputs () {
		if (!_Actions.isPaused)
		{
			//Grounded Actions
			if (_PlayerPhys._isGrounded)
			{
				//Enter Rolling state, must have been rolling for a long enough time first.
				if (_Input.RollPressed && _PlayerPhys._isGrounded && _PlayerPhys._horizontalSpeedMagnitude > _rollingStartSpeed_)
				{
					Curl();
				}
				//Exit rolling state
				if ((!_Input.RollPressed && _rollCounter > _minRollTime) || !_PlayerPhys._isGrounded)
				{
					UnCurl();
				}
				if (_PlayerPhys._isRolling)
					_rollCounter += Time.deltaTime;

				//Moving camera behind
				_CamHandler.AttemptCameraReset();
			}

			//Moving camera behind
			_CamHandler.AttemptCameraReset();

			//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
			_Actions.HandleInputs(_positionInActionList);
		}
	}

	//Called when the action begins, but wont apply the effects until grounded.
	IEnumerator ApplyWhenGrounded() {
		while (true)
		{
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();

			if(_PlayerPhys._isGrounded)
			{
				//Reset trackers for other actions.
				if (_Actions.Action02 != null)
				{
					_Actions.Action02._isHomingAvailable = true;
				}
				if (_Actions.Action06.BounceCount > 0)
				{
					_Actions.Action06.BounceCount = 0;
				}
				break;
			}
		}
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//Updates the animator with relevant data so it can perform the correct animations.
	public void HandleAnimator ( int action ) {

		//Action
		_CharacterAnimator.SetInteger("Action", action); 
		//Vertical speed
		_CharacterAnimator.SetFloat("YSpeed", _PlayerPhys._RB.velocity.y);
		//Horizontal speed
		_CharacterAnimator.SetFloat("GroundSpeed", _PlayerPhys._horizontalSpeedMagnitude);
		//Horizontal Input
		_CharacterAnimator.SetFloat("HorizontalInput", Mathf.Max(_Input.moveX, _Input.moveY));
		//Is grounded
		_CharacterAnimator.SetBool("Grounded", _PlayerPhys._isGrounded);
	}

	//Points the player visuals (model, effects, etc) in the direction of movement.
	public void SetSkinRotation(float rotateSpeed) {

		//Gets a direction based on core velocity, and if there isn't one, don't chnage it.
		Vector3 newForward = _PlayerPhys._coreVelocity - transform.up * Vector3.Dot(_PlayerPhys._coreVelocity, transform.up);
		if (newForward.sqrMagnitude > 0.01f)
		{
			//Makes a rotation that only changes horizontally, never looking up or down.
			_CharRot = Quaternion.LookRotation(newForward, transform.up);

			//Move towards it, slower if in the air.
			if (_PlayerPhys._isGrounded)
			{
				_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, _CharRot, Time.deltaTime * rotateSpeed);
			}
			else
			{
				_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, _CharRot, Time.deltaTime * rotateSpeed * 0.75f);
			}
		}
	}

	//When the player starts rolling on the ground, play the sound and set rolling stats.
	public void Curl () {
		if (!_PlayerPhys._isRolling)
		{
			if (_Actions.eventMan != null) _Actions.eventMan.RollsPerformed += 1;
			_Sounds.SpinningSound();
		}
		SetIsRolling(true);
	}

	//When the player wants to stop rolling while on the ground, check if there's enough room to stand up.
	public void UnCurl () {
		CapsuleCollider col = _CharacterCapsule.GetComponent<CapsuleCollider>();
		if (!Physics.BoxCast(col.transform.position, new Vector3 (col.radius, col.height / 2.1f, col.radius), Vector3.zero, transform.rotation, 0))
		{
			SetIsRolling(false);
		}
	}

	//Called whenever IsRolling is to be changed, since multiple things should change whenever this does.
	public void SetIsRolling(bool value) {
		if(value != _PlayerPhys._isRolling)
		{
			_PlayerPhys._isRolling = value;
			if(value)
			{
				_CharacterAnimator.SetBool("isRolling", true);
				_RollingCapsule.SetActive(true);
				_CharacterCapsule.SetActive(false);
			}
			else
			{
				_CharacterAnimator.SetBool("isRolling", false);
				_CharacterCapsule.SetActive(true);
				_RollingCapsule.SetActive(false);
				_rollCounter = 0f;
			}
		}
	}

	//When grounded, tracks the current floor so it can be used for coyote time if the ground is lost next frame.
	public void ReadyCoyote () {
		_coyoteInEffect = true;

		_coyoteRememberDir = _PlayerPhys._groundNormal;
		_coyoteRememberSpeed = _PlayerPhys._RB.velocity.y;
	}

	//Called when the ground is lost but before coyote is in effect, this confirms it's being tracked and end it after a while.
	IEnumerator CoyoteTime () {
		_coyoteInEffect = true;
		float waitFor = _CoyoteTimeBySpeed_.Evaluate(_PlayerPhys._horizontalSpeedMagnitude / 100);

		yield return new WaitForSeconds(waitFor);

		_coyoteInEffect = false;
	}

	//Called externally if the coyote time has to be ended prematurely.
	public void CancelCoyote () {
		_coyoteInEffect = false;
		StopCoroutine(CoyoteTime());
	}
	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	private void AssignStats () {
		_CoyoteTimeBySpeed_ = _Tools.Stats.JumpStats.CoyoteTimeBySpeed;
		_CanDashDuringFall_ = _Tools.Stats.HomingStats.canDashWhenFalling;
		_rollingStartSpeed_ = _Tools.Stats.RollingStats.rollingStartSpeed;
	}

	private void AssignTools () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Input = GetComponent<S_PlayerInput>();
		_Actions = GetComponent<S_ActionManager>();
		_JumpAction = GetComponent<S_Action01_Jump>();
		_CamHandler = GetComponent<S_Handler_Camera>();
		_QuickstepManager = GetComponent<S_SubAction_Quickstep>();
		_Sounds = _Tools.SoundControl;
		_CharacterCapsule = _Tools.characterCapsule;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.mainSkin;
		_RollingCapsule = _Tools.crouchCapsule;

		_CharacterAnimator.SetBool("isRolling", false);
	}
	#endregion
}
