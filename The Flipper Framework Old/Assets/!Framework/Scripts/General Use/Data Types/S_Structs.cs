using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Structs
{
	[Serializable]
	public struct StrucMainActionTracker {
		public S_Enums.PrimaryPlayerStates State;
		public IMainAction Action;
		public List<S_Enums.PlayerControlledStates> ConnectedStates;
		public List<IMainAction> ConnectedActions;
		public List<S_Enums.PlayerSituationalStates> SituationalStates;
		public List<IMainAction> SituationalActions;
		public List<S_Enums.SubPlayerStates> PerformableSubStates;
		public List<ISubAction> SubActions;
	}
}
