using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

public class S_SubAction_Quickstep : MonoBehaviour, ISubAction
{


	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_PlayerPhysics	_PlayerPhys;
	private S_CharacterTools	_Tools;
	private S_ActionManager	_Actions;
	private S_Handler_Camera	_CamHandler;
	private S_PlayerInput	_Input;
	private Transform             _MainSkin;
	private CapsuleCollider       _CharacterCapsule;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	private float	_distanceToStep_;
	private float	_quickStepSpeed_;
	private LayerMask	_StepPlayermask_;
	#endregion

	// Trackers
	#region trackers
	private S_Enums.PrimaryPlayerStates _whatActionWasOn;
	private bool	_isSteppingRight;
	private bool	_canStep;
	private bool	_inAir;
	#endregion

	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Tools = GetComponent<S_CharacterTools>();
		_Actions = GetComponent<S_ActionManager>();
		_MainSkin = _Tools.mainSkin;
		_CamHandler = GetComponent<S_Handler_Camera>();
		_Input = GetComponent<S_PlayerInput>();
		_CharacterCapsule = _Tools.characterCapsule.GetComponent<CapsuleCollider>();
		_StepPlayermask_ = _Tools.Stats.QuickstepStats.StepLayerMask;

		enabled = false;
	}

	// Update is called once per frame
	void Update () {
		
	}

	//Only called when enabled, but tracks the time of the quickstep and performs it until its up.
	private void FixedUpdate () {

		//If performed in the air but lands, end the step
		if (_inAir && _PlayerPhys._isGrounded)
			enabled = false;
		else if (!_inAir && !_PlayerPhys._isGrounded)
			_inAir = true;
		//If changed action during the step, end the step.
		if (_whatActionWasOn != _Actions.whatAction)
			enabled = false;

		if (_distanceToStep_ > 0)
		{
			PerformStep();
		}
		else
		{
			StartCoroutine(CoolDown());
		}
	}


	//Called when attempting to perform an action, checking and preparing inputs.
	public bool AttemptAction() {
		bool willStartAction = false;

		//Enable Quickstep if a position to do so, otherwise end the function.
		if (_PlayerPhys._horizontalSpeedMagnitude > 10f && !enabled)
		{
			//Gets an input and makes it relevant to camera, then start the action if it's still there.
			if (_Input.RightStepPressed)
			{
				PressRight();
				StartAction();
				willStartAction = true;
			}
			else if (_Input.LeftStepPressed)
			{
				PressLeft();
				StartAction();
				willStartAction=true;
			}
		}
		return willStartAction;
	}

	//Called when the action is enabled and readies all variables for it to be performed.
	public void StartAction() {	
		if (_Input.RightStepPressed)
		{
			_isSteppingRight = true;
		}
		else
		{
			_isSteppingRight = false;
		}
		enabled = true;
		
		//Used for checking if the main action changes during the step.
		_whatActionWasOn = _Actions.whatAction;
		_Actions.whatSubAction = S_Enums.SubPlayerStates.Quickstepping;

		//Prevents buttons from being held to spam.
		_Input.RightStepPressed = false;
		_Input.LeftStepPressed = false;

		_canStep = true;

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

		float toTravel = _quickStepSpeed_;

		//Get placement from step and direction of it.
		float dir = _isSteppingRight ? 1 : -1;

		//Check sides based on step direction for if there's a wall preventing the step. If there isn't, change position.
		if (!Physics.BoxCast(transform.position, new Vector3(0, _CharacterCapsule.height / 2, _CharacterCapsule.radius), _MainSkin.right * dir, _MainSkin.rotation, 1.5f, _StepPlayermask_) && _canStep)
		{
			_PlayerPhys.AddGeneralVelocity(_MainSkin.transform.right * dir * toTravel); //This will add velocity to this frame, that will be ignored next update.
			
		}
		else
			enabled = false;

		//Decrease distance by how far moved, this is used to track when the step ends.
		toTravel = Mathf.Clamp(toTravel * Time.fixedDeltaTime, 0, _distanceToStep_);
		_distanceToStep_ -= toTravel;
	}


	//Gets the stats for the activated step to perform, the grounded or air version.
	private void SetSpeedAndDistance () {
		if (_PlayerPhys._isGrounded)
		{
			_quickStepSpeed_ = _Tools.Stats.QuickstepStats.stepSpeed;
			_distanceToStep_ = _Tools.Stats.QuickstepStats.stepDistance;
			_inAir = false;
		}
		else
		{
			_distanceToStep_ = _Tools.Stats.QuickstepStats.airStepDistance;
			_quickStepSpeed_ = _Tools.Stats.QuickstepStats.airStepSpeed;
			_inAir = true;
		}
	}

	//Called when the action has finished and makes it so it can't be performed again until the time is up.
	IEnumerator CoolDown () {
		if (_PlayerPhys._isGrounded)
			yield return new WaitForSeconds(0.05f);
		else
			yield return new WaitForSeconds(0.20f);

		this.enabled = false;
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
			_Input.RightStepPressed = false;
			_Input.LeftStepPressed = true;
		}
	}
	public void PressLeft () {
		Vector3 Direction = _MainSkin.position - _CamHandler._HedgeCam.transform.position;
		bool Facing = Vector3.Dot(_MainSkin.forward, Direction.normalized) < 0f;
		if (Facing)
		{
			_Input.RightStepPressed = true;
			_Input.LeftStepPressed = false;
		}
	}
	#endregion
}
