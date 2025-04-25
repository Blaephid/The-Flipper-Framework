using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Cinemachine.AxisState;

public class S_Triggered_PlayAnimation : S_Triggered_Base, ITriggerable
{
	[SerializeField] Animator _Animator;

	[SerializeField, Min(0.1f)]
	private float _defaultSpeed = 1;
	[SerializeField]
	private AnimationCurve _animSpeedByPlayerSpeed = new AnimationCurve (new Keyframe[] {
		new Keyframe (0f,1f),
		new Keyframe (1f,1f) });

	private void Awake () {
		if (_Animator == null)
		{
			if (!gameObject.TryGetComponent(out _Animator)) enabled = false;
		}
	}

	public void TriggerObjectOn ( S_PlayerPhysics Player = null ) {
		if (!CanBeTriggeredOn(Player)) { return; }

		SetAnimatorSpeed(Player);
		_isCurrentlyOn = true;
		_Animator.SetTrigger("TriggerOn");

	}

	public void TriggerObjectOff ( S_PlayerPhysics Player = null ) {
		if (!CanBeTriggeredOff(Player)) { return; }

		SetAnimatorSpeed(Player);
		_isCurrentlyOn = false;
		_Animator.SetTrigger("TriggerOff");

	}

	private void SetAnimatorSpeed ( S_PlayerPhysics Player = null ) {
		float speedModi = 1;
		if (Player)
			speedModi = _animSpeedByPlayerSpeed.Evaluate(Player._PlayerVelocity._horizontalSpeedMagnitude / Player._PlayerMovement._currentMaxSpeed);

		_Animator.speed = (_defaultSpeed * speedModi);
	}

	public override void ResetToOriginal () {
		if (!_Animator) { return; }

		_Animator.speed = 50;
		_Animator.SetTrigger("TriggerOff");
	}

	public override void EventReturnOnDeath ( object sender, EventArgs e ) {

		_isCurrentlyOn = false;
		base.EventReturnOnDeath(sender, e);
	}
}
