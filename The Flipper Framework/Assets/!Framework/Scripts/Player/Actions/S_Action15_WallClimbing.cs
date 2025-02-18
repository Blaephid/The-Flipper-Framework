using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(S_Handler_WallActions))]
public class S_Action15_WallClimbing : S_Action12_WallRunning
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	// Trackers
	#region trackers
	[Header("Wall Climbing")]
	private float        _goalClimbingSpeed;

	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {

	}

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyAction();
	}

	new private void FixedUpdate () {
		base.FixedUpdate();
		if (_isWall)
		{
			ClimbingInteraction();
			CheckCanceling();
			CheckSpeed();
			ClimbingPhysics();

			HandleInputs();
		}
		else
		{
			_Input.UnLockInput();
			_Actions._ActionDefault.StartAction();
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private


	//Monitors progression along the wall, checking its still there.
	void ClimbingInteraction () {

		_Input.LockInputForAWhile(20f, false, Vector3.zero); //Locks input for half a second so any actions that end this don't have immediate control.

		_raycastOrigin = _PlayerPhys._CharacterCenterPosition + (_MainSkin.up * 0.2f) - (_MainSkin.forward * 0.3f);
		_isWall = Physics.Raycast(_raycastOrigin, _MainSkin.forward, out RaycastHit tempHit, _checkDistance, _wallLayerMask_);

		//First x seconds are too attach to the wall from starting point, so decrease check range after.
		if (_counter > 0.2f)
		{
			_checkDistance = _wallCheckDistance_.x;
			_CamHandler._HedgeCam.ChangeHeight(Mathf.Sign(_goalClimbingSpeed) * -50, 80); //Moves camera to look up or down (based on which direction moving)
		}
		else
		{
			_CamHandler._HedgeCam.ChangeHeight(-50, 250); //Quickly look up to show start of climb
		}

		//If they reach the top of the wall
		if (!_isWall)
		{
			//Bounces the player up to keep momentum
			StartCoroutine(ReachLipOfWall(-_wallHit.normal));
		}
		else
		{
			_Actions._ActionDefault.HandleAnimator(1);
			_wallHit = tempHit;
			_CurrentWall = _wallHit.collider.gameObject;
		}

		//Ensures the player faces the wall
		_Actions._ActionDefault.SetSkinRotationToVelocity(0, -_wallHit.normal, Vector2.zero, GetUpDirectionOfWall(_wallHit.normal));

		//Setting global variables for other scripts
		_Actions._jumpAngle = Vector3.Lerp(_wallHit.normal, Vector3.up, 0.2f) ;
		_Actions._dashAngle = _wallHit.normal;

		//To control camera distance and FOV
		_PlayerVel._currentRunningSpeed = Mathf.Max(Mathf.Abs(_goalClimbingSpeed), 60);

		//If the wall stops being very steep
		if (!_WallHandler.IsWallVerticalEnough(_wallHit.normal, 0.6f) && _goalClimbingSpeed > 5)
		{
			//Sets variables to go to swtich to ground option in FixedUpdate
			FromWallToGround();
		}
	}

	//Changes current speed and decreases velocity up the wall.
	private void CheckSpeed () {
		_goalClimbingSpeed = Mathf.Lerp(_goalClimbingSpeed, -30, 0.015f);
		_currentClimbingSpeed = _currentClimbingSpeed < _goalClimbingSpeed ? Mathf.Lerp(_currentClimbingSpeed, _goalClimbingSpeed, 0.1f) : _goalClimbingSpeed;
	}

	//Responsible for applying movement 
	private void ClimbingPhysics () {
		//After being on the wall for too long or about to hit a ceiling.
		if (_goalClimbingSpeed < -_fallOffAtFallSpeed_ || Physics.Raycast(transform.position, _MainSkin.up, 12, _wallLayerMask_))
		{
			//Drops and send the player back a bit.
			Vector3 newVec = new Vector3(0f, _goalClimbingSpeed, 0f);
			newVec += (-_MainSkin.forward * 10f);
			_PlayerVel.SetCoreVelocity(newVec);

			_Actions._ActionDefault.StartAction();
		}
		//Apply force into and up the wall.
		else
		{
			Vector3 newVec = GetUpDirectionOfWall(_wallHit.normal) * _currentClimbingSpeed;
			newVec += (_MainSkin.forward * 20f);
			_PlayerVel.SetCoreVelocity(newVec);
		}
	}

	//Called when wall is lost, to add velocity up and over the lip, to keep going.
	private IEnumerator ReachLipOfWall ( Vector3 inwards ) {
		Vector3 newVelocity = _MainSkin.up * _currentClimbingSpeed;

		yield return new WaitForFixedUpdate();
		_Actions._ActionDefault.StartAction();

		newVelocity += inwards * _PlayerPhys.GetRelevantVector(_originalVelocity).x;
		_PlayerVel.SetCoreVelocity(newVelocity);
	}

	//Called when wall being climbed is flattening out, to transition to running along the wall as actual floor.
	private void FromWallToGround () {

		_PlayerPhys._canChangeGrounded = true; //This is also said in stopAction, but this is also called here so the below works.
		_PlayerPhys.AlignToGround(_wallHit.normal, true); //Rotate so the wall is now under the feet
		_PlayerPhys.CheckForGround(); //Check under the feet for ground.

		//Set velocity to move along and push down to the ground
		Vector3 newVec = GetUpDirectionOfWall(_wallHit.normal) * (_goalClimbingSpeed);

		_Input.UnLockInput(); //Because this action sets input to lock for 30 frames, undo this immediately when regaing control.
		StartCoroutine(StayRollingAnimationForAWhile(7));

		StartCoroutine(_CamHandler._HedgeCam.KeepGoingToHeightForFrames(30, 20, 170));

		_PlayerVel.SetCoreVelocity(newVec);
		_Actions._ActionDefault.StartAction();
	}

	//To ensure snapping to ground isn't sudden, hide via using the same animation as on the wall before returning to standing.
	private IEnumerator StayRollingAnimationForAWhile (int frames) {

		for(int i = 0; i < frames; i++)
		{
			yield return new WaitForFixedUpdate();
			_Actions._ActionDefault._isAnimatorControlledExternally = true;
			_Actions._ActionDefault._animationAction = 1;

			_Input.LockInputForAWhile(0, false, Vector3.zero, S_GeneralEnums.LockControlDirection.CharacterForwards);
		}

		_Actions._ActionDefault._isAnimatorControlledExternally = false;
		_Actions._ActionDefault._animationAction = 0;
		_CharacterAnimator.SetTrigger("ChangedState");
	}

	#endregion

	public void SetupClimbing ( RaycastHit wallHit ) {

		//Set wall and type of movement
		_wallHit = wallHit;

		//Set speed to start movement on.
		_currentClimbingSpeed = _PlayerVel._worldVelocity.y;
		if (_currentClimbingSpeed < 0)
		{
			_currentClimbingSpeed = Mathf.Lerp(_currentClimbingSpeed, 0, 0.5f);
		}
		_currentClimbingSpeed = Mathf.Clamp(_currentClimbingSpeed, -20, 90);

		//Set the climbing speed based on player's speed, but won't reach it until lerped.
		_goalClimbingSpeed = Mathf.Max(_PlayerVel._horizontalSpeedMagnitude * _climbModi_, _currentClimbingSpeed);

		//Sets min and max climbing speed while ensuring climbing is in increments of x
		_goalClimbingSpeed = Mathf.Clamp(S_S_MoreMathMethods.GetNumberAsIncrement(_goalClimbingSpeed, 10f), 50, 160);

		_checkDistance = wallHit.distance + 1; //Ensures first checks for x seconds will find the wall.

		_Actions._ActionDefault.SetSkinRotationToVelocity(0, -_wallHit.normal, Vector2.zero, GetUpDirectionOfWall(wallHit.normal));

		_Actions.ChangeAction(S_S_ActionHandling.PrimaryPlayerStates.WallClimbing); //Not part of startAction because that is inherited from action12, and other stopActions should be triggered before this (E.G. so CanChangeGrounded is accurate).
		StartAction();
	}
}
