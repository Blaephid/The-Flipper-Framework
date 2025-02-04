using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Trigger_PlayerEffect : S_Trigger_Base
{

	public S_Trigger_PlayerEffect () {
		_TriggerObjects._isLogicInPlayScript = true;
	}

	[Header("Interaction")]
	[SetBoolIfOther(true, "_ActionsToDisable", 0), SetBoolIfOther(true, "_SubActionsToDisable", 0)]
	public bool		_deactivateOnExit;
	public float	_framesBeforeDeactivate = 0;

	[Header("Input")]
	[Range(-1, 100)]
	public int		_lockPlayerInputFor = 5;
	public S_GeneralEnums.LockControlDirection _LockInputTo_;
	public Vector3          _lockToDirectionIfChange;

	[Header("Actions")]
	public S_S_ActionHandling.PrimaryPlayerStates[]		_ActionsToDisable;
	public S_S_ActionHandling.SubPlayerStates[]		_SubActionsToDisable;

	[Header("Effects")]
	[Tooltip("In case the player needs to be in a specific state. Mainly used to call on Grounded events while still in the air. E.G. Returning jump dash in scripted sections.")]
	public S_GeneralEnums.ChangeGroundedState      _setPlayerGrounded;


#if UNITY_EDITOR
	public override void DrawAdditionalGizmos (bool selected, Color colour ) {
		if (_hasTrigger)
			S_S_DrawingMethods.DrawArrowHandle(colour, transform, 0.2f, true);
	}
#endif
}
