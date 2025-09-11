using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Unity.Cinemachine.InputAxis;

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

	public void TriggerObjectOn ( S_CharacterTools Player = null ) {
		if (!CanBeTriggeredOn(Player)) { return; }

		SetAnimatorSpeed(Player);
		_Animator.SetTrigger("TriggerOn");

	}

	public void TriggerObjectOff ( S_CharacterTools Player = null ) {
		if (!CanBeTriggeredOff(Player)) { return; }

		SetAnimatorSpeed(Player);
		_notInDefaultState = false;
		_Animator.SetTrigger("TriggerOff");

	}

	private void SetAnimatorSpeed ( S_CharacterTools Player = null ) {
		float speedModi = 1;
		if (Player)
			speedModi = _animSpeedByPlayerSpeed.Evaluate(Player._PlayerVel._horizontalSpeedMagnitude / Player._PlayerMove._currentMaxSpeed);

		_Animator.speed = (_defaultSpeed * speedModi);
	}

	public override void ResetToOriginal () {
		if (!_Animator) { return; }

		_Animator.speed = 50;
		_Animator.SetTrigger("TriggerOff");
	}

	public override void EventReturnOnDeath ( object sender, EventArgs e ) {

		_notInDefaultState = false;
		base.EventReturnOnDeath(sender, e);
	}
}
