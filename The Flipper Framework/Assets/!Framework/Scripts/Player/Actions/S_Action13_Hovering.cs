using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Action13_Hovering : MonoBehaviour, IMainAction
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

	Animator _CharacterAnimator;
	private Transform   _MainSkin;
	Transform _PlayerSkin;
	S_Control_SoundsPlayer _Sounds;
	S_Trigger_Updraft _hoverForce;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	#endregion

	// Trackers
	#region trackers
	float floatSpeed = 15;
	public AnimationCurve forceFromSource;

	[HideInInspector] public bool inWind;
	float exitWindTimer;
	float exitWind = 0.6f;
	Vector3 forward;
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
			_Tools = GetComponentInParent<S_CharacterTools>();
			AssignTools();
			AssignStats();
		}
	}
	private void OnDisable () {
		_CharacterAnimator.SetInteger("Action", 1);
		_PlayerSkin.forward = _MainSkin.forward;
		_PlayerPhys._isGravityOn = true;
		inWind = false;
	}

	// Update is called once per frame
	void Update () {
		_CharacterAnimator.SetInteger("Action", 13);

	}

	private void FixedUpdate () {
		updateModel();
		_PlayerPhys.SetIsGrounded(false);

		getForce();

		if (inWind)
		{
			exitWindTimer = 0;

			if (_PlayerPhys._RB.velocity.y < floatSpeed)
			{
				_PlayerPhys.AddCoreVelocity(_hoverForce.transform.up * floatSpeed);
			}

		}
		else
		{
			exitWindTimer += Time.deltaTime;

			if (_PlayerPhys._RB.velocity.y < floatSpeed)
			{
				_PlayerPhys.AddCoreVelocity(_hoverForce.transform.up * (floatSpeed * 0.35f));
			}

			if (exitWindTimer >= exitWind)
			{
				_Actions._ActionDefault.StartAction();
			}
		}
	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		willChangeAction = true;
		return willChangeAction;
	}

	public void StartAction () {

	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.

		enabled = false;

		if (isFirstTime) { return; } //If first time, then return after setting to disabled.
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
		
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools.GetComponent<S_ActionManager>();
		_Input = _Tools.GetComponent<S_PlayerInput>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.MainSkin;
		_PlayerSkin = _Tools.CharacterModelOffset;
		_Sounds = _Tools.SoundControl;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {

	}
	#endregion

	public void InitialEvents ( S_Trigger_Updraft up ) {
		_PlayerPhys._isGravityOn = false;
		inWind = true;
		forward = _PlayerSkin.forward;

		_hoverForce = up;
	}

	public void updateHover ( S_Trigger_Updraft up ) {
		inWind = true;
		_hoverForce = up;
	}

	void updateModel () {
		//Set Animation Angle
		Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
		if (VelocityMod != Vector3.zero)
		{
			Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
			_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, CharRot, Time.deltaTime * _Actions._ActionDefault._skinRotationSpeed);
		}
		_PlayerSkin.forward = forward;
	}

	void getForce () {
		float distance = transform.position.y - _hoverForce.bottom.position.y;
		float difference = distance / (_hoverForce.top.position.y - _hoverForce.bottom.position.y);
		floatSpeed = forceFromSource.Evaluate(difference) * _hoverForce.power;
		Debug.Log(difference);

		if (difference > 0.98)
		{
			floatSpeed = -Mathf.Clamp(_PlayerPhys._RB.velocity.y, -100, 0);
		}
		else if (_PlayerPhys._RB.velocity.y > 0)
		{
			floatSpeed = Mathf.Clamp(floatSpeed, 0.5f, _PlayerPhys._RB.velocity.y);
		}
	}
}
