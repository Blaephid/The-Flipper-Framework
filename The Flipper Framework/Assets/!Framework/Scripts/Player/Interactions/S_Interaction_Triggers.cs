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
using System.ComponentModel;
using System;
using static UnityEngine.Rendering.DebugUI;
using System.Reflection;

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
	private List<S_Trigger_External> _CurrentActiveEffectTriggers = new List<S_Trigger_External>();

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

	#region Enabling and Disabling

	public void CheckEffectsTriggerEnter ( Collider Col ) {
		//This static method determines the data of the trigger entered, and returns data if its different, or null if it isn't. It also adds to the list of camera triggers if it shares data.
		List<S_Trigger_Base> EffectsData = S_Interaction_Triggers.CheckTriggerEnter(Col, ref _CurrentActiveEffectTriggers, typeof(S_Trigger_Camera));

		for (int i = 0 ; i < EffectsData.Count ; i++)
		{
			ApplyEffectsOnPlayer(EffectsData[i] as S_Trigger_PlayerEffect);
		}
	}


	public void CheckEffectsTriggerExit ( Collider Col ) {
		//This static method determines the data of the trigger entered, and returns data if its different, or null if it isn't. It also adds to the list of camera triggers if it shares data.
		List<S_Trigger_Base> EffectsData = S_Interaction_Triggers.CheckTriggerEnter(Col, ref _CurrentActiveEffectTriggers, typeof(S_Trigger_Camera));

		for (int i = 0 ; i < EffectsData.Count ; i++)
		{
			ApplyEffectsOnPlayer(EffectsData[i] as S_Trigger_PlayerEffect);
		}
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

		ChangePlayerValues(EffectsData);
	}

	private IEnumerator DelayBeforeRemovingEffectsOnPlayer ( S_Trigger_PlayerEffect EffectsData ) {
		for (int i = 0 ; i < EffectsData._framesBeforeDeactivate ; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		RemoveEffectsOnPlayer(EffectsData);
	}

	private void RemoveEffectsOnPlayer ( S_Trigger_PlayerEffect EffectsData ) {
		if (EffectsData._lockPlayerInputFor > 0)
			_Input.UnLockInput();

		DisableOrEnableActions(EffectsData, true);

		ResetPlayerValues(EffectsData);
	}

	#endregion

	private void ChangePlayerValues ( S_Trigger_PlayerEffect EffectsData ) {
		for (int i = 0 ; i < EffectsData._EditValues.Length ; i++)
		{
			ValueEditing ValueEditor = EffectsData._EditValues[i];

			UnityEngine.Component component = _Tools.Root.GetComponentInChildren(Type.GetType(ValueEditor.ComponentName));
			if (!component) { continue; }

			FieldAndValue fieldAndvalue = S_S_Editor.FindFieldByName(component, ValueEditor.valueName);
			object value = fieldAndvalue.value;
			FieldInfo field = fieldAndvalue.field;

			if (value == null) { Debug.LogError("Could not find " + ValueEditor.valueName); continue; }
			else Debug.Log(value.ToString());

			ValueEditor.rememberComponent = component;
			ValueEditor.rememberValueObject = value;
			ValueEditor.rememberField = field;
			SetOrResetValue(ValueEditor, value, value, true, component, field);
		}
	}

	private void ResetPlayerValues ( S_Trigger_PlayerEffect EffectsData ) {
		for (int i = 0 ; i < EffectsData._EditValues.Length ; i++)
		{
			ValueEditing ValueEditor = EffectsData._EditValues[i];

			UnityEngine.Component component = ValueEditor.rememberComponent;
			FieldInfo field = ValueEditor.rememberField;
			object value = field.GetValue(component);

			if (value == null || component == null) { continue; }

			SetOrResetValue(ValueEditor, value, null, false, component, field);
		}
	}

	private void SetOrResetValue ( ValueEditing ValueEditor, object value, object setRemember, bool replace, UnityEngine.Component component, FieldInfo field ) {
		switch (ValueEditor.DataType)
		{
			case DataTypes.Float:
				field.SetValue(component, replace ? ValueEditor.replaceFloat : ValueEditor.rememberFloat);
				ValueEditor.rememberFloat = setRemember == null ? 0f : (float)setRemember;
				break;

			case DataTypes.Boolean:
				field.SetValue(component, replace ? ValueEditor.replaceBool : ValueEditor.rememberBool);
				ValueEditor.rememberBool = setRemember == null ? false : (bool)setRemember;
				break;

			case DataTypes.Vector3:
				field.SetValue(component, replace ? ValueEditor.replaceVector3 : ValueEditor.rememberVector3);
				ValueEditor.rememberVector3 = setRemember == null ? Vector3.zero : (Vector3)setRemember;
				break;

			case DataTypes.Vector2:
				field.SetValue(component, replace ? ValueEditor.replaceVector2 : ValueEditor.rememberVector2);
				ValueEditor.rememberVector2 = setRemember == null ? Vector2.zero : (Vector2)setRemember;
				break;

			case DataTypes.String:
				field.SetValue(component, replace ? ValueEditor.replaceString : ValueEditor.rememberString);
				ValueEditor.rememberString = setRemember == null ? "" : (string)setRemember;
				break;

			case DataTypes.Int:
				field.SetValue(component, replace ? ValueEditor.replaceInt : ValueEditor.rememberInt);
				ValueEditor.rememberInt = setRemember == null ? 0 : (int)setRemember;
				break;

			default:
				Debug.LogError("Unhandled DataType: " + ValueEditor.DataType);
				break;
		}
	}

	private void DisableOrEnableActions ( S_Trigger_PlayerEffect EffectsData, bool set ) {
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
	public static List<S_Trigger_Base> CheckTriggerEnter ( Collider Col, ref List<S_Trigger_External> list, Type TypeOfTriggerScript ) {

		List <S_Trigger_Base> TriggerList = new List<S_Trigger_Base>();

		//What happens depends on the data set to the camera trigger in its script.
		if (!Col.TryGetComponent(out S_Trigger_External TriggerData)) { return null; };

		//If no logic is found, ignore.
		if (TriggerData == null || TriggerData._TriggersForPlayerToRead.Count == 0) return null;

		for(int i = 0 ; i < TriggerData._TriggersForPlayerToRead.Count ; i++)
		{
			TriggerData = TriggerData._TriggersForPlayerToRead[i].GetComponent<S_Trigger_External>();

			//If either there isn't any camera logic already in effect, or this is a new trigger unlike the already active one, set this as the first active.
			if (list.Count == 0) { list = new List<S_Trigger_External>(); }

			//If the new trigger is set to trigger the logic already in effect, add it to list for tracking how long until out of every trigger, and don't restart the logic.
			else if (list.Contains(TriggerData))
			{ list.Add(TriggerData); return null; }

			list.Add(TriggerData);
			TriggerData._isSelected = true;

			TriggerList.Add(TriggerData);
		}
		return TriggerList;
	}

	public static List<S_Trigger_Base> CheckTriggerExit ( Collider Col, ref List<S_Trigger_External> list ) {


		List <S_Trigger_Base> TriggerList = new List<S_Trigger_Base>();

		//What happens depends on the data set to the camera trigger in its script.
		if (!Col.TryGetComponent(out S_Trigger_External TriggerData)) { return null; }

		//If no logic is found, ignore.
		if (TriggerData == null || TriggerData._TriggersForPlayerToRead == null) return null;

		for (int i = 0 ; i < TriggerData._TriggersForPlayerToRead.Count ; i++)
		{

			TriggerData = TriggerData._TriggersForPlayerToRead[i].GetComponent<S_Trigger_External>();

			//If the trigger exited is NOT set to the same logic as currently active, then don't do anything.
			if (list.Count > 0 && !list.Contains(TriggerData)) { return null; }

			//If it is, then remove one from the list to track how many triggers under the same logic have been left. This allows the effect to not end until not in any triggers under the same logic.
			list.Remove(TriggerData);

			if (list.Contains(TriggerData)) { return null; } //Only perform exit logic when out of all triggers using that logic.

			TriggerData._isSelected = false;
			TriggerList.Add(TriggerData);
		}

		return TriggerList;
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
