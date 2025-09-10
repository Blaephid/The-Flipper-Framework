using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class S_PlayerCoreValues : S_Player_Base
{
	[NonSerialized] public AudioSource _Music;

	private S_PlayerScore _Score;
	private S_Control_EffectsPlayer _Effects;

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
	public float _multiplierFromEnergy { get; private set; }

	private float _displayEnergy;
	private float _prevDisplayEnergy;

	[HideInInspector] public float _currentMaxEnergy;
	[HideInInspector] public float _startMaxEnergy_;
	public AnimationCurve _barFillByEnergy;
	[NonSerialized] public bool _currentlyDrainingEnergy;

	private bool        _gainEnergyFromRings_ = true;
	private bool        _gainEnergyOverTime_ = false;
	private float       _energyGainPerSecond_ = 5;
	private float       _energyGainPerRing_ = 5;
	private Vector2 _energyOnStartAndDeath_;


	//Points
	private float _currentPointsNeedForNextLevel;
	public float _pointsCount { get; private set; }
	public AnimationCurve _barFillByPointsCount;

	private float _pointsInQuickSuccession;
	private bool _hasPointsHitThreshold;
	private float _countdownForPointsQuickSuccession;
	private float _setCountdownTo = 0.5f;

	private float _displayPoints;
	private float _prevDisplayPoints;

	private void Start () {
		_Score = GetComponent<S_PlayerScore>();
		SetValuesOnLevelStart();
	}

	private void LateUpdate () {
		if(_Actions._isPaused) { return; }

		UpdateSpeed();
		UpdateRingDisplay();
		UpdateEnergy();
		UpdatePoints();

		if (!_currentlyDrainingEnergy) { GainEnergyFromTime(); }


		CheckPointsInQuickSuccession();
	}

	//In case getting a lot of small points in quick succession, add them up to an amount, and apply effects if they pass a threshold.
	private void CheckPointsInQuickSuccession () {

		//If got points recently, do countdown.
		if (_countdownForPointsQuickSuccession >= 0)
		{

			_countdownForPointsQuickSuccession -= Time.deltaTime;
			//Remove in quick succesion.
			if (_countdownForPointsQuickSuccession <= 0)
			{
				_pointsInQuickSuccession = 0;
				_hasPointsHitThreshold = false;
			}

			if (!_hasPointsHitThreshold && _pointsInQuickSuccession >= 4)
			{
				CheckPointsEffects(_pointsInQuickSuccession);
				_hasPointsHitThreshold = true;
				_pointsInQuickSuccession = 0;
			}
		}
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
		float proportionalSpeed = _PlayerVel._horizontalSpeedMagnitude / (_PlayerMovement._currentMaxSpeed * 1.1f);
		proportionalSpeed = Mathf.Clamp(proportionalSpeed, 0, 1);
		proportionalSpeed = _barFillBySpeed.Evaluate(proportionalSpeed);

		_displaySpeed = LerpDisplayValue(proportionalSpeed, ref _prevDisplaySpeed);
		_CoreUIElements.SpeedBar.fillAmount = _displaySpeed;
	}

	private void UpdateEnergy () {
		float proportionalEnergy = _energy / _currentMaxEnergy;
		_displayEnergy = _barFillByEnergy.Evaluate(proportionalEnergy);

		_displayEnergy = LerpDisplayValue(proportionalEnergy, ref _prevDisplayEnergy);
		_CoreUIElements.EnergyBar.fillAmount = _displayEnergy;
	}

	private void UpdatePoints () {
		float proportionalPoints = _pointsCount / _currentPointsNeedForNextLevel;

		_displayPoints = LerpDisplayValue(proportionalPoints, ref _prevDisplayPoints);
		_CoreUIElements.LevelBar.fillAmount = _displayPoints;
	}

	private float LerpDisplayValue ( float current, ref float previous ) {
		//Smooth bar movement.
		float lerpSpeed = Mathf.Abs(previous - current) < 0.2f ? 0.1f : 0.3f;
		lerpSpeed += previous > current ? 0.1f : 0;
		current = Mathf.Lerp(previous, current, lerpSpeed);

		//Update
		previous = current;
		return current;
	}
	#endregion


	/// <summary>
	/// Public Methods
	/// </summary>
	#region public

	public void SetValuesOnLevelStart () {
		_ringCount = (int)_ringsOnStartAndDeath_.x;
		_energy = _currentMaxEnergy * _energyOnStartAndDeath_.x;

		_Score.SetValuesOnRespawn();
	}

	public void SetValuesOnRespawn () {
		int change = (int)_ringsOnStartAndDeath_.y - _ringCount;
		AdjustRings(change, false);

		_Score._redRings = _Score._savedRedRings;
		_energy = _currentMaxEnergy * _energyOnStartAndDeath_.y;
	}

	//Gain or lose energy
	public void AdjustEnergy ( float change ) {
		if (change > 4 && _energy < _currentMaxEnergy) _CoreUIElements.GaugeAnimator.SetTrigger("GetEnergy");

		_energy += change;
		_energy = Mathf.Clamp(_energy, 0, _currentMaxEnergy);
	}

	//This multiplier is a general float used in a variety of calculation to improve or weaken certain actions. For instance, the roll action uses energy to increase this, which affects downhill boost.
	public void SetMultiplierFromEnergy(float set ) {
		_multiplierFromEnergy = set;
	}

	//Gain or lose rings
	public void AdjustRings ( int change, bool forEnergy ) {
		_ringCount += change;

		if (_ringCount < _currentMaxRings)
			_Score.AdjustRingScore(change);

		_ringCount = Mathf.Clamp(_ringCount, 0, _currentMaxRings);

		//UI
		if (change > 0) _CoreUIElements.GaugeAnimator.SetTrigger("GetRing");

		//Used for energy
		if (OnRingGet != null && change > 0)
		{ OnRingGet.Invoke(null, change); }
	}

	public void AdjustPoints ( float change ) {
		_pointsCount += change;
		_pointsCount = Mathf.Clamp(_pointsCount, 0, _currentPointsNeedForNextLevel);

		_pointsInQuickSuccession += change;
		_countdownForPointsQuickSuccession = _setCountdownTo;

		CheckPointsEffects(change);
		CheckLevelUp();
	}

	private void CheckPointsEffects ( float value ) {
		if (value > 3)
		{
			_CoreUIElements.GaugeAnimator.SetTrigger("GetPoints");

			if (value > 4)
				_Effects.PointsGain(value);
		}
	}

	private void CheckLevelUp () {
		if (_pointsCount < _currentPointsNeedForNextLevel && _currentPointsNeedForNextLevel != 0) { return; }
		if (_level == _Tools.LevelUpStats._Levels.Count + 1) { return; }

		LevelUp();
	}

	private void LevelUp () {
		_level++;
		int index = _level - 1;

		_pointsCount = 0;

		//Increase values to new level
		_currentPointsNeedForNextLevel = _Tools.LevelUpStats._Levels[index].requiredPoints;
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
		_Score.SaveValuesOnCheckpoint();
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

	public override void AssignTools () {
		base.AssignTools();

		_Score = GetComponent<S_PlayerScore>();
		_Effects = _Tools.EffectsControl;
	}

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
