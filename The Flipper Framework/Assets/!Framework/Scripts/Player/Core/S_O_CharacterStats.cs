using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using static S_O_CharacterStats;
#if UNITY_EDITOR
using UnityEditor;
#endif

//[CreateAssetMenu(fileName = "Character X Stats")]
public class S_O_CharacterStats : ScriptableObject
{
        [HideInInspector] public string Title = "Title";

        public int IIintIna;

        #region acceleration
        //-------------------------------------------------------------------------------------------------

        public structAcceleration DefaultAccelerationStats = setstructAcceleration();
        public structAcceleration AccelerationStats = setstructAcceleration();

        static structAcceleration setstructAcceleration () {
                return new structAcceleration
                {
                        acceleration = 0.16f,
                        AccelBySpeed = new AnimationCurve(),
                        accelShiftOverSpeed = 1f
                };
        }

        [System.Serializable]
        public struct structAcceleration
        {
                [Tooltip("Surface: This determines the average acceleration")]
                public float acceleration;
                [Tooltip("Core:")]
                public AnimationCurve AccelBySpeed;
                [Tooltip("Core:")]
                public float accelShiftOverSpeed;
        };
        #endregion

        #region turning
        //-------------------------------------------------------------------------------------------------
        public structTurning DefaultTurningStats = setstructTurning();
        public structTurning TurningStats = setstructTurning();

        static structTurning setstructTurning () {
                return new structTurning
                {
                        tangentialDrag = 7.5f,
                        tangentialDragShiftSpeed = 1f,
                        turnSpeed = 14f,
                        TurnRateByAngle = new AnimationCurve(),
                        TurnRateBySpeed = new AnimationCurve(),
                        TangDragByAngle = new AnimationCurve(),
                        TangDragBySpeed = new AnimationCurve()
                };
        }

        [System.Serializable]
        public struct structTurning
        {
                public float tangentialDrag;
                public float tangentialDragShiftSpeed;
                public float turnSpeed;
                public AnimationCurve TurnRateByAngle;
                public AnimationCurve TurnRateBySpeed;
                public AnimationCurve TangDragByAngle;
                public AnimationCurve TangDragBySpeed;
        }
        #endregion

        #region Deceleration
        //-------------------------------------------------------------------------------------------------
        public structDeceleration DefaultDecelerationStats = setstructDeceleration();
        public structDeceleration DecelerationStats = setstructDeceleration();

        static structDeceleration setstructDeceleration () {
                return new structDeceleration
                {
                        moveDeceleration = 1.05f,
                        airDecel = 1.25f,
                        DecelBySpeed = new AnimationCurve(),
                        decelShiftOverSpeed = 10f,
                        naturalAirDecel = 1.002f
                };
        }

        [System.Serializable]
        public struct structDeceleration
        {
                public float moveDeceleration;
                public float airDecel;
                public AnimationCurve DecelBySpeed;
                public float decelShiftOverSpeed;
                public float naturalAirDecel;
        }
        #endregion

        #region speeds
        //-------------------------------------------------------------------------------------------------
        public structSpeeds DefaultSpeedStats = setStructSpeeds();
        public structSpeeds SpeedStats = setStructSpeeds();

        static structSpeeds setStructSpeeds () {
                return new structSpeeds
                {
                        topSpeed = 90f,
                        maxSpeed = 160f
                };
        }

        [System.Serializable]
        public struct structSpeeds
        {
                public float topSpeed;
                public float maxSpeed;
        }
        #endregion

        #region slopes
        //-------------------------------------------------------------------------------------------------

        public structSlopes DefaultSlopeStats = setStructSlopes();
        public structSlopes SlopeStats = setStructSlopes();

        static structSlopes setStructSlopes () {
                return new structSlopes
                {
                        slopeEffectLimit = 0.85f,
                        standOnSlopeLimit = 0.8f,
                        slopePower = -1.4f,
                        slopeRunningAngleLimit = 0.4f,
                        SlopeLimitBySpeed = new AnimationCurve(),
                        generalHillMultiplier = 1.0f,
                        uphillMultiplier = 0.6f,
                        downhillMultiplier = 0.5f,
                        startDownhillMultiplier = -1.7f,
                        SlopePowerByCurrentSpeed = new AnimationCurve(),
                        UpHillEffectByTime = new AnimationCurve()
                };
        }

        [System.Serializable]
        public struct structSlopes
        {
                [Tooltip("Core:")]
                public float slopeEffectLimit;
                [Tooltip("Core:")]
                public float standOnSlopeLimit;
                [Tooltip("Core:")]
                public float slopePower;
                [Tooltip("Core:")]
                public float slopeRunningAngleLimit;
                [Tooltip("Core:")]
                public AnimationCurve SlopeLimitBySpeed;
                public float generalHillMultiplier;
                [Tooltip("Core : This is multiplied with the force of a slope when going uphill to determine the force against.")]
                public float uphillMultiplier;
                [Tooltip("This is multiplied with the force of a slope when going downhill to determine the force for.")]
                public float downhillMultiplier;
                [Tooltip("Core:")]
                public float startDownhillMultiplier;
                [Tooltip("Core: This determines how much force is gained from the slope depending on the current speed. ")]
                public AnimationCurve SlopePowerByCurrentSpeed;
                [Tooltip("Core:")]
                public AnimationCurve UpHillEffectByTime;
        }
        #endregion

        #region sticking
        //-------------------------------------------------------------------------------------------------

        public structGeneralStick DefaultStickToGround = setStructGeneralStick();
        public structGeneralStick StickToGround = setStructGeneralStick();

        static structGeneralStick setStructGeneralStick () {
                return new structGeneralStick
                {
                        GroundMask = new LayerMask(),
                        groundStickingDistance = 0.2f,
                        groundStickingPower = -1.45f
                };
        }

        [System.Serializable]
        public struct structGeneralStick
        {
                [Tooltip("Core: Decides what collision layer an object must have to be considered ground, and therefore if the player is grounded when standing on it.")]
                public LayerMask GroundMask;

                [Tooltip("Core:")]
                public float groundStickingDistance;
                [Tooltip("Core:")]
                public float groundStickingPower;
        }

        public structGreedyStick DefaultGreedysStickToGround = setStructGreedyStick();
        public structGreedyStick GreedysStickToGround = setStructGreedyStick();

        static structGreedyStick setStructGreedyStick () {
                return new structGreedyStick
                {
                        stickingLerps = new Vector2(0.885f, 1.005f),
                        stickingNormalLimit = 0.5f,
                        stickCastAhead = 1.9f,
                        negativeGHoverHeight = 0.8f,
                        rayToGroundDistance = 1.4f,
                        raytoGroundSpeedRatio = 0.01f,
                        raytoGroundSpeedMax = 2.6f,
                        rayToGroundRotDistance = 1.1f,
                        raytoGroundRotSpeedMax = 2.6f,
                        rotationResetThreshold = -0.1f
                };
        }

        [System.Serializable]
        public struct structGreedyStick
        {
                [Tooltip("Core: This is the values of the Lerps when the player encounters a slope , the first one is negative slopes (loops), and the second one is positive Slopes (imagine running on the outside of a loop),This values shouldnt be touched unless yuou want to go absurdly faster. Default values 0.885 and 1.5")]
                public Vector2 stickingLerps;
                [Tooltip("Core: This is the limit from 0 to 1 the degrees that the player should be sticking 0 is no angle , 1 is everything bellow 90°, and 0.5 is 45° angles, default 0.4")]
                public float stickingNormalLimit;
                [Tooltip("Core: This is the cast ahead when the player hits a slope, this will be used to predict it's path if it is going on a high speed. too much of this value might send the player flying off before it hits the loop, too little might see micro stutters, default value 1.9")]
                public float stickCastAhead;
                [Tooltip("Core: This is the position above the raycast hit point that the player will be placed if he is loosing grip on positive G turns, this value will snap the player back into the mesh, it shouldnt be moved unless you scale the collider, default value 0.6115")]
                public float negativeGHoverHeight;
                [Tooltip("Core:")]
                public float rayToGroundDistance;
                [Tooltip("Core:")]
                public float raytoGroundSpeedRatio;
                [Tooltip("Core:")]
                public float raytoGroundSpeedMax;
                [Tooltip("Core:")]
                public float rayToGroundRotDistance;
                [Tooltip("Core:")]
                public float raytoGroundRotSpeedMax;
                [Tooltip("Core:")]
                public float rotationResetThreshold;
        }
        #endregion

        #region air
        //-------------------------------------------------------------------------------------------------
        public structInAir WhenInAir = setStructInAir();
        public structInAir DefaultWhenInAir = setStructInAir();
        static structInAir setStructInAir () {
                return new structInAir
                {
                        startMaxFallingSpeed = -400f,
                        startJumpPower = 1.2f,
                        shouldStopAirMovementWhenNoInput = true,
                        upGravity = new Vector3(0f, -1.45f, 0),
                        keepNormalForThis = 0.183f,
                        controlAmmount = 1.3f,
                        skiddingForce = -2.5f,
                        fallGravity = new Vector3(0, -1.5f, 0)
                };
        }

        [System.Serializable]
        public struct structInAir
        {
                [Tooltip("Surface:")]
                public float startMaxFallingSpeed;
                [Tooltip("Surface:")]
                public float startJumpPower;
                [Tooltip("Core:")]
                public bool shouldStopAirMovementWhenNoInput;
                [Tooltip("Core:")]
                public Vector3 upGravity;
                [Tooltip("Core:")]
                public float keepNormalForThis;
                [Tooltip("Surface:")]
                public float controlAmmount;
                [Tooltip("Surface:")]
                public float skiddingForce;
                [Tooltip("Surface:")]
                public Vector3 fallGravity;
        }

        #endregion

        #region rolling
        //-------------------------------------------------------------------------------------------------


        public structRolling RollingStats = setStructRolling();
        public structRolling DefaultRollingStats = setStructRolling();

        static structRolling setStructRolling () {
                return new structRolling
                {
                        rollingLandingBoost = 1.4f,
                        rollingDownhillBoost = 1.9f,
                        rollingUphillBoost = 1.2f,
                        rollingStartSpeed = 5f,
                        rollingTurningDecrease = 0.6f,
                        rollingFlatDecell = 1.004f,
                        slopeTakeoverAmount = 0.995f
                };
        }



        [System.Serializable]
        public struct structRolling
        {
                [Tooltip("Core:")]
                public float rollingLandingBoost;
                [Tooltip("Core:")]
                public float rollingDownhillBoost;
                [Tooltip("Core:")]
                public float rollingUphillBoost;
                [Tooltip("Core:")]
                public float rollingStartSpeed;
                [Tooltip("Core:")]
                public float rollingTurningDecrease;
                [Tooltip("Core:")]
                public float rollingFlatDecell;
                [Tooltip("Core:")]
                public float slopeTakeoverAmount; // This is the normalized slope angle that the player has to be in order to register the land as "flat"
        }

        #endregion

        #region skidding
        //-------------------------------------------------------------------------------------------------
        public structSkidding SkiddingStats = setStructSkidding();
        public structSkidding DefaultSkiddingStats = setStructSkidding();

        static structSkidding setStructSkidding () {
                return new structSkidding
                {
                        speedToStopAt = 10,
                        skiddingStartPoint = 5,
                        skiddingIntensity = -5
                };
        }


        [System.Serializable]
        public struct structSkidding
        {
                public float speedToStopAt;

                public float skiddingStartPoint;
                public float skiddingIntensity;
        }

        #endregion

        #region Jumps
        //-------------------------------------------------------------------------------------------------

        public structJumps JumpStats = setStructJumps();
        public structJumps DefaultJumpStats = setStructJumps();

        static structJumps setStructJumps () {
                return new structJumps
                {
                        CoyoteTimeBySpeed = new AnimationCurve(),
                        jumpSlopeConversion = 0.03f,
                        stopYSpeedOnRelease = 2f,
                        jumpRollingLandingBoost = 0f,
                        startJumpDuration = 0.2f,
                        startSlopedJumpDuration = 0.2f,
                        startJumpSpeed = 4f,
                        speedLossOnJump = 0.99f
                };
        }


        [System.Serializable]
        public struct structJumps
        {
                public AnimationCurve CoyoteTimeBySpeed;
                public float jumpSlopeConversion;
                public float stopYSpeedOnRelease;
                public float jumpRollingLandingBoost;
                public float startJumpDuration;
                public float startSlopedJumpDuration;
                public float startJumpSpeed;
                public float speedLossOnJump;
        }
        #endregion

        #region multiJumps
        //-------------------------------------------------------------------------------------------------
        public structMultiJumps MultipleJumpStats = setStructMultiJumps();
        public structMultiJumps DefaultMultipleJumpStats = setStructMultiJumps();

        static structMultiJumps setStructMultiJumps () {
                return new structMultiJumps
                {
                        canDoubleJump = true,
                        canTripleJump = false,
                        jumpCount = 2,
                        doubleJumpSpeed = 4.5f,
                        doubleJumpDuration = 0.14f,
                        speedLossOnDoubleJump = 0.978f
                };
        }



        [System.Serializable]
        public struct structMultiJumps
        {
                public bool canDoubleJump;
                public bool canTripleJump;
                public int jumpCount;

                public float doubleJumpSpeed;
                public float doubleJumpDuration;
                public float speedLossOnDoubleJump;
        }
        #endregion

        #region Quickstep
        //-------------------------------------------------------------------------------------------------
        public structQuickstep QuickstepStats = setStructQuickstep();
        public structQuickstep DefaultQuickstepStats = setStructQuickstep();

        static structQuickstep setStructQuickstep () {
                return new structQuickstep
                {
                        stepSpeed = 55f,
                        stepDistance = 8f,
                        airStepSpeed = 48f,
                        airStepDistance = 7f,
                        StepLayerMask = new LayerMask()
                };
        }


        [System.Serializable]
        public struct structQuickstep
        {
                public float stepSpeed;
                public float stepDistance;
                public float airStepSpeed;
                public float airStepDistance;
                public LayerMask StepLayerMask;

        }
        #endregion

        #region homing
        //-------------------------------------------------------------------------------------------------
        public structHomingSearch HomingSearch = setStructHomingSearch();
        public structHomingSearch DefaultHomingSearch = setStructHomingSearch();

        static structHomingSearch setStructHomingSearch () {
                return new structHomingSearch
                {
                        targetSearchDistance = 44f,
                        faceRange = 70f,
                        TargetLayer = new LayerMask(),
                        blockingLayers = new LayerMask(),
                        fieldOfView = 0.3f,
                        iconScale = 1.5f,
                        iconDistanceScaling = 0.2f,
                        facingAmount = 0.91f
                };
        }

        [System.Serializable]
        public struct structHomingSearch
        {
                public float targetSearchDistance;
                public float faceRange;
                public LayerMask TargetLayer;
                public LayerMask blockingLayers;
                public float fieldOfView;
                public float iconScale;
                public float iconDistanceScaling;
                public float facingAmount;

        }

        public structHomingAction HomingStats = setStructHomingAction();
        public structHomingAction DefaultHomingStats = setStructHomingAction();

        static structHomingAction setStructHomingAction () {
                return new structHomingAction
                {
                        canDashWhenFalling = true,
                        attackSpeed = 100f,
                        timerLimit = 1f,
                        successDelay = 0.3f
                };
        }

        [System.Serializable]
        public struct structHomingAction
        {
                public bool canDashWhenFalling;
                public float attackSpeed;
                public float timerLimit;
                public float successDelay;

        }

        #endregion

        #region jumpDash
        //-------------------------------------------------------------------------------------------------
        public structJumpDash JumpDashStats = setStructJumpDash();
        public structJumpDash DefaultJumpDashStats = setStructJumpDash();

        static structJumpDash setStructJumpDash () {
                return new structJumpDash
                {
                        dashSpeed = 80f,
                        duration = 0.3f,
                        shouldUseCurrentSpeedAsMinimum = true
                };
        }
        public struct structJumpDash
        {
                public float dashSpeed;
                public float duration;
                public bool shouldUseCurrentSpeedAsMinimum;

        }

        #endregion

        #region spin Charge
        //-------------------------------------------------------------------------------------------------

        public structSpinCharge SpinChargeStats = setStructSpinCharge();
        public structSpinCharge DefaultSpinChargeStats = setStructSpinCharge();

        static structSpinCharge setStructSpinCharge () {
                return new structSpinCharge
                {
                        chargingSpeed = 1.02f,
                        minimunCharge = 20f,
                        maximunCharge = 110f,
                        forceAgainstMovement = 0.015f,
                        maximumSpeedPerformedAt = 200f,
                        maximumSlopePerformedAt = -1f,
                        releaseShakeAmmount = 1.5f,
                        SpeedLossByTime = new AnimationCurve(),
                        ForceGainByAngle = new AnimationCurve(),
                        ForceGainByCurrentSpeed = new AnimationCurve(),
                        skidStartPoint = 10f,
                        skidIntesity = 3f
                };
        }

        [System.Serializable]
        public struct structSpinCharge
        {
                public float chargingSpeed;
                public float minimunCharge;
                public float maximunCharge;
                public float forceAgainstMovement;
                public float maximumSpeedPerformedAt; //The max amount of speed you can be at to perform a Spin Dash
                public float maximumSlopePerformedAt; //The highest slope you can be on to Spin Dash
                public float releaseShakeAmmount;
                public AnimationCurve SpeedLossByTime;
                public AnimationCurve ForceGainByAngle;
                public AnimationCurve ForceGainByCurrentSpeed;
                public float skidStartPoint;
                public float skidIntesity;

        }
        #endregion

        #region Bounce
        //-------------------------------------------------------------------------------------------------

        public StructBounce BounceStats = setStructBounce();
        public StructBounce DefaultBounceStats = setStructBounce();

        static StructBounce setStructBounce () {
                return new StructBounce
                {
                        dropSpeed = 100f,
                        BounceMaxSpeed = 140f,
                        listOfBounceSpeeds = new List<float>(),
                        bounceHaltFactor = 0.7f,
                        bounceCoolDown = 8f,
                        bounceUpMaxSpeed = 75f,
                        bounceConsecutiveFactor = 1.05f
                };
        }


        [System.Serializable]
        public struct StructBounce
        {
                public float dropSpeed;
                public float BounceMaxSpeed;
                public List<float> listOfBounceSpeeds;
                public float bounceHaltFactor;
                public float bounceCoolDown;
                public float bounceUpMaxSpeed;
                public float bounceConsecutiveFactor;

        }
        #endregion

        #region ring Road
        //-------------------------------------------------------------------------------------------------
        public structRingRoad RingRoadStats = setStructRingRoad();
        public structRingRoad DefaultRingRoadStats = setStructRingRoad();
        static structRingRoad setStructRingRoad () {
                return new structRingRoad
                {
                        dashSpeed = 100f,
                        endingSpeedFactor = 1.23f,
                        minimumEndingSpeed = 60f,
                        RingTargetSearchDistance = 8f,
                        RingRoadIconScale = 0f,
                        RingRoadLayer = new LayerMask()
                };
        }



        [System.Serializable]
        public struct structRingRoad
        {
                public float dashSpeed;
                public float endingSpeedFactor;
                public float minimumEndingSpeed;
                public float RingTargetSearchDistance;
                public float RingRoadIconScale;
                public LayerMask RingRoadLayer;
        }
        #endregion

        #region DropCharge
        //-------------------------------------------------------------------------------------------------

        public structDropCharge DropChargeStats = setStructDropCharge();
        public structDropCharge DefaultDropChargeStats = setStructDropCharge();

        static structDropCharge setStructDropCharge () {
                return new structDropCharge
                {
                        chargingSpeed = 1.2f,
                        minimunCharge = 40f,
                        maximunCharge = 150f
                };
        }



        [System.Serializable]
        public struct structDropCharge
        {
                public float chargingSpeed;
                public float minimunCharge;
                public float maximunCharge;

        }
        #endregion

        #region enemy interaction
        //-------------------------------------------------------------------------------------------------

        public structEnemyInteract EnemyInteraction = setStructEnemyInteract();
        public structEnemyInteract DefaultEnemyInteraction = setStructEnemyInteract();
        static structEnemyInteract setStructEnemyInteract () {
                return new structEnemyInteract
                {
                        bouncingPower = 45f,
                        homingBouncingPower = 40f,
                        enemyHomingStoppingPowerWhenAdditive = 40f,
                        shouldStopOnHomingAttackHit = true,
                        shouldStopOnHit = true,
                        enemyDamageShakeAmmount = 0.5f,
                        enemyHitShakeAmmount = 1.2f
                };
        }



        [System.Serializable]
        public struct structEnemyInteract
        {
                public float bouncingPower;
                public float homingBouncingPower;
                public float enemyHomingStoppingPowerWhenAdditive;
                public bool shouldStopOnHomingAttackHit;
                public bool shouldStopOnHit;
                public float enemyDamageShakeAmmount;
                public float enemyHitShakeAmmount;
        }
        #endregion

        #region pull Items
        //-------------------------------------------------------------------------------------------------
        public structItemPull ItemPulling = setStructItemPull();
        public structItemPull DefaultItemPulling = setStructItemPull();

        static structItemPull setStructItemPull () {
                return new structItemPull
                {
                        RadiusBySpeed = new AnimationCurve(),
                        RingMask = new LayerMask(),
                        basePullSpeed = 1.2f
                };
        }


        [System.Serializable]
        public struct structItemPull
        {
                [Tooltip("Core:")]
                public AnimationCurve RadiusBySpeed;
                [Tooltip("Core:")]
                public LayerMask RingMask;
                [Tooltip("Core:")]
                public float basePullSpeed;
        }
        #endregion

        #region Bonk
        //-------------------------------------------------------------------------------------------------
        public StructBonk WhenBonked = setStructBonk();
        public StructBonk DefaultWhenBonked = setStructBonk();

        static StructBonk setStructBonk () {
                return new StructBonk
                {
                        BonkOnWalls = new LayerMask(),
                        bonkUpwardsForce = 16f,
                        bonkBackwardsForce = 18f,
                        bonkControlLock = 20f,
                        bonkControlLockAir = 40f
                };
        }

        [System.Serializable]
        public struct StructBonk
        {
                public LayerMask BonkOnWalls;
                public float bonkUpwardsForce;
                public float bonkBackwardsForce;
                public float bonkControlLock;
                public float bonkControlLockAir;

        }
        #endregion

        #region Hurt
        //-------------------------------------------------------------------------------------------------


        public StructHurt WhenHurt = setStructHurt();
        public StructHurt DefaultWhenHurt = setStructHurt();

        static StructHurt setStructHurt () {
                return new StructHurt
                {
                        invincibilityTime = 90,
                        maxRingLoss = 20,
                        knockbackUpwardsForce = 30f,
                        shouldResetSpeedOnHit = false,
                        recoilFrom = new LayerMask(),
                        knockbackForce = 25f,
                        ringReleaseSpeed = 550f,
                        ringArcSpeed = 250f,
                        flickerSpeed = 3f,
                        hurtControlLock = 15f,
                        hurtControlLockAir = 30f
                };
        }


        [System.Serializable]
        public struct StructHurt
        {
                public int invincibilityTime;
                public int maxRingLoss;
                public float knockbackUpwardsForce;
                public bool shouldResetSpeedOnHit;
                public LayerMask recoilFrom;
                public float knockbackForce;
                public float ringReleaseSpeed;
                public float ringArcSpeed;
                public float flickerSpeed;
                public float hurtControlLock;
                public float hurtControlLockAir;

        }
        #endregion

        #region rails
        //-------------------------------------------------------------------------------------------------

        public StructRails RailStats = setStructRails();
        public StructRails DefaultRailStats = setStructRails();

        static StructRails setStructRails () {
                return new StructRails
                {
                        railMaxSpeed = 125f,
                        railTopSpeed = 80f,
                        railDecaySpeedHigh = 0.18f,
                        railDecaySpeedLow = 0.06f,
                        MinStartSpeed = 20f,
                        RailPushFowardmaxSpeed = 90f,
                        RailPushFowardIncrements = 5f,
                        RailPushFowardDelay = 0.42f,
                        RailSlopePower = 2.5f,
                        RailUpHillMultiplier = 1.9f,
                        RailDownHillMultiplier = 0.5f,
                        RailUpHillMultiplierCrouching = 2.3f,
                        RailDownHillMultiplierCrouching = 0.65f,
                        RailDragVal = 0.0001f,
                        RailPlayerBrakePower = 0.97f,
                        hopDelay = 0.3f,
                        hopSpeed = 3.5f,
                        hopDistance = 12f,
                        RailAccelerationBySpeed = new AnimationCurve(),
                        railBoostDecaySpeed = 0.45f,
                        railBoostDecayTime = 0.45f
                };
        }


        [System.Serializable]
        public struct StructRails
        {
                public float railMaxSpeed;
                public float railTopSpeed;
                public float railDecaySpeedHigh;
                public float railDecaySpeedLow;
                public float MinStartSpeed;
                public float RailPushFowardmaxSpeed;
                public float RailPushFowardIncrements;
                public float RailPushFowardDelay;
                public float RailSlopePower;
                public float RailUpHillMultiplier;
                public float RailDownHillMultiplier;
                public float RailUpHillMultiplierCrouching;
                public float RailDownHillMultiplierCrouching;
                public float RailDragVal;
                public float RailPlayerBrakePower;
                public float hopDelay;
                public float hopSpeed;
                public float hopDistance;
                public AnimationCurve RailAccelerationBySpeed;
                public float railBoostDecaySpeed;
                public float railBoostDecayTime;

        }

        public StructPositionOnRail RailPosition = setStructPositionOnRail();
        public StructPositionOnRail DefaultRailPosition = setStructPositionOnRail();

        static StructPositionOnRail setStructPositionOnRail () {
                return new StructPositionOnRail
                {
                        offsetRail = 1.3f,
                        offsetZip = -6.7f,
                        upreel = 0.3f
                };
        }


        [System.Serializable]
        public struct StructPositionOnRail
        {
                public float offsetRail;
                public float offsetZip;
                public float upreel;

        }
        #endregion

        #region WallRules
        //-------------------------------------------------------------------------------------------------

        public StructWallRunning WallRunningStats = setWallRunning();
        public StructWallRunning DefaultWallRunningStats = setWallRunning();

        static StructWallRunning setWallRunning () {
                return new StructWallRunning
                {
                        wallCheckDistance = 1.2f,
                        minHeight = 5f,
                        WallLayerMask = new LayerMask(),
                        wallDuration = 0f,
                        scrapeModifier = 1f,
                        climbModifier = 1f
                };
        }

        [System.Serializable]
        public struct StructWallRunning
        {
                public float wallCheckDistance;
                public float minHeight;
                public LayerMask WallLayerMask;
                public float wallDuration;
                public float scrapeModifier;
                public float climbModifier;

        }

        #region Defaults
        public S_O_CharacterStats DefaultStats;


        #endregion
}
#endregion

#if UNITY_EDITOR
[CustomEditor(typeof(S_O_CharacterStats))]
public class S_O_CharacterStatsEditor : Editor
{
        public override void OnInspectorGUI () {
                //base.OnInspectorGUI();

                //Setting variables
                S_O_CharacterStats stats = (S_O_CharacterStats)target;
                GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 19;

                GUIStyle ResetToDefaultButton;
                ResetToDefaultButton = new GUIStyle(GUI.skin.button);
                ResetToDefaultButton.fontSize = 9;
                ResetToDefaultButton.normal.textColor = Color.black;
                ResetToDefaultButton.fixedWidth = 160;



                //Will only happen if above is attatched.
                if (stats == null) return;

                serializedObject.Update();

                //Start Tite and description
                stats.Title = EditorGUILayout.TextField(stats.Title);

                EditorGUILayout.TextArea("This objects contains a bunch of stats you can change to adjust how the character controls. \n" +
                "Feel free to copy and paste at your leisure, or input your own. \n" +
                "Every stat is organised in structs relevant to its purpose, and hovering over one will display a tooltip describing its function. \n" +
                "The tooltip will also say if it's a core stat (meaning it has large effects on the controller and should be changed with care), or a surface stat " +
                "(meaning it can easily be changed without much damage, and is ideal for making different character control different in the same playstyle). \n" +
                "At the botton are a number of buttons to reset any struct to whatever is set as the default.", EditorStyles.textArea);

                //Order of Drawing
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Core Movement", headerStyle);
                DrawSpeed();
                DrawAccel();
                DrawDecel();
                DrawTurning();
                DrawSlopes();
                DrawSticking();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Additional Movement", headerStyle);
                DrawAir();
                DrawRolling();
                DrawSkidding();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Aerial Actions", headerStyle);
                DrawJumping();
                DrawMultiJumps();
                DrawJumpDash();
                DrawHoming();
                DrawBounce();
                DrawDropCharge();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Grounded Actions", headerStyle);
                DrawSpinCharge();
                DrawQuickstep();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Situational Actions", headerStyle);
                DrawRingRoad();
                DrawRailStats();
                DrawRailPosition();
                DrawWallRunningStats();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Interactions", headerStyle);
                DrawEnemyInteraction();
                DrawItemPulling();
                DrawWhenBonked();
                drawWhenHurt();

                //Speeds
                #region Speeds
                //Speeds
                void DrawSpeed () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("SpeedStats"), new GUIContent("Speeds"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.SpeedStats = stats.DefaultSpeedStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Acceleration
                #region Acceleration
                void DrawAccel () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("AccelerationStats"), new GUIContent("Acceleration"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.AccelerationStats = stats.DefaultAccelerationStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Decel
                #region Deceleration
                void DrawDecel () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("DecelerationStats"), new GUIContent("Deceleration"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.DecelerationStats = stats.DefaultDecelerationStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Turning
                #region Turning
                void DrawTurning () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("TurningStats"), new GUIContent("Turning"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.TurningStats = stats.DefaultTurningStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Slopes
                #region Slopes
                void DrawSlopes () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("SlopeStats"), new GUIContent("On Slopes"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.SlopeStats = stats.DefaultSlopeStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Sticking
                #region Sticking
                void DrawSticking () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("StickToGround"), new GUIContent("Sticking To the Ground"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("GreedysStickToGround"), new GUIContent("Sticking to the Ground (Greedy's Version)"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.StickToGround = stats.DefaultStickToGround;
                                stats.GreedysStickToGround = stats.DefaultGreedysStickToGround;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //In Air
                #region Sticking
                void DrawAir () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("WhenInAir"), new GUIContent("Air Control"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.WhenInAir = stats.DefaultWhenInAir;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Rolling
                #region Sticking
                void DrawRolling () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("RollingStats"), new GUIContent("Rolling"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.RollingStats = stats.DefaultRollingStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Skidding
                #region Skidding
                void DrawSkidding () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("SkiddingStats"), new GUIContent("Skidding"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.SkiddingStats = stats.DefaultSkiddingStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Jumping
                #region Jumping
                void DrawJumping () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("JumpStats"), new GUIContent("Jumps"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.JumpStats = stats.DefaultJumpStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Multi Jumping
                #region MultiJumping
                void DrawMultiJumps () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("MultipleJumpStats"), new GUIContent("Additional Jumps"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.MultipleJumpStats = stats.DefaultMultipleJumpStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Quickstep
                #region Quickstep
                void DrawQuickstep () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("QuickstepStats"), new GUIContent("Quickstep"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.QuickstepStats = stats.DefaultQuickstepStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Spin Charge
                #region SpinCharge
                void DrawSpinCharge () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("SpinChargeStats"), new GUIContent("Spin Charge"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.SpinChargeStats = stats.DefaultSpinChargeStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Homing
                #region Homing
                void DrawHoming () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("HomingStats"), new GUIContent("Homing Attack"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("HomingSearch"), new GUIContent("Homing Targetting"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.HomingStats = stats.DefaultHomingStats;
                                stats.HomingSearch = stats.DefaultHomingSearch;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //Bounce
                #region Bounce
                void DrawBounce () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("BounceStats"), new GUIContent("Bounce"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.BounceStats = stats.DefaultBounceStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //RingRoad        
                #region RingRoad
                void DrawRingRoad () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("RingRoadStats"), new GUIContent("Ring Road"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.RingRoadStats = stats.DefaultRingRoadStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //DropCharge
                #region DropCharge
                void DrawDropCharge () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("DropChargeStats"), new GUIContent("Drop Charge"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.DropChargeStats = stats.DefaultDropChargeStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //JumpDash
                #region JumpDash
                void DrawJumpDash () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("JumpDashStats"), new GUIContent("Jump Dash"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.JumpDashStats = stats.DefaultJumpDashStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //EnemyInteraction
                #region EnemyInteraction
                void DrawEnemyInteraction () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("EnemyInteraction"), new GUIContent("Interacting with Enemies"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.EnemyInteraction = stats.DefaultEnemyInteraction;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //ItemPulling
                #region ItemPulling
                void DrawItemPulling () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemPulling"), new GUIContent("Pulling in Items"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.ItemPulling = stats.DefaultItemPulling;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion


                //WhenBonked
                #region WhenBonked
                void DrawWhenBonked () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("WhenBonked"), new GUIContent("Bonking"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.WhenBonked = stats.DefaultWhenBonked;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion


                //WhenHurt       
                #region WhenHurt
                void drawWhenHurt () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("WhenHurt"), new GUIContent("When Hurt"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.WhenHurt = stats.DefaultWhenHurt;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //RailStats    
                #region RailStats
                void DrawRailStats () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("RailStats"), new GUIContent("Rail Grinding"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.RailStats = stats.DefaultRailStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //RailPosition
                #region RailPosition
                void DrawRailPosition () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("RailPosition"), new GUIContent("Position on Rails"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.RailPosition = stats.DefaultRailPosition;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion

                //WallRunningStats
                #region WallRunningStats
                void DrawWallRunningStats () {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("WallRunningStats"), new GUIContent("Wall Running"));

                        Undo.RecordObject(stats, "set to defaults");
                        if (GUILayout.Button("Default", ResetToDefaultButton))
                        {
                                stats.WallRunningStats = stats.DefaultWallRunningStats;
                        }
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion


                //DrawDefaultInspector();       
        }

}
#endif
