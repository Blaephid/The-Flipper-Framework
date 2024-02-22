using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_Skidding : MonoBehaviour
{
	S_PlayerPhysics Player;
	S_CharacterTools Tools;
	S_PlayerInput Inp;


	S_Control_PlayerSound sounds;


	[HideInInspector] public float _regularSkiddingStartPoint_;
	[HideInInspector] public float _regularSkiddingIntensity_;
	float _airSkiddingIntensity_;
	public bool _hasSked;
	float _spinSkiddingStartPoint_;
	float _spinSkiddingIntensity_;

	// Start is called before the first frame update
	void Awake () {
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
	void Update () {

	}

	public void RegularSkid () {
		if (Player._inputVelocityDifference < -_regularSkiddingStartPoint_ && !Inp._isInputLocked)
		{
			Tools.CharacterAnimator.SetTrigger("Skidding");

			float thisSkid;
			if (Player._isGrounded)
				thisSkid = _regularSkiddingIntensity_;
			else
				thisSkid = _airSkiddingIntensity_;

			Vector3 releVec = Player.GetRelevantVec(Player._RB.velocity);
			if (Player._horizontalSpeedMagnitude >= -thisSkid) Player.AddCoreVelocity(Player._RB.velocity.normalized * thisSkid * (Player._isRolling ? 0.5f : 1));

			if (!_hasSked && Player._isGrounded && !Player._isRolling)
			{
				sounds.SkiddingSound();
				_hasSked = true;


			}
			if (Player._speedMagnitude < 4)
			{
				_hasSked = false;

			}
		}
		else
		{
			_hasSked = false;

		}
	}

	public void jumpSkid () {

		if ((Player._inputVelocityDifference < -_regularSkiddingStartPoint_) && !Player._isGrounded && !Inp._isInputLocked)
		{

			Vector3 releVec = Player.GetRelevantVec(Player._RB.velocity);
			if (Player._speedMagnitude >= -_airSkiddingIntensity_) Player.AddCoreVelocity(new Vector3(releVec.x, 0f, releVec.z).normalized * _airSkiddingIntensity_ * (Player._isRolling ? 0.5f : 1));


			if (Player._speedMagnitude < 4)
			{
				Player._isRolling = false;

			}
		}
	}

	public void spinSkid () {
		//Skidding
		if (Player._inputVelocityDifference < -_spinSkiddingStartPoint_ && !Inp._isInputLocked)
		{
			Vector3 releVec = Player.GetRelevantVec(Player._RB.velocity);
			if (Player._horizontalSpeedMagnitude >= -_spinSkiddingIntensity_) Player.AddCoreVelocity(Player._RB.velocity.normalized * _spinSkiddingIntensity_ * (Player._isRolling ? 0.5f : 1));

		}
	}
}
