using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System;
using System.Linq;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public class S_AddOnRail : MonoBehaviour
{
	[Header("Updating in Editor")]
	[AsButton("Update", "Place", null)]
	public bool UpdateAll = false;
	[AsButton("Update All", "UpdateAllInstances", null)]
	public bool UpdateNow = false;
	[AsButton("Set Values Of Connected Rails", "SetValueOfConnectedRails", null)]
	public bool SetValues = false;

	public S_AddOnRail[] AddThis = new S_AddOnRail[0];
	public S_AddOnRail[] AddBehindThese = new S_AddOnRail[0];

	[Header("Main Rails")]
	public S_AddOnRail nextRail;
	public S_AddOnRail PrevRail;

	[Header("Switch Rails")]
	public S_AddOnRail altNextRail;
	public S_AddOnRail altPrevRail;

	Vector3 offset;

#if UNITY_EDITOR

	private void OnEnable () {
		if (Application.isPlaying) return;
		Place();
	}
#endif

	public void SwitchRailsToAlternate () {
		if (altNextRail != null)
		{
			S_AddOnRail temp = nextRail;
			nextRail = altNextRail;
			altNextRail = temp;
		}
		if (altPrevRail != null)
		{
			S_AddOnRail temp = PrevRail;
			PrevRail = altPrevRail;
			altPrevRail = temp;
		}
	}

#if UNITY_EDITOR
	public void SetValueOfConnectedRails () {

		if (PrevRail != null)
			PrevRail.nextRail = this;

		if (nextRail != null)
			nextRail.PrevRail = this;

		if (altNextRail != null)
			altNextRail.PrevRail = this;

		if (altPrevRail != null)
			altPrevRail.nextRail = this;
	}

	public void UpdateAllInstances () {
		S_AddOnRail[] railsMeshes = FindObjectsByType<S_AddOnRail>(FindObjectsSortMode.None);
		for (int i = 0 ; i < railsMeshes.Length ; i++)
		{
			S_AddOnRail add = railsMeshes[i];
			if (add.AddThis.Length != 0)
			{
				add.Place();
			}
		}
	}

	public void Place () {
		offset = new Vector3(GetComponent<S_PlaceOnSpline>()._offset3d_.x, 0, 0);

		if (AddThis.Length > 0)
		{
			for (int i = 0 ; i < AddThis.Length ; i++)
			{
				S_AddOnRail rail = AddThis[i];
				Spline thisSpline = GetComponentInParent<Spline>();
				Spline otherSpline = rail.GetComponentInParent<Spline>();

				CurveSample sample = thisSpline.GetSampleAtDistance(thisSpline.Length);
				offset = sample.Rotation * offset;

				otherSpline.gameObject.transform.parent.position = thisSpline.gameObject.transform.parent.position + offset;
				otherSpline.nodes[0].Position = thisSpline.nodes[thisSpline.nodes.Count - 1].Position;
				otherSpline.nodes[0].Direction = thisSpline.nodes[thisSpline.nodes.Count - 1].Direction;
			}

			for (int i = 0 ; i < AddBehindThese.Length ; i++)
			{
				S_AddOnRail rail = AddBehindThese[i];
				Spline thisSpline = GetComponentInParent<Spline>();
				Spline otherSpline = rail.GetComponentInParent<Spline>();

				CurveSample sample = thisSpline.GetSampleAtDistance(thisSpline.Length);
				offset = sample.Rotation * offset;

				otherSpline.gameObject.transform.parent.position = thisSpline.gameObject.transform.parent.position + offset;
				otherSpline.nodes[otherSpline.nodes.Count - 1].Position = thisSpline.nodes[0].Position;
				otherSpline.nodes[otherSpline.nodes.Count - 1].Direction = thisSpline.nodes[0].Direction;
			}

		}
	}
#endif

}
