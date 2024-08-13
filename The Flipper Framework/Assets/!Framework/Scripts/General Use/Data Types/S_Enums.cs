using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Enums
{
	//Actions.
	//How they're ordered here is also how they're ordered in priority, so the action manager will list then in this order, the higher, the sooner it will be checked (so if boost is above quickstep, it will be called before).
	public enum PrimaryPlayerStates
	{
		Default = 0,
		Jump = 2,
		Homing = 3,
		SpinCharge = 4,
		Hurt = 1,
		Rail = 5,
		Bounce = 6,
		RingRoad = 7,
		DropCharge = 8,
		Path = 9,
		JumpDash = 10,
		WallRunning = 11,
		WallClimbing = 14,
		Hovering = 12,
		Upreel = 13,
	}

	public enum PlayerControlledStates {
		Jump,
		Homing,
		SpinCharge,
		Bounce,
		DropCharge,
		JumpDash
	}

	public enum PlayerSituationalStates {
		Default,
		Hurt,
		Rail,
		RingRoad,
		Path,
		WallRunning,
		WallClimbing,
		Hovering,
		Upreel
	}

	public enum SubPlayerStates {
		Skidding = 0,
		Quickstepping = 4,
		Rolling = 3,
		Boost = 1
	}

	//General
	//
	public enum VelocityTypes {
		Total,
		Core,
		Environmental,
		CoreNoVertical,
		CoreNoLateral,
		Custom
	}


	//Interaction
	//
	public enum PlayerAttackTypes {
		Rolling,
		SpinJump,
		HomingAttack,
	}

	public enum PlayerAttackStates {
		Unprotected,
		Ball,
		Homing,
		Boost,
	}

	public enum AttackTargets {
		Enemy,
		Monitor
	}

	//Action specific
	public enum SpinChargeAimingTypes
	{
		Input,
		Camera
	}

	public enum JumpDashTypes {
		ControlledDash,
		Push,
	}

	public enum BoostTypes {
		Normal,
		Segmented,
	}

	public enum HomingHitResponses {
		BounceThrough,
		bounceOff,
		Rebound,
	}

	public enum HurtResponses {
		Normal,
		ResetSpeed,
		Frontiers,
		FrontiersSansDeathDelay,
		NormalSansDeathDelay,
	}


	//Objects & Interactions

	public enum ChangeLockState {
		Ignore,
		Lock,
		Unlock
	}

	public enum LockControlDirection {
		Change,
		NoInput,
		CharacterForwards,
	}
}
