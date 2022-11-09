

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
	PlayerBhysics Player;
	Objects_Interaction obj_Int;
	ActionManager Actions;
	CharacterTools Tools;

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
		Tools = GetComponent<CharacterTools>();
		AssignTools();

		AssignStats();
	}

	public void AttackThing(Collider col, string AttackType = "", string Target = "Enemy", int damage = 1)
	{
		//If hitting a monitor with a Spinjump
		if (Target == "Monitor" && AttackType == "SpinJump")
		{
			
			if (!Player.isRolling)
			{

				Vector3 newSpeed = new Vector3(1, 0, 1);

				//From a Jump
				if ((Actions.Action == 1 || Actions.Action == 0) && CanHitAgain)
				{

					StartCoroutine(ResetTriggerBool());

					newSpeed = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
					newSpeed.y = BouncingPower + Mathf.Abs(Player.rb.velocity.y);
					if (newSpeed.y > Player.rb.velocity.y * 1.5f)
						newSpeed.y = Player.rb.velocity.y * 1.5f;

					Player.rb.velocity = newSpeed;

				}

				//From a Homing Attack
				else if ((Actions.Action == 2 || Actions.PreviousAction == 2) && CanHitAgain)
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


				//From a Bounce
				else if (Actions.Action == 6 && CanHitAgain)
				{
					StartCoroutine(ResetTriggerBool());

					newSpeed = new Vector3(1, 0, 1);

					newSpeed = Vector3.Scale(Player.rb.velocity, newSpeed);
					newSpeed.y = HomingBouncingPower * 1.8f;
					Player.rb.velocity = newSpeed;

					Player.HomingDelay = Tools.stats.HomingSuccessDelay;

					JumpBall.SetActive(false);
					if (Actions.Action08 != null)
					{
						if (Actions.Action08.DropEffect.isPlaying == true)
						{
							Actions.Action08.DropEffect.Stop();
						}
					}
				}

			}

			obj_Int.updateTargets = true;
			JumpBall.SetActive(false);
			if (Actions.Action08 != null)
			{
				if (Actions.Action08.DropEffect.isPlaying == true)
				{
					Actions.Action08.DropEffect.Stop();
				}
			}
			Actions.ChangeAction(0);
		}


		else
		{
			if (AttackType == "SpinDash")
			{
				col.transform.parent.GetComponent<EnemyHealth>().DealDamage(1);
				obj_Int.updateTargets = true;
			}

			else if (AttackType == "SpinJump")
			{
				
				//Actions.Action01.JumpBall.enabled = false;
				if (col.transform.parent.GetComponent<EnemyHealth>() != null)
				{
					if (!Player.isRolling)
					{

						Vector3 newSpeed = new Vector3(1, 0, 1);

						//From a Jump
						if ((Actions.Action == 1 || Actions.Action == 0) && CanHitAgain)
						{

							StartCoroutine(ResetTriggerBool());

							
							newSpeed = new Vector3(Player.rb.velocity.x, 0, Player.rb.velocity.z);
							newSpeed.y = BouncingPower + Mathf.Abs(Player.rb.velocity.y);
							if (newSpeed.y > Player.rb.velocity.y * 1.5f)
								newSpeed.y = Player.rb.velocity.y * 1.5f;
							
							Player.rb.velocity = newSpeed;

						}

						//From a Homing Attack
						else if ((Actions.Action == 2 || Actions.PreviousAction == 2) && CanHitAgain)
						{
							StartCoroutine(ResetTriggerBool());

							//An additive hit that keeps momentum
							if (Actions.HomingPressed || Actions.JumpPressed)
                            {
								newSpeed = new Vector3(Player.rb.velocity.x * 0.8f, HomingBouncingPower, Player.rb.velocity.z * 0.8f);
								//Actions.SpecialPressed = false;
								//Actions.HomingPressed = false;

							}
							
							//A normal hit that decreases speed
							else
							{ 		
								newSpeed = new Vector3(Player.rb.velocity.x * 0.3f, HomingBouncingPower, Player.rb.velocity.z * 0.15f);
							}
								

							Player.rb.velocity = newSpeed;
							
							
							Player.HomingDelay = Tools.stats.HomingSuccessDelay;
						}


						//From a Bounce
						else if (Actions.Action == 6 && CanHitAgain)
						{
							StartCoroutine(ResetTriggerBool());
							
							newSpeed = new Vector3(1, 0, 1);
							
							newSpeed = Vector3.Scale(Player.rb.velocity, newSpeed);
							newSpeed.y = HomingBouncingPower * 1.8f;
							Player.rb.velocity = newSpeed;

							Player.HomingDelay = Tools.stats.HomingSuccessDelay;

							JumpBall.SetActive(false);
							if (Actions.Action08 != null)
							{
								if (Actions.Action08.DropEffect.isPlaying == true)
								{
									Actions.Action08.DropEffect.Stop();
								}
							}
						}
					}

					col.transform.parent.GetComponent<EnemyHealth>().DealDamage(damage);
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
			}
		}

		Debug.Log(Player.rb.velocity);
	}

	private IEnumerator ResetTriggerBool()
	{
		CanHitAgain = false;
		yield return new WaitForSeconds(0.05f);
		CanHitAgain = true;
	}

	private void AssignStats()
	{
		BouncingPower = Tools.stats.BouncingPower;
		HomingBouncingPower = Tools.stats.HomingBouncingPower;
		EnemyHomingStoppingPowerWhenAdditive = Tools.stats.EnemyHomingStoppingPowerWhenAdditive;
		StopOnHomingAttackHit = Tools.stats.StopOnHomingAttackHit;
		StopOnHit = Tools.stats.StopOnHit;

	}

	private void AssignTools()
    {
		Player = GetComponent<PlayerBhysics>();
		obj_Int = GetComponent<Objects_Interaction>();
		Actions = GetComponent<ActionManager>();

		JumpBall = Tools.JumpBall;
	}
}

