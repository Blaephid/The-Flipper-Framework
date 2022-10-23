using UnityEngine;
using System.Collections;

public class SonicEffectsControl : MonoBehaviour {

    public PlayerBhysics Player;
    public ParticleSystem RunningDust;
	public ParticleSystem SpeedLines;
    public ParticleSystem SpinDashDust;
    public ParticleSystem SpinDashEnergy;
    public float RunningDustThreshold;
	public float SpeedLinesThreshold;

    [Header("Rails")]
    [SerializeField]
    ParticleSystem RailsSparks1;
    [SerializeField]
    ParticleSystem RailsSparks2;

    [Header("Mouth Sides")]
    [SerializeField]
    Transform Head;
    [SerializeField]
    Transform LeftMouth, RightMouth;
    [SerializeField]
    Transform[] MouthsToHide;
    [SerializeField]
    Transform Eyelids;

    void FixedUpdate () {
	
		if(Player.rb.velocity.sqrMagnitude > RunningDustThreshold && Player.Grounded && RunningDust != null)
        {
            RunningDust.Emit(Random.Range(0,20));
        }

		if (Player.rb.velocity.sqrMagnitude > SpeedLinesThreshold && Player.Grounded && SpeedLines != null && SpeedLines.isPlaying == false) 
		{
			SpeedLines.Play ();
		} 
		else if (Player.rb.velocity.sqrMagnitude < SpeedLinesThreshold && SpeedLines.isPlaying == true || (!Player.Grounded)) 
		{
			SpeedLines.Stop ();
		}

	}
    public void DoSpindash(int amm, float speed, float charge)
    {
        SpinDashDust.startSpeed = speed;
        SpinDashDust.Emit(amm);
        
        if (!SpinDashEnergy.isPlaying)
        {
            SpinDashEnergy.gameObject.SetActive(true);
            SpinDashEnergy.Play();
            charge = 0;
        }
        var emission = SpinDashEnergy.emission;
        emission.rateOverTime = charge;
    }

    public void EndSpinDash()
    {
        SpinDashEnergy.Stop();
        var emission = SpinDashEnergy.emission;
        emission.rateOverTime = 0f;
        SpinDashEnergy.gameObject.SetActive(false);
    }

}
