using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_Triggered_Base : S_Vis_Base, ITriggerable
{
	[NonSerialized] public bool _notInStartState;
	[NonSerialized] public bool _canBeTriggeredOn = true;

	[NonSerialized] public bool _doesTriggeringOffResetToStartState = true; //Set per child. If false, then the script doesnt go back to its default state when turned off.

	public void TriggerObjectOn ( S_CharacterTools Player = null ) {
		if (!CanBeTriggeredOn(Player)) { return; }
		ChildTriggerObjectOn(Player);
	}

	public virtual void ChildTriggerObjectOn ( S_CharacterTools Player = null ) {

	}

	private bool CanBeTriggeredOn ( S_CharacterTools Player ) {
		if (!enabled || !_canBeTriggeredOn) { return false; }
		GameObject Go = gameObject;

		//If first turned on since start or player death.
		if (!_notInStartState)
		{
			Debug.Log(gameObject + " No longer in start state");
			S_Manager_LevelProgress.OnReset += EventReturnOnDeath;
		}
		_notInStartState = true;

		return true;
	}

	//
	public void StartTriggeredOn ( S_CharacterTools Player = null ) {
		ChildTriggerObjectOn(Player);
	}

	public void TriggerObjectOff ( S_CharacterTools Player = null ) {
		if (!CanBeTriggeredOff(Player, _doesTriggeringOffResetToStartState)) { return; }
		ChildTriggerObjectOff();
	}

	public virtual void ChildTriggerObjectOff ( S_CharacterTools Player = null ) {

	}

	private bool CanBeTriggeredOff ( S_CharacterTools Player, bool settingToStartState ) {

		if (settingToStartState && _notInStartState)
		{
			Debug.Log(gameObject + "Back in start state");
			S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
			_notInStartState = false;
		}
		return true;
	}

	public virtual void ChildResetToOriginal () {

	}

	public virtual void EventReturnOnDeath ( object sender, EventArgs e ) {
		Debug.Log(" reset " + this);
		this.enabled = true; //In case this component has been disabled or destroyed at some point, which interupts the reset.

		if (_notInStartState) { ChildResetToOriginal(); } //if changed from start state. The start state may have been changed by StartTriggeredOn

		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
		_notInStartState = false;
		_canBeTriggeredOn = true;

		Debug.Log("finish reset from " + this);
	}
}
