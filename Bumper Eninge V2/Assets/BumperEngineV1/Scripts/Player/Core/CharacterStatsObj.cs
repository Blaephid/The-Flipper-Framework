using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character X Stats")]
public class CharacterStatsObj : ScriptableObject
{
    public string Title;

    [Header("Core movement")]

    [Header("Movement Values")]

    public float StartAccell = 0.16f;

    public AnimationCurve AccellOverSpeed;
    public float AccellShiftOverSpeed = 1;

    public float TangentialDrag = 7.5f;
    public float TangentialDragShiftSpeed = 1;

    public float TurnSpeed = 70f;
    public float SlowedTurnSpeed = 200f;

    public AnimationCurve TurnRateOverAngle;
    public AnimationCurve TurnRateOverAngleSlowed;
    public AnimationCurve TangDragOverAngle;
    public AnimationCurve TangDragOverSpeed;

    public float StartTopSpeed = 90f;
    public float StartMaxSpeed = 170f;
    public float StartMaxFallingSpeed = -400f;
    public float StartJumpPower = 1.2f;

    public float MoveDecell = 1.05f;
    public float naturalAirDecell = 1.002f;
    public float AirDecell = 1.25f;

    public float GroundStickingDistance = 0.15f;
    public float GroundStickingPower = -1.45f;


    [Header ("Turning - PlayerBInput")]
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
    public float SlopeSpeedLimit = 110;

    public float UphillMultiplier = 0.55f;
    public float DownhillMultiplier = 0.5f;
    public float StartDownhillMultiplier = -1.8f; 
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
    public float AirControlAmmount = 0.8f;
    public float AirSkiddingForce = 6;
    public bool StopAirMovementIfNoInput = true;
    public Vector3 Gravity = new Vector3 (0f, -1.5f, 0);


    [Header("Rolling Values")]

    public float RollingLandingBoost = 1.4f;
    public float RollingDownhillBoost = 1.9f;
    public float RollingUphillBoost = 1.2f;
    public float RollingStartSpeed = 30;
    public float RollingTurningDecreace = 0.6f;
    public float RollingFlatDecell = 1.004f;
    public float SlopeTakeoverAmount = 0.995f; // This is the normalized slope angle that the player has to be in order to register the land as "flat"

    [Header("Skid & Stop")]
    public float SpeedToStopAt = 16;

    public float SkiddingStartPoint = 5;
    public float SkiddingIntensity = -4;

    [Header("Jump")]
    public float StartJumpDuration = 0.2f;
    public float StartSlopedJumpDuration = 0.2f;
    public float StartJumpSpeed = 4;
    public float JumpSlopeConversion = 0.03f;
    public float StopYSpeedOnRelease = 2;
    public float JumpRollingLandingBoost;

    [Header("Adittional Jumps")]
    public bool canDoubleJump = true;
    public bool canTripleJump = false;

    public float doubleJumpSpeed = 4.5f;
    public float doubleJumpDuration = 0.14f;

    [Header("QuickStep")]
    public float StepSpeed = 50f;
    public float StepDistance = 8f;
    public float AirStepSpeed = 45f;
    public float AirStepDistance = 7f;
    public LayerMask StepLayerMask;

    [Header ("Homing Search")]
    public float TargetSearchDistance = 30;
    public LayerMask TargetLayer;
    public LayerMask BlockingLayers;
    public float FieldOfView = 0.2f;
    public float IconScale = 1.5f;
    public float IconDistanceScaling = 0.2f;
    public float FacingAmount = 0.6f;

    [Header ("Homing")]

    public float HomingAttackSpeed = 70;
    public float HomingTimerLimit = 1;
    public float HomingSuccessDelay = 0.4f;
    //public float FacingAmount;
    public bool CanDashDuringFall = true;

    [Header ("Air Dash")]
    public bool isAdditive = true;
    public float AirDashSpeed = 60;
    public float AirDashDuration = 0.4f;

    [Header("Spin Dash")]
    public float MaximumSpeed = 25; //The max amount of speed you can be at to perform a Spin Dash
    public float MaximumSlope = 0.9f; //The highest slope you can be on to Spin Dash
    public float SpinDashChargingSpeed = 1.08f;
    public float MinimunCharge = 20;
    public float MaximunCharge = 100;
    public float SpinDashStillForce = 1.05f;


    [Header("Bounce")]
    public float DropSpeed = 100;
    public float BounceMaxSpeed = 140;
    public List<float> BounceUpSpeeds;
    public float BounceUpMaxSpeed = 75;
    public float BounceConsecutiveFactor = 1.05f;
    public float BounceHaltFactor = 0.85f;

    [Header("Light Dash Search")]
    public float LightDashTargetSearchDistance = 60;
    public float LightDashIconScale = 0;

    [Header("Light Dash")]
    //[SerializeField] float DashingTimerLimit;
    public float DashSpeed = 100;
    public float EndingSpeedFactor = 1.2f;
    public float MinimumEndingSpeed = 60;


    [Header("Drop Dash")]
    public float DropDashChargingSpeed = 1.2f;
    public float DropMinimunCharge = 40;
    public float DropMaximunCharge = 150;


    [Header("Interact with Enemies")]

    public float BouncingPower = 40;
    public float HomingBouncingPower = 38;
    public float EnemyHomingStoppingPowerWhenAdditive = 40;
    public bool StopOnHomingAttackHit = true;
    public bool StopOnHit = true;
    public float EnemyDamageShakeAmmount = 0.5f;
    public float EnemyHitShakeAmmount = 1.2f;


    [Header("Hurt")]
    public float KnockbackUpwardsForce = 30;
    public bool ResetSpeedOnHit = false;
    public float KnockbackForce = 40;

    public int InvincibilityTime = 90;
    public int MaxRingLoss = 20;
    public float RingReleaseSpeed = 550;
    public float RingArcSpeed = 20;
    public float FlickerSpeed = 3f;

    [Header("Rails")]
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

    [Header("Position on Rails")]
    public Vector3 SkinOffsetPosRail = new Vector3(0, -0.4f, 0);
    public Vector3 SkinOffsetPosZip = new Vector3(0, -0.4f, 0);
    public float OffsetRail = 2.05f;
    public float OffsetZip = -5f;

    [Header("Slide")]
    public float SlideDuration = 0.7f;
    public float SlideForce = 2000f;
    public float slideDamage = 1f;

    [Header("Wall Rules")]
    public float WallCheckDistance = 1.2f;
    public float minHeight = 2;
    public LayerMask wallLayerMask;
    public float wallDuration = 0;

    [Header("Wall Effects")]
    public float scrapeModi = 1f;
    public float climbModi = 1f;

}
