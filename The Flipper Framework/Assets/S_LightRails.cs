using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_LightRails : MonoBehaviour, ITriggerable
{
	[ColourIfNull(.7f,0,0,1)]
	[SerializeField] private Spline _BlueRail;
	[ColourIfNull(.7f,0,0,1)]
	[SerializeField] private Spline _RedRail;

	private S_AddOnRail[] _BlueRailAddons;
	private S_AddOnRail[] _RedRailAddons;

	private bool _blueActive;

	private void Start () {
		_BlueRailAddons = _BlueRail.gameObject.GetComponentsInChildren<S_AddOnRail>();
		_RedRailAddons = _RedRail.gameObject.GetComponentsInChildren<S_AddOnRail>();

		SetAddonRailsIfNull(ref _BlueRailAddons, ref _RedRailAddons);
		SetAddonRailsIfNull(ref _RedRailAddons, ref _BlueRailAddons);

		SetBlueActive(true);
	}

	private void SetAddonRailsIfNull(ref S_AddOnRail[] RailsA, ref S_AddOnRail[] RailsB ) {
		for (int i = 0 ; i < RailsA.Length ; i++)
		{
			if (!RailsA[i].NextRail) { RailsA[i].NextRail = RailsB[i].NextRail; }
			if (!RailsA[i].PrevRail) { RailsA[i].PrevRail = RailsB[i].PrevRail; }
		}
	}

	public void TriggerObjectOn ( S_PlayerPhysics Player = null ) {
		if(!_BlueRail || !_RedRail) return;

		if (_blueActive)
		{
			SetBlueActive(false);
			SetExternalConnectedRails(ref _BlueRailAddons, ref _RedRailAddons);
		}
		else
		{
			SetBlueActive(true);
			SetExternalConnectedRails(ref _RedRailAddons, ref _BlueRailAddons);
		}
	}

	private void SetBlueActive(bool active ) {
		_blueActive = active;
		_BlueRail.gameObject.SetActive(active);
		_RedRail.gameObject.SetActive(!active);
	}

	private void SetExternalConnectedRails(ref S_AddOnRail[] SetAddonsFromThis, ref S_AddOnRail[] ToThis ) {
		for (int i = 0 ; i < SetAddonsFromThis.Length ; i++)
		{
			SetAddonsFromThis[i].PrevRail.NextRail = ToThis[i];
			SetAddonsFromThis[i].NextRail.PrevRail = ToThis[i];
		}
	}
}
