using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quickstepManager : MonoBehaviour
{
	PlayerBhysics Player;
	CharacterTools Tools;
	ActionManager Actions;

	Animator CharacterAnimator;


	bool QuickStepping;

	float DistanceToStep;
	float quickStepSpeed;
	LayerMask StepPlayermask;
	RaycastHit hit;

	bool StepRight;
	float StepCounter;
	bool canStep;
	bool air;


    private void Awake()
    {
        Player = GetComponent<PlayerBhysics>();
		Tools = GetComponent<CharacterTools>();
		Actions = GetComponent<ActionManager>();
		CharacterAnimator = Tools.CharacterAnimator;

		StepPlayermask = Tools.coreStats.StepLayerMask;

		this.enabled = false;
	}


    public void initialEvents(bool right)
    {
		if(right)
        {

			Actions.RightStepPressed = false;
			Actions.LeftStepPressed = false;

			
			QuickStepping = true;
			canStep = true;
			StepRight = true;
			if (Player.Grounded)
            {
				quickStepSpeed = Tools.stats.StepSpeed;
				DistanceToStep = Tools.stats.StepDistance;
				air = false;
			}
			else
            {
				DistanceToStep = Tools.stats.AirStepDistance;
				DistanceToStep = Tools.stats.AirStepSpeed;
				air = true;
			}
				

			
		}
		else
        {
			Actions.RightStepPressed = false;
			Actions.LeftStepPressed = false;

			
			QuickStepping = true;
			canStep = true;
			StepRight = false;
			if (Player.Grounded)
			{
				quickStepSpeed = Tools.stats.StepSpeed;
				DistanceToStep = Tools.stats.StepDistance;
				air = false;
			}
			else
			{
				DistanceToStep = Tools.stats.AirStepDistance;
				DistanceToStep = Tools.stats.AirStepSpeed;
				air = true;
			}


		}

	}

    // Update is called once per frame
    void FixedUpdate()
    {
		if (air && Player.Grounded)
			this.enabled = false;
		else if (!air && !Player.Grounded)
			air = true;
	
		if (DistanceToStep > 0)
		{
			float stepSpeed = quickStepSpeed;

			//Debug.Log(stepSpeed);

			if (StepRight)
			{
				Vector3 positionTo = transform.position + (CharacterAnimator.transform.right * DistanceToStep);
				float ToTravel = stepSpeed * Time.deltaTime;

				if (DistanceToStep - ToTravel <= 0)
				{
					ToTravel = DistanceToStep;
					DistanceToStep = 0;
				}

				DistanceToStep -= ToTravel;

				if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.35f, transform.position.z), CharacterAnimator.transform.right * 1, out hit, 1.5f, StepPlayermask) && canStep)
					if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), CharacterAnimator.transform.right * 1, out hit, .8f, StepPlayermask))
						transform.position = Vector3.MoveTowards(transform.position, positionTo, ToTravel);
					else
						canStep = false;
			}

			// !(Physics.Raycast(transform.position, CharacterAnimator.transform.right * -1, out hit, 4f, StepPlayermask)
			else if (!StepRight)
			{
				Vector3 positionTo = transform.position + (-CharacterAnimator.transform.right * DistanceToStep);
				float ToTravel = stepSpeed * Time.deltaTime;

				if (DistanceToStep - ToTravel <= 0)
				{
					ToTravel = DistanceToStep;
					DistanceToStep = 0;
				}

				DistanceToStep -= ToTravel;

				if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.35f, transform.position.z), CharacterAnimator.transform.right * -1, out hit, 1.5f, StepPlayermask) && canStep)
					if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), CharacterAnimator.transform.right * -1, out hit, .8f, StepPlayermask))
						transform.position = Vector3.MoveTowards(transform.position, positionTo, ToTravel);
					else
						canStep = false;
			}

		}

		else
		{
			StartCoroutine(CoolDown());
			
		}

	}

	IEnumerator CoolDown()
    {
		if (Player.Grounded)
			yield return new WaitForSeconds(0.06f);
		else
			yield return new WaitForSeconds(0.25f);

		this.enabled = false;
    }
}
