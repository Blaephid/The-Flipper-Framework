using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Action13_Hovering : MonoBehaviour
{
	S_CharacterTools Tools;
	S_PlayerPhysics PlayerPhys;
	S_ActionManager Actions;
	S_PlayerInput _Input;
	Animator CharacterAnimator;
	Transform PlayerSkin;
	S_Control_PlayerSound Sounds;

	float floatSpeed = 15;
	public AnimationCurve forceFromSource;

	[HideInInspector] public bool inWind;
	float exitWindTimer;
	float exitWind = 0.6f;
	Vector3 forward;

	[HideInInspector] public float _skiddingStartPoint_;
	float _airSkiddingIntensity_;

	S_Trigger_Updraft hoverForce;

	private void Awake () {
		if (PlayerPhys == null)
		{
			Tools = GetComponent<S_CharacterTools>();
			AssignTools();

			AssignStats();
		}

	}

	private void AssignTools () {
		PlayerPhys = GetComponent<S_PlayerPhysics>();
		Actions = GetComponent<S_ActionManager>();
		CharacterAnimator = Tools.CharacterAnimator;
		PlayerSkin = Tools.PlayerSkinTransform;
		_Input = GetComponent<S_PlayerInput>();

		Sounds = Tools.SoundControl;
	}

	private void AssignStats () {
		_skiddingStartPoint_ = Tools.Stats.SkiddingStats.angleToPerformSkid;
		_airSkiddingIntensity_ = Tools.Stats.SkiddingStats.skiddingIntensity;
	}

	public void InitialEvents ( S_Trigger_Updraft up ) {
		PlayerPhys._isGravityOn = false;
		inWind = true;
		forward = PlayerSkin.forward;

		hoverForce = up;
	}

	public void updateHover ( S_Trigger_Updraft up ) {
		inWind = true;
		hoverForce = up;
	}

	private void Update () {
		CharacterAnimator.SetInteger("Action", 13);

		//Do a homing attack
		if (Actions.Action02._isHomingAvailable && Actions.Action02Control._HasTarget && _Input.HomingPressed)
		{

			//Do a homing attack
			if (Actions.Action02 != null && PlayerPhys._homingDelay_ <= 0)
			{
				if (Actions.Action02Control._isHomingAvailable)
				{
					Sounds.HomingAttackSound();
					Actions.ChangeAction(S_Enums.PlayerStates.Homing);
					Actions.Action02.InitialEvents();
				}
			}

		}
	}



	private void FixedUpdate () {
		updateModel();
		PlayerPhys.SetIsGrounded(false);

		getForce();

		if (inWind)
		{
			exitWindTimer = 0;

			if (PlayerPhys._RB.velocity.y < floatSpeed)
			{
				PlayerPhys.AddCoreVelocity(hoverForce.transform.up * floatSpeed);
			}

		}
		else
		{
			exitWindTimer += Time.deltaTime;

			if (PlayerPhys._RB.velocity.y < floatSpeed)
			{
				PlayerPhys.AddCoreVelocity(hoverForce.transform.up * (floatSpeed * 0.35f));
			}

			if (exitWindTimer >= exitWind)
			{
				Actions.ChangeAction(S_Enums.PlayerStates.Regular);
			}
		}

		//Skidding
		if ((PlayerPhys._inputVelocityDifference < -_skiddingStartPoint_) && !PlayerPhys._isGrounded)
		{
			if (PlayerPhys._speedMagnitude >= -(_airSkiddingIntensity_ * 0.8f)) PlayerPhys.AddCoreVelocity(PlayerPhys._RB.velocity.normalized * (_airSkiddingIntensity_ * 0.8f) * (PlayerPhys._isRolling ? 0.5f : 1));


			if (PlayerPhys._speedMagnitude < 4)
			{
				Actions.Action00.SetIsRolling(false);

			}
		}


	}
	void updateModel () {
		//Set Animation Angle
		Vector3 VelocityMod = new Vector3(PlayerPhys._RB.velocity.x, 0, PlayerPhys._RB.velocity.z);
		if (VelocityMod != Vector3.zero)
		{
			Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
			CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * Actions.Action00._skinRotationSpeed);
		}
		PlayerSkin.forward = forward;
	}

	void getForce () {
		float distance = transform.position.y - hoverForce.bottom.position.y;
		float difference = distance / (hoverForce.top.position.y - hoverForce.bottom.position.y);
		floatSpeed = forceFromSource.Evaluate(difference) * hoverForce.power;
		Debug.Log(difference);

		if (difference > 0.98)
		{
			floatSpeed = -Mathf.Clamp(PlayerPhys._RB.velocity.y, -100, 0);
		}
		else if (PlayerPhys._RB.velocity.y > 0)
		{
			floatSpeed = Mathf.Clamp(floatSpeed, 0.5f, PlayerPhys._RB.velocity.y);
		}
	}

	private void OnDisable () {
		CharacterAnimator.SetInteger("Action", 1);
		PlayerSkin.forward = CharacterAnimator.transform.forward;
		PlayerPhys._isGravityOn = true;
		inWind = false;
	}
}
