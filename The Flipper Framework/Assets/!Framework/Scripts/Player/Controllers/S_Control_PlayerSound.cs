using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S_Control_PlayerSound : MonoBehaviour {

    public S_PlayerPhysics Player;

    AudioSource Source;
    public AudioSource Source2;
    public AudioSource Source3;
	public AudioSource Source4;
    public AudioClip[] FootSteps;
    public AudioClip Jumping;
    public AudioClip AirDash;
    public AudioClip HomingAttack;
    public AudioClip Skidding;
    public AudioClip Spin;
    public AudioClip SpinDash;
    public AudioClip SpinDashRelease;
	public AudioClip BounceStart;
	public AudioClip BounceImpact;
	public AudioClip StompImpact;
    public AudioClip RingLoss;
    public AudioClip Die;
    public AudioClip Spiked;

	public AudioClip[] CombatVoiceClips;
	public AudioClip[] JumpingVoiceClips;
	public AudioClip[] PainVoiceClips;

    public float pitchBendingRate = 1;

    public void Test(string i)
    {

    }

    void Start()
    {
        Source = GetComponent<AudioSource>();
    }

	public void CombatVoicePlay()
	{
		int rand = Random.Range(0, CombatVoiceClips.Length);
		Source4.clip = CombatVoiceClips[rand];
		Source4.Play();
	}
	public void JumpingVoicePlay()
	{
		int rand = Random.Range(0, JumpingVoiceClips.Length);
		Source4.clip = JumpingVoiceClips[rand];
		Source4.Play();
	}
	public void PainVoicePlay()
	{
		int rand = Random.Range(0, PainVoiceClips.Length);
		Source4.clip = PainVoiceClips[rand];
		Source4.Play();
	}
    public void FootStepSoundPlay()
    {
		if (FootSteps.Length > 0) {
			int rand = Random.Range (0, FootSteps.Length);
			Source.clip = FootSteps [rand];
			Source.Play ();
		}
    }
    public void JumpSound()
    {
		if (JumpingVoiceClips.Length > 0) {
			JumpingVoicePlay ();
		}
        Source2.clip = Jumping;
        Source2.Play();
    }
    public void SkiddingSound()
    {
        Source2.clip = Skidding;
        Source2.Play();
    }
    public void HomingAttackSound()
    {
        Source2.clip = HomingAttack;
        Source2.Play();
		if (CombatVoiceClips.Length > 0) {
			CombatVoicePlay ();
		}
    }
    public void AirDashSound()
    {
        Source2.clip = AirDash;
        Source2.Play();
    }
    public void SpinningSound()
    {
        if(!(Source2.clip == SpinDashRelease && Source2.isPlaying))
        {
            Source2.clip = Spin;
            Source2.Play();
        }
    }
    public void SpinDashSound()
    {
        Source2.clip = SpinDash;
        Source2.Play();
    }
	public void BounceStartSound()
	{
		Source2.clip = BounceStart;
		Source2.Play();
	}
	public void BounceImpactSound()
	{
		Source2.clip = BounceImpact;
		Source2.Play();
	}
	public void StompImpactSound()
	{
		Source2.clip = StompImpact;
		Source2.Play();
	}
    public void SpinDashReleaseSound()
    {
        Source2.clip = SpinDashRelease;
        Source2.Play();
    }
    public void RingLossSound()
    {
        Source3.clip = RingLoss;
		if (PainVoiceClips.Length > 0) {
			PainVoicePlay ();
		}
        Source3.Play();
    }
    public void DieSound()
    {
        Source3.clip = Die;
		if (PainVoiceClips.Length > 0) {
			PainVoicePlay ();
		}
        Source3.Play();
    }
    public void SpikedSound()
    {
        Source3.clip = Spiked;
        Source3.Play();
    }

}
