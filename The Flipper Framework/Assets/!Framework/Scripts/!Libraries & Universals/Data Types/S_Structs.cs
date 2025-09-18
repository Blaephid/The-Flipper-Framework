using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[Serializable]
public class LaunchPlayerData {
	[Tooltip("Wont calculate launch until this many frames have passed, useful for specific scripted moments.")]
	public int _frameDelay;

	[Header ("Physics")]
	[Tooltip("The magnitude of the environmental velocity added to player.")]
	public float	_force_;
	[Tooltip("The direction for the player to be launched in. Affected By Transform")]
	public Vector3 _direction_;
	[CustomReadOnly]
	public Vector3 _directionToUse_;
	[Tooltip("If null, uses gameObject, if set to a transform then the player launches from this position, snapping to it beforehand.")]
	public Transform _shotOrigin;

	[Header("Overwrite Values")]
	[Tooltip("Since characters can have different gravities. If this is not zero, the player gravity will be this until they hit the ground.")]
	public Vector3 _overwriteGravity_;
	[Tooltip("How much of the launch should be applied as core velocity (which can be controlled or accelerated). Core velocity will always be overwritten if greater than launch force."), Range(0,1)]
	public float _coreVelocityImportance;
	[Tooltip("As some launchers are close to the ground, but must be aerial, this prevents the player being grounded until after launched a bit. Prevents adjusting velocity to ground.")]
	public int _delayGroundedFor = 8;

	[Header("Effects")]
	[Tooltip("How many frames until the player regains control.")]
	[DrawHorizontalWithOthers(new string[]{"_lockAirMovesFrames_"})]
	public int          _lockInputFrames_;
	[Tooltip("How amy frames until the player can perform aerial actions like jumps and jump dashes."), HideInInspector]
	public int	 _lockAirMovesFrames_;
	[Tooltip("What the player's input will be during the frames their control is locked.")]
	public S_GeneralEnums.LockControlDirection _lockInputTo_;

	//Due to the world being calculated each frame in editor, ensure very value above is included here as well.
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
			_coreVelocityImportance = _launchData_._coreVelocityImportance,
			_frameDelay = _launchData_._frameDelay,
			_delayGroundedFor = _launchData_._delayGroundedFor,
			_shotOrigin = _launchData_._shotOrigin,
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

[Serializable]
public struct StrucTriggerAnimation
{
	public Animation AnimClip;
	public Animator Animator;
	public string trigger;
}

