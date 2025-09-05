using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework;

public class S_ActionChain : S_Player_Base
{

	S_Control_EffectsPlayer _Effects;

	public bool _isActionChainEnabled = true;


	[NonSerialized] public List<string> _ChainList = new List<string>();

	private float _chainLevel = 0;
	private float _pointsGained = 0;

	[SerializeField] AnimationCurve _ChainCountdownByLevel_;
	[SerializeField] AnimationCurve _PointsGainedByLevel_;
	private float _currentCountdown;
	private float _currentTime;
	private float _countdownSpeed;
	private float _countdownSpeedSetFor;

	private bool _chainActive;

	private float _delayBetweenChains = 1.5f;
	private float _delay;

	private void Start () {
		ClearChain();
	}

	public override void AssignTools () {
		base.AssignTools();
		_Effects = _Tools.EffectsControl;
	}

	private void Update () {

		//Somtimes countdown is set to be slower (like in scripted sections that wouldn't be fair to reset chain).
		_countdownSpeedSetFor -= Time.deltaTime;
		if (_countdownSpeedSetFor <= 0) { SetCountdownSpeed(1); }

		//Ensure can't be done immediately after ending. To allow animation to finish from ending.
		if (_delay > 0) { _delay -= Time.deltaTime; return; }

		//Countdown
		if(_currentTime <= 0) { return; }
		_currentTime -= Time.deltaTime * _countdownSpeed;
		if (_currentTime <= 0) { ClearChain(); }
	}

	private void ClearChain () {
		_chainActive = false;

		_pointsGained = _PointsGainedByLevel_.Evaluate(_chainLevel);
		_pointsGained = Mathf.Round(_pointsGained);

		_CoreUIElements.AChainResultText.text = _pointsGained.ToString() ;
		_CoreUIElements.AChainUIScript.EndChain(ExitAnimationFinished);

		_chainLevel = 0;
		_ChainList.Clear();

		_delay = _delayBetweenChains; 
	}

	public void AddToChain ( string source, int value, int requiredActionsSinceLastSource = 2, string subSource = "" ) {
		if (!_isActionChainEnabled || _delay >= 0) return;

		int firstIndex = _ChainList.IndexOf(source);
		int count = _ChainList.Count(s => s == source);

		//To prevent spamming the same objects
		if (subSource != "") 
		{
			if(_ChainList.Contains(subSource)) { return; }

			_ChainList.Add(subSource);
		}

		//This only advances the chain if it hasn't been used too recently, or 
		bool showButDontAdvance =  count > 0 && (firstIndex + 1 <= requiredActionsSinceLastSource) && requiredActionsSinceLastSource != 0;
		value = showButDontAdvance ? 0 : value;

		//Add action to list
		_CoreUIElements.AChainUIScript.SpawnNewText(source, value);

		if (!showButDontAdvance)
		{
			//UI UPDATES
			if (!_chainActive) { _CoreUIElements.AChainAnimator.SetTrigger("Enter"); _chainActive = true; }
			else
			{
				_CoreUIElements.AChainAnimator.SetTrigger("New Action");
				_CoreUIElements.AChainAnimator.SetTrigger("New Action"); //Applied twice for multiple layers.
			}

			//Add at start of list, so we can tell how long ago it was.
			_ChainList.Insert(0, source);
			_chainLevel += value;

			//Set countdown
			_currentCountdown = _ChainCountdownByLevel_.Evaluate(_chainLevel);
			_currentTime = _currentCountdown;

			//UI Countdown
			float animationLength = 2;
			_CoreUIElements.AChainAnimator.SetFloat("CountdownModifier", animationLength / _currentTime);

			//UI Level
			_CoreUIElements.AChainLevelText.text = _chainLevel.ToString();

			_Effects.ActionChainAdd();
		}
	}

	public void SetCountdownSpeed ( float value ) { _countdownSpeed = value; }

	public void SetCountdownSpeedTemp ( float value, float time ) { _countdownSpeed = value; _countdownSpeedSetFor = time; }

	public void ExitAnimationFinished () {
		_CoreValues.AdjustPoints(_pointsGained);
	}

	public override void AssignStats () {
		base.AssignStats();

		_PointsGainedByLevel_ = _Tools.LevelUpStats.pointsPerActionChainLevel;
		_ChainCountdownByLevel_ = _Tools.LevelUpStats.chainCountDownPerLevel;
	}

}
