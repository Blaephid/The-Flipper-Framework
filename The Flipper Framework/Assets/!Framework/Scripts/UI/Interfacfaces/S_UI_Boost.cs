using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class S_UI_Boost : MonoBehaviour
{
	public TextMeshProUGUI EnergyTracker;

	private void Awake () {
		GetComponentInParent<S_Spawn_UI>()._BoostUI = this;
	}
}
