using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class S_Spawn_UI : MonoBehaviour
{
	[Header("Interfaces")]
	public GameObject		_BaseUI;
	public GameObject[]		_AdditionalUI;

	[Header("Transfer To UI")]
	public S_HedgeCamera	_HedgeCamera;

	[Header("Transfered from UIs")]
	public StrucCoreUIElements	_BaseUIElements;
	public S_UI_Boost		_BoostUI;

	[Serializable]
	public struct StrucCoreUIElements {
		public TextMeshProUGUI	RingsCounter;
		public TextMeshProUGUI	SpeedCounter;
		public S_HintBox		HintBox;
		public Image                  FadeOutBox;
	}


	private GameObject _SpawnedUI;

	private void Awake () {
		//Change name of UI and parent to better track in hiearchy.
		transform.parent.gameObject.name = transform.parent.gameObject.name + gameObject.layer;
		gameObject.name = gameObject.name + " - " + gameObject.layer;
		transform.parent = null; //Also make this have no parent.

		//Spawn main UI and send and receive important variable so interactions between prefabs are possible.
		_SpawnedUI = Instantiate(_BaseUI, transform);
		_SpawnedUI.GetComponentInChildren<S_UI_PauseControl>().Cam = _HedgeCamera;
		_BaseUIElements = _SpawnedUI.GetComponentInChildren<S_UI_PauseControl>().PassOnToSpawner;

		foreach(GameObject UI in _AdditionalUI)
		{
			Instantiate(UI, _SpawnedUI.transform);
		}

	}

}
