using UnityEngine;
using System.Collections;
using SplineMesh;

[ExecuteInEditMode]
[AddComponentMenu("Data Components/Dash Ring")]
public class S_Data_DashRing : S_Data_Base
{

	[Header("Force")]
	public LaunchPlayerData _launchData_ = new LaunchPlayerData();
	[Tooltip ("If true, then when launching the player, will use the players speed if its higher than the launch force")]
	public bool         _willCarrySpeed_ = true;

	[Header("Effects")]
	public S_Structs.ObjectCameraEffect _cameraEffect = new S_Structs.ObjectCameraEffect
	{
		_willAffectCamera_ = true,
		_CameraRotateTime_ = new Vector2 (0.15f, 20f),
	};

	[Header("Lock?")]
	public Vector3	_PositionToLockTo;
	public int	_lockControlFrames_ = 30;
	public S_GeneralEnums.LockControlDirection _lockInputTo_;

	public Vector3      _overwriteGravity_;
	public float	_lockAirMovesFor_ = 30f;

	[ExecuteInEditMode]
	private void Update () {
		if(!Application.isPlaying){UpdateLaunchDataToDirection();}
	}

	void UpdateLaunchDataToDirection () {
		_launchData_ = LaunchPlayerData.SetLaunchDataToDirection(transform, _launchData_);
	}
}

