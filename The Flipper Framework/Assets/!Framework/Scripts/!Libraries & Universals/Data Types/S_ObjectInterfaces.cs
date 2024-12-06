using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectData
{

}

public interface ITriggerable
{
	void TriggerObjectOn (S_PlayerPhysics Player = null ) {

	}

	void TriggerObjectOff ( S_PlayerPhysics Player = null ) {

	}

	void TriggerObjectEither ( S_PlayerPhysics Player = null ) {

	}

	void ResetObject ( S_PlayerPhysics Player = null) {

	}
}
