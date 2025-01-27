using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Action13_Hovering : S_Action_Base, IMainAction
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_Handler_HealthAndHurt         _HurtControl;
	private S_Interaction_Objects           _Objects;
	private Transform			_SkinOffset;

	#endregion


	// Trackers
	#region trackers

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

	new public bool AttemptAction () {
		_Objects._canHover = true;
		return false;
	}

	new public void StartAction (bool overwrite = false) {
		if (enabled || (!_Actions._canChangeActions && !overwrite)) { return; }

		//Visuals
		_CharacterAnimator.SetInteger("Action", 13);
		_CharacterAnimator.SetTrigger("ChangedState");
		_Actions._ActionDefault.SwitchSkin(true);
		_JumpBall.SetActive(false);

		_Input._JumpPressed = false;

		_counter = 0;
		_startForwardDirection = _MainSkin.forward;

		_Actions.ChangeAction(S_S_ActionHandling.PrimaryPlayerStates.Hovering);
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

	//Responsible for assigning objects and components from the tools script.
	public override void AssignTools () {
		base.AssignTools();
		_SkinOffset = _Tools.CharacterModelOffset;
		_HurtControl = _Tools.GetComponent<S_Handler_HealthAndHurt>();
		_Objects = _HurtControl._Objects;
	}

	//Reponsible for assigning stats from the stats script.
	public override void AssignStats () {

	}
	#endregion

}
