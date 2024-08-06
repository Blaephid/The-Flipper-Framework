using UnityEngine;
using System.Collections;
using UnityEditor;

public class S_Action08_DropCharge : MonoBehaviour, IMainAction
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
	private S_Handler_Camera      _CamHandler;
	private S_Control_SoundsPlayer _Sounds;

	private Animator              _CharacterAnimator;
	private Transform             _MainSkin;
	private Transform             _FeetPoint;
	[HideInInspector]
	public ParticleSystem         _DropEffect;
	private GameObject            _JumpBall;
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private float        _chargingSpeed_ = 1.5f;
	private float        _minimunCharge_ = 10;
	private float        _maximunCharge_ = 100;

	private Vector2	_cameraPauseEffect_ = new Vector2(3, 40);

	private float       _minimumHeightToDropCharge_;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;         //In every action script, takes note of where in the Action Managers Main action list this script is. 

	private bool        _isCharging = false;          //If true, increase charge, if false (set if not inputting), prepare to exit action after a delay.
	private bool        _hasLanded;		//Used to prevent release being called mutliple times in the time between being grounded and changing action.

	[HideInInspector]
	public float        _charge;			//The current speed charged up and ready.
	private RaycastHit  _FloorHit;		//A hit on the ground

	public float        _releaseShakeAmmount;	//Camera shake when performed
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
		ReadyAction();
	}

	private void OnDisable () {
		_DropEffect.Stop();
	}

	// Update is called once per frame
	void Update () {
		//Set Animator Parameters
		_Actions._ActionDefault.HandleAnimator(1);
		_Actions._ActionDefault.SetSkinRotationToVelocity(_Actions._ActionDefault._skinRotationSpeed);
	}

	private void FixedUpdate () {
		HandleInputs();

		ChargeDash();
		CheckGround();
	}

	public bool AttemptAction () {
		if (!_PlayerPhys._isGrounded && _Input.RollPressed && _PlayerPhys._RB.velocity.y < 40f)
		{
			if (!Physics.Raycast(_FeetPoint.position, -transform.up, _minimumHeightToDropCharge_, _PlayerPhys._Groundmask_))
			{
				StartAction();
				return true;
			}
		}
		return false;
	}

	public void StartAction () {

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.DropCharge);
		this.enabled = true;

		//Effects
		//Animator
		_CharacterAnimator.SetInteger("Action", 1);
		_CharacterAnimator.SetBool("Grounded", false);
		_Actions._ActionDefault.SwitchSkin(false); //Ensures player is a ball
						  //Sound
		_Sounds.SpinDashSound();


		//Ensures the effect is surrounding the character
		if (!_DropEffect.isPlaying)
		{
			_DropEffect.Play();
		}

		_isCharging = true;
		_hasLanded = false;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		if (_DropEffect.isPlaying)
		{
			_DropEffect.Stop();
		}
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

	//Increases power to launch with based on input still being pressed.
	private void ChargeDash () {
		if (_isCharging)
		{
			_charge = Mathf.Clamp(_charge + (_chargingSpeed_ * Time.deltaTime), 0, _maximunCharge_); //Increase charge

			//If input is released, then end sound and prepare to end action after a delay.
			if (!_Input.RollPressed)
			{
				_isCharging = false;
				StartCoroutine(DelayEndingAction());
			}
		}
		//Won't increase charge, but can return to charging if input is pressed again.
		else
		{
			if (_Input.RollPressed)
			{
				_isCharging = true;
				StopCoroutine(DelayEndingAction());
			}
		}
	}

	//When releasing button, add a delay before exiting the action. This is to make launching when grounded easier as players will naturally release the button right before the ground.
	private IEnumerator DelayEndingAction () {
		_Sounds.Source2.Stop(); //Ends sounds, to show no longer charging.

		yield return new WaitForSeconds(0.45f);

		//If coroutine is not stopped or interupted, then end the action
		if (_Actions._whatAction == S_Enums.PrimaryPlayerStates.DropCharge)
		{
			_Actions._ActionDefault._animationAction = 1;
			_Actions._ActionDefault.StartAction();
		}
	}

	//Checks whether or not to launch the player, either by landing on the ground or performing a dash.
	private void CheckGround () {

		//Pressing the special button will cause a dash while still in the air, affected by charge.
		if (_Input.SpecialPressed && _charge > _minimunCharge_)
		{
			AirRelease();
		}

		else
		{
			//Check if on the ground, either by using the ground check or physics, or a different one based on fall speed with a capsule (to ensure not hitting a corner and not bouncing).
			bool isRaycasthit = Physics.SphereCast(_FeetPoint.position, 3 , -transform.up, out _FloorHit, (_PlayerPhys._coreVelocity.y * Time.deltaTime * 0.6f), _PlayerPhys._Groundmask_);
			bool isGroundHit = _PlayerPhys._isGrounded || isRaycasthit;
			RaycastHit UseHit = isRaycasthit ? _FloorHit : _PlayerPhys._HitGround; //Get which one to use

			//If hits the ground, apply new forces and effects but also change back to default action.
			if (isGroundHit && !_hasLanded)
			{
				_hasLanded = true;
				Release(UseHit.normal);
			}
		}
	}

	//Called when dashing before hitting the ground, disabled buttons, and decreases charge before normal release
	private void AirRelease () {

		//Since activated by pressing a button, ensure none others are pressed so there aren't immediate transitions.
		_Input.JumpPressed = false;
		_Input.SpecialPressed = false;
		_Input.HomingPressed = false;

		_charge *= 0.6f; //Launcing in the air has less power than grounded.

		StartCoroutine(DashThroughAir());

		Release(transform.up);
	}

	//Takes in a normal, aligns direction relative to it, then gets a force to apply/
	private void Release ( Vector3 upNormal ) {
		_charge = Mathf.Clamp(_charge, _minimunCharge_, _maximunCharge_);
		Vector3 force = _PlayerPhys.AlignWithNormal(_MainSkin.forward, upNormal, _charge);

		StartCoroutine(DelayForce(force, 2)); //Will apply this force after a few frames, this is to give a chance to properly align to the ground.

		//Effects
		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraPause(_cameraPauseEffect_, new Vector2(_PlayerPhys._horizontalSpeedMagnitude, _charge), 0.25f)); //The camera will fall back before catching up.

		//Control
		StartCoroutine(_PlayerPhys.LockFunctionForTime(S_PlayerPhysics.EnumControlLimitations.canDecelerate, 0, 15));
	}

	//When releasing in the air, requires different effects to ensure not falling.
	private IEnumerator DashThroughAir () {
		float time = 1 + Mathf.Round(_charge / 30); //Seperate in increments (0 - 30 charge = 1 second)
		time = Mathf.Clamp(time / 10, 0.1f, 10); //Change seconds into 0.1 seconds.

		//Prevent downward velocity from gravity until completed
		_PlayerPhys._isGravityOn = false;
		yield return new WaitForSeconds(time);
		_PlayerPhys._isGravityOn = true;
	}

	//Prevents force being applied until enough fixed frames have passed. This is to give some time to properly rotate to match ground.
	private IEnumerator DelayForce ( Vector3 force, int delay ) {
		for (int i = 1 ; i <= delay ; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		Launch(force);
	}

	//Launch the player forwards
	private void Launch ( Vector3 force ) {

		//Effects
		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraShake((_releaseShakeAmmount * _charge) / 100, 10));
		_Sounds.SpinDashReleaseSound();

		//Ensure player is aligned to the ground below them.
		if (_PlayerPhys._isGrounded) { _PlayerPhys.AlignToGround(_PlayerPhys._groundNormal, true); }

		//Make force relevant to character's current rotation
		Vector3 releVec = _PlayerPhys.GetRelevantVector(force, false);

		//If the new total force is higher than current speed, then apply it. Uses sqrs because it's faster with comparing magnitudes
		if (releVec.sqrMagnitude > Mathf.Pow(_PlayerPhys._horizontalSpeedMagnitude + _charge * 0.1f, 2))
		{
			_PlayerPhys.SetCoreVelocity(force, "Overwrite");
			_CamHandler._HedgeCam.ChangeHeight(18, 25f); //Ensures camera will go behind the player as they launch forwards from falling.
		}
		//Else, just add force to increase total speed. This will also happen if the new speed it only slightly more than the movement speed.
		else
		{
			_PlayerPhys.AddCoreVelocity(force * 0.2f);
			_CamHandler._HedgeCam.ChangeHeight(20, 15f); //Ensures camera will go behind the player as they launch forwards from falling.
		}

		_Actions._ActionDefault.StartAction();
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//Returns the current charge. This is for carry over charge gained when this can't add velocity itself (like when landing on a rail)
	public float GetCharge () {
		//Effects
		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraShake((_releaseShakeAmmount * _charge) / 150, 1));
		_Sounds.SpinDashReleaseSound();

		return _charge;
	}

	//This has to be set up in Editor. The invoker is in the PlayerPhysics script component, adding this event to it will mean this is called whenever the player lands.
	public void EventOnGrounded () {
		StartCoroutine(ApplyWhenOutOfAction());
	}

	//Only set to 0 on grounded, which means charge can keep increasing until used, allowing to carry over if interupting a charge with a different action but not hitting the ground.
	//However, it won't reset charge until action has changed because this would get in the way of the force due to the event being called first.
	private IEnumerator ApplyWhenOutOfAction () {
		while (true)
		{
			yield return new WaitForFixedUpdate();
			if (_Actions._whatAction != S_Enums.PrimaryPlayerStates.DropCharge)
			{
				yield return new WaitForFixedUpdate();
				_charge = 0;
			}
		}
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
			_Tools = GetComponentInParent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
			for (int i = 0 ; i < _Actions._MainActions.Count ; i++)
			{
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.DropCharge)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools._ActionManager;
		_CamHandler = _Tools.CamHandler;

		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
		_DropEffect = _Tools.DropEffect;

		_MainSkin = _Tools.MainSkin;
		_FeetPoint = _Tools.FeetPoint;
		_JumpBall = _Tools.JumpBall;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_chargingSpeed_ =	_Tools.Stats.DropChargeStats.chargingSpeed;
		_minimunCharge_ =		_Tools.Stats.DropChargeStats.minimunCharge;
		_maximunCharge_ =		_Tools.Stats.DropChargeStats.maximunCharge;
		_minimumHeightToDropCharge_ =	_Tools.Stats.DropChargeStats.minimumHeightToPerform;

		_cameraPauseEffect_ =	_Tools.Stats.DropChargeStats.cameraPauseEffect;
	}
	#endregion
}
