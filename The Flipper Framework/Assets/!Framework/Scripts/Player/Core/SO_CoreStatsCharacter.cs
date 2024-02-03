using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character X Stats")]
public class SO_CoreStatsCharacter : ScriptableObject
{
    public string Title;

    [Header("Core movement")]

    [Header("Acceleration Values")]

    public AnimationCurve AccellOverSpeed;
    public float AccellShiftOverSpeed = 1;

    [Header("Turning values")]
    public float TangentialDragShiftSpeed = 1;
    [Tooltip("This decides how fast the turn will be based on the angle of input.")]
    public AnimationCurve TurnRateOverAngle;
    [HideInInspector]public AnimationCurve TurnRateOverAngleSlowed;
    public AnimationCurve TurnRateOverSpeed;
    public AnimationCurve TangDragOverAngle;
    public AnimationCurve TangDragOverSpeed;

    [Header("Deceleration Values")]
    public AnimationCurve DecellBySpeed;
    public float DecellShiftOverSpeed = 1;
    public float naturalAirDecell = 1.002f;

    [Header ("Stick to ground")]
    public float GroundStickingDistance = 0.15f;
    public float GroundStickingPower = -1.45f;


    [Header ("Special Turning - PlayerBInput")]
    public AnimationCurve InputLerpingRateOverSpeed;
    public bool UtopiaTurning = true;
    public AnimationCurve UtopiaInputLerpingRateOverSpeed;
    public float UtopiaIntensity = 180;
    public float UtopiaInitialInputLerpSpeed = 10;

    [Header("Slope effects")]
    public float SlopeEffectLimit = 0.9f;
    public float StandOnSlopeLimit = 0.8f;
    public float SlopePower = -1.5f;
    public float SlopeRunningAngleLimit = 0.5f;
    public AnimationCurve SlopeSpeedLimit;

    [Tooltip("This is multiplied with the force of a slope when going uphill to determine the force against.")]
    public float UphillMultiplier = 0.55f;
    [Tooltip("This is multiplied with the force of a slope when going downhill to determine the force for.")]
    public float DownhillMultiplier = 0.5f;
    public float StartDownhillMultiplier = -1.8f; 
    [Tooltip("This determines how much force is gained from the slope depending on the current speed. ")]
    public AnimationCurve SlopePowerOverSpeed;
    public AnimationCurve UpHillOverTime;

    [Header("Greedy Stick to Ground")]
    [Tooltip("This is the values of the Lerps when the player encounters a slope , the first one is negative slopes (loops), and the second one is positive Slopes (imagine running on the outside of a loop),This values shouldnt be touched unless yuou want to go absurdly faster. Default values 0.885 and 1.5")]
    public Vector2 StickingLerps = new Vector2(0.885f, 1.5f);
    [Tooltip("This is the limit from 0 to 1 the degrees that the player should be sticking 0 is no angle , 1 is everything bellow 90°, and 0.5 is 45° angles, default 0.4")]
    public float StickingNormalLimit = 0.3f;
    [Tooltip("This is the cast ahead when the player hits a slope, this will be used to predict it's path if it is going on a high speed. too much of this value might send the player flying off before it hits the loop, too little might see micro stutters, default value 1.9")]
    public float StickCastAhead = 1.6f;
    [Tooltip("This is the position above the raycast hit point that the player will be placed if he is loosing grip on positive G turns, this value will snap the player back into the mesh, it shouldnt be moved unless you scale the collider, default value 0.6115")]
    public float negativeGHoverHeight = 0.6115f;
    public float RayToGroundDistance = 0.9f;
    public float RaytoGroundSpeedRatio = 0.01f;
    public float RaytoGroundSpeedMax = 2.4f;
    public float RayToGroundRotDistance = 1.1f;
    public float RaytoGroundRotSpeedMax = 2.6f;
    public float RotationResetThreshold = -0.1f;

    public LayerMask Playermask;

    [Header("AirMovementExtras")]
    public bool StopAirMovementIfNoInput = true;
    public Vector3 UpGravity = new Vector3(0f, -1.7f, 0);
    public float keepNormalForThis = 0.083f;


    [Header("Rolling Values")]

    public float RollingLandingBoost = 1.4f;
    public float RollingDownhillBoost = 1.9f;
    public float RollingUphillBoost = 1.2f;
    public float RollingStartSpeed = 30;
    public float RollingTurningDecreace = 0.6f;
    public float RollingFlatDecell = 1.004f;
    public float SlopeTakeoverAmount = 0.995f; // This is the normalized slope angle that the player has to be in order to register the land as "flat"

    [Header("Pull Items")]
    public AnimationCurve radiusBySpeed;
    public LayerMask ringMask;
    public float basePullSpeed;

    [Header("Jump")]
    public AnimationCurve CoyoteTimeOverSpeed;
    public float JumpSlopeConversion = 0.03f;
    public float StopYSpeedOnRelease = 2;
    public float JumpRollingLandingBoost;

    [Header("QuickStep")]
    public LayerMask StepLayerMask;

    [Header ("Homing Search")]
    public float TargetSearchDistance = 30;
    public float faceRange = 66;
    public LayerMask TargetLayer;
    public LayerMask BlockingLayers;
    public float FieldOfView = 0.2f;
    public float IconScale = 1.5f;
    public float IconDistanceScaling = 0.2f;
    public float FacingAmount = 0.6f;

    [Header ("Homing")]

    public bool CanDashDuringFall = true;

    [Header ("Jump Dash")]
    public bool isAdditive = true;

    [Header("Spin Charge")]
    public float MaximumSpeed = 25; //The max amount of speed you can be at to perform a Spin Dash
    public float MaximumSlope = 0.9f; //The highest slope you can be on to Spin Dash
    public float ReleaseShakeAmmount;
    public AnimationCurve SpeedLossByTime;
    public AnimationCurve ForceGainByAngle;
    public AnimationCurve gainBySpeed;
    public float spinSkidStartPoint;
    public float spinSkidIntesity;


    [Header("Bounce")]
    public float BounceUpMaxSpeed = 75;
    public float BounceConsecutiveFactor = 1.05f;

    [Header("Ring Road Search")]
    public float RingTargetSearchDistance = 60;
    public float RingRoadIconScale = 0;
    public LayerMask RingRoadLayer;


    [Header("Interact with Enemies")]

    public float BouncingPower = 40;
    public float HomingBouncingPower = 38;
    public float EnemyHomingStoppingPowerWhenAdditive = 40;
    public bool StopOnHomingAttackHit = true;
    public bool StopOnHit = true;
    public float EnemyDamageShakeAmmount = 0.5f;
    public float EnemyHitShakeAmmount = 1.2f;


    [Header("Bonk")]
    public LayerMask BonkOnWalls;
    public float BonkUpwardsForce = 20;
    public float BonkBackwardsForce = 15f;
    public float BonkControlLock = 10f;
    public float BonkControlLockAir = 40f;

    [Header("Hurt")]
    public float KnockbackUpwardsForce = 30;
    public bool ResetSpeedOnHit = false;
    public LayerMask RecoilFrom;
    public float KnockbackForce = 40;
    public float RingReleaseSpeed = 550;
    public float RingArcSpeed = 20;
    public float FlickerSpeed = 3f;
    public float HurtControlLock = 10f;
    public float HurtControlLockAir = 40f;


    [Header("Rails")]
    public float railMaxSpeed = 125f;
    public float railTopSpeed = 80;
    public float railDecaySpeedHigh = 0.4f;
    public float railDecaySpeedLow = 0.2f;
    public float MinStartSpeed = 20f;
    public float RailPushFowardmaxSpeed = 100f;
    public float RailPushFowardIncrements = 5f;
    public float RailPushFowardDelay = 0.8f;
    public float RailSlopePower = 2.5f;
    public float RailUpHillMultiplier = 0.5f;
    public float RailDownHillMultiplier = 0.5f;
    public float RailUpHillMultiplierCrouching = 0.9f;
    public float RailDownHillMultiplierCrouching = 0.8f;
    public float RailDragVal = 0.0001f;
    public float RailPlayerBrakePower = 0.7f;
    public float hopDelay = 0.5f;
    public float hopSpeed = 3f;
    public float hopDistance = 12;
    public AnimationCurve railAccelBySpeed;
    public float railBoostDecaySpeed = 0.02f;
    public float railBoostDecayTime = 0.1f;

    [Header("Position on Rails")]
    //public Vector3 SkinOffsetPosRail = new Vector3(0, -0.4f, 0);
    //public Vector3 SkinOffsetPosZip = new Vector3(0, -0.4f, 0);
    public float OffsetRail = 2.05f;
    public float OffsetZip = -5f;
    public float OffsetUpreel = 1.5f;

    [Header("Wall Rules")]
    public float WallCheckDistance = 1.2f;
    public float minHeight = 2;
    public LayerMask wallLayerMask;
    public float wallDuration = 0;

}
