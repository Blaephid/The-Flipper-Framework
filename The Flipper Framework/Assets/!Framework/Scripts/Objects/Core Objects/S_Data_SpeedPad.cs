using UnityEngine;
using System.Collections;
using SplineMesh;

public class S_Data_SpeedPad : MonoBehaviour
{
	[Header ("Type")]
	public bool	_isOnRail_;
	public bool	_isDashRing_;

	[Header ("On Rail")]
	public bool	_willSetBackwards_;
	public Spline	_Path;
	public bool	_willSetSpeed_ = true;
	public float	_addSpeed_ = 15;

	[Header("Off Rail")]

	[Header("Effects")]
	public float	_speedToSet_;
	public bool	_lockToDirection_;

	public bool	_willSnap;
	public bool	_willAffectCamera_ = true;
	public bool	_willResetRotation_ = true;

	[Header("Lock?")]
	public Transform	_PositionToLockTo;
	public bool	_willLockControl;
	public float	_lockControlFor_ = 0.5f;
	public S_Enums.LockControlDirection _lockInputTo_;

	public Vector3      _overwriteGravity_;
	public bool	_willLockAirMoves_ = false;
	public float	_lockAirMovesFor_ = 30f;

}

