using UnityEngine;
using System.Collections;

public class S_Control_EffectsPlayer : MonoBehaviour
{

	private S_PlayerPhysics _PlayerPhys;
	public S_CharacterTools _Tools;
	private S_Handler_Camera _CamHandler;

	public ParticleSystem RunningDust;
	public ParticleSystem SpeedLines;
	public ParticleSystem SpinDashDust;
	public ParticleSystem SpinDashEnergy;
	public float RunningDustThreshold;
	public float SpeedLinesThreshold;

	private GameObject  _HomingTrailContainer;
	private GameObject  _JumpDashParticle;

	[Header("Rails")]
	[SerializeField]
	ParticleSystem RailsSparks1;
	[SerializeField]
	ParticleSystem RailsSparks2;

	[Header("Mouth Sides")]
	[SerializeField]
	Transform _Head;
	[SerializeField]
	Transform LeftMouth, RightMouth;
	[SerializeField]
	Transform[] MouthsToHide;
	[SerializeField]
	Transform Eyelids;

	private void Start () {
		_HomingTrailContainer = _Tools.HomingTrailContainer;
		_JumpDashParticle = _Tools.JumpDashParticle;
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_CamHandler = _Tools.CamHandler;
	}

	void FixedUpdate () {

		if (_PlayerPhys._RB.velocity.sqrMagnitude > RunningDustThreshold && _PlayerPhys._isGrounded && RunningDust != null)
		{
			RunningDust.Emit(Random.Range(0, 20));
		}

		if (_PlayerPhys._RB.velocity.sqrMagnitude > SpeedLinesThreshold && _PlayerPhys._isGrounded && SpeedLines != null && SpeedLines.isPlaying == false)
		{
			SpeedLines.Play();
		}
		else if (_PlayerPhys._RB.velocity.sqrMagnitude < SpeedLinesThreshold && SpeedLines.isPlaying == true || (!_PlayerPhys._isGrounded))
		{
			SpeedLines.Stop();
		}

		HandleMouths();

	}

	private void HandleMouths () {
		Vector3 direction = _CamHandler._HedgeCam.transform.position - _Head.position;
		bool _isFacingRightSide = Vector3.Dot(_Head.forward, direction.normalized) < 0f;

		LeftMouth.localScale = _isFacingRightSide ? Vector3.zero : Vector3.one;
		RightMouth.localScale = !_isFacingRightSide ? Vector3.zero : Vector3.one;
		

	}

	public ParticleSystem GetSpinDashDust () {
		return SpinDashDust;
	}

	public void DoSpindash ( int amm, float speed, float charge, ParticleSystem spinDashDust, float maxCharge ) {

		float energyCharge = charge * 0.15f;
		if (energyCharge > 55f)
			energyCharge = 55f;

		ParticleSystem.MainModule ma = spinDashDust.main;
		ma.startSpeed = speed;
		SpinDashDust.Emit(amm);

		if (!SpinDashEnergy.isPlaying)
		{
			SpinDashEnergy.gameObject.SetActive(true);
			SpinDashEnergy.Play();
			charge = 0;
		}
		var emission = SpinDashEnergy.emission;
		emission.rateOverTime = energyCharge;

		ma = SpinDashEnergy.main;

		if (charge > maxCharge - 0.3f)
			ma.startColor = new Color(0.2f, 0.13f, 0.13f, 1);
		else
			ma.startColor = new Color(1f, 1f, 1f, 1);
	}

	public void EndSpinDash () {
		SpinDashEnergy.Stop();
		var emission = SpinDashEnergy.emission;
		emission.rateOverTime = 0f;
		SpinDashEnergy.gameObject.SetActive(false);
	}


	public void AirDashParticle () {
		GameObject JumpDashParticleClone = Instantiate(_JumpDashParticle, _Tools.transform.position, Quaternion.identity) as GameObject;

		if (_PlayerPhys._speedMagnitude > 60)
			JumpDashParticleClone.transform.localScale = new Vector3(_PlayerPhys._speedMagnitude / 60f, _PlayerPhys._speedMagnitude / 60f, _PlayerPhys._speedMagnitude / 60f);

		JumpDashParticleClone.transform.position = _Tools.transform.position;
		JumpDashParticleClone.transform.rotation = _Tools.MainSkin.transform.rotation;
	}

}
