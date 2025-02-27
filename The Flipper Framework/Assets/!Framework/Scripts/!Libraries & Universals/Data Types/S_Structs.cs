using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class S_Structs
{
	///
	//ACTIONS
	///
	[Serializable]
	public struct MainActionTracker {
		public S_S_ActionHandling.PrimaryPlayerStates State;
		public IMainAction Action;
		public List<S_S_ActionHandling.PlayerControlledStates> ConnectedStates;
		public List<IMainAction> ConnectedActions;
		public List<S_S_ActionHandling.PlayerSituationalStates> SituationalStates;
		public List<IMainAction> SituationalActions;
		public List<S_S_ActionHandling.SubPlayerStates> PerformableSubStates;
		public List<ISubAction> SubActions;
	}


	///
	//Physics
	///
	[Serializable]
	public struct LaunchPlayerData
	{
		[Tooltip("The magnitude of the environmental velocity added to player.")]
		public float	_force_;
		[Tooltip("The direction for the player to be launched in. Affected By Transform")]
		public Vector3  _direction_;
		[HideInInspector]
		public Vector3	_directionToUse_;
		[Tooltip("How many frames until the player regains control.")]
		public int          _lockInputFrames_;
		[Tooltip("How amy frames until the player can perform aerial actions like jumps and jump dashes.")]
		public int	 _lockAirMovesFrames_;
		[Tooltip("Since characters can have different gravities. If this is not zero, the player gravity will be this until they hit the ground.")]
		public Vector3      _overwriteGravity_;
		[Tooltip("What the player's input will be during the frames their control is locked.")]
		public S_GeneralEnums.LockControlDirection _lockInputTo_;
		
	}


	///
	//For Objects
	///

	[Serializable]
	public struct ObjectCameraEffect
	{
		public bool         _willAffectCamera_;
		public Vector2      _CameraRotateTime_;
	}


	///
	//Scripting
	///
}

