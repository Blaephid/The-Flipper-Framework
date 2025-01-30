using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

public class S_SubAction_Quickstep : S_Action_Base, ISubAction
{


	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity

	//Stats
	#region Stats
	private float	_distanceToStep_;
	private float	_stepDuration_;
	private Vector2     _quickStepCooldown_;
	#endregion

	// Trackers
	#region trackers
	private S_S_ActionHandling.PrimaryPlayerStates _whatActionWasOn;
	private bool	_isSteppingRight;
	private bool	_canStep;
	private bool	_inAir;

	private float       _thisStepSpeed;
	#endregion

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	private void OnEnable () {
		ReadyAction();
	}

	//Only called when enabled, but tracks the time of the quickstep and performs it until its up.
	private void FixedUpdate () {

		if (_distanceToStep_ > 0)
		{
			//If performed in the air but lands, end the step
			if (_inAir && _PlayerPhys._isGrounded)
				_distanceToStep_ = 0;
			else if (!_inAir && !_PlayerPhys._isGrounded)
				_distanceToStep_ = 0;
			//If changed action during the step, end the step.
			if (_whatActionWasOn != _Actions._whatCurrentAction)
				_distanceToStep_ = 0;

			PerformStep();
		}
		if (_distanceToStep_ == 0) 
		{
			_distanceToStep_ = -1;  //To prevent the cooldown being called repeatedly.
			StartCoroutine(CoolDown());
		}
	}


	//Called when attempting to perform an action, checking and preparing inputs.
	new public bool AttemptAction() {
		if (!base.AttemptAction()) return false;

		//Enable Quickstep if in a position to do so, otherwise end the function.
		if (_PlayerVel._horizontalSpeedMagnitude > 10f && !enabled)
		{
			//Gets an input and makes it relevant to camera, then start the action if it's still there.
			if (_Input._RightStepPressed)
			{
				PressRight();
				StartAction();
				return true;
			}
			else if (_Input._LeftStepPressed)
			{
				PressLeft();
				StartAction();
				return true;
			}
		}
		return false;
	}

	//Called when the action is enabled and readies all variables for it to be performed.
	new public void StartAction(bool overwrite = false) {	
		if (_Input._RightStepPressed)
		{
			_isSteppingRight = true;
		}
		else
		{
			_isSteppingRight = false;
		}
		enabled = true;
		
		//Used for checking if the main action changes during the step.
		_whatActionWasOn = _Actions._whatCurrentAction;
		_Actions._whatSubAction = S_S_ActionHandling.SubPlayerStates.Quickstepping;

		//Prevents buttons from being held to spam.
		_Input._RightStepPressed = false;
		_Input._LeftStepPressed = false;

		_Sounds.QuickStepSound();

		SetSpeedAndDistance();
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	//Called every frame to move the character to the left or right.
	private void PerformStep () {

		float toTravel = _thisStepSpeed;

		//Get placement from step and direction of it.
		float dir = _isSteppingRight ? 1 : -1;

		Vector3 velocityInStepDirection = _MainSkin.transform.right * dir * (_thisStepSpeed / Time.fixedDeltaTime);
		velocityInStepDirection = _PlayerPhys.GetRelevantVector(velocityInStepDirection, false);
		velocityInStepDirection = transform.TransformDirection(velocityInStepDirection);

		_PlayerVel.AddGeneralVelocity(velocityInStepDirection, false, false); //This will add velocity to this frame, that will be ignored next update.		

		//Decrease distance by how far moved, this is used to track when the step ends.
		_distanceToStep_ = Mathf.Max(_distanceToStep_ - toTravel, 0);
	}


	//Gets the stats for the activated step to perform, the grounded or air version.
	private void SetSpeedAndDistance () {
		if (_PlayerPhys._isGrounded)
		{
			_stepDuration_ = _Tools.Stats.QuickstepStats.stepDuration;
			_distanceToStep_ = _Tools.Stats.QuickstepStats.stepDistance;
			_inAir = false;
		}
		else
		{
			_distanceToStep_ = _Tools.Stats.QuickstepStats.airStepDistance;
			_stepDuration_ = _Tools.Stats.QuickstepStats.airStepDuration;
			_inAir = true;
		}
		_thisStepSpeed = _distanceToStep_ / _stepDuration_;
	}

	//Called when the action has finished and makes it so it can't be performed again until the frame count is up
	IEnumerator CoolDown () {
		int framesToDelay =(int) (_PlayerPhys._isGrounded ? _quickStepCooldown_.x : _quickStepCooldown_.y);

		for(int i = 0; i < framesToDelay; i++)
			yield return new WaitForFixedUpdate();

		enabled = false;
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//Takes in the input and makes it relevant to the camera, flipping the input so right is always to the right.
	public void PressRight () {
		Vector3 direction = _MainSkin.position - _CamHandler._HedgeCam.transform.position;
		bool _isFacing = Vector3.Dot(_MainSkin.forward, direction.normalized) < 0f;
		if (_isFacing)
		{
			_Input._RightStepPressed = false;
			_Input._LeftStepPressed = true;
		}
	}
	public void PressLeft () {
		Vector3 Direction = _MainSkin.position - _CamHandler._HedgeCam.transform.position;
		bool Facing = Vector3.Dot(_MainSkin.forward, Direction.normalized) < 0f;
		if (Facing)
		{
			_Input._RightStepPressed = true;
			_Input._LeftStepPressed = false;
		}
	}
	#endregion


	public override void AssignTools () {
		base.AssignTools();
	}

	public override void AssignStats () {
		base.AssignStats();
		_quickStepCooldown_ = _Tools.Stats.QuickstepStats.cooldown;
		enabled = false;
	}
}
