using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System;
using System.Linq;
using UnityEditor;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(S_PlaceOnSpline))]
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
	public S_AddOnRail NextRail;
	public S_AddOnRail PrevRail;

	[HideInInspector] public S_AddOnRail UseNextRail;
	[HideInInspector] public S_AddOnRail UsePrevRail;

	[Header("Switch Rails")]
	public S_AddOnRail AltNextRail;
	public S_AddOnRail AltPrevRail;

	Vector3 _selfOffset, _otherOffset;
	[SerializeField, HideInInspector]
	Vector3? _startPos, _endPos, _nextPos, _prevPos;

#if UNITY_EDITOR

	private void Start () {
		UseNextRail = NextRail;
		UsePrevRail = PrevRail;
	}

	private void OnEnable () {
		if (Application.isPlaying) return;
		Place();
	}
#endif

	public void SwitchRailsToAlternate () {
		if (AltNextRail != null)
		{
			UseNextRail = UseNextRail == NextRail ? AltNextRail : NextRail;
		}
		if (AltPrevRail != null)
		{
			UsePrevRail = UsePrevRail == PrevRail ? AltPrevRail : NextRail;
		}
	}

#if UNITY_EDITOR
	private void OnValidate () {
		GetPositions();
	}

	public void SetValueOfConnectedRails () {
		if(!this.isActiveAndEnabled) { return; }

		if (PrevRail != null)
		{
			PrevRail.NextRail = this; 
			PrevRail.GetPositions();
			EditorUtility.SetDirty(PrevRail);
		}

		if (NextRail != null)
		{
			NextRail.PrevRail = this; 
			NextRail.GetPositions();
			EditorUtility.SetDirty(NextRail);
		}

		if (AltNextRail != null)
		{
			AltNextRail.PrevRail = this; 
			PrevRail.GetPositions();
			EditorUtility.SetDirty(AltNextRail);
		}

		if (AltPrevRail != null)
		{
			AltPrevRail.NextRail = this; 
			NextRail.GetPositions();
			EditorUtility.SetDirty(AltPrevRail);
		}

		GetPositions();
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
		_selfOffset = new Vector3(GetComponent<S_PlaceOnSpline>()._offset3d_.x, 0, 0);

		if (AddThis.Length > 0)
		{
			for (int i = 0 ; i < AddThis.Length ; i++)
			{
				S_AddOnRail rail = AddThis[i];
				if(!rail) { continue; }

				Spline thisSpline = GetComponentInParent<Spline>();
				Spline otherSpline = rail.GetComponentInParent<Spline>();

				CurveSample sample = thisSpline.GetSampleAtDistance(thisSpline.Length);

				otherSpline.gameObject.transform.position = thisSpline.gameObject.transform.parent.position;
				otherSpline.nodes[0].Position = thisSpline.nodes[thisSpline.nodes.Count - 1].Position;
				otherSpline.nodes[0].Direction = thisSpline.nodes[thisSpline.nodes.Count - 1].Direction;
			}

			for (int i = 0 ; i < AddBehindThese.Length ; i++)
			{
				S_AddOnRail rail = AddBehindThese[i];
				Spline thisSpline = GetComponentInParent<Spline>();
				Spline otherSpline = rail.GetComponentInParent<Spline>();

				CurveSample sample = thisSpline.GetSampleAtDistance(thisSpline.Length);

				otherSpline.gameObject.transform.position = thisSpline.gameObject.transform.parent.position;
				otherSpline.nodes[otherSpline.nodes.Count - 1].Position = thisSpline.nodes[0].Position;
				otherSpline.nodes[otherSpline.nodes.Count - 1].Direction = thisSpline.nodes[0].Direction;
			}
		}

		GetPositions();
	}

	public void GetPositions () {
		Spline thisSpline = GetComponentInParent<Spline>();
		if(!thisSpline) { return; }

		_endPos = GetPositionFromSplineData(thisSpline, _selfOffset, thisSpline.Length);
		_startPos = GetPositionFromSplineData(thisSpline, _selfOffset, 0);

		if(NextRail)
		{
			Spline otherSpline = NextRail.GetComponentInParent<Spline>();
			_nextPos = GetPositionFromSplineData(otherSpline, _selfOffset, 0);
		}
		if (PrevRail)
		{
			Spline otherSpline = PrevRail.GetComponentInParent<Spline>();
			_prevPos = GetPositionFromSplineData(otherSpline, _selfOffset, otherSpline.Length);
		}
	}

	private Vector3? GetPositionFromSplineData(Spline spline, Vector3 offset, float point ) {
		if(!spline) { return null; }

		CurveSample sample = spline.GetSampleAtDistance(point);
		Spline.SampleTransforms sampleTransform = Spline.GetSampleTransformInfo(spline.transform, sample);

		Vector3 useOffset = sampleTransform.rotation * _selfOffset;
		return sampleTransform.location + useOffset;
	}

	private void OnDrawGizmosSelected () {

		if (NextRail)
		{
			Gizmos.color = Color.magenta;
			if(_endPos.HasValue)
				Gizmos.DrawWireCube(_endPos.Value + (Vector3.up * 2f), Vector3.one * 5);
			if(_nextPos.HasValue)
				Gizmos.DrawWireCube(_nextPos.Value + (Vector3.up * -2f), Vector3.one * 5);
		}
		if (PrevRail)
		{
			Gizmos.color = Color.magenta;
			if(_startPos.HasValue)
				Gizmos.DrawWireCube(_startPos.Value + (Vector3.up * 2f), Vector3.one * 5);
			if(_prevPos.HasValue)
				Gizmos.DrawWireCube(_prevPos.Value + (Vector3.up * -2f), Vector3.one * 5);
		}
	}
#endif

}
