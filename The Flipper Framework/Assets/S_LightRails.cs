using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_LightRails : MonoBehaviour, ITriggerable
{

	[SerializeField]
	private bool _startOnBlue = true;

	[ColourIfNull(.7f,0,0,1)]
	[SerializeField] private Spline _BlueRail;
	[ColourIfNull(.7f,0,0,1)]
	[SerializeField] private Spline _RedRail;

	private S_AddOnRail[] _BlueRailAddons;
	private S_AddOnRail[] _RedRailAddons;

	private bool _blueActive;

	private void Start () {
		_BlueRailAddons = _BlueRail.gameObject.GetComponentsInChildren<S_AddOnRail>(true);
		_RedRailAddons = _RedRail.gameObject.GetComponentsInChildren<S_AddOnRail>(true);

		//SetAddonRailsIfNull(ref _BlueRailAddons, ref _RedRailAddons);
		//SetAddonRailsIfNull(ref _RedRailAddons, ref _BlueRailAddons);

		if (!_startOnBlue)
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

	//Takes two arrays of addons, and applies the first next and prev to match what's now being assigned.
	private void SetExternalConnectedRails(ref S_AddOnRail[] SetAddonsFromThis, ref S_AddOnRail[] ToThis ) {
		for (int i = 0 ; i < SetAddonsFromThis.Length ; i++)
		{
			S_AddOnRail SetTo = ToThis[i];
			S_AddOnRail SetFrom = SetAddonsFromThis[i];
			if(!SetFrom) { return; }

			//Set up bools before asigning because if NextRail was set, then the next if would be true.
			bool currentlyHasPrev = SetFrom.PrevRail;
			bool currentlyHasNext = SetFrom.NextRail;

			if (currentlyHasPrev)
				SetFrom.PrevRail.useNextRail = SetTo;
			if(currentlyHasNext)
				SetFrom.NextRail.usePrevRail = SetTo;
		}
	}
}
