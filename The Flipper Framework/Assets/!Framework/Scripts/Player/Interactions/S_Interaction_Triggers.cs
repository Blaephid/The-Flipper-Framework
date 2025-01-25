using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;
using UnityEngine.ProBuilder;

public class S_Interaction_Triggers : MonoBehaviour
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties

	[Header("Scripts")]
	//Player
	private S_CharacterTools      _Tools;
	private S_PlayerPhysics       _PlayerPhys;
	private S_PlayerVelocity      _PlayerVel;
	private S_ActionManager       _Actions;
	private S_PlayerInput         _Input;
	private S_PlayerEvents        _Events;

	private S_Handler_CharacterAttacks      _AttackHandler;
	private S_Handler_HealthAndHurt         _HurtAndHealth;

	private S_Spawn_UI.StrucCoreUIElements _CoreUIElements;
	#endregion


	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited
	private void Start () {
		if (_PlayerPhys == null)
		{
			AssignTools(); //Called during start instead of awake because it gives time for tools to be acquired (such as the UI needing to be spawned).
		}
	}


	public void EventTriggerEnter ( Collider Col ) {
		switch (Col.tag)
		{
			case "Switch":
				if (Col.GetComponent<S_Data_Switch>() != null)
				{
					Col.GetComponent<S_Data_Switch>().Activate();
				}
				break;

			case "HintRing":
				ActivateHintBox(Col);
				break;


			case "Player Effects":
				ApplyEffectsOnPlayer(Col); break;
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	
	private void ApplyEffectsOnPlayer ( Collider Col ) {

		if (!Col.TryGetComponent(out S_Trigger_PlayerEffect Effects)) { return; }

		switch (Effects._setPlayerGrounded)
		{
			case S_GeneralEnums.ChangeGroundedState.SetToNo:
				_PlayerPhys.SetIsGrounded(false); break;
			case S_GeneralEnums.ChangeGroundedState.SetToYes:
				_PlayerPhys.SetIsGrounded(true); break;
			case S_GeneralEnums.ChangeGroundedState.SetToOppositeThenBack:
				bool current = _PlayerPhys._isGrounded;
				_PlayerPhys.SetIsGrounded(!current); _PlayerPhys.SetIsGrounded(current);
				break;
		}

		if (Effects._lockPlayerInputFor > 0)
			_Input.LockInputForAWhile(Effects._lockPlayerInputFor, true, Vector3.zero, Effects._LockInputTo_);
	}

	private void ActivateHintBox ( Collider Col ) {
		if (!Col.TryGetComponent(out S_Data_HintRing HintRingScript)) { return; } //Ensures object has necessary script, and saves as varaible for efficiency.

		if (Col.gameObject == _CoreUIElements.HintBox._CurrentHintRing) { return; } //Do not perform function if this hint is already being displayed in the hintBox. Prevents restarting a hint when hitting it multiple times until its complete.
		_CoreUIElements.HintBox._CurrentHintRing = Col.gameObject; //Relevant to the above check.

		//Effects
		HintRingScript.hintSound.Play();

		//Using mouse is set when _PlayerInput detects a camera or move input coming from a keyboard or mouse, and this ensures the correct text will be displayed to match the controller device.
		if (_Input._isUsingMouse)
		{
			_CoreUIElements.HintBox.ShowHint(HintRingScript.hintText, HintRingScript.hintDuration);
		}

		//If not using a mouse, must be using a gamepad.
		else
		{
			Gamepad input = Gamepad.current;

			//Depending on the type of input, will set the display string array to the one matching that input.
			//Note, this could be done much better. This version requires copying out the same data for every array for each input on the hint ring object, but a system could be built to have only one array using 
			//KEYWORDS that are replaced with different strings matching the input. E.G. "Press the JUMPBUTTON to", replaces JUMPBUTTON with a string matching the binding for the current input in the PlayerInput file.
			switch (input)
			{
				case (SwitchProControllerHID):
					CheckHint(HintRingScript.hintTextGamePad, HintRingScript.hintText, HintRingScript.hintDuration);
					break;
				case (DualSenseGamepadHID):
					CheckHint(HintRingScript.hintTextPS4, HintRingScript.hintText, HintRingScript.hintDuration);
					break;
				case (DualShock3GamepadHID):
					CheckHint(HintRingScript.hintTextPS4, HintRingScript.hintText, HintRingScript.hintDuration);
					break;
				case (DualShock4GamepadHID):
					CheckHint(HintRingScript.hintTextPS4, HintRingScript.hintText, HintRingScript.hintDuration);
					break;
				case (DualShockGamepad):
					CheckHint(HintRingScript.hintTextPS4, HintRingScript.hintText, HintRingScript.hintDuration);
					break;
				case (XInputController):
					CheckHint(HintRingScript.hintTextXbox, HintRingScript.hintText, HintRingScript.hintDuration);
					break;
				//If input is none of the above, display the default.
				default:
					_CoreUIElements.HintBox.ShowHint(HintRingScript.hintText, HintRingScript.hintDuration);
					break;
			}
		}
	}

	private void CheckHint ( string[] thisHint, string[] baseHint, float[] duration ) {
		if (thisHint.Length == 0)
			_CoreUIElements.HintBox.ShowHint(baseHint, duration);
		else
			_CoreUIElements.HintBox.ShowHint(thisHint, duration);
	}

	#endregion


	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	private void AssignTools () {
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_PlayerVel = _Tools.GetComponent<S_PlayerVelocity>();

		_Actions = _Tools._ActionManager;
		_Events = _Tools.PlayerEvents;
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_AttackHandler = GetComponent<S_Handler_CharacterAttacks>();
		_HurtAndHealth = _Tools.GetComponent<S_Handler_HealthAndHurt>();

		_CoreUIElements = _Tools.UISpawner._BaseUIElements;
	}
	#endregion
}
