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

	private Animator			_CharacterAnimator;
	private Animator			_BallAnimator;
	private S_Handler_Camera		_CamHandler;

	private GameObject			_LowerCapsule;
	private GameObject			_CharacterCapsule;

	private S_ActionManager		_Actions;
	private S_SubAction_Quickstep		_QuickstepManager;

	private S_PlayerPhysics		_PlayerPhys;
	private S_Control_PlayerSound		_Sounds;
	private S_Control_EffectsPlayer	_Effects;

	private SkinnedMeshRenderer[]		_PlayerSkin;
	private SkinnedMeshRenderer		_SpinDashBall;
	private Transform			_PlayerSkinTransform;
	#endregion

	//General
	#region General Properties

	//Stats
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
	private AnimationCurve	_gainBySpeed_;
	private float                 _releaseShakeAmmount_;
	private S_Enums.SpinChargeAiming _whatControl_;
	#endregion

	// Trackers
	#region trackers
	private bool		_wasTapped = false;
	private bool                  _isPressedCurrently = true;

	private float		_currentCharge;
	public float		_spinDashChargedEffectAmm;
	public float		_ballAnimationSpeedMultiplier;

	private float		_time = 0;

	private Quaternion		_CharRot;

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
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();

		}
	}

	// Update is called once per frame
	void Update () {
		SetAnimatorAndRotation();
	}

	private void FixedUpdate () {
		ChargeSpin();
	}

	//Checks if this action can currently be performed, based on the input and environmental factors.
	public bool AttemptAction () {
		bool willChangeAction = false;
		//Pressed on the ground
		if (_Input.spinChargePressed && _PlayerPhys._isGrounded)
		{
			//At a slow enough speed and not on too sharp a slope.
			if (_PlayerPhys._groundNormal.y > _MaximumSlopeForSpinDash_ && _PlayerPhys._horizontalSpeedMagnitude < _MaximumSpeedForSpinDash_)
			{
				_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.SpinCharge);
				StartAction();
				willChangeAction = true;
			}
		}
		return willChangeAction;
	}

	//Called when the action should be enabled.
	public void StartAction () {
		//Play sound
		_Sounds.SpinDashSound();

		//Ready trackers
		_currentCharge = 20;
		_time = 0;
		_isPressedCurrently = true;
		_wasTapped = true;

		//Change size and visual
		_LowerCapsule.SetActive(true);
		_CharacterCapsule.SetActive(false);
		_SpinDashBall.enabled = true;
	}

	public void StopAction() {

	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	private void ChargeSpin () {
		_currentCharge += _spinDashChargingSpeed_;
		_time += Time.deltaTime;

		//Lock camera on behind
		//Cam.Cam.FollowDirection(3, 14f, -10, 0);
		_Input._isCamLocked = false;
		_CamHandler._HedgeCam._lookTimer = 0;


		_Effects.DoSpindash(1, _spinDashChargedEffectAmm * _currentCharge, _currentCharge,
		_Effects.GetSpinDashDust(), _maximunCharge_);

		_Input._move *= 0.4f;
		float stillForce = (_spinDashStillForce_ * _speedLossByTime_.Evaluate(_time)) + 1;
		if (stillForce * 4 < _PlayerPhys._horizontalSpeedMagnitude)
		{
			_PlayerPhys.AddCoreVelocity(_PlayerPhys._RB.velocity.normalized * -stillForce, false);
		}

		//Counter to exit after not pressing button for a bit;


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
				_wasTapped = false;
				_currentCharge += (_spinDashChargingSpeed_ * 2.5f);
			}

			_isPressedCurrently = true;
		}

		//Prevents going over the maximum
		if (_currentCharge > _maximunCharge_)
		{
			_currentCharge = _maximunCharge_;
		}

		startFall();

		_Actions.skid.AttemptAction();

		HandleInputs();
	}

	private IEnumerator DelayRelease () {
		int waitFor = 14;
		if (_wasTapped)
			waitFor = 8;

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

	private void Release () {
		if (_Actions.eventMan != null) _Actions.eventMan.SpinChargesPeformed += 1;

		_Effects.EndSpinDash();
		_CamHandler._HedgeCam.ApplyCameraShake((_releaseShakeAmmount_ * _currentCharge) / 10, 40);
		if (_currentCharge < _minimunCharge_)
		{
			_Sounds.Source2.Stop();
			_Actions.Action00.StartAction();
			_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Default);
		}
		else
		{
			_Sounds.SpinDashReleaseSound();

			Vector3 newForce = _currentCharge * (_PlayerSkinTransform.forward);
			float dif = Vector3.Dot(newForce.normalized, _PlayerPhys._RB.velocity.normalized);

			if (_PlayerPhys._horizontalSpeedMagnitude > 20)
				newForce *= _forceGainByAngle_.Evaluate(dif);
			newForce *= _gainBySpeed_.Evaluate(_PlayerPhys._horizontalSpeedMagnitude / _PlayerPhys._currentMaxSpeed);

			_PlayerPhys.AddCoreVelocity(newForce, false);

			_CharacterAnimator.SetFloat("GroundSpeed", _PlayerPhys._RB.velocity.magnitude);


			_Actions.Action00.SetIsRolling(true);
			_Actions.Action00._rollCounter = 0.3f;

			_Input.LockInputForAWhile(0, false);

			_Actions.Action00.StartAction();
			_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Default);
		}

	}

	private void SetAnimatorAndRotation () {

		//Handle animator, both regular and ball ones.
		_Actions.Action00.HandleAnimator(3);
		_BallAnimator.SetInteger("Action", 3);
		_BallAnimator.SetFloat("SpinCharge", _currentCharge * _ballAnimationSpeedMultiplier);
		//_BallAnimator.speed = _currentCharge * _ballAnimationSpeedMultiplier;

		//Configured to either rotate towards where the camera is facing, or to rotate to where the player is moving.
		switch (_whatControl_)
		{
			case S_Enums.SpinChargeAiming.Camera:
				if (_Input._move.sqrMagnitude > 0.1f)
				{
					_CharRot = Quaternion.LookRotation(_Tools.MainCamera.transform.forward - _PlayerPhys._groundNormal * Vector3.Dot(_Tools.MainCamera.transform.forward, _PlayerPhys._groundNormal), transform.up);
				}
				break;

			case S_Enums.SpinChargeAiming.Input:
				if (_PlayerPhys._RB.velocity != Vector3.zero)
				{
					_CharRot = Quaternion.LookRotation(_PlayerPhys.GetRelevantVel(_PlayerPhys._coreVelocity), transform.up);
				}
				break;
		}

		_CharacterAnimator.transform.rotation = Quaternion.Lerp(_CharacterAnimator.transform.rotation, _CharRot, Time.deltaTime * _Actions.Action00._skinRotationSpeed);


		for (int i = 0 ; i < _PlayerSkin.Length ; i++)
		{
			_PlayerSkin[i].enabled = false;
		}
	}

	private void startFall () {
		//Stop if not grounded
		if (!_PlayerPhys._isGrounded)
		{
			_Input.SpecialPressed = false;
			_Effects.EndSpinDash();
			if (_Input.RollPressed)
			{
				_Actions.Action08.TryDropCharge();
			}
			else
			{
				_Actions.Action00.StartAction();
				_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Default);
			}

		}
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

	#endregion



	public void HandleInputs () {
		_QuickstepManager.AttemptAction();
	}


	

	public void ResetSpinDashVariables () {
		for (int i = 0 ; i < _PlayerSkin.Length ; i++)
		{
			_PlayerSkin[i].enabled = true;
		}
		_SpinDashBall.enabled = false;
		_currentCharge = 0;
	}

	private void AssignStats () {
		_spinDashChargingSpeed_ = _Tools.Stats.SpinChargeStats.chargingSpeed;
		_minimunCharge_ = _Tools.Stats.SpinChargeStats.minimunCharge;
		_maximunCharge_ = _Tools.Stats.SpinChargeStats.maximunCharge;
		_spinDashStillForce_ = _Tools.Stats.SpinChargeStats.forceAgainstMovement;
		_speedLossByTime_ = _Tools.Stats.SpinChargeStats.SpeedLossByTime;
		_forceGainByAngle_ = _Tools.Stats.SpinChargeStats.ForceGainByAngle;
		_gainBySpeed_ = _Tools.Stats.SpinChargeStats.ForceGainByCurrentSpeed;
		_releaseShakeAmmount_ = _Tools.Stats.SpinChargeStats.releaseShakeAmmount;
		_MaximumSlopeForSpinDash_ = _Tools.Stats.SpinChargeStats.maximumSlopePerformedAt;
		_MaximumSpeedForSpinDash_ = _Tools.Stats.SpinChargeStats.maximumSpeedPerformedAt;
		_whatControl_ = _Tools.Stats.SpinChargeStats.whatAimMethod;
}
	private void AssignTools () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Actions = GetComponent<S_ActionManager>();
		_CamHandler = GetComponent<S_Handler_Camera>();
		_Input = GetComponent<S_PlayerInput>();
		_QuickstepManager = GetComponent<S_SubAction_Quickstep>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_BallAnimator = _Tools.BallAnimator;
		_Sounds = _Tools.SoundControl;
		_Effects = _Tools.EffectsControl;

		_PlayerSkin = _Tools.PlayerSkin;
		_PlayerSkinTransform = _Tools.PlayerSkinTransform;
		_SpinDashBall = _Tools.SpinDashBall.GetComponent<SkinnedMeshRenderer>();
		_LowerCapsule = _Tools.crouchCapsule;
		_CharacterCapsule = _Tools.characterCapsule;
	}
}