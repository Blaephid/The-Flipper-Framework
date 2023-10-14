using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Character", menuName = "SonicGT/Character/New Character")]
public class CharacterStatsHolder : ScriptableObject
{

    public new string name;
    public string Description;
    public Sprite RaceUIIcon;

    [Header("Speed Values")]
    public float MoveAccell = 0.45f;
    public AnimationCurve AccellOverSpeed;
    public float AccellShiftOverSpeed = 1;
    public float MoveDecell = 1.025f;
    public float TangentialDrag = 7.5f;
    public float TangentialDragShiftSpeed = 1;
    public float TurnSpeed = 100;
    public AnimationCurve TurnRateOverAngle;
    public AnimationCurve TangDragOverAngle;
    public AnimationCurve TangDragOverSpeed;
    public float TopSpeed = 130;
    public float MaxSpeed = 343;
    public float MaxFallingSpeed = -500;
    [Tooltip("Speed at which Char stops very quickly if there no input")]
    public float SpeedToStopAt = 70f;
    public float SkiddingStartPoint = 55;
    public float SkiddingIntensity = -1.9F;
    public float Brakepower = 0.975f;

    [Space]
    [Header("Water")]
    public bool DontNeedToBreathe = false;
    public float DrowningTimeLimit = 30;
    public float WaterRunningSpeed = 170;
    public float UnderwaterDownGravMult = 0.8f;
    public float UnderwaterMoveAccell = 0.25f;
    public float UnderwaterAirControlAmmount = 2;


    [Space]
    [Header("Aerials values")]
    public float AirDecell = 1.00175f;
    public float AirControlAmmount = 2;
    [Tooltip("Brake Power in the air, multiplied by chars move aceleration")]
    public float AirSkiddingForce = 10;

    [Space]
    [Header("Slopes values")]
    public float GroundStickingDistance = 0.2f;
    [Tooltip("Downforce Aplied to Character to Stick to the ground")]
    public float GroundStickingPower = -1.68f;
    [Tooltip("Dot limit for standing on slopes bellow speed limit")]
    public float SlopeStandingLimit = 0.95f;
    public float SlopePower = -2.5f;
    [Tooltip("Dot limit for running Bellow Speed limit")]
    public float SlopeRunningAngleLimit = 0.75f;
    [Tooltip("Minimum Speed for slopes")]
    public float SlopeSpeedLimit = 70;
    [Tooltip("Multiplication of speed Uphill, Higher Harder")]
    public float UphillMultiplier = 0.15f;
    [Tooltip("Multiplication of speed Downhill")]
    public float DownhillMultiplier = 1.25f;
    [Tooltip("Minimum Y velocity that start gaining speed")]
    public float StartDownhillMultiplier = -7;
    public AnimationCurve SlopePowerOverSpeed;
    public float SlopePowerShiftSpeed = 1;
    public float LandingConversionFactor = 25;

    [Space]
    [Header("Rolling Values")]
    public float RollingLandingBoost = 2.5f;
    public float RollingDownhillBoost = 2.5f;
    public float RollingUphillBoost = 1.5f;
    public float RollingStartSpeed = 1000;
    public float RollingTurningDecreace = 0.75f;
    public float RollingFlatDecell = 1.005f;
    [Tooltip("This is the normalized slope angle that the player has to be in order to register the land as 'flat'")]
    public float SlopeTakeoverAmount = 0.995f;

    [Space]
    [Header("Jumping")]
    public float JumpDuration = 0.09f;
    public float SlopedJumpDuration = 0.09f;
    public float JumpSpeed = 13;
    public float JumpSlopeConversion = 0.02f;
    public float StopYSpeedOnRelease = 2;

    [Space]
    [Header("Dash/Homing")]
    public bool CanDashHome = true;
    public bool isAdditive = true;
    public bool CanDashDuringFall = true;
    public float AirDashSpeed = 60;
    public float AirdashTimerLimit = 0.3f;
    [Space]
    public float HomingAttackSpeed = 130;
    public float HomingTimerLimit = 1;
    public float HomingHeightLimit = 30;
    public float HomingTurning = 0.5f;
    public float HomingAttackDist = 80;
    public float HomingSpeedRatio = 0.25f;

    [Header ("Enemy Contact Variables")]
    public float HomingBouncingPower = 60;
    public float EnemyHomingStoppingPowerWhenAdditive = 3.5f;
    public float HomingExitSpeedMultiplier = 0.1f;
    public float HomingResetInterval = 0.05f;
    public float MomentumHomingExitSpeedMultiplier = 0.9f;
    public int EnemyVoiceClipInterval = 3;
    public float EnemyDamageShakeAmmount = 0.5f;

    [Space]
    [Header("Spindash Values")]
    public float SpindashMaximumSpeedToStart = 70;
    public float SpindashMaximumSlopeToStart = 0.5F;
    public float BallAnimationSpeedMultiplier = 5;
    public float SpinDashChargedEffectAmm = 0.05f;
    public float SpinDashChargingSpeed = 1;
    public float MinimunCharge = 70;
    public float MaximunCharge = 210;
    public float Overcharge = 25;
    public float SpinDashStillForce = 1.045f;
    public float ReleaseCamLagAmmount = 0.1f;
    public float ReleaseShakeAmmount = 0;
    public float ForceIntoRollfor = 0.6f;



    [Space]
    [Header("Rail Values")]
    public float RailMinStartSpeed = 60f;
    public float RailPushFowardmaxSpeed = 80f;
    public float RailPushFowardIncrements = 15f;
    public float RailPushFowardDelay = 0.5f;
    public float RailSlopePower = 3.5f;
    public float RailUpHillMultiplier = 0.45f;
    public float RailDownHillMultiplier = 0.8f;
    public float RailUpHillMultiplierCrouching = 0.8f;
    public float RailDownHillMultiplierCrouching = 1f;
    public float RailDragVal = 0.0001f;
    public float railPlayerBrakePower = 0.95f;

    [Space]
    [Header("Bounce Values")]
    public bool BounceEnabled;
    public float BounceDownwardsDirectioRatio = 0.75f;
    public float BounceMaxSpeed = 240f;
    public float BounceDropSpeed = 500;
    [Tooltip("Default 50,75,100")]
    public float[] BounceUpSpeeds = new float[3];
    public float BounceUpMaxSpeed = 100;
    public float BounceHaltFactor = 0.75f;


    [Space]
    [Header("Melee Combat")]
    public bool MeleeEnabled;

    public bool hasMelee;
    public bool hasRanged;
    public bool MeleeRangedHoming;
    public bool MeleeRangedAuto;
    //public PlayerProjectiles MeleeProjectile;
    public float MeleeCooldown = 0.3f;
    public float MeleeAttackLenght = 0.3f;
    public float MeleeRangedtargetingDistance;
    public float MeleeVelocityMultiplyGrounded = 1;
    public float MeleeVelocityMultiplyAir = 1;
    public int MeleeMaxShots = 10;


    [Space]
    [Header("Stomp Values")]
    public bool StompEnabled;
    public float StompDropSpeed = 500;
    public float StompHaltFactor = 0.58f;
    public float StompSpeedTransferRatio = 1f;
    public float StompMinimalSlope = 0.07f;
    public float StompDelayTime = 0.15f;

    [Space]
    [Header("LightDash Values")]
    public bool LightDashEnabled;
    public float LSReach = 20;
    public float LSDashSpeed = 180;
    public float LSEndingSpeedFactor = 1.3f;
    public float LSEndingSpeedFactorFail = 1.1f;
    public float LSMinimumEndingSpeed = 130;
    public float LSMaximumEndingSpeed = 210;

    [Space]
    [Header("Wall Jump")]
    public bool WallJumpEnabled;
    public float WJInitialDelay = 0.25f;
    public float WJmaxDotDir = 0.9f;
    public float WJWallStickRay = 2f;
    public float WJWallSpeedDrag = 0.1f;
    public float WJMaxFallSpeed = 30f;
    public float WJDirectionMultiplier = 0.5f;
    public float WJExitJumpSpeed = 7f;
    public float WJFollowWallThreshold = 0.5f;
    public float WJHighToLowThreshold = 60;
    public Vector3 WJSpeedReduction = new Vector3(0.94f, 0.96f, 0.94f);
    public Vector3 WJSpeedReductionHighSpeed = new Vector3(0.995f, 0.995f, 0.995f);
    public Vector3 WJSkinOffsetPos = new Vector3(0, 0, -0.585f);


    [Space]
    [Header("Tricks Values")]
    public bool TricksEnabled;
    public float TrickTimeToPoint = 0.35f;
    public Vector3 TrickSmallBoostUp = new Vector3(0, 10, 0);

    

}
