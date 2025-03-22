using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.AxisState;

public class S_Triggered_PlayAnimation : MonoBehaviour, ITriggerable
{
	private Animator _Animator;

	[SerializeField, Min(0.1f)]
	private float _defaultSpeed = 1;
	[SerializeField] private AnimationCurve _animSpeedByPlayerSpeed = new AnimationCurve (new Keyframe[] {
		new Keyframe (0f,1f),
		new Keyframe (1f,1f) });

	private void Awake () {
		if (!gameObject.TryGetComponent(out _Animator)) enabled = false;
	}
	public void TriggerObjectOn ( S_PlayerPhysics Player = null ) {
		if (!enabled) { return; }

		SetAnimatorSpeed(Player);
		_Animator.SetTrigger("TriggerOn");

		Debug.DrawRay(Player.transform.position, Vector3.up * 10, Color.black, 200f);

		S_Manager_LevelProgress.OnReset += EventReturnOnDeath;
	}

	public void TriggerObjectOff ( S_PlayerPhysics Player = null ) {
		if (!enabled) { return; }

		SetAnimatorSpeed(Player);
		_Animator.SetTrigger("TriggerOff");

		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
	}

	private void SetAnimatorSpeed ( S_PlayerPhysics Player = null ) {
		float speedModi = 1;
		if (Player)
			speedModi = _animSpeedByPlayerSpeed.Evaluate(Player._PlayerVelocity._horizontalSpeedMagnitude / Player._PlayerMovement._currentMaxSpeed);

		_Animator.speed = (_defaultSpeed * speedModi);
	}

	void EventReturnOnDeath ( object sender, EventArgs e ) {
		_Animator.speed = 50;
		_Animator.SetTrigger("TriggerOff");

		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
	}
}
