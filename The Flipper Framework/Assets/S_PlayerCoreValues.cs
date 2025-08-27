using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class S_PlayerCoreValues : S_Player_Base
{
	private S_PlayerScore _Score;

	//Health
	public event    EventHandler<float> OnRingGet;

	public int _ringCount { get; private set; }
	private int _currentMaxRings;
	private int _startMaxRings_;
	private Vector2 _ringsOnStartAndDeath_;
	[HideInInspector] public int _dieAtRingCount_;

	//Levels
	private int _level = 0;

	//Velocity & Speed
	private float _startSpeedMultiplier_ = 1;
	[NonSerialized] public float _currentSpeedMultiplier = 1;
	private float _displaySpeed;
	private float _prevDisplaySpeed;
	public AnimationCurve _barFillBySpeed;

	//Energy
	public float _energy { get; private set; }
	[HideInInspector] public float _currentMaxEnergy;
	[HideInInspector] public float _startMaxEnergy_;
	public AnimationCurve _barFillByEnergy;
	[NonSerialized] public bool _currentlyDrainingEnergy;

	private bool        _gainEnergyFromRings_ = true;
	private bool        _gainEnergyOverTime_ = false;
	private float       _energyGainPerSecond_ = 5;
	private float       _energyGainPerRing_ = 5;
	private Vector2 _energyOnStartAndDeath_;


	//Power
	private float _currentPowerNeedForNextLevel;
	public float _powerCount { get; private set; }
	public AnimationCurve _barFillByPowerCount;

	private void Start () {
		_Score = GetComponent<S_PlayerScore>();
		SetValuesOnLevelStart();
	}

	private void LateUpdate () {
		UpdateSpeed();
		UpdateRingDisplay();
		UpdateEnergy();
		UpdatePower();

		if (!_currentlyDrainingEnergy) { GainEnergyFromTime(); }
	}

	/// <summary>
	/// DISPLAYING VALUES
	/// </summary>
	#region display Values On hud

	//Ensure hud text is up to date with ring count.
	private void UpdateRingDisplay () {
		_CoreUIElements.HealthyRingsCounter.text = "" + (int)_ringCount;
		_CoreUIElements.DangerousRingsCounter.text = "" + (int)_ringCount;

		bool canDieFromDamage = _ringCount <= _dieAtRingCount_;
		_CoreUIElements.HealthyRingsCounter.gameObject.SetActive(!canDieFromDamage);
		_CoreUIElements.DangerousRingsCounter.gameObject.SetActive(canDieFromDamage);

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
		float proportionalEnergy = _energy / _currentMaxEnergy;
		_CoreUIElements.EnergyBar.fillAmount = _barFillByEnergy.Evaluate(proportionalEnergy);
	}

	private void UpdatePower () {
		float proportionalPower = _powerCount / _currentPowerNeedForNextLevel;
		_CoreUIElements.LevelBar.fillAmount = _barFillByPowerCount.Evaluate(proportionalPower);
	}
	#endregion


	/// <summary>
	/// Public Methods
	/// </summary>
	#region public

	public void SetValuesOnLevelStart () {
		_ringCount = (int)_ringsOnStartAndDeath_.x;
		_energy = _currentMaxEnergy * _energyOnStartAndDeath_.x;
	}

	public void SetValuesOnRespawn () {
		int change = (int)_ringsOnStartAndDeath_.y - _ringCount;
		AdjustRings(change, false);

		_Score._redRings = _Score._savedRedRings;
		_energy = _currentMaxEnergy * _energyOnStartAndDeath_.y;
	}

	public void AdjustEnergy ( float change ) {
		_energy += change;
		_energy = Mathf.Clamp(_energy, 0, _currentMaxEnergy);
	}

	public void AdjustRings ( int change, bool forEnergy ) {
		_ringCount += change;

		if (_ringCount < _currentMaxRings)
			_Score.AdjustRingScore(change);

		_ringCount = Mathf.Clamp(_ringCount, 0, _currentMaxRings);

		if (change > 0) _CoreUIElements.GaugeAnimator.SetTrigger("GetRing");

		if (OnRingGet != null && change > 0)
			{ OnRingGet.Invoke(null, change); }
	}

	public void AdjustPower ( float change ) {
		_powerCount += change;
		_powerCount = Mathf.Clamp(_powerCount, 0, _currentPowerNeedForNextLevel);

		CheckLevels();
	}

	private void CheckLevels () {
		if (_powerCount < _currentPowerNeedForNextLevel && _currentPowerNeedForNextLevel != 0) { return; }
		if(_level == _Tools.LevelUpStats._Levels.Count + 1 ) { return; }

		LevelUp();
	}

	private void LevelUp () {
		_level++;
		int index = _level - 1;

		_powerCount = 0;

		//Increase values to new level
		_currentPowerNeedForNextLevel = _Tools.LevelUpStats._Levels[index].requiredPower;
		_currentMaxEnergy = _startMaxEnergy_ * _Tools.LevelUpStats._Levels[index].energyMaxMultiplier;
		_currentMaxRings = (int)(_startMaxRings_ * _Tools.LevelUpStats._Levels[index].ringsMaxMultiplier);
		_currentSpeedMultiplier = _startSpeedMultiplier_ * _Tools.LevelUpStats._Levels[index].speedMaxMultiplier;


		//Effects
		_CoreUIElements.GaugeAnimator.SetInteger("Level", _level);
		if (_level > 1)
		{
			_CoreUIElements.GaugeAnimator.SetTrigger("LevelUp");
			_Sounds.LevelUpSound();
		}
	}

	public void SaveValuesOnCheckpoint () {

	}

	//These functions will handle increasing boost energy from various sources. Some are events that will be attached to event Handlers.
	void EventGainEnergyFromRings ( object sender, float source ) {
		source *= _energyGainPerRing_; //The source is how many rings, so gain energy for each multiplied by amount per ring.
		_CoreValues.AdjustEnergy(source);
	}

	//Not an event, but depending on stats will be called every frame to increase energy.
	void GainEnergyFromTime () {
		if (!_gainEnergyOverTime_) { return; }
		float source = Time.fixedDeltaTime * _energyGainPerSecond_;
		_CoreValues.AdjustEnergy(source);
	}

	#endregion

	public override void AssignStats () {
		base.AssignStats();

		_dieAtRingCount_ = _Tools.Stats.CoreValuesStats.dieAtDamageFromRingCount;
		_startMaxRings_ = _Tools.Stats.CoreValuesStats.startMaxRingCount;
		_currentMaxRings = _startMaxRings_;
		_ringsOnStartAndDeath_ = _Tools.Stats.CoreValuesStats.ringsOnPlayerStart;

		_startSpeedMultiplier_ = 1;

		_startMaxEnergy_ = _Tools.Stats.CoreValuesStats.startMaxEnergy;
		_currentMaxEnergy = _startMaxEnergy_;
		_gainEnergyFromRings_ = _Tools.Stats.CoreValuesStats.gainEnergyFromRings;
		_gainEnergyOverTime_ = _Tools.Stats.CoreValuesStats.gainEnergyOverTime;
		_energyGainPerRing_ = _Tools.Stats.CoreValuesStats.energyGainPerRing;
		_energyGainPerSecond_ = _Tools.Stats.CoreValuesStats.energyGainPerSecond;
		_energyOnStartAndDeath_ = _Tools.Stats.CoreValuesStats.energyOnPlayerStart;
		if (_gainEnergyFromRings_)
			OnRingGet += EventGainEnergyFromRings;

		LevelUp();
	}
}
