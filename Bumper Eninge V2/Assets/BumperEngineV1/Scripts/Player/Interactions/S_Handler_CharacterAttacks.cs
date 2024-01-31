

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

	[HideInInspector] public float BouncingPower;
	[HideInInspector] public float HomingBouncingPower;
	[HideInInspector] public float EnemyHomingStoppingPowerWhenAdditive;
	[HideInInspector] public bool StopOnHomingAttackHit;
	[HideInInspector] public bool StopOnHit;

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
                if (!Player.isRolling)
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
            if (!Player.isRolling)
            {
                airAttack();
            }
        }   
    }

    private void airAttack()
    {
        Vector3 newSpeed = new Vector3(1, 0, 1);

        //From a Jump
        if ((Actions.Action == S_ActionManager.States.Jump || Actions.Action == S_ActionManager.States.Regular) && CanHitAgain)
        {

            AttackFromJump(newSpeed);

        }

        //From a Homing Attack
        else if ((Actions.Action == S_ActionManager.States.Homing || Actions.PreviousAction == S_ActionManager.States.Homing) && CanHitAgain)
        {
            AttackFromHoming(newSpeed);
        }


        //From a Bounce
        else if (Actions.Action == S_ActionManager.States.Bounce && CanHitAgain)
        {
            AttackFromBounce(newSpeed);
        }

        EndAirAttack();
    }

    void AttackFromJump(Vector3 newSpeed)
    {
        StartCoroutine(ResetTriggerBool());

        newSpeed = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
        newSpeed.y = BouncingPower + Mathf.Abs(Player.rb.velocity.y);
        if (newSpeed.y > Player.rb.velocity.y * 1.5f)
            newSpeed.y = Player.rb.velocity.y * 1.5f;

        Player.rb.velocity = newSpeed;
    }

    private void AttackFromHoming(Vector3 newSpeed)
    {
        StartCoroutine(ResetTriggerBool());

        //An additive hit that keeps momentum
        if (Actions.HomingPressed || Actions.JumpPressed)
        {
            newSpeed = new Vector3(Player.rb.velocity.x * 0.8f, HomingBouncingPower, Player.rb.velocity.z * 0.8f);
            Actions.SpecialPressed = false;
            Actions.HomingPressed = false;
            //Debug.Log("Additive Hit");

        }

        //A normal hit that decreases speed
        else
        {
            newSpeed = new Vector3(Player.rb.velocity.x * 0.3f, HomingBouncingPower, Player.rb.velocity.z * 0.15f);
            //Debug.Log("Normal Hit");
        }


        Player.rb.velocity = newSpeed;



        Player.HomingDelay = Tools.stats.HomingSuccessDelay;
    }

    private void AttackFromBounce(Vector3 newSpeed)
    {
        StartCoroutine(ResetTriggerBool());

        newSpeed = new Vector3(1, 0, 1);

        newSpeed = Vector3.Scale(Player.rb.velocity, newSpeed);
        newSpeed.y = HomingBouncingPower * 1.8f;
        Player.rb.velocity = newSpeed;

        Player.HomingDelay = Tools.stats.HomingSuccessDelay;
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
        Actions.ChangeAction(0);
    }

	private IEnumerator ResetTriggerBool()
	{
		CanHitAgain = false;
		yield return new WaitForSeconds(0.05f);
		CanHitAgain = true;
	}

	private void AssignStats()
	{
		BouncingPower = Tools.coreStats.BouncingPower;
		HomingBouncingPower = Tools.coreStats.HomingBouncingPower;
		EnemyHomingStoppingPowerWhenAdditive = Tools.coreStats.EnemyHomingStoppingPowerWhenAdditive;
		StopOnHomingAttackHit = Tools.coreStats.StopOnHomingAttackHit;
		StopOnHit = Tools.coreStats.StopOnHit;

	}

	private void AssignTools()
    {
		Player = GetComponent<S_PlayerPhysics>();
		obj_Int = GetComponent<S_Interaction_Objects>();
		Actions = GetComponent<S_ActionManager>();

		JumpBall = Tools.JumpBall;
	}
}

