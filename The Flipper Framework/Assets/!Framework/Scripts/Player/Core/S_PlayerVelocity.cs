using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(S_PlayerPhysics))]
public class S_PlayerVelocity : MonoBehaviour
{
	#region properties
	#region Unity
	private S_CharacterTools	_Tools;
	private S_PlayerPhysics	_PlayerPhys;
	private Rigidbody		_RB;
	#endregion

	#region trackers
	//Stats
	private float                 _landingConversionFactor_ = 2;
	private float                 _rollingLandingBoost_;

	//Velocities in use
	[HideInInspector]
	public Vector3                _coreVelocity;                //Core velocity is the velocity under the player's control. Whether it be through movement, actions or more. It cannot exceed maximum speed. Most calculations are based on this
	[HideInInspector]
	public Vector3                _environmentalVelocity;       //Environmental velocity is the velocity applied by external forces, such as springs, fans and more.
	[HideInInspector]
	public Vector3                _totalVelocity;               //The combination of core and environmetal velocity determening actual movement direction and speed in game.
							[HideInInspector]
	public Vector3                _totalVelocityLocal;          //The speed above but relative to the players rotation.
	[HideInInspector]
	public Vector3                _worldVelocity;               //This is set at the start of a frame as a temporary total velocity, based on the actual velocity in physics. So Total Velocity is set, then affected by collision after the FixedUpdate, then adjusted by TrackAndChangeVelocity, then set here.
	[HideInInspector]
	public List<Vector3>          _previousVelocity = new List<Vector3>() {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };           //The total velocity at the end of the previous TWO frames, compared to Unity physics at the start of a frame to see if anything major like collision has changed movement.


	//ADDING 
	private List<Vector3>         _listOfVelocityToAddThisUpdate = new List<Vector3>(); //Rather than applied across all scripts, added forces are stored here and applied at the end of the frame.
	private List<Vector3>         _listOfCoreVelocityToAdd= new List<Vector3>();
	private Vector3               _externalCoreVelocity;        //Replaces core velocity this frame instead of just add to it.
	[HideInInspector]
	public float                 _externalRunningSpeed;                  //Replaces core velocity magnitude this frame, but keeps direction and applied forces.
	private bool                  _isOverwritingCoreVelocity;   //Set to true if core velocity should be completely replaced, including any aditions that would be made. If false, added forces will still be applied.

	private Vector3                 _velocityToNotCountWhenCheckingForAChange; //Increase in AddGeneralVelocity, as velocity added this here will be ignored when comparing velocity changes after collisions.
	private Vector3                 _velocityToCarryOntoNextFrame;                  //Velocity here will also be added to the above field, but is readded to worldvelocity after the changes are checked.		

	//SPEEDS
	[HideInInspector]
	public float                  _speedMagnitudeSquared;              //The speed of the player at the end of the frame.
	[HideInInspector]
	public float                  _horizontalSpeedMagnitude;    //The speed of the player relative to the character transform, so only shows running speed.
	[HideInInspector]
	public float                  _currentRunningSpeed;         //Similar to horizontalSpedMagnitde, but only core velocity, therefore the actual running velocity applied through this script.
	[HideInInspector]
	public List<float>            _previousHorizontalSpeeds = new List<float>() {1f, 2f, 3f, 4 }; //The horizontal speeds across the last few frames. Useful for collision checks.
	[HideInInspector]
	public List<float>            _previousRunningSpeeds = new List<float>() {1f, 2f, 3f}; //The horizontal speeds across the last few frames. Useful for checking if core speed has been changed in external scripts.



	//Environmental velocity can be reset based on a variety of factors. These will be set when environmental velocity is set, and then set environmental when checked and true.
	[HideInInspector]
	public bool                  _resetEnvironmentalOnGrounded;
	[HideInInspector]
	public bool                  _resetEnvironmentalOnAirAction;
	#endregion
	#endregion

	#region Inherited

	private void Awake () {
		_Tools = GetComponent<S_CharacterTools>();
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_RB = GetComponent<Rigidbody>();
	}

	/// <summary>
	/// IMPORTANT
	/// </summary>
	/// In the Unity Project Settings Script Execution Order, this is set to happen after ALL OTHER DEFAULT SCRIPTS. 
	/// This means this fixed Update will be called after every other fixedUpdate, which allows the velocity to be applied in total, at the very end.
	/// On the other end, PlayerPhysics FixedUpdate happens BEFORE every other fixedUpdate.
	private void FixedUpdate () {
		SetTotalVelocity();
		_PlayerPhys.ClearListsOfCollisions();
	}
	#endregion

	#region public
	//If the rigidbody velocity is smaller than it was last frame (such as from hitting a wall),
	//Then apply the difference to the _corevelocity as well so it knows there's been a change and can make calculations based on it.
	public void CheckAndApplyVelocityChanges () {

		Vector3 velocityThisFrame = _RB.velocity;
		Vector3 velocityLastFrame = _previousVelocity[0];

		bool fromAirToGround = _PlayerPhys._isGrounded && _PlayerPhys._wasInAirLastFrame;

		if (fromAirToGround)
		{
			//If the last time environmental velocity was set, it was set to reset here, then remove environmnetal velocity.
			if (_resetEnvironmentalOnGrounded)
			{
				//Must remove from the velocity change calculations otherwise coreVelocity wont be updated accurately.
				velocityThisFrame -= _environmentalVelocity;
				velocityLastFrame -= _environmentalVelocity;
				SetEnvironmentalVelocity(Vector3.zero, false, false, S_GeneralEnums.ChangeLockState.Unlock);
			}
			_PlayerPhys._wasInAirLastFrame = false;
		}

		Debug.DrawRay(_PlayerPhys._CharacterCenterPosition, velocityThisFrame * Time.deltaTime, Color.blue, 10f);
		Debug.DrawRay(_PlayerPhys._CharacterCenterPosition, velocityLastFrame * Time.deltaTime, Color.green, 10f);

		//General velocities applied just for last frame (like an anti offset set when groundsticking) are removed later on in this script so should not be factored in here.
		if (_velocityToNotCountWhenCheckingForAChange != Vector3.zero)
		{
			velocityLastFrame -= _velocityToNotCountWhenCheckingForAChange;
			velocityThisFrame -= _velocityToNotCountWhenCheckingForAChange;
		}

		//The magnitudes of the old and current total velocities
		float speedThisFrameSquared = velocityThisFrame.sqrMagnitude;
		float speedLastFrameSquared = velocityLastFrame.sqrMagnitude;
		//Sqrmagnitude is much faster, but not the actual speed, so when needing to apply the speed later, must use a single pricey square root function.

		//Only apply the changes if physics decreased the speed.
		if (speedThisFrameSquared < speedLastFrameSquared)
		{
			Debug.DrawRay(transform.position, Vector3.up * 2, Color.black, 10f);
			Debug.DrawRay(transform.position, _coreVelocity.normalized * 8, Color.red, 10f);

			float angleChange = Vector3.Angle(velocityThisFrame, velocityLastFrame);
			if (speedThisFrameSquared < 0.01f) { angleChange = 0; } //Because angle would still be calculated even if a one vector is zero.

			float speedSquaredDifference = Mathf.Max(speedLastFrameSquared - speedThisFrameSquared, 0);

			//----Undoing Changes----

			//Converting speed from landing onto running downhill
			if (fromAirToGround)
			{
				Vector4 newVelocityAndSpeed = LandOnSlope(velocityThisFrame, velocityLastFrame, speedThisFrameSquared);
				velocityThisFrame = newVelocityAndSpeed;
				speedThisFrameSquared = Mathf.Pow(newVelocityAndSpeed.w, 2);
			}

			// If already moving and the change is just making the player bounce off upwards slightly, then ignore velocity change
			else if (angleChange > 1 && angleChange < 15  //If a slight angle change
				&& speedLastFrameSquared > 2	//To help avoid jittering against a wall when moving from zero speed.
				&& Vector3.Angle(velocityThisFrame, transform.up) - 5 < Vector3.Angle(velocityLastFrame, transform.up) //If new velocity is taking the player noticeably more upwards
				&& speedSquaredDifference < Mathf.Min(5*5f, speedLastFrameSquared * 0.1f)) //If not too much speed was lost
			{
				//While this undoes changes, if running into a wall and the velocity keeps resetting, then the player would slide up the wall slowly.
				velocityThisFrame = velocityLastFrame;
				speedThisFrameSquared = speedLastFrameSquared;
			}

			//If the difference in speed is minor(such as lightly colliding with a slope when going up), then ignore the change.
			else if (speedSquaredDifference < Mathf.Max(15*15, speedLastFrameSquared * 0.3f) && speedSquaredDifference > 0.5f)
			{
				//These sudden changes will almost always be caused by collision, but running into a wall at an angle redirects the player, while running into a floor or ceiling should be ignored.
				//If only having horizontal velocity changed, don't change direction by increase speed slightly to what it was before for smoothness.
				if (Mathf.Abs(velocityThisFrame.normalized.y - velocityLastFrame.normalized.y) < 0.12f)
				{
					speedThisFrameSquared = Mathf.Lerp(speedThisFrameSquared, speedLastFrameSquared, 0.1f);
					velocityThisFrame = velocityThisFrame.normalized * Mathf.Sqrt(speedThisFrameSquared);
				}
				//If changing vertically, this will either be an issue with bumping into the ground while running, or landing and converting fall speed to run speed.
				else if (_PlayerPhys._isGrounded)
				{
					speedThisFrameSquared = speedLastFrameSquared;
					velocityThisFrame = velocityThisFrame.normalized * Mathf.Sqrt(speedThisFrameSquared);
				}
			}

			//----Confirming Changes-----

			//Apply to local velocities what happened to the physics one
			Vector3 vectorDifference =  velocityThisFrame - velocityLastFrame;

			//Since collisions will very rarely put velocity to 0 exactly, add some wiggle room to end player movement if currentVelocity has not been reverted. This will only trigger if player was already moving.
			if (speedThisFrameSquared < 1f && speedLastFrameSquared > speedThisFrameSquared + 0.1f)
			{
				_coreVelocity = Vector3.zero;
			}
			//Set to zero if the loss was subsantial and not almost entirely just in vertical difference
			//(to avoid losing lateral speed when landing and losing vertical speed)
			else if (speedThisFrameSquared < speedLastFrameSquared * 0.15f 
				&& (speedThisFrameSquared + Mathf.Pow(Mathf.Abs(velocityLastFrame.y),2)) < speedLastFrameSquared)
			{
				_coreVelocity = Vector3.zero;
			}
			//To ensure core Velocity isn't inverted or even increased if it loses more than itself.
			else if (speedSquaredDifference > _coreVelocity.sqrMagnitude + 0.1f)
			{
				_coreVelocity = Vector3.zero;
			}
			//Otherwise, apply changes so coreVelocity is aware.
			else
			{
				_coreVelocity += vectorDifference;
			}

			//If environmental velocity is in use, decrease as well to track the collisions.
			if (_environmentalVelocity.sqrMagnitude > 3)
			{
				if (_environmentalVelocity.sqrMagnitude > vectorDifference.sqrMagnitude)
				{
					_environmentalVelocity += vectorDifference;
				}
				else
				{
					_environmentalVelocity = Vector3.zero;
				}
			}

			//Debug.DrawRay(transform.position, _coreVelocity.normalized * 5, Color.cyan, 10f);
		}
		//World velocity is the actual rigidbody velocity found at the start of the frame, edited here if needed, with some of the removed velocity reapplied.
		_worldVelocity = velocityThisFrame + _velocityToCarryOntoNextFrame;

		_velocityToCarryOntoNextFrame = Vector3.zero;
		_velocityToNotCountWhenCheckingForAChange = Vector3.zero; //So this can be increased over this update, then checked again at the start of this method.
	}

	//If just landed, apply additional speed dependant on slope angle.
	public Vector4 LandOnSlope ( Vector4 currentVelocity, Vector3 previousVelocity, float physicsCalculatedSpeedSquared ) {

		float newSpeed = Mathf.Max(_previousHorizontalSpeeds[1], _previousRunningSpeeds[1]);
		Vector3 horizontalDirection = _totalVelocity.normalized;
		horizontalDirection.y = 0;

		//If was falling down faster last frame, but still going downhill and not uphill.
		if (previousVelocity.y < currentVelocity.y && currentVelocity.y < -10 && Vector3.Dot(_PlayerPhys._groundNormal, horizontalDirection) > 0f)
		{
			//Get magnitude,higher if rolling.
			float lerpValue = _PlayerPhys._isRolling ?  _landingConversionFactor_ * _rollingLandingBoost_ :  _landingConversionFactor_;

			newSpeed = Mathf.Lerp(newSpeed, Mathf.Sqrt(physicsCalculatedSpeedSquared), lerpValue);
		}


		currentVelocity = currentVelocity.normalized * newSpeed;
		currentVelocity.w = newSpeed;
		return currentVelocity;
	}

	//After every other calculation has been made, all of the new velocities and combined and set to the rigidbody.
	//This includes the core and environmental velocities, but also the others that have been added into lists using the addvelocity methods.
	public void SetTotalVelocity () {

		//Core velocity that's been calculated across this script. Either assigns what it should be, or adds the stored force pushes.
		if (_externalCoreVelocity != default(Vector3))
		{
			_coreVelocity = _externalCoreVelocity;
			_externalCoreVelocity = default(Vector3);
		}
		if (!_isOverwritingCoreVelocity)
		{
			//Using a for loop instead of a foreach makes it longer to read, but creates less garbage so improves performance
			for (int i = 0 ; i < _listOfCoreVelocityToAdd.Count ; i++)
			{
				_coreVelocity += _listOfCoreVelocityToAdd[i];
			}
		}

		//Calculate total velocity this frame.
		_totalVelocity = _coreVelocity + _environmentalVelocity;
		for (int i = 0 ; i < _listOfVelocityToAddThisUpdate.Count ; i++)
		{
			_totalVelocity += _listOfVelocityToAddThisUpdate[i];
		}

		_totalVelocityLocal = transform.InverseTransformDirection(_totalVelocity);

		//Clear the lists to prevent forces carrying over multiple frames.
		_listOfCoreVelocityToAdd.Clear();
		_listOfVelocityToAddThisUpdate.Clear();
		_isOverwritingCoreVelocity = false;

		//Sets rigidbody velocity, this should be the only line in the player scripts to do so.
		_RB.velocity = _totalVelocity;

		//Adds this new velocity to a list of 2, tracking the last 2 frames.
		_previousVelocity.Insert(0, _totalVelocity);
		_previousVelocity.RemoveAt(5);

		//Assigns the global variables for the current movement, since it's assigned at the end of a frame, changes between frames won't be counted when using this,
		_speedMagnitudeSquared = _totalVelocity.sqrMagnitude;
		Vector3 releVec = _PlayerPhys.GetRelevantVector(_totalVelocity, false);
		_horizontalSpeedMagnitude = releVec.magnitude;

		//Running speed is the speed specififically controlled by the player, which if there aren't notable additions from other types of velociity, is the same as horizontal speed.
		if ((_totalVelocity - _coreVelocity).sqrMagnitude > 25)
		{
			releVec = _PlayerPhys.GetRelevantVector(_coreVelocity, false);
			_currentRunningSpeed = releVec.magnitude;
		}
		else
			_currentRunningSpeed = _horizontalSpeedMagnitude;

		//Adds this new speed to a list of 3
		_previousHorizontalSpeeds.Insert(0, _horizontalSpeedMagnitude);
		_previousHorizontalSpeeds.RemoveAt(4);

		_previousRunningSpeeds.Insert(0, _currentRunningSpeed);
		_previousRunningSpeeds.RemoveAt(3);
	}
	#endregion

	#region public commonly used
	//the following methods are called by other scripts when they want to affect the velocity. The changes are stored and applied in the SetTotalVelocity method.
	public void AddCoreVelocity ( Vector3 force, bool shouldPrintForce = false ) {

		_listOfCoreVelocityToAdd.Add(force);
		if (shouldPrintForce) Debug.Log("ADD Core FORCE  ");
	}
	public void SetCoreVelocity ( Vector3 force, string willOverwrite = "", bool shouldPrintForce = false ) {
		if (_isOverwritingCoreVelocity && willOverwrite == "")
		{ return; } //If a previous call set isoverwriting to true, then if this isn't doing the same it will be ignored.

		//If both lateral values are set to minus 1, this means we only want to change the vertical velocity.
		if(force.x == -1 && force.z == -1) { force = new Vector3(_coreVelocity.x, force.y, _coreVelocity.z); }

		_isOverwritingCoreVelocity = willOverwrite == "Overwrite"; //If true, core velocity will be fully replaced, including additions. Sets to true rather than same bool, because setting to false would overwrite this.

		_externalCoreVelocity = force == Vector3.zero ? new Vector3(0, 0.02f, 0) : force; //Will not use Vector3.zero because that upsets the check seeing if this is not default(Vector3). To avoid that, use a tiny velocity.
		if (shouldPrintForce) Debug.Log("Set Core FORCE");
	}

	//This will change the magnitude of the local lateral velocity vector in ControlledVelocity but will not change the direction.
	public void SetLateralSpeed ( float speed, bool shouldPrintForce = true ) {
		_externalRunningSpeed = speed; //This will be set to negative at the end of the frame, but if changed here will be applied in HandleControlledVelocity (NOT SetTotalVelocity). This is because this should only change running speed.
		if (shouldPrintForce) Debug.Log("Set Core SPEED");
	}
	public void SetBothVelocities ( Vector3 force, Vector2 split, string willOverwrite = "", bool shouldPrintForce = false ) {
		_environmentalVelocity = force * split.y;

		SetCoreVelocity(force * split.x, willOverwrite, shouldPrintForce);

		if (shouldPrintForce) Debug.Log("Set Total FORCE To " + force);
	}

	//Bear in mind velocity added in this method will only last this frame, as the velocity will be recalclated without it next fixedUpdate.
	public void AddGeneralVelocity ( Vector3 force, bool shouldIncludeThisNextCheck = true, bool carryOntoFrameAfterCheck = false, bool shouldPrintForce = false ) {
		_listOfVelocityToAddThisUpdate.Add(force);

		if (!shouldIncludeThisNextCheck)
		{
			_velocityToNotCountWhenCheckingForAChange += force;
			if (carryOntoFrameAfterCheck)
				_velocityToCarryOntoNextFrame += force;
		}

		if (shouldPrintForce) Debug.Log("ADD Total FORCE  " + force);
	}

	//Environmental. Caused by objects in the world, but can b removed by others.
	public void SetEnvironmentalVelocity ( Vector3 force, bool willReturnDecelOnGrounded, bool willReturnDecelOnAirAction,
		S_GeneralEnums.ChangeLockState whatToDoWithDeceleration = S_GeneralEnums.ChangeLockState.Ignore, bool shouldPrintForce = false ) {

		_environmentalVelocity = force;

		//Because HandleDeceleration can be called to lock multiple times before being called to unlock (because Unlock should only be called when removing environmnetal velocity),
		//only add a new lock if it hasn't locked this way already.
		if ((_resetEnvironmentalOnAirAction && willReturnDecelOnAirAction) || (_resetEnvironmentalOnGrounded && willReturnDecelOnGrounded))
		{
			//Intentionally empty, as this prevents applying multiple lockDecelerations from env velocity.
		}
		else
		{
			//This will apply or remove constraints on deceleration, as certain calls will prevent manual deceleration, while calls that remove this velocity will allow it again.
			if (willReturnDecelOnAirAction && willReturnDecelOnGrounded) { whatToDoWithDeceleration = S_GeneralEnums.ChangeLockState.Lock; }
			HandleDecelerationWhenEnvironmentalForce(whatToDoWithDeceleration);
		}

		_resetEnvironmentalOnGrounded = willReturnDecelOnGrounded;
		_resetEnvironmentalOnAirAction = willReturnDecelOnAirAction;

		if (shouldPrintForce) Debug.Log("Set Environmental FORCE  " + force);
	}

	private void HandleDecelerationWhenEnvironmentalForce ( S_GeneralEnums.ChangeLockState whatCase ) {

		//Due to deceleration working with core velocity at different speeds. Sometimes when environmental velocity is set, it will prevent deceleration because that would make the movement path inconsistent
		//(as core velocity won't always be the same before environmental is added).
		switch (whatCase)
		{
			//This should always be called before Unlock. As such, whenever an environmental velocity is setting willRemoveOnGrounded to true, it should do this. Because the check will call unlock if true, then stop checking.
			case S_GeneralEnums.ChangeLockState.Lock:
				S_S_Logic.AddLockToList(ref _PlayerPhys._locksForCanDecelerate, "EnvironmentForce");
				//_PlayerPhys._locksForCanDecelerate.Add(false); 
				break;
			//This should only be called when environmental velocity is being removed.
			case S_GeneralEnums.ChangeLockState.Unlock:
				S_S_Logic.RemoveLockFromList(ref _PlayerPhys._locksForCanDecelerate, "EnvironmentForce"); break;
				//Ignore is the default state, which means this call won't change the deceleration ability.
		}
	}

	//Called by air actions to check if environmental velocity should be removed.
	public void RemoveEnvironmentalVelocityAirAction () {
		//If The last time environmental velocity was set, it was set to reset here, then remove environmental velocity. This will be called in other air actions as well.
		if (_resetEnvironmentalOnGrounded)
		{
			SetEnvironmentalVelocity(Vector3.zero, false, false, S_GeneralEnums.ChangeLockState.Unlock);
		}
	}
	#endregion


	#region Assigning
	private void AssignStats () {
		_landingConversionFactor_ = _Tools.Stats.SlopeStats.landingConversionFactor;
		_rollingLandingBoost_ = _Tools.Stats.RollingStats.rollingLandingBoost;

	}
	#endregion
}
