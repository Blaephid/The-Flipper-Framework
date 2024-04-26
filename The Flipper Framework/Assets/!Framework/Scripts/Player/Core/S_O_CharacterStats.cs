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

	public StrucAcceleration StartAccelerationStats = SetStrucAcceleration();
	public StrucAcceleration AccelerationStats = SetStrucAcceleration();

	static StrucAcceleration SetStrucAcceleration () {
		return new StrucAcceleration
		{
			runAcceleration = 0.8f,
			rollAccel = 0.4f,
			AccelBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 3),
				new Keyframe(0.22f, 2.5f),
				new Keyframe(0.6f, 0.1f),
				new Keyframe(1f, 0.02f),
			}),
			AccelBySlopeAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(-1, 0.2f),
				new Keyframe(-0.1f, 0),
				new Keyframe(0.1f, 0),
				new Keyframe(0.47f, 1),
				new Keyframe(1, 1)
			}),
			angleToAccelerate = 120,
		};
	}

	[System.Serializable]
	public struct StrucAcceleration
	{
		[Header("Base Values")]
		[Tooltip("Surface: Decides the average acceleration when running or in the air. How much speed to be added per frame.")]
		public float              runAcceleration;
		[Tooltip("Surface: Decides the average acceleration when curled in a ball on the ground. How much speed to be added per frame")]
		public float              rollAccel;
		[Tooltip("Core: If the angle between current direction and input direction is greater than this, then the player will not gain speed")]
		public float                    angleToAccelerate;
		[Header("Effected values")]
		[Tooltip("Core: Decides how much of the acceleration values to accelerate by based on current running speed by Top Speed (not max speed)")]
		public AnimationCurve     AccelBySpeed;
		[Tooltip("Core: Decides how much of the acceleration values to accelerate by based on y normal of current slope. 0 = horizontal wall. Speed will still be changed due to slope physics. ")]
		public AnimationCurve         AccelBySlopeAngle;

	}
	#endregion

	#region turning
	//-------------------------------------------------------------------------------------------------
	public StrucTurning StarTurningStats = SetStrucTurning();
	public StrucTurning TurningStats = SetStrucTurning();

	static StrucTurning SetStrucTurning () {
		return new StrucTurning
		{
			turnDrag = 0.7f,
			turnSpeed = 4f,
			TurnRateByAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0.3f),
				new Keyframe(0.07f, 0.7f),
				new Keyframe(0.5f, 1.3f),
				new Keyframe(0.8f, 1.5f),
				new Keyframe(0.95f, 0.2f),
				new Keyframe(1f, 0f),
			}),
			TurnRateBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 2f),
				new Keyframe(0.1f, 2f),
				new Keyframe(0.12f, 1f),
				new Keyframe(0.25f, 0.85f),
				new Keyframe(0.7f, 0.7f),
				new Keyframe(1f, 0.5f),
			}),
			DragByAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0f),
				new Keyframe(0.15f, 0.3f),
				new Keyframe(0.25f, 0.7f),
				new Keyframe(0.62f, 0.8f),
				new Keyframe(0.8f, 0.8f),
				new Keyframe(1f, 1.1f),
			}),
			DragBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0f),
				new Keyframe(0.2f, 0.02f),
				new Keyframe(0.4f, 0.5f),
				new Keyframe(0.8f, 0.6f),
				new Keyframe(1, 0.7f),
			}),
			TurnRateByInputChange = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 2f),
				new Keyframe(0.2f, 2f),
				new Keyframe(0.5f, 1),
				new Keyframe(1, 1f),
			}),
		};
	}

	[System.Serializable]
	public struct StrucTurning
	{
		[Header("Base values")]
		[Tooltip("Surface : Decides how fast the character will turn. Core calculations are applied to this number, but it can easily be changed. How many degrees to change per frame")]
		public float              turnSpeed;
		[Tooltip("Surface : Decides how much speed will be lost when turning. Calculations are applied to this, but it can easily be changed. How much speed to lose per frame")]
		public float              turnDrag;
		[Header("Effected Values")]
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
	public StrucDeceleration StartDecelerationStats = SetStrucDeceleration();
	public StrucDeceleration DecelerationStats = SetStrucDeceleration();

	static StrucDeceleration SetStrucDeceleration () {
		return new StrucDeceleration
		{
			moveDeceleration = 4,
			airManualDecel = 1.5f,
			DecelBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1.005f),
				new Keyframe(0.2f, 1.002f),
				new Keyframe(0.5f, 1.01f),
				new Keyframe(0.7f, 1.03f),
				new Keyframe(1f, 1.1f),
			}),
			rollingFlatDecell = 0.5f,
			airConstantDecel = 0.15f
		};
	}

	[System.Serializable]
	public struct StrucDeceleration
	{
		[Tooltip("Surface : Decides how fast the player will lose speed when not inputing on the ground. How much speed to lose per frame.")]
		public float              moveDeceleration;
		[Tooltip("Surface : Decides how fast the player will lose speed when not inputing in the air. How much speed to lose per frame.")]
		public float              airManualDecel;
		[Tooltip("Surface : Decides how much horizontal speed the player will lose for each frame in the air. Stacks with other decelerations")]
		public float              airConstantDecel;
		[Tooltip("Surface: Decides how fast the player will lose speed when rolling and not on a slope, even if there is an input. Applied against the roll acceleration.")]
		public float                  rollingFlatDecell;
		[Tooltip("Core : Multiplies the deceleration this frame, based on the current speed divided by Max speed.")]
		public AnimationCurve     DecelBySpeed;
	}
	#endregion

	#region speeds
	//-------------------------------------------------------------------------------------------------
	public StrucSpeeds StartSpeedStats = SetStrucSpeeds();
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

	public StrucSlopes StartSlopeStats = SetStrucSlopes();
	public StrucSlopes SlopeStats = SetStrucSlopes();

	static StrucSlopes SetStrucSlopes () {
		return new StrucSlopes
		{
			isUsingSlopePhysics = true,
			slopeEffectLimit = 0.92f,
			SpeedLimitBySlopeAngle = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(-1, 50f),
				new Keyframe(0f, 30f),
				new Keyframe(0.4f, 0f),
				new Keyframe(1f, 0f),
			}),
			generalHillMultiplier = 1.0f,
			uphillMultiplier = 0.3f,
			downhillMultiplier = 0.4f,
			downhillThreshold = -1.7f,
			uphillThreshold = 1f,
			SlopePowerByCurrentSpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1f),
				new Keyframe(1f, 0.9f),
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
		[Header("Limits")]
		public bool                   isUsingSlopePhysics;
		[Tooltip("Core: If the y normal of the floor is below this, then the player is considered on a slope. 1 = flat ground, as it's pointing straight up.")]
		public float              slopeEffectLimit;
		[Tooltip("Core: Decides if the player will fall off the slope. If their speed is under what this curve has for the current normal's y value, then they fall off.")]
		public AnimationCurve     SpeedLimitBySlopeAngle;
		[Header("Forces")]
		[Tooltip("Surface : Sets the overall force power of slopes.")]
		public float              generalHillMultiplier;
		[Tooltip("Surface : Multiplied with the force of a slope when going uphill to determine the force against.")]
		public float              uphillMultiplier;
		[Tooltip("Surface : Multiplied with the force of a slope when going downhill to determine the force for.")]
		public float              downhillMultiplier;
		[Tooltip("Core : Determines the power of the slope by current speed divided by max. ")]
		public AnimationCurve     SlopePowerByCurrentSpeed;
		[Tooltip("Core: Determines the power of the slope when going uphill, based on how long has been spent going uphill since they were last going downhill or on flat ground.")]
		public AnimationCurve     UpHillEffectByTime;
		[Header("Triggers")]
		[Tooltip("Core: Amount of force gained when landing on a slope.")]
		public float                  landingConversionFactor;
		[Tooltip("Core : The speed the player should be moving downwards when grounded on a slope to be considered going downhill.")]
		public float              downhillThreshold;
		[Tooltip("Core : The speed the player should be moving upwards when grounded on a slope to be considered going uphill.")]
		public float              uphillThreshold;
	}
	#endregion


	#region sticking
	//-------------------------------------------------------------------------------------------------

	public StrucStickToGround StartStickToGround = SetStrucGreedyStick();
	public StrucStickToGround GreedysStickToGround = SetStrucGreedyStick();

	static StrucStickToGround SetStrucGreedyStick () {
		return new StrucStickToGround
		{
			stickingLerps = new Vector2(0.98f, 1.02f),
			stickingNormalLimit = 0.519f,
			stickCastAhead = 1.7f,
			groundBuffer = 0.8f,
			rotationResetThreshold = -0.05f,
			upwardsLimitByCurrentSlope = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0f, 0.7f),
				new Keyframe(1f, 0.3f),
			}),
			stepHeight = 1f,
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
		[Tooltip("Core: The cast ahead to check for slopes to align to. Multiplied by the movement this frame. Too much of this value might send the player flying off before it hits the loop, too little might see micro stutters, Default value 1.9")]
		public float        stickCastAhead;
		[Tooltip("Core: This is the position above the raycast hit point that the player will be placed if they are loosing grip.")]
		public float        groundBuffer;
		[Tooltip("Core: If the y value of the player's relative up direction is less than this (-1 is fully upside down) when in the air, then they will rotate sideways to face back up, rather than the conventonal rotation approach. This keeps them facing in their movement direction.")]
		public float        rotationResetThreshold;
		[Tooltip("Core: When lerping up negative slopes, if the difference between the two is under this, then will lerp up it, otherwise it is seen as a wall, not a slope. The value is based on the normal y of the current slope, so running on a horizonal wall can have a different difference limit to running on flat ground.")]
		public AnimationCurve    upwardsLimitByCurrentSlope;
		[Range (0, 1.5f)]
		[Tooltip("Core: The maximum height a wall infront can be to be considered a step to move up onto when running.")]
		public float        stepHeight;
	}

	public StrucFindGround StartFindGround = SetStrucFindGround();
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
	public StrucInAir StartWhenInAir = SetStrucInAir();
	static StrucInAir SetStrucInAir () {
		return new StrucInAir
		{
			startMaxFallingSpeed = -400f,
			shouldStopAirMovementWhenNoInput = true,
			upGravity = new Vector3(0f, -1.45f, 0),
			keepNormalForThis = 0.4f,
			controlAmmount = new Vector2(0.6f, 0.8f),
			fallGravity = new Vector3(0, -1.5f, 0)
		};
	}

	[System.Serializable]
	public struct StrucInAir
	{
		[Header("Control")]
		[Tooltip("Surface: X is multiplied with the turning speed when in the air, Y is multiplied with acceleration when in the air.")]
		public Vector2   controlAmmount;
		[Tooltip("Core: Whether or not the player should decelerate when there's no input in the air.")]
		public bool               shouldStopAirMovementWhenNoInput;
		[Tooltip("Core: How long to keep rotation relative to ground after losing it.")]
		public float    keepNormalForThis;

		[Header("Falling")]
		[Tooltip("Surface: The maximum speed the player can ever be moving downwards when not grounded.")]
		public float    startMaxFallingSpeed;
		[Tooltip("Surface: Force to add onto the player per frame when in the air and falling downwards.")]
		public Vector3  fallGravity;
		[Tooltip("Surface: Force to add onto the player per frame when in the air but currently moving upwards (such as launched by a slope)")]
		public Vector3  upGravity;
	}

	#endregion

	#region rolling
	//-------------------------------------------------------------------------------------------------


	public StrucRolling RollingStats = SetStrucRolling();
	public StrucRolling StartRollingStats = SetStrucRolling();

	static StrucRolling SetStrucRolling () {
		return new StrucRolling
		{
			rollingLandingBoost = 1.4f,
			rollingDownhillBoost = 1.9f,
			minRollingTime = 0.4f,
			rollingUphillBoost = 1.2f,
			rollingStartSpeed = 5f,
			rollingTurningModifier = 0.6f,
		};
	}



	[System.Serializable]
	public struct StrucRolling
	{
		[Header("Effects")]
		[Tooltip("Core: Multiplies force for when rolling downhill..")]
		public float    rollingDownhillBoost;
		[Tooltip("Core: Multiplies force against when rolling uphill")]
		public float    rollingUphillBoost;
		[Tooltip("Core: Minimum speed to be at to start rolling.")]
		public float    rollingStartSpeed;
		[Tooltip("Core: When rolling, multiplies turn speed.")]
		public float    rollingTurningModifier;
		[Header("Interactions")]
		[Tooltip("Core: Multiplied by landing conversion factor to gain more force when landing on a slope and immediately rolling.")]
		public float    rollingLandingBoost;
		[Tooltip("Core: Can only exit a role after benn rolling for this many seconds.")]
		public float    minRollingTime;
	}

	#endregion

	#region skidding
	//-------------------------------------------------------------------------------------------------
	public StrucSkidding SkiddingStats = SetStrucSkidding();
	public StrucSkidding StartSkiddingStats = SetStrucSkidding();

	static StrucSkidding SetStrucSkidding () {
		return new StrucSkidding
		{
			speedToStopAt = 10,
			shouldSkiddingDisableTurning = true,
			angleToPerformSkid = 160,
			angleToPerformSpinSkid = 140,
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
		[Header("Interaction")]
		[Tooltip("Surface: How precise the angle has to be against the character's movement. E.G. a value of 160 means the player's input should be between a 160 and 180 degrees angle from movement.")]
		public int          angleToPerformSkid;
		public float                  angleToPerformSpinSkid;
		public int          angleToPerformHomingSkid;
		[Tooltip("Surface: Whehter or not the player can perform a skid while airborn.")]
		public bool         canSkidInAir;
		[Tooltip("Surface: Whether the player can change their direction while skidding")]
		public bool         shouldSkiddingDisableTurning;

		[Header("Effects")]
		[Range(-100, 0)]
		[Tooltip("Surface: How much force to apply against the character per frame as they skid on the ground.")]
		public float        skiddingIntensity;
		[Range(-100, 0)]
		[Tooltip("Surface: How much force to apply against the character per frame as they skid in the air.")]
		public float        skiddingIntensityInAir;
		[Tooltip("Surface: Immediately stop if skidding while under this speed.")]
		public float        speedToStopAt;
	}

	#endregion

	#region Jumps
	//-------------------------------------------------------------------------------------------------

	public StrucJumps JumpStats = SetStrucJumps();
	[HideInInspector] public StrucJumps StarJumpStats = SetStrucJumps();

	static StrucJumps SetStrucJumps () {
		return new StrucJumps
		{
			CoyoteTimeBySpeed = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0.1f),
				new Keyframe(0.25f, 0.15f),
				new Keyframe(1f, 0.2f),
				new Keyframe(1.5f, 0.25f),
			}),
			JumpForceByTime = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0,2f),
				new Keyframe(0.2f, 1),
				new Keyframe(0.4f, 1),
				new Keyframe(0.8f, 0.7f),
				new Keyframe(1, 0.5f),
			}),
			jumpSlopeConversion = 0.03f,
			jumpDuration = new Vector2(0.15f, 0.25f),
			startSlopedJumpDuration = 0.2f,
			jumpSpeed = 4f,
			jumpExtraControlThreshold = 0.16f,
			jumpAirControl = new Vector2(1.3f, 1.4f)
		};
	}


	[System.Serializable]
	public struct StrucJumps
	{
		[Header("Force")]
		[Tooltip("Surface: The force applied per frame in jump direction.")]
		public float    jumpSpeed;
		[Tooltip("Core: When running up and jumping off a slope, the jump force becomes the current upwards speed, times this.")]
		public float    jumpSlopeConversion;

		public AnimationCurve JumpForceByTime;

		[Header("Durations")]
		[Tooltip("Surface: How long in seconds the jump will last. X is the minimum time (can end jump early by releasing the button after this). Y is the maximum (will always end after this).")]
		public Vector2    jumpDuration;
		[Tooltip("Core: How long in seconds the force calculated with jumpSlopeConversion will be applied before returning to normal force.")]
		public float    startSlopedJumpDuration;
		[Tooltip("Core: How long in seconds the player will have after running off an edge where they can still perform a grounded jump. The amount depends on their running speed beforehand.")]
		public AnimationCurve CoyoteTimeBySpeed;

		[Header ("Control")]
		[Tooltip("Surface: The modifiers that will be applied to control when jumping. X modifies turning. Y modifies acceleration.")]
		public Vector2      jumpAirControl;
		[Tooltip("Core: The duration in seconds where the player gains the above control.")]
		public float      jumpExtraControlThreshold;
	}
	#endregion

	#region multiJumps
	//-------------------------------------------------------------------------------------------------
	public StrucMultiJumps MultipleJumpStats = SetStrucMultiJumps();
	public StrucMultiJumps StartMultipleJumpStats = SetStrucMultiJumps();

	static StrucMultiJumps SetStrucMultiJumps () {
		return new StrucMultiJumps
		{
			maxJumpCount = 2,
			doubleJumpSpeed = 4.5f,
			doubleJumpDuration = new Vector2(0.04f, 0.14f),
			speedLossOnDoubleJump = 0.98f
		};
	}



	[System.Serializable]
	public struct StrucMultiJumps
	{
		[Tooltip("Surface: The maxinum number of jumps that can be applied before landing on the ground.")]
		public int          maxJumpCount;

		[Header("Effects")]
		[Tooltip("Surface: The force upwards when performing an air jump.")]
		public float        doubleJumpSpeed;
		[Tooltip("Surface: The minimum and maximum time in seconds an air jump can last.")]
		public Vector2        doubleJumpDuration;
		[Tooltip("Surface: Horizontal speed will be multiplied by this when performed.")]
		public float        speedLossOnDoubleJump;
	}
	#endregion

	#region Quickstep
	//-------------------------------------------------------------------------------------------------
	public StrucQuickstep QuickstepStats = SetStrucQuickstep();
	public StrucQuickstep StartQuickstepStats = SetStrucQuickstep();

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
		[Header("Grounded")]
		[Tooltip("Surface: The speed to move left or right when stepping. This is a force, so the distance traveled will equal this multiplied by time between frames.")]
		public float        stepSpeed;
		[Tooltip("Surface: How far to the right or left to move in total when performing a step (will stop if hits an obstruction)")]
		public float        stepDistance;
		[Header("In Air")]
		[Tooltip("Surface: Same as above but when the step is started in the air.")]
		public float        airStepSpeed;
		[Tooltip("Surface: Same as above but when the step is started in the air.")]
		public float        airStepDistance;
		[Header("Interaction")]
		[Tooltip("Core: Objects on this layer will end a step if in the way of the left or right movement.")]
		public LayerMask    StepLayerMask;

	}
	#endregion

	#region jumpDash
	//-------------------------------------------------------------------------------------------------


	#endregion

	public StrucAirDash JumpDashStats = SetStrucJumpDash();
	public StrucAirDash StartJumpDashStats = SetStrucJumpDash();

	static StrucAirDash SetStrucJumpDash () {
		return new StrucAirDash
		{
			dashSpeed = 80f,
			maxDuration = 0.3f,
			minDuration = 0.15f,
			turnSpeed = 8,
			dashIncrease = 15,
			forceUpwards = 0,
			horizontalAngle = 45,
			faceDownwardsSpeed = 0.02f,
			maxDownwardsSpeed = -5,
			lockMoveInputOnStart = 0,
			speedAfterDash = -5,
			framesToChangeSpeed = 5,
		};
	}

	[System.Serializable]
	public struct StrucAirDash
	{
		[Tooltip("Core: The type of dash that will be performed. Controlled means it will be treated as its own temporary state with its own turn values, and gravity calculations. Push means it will immeidately add force in the direction.")]
		public S_Enums.JumpDashType   behaviour;
		[Header("Pre Dash")]
		[Tooltip("Surface: The minimum force to move in when in this state.")]
		public float        dashSpeed;
		[Tooltip("Surface: If moving faster than the dash speed, add this to the current speed instead of setting it directly.")]
		public float        dashIncrease;
		[Tooltip("Surface: When performed this will be added as upwards force (this can lead to an arc)")]
		public int          forceUpwards;
		[Range(0, 180)]
		[Tooltip("Core: The maximum turn that can be made when starting a dash. 90 means can turn fully right or left.")]
		public int          horizontalAngle;
		[Tooltip("Core: How long to disable changing input when performed. This will prevent turning with a controlled dash, and lock input after push.")]
		public int          lockMoveInputOnStart;

		[Header("In Dash")]
		[Tooltip("Surface: How quickly will change direction after the first turn when in a controlled dash.")]
		public float        turnSpeed;
		[Tooltip("Surface: How long the controlled dash can last in seconds before ending.")]
		public float        maxDuration;
		[Tooltip("Sufrace: How long in seconds before the controlled dash can end when button is released.")]
		public float        minDuration;
		[Tooltip("Surface: How quickly a controlled dash will start to move downwards. Acts like internal gravity.")]
		public float        faceDownwardsSpeed;
		[Tooltip("Surface: The maximum downwards speed that can be reached in a controlled dash.")]
		public float        maxDownwardsSpeed;

		[Header("Post Dash")]
		public int          lockMoveInputOnEnd;
		public float        speedAfterDash;
		public float        framesToChangeSpeed;
	}

	#region homing
	//-------------------------------------------------------------------------------------------------
	public StrucHomingSearch HomingSearch = SetStrucHomingSearch();
	public StrucHomingSearch StartHomingSearch = SetStrucHomingSearch();

	static StrucHomingSearch SetStrucHomingSearch () {
		return new StrucHomingSearch
		{
			targetSearchDistance = 44f,
			distanceModifierInCameraDirection = 1.2f,
			minimumTargetDistance = 4,
			maximumTargetDistance = 80,
			TargetLayer = new LayerMask(),
			blockingLayers = new LayerMask(),

			iconScale = 1.5f,
			iconDistanceScaling = 0.2f,
			facingAmount = 100f,
			currentTargetPriority = 0.4f,
			timeToKeepTarget = new Vector2(0.18f, 0.35f),
			timeBetweenScans = 0.06f,
			radiusOfCameraTargetCheck = 15,
			cameraDirectionPriority = 0.5f,
		};
	}

	[System.Serializable]
	public struct StrucHomingSearch
	{
		[Tooltip("Core: The time in seconds before every check of targets around. This is not done every frame for efficiency.")]
		public float                  timeBetweenScans;
		[Header("Ranges")]
		[Tooltip("Core: The maximum range of the sphere check for targets around the character.")]
		public float                  targetSearchDistance;
		[Tooltip("Core: In addition to the sphere check is a sphere cast from the character in camera direction. This value is multiplied by the above distance to get the range of this check.")]
		public float                  distanceModifierInCameraDirection;
		[Tooltip("Core: The radius of the sphere used in the above sphere cast.")]
		public int                    radiusOfCameraTargetCheck;
		[Tooltip("Core: An object can only be set as a target if more than this distance away.")]
		public int                    minimumTargetDistance;
		[Tooltip("Core: An object cannot be a target if further than this distance away, no matter the modifiers.")]
		public int                    maximumTargetDistance;

		[Header("Target Selection")]
		[Tooltip("Core: The layers the sphere checks and casts will look for. This should only ever be set to 'Homing Target'")]
		public LayerMask              TargetLayer;
		[Tooltip("Core: Will ignore a target if an object of this layer is between it and the character")]
		public LayerMask              blockingLayers;
		[Range(0f, 1f), Tooltip("Core: Determines how much to favour a target found by the sphere cast rather than the sphere check. It does this by treating the camera one as closer by this amount. (So 1 means a target found through the cast will be treated as 0 distance from player.)")]
		public float                  cameraDirectionPriority;
		[Tooltip("Core: The maximum angle there can be between the characters facing direction and direction of target for the target to be allowed. So if 90, then any targets behind the character will not be counted.")]
		public float                  facingAmount;
		[Range(0f, 1f), Tooltip("Core: If switching target, this sets how much to prioritise the old target to the new one. 0.5 means the new target must be more than twice as close.")]
		public float                  currentTargetPriority;
		[Tooltip("Core: The minimum time in seconds an object can be considered the closest target before changing. X = How long before switching to the new closest target. Y = How long before setting there as being no current target.")]
		public Vector2                timeToKeepTarget;

		[Header("Reticle")]
		[Tooltip("Core: How large to make the homing reticle when placed over the target.")]
		public float                  iconScale;
		[Tooltip("Core: How much to increase the icon by per unit of distance. Combats depth perception as the icon is an object in 3D space.")]
		public float                  iconDistanceScaling;
	}

	public StrucHomingAction HomingStats = SetStrucHomingAction();
	public StrucHomingAction StartHomingStats = SetStrucHomingAction();

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
			timerLimit = 1.5f,
			successDelay = 0.3f,
			turnSpeed = 7f,
			lerpToNewInputOnHit = 0.2f,
			lerpToPreviousDirectionOnHit = 0.85f,
			deceleration = 55f,
			acceleration = 70,
			homingCountLimit = 0,

		};
	}

	[System.Serializable]
	public struct StrucHomingAction
	{
		[Header("States")]
		[Tooltip("Core: If true, can perform a homing attack when grounded")]
		public bool         canBePerformedOnGround;
		[Tooltip("Core: If true, can perform a homing attack when lost the ground, rather than specifically a jump.")]
		public bool         canDashWhenFalling;
		[Range(0, 10), Tooltip("Core: The maxinum number of homing attacks that can be performed before landing. 0 = infinite")]
		public int          homingCountLimit;
		[Header("Effects")]
		[Tooltip("Surface: The minimum speed the attack will home in on the target.")]
		public float        attackSpeed;
		[Tooltip("Surface: Will end the attack if the target hasn't been reached before this long in seconds.")]
		public float        timerLimit;
		[Tooltip("Surface: How quickly the attack will rotate towards the target.")]
		public float        turnSpeed;
		[Header("On Hit")]
		[Tooltip("Core: How long after a succesful attack until another can be performed.")]
		public float        successDelay;
		[Tooltip("Surface: If bouncing through the target on hit (like if it's destroyed and the button is held), this is the minimum speed to be set to.")]
		public int          minimumSpeedOnHit;
		[Range(0, 1), Tooltip("Core: If holding an input on hit, and that input is within this angle of the direction the dash was moving, then on hit will start moving in this input direction. 0.5 = Will move in input direction if less than 90 degrees from dash direction.")]
		public float        lerpToNewInputOnHit;
		[Range(0, 1), Tooltip("Core: If not following input, bounce in a direction lerped from the dash direction to the direction before the attack. 1 = will always move in direction before attack. 0.5 = halfway between")]
		public float        lerpToPreviousDirectionOnHit;
		[Header("Control")]
		[Tooltip("Core: If true, the player can have some control over the speed and angles of the attack.")]
		public bool         canBeControlled;
		[Tooltip("Surface: The homing attack can never move faster than this. No matter what speed it was started at.")]
		public int          maximumSpeed;
		[Tooltip("Surface: A homing attack can never move slower than this, even if decelerating.")]
		public int          minimumSpeed;
		[Tooltip("Surface: How must speed to lose per frame when inputing against homing direction.")]
		public float         deceleration;
		[Tooltip("Surface: How must speed to gain per frame when inputing with homing direction. This cannot accelerate past the speed the attack started at.")]
		public float         acceleration;
	}
	#endregion



	#region spin Charge
	//-------------------------------------------------------------------------------------------------

	public StrucSpinCharge SpinChargeStats = SetStrucSpinCharge();
	public StrucSpinCharge StartSpinChargeStat = SetStrucSpinCharge();

	static StrucSpinCharge SetStrucSpinCharge () {
		return new StrucSpinCharge
		{
			chargingSpeed = 1.05f,
			tappingBonus = 2.1f,
			delayBeforeLaunch = 12,
			minimunCharge = 20f,
			maximunCharge = 120f,
			forceAgainstMovement = 1.2f,
			shouldSetRolling = true,
			maximumSpeedPerformedAt = 200f,
			maximumSlopePerformedAt = -0.5f,
			releaseShakeAmmount = new Vector4 (7, 0.1f, 15, 10),
			cameraPauseEffect = new Vector2(3,40),
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
		};
	}

	[System.Serializable]
	public struct StrucSpinCharge
	{
		[Tooltip("Core: The means in which the spin charge will be aimed. By input means it will follow player input and velocity. Camera means it will always point in camera direction (unless camera is locked by something)")]
		public S_Enums.SpinChargeAiming whatAimMethod;
		[Header ("Charge")]
		[Tooltip("Surface: How much charge to gain every frame this is being performed.")]
		public float                  chargingSpeed;
		[Tooltip("Surface: How much charge to gain when pressing down on the charge button (after temporarily releasing)")]
		public float                  tappingBonus;
		[Tooltip("Core: How many frames to wait after the button is released before launching. (Can allow time for tapping).")]
		public int                    delayBeforeLaunch;
		[Tooltip("Surface: The minimum value the charge must hit to actually launch forwards.")]
		public float                  minimunCharge;
		[Tooltip("Surface: The maximum charge to be used when launching.")]
		public float                  maximunCharge;
		[Header("Release")]
		[Tooltip("How much to shake the camera when launching. X is applied and multipled by charge, Y is minimum including this multiplication, Z is maximum. W is how long it lasts.")]
		public Vector4                  releaseShakeAmmount;
		[Tooltip("Core: Launch force will be multiplied by the angle between current velocity and facing direction. The Y value at -1 = how much to multiply force by if launching backwards.")]
		public AnimationCurve         ForceGainByAngle;
		[Tooltip("Core: Launch force will be multiplied by this, based on current speed moving at.")]
		public AnimationCurve         ForceGainByCurrentSpeed;
		[Tooltip("Core: How much to rotate launch direction from velocity to facing direction, by the angle between.")]
		public AnimationCurve         LerpRotationByAngle;
		[Tooltip("Core: The fallback on the camera when this action is performed. The x is how many frames the camera will stay in place, the y is how many frames it will take to catch up again.")]
		public Vector2 cameraPauseEffect;
		[Header("Control")]
		[Tooltip("Core: If true, movement calculations will be taken as if the player is in the rolling state. Will also enter the rolling state when launched..")]
		public bool                   shouldSetRolling;
		[Tooltip("Surface: How much to decrease speed by every frame")]
		public float                  forceAgainstMovement;
		[Tooltip("Core: Increases speed lost per frame by how long has been charging for.")]
		public AnimationCurve         SpeedLossByTime;
		[Header("Performing")]
		[Tooltip("Core: Can only start a spin charge if moving slower than this speed.")]
		public float                  maximumSpeedPerformedAt; //The max amount of speed you can be at to perform a Spin Dash
		[Tooltip("Core: Can only start a spin charge if on a slope angle less steep than this. 1 = flat ground. 0 = horizontal wall.")]
		public float                  maximumSlopePerformedAt; //The highest slope you can be on to Spin Dash

	}
	#endregion

	#region Bounce
	//-------------------------------------------------------------------------------------------------

	public StrucBounce BounceStats = SetStrucBounce();
	public StrucBounce StartBounceStats = SetStrucBounce();

	static StrucBounce SetStrucBounce () {
		return new StrucBounce
		{
			startDropSpeed = -100f,
			maxDropSpeed = -200f,
			bounceHaltFactor = 0.7f,
			bounceAirControl = new Vector2(1.4f, 1.1f),
			horizontalSpeedDecay = new Vector2(0.1f, 0.001f),

			listOfBounceSpeeds = new List<float> { 40f, 42f, 44f },
			minimumPushForce = 30,
			lerpTowardsInput = 0.5f,

			bounceCoolDown = 0.4f,
			coolDownModiferBySpeed = 0.003f,

			cameraPauseEffect = new Vector2(2, 30)
		};
	}


	[System.Serializable]
	public struct StrucBounce
	{
		[Header("Movement")]
		[Tooltip("Surface: How fast to immediately fall when performing a bounce.")]
		public float                  startDropSpeed;
		[Tooltip("Surface: How fast to fall eventually when performing a bounce.")]
		public float                  maxDropSpeed;
		[Tooltip("Surface: Multiplied by horizontal speed at start to decrease speed during bounce.")]
		public float                  bounceHaltFactor;
		[Tooltip("Core: Speed before action is saved when started, but will decrease by this amount per frame. X = flat value. Y = percentage of current saved speed. Will decrease by the higher.")]
		public Vector2                horizontalSpeedDecay;
		[Tooltip("Core: X = turning modifier in bounce. Y = acceleration modifier in bounce.")]
		public Vector2                bounceAirControl;
		[Header("Bounces")]
		[Tooltip("Surface: How much force to add upwards for each bounce. Resets to the first when properly landing.")]
		public List<float>            listOfBounceSpeeds;
		[Tooltip("Surface: The minimum horizontal speed to gain on bounce (if saved speed is higher, it will be that instead.)")]
		public float                  minimumPushForce;
		[Tooltip("Core: How much to rotate direction towards input on bounce.")]
		public float                  lerpTowardsInput;
		[Header("Cooldown")]
		[Tooltip("Core: How long until in seconds another bounce can be performed after a successful one.")]
		public float                  bounceCoolDown;
		[Tooltip("Core: Delay between bounces will be increase by this per unit of speed")]
		public float                  coolDownModiferBySpeed;
		[Tooltip("Core: The fallback on the camera when this action is performed. The x is how many frames the camera will stay in place, the y is how many frames it will take to catch up again.")]
		public Vector2 cameraPauseEffect;
	}
	#endregion

	#region ring Road
	//-------------------------------------------------------------------------------------------------
	public StrucRingRoad RingRoadStats = SetStrucRingRoad();
	public StrucRingRoad StartRingRoadStats = SetStrucRingRoad();
	static StrucRingRoad SetStrucRingRoad () {
		return new StrucRingRoad
		{
			willCarrySpeed = true,
			dashSpeed = 160f,
			minimumEndingSpeed = 80f,
			speedGained = 1.35f,

			searchDistance = 10f,
			RingRoadLayer = new LayerMask()
		};
	}

	[System.Serializable]
	public struct StrucRingRoad
	{
		[Header ("Performing")]
		[Tooltip("Surface: The minimum speed to dash along the road in.")]
		public float                  dashSpeed;
		[Tooltip("Surface: The minimum speed be set to after finishing the dash.")]
		public float                  minimumEndingSpeed;
		[Range (0, 2), Tooltip("Surface: If started action faster than minimum ending speed, multiply that value by this and move at that new speed.")]
		public float                  speedGained;
		[Tooltip("Core: If true, will keep moving at speed from ring road once it's over.")]
		public bool                   willCarrySpeed;
		[Header ("Scanning")]
		[Tooltip("Core: The range of the sphere check for nearby rings.")]
		public float                  searchDistance;
		[Tooltip("Core: To be considered targets for a ring road, objects must be on this layer.")]
		public LayerMask              RingRoadLayer;
	}
	#endregion

	#region DropCharge
	//-------------------------------------------------------------------------------------------------

	public StrucDropCharge DropChargeStats = SetStrucDropCharge();
	public StrucDropCharge StartDropChargeStats = SetStrucDropCharge();

	static StrucDropCharge SetStrucDropCharge () {
		return new StrucDropCharge
		{
			chargingSpeed = 110f,
			minimunCharge = 40f,
			maximunCharge = 150f,
			minimumHeightToPerform = 3,
			cameraPauseEffect = new Vector2 (3,40),
		};
	}

	[System.Serializable]
	public struct StrucDropCharge
	{
		[Tooltip("Surface: How much charge to gain per second")]
		public float      chargingSpeed;
		[Tooltip("Surface: The minimum speed to launch at. Does not start charging from here, but will always launch with this or more force.")]
		public float      minimunCharge;
		[Tooltip("Surface: The maximum speed to launch at, charge cannot exceed this.")]
		public float      maximunCharge;
		[Tooltip("Core: Can only start the action if higher than this above the ground.")]
		public float      minimumHeightToPerform;
		[Tooltip("Core: The fallback on the camera when this action is performed. The x is how many frames the camera will stay in place, the y is how many frames it will take to catch up again.")]
		public Vector2 cameraPauseEffect;
	}
	#endregion

	#region Boost
	//-------------------------------------------------------------------------------------------------

	public StrucBoost BoostStats = SetStrucBoost();
	public StrucBoost StartBoostStats = SetStrucBoost();

	static StrucBoost SetStrucBoost () {
		return new StrucBoost
		{
			startBoostSpeed = 70,
			framesToReachBoostSpeed = 5,
			boostSpeed = 150,
			maxSpeedWhileBoosting = 180,
			regainBoostSpeed = 3,
			turnCharacterThreshold = 48,
			boostTurnSpeed = 2,
			faceTurnSpeed = 6,
			speedLostOnEndBoost = 20,
			framesToLoseSpeed = 30,
			cooldown = 0.5f,
			hasAirBoost = true,
			boostFramesInAir = 40,
			AngleOfAligningToEndBoost = 80,
			gainEnergyFromRings = true,
			gainEnergyOverTime = false,
			energyGainPerRing = 4,
			energyGainPerSecond = 10,
			maxBoostEnergy = 100,
			energyDrainedOnStart = 5,
			energyDrainedPerSecond = 5,
			cameraPauseEffect = new Vector2(2,40),
		};
	}

	[System.Serializable]
	public struct StrucBoost
	{
		[Header("Start Boost")]
		[Tooltip("The speed the player will immediately run at when starting a boost, and will approach proper boost speed.")]
		public float       startBoostSpeed ;
		[Tooltip("How many frames to go from start speed above, to ideal boost speed below.")]
		public int         framesToReachBoostSpeed;
		[Header("Boosting")]
		[Tooltip("The ideal speed to boost at after the frames above, and where the player should remain during the main boost.")]
		public float       boostSpeed;
		[Tooltip("Boost speed can still be exceeded through other means like slope physics, this changes the max speed players can reach normally.")]
		public float       maxSpeedWhileBoosting;
		[Tooltip("If speed has been decreased through other means, this is how much speed to regain every frame when running on flat ground until back at boost speed.")]
		public float	regainBoostSpeed;
		[Header("Turning")]
		[Tooltip("The angle of degrees input should exceed to count as a full turn rather than a strafe. 45 = if camera is behind player and they input more that 45 degrees from forwards, character will turn fully, not strafe.")]
		public float        turnCharacterThreshold;
		[Tooltip("How quickly to change velocity towards input. Degrees per frame.")]
		public float        boostTurnSpeed;
		[Tooltip("How quickly the character will change the direction the model is facing to match velocity when not strafing. Degrees per frame.")]
		public float        faceTurnSpeed;
		[Header("End Boost")]
		[Tooltip("How much to decrease running or path speed when a boost ends.")]
		public float       speedLostOnEndBoost;
		[Tooltip("How many frames it takes to remove the above speed.")]
		public int         framesToLoseSpeed;
		[Tooltip("How many seconds until another boost can start after one ended.")]
		public float        cooldown;
		[Header("Air Boost")]
		[Tooltip("If true, boost will act as normal in the air. If false, boost will end in the air.")]
		public bool        hasAirBoost;
		[Tooltip("How many frames until a boost ends when started in, or entered the air. Will not apply is hasAirBoost is true.")]
		public float       boostFramesInAir;
		[Tooltip("If character rotates more than this many degrees when in the air (from automatic alignign to face up), then end boost.")]
		public float       AngleOfAligningToEndBoost;
		[Header("Energy")]
		[Tooltip("If true, boost energy will increase when a ring is picked up.")]
		public bool        gainEnergyFromRings;
		[Tooltip("If true, boost energy will increase every fixed frame (55 a second).")]
		public bool        gainEnergyOverTime;
		[Tooltip("How much energy is gained whenever a ring is picked up.")]
		public float       energyGainPerRing;
		[Tooltip("How much energy is gained each fixed frame.")]
		public float       energyGainPerSecond;
		[Tooltip("Cannot gain more boost energy than this.")]
		public float	maxBoostEnergy;
		[Tooltip("How much boost energy is lost every second while boosting.")]
		public float       energyDrainedPerSecond;
		[Tooltip("How much energy is consumed when a boost starts.")]
		public float       energyDrainedOnStart;
		[Header("Effects")]
		[Tooltip("Core: The fallback on the camera when this action is performed. The x is how many frames the camera will stay in place, the y is how many frames it will take to catch up again.")]
		public Vector2 cameraPauseEffect;
	}
	#endregion

	#region enemy interaction
	//-------------------------------------------------------------------------------------------------

	public StrucEnemyInteract EnemyInteraction = SetStrucEnemyInteract();
	public StrucEnemyInteract StartEnemyInteraction = SetStrucEnemyInteract();
	static StrucEnemyInteract SetStrucEnemyInteract () {
		return new StrucEnemyInteract
		{
			bouncingPower = 45f,
			homingBouncingPower = 40f,
			shouldStopOnHit = false,
			damageShakeAmmount = 0.5f,
			hitShakeAmmount = 1.2f,
		};
	}



	[System.Serializable]
	public struct StrucEnemyInteract
	{
		[Header("Force on hit")]
		[Tooltip("Surface: How much force pushes upwards after damaging an enemy with a jump attack.")]
		public float                  bouncingPower;
		[Tooltip("Surface: How much force pushes upwards after damaging an enemy with a  homing attack.")]
		public float                  homingBouncingPower;
		[Tooltip("Core: If true, cannot carry momentum after hitting an enemy, instead being set to a specific velocity.")]
		public bool                   shouldStopOnHit;
		[Header("Effects")]
		[Tooltip("Core: How much to shake the camera when hurt.")]
		public float                  damageShakeAmmount;
		[Tooltip("Core: How much to shake the camera when succesffuly hiting an enemy.")]
		public float                  hitShakeAmmount;
	}
	#endregion

	#region pull Items
	//-------------------------------------------------------------------------------------------------
	public StrucItemPull ItemPulling = SetStrucItemPull();
	public StrucItemPull StartItemPulling = SetStrucItemPull();

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
		[Tooltip("Core: How close rings need to be to get pulled towards the player, by current running speed.")]
		public AnimationCurve RadiusBySpeed;
		[Tooltip("Core: To be pulled in, objects must be on this layer")]
		public LayerMask RingMask;
		[Tooltip("Core: How quickly to pull rings in, this will scale with player's running speed.")]
		public float basePullSpeed;
	}
	#endregion

	#region Bonk
	//-------------------------------------------------------------------------------------------------
	public StrucBonk WhenBonked = SetStrucBonk();
	public StrucBonk StartWhenBonked = SetStrucBonk();

	static StrucBonk SetStrucBonk () {
		return new StrucBonk
		{
			BonkOnWalls = new LayerMask(),
			bonkUpwardsForce = 20f,
			bonkBackwardsForce = 27f,
			bonkControlLock = 20f,
			bonkControlLockAir = 40f,
			bonkTime = 35
		};
	}

	[System.Serializable]
	public struct StrucBonk
	{
		[Tooltip("Core: If an object is on this layer, running face first into it will cause the player to rebound. Set to none to disable bonking.")]
		public LayerMask              BonkOnWalls;
		[Tooltip("Surface: How much the player will be knocked off the ground when bonking. Will be less in the air.")]
		public float                  bonkUpwardsForce;
		[Tooltip("Surface: How much the player will be knocked backwards, away from the wall.")]
		public float                  bonkBackwardsForce;
		[Tooltip("Core: How long in frames control should be disabled after a bonk, not being able to move until this is over")]
		public float                  bonkControlLock;
		[Tooltip("Core: Same as above, but typically longer when in the air to prevent using bonks to scale up.")]
		public float                  bonkControlLockAir;
		[Tooltip("Core: How long in frames to stay in the state. This won't lock control (see above for that), but will affect movemement and performable actions.")]
		public int                    bonkTime;

	}
	#endregion

	#region Hurt
	//-------------------------------------------------------------------------------------------------


	public StrucHurt WhenHurt = SetStrucHurt();
	public StrucHurt StartWhenHurt = SetStrucHurt();

	static StrucHurt SetStrucHurt () {
		return new StrucHurt
		{
			invincibilityTime = 90,
			maxRingLoss = 20,
			ringReleaseSpeed = 550f,
			respawnAfter = new Vector3(90, 120, 170),
			ringArcSpeed = 250f,
			flickerTimes = new Vector2(5, 10),
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
		[Header("Damaged")]
		[Tooltip("Surface: How long in frames to be impervious to being hit again after taking damage.")]
		public int              invincibilityTime;
		[Tooltip("Core: The character will flicker when invincible. X = how long it will be visible, Y = how long it will be hidden.")]
		public Vector2            flickerTimes;
		[Tooltip("Core: How long in frames the three stages of respawning will take in total. X = when to start fading out. Y = when to end fading out (this is when the level will be reset). Z = When to respawn and fade back in.")]
		public Vector3                respawnAfter;
		[Header("Ring Loss")]
		[Tooltip("Surface: When damage, will never lose more rings than this.")]
		public int              maxRingLoss;
		[Tooltip("Core: The force to apply on rings to move them away from the player when lost.")]
		public float            ringReleaseSpeed;
		[Tooltip("Core: Rings won't all be shot out in the same direction. Each frame the next ring will be shot out at this much of an angle from the last.")]
		public float            ringArcSpeed;
		[Tooltip("Core: Not every ring lost will be spawned to be picked up again. This decreases how many rings to drop based on how many rings are left to lose. So if losing 30 rings, the first dropped would decrease how many to dropped by the Y value at x30.")]
		public AnimationCurve         RingsLostInSpawnByAmount;
	}

	public StrucRebound KnockbackStats = SetStrucRebound();
	public StrucRebound StartKnockBackStats = SetStrucRebound();

	static StrucRebound SetStrucRebound () {
		return new StrucRebound
		{
			whatResponse = S_Enums.HurtResponse.Normal,
			knockbackUpwardsForce = 30f,
			recoilFrom = new LayerMask(),
			knockbackForce = 25f,
			hurtControlLock = new Vector2 (10, 15f),
			hurtControlLockAir = new Vector2(10, 35f),
			stateLengthWithKnockback = 130,
			stateLengthWithoutKnockback = 90,
		};
	}

	[System.Serializable]
	public struct StrucRebound
	{
		[Header("Interactions")]
		[Tooltip("Core: How the player will respond when damaged. Normal = carrying on without losing much speed and 'phasing' through attack. Reset speed = being knocked back with a new set force, losing all speed. Frontier = being knocked back but not taking damage until hitting the ground in the damaged state.")]
		public S_Enums.HurtResponse whatResponse;
		[Tooltip("Core: Even if set to normal, solid objects of this layer will still knock the player back when damaged. E.G. if running into a spike wall, don't want to keep player momentum as they'd get stuck on it, so still bounce backwards there.")]
		public LayerMask        recoilFrom;
		[Tooltip("Surface: If being knocked back, this is how much to be sent upwards. Less in the air.")]
		public float            knockbackUpwardsForce;
		[Tooltip("Surface: How much to be knocked backwards.")]
		public float            knockbackForce;
		[Header("Durations")]
		[Tooltip("Core: How long in frames control should be disabled after taking damage, not being able to change movement input until this is over. X = rebound when not being knocked backwards, Y = when is being knocked backwards.")]
		public Vector2            hurtControlLock;
		[Tooltip("Core: Same as above, but likely longer when in the air.")]
		public Vector2           hurtControlLockAir;
		[Tooltip("Core: If rebounding, how long in frames to be stuck in the state for. This doesn't lock control but affects movement, actions and animations.")]
		public int         stateLengthWithKnockback;
		[Tooltip("Core: If not being knocked back, will be in the state for a differnet time.")]
		public int         stateLengthWithoutKnockback;

	}
	#endregion

	#region rails
	//-------------------------------------------------------------------------------------------------

	public StrucRails RailStats = SetStrucRails();
	public StrucRails StartRailStats = SetStrucRails();

	static StrucRails SetStrucRails () {
		return new StrucRails
		{
			railMaxSpeed = 125f,
			railTopSpeed = 80f,
			railDecaySpeed = 0.13f,
			minimumStartSpeed = 20f,
			RailPushFowardmaxSpeed = 100f,
			RailPushFowardIncrements = 5f,
			RailPushFowardDelay = 0.42f,
			RailSlopePower = 2.5f,
			RailUpHillMultiplier = new Vector2(1.9f, 2.3f),
			RailDownHillMultiplier = new Vector2(0.5f, 0.75f),
			RailPlayerBrakePower = 0.97f,
			hopDelay = 0.3f,
			hopSpeed = 70f,
			hopDistance = 12f,
			PushBySpeed = new AnimationCurve(new Keyframe[]
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
		[Header("Speeds")]
		[Tooltip("Surface: The maximum speed that can be reached on a rail.")]
		public float            railMaxSpeed;
		[Tooltip("Surface: Might exceed this speed, but will face drag when done so.")]
		public float            railTopSpeed;
		[Tooltip("Surface: If entering a rail, will start grinding at at least this speed.")]
		public float            minimumStartSpeed;
		[Header("Push")]
		[Tooltip("Core: Pushes will only increase speed if grinding slower than this.")]
		public float            RailPushFowardmaxSpeed;
		[Tooltip("Core: How much speed to gain when pushing.")]
		public float            RailPushFowardIncrements;
		[Tooltip("Core: How long until another push can be performed.")]
		public float            RailPushFowardDelay;
		[Tooltip("Core: Pushes will be multiplied by this, depending on current grinding speed.")]
		public AnimationCurve   PushBySpeed;
		[Header("Slopes")]
		[Tooltip("Surface: The general force applied when grinding up (against) or down (for)")]
		public float            RailSlopePower;
		[Tooltip("Core: Force against multiplied by this when grinding upwards. X = normal. Y = when crouching")]
		public Vector2           RailUpHillMultiplier;
		[Tooltip("Core: Force for multiplied by this when grinding downwards. X = normal. Y = when crouching")]
		public Vector2            RailDownHillMultiplier;
		[Header("Drag")]
		[Tooltip("Surface: How much speed to lose a frame when over top speed.")]
		public float            railDecaySpeed;
		[Tooltip("Surface: How much speed to lose a frame when intentionally breaking.")]
		public float            RailPlayerBrakePower;
		[Tooltip("Core: How much speed to lose per frame after a booster has finished apply new speed.")]
		public float            railBoostDecaySpeed;
		[Tooltip("Core: How long in seconds for boost to wear off after it has finished. Won't remove all speed gained, just some will fall off, depending on above stat.")]
		public float            railBoostDecayTime;
		[Header("Hopping")]
		[Tooltip("Core: How long in seconds after landing on a rail before a hop can be performed.")]
		public float            hopDelay;
		[Tooltip("Core: How much distance is over 1 second when hopping to rails on the right or left.")]
		public float            hopSpeed;
		[Tooltip("Core: The total distance a hop will travel to hit a rail.")]
		public float            hopDistance;

	}

	public StrucPositionOnRail RailPosition = SetStrucPositionOnRail();
	public StrucPositionOnRail StartRailPosition = SetStrucPositionOnRail();

	static StrucPositionOnRail SetStrucPositionOnRail () {
		return new StrucPositionOnRail
		{
			offsetRail = 2f,
			offsetZip = -6f,
			upreel = 0.3f
		};
	}


	[System.Serializable]
	public struct StrucPositionOnRail
	{
		[Tooltip("Surface: How much above the spline the player should be to be visibly on the rail.")]
		public float    offsetRail;
		[Tooltip("Surface: How much below the the spline the player should be to be visibly holding onto the handle.")]
		public float    offsetZip;
		[Tooltip("Surface: How much below the the handle the player should be to be visibly holding onto it.")]
		public float    upreel;
	}
	#endregion

	#region objects
	public StrucInteractions ObjectInteractions = SetStrucInteractions ();
	public StrucInteractions StartObjectInteractions = SetStrucInteractions ();

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
	public StrucWallRunning StartWallRunningStats = SetWallRunning();

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
		"You can also return each struct to its default values if you're not happy with your changes.", EditorStyles.textArea);

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
		DrawBoost();

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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.SpeedStats = stats.StartSpeedStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.AccelerationStats = stats.StartAccelerationStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.DecelerationStats = stats.StartDecelerationStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.TurningStats = stats.StarTurningStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.SlopeStats = stats.StartSlopeStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.GreedysStickToGround = stats.StartStickToGround;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.FindingGround = stats.StartFindGround;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.WhenInAir = stats.StartWhenInAir;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.RollingStats = stats.StartRollingStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.SkiddingStats = stats.StartSkiddingStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.JumpStats = stats.StarJumpStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.MultipleJumpStats = stats.StartMultipleJumpStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.QuickstepStats = stats.StartQuickstepStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//Spin Charge
		#region SpinCharge
		void DrawSpinCharge () {
			EditorGUILayout.Space();
			DrawProperty("SpinChargeStats", "Spin Charge");

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.SpinChargeStats = stats.StartSpinChargeStat;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.HomingStats = stats.StartHomingStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}

		void DrawHomingSearch () {
			EditorGUILayout.Space();
			DrawProperty("HomingSearch", "Homing Targetting");

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.HomingSearch = stats.StartHomingSearch;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.BounceStats = stats.StartBounceStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.RingRoadStats = stats.StartRingRoadStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.DropChargeStats = stats.StartDropChargeStats;
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
				stats.JumpDashStats = stats.StartJumpDashStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion

		//Boost
		#region Boost
		void DrawBoost () {
			EditorGUILayout.Space();
			DrawProperty("BoostStats", "Boost Stats");

			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.BoostStats = stats.StartBoostStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.EnemyInteraction = stats.StartEnemyInteraction;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.ItemPulling = stats.StartItemPulling;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.WhenBonked = stats.StartWhenBonked;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.WhenHurt = stats.StartWhenHurt;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.KnockbackStats = stats.StartKnockBackStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.RailStats = stats.StartRailStats;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.RailPosition = stats.StartRailPosition;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.ObjectInteractions = stats.StartObjectInteractions;
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

			Undo.RecordObject(stats, "set to Defaults");
			if (GUILayout.Button("Default", ResetToDefaultButton))
			{
				stats.WallRunningStats = stats.StartWallRunningStats;
			}
			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
		#endregion
	}

}
#endif
