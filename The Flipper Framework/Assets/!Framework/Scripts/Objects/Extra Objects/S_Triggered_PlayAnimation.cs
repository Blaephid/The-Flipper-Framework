using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	}

	public void TriggerObjectOff ( S_PlayerPhysics Player = null ) {
		if (!enabled) { return; }

		SetAnimatorSpeed(Player);
		_Animator.SetTrigger("TriggerOff");
	}

	private void SetAnimatorSpeed ( S_PlayerPhysics Player = null ) {
		float speedModi = 1;
		if (Player)
			speedModi = _animSpeedByPlayerSpeed.Evaluate(Player._PlayerVelocity._horizontalSpeedMagnitude / Player._PlayerMovement._currentMaxSpeed);

		_Animator.speed = _defaultSpeed * speedModi;
	}
}
