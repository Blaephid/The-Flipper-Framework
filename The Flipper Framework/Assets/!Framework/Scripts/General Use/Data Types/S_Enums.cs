using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Enums
{
	//Actions
	public enum PrimaryPlayerStates
	{
		Default,
		Jump,
		Homing,
		SpinCharge,
		Hurt,
		Rail,
		Bounce,
		RingRoad,
		DropCharge,
		Path,
		JumpDash,
		WallRunning,
		Hovering
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
		Skidding,
		Quickstepping,
		Rolling,
	}

	//Interaction

	public enum PlayerAttackTypes {
		Rolling,
		SpinJump,
		HomingAttack,
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
		AngledPush,
		ArcedDash,
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
