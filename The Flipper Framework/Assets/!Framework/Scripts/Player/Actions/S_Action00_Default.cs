using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using UnityEditor;
using System.Collections.Generic;

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

	private Animator              _CurrentAnimator;
	private Animator              _CharacterAnimator;
	private Animator              _BallAnimator;
	private Transform             _MainSkin;
	private S_CharacterTools      _Tools;
	private S_PlayerPhysics       _PlayerPhys;
	private S_PlayerInput         _Input;
	private S_ActionManager       _Actions;
	private S_Handler_Camera      _CamHandler;

	private SkinnedMeshRenderer[]           _PlayerSkin;
	private SkinnedMeshRenderer             _SpinDashBall;
	private List<SkinnedMeshRenderer>       _CurrentSkins = new List<SkinnedMeshRenderer>();


	#endregion

	//General
	#region General Properties

	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	public float                  _skinRotationSpeed = 2;
	[HideInInspector]
	public bool                   _canDashDuringFall_;
	private AnimationCurve        _coyoteTimeBySpeed_;

	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;        //In every action script, takes note of where in the Action Managers Main action list this script is. 

	//Coyote
	[HideInInspector]
	public bool         _isCoyoteInEffect = false;    //Enabled when ground is lost and tells if coyote time is happening.
	[HideInInspector]
	public Vector3      _coyoteRememberDirection;     //Tracks the up direction of the floor when ground was lost
	[HideInInspector]
	public float        _coyoteRememberSpeed;         //Tracks the world vertical speed when ground was lost.

	[HideInInspector]
	public int          _animationAction = 0;

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

		HandleAnimator(_animationAction);
		SetSkinRotationToVelocity(_skinRotationSpeed);
		HandleInputs();
	}

	private void FixedUpdate () {
	}

	//Called when the current action should be set to this.
	public void StartAction () {
		ReadyAction();

		//Set private
		_isCoyoteInEffect = _PlayerPhys._isGrounded;

		//Set Public
		StartCoroutine(ApplyWhenGrounded());
		_CharacterAnimator.SetInteger("Action", _animationAction);
		_CharacterAnimator.SetTrigger("ChangedState");
		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Default);
	}

	public bool AttemptAction () {
		return true;
	}
	public void StopAction () {
		if (enabled) enabled = false;
		else return;
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

		if(_CurrentAnimator == _CharacterAnimator)
		{
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
		else if (_CurrentAnimator == _BallAnimator)
		{
			_BallAnimator.SetInteger("Action", action);
		}
	}

	//Points the player visuals (model, effects, etc) in the direction of movement.
	public void SetSkinRotationToVelocity ( float rotateSpeed ) {

		//Gets a direction based on core velocity, and if there isn't one, don't chnage it.
		Vector3 newForward = _PlayerPhys._coreVelocity - transform.up * Vector3.Dot(_PlayerPhys._coreVelocity, transform.up);
		if (newForward.sqrMagnitude > 0.01f)
		{
			//Makes a rotation that only changes horizontally, never looking up or down.
			Quaternion charRot = Quaternion.LookRotation(newForward, transform.up);

			//Move towards it, slower if in the air.
			if (_PlayerPhys._isGrounded)
			{
				_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, charRot, Time.deltaTime * rotateSpeed);
			}
			else
			{
				_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, charRot, Time.deltaTime * rotateSpeed * 0.75f);
			}
		}
	}

	//Switches from one character model to another, typically used to switch between the spinball and the proper character.
	public void SwitchSkin(bool setMainSkin) {
		_CurrentSkins.Clear(); //Adds all of the enabled skins to a list so they can be handled later.

		//Handles the proper player skins, enabling/disabling them and adding them to the list if visible.
		for (int i = 0 ; i < _PlayerSkin.Length ; i++)
		{
			_PlayerSkin[i].enabled = setMainSkin;
			if (_PlayerSkin[i].enabled) { _CurrentSkins.Add(_PlayerSkin[i]); }
		}

		_SpinDashBall.enabled = !setMainSkin;
		//If ball enabled, disable the animator so its sounds don't overlap.
		if(_SpinDashBall.enabled) { 
			_CurrentAnimator = _BallAnimator;
			_CharacterAnimator.speed = 0;
			_CurrentSkins.Add(_SpinDashBall);
		}
		else { 
			_CurrentAnimator = _CharacterAnimator;
			_CharacterAnimator.speed = 1;
		}
	}

	public void HideCurrentSkins(bool hide) {
		foreach (SkinnedMeshRenderer sk in _CurrentSkins)
		{
			sk.enabled = hide;
		}
	}

	//Coyote time refers to the short time after losing ground where a player can still jump as if they were still on it.

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

	//This has to be set up in Editor. The invoker is in the PlayerPhysics script component, adding this event to it will mean this is called whenever the player lands.
	public void EventOnGrounded() {
		//May be in a ball even in this state (like after a homing attack), so change that on land
		if(_animationAction == 1)
		{
			_animationAction = 0;
			_CharacterAnimator.SetTrigger("ChangedState");
		}
		ReadyCoyote();
	}


	public void EventOnGroundLost () {
		if(enabled) { StartCoroutine(CoyoteTime()); }
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
		_BallAnimator = _Tools.BallAnimator;
		_CurrentAnimator = _CharacterAnimator;
		_MainSkin = _Tools.mainSkin;
		_PlayerSkin = _Tools.PlayerSkin;
		_SpinDashBall = _Tools.SpinDashBall.GetComponent<SkinnedMeshRenderer>();

		_CharacterAnimator.SetBool("isRolling", false);
	}
	#endregion
}
