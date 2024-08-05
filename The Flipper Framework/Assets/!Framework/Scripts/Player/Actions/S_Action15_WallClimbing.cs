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
	private float       _climbWallDistance;
	private bool        _isSwitchingToGround;

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

	private void FixedUpdate () {
		if (_isWall)
		{
			CheckCanceling();
			ClimbingInteraction();
			CheckSpeed();
			ClimbingPhysics();

			//If going from climbing wall to running on flat ground normally.
			if (_isSwitchingToGround)
			{
				FromWallToGround();
			}
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	public void SetupClimbing ( RaycastHit wallHit ) {

		//Set wall and type of movement
		_wallHit = wallHit;

		//Set speed to start movement on.
		_currentClimbingSpeed = _PlayerPhys._totalVelocity.y;
		if (_currentClimbingSpeed < 0)
		{
			_currentClimbingSpeed = Mathf.Lerp(_currentClimbingSpeed, 0, 0.5f);
		}
		_currentClimbingSpeed = Mathf.Clamp(_currentClimbingSpeed, -20, 90);

		//Set the climbing speed based on player's speed, but won't reach it until lerped.
		_goalClimbingSpeed = Mathf.Max(_PlayerPhys._horizontalSpeedMagnitude * 0.8f, _currentClimbingSpeed);
		_goalClimbingSpeed *= _climbModi_;

		//Sets min and max climbing speed
		_goalClimbingSpeed = 8f * (int)(_goalClimbingSpeed / 8);
		_goalClimbingSpeed = Mathf.Clamp(_goalClimbingSpeed, 48, 176);

		_climbWallDistance = Vector3.Distance(wallHit.point, transform.position) + 1; //Ensures first checks for x seconds will find the wall.

		_Actions._ActionDefault.SetSkinRotationToVelocity(0, -_wallHit.normal, Vector2.zero, GetUpDirectionOfWall(wallHit.normal));

		StartAction();
		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.WallClimbing); //Not part of startaction because that is inherited from action12
	}

	//Monitors progression along the wall, checking its still there.
	void ClimbingInteraction () {

		Vector3 raycastOrigin = transform.position + _MainSkin.up * 0.3f;
		_isWall = Physics.Raycast(raycastOrigin, _MainSkin.forward, out RaycastHit tempHit, _climbWallDistance, _wallLayerMask_);

		//First x seconds are too attach to the wall from starting point, so decrease check range after.
		if (_counter > 0.3f)
		{
			_climbWallDistance = _wallCheckDistance_;
		}

		//If they reach the top of the wall
		if (!_isWall)
		{
			//Bounces the player up to keep momentum
			StartCoroutine(JumpOverWall(-_wallHit.normal));
		}
		else
		{
			_wallHit = tempHit;
		}

		//Ensures the player faces the wall
		_Actions._ActionDefault.SetSkinRotationToVelocity(0, -_wallHit.normal, Vector2.zero, GetUpDirectionOfWall(_wallHit.normal));
		_CurrentWall = _wallHit.collider.gameObject;
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
			_PlayerPhys.SetCoreVelocity(newVec);

			_Actions._ActionDefault.StartAction();
		}
		//Apply force into and up the wall.
		else
		{
			Vector3 newVec = GetUpDirectionOfWall(_wallHit.normal) * _currentClimbingSpeed;
			newVec += (_MainSkin.forward * 20f);
			_PlayerPhys.SetCoreVelocity(newVec);
		}

		//If the wall stops being very steep
		if (!_WallHandler.IsWallVerticalEnough(_wallHit.normal, 0.45f))
		{
			//Sets variables to go to swtich to ground option in FixedUpdate
			_isSwitchingToGround = true;
		}
	}


	//Called when wall being climbed is flattening out, to transition to running along the wall as actual floor.
	void FromWallToGround () {
		_PlayerPhys._isGravityOn = true;

		//Set rotation to put feet on ground.
		Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), -_MainSkin.up, out _wallHit, _climbWallDistance, _wallLayerMask_);

		_PlayerPhys.AlignToGround(_wallHit.normal, true);
		_PlayerPhys.CheckForGround();

		//Set velocity to move along and push down to the ground
		Vector3 newVec = _MainSkin.forward * (_goalClimbingSpeed);
		newVec += -_wallHit.normal * 10f;

		_PlayerPhys.SetCoreVelocity(newVec);
		_Actions._ActionDefault.StartAction();
	}

	//Called when wall is lost, to add velocity up and over the lip, to keep going.
	IEnumerator JumpOverWall ( Vector3 inwards ) {
		Vector3 newVelocity = _MainSkin.up * _currentClimbingSpeed;

		yield return new WaitForFixedUpdate();

		Debug.Log("Lost Wall");
		_Actions._ActionDefault.StartAction();

		newVelocity += inwards * _PlayerPhys.GetRelevantVel(_originalVelocity).x;
		_PlayerPhys.SetCoreVelocity(newVelocity);
	}
	#endregion
}
