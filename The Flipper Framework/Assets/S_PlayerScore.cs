using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerScore : S_Player_Base
{
	//SCORE
	public float _ringScore { get; private set; }
	public int _savedRingScore;

	[NonSerialized] public int _redRings;
	[NonSerialized] public int _savedRedRings;

	//Time
	[HideInInspector] public float _trueTime;
	[HideInInspector] public int _minutes;
	[HideInInspector] public float _seconds;
	[HideInInspector] public float _milliseconds;

	public void AdjustRingScore (float change) {
		if (change > 0) { _ringScore += change; }
		else { _ringScore -= (int)(change * 0.5f); }
	}

	private void LateUpdate () {
		UpdateTime();
	}

	//Time is displayed by three different TMPros.
	private void UpdateTime () {
		_trueTime += Time.deltaTime;
		_seconds += Time.deltaTime;

		if (_seconds >= 60)
		{
			_minutes = Mathf.Min(_minutes + 1, 99);
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
		//To prevent TMPros becoming too narrow.
		string DisplayIn2Digits ( int value ) {
			return value >= 10 ? value.ToString() : "0" + value.ToString();
		}
	}
}
