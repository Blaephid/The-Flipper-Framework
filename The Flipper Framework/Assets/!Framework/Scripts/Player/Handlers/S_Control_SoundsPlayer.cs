using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S_Control_SoundsPlayer : MonoBehaviour
{
	[Header("Tools")]
	public S_PlayerPhysics Player;

	[Header("Audio Sources")]
	public AudioSource  FeetSource;
	public AudioSource GeneralSource;
	public AudioSource ExtraSource;
	public AudioSource DamageSource;
	public AudioSource VoiceSource;
	public AudioSource BoostSource1, BoostSource2;


	[Header("Clips")]
	public AudioClip[] FootSteps;
	[Header("Actions")]
	[Header("Air")]
	public AudioClip Jumping;
	public AudioClip HomingAttack;
	public AudioClip JumpDash;
	public AudioClip BounceStart;
	public AudioClip BounceImpact;
	public AudioClip StompImpact;

	[Header("Grounded")]
	public AudioClip SpinDash;
	public AudioClip SpinDashRelease;

	[Header("Sub Actions")]
	public AudioClip Skidding;
	public AudioClip Spin;
	public AudioClip QuickStep;
	public AudioClip BoostStart;
	public AudioClip BoostRepeat;

	[Header("Other Actions")]
	public AudioClip LightSpeedDash;
	public AudioClip RailLand;
	public AudioClip RailGrind;

	[Header("Damage")]
	public AudioClip RingLoss;
	public AudioClip Die;
	public AudioClip Spiked;
	public AudioClip Bonk;
	public AudioClip HitByDanger;

	[Header("Voice")]
	public AudioClip[] CombatVoiceClips;
	public AudioClip[] JumpingVoiceClips;
	public AudioClip[] PainVoiceClips;

	public float pitchBendingRate = 1;


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

	#region FeetSource

	//This is called by an Animation event in specific animations. Any walking/running animation will call this.
	//Make sure the animator component is on the same object as this script. And that this method isn't renamed.
	public void FootStepSoundPlay () {
		//if (FootSteps.Length > 0 && !FeetSource.isPlaying)
		if (FootSteps.Length > 0 )
		{
			int rand = Random.Range (0, FootSteps.Length);
			FeetSource.clip = FootSteps[rand];
			FeetSource.Play();
		}
	}

	public void RailGrindSound () {
		if (FeetSource.isPlaying) { return; }
		FeetSource.clip = RailGrind;
		FeetSource.Play();
	}
	#endregion

	#region GeneralSource

	public void JumpSound () {
		if (JumpingVoiceClips.Length > 0)
		{
			JumpingVoicePlay();
		}
		GeneralSource.clip = Jumping;
		GeneralSource.Play();
	}
	public void SkiddingSound () {
		GeneralSource.clip = Skidding;
		GeneralSource.Play();
	}
	public void HomingAttackSound () {
		GeneralSource.clip = HomingAttack;
		GeneralSource.Play();
		if (CombatVoiceClips.Length > 0)
		{
			CombatVoicePlay();
		}
	}

	public void JumpDashSound () {
		GeneralSource.clip = JumpDash;
		GeneralSource.Play();
	}
	public void LightSpeedDashSound () {
		GeneralSource.clip = LightSpeedDash;
		GeneralSource.Play();
	}

	public void SpinDashSound () {
		GeneralSource.clip = SpinDash;
		GeneralSource.Play();
	}
	public void BounceStartSound () {
		GeneralSource.clip = BounceStart;
		GeneralSource.Play();
	}
	public void BounceImpactSound () {
		GeneralSource.clip = BounceImpact;
		GeneralSource.Play();
	}
	public void StompImpactSound () {
		GeneralSource.clip = StompImpact;
		GeneralSource.Play();
	}
	public void SpinDashReleaseSound () {
		GeneralSource.clip = SpinDashRelease;
		GeneralSource.Play();
	}
	public void QuickStepSound () {
		GeneralSource.clip = QuickStep;
		GeneralSource.Play();
	}
	#endregion

	#region DamageSource
	public void RingLossSound () {
		DamageSource.clip = RingLoss;
		DamageSource.Play();
	}
	public void HitSound () {
		DamageSource.clip = HitByDanger;
		DamageSource.Play ();
	}
	public void BonkSound () {
		DamageSource.clip = Bonk;
		DamageSource.Play();
	}
	public void DieSound () {
		DamageSource.clip = Die;
		if (PainVoiceClips.Length > 0)
		{
			PainVoicePlay();
		}
		DamageSource.Play();
	}
	public void SpikedSound () {
		DamageSource.clip = Spiked;
		DamageSource.Play();
	}
	#endregion

	#region extraSource
	public void RailLandSound () {
		ExtraSource.clip = RailLand;
		ExtraSource.Play();
	}
	#endregion

	#region specificSources

	public void BoostStartSound () {
		BoostSource1.clip = BoostStart;
		BoostSource2.clip = BoostRepeat;
		BoostSource2.Play();
		BoostSource1.Play();
	}

	public void StartRollingSound () {
		//Wont play the sound if spin charge release is currently audible, as they conflict.
		if (!(GeneralSource.clip == SpinDashRelease && GeneralSource.isPlaying))
		{
			BoostSource2.clip = Spin;
			BoostSource2.Play();
		}
	}

	#endregion

}
