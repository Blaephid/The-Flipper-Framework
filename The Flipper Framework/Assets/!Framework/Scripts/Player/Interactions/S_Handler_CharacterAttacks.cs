

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_CharacterAttacks : MonoBehaviour
{
	private S_PlayerPhysics	_PlayerPhys;
	private S_PlayerVelocity	_PlayerVel;
	private S_Interaction_Objects _ObjectInteraction;
	private S_ActionManager	_Actions;
	private S_CharacterTools	_Tools;

	//Stats - See Character stats for functions
	[HideInInspector] 
	public float	_bouncingPower_;
	[HideInInspector] 
	public float	_homingBouncingPower_;
	[HideInInspector] 
	public bool	_shouldStopOnHit_;

	private bool	_canHitAgain = true;
	private bool	_hasHitThisFrame = false; //Prevents multiple attacks from being calculated in one frame

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
		if(!_hasHitThisFrame) //Will only try once per update.
		{
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

		if (col.transform.parent.TryGetComponent(out S_AI_Health EnemyHealth) && _canHitAgain)
		{
			wasDestroyed = EnemyHealth.DealDamage(damage);
		}

		BounceAfterAttack(wasDestroyed, attackType);
	}

	//Calls the method that handles destroying and gaining items from a monitor.
	private void MonitorAttack ( Collider col, S_GeneralEnums.PlayerAttackTypes attackType ) {
		_ObjectInteraction.TriggerMonitor(col);

		BounceAfterAttack(true, attackType); //Will always be destroyed due to monitors not having health.
	}

	//Gets attack state to decide how to affect player after an aerial attack.
	private void BounceAfterAttack ( bool wasDestroyed, S_GeneralEnums.PlayerAttackTypes attackType ) {
		StartCoroutine(DelayAttacks());

		switch (attackType)
		{
			//Player continues unaffected after a roll attack.
			case S_GeneralEnums.PlayerAttackTypes.Rolling:
				break;
				//Force player upwards on hit.
			case S_GeneralEnums.PlayerAttackTypes.SpinJump:
				AttackFromJump();
				break;
			case S_GeneralEnums.PlayerAttackTypes.HomingAttack:
				AttackFromHoming(wasDestroyed);
				break;
		}
	}
	//Bounces player upwards, amount depending on player state.
	void AttackFromJump () {
		switch(_Actions._whatCurrentAction)
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
	private void AttackFromHoming ( bool wasDestroyed ) {
		//If destroyed enemy, will bounce through, if not, will take knockback from it.
		if(_shouldStopOnHit_)
		{
			_Actions._ObjectForActions.GetComponent<S_Action02_Homing>().HittingTarget(S_GeneralEnums.HomingHitResponses.bounceOff);
		}
		if (wasDestroyed)
		{
			_Actions._ObjectForActions.GetComponent<S_Action02_Homing>().HittingTarget(S_GeneralEnums.HomingHitResponses.BounceThrough);
		}
		else
			_Actions._ObjectForActions.GetComponent<S_Action02_Homing>().HittingTarget(S_GeneralEnums.HomingHitResponses.Rebound);
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

