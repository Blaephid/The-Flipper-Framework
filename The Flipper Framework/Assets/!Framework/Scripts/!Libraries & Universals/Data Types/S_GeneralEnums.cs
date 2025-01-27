using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_GeneralEnums
{

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

	public enum ChangeGroundedState {
		DontChange,
		SetToYes,
		SetToNo,
		SetToOppositeThenBack,
	}
}
