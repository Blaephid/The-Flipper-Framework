using UnityEngine;
using System.Collections;
using SplineMesh;

public class S_Data_SpeedPad : MonoBehaviour, IObjectData
{
	[Header ("Type")]
	public bool	_isOnRail_;
	public bool	_isDashRing_;
	public Spline       _Path;

	[Header ("On Rail")]
	public bool	_willSetBackwards_;
	public bool	_willSetSpeed_ = true;
	public float	_addSpeed_ = 15;


	[Header("Effects")]
	public float	_speedToSet_ = 100;
	public bool         _willCarrySpeed_ = true;

	public bool	_willSnap;
	public bool	_willAffectCamera_ = true;
	public Vector2      _CameraRotateTime_ = new Vector2 (0.15f, 20f);

	[Header("Lock?")]
	public Transform	_PositionToLockTo;
	public bool	_willLockControl;
	public int	_lockControlFrames_ = 30;
	public S_GeneralEnums.LockControlDirection _lockInputTo_;

	public Vector3      _overwriteGravity_;
	public float	_lockAirMovesFor_ = 30f;
}

