using UnityEngine;
using System.Collections;
using SplineMesh;
using System;

public class S_Data_SpeedPad : S_Data_Base
{

	[Header("Effects")]
	[Tooltip("If not null, this pad won't apply speed, but should have the tag to act as a trigger for the action moveAlongPathAction (see pathers interaction)")]
	public Spline       _Path;
	public float	_speedToSet_ = 100;
	public bool         _willCarrySpeed_ = true;

	public bool         _willSnap;

	public S_Structs.ObjectCameraEffect _cameraEffect = new S_Structs.ObjectCameraEffect
	{
		_willAffectCamera_ = true,
		_CameraRotateTime_ = new Vector2 (0.15f, 20f),
	};

	[Header("Lock?")]
	public Transform	_PositionToLockTo;
	public int	_lockControlFrames_ = 30;
	public S_GeneralEnums.LockControlDirection _lockInputTo_;
}

