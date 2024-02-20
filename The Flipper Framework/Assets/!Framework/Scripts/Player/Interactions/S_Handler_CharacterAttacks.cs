

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_CharacterAttacks : MonoBehaviour
{
	S_PlayerPhysics Player;
	S_Interaction_Objects obj_Int;
	S_ActionManager Actions;
	S_CharacterTools Tools;

	GameObject JumpBall;

	[Header("Enemies")]

	[HideInInspector] public float _bouncingPower_;
	[HideInInspector] public float _homingBouncingPower_;
	[HideInInspector] public float _enemyHomingStoppingPowerWhenAdditive_;
	[HideInInspector] public bool _shouldStopOnHomingAttackHit_;
	[HideInInspector] public bool _shouldStopOnHit_;

	private bool CanHitAgain = true;

	private void Start()
	{
		Tools = GetComponent<S_CharacterTools>();
		AssignTools();

		AssignStats();
	}

	public void AttackThing(Collider col, string AttackType = "", string Target = "Enemy", int damage = 1)
	{
		switch(Target)
		{
			case "Monitor":
				if (AttackType == "SpinJump")
					MonitorAttack(AttackType);
				break;

			case "Enemy":
				EnemyAttack( AttackType, col, damage);
                break;
		}

	}

    private void EnemyAttack(string AttackType, Collider col, int damage)
    {
        if (AttackType == "SpinDash")
        {
            col.transform.parent.GetComponent<S_AI_Health>().DealDamage(1);
            obj_Int.updateTargets = true;
        }

        else if (AttackType == "SpinJump")
        {
            if (col.transform.parent.GetComponent<S_AI_Health>() != null)
            {
                if (!Player._isRolling)
                {
                    col.transform.parent.GetComponent<S_AI_Health>().DealDamage(damage);
                    airAttack();
                }
            }
        }
    }

	private void MonitorAttack(string AttackType)
	{
        if(AttackType == "SpinJump")
        {
            if (!Player._isRolling)
            {
                airAttack();
            }
        }   
    }

    private void airAttack()
    {
        Vector3 newSpeed = new Vector3(1, 0, 1);

        //From a Jump
        if ((Actions.whatAction == S_Enums.PlayerStates.Jump || Actions.whatAction == S_Enums.PlayerStates.Regular) && CanHitAgain)
        {

            AttackFromJump(newSpeed);

        }

        //From a Homing Attack
        else if ((Actions.whatAction == S_Enums.PlayerStates.Homing || Actions.whatPreviousAction == S_Enums.PlayerStates.Homing) && CanHitAgain)
        {
            AttackFromHoming(newSpeed);
        }


        //From a Bounce
        else if (Actions.whatAction == S_Enums.PlayerStates.Bounce && CanHitAgain)
        {
            AttackFromBounce(newSpeed);
        }

        EndAirAttack();
    }

    void AttackFromJump(Vector3 newSpeed)
    {
        StartCoroutine(ResetTriggerBool());

        newSpeed = new Vector3(Player._RB.velocity.x, 0, Player._RB.velocity.z);
        newSpeed.y = _bouncingPower_ + Mathf.Abs(Player._RB.velocity.y);
        if (newSpeed.y > Player._RB.velocity.y * 1.5f)
            newSpeed.y = Player._RB.velocity.y * 1.5f;

        Player.SetTotalVelocity(newSpeed);
    }

    private void AttackFromHoming(Vector3 newSpeed)
    {
        StartCoroutine(ResetTriggerBool());

        //An additive hit that keeps momentum
        if (Actions.HomingPressed || Actions.JumpPressed)
        {
            newSpeed = new Vector3(Player._RB.velocity.x * 0.8f, _homingBouncingPower_, Player._RB.velocity.z * 0.8f);
            Actions.SpecialPressed = false;
            Actions.HomingPressed = false;
            //Debug.Log("Additive Hit");

        }

        //A normal hit that decreases speed
        else
        {
            newSpeed = new Vector3(Player._RB.velocity.x * 0.3f, _homingBouncingPower_, Player._RB.velocity.z * 0.15f);
            //Debug.Log("Normal Hit");
        }


        Player.SetTotalVelocity(newSpeed);



        Player._homingDelay_ = Tools.Stats.HomingStats.successDelay;
    }

    private void AttackFromBounce(Vector3 newSpeed)
    {
        StartCoroutine(ResetTriggerBool());

        newSpeed = new Vector3(1, 0, 1);

        newSpeed = Vector3.Scale(Player._RB.velocity, newSpeed);
        newSpeed.y = _homingBouncingPower_ * 1.8f;
        Player.SetTotalVelocity(newSpeed);

        Player._homingDelay_ = Tools.Stats.HomingStats.successDelay;
    }

    private void EndAirAttack()
    {
        obj_Int.updateTargets = true;
        JumpBall.SetActive(false);
        if (Actions.Action08 != null)
        {
            if (Actions.Action08.DropEffect.isPlaying == true)
            {
                Actions.Action08.DropEffect.Stop();
            }
        }
        Actions.SpecialPressed = false;
        Actions.HomingPressed = false;
        Actions.ChangeAction(S_Enums.PlayerStates.Regular);
    }

	private IEnumerator ResetTriggerBool()
	{
		CanHitAgain = false;
		yield return new WaitForSeconds(0.05f);
		CanHitAgain = true;
	}

	private void AssignStats()
	{
		_bouncingPower_ = Tools.Stats.EnemyInteraction.bouncingPower;
		_homingBouncingPower_ = Tools.Stats.EnemyInteraction.homingBouncingPower;
		_enemyHomingStoppingPowerWhenAdditive_ = Tools.Stats.EnemyInteraction.enemyHomingStoppingPowerWhenAdditive;
		_shouldStopOnHomingAttackHit_ = Tools.Stats.EnemyInteraction.shouldStopOnHomingAttackHit;
		_shouldStopOnHit_ = Tools.Stats.EnemyInteraction.shouldStopOnHit;

	}

	private void AssignTools()
    {
		Player = GetComponent<S_PlayerPhysics>();
		obj_Int = GetComponent<S_Interaction_Objects>();
		Actions = GetComponent<S_ActionManager>();

		JumpBall = Tools.JumpBall;
	}
}

