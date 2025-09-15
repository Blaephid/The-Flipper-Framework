using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;
using UnityEngine.ProBuilder;
using UnityEditor;
using Unity.VisualScripting;

public class S_Interaction_Objects : S_Player_Base
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

	private S_Handler_CharacterAttacks      _AttackHandler;
	private S_Handler_HealthAndHurt         _HurtAndHealth;
	private S_Interaction_Triggers          _TriggerInteraction;
	private S_Control_EffectsPlayer         _Effects;

	//External
	private GameObject                       _PlatformAnchor;


	[Header("For Rings, Springs and so on")]
	public GameObject RingCollectParticle;
	public GameObject SphereCollectParticle;
	public Material SpeedPadTrack;
	public Material DashRingMaterial;
	public Material NormalShieldMaterial;
	public Color DashRingLightsColor;

	#endregion

	//Stats
	#region Stats
	private float _powerFromSpheres_ = 1f; //How much power is gained from collecting a sphere
	#endregion

	// Trackers
	#region trackers
	private Vector3     _translateOnPlatform;

	[HideInInspector]
	public float        _displaySpeed;

	private Vector3     _previousPlatformPointPosition;

	[Header("Wind Force")]
	private Vector3      _currentWindVector;
	[HideInInspector]
	public Vector3      _finalWindVector;
	private int         _numberOfWindForces; //How many winds are currently operating on the player. Up when entering one, down when exiting one.
	private int         _windCounter; //0 At the start of every frame, and goes up for each wind calculaton, when equal to number of wind forces, it's at the last one.

	#endregion

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited


	private void FixedUpdate () {

		//For tracking wind forces
		_currentWindVector = Vector3.zero;
		_windCounter = 0;
	}

	public void EventTriggerEnter ( Collider Col ) {
		switch (Col.tag)
		{
			case "SpeedPad":
				DashOffPad(Col);
				break;
			case "Dash Launcher":
				LaunchFromDashLauncher(Col);
				break;
			case "RailBooster":
				BoostOnRail(Col);
				break;
			case "Spring":
				LaunchFromSpring(Col);
				break;
			case "Bumper":
				break;
			case "Wind":
				EnterWind(Col);
				break;

			case "Monitor":
				Col.GetComponentInChildren<BoxCollider>().enabled = false;
				_AttackHandler.AttemptAttackOnContact(Col, S_GeneralEnums.AttackTargets.Monitor);
				break;

			case "Ring":
				StartCoroutine(_HurtAndHealth.GainRing(1f, Col, RingCollectParticle));
				break;

			case "Ring Road":
				StartCoroutine(_HurtAndHealth.GainRing(0.5f, Col, RingCollectParticle));
				break;

			case "Sphere":
				StartCoroutine(GainSphere(Col));
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
			case "Enable Objects Physics":
				SetMovingPlatformAsActive(Col, true);
				break;

			case "Switch":
				if (Col.GetComponent<S_Data_Switch>() != null)
				{
					Col.GetComponent<S_Data_Switch>().Activate();
				}
				break;

			case "HintRing":
				_TriggerInteraction.ActivateHintBox(Col);
				break;


			case "EffectTrigger":
				_TriggerInteraction.CheckEffectsTriggerEnter(Col); break;

			case "Special":
				ObjectWithNoSpecificTag(Col); break;
		}
	}

	public void EventTriggerExit ( Collider Col ) {
		switch (Col.tag)
		{
			case "MovingPlatform":
				Destroy(_PlatformAnchor);
				_PlatformAnchor = null;
				break;

			case "Enable Objects Physics":
				SetMovingPlatformAsActive(Col, false);
				break;

			case "Wind":
				ExitWind(Col);
				break;
			case "EffectTrigger":
				_TriggerInteraction.CheckEffectsTriggerExit(Col); break;
		}
	}

	public void EventTriggerStay ( Collider Col ) {
		switch (Col.tag)
		{
			case "MovingPlatform":
				FollowPlatform();
				AttachAnchorToPlatform(Col);
				break;

			case "Wind":
				StayinWindPerForce(Col);
				break;
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	private IEnumerator GainSphere ( Collider col ) {

		Instantiate(SphereCollectParticle, _PlayerPhys._CharacterCenterPosition, Quaternion.identity, transform);
		Destroy(col.gameObject);

		float ThisFramesPowerCount = _CoreValues._pointsCount;
		yield return new WaitForEndOfFrame();

		//Prevents multiple spheres being gained in the same frame.
		if (_CoreValues._pointsCount != ThisFramesPowerCount + _powerFromSpheres_)
		{
			_CoreValues.AdjustPoints(_powerFromSpheres_);
		}
	}


	//
	//Wind Interactions
	//

	private void EnterWind ( Collider Col ) {
		S_Data_Updraft UpdraftScript = Col.GetComponentInParent<S_Data_Updraft>();
		if (UpdraftScript == null) { return; }

		_numberOfWindForces += 1;

		if (Col.transform.up.y > 0.7f && _numberOfWindForces == 1)
		{
			StartCoroutine(RemoveAdditionalVerticalVelocity(_PlayerVel._coreVelocity.y));
			S_S_Logic.AddLockToList(ref _PlayerPhys._locksForIsGravityOn, "InUpDraft");
			_PlayerPhys.SetIsGrounded(false);
		}

	}

	private void ExitWind ( Collider Col ) {
		S_Data_Updraft UpdraftScript = Col.GetComponentInParent<S_Data_Updraft>();
		if (UpdraftScript == null) { return; }

		_numberOfWindForces -= 1;

		//Col.transform.up.y > 0.7f && 
		if (_numberOfWindForces == 0)
		{
			S_S_Logic.RemoveLockFromList(ref _PlayerPhys._locksForIsGravityOn, "InUpDraft");

			StartCoroutine(LerpFromWindAffectedToNormalSpeed(_finalWindVector));

			_finalWindVector = Vector3.zero;
		}
	}

	//To prevent player blasting off as soon wind stops slowing them down, lerp from the speed the wind limited to, to their actual speed.
	private IEnumerator LerpFromWindAffectedToNormalSpeed ( Vector3 windVector ) {
		windVector.y = 0;

		float localSpeed = _PlayerVel._currentRunningSpeed;
		float startWorldSpeed = _PlayerVel._horizontalSpeedMagnitude;
		float frames = 25;

		if (startWorldSpeed > localSpeed) { yield break; }

		for (int i = 0 ; i < frames ; i++)
		{
			yield return new WaitForFixedUpdate();

			if (_PlayerVel._currentRunningSpeed < localSpeed) localSpeed = _PlayerVel._currentRunningSpeed;
			if (_PlayerVel._horizontalSpeedMagnitude < startWorldSpeed - 2) { yield break; }

			//Add a single frame push against the player, being lighter each frame until its gone.
			float thisPush = localSpeed - startWorldSpeed;
			thisPush = Mathf.Lerp(thisPush, 0, i / frames);
			_PlayerVel.AddCoreVelocity(_PlayerVel._coreVelocity.normalized * -thisPush);
		}
	}

	private void StayinWindPerForce ( Collider Col ) {
		S_Data_Updraft UpdraftScript = Col.GetComponentInParent<S_Data_Updraft>();
		if (UpdraftScript == null) { return; }

		if (_Actions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.Homing
			|| _Actions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.RingRoad
			|| _Actions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.Upreel) { return; } //Homing attack is immune to wind as it goes to targets on its own.

		Vector3 thisForce = GetForceOfWindSource(UpdraftScript);
		_currentWindVector += thisForce;

		_windCounter++;
		ApplyWindAfterAllForces();

	}

	//If entering wind with force upwards already (like from a jump), this would carry the whole way, so only use gravity to remove this, but not go against the wind.
	private IEnumerator RemoveAdditionalVerticalVelocity ( float coreVelocityUpwards ) {
		yield return new WaitForFixedUpdate();

		//If the force applied by the wind is substantially more than the core velocity before, then just remove the core velocity immediately
		if (coreVelocityUpwards * 1.5f < _PlayerVel._worldVelocity.y)
		{
			_PlayerVel.AddCoreVelocity(Vector3.down * coreVelocityUpwards);
		}
		//Otherwise, remove it with normal gravity calculations.
		else
		{
			while (coreVelocityUpwards > 0 && _PlayerVel._coreVelocity.y > 0)
			{
				//Calculate how much gravity would have an affect on this velocity, then apply it seperately, so only this is being counteracted.
				//(Allowing the player speed up to slow but not counteract the wind).
				Vector3 forceDownwards = Vector3.up;
				forceDownwards = _PlayerPhys.TryGravity(forceDownwards * coreVelocityUpwards, true);
				float change = forceDownwards.y - coreVelocityUpwards;
				coreVelocityUpwards = change;

				_PlayerVel.AddCoreVelocity(Vector3.down * change);
			}
		}
	}

	//Takes an origin of wind and gets how much force to apply onto the player from it, based on its power and distance in the wind direction
	private Vector3 GetForceOfWindSource ( S_Data_Updraft UpdraftScript ) {
		Vector3 direction = UpdraftScript._Direction.up;
		direction = S_S_MoreMaths.TreatAxisAsZeroForVector(direction, 0.005f);

		// Create a temporary game object and place it at player position in the local space of the wind
		GameObject newGameObject = new GameObject("TEMP");
		Transform newTransform = newGameObject.transform;
		newTransform.position = _PlayerPhys._CharacterCenterPosition;
		newTransform.parent = UpdraftScript._Direction;

		float distanceAlongWindDirection = newTransform.localPosition.y / newTransform.lossyScale.y;

		//Remove the vertical component, ensuring this temp object is only along the base, at any rotation. InverseTrasformDirection does not work because it does not account for rotation.
		newTransform.localPosition = new Vector3(newTransform.localPosition.x, 0, newTransform.localPosition.z);

		//Get the player positions in regards to the wind origin, and remove the height so it is only along the base of the origin, not along the wind direction.
		Vector3 relativePlayerPosition = newTransform.position;
		Destroy(newGameObject);

		//Get the difference between current position and this affected position, and this will be how far along the direction the player is.
		float distanceSquared = S_S_MoreMaths.GetDistanceSqrOfVectors(relativePlayerPosition, _PlayerPhys._CharacterPivotPosition);


		float power = 0;
		//if (distanceSquared < 9)
		if (distanceAlongWindDirection < 3)
		{
			//If under 3 units away and moving towards the wind, apply force against equal to the player's speed in that direction, ensuring they can't fall beyond it.
			Vector3 WindProjectedAgainstVelocity = Vector3.Project(_PlayerVel._coreVelocity, -direction);
			if (WindProjectedAgainstVelocity.sqrMagnitude > 1)
			{ power = WindProjectedAgainstVelocity.magnitude; }
		}
		else
			//Affect power by distance along in this direction
			//power = Mathf.Max(power, UpdraftScript._power * UpdraftScript._FallOfByPercentageDistance.Evaluate(distanceSquared / UpdraftScript._getRangeSquared));
			//power = UpdraftScript._power * UpdraftScript._FallOfByPercentageDistance.Evaluate(distanceSquared / UpdraftScript._totalRangeSquare);
			power = UpdraftScript._power * UpdraftScript._FallOfByPercentageDistance.Evaluate(distanceAlongWindDirection / UpdraftScript._setRange);

		power = Mathf.Max(power, 0);
		return power * direction;
	}

	//After going over each wind force, apply all at once, either as general or core velocity, split vertical and lateral.
	private void ApplyWindAfterAllForces () {
		//To prevent up and down differences being extremely sudden, apply to CoreVelocity is this will increase it, but if it will decrease it, apply temporary.
		if (_windCounter == _numberOfWindForces)
		{

			_finalWindVector = _currentWindVector; //Saves the total wind force so other scripts can access it before current is set to zero again next frame.

			//Split wind between vertical and lateral, because these should operate differently due to gravity interactions.
			Vector3 lateralWind = _currentWindVector;
			lateralWind.y = 0;
			Vector3 verticalWind = new Vector3(0, _currentWindVector.y, 0);

			//Apply lateral
			Vector3 relevantCoreVelocity = new Vector3 (_PlayerVel._coreVelocity.x, 0, _PlayerVel._coreVelocity.z);
			Vector3 nextVelocity = relevantCoreVelocity + lateralWind;

			//If the wind will increase velocity overall, then apply to coreVelocity so it remains, rather than just being temporary like with the constant general.
			if (nextVelocity.sqrMagnitude > relevantCoreVelocity.sqrMagnitude)
			{
				lateralWind = S_S_MoreMaths.ClampMagnitudeWithSquares(lateralWind, 0, 30); //To prevent player suddenly shooting off at 100+ speed when slowing down infront of a strong fan.

				//If added normally, then running perpendicular to the wind, the full force would be added, but immediately turned away, increasing velocity in the unintended direction.
				//So only add the amount specifically in the wind direction, using project.
				Vector3 nextSpeedInFanDirection = Vector3.Project(nextVelocity, lateralWind);
				Vector3 increase = nextSpeedInFanDirection - relevantCoreVelocity;

				if (relevantCoreVelocity.sqrMagnitude > increase.sqrMagnitude * Time.fixedDeltaTime + 1)
					_PlayerVel.AddCoreVelocity(increase * Time.fixedDeltaTime * 0.5f);
			}

			_PlayerVel.AddGeneralVelocity(lateralWind, false, true); //Using general velocity so the player believably is still running at speed, even if going nowhere in the world.

			if (_PlayerPhys._locksForIsGravityOn.Count > 0)
			{

				//Apply vertical, decreasing core velocity if going towards wind, to combat gravity.
				float x = 0;
				if (_PlayerVel._coreVelocity.y >= x)//If already being pushed up by wind
					_PlayerVel.AddGeneralVelocity(verticalWind, false, true);

				else //Fallspeed wont increase while in wind, so apply velocity until upwards force is x, overcoming gravity
					_PlayerVel.AddCoreVelocity(verticalWind * Mathf.Min(Time.fixedDeltaTime, Mathf.Abs(_PlayerVel._coreVelocity.y - x)));
			}
			else
				_PlayerVel.AddCoreVelocity(verticalWind * Time.fixedDeltaTime);


			//Action
			if (_Actions._ObjectForActions.TryGetComponent(out S_Action13_Hovering Hovering))
			{
				//If being blown upwards, enter the hovering state to change actions and animation.
				//canHover can only be set to true by the Hovering AttemptAction, so GetComponent is safe, and Hovering being enabled shouldn't enable canhover.
				if (Hovering._inAStateConnectedToThis && _finalWindVector.normalized.y > 0.72f && _finalWindVector.y > 3)
				{
					Hovering.StartAction(); //Not placed in enterTrigger incase was already in the trigger, but not in a state that could enter the hover action.
				}
			}
		}
	}

	//If the trigger is on the same object as the movePlatform component, then switch the platform to move with physics rather than transform. This is for more accurate interactions when close but cheaper interactions further away.
	private void SetMovingPlatformAsActive ( Collider Col, bool activePhysics ) {
		if (Col.TryGetComponent(out S_Control_MovingPlatform Control))
		{
			if (Control._canCarryPlayer)
				Control._isPhysicsActive = activePhysics; //See the S_ControlMoving Platform script for how it switches to applying velocity every fixedUpdate.
		}
	}

	//When on a moving platform, check is an anchor has currently been spawned, and if not, create one.
	private void AttachAnchorToPlatform ( Collider Col ) {

		if (_PlatformAnchor == null)
		{
			//The reason we're using an anchor reference attached as a child to the mover is because it means we can compare the changes in world position every frame, no matter what happens.
			//For instance, if the object is rotating, this anchor will reflect that as it will move around as a child of the rotating.
			_PlatformAnchor = GameObject.Instantiate(new GameObject("Anchor"), _PlayerVel.transform.position, Quaternion.identity);
		}
		else
			_PlatformAnchor.transform.position = _PlayerPhys._CharacterPivotPosition;

		_PlatformAnchor.transform.parent = Col.transform;
		_previousPlatformPointPosition = _PlayerPhys._CharacterPivotPosition;
	}

	//If there is currently a platform script saved from being in a trigger with one, adjust the players position every frame to match it.
	private void FollowPlatform () {

		if (_PlatformAnchor != null)
		{
			//Get how much the anchor has moved, and apply that same movement to the player.
			Vector3 direction = _PlatformAnchor.transform.position - _previousPlatformPointPosition;
			_previousPlatformPointPosition = _PlatformAnchor.transform.position;

			_PlayerVel.AddGeneralVelocity(direction / Time.fixedDeltaTime, true, false);
			return;
		}
	}

	//Called on triggers

	private void DashOffPad ( Collider Col ) {
		GameObject GO = GetObjectWithData(Col);
		if (!GO.TryGetComponent(out S_Data_SpeedPad SpeedPadScript)) { return; } //Ensures object has necessary script, and saves as varaible for efficiency.
		GO.GetComponent<AudioSource>().Play();

		if (SpeedPadScript._Path) { return; }

		//Magnitude of force
		float speed = SpeedPadScript._speedToSet_;
		if (SpeedPadScript._willCarrySpeed_)
		{
			speed = Mathf.Max(speed, _PlayerVel._currentRunningSpeed);
		}

		SnapToObject(GO.transform, SpeedPadScript._PositionToLockTo);

		//Effects
		ObjectRotatesCamera(GO, SpeedPadScript._cameraEffect);
		if (speed > 100) { _Effects.TriggerBlurBurstScreen(); }

		//Player visual
		_CharacterAnimator.SetBool("Grounded", true);
		_Actions._ActionDefault.SetSkinRotationToVelocity(0, GO.transform.forward);

		//Ground interactions
		_PlayerPhys.SetIsGrounded(true);
		_PlayerPhys._groundNormal = GO.transform.up;
		_PlayerPhys.AlignToGround(GO.transform.up, true);

		//Pushes player in direction
		_PlayerVel.SetCoreVelocity(GO.transform.forward * speed, "Overwrite");

		if (_Actions._listOfSpeedOnPaths.Count > 0)
		{ _Actions._listOfSpeedOnPaths[0] = speed; }

		if (SpeedPadScript._lockControlFrames_ > 0)
		{
			_Input.LockInputForAWhile(SpeedPadScript._lockControlFrames_, false, GO.transform.forward, SpeedPadScript._lockInputTo_);
		}

		_ActionChain.AddToChain("Speed Pad", 2, 1, GO.GetInstanceID().ToString());
	}

	private void LaunchFromDashLauncher ( Collider Col ) {
		GameObject GO = GetObjectWithData(Col);
		if (!GO.TryGetComponent(out S_Data_DashLauncher DashRingScript)) { return; } //Ensures object has necessary script, and saves as varaible for efficiency.
		GO.GetComponent<AudioSource>().Play();

		HitAirLauncher();

		//Calculate launch
		float force = DashRingScript._willCarrySpeed_ ? Mathf.Max(DashRingScript._launchData_._force_, _PlayerVel._currentRunningSpeed) :
			DashRingScript._launchData_._force_ ;
		Vector3 direction = DashRingScript._launchData_._directionToUse_;

		LaunchInDirection(direction, force, GO.transform, _PlayerPhys.transform, DashRingScript._launchData_);

		ObjectRotatesCamera(GO, DashRingScript._cameraEffect);

		_ActionChain.AddToChain(DashRingScript.source, 2, 1, GO.GetInstanceID().ToString());
	}

	private void BoostOnRail ( Collider Col ) {

		if (!_Actions._ObjectForActions.TryGetComponent(out S_Action05_Rail RailAction)){return;}
		if(RailAction._RF._distanceToHop > 6) { return; } //Won't activate if hopping OFF this rail

		GameObject GO = GetObjectWithData(Col);
		if (!GO.TryGetComponent(out S_Data_RailBooster RailBoosterScript)) { return; } //Ensures object has necessary script, and saves as varaible for efficiency.
		GO.GetComponent<AudioSource>().Play();

		//Attaches the player to the rail this rail booster is on.
		if (_Actions._whatCurrentAction != S_S_ActionHandling.PrimaryPlayerStates.Rail)
		{
			SnapToObject(GO.transform, RailBoosterScript._PositionToLockTo);
		}

		_ActionChain.AddToChain("Rail Booster", 2, 1, GO.GetInstanceID().ToString());
		StartCoroutine(RailAction.ApplyBoosters(RailBoosterScript));

		ObjectRotatesCamera(GO, RailBoosterScript._cameraEffect);
	}

	private GameObject GetObjectWithData ( Collider Col ) {
		if (Col.TryGetComponent(out S_Data_Redirect Redirect))
		{
			return Redirect._ObjectWithMainScript;
		}
		return Col.gameObject;
	}

	private Vector3 SnapToObject ( Transform Object, Vector3 Offset, Vector3 mainSkinForwards = default(Vector3), bool toFeet = true ) {

		//Rotation
		mainSkinForwards = mainSkinForwards == default(Vector3) ? _MainSkin.forward : mainSkinForwards;

		_PlayerPhys.SetPlayerRotation(Quaternion.identity, true);
		_Actions._ActionDefault.SetSkinRotationToVelocity(0, mainSkinForwards, Vector2.zero, transform.up);

		//Locations
		//For consistency, ensure player always launches out of ring of off booster from the same point.
		Vector3 snapPosition = Object.position + (Object.rotation * Offset);
		if (toFeet)
			snapPosition -= _PlayerPhys._feetOffsetFromPivotPoint; //Because on ground, feet should be set to pad position.
		else
			snapPosition -= _PlayerPhys._colliderOffsetFromPivot;

		_PlayerPhys.SetPlayerPosition(snapPosition);

		return snapPosition;
	}

	private void ObjectRotatesCamera ( GameObject GO, S_Structs.ObjectCameraEffect strucCamEffect ) {

		//If pad is set to, rotate camera horizontally towards dash direction.
		if (strucCamEffect._willAffectCamera_)
		{
			_CamHandler._HedgeCam.SetCameraNoSeperateHeight(GO.transform.forward, strucCamEffect._CameraRotateTime_.x, strucCamEffect._CameraRotateTime_.y, Vector3.zero, false);
		}
	}


	private void HitAirLauncher () {
		//Player Visual
		_CharacterAnimator.SetBool("Grounded", false);

		// Immediate effects on player
		_Actions._ActionDefault.CancelCoyote(); //Ensures can't make a normal jump being launched.
		_PlayerPhys._locksForIsGravityOn.Clear(); //Counteracts any actions that might have disabled this.

		//Sets player to immediately face upwards so launch direction is always correct.
		_PlayerPhys.SetPlayerRotation(Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation, true);

		//Returns air actions
		_Actions._isAirDashAvailable = true;
		_Actions._jumpCount = 1;

		//Prevents immediate air actions.
		_Input._JumpPressed = false;
		_Input._SpecialPressed = false;
		_Input._BouncePressed = false;



		_Events._OnTriggerAirLauncher.Invoke();
	}

	private void LaunchFromSpring ( Collider Col ) {
		GameObject GO = GetObjectWithData(Col);
		if (!GO.TryGetComponent(out S_Data_Spring SpringScript)) { return; } //Ensures object has necessary script, and saves as varaible for efficiency.

		HitAirLauncher();

		_Actions._ActionDefault.StartAction();

		Vector3 direction = SpringScript._launchData_._directionToUse_;

		//Calculate force

		//If spring should not take complete control of player velocity, calculate direction based on movement into spring, including spring direction.
		//Horizontal speed is calculated using core velocity, while vertical is environmental. Horizontal cannot be greater than the larger of running speed or launch speed.
		if (SpringScript._keepHorizontal_)
		{
			//Since vertical will be taken over by environment, get horizontal core velocity.
			Vector3 newCoreVelocity = _PlayerPhys.GetRelevantDirection(_PlayerVel._coreVelocity, false);

			if (_Actions._speedBeforeAction != 0) { newCoreVelocity = newCoreVelocity.normalized * _Actions._speedBeforeAction; }

			newCoreVelocity *= 0.85f;

			Vector3 launchHorizontalVelocity = _PlayerPhys.GetRelevantDirection(direction * SpringScript._launchData_._force_, false); //Combined the spring direction with force to get the only the force horizontally.

			Vector3 combinedVelocityMagnitude = (launchHorizontalVelocity + newCoreVelocity); //The two put together normally so the magnitude is accurate.
			Vector3 combinedVelocityDirection = (_PlayerVel.transform.TransformDirection(launchHorizontalVelocity) * 2) + newCoreVelocity; //The direction of the two put together, with the bounce being prioritised.
			Vector3 upDirection = new Vector3(0, direction.y, 0);

			//If the velocity after bounce is greater than velocity going in to bounce,
			//then take the larger of the two that made it, without losing direction. This will prevent speed increasing too much.
			if (combinedVelocityMagnitude.sqrMagnitude > newCoreVelocity.sqrMagnitude)
			{
				//Rather than using Max / Min, use IF statements to compare with sqrMagnitude before rotating the larger in the right direction.
				if (launchHorizontalVelocity.sqrMagnitude > newCoreVelocity.sqrMagnitude)
				{
					//newCoreVelocity = combinedVelocityDirection.normalized * launchHorizontalVelocity.magnitude;
					newCoreVelocity = Vector3.RotateTowards(launchHorizontalVelocity, combinedVelocityDirection.normalized, 360, 0);
				}
				else
				{
					//newCoreVelocity = combinedVelocityDirection.normalized * newCoreVelocity.magnitude;
					newCoreVelocity = Vector3.RotateTowards(newCoreVelocity, combinedVelocityDirection.normalized, 360, 0);
				}
			}
			else
			{
				newCoreVelocity = combinedVelocityMagnitude;
			}

			StartCoroutine(ApplyForceAfterDelay(upDirection * SpringScript._launchData_._force_
				, newCoreVelocity, GO.transform, SpringScript._launchData_));
		}
		//If not keeping horizontal, then player will always travel along the same "path" created by this instance until control is restored or their stats change. See S_drawShortDirection for a representation of this path as a gizmo.
		else if (!SpringScript._keepHorizontal_)
		{
			LaunchInDirection(direction, SpringScript._launchData_._force_, GO.transform, _PlayerPhys.transform, SpringScript._launchData_);
		}


		//Additional effects based on sprint instance properties.

		//If needed, rotate character in set direction, this will be run after the player rotation is set to velocity in ApplyForceAfterDelay, overwriting it.
		if (SpringScript._changePlayerForwards)
		{
			_Actions._ActionDefault.SetSkinRotationToVelocity(0, SpringScript.transform.forward, Vector2.zero, transform.up);
			_Actions._ActionDefault.LockSkinRotationToDirection(SpringScript.transform.forward);
		}

		//Effects on spring
		if (GO.GetComponent<AudioSource>()) { GO.GetComponent<AudioSource>().Play(); }
		if (SpringScript._Animator != null)
			SpringScript._Animator.SetTrigger("Hit");

		_ActionChain.AddToChain("Spring", 2, 1, GO.GetInstanceID().ToString());
	}

	public void ApplyLaunchEffects ( LaunchPlayerData launchData ) {
		//Delays air actions
		if (launchData._lockAirMovesFrames_ > 0)
		{
			_Actions.LockAirMovesForFrames(launchData._lockAirMovesFrames_);
		}

		//Effect on movement
		if (launchData._lockInputFrames_ > 0)
		{
			_Input.LockInputForAWhile(launchData._lockInputFrames_, false, launchData._directionToUse_, launchData._lockInputTo_);
		}

		//Because this is to launch the player through the sky, certain stats can have different gravities. This ensures characters will always fall the same way by overwriting their stats until landing.
		StartCoroutine(LockPlayerGravityUntilGrounded(launchData._overwriteGravity_));
	}

	//Takes a power and direction and splits it across environmental and core velocity, then pushes player in the direction after a slight delay.
	public void LaunchInDirection ( Vector3 direction, float launchPower, Transform ObjectlaunchPosition, Transform Player, LaunchPlayerData launchData ) {

		if (launchData._shotOrigin != null)
			ObjectlaunchPosition = launchData._shotOrigin;

		Vector3[] split = SplitCoreAndEnvironmentalVelocities(Player,direction,launchPower,_PlayerVel._horizontalSpeedMagnitude,_PlayerPhys._PlayerMovement._currentMaxSpeed,launchData._coreVelocityImportance);
		StartCoroutine(ApplyForceAfterDelay(split[0], split[1], ObjectlaunchPosition, launchData));
	}

	public static Vector3[] SplitCoreAndEnvironmentalVelocities ( Transform Player, Vector3 direction, float launchPower, float currentCoreSpeed, float maxSpeed, float coreVelocityImportance ) {
		//While the player will always move at the same velocity, the combination between environmental and core can vary, with one being prioritised.
		//This is because if the player enters a spring at speed, they will want to keep that speed when the spring is finished.
		//Core velocity vertically is removed, and handled by environment, but horizontal will be a combo of both velocity types, both going in the same direction.

		Vector3 launchHorizontalVelocity = Player? Player.InverseTransformDirection(direction * launchPower) : direction * launchPower; //Combined the spring direction with force to get only the force horizontally
		launchHorizontalVelocity.y = 0;

		Vector2 speeds = GetSpeedsForLaunch(launchHorizontalVelocity, currentCoreSpeed,maxSpeed, coreVelocityImportance );
		float coreSpeed = speeds.x;
		float horizontalEnvSpeed = speeds.y;

		Vector3 totalEnvironment = GetEnvironmentalVelocityForLaunch(launchHorizontalVelocity, horizontalEnvSpeed, direction, launchPower);
		launchHorizontalVelocity = Player ? Player.TransformDirection(launchHorizontalVelocity) : launchHorizontalVelocity;

		return new Vector3[2] { totalEnvironment, launchHorizontalVelocity.normalized * coreSpeed };
	}

	public static Vector2 GetSpeedsForLaunch ( Vector3 horizontalVelocity, float currentCoreSpeed, float maxSpeed, float coreVelocityImportance ) {

		float newHorizontalSpeed = horizontalVelocity.magnitude; //Get the total speed that will actually be applied in world horizontally.
		coreVelocityImportance = currentCoreSpeed > newHorizontalSpeed ? 1 : coreVelocityImportance; //If core is greater than force, then limit to launch force, but dont decrease unnecesarily.

		currentCoreSpeed = Mathf.Clamp(newHorizontalSpeed * coreVelocityImportance, 2, maxSpeed - 2);

		float horizontalEnvSpeed = Mathf.Max(newHorizontalSpeed -  currentCoreSpeed, 2); //Environmental force will be added to make up for the speed lacking.

		return new Vector2(currentCoreSpeed, horizontalEnvSpeed);
		//This is all in order to prevent springs being used to increase running speed, as the players running speed will not change if they don't unless they have control (most springs should take control away temporarily).
	}

	public static Vector3 GetEnvironmentalVelocityForLaunch ( Vector3 launchHorizontalVelocity, float horizontalEnvSpeed, Vector3 direction, float launchPower ) {
		Vector3 envHorizontal = (launchHorizontalVelocity.normalized * horizontalEnvSpeed);
		Vector3 envVertical = new Vector3(0, (direction * launchPower).y,0);
		Vector3 totalEnvironment = envHorizontal + envVertical;
		return totalEnvironment;
	}

	//To ensure force is accurate, and player is in start position, spend a few frames to lock them in position, before chaning velocity.
	private IEnumerator ApplyForceAfterDelay ( Vector3 enVelocity, Vector3 coreVelocity, Transform Object, LaunchPlayerData launchData ) {

		int frames = Mathf.Max(3,launchData._frameDelay);

		//Lock Player
		_Actions._canChangeActions = false;
		_Actions._ActionDefault.StartAction(true); //Ensures player is still in correct state after delay.
		_Actions._ActionDefault._canHandleSkinRotation = false;

		S_S_Logic.AddLockToList(ref _PlayerPhys._locksForCanControl, "ReadyLaunch");
		S_S_Logic.AddLockToList(ref _PlayerPhys._locksForIsGravityOn, "ReadyLaunch");
		if (launchData._delayGroundedFor > 0)
		{
			_PlayerPhys.SetIsGrounded(false);
			_PlayerPhys._canChangeGrounded = false;
			_PlayerPhys._keepNormal = Vector3.up;
		}

		_Input.LockInputForAWhile(frames + 2, false, Vector3.zero, S_GeneralEnums.LockControlDirection.NoInput);

		//Player rotation. Will be determined by the force direction. Usually based on core, but if that isnt present, based on environment.
		Vector3 lookDirection = coreVelocity.sqrMagnitude > 2 ? coreVelocity.normalized : enVelocity.normalized;

		//Keep the player in position, with zero velocity, until delay is over.
		for (int i = 0 ; i < frames ; i++)
		{
			_Actions._ActionDefault.StartAction(); //Ensures player cant change into another action, like a rail, while hitting a spring.
			SnapToObject(Object, Vector3.zero, lookDirection, false);

			_PlayerVel.SetCoreVelocity(Vector3.zero, "Overwrite");
			_PlayerVel.SetBothVelocities(Vector3.zero, Vector2.one);
			yield return new WaitForFixedUpdate();
		}

		SnapToObject(Object, Vector3.zero, lookDirection, false); ;

		Debug.DrawRay(Object.position, lookDirection * 12, Color.cyan, 10f);
		Debug.DrawRay(Object.position, coreVelocity.normalized * 10, Color.red, 10f);
		Debug.DrawRay(Object.position, enVelocity.normalized * 8, Color.green, 10f);
		Debug.DrawRay(Object.position, _MainSkin.forward * 6, Color.black, 10f);

		_PlayerVel.SetCoreVelocity(coreVelocity, "Overwrite"); //Undoes this being set to zero during delay.
		_PlayerVel.SetEnvironmentalVelocity(enVelocity, true, true, S_GeneralEnums.ChangeLockState.Lock); //Apply bounce

		//Unlocking player

		if (launchData != default(LaunchPlayerData))
			ApplyLaunchEffects(launchData);

		StartCoroutine(CanChangeActionsAfterDelay());
		StartCoroutine(CanChangeGroundedAfterDelay(launchData._delayGroundedFor));

		for (int i = 0 ; i < 2 ; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		_Actions._ActionDefault._canHandleSkinRotation = true;
		S_S_Logic.RemoveLockFromList(ref _PlayerPhys._locksForCanControl, "ReadyLaunch");
		S_S_Logic.RemoveLockFromList(ref _PlayerPhys._locksForIsGravityOn, "ReadyLaunch");
	}

	IEnumerator CanChangeActionsAfterDelay () {
		//To ensure launch isn't interupted by entering a rail until launched a bit away.
		for (int i = 0 ; i < 20 ; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		_Actions._canChangeActions = true;
	}

	IEnumerator CanChangeGroundedAfterDelay ( int frames ) {
		for (int i = 0 ; i < frames ; i++)
		{
			//_PlayerPhys.SetPlayerRotation(Quaternion.identity, true);
			yield return new WaitForFixedUpdate();
		}
		_PlayerPhys._canChangeGrounded = true;
	}

	//Until the players hit the ground, all gravity calculations will use the set gravity value.
	private IEnumerator LockPlayerGravityUntilGrounded ( Vector3 newGrav ) {

		if (newGrav == Vector3.zero) yield break;

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


	private void ObjectWithNoSpecificTag ( Collider Col ) {
		if (Col == null) return;

		if (!Col.TryGetComponent(out S_Data_Base Data)) { return; }

		Data.OnGet(transform);

		switch (Data)
		{
			case S_Data_RedStarRing:
				S_Data_RedStarRing DataRSR = (S_Data_RedStarRing)Data;
				_CoreValues.AdjustRings(DataRSR._ringsGained, false);
				_CoreValues.AdjustEnergy(DataRSR._energyGained);
				_CoreValues.AdjustPoints(DataRSR._powerGained);

				_ActionChain.AddToChain("Red Star Ring", 2, 1, Col.gameObject.GetInstanceID().ToString(), 15);
				break;
		}
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
			_CoreValues.AdjustRings(col.GetComponent<S_Data_Monitor>().RingAmount, true);
		}
		else if (MonitorData.Type == MonitorType.Shield) //Activates shield
		{
			_HurtAndHealth.SetShield(true);
		}

		MonitorData.DestroyMonitor();

		_ActionChain.AddToChain("Monitor", 2, 0, MonitorData.gameObject.GetInstanceID().ToString());
	}
	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	public override void AssignTools () {
		base.AssignTools();

		_AttackHandler = GetComponent<S_Handler_CharacterAttacks>();
		_HurtAndHealth = _Tools.GetComponent<S_Handler_HealthAndHurt>();
		_TriggerInteraction = GetComponent<S_Interaction_Triggers>();

		_MainSkin = _Tools.MainSkin;
		_CharacterAnimator = _Tools.CharacterAnimator;

		_Effects = _Tools.EffectsControl;
	}

	public override void AssignStats () {
		base.AssignStats();
		_powerFromSpheres_ = _Tools.LevelUpStats.pointsFromSpheres;
	}
	#endregion
}
