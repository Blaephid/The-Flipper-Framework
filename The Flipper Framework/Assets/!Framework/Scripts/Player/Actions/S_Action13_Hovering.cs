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
	private Transform			_SkinOffset;
	private S_Control_SoundsPlayer	_Sounds;

	private  GameObject            _JumpBall;

	#endregion


	// Trackers
	#region trackers
	private int	 _positionInActionList;

	private Vector3	_startForwardDirection;
	private float	_counter;

	#endregion
	#endregion


	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Awake () {
		StartCoroutine(DisableCanHoverEveryFixedUpdate ());
	}

	// Update is called once per frame
	void Update () {
		_Actions._ActionDefault.SetSkinRotationToVelocity(10);
		_SkinOffset.forward = _startForwardDirection; //Because the hover animation spins around, this ensures the player skin doesn't rotate against the animation, even though the main skin goes towards velocity.
	}

	private void FixedUpdate () {
		//This action mainly only exists to have unique connections with inputs and disable other actions. It currently has no unique properties as Interaction_Objects applies the wind force
		HandleInputs ();
		_Actions._ActionDefault.HandleAnimator(13);

		_counter += Time.fixedDeltaTime;
		
		//As soon as there stopes being wind force upwards, end action.
		if (_Objects._totalWindDirection.normalized.y < 0.7f && _counter > 0.2f)
		{
			_Actions._ActionDefault.StartAction();
		}	
	}

	public bool AttemptAction () {
		_Objects._canHover = true;
		return false;
	}

	public void StartAction () {
		//Visuals
		_CharacterAnimator.SetTrigger("ChangedState");
		_Actions._ActionDefault.SwitchSkin(true);
		_JumpBall.SetActive(false);

		_Input._JumpPressed = false;

		_counter = 0;
		_startForwardDirection = _MainSkin.forward;

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Hovering);
		enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { ReadyAction(); return; } //First time is called on ActionManager Awake() to ensure this starts disabled and has a single opportunity to assign tools and stats.	

		_SkinOffset.localEulerAngles = Vector3.zero;
	}
	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	//Ensures canHover is false at the start of the frame, then set to true if AttemptAction() is called. In a coroutine rather than fixed update so it goes even if this script is disabled
	private IEnumerator DisableCanHoverEveryFixedUpdate () {
		while (true)
		{
			yield return new WaitForFixedUpdate();
			_Objects._canHover = false;
		}
	}

	public void HandleInputs () {
		//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
		_Actions.HandleInputs(_positionInActionList);
	}
	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

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
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Hovering)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools._ActionManager;
		_Input = _Tools.GetComponent<S_PlayerInput>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.MainSkin;
		_SkinOffset = _Tools.CharacterModelOffset;
		_Sounds = _Tools.SoundControl;
		_JumpBall = _Tools.JumpBall;

		_HurtControl = _Tools.GetComponent<S_Handler_HealthAndHurt>();
		_Objects = _HurtControl._Objects;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {

	}
	#endregion

}
