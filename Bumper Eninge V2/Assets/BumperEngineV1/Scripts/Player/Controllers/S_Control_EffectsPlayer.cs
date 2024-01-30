using UnityEngine;
using System.Collections;

public class S_Control_EffectsPlayer : MonoBehaviour {

    public S_PlayerPhysics Player;
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

    public ParticleSystem GetSpinDashDust()
    {
        return SpinDashDust;
    }

    public void DoSpindash(int amm, float speed, float charge, ParticleSystem spinDashDust, float maxCharge)
    {

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

        if(charge > maxCharge - 0.3f)
            ma.startColor = new Color(0.2f, 0.13f, 0.13f, 1);
        else
            ma.startColor = new Color(1f, 1f, 1f, 1);
    }

    public void EndSpinDash()
    {
        SpinDashEnergy.Stop();
        var emission = SpinDashEnergy.emission;
        emission.rateOverTime = 0f;
        SpinDashEnergy.gameObject.SetActive(false);
    }

}
