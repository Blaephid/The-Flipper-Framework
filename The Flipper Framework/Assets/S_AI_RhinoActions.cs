using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SplineMesh.Spline;

[RequireComponent(typeof(S_AI_RailEnemy))]
public class S_AI_RhinoActions : MonoBehaviour
{
	[SerializeField, ColourIfNull(1f, 0.5f, 0.5f, 1f)] GameObject _Projectile_;
	[SerializeField, ColourIfNull(1f, 0.5f, 0.5f, 1f)] Transform _ShootPoint_;

	private S_AI_RailEnemy _RailBehaviour;

	private bool _isSwitching;
	private bool _isShooting;

	private Transform _Target;
	private S_PlayerVelocity _TargetVel;
	private S_ActionManager _TargetActions;

	const float _timeToReadyShot = 0.6f;
	float _timeSpentReadyingShot;
	[SerializeField] private float _shotSpeed_;

	private void Start () {
		_RailBehaviour = GetComponent<S_AI_RailEnemy>();
	}

	private void Update () {
		if (_isShooting)
		{
			_timeSpentReadyingShot += Time.fixedDeltaTime;
			Debug.DrawLine(transform.position, _Target.position, Color.red, 10f);

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

	public void ReadyShot (Transform Player, S_PlayerVelocity PlayerVel) {
		_isShooting = true;

		if (!_Target)
		{
			_Target = Player;
			_TargetVel = PlayerVel;
			_TargetActions = PlayerVel.GetComponent<S_CharacterTools>()._ActionManager;
		}

		_timeSpentReadyingShot = 0;
	}

	private void Shoot () {
		//Create projectile at point
		GameObject GO = Instantiate(_Projectile_);
		GO.transform.position = _ShootPoint_.position;

		const float framesToInterceptPlayer = 30;

		//Calculate target
		Vector3 targetPosition = GetPlayerPositionInXFrames(framesToInterceptPlayer);

		Debug.DrawRay(targetPosition, Vector3.up * 20, Color.magenta, 10f);


		//Calculate how fast to make projectile travel to intercept player.
		float distanceToTravel = Vector3.Distance(targetPosition, _ShootPoint_.position);
		float timeToTravel = Time.fixedDeltaTime * framesToInterceptPlayer;
		float neededSpeed = distanceToTravel / timeToTravel;

		//Get and apply velocity
		Vector3 targetDirection = (targetPosition - GO.transform.position).normalized;
		Vector3 velocity = targetDirection * neededSpeed;

		GO.transform.forward = targetDirection;
		GO.GetComponent<Rigidbody>().velocity = velocity;

		Debug.DrawRay(_ShootPoint_.position, velocity, Color.yellow, 10f);
		_isShooting = false;

		_RailBehaviour.SetAnimatorTrigger("Attack");
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
}
