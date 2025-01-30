using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;
using UnityEngine.ProBuilder;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

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
	private S_Handler_Camera            _CamHandler;

	//This is used to check what the current dominant trigger is, as multiple triggers might be working together under one effect. These will have their read values set to the same.
	private List<S_Trigger_Base> _CurrentActiveEffectTriggers = new List<S_Trigger_Base>();

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

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private (can be called externally as long as only working on scripts purpose)

	public void CheckEffectsTriggerEnter(Collider Col ) {
		//This static method determines the data of the trigger entered, and returns data if its different, or null if it isn't. It also adds to the list of camera triggers if it shares data.
		S_Trigger_PlayerEffect EffectsData = S_Interaction_Triggers.CheckTriggerEnter(Col, ref _CurrentActiveEffectTriggers) as S_Trigger_PlayerEffect;
		if (EffectsData) { ApplyEffectsOnPlayer(EffectsData); }
	}
	
	private void ApplyEffectsOnPlayer ( S_Trigger_PlayerEffect EffectsData ) {

		switch (EffectsData._setPlayerGrounded)
		{
			case S_GeneralEnums.ChangeGroundedState.SetToNo:
				_PlayerPhys.SetIsGrounded(false); break;
			case S_GeneralEnums.ChangeGroundedState.SetToYes:
				_PlayerPhys.SetIsGrounded(true); break;
			case S_GeneralEnums.ChangeGroundedState.SetToOppositeThenBack:
				bool current = _PlayerPhys._isGrounded;
				_PlayerPhys.SetIsGrounded(!current); 
				_PlayerPhys.SetIsGrounded(current);
				break;
		}

		if (EffectsData._lockPlayerInputFor > 0)
			_Input.LockInputForAWhile(EffectsData._lockPlayerInputFor, true, Vector3.zero, EffectsData._LockInputTo_);
		else if (EffectsData._lockPlayerInputFor == -1)
			_Input.LockInputIndefinately(true, Vector3.zero, EffectsData._LockInputTo_);

		DisableOrEnableActions(EffectsData, false);
	}

	public void CheckEffectsTriggerExit ( Collider Col ) {
		//This static method determines the data of the trigger entered, and returns data if its different, or null if it isn't. It also adds to the list of camera triggers if it shares data.
		S_Trigger_PlayerEffect EffectsData = S_Interaction_Triggers.CheckTriggerExit(Col, ref _CurrentActiveEffectTriggers) as S_Trigger_PlayerEffect;
		if (EffectsData) { StartCoroutine(DelayBeforeRemovingEffectsOnPlayer(EffectsData)); }
	}

	private IEnumerator DelayBeforeRemovingEffectsOnPlayer ( S_Trigger_PlayerEffect EffectsData ) {
		for (int i = 0 ; i < EffectsData._framesBeforeDeactivate ; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		RemoveEffectsOnPlayer(EffectsData);
	}

	private void RemoveEffectsOnPlayer ( S_Trigger_PlayerEffect EffectsData ) {
		if(EffectsData._deactivateOnExit)
		{
			_Input.UnLockInput();

			DisableOrEnableActions(EffectsData, true);
		}
	}

	private void DisableOrEnableActions( S_Trigger_PlayerEffect EffectsData, bool set ) {
		for (int i = 0 ; i < EffectsData._ActionsToDisable.Length ; i++)
		{
			_Actions.DisableOrEnableActionOfType(EffectsData._ActionsToDisable[i], set);
		}
		for (int i = 0 ; i < EffectsData._SubActionsToDisable.Length ; i++)
		{
			_Actions.DisableOrEnableSubActionOfType(EffectsData._SubActionsToDisable[i], set);
		}
	}

	public void ActivateHintBox ( Collider Col ) {
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
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	#region Public And Static
	public static S_Trigger_Base CheckTriggerEnter ( Collider Col, ref List<S_Trigger_Base> list ) {

		//What happens depends on the data set to the camera trigger in its script.
		if (!Col.TryGetComponent(out S_Trigger_Base TriggerData)) { return null; };

		//If no logic is found, ignore.
		if (TriggerData == null || TriggerData._TriggerForPlayerToRead == null) return null;

		TriggerData = TriggerData._TriggerForPlayerToRead.GetComponent<S_Trigger_Base>();

		//If either there isn't any camera logic already in effect, or this is a new trigger unlike the already active one, set this as the first active.
		if (list.Count == 0) { list = new List<S_Trigger_Base>() { TriggerData }; }

		//If the new trigger is set to trigger the logic already in effect, add it to list for tracking how long until out of every trigger, and don't restart the logic.
		else if (TriggerData == list[0]) { list.Add(TriggerData); return null; }

		TriggerData._isSelected = true;
		return TriggerData;
	}

	public static S_Trigger_Base CheckTriggerExit ( Collider Col, ref List<S_Trigger_Base> list ) {
		//What happens depends on the data set to the camera trigger in its script.
		if (!Col.TryGetComponent(out S_Trigger_Base TriggerData)) { return null; }

		//If no logic is found, ignore.
		if (TriggerData == null || TriggerData._TriggerForPlayerToRead == null) return null;

		TriggerData = TriggerData._TriggerForPlayerToRead.GetComponent<S_Trigger_Base>();

		//If the trigger exited is NOT set to the same logic as currently active, then don't do anything.
		if (list.Count > 0 && TriggerData != list[0]) { return null; }
		//If it is, then remove one from the list to track how many triggers under the same logic have been left. This allows the effect to not end until not in any triggers under the same logic.
		list.RemoveAt(list.Count - 1);

		if (list.Count > 0) { return null; } //Only perform exit logic when out of all triggers using that logic.

		TriggerData._isSelected = false;
		return TriggerData;
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
		_CamHandler = _Tools.CamHandler;

		_CoreUIElements = _Tools.UISpawner._BaseUIElements;
	}
	#endregion
}
