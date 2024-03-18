using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//[CreateAssetMenu(fileName = "Character X Stats")]
public class S_O_CharacterStats : ScriptableObject
{
	[HideInInspector] public string Title = "Title";

	#region acceleration
	//-------------------------------------------------------------------------------------------------

	public StrucAcceleration BasicAccelerationStats = SetStrucAcceleration();
	public StrucAcceleration AccelerationStats = SetStrucAcceleration();

	static StrucAcceleration SetStrucAcceleration () {
		return new StrucAcceleration
		{
			runAcceleration = 0.16f,
			rollAccel = 0.02f,
			AccelBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 3),
				new Keyframe(0.22f, 2.5f),
				new Keyframe(0.6f, 0.1f),
				new Keyframe(1f, 0.02f),
			}),
			angleToAccelerate = 120,
		};
	}

	[System.Serializable]
	public struct StrucAcceleration
	{
		[Tooltip("Surface: Decides the average acceleration when running or in the air. How much speed to be added per frame.")]
		public float              runAcceleration;
		[Tooltip("Surface: Decides the average acceleration when curled in a ball on the ground. How much speed to be added per frame")]
		public float              rollAccel;
		[Tooltip("Core: Decides how much of the acceleration values to accelerate by based on current running speed by Top Speed (not max speed)")]
		public AnimationCurve     AccelBySpeed;
		[Tooltip("Core: Decides how much of the acceleration values to accelerate by based on y normal of current slope. 0 = horizontal wall. ")]
		public AnimationCurve         AccelBySlopeAngle;
		[Tooltip("Core: If the angle between current direction and input direction is greater than this, then the player will not gain speed")]
		public float                    angleToAccelerate;

	}
	#endregion

	#region turning
	//-------------------------------------------------------------------------------------------------
	public StrucTurning BasicTurningStats = SetStrucTurning();
	public StrucTurning TurningStats = SetStrucTurning();

	static StrucTurning SetStrucTurning () {
		return new StrucTurning
		{
			turnDrag = 7.5f,
			turnSpeed = 14f,
			TurnRateByAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0),
				new Keyframe(0.05f, 0.05f),
				new Keyframe(0.21f, 0.23f),
				new Keyframe(0.5f, 0.5f),
				new Keyframe(0.75f, 0.5f),
				new Keyframe(0.99f, 0.36f),
				new Keyframe(1f, 0f),
			}),
			TurnRateBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1f),
				new Keyframe(0.12f, 1f),
				new Keyframe(0.25f, 0.85f),
				new Keyframe(0.73f, 0.85f),
				new Keyframe(0.78f, 0.75f),
				new Keyframe(1f, 0.7f),
			}),
			DragByAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0f),
				new Keyframe(0.15f, 0.02f),
				new Keyframe(0.45f, 0.06f),
				new Keyframe(0.75f, 0.08f),
				new Keyframe(0.85f, 0.14f),
				new Keyframe(1f, 0.22f),
			}),
			DragBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0f),
				new Keyframe(0.4f, 0.02f),
				new Keyframe(0.55f, 0.2f),
				new Keyframe(0.8f, 0.6f),
				new Keyframe(1, 0.6f),
			}),
			TurnRateByInputChange = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1.2f),
				new Keyframe(0.2f, 1.2f),
				new Keyframe(0.5f, 1),
				new Keyframe(1, 1f),
			}),
		};
	}

	[System.Serializable]
	public struct StrucTurning
	{
		[Tooltip("Surface : Decides how fast the character will turn. Core calculations are applied to this number, but it can easily be changed. How many degrees to change per frame")]
		public float              turnSpeed;
		[Tooltip("Surface : Decides how much speed will be lost when turning. Calculations are applied to this, but it can easily be changed. How much speed to lose per frame")]
		public float              turnDrag;
		[Tooltip("Core : Multiplies the turn speed based on the the angle difference between input direction and moving direction")]
		public AnimationCurve     TurnRateByAngle;
		[Tooltip("Core : Multiplies the turn speed based on the the current speed divided by max speed.")]
		public AnimationCurve     TurnRateBySpeed;
		[Tooltip("Core : Multiplies the turn speed based on how different the actual controller input is compared to last frame. This is to allow turning speed to be different if the camera is changing rather than the controller input.")]
		public AnimationCurve     TurnRateByInputChange;
		[Tooltip("Core : Multiplies the speed loss when turning by the angle difference between input direction and moving direction")]
		public AnimationCurve     DragByAngle;
		[Tooltip("Core : Multiplies the speed loss when turning based on the the current speed divided by max speed.")]
		public AnimationCurve     DragBySpeed;
	}
	#endregion

	#region Deceleration
	//-------------------------------------------------------------------------------------------------
	public StrucDeceleration BasicDecelerationStats = SetStrucDeceleration();
	public StrucDeceleration DecelerationStats = SetStrucDeceleration();

	static StrucDeceleration SetStrucDeceleration () {
		return new StrucDeceleration
		{
			moveDeceleration = 1.05f,
			airDecel = 1.25f,
			DecelBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1.005f),
				new Keyframe(0.2f, 1.002f),
				new Keyframe(0.5f, 1.01f),
				new Keyframe(0.7f, 1.03f),
				new Keyframe(1f, 1.1f),
			}),
			rollingFlatDecell = 1.004f,
			naturalAirDecel = 1.002f
		};
	}

	[System.Serializable]
	public struct StrucDeceleration
	{
		[Tooltip("Surface : Decides how fast the player will lose speed when not inputing on the ground. How much speed to lose per frame.")]
		public float              moveDeceleration;
		[Tooltip("Surface : Decides how fast the player will lose speed when not inputing in the air. How much speed to lose per frame.")]
		public float              airDecel;
		[Tooltip("Surface: Decides how fast the player will lose speed when rolling and not on a slope, even if there is an input. Applied against the roll acceleration.")]
		public float                  rollingFlatDecell;
		[Tooltip("Core : Multiplies the deceleration this frame, based on the current speed divided by Max speed.")]
		public AnimationCurve     DecelBySpeed;
		[Tooltip("Surface : Decides how much horizontal speed the player will lose for each frame in the air. Stacks with other decelerations")]
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
		[Tooltip("Surface : The highest speed the player can reach normally. By just running on flat ground they cannot exceed this. If they do via actions, slopes or other means, then their acceleration will not increase their speed further.")]
		public float    topSpeed;
		[Tooltip("Surface : The highest speed the player can ever reach. Certain environmental pieces may increase speed past this, but this is not part of character movement, and will not last long.")]
		public float    maxSpeed;
	}
	#endregion

	#region slopes
	//-------------------------------------------------------------------------------------------------

	public StrucSlopes BasicSlopeStats = SetStrucSlopes();
	public StrucSlopes SlopeStats = SetStrucSlopes();

	static StrucSlopes SetStrucSlopes () {
		return new StrucSlopes
		{
			slopeEffectLimit = 0.85f,
			SpeedLimitBySlopeAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(-1, 15f),
				new Keyframe(0f, 10f),
				new Keyframe(0.6f, -10f),
				new Keyframe(1f, -10f),
			}),
			generalHillMultiplier = 1.0f,
			uphillMultiplier = 0.6f,
			downhillMultiplier = 0.5f,
			downhillThreshold = -1.7f,
			uphillThreshold = 0.1f,
			SlopePowerByCurrentSpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1f),
				new Keyframe(1f, 0.5f),
			}),
			UpHillEffectByTime = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0.6f),
				new Keyframe(1f, 1f),
				new Keyframe(4f, 1.2f),
				new Keyframe(10f, 1.3f),
			}),
			landingConversionFactor = 2,
		};
	}

	[System.Serializable]
	public struct StrucSlopes
	{
		[Tooltip("Core: If the y normal of the floor is below this, then the player is considered on a slope. 1 = flat ground, as it's pointing straight up.")]
		public float              slopeEffectLimit;
		[Tooltip("Core: Decides if the player will fall off the slope. If their speed is under what this curve has for the current normal's y value, then they fall off.")]
		public AnimationCurve     SpeedLimitBySlopeAngle;
		[Tooltip("Surface : Sets the overall force power of slopes.")]
		public float              generalHillMultiplier;
		[Tooltip("Surface : Multiplied with the force of a slope when going uphill to determine the force against.")]
		public float              uphillMultiplier;
		[Tooltip("Surface : Multiplied with the force of a slope when going downhill to determine the force for.")]
		public float              downhillMultiplier;
		[Tooltip("Core : The speed the player should be moving downwards when grounded on a slope to be considered going downhill.")]
		public float              downhillThreshold;
		[Tooltip("Core : The speed the player should be moving upwards when grounded on a slope to be considered going uphill.")]
		public float              uphillThreshold;
		[Tooltip("Core : Determines the power of the slope by current speed divided by max. ")]
		public AnimationCurve     SlopePowerByCurrentSpeed;
		[Tooltip("Core: Determines the power of the slope when going uphill, based on how long has been spent going uphill since they were last going downhill or on flat ground.")]
		public AnimationCurve     UpHillEffectByTime;
		[Tooltip("Core: Amount of force gained when landing on a slope.")]
		public float                  landingConversionFactor;
	}
	#endregion


	#region sticking
	//-------------------------------------------------------------------------------------------------

	public StrucStickToGround DefaultStickToGround = SetStrucGreedyStick();
	public StrucStickToGround GreedysStickToGround = SetStrucGreedyStick();

	static StrucStickToGround SetStrucGreedyStick () {
		return new StrucStickToGround
		{
			stickingLerps = new Vector2(0.885f, 1.005f),
			stickingNormalLimit = 0.5f,
			stickCastAhead = 1.9f,
			negativeGHoverHeight = 0.05f,
			rotationResetThreshold = -0.1f,
			upwardsLimitByCurrentSlope = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0.2f, 0.5f),
				new Keyframe(0.85f, 0.3f),
				new Keyframe(1f, 0.2f),
			}),
			stepHeight = 0.75f,
		};
	}

	[System.Serializable]
	public struct StrucStickToGround
	{
		[Tooltip("Core:  The lerping from current velocity to velocity aligned to the current slope. X is negative slopes (loops), and Y is positive Slopes (imagine running on the outside of a loop). ")]
		public Vector2      stickingLerps;
		[Range(0, 1)]
		[Tooltip("Core: The maximum difference betwen current ground angle and movement direction that allows the player to stick. 0 means the player can't stick to the ground, 1 is everything bellow 180° difference, and 0.5 is 90° angles")]
		public float        stickingNormalLimit;
		[Tooltip("Core: The cast ahead to check for slopes to align to. Multiplied by the movement this frame. Too much of this value might send the player flying off before it hits the loop, too little might see micro stutters, default value 1.9")]
		public float        stickCastAhead;
		[Tooltip("Core: This is the position above the raycast hit point that the player will be placed if they are loosing grip.")]
		public float        negativeGHoverHeight;
		[Tooltip("Core: If the y value of the player's relative up direction is less than this (-1 is fully upside down) when in the air, then they will rotate sideways to face back up, rather than the conventonal rotation approach. This keeps them facing in their movement direction.")]
		public float        rotationResetThreshold;
		[Tooltip("Core: When lerping up negative slopes, if the difference between the two is under this, then will lerp up it, otherwise it is seen as a wall, not a slope. The value is based on the normal y of the current slope, so running on a horizonal wall can have a different difference limit to running on flat ground.")]
		public AnimationCurve    upwardsLimitByCurrentSlope;
		[Range (0, 1.5f)]
		[Tooltip("Core: The maximum height a wall infront can be to be considered a step to move up onto when running.")]
		public float        stepHeight;
	}

	public StrucFindGround DefaultFindGround = SetStrucFindGround();
	public StrucFindGround FindingGround = SetStrucFindGround();

	static StrucFindGround SetStrucFindGround () {
		return new StrucFindGround
		{
			GroundMask = new LayerMask(),
			rayToGroundDistance = 1.4f,
			raytoGroundSpeedRatio = 0.01f,
			raytoGroundSpeedMax = 2.6f,
			groundDifferenceLimit = 0.3f,
		};
	}

	[System.Serializable]
	public struct StrucFindGround
	{
		[Tooltip("Core: The layer an object must be set to, to be considered ground the player can run on.")]
		public LayerMask    GroundMask;
		[Range(0, 1)]
		[Tooltip("Core: The maximum difference between the current rotation and floor when checking if grounded. If the floor is too different to this then won't be grounded. 1 = 180 degrees")]
		public float        groundDifferenceLimit;
		[Tooltip("Core: The max range downwards of the ground checker.")]
		public float        rayToGroundDistance;
		[Tooltip("Core: Adds current horizontal speed multiplied by this to the ground checker when running along the ground. Combats how it's easier to lose when at higher speed.")]
		public float        raytoGroundSpeedRatio;
		[Tooltip("Core: The maximum range the ground checker can reach when increased by the above ratio.")]
		public float        raytoGroundSpeedMax;
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
			shouldStopAirMovementWhenNoInput = true,
			upGravity = new Vector3(0f, -1.45f, 0),
			keepNormalForThis = 0.183f,
			controlAmmount = new Vector2(0.7f, 0.8f),
			fallGravity = new Vector3(0, -1.5f, 0)
		};
	}

	[System.Serializable]
	public struct StrucInAir
	{
		[Tooltip("Surface: The maximum speed the player can ever be moving downwards when not grounded.")]
		public float    startMaxFallingSpeed;
		[Tooltip("Core: Whether or not the player should decelerate when there's no input in the air.")]
		public bool               shouldStopAirMovementWhenNoInput;
		[Tooltip("Core: How long to keep rotation relative to ground after losing it.")]
		public float    keepNormalForThis;
		[Tooltip("Surface: X is multiplied with the turning speed when in the air, Y is multiplied with acceleration when in the air.")]
		public Vector2   controlAmmount;
		[Tooltip("Surface: Force to add onto the player per frame when in the air and falling downwards.")]
		public Vector3  fallGravity;
		[Tooltip("Surface: Force to add onto the player per frame when in the air but currently moving upwards (such as launched by a slope)")]
		public Vector3  upGravity;
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
			minRollingTime = 0.3f,
			rollingUphillBoost = 1.2f,
			rollingStartSpeed = 5f,
			rollingTurningModifier = 0.6f,
		};
	}



	[System.Serializable]
	public struct StrucRolling
	{
		[Tooltip("Core: Multiplied by landing conversion factor to gain more force when landing on a slope and immediately rolling.")]
		public float    rollingLandingBoost;
		public float    minRollingTime;
		[Tooltip("Core: Multiplies force for when rolling downhill..")]
		public float    rollingDownhillBoost;
		[Tooltip("Core: Multiplies force against when rolling uphill")]
		public float    rollingUphillBoost;
		[Tooltip("Core: Minimum speed to be at to start rolling.")]
		public float    rollingStartSpeed;
		[Tooltip("Core: When rolling, multiplied by turn speed.")]
		public float    rollingTurningModifier;
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
			shouldSkiddingDisableTurning = true,
			angleToPerformSkid = 160,
			angleToPerformHomingSkid = 130,
			skiddingIntensity = -5,
			canSkidInAir = true,
			skiddingIntensityInAir = -2.5f,
		};
	}


	[System.Serializable]
	[Tooltip("Surface: Skidding is when the player holds inputs against their movement direction to quickly slow down.")]
	public struct StrucSkidding
	{
		[Tooltip("Surface: Immediately stop if skidding while under this speed.")]
		public float	speedToStopAt;
		[Tooltip("Surface: Whether the player can change their direction while skidding")]
		public bool	shouldSkiddingDisableTurning;
		[Tooltip("Surface: How precise the angle has to be against the character's movement. E.G. a value of 160 means the player's input should be between a 160 and 180 degrees angle from movement.")]
		public int	angleToPerformSkid;
		public int	angleToPerformHomingSkid;
		[Range(-100, 0)]
		[Tooltip("Surface: How much force to apply against the character per frame as they skid on the ground.")]
		public float	skiddingIntensity;
		[Tooltip("Surface: Whehter or not the player can perform a skid while airborn.")]
		public bool	canSkidInAir;
		[Range(-100, 0)]
		[Tooltip("Surface: How much force to apply against the character per frame as they skid in the air.")]
		public float	skiddingIntensityInAir;
	}

	#endregion

	#region Jumps
	//-------------------------------------------------------------------------------------------------

	public StrucJumps JumpStats = SetStrucJumps();
	[HideInInspector] public StrucJumps BasicJumpStats = SetStrucJumps();

	static StrucJumps SetStrucJumps () {
		return new StrucJumps
		{
			CoyoteTimeBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0.175f),
				new Keyframe(0.05f, 0.28f),
				new Keyframe(0.25f, 0.3f),
				new Keyframe(1f, 0.42f),
				new Keyframe(1.5f, 0.55f),
			}),
			jumpSlopeConversion = 0.03f,
			stopYSpeedOnRelease = 2.1f,
			jumpRollingLandingBoost = 0f,
			startJumpDuration = new Vector2 (0.15f, 0.25f),
			startSlopedJumpDuration = 0.2f,
			startJumpSpeed = 4f,
			speedLossOnJump = 0.99f,
			jumpExtraControlThreshold = 0.4f,
			jumpAirControl = new Vector2(1.3f, 1.1f)
		};
	}


	[System.Serializable]
	public struct StrucJumps
	{
		public AnimationCurve CoyoteTimeBySpeed;
		public float    jumpSlopeConversion;
		public float    stopYSpeedOnRelease;
		public float    jumpRollingLandingBoost;
		public Vector2    startJumpDuration;
		public float    startSlopedJumpDuration;
		public float    startJumpSpeed;
		public float    speedLossOnJump;
		public float      jumpExtraControlThreshold;
		public Vector2      jumpAirControl;
	}
	#endregion

	#region multiJumps
	//-------------------------------------------------------------------------------------------------
	public StrucMultiJumps MultipleJumpStats = SetStrucMultiJumps();
	public StrucMultiJumps DefaultMultipleJumpStats = SetStrucMultiJumps();

	static StrucMultiJumps SetStrucMultiJumps () {
		return new StrucMultiJumps
		{
			maxJumpCount = 2,
			doubleJumpSpeed = 4.5f,
			doubleJumpDuration = 0.14f,
			speedLossOnDoubleJump = 0.978f
		};
	}



	[System.Serializable]
	public struct StrucMultiJumps
	{
		public bool         canDoubleJump;
		public bool         canTripleJump;
		public int          maxJumpCount;

		public float        doubleJumpSpeed;
		public float        doubleJumpDuration;
		public float        speedLossOnDoubleJump;
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
		public float        stepSpeed;
		public float        stepDistance;
		public float        airStepSpeed;
		public float        airStepDistance;
		public LayerMask    StepLayerMask;

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
			rangeInCameraDirection = 1.2f,
			minimumTargetDistance = 15,
			maximumTargetDistance = 80,
			TargetLayer = new LayerMask(),
			blockingLayers = new LayerMask(),

			iconScale = 1.5f,
			iconDistanceScaling = 0.2f,
			facingAmount = 0.91f,
			currentTargetPriority = 0.4f,
			timeToKeepTarget = new Vector2 (0.2f, 0.3f),
			timeBetweenScans = 0.12f,
			radiusOfCameraTargetCheck = 15,
			cameraDirectionPriority = 0.5f,
		};
	}

	[System.Serializable]
	public struct StrucHomingSearch
	{
		public float                  targetSearchDistance;
		public float                  rangeInCameraDirection;
		public int                    minimumTargetDistance;
		public int                    maximumTargetDistance;
		public LayerMask              TargetLayer;
		public LayerMask              blockingLayers;
		public float                  iconScale;
		public float                  iconDistanceScaling;
		public float                  facingAmount;
		[Range(0f, 1f)]
		public float		currentTargetPriority;
		public Vector2                timeToKeepTarget;
		public float                  timeBetweenScans;
		public int                    radiusOfCameraTargetCheck;
		[Range(0f, 1f)]
		public float                  cameraDirectionPriority;

	}

	public StrucHomingAction HomingStats = SetStrucHomingAction();
	public StrucHomingAction DefaultHomingStats = SetStrucHomingAction();

	static StrucHomingAction SetStrucHomingAction () {
		return new StrucHomingAction
		{
			canBePerformedOnGround = false,
			canBeControlled = true,
			canDashWhenFalling = true,
			attackSpeed = 100f,
			maximumSpeed = 140,
			minimumSpeed = 60,
			minimumSpeedOnHit = 60,
			timerLimit = 1f,
			successDelay = 0.3f,
			turnSpeed = 0.8f,
			lerpToNewInputOnHit = 0.5f,
			lerpToPreviousDirectionOnHit = 0,
			deceleration = 55,
			acceleration = 70,
			homingCountLimit = 0,

		};
	}

	[System.Serializable]
	public struct StrucHomingAction
	{
		public bool         canBePerformedOnGround;
		public bool         canBeControlled;
		public bool         canDashWhenFalling;
		public float	attackSpeed;
		public int          maximumSpeed;
		public int          minimumSpeed;
		public int          minimumSpeedOnHit;
		public float	timerLimit;
		public float	successDelay;
		public float	turnSpeed;
		public int	deceleration;
		public int	acceleration;
		[Range(0, 1)]
		public float        lerpToPreviousDirectionOnHit;
		[Range(0, 1)]
		public float        lerpToNewInputOnHit;
		[Range(0, 10)]
		public int          homingCountLimit;
	}

	#endregion



	#region spin Charge
	//-------------------------------------------------------------------------------------------------

	public StrucSpinCharge SpinChargeStat = SetStrucSpinCharge();
	public StrucSpinCharge DefaultSpinChargeStat = SetStrucSpinCharge();

	static StrucSpinCharge SetStrucSpinCharge () {
		return new StrucSpinCharge
		{
			whatAimMethod = S_Enums.SpinChargeAiming.Camera,
			chargingSpeed = 1.02f,
			tappingBonus = 2.5f,
			delayBeforeLaunch = 20,
			minimunCharge = 20f,
			maximunCharge = 110f,
			forceAgainstMovement = 0.015f,
			shouldSetRolling = true,
			maximumSpeedPerformedAt = 200f,
			maximumSlopePerformedAt = -1f,
			releaseShakeAmmount = 1.5f,
			SpeedLossByTime = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0f, 0.1f),
				new Keyframe(0.4f, 0.25f),
				new Keyframe(1f, 0.35f),
			}),
			ForceGainByAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(-1, 2f),
				new Keyframe(-0.85f, 2f),
				new Keyframe(-0.65f, 1.5f),
				new Keyframe(0f, 1.2f),
				new Keyframe(0.3f, 1.05f),
				new Keyframe(1f, 1f),
			}),
			ForceGainByCurrentSpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1.2f),
				new Keyframe(0.05f, 1.2f),
				new Keyframe(0.15f, 1f),
				new Keyframe(0.3f, 1f),
				new Keyframe(0.55f, 0.4f),
				new Keyframe(0.75f, 0.15f),
				new Keyframe(1f, 0f),
			}),
			LerpRotationByAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1f),
				new Keyframe(0.1f, 1f),
				new Keyframe(0.15f, 0.85f),
				new Keyframe(0.25f, 0.7f),
				new Keyframe(0.65f, 0.7f),
				new Keyframe(0.85f, 0.9f),
				new Keyframe(1f, 0.9f),
			}),
			angleToPerformSkid = 10f,
			skidIntesity = 3f
		};
	}

	[System.Serializable]
	public struct StrucSpinCharge
	{
		public S_Enums.SpinChargeAiming whatAimMethod;
		public float              chargingSpeed;
		public float                  tappingBonus;
		public int                    delayBeforeLaunch;
		public float              minimunCharge;
		public float              maximunCharge;
		public float              forceAgainstMovement;
		public bool                   shouldSetRolling;
		public float              maximumSpeedPerformedAt; //The max amount of speed you can be at to perform a Spin Dash
		public float              maximumSlopePerformedAt; //The highest slope you can be on to Spin Dash
		public float              releaseShakeAmmount;
		public AnimationCurve     SpeedLossByTime;
		public AnimationCurve     ForceGainByAngle;
		public AnimationCurve     LerpRotationByAngle;
		public AnimationCurve     ForceGainByCurrentSpeed;
		public float              angleToPerformSkid;
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
			bounceHaltFactor = 0.7f,
			bounceAirControl = new Vector2(1.4f, 1.1f),
			horizontalSpeedDecay = new Vector2(0.1f, 0.001f),

			listOfBounceSpeeds = new List<float> { 40f, 42f, 44f },
			minimumPushForce = 30,
			bounceUpMaxSpeed = 75f,
			lerpTowardsInput = 0.5f,

			bounceCoolDown = 8f,
			coolDownModiferBySpeed = 0.005f,
		};
	}


	[System.Serializable]
	public struct StrucBounce
	{
		[Header("Movement")]
		public float              dropSpeed;
		public float              bounceHaltFactor;
		public Vector2                horizontalSpeedDecay;
		public Vector2      bounceAirControl;
		[Header("Bounces")]
		public List<float>        listOfBounceSpeeds;
		public float              bounceUpMaxSpeed;
		public float                  minimumPushForce;
		public float                  lerpTowardsInput;
		[Header("Cooldown")]
		public float              bounceCoolDown;
		public float              coolDownModiferBySpeed;

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
			damageShakeAmmount = 0.5f,
			hitShakeAmmount = 1.2f,
		};
	}



	[System.Serializable]
	public struct StrucEnemyInteract
	{
		public float		bouncingPower;
		public float		homingBouncingPower;
		public float		enemyHomingStoppingPowerWhenAdditive;
		public bool		shouldStopOnHomingAttackHit;
		public bool		shouldStopOnHit;
		public float		damageShakeAmmount;
		public float		hitShakeAmmount;
	}
	#endregion

	#region pull Items
	//-------------------------------------------------------------------------------------------------
	public StrucItemPull ItemPulling = SetStrucItemPull();
	public StrucItemPull BasicItemPulling = SetStrucItemPull();

	static StrucItemPull SetStrucItemPull () {
		return new StrucItemPull
		{
			RadiusBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1.3f),
				new Keyframe(0.15f, 1.3f),
				new Keyframe(0.25f, 4f),
				new Keyframe(0.7f, 7f),
				new Keyframe(1f, 9f),
			}),
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
			bonkControlLockAir = 40f,
			bonkTime = 100
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
		public int                    bonkTime;

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
			ringReleaseSpeed = 550f,
			respawnAfter = new Vector3(90, 120, 170),
			ringArcSpeed = 250f,
			flickerTimes = new Vector2 (5, 10),
			RingsLostInSpawnByAmount = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1f),
				new Keyframe(30f, 2f),
				new Keyframe(50f, 3f),
				new Keyframe(75, 4f),
				new Keyframe(100f, 5f),
			}),
		};
	}


	[System.Serializable]
	public struct StrucHurt
	{
		public int              invincibilityTime;
		public int              maxRingLoss;
		public float            ringReleaseSpeed;
		public float            ringArcSpeed;
		public Vector2            flickerTimes;
		public Vector3                respawnAfter;
		public AnimationCurve         RingsLostInSpawnByAmount;

	}

	public StrucRebound KnockbackStats = SetStrucRebound();
	public StrucRebound DefaultKnockBackStats = SetStrucRebound();

	static StrucRebound SetStrucRebound () {
		return new StrucRebound
		{
			whatResponse = S_Enums.HurtResponse.Normal,
			knockbackUpwardsForce = 30f,
			recoilFrom = new LayerMask(),
			knockbackForce = 25f,
			hurtControlLock = 15f,
			hurtControlLockAir = 30f,
			stateLengthWithKnockback = 130,
			stateLengthWithoutKnockback = 90,
		};
	}

	[System.Serializable]
	public struct StrucRebound
	{
		public S_Enums.HurtResponse whatResponse;
		public float            knockbackUpwardsForce;
		public LayerMask        recoilFrom;
		public float            knockbackForce;
		public float            hurtControlLock;
		public float            hurtControlLockAir;
		public int         stateLengthWithKnockback;
		public int         stateLengthWithoutKnockback;

	}
	#endregion

	#region rails
	//-------------------------------------------------------------------------------------------------

	public StrucRails RailStats = SetStrucRails();
	public StrucRails BasicRailStats = SetStrucRails();

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
			RailAccelerationBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 3.5f),
				new Keyframe(0.12f, 3.5f),
				new Keyframe(0.25f, 1.82f),
				new Keyframe(0.825f, 1f),
				new Keyframe(1f, 1.2f),
			}),
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

	#region objects
	public StrucInteractions ObjectInteractions = SetStrucInteractions ();
	public StrucInteractions DefaultObjectInteractions = SetStrucInteractions ();

	static StrucInteractions SetStrucInteractions () {
		return new StrucInteractions
		{
			UpreelSpeedKeptAfter = 0.5f,
		};
	}


	[System.Serializable]
	public struct StrucInteractions
	{
		[Range(0,1)]
		public float UpreelSpeedKeptAfter;

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
	S_O_CharacterStats stats;
	GUIStyle headerStyle;
	GUIStyle ResetToDefaultButton;

	public override void OnInspectorGUI () {
		DrawInspector();
	}
	private void OnEnable () {
		//Setting variables
		stats = (S_O_CharacterStats)target;

		if (stats.InspectorTheme == null) { return; }
		headerStyle = stats.InspectorTheme._MainHeaders;
		ResetToDefaultButton = stats.InspectorTheme._ResetButton;
	}

	private void DrawInspector () {

		EditorGUILayout.PropertyField(serializedObject.FindProperty("InspectorTheme"), new GUIContent("Inspector Theme"));
		serializedObject.ApplyModifiedProperties();

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
		DrawFindGround();
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
		DrawHomingSearch();
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
		DrawObjectInteraction();
		DrawItemPulling();
		DrawWhenBonked();
		DrawWhenHurt();
		DrawKnockback();

		void DrawProperty ( string property, string outputName ) {
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
		}

		//Speeds
		#region Speeds
		void DrawSpeed () {
			EditorGUILayout.Space();
			DrawProperty("SpeedStats", "Speeds");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.SpeedStats = stats.DefaultSpeedStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
				stats.AccelerationStats = stats.BasicAccelerationStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
				stats.DecelerationStats = stats.BasicDecelerationStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
				stats.TurningStats = stats.BasicTurningStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
				stats.SlopeStats = stats.BasicSlopeStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//Sticking
		#region Sticking
		void DrawSticking () {
			EditorGUILayout.Space();
			DrawProperty("GreedysStickToGround", "Sticking to the Ground (Greedy's Version)");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.GreedysStickToGround = stats.DefaultStickToGround;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//Sticking
		#region Ground
		void DrawFindGround () {
			EditorGUILayout.Space();
			DrawProperty("FindingGround", "Finding the ground");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.FindingGround = stats.DefaultFindGround;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
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
				stats.JumpStats = stats.BasicJumpStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
		}
		#endregion

		//Spin Charge
		#region SpinCharge
		void DrawSpinCharge () {
			EditorGUILayout.Space();
			DrawProperty("SpinChargeStat", "Spin Charge");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.SpinChargeStat = stats.DefaultSpinChargeStat;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//Homing
		#region Homing
		void DrawHoming () {
			EditorGUILayout.Space();
			DrawProperty("HomingStats", "Homing Attack");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.HomingStats = stats.DefaultHomingStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}

		void DrawHomingSearch () {
			EditorGUILayout.Space();
			DrawProperty("HomingSearch", "Homing Targetting");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.HomingSearch = stats.DefaultHomingSearch;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
		}
		#endregion

		//JumpDash
		#region JumpDash
		void DrawJumpDash () {
			EditorGUILayout.Space();
			DrawProperty("JumpDashStats", "Jump Dash");

			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.JumpDashStats = stats.DefaultJumpDashStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
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
				stats.ItemPulling = stats.BasicItemPulling;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
		}
		#endregion

		//Health      
		#region WhenHurt
		void DrawWhenHurt () {
			EditorGUILayout.Space();
			DrawProperty("WhenHurt", "Health interactions");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.WhenHurt = stats.DefaultWhenHurt;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//WhenHurt       
		#region Knockback
		void DrawKnockback () {
			EditorGUILayout.Space();
			DrawProperty("KnockbackStats", "Knockback");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.KnockbackStats = stats.DefaultKnockBackStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
				stats.RailStats = stats.BasicRailStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
		}
		#endregion

		//Object Interaction
		#region Object
		void DrawObjectInteraction () {
			EditorGUILayout.Space();
			DrawProperty("ObjectInteractions", "Interactions");

			Undo.RecordObject(stats, "set to defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.ObjectInteractions = stats.DefaultObjectInteractions;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
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
			GUILayout.EndHorizontal();
		}
		#endregion    
	}

}
#endif
