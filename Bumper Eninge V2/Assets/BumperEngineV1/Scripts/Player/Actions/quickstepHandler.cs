using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quickstepHandler : MonoBehaviour
{
	PlayerBhysics Player;
	CharacterTools Tools;
	ActionManager Actions;
	CameraControl Cam;
	ActionManager.States startAction;

	Animator CharacterAnimator;

	float DistanceToStep;
	float quickStepSpeed;
	LayerMask StepPlayermask;
	RaycastHit hit;

	bool StepRight;
	float StepCounter;
	bool canStep;
	bool air;

	float timeTrack;

    private void Awake()
    {
        Player = GetComponent<PlayerBhysics>();
		Tools = GetComponent<CharacterTools>();
		Actions = GetComponent<ActionManager>();
		CharacterAnimator = Tools.CharacterAnimator;
		Cam = Tools.GetComponent<CameraControl>();

		StepPlayermask = Tools.coreStats.StepLayerMask;

		this.enabled = false;
	}

	public void pressRight()
    {
		Vector3 Direction = CharacterAnimator.transform.position - Cam.Cam.transform.position;
		bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < 0f;
		if (Facing)
		{
			Actions.RightStepPressed = false;
			Actions.LeftStepPressed = true;
		}
	}

	public void pressLeft()
    {
		Vector3 Direction = CharacterAnimator.transform.position - Cam.Cam.transform.position;
		bool Facing = Vector3.Dot(CharacterAnimator.transform.forward, Direction.normalized) < 0f;
		if (Facing)
		{
			Actions.RightStepPressed = true;
			Actions.LeftStepPressed = false;
		}
	}

    public void initialEvents(bool right)
    {
		startAction = Actions.Action;

		if (Actions.eventMan != null) Actions.eventMan.quickstepsPerformed += 1;

		timeTrack = 0;

		if (right)
        {

			Actions.RightStepPressed = false;
			Actions.LeftStepPressed = false;


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
				quickStepSpeed = Tools.stats.AirStepSpeed;
				air = true;
			}
						
		}
		else
        {
			Actions.RightStepPressed = false;
			Actions.LeftStepPressed = false;

			
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
				quickStepSpeed = Tools.stats.AirStepSpeed;
				air = true;
			}
		}

	}

    // Update is called once per frame
    void FixedUpdate()
    {

		timeTrack = Time.fixedDeltaTime;

		if (air && Player.Grounded)
			this.enabled = false;
		else if (!air && !Player.Grounded)
			air = true;

		if (startAction != Actions.Action)
			DistanceToStep = 0;
	
		if (DistanceToStep > 0)
		{
			//Debug.Log(DistanceToStep);

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
			yield return new WaitForSeconds(0.05f);
		else
			yield return new WaitForSeconds(0.20f);

		this.enabled = false;
    }
}
