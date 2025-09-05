using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_S_ActionHandling;


public interface ITriggerable
{
	public void TriggerObjectOn (S_CharacterTools Player = null ) {

	}

	public void TriggerObjectOnce ( S_CharacterTools Player = null ) {

	}

	public void TriggerObjectOff ( S_CharacterTools Player = null ) {

	}

	public void TriggerObjectEachFrame( S_CharacterTools Player = null ) { 
	}

	public void TriggerObjectEither ( S_CharacterTools Player = null ) {

	}

	public void ResetObject ( S_CharacterTools Player = null) {

	}
	public void StartTriggeredOn ( S_CharacterTools Player = null ) {
	}
}

public enum TriggerTypes
{
	On,
	Once,
	Off,
	Either,
	Reset,
	Frame,
	Start
}
