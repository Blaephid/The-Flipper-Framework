using UnityEngine;
using System.Collections;
using SplineMesh;

[ExecuteInEditMode]
public class S_Data_DashRing : S_Data_Base
{

	[Header("Force")]
	public S_Structs.LaunchPlayerData _launchData_;
	[Tooltip ("If true, then when launching the player, will use the players speed if its higher than the launch force")]
	public bool         _willCarrySpeed_ = true;

	[Header("Effects")]
	public float	_speedToSet_ = 100;

	public S_Structs.ObjectCameraEffect _cameraEffect = new S_Structs.ObjectCameraEffect
	{
		_willAffectCamera_ = true,
		_CameraRotateTime_ = new Vector2 (0.15f, 20f),
	};

	[Header("Lock?")]
	public Transform	_PositionToLockTo;
	public int	_lockControlFrames_ = 30;
	public S_GeneralEnums.LockControlDirection _lockInputTo_;

	public Vector3      _overwriteGravity_;
	public float	_lockAirMovesFor_ = 30f;

	private new void OnValidate () {
		base.OnValidate();

		_launchData_ = new S_Structs.LaunchPlayerData()
		{
			_force_ = _launchData_._force_,
			_direction_ = transform.forward,
			_lockInputFrames_ = _launchData_._lockInputFrames_,
			_lockAirMovesFrames_ = _launchData_._lockAirMovesFrames_,
			_overwriteGravity_ = _launchData_._overwriteGravity_,
			_lockInputTo_ = _launchData_._lockInputTo_
		};
	}
}

