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

	private S_AI_RailEnemy _RailBehaviour;

	private bool _isSwitching;
	private bool _isShooting;

	private Transform _Target;
	private S_PlayerVelocity _TargetVel;
	private S_ActionManager _TargetActions;

	[Header("Attacks")]
	[Tooltip("How many frames after the shot until the cannonball hits the player. This effects shot speeds but calculates where the player will be in x frames, and sets speed to reach that.")]
	[SerializeField] private float _framesToInterceptPlayer_ = 30;
	[SerializeField] public float  _timeToReadyShot = 0.6f;
	[SerializeField] private Vector2 _minMaxShotSpeeds = new Vector2(60, 120);

	[Header("Jumping")]

	float _timeSpentReadyingShot;

	private void Start () {
		_RailBehaviour = GetComponent<S_AI_RailEnemy>();
	}

	private void Update () {
		if (_isShooting)
		{
			_timeSpentReadyingShot += Time.fixedDeltaTime;

			if (_timeSpentReadyingShot >= _timeToReadyShot)
			{
				Shoot();
			}
		}
	}

	public void RailSwitch (float distance) {
		_isSwitching = true;
		_RailBehaviour.SetAnimatorBool("CurrentlyOnRail", false);
		_RailBehaviour.SetAnimatorTrigger("Twirl");
	}

	#region Jumping

	public void Jump () {

	}

	#endregion

	#region Attacking

	public void ReadyShot (Transform Player, S_PlayerVelocity PlayerVel) {
		_isShooting = true;

		if (!_Target)
		{
			_Target = Player;
			_TargetVel = PlayerVel;
			_TargetActions = PlayerVel.GetComponent<S_CharacterTools>()._ActionManager;
		}

		_ReadyShotParticle_.Play();
		_timeSpentReadyingShot = 0;
	}

	private void Shoot () {
		//Create projectile at point
		GameObject GO = Instantiate(_Projectile_);
		GO.transform.position = _ShootPoint_.position;

		//Calculate target
		Vector3 targetPosition = GetPlayerPositionInXFrames(_framesToInterceptPlayer_);

		Debug.DrawRay(targetPosition, Vector3.up * 20, Color.magenta, 10f);


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

		Debug.DrawRay(_ShootPoint_.position, velocity, Color.yellow, 10f);
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
