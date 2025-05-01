using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	const float _timeToReadyShot = 0.5f;
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
		_Target = Player;
		_TargetVel = PlayerVel;

		_timeSpentReadyingShot = 0;
	}

	private void Shoot () {
		GameObject Go = Instantiate(_Projectile_);
		Go.transform.position = _ShootPoint_.position;

		Vector3 targetPosition = _Target.position;
		if (_TargetVel)
		{
			targetPosition += (_TargetVel._worldVelocity * Time.fixedDeltaTime * 8); //If player is moving, aim where they'll be in x frames if they dont stop.
		}

		Vector3 targetDirection = (targetPosition - Go.transform.position).normalized;
		Vector3 velocity = targetDirection * _shotSpeed_;
	}


}
