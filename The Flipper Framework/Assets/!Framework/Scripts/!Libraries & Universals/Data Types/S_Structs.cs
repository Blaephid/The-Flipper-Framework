using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class LaunchPlayerData {
	[Header ("Physics")]
	[Tooltip("The magnitude of the environmental velocity added to player.")]
	public float	_force_;
	[Tooltip("The direction for the player to be launched in. Affected By Transform")]
	public Vector3 _direction_;
	[HideInInspector]
	public Vector3 _directionToUse_;

	[Tooltip("Since characters can have different gravities. If this is not zero, the player gravity will be this until they hit the ground.")]
	public Vector3 _overwriteGravity_;
	public bool _useCore;

	[Header("Effects")]
	[Tooltip("How many frames until the player regains control.")]
	[DrawHorizontalWithOthers(new string[]{"_lockAirMovesFrames_"})]
	public int          _lockInputFrames_;
	[Tooltip("How amy frames until the player can perform aerial actions like jumps and jump dashes."), HideInInspector]
	public int	 _lockAirMovesFrames_;
	[Tooltip("What the player's input will be during the frames their control is locked.")]
	public S_GeneralEnums.LockControlDirection _lockInputTo_;

	public static LaunchPlayerData SetLaunchDataToDirection ( Transform transformForRotation, LaunchPlayerData _launchData_ ) {

		return new LaunchPlayerData()
		{
			_force_ = _launchData_._force_,
			_direction_ = _launchData_._direction_,
			_directionToUse_ = (transformForRotation.rotation * _launchData_._direction_),
			_lockInputFrames_ = _launchData_._lockInputFrames_,
			_lockAirMovesFrames_ = _launchData_._lockAirMovesFrames_,
			_overwriteGravity_ = _launchData_._overwriteGravity_,
			_lockInputTo_ = _launchData_._lockInputTo_,
			_useCore = _launchData_._useCore,
		};

	}
}


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

