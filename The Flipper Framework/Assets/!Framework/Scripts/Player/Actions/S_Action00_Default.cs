using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using UnityEditor;
using System.Collections.Generic;
using Unity.VisualScripting;

public class S_Action00_Default :  S_Action_Base, IMainAction
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties

	private Animator              _CurrentAnimator;
	private Transform             _SkinOffset;

	private CapsuleCollider                 _StandingCapsule;
	private List<SkinnedMeshRenderer>       _PlayerSkin = new List<SkinnedMeshRenderer>();
	private SkinnedMeshRenderer             _SpinDashBall;
	private List<SkinnedMeshRenderer>       _CurrentSkins = new List<SkinnedMeshRenderer>();
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	public float                  _skinRotationSpeed = 13;
	[HideInInspector]
	public bool                   _canDashDuringFall_;
	private AnimationCurve        _coyoteTimeBySpeed_;

	#endregion

	// Trackers
	#region trackers

	//Coyote
	[HideInInspector]
	public bool         _isCoyoteInEffect = false;    //Enabled when ground is lost and tells if coyote time is happening.
	[HideInInspector]
	public Vector3      _coyoteRememberDirection;     //Tracks the up direction of the floor when ground was lost
	[HideInInspector]
	public float        _coyoteRememberSpeed;         //Tracks the world vertical speed when ground was lost.

	[HideInInspector]
	public int          _animationAction = 0;

	[HideInInspector]
	public bool	_isAnimatorControlledExternally = false;

	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited
	private void Start () {
		SwitchSkin(true);
		OverWriteCollider(_StandingCapsule);
	}

	// Update is called once per frame
	void Update () {
		if (!_isAnimatorControlledExternally)
		{
			HandleAnimator(_animationAction);
			SetSkinRotationToVelocity(_skinRotationSpeed);
		}
	}

	new private void FixedUpdate () {
		base.FixedUpdate();
		HandleInputs();
	}

	//Called when the current action should be set to this.
	new public void StartAction (bool overwrite = false ) {
		if(enabled || (!_Actions._canChangeActions && !overwrite)) { return; } //Because this method can be called when this state is already active (object interactions), end early if so.

		//Set private
		_isCoyoteInEffect = _PlayerPhys._isGrounded;

		_PlayerPhys._canStickToGround = true; //Allows following the ground when in a normal grounded state.

		//Set Effects
		if(_CharacterAnimator.GetInteger("Action") != 0)
		{
			_CharacterAnimator.SetTrigger("ChangedState"); //This is the only animation change because if set to this in the air, should keep the apperance from other actions. The animator will only change when action is changed.
			if (!_isAnimatorControlledExternally) _CharacterAnimator.SetInteger("Action", _animationAction);
		}

		_Actions.ChangeAction(S_S_ActionHandling.PrimaryPlayerStates.Default);
		enabled = true;
	}

	new public bool AttemptAction () {
		 if (_isActionCurrentlyValid) { return false; }
		return true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { ReadyAction(); return; } //First time is called on ActionManager Awake() to ensure this starts disabled and has a single opportunity to assign tools and stats.
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	
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
			_CharacterAnimator.SetFloat("GroundSpeed", _PlayerVel._currentRunningSpeed);
			//How much greater running speed is then air speed. This affects what animation to perform in the air. Verti velocity is to the power of x, to make it less exponentially take more priority.
			_CharacterAnimator.SetFloat("HorizSpeedOverVertiSpeed", _PlayerVel._currentRunningSpeed - Mathf.Pow(Mathf.Abs(_PlayerVel._totalVelocityLocal.y), 1.1f));
			//Horizontal Input
			_CharacterAnimator.SetFloat("HorizontalInput", Mathf.Max(_Input.moveX, _Input.moveY));
			//Is grounded
			_CharacterAnimator.SetBool("Grounded", _PlayerPhys._isGrounded);
			//Is rolling
			_CharacterAnimator.SetBool("isRolling", _PlayerPhys._isRolling);
			if (_PlayerPhys._isRolling)
			{
				if(_PlayerVel._currentRunningSpeed < 1) { _CharacterAnimator.speed = 0; }
				else { _CharacterAnimator.speed = 1; }
			}
		}
		else if (_CurrentAnimator == _BallAnimator)
		{
			_BallAnimator.SetInteger("Action", action);
		}
	}

	//Points the player visuals (model, effects, etc) in the direction of movement or a custom direction. Can also apply an offset which will only rotate character models, leaving effects and other children where they are.
	public void SetSkinRotationToVelocity ( float rotateSpeed, Vector3 direction = default(Vector3), Vector2 offset = default(Vector2), Vector3 upDirection = default(Vector3) ) {

		//If no direction was passed, use moving direction.
		if (direction == default(Vector3))
		{
			direction = _PlayerVel._coreVelocity;
		}

		if(upDirection == default(Vector3))
		{
			upDirection = transform.up;
		}

		//Gets a direction to rotate towards based on input direction, and if there isn't one, don't change it.
		Vector3 newForward = direction - upDirection * Vector3.Dot(direction, upDirection);
		if (newForward.sqrMagnitude > 0.01f)
		{
			//Makes a rotation that only changes horizontally, never looking up or down.
			Quaternion targetRotation = Quaternion.LookRotation(newForward, upDirection);

			//Rotate towards it, slower if in the air. If rotateSpeed input was 0, then turn becomes instant.
			if (_PlayerPhys._isGrounded)
			{
				rotateSpeed = rotateSpeed != 0 ? rotateSpeed * Time.deltaTime : 1;
			}
			else
			{
				rotateSpeed = rotateSpeed != 0 ? rotateSpeed * Time.deltaTime * 0.75f : 1;
			}
			_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, targetRotation, rotateSpeed);


			if (offset == default(Vector2)) { offset = Vector2.zero; } //If no offset is input, should lerp to remove offset.

			//Apply a local rotation to the offset object, based on input offset.
			float xEuler = Mathf.Lerp(_SkinOffset.localEulerAngles.x + 360, offset.y + 360,  rotateSpeed * 0.5f) - 360;
			float yEuler = _SkinOffset.localEulerAngles.y > 180 ? _SkinOffset.localEulerAngles.y - 360 : _SkinOffset.localEulerAngles.y; //Because euler angles update automaitcally to different numbers, ensure is within the range of -180 -> 180.
			yEuler = Mathf.Lerp(yEuler, offset.x , rotateSpeed * 0.75f);

			_SkinOffset.localEulerAngles = new Vector3(xEuler, yEuler, 0); //Lerp angles seperately, then apply, this ensures it will only change on these angles, not rotate through z.
		}
	}

	//Switches from one character model to another, typically used to switch between the spinball and the proper character. Every action should call this when started, and not when stopped.
	public void SwitchSkin(bool setMainSkin) {
		if(setMainSkin && !_SpinDashBall.enabled) { return; }

		_CurrentSkins.Clear(); //Adds all of the enabled skins to a list so they can be handled later.

		//Handles the proper player skins, enabling/disabling them and adding them to the list if visible.
		for (int i = 0 ; i < _PlayerSkin.Count ; i++)
		{
			SkinnedMeshRenderer Skin = _PlayerSkin[i];
			Skin.enabled = setMainSkin;
			if (Skin.enabled) { _CurrentSkins.Add(Skin); }
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
		for (int i = 0 ; i < _CurrentSkins.Count ; i++)
		{
			_CurrentSkins[i].enabled = hide;
		}
	}

	public void OverWriteCollider ( CapsuleCollider newCollider ) {
		_CharacterCapsule.radius = newCollider.radius;
		_CharacterCapsule.center = newCollider.center;
		//_CharacterCapsule.transform.localPosition = newCollider.transform.position - _CharacterCapsule.transform.parent.position;
		_CharacterCapsule.material = newCollider.material;
		_CharacterCapsule.height = newCollider.height;
	}

	//Called when the ground is lost but before coyote is in effect, this confirms it's being tracked and end it after a while.
	public IEnumerator CoyoteTime () {

		_isCoyoteInEffect = true; //Checked externally, mainly in the jump action.

		//Remember upwards direction and angle downwards of floor before it was lost;
		_coyoteRememberDirection = transform.up;
		_coyoteRememberSpeed = _PlayerPhys._RB.velocity.y;

		//Length of coyote time dependant on speed.
		float waitFor = _coyoteTimeBySpeed_.Evaluate(_PlayerVel._currentRunningSpeed / 100);
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
		if (enabled)
		{
			//May be in a ball even in this state (like after a homing attack), so change that on land
			if (_animationAction != 0)
			{
				_animationAction = 0;
				_CharacterAnimator.SetTrigger("ChangedState");
			}
			CancelCoyote();
			SwitchSkin(true);
		}
		else
		{
			//May be in a ball even in this state (like after a homing attack), so change that on land
			if (_animationAction == 1)
			{
				_animationAction = 0;
				_CharacterAnimator.SetTrigger("ChangedState");
			}
		}
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
	
	public override void AssignStats () {
		_coyoteTimeBySpeed_ = _Tools.Stats.JumpStats.CoyoteTimeBySpeed;
		_canDashDuringFall_ = _Tools.Stats.HomingStats.canDashWhenFalling;
	}

	public override void AssignTools () {
		base.AssignTools();

		_CurrentAnimator =	_CharacterAnimator;
		_PlayerSkin.Add(_Tools.SkinRenderer);
		_SkinOffset =	_Tools.CharacterModelOffset;
		_SpinDashBall =	_Tools.SpinDashBall.GetComponent<SkinnedMeshRenderer>();
		_StandingCapsule = _Tools.StandingCapsule.GetComponent<CapsuleCollider>();
	}
	#endregion
}
