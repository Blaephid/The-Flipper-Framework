using UnityEngine;
using System.Collections;
using SplineMesh;

[AddComponentMenu("Data Components/Rail Booster")]
public class S_Data_RailBooster : S_Data_Base
{
	[Header ("Position")]
	public bool	_willSetBackwards_;
	public Vector3    _PositionToLockTo;

	[Header("Speed gained and lost")]
	[Tooltip("X = players speed on rail before booster, y = the new speed they will gain and stay at (though it can still be affected by slopes and other sources.)")]
	public AnimationCurve _NewSpeedByCurrentSpeed = new AnimationCurve (new Keyframe[] {
		new Keyframe (0f,50f),
		new Keyframe (50f,90f),
		new Keyframe (100f,120f),
		new Keyframe (150,160f),
		new Keyframe (160,160) });

	[Tooltip("X = players speed on rail before booster, Y = the temporary speed gained. This can go over the max grind speed, but decays away shortly.")]
	public AnimationCurve _BoostAddOnByCurrentSpeed = new AnimationCurve (new Keyframe[] {
		new Keyframe (0f,15f),
		new Keyframe (160f,20f) });

	public float _timeBeforeDecay = 0.7f;

	[Header("Effects")]
	public S_Structs.ObjectCameraEffect _cameraEffect = new S_Structs.ObjectCameraEffect
	{
		_willAffectCamera_ = true,
		_CameraRotateTime_ = new Vector2 (0.15f, 20f),
	};

}

