using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "New BaseGame", menuName = "SonicGT/General/Base Game")]
public class BaseGameVariables : ScriptableObject {

    [Header("Gravity")]
    public Vector3 Gravity;
    public float DownGravMult = 1.8f;

    [Header("Lives Sytem")]
    public bool UseLivesSytem;
    public int StartingLives = 3;
    public AudioClip ExraLifeSound;
    
    [Header ("Ground Raycaster")]
    public float RotationResetThreshold = -0.1f;
    public float RayToGroundDistance = 0.55f;
    public float RaytoGroundSpeedRatio = 0.01f;
    public float RaytoGroundSpeedMax = 2.2f;
    public float RayToGroundRotDistance = 1.1f;
    public float RaytoGroundRotSpeedMax = 1.8f;
    [Space]
    public Vector2 StickingLerps = new Vector2(1f,1f);
    public float StickingNormalLimit = 1;
    public float StickCastAhead = 2;
    public float sticknegativeGHoverHeight = 0.6115f;

    [Space]
    public LayerMask LayerMask,HomingAtkMask, wallJumpMask;

    [Header ("Player Damage")]
    public float InvencibilityTime = 3.0f;
    public float FlickerSpeed = 3;
    [Space]
    public float KnockbackUpwardsForce = 30;
    public bool ResetSpeedOnHit = false;
    public float KnockbackForce = 30;
    public bool ModularDamageSpeedHit = true;
    public float MaxSpeedToPushBackwards = 100f;
    public float MaxSpeedToPushSideways = 150f;
    [Space]
    public S_MovingRing MovingRing;
    public int MaxRingLoss = 30;
    public float RingReleaseSpeed = 700;
    public float RingArcSpeed = 30;
    [Space]
    public AudioClip SuperWarningLow;
    public AudioClip SuperWarningHigh;
    [Space]
    [Header("Death")]
    public float DeathDelayRespawn = 1; 
    public float DeathFadeinTime = 0.7f, DeathFadeoutTime = 0.2f;



    [Space]
    [Header("Water")]
    public Vector3 UnderWaterGravity = new Vector3(0, -1.2f, 0);
    public float UnderwaterDownGravMult = 0.8f;
    public float UnderwaterEntryVelocityMult = 0.333f;
    public float UnderwaterExitVelocityMult = 1.2f;
    public float MinimulSplashSpeed = 10;
    public LayerMask WaterMask;
    public AudioClip UnderwaterBell;
    public AudioClip UnderwaterPanicMusic;
    

    [Header("Invencibility Monitor")]

    public float MonitorInvencibilityTimeout = 10;
    public AudioClip InvencibilityMusic;

    [Header("Speed up Monitor")]

    public float MonitorSpeedUpTimeout = 10;    
    public float MonitorSpeedUpSpeedIncrease = 70;
    public float MonitorSpeedUpAcellIncrease = 0.2f;
    public AudioClip SpeedUpMusic;

    [Header("Energy Shield")]
    public AudioClip ThuderShieldSFX;
    public float ThunderShieldRange = 20;
    public float ThunderShieldPullSpeed = 230;
    public float ThunderShieldSpeedMultiplier = 0.7f;
    public float ThunderShieldJumpSpeed = 60;

    [Header ("Flame Shield")]
    public AudioClip FlameShieldSFX;
    public float FlameShieldTimeout = 0.1667f;
    public float FlameShieldSpeed = 150;


}
