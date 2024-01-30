using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_Skidding : MonoBehaviour
{
	S_PlayerPhysics Player;
	S_CharacterTools Tools;
	S_PlayerInput Inp;


	S_Control_SoundsPlayer sounds;


	[HideInInspector] public float RegularSkiddingStartPoint;
	[HideInInspector] public float RegularSkiddingIntensity;
	float AirSkiddingIntensity;
	public bool hasSked;
	float SpinSkiddingStartPoint;
	float SpinSkiddingIntensity;

	// Start is called before the first frame update
	void Awake()
    {
        Player = GetComponent<S_PlayerPhysics>();
		Tools = GetComponent<S_CharacterTools>();
		Inp = GetComponent<S_PlayerInput>();
		sounds = Tools.SoundControl;

		RegularSkiddingIntensity = Tools.stats.SkiddingIntensity;
		AirSkiddingIntensity = Tools.stats.AirSkiddingForce;
		RegularSkiddingStartPoint = Tools.stats.SkiddingStartPoint;
		SpinSkiddingIntensity = Tools.coreStats.spinSkidIntesity;
		SpinSkiddingStartPoint = Tools.coreStats.spinSkidStartPoint;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RegularSkid()
    {
		if (Player.b_normalSpeed < -RegularSkiddingStartPoint && !Inp.LockInput)
		{
			float thisSkid;
			if (Player.Grounded)
				thisSkid = RegularSkiddingIntensity;
			else
				thisSkid = AirSkiddingIntensity;

			Vector3 releVec = Player.getRelevantVec(Player.rb.velocity);
			if (Player.HorizontalSpeedMagnitude >= -thisSkid) Player.AddVelocity(Player.rb.velocity.normalized * thisSkid * (Player.isRolling ? 0.5f : 1));

			if (!hasSked && Player.Grounded && !Player.isRolling)
			{
				sounds.SkiddingSound();
				hasSked = true;


			}
			if (Player.SpeedMagnitude < 4)
			{
				Player.b_normalSpeed = 0;
				hasSked = false;

			}
		}
		else
		{
			hasSked = false;

		}
	}

	public void jumpSkid()
    {

		if ((Player.b_normalSpeed < -RegularSkiddingStartPoint) && !Player.Grounded && !Inp.LockInput)
		{

			Vector3 releVec = Player.getRelevantVec(Player.rb.velocity);
			if (Player.SpeedMagnitude >= -AirSkiddingIntensity) Player.AddVelocity(new Vector3(releVec.x, 0f, releVec.z).normalized * AirSkiddingIntensity * (Player.isRolling ? 0.5f : 1));


			if (Player.SpeedMagnitude < 4)
			{
				Player.isRolling = false;
				Player.b_normalSpeed = 0;

			}
		}
	}

	public void spinSkid()
    {
		//Skidding
		if (Player.b_normalSpeed < -SpinSkiddingStartPoint && !Inp.LockInput)
		{
			Vector3 releVec = Player.getRelevantVec(Player.rb.velocity);
			if (Player.HorizontalSpeedMagnitude >= -SpinSkiddingIntensity) Player.AddVelocity(Player.rb.velocity.normalized * SpinSkiddingIntensity * (Player.isRolling ? 0.5f : 1));


			if (Player.SpeedMagnitude < 4)
			{
				Player.b_normalSpeed = 0;

			}
		}
	}
}
