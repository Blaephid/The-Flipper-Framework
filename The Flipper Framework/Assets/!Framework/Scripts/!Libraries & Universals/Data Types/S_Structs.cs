using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Structs
{
	[Serializable]
	public struct StrucMainActionTracker {
		public S_GeneralEnums.PrimaryPlayerStates State;
		public IMainAction Action;
		public List<S_GeneralEnums.PlayerControlledStates> ConnectedStates;
		public List<IMainAction> ConnectedActions;
		public List<S_GeneralEnums.PlayerSituationalStates> SituationalStates;
		public List<IMainAction> SituationalActions;
		public List<S_GeneralEnums.SubPlayerStates> PerformableSubStates;
		public List<ISubAction> SubActions;
	}
}
