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

	public StrucAcceleration DefaultAccelerationStats = SetStrucAcceleration();
	public StrucAcceleration AccelerationStats = SetStrucAcceleration();

	static StrucAcceleration SetStrucAcceleration () {
		return new StrucAcceleration
		{
			acceleration = 0.16f,
			AccelBySpeed = new AnimationCurve(),
			accelShiftOverSpeed = 1f
		};
	}

	[System.Serializable]
	public struct StrucAcceleration
	{
		[Tooltip("Surface: This determines the average acceleration")]
		public float              acceleration;
		[Tooltip("Core:")]
		public AnimationCurve     AccelBySpeed;
		[Tooltip("Core:")]
		public float              accelShiftOverSpeed;
	};
	#endregion

	#region turning
	//-------------------------------------------------------------------------------------------------
	public StrucTurning DefaultTurningStats = SetStrucTurning();
	public StrucTurning TurningStats = SetStrucTurning();

	static StrucTurning SetStrucTurning () {
		return new StrucTurning
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
	public struct StrucTurning
	{
		public float              tangentialDrag;
		public float              tangentialDragShiftSpeed;
		public float              turnSpeed;
		public AnimationCurve     TurnRateByAngle;
		public AnimationCurve     TurnRateBySpeed;
		public AnimationCurve     TangDragByAngle;
		public AnimationCurve     TangDragBySpeed;
	}
	#endregion

	#region Deceleration
	//-------------------------------------------------------------------------------------------------
	public StrucDeceleration DefaultDecelerationStats = SetStrucDeceleration();
	public StrucDeceleration DecelerationStats = SetStrucDeceleration();

	static StrucDeceleration SetStrucDeceleration () {
		return new StrucDeceleration
		{
			moveDeceleration = 1.05f,
			airDecel = 1.25f,
			DecelBySpeed = new AnimationCurve(),
			decelShiftOverSpeed = 10f,
			naturalAirDecel = 1.002f
		};
	}

	[System.Serializable]
	public struct StrucDeceleration
	{
		public float              moveDeceleration;
		public float              airDecel;
		public AnimationCurve     DecelBySpeed;
		public float              decelShiftOverSpeed;
		public float              naturalAirDecel;
	}
	#endregion

	#region speeds
	//-------------------------------------------------------------------------------------------------
	public StrucSpeeds DefaultSpeedStats = SetStrucSpeeds();
	public StrucSpeeds SpeedStats = SetStrucSpeeds();

	static StrucSpeeds SetStrucSpeeds () {
		return new StrucSpeeds
		{
			topSpeed = 90f,
			maxSpeed = 160f
		};
	}

	[System.Serializable]
	public struct StrucSpeeds
	{
		public float    topSpeed;
		public float    maxSpeed;
	}
	#endregion

	#region slopes
	//-------------------------------------------------------------------------------------------------

	public StrucSlopes DefaultSlopeStats = SetStrucSlopes();
	public StrucSlopes SlopeStats = SetStrucSlopes();

	static StrucSlopes SetStrucSlopes () {
		return new StrucSlopes
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
	public struct StrucSlopes
	{
		[Tooltip("Core:")]
		public float              slopeEffectLimit;
		[Tooltip("Core:")]
		public float              standOnSlopeLimit;
		[Tooltip("Core:")]
		public float              slopePower;
		[Tooltip("Core:")]
		public float              slopeRunningAngleLimit;
		[Tooltip("Core:")]
		public AnimationCurve     SlopeLimitBySpeed;
		public float              generalHillMultiplier;
		[Tooltip("Core : This is multiplied with the force of a slope when going uphill to determine the force against.")]
		public float              uphillMultiplier;
		[Tooltip("This is multiplied with the force of a slope when going downhill to determine the force for.")]
		public float              downhillMultiplier;
		[Tooltip("Core:")]
		public float              startDownhillMultiplier;
		[Tooltip("Core: This determines how much force is gained from the slope depending on the current speed. ")]
		public AnimationCurve     SlopePowerByCurrentSpeed;
		[Tooltip("Core:")]
		public AnimationCurve     UpHillEffectByTime;
	}
	#endregion

	#region sticking
	//-------------------------------------------------------------------------------------------------

	public StrucGeneralStick DefaultStickToGround = SetStrucGeneralStick();
	public StrucGeneralStick StickToGround = SetStrucGeneralStick();

	static StrucGeneralStick SetStrucGeneralStick () {
		return new StrucGeneralStick
		{
			GroundMask = new LayerMask(),
			groundStickingDistance = 0.2f,
			groundStickingPower = -1.45f
		};
	}

	[System.Serializable]
	public struct StrucGeneralStick
	{
		[Tooltip("Core: Decides what collision layer an object must have to be considered ground, and therefore if the player is grounded when standing on it.")]
		public LayerMask          GroundMask;

		[Tooltip("Core:")]
		public float    groundStickingDistance;
		[Tooltip("Core:")]
		public float    groundStickingPower;
	}

	public StrucGreedyStick DefaultGreedysStickToGround = SetStrucGreedyStick();
	public StrucGreedyStick GreedysStickToGround = SetStrucGreedyStick();

	static StrucGreedyStick SetStrucGreedyStick () {
		return new StrucGreedyStick
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
	public struct StrucGreedyStick
	{
		[Tooltip("Core: This is the values of the Lerps when the player encounters a slope , the first one is negative slopes (loops), and the second one is positive Slopes (imagine running on the outside of a loop),This values shouldnt be touched unless yuou want to go absurdly faster. Default values 0.885 and 1.5")]
		public Vector2  stickingLerps;
		[Tooltip("Core: This is the limit from 0 to 1 the degrees that the player should be sticking 0 is no angle , 1 is everything bellow 90°, and 0.5 is 45° angles, default 0.4")]
		public float    stickingNormalLimit;
		[Tooltip("Core: This is the cast ahead when the player hits a slope, this will be used to predict it's path if it is going on a high speed. too much of this value might send the player flying off before it hits the loop, too little might see micro stutters, default value 1.9")]
		public float    stickCastAhead;
		[Tooltip("Core: This is the position above the raycast hit point that the player will be placed if he is loosing grip on positive G turns, this value will snap the player back into the mesh, it shouldnt be moved unless you scale the collider, default value 0.6115")]
		public float    negativeGHoverHeight;
		[Tooltip("Core:")]
		public float    rayToGroundDistance;
		[Tooltip("Core:")]
		public float    raytoGroundSpeedRatio;
		[Tooltip("Core:")]
		public float    raytoGroundSpeedMax;
		[Tooltip("Core:")]
		public float    rayToGroundRotDistance;
		[Tooltip("Core:")]
		public float    raytoGroundRotSpeedMax;
		[Tooltip("Core:")]
		public float    rotationResetThreshold;
	}
	#endregion

	#region air
	//-------------------------------------------------------------------------------------------------
	public StrucInAir WhenInAir = SetStrucInAir();
	public StrucInAir DefaultWhenInAir = SetStrucInAir();
	static StrucInAir SetStrucInAir () {
		return new StrucInAir
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
	public struct StrucInAir
	{
		[Tooltip("Surface:")]
		public float    startMaxFallingSpeed;
		[Tooltip("Surface:")]
		public float    startJumpPower;
		[Tooltip("Core:")]
		public bool               shouldStopAirMovementWhenNoInput;
		[Tooltip("Core:")]
		public Vector3  upGravity;
		[Tooltip("Core:")]
		public float    keepNormalForThis;
		[Tooltip("Surface:")]
		public float    controlAmmount;
		[Tooltip("Surface:")]
		public float    skiddingForce;
		[Tooltip("Surface:")]
		public Vector3  fallGravity;
	}

	#endregion

	#region rolling
	//-------------------------------------------------------------------------------------------------


	public StrucRolling RollingStats = SetStrucRolling();
	public StrucRolling DefaultRollingStats = SetStrucRolling();

	static StrucRolling SetStrucRolling () {
		return new StrucRolling
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
	public struct StrucRolling
	{
		[Tooltip("Core:")]
		public float    rollingLandingBoost;
		[Tooltip("Core:")]
		public float    rollingDownhillBoost;
		[Tooltip("Core:")]
		public float    rollingUphillBoost;
		[Tooltip("Core:")]
		public float    rollingStartSpeed;
		[Tooltip("Core:")]
		public float    rollingTurningDecrease;
		[Tooltip("Core:")]
		public float    rollingFlatDecell;
		[Tooltip("Core:")]
		public float    slopeTakeoverAmount; // This is the normalized slope angle that the player has to be in order to register the land as "flat"
	}

	#endregion

	#region skidding
	//-------------------------------------------------------------------------------------------------
	public StrucSkidding SkiddingStats = SetStrucSkidding();
	public StrucSkidding DefaultSkiddingStats = SetStrucSkidding();

	static StrucSkidding SetStrucSkidding () {
		return new StrucSkidding
		{
			speedToStopAt = 10,
			skiddingStartPoint = 5,
			skiddingIntensity = -5
		};
	}


	[System.Serializable]
	public struct StrucSkidding
	{
		public float    speedToStopAt;

		public float    skiddingStartPoint;
		public float    skiddingIntensity;
	}

	#endregion

	#region Jumps
	//-------------------------------------------------------------------------------------------------

	public StrucJumps JumpStats = SetStrucJumps();
	public StrucJumps DefaultJumpStats = SetStrucJumps();

	static StrucJumps SetStrucJumps () {
		return new StrucJumps
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
	public struct StrucJumps
	{
		public AnimationCurve CoyoteTimeBySpeed;
		public float    jumpSlopeConversion;
		public float    stopYSpeedOnRelease;
		public float    jumpRollingLandingBoost;
		public float    startJumpDuration;
		public float    startSlopedJumpDuration;
		public float    startJumpSpeed;
		public float    speedLossOnJump;
	}
	#endregion

	#region multiJumps
	//-------------------------------------------------------------------------------------------------
	public StrucMultiJumps MultipleJumpStats = SetStrucMultiJumps();
	public StrucMultiJumps DefaultMultipleJumpStats = SetStrucMultiJumps();

	static StrucMultiJumps SetStrucMultiJumps () {
		return new StrucMultiJumps
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
	public struct StrucMultiJumps
	{
		public bool               canDoubleJump;
		public bool               canTripleJump;
		public int                jumpCount;

		public float    doubleJumpSpeed;
		public float    doubleJumpDuration;
		public float    speedLossOnDoubleJump;
	}
	#endregion

	#region Quickstep
	//-------------------------------------------------------------------------------------------------
	public StrucQuickstep QuickstepStats = SetStrucQuickstep();
	public StrucQuickstep DefaultQuickstepStats = SetStrucQuickstep();

	static StrucQuickstep SetStrucQuickstep () {
		return new StrucQuickstep
		{
			stepSpeed = 55f,
			stepDistance = 8f,
			airStepSpeed = 48f,
			airStepDistance = 7f,
			StepLayerMask = new LayerMask()
		};
	}


	[System.Serializable]
	public struct StrucQuickstep
	{
		public float    stepSpeed;
		public float    stepDistance;
		public float    airStepSpeed;
		public float    airStepDistance;
		public LayerMask          StepLayerMask;

	}
	#endregion

	#region jumpDash
	//-------------------------------------------------------------------------------------------------


	#endregion

	public StrucAirDash JumpDashStats = SetStrucJumpDash();
	public StrucAirDash DefaultJumpDashStats = SetStrucJumpDash();

	static StrucAirDash SetStrucJumpDash () {
		return new StrucAirDash
		{
			dashSpeed = 80f,
			duration = 0.3f,
			shouldUseCurrentSpeedAsMinimum = true
		};
	}

	[System.Serializable]
	public struct StrucAirDash
	{
		public float        dashSpeed;
		public float        duration;
		public bool         shouldUseCurrentSpeedAsMinimum;

	}

	#region homing
	//-------------------------------------------------------------------------------------------------
	public StrucHomingSearch HomingSearch = SetStrucHomingSearch();
	public StrucHomingSearch DefaultHomingSearch = SetStrucHomingSearch();

	static StrucHomingSearch SetStrucHomingSearch () {
		return new StrucHomingSearch
		{
			targetSearchDistance = 44f,
			faceRange = 70f,
			TargetLayer = new LayerMask(),
			blockingLayers = new LayerMask(),
		
			iconScale = 1.5f,
			iconDistanceScaling = 0.2f,
			facingAmount = 0.91f
		};
	}

	[System.Serializable]
	public struct StrucHomingSearch
	{
		public float    targetSearchDistance;
		public float    faceRange;
		public LayerMask          TargetLayer;
		public LayerMask          blockingLayers;
		public float    iconScale;
		public float    iconDistanceScaling;
		public float    facingAmount;

	}

	public StrucHomingAction HomingStats = SetStrucHomingAction();
	public StrucHomingAction DefaultHomingStats = SetStrucHomingAction();

	static StrucHomingAction SetStrucHomingAction () {
		return new StrucHomingAction
		{
			canDashWhenFalling = true,
			attackSpeed = 100f,
			timerLimit = 1f,
			successDelay = 0.3f
		};
	}

	[System.Serializable]
	public struct StrucHomingAction
	{
		public bool               canDashWhenFalling;
		public float    attackSpeed;
		public float    timerLimit;
		public float    successDelay;

	}

	#endregion

	

	#region spin Charge
	//-------------------------------------------------------------------------------------------------

	public StrucSpinCharge SpinChargeStats = SetStrucSpinCharge();
	public StrucSpinCharge DefaultSpinChargeStats = SetStrucSpinCharge();

	static StrucSpinCharge SetStrucSpinCharge () {
		return new StrucSpinCharge
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
	public struct StrucSpinCharge
	{
		public float              chargingSpeed;
		public float              minimunCharge;
		public float              maximunCharge;
		public float              forceAgainstMovement;
		public float              maximumSpeedPerformedAt; //The max amount of speed you can be at to perform a Spin Dash
		public float              maximumSlopePerformedAt; //The highest slope you can be on to Spin Dash
		public float              releaseShakeAmmount;
		public AnimationCurve     SpeedLossByTime;
		public AnimationCurve     ForceGainByAngle;
		public AnimationCurve     ForceGainByCurrentSpeed;
		public float              skidStartPoint;
		public float              skidIntesity;

	}
	#endregion

	#region Bounce
	//-------------------------------------------------------------------------------------------------

	public StrucBounce BounceStats = SetStrucBounce();
	public StrucBounce DefaultBounceStats = SetStrucBounce();

	static StrucBounce SetStrucBounce () {
		return new StrucBounce
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
	public struct StrucBounce
	{
		public float              dropSpeed;
		public float              BounceMaxSpeed;
		public List<float>                  listOfBounceSpeeds;
		public float              bounceHaltFactor;
		public float              bounceCoolDown;
		public float              bounceUpMaxSpeed;
		public float              bounceConsecutiveFactor;

	}
	#endregion

	#region ring Road
	//-------------------------------------------------------------------------------------------------
	public StrucRingRoad RingRoadStats = SetStrucRingRoad();
	public StrucRingRoad DefaultRingRoadStats = SetStrucRingRoad();
	static StrucRingRoad SetStrucRingRoad () {
		return new StrucRingRoad
		{
			dashSpeed = 100f,
			endingSpeedFactor = 1.23f,
			minimumEndingSpeed = 60f,
			SearchDistance = 8f,
			iconScale = 0f,
			RingRoadLayer = new LayerMask()
		};
	}



	[System.Serializable]
	public struct StrucRingRoad
	{
		public float    dashSpeed;
		public float    endingSpeedFactor;
		public float    minimumEndingSpeed;
		public float    SearchDistance;
		public float    iconScale;
		public LayerMask          RingRoadLayer;
	}
	#endregion

	#region DropCharge
	//-------------------------------------------------------------------------------------------------

	public StrucDropCharge DropChargeStats = SetStrucDropCharge();
	public StrucDropCharge DefaultDropChargeStats = SetStrucDropCharge();

	static StrucDropCharge SetStrucDropCharge () {
		return new StrucDropCharge
		{
			chargingSpeed = 1.2f,
			minimunCharge = 40f,
			maximunCharge = 150f
		};
	}



	[System.Serializable]
	public struct StrucDropCharge
	{
		public float      chargingSpeed;
		public float      minimunCharge;
		public float      maximunCharge;
	}
	#endregion

	#region enemy interaction
	//-------------------------------------------------------------------------------------------------

	public StrucEnemyInteract EnemyInteraction = SetStrucEnemyInteract();
	public StrucEnemyInteract DefaultEnemyInteraction = SetStrucEnemyInteract();
	static StrucEnemyInteract SetStrucEnemyInteract () {
		return new StrucEnemyInteract
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
	public struct StrucEnemyInteract
	{
		public float      bouncingPower;
		public float      homingBouncingPower;
		public float      enemyHomingStoppingPowerWhenAdditive;
		public bool       shouldStopOnHomingAttackHit;
		public bool       shouldStopOnHit;
		public float      enemyDamageShakeAmmount;
		public float      enemyHitShakeAmmount;
	}
	#endregion

	#region pull Items
	//-------------------------------------------------------------------------------------------------
	public StrucItemPull ItemPulling = SetStrucItemPull();
	public StrucItemPull DefaultItemPulling = SetStrucItemPull();

	static StrucItemPull SetStrucItemPull () {
		return new StrucItemPull
		{
			RadiusBySpeed = new AnimationCurve(),
			RingMask = new LayerMask(),
			basePullSpeed = 1.2f
		};
	}


	[System.Serializable]
	public struct StrucItemPull
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
	public StrucBonk WhenBonked = SetStrucBonk();
	public StrucBonk DefaultWhenBonked = SetStrucBonk();

	static StrucBonk SetStrucBonk () {
		return new StrucBonk
		{
			BonkOnWalls = new LayerMask(),
			bonkUpwardsForce = 16f,
			bonkBackwardsForce = 18f,
			bonkControlLock = 20f,
			bonkControlLockAir = 40f
		};
	}

	[System.Serializable]
	public struct StrucBonk
	{
		public LayerMask              BonkOnWalls;
		public float                  bonkUpwardsForce;
		public float                  bonkBackwardsForce;
		public float                  bonkControlLock;
		public float                  bonkControlLockAir;

	}
	#endregion

	#region Hurt
	//-------------------------------------------------------------------------------------------------


	public StrucHurt WhenHurt = SetStrucHurt();
	public StrucHurt DefaultWhenHurt = SetStrucHurt();

	static StrucHurt SetStrucHurt () {
		return new StrucHurt
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
	public struct StrucHurt
	{
		public int              invincibilityTime;
		public int              maxRingLoss;
		public float            knockbackUpwardsForce;
		public bool             shouldResetSpeedOnHit;
		public LayerMask        recoilFrom;
		public float            knockbackForce;
		public float            ringReleaseSpeed;
		public float            ringArcSpeed;
		public float            flickerSpeed;
		public float            hurtControlLock;
		public float            hurtControlLockAir;

	}
	#endregion

	#region rails
	//-------------------------------------------------------------------------------------------------

	public StrucRails RailStats = SetStrucRails();
	public StrucRails DefaultRailStats = SetStrucRails();

	static StrucRails SetStrucRails () {
		return new StrucRails
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
	public struct StrucRails
	{
		public float            railMaxSpeed;
		public float            railTopSpeed;
		public float            railDecaySpeedHigh;
		public float            railDecaySpeedLow;
		public float            MinStartSpeed;
		public float            RailPushFowardmaxSpeed;
		public float            RailPushFowardIncrements;
		public float            RailPushFowardDelay;
		public float            RailSlopePower;
		public float            RailUpHillMultiplier;
		public float            RailDownHillMultiplier;
		public float            RailUpHillMultiplierCrouching;
		public float            RailDownHillMultiplierCrouching;
		public float            RailDragVal;
		public float            RailPlayerBrakePower;
		public float            hopDelay;
		public float            hopSpeed;
		public float            hopDistance;
		public AnimationCurve   RailAccelerationBySpeed;
		public float            railBoostDecaySpeed;
		public float            railBoostDecayTime;

	}

	public StrucPositionOnRail RailPosition = SetStrucPositionOnRail();
	public StrucPositionOnRail DefaultRailPosition = SetStrucPositionOnRail();

	static StrucPositionOnRail SetStrucPositionOnRail () {
		return new StrucPositionOnRail
		{
			offsetRail = 1.3f,
			offsetZip = -6.7f,
			upreel = 0.3f
		};
	}


	[System.Serializable]
	public struct StrucPositionOnRail
	{
		public float    offsetRail;
		public float    offsetZip;
		public float    upreel;

	}
	#endregion

	#region WallRules
	//-------------------------------------------------------------------------------------------------

	public StrucWallRunning WallRunningStats = SetWallRunning();
	public StrucWallRunning DefaultWallRunningStats = SetWallRunning();

	static StrucWallRunning SetWallRunning () {
		return new StrucWallRunning
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
	public struct StrucWallRunning
	{
		public float            wallCheckDistance;
		public float            minHeight;
		public LayerMask        WallLayerMask;
		public float            wallDuration;
		public float            scrapeModifier;
		public float            climbModifier;

	}


	public S_O_CustomInspectorStyle InspectorTheme;

}
#endregion

#if UNITY_EDITOR
[CustomEditor(typeof(S_O_CharacterStats))]
public class S_O_CharacterStatsEditor : Editor
{
	

	public override void OnInspectorGUI () {
		//base.OnInspectorGUI();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("InspectorTheme"), new GUIContent("Inspector Theme"));
		serializedObject.ApplyModifiedProperties();


		//Setting variables
		S_O_CharacterStats stats = (S_O_CharacterStats)target;

		if (stats.InspectorTheme == null) { return; }
		GUIStyle headerStyle = stats.InspectorTheme._MainHeaders;
		

		GUIStyle ResetToDefaultButton = stats.InspectorTheme._DefaultButton;


		//Will only happen if above is attatched.
		if (stats == null) return;

		serializedObject.Update();

		//Start Tite and description
		stats.Title = EditorGUILayout.TextField(stats.Title);

		EditorGUILayout.TextArea("This objects contains a bunch of stats you can change to adjust how the character controls. \n" +
		"Feel free to copy and paste at your leisure, or input your own. \n" +
		"Every stat is organised in Strucs relevant to its purpose, and hovering over one will display a tooltip describing its function. \n" +
		"The tooltip will also say if it's a core stat (meaning it has large effects on the controller and should be changed with care), or a surface stat " +
		"(meaning it can easily be changed without much damage, and is ideal for making different character control different in the same playstyle). \n" +
		"At the botton are a number of buttons to reset any Struc to whatever is Set as the default.", EditorStyles.textArea);

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
		DrawHoming();
		DrawJumpDash();
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
		DrawWhenHurt();

		//Speeds
		#region Speeds
		//Speeds
		void DrawProperty ( string property, string outputName ) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
		}


		void DrawSpeed () {
			EditorGUILayout.Space();
			DrawProperty("SpeedStats", "Speeds");

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
			DrawProperty("AccelerationStats", "Acceleration");

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
			DrawProperty("DecelerationStats", "Deceleration");

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
			DrawProperty("TurningStats", "Turning");

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
			DrawProperty("SlopeStats", "On Slopes");

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
			DrawProperty("StickToGround", "Sticking To the Ground");
			DrawProperty("GreedysStickToGround", "Sticking to the Ground (Greedy's Version)");

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
		#region InAir
		void DrawAir () {
			EditorGUILayout.Space();
			DrawProperty("WhenInAir", "Air Control");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.WhenInAir = stats.DefaultWhenInAir;
			}
			serializedObject.ApplyModifiedProperties();
		}
		#endregion

		//Rolling
		#region Rolling
		void DrawRolling () {
			EditorGUILayout.Space();
			DrawProperty("RollingStats", "Rolling");

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
			DrawProperty("SkiddingStats", "Skidding");

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
			DrawProperty("JumpStats", "Jumps");

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
			DrawProperty("MultipleJumpStats", "Additional Jumps");

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
			DrawProperty("QuickstepStats", "Quickstep");

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
			DrawProperty("SpinChargeStats", "Spin Charge");

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
			DrawProperty("HomingStats", "Homing Attack");
			DrawProperty("HomingSearch", "Homing Targetting");

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
			DrawProperty("BounceStats", "Bounce");

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
			DrawProperty("RingRoadStats", "Ring Road");

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
			DrawProperty("DropChargeStats", "Drop Charge");

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
		void DrawJumpDash() {
			EditorGUILayout.Space();
			DrawProperty("JumpDashStats", "Jump Dash");

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
			DrawProperty("EnemyInteraction", "Interacting with Enemies");

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
			DrawProperty("ItemPulling", "Pulling in Items");

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
			DrawProperty("WhenBonked", "Bonking");

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
		void DrawWhenHurt () {
			EditorGUILayout.Space();
			DrawProperty("WhenHurt", "When Hurt");

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
			DrawProperty("RailStats", "Rail Grinding");

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
			DrawProperty("RailPosition", "Position on Rails");

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
			DrawProperty("WallRunningStats", "Wall Running");

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
