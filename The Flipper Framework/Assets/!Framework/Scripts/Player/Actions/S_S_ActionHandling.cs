using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static S_S_ActionHandling;
using static UnityEngine.Rendering.DebugUI;



public class S_S_ActionHandling : MonoBehaviour
{
	public static readonly Dictionary<PrimaryPlayerStates, Type>  ActionsDictionaryAll = new Dictionary<PrimaryPlayerStates, Type>() 
	{
		{PrimaryPlayerStates.Default, typeof(S_Action00_Default)},
		{PrimaryPlayerStates.Jump, typeof(S_Action01_Jump)},
		{PrimaryPlayerStates.Homing, typeof(S_Action02_Homing)},
		{PrimaryPlayerStates.SpinCharge, typeof(S_Action03_SpinCharge)},
		{PrimaryPlayerStates.Hurt, typeof(S_Action04_Hurt)},
		{PrimaryPlayerStates.Rail, typeof(S_Action05_Rail)},
		{PrimaryPlayerStates.Bounce, typeof(S_Action06_Bounce)},
		{PrimaryPlayerStates.RingRoad, typeof(S_Action07_RingRoad)},
		{PrimaryPlayerStates.DropCharge, typeof(S_Action08_DropCharge)},
		{PrimaryPlayerStates.Path, typeof(S_Action10_FollowAutoPath)},
		{PrimaryPlayerStates.JumpDash, typeof(S_Action11_JumpDash)},
		{PrimaryPlayerStates.WallRunning, typeof(S_Action12_WallRunning)},
		{PrimaryPlayerStates.WallClimbing, typeof(S_Action15_WallClimbing)},
		{PrimaryPlayerStates.Upreel, typeof(S_Action14_Upreel)},
		{PrimaryPlayerStates.Hovering, typeof(S_Action13_Hovering)},
	};

	public static readonly Dictionary<PlayerControlledStates, Type>  ActionsDictionaryCon = new Dictionary<PlayerControlledStates, Type>()
	{
		 {PlayerControlledStates.Jump, typeof(S_Action01_Jump)},
		 {PlayerControlledStates.Homing, typeof(S_Action02_Homing)},
		{PlayerControlledStates.SpinCharge, typeof(S_Action03_SpinCharge)},
		{PlayerControlledStates.Bounce, typeof(S_Action06_Bounce)},
		{PlayerControlledStates.DropCharge, typeof(S_Action08_DropCharge)},
		{PlayerControlledStates.JumpDash, typeof(S_Action11_JumpDash)},
	};

	public static readonly Dictionary<PlayerSituationalStates, Type>  ActionsDictionarySit = new Dictionary<PlayerSituationalStates, Type>()
	{
		 {PlayerSituationalStates.Default, typeof(S_Action00_Default)},
		{PlayerSituationalStates.Hurt, typeof(S_Action04_Hurt)},
		{PlayerSituationalStates.Rail, typeof(S_Action05_Rail)},
		{PlayerSituationalStates.RingRoad, typeof(S_Action07_RingRoad)},
		{PlayerSituationalStates.Path, typeof(S_Action10_FollowAutoPath)},
		{PlayerSituationalStates.WallRunning, typeof(S_Action12_WallRunning)},
		{PlayerSituationalStates.WallClimbing, typeof(S_Action15_WallClimbing)},
		{PlayerSituationalStates.Upreel, typeof(S_Action14_Upreel)},
		{PlayerSituationalStates.Hovering, typeof(S_Action13_Hovering)},
	};

	public static readonly Dictionary<SubPlayerStates, Type>  ActionsDictionarySub = new Dictionary<SubPlayerStates, Type>()
	{
		{SubPlayerStates.Rolling, typeof(S_SubAction_Roll)},
		{SubPlayerStates.Boost, typeof(S_SubAction_Boost)},
		{SubPlayerStates.Skidding, typeof(S_SubAction_Skid)},
		{SubPlayerStates.Quickstepping, typeof(S_SubAction_Quickstep)},
	};



	#region AsigningEnums
	//Actions.
	//How they're ordered here is also how they're ordered in priority, so the action manager will list then in this order, the higher, the sooner it will be checked (so if boost is above quickstep, it will be called before).
	public enum PrimaryPlayerStates
	{
		None = -1,
		Default = 0,
		Jump = 2,
		Homing = 3,
		SpinCharge = 4,
		Hurt = 1,
		Rail = 5,
		Bounce = 6,
		RingRoad = 7,
		DropCharge = 8,
		Path = 9,
		JumpDash = 10,
		WallRunning = 11,
		WallClimbing = 14,
		Hovering = 12,
		Upreel = 13,
	}

	public enum PlayerControlledStates
	{
		Jump,
		Homing,
		SpinCharge,
		Bounce,
		DropCharge,
		JumpDash,
		None
	}

	public enum PlayerSituationalStates
	{
		Default,
		Hurt,
		Rail,
		RingRoad,
		Path,
		WallRunning,
		WallClimbing,
		Hovering,
		Upreel,
		None
	}

	public enum SubPlayerStates
	{
		Skidding = 0,
		Quickstepping = 4,
		Rolling = 3,
		Boost = 1
	}

	#endregion

	#region Translating Components and Enums

	public static IMainAction GetActionFromEnum ( PrimaryPlayerStates state, GameObject ObjectForActions ) {

		Type ActionClassAsType;

		//Test if that enum and/or class are currently defined in the Dictionaries at the top of this script.
		if (!ActionsDictionaryAll.TryGetValue(state, out ActionClassAsType)) { Debug.LogError("That class is not currently assigned to the ActionsDictionary"); return null; }

		//Search for an action class that matches the given enum according to the dictionary
		if (ObjectForActions.GetComponent(ActionClassAsType)) { return (IMainAction)ObjectForActions.GetComponent(ActionClassAsType); }
		return null;
	}

	public static ISubAction GetSubActionFromEnum ( SubPlayerStates state, GameObject ObjectForSubActions ) {

		Type ActionClassAsType;

		//Test if that enum and/or class are currently defined in the Dictionaries at the top of this script.
		if (!ActionsDictionarySub.TryGetValue(state, out ActionClassAsType)) { Debug.LogError("That class is not currently assigned to the ActionsDictionary"); return null; }

		//Search for an action class that matches the given enum according to the dictionary
		if (ObjectForSubActions.GetComponent(ActionClassAsType)) { return (ISubAction)ObjectForSubActions.GetComponent(ActionClassAsType); }
		return null;
	}

	//Each enum of playerstate corresponds to a different script that handles its behaviour. This assigns the matching script.
	public static IMainAction GetControlledActionFromEnum ( PlayerControlledStates state, GameObject ObjectForActions ) {

		Type ActionClassAsType;

		//Test if that enum and/or class are currently defined in the Dictionaries at the top of this script.
		if (!ActionsDictionaryCon.TryGetValue(state, out ActionClassAsType)) { Debug.LogError("That class is not currently assigned to the ActionsDictionary"); return null; }

		//Search for an action class that matches the given enum according to the dictionary
		if (ObjectForActions.GetComponent(ActionClassAsType)) { return (IMainAction)ObjectForActions.GetComponent(ActionClassAsType); }
		return null;
	}
	public static IMainAction GetSituationalActionFromEnum ( PlayerSituationalStates state, GameObject ObjectForActions ) {
		Type ActionClassAsType;

		//Test if that enum and/or class are currently defined in the Dictionaries at the top of this script.
		if (!ActionsDictionarySit.TryGetValue(state, out ActionClassAsType)) { Debug.LogError("That class is not currently assigned to the ActionsDictionary"); return null; }

		//Search for an action class that matches the given enum according to the dictionary
		if (ObjectForActions.GetComponent(ActionClassAsType)) { return (IMainAction)ObjectForActions.GetComponent(ActionClassAsType); }
		return null;

	}

	public static IMainAction AddOrFindMainActionComponent ( PrimaryPlayerStates state, GameObject ObjectForActions ) {

		Type ActionClassAsType;

		//Test if that enum and/or class are currently defined in the Dictionaries at the top of this script.
		if (!ActionsDictionaryAll.TryGetValue(state, out ActionClassAsType)) { Debug.LogError("That class is not currently assigned to the ActionsDictionary"); return null; }

		//Search for an action class that matches the given enum according to the dictionary. Either returns it, or adds then returns.
		if(ObjectForActions.GetComponent(ActionClassAsType)) {return (IMainAction) ObjectForActions.GetComponent(ActionClassAsType);}
		else { return (IMainAction)ObjectForActions.AddComponent(ActionClassAsType); }
	}

	//
	public static ISubAction AddOrFindSubActionComponent ( SubPlayerStates state, GameObject ObjectForSubActions ) {

		Type ActionClassAsType;

		//Test if that enum and/or class are currently defined in the Dictionaries at the top of this script.
		if (!ActionsDictionarySub.TryGetValue(state, out ActionClassAsType)) { Debug.LogError("That class is not currently assigned to the ActionsDictionary"); return null; }

		//Search for an action class that matches the given enum according to the dictionary. Either returns it, or adds then returns.
		if (ObjectForSubActions.GetComponent(ActionClassAsType)) { return (ISubAction)ObjectForSubActions.GetComponent(ActionClassAsType); }
		else { return (ISubAction)ObjectForSubActions.AddComponent(ActionClassAsType); }
	}

	//Iterates through the dictionary of all actions to find the enum that acts as a key for that action. Much slower than getting the action from the key.
	public static PrimaryPlayerStates GetEnumFromActionClass (IAction InterfaceReference) {

		foreach (var pair in ActionsDictionaryAll)
		{
			if (pair.Value.Equals(InterfaceReference as Type))
			{
				return  pair.Key; // Return the key if the value matches
			}
		}

		return PrimaryPlayerStates.None;
	}

	#endregion
}
