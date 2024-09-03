using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S_Control_SoundsPlayer : MonoBehaviour
{
	[Header("Tools")]
	public S_PlayerPhysics Player;

	[Header("Audio Sources")]
	public AudioSource	FeetSource;
	public AudioSource GeneralSource;
	public AudioSource DamageSource;
	public AudioSource VoiceSource;
	public AudioSource BoostSource1, BoostSource2;


	[Header("Clips")]
	public AudioClip[] FootSteps;
	[Header("Actions")]
	[Header("Air")]
	public AudioClip Jumping;
	public AudioClip AirDash;
	public AudioClip HomingAttack;
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

	[Header("Damage")]
	public AudioClip RingLoss;
	public AudioClip Die;
	public AudioClip Spiked;

	[Header("Voice")]
	public AudioClip[] CombatVoiceClips;
	public AudioClip[] JumpingVoiceClips;
	public AudioClip[] PainVoiceClips;

	public float pitchBendingRate = 1;

	public void Test ( string i ) {

	}

	void Start () {
	}

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

	//This is called by an Animation event in specific animations. Any walking/running animation will call this.
	//Make sure the animator component is on the same object as this script. And that this method isn't renamed.
	public void FootStepSoundPlay () {
		if (FootSteps.Length > 0 && !FeetSource.isPlaying)
		{
			int rand = Random.Range (0, FootSteps.Length);
			FeetSource.clip = FootSteps[rand];
			FeetSource.Play();
		}
	}

	public void QuickStepSound () {
		FeetSource.clip = QuickStep;
		FeetSource.Play();
	}

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
	public void AirDashSound () {
		GeneralSource.clip = AirDash;
		GeneralSource.Play();
	}
	public void SpinningSound () {
		if (!(GeneralSource.clip == SpinDashRelease && GeneralSource.isPlaying))
		{
			GeneralSource.clip = Spin;
			GeneralSource.Play();
		}
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
	public void RingLossSound () {
		DamageSource.clip = RingLoss;
		if (PainVoiceClips.Length > 0)
		{
			PainVoicePlay();
		}
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

	public void BoostStartSound () {
		BoostSource1.clip = BoostStart;
		BoostSource2.clip = BoostRepeat;
		BoostSource2.Play();
		BoostSource1.Play();
	}

}
