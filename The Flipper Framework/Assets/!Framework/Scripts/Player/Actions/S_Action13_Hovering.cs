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

	private S_Handler_HealthAndHurt         _HurtControl;
	private S_Interaction_Objects           _Objects;

	private Animator			_CharacterAnimator;
	private Transform			_MainSkin;
	private Transform			_PlayerSkin;
	private S_Control_SoundsPlayer	_Sounds;
	private S_Trigger_Updraft		_hoverForce;

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
	void Awake () {
		StartCoroutine(DisableCanHoverEveryFixedUpdate());
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

	// Update is called once per frame
	void Update () {

	}

	private void FixedUpdate () {
		
	}

	public bool AttemptAction () {
		_Objects._canHover = true;
		return false;
	}

	public void StartAction () {
		_PlayerPhys._listOfIsGravityOn.Add(false);

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Hovering);
		enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		
	}

	private IEnumerator DisableCanHoverEveryFixedUpdate () {
		yield return new WaitForFixedUpdate();
		_Objects._canHover = false;
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
		_Actions = _Tools._ActionManager;
		_Input = _Tools.GetComponent<S_PlayerInput>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.MainSkin;
		_PlayerSkin = _Tools.CharacterModelOffset;
		_Sounds = _Tools.SoundControl;

		_HurtControl = _Tools.GetComponent<S_Handler_HealthAndHurt>();
		_Objects = _HurtControl._Objects;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {

	}
	#endregion

}
