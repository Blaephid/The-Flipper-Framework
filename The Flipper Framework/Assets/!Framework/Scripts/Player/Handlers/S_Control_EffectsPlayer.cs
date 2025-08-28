using UnityEngine;
using System.Collections;
using UnityEngine.VFX;
using System;

public class S_Control_EffectsPlayer : S_Player_Base
{


	Camera MainCamera;

	[Header("VFX to Trigger")]

	[SerializeField, ColourIfNull(0.8f,0,0,1)] ParticleSystem RunningDust;
	[SerializeField, ColourIfNull(0.8f,0,0,1)] VisualEffect _SpeedLinesWind;
	[SerializeField, ColourIfNull(0.8f,0,0,1)] VisualEffect _SpeedLinesCharacter;

	[SerializeField, ColourIfNull(0.8f, 0, 0, 1)] VisualEffect _ActionChainEffect;

	[SerializeField, ColourIfNull(0.8f, 0, 0, 1)] ParticleSystem SpinDashEnergy;
	[SerializeField, ColourIfNull(0.8f, 0, 0, 1)] ParticleSystem RailsSparks1;

	[Header("Screen VFX To Trigger")]
	[SerializeField, ColourIfNull(0.8f,0,0,1)] VisualEffect             _SpeedLinesScreen;
	[SerializeField, ColourIfNull(0.8f, 0, 0, 1)] VisualEffect             _BlurBurst;
	[SerializeField, ColourIfNull(0.8f, 0, 0, 1)] VisualEffect             _PointsGain;

	[Header("Trails")]
	[SerializeField, ColourIfNull(0.8f, 0, 0, 1)] GameObject  _LesserTrails;
	[SerializeField, ColourIfNull(0.8f, 0, 0, 1)] TrailRenderer   _DefaultSpeedTrail;
	[SerializeField, ColourIfNull(0.8f, 0, 0, 1)] S_VolumeTrailRenderer  _LargeSpeedTrail;

	[Header("VFX to Spawn")]
	[SerializeField, ColourIfNull(0.8f, 0, 0, 1)] GameObject  _JumpDashParticle;
	[SerializeField, ColourIfNull(0.8f, 0, 0, 1)] GameObject  _BonkParticle;

	[Header("Mouth Sides")]
	Transform _Head;
	Transform LeftMouth, RightMouth;

	[Header("Values")]
	public float RunningDustThreshold;
	public Vector2 _speedLinesThreshold;

	//Trackers
	private bool _canShowLesserTrails = true;
	private bool _canShowDefaultTrail = true;
	[NonSerialized] public float _largeTrailEmitTime;

	private void Start () {
		SpinDashEnergy.Stop();
		RailsSparks1.Stop();
		_BlurBurst.gameObject.SetActive(false);
		_SpeedLinesWind.Stop();
		_SpeedLinesCharacter.Stop();
		_ActionChainEffect.Stop();
		_PointsGain.Stop();
	}

	void Update () {

		HandleMouths();

		HandleSpeedLinesOnCharacter();
		HandleSpeedLinesOnScreen();
		HandleTrailsOnCharacter();
	}

	//VFX
	#region VFX

	//Private

	private void HandleSpeedLinesOnCharacter () {

		CheckIntensity(_SpeedLinesWind, _speedLinesThreshold.x);
		CheckIntensity(_SpeedLinesCharacter, _speedLinesThreshold.y);
		return;

		void CheckIntensity (VisualEffect lines, float threshold) {
			if (_PlayerVel._currentRunningSpeed > threshold)
			{
				lines.Play();

				float intensity = (_PlayerVel._horizontalSpeedMagnitude - (threshold *0.3f)) / _PlayerMovement._currentMaxSpeed;
				lines.SetFloat("Intensity", intensity);
			}
			else if (_PlayerVel._currentRunningSpeed < threshold - 5)
			{
				lines.Stop();
			}
		}

	}

	//Controls and activates the anime style speedlines on the screen edges based on speed.
	private void HandleSpeedLinesOnScreen () {
		if (_PlayerVel._horizontalSpeedMagnitude > 50)
		{
			float intensity = Mathf.Min(_PlayerVel._horizontalSpeedMagnitude / _PlayerPhys._PlayerMovement._currentMaxSpeed , 1.1f);
			intensity = Mathf.Max(Mathf.Abs(intensity - Mathf.Lerp(intensity, 1.1f, 0.5f)) - intensity, intensity - Mathf.Abs(intensity - Mathf.Lerp(intensity, 1.1f, 0.5f)));
			_SpeedLinesScreen.SetFloat("Intensity", intensity);
		}
		else
		{
			_SpeedLinesScreen.SetFloat("Intensity", 0);
		}
	}

	private void HandleTrailsOnCharacter () {

		DefaultTrail();
		LesserTrails();
		return;

		void DefaultTrail () {
			if (_canShowDefaultTrail)
			{
				if (_PlayerVel._horizontalSpeedMagnitude > 60)
				{
					_DefaultSpeedTrail.emitting = true;
					return;
				}
			}
			_DefaultSpeedTrail.emitting = false;
		}

		void LesserTrails () {
			if (!_canShowLesserTrails) { return; }
			EnableLesserTrails(_PlayerVel._speedMagnitudeSquared > 40 * 40 && _PlayerVel._horizontalSpeedMagnitude > 10, false);
		}
	}


	//Public
	public void EnableLesserTrails ( bool enable, bool locked ) {
		if (_LesserTrails.activeSelf != enable)
			_LesserTrails.SetActive(enable);

		_canShowLesserTrails = !locked;
	}

	public void EnableLargeTrail ( float time, bool special = false ) {
		_LargeSpeedTrail.StartEmit(time, DisableLargeTrail, special);

		_DefaultSpeedTrail.emitting = false;
		_canShowDefaultTrail = false;
	}

	public void DisableLargeTrail () {
		_canShowDefaultTrail = true;
	}

	public void HandleSpinDashEffect ( int amm, float speed, float currentCharge, float maxCharge ) {

		float chargeUsedForEffect = Mathf.Min( currentCharge * 0.15f, 55);


		//Activate spin dash energy effect
		if (!SpinDashEnergy.isPlaying)
		{
			SpinDashEnergy.gameObject.SetActive(true);
			SpinDashEnergy.Play();
		}

		var emission = SpinDashEnergy.emission;
		emission.rateOverTime = chargeUsedForEffect;

		ParticleSystem.MainModule Main = SpinDashEnergy.main;

		//Once fully charged, dim effect slightly.
		if (currentCharge > maxCharge - 0.3f)
			Main.startColor = new Color(0.6f, 0.6f, 0.6f, 1);
		else
			Main.startColor = new Color(1f, 1f, 1f, 1);
	}

	//Disable spin dash energy effect
	public void EndSpinDashEffect () {
		SpinDashEnergy.Stop();
		var emission = SpinDashEnergy.emission;
		emission.rateOverTime = 0f;
		SpinDashEnergy.gameObject.SetActive(false);
	}

	public void HandleGrindSparks ( float speed ) {

		//Activate or deactivate effect
		if (speed > 30 && !RailsSparks1.isPlaying)
			RailsSparks1.Play();

		else if (speed <= 30 && RailsSparks1.isPlaying)
		{
			RailsSparks1.Stop(); return;
		}

		ParticleSystem.MainModule Main = RailsSparks1.main;

		Main.startSpeed = Mathf.Clamp(speed * 0.3f, 20, 60);

	}

	public void ActionChainAdd () {
		_ActionChainEffect.Stop();
		_ActionChainEffect.Play();
	}

	public void PointsGain(float amount ) {

		amount /= 15;
		amount += 1;
		amount = Mathf.Min(amount, 2);
		_PointsGain.SetFloat("Intensity", amount);
		_PointsGain.Play();
	}


	//Spawn

	//Spawn an instance of the air dash particle, which is not locked onto the character.
	public void SpawnAirDashParticle ( Transform characterReferencePoint ) {
		GameObject JumpDashParticleClone = Instantiate(_JumpDashParticle, characterReferencePoint.position, characterReferencePoint.rotation);

		//Affect by player speed.
		if (_PlayerVel._speedMagnitudeSquared > Mathf.Pow(100, 2))
		{
			float scale = _PlayerVel._speedMagnitudeSquared / Mathf.Pow(100,2);
			JumpDashParticleClone.transform.localScale = Vector3.one * scale;
		}
	}

	public void SpawnBonkParticle ( Vector3 position, Vector3 normal ) {
		GameObject BonkParticleClone = Instantiate(_BonkParticle, position, Quaternion.LookRotation(normal)) ;
	}


	//Trigger Screen

	public void TriggerBlurBurstScreen () {
		Vector2 screenPosition = MainCamera.WorldToViewportPoint(_PlayerPhys._CharacterCenterPosition);

		_BlurBurst.gameObject.SetActive(false);
		_BlurBurst.gameObject.SetActive(true);
		_BlurBurst.SetVector3("Screen Position", screenPosition);
	}

	#endregion

	//Not a VFX, but ensures which of the characters two possible mouths is used based on camera angle. Gives the Side Mouth effect.
	private void HandleMouths () {
		Vector3 direction = _CamHandler._HedgeCam.transform.position - _Head.position;
		bool _isFacingRightSide = Vector3.Dot(_Head.forward, direction.normalized) < 0f;

		LeftMouth.localScale = _isFacingRightSide ? Vector3.zero : Vector3.one;
		RightMouth.localScale = !_isFacingRightSide ? Vector3.zero : Vector3.one;
	}

	public override void AssignTools () {
		base.AssignTools();

		_Head = _Tools.Head;
		LeftMouth = _Tools.LeftMouth;
		RightMouth = _Tools.RightMouth;

		MainCamera = Camera.main;
	}

}
