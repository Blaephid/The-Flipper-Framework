using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SplitTriggered : MonoBehaviour, ITriggerable
{
	[SerializeField] List<GameObject> _FurtherTriggerThese;

	public void TriggerObjectOn ( S_PlayerPhysics Player = null ) {
		S_Trigger_External.TriggerGivenObjects(TriggerTypes.On, _FurtherTriggerThese, Player);
	}

	public void TriggerObjectOff ( S_PlayerPhysics Player = null ) {
		S_Trigger_External.TriggerGivenObjects(TriggerTypes.Off, _FurtherTriggerThese, Player);

	}

	public void TriggerObjectEachFrame ( S_PlayerPhysics Player = null ) {
		S_Trigger_External.TriggerGivenObjects(TriggerTypes.Frame, _FurtherTriggerThese, Player);
	}


	public void TriggerObjectEither ( S_PlayerPhysics Player = null ) {
		S_Trigger_External.TriggerGivenObjects(TriggerTypes.Either, _FurtherTriggerThese, Player);

	}

	public void ResetObject ( S_PlayerPhysics Player = null ) {
		S_Trigger_External.TriggerGivenObjects(TriggerTypes.Reset, _FurtherTriggerThese, Player);

	}
}
