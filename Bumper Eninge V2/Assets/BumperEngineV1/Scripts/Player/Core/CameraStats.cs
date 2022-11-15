using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Camera X Stats")]
public class CameraStats : ScriptableObject
{
    public bool UseAutoRotation = true;
    public bool UseCurve = true;
    public float AutoXRotationSpeed = 1;
    public AnimationCurve AutoXRotationCurve;
    public bool LockHeight = true;
    public float LockHeightSpeed = 0.8f;
    public bool MoveHeightBasedOnSpeed = true;
    public float HeightToLock = 15;
    public float HeightFollowSpeed = 6;
    public float FallSpeedThreshold = -80;

    public float CameraMaxDistance = -13;
    public float AngleThreshold = 0.3f;

    public LayerMask CollidableLayers;

    public float CameraRotationSpeed = 100;
    public float CameraVerticalRotationSpeed = 12;
    public float CameraMoveSpeed = 100;

    public float InputXSpeed = 70;
    public float InputYSpeed = 55f;
    public float InputSensi = 1f;
    public float InputMouseSensi = 0.06f;
    public float stationaryCamIncrease = 1.3f;

    public float yMinLimit = -100f;
    public float yMaxLimit = 100f;

    public float LockCamAtHighSpeed = 45;

    public float MoveLerpingSpeed = 4;
    public float RotationLerpingSpeed = 8;

    public float LockedRotationSpeed = 6;
    public float ShakeDampen = 4;
}

