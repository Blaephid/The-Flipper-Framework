using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_Skidding : MonoBehaviour
{
	S_PlayerPhysics Player;
	S_CharacterTools Tools;
	S_PlayerInput Inp;


	S_Control_SoundsPlayer sounds;


	[HideInInspector] public float _regularSkiddingStartPoint_;
	[HideInInspector] public float _regularSkiddingIntensity_;
	float _airSkiddingIntensity_;
	public bool _hasSked;
	float _spinSkiddingStartPoint_;
	float _spinSkiddingIntensity_;

	// Start is called before the first frame update
	void Awake()
    {
        Player = GetComponent<S_PlayerPhysics>();
		Tools = GetComponent<S_CharacterTools>();
		Inp = GetComponent<S_PlayerInput>();
		sounds = Tools.SoundControl;

		_regularSkiddingIntensity_ = Tools.Stats.SkiddingStats.skiddingIntensity;
		_airSkiddingIntensity_ = Tools.Stats.WhenInAir.skiddingForce;
		_regularSkiddingStartPoint_ = Tools.Stats.SkiddingStats.skiddingStartPoint;
		_spinSkiddingIntensity_ = Tools.Stats.SpinChargeStats.skidIntesity;
		_spinSkiddingStartPoint_ = Tools.Stats.SpinChargeStats.skidStartPoint;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RegularSkid()
    {
		if (Player.b_normalSpeed < -_regularSkiddingStartPoint_ && !Inp.LockInput)
		{
			float thisSkid;
			if (Player.Grounded)
				thisSkid = _regularSkiddingIntensity_;
			else
				thisSkid = _airSkiddingIntensity_;

			Vector3 releVec = Player.getRelevantVec(Player.rb.velocity);
			if (Player.HorizontalSpeedMagnitude >= -thisSkid) Player.AddVelocity(Player.rb.velocity.normalized * thisSkid * (Player.isRolling ? 0.5f : 1));

			if (!_hasSked && Player.Grounded && !Player.isRolling)
			{
				sounds.SkiddingSound();
				_hasSked = true;


			}
			if (Player.SpeedMagnitude < 4)
			{
				Player.b_normalSpeed = 0;
				_hasSked = false;

			}
		}
		else
		{
			_hasSked = false;

		}
	}

	public void jumpSkid()
    {

		if ((Player.b_normalSpeed < -_regularSkiddingStartPoint_) && !Player.Grounded && !Inp.LockInput)
		{

			Vector3 releVec = Player.getRelevantVec(Player.rb.velocity);
			if (Player.SpeedMagnitude >= -_airSkiddingIntensity_) Player.AddVelocity(new Vector3(releVec.x, 0f, releVec.z).normalized * _airSkiddingIntensity_ * (Player.isRolling ? 0.5f : 1));


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
		if (Player.b_normalSpeed < -_spinSkiddingStartPoint_ && !Inp.LockInput)
		{
			Vector3 releVec = Player.getRelevantVec(Player.rb.velocity);
			if (Player.HorizontalSpeedMagnitude >= -_spinSkiddingIntensity_) Player.AddVelocity(Player.rb.velocity.normalized * _spinSkiddingIntensity_ * (Player.isRolling ? 0.5f : 1));


			if (Player.SpeedMagnitude < 4)
			{
				Player.b_normalSpeed = 0;

			}
		}
	}
}
