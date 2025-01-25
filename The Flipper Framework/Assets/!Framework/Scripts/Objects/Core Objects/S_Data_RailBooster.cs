using UnityEngine;
using System.Collections;
using SplineMesh;

public class S_Data_RailBooster : S_Data_Base
{
	[Header ("On Rail")]
	public bool	_willSetBackwards_;
	public bool	_willSetSpeed_ = true;
	public float	_addSpeed_ = 15;
	public float	_speedToSet_ = 100;

	public Transform    _PositionToLockTo;

	[Header("Effects")]
	public S_Structs.ObjectCameraEffect _cameraEffect = new S_Structs.ObjectCameraEffect
	{
		_willAffectCamera_ = true,
		_CameraRotateTime_ = new Vector2 (0.15f, 20f),
	};

}

