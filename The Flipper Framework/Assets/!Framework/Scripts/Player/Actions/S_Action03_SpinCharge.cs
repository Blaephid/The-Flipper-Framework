using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class S_Action03_SpinCharge : S_Action_Base, IMainAction
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_Control_EffectsPlayer         _Effects;

	private CapsuleCollider		_LowerCapsule;
	private CapsuleCollider		_StandingCapsule;

	private Transform			_PlayerSkinTransform;
	private Transform                       _MainCamera;
	#endregion

	//General
	#region General Properties

	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	[HideInInspector] 
	public float		_spinDashChargingSpeed_ = 0.3f;
	[HideInInspector] 
	public float		_minimunCharge_ = 10;
	[HideInInspector] 
	public float		_maximunCharge_ = 100;
	[HideInInspector] 
	public float		_spinDashStillForce_ = 20f;
	private float		_MaximumSpeedForSpinDash_;
	private float		_MaximumSlopeForSpinDash_;
	private AnimationCurve	_speedLossByTime_;
	private AnimationCurve	_forceGainByAngle_;
	private AnimationCurve        _turnAmountByAngle_;
	private AnimationCurve	_gainBySpeed_;
	private Vector4                 _releaseShakeAmmount_;
	private Vector2               _cameraPauseEffect_ = new Vector2(3, 40);
	private S_GeneralEnums.SpinChargeAimingTypes _whatControl_;
	private float                 _tappingBonus_;
	private int                   _delayBeforeLaunch_;
	private bool                  _shouldSetRolling_;
	#endregion

	// Trackers
	#region trackers
	

	private bool                  _isPressedCurrently = true;		//Involed in mashing. Reflects whether the button is pressed, if false, start exiting, if false when button is true, reset exiting.	

	private float		_spinDashChargedEffectAmm = 1;		//How active the spin dash particle effect should be
	private float		_ballAnimationSpeedMultiplier = 1;

	private float		_counter = 0;	//Tracks how long in this state for

	private Quaternion		_characterRotation;		//This has unique rotation properties different to most actions, so this tracks what rotation the character should have

	#endregion
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Update is called once per frame
	void Update () {
		SetAnimatorAndRotation();
	}

	private void FixedUpdate () {
		ChargeSpin();
		AffectMovement();
		HandleInputs();
	}

	//Checks if this action can currently be performed, based on the input and environmental factors.
	new public bool AttemptAction () {
		if (!base.AttemptAction()) return false;
		//Pressed on the ground
		if (_Input._SpinChargePressed && _PlayerPhys._isGrounded)
		{
			//At a slow enough speed and not on too sharp of a slope.
			if (_PlayerPhys._groundNormal.y > _MaximumSlopeForSpinDash_ && _PlayerVel._horizontalSpeedMagnitude < _MaximumSpeedForSpinDash_)
			{
				StartAction();
				return true;
			}
		}
		return false;
	}

	//Called when the action should be enabled.
	new public void StartAction ( bool overwrite = false ) {
		if (enabled || (!_Actions._canChangeActions && !overwrite)) { return; }

		//Setting private
		_Actions._charge = 20;
		_counter = 0;
		_isPressedCurrently = true;

		//Change collider to be smaller
		_Actions._ActionDefault.OverWriteCollider(_LowerCapsule);

		_PlayerPhys._canStickToGround = true; //Allows following the ground when in a normal grounded state.

		//Visuals & Effects
		_Actions._ActionDefault.SwitchSkin(false);
		_Sounds.SpinDashSound();

		_Actions.ChangeAction(S_S_ActionHandling.PrimaryPlayerStates.SpinCharge);
		enabled = true;
	}

	//Called by the action manager whenever action is changing. Will only perform if enabled right now. Similar to OnDisable.
	public void StopAction(bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { ReadyAction(); return; } //First time is called on ActionManager Awake() to ensure this starts disabled and has a single opportunity to assign tools and stats.

		//Return to normal skin and collider size
		_Actions._ActionDefault.OverWriteCollider(_StandingCapsule);
		_Actions._ActionDefault.SwitchSkin(true);

		_PlayerPhys._isRolling = false;
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	public void HandleInputs () {
			//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
			_Actions.HandleInputs(_positionInActionList);
	}

	//Increases power in the spin for release
	private void ChargeSpin () {
		_Actions._charge += _spinDashChargingSpeed_;
		_counter += Time.deltaTime;

		//Effects
		_Effects.DoSpindash(1, _spinDashChargedEffectAmm * _Actions._charge, _Actions._charge,
		_Effects.GetSpinDashDust(), _maximunCharge_);

		//If not pressed, sets the player as exiting
		if (!_Input._SpinChargePressed)
		{
			if (_isPressedCurrently)
				StartCoroutine(DelayRelease());
			_isPressedCurrently = false;
		}

		//If the button is pressed while exiting, charge more, means mashing the button is more effective.
		else
		{
			if (!_isPressedCurrently)
			{
				_Actions._charge += (_spinDashChargingSpeed_ * _tappingBonus_);
			}

			_isPressedCurrently = true;
		}

		//Prevents going over the maximum
		_Actions._charge = Mathf.Min(_Actions._charge, _maximunCharge_);
	}

	//Changes how the player moves when in this state.
	private void AffectMovement () {

		_PlayerMovement._moveInput *= 0.65f; //Limits input, lessening turning and deceleration
		
		if(_shouldSetRolling_) _PlayerPhys._isRolling = true; // set every frame to counterballanced the rolling subaction

		//Apply a force against the player movement to decrease speed.
		float stillForce = _spinDashStillForce_ * _speedLossByTime_.Evaluate(_counter);
		if (_PlayerVel._horizontalSpeedMagnitude > 20)
		{
			_PlayerVel.AddCoreVelocity(- _PlayerVel._coreVelocity.normalized * Mathf.Min(stillForce, _PlayerVel._horizontalSpeedMagnitude));
		}
	}

	//Once button is release, wait for a bit before launching, checking each frame for if the button is pressed again.
	private IEnumerator DelayRelease () {
		//Get time to delay, if only just started the action then delay will be shorter
		int waitFor = _delayBeforeLaunch_;
		if (_counter < 0.1f) waitFor /= 2;

		//Checks every frame for if the button is pressed
		for (int s = 0 ; s < waitFor ; s++)
		{
			yield return new WaitForFixedUpdate();
			if (_isPressedCurrently)
			{
				yield break;
			}
		}
		Release();
	}

	//Launches player forwards at speed.
	private void Release () {

		float newSpeed = 0;

		//Only launches forwards if charged long enough.
		if (_Actions._charge < _minimunCharge_)
		{
			_Sounds.GeneralSource.Stop();
			_Actions._ActionDefault.StartAction();
		}
		else
		{
			//Effects
			_Sounds.SpinDashReleaseSound();

			//New speed to gain is determined by charge but affected by -
			Vector3 addForce = _PlayerSkinTransform.forward;
			newSpeed = _Actions._charge;

			//The angle between movement direction and this new force (typically higher with bigger angles)
			float dif = Vector3.Dot(addForce.normalized, _PlayerPhys._RB.velocity.normalized);
			if (_PlayerVel._currentRunningSpeed > 20)
				newSpeed *= _forceGainByAngle_.Evaluate(dif);

			//And the current speed (typically lower when at higher speed)
			newSpeed *= _gainBySpeed_.Evaluate(_PlayerVel._currentRunningSpeed / _PlayerMovement._currentMaxSpeed);
			addForce *= newSpeed; //Adds speed to direction to get the force

			_PlayerVel.AddCoreVelocity(addForce);

			//Adding velocity is more natural/realistic, but for accuracy in aiming, there is also a rotation towards the new direction.
			Vector3 newDir = _PlayerPhys._RB.velocity;
			newDir.Normalize();
			dif = Vector3.Angle(_MainSkin.forward, newDir);
			dif *= _turnAmountByAngle_.Evaluate(dif);

			newDir = Vector3.RotateTowards(newDir, _MainSkin.forward, Mathf.Deg2Rad * dif, 0);
			_PlayerVel.SetCoreVelocity(newDir * _PlayerVel._currentRunningSpeed);

			_CharacterAnimator.SetFloat("GroundSpeed", newSpeed);

			_Actions._ActionDefault.StartAction();
		}

		//Effects
		_Effects.EndSpinDash();

		float shake = Mathf.Clamp(_releaseShakeAmmount_.x * _Actions._charge, _releaseShakeAmmount_.y, _releaseShakeAmmount_.z);
		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraShake(shake, (int)_releaseShakeAmmount_.w));

		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraPause(_cameraPauseEffect_, new Vector2(_PlayerVel._horizontalSpeedMagnitude, newSpeed), 0.25f)); //The camera will fall back before catching up.

	}

	//Handles the ball animations and set the direction the character faces, by extent aims the direction of the dash on release.
	private void SetAnimatorAndRotation () {

		//Handle animator, both regular and ball ones.
		_Actions._ActionDefault.HandleAnimator(3);
		_BallAnimator.SetInteger("Action", 3);
		_BallAnimator.SetFloat("SpinCharge", _Actions._charge * _ballAnimationSpeedMultiplier);

		//Configured to either rotate towards where the camera is facing, or to rotate to where the player is moving.
		switch (_whatControl_)
		{
			case S_GeneralEnums.SpinChargeAimingTypes.Camera:
				//Since it requires camera movement, if the camera can't be moved, instead aims by input.
				if (_CamHandler._HedgeCam._isLocked || _CamHandler._HedgeCam._isXLocked)
					FaceByInput();
				else
					FaceByCamera();
				break;

			case S_GeneralEnums.SpinChargeAimingTypes.Input:
				FaceByInput();		
				break;
		}

		//Moves independant of movement and always rotates to have back to the camera.
		void FaceByCamera () {
			if (_Input._move.sqrMagnitude > 0.1f)
			{
				_characterRotation = Quaternion.LookRotation(_MainCamera.forward - _PlayerPhys._groundNormal * Vector3.Dot(_MainCamera.forward, _PlayerPhys._groundNormal), transform.up);
			}
		}

		//Follows movement with a slight lerp towards input (since turning is not instant)
		void FaceByInput () {
			Vector3 faceDirection = _PlayerPhys._RB.velocity.sqrMagnitude > 1 ? _PlayerVel._coreVelocity.normalized : _MainSkin.forward; //If not moving, the direction is based on character

			//Rotate slightly to player input
			if (_PlayerMovement._moveInput.sqrMagnitude > 0.2)
			{
				Vector3 inputDirection = transform.TransformDirection(_PlayerMovement._moveInput.normalized);
				faceDirection = Vector3.RotateTowards(faceDirection, inputDirection, Mathf.Deg2Rad * 100, 0);
			}

			if(faceDirection != Vector3.zero)
				_characterRotation = Quaternion.LookRotation(faceDirection, transform.up);
		}

		//Rotate towards this new direction.
		_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, _characterRotation, Time.deltaTime * _Actions._ActionDefault._skinRotationSpeed);
	}


	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//This has to be set up in Editor. The invoker is in the PlayerPhysics script component, adding this event to it will mean this is called whenever the player leaves or loses the ground
	public void EventOnGroundLost () {
		_Input._SpecialPressed = false; // Ensures an action like a jump dash won't be performed immediately.
		_Effects.EndSpinDash();

		StartCoroutine(DelayOnFall());
	}

	//Allows time for certain aerial actions to be performed when falling off the ground
	private IEnumerator DelayOnFall() {
		yield return new WaitForSeconds(0.2f);
		if(enabled)
		{
			_Actions._ActionDefault.StartAction();
		}
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	public override void AssignStats () {
		_spinDashChargingSpeed_ =	_Tools.Stats.SpinChargeStats.chargingSpeed;
		_minimunCharge_ =		_Tools.Stats.SpinChargeStats.minimunCharge;
		_maximunCharge_ =		_Tools.Stats.SpinChargeStats.maximunCharge;
		_spinDashStillForce_ =	_Tools.Stats.SpinChargeStats.forceAgainstMovement;
		_speedLossByTime_ =		_Tools.Stats.SpinChargeStats.SpeedLossByTime;
		_forceGainByAngle_ =	_Tools.Stats.SpinChargeStats.ForceGainByAngle;
		_turnAmountByAngle_ =	_Tools.Stats.SpinChargeStats.LerpRotationByAngle;
		_gainBySpeed_ =		_Tools.Stats.SpinChargeStats.ForceGainByCurrentSpeed;
		_releaseShakeAmmount_ =	_Tools.Stats.SpinChargeStats.releaseShakeAmmount;
		_MaximumSlopeForSpinDash_ =	_Tools.Stats.SpinChargeStats.maximumSlopePerformedAt;
		_MaximumSpeedForSpinDash_ =	_Tools.Stats.SpinChargeStats.maximumSpeedPerformedAt;
		_whatControl_ =		_Tools.Stats.SpinChargeStats.whatAimMethod;
		_tappingBonus_ =		_Tools.Stats.SpinChargeStats.tappingBonus;
		_delayBeforeLaunch_ =	_Tools.Stats.SpinChargeStats.delayBeforeLaunch;
		_shouldSetRolling_ =	_Tools.Stats.SpinChargeStats.shouldSetRolling;

		_cameraPauseEffect_ =	_Tools.Stats.SpinChargeStats.cameraPauseEffect;
	}
	public override void AssignTools () {
		base.AssignTools();
		_Effects = _Tools.EffectsControl;
		_MainCamera = Camera.main.transform;

		_PlayerSkinTransform = _Tools.CharacterModelOffset;
		_LowerCapsule = _Tools.CrouchCapsule.GetComponent<CapsuleCollider>();
		_StandingCapsule = _Tools.StandingCapsule.GetComponent<CapsuleCollider>();	
	}
	#endregion

}