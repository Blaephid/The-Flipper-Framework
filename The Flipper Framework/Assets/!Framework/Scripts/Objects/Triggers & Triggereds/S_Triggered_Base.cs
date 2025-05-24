using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_Triggered_Base : S_Vis_Base, ITriggerable
{
	[NonSerialized] public bool _startedTriggeredOn;
	[NonSerialized] public bool _isCurrentlyOn;

	public virtual bool CanBeTriggeredOn ( S_CharacterTools Player ) {
		if (!enabled) { return false; }

		S_Manager_LevelProgress.OnReset += EventReturnOnDeath;

		return true;
	}

	public virtual bool CanBeTriggeredOff ( S_CharacterTools Player ) {
		if (!enabled) { return false; }

		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
		return true;
	}

	public virtual void StartTriggeredOn ( S_CharacterTools Player = null ) {
		_startedTriggeredOn = true;
	}

	public virtual void ResetToOriginal () {

	}

	public virtual void EventReturnOnDeath ( object sender, EventArgs e ) {
		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;

		if (_isCurrentlyOn && !_startedTriggeredOn) { ResetToOriginal(); }
		else if (!_isCurrentlyOn && _startedTriggeredOn) { ResetToOriginal(); }
	}
}
