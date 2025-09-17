using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class S_Spawn_UI : MonoBehaviour
{
	[Header("Interfaces")]
	public GameObject               _BaseUI;
	public GameObject[]             _AdditionalUI;

	[Header("Transfer To UI")]
	public S_HedgeCamera    _HedgeCamera;

	[Header("Transfered from UIs")]
	public StrucCoreUIElements      _BaseUIElements;

	[Serializable]
	public struct StrucCoreUIElements
	{
		[Header("General")]
		[ColourIfNullAttribute(.8f, 0,0, 1)]    public GameObject       _HudRoot;
		[ColourIfNullAttribute(.8f, 0,0, 1)]    public S_HomingIcon	_HomingIconScript;
		[ColourIfNullAttribute(.8f, 0,0, 1)]    public GameObject       _HomingIconObject;
		[Header("Gauge")]
		[ColourIfNullAttribute(.8f, 0,0, 1)]	public Animator         GaugeAnimator;
		[ColourIfNullAttribute(.8f, 0,0, 1)]	public TextMeshProUGUI  HealthyRingsCounter;
		[ColourIfNullAttribute(.8f, 0, 0, 1)]	public TextMeshProUGUI  DangerousRingsCounter;
		[ColourIfNullAttribute(.8f, 0, 0, 1)]	public TextMeshProUGUI  MaxRingsCounter;
		[ColourIfNullAttribute(.8f, 0, 0, 1)]	public Image                    EnergyBar;
		[ColourIfNullAttribute(.8f, 0, 0, 1)]	public Image                    LevelBar;
		[ColourIfNullAttribute(.8f, 0, 0, 1)]	public Image                    SpeedBar;
		[Header("Time")]
		[ColourIfNullAttribute(.8f, 0,0, 1)]	public TextMeshProUGUI  MinutesText;
		[ColourIfNullAttribute(.8f, 0,0, 1)]	public TextMeshProUGUI  SecondsText;
		[ColourIfNullAttribute(.8f, 0,0, 1)]	public TextMeshProUGUI  MillisecondsText;
		[Header("Action Chain")]
		[ColourIfNullAttribute(.8f, 0,0, 1)]	public GameObject AChainObject;
		[ColourIfNullAttribute(.8f, 0,0, 1)]	public Animator AChainAnimator;
		[ColourIfNullAttribute(.8f, 0,0, 1)]	public S_ActionChainUI AChainUIScript;
		[ColourIfNullAttribute(.8f, 0, 0, 1)]	public TextMeshProUGUI AChainLevelText;
		[ColourIfNullAttribute(.8f, 0, 0, 1)]	public TextMeshProUGUI AChainResultText;
		[Header("Effects")]
		[ColourIfNullAttribute(.8f, 0, 0, 1)]	public S_HintBox                HintBox;
		[ColourIfNullAttribute(.8f, 0,0, 1)]	public Image                  FadeOutBox;

		[Header("Menus")]
		public S_UI_Pause     PauseMenu;

	}


	private GameObject _SpawnedUI;

	private void Awake () {
		//Change name of UI and parent to better track in hiearchy.
		transform.parent.gameObject.name = transform.parent.gameObject.name + gameObject.layer;
		gameObject.name = gameObject.name + " - " + gameObject.layer;
		transform.parent = null; //Also make this have no parent.

		//Spawn main UI and send and receive important variable so interactions between prefabs are possible.
		_SpawnedUI = Instantiate(_BaseUI, transform);
		_SpawnedUI.GetComponentInChildren<S_UI_IngameInterface>().Cam = _HedgeCamera;
		_BaseUIElements = _SpawnedUI.GetComponentInChildren<S_UI_IngameInterface>().PassOnToSpawner;

		foreach (GameObject UI in _AdditionalUI)
		{
			Instantiate(UI, _SpawnedUI.transform);
		}

	}

}
