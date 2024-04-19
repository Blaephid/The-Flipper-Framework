using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;


public class S_Interaction_Objects : MonoBehaviour
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties

	[Header("Scripts")]
	//Player
	private S_CharacterTools	_Tools;
	private S_PlayerPhysics	_PlayerPhys;
	private S_ActionManager	_Actions;
	private S_PlayerInput	_Input;

	private S_Handler_CharacterAttacks	_AttackHandler;
	private S_Handler_HealthAndHurt	_HurtAndHealth;

	private S_Handler_Camera      _CamHandler;
	private S_Control_SoundsPlayer _Sounds;

	//External
	private S_Control_MovingPlatform	_PlatformScript;

	[Header("Unity Objects")]
	private GameObject	_JumpBall;
	private Animator	_CharacterAnimator;

	[Header("For Rings, Springs and so on")]
	public GameObject RingCollectParticle;
	public Material SpeedPadTrack;
	public Material DashRingMaterial;
	public Material NormalShieldMaterial;
	public Color DashRingLightsColor;

	private S_Spawn_UI.StrucCoreUIElements _CoreUIElements;
	#endregion

	//Stats
	#region Stats
	#endregion

	// Trackers
	#region trackers
	private Vector3	_translateOnPlatform;

	[HideInInspector] 
	public float	_displaySpeed;

	#endregion

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited
	private void Start () {
		if (_PlayerPhys == null)
		{
			AssignTools(); //Called during start instead of awake because it gives time for tools to be acquired (such as the UI needing to be spawned).
		}
	}

	//Displays rings and speed on UI
	private void LateUpdate () {
		UpdateSpeed();

		_CoreUIElements.RingsCounter.text = ": " + (int)_HurtAndHealth._ringAmount;
	}

	void FixedUpdate () {
		FollowPlatform();
	}

	public void EventTriggerEnter ( Collider Col ) {
		switch (Col.tag)
		{
			case "SpeedPad":
				LaunchFromPadOrDashRing(Col);
				break;

			case "Switch":
				if (Col.GetComponent<S_Data_Switch>() != null)
				{
					Col.GetComponent<S_Data_Switch>().Activate();
				}
				break;
			case "Spring":
				LaunchFromSpring(Col);
				break;

			case "Bumper":
				
				break;
			case "Wind":
				if (Col.TryGetComponent(out S_Trigger_Updraft UpdraftScript))
				{
					if (_Actions._whatAction == S_Enums.PrimaryPlayerStates.Hovering)
					{
						GetComponent<S_Action13_Hovering>().updateHover(UpdraftScript);
					}
					else
					{
						GetComponent<S_Action13_Hovering>().InitialEvents(UpdraftScript);
						_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Hovering);
					}
				}
				break;

			case "HintRing":
				ActivateHintBox(Col);
				break;

			case "Monitor":
				Col.GetComponentInChildren<BoxCollider>().enabled = false;
				_AttackHandler.AttemptAttackOnContact(Col, S_Enums.AttackTargets.Monitor);
				break;

			case "Ring":
				StartCoroutine(_HurtAndHealth.GainRing(1f, Col, RingCollectParticle));
				break;

			case "Ring Road":
				StartCoroutine(_HurtAndHealth.GainRing(0.5f, Col, RingCollectParticle));
				break;

			case "MovingRing":
				if (Col.TryGetComponent(out S_MovingRing MovingRingScript))
				{
					if (MovingRingScript.colectable)
					{
						StartCoroutine(_HurtAndHealth.GainRing(1f, Col, RingCollectParticle));
					}
				}
				break;
		}
	}

	public void EventTriggerExit ( Collider col ) {
		if (col.tag == "Wind")
		{
			GetComponent<S_Action13_Hovering>().inWind = false;
		}
	}

	public void EventTriggerStay ( Collider col ) {

		if (col.gameObject.tag == "MovingPlatform")
		{
			_PlatformScript = col.gameObject.GetComponent<S_Control_MovingPlatform>();
		}
		else
		{
			_PlatformScript = null;
		}

	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	//Called every frame
	private void UpdateSpeed () {
		switch (_Actions._whatAction)
		{
			default:
				_displaySpeed = _PlayerPhys._horizontalSpeedMagnitude;
				break;

			case S_Enums.PrimaryPlayerStates.WallRunning:
				if (_Actions.Action12._runningSpeed > _Actions.Action12._climbingSpeed)
					_displaySpeed = _Actions.Action12._runningSpeed;
				else
					_displaySpeed = _Actions.Action12._climbingSpeed;
				break;
		}

		if (_CoreUIElements.SpeedCounter != null && _PlayerPhys._speedMagnitude > 10f) _CoreUIElements.SpeedCounter.text = _displaySpeed.ToString("F0");
		else if (_CoreUIElements.SpeedCounter != null && _displaySpeed < 10f) _CoreUIElements.SpeedCounter.text = "0";
	}

	private void FollowPlatform () {
		if (_PlatformScript != null)
		{
			transform.position += (-_PlatformScript.TranslateVector);
		}
		if (!_PlayerPhys._isGrounded)
		{
			_PlatformScript = null;
		}
	}

	//Called on triggers

	private void LaunchFromPadOrDashRing (Collider Col) {
		if (!Col.TryGetComponent(out S_Data_SpeedPad SpeedPadScript)) { return; } //Ensures object has necessary script, and saves as varaible for efficiency.

		Col.GetComponent<AudioSource>().Play();

		if (SpeedPadScript._isOnRail_)
		{

			if (_Actions._whatAction != S_Enums.PrimaryPlayerStates.Rail)
			{
				transform.position = Col.GetComponent<S_Data_SpeedPad>()._PositionToLockTo.position;
			}
			else
			{
				StartCoroutine(GetComponent<S_Action05_Rail>().ApplyBoost(SpeedPadScript._speedToSet_, SpeedPadScript._willSetSpeed_, SpeedPadScript._addSpeed_, SpeedPadScript._willSetBackwards_));
			}
			return;
		}


		else if (!Col.GetComponent<S_Data_SpeedPad>()._Path)
		{

			_Actions._isAirDashAvailables = true;

			transform.rotation = Quaternion.identity;
			//ResetPlayerRotation

			Vector3 lockpos;
			if (Col.GetComponent<S_Data_SpeedPad>()._PositionToLockTo != null)
				lockpos = Col.GetComponent<S_Data_SpeedPad>()._PositionToLockTo.position;
			else
				lockpos = Col.transform.position;


			if (SpeedPadScript._lockToDirection_)
			{
				float speed = Col.GetComponent<S_Data_SpeedPad>()._speedToSet_;
				if (speed < _PlayerPhys._horizontalSpeedMagnitude)
					speed = _PlayerPhys._horizontalSpeedMagnitude;

				if (!SpeedPadScript._isDashRing_)
					StartCoroutine(ApplyForceAfterDelay(Col.transform.forward * speed, lockpos, Vector3.zero));
				else
					StartCoroutine(ApplyForceAfterDelay(Col.transform.forward * Col.GetComponent<S_Data_SpeedPad>()._speedToSet_, lockpos, Vector3.zero));
			}
			else
			{
				_PlayerPhys.AddCoreVelocity(Col.transform.forward * Col.GetComponent<S_Data_SpeedPad>()._speedToSet_);

				if (Col.GetComponent<S_Data_SpeedPad>()._willSnap)
				{
					transform.position = lockpos;
				}
			}


			if (SpeedPadScript._isDashRing_)
			{

				_Actions._ActionDefault.CancelCoyote();
				_Actions._ActionDefault.StartAction();
				_CharacterAnimator.SetBool("Grounded", false);

				if (SpeedPadScript._willLockAirMoves_)
				{
					StopCoroutine(_Actions.lockAirMoves(SpeedPadScript._lockAirMovesFor_));
					StartCoroutine(_Actions.lockAirMoves(SpeedPadScript._lockAirMovesFor_));
				}

				if (SpeedPadScript._overwriteGravity_ != Vector3.zero)
				{
					StartCoroutine(LockPlayerGraivtyUntilGrounded(SpeedPadScript._overwriteGravity_));
				}



			}
			else
			{
				transform.up = Col.transform.up;
				_CharacterAnimator.transform.forward = Col.transform.forward;
			}

			if (SpeedPadScript._willLockControl)
			{
				_Input.LockInputForAWhile(SpeedPadScript._lockControlFor_, true, Col.transform.forward, SpeedPadScript._lockInputTo_);
			}
			if (SpeedPadScript._willAffectCamera_)
			{
				Vector3 dir = Col.transform.forward;
				_CamHandler._HedgeCam.SetCamera(dir, 2.5f, 20, 5f, 1);

			}

		}
	}

	private void LaunchFromSpring (Collider Col) {
		if (!Col.TryGetComponent(out S_Data_Spring SpringScript)) { return; } //Ensures object has necessary script, and saves as varaible for efficiency.

		//Immediate effects on player
		_Actions._ActionDefault.CancelCoyote(); //Ensures can't make a normal jump being launched.
		_PlayerPhys._isGravityOn = true; //Counter acts any actions that might have disabled this.
		_Actions._isAirDashAvailables = true;
		_Actions._jumpCount = 1;

		_Actions._ActionDefault.StartAction();
		_CharacterAnimator.SetBool("Grounded", false);

		_PlayerPhys._OnTriggerAirLauncher.Invoke();

		//Prevents immediate air actions.
		_Input.JumpPressed = false;
		_Input.SpecialPressed = false;
		_Input.BouncePressed = false;

		//Calculate force

		//Since vertical will be taken over by environment, get horizontal core velocity.
		Vector3 newCoreVelocity = _PlayerPhys.GetRelevantVel(_PlayerPhys._coreVelocity, false);
		Vector3 direction = SpringScript._BounceTransform.up;
		Vector3 bounceHorizontalVelocity = _PlayerPhys.GetRelevantVel(direction * SpringScript._springForce_, false); //Combined the spring direction with force to get the only the force horizontally.

		//If spring should not take complete control of player velocity, calculate direction based on movement into spring, including spring direction.
		//Horizontal speed is calculated using core velocity, while vertical is environmental. Horizontal cannot be greater than the larger of running speed or launch speed.
		if (SpringScript._keepHorizontal_)
		{
			Vector3 combinedVelocityDirection = (bounceHorizontalVelocity * 2) + newCoreVelocity; //The direction of the two put together, with the bounce being prioritised.
			Vector3 combinedVelocityMagnitude = (bounceHorizontalVelocity + newCoreVelocity); //The magnitude of the two put together.
			Vector3 upDirection = new Vector3(0, direction.y, 0);

			//If the velocity after bounce is greater than velocity going in to bounce, the take the larger of the two that made it, without losing direction. This will prevent speed increasing too much.
			if (combinedVelocityMagnitude.sqrMagnitude > newCoreVelocity.sqrMagnitude)
			{
				//Rather than using Max / Min, use IF statements to compare with sqrMagnitude before getting an actual "magnitude".
				if (bounceHorizontalVelocity.sqrMagnitude > newCoreVelocity.sqrMagnitude)
				{
					newCoreVelocity = combinedVelocityDirection.normalized * bounceHorizontalVelocity.magnitude;
				}
				else
				{
					newCoreVelocity = combinedVelocityDirection.normalized * newCoreVelocity.magnitude;
				}
			}
			else
			{
				newCoreVelocity = combinedVelocityMagnitude;
			}

			StartCoroutine(ApplyForceAfterDelay(upDirection * SpringScript._springForce_, SpringScript._BounceTransform.position, newCoreVelocity));
		}
		//If not keeping horizontal, then player will always travel along the same "path" created by this instance until control is restored or their stats change. See S_drawShortDirection for a representation of this path as a gizmo.
		else
		{
			//While the player will always move at the same velocity, the combination between environmental and core can vary, with one being prioritised.
			//This is because if the player enters a spring at speed, they will want to keep that speed when the spring is finished.
			//Core velocity vertically is removed, and handled by environment, but horizontal will be a combo of both velocity types, both going in the same direction.
			float horizontalSpeed = bounceHorizontalVelocity.magnitude; //Get the total speed that will actually be applied in world horizontally.

			float horizontalEnvSpeed = Mathf.Max(horizontalSpeed -  _PlayerPhys._horizontalSpeedMagnitude, 1); //Environmental force will be added to make up for the speed lacking before going into the spring.

			//The value of core over velocity will either be what it was before (as environment makes up for whats lacking), or the bounce force itself (decreasing running speed if need be)
			float coreSpeed = _PlayerPhys._horizontalSpeedMagnitude;
			if(coreSpeed > horizontalSpeed) { 
				horizontalEnvSpeed = 1; //Ensure's theres still a direction even though this won't factor in much to world velocity.
				coreSpeed = horizontalSpeed; //In this case, bounce will be entirely through core velocity, not environmental.
			}
			//else { coreSpeed = horizontalSpeed - horizontalEnvSpeed; }

			//This is all in order to prevent springs being used to increase running speed, as the players running speed will not change if they don't unless they have control (most springs should take control away temporarily).

			Vector3 totalEnvironment = (bounceHorizontalVelocity.normalized * horizontalEnvSpeed) + (new Vector3(0, (direction * SpringScript._springForce_).y,0));

			StartCoroutine(ApplyForceAfterDelay(totalEnvironment, SpringScript._BounceTransform.position, bounceHorizontalVelocity.normalized * coreSpeed));
		}


		//Additional effects based on sprint instance properties.

		//Locks input to nothing, preventing turning and enforcing deceleration.
		if (SpringScript._willLockControl_)
		{
			_Input.LockInputForAWhile(SpringScript._lockForFrames_, false, Vector3.zero, SpringScript._LockInputTo_);
		}

		//Prevents using air moves until after some time
		if (SpringScript._lockAirMovesTime_ > 0)
		{
			StopCoroutine(_Actions.lockAirMoves(SpringScript._lockAirMovesTime_)); //Overwrites coroutine if already in use, resetting the timer.
			StartCoroutine(_Actions.lockAirMoves(SpringScript._lockAirMovesTime_));
		}

		//Since a new character may be created with different gravity to the normal, this temporarily changes gravity to ensure all launch angle will not be affected by chracter's gravity stats.
		if (SpringScript._overwriteGravity_ != Vector3.zero)
		{
			StartCoroutine(LockPlayerGraivtyUntilGrounded(SpringScript._overwriteGravity_));
		}

		//Spring effects
		if (Col.GetComponent<AudioSource>()) { Col.GetComponent<AudioSource>().Play(); }
		if (SpringScript._Animator != null)
			SpringScript._Animator.SetTrigger("Hit");
	}

	//To ensure force is accurate, and player is in start position, spend a few frames to lock them in position, before chaning velocity.
	private IEnumerator ApplyForceAfterDelay ( Vector3 environmentalVelocity, Vector3 position, Vector3 coreVelocity, int frames = 3 ) {

		_PlayerPhys._listOfCanControl.Add(false); //Prevents any input interactions changing core velocity while locked here.

		//Player rotation. Will be determined by the force direction. Usually based on core, but if that isnt present, based on environment.
		if (coreVelocity.sqrMagnitude > 1)
		{
			_Actions._ActionDefault.SetSkinRotationToVelocity(0, coreVelocity);
		}
		else
		{
			_Actions._ActionDefault.SetSkinRotationToVelocity(0, environmentalVelocity);
		}

		//Keep the player in position, with zero velocity, until delay is over.
		for (int i = 0 ; i < frames ; i++)
		{
			_PlayerPhys.transform.position = position;
			_PlayerPhys.SetCoreVelocity(Vector3.zero, true);
			_PlayerPhys.SetBothVelocities(Vector3.zero, Vector2.one);
			yield return new WaitForFixedUpdate();
		}

		_Actions._ActionDefault.StartAction(); //Ensures player is still in correct state after delay.

		_PlayerPhys.transform.position = position; //Ensures player is set to inside of spring, so bounce is consistant. 

		_PlayerPhys._listOfCanControl.RemoveAt(0);

		_PlayerPhys.SetCoreVelocity(coreVelocity, true); //Undoes this being set to zero during delay.

		_PlayerPhys.SetEnvironmentalVelocity(environmentalVelocity, true, true, S_Enums.ChangeLockState.Lock); //Apply bounce
	}


	private void ActivateHintBox (Collider Col) {
		if(!Col.TryGetComponent(out S_Data_HintRing HintRingScript)){ return; } //Ensures object has necessary script, and saves as varaible for efficiency.

		if(Col.gameObject == _CoreUIElements.HintBox._CurrentHintRing) { return; } //Do not perform function if this hint is already being displayed in the hintBox. Prevents restarting a hint when hitting it multiple times until its complete.
		_CoreUIElements.HintBox._CurrentHintRing = Col.gameObject; //Relevant to the above check.

		//Effects
		HintRingScript.hintSound.Play();

		//Using mouse is set when _PlayerInput detects a camera or move input coming from a keyboard or mouse, and this ensures the correct text will be displayed to match the controller device.
		if (_Input.usingMouse)
		{
			_CoreUIElements.HintBox.ShowHint(HintRingScript.hintText, HintRingScript.hintDuration);
		}

		//If not using a mouse, must be using a gamepad.
		else
		{
			Gamepad input = Gamepad.current;

			//Depending on the type of input, will set the display string array to the one matching that input.
			//Note, this could be done much better. This version requires copying out the same data for every array for each input on the hint ring object, but a system could be built to have only one array using 
			//KEYWORDS that are replaced with different strings matching the input. E.G. "Press the JUMPBUTTON to", replaces JUMPBUTTON with a string matching the binding for the current input in the PlayerInput file.
			switch (input)
			{
				case (SwitchProControllerHID):
					_CoreUIElements.HintBox.ShowHint(HintRingScript.hintTextGamePad, HintRingScript.hintDuration);
					break;
				case (DualSenseGamepadHID):
					_CoreUIElements.HintBox.ShowHint(HintRingScript.hintTextPS4, HintRingScript.hintDuration);
					break;
				case (DualShock3GamepadHID):
					_CoreUIElements.HintBox.ShowHint(HintRingScript.hintTextPS4, HintRingScript.hintDuration);
					break;
				case (DualShock4GamepadHID):
					_CoreUIElements.HintBox.ShowHint(HintRingScript.hintTextPS4, HintRingScript.hintDuration);
					break;
				case (DualShockGamepad):
					_CoreUIElements.HintBox.ShowHint(HintRingScript.hintTextPS4, HintRingScript.hintDuration);
					break;
				case (XInputController):
					_CoreUIElements.HintBox.ShowHint(HintRingScript.hintTextXbox, HintRingScript.hintDuration);
					break;
				//If input is none of the above, display the default.
				default:
					_CoreUIElements.HintBox.ShowHint(HintRingScript.hintText, HintRingScript.hintDuration);
					break;
			}
		}	
	}

	//Until the players hit the ground, all gravity calculations will use the set gravity value.
	private IEnumerator LockPlayerGraivtyUntilGrounded ( Vector3 newGrav ) {

		//Set to new value
		_PlayerPhys._currentFallGravity = newGrav;
		_PlayerPhys._currentUpwardsFallGravity = newGrav;

		yield return new WaitForSeconds(0.2f); //To ensure player has time to be set to not grounded.

		//Keep checkng for if player is grounded.
		while (true)
		{
			yield return new WaitForFixedUpdate();
			if (_PlayerPhys._isGrounded)
				break;
		}

		//Set back to normal.
		_PlayerPhys._currentFallGravity = _PlayerPhys._startFallGravity_;
		_PlayerPhys._currentUpwardsFallGravity = _PlayerPhys._gravityWhenMovingUp_;
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//Called by the attack script to apply benefits from monitors.
	public void TriggerMonitor ( Collider col ) {
		if(!col.TryGetComponent(out S_Data_Monitor MonitorData)) { return; } //Ensures the collider has a monitor script.

		//Monitors data
		if (MonitorData.Type == MonitorType.Ring) //Increases player ring count.
		{
			_HurtAndHealth._ringAmount = (int)GetComponent<S_Handler_HealthAndHurt>()._ringAmount + col.GetComponent<S_Data_Monitor>().RingAmount;
		}
		else if (MonitorData.Type == MonitorType.Shield) //Activates shield
		{
			_HurtAndHealth.SetShield(true);
		}

		MonitorData.DestroyMonitor();
	}
	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	private void AssignTools () {
		_Tools =		GetComponentInParent<S_CharacterTools>();
		_PlayerPhys =	_Tools.GetComponent<S_PlayerPhysics>();
		_CamHandler =	_Tools.CamHandler;
		_Actions =	_Tools.GetComponent<S_ActionManager>();
		_Input =		_Tools.GetComponent<S_PlayerInput>();
		_AttackHandler =	GetComponent<S_Handler_CharacterAttacks>();
		_HurtAndHealth =	_Tools.GetComponent<S_Handler_HealthAndHurt>();

		_CharacterAnimator =	_Tools.CharacterAnimator;
		_Sounds =			_Tools.SoundControl;
		_JumpBall =		_Tools.JumpBall;
		_CoreUIElements =		_Tools.UISpawner._BaseUIElements;
	}
	#endregion
}
