using SplineMesh;
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
		if (_isJumping)
		{
			_framesSinceJump++;
 			if (_framesSinceJump >= _framesAfterJumpToStartSearchingForNewRail) 
  			{ LookForNewRail(); }
		}
		else if (_isShooting)
		{
			_timeSpentReadyingShot += Time.fixedDeltaTime;

			if (_timeSpentReadyingShot >= _timeToReadyShot)
			{
				Shoot();
			}
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
		Vector3 JumpForce = transform.up * 50;
		_RailBehaviour._RB.velocity += JumpForce;

		_isJumping = true;
		_isShooting = false;
		_isSwitching = false;

		_CustomGravity._isGravityOn = true;

		SetOnRail(false);
		_framesSinceJump = 0;
	}

	#endregion

	#region Switching

	public bool CanSwitch ( float distanceBetweenRails) {

		if (_isShooting || _isSwitching || _isJumping) { return false; }

		float currentSpeed = _RailBehaviour._RF._grindingSpeed;
		float distanceNeeded = currentSpeed * Time.fixedDeltaTime * 80;

		//Cant switch if too close to ending current rail.
		if (!_RailBehaviour._RF._isGoingBackwards && _RailBehaviour._RF._PathSpline.Length - _RailBehaviour._RF._pointOnSpline < distanceNeeded) return false;
		else if(_RailBehaviour._RF._isGoingBackwards && _RailBehaviour._RF._pointOnSpline < distanceNeeded) { return false; }

		float firstDirection =  Random.value < 0.5f ? -1 : 1;

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

		Debug.DrawRay(AboveRail, transform.up * -15, Color.cyan, 10);
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

		Debug.DrawRay(transform.position, transform.right * 2, Color.magenta, 10f);
		Debug.DrawRay(transform.position, -transform.right * 2, Color.magenta, 10f);
		Debug.DrawRay(transform.position, transform.forward * 2, Color.magenta, 10f);
		Debug.DrawRay(transform.position, -transform.forward * 2, Color.magenta, 10f);

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

		if(S_S_MoreMaths.GetDistanceSqrOfVectors(_Target.position, _ShootPoint_.position) < 20*20) { return false; } //Can't shoot if player is too close.
		if(Vector3.Angle(S_S_MoreMaths.GetDirection( _ShootPoint_.position, _Target.position), transform.forward) < 90) { return false; } //Cant shoot if player is in front.
		if(Physics.Linecast(_Target.position, _ShootPoint_.position)) { return false; } //Cant shoot if solid object or other enemy blocking the way.

		_isShooting = true;
		_ReadyShotParticle_.Play();
		_timeSpentReadyingShot = 0;

		return true;
	}

	private void Shoot () {
		//Create projectile at point
		GameObject GO = Instantiate(_Projectile_);
		GO.transform.position = _ShootPoint_.position;

		//Calculate target
		Vector3 targetPosition = GetPlayerPositionInXFrames(_framesToInterceptPlayer_);

		//Calculate how fast to make projectile travel to intercept player.
		float distanceToTravel = Vector3.Distance(targetPosition, _ShootPoint_.position);
		float timeToTravel = Time.fixedDeltaTime * _framesToInterceptPlayer_;
		float neededSpeed = distanceToTravel / timeToTravel;
		neededSpeed = Mathf.Clamp(neededSpeed, _minMaxShotSpeeds.x, _minMaxShotSpeeds.y);

		//Get and apply velocity
		Vector3 targetDirection = (targetPosition - GO.transform.position).normalized;
		Vector3 velocity = targetDirection * neededSpeed;

		GO.transform.forward = targetDirection;
		GO.GetComponent<Rigidbody>().velocity = velocity;

		_isShooting = false;

		_RailBehaviour.SetAnimatorTrigger("Attack");
		_ReadyShotParticle_.Stop();
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
					float targetPointOnSpline = playerPointOnSpline + (PlayerRail._RF._grindingSpeed * Time.fixedDeltaTime * xFrames) ;

					if (targetPointOnSpline >= PlayerRail._RF._PathSpline.Length) { GetBasedOnVelocity(); break; } //If this exceeds the current rail, aim normally.
																       //If valid, get it as a point in space to aim at.
					CurveSample TargetSampleOnSpline = PlayerRail._RF._PathSpline.GetSampleAtDistance(targetPointOnSpline);
					Vector3 targetPositionOnSpline = Spline.GetSampleTransformInfo( PlayerRail._RF._RailTransform, TargetSampleOnSpline).location;
					targetPositionOnSpline += PlayerRail._RF._currentLocalOffset;

					targetPosition = targetPositionOnSpline;

					break;
				default:
					GetBasedOnVelocity(); break;
			}
		}

		return targetPosition;

		//If player is moving, aim where they'll be in x frames if they dont change their velocity.
		void GetBasedOnVelocity () {
			targetPosition += (_TargetVel._worldVelocity * Time.fixedDeltaTime * xFrames);
		}
	}

	#endregion
}
