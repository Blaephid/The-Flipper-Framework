using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using UnityEditor;

[RequireComponent(typeof(S_ActionManager))]
public class S_Action00_Default : MonoBehaviour, IMainAction
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties

	private Animator              _CharacterAnimator;
	private Transform             _MainSkin;
	private S_CharacterTools      _Tools;
	private S_PlayerPhysics       _PlayerPhys;
	private S_PlayerInput         _Input;
	private S_ActionManager       _Actions;
	private S_Handler_Camera      _CamHandler;


	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	public float                  _skinRotationSpeed = 2;
	[HideInInspector]
	public bool                   _canDashDuringFall_;
	private AnimationCurve        _coyoteTimeBySpeed_;

	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;

	private Quaternion  _charRot;

	//Coyote
	[HideInInspector]
	public bool         _isCoyoteInEffect = false;
	[HideInInspector]
	public Vector3      _coyoteRememberDirection;
	[HideInInspector]
	public float        _coyoteRememberSpeed;

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
		ReadyAction();
	}

	// Update is called once per frame
	void Update () {

		HandleAnimator(0);
		SetSkinRotationToVelocity(_skinRotationSpeed);
		HandleInputs();
	}

	private void FixedUpdate () {

		//Coyote time refers to the short time after losing ground where a player can still jump as if they were still on it.
		if (_PlayerPhys._isGrounded)
		{
			ReadyCoyote();

		}
		else if (!_isCoyoteInEffect)
		{
			StartCoroutine(CoyoteTime());
		}
	}

	//Called when the current action should be set to this.
	public void StartAction () {
		ReadyAction();

		StartCoroutine(ApplyWhenGrounded());
		_CharacterAnimator.SetInteger("Action", 0);
		_CharacterAnimator.SetTrigger("ChangedState");
		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Default);
	}

	public bool AttemptAction () {
		return true;
	}
	public void StopAction () {
		enabled = false;
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	//Responsible for taking in inputs the player performs to switch or activate other actions, or other effects.
	public void HandleInputs () {
		if (!_Actions.isPaused)
		{
			//Moving camera behind
			_CamHandler.AttemptCameraReset();


			//Moving camera behind
			_CamHandler.AttemptCameraReset();

			//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
			_Actions.HandleInputs(_positionInActionList);
		}
	}

	//Called when the action begins, but wont apply the effects until grounded.
	IEnumerator ApplyWhenGrounded () {
		while (true)
		{
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();

			if (_PlayerPhys._isGrounded)
			{
				//Reset trackers for other actions.
				if (_Actions.Action02 != null)
				{
					_Actions._isHomingAvailable = true;
				}
				if (_Actions._bounceCount > 0)
				{
					_Actions._bounceCount = 0;
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
	public void SetSkinRotationToVelocity ( float rotateSpeed ) {

		//Gets a direction based on core velocity, and if there isn't one, don't chnage it.
		Vector3 newForward = _PlayerPhys._coreVelocity - transform.up * Vector3.Dot(_PlayerPhys._coreVelocity, transform.up);
		if (newForward.sqrMagnitude > 0.01f)
		{
			//Makes a rotation that only changes horizontally, never looking up or down.
			_charRot = Quaternion.LookRotation(newForward, transform.up);

			//Move towards it, slower if in the air.
			if (_PlayerPhys._isGrounded)
			{
				_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, _charRot, Time.deltaTime * rotateSpeed);
			}
			else
			{
				_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, _charRot, Time.deltaTime * rotateSpeed * 0.75f);
			}
		}
	}

	//When grounded, tracks the current floor so it can be used for coyote time if the ground is lost next frame.
	public void ReadyCoyote () {
		_isCoyoteInEffect = true;

		_coyoteRememberDirection = _PlayerPhys._groundNormal;
		_coyoteRememberSpeed = _PlayerPhys._RB.velocity.y;
	}

	//Called when the ground is lost but before coyote is in effect, this confirms it's being tracked and end it after a while.
	IEnumerator CoyoteTime () {
		_isCoyoteInEffect = true;
		float waitFor = _coyoteTimeBySpeed_.Evaluate(_PlayerPhys._horizontalSpeedMagnitude / 100);

		yield return new WaitForSeconds(waitFor);

		_isCoyoteInEffect = false;
	}

	//Called externally if the coyote time has to be ended prematurely.
	public void CancelCoyote () {
		_isCoyoteInEffect = false;
		StopCoroutine(CoyoteTime());
	}
	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	//Assigns all external elements of the action.
	public void ReadyAction () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
			for (int i = 0 ; i < _Actions._MainActions.Count ; i++)
			{
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Default)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	private void AssignStats () {
		_coyoteTimeBySpeed_ = _Tools.Stats.JumpStats.CoyoteTimeBySpeed;
		_canDashDuringFall_ = _Tools.Stats.HomingStats.canDashWhenFalling;
	}

	private void AssignTools () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Input = GetComponent<S_PlayerInput>();
		_Actions = GetComponent<S_ActionManager>();
		_CamHandler = GetComponent<S_Handler_Camera>();
		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.mainSkin;

		_CharacterAnimator.SetBool("isRolling", false);
	}
	#endregion
}
