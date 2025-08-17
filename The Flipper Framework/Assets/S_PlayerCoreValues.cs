using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class S_PlayerCoreValues : S_Player_Base
{

	//Energy
	[HideInInspector] public float _energy;
	public AnimationCurve _barFillByEnergy;

	//Velocity & Speed
	private float _displaySpeed;
	private float _prevDisplaySpeed;
	public AnimationCurve _barFillBySpeed;

	//Health
	[HideInInspector] public float _ringCount;

	//Levels
	[HideInInspector] public float _powerCount;
	[HideInInspector] public int _currentLevel;
	public AnimationCurve _barFillByPowerCount;

	//Time
	[HideInInspector] public float _trueTime;
	[HideInInspector] public int _minutes;
	[HideInInspector] public float _seconds;
	[HideInInspector] public float _milliseconds;
	


	private void LateUpdate () {
		UpdateTime();
		UpdateSpeed();
		UpdateRingDisplay();

	}

	private void UpdateTime () {
		_trueTime += Time.deltaTime;
		_seconds += Time.deltaTime;

		if(_seconds >= 60)
		{
			_minutes = Mathf.Min(_minutes+1,99);
			_seconds -= 60;
		}

		_milliseconds = _seconds - (int)_seconds;
		_milliseconds = (int)(_milliseconds * 100);

		string displayMinutes = DisplayIn2Digits(_minutes);
		string displaySeconds = DisplayIn2Digits((int)_seconds);
		string displayMilli = DisplayIn2Digits((int)_milliseconds);

		_CoreUIElements.MillisecondsText.text = displayMilli;
		_CoreUIElements.SecondsText.text = displaySeconds;
		_CoreUIElements.MinutesText.text = displayMinutes;

		return;
		string DisplayIn2Digits(int value) {
			return value >= 10 ? value.ToString() : "0" + value.ToString();
		}
	}

	//Ensure hud text is up to date with ring count.
	private void UpdateRingDisplay () {
		_CoreUIElements.RingsCounter.text = "" + (int)_ringCount;
	}


	//Fill an amount of the speedometer 
	private void UpdateSpeed () {

		//Get a value proportional to max speed.
		_displaySpeed = _PlayerVel._horizontalSpeedMagnitude / (_PlayerMovement._currentMaxSpeed * 1.1f);
		_displaySpeed = Mathf.Clamp(_displaySpeed, 0, 1);
		_displaySpeed = _barFillBySpeed.Evaluate(_displaySpeed);

		//Smooth bar movement.
		float lerpSpeed = Mathf.Abs(_prevDisplaySpeed - _displaySpeed) < 0.2f ? 0.1f : 0.3f;
		_displaySpeed = Mathf.Lerp(_prevDisplaySpeed, _displaySpeed, lerpSpeed);

		//Update
		_prevDisplaySpeed = _displaySpeed;
		_CoreUIElements.SpeedBar.fillAmount = _displaySpeed;
	}

	private void UpdateEnergy () {

	}

	private void UpdatePower () {

	}

}
