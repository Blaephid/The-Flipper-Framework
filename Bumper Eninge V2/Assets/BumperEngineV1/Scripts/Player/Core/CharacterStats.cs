//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CharacterStats : MonoBehaviour 
//{
//    [Header("Core movement")]

//    [Header("Movement Values")]

//    public float StartAccell = 0.5f;

//    public AnimationCurve AccellOverSpeed;
//    public float AccellShiftOverSpeed;

//    public float TangentialDrag;
//    public float TangentialDragShiftSpeed;

//    public float TurnSpeed = 16f;
//    public float SlowedTurnSpeed = 50f;

//    public AnimationCurve TurnRateOverAngle;
//    public AnimationCurve TurnRateOverAngleSlowed;
//    public AnimationCurve TangDragOverAngle;
//    public AnimationCurve TangDragOverSpeed;

//    public float StartTopSpeed = 65f;
//    public float StartMaxSpeed = 230f;
//    public float StartMaxFallingSpeed = -500f;
//    public float StartJumpPower = 2;

//    public float MoveDecell = 1.3f;
//    public float naturalAirDecell = 1.005f;
//    public float AirDecell = 1.05f;

//    public float GroundStickingDistance = 1;
//    public float GroundStickingPower = -1;


//    [Header ("Turning - PlayerBInput")]
//    public AnimationCurve InputLerpingRateOverSpeed;
//    public bool UtopiaTurning;
//    public AnimationCurve UtopiaInputLerpingRateOverSpeed;
//    public float UtopiaIntensity;
//    public float UtopiaInitialInputLerpSpeed;

//    [Header("Slope effects")]
//    public float SlopeEffectLimit = 0.9f;
//    public float StandOnSlopeLimit = 0.8f;
//    public float SlopePower = 0.5f;
//    public float SlopeRunningAngleLimit = 0.5f;
//    public float SlopeSpeedLimit = 10;

//    public float UphillMultiplier = 0.5f;
//    public float DownhillMultiplier = 2;
//    public float StartDownhillMultiplier = -7; 
//    public AnimationCurve SlopePowerOverSpeed;
//    public AnimationCurve UpHillOverTime;

//    [Header("Greedy Stick to Ground")]
//    [Tooltip("This is the values of the Lerps when the player encounters a slope , the first one is negative slopes (loops), and the second one is positive Slopes (imagine running on the outside of a loop),This values shouldnt be touched unless yuou want to go absurdly faster. Default values 0.885 and 1.5")]
//    public Vector2 StickingLerps = new Vector2(0.885f, 1.5f);
//    [Tooltip("This is the limit from 0 to 1 the degrees that the player should be sticking 0 is no angle , 1 is everything bellow 90°, and 0.5 is 45° angles, default 0.4")]
//    public float StickingNormalLimit = 0.4f;
//    [Tooltip("This is the cast ahead when the player hits a slope, this will be used to predict it's path if it is going on a high speed. too much of this value might send the player flying off before it hits the loop, too little might see micro stutters, default value 1.9")]
//    public float StickCastAhead = 1.9f;
//    [Tooltip("This is the position above the raycast hit point that the player will be placed if he is loosing grip on positive G turns, this value will snap the player back into the mesh, it shouldnt be moved unless you scale the collider, default value 0.6115")]
//    public float negativeGHoverHeight = 0.6115f;
//    public float RayToGroundDistance = 0.55f;
//    public float RaytoGroundSpeedRatio = 0.01f;
//    public float RaytoGroundSpeedMax = 2.4f;
//    public float RayToGroundRotDistance = 1.1f;
//    public float RaytoGroundRotSpeedMax = 2.6f;
//    public float RotationResetThreshold = -0.1f;

//    public LayerMask Playermask;

//    [Header("AirMovementExtras")]
//    public float AirControlAmmount = 2;
//    public float AirSkiddingForce = 10;
//    public bool StopAirMovementIfNoInput = false;
//    public Vector3 Gravity;


//    [Header("Energy Values")]


//    public float MaxEnergy = 200;
//    public float ChunkEnergy = 40;
//    public float EnergyBonus = 0;
//    public float CurrentEnergy;


//    [Header("Rolling Values")]

//    public float RollingLandingBoost;
//    public float RollingDownhillBoost;
//    public float RollingUphillBoost;
//    public float RollingStartSpeed;
//    public float RollingTurningDecreace;
//    public float RollingFlatDecell;
//    public float SlopeTakeoverAmount; // This is the normalized slope angle that the player has to be in order to register the land as "flat"

//    [Header("Skid & Stop")]
//    public float SpeedToStopAt;

//    public float SkiddingStartPoint;
//    public float SkiddingIntensity;

//    [Header("Jump")]
//    public float StartJumpDuration;
//    public float StartSlopedJumpDuration;
//    public float StartJumpSpeed;
//    public float JumpSlopeConversion;
//    public float StopYSpeedOnRelease;
//    public float JumpRollingLandingBoost;

//    [Header("Adittional Jumps")]
//    public bool canDoubleJump;
//    public bool canTripleJump;

//    public float doubleJumpSpeed;
//    public float doubleJumpDuration;

//    [Header("QuickStep")]
//    public float StepSpeed = 60f;
//    public float StepDistance = 12f;
//    public float AirStepSpeed = 50f;
//    public float AirStepDistance = 15f;
//    public LayerMask StepLayerMask;

//    [Header ("Homing Search")]
//    public float TargetSearchDistance = 10;
//    public LayerMask TargetLayer;
//    public LayerMask BlockingLayers;
//    public float FieldOfView;
//    public float IconScale;
//    public float IconDistanceScaling;
//    public float FacingAmount = 0.6f;

//    [Header ("Homing")]

//    public float HomingAttackSpeed;
//    public float HomingTimerLimit;
//    public float HomingSuccessDelay;
//    //public float FacingAmount;
//    public bool CanDashDuringFall;

//    [Header ("Air Dash")]
//    public bool isAdditive;
//    public float AirDashSpeed;
//    public float AirDashDuration;

//    [Header("Spin Dash")]
//    public float MaximumSpeed; //The max amount of speed you can be at to perform a Spin Dash
//    public float MaximumSlope; //The highest slope you can be on to Spin Dash
//    public float SpinDashChargingSpeed = 0.3f;
//    public float MinimunCharge = 10;
//    public float MaximunCharge = 100;
//    public float SpinDashStillForce = 20f;


//    [Header("Bounce")]
//    public float DropSpeed;
//    public float BounceMaxSpeed;
//    public List<float> BounceUpSpeeds;
//    public float BounceUpMaxSpeed;
//    public float BounceConsecutiveFactor;
//    public float BounceHaltFactor;

//    [Header("Light Dash Search")]
//    public float LightDashTargetSearchDistance = 10;
//    public float LightDashIconScale;

//    [Header("Light Dash")]
//    //[SerializeField] float DashingTimerLimit;
//    public float DashSpeed;
//    public float EndingSpeedFactor;
//    public float MinimumEndingSpeed;


//    [Header("Drop Dash")]
//    public float DropDashChargingSpeed = 0.3f;
//    public float DropMinimunCharge = 10;
//    public float DropMaximunCharge = 100;


//    [Header("Interact with Enemies")]

//    public float BouncingPower;
//    public float HomingBouncingPower;
//    public float EnemyHomingStoppingPowerWhenAdditive;
//    public bool StopOnHomingAttackHit;
//    public bool StopOnHit;
//    public float EnemyDamageShakeAmmount;
//    public float EnemyHitShakeAmmount;


//    [Header("Hurt")]
//    public float KnockbackUpwardsForce = 10;
//    public bool ResetSpeedOnHit = false;
//    public float KnockbackForce = 10;

//    public int InvincibilityTime;
//    public int MaxRingLoss;
//    public float RingReleaseSpeed;
//    public float RingArcSpeed;
//    public float FlickerSpeed = 3f;

//    [Header("Rails")]
//    public float MinStartSpeed = 60f;
//    public float RailPushFowardmaxSpeed = 80f;
//    public float RailPushFowardIncrements = 15f;
//    public float RailPushFowardDelay = 0.5f;
//    public float RailSlopePower = 2.5f;
//    public float RailUpHillMultiplier = 0.25f;
//    public float RailDownHillMultiplier = 0.35f;
//    public float RailUpHillMultiplierCrouching = 0.4f;
//    public float RailDownHillMultiplierCrouching = 0.6f;
//    public float RailDragVal = 0.0001f;
//    public float RailPlayerBrakePower = 0.95f;

//    [Header("Position on Rails")]
//    public Vector3 SkinOffsetPosRail = new Vector3(0, -0.4f, 0);
//    public Vector3 SkinOffsetPosZip = new Vector3(0, -0.4f, 0);
//    public float OffsetRail = 2.05f;
//    public float OffsetZip = -2.05f;

//    [Header("Spin Kick")]
//    public float KickDuration = 2f;
//    public float kickForce = 20f;
//    public float kickDamage = 1f;

//    [Header("Wall Rules")]
//    public float WallCheckDistance;
//    public float minHeight;
//    public LayerMask wallLayerMask;
//    public float wallDuration;

//}
