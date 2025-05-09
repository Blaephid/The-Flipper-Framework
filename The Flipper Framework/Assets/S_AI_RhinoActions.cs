using SplineMesh;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SplineMesh.Spline;

[RequireComponent(typeof(S_AI_RailEnemy))]
public class S_AI_RhinoActions : MonoBehaviour
{
	[BaseColour(0.5f,0.5f,0.5f,1)]
	[SerializeField, ColourIfNull(1f, 0.5f, 0.5f, 1f)] GameObject _Projectile_;
	[BaseColour(0.5f,0.5f,0.5f,1)]
	[SerializeField, ColourIfNull(1f, 0.5f, 0.5f, 1f)] Transform _ShootPoint_;
	[BaseColour(0.5f,0.5f,0.5f,1)]
	[SerializeField, ColourIfNull(1f, 0.5f, 0.5f, 1f)] ParticleSystem _ReadyShotParticle_;
	[BaseColour(0.5f,0.5f,0.5f,1)]
	[SerializeField, ColourIfNull(1f, 0.5f, 0.5f, 1f)] S_Data_HomingTarget _HomingTarget;
	[BaseColour(0.5f,0.5f,0.5f,1)]
	[SerializeField, ColourIfNull(1f, 0.5f, 0.5f, 1f)] S_CustomGravity _CustomGravity;
	[BaseColour(0.5f,0.5f,0.5f,1)]
	[SerializeField, ColourIfNull(1f, 0.5f, 0.5f, 1f)] Collider _SolidCollider;

	private S_AI_RailEnemy _RailBehaviour;

	private bool _currentlyOnRail;
	private bool _isSwitching;
	private bool _isShooting;
	private bool _isJumping;

	private Transform _Target;
	private S_PlayerVelocity _TargetVel;
	private S_ActionManager _TargetActions;

	[Header("Attacks")]
	[Tooltip("How many frames after the shot until the cannonball hits the player. This effects shot speeds but calculates where the player will be in x frames, and sets speed to reach that.")]
	[SerializeField] private float _framesToInterceptPlayer_ = 30;
	[SerializeField] public float  _timeToReadyShot = 0.6f;
	[SerializeField] private Vector2 _minMaxShotSpeeds = new Vector2(60, 120);
	[SerializeField] private LayerMask _BlockingShotLayers;

	[Header("Hopping")]
	[SerializeField] private LayerMask _LayersOfRailsAndBlockers;
	[SerializeField] private float _hopSpeed_ = 8;
	float _timeSpentReadyingShot;

	int _framesSinceJump;
	const int _framesAfterJumpToStartSearchingForNewRail = 30;

	private void Start () {
		_RailBehaviour = GetComponent<S_AI_RailEnemy>();
	}

	private void Update () {
		_RailBehaviour._RF.ApplyHopUpdate(_hopSpeed_, null);
	}

	private void FixedUpdate () {
		//After time, start checking for new rails.
		if (_isJumping)
		{
			_framesSinceJump++;
			if (_framesSinceJump >= _framesAfterJumpToStartSearchingForNewRail)
			{ LookForNewRail(); }
		}
		//Increment towards making shot.
		else if (_isShooting)
		{
			_timeSpentReadyingShot += Time.fixedDeltaTime;

			if (_timeSpentReadyingShot >= _timeToReadyShot)
			{
				Shoot();
			}
			else if (!CanShoot(_Target.position)) SetIsShooting(false);
		}
		else if (_isSwitching)
		{
			ApplyHopFixedUpdate();
		}
	}


	public void TriggerOn () {
		SetOnRail(true);
	}

	#region JumpOffRail
	public void JumpFromRail () {
		Vector3 JumpForce = transform.up * 60;
		_RailBehaviour._RB.velocity *= 0.95f; //Allow player to catch up if they're jumping ahead.
		_RailBehaviour._RB.velocity += JumpForce;

		_isJumping = true;
		SetIsShooting(false);
		_isSwitching = false;

		_CustomGravity._isGravityOn = true;

		SetOnRail(false);
		_framesSinceJump = 0;
	}

	#endregion

	#region Switching

	public bool CanSwitch ( float distanceBetweenRails ) {

		if (_isShooting || _isSwitching || _isJumping) { return false; }

		float currentSpeed = _RailBehaviour._RF._grindingSpeed;
		float distanceNeeded = currentSpeed * Time.fixedDeltaTime * 80;

		//Cant switch if too close to ending current rail.
		if (!_RailBehaviour._RF._isGoingBackwards && _RailBehaviour._RF._PathSpline.Length - _RailBehaviour._RF._pointOnSpline < distanceNeeded) return false;
		else if (_RailBehaviour._RF._isGoingBackwards && _RailBehaviour._RF._pointOnSpline < distanceNeeded) { return false; }

		float firstDirection =  UnityEngine.Random.value < 0.5f ? -1 : 1;

		if (IsValidRailOnSide(firstDirection, distanceBetweenRails))
		{
			RailSwitch(firstDirection > 0, distanceBetweenRails); return true;
		}
		if (IsValidRailOnSide(-firstDirection, distanceBetweenRails))
		{
			RailSwitch(-firstDirection > 0, distanceBetweenRails); return true;
		}

		return false;
	}


	private void RailSwitch ( bool right, float distance ) {
		_isSwitching = true;
		SetOnRail(false);
		_RailBehaviour.SetAnimatorTrigger("Twirl");

		_RailBehaviour._RF.ReadyHopValues(right, distance, _hopSpeed_);
	}

	//Uses a box to check for rail at the distance distance and direction. Can be interupted by other colliders.
	private bool IsValidRailOnSide ( float side, float distance ) {
		Vector3 AboveRail = transform.position + transform.up * 15;

		AboveRail += transform.right * side * distance;

		Vector3 boxHalfExtents = new Vector3 (1,1,15); //Longer than it is wide so it can check for player or other rhinos.
		if (Physics.BoxCast(AboveRail, boxHalfExtents, -transform.up, out RaycastHit Hit, transform.rotation, 15, _LayersOfRailsAndBlockers, QueryTriggerInteraction.Collide))
		{
			//If hit something, check if a rail, or a blocker.
			return Hit.collider.CompareTag("Rail");
		}

		return false;
	}

	private void ApplyHopFixedUpdate () {
		if (_RailBehaviour._RF._distanceToHop > 0)
		{

			//Near the end of a step, renable collision so can collide again with grind on them instead.
			if (_RailBehaviour._RF._distanceToHop < 1)
			{
				if (LookForNewRail()) return;
			}

			//If no rail, fall.
			if (_RailBehaviour._RF._distanceToHop <= 0.1f)
			{
				_RailBehaviour._RB.velocity += -transform.up * 2;
				_RailBehaviour._RF._distanceToHop = 0;
				_RailBehaviour.LoseRail();
			}
			else
			{
				_RailBehaviour._RB.velocity += (_RailBehaviour._RF._sampleRight * ((_RailBehaviour._RF._hopThisFrame * -1) / Time.deltaTime));
			}
		}
	}

	#endregion

	private void SetOnRail ( bool set ) {

		if (set != _currentlyOnRail)
		{
			_currentlyOnRail = set;
			if (set)
			{
				_RailBehaviour._isActive = true;
				_RailBehaviour.SetAnimatorBool("CurrentlyOnRail", true);
				_HomingTarget.OnHit = S_Data_HomingTarget.EffectOnHoming.shootdown;
				_HomingTarget.OnDestroy = S_Data_HomingTarget.EffectOnHoming.shootdown;
				_CustomGravity._isGravityOn = false;
			}
			else
			{
				_RailBehaviour.SetAnimatorBool("CurrentlyOnRail", false);
				_HomingTarget.OnHit = S_Data_HomingTarget.EffectOnHoming.normal;
				_HomingTarget.OnDestroy = S_Data_HomingTarget.EffectOnHoming.normal;
			}
		}
	}

	private bool LookForNewRail () {
		Collider[] FindRailColliders = Physics.OverlapSphere(transform.position, 2, _LayersOfRailsAndBlockers, QueryTriggerInteraction.Collide);
		if (FindRailColliders.Length > 0)
		{
			Collider Col = FindRailColliders[0];
			if (CanLandOnNewRail(Col))
			{
				return true;
			}
		}
		return false;
	}


	private bool CanLandOnNewRail ( Collider Col ) {
		if (Col.TryGetComponent(out S_SplineInParent SplineFromCollider))
		{
			Debug.DrawLine(transform.position, Col.transform.position, Color.cyan, 10f);

			//End hop
			_isSwitching = false;
			_isJumping = false;
			_RailBehaviour._RF._distanceToHop = 0;

			//Animator
			SetOnRail(true);

			//New spline logic
			_RailBehaviour.SetRail(SplineFromCollider._SplineParent, SplineFromCollider._ConnectedRails);
			_RailBehaviour._RF._pointOnSpline = S_RailFollow_Base.GetClosestPointOfSpline(transform.position, SplineFromCollider._SplineParent, Vector2.zero).x;
			_RailBehaviour._RF._setOffSet = -SplineFromCollider._Placer._mainOffset;

			return true;
		}
		return false;
	}

	#region Attacking

	public bool ReadyShot ( Transform Player, S_PlayerVelocity PlayerVel ) {
		if (_isShooting || _isSwitching || _isJumping) { return false; }

		if (!_Target)
		{
			_Target = Player;
			_TargetVel = PlayerVel;
			_TargetActions = PlayerVel.GetComponent<S_CharacterTools>()._ActionManager;
		}

		if (!CanShoot(_Target.position)) { return false; }

		SetIsShooting(true);
		_timeSpentReadyingShot = 0;

		return true;
	}

	private bool CanShoot ( Vector3 target ) {
		if (S_S_MoreMaths.GetDistanceSqrOfVectors(target, _ShootPoint_.position) < 20 * 20)
		{ return false; } //Can't shoot if player is too close.
		if (Vector3.Angle(S_S_MoreMaths.GetDirection(_ShootPoint_.position, target), transform.forward) < 80) //Cant shoot if player is in front.
		{ return false; }
		if (Physics.Linecast(target, _ShootPoint_.position, out RaycastHit Hit, _BlockingShotLayers)) //Cant shoot if solid object or other enemy blocking the way.
		{
			if (Hit.collider != _SolidCollider)
			{ return false; }
		}

		return true;
	}


	private void SetIsShooting ( bool set ) {
		if (set != _isShooting)
		{
			_isShooting = set;
			if (set)
			{
				_ReadyShotParticle_.Play();
			}
			else
			{
				_ReadyShotParticle_.Stop();
			}
		}
	}

	private void Shoot () {
		Vector3 velocity;

		//If player is not at a speed that would reach the rhino in one second, shoot normally.
		//if (S_S_MoreMaths.GetDistanceSqrOfVectors(_ShootPoint_.position, _Target.position) > _TargetVel._worldVelocity.sqrMagnitude * Time.fixedDeltaTime * _framesToInterceptPlayer_)
		if (true)
			velocity = GetVelocityOfShot( _framesToInterceptPlayer_);
		//if player is, then the bomb is likely going to be shot forwards to intercept (it will be slower than the rhino so it won't look like its being shot the wrong direction.
		else
			velocity = GetVelocityOfShot(_framesToInterceptPlayer_ / 1.5f, true);

		if(velocity == Vector3.zero) { SetIsShooting(false); return; }

		//Create projectile at point
		GameObject GO = Instantiate(_Projectile_);
		GO.transform.position = _ShootPoint_.position;
		Physics.IgnoreCollision(GO.GetComponent<Collider>(), _SolidCollider);

		GO.transform.forward = velocity.normalized;
		GO.GetComponent<Rigidbody>().velocity = velocity;

		SetIsShooting(false);
		_RailBehaviour.SetAnimatorTrigger("Attack");
	}

	private Vector3 GetVelocityOfShot (float framesToIntercept, bool playerWillPassCurrentPos = false ) {

		const int minFramesToIncercept = 10;

		//Calculate target
		framesToIntercept = Mathf.Round(framesToIntercept);
		framesToIntercept = Mathf.Max(minFramesToIncercept, framesToIntercept);
		Vector3 targetPosition = GetPlayerPositionInXFrames(framesToIntercept);

		//Calculate how fast to make projectile travel to intercept player.
		float distanceToTravel = Vector3.Distance(targetPosition, _ShootPoint_.position);
		float timeToTravel = Time.fixedDeltaTime * framesToIntercept;
		float neededSpeed = distanceToTravel / timeToTravel;

		////If sniping from a distance, apply the max speed. If needing max speed to aim forwards and intercept player, don't apply max.
		//if (playerWillPassCurrentPos && IsTooFast() && !IsTooClose())
		//{
		//	neededSpeed = Mathf.Min(neededSpeed, _minMaxShotSpeeds.y);
		//}

		Debug.Log("Position is " + targetPosition);
		Debug.Log("Speed is " + neededSpeed);
		Debug.Log("Distance is " + Vector3.Distance(targetPosition, _ShootPoint_.position));
		Debug.Log("Angle is " + Vector3.Angle(S_S_MoreMaths.GetDirection(_ShootPoint_.position, targetPosition), transform.forward));

		Debug.Log("Frames is " + framesToIntercept);

		////If shot would either be too slow, aim too close, or aiming ahead, then
		//if (framesToIntercept > minFramesToIncercept)
		//{
		//	if (CanShoot(targetPosition) || IsTooClose() || IsTooSlow() || IsShootingAhead())
		//	{
		//		Debug.Log("Pass To New");
		//		return GetVelocityOfShot(framesToIntercept / 1.5f, playerWillPassCurrentPos); //Try again for another angle to intercept player with, the framesToIncercept requirement above prevents stack overflow.
		//	}
		//}

		//if(!CanShoot(targetPosition)) { return Vector3.zero; }

		Debug.DrawRay(_ShootPoint_.position, transform.forward * 100, Color.green, 10f);
		Debug.DrawLine(targetPosition, _ShootPoint_.position, Color.cyan, 20f);

		//Get and apply velocity
		Vector3 targetDirection = (targetPosition - _ShootPoint_.position).normalized;
		Vector3 velocity = targetDirection * neededSpeed;

		Debug.Log("SHOOOOOOOOT " + framesToIntercept);
		return velocity;


		bool IsTooClose () { return S_S_MoreMaths.GetDistanceSqrOfVectors(targetPosition, _ShootPoint_.position) < 40 * 40; }
		bool IsTooSlow () { return neededSpeed < _minMaxShotSpeeds.x; }
		bool IsTooFast () { return neededSpeed > _minMaxShotSpeeds.y; }
		bool IsShootingAhead () { return Vector3.Angle(S_S_MoreMaths.GetDirection(_ShootPoint_.position, targetPosition), transform.forward) < 80; }
	}

	private Vector3 GetPlayerPositionInXFrames ( float xFrames ) {
		Vector3 targetPosition = _Target.position;
		if (_TargetVel)
		{
			switch (_TargetActions._whatCurrentAction)
			{
				//If currently on a rail, aim for the point on the rail that the player will next be, incluidng offset. This will allow them to handle curves, hills, and quicksteps.
				case S_S_ActionHandling.PrimaryPlayerStates.Rail:
					S_Action05_Rail PlayerRail = _TargetActions._ObjectForActions.GetComponent<S_Action05_Rail>();
					if (!PlayerRail._isGrinding) { GetBasedOnVelocity(); break; }

					//Find the point on the spline the player will be in x frames
					float playerPointOnSpline = PlayerRail._RF._pointOnSpline;
					float targetPointOnSpline = playerPointOnSpline + (PlayerRail._RF._grindingSpeed + PlayerRail._RF._movingDirection * Time.fixedDeltaTime * xFrames) ;

					if (targetPointOnSpline >= PlayerRail._RF._PathSpline.Length) { GetBasedOnVelocity(); break; } //If this exceeds the current rail, aim normally.
																       //If valid, get it as a point in space to aim at.
					CurveSample TargetSampleOnSpline = PlayerRail._RF._PathSpline.GetSampleAtDistance(targetPointOnSpline);
					Vector3 targetPositionOnSpline = Spline.GetSampleTransformInfo( PlayerRail._RF._RailTransform, TargetSampleOnSpline).location;
					targetPositionOnSpline += PlayerRail._RF._currentLocalOffset;
					targetPositionOnSpline += PlayerRail._RF._currentCenterOffset;
					targetPositionOnSpline += PlayerRail._PlayerPhys._colliderOffsetFromPivot;

					targetPosition = targetPositionOnSpline;

					break;
				default:
					GetBasedOnVelocity(); break;
			}
		}

		Debug.DrawLine(_Target.position, targetPosition, Color.red, 20f);
		Debug.DrawRay(_Target.position, _TargetVel._worldVelocity * 3, Color.yellow, 20f);
		Debug.DrawRay(targetPosition, Vector3.up * 10, Color.yellow, 20f);

		return targetPosition;

		//If player is moving, aim where they'll be in x frames if they dont change their velocity.
		void GetBasedOnVelocity () {
			targetPosition += (_TargetVel._worldVelocity * Time.fixedDeltaTime * xFrames);
		}
	}

	#endregion
}
