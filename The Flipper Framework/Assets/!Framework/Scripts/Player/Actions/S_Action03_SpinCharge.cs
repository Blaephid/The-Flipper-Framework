using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class S_Action03_SpinCharge : MonoBehaviour, IMainAction
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_CharacterTools		_Tools;
	private S_PlayerInput		_Input;
	private S_ActionManager                 _Actions;
	private S_PlayerPhysics                 _PlayerPhys;
	private S_Control_SoundsPlayer           _Sounds;
	private S_Control_EffectsPlayer         _Effects;

	private Animator			_CharacterAnimator;
	private Animator			_BallAnimator;
	private S_Handler_Camera		_CamHandler;
	private Transform                       _MainSkin;

	private GameObject			_LowerCapsule;
	private GameObject			_CharacterCapsule;

	private Transform			_PlayerSkinTransform;
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
	private float                 _releaseShakeAmmount_;
	private S_Enums.SpinChargeAiming _whatControl_;
	private float                 _tappingBonus_;
	private int                   _delayBeforeLaunch_;
	private bool                  _shouldSetRolling_;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;

	private bool                  _isPressedCurrently = true;		//Involed in mashing. Reflects whether the button is pressed, if false, start exiting, if false when button is true, reset exiting.	

	private float		_currentCharge;			//Tracks how much power gained this use of the action,  starting from minimum.
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

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyAction();
	}

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
	public bool AttemptAction () {
		//Pressed on the ground
		if (_Input.spinChargePressed && _PlayerPhys._isGrounded)
		{
			//At a slow enough speed and not on too sharp of a slope.
			if (_PlayerPhys._groundNormal.y > _MaximumSlopeForSpinDash_ && _PlayerPhys._horizontalSpeedMagnitude < _MaximumSpeedForSpinDash_)
			{
				StartAction();
				return true;
			}
		}
		return false;
	}

	//Called when the action should be enabled.
	public void StartAction () {
		//Setting private
		_currentCharge = 20;
		_counter = 0;
		_isPressedCurrently = true;

		//Setting public
		_LowerCapsule.SetActive(true);
		_CharacterCapsule.SetActive(false);

		//Visuals & Effects
		_Actions._ActionDefault.SwitchSkin(false);
		_Sounds.SpinDashSound();

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.SpinCharge);
		this.enabled = true;
	}

	//Called by the action manager whenever action is changing. Will only perform if enabled right now. Similar to OnDisable.
	public void StopAction(bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		//Setting public
		_LowerCapsule.SetActive(false);
		_CharacterCapsule.SetActive(true);
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
		_currentCharge += _spinDashChargingSpeed_;
		_counter += Time.deltaTime;

		//Effects
		_Effects.DoSpindash(1, _spinDashChargedEffectAmm * _currentCharge, _currentCharge,
		_Effects.GetSpinDashDust(), _maximunCharge_);

		//If not pressed, sets the player as exiting
		if (!_Input.spinChargePressed)
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
				_currentCharge += (_spinDashChargingSpeed_ * _tappingBonus_);
			}

			_isPressedCurrently = true;
		}

		//Prevents going over the maximum
		if (_currentCharge > _maximunCharge_)
		{
			_currentCharge = _maximunCharge_;
		}
	}

	//Changes how the player moves when in this state.
	private void AffectMovement () {

		_PlayerPhys._moveInput *= 0.75f; //Limits input, lessening turning and deceleration
		
		if(_shouldSetRolling_) _PlayerPhys._isRolling = true; // set every frame to counterballanced the rolling subaction

		//Apply a force against the player movement to decrease speed.
		float stillForce = _spinDashStillForce_ * _speedLossByTime_.Evaluate(_counter);
		if (stillForce * 4 < _PlayerPhys._horizontalSpeedMagnitude)
		{
			_PlayerPhys.AddCoreVelocity(_PlayerPhys._RB.velocity.normalized * -stillForce, false);
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
		//Effects
		_Effects.EndSpinDash();
		_CamHandler._HedgeCam.ApplyCameraShake((_releaseShakeAmmount_ * _currentCharge) / 10, 40);
		_Actions._ActionDefault.SwitchSkin(true);

		//Only launches forwards if charged long enough.
		if (_currentCharge < _minimunCharge_)
		{
			_Sounds.Source2.Stop();
			_Actions._ActionDefault.StartAction();
		}
		else
		{
			//Effects
			_Sounds.SpinDashReleaseSound();

			//New speed to gain is determined by charge but affected by -
			Vector3 addForce = _PlayerSkinTransform.forward;
			float speed = _currentCharge;

			//The angle between movement direction and this new force (typically higher with bigger angles)
			float dif = Vector3.Dot(addForce.normalized, _PlayerPhys._RB.velocity.normalized);
			if (_PlayerPhys._horizontalSpeedMagnitude > 20)
				speed *= _forceGainByAngle_.Evaluate(dif);

			//And the current speed (typically lower when at higher speed)
			speed *= _gainBySpeed_.Evaluate(_PlayerPhys._horizontalSpeedMagnitude / _PlayerPhys._currentMaxSpeed);
			addForce *= speed; //Adds speed to direction to get the force

			_PlayerPhys.AddCoreVelocity(addForce, false);

			//Adding velocity is more natural/realistic, but for accuracy in aiming, there is also a rotation towards the new direction.
			Vector3 newSpeed = _PlayerPhys._RB.velocity;
			newSpeed.Normalize();
			dif = Vector3.Angle(_MainSkin.forward, newSpeed);
			dif *= _turnAmountByAngle_.Evaluate(dif);

			newSpeed = Vector3.RotateTowards(newSpeed, _MainSkin.forward, Mathf.Deg2Rad * dif, 0);
			_PlayerPhys.SetCoreVelocity(newSpeed * _PlayerPhys._horizontalSpeedMagnitude, false);

			_CharacterAnimator.SetFloat("GroundSpeed", speed);

			_Actions._ActionDefault.StartAction();
		}

	}

	//Handles the ball animations and set the direction the character faces, by extent aims the direction of the dash on release.
	private void SetAnimatorAndRotation () {

		//Handle animator, both regular and ball ones.
		_Actions._ActionDefault.HandleAnimator(3);
		_BallAnimator.SetInteger("Action", 3);
		_BallAnimator.SetFloat("SpinCharge", _currentCharge * _ballAnimationSpeedMultiplier);

		//Configured to either rotate towards where the camera is facing, or to rotate to where the player is moving.
		switch (_whatControl_)
		{
			case S_Enums.SpinChargeAiming.Camera:
				//Since it requires camera movement, if the camera can't be moved, instead aims by input.
				if (_CamHandler._HedgeCam._isLocked)
					FaceByInput();
				else
					FaceByCamera();
				break;

			case S_Enums.SpinChargeAiming.Input:
				FaceByInput();		
				break;
		}

		//Moves independant of movement and always rotates to have back to the camera.
		void FaceByCamera () {
			if (_Input._move.sqrMagnitude > 0.1f)
			{
				_characterRotation = Quaternion.LookRotation(_Tools.MainCamera.transform.forward - _PlayerPhys._groundNormal * Vector3.Dot(_Tools.MainCamera.transform.forward, _PlayerPhys._groundNormal), transform.up);
			}
		}

		//Follows movement with a slight lerp towards input (since turning is not instant)
		void FaceByInput () {
			Vector3 faceDirection = _PlayerPhys._RB.velocity.sqrMagnitude > 1 ? _PlayerPhys._coreVelocity.normalized : _MainSkin.forward; //If not moving, the direction is based on character

			//Rotate slightly to player input
			if (_PlayerPhys._moveInput.sqrMagnitude > 0.2)
			{
				Vector3 inputDirection = transform.TransformDirection(_PlayerPhys._moveInput.normalized);
				faceDirection = Vector3.RotateTowards(faceDirection, inputDirection, Mathf.Deg2Rad * 100, 0);
			}

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
		_Input.SpecialPressed = false; // Ensures an action like a jump dash won't be performed immediately.
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
	//If not assigned already, sets the tools and stats and gets placement in Action Manager's action list.
	public void ReadyAction () {
		if (_PlayerPhys == null)
		{

			//Assign all external values needed for gameplay.
			_Tools = GetComponentInParent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
			for (int i = 0 ; i < _Actions._MainActions.Count ; i++)
			{
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.SpinCharge)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	private void AssignStats () {
		_spinDashChargingSpeed_ = _Tools.Stats.SpinChargeStat.chargingSpeed;
		_minimunCharge_ = _Tools.Stats.SpinChargeStat.minimunCharge;
		_maximunCharge_ = _Tools.Stats.SpinChargeStat.maximunCharge;
		_spinDashStillForce_ = _Tools.Stats.SpinChargeStat.forceAgainstMovement;
		_speedLossByTime_ = _Tools.Stats.SpinChargeStat.SpeedLossByTime;
		_forceGainByAngle_ = _Tools.Stats.SpinChargeStat.ForceGainByAngle;
		_turnAmountByAngle_ = _Tools.Stats.SpinChargeStat.LerpRotationByAngle;
		_gainBySpeed_ = _Tools.Stats.SpinChargeStat.ForceGainByCurrentSpeed;
		_releaseShakeAmmount_ = _Tools.Stats.SpinChargeStat.releaseShakeAmmount;
		_MaximumSlopeForSpinDash_ = _Tools.Stats.SpinChargeStat.maximumSlopePerformedAt;
		_MaximumSpeedForSpinDash_ = _Tools.Stats.SpinChargeStat.maximumSpeedPerformedAt;
		_whatControl_ = _Tools.Stats.SpinChargeStat.whatAimMethod;
		_tappingBonus_ = _Tools.Stats.SpinChargeStat.tappingBonus;
		_delayBeforeLaunch_ = _Tools.Stats.SpinChargeStat.delayBeforeLaunch;
		_shouldSetRolling_ = _Tools.Stats.SpinChargeStat.shouldSetRolling;
	}
	private void AssignTools () {
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools.GetComponent<S_ActionManager>();
		_CamHandler = _Tools.CamHandler;
		_Input = _Tools.GetComponent<S_PlayerInput>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.MainSkin;
		_BallAnimator = _Tools.BallAnimator;
		_Sounds = _Tools.SoundControl;
		_Effects = _Tools.EffectsControl;

		_PlayerSkinTransform = _Tools.CharacterModelOffset;
		_LowerCapsule = _Tools.CrouchCapsule;
		_CharacterCapsule = _Tools.CharacterCapsule;
	}

	#endregion

}