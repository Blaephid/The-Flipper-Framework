using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Camera", menuName = "SonicGT/General/CameraStats")]
public class GeneralCameraStats : ScriptableObject {

    [Header("AutoRotation")]
    public float LockCamAtHighSpeed = 130;
    public bool UseCurve = true;
    public float AutoXRotationSpeed = 1;
    public AnimationCurve AutoXRotationCurve;
    public float LockHeightSpeed = 8;
    public bool MoveHeightBasedOnSpeed = true;
    public float HeightToLock = 10;
    public float HeightFollowSpeed = 6;
    public float FallSpeedThreshold = -50;
    public float LockedRotationSpeed = 3;

    [Header("Rotation Camera")]
    public float SensitivityRatio = 800f;
    [Space]
    public float RotationLerpingSpeed = 1;
    public float CameraRotationSpeed = 1080;
    public float CameraVerticalRotationSpeed = 6.5f;

    [Header("Camera Movement")]
    public float MoveLerpingSpeed = 1;
    public float CameraMoveTime = 0.03f;

    [Header("Camera Vertical Boundaries")]
    public float yMinLimit = -40f;
    public float yMaxLimit = 65f;

    [Header("Camera Distances")]
    public float CameraDistaceBottom = -6f;
    public float CameraDistanceTop = -20;

    [Header("Camera Shake")]
    public float ShakeDampen = 10;

    public LayerMask CollidableLayers;

}
