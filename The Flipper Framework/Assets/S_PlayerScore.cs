using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerScore : S_Player_Base
{

	//Ranks
	[NonSerialized] public S_O_StageInfo _StageInfo;
	int _timeRank;
	int _ringsRank;
	[NonSerialized] public string _ringsRankText;
	[NonSerialized] public string _timeRankText;
	[NonSerialized] public string _totalRankText;

	[NonSerialized] public Vector3	_toNextRankTime;
	[NonSerialized] public float	_toNextRankRings;


	//SCORE
	public float _ringScore { get; private set; }
	public float _ringScoreGained { get; private set; }
	public float _ringScoreLost { get; private set; }

	[NonSerialized] public int _redRings;
	[NonSerialized] public int _savedRedRings;

	//Time
	[HideInInspector] public bool _paused;
	[HideInInspector] public float _trueTime;
	[HideInInspector] public int _minutes;
	[HideInInspector] public int _seconds;
	[HideInInspector] public int _milliseconds;

	public static Dictionary<int, string> RankValueToLetter = new Dictionary<int, string>()
	{
		{ 1, "D" },
		{ 2, "C" },
		{ 3, "B" },
		{ 4, "A" },
		{ 5, "S" },
		{ 6, "X" },

	};

	#region private

	private void LateUpdate () {
		if(_paused) { return; }
		UpdateTime();
	}

	//Time is displayed by three different TMPros.
	private void UpdateTime () {
		_trueTime += Time.deltaTime;

		_trueTime = Mathf.Min(_trueTime, 5999);

		Vector3 timeVector = S_S_MoreMaths.ConvertFloatTimeToMinutesVector(_trueTime);
		_minutes = (int)timeVector.x;
		_seconds = (int)timeVector.y;
		_milliseconds = (int)timeVector.z;

		//To prevent TMPros becoming too narrow.
		string displayMinutes = S_S_MoreMaths.DisplayIntAsStringInXDigits(_minutes);
		string displaySeconds = S_S_MoreMaths.DisplayIntAsStringInXDigits(_seconds);
		string displayMilli = S_S_MoreMaths.DisplayIntAsStringInXDigits(_milliseconds);

		_CoreUIElements.MillisecondsText.text = displayMilli;
		_CoreUIElements.SecondsText.text = displaySeconds;
		_CoreUIElements.MinutesText.text = displayMinutes;

		return;
	}

	#endregion

	#region public
	public void AdjustRingScore ( float change ) {
		if (change > 0)
		{
			_ringScore += change;
			_ringScoreGained += change;
		}
		else
		{
			float lossPriority = 0.85f;
			_ringScore += (int)(change * lossPriority);
			_ringScoreLost += Mathf.Abs(change * lossPriority);
		}
	}

	public void SaveValuesOnCheckpoint () {
		_savedRedRings = _redRings;
	}

	public void SetValuesOnRespawn () {
		_redRings = _savedRedRings;
	}


	//Takes the score values, compare to the stage ranking requirements, and apply.
	public void CalculateRank () {

		CheckTimeRank();
		CheckRingsRank();

		_timeRankText = S_PlayerScore.RankValueToLetter[_timeRank];
		_ringsRankText = S_PlayerScore.RankValueToLetter[_ringsRank];

		Debug.Log(_trueTime);
		Debug.Log(S_S_MoreMaths.ConvertFloatTimeToMinutesVector(_trueTime));
		Debug.Log(_toNextRankTime);
		Debug.Log(_ringScore);
		Debug.Log("+ " +_ringScoreGained + "  -  " +_ringScoreLost);
		Debug.Log(_toNextRankRings);


		if (_timeRankText == "X") { _totalRankText = "X"; }
		else { _totalRankText = _timeRankText + _ringsRankText; }
	}

	//Add a time rank point for each level.
	private void CheckTimeRank () {
		if (_trueTime <= _StageInfo._Ranks.TimeTotal_DRank) { _timeRank++; }
		else { _toNextRankTime = _StageInfo._Ranks.Time_DRank;  return; }

		if (_trueTime <= _StageInfo._Ranks.TimeTotal_CRank) { _timeRank++; }
		else { _toNextRankTime = _StageInfo._Ranks.Time_CRank; return; }

		if (_trueTime <= _StageInfo._Ranks.TimeTotal_BRank) { _timeRank++; }
		else { _toNextRankTime = _StageInfo._Ranks.Time_BRank; return; }

		if (_trueTime <= _StageInfo._Ranks.TimeTotal_ARank) { _timeRank++; }
		else { _toNextRankTime = _StageInfo._Ranks.Time_ARank; return; }

		if (_trueTime <= _StageInfo._Ranks.TimeTotal_SRank) { _timeRank++; }
		else { _toNextRankTime = _StageInfo._Ranks.Time_SRank; return; }

		if (_trueTime <= _StageInfo._Ranks.TimeTotal_XRank) { _timeRank++; }
		else { _toNextRankTime = _StageInfo._Ranks.Time_XRank; return; }
	}

	private void CheckRingsRank () {
		if (_ringScore >= _StageInfo._Ranks.Rings_DRank) { _ringsRank++; }
		else { _toNextRankRings = _StageInfo._Ranks.Rings_DRank; return; }

		if (_ringScore >= _StageInfo._Ranks.Rings_CRank) { _ringsRank++; }
		else { _toNextRankRings = _StageInfo._Ranks.Rings_CRank; return; }

		if (_ringScore >= _StageInfo._Ranks.Rings_BRank) { _ringsRank++; }
		else { _toNextRankRings = _StageInfo._Ranks.Rings_BRank; return; }

		if (_ringScore >= _StageInfo._Ranks.Rings_ARank) { _ringsRank++; }
		else { _toNextRankRings = _StageInfo._Ranks.Rings_ARank; return; }

		if (_ringScore >= _StageInfo._Ranks.Rings_SRank) { _ringsRank++; }
		else { _toNextRankRings = _StageInfo._Ranks.Rings_SRank; return; }
	}

	#endregion
}
