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
		Hovering = 12
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
		Hovering,
	}

	public enum SubPlayerStates {
		Skidding = 0,
		Quickstepping = 4,
		Rolling = 3,
		Boost = 1
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
	public enum SpinChargeAiming
	{
		Input,
		Camera
	}

	public enum JumpDashType {
		ControlledDash,
		Push,
	}

	public enum BoostTypes {
		Normal,
		Segmented,
	}

	public enum HomingRebounding {
		BounceThrough,
		bounceOff,
		Rebound,
	}

	public enum HurtResponse {
		Normal,
		ResetSpeed,
		Frontiers,
		FrontiersWithoutDeathDelay
	}
}
