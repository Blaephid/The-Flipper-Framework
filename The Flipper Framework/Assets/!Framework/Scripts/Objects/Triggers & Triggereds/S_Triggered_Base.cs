using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_Triggered_Base : S_Vis_Base, ITriggerable
{
	[NonSerialized] public bool _triggeredOnStart;
	[NonSerialized] public bool _notInDefaultState;

	public virtual bool CanBeTriggeredOn ( S_CharacterTools Player ) {
		if (!enabled) { return false; }
		GameObject Go = gameObject;

		_notInDefaultState = true;
		S_Manager_LevelProgress.OnReset += EventReturnOnDeath;

		return true;
	}

	public virtual bool CanBeTriggeredOff ( S_CharacterTools Player ) {
		if (!enabled) { return false; }

		_notInDefaultState = false;
		return true;
	}

	public virtual void StartTriggeredOn ( S_CharacterTools Player = null ) {
		_triggeredOnStart = true;
	}

	public virtual void ResetToOriginal () {

	}

	public virtual void EventReturnOnDeath ( object sender, EventArgs e ) {
		Debug.Log(" reset from " + this);
		this.enabled = true; //In case this component has been disabled or destroyed at some point, which interupts the reset.
		
		GameObject GO = gameObject;
		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;

		//_notInDefaultState is handled seperately by most children of this class, but by default is set to true whenever triggered on.

		if (_notInDefaultState && !_triggeredOnStart) { ResetToOriginal(); } //if changed from default state, and not at the start
		else if (!_notInDefaultState && _triggeredOnStart) { ResetToOriginal(); } //if was set at the start, but has since been changed back
	}
}
