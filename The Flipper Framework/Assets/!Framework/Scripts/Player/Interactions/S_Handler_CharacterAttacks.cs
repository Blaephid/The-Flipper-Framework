

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_CharacterAttacks : MonoBehaviour
{
	S_PlayerPhysics _PlayerPhys;
	S_Interaction_Objects _ObjectInteraction;
	S_ActionManager _Actions;
	S_CharacterTools Tools;
	S_PlayerInput _Input;

	GameObject JumpBall;

	[Header("Enemies")]

	[HideInInspector] public float _bouncingPower_;
	[HideInInspector] public float _homingBouncingPower_;
	[HideInInspector] public float _enemyHomingStoppingPowerWhenAdditive_;
	[HideInInspector] public bool _shouldStopOnHomingAttackHit_;
	[HideInInspector] public bool _shouldStopOnHit_;

	private bool CanHitAgain = true;
	private bool _hasHitThisFrame = false; //Prevents multiple attacks from being calculated in one frame

	private void Start () {
		Tools = GetComponent<S_CharacterTools>();
		AssignTools();

		AssignStats();
	}

	private void FixedUpdate () {
		_hasHitThisFrame = false;
	}

	public bool AttemptAttackOnContact ( Collider other, S_Enums.AttackTargets target ) {
		if(!_hasHitThisFrame)
		{
			switch (_Actions.whatAction)
			{
				case S_Enums.PrimaryPlayerStates.Default:
					if (_PlayerPhys._isRolling)
					{
						AttackThing(other, S_Enums.PlayerAttackTypes.Rolling, target);
						_hasHitThisFrame = true;
						break;
					}
					_hasHitThisFrame = false;
					break;
				case S_Enums.PrimaryPlayerStates.SpinCharge:
					AttackThing(other, S_Enums.PlayerAttackTypes.Rolling, target);
					_hasHitThisFrame = true;
					break;
				case S_Enums.PrimaryPlayerStates.Jump:
					AttackThing(other, S_Enums.PlayerAttackTypes.SpinJump, target);
					_hasHitThisFrame = true;
					break;
				case S_Enums.PrimaryPlayerStates.JumpDash:
					AttackThing(other, S_Enums.PlayerAttackTypes.SpinJump, target);
					_hasHitThisFrame = true;
					break;
				case S_Enums.PrimaryPlayerStates.Homing:
					AttackThing(other, S_Enums.PlayerAttackTypes.HomingAttack, target);
					_hasHitThisFrame = true;
					break;
			}
		}
		return _hasHitThisFrame;
	}

	private void AttackThing ( Collider col, S_Enums.PlayerAttackTypes attackType, S_Enums.AttackTargets target, int damage = 1 ) {
		switch (target)
		{
			case S_Enums.AttackTargets.Monitor:
				MonitorAttack(col, attackType);
				break;

			case S_Enums.AttackTargets.Enemy:
				EnemyAttack(attackType, col, damage);
				break;
		}

	}

	private void EnemyAttack ( S_Enums.PlayerAttackTypes attackType, Collider col, int damage = 1 ) {
		bool wasDestroyed = true;

		if (col.transform.parent.GetComponent<S_AI_Health>() != null && CanHitAgain)
		{
			wasDestroyed = col.transform.parent.GetComponent<S_AI_Health>().DealDamage(damage);
		}

		BounceAfterAttack(wasDestroyed, attackType);
	}

	private void MonitorAttack ( Collider col, S_Enums.PlayerAttackTypes attackType ) {
		_ObjectInteraction.TriggerMonitor(col);

		BounceAfterAttack(true, attackType);

	}

	private void BounceAfterAttack ( bool wasDestroyed, S_Enums.PlayerAttackTypes attackType ) {
		switch (attackType)
		{
			case S_Enums.PlayerAttackTypes.Rolling:
				break;

			case S_Enums.PlayerAttackTypes.SpinJump:
				AttackFromJump();
				break;
			case S_Enums.PlayerAttackTypes.HomingAttack:
				AttackFromHoming(wasDestroyed);
				break;
		}
	}

	void AttackFromJump () {
		StartCoroutine(DelayAttacks());

		Vector3 newSpeed = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
		newSpeed.y = _bouncingPower_ + Mathf.Abs(_PlayerPhys._RB.velocity.y);
		if (newSpeed.y > _PlayerPhys._RB.velocity.y * 1.5f)
			newSpeed.y = _PlayerPhys._RB.velocity.y * 1.5f;

		_PlayerPhys.SetTotalVelocity(newSpeed, new Vector2(1, 0));
	}

	private void AttackFromHoming ( bool wasDestroyed ) {
		StartCoroutine(DelayAttacks());

		if (wasDestroyed)
		{
			_Actions.Action02.HittingTarget(S_Enums.HomingRebounding.BounceThrough);
		}
		else
			_Actions.Action02.HittingTarget(S_Enums.HomingRebounding.Rebound);
	}

	private void AttackFromBounce ( Vector3 newSpeed ) {
		StartCoroutine(DelayAttacks());

		newSpeed = new Vector3(1, 0, 1);

		newSpeed = Vector3.Scale(_PlayerPhys._RB.velocity, newSpeed);
		newSpeed.y = _homingBouncingPower_ * 1.8f;
		_PlayerPhys.SetTotalVelocity(newSpeed, new Vector2(1, 0));
	}

	private IEnumerator DelayAttacks () {
		CanHitAgain = false;
		yield return new WaitForSeconds(0.05f);
		CanHitAgain = true;
	}

	private void AssignStats () {
		_bouncingPower_ = Tools.Stats.EnemyInteraction.bouncingPower;
		_homingBouncingPower_ = Tools.Stats.EnemyInteraction.homingBouncingPower;
		_enemyHomingStoppingPowerWhenAdditive_ = Tools.Stats.EnemyInteraction.enemyHomingStoppingPowerWhenAdditive;
		_shouldStopOnHomingAttackHit_ = Tools.Stats.EnemyInteraction.shouldStopOnHomingAttackHit;
		_shouldStopOnHit_ = Tools.Stats.EnemyInteraction.shouldStopOnHit;

	}

	private void AssignTools () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_ObjectInteraction = GetComponent<S_Interaction_Objects>();
		_Actions = GetComponent<S_ActionManager>();
		_Input = GetComponent<S_PlayerInput>();

		JumpBall = Tools.JumpBall;
	}
}

