using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S_Control_SoundsPlayer : S_Player_Base
{

	[Header("Audio Sources")]
	[ColourIfNull(0.8f,0.6f,0.6f,1f)]public AudioSource  FeetSource;
	[ColourIfNull(0.8f,0.6f,0.6f,1f)]public AudioSource  GrindSource;
	[ColourIfNull(0.8f,0.6f,0.6f,1f)]public AudioSource GeneralSource;
	[ColourIfNull(0.8f,0.6f,0.6f,1f)]public AudioSource ExtraSource;
	[ColourIfNull(0.8f,0.6f,0.6f,1f)]public AudioSource DamageSource;
	[ColourIfNull(0.8f,0.6f,0.6f,1f)]public AudioSource VoiceSource;
	[ColourIfNull(0.8f,0.6f,0.6f,1f)]public AudioSource BoostSource1;
	[ColourIfNull(0.8f,0.6f,0.6f,1f)]public AudioSource SpinDashSource;
	[ColourIfNull(0.8f,0.6f,0.6f,1f)]public AudioSource WindSource;
	private float _startWindSourceVolume;
	private bool _windActive;


	[Header("Clips")]
	public AudioClip[] FootSteps;
	[Header("Actions")]
	[Header("Air")]
	public AudioClip Clip_Jump;
	public AudioClip Clip_HomingAttack;
	public AudioClip Clip_PerfectHomingAttack;
	public AudioClip Clip_JumpDash;
	public GameObject Ob_BounceStart;
	public AudioClip Clip_BounceImpact;
	public AudioClip Clip_StompImpact;

	[Header("Grounded")]
	public AudioClip Clip_SpinDashLoop;
	public AudioClip Clip_SpinDashRelease;
	public GameObject Ob_SpinDashStart;

	[Header("Sub Actions")]
	public AudioClip Clip_Skid;
	public AudioClip Clip_Roll;
	public AudioClip Clip_Quickstep;
	public GameObject Ob_BoostStart;
	public AudioClip Clip_BoostLoop;

	[Header("Other Actions")]
	public AudioClip Clip_RingDash;
	public GameObject Ob_RailLand;
	public AudioClip Clip_RailGrind;

	[Header("Damage")]
	public AudioClip Clip_RingLoss;
	public AudioClip Clip_DIe;
	public AudioClip Clip_Spiked;
	public AudioClip Clip_Bonk;
	public AudioClip Clip_HitByDanger;

	[Header("Additional SFX")]
	public GameObject _LevelUpSound;

	[Header("Voice")]
	public AudioClip[] CombatVoiceClips;
	public AudioClip[] JumpingVoiceClips;
	public AudioClip[] PainVoiceClips;

	public float pitchBendingRate = 1;


	public override void Awake () {
		base.Awake();
		_startWindSourceVolume = WindSource.volume;
		_windActive = _PlayerVel._speedMagnitudeSquared > 70 * 70;

		if (!_windActive)
		{
			WindSource.volume = 0;
		}

	}

	#region update

	private void Update () {
		HandleWindSound();
	}

	private void HandleWindSound () {
		if(_PlayerVel._speedMagnitudeSquared > 50 * 50)
		{
			if (!WindSource.isPlaying)
				WindSource.Play();

			//Lerp to max wind volume based on how close to maximum speed laterally or vertically.
			float lerpAmount = (_PlayerVel._horizontalSpeedMagnitude) / _PlayerMovement._currentMaxSpeed;
			lerpAmount = Mathf.Max(lerpAmount, (Mathf.Abs(_PlayerPhys._RB.velocity.y) + 50) / _PlayerPhys._maxFallingSpeed_);

			WindSource.volume = Mathf.Lerp(0, _startWindSourceVolume, lerpAmount);
			Debug.Log(lerpAmount + " For volume of " + WindSource.volume);
		}
		else
			WindSource.volume = Mathf.Lerp(WindSource.volume, 0, 0.2f);
	}

	private void HandleWindSoundOld () {
		if (_PlayerVel._speedMagnitudeSquared > 70 * 70)
		{
			if (_windActive) { return; }
			_windActive = true;
			StopCoroutine(S_S_Objects.LerpAudioSourceVolume(WindSource, 2, 0));
			StartCoroutine(S_S_Objects.LerpAudioSourceVolume(WindSource, 2, _startWindSourceVolume));

			if (!WindSource.isPlaying)
				WindSource.Play();
		}
		else
		{
			if (!_windActive) { return; }
			_windActive = false;
			StopCoroutine(S_S_Objects.LerpAudioSourceVolume(WindSource, 2, _startWindSourceVolume));
			StartCoroutine(S_S_Objects.LerpAudioSourceVolume(WindSource, 1, 0));
		}
	}

	#endregion


	#region VoiceSource

	public void CombatVoicePlay () {
		int rand = Random.Range(0, CombatVoiceClips.Length);
		VoiceSource.clip = CombatVoiceClips[rand];
		VoiceSource.Play();
	}
	public void JumpingVoicePlay () {
		int rand = Random.Range(0, JumpingVoiceClips.Length);
		VoiceSource.clip = JumpingVoiceClips[rand];
		VoiceSource.Play();
	}
	public void PainVoicePlay () {
		int rand = Random.Range(0, PainVoiceClips.Length);
		VoiceSource.clip = PainVoiceClips[rand];
		VoiceSource.Play();
	}
	#endregion

	#region Feet&GrindSource

	//This is called by an Animation event in specific animations. Any walking/running animation will call this.
	//Make sure the animator component is on the same object as this script. And that this method isn't renamed.
	public void FootStepSoundPlay () {
		//if (FootSteps.Length > 0 && !FeetSource.isPlaying)
		if (FootSteps.Length > 0)
		{
			int rand = Random.Range (0, FootSteps.Length);
			FeetSource.clip = FootSteps[rand];
			FeetSource.Play();
		}
	}

	public void RailGrindSound ( bool overwrite = false ) {
		if (GrindSource.isPlaying && !overwrite) { return; }
		FeetSource.Stop();
		GrindSource.clip = Clip_RailGrind;
		GrindSource.Play();
	}

	public void RailGrindStop () {
		GrindSource.Stop();
	}
	#endregion

	#region GeneralSource

	public void JumpSound () {
		if (JumpingVoiceClips.Length > 0)
		{
			JumpingVoicePlay();
		}
		GeneralSource.clip = Clip_Jump;
		GeneralSource.Play();
	}
	public void SkiddingSound () {
		GeneralSource.clip = Clip_Skid;
		GeneralSource.Play();
	}
	public void HomingAttackSound ( bool perfect ) {
		GeneralSource.clip = perfect ? Clip_PerfectHomingAttack : Clip_HomingAttack;
		GeneralSource.Play();
		if (CombatVoiceClips.Length > 0)
		{
			CombatVoicePlay();
		}
	}

	public void JumpDashSound () {
		GeneralSource.clip = Clip_JumpDash;
		GeneralSource.loop = false;
		GeneralSource.Play();
	}
	public void LightSpeedDashSound () {
		GeneralSource.clip = Clip_RingDash;
		GeneralSource.loop = false;
		GeneralSource.Play();
	}

	public void BounceImpactSound () {
		GeneralSource.clip = Clip_BounceImpact;
		GeneralSource.loop = false;
		GeneralSource.Play();
	}
	public void StompImpactSound () {
		GeneralSource.clip = Clip_StompImpact;
		GeneralSource.loop = false;
		GeneralSource.Play();
	}

	public void QuickStepSound () {
		GeneralSource.clip = Clip_Quickstep;
		GeneralSource.loop = false;
		GeneralSource.Play();
	}
	#endregion

	#region DamageSource
	public void RingLossSound () {
		DamageSource.clip = Clip_RingLoss;
		DamageSource.Play();
	}
	public void HitSound () {
		DamageSource.clip = Clip_HitByDanger;
		DamageSource.Play();
	}
	public void BonkSound () {
		DamageSource.clip = Clip_Bonk;
		DamageSource.Play();
	}
	public void DieSound () {
		DamageSource.clip = Clip_DIe;
		if (PainVoiceClips.Length > 0)
		{
			PainVoicePlay();
		}
		DamageSource.Play();
	}
	public void SpikedSound () {
		DamageSource.clip = Clip_Spiked;
		DamageSource.Play();
	}
	#endregion

	#region extraSource

	#endregion

	#region specificSources

	public void BoostSound () {
		BoostSource1.clip = Clip_BoostLoop;
		BoostSource1.loop = true;
		BoostSource1.Play();

		StartBoostSound();

	}
	public void EndBoostSound () {
		BoostSource1.Stop();
	}

	public void StartRollingSound () {
		//Wont play the sound if spin charge release is currently audible, as they conflict.
		if (!(GeneralSource.clip == Clip_SpinDashRelease && GeneralSource.isPlaying))
		{
			BoostSource1.clip = Clip_Roll;
			BoostSource1.loop = false;
			BoostSource1.Play();
		}
	}

	public void SpinDashSound () {
		SpinDashSource.clip = Clip_SpinDashLoop;
		SpinDashSource.loop = true;
		SpinDashSource.Play();
	}

	public void SpinDashReleaseSound () {
		SpinDashSource.clip = Clip_SpinDashRelease;
		SpinDashSource.loop = false;
		SpinDashSource.Play();
	}

	#endregion

	#region SpawningSFXObjects

	public void LevelUpSound () {
		Instantiate(_LevelUpSound, transform.position, transform.rotation);
	}

	public void SpinDashRev () {
		Instantiate(Ob_SpinDashStart, transform.position, transform.rotation);
	}

	public void StartBoostSound () {
		Instantiate(Ob_BoostStart, transform.position, Quaternion.identity);
	}

	public void BounceStartSound () {
		Instantiate(Ob_BounceStart, transform.position, transform.rotation);
	}

	public void RailLandSound () {
		Instantiate(Ob_RailLand, transform.position, transform.rotation);
	}

	#endregion

}
