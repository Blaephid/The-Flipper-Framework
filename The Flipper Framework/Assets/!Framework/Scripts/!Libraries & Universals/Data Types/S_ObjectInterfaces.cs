using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_S_ActionHandling;


public interface ITriggerable
{
	void TriggerObjectOn (S_PlayerPhysics Player = null ) {

	}

	void TriggerObjectOff ( S_PlayerPhysics Player = null ) {

	}

	void TriggerObjectEachFrame( S_PlayerPhysics Player = null ) { }

	public void TriggerObjectEither ( S_PlayerPhysics Player = null ) {

	}

	public void ResetObject ( S_PlayerPhysics Player = null) {

	}
}

public enum TriggerTypes
{
	On,
	Off,
	Either,
	Reset,
	Frame
}
