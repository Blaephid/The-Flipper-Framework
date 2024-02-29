using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Enums
{
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
	}

	public enum SpinChargeAiming
	{
		Input,
		Camera
	}
}
