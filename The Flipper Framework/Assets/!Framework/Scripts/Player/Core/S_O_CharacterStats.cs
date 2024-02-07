using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

//[CreateAssetMenu(fileName = "Character X Stats")]
public class S_O_CharacterStats : ScriptableObject
{
    public string Title;
    [Space]

    [Header("Control")]
    [Header("Core Grounded Movement")]
    [Space]
    [Tooltip("Decides what collision layer an object must have to be considered ground, and therefore if the player is grounded when standing on it.")]
    public LayerMask GroundMask;

    [Space]
    #region acceleration
    //-------------------------------------------------------------------------------------------------

    public structAcceleration AccelerationStats = new structAcceleration
    {
        acceleration = 0.16f,
        AccelBySpeed = new AnimationCurve(),
        accelShiftOverSpeed = 1f
    };

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

    [Space]
    #region turning
    //-------------------------------------------------------------------------------------------------

    public structTurning TurningStats = new structTurning
    {
        tangentialDrag = 7.5f,
        tangentialDragShiftSpeed = 1f,
        turnSpeed = 70f,
        TurnRateByAngle = new AnimationCurve(),
        TurnRateBySpeed = new AnimationCurve(),
        TangDragByAngle = new AnimationCurve(),
        TangDragBySpeed = new AnimationCurve()
    };

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

    [Space]
    #region Deceleration
    //-------------------------------------------------------------------------------------------------
    public structDeceleration DecelerationStats = new structDeceleration
    {
        moveDeceleration = 1.05f,
        airDecel = 1.25f,
        DecelBySpeed = new AnimationCurve(),
        decelShiftOverSpeed = 10f,
        naturalAirDecel = 1.002f
    };

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

    [Space]
    #region speeds
    //-------------------------------------------------------------------------------------------------

    public
    structSpeeds SpeedStats = new structSpeeds
    {
        startTopSpeed = 90f,
        startMaxSpeed = 160f
    };

    [System.Serializable]
    public struct structSpeeds
    {
        public float startTopSpeed;
        public float startMaxSpeed;
    }
    #endregion

    [Space]
    #region slopes
    //-------------------------------------------------------------------------------------------------

    public
    structSlopes SlopeStats = new structSlopes
    {
        slopeEffectLimit = 0.85f,
        standOnSlopeLimit = 0.8f,
        slopePower = -1.4f,
        slopeRunningAngleLimit = 0.4f,
        slopeSpeedLimit = new AnimationCurve(),
        generalHillMultiplier = 1.0f,
        uphillMultiplier = 0.6f,
        downhillMultiplier = 0.5f,
        startDownhillMultiplier = -1.7f,
        SlopePowerOverSpeed = new AnimationCurve(),
        UpHillOverTime = new AnimationCurve()
    };

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
        public AnimationCurve slopeSpeedLimit;
        public float generalHillMultiplier;
        [Tooltip("Core : This is multiplied with the force of a slope when going uphill to determine the force against.")]
        public float uphillMultiplier;
        [Tooltip("This is multiplied with the force of a slope when going downhill to determine the force for.")]
        public float downhillMultiplier;
        [Tooltip("Core:")]
        public float startDownhillMultiplier;
        [Tooltip("Core: This determines how much force is gained from the slope depending on the current speed. ")]
        public AnimationCurve SlopePowerOverSpeed;
        [Tooltip("Core:")]
        public AnimationCurve UpHillOverTime;
    }
    #endregion

    [Space]
    #region sticking
    //-------------------------------------------------------------------------------------------------



    public
    structGeneralStick StickToGround = new structGeneralStick
    {
        groundStickingDistance = 0.2f,
        groundStickingPower = -1.45f
    };

    [System.Serializable]
    public struct structGeneralStick
    {
        [Tooltip("Core:")]
        public float groundStickingDistance;
        [Tooltip("Core:")]
        public float groundStickingPower;
    }

    public
    structGreedyStick GreedysStickToGround = new structGreedyStick
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

    [Header("Additional Movement")]
    [Space]
    #region air
    //-------------------------------------------------------------------------------------------------
    public
    structInAir WhenInAir = new structInAir
    {
        startMaxFallingSpeed = -400f,
        startJumpPower = 1.2f,
        shouldStopAirMovementWhenNoInput = true,
        upGravity = new Vector3(0f, -1.45f, 0),
        keepNormalForThis = 0.183f,
        airControlAmmount = 1.3f,
        airSkiddingForce = -2.5f,
        fallGravity = new Vector3(0, -1.5f, 0)
    };

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
        public float airControlAmmount;
        [Tooltip("Surface:")]
        public float airSkiddingForce;
        [Tooltip("Surface:")]
        public Vector3 fallGravity;
    }

    #endregion

    [Space]
    #region rolling
    //-------------------------------------------------------------------------------------------------

    public
    structRolling RollingStats = new structRolling
    {
        rollingLandingBoost = 1.4f,
        rollingDownhillBoost = 1.9f,
        rollingUphillBoost = 1.2f,
        rollingStartSpeed = 5f,
        rollingTurningDecreace = 0.6f,
        rollingFlatDecell = 1.004f,
        slopeTakeoverAmount = 0.995f
    };

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
        public float rollingTurningDecreace;
        [Tooltip("Core:")]
        public float rollingFlatDecell;
        [Tooltip("Core:")]
        public float slopeTakeoverAmount; // This is the normalized slope angle that the player has to be in order to register the land as "flat"
    }

    #endregion

    [Space]
    #region skidding
    //-------------------------------------------------------------------------------------------------
    public
    structSkidding SkiddingStats = new structSkidding
    {
        speedToStopAt = 10,
        skiddingStartPoint = 5,
        skiddingIntensity = -5
    };

    [System.Serializable]
    public struct structSkidding
    { 
        public float speedToStopAt;

        public float skiddingStartPoint;
        public float skiddingIntensity;
    }

    #endregion

    [Header("Actions")]

    [Space]
    #region Jumps
    //-------------------------------------------------------------------------------------------------


    public
    structJumps JumpStats = new structJumps
    {
        coyoteTimeOverSpeed = new AnimationCurve(),
        jumpSlopeConversion = 0.03f,
        stopYSpeedOnRelease = 2f,
        jumpRollingLandingBoost = 0f,
        startJumpDuration = 0.2f,
        startSlopedJumpDuration = 0.2f,
        startJumpSpeed = 4f,
        speedLossOnJump = 0.99f
    };

    [System.Serializable]
    public struct structJumps
    {
        public AnimationCurve coyoteTimeOverSpeed;
        public float jumpSlopeConversion;
        public float stopYSpeedOnRelease;
        public float jumpRollingLandingBoost;
        public float startJumpDuration;
        public float startSlopedJumpDuration;
        public float startJumpSpeed;
        public float speedLossOnJump;
    }
    #endregion

    [Space]
    #region multiJumps
    //-------------------------------------------------------------------------------------------------


    public
    structMultiJumps MultipleJumpStats = new structMultiJumps
    {
        canDoubleJump = true,
        canTripleJump = false,
        jumpCount = 2,
        doubleJumpSpeed = 4.5f,
        doubleJumpDuration = 0.14f,
        speedLossOnDoubleJump = 0.978f
    };

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

    [Space]
    #region Quickstep
    //-------------------------------------------------------------------------------------------------

    public
    structQuickstep QuickstepStats = new structQuickstep
    {
        stepSpeed = 55f,
        stepDistance = 8f,
        airStepSpeed = 48f,
        airStepDistance = 7f,
        StepLayerMask = new LayerMask()
    };

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

    [Space]
    #region homing
    //-------------------------------------------------------------------------------------------------

    public
    structHomingSeach HomingSearch = new structHomingSeach
    {
        targetSearchDistance = 44f,
        faceRange = 70f,
        targetLayer = new LayerMask(),
        blockingLayers = new LayerMask(),
        fieldOfView = 0.3f,
        iconScale = 1.5f,
        iconDistanceScaling = 0.2f,
        facingAmount = 0.91f
    };

    [System.Serializable]
    public struct structHomingSeach
    {
        public float targetSearchDistance;
        public float faceRange;
        public LayerMask targetLayer;
        public LayerMask blockingLayers;
        public float fieldOfView;
        public float iconScale;
        public float iconDistanceScaling;
        public float facingAmount;

    }

    public structHomingAction HomingStats = new structHomingAction
    {
        canDashDuringFall = true,
        attackSpeed = 100f,
        timerLimit = 1f,
        homingSuccessDelay = 0.3f
    };

    [System.Serializable]
    public struct structHomingAction
    {
        public bool canDashDuringFall;
        public float attackSpeed;
        public float timerLimit;
        public float homingSuccessDelay;

    }

    #endregion

    [Space]
    #region jumpDash
    //-------------------------------------------------------------------------------------------------
    public structJumpDash JumpDashStats = new structJumpDash
    {
        dashSpeed = 80f,
        duration = 0.3f,
        isAdditive = true
    };

    public struct structJumpDash
    {
        public float dashSpeed;
        public float duration;
        public bool isAdditive;

    }

    #endregion

    [Space]
    #region spin Charge
    //-------------------------------------------------------------------------------------------------

    public structSpinCharge SpinChargeStats = new structSpinCharge
    {
        spinDashChargingSpeed = 1.02f,
        minimunCharge = 20f,
        maximunCharge = 110f,
        spinDashStillForce = 0.015f,
        maximumSpeed = 200f,
        maximumSlope = -1f,
        releaseShakeAmmount = 1.5f,
        speedLossByTime = new AnimationCurve(),
        forceGainByAngle = new AnimationCurve(),
        gainBySpeed = new AnimationCurve(),
        spinSkidStartPoint = 10f,
        spinSkidIntesity = 3f
    };

    [System.Serializable]
    public struct structSpinCharge
    {
        public float spinDashChargingSpeed;
        public float minimunCharge;
        public float maximunCharge;
        public float spinDashStillForce;
        public float maximumSpeed; //The max amount of speed you can be at to perform a Spin Dash
        public float maximumSlope; //The highest slope you can be on to Spin Dash
        public float releaseShakeAmmount;
        public AnimationCurve speedLossByTime;
        public AnimationCurve forceGainByAngle;
        public AnimationCurve gainBySpeed;
        public float spinSkidStartPoint;
        public float spinSkidIntesity;

    }
    #endregion

    [Space]
    #region Bounce
    //-------------------------------------------------------------------------------------------------

    public StructBounce BounceStats = new StructBounce
    {
        dropSpeed = 100f,
        BounceMaxSpeed = 140f,
        bounceUpSpeeds = new List<float>(),
        bounceHaltFactor = 0.7f,
        bounceCoolDown = 8f,
        bounceUpMaxSpeed = 75f,
        bounceConsecutiveFactor = 1.05f
    };

    [System.Serializable]
    public struct StructBounce
    {
        public float dropSpeed;
        public float BounceMaxSpeed;
        public List<float> bounceUpSpeeds;
        public float bounceHaltFactor;
        public float bounceCoolDown;
        public float bounceUpMaxSpeed;
        public float bounceConsecutiveFactor;

    }
    #endregion

    [Space]
    #region ring Road
    //-------------------------------------------------------------------------------------------------
    public structRingRoad RingRoadStats = new structRingRoad
    {
        dashSpeed = 100f,
        endingSpeedFactor = 1.23f,
        minimumEndingSpeed = 60f,
        RingTargetSearchDistance = 8f,
        RingRoadIconScale = 0f,
        RingRoadLayer = new LayerMask()
    };

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

    [Space]
    #region DropCharge
    //-------------------------------------------------------------------------------------------------

    public structDropCharge DropChargeStats = new structDropCharge
    {
        chargingSpeed = 1.2f,
        minimunCharge = 40f,
        maximunCharge = 150f
    };

    [System.Serializable]
    public struct structDropCharge
    {
        public float chargingSpeed;
        public float minimunCharge;
        public float maximunCharge;

    }
    #endregion

    [Header("Interactions")]

    [Header("Objects and Enemies")]
    [Space]
    #region enemy interaction
    //-------------------------------------------------------------------------------------------------

    public structEnemyInteract EnemyInteraction = new structEnemyInteract
    {
        bouncingPower = 45f,
        homingBouncingPower = 40f,
        enemyHomingStoppingPowerWhenAdditive = 40f,
        shouldStopOnHomingAttackHit = true,
        shouldStopOnHit = true,
        enemyDamageShakeAmmount = 0.5f,
        enemyHitShakeAmmount = 1.2f
    };

    [System.Serializable]
    public struct structEnemyInteract
    {
        public float bouncingPower;
        public float homingBouncingPower;
        public float enemyHomingStoppingPowerWhenAdditive;
        public bool shouldStopOnHomingAttackHit ;
        public bool shouldStopOnHit ;
        public float enemyDamageShakeAmmount;
        public float enemyHitShakeAmmount;
    }
    #endregion

    [Space]
    #region pull Items
    //-------------------------------------------------------------------------------------------------
    public
    structItemPull ItemPulling = new structItemPull
    {
        RadiusBySpeed = new AnimationCurve(),
        RingMask = new LayerMask(),
        basePullSpeed = 1.2f
    };

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

    [Header("Damage")]
    [Space]
    #region Bonk
    //-------------------------------------------------------------------------------------------------
    public StructBonk WhenBonked = new StructBonk
    {
        BonkOnWalls = new LayerMask(),
        bonkUpwardsForce = 16f,
        bonkBackwardsForce = 18f,
        bonkControlLock = 20f,
        bonkControlLockAir = 40f
    };

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

    [Space]
    #region Hurt
    //-------------------------------------------------------------------------------------------------


    public StructHurt WhenHurt = new StructHurt
    {
        invincibilityTime = 90,
        maxRingLoss = 20,
        knockbackUpwardsForce = 30f,
        resetSpeedOnHit = false,
        recoilFrom = new LayerMask(),
        knockbackForce = 25f,
        ringReleaseSpeed = 550f,
        ringArcSpeed = 250f,
        flickerSpeed = 3f,
        hurtControlLock = 15f,
        hurtControlLockAir = 30f
    };

    [System.Serializable]
    public struct StructHurt
    {
        public int invincibilityTime;
        public int maxRingLoss;
        public float knockbackUpwardsForce;
        public bool resetSpeedOnHit;
        public LayerMask recoilFrom;
        public float knockbackForce;
        public float ringReleaseSpeed;
        public float ringArcSpeed;
        public float flickerSpeed;
        public float hurtControlLock;
        public float hurtControlLockAir;

    }
    #endregion

    [Header("Situational Actions")]
    [Space]
    #region rails
    //-------------------------------------------------------------------------------------------------

    public StructRails RailStats = new StructRails
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
        railAccelBySpeed = new AnimationCurve(),
        railBoostDecaySpeed = 0.45f,
        railBoostDecayTime = 0.45f
    };

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
        public AnimationCurve railAccelBySpeed;
        public float railBoostDecaySpeed;
        public float railBoostDecayTime;

    }

    public StructPositionOnRail PositionWhenOnRails = new StructPositionOnRail
    {
        offsetRail = 1.3f,
        offsetZip = -6.7f,
        upreel = 0.3f
    };

    [System.Serializable]
    public struct StructPositionOnRail
    {
        public float offsetRail;
        public float offsetZip;
        public float upreel;

    }
    #endregion

    [Space]
    #region WallRules
    //-------------------------------------------------------------------------------------------------

    public StructWallRunning WallRunningStats = new StructWallRunning
    {
        wallCheckDistance = 1.2f,
        minHeight = 5f,
        WallLayerMask = new LayerMask(),
        wallDuration = 0f,
        scrapeModifier = 1f,
        climbModifier = 1f
    };

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

}
#endregion
