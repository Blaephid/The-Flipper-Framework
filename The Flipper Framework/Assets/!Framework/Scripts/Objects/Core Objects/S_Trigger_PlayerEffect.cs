using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Trigger_PlayerEffect : S_Trigger_Base
{

	[Header("Input")]
	public int          _lockPlayerInputFor = 5;
	public S_GeneralEnums.LockControlDirection _LockInputTo_;

	[Header("Effects")]
	[Tooltip("In case the player needs to be in a specific state. Mainly used to call on Grounded events while still in the air. E.G. Returning jump dash in scripted sections.")]
	public S_GeneralEnums.ChangeGroundedState      _setPlayerGrounded;
}
