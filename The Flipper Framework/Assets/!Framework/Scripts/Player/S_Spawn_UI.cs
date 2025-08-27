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
		[Header("Gauge")]
		public Animator         GaugeAnimator;
		public TextMeshProUGUI  HealthyRingsCounter;
		public TextMeshProUGUI  DangerousRingsCounter;
		public Image                    EnergyBar;
		public Image                    LevelBar;
		public Image                    SpeedBar;
		[Header("Time")]
		public TextMeshProUGUI  MinutesText;
		public TextMeshProUGUI  SecondsText;
		public TextMeshProUGUI  MillisecondsText;
		[Header("Action Chain")]
		public GameObject AChainObject;
		public Animator AChainAnimator;
		public S_ActionChainUI AChainUIScript;
		public TextMeshProUGUI AChainLevelText;
		[Header("Effects")]
		public GameObject               _HomingIcon;
		public S_HintBox                HintBox;
		public Image                  FadeOutBox;

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
