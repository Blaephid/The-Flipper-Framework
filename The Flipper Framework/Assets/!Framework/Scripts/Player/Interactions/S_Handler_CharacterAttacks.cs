

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_CharacterAttacks : MonoBehaviour
{
	private S_PlayerPhysics _PlayerPhys;
	private S_PlayerVelocity        _PlayerVel;
	private S_Interaction_Objects _ObjectInteraction;
	private S_ActionManager _Actions;
	private S_CharacterTools        _Tools;

	//Stats - See Character stats for functions
	[HideInInspector]
	public float    _bouncingPower_;
	[HideInInspector]
	public float    _homingBouncingPower_;
	[HideInInspector]
	public bool     _shouldStopOnHit_;

	private bool    _canHitAgain = true;
	private bool    _hasHitThisFrame = false; //Prevents multiple attacks from being calculated in one frame

	private void Start () {
		_Tools = GetComponentInParent<S_CharacterTools>();
		AssignTools();
		AssignStats();
	}

	//Set every frame to ensure only one attack will be calculated per update.
	private void FixedUpdate () {
		_hasHitThisFrame = false;
	}

	//Called when making contact with an object, to handle if in a current attack state. Returns false if not, and may take damage.
	public bool AttemptAttackOnContact ( Collider other, S_GeneralEnums.AttackTargets target ) {
		if (!_hasHitThisFrame) //Will only try once per update.
		{
			if(other.TryGetComponent(out S_EnemyAttack enemyAttack))
				if(enemyAttack._isInvincible) { return false; }

			//Certain actions will count as attacks, other require other states.
			switch (_Actions._whatCurrentAction)
			{
				//If in default, will only attack if rolling.
				case S_S_ActionHandling.PrimaryPlayerStates.Default:
					if (_PlayerPhys._isRolling)
					{
						AttackThing(other, S_GeneralEnums.PlayerAttackTypes.Rolling, target);
						_hasHitThisFrame = true;
						break;
					}
					else if (_PlayerPhys._isBoosting) //Boost attack is handled in its own unique script, but this prevents damage being taken.
					{
						_hasHitThisFrame = true;
					}
					//If currently in a jump ball in the air
					else if (_Actions._ActionDefault._animationAction == 1)
					{
						AttackThing(other, S_GeneralEnums.PlayerAttackTypes.SpinJump, target);
						_hasHitThisFrame = true;
					}
					else { _hasHitThisFrame = false; }
					break;
				//Spin charge counts as a rolling attack.
				case S_S_ActionHandling.PrimaryPlayerStates.SpinCharge:
					AttackThing(other, S_GeneralEnums.PlayerAttackTypes.Rolling, target);
					_hasHitThisFrame = true;
					break;
				//Jump states means in a jump ball.
				case S_S_ActionHandling.PrimaryPlayerStates.Jump:
					AttackThing(other, S_GeneralEnums.PlayerAttackTypes.SpinJump, target);
					_hasHitThisFrame = true;
					break;
				//Despite not being in a ball, jump dash conts as a spin jump attack.
				case S_S_ActionHandling.PrimaryPlayerStates.JumpDash:
					AttackThing(other, S_GeneralEnums.PlayerAttackTypes.SpinJump, target);
					_hasHitThisFrame = true;
					break;
				//The most common attack, and involves being in a ball.
				case S_S_ActionHandling.PrimaryPlayerStates.Homing:
					AttackThing(other, S_GeneralEnums.PlayerAttackTypes.HomingAttack, target);
					_hasHitThisFrame = true;
					break;
			}
		}
		return _hasHitThisFrame;
	}

	//Called in order to deal damage and affect the player afterwards.
	public void AttackThing ( Collider col, S_GeneralEnums.PlayerAttackTypes attackType, S_GeneralEnums.AttackTargets target, int damage = 1 ) {
		//Different targets require different means of taking damage.

		switch (target)
		{
			case S_GeneralEnums.AttackTargets.Monitor:
				MonitorAttack(col, attackType);
				break;

			case S_GeneralEnums.AttackTargets.Enemy:
				EnemyAttack(attackType, col, damage);
				break;
		}

	}

	//Called when attacking an enemy. Gets their health and applies effects if destroyed or just damaged.
	private void EnemyAttack ( S_GeneralEnums.PlayerAttackTypes attackType, Collider col, int damage = 1 ) {
		bool wasDestroyed = true;

		S_AI_Health EnemyHealth = col.GetComponentInParent<S_AI_Health>();

		if (EnemyHealth && _canHitAgain)
		{
			wasDestroyed = EnemyHealth.DealDamage(damage);
		}

		PhysicsAfterAttack(wasDestroyed, attackType, col);
	}

	//Calls the method that handles destroying and gaining items from a monitor.
	private void MonitorAttack ( Collider col, S_GeneralEnums.PlayerAttackTypes attackType ) {
		_ObjectInteraction.TriggerMonitor(col);

		PhysicsAfterAttack(true, attackType, col); //Will always be destroyed due to monitors not having health.
	}

	//Gets attack state to decide how to affect player after an aerial attack.
	private void PhysicsAfterAttack ( bool wasDestroyed, S_GeneralEnums.PlayerAttackTypes attackType, Collider col ) {
		StartCoroutine(DelayAttacks());

		S_Data_HomingTarget.EffectOnHoming customResponse = S_Data_HomingTarget.EffectOnHoming.normal;
		//Response to attack may be changed if homing target has custom logic.
		S_Data_HomingTarget tempTarget = col.transform.parent.GetComponentInChildren<S_Data_HomingTarget>();
		if (tempTarget)
		{
			customResponse = wasDestroyed ? tempTarget.OnDestroy : tempTarget.OnHit;
		}

		//Check objects built in response first.
		switch (customResponse)
		{
			//If no unique response, apply normal response based on attack.
			case S_Data_HomingTarget.EffectOnHoming.normal:
				switch (attackType)
				{
					//Player continues unaffected after a roll attack.
					case S_GeneralEnums.PlayerAttackTypes.Rolling:
						break;
					//Force player upwards on hit.
					case S_GeneralEnums.PlayerAttackTypes.SpinJump:
						NormalAttackFromJump();
						break;
					case S_GeneralEnums.PlayerAttackTypes.HomingAttack:
						NormalAttackFromHoming(wasDestroyed);
						break;
				}
				break;
			case S_Data_HomingTarget.EffectOnHoming.shootdownWithCarry:
				ShootDownOnAttack(col, true);
				break;
			case S_Data_HomingTarget.EffectOnHoming.shootdownStraight:
				ShootDownOnAttack(col, false);
				break;
		}
	}

	//Bounces player upwards, amount depending on player state.
	void NormalAttackFromJump () {

		switch (_Actions._whatCurrentAction)
		{
			default:
				_PlayerVel.AddCoreVelocity(transform.up * _bouncingPower_);
				break;

			case S_S_ActionHandling.PrimaryPlayerStates.Bounce:
				_PlayerVel.AddCoreVelocity(transform.up * _bouncingPower_ * 1.5f);
				break;
		}
	}

	//Responses are inside the homing script for ease.
	private void NormalAttackFromHoming ( bool wasDestroyed ) {
		//If destroyed enemy, will bounce through, if not, will take knockback from it.
		if (_shouldStopOnHit_)
		{
			_Actions._ObjectForActions.GetComponent<S_Action02_Homing>().RespondToHitTarget(S_GeneralEnums.HomingHitResponses.bounceOff);
		}
		if (wasDestroyed)
		{
			_Actions._ObjectForActions.GetComponent<S_Action02_Homing>().RespondToHitTarget(S_GeneralEnums.HomingHitResponses.BounceThrough);
		}
		else
			_Actions._ObjectForActions.GetComponent<S_Action02_Homing>().RespondToHitTarget(S_GeneralEnums.HomingHitResponses.Rebound);
	}

	//Immediately show towards the floow on hitting target, without losing speed.
	private void ShootDownOnAttack ( Collider Col, bool setToEnemyDirection ) {
		if (_PlayerPhys._isGrounded) { return; }

		switch (_Actions._whatCurrentAction)
		{
			//If was homing, ensure homing ends properly before applying physics.
			case S_S_ActionHandling.PrimaryPlayerStates.Homing:
				_Actions._ObjectForActions.GetComponent<S_Action02_Homing>().HitWhileHoming();
				_Actions._ObjectForActions.GetComponent<S_Action02_Homing>().StopHoming();
				break;
		}

		Vector3 newDownDirection = Vector3.down;
		Vector3 newForwardDirection = new Vector3(_PlayerVel._worldVelocity.x, 0, _PlayerVel._worldVelocity.z).normalized;

		Vector3 newLocation = transform.position;
		float newSpeed = _Actions._speedBeforeAction > 0 ? _Actions._speedBeforeAction : _PlayerVel._horizontalSpeedMagnitude;

		if (setToEnemyDirection)
		{
			//If target has a rigidbody, make new directions relative to its velocity. 
			Rigidbody targetRB = Col.GetComponent<Rigidbody>();
			targetRB = targetRB == null ? Col.transform.parent.gameObject.GetComponent<Rigidbody>() : targetRB;
			targetRB = targetRB == null ? Col.transform.parent.parent.gameObject.GetComponent<Rigidbody>() : targetRB;

			if (targetRB)
			{
				newForwardDirection = targetRB.velocity.normalized;
				newLocation = targetRB.transform.position;
				newDownDirection = -targetRB.transform.up;
			}
		}


		_PlayerPhys.SetPlayerPosition(newLocation);
		_PlayerPhys.SetPlayerRotation(Quaternion.LookRotation(newForwardDirection, -newDownDirection), true);

		Vector3 newVelocity = newForwardDirection * newSpeed;
		newVelocity += newDownDirection * 30;

		_PlayerVel.SetBothVelocities(newVelocity, new Vector2(1, 0));
	}

	//Prevents multiple attacks in quick succession.
	private IEnumerator DelayAttacks () {
		_canHitAgain = false;
		yield return new WaitForSeconds(0.05f);
		_canHitAgain = true;
	}

	private void AssignStats () {
		_bouncingPower_ = _Tools.Stats.EnemyInteraction.bouncingPower;
		_homingBouncingPower_ = _Tools.Stats.EnemyInteraction.homingBouncingPower;
		_shouldStopOnHit_ = _Tools.Stats.EnemyInteraction.shouldStopOnHit;
	}

	private void AssignTools () {
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_PlayerVel = _Tools.GetComponent<S_PlayerVelocity>();
		_ObjectInteraction = GetComponent<S_Interaction_Objects>();
		_Actions = _Tools._ActionManager;
	}
}

