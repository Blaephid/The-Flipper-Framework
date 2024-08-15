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
using UnityEngine.Rendering.Universal;
using UnityEditor.PackageManager;
using NUnit.Framework;
using Unity.VisualScripting;
using System;
using System.Xml.Linq;


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
	private S_CharacterTools      _Tools;
	private S_PlayerPhysics       _PlayerPhys;
	private S_ActionManager       _Actions;
	private S_PlayerInput         _Input;
	private S_PlayerEvents        _Events;

	private S_Handler_CharacterAttacks      _AttackHandler;
	private S_Handler_HealthAndHurt         _HurtAndHealth;

	private S_Handler_Camera      _CamHandler;
	private S_Control_SoundsPlayer _Sounds;

	private Transform             _FeetPoint;

	//External
	private GameObject                       _PlatformAnchor;

	[Header("Unity Objects")]
	private Animator    _CharacterAnimator;

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
	private Vector3     _translateOnPlatform;

	[HideInInspector]
	public float        _displaySpeed;

	private Vector3     _previousPlatformPointPosition;

	[Header("Wind Force")]
	[HideInInspector]
	public bool         _canHover; //Only true if Hovering AttemptAction is called, and decides if can enter said action.
	private Vector3      _currentWindDirection;
	[HideInInspector]
	public Vector3      _totalWindDirection;
	private int         _numberOfWindForces; //How many winds are currently operating on the player. Up when entering one, down when exiting one.
	private int         _windCounter; //0 At the start of every frame, and goes up for each wind calculaton, when equal to number of wind forces, it's at the last one.

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

	private void Update () {
		FollowPlatform();
	}

	private void FixedUpdate () {

		//For tracking wind forces
		_currentWindDirection = Vector3.zero;
		_windCounter = 0;
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
				_numberOfWindForces += 1;
				S_Trigger_Updraft UpdraftScript = Col.GetComponentInParent<S_Trigger_Updraft>();
				if (UpdraftScript != null)
				{
					if (Col.transform.up.y > 0.7f)
					{
						_PlayerPhys._listOfIsGravityOn.Add(false);
						_PlayerPhys.SetIsGrounded(false);
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
					//The script handles this, applying a delay after being spawned until this is true.
					if (MovingRingScript._isCollectable)
					{
						StartCoroutine(_HurtAndHealth.GainRing(1f, Col, RingCollectParticle));
					}
				}
				break;
		}
	}

	public void EventTriggerExit ( Collider Col ) {
		switch (Col.tag)
		{
			case "MovingPlatform":
				Destroy(_PlatformAnchor);
				break;
			case "Wind":
				_numberOfWindForces -= 1;
				if (Col.transform.up.y > 0.7f)
					_PlayerPhys._listOfIsGravityOn.RemoveAt(0);
				if (_numberOfWindForces == 0)
					_totalWindDirection = Vector3.zero;
					break;
		}
	}

	public void EventTriggerStay ( Collider Col ) {
		switch (Col.tag)
		{
			case "MovingPlatform":
				AttachAnchorToPlatform(Col);
				break;

			case "Wind":
				S_Trigger_Updraft UpdraftScript = Col.GetComponentInParent<S_Trigger_Updraft>();
				if (UpdraftScript != null)
				{
					Vector3 thisForce = GetForceOfWind(UpdraftScript);
					_currentWindDirection += thisForce;

					_windCounter++;
					ApplyWind();
				}
				break;
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
		//If a text element of the UI has been set for speed, update it to show current running speed.
		if (_CoreUIElements.SpeedCounter != null && _PlayerPhys._speedMagnitude > 10f) _CoreUIElements.SpeedCounter.text = _PlayerPhys._currentRunningSpeed.ToString("F0");
		else if (_CoreUIElements.SpeedCounter != null && _displaySpeed < 10f) _CoreUIElements.SpeedCounter.text = "0";
	}

	//
	//Wind Interactions
	//
	//Takes an origin of wind and gets how much force to apply onto the player from it, based on its power and distance in the wind direction
	private Vector3 GetForceOfWind ( S_Trigger_Updraft UpdraftScript ) {
		Vector3 direction = UpdraftScript._Direction.up;

		// Create a temporary game object and place it at player position in the local space of the wind
		GameObject newGameObject = new GameObject("TEMP");
		Transform newTransform = newGameObject.transform;
		newTransform.position = transform.position;
		newTransform.parent = UpdraftScript._Direction;

		//Remove the vertical component, ensuring this temp object is only along the base, at any rotation. InverseTrasformDirection does not work because it does not account for rotation.
		newTransform.localPosition = new Vector3 (newTransform.localPosition.x, 0, newTransform.localPosition.z);

		//Get the player positions in regards to the wind origin, and remove the height so it is only along the base of the origin, not along the wind direction.
		Vector3 relativePlayerPosition = newTransform.position;
		Destroy(newGameObject);

		//Get the difference between current position and this affected position, and this will be how far along the direction the player is.
		float distance = Vector3.Distance(relativePlayerPosition, transform.position);

		float power = 0;
		if (distance < 5)
		{
			//If under 3 units away and moving towards the wind, apply force against equal to the player's speed in that direction, ensuring they can't fall beyond it.
			float dot = _PlayerPhys.GetPlayersSpeedInGivenDirection(-direction, S_Enums.VelocityTypes.Core, false);
			if(dot > 5) { power = dot; }
		}
		//Affect power by distance along in this direction
		power = Mathf.Max(power, UpdraftScript._power * UpdraftScript._FallOfByPercentageDistance.Evaluate(distance / UpdraftScript._getRange));
		
		return power * direction;
	}

	//After going over each wind force, apply all at once, either as general or core velocity, split vertical and lateral.
	private void ApplyWind () {
		//To prevent up and down differences being extremely sudden, apply to CoreVelocity is this will increase it, but if it will decrease it, apply temporary.
		if (_windCounter == _numberOfWindForces)
		{
			_totalWindDirection = _currentWindDirection; //Saves the total wind force so other scripts can access it before current is set to zero again next frame.

			//Split wind between vertical and lateral, because these should operate differently due to gravity interactions.
			Vector3 lateralWind = _currentWindDirection;
			lateralWind.y = 0;
			Vector3 verticalWind = new Vector3(0, _currentWindDirection.y, 0);

			//Apply lateral
			//Get how fast the coreVelocity is moving against the wind.
			float dot = _PlayerPhys.GetPlayersSpeedInGivenDirection(-lateralWind, S_Enums.VelocityTypes.Custom, true);
			//If player is not moving in favour of the wind.
			if (dot > -0.7f)
				_PlayerPhys.AddGeneralVelocity(lateralWind, false);
			//If pushing away from the wind
			else
				_PlayerPhys.AddCoreVelocity(lateralWind * Time.fixedDeltaTime);

			float x = 0;

			//Apply vertical, decreasing core velocity if going towards wind, to combat gravity.
			dot = _PlayerPhys.GetPlayersSpeedInGivenDirection(verticalWind, S_Enums.VelocityTypes.CoreNoLateral, false);
			if (dot >= x)//If already being pushed up by wind.
				_PlayerPhys.AddGeneralVelocity(verticalWind, false);
			else //Fallspeed wont increase while in wind, so apply velocity until upwards force is x, overcoming gravity
				_PlayerPhys.AddCoreVelocity(verticalWind * Mathf.Min( verticalWind.y * Time.fixedDeltaTime, Mathf.Abs(dot - x)));

			//If being blown upwards, enter the hovering state to change actions and animation.
			//canHover can only be set to true by the Hovering AttemptAction, so GetComponent is safe, and Hovering being enabled shouldn't enable canhover.
			if (_canHover && _totalWindDirection.normalized.y > 0.72f)
			{
				 _Actions._ObjectForActions.GetComponent<S_Action13_Hovering>().StartAction(); //Not placed in enterTrigger incase was already in the trigger, but not in a state that could enter the hover action.
			}
		}
	}

	//When on a moving platform, check is an anchor has currently been spawned, and if not, create one.
	private void AttachAnchorToPlatform ( Collider Col ) {
		if (_PlatformAnchor == null)
		{
			//The reason we're using an anchor reference attached as a child to the mover is because it means we can compare the changes in world position every frame, no matter what happens.
			//For instance, if the object is rotating, this anchor will reflect that as it will move around as a child of the rotating.
			_PlatformAnchor = GameObject.Instantiate(new GameObject("Anchor"), _PlayerPhys.transform.position, Quaternion.identity);
			_PlatformAnchor.transform.parent = Col.transform;
			_previousPlatformPointPosition = _PlatformAnchor.transform.position;
		}
	}

	//If there is currently a platform script saved from being in a trigger with one, adjust the players position every frame to match it.
	private void FollowPlatform () {

		if (_PlatformAnchor != null)
		{
			//Get how much the anchor has moved, and apply that same movement to the player.
			Vector3 direction = _PlatformAnchor.transform.position - _previousPlatformPointPosition;
			_previousPlatformPointPosition = _PlatformAnchor.transform.position;

			//These alternate approaches are because when moving, changing rigidbody is smoother, when not, changing transform is smoother.
			if (_PlayerPhys._coreVelocity.sqrMagnitude < 5)
				_PlayerPhys.transform.position += direction;
			else
				_PlayerPhys._RB.position += direction;
		}
	}

	//Called on triggers

	private void LaunchFromPadOrDashRing ( Collider Col ) {
		if (!Col.TryGetComponent(out S_Data_SpeedPad SpeedPadScript)) { return; } //Ensures object has necessary script, and saves as varaible for efficiency.

		Col.GetComponent<AudioSource>().Play();

		//If a rail booster
		if (SpeedPadScript._isOnRail_)
		{
			//Attaches the player to the rail this rail booster is on.
			if (_Actions._whatCurrentAction != S_Enums.PrimaryPlayerStates.Rail)
			{
				_PlayerPhys.SetPlayerPosition(SpeedPadScript._PositionToLockTo.position);
			}
			else
			{
				StartCoroutine(_Actions._ObjectForActions.GetComponent<S_Action05_Rail>().ApplyBoosters(SpeedPadScript._speedToSet_, SpeedPadScript._willSetSpeed_, SpeedPadScript._addSpeed_, SpeedPadScript._willSetBackwards_));
			}
			return;
		}

		//Normal Dash pad
		else if (!SpeedPadScript._Path)
		{
			//For consistency, ensure player always launches out of ring of off booster from the same point.
			Vector3 snapPosition;
			if (SpeedPadScript._PositionToLockTo != null)
				snapPosition = SpeedPadScript._PositionToLockTo.position;
			else
				snapPosition = Col.transform.position;

			//Magnitude of force
			float speed = SpeedPadScript._speedToSet_;
			if (SpeedPadScript._willCarrySpeed_)
			{
				speed = Mathf.Max(speed, _PlayerPhys._currentRunningSpeed);
			}

			//Dash ring
			if (SpeedPadScript._isDashRing_)
			{
				HitAirLauncher();

				//Effects
				_CharacterAnimator.SetBool("Grounded", false);

				//Prevents immediately using air actions like bounce or jumps.
				if (SpeedPadScript._lockAirMovesFor_ > 0)
				{
					StopCoroutine(_Actions.LockAirMovesForFrames(SpeedPadScript._lockAirMovesFor_));
					StartCoroutine(_Actions.LockAirMovesForFrames(SpeedPadScript._lockAirMovesFor_));
				}

				//Because this is to launch the player through the sky, certain stats can have different gravities. This ensures characters will always fall the same way by overwriting their stats until landing.
				if (SpeedPadScript._overwriteGravity_ != Vector3.zero)
				{
					StartCoroutine(LockPlayerGraivtyUntilGrounded(SpeedPadScript._overwriteGravity_));
				}

				Vector3 direction = SpeedPadScript._PositionToLockTo.forward;
				LaunchInDirection(direction, speed, snapPosition);
			}
			//Speed pad
			else
			{
				//Effects
				_CharacterAnimator.SetBool("Grounded", true);

				//Set rotation to immediately be in line with pad.
				transform.up = Col.transform.up;
				_Actions._ActionDefault.SetSkinRotationToVelocity(0, Col.transform.forward);


				//Apply location to start moving from.
				if (SpeedPadScript._willSnap)
				{
					snapPosition -= _PlayerPhys._feetOffsetFromCentre; //Because on ground, feet should be set to pad position.
					_PlayerPhys.SetPlayerPosition(snapPosition);
				}

				//Pushes player in direction
				_PlayerPhys.SetCoreVelocity(Col.transform.forward * speed, "Overwrite");

				if (_Actions._listOfSpeedOnPaths.Count > 0) 
				{  _Actions._listOfSpeedOnPaths[0] = speed; }
			}

			if (SpeedPadScript._willLockControl)
			{
				_Input.LockInputForAWhile(SpeedPadScript._lockControlFrames_, false, Col.transform.forward, SpeedPadScript._lockInputTo_);
			}

			//If pad is set to, rotate camera horizontally towards dash direction.
			if (SpeedPadScript._willAffectCamera_)
			{
				Quaternion targetRotation = Quaternion.LookRotation(Col.transform.forward, _PlayerPhys.transform.up);
				_CamHandler._HedgeCam.SetCameraNoHeight(Col.transform.forward, SpeedPadScript._CameraRotateTime_.x, SpeedPadScript._CameraRotateTime_.y, targetRotation, false, false);
			}

		}
	}

	private void HitAirLauncher () {
		// Immediate effects on player
		_Actions._ActionDefault.CancelCoyote(); //Ensures can't make a normal jump being launched.
		_PlayerPhys._listOfIsGravityOn.Clear(); //Counteracts any actions that might have disabled this.

		//_PlayerPhys.SetPlayerRotation(Quaternion.identity, false);

		//Returns air actions
		_Actions._isAirDashAvailables = true;
		_Actions._jumpCount = 1;

		_Events._OnTriggerAirLauncher.Invoke();
	}

	private void LaunchFromSpring ( Collider Col ) {
		if (!Col.TryGetComponent(out S_Data_Spring SpringScript)) { return; } //Ensures object has necessary script, and saves as varaible for efficiency.

		HitAirLauncher();

		_Actions._ActionDefault.StartAction();
		_CharacterAnimator.SetBool("Grounded", false);

		//Prevents immediate air actions.
		_Input._JumpPressed = false;
		_Input._SpecialPressed = false;
		_Input._BouncePressed = false;

		Vector3 direction = SpringScript._BounceTransform.up;
		//Calculate force

		//If spring should not take complete control of player velocity, calculate direction based on movement into spring, including spring direction.
		//Horizontal speed is calculated using core velocity, while vertical is environmental. Horizontal cannot be greater than the larger of running speed or launch speed.
		if (SpringScript._keepHorizontal_)
		{
			//Since vertical will be taken over by environment, get horizontal core velocity.
			Vector3 newCoreVelocity = _PlayerPhys.GetRelevantVector(_PlayerPhys._coreVelocity, false);
			Vector3 launchHorizontalVelocity = _PlayerPhys.GetRelevantVector(direction * SpringScript._springForce_, false); //Combined the spring direction with force to get the only the force horizontally.

			Vector3 combinedVelocityMagnitude = (launchHorizontalVelocity + newCoreVelocity); //The two put together normally so the magnitude is accurate.
			Vector3 combinedVelocityDirection = (_PlayerPhys.transform.TransformDirection(launchHorizontalVelocity) * 2) + newCoreVelocity; //The direction of the two put together, with the bounce being prioritised.
			Vector3 upDirection = new Vector3(0, direction.y, 0);

			//If the velocity after bounce is greater than velocity going in to bounce, the take the larger of the two that made it, without losing direction. This will prevent speed increasing too much.
			if (combinedVelocityMagnitude.sqrMagnitude > newCoreVelocity.sqrMagnitude)
			{
				//Rather than using Max / Min, use IF statements to compare with sqrMagnitude before getting an actual "magnitude".
				if (launchHorizontalVelocity.sqrMagnitude > newCoreVelocity.sqrMagnitude)
				{
					newCoreVelocity = combinedVelocityDirection.normalized * launchHorizontalVelocity.magnitude;
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
			LaunchInDirection(direction, SpringScript._springForce_, SpringScript._BounceTransform.position);
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
			StopCoroutine(_Actions.LockAirMovesForFrames(SpringScript._lockAirMovesTime_)); //Overwrites coroutine if already in use, resetting the timer.
			StartCoroutine(_Actions.LockAirMovesForFrames(SpringScript._lockAirMovesTime_));
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

	//Takes a power and direction and splits it across environmental and core velocity, then pushes player in the direction after a slight delay.
	private void LaunchInDirection ( Vector3 direction, float launchPower, Vector3 lockPosition ) {
		//While the player will always move at the same velocity, the combination between environmental and core can vary, with one being prioritised.
		//This is because if the player enters a spring at speed, they will want to keep that speed when the spring is finished.
		//Core velocity vertically is removed, and handled by environment, but horizontal will be a combo of both velocity types, both going in the same direction.

		Vector3 launchHorizontalVelocity = _PlayerPhys.GetRelevantVector(direction * launchPower, false); //Combined the spring direction with force to get only the force horizontally
		float horizontalSpeed = launchHorizontalVelocity.magnitude; //Get the total speed that will actually be applied in world horizontally.

		float horizontalEnvSpeed = Mathf.Max(horizontalSpeed -  _PlayerPhys._horizontalSpeedMagnitude, 1); //Environmental force will be added to make up for the speed lacking before going into the spring.

		//The value of core over velocity will either be what it was before (as environment makes up for whats lacking), or the bounce force itself (decreasing running speed if need be)
		float coreSpeed = _PlayerPhys._horizontalSpeedMagnitude;
		if (coreSpeed > horizontalSpeed)
		{
			horizontalEnvSpeed = 1; //Ensure's theres still a direction even though this won't factor in much to world velocity.
			coreSpeed = horizontalSpeed; //In this case, bounce will be entirely through core velocity, not environmental.
		}

		//This is all in order to prevent springs being used to increase running speed, as the players running speed will not change if they don't unless they have control (most springs should take control away temporarily).

		Vector3 totalEnvironment = (launchHorizontalVelocity.normalized * horizontalEnvSpeed) + (new Vector3(0, (direction * launchPower).y,0));

		launchHorizontalVelocity = _PlayerPhys.transform.TransformDirection(launchHorizontalVelocity);

		StartCoroutine(ApplyForceAfterDelay(totalEnvironment, lockPosition, launchHorizontalVelocity.normalized * coreSpeed));
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
			_PlayerPhys.SetPlayerPosition(position);
			_PlayerPhys.SetCoreVelocity(Vector3.zero, "Overwrite");
			_PlayerPhys.SetBothVelocities(Vector3.zero, Vector2.one);
			yield return new WaitForFixedUpdate();
		}

		_Actions._ActionDefault.StartAction(); //Ensures player is still in correct state after delay.

		_PlayerPhys.SetPlayerPosition(position); //Ensures player is set to inside of spring, so bounce is consistant. 

		_PlayerPhys._listOfCanControl.RemoveAt(0);

		_PlayerPhys.SetCoreVelocity(coreVelocity, "Overwrite"); //Undoes this being set to zero during delay.

		_PlayerPhys.SetEnvironmentalVelocity(environmentalVelocity, true, true, S_Enums.ChangeLockState.Lock); //Apply bounce
	}


	private void ActivateHintBox ( Collider Col ) {
		if (!Col.TryGetComponent(out S_Data_HintRing HintRingScript)) { return; } //Ensures object has necessary script, and saves as varaible for efficiency.

		if (Col.gameObject == _CoreUIElements.HintBox._CurrentHintRing) { return; } //Do not perform function if this hint is already being displayed in the hintBox. Prevents restarting a hint when hitting it multiple times until its complete.
		_CoreUIElements.HintBox._CurrentHintRing = Col.gameObject; //Relevant to the above check.

		//Effects
		HintRingScript.hintSound.Play();

		//Using mouse is set when _PlayerInput detects a camera or move input coming from a keyboard or mouse, and this ensures the correct text will be displayed to match the controller device.
		if (_Input._isUsingMouse)
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
		if (!col.TryGetComponent(out S_Data_Monitor MonitorData)) { return; } //Ensures the collider has a monitor script.

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
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_CamHandler = _Tools.CamHandler;
		_Actions = _Tools._ActionManager;
		_Events = _Tools.PlayerEvents;
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_AttackHandler = GetComponent<S_Handler_CharacterAttacks>();
		_HurtAndHealth = _Tools.GetComponent<S_Handler_HealthAndHurt>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
		_CoreUIElements = _Tools.UISpawner._BaseUIElements;
		_FeetPoint = _Tools.FeetPoint;
	}
	#endregion
}
