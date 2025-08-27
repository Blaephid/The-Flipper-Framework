using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework;

public class S_ActionChain : S_Player_Base
{

	public bool _isActionChainEnabled = true;


	[NonSerialized] public List<string> _ChainList = new List<string>();

	private float _chainLevel = 0;

	[SerializeField] AnimationCurve _ChainCountdownByLevel;
	private float _currentCountdown;
	private float _currentTime;
	private float _countdownSpeed;
	private float _countdownSpeedSetFor;

	private bool _chainActive;

	private void Start () {
		ClearChain();
	}

	private void Update () {
		_currentTime -= Time.deltaTime * _countdownSpeed;
		_countdownSpeedSetFor -= Time.deltaTime;

		if (_currentTime <= 0) { ClearChain(); }

		if (_countdownSpeedSetFor <= 0) { SetCountdownSpeed(1); }
	}

	private void ClearChain () {
		_chainActive = false;

		_chainLevel = 0;
		_ChainList.Clear();

		_CoreUIElements.AChainUIScript.EndChain();
	}

	public void AddToChain ( string source, int value, int requiredActionsSinceLastSource = 2, string subSource = "" ) {
		if (!_isActionChainEnabled) return;

		int firstIndex = _ChainList.IndexOf(source);
		int count = _ChainList.Count(s => s == source);

		//To prevent spamming the same objects
		if (subSource != "" && _ChainList.Contains(subSource)) { return; }

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
			_currentCountdown = _ChainCountdownByLevel.Evaluate(_chainLevel);
			_currentTime = _currentCountdown;

			//UI Countdown
			float animationLength = 2;
			_CoreUIElements.AChainAnimator.SetFloat("CountdownModifier", animationLength / _currentTime);

			//UI Level
			_CoreUIElements.AChainLevelText.text = _chainLevel.ToString();
		}
	}

	public void SetCountdownSpeed ( float value ) { _countdownSpeed = value; }

	public void SetCountdownSpeedTemp ( float value, float time ) { _countdownSpeed = value; _countdownSpeedSetFor = time; }

}
