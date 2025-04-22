using UnityEngine;
using SplineMesh;
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
	public S_AddOnRail[] AddThisBehind = new S_AddOnRail[0];

	[Header("Main Rails")]
	public S_AddOnRail NextRail;
	public S_AddOnRail PrevRail;

	[CustomReadOnly] public S_AddOnRail useNextRail;
	[CustomReadOnly] public S_AddOnRail usePrevRail;

	Vector3 _selfOffset, _otherOffset;
	[SerializeField, HideInInspector]
	Vector3? _startPos, _endPos, _nextPos, _prevPos;



	private void Start () {
		useNextRail = NextRail;
		usePrevRail = PrevRail;
	}

#if UNITY_EDITOR
	private void OnEnable () {
		if (Application.isPlaying) return;
		Place();
	}
#endif


#if UNITY_EDITOR
	private void OnValidate () {
		GetPositions();
	}

	public void SetValueOfConnectedRails () {
		if (!this.isActiveAndEnabled) { return; }

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

		_selfOffset = new Vector3(GetComponent<S_PlaceOnSpline>()._mainOffset.x, 0, 0);

		for (int i = 0 ; i < AddThis.Length ; i++)
		{
			S_AddOnRail rail = AddThis[i];
			if (!rail) { continue; }

			Spline thisSpline = GetComponentInParent<Spline>();
			Spline otherSpline = rail.GetComponentInParent<Spline>();

			if (!otherSpline || !thisSpline) { continue; }

			CurveSample sample = thisSpline.GetSampleAtDistance(thisSpline.Length);
			SetTransforms(thisSpline, otherSpline, 0, thisSpline.nodes.Count - 1);
		}

		for (int i = 0 ; i < AddThisBehind.Length ; i++)
		{

			S_AddOnRail rail = AddThisBehind[i];
			Spline thisSpline = GetComponentInParent<Spline>();
			Spline otherSpline = rail.GetComponentInParent<Spline>();

			if (!otherSpline || !thisSpline) { continue; }

			CurveSample sample = thisSpline.GetSampleAtDistance(thisSpline.Length);

			SetTransforms(thisSpline, otherSpline, otherSpline.nodes.Count - 1, 0);
		}

		GetPositions();

		return;

		void SetTransforms ( Spline thisSpline, Spline otherSpline, int node1, int node2 ) {
			otherSpline.transform.position = thisSpline.transform.position;
			otherSpline.transform.rotation = thisSpline.gameObject.transform.rotation;
			//otherSpline.gameObject.transform.localPosition = Vector3.zero;

			otherSpline.nodes[node1].Position = thisSpline.nodes[node2].Position;
			otherSpline.nodes[node1].Direction = thisSpline.nodes[node2].Direction;
			otherSpline.nodes[node1].Up = thisSpline.nodes[node2].Up;
		}
	}

	public void GetPositions () {

		if (Application.isPlaying) return;

		Spline thisSpline = GetComponentInParent<Spline>();
		if (!thisSpline) { return; }

		_endPos = GetPositionFromSplineData(thisSpline, _selfOffset, thisSpline.Length - 10);
		_startPos = GetPositionFromSplineData(thisSpline, _selfOffset, 10);

		if (NextRail)
		{
			Spline otherSpline = NextRail.GetComponentInParent<Spline>();
			if (!otherSpline) { return; }
			_nextPos = GetPositionFromSplineData(otherSpline, _selfOffset, 10);
		}
		if (PrevRail)
		{
			Spline otherSpline = PrevRail.GetComponentInParent<Spline>();
			if (!otherSpline) { return; }
			_prevPos = GetPositionFromSplineData(otherSpline, _selfOffset, otherSpline.Length - 10);
		}
	}

	private Vector3? GetPositionFromSplineData ( Spline spline, Vector3 offset, float point ) {
		if (!spline) { return null; }

		CurveSample sample = spline.GetSampleAtDistance(point);
		Spline.SampleTransforms sampleTransform = Spline.GetSampleTransformInfo(spline.transform, sample);

		Vector3 useOffset = sampleTransform.rotation * _selfOffset;
		return sampleTransform.location + useOffset;
	}

	private void OnDrawGizmosSelected () {

		if (NextRail)
		{
			Gizmos.color = Color.magenta;
			if (_endPos.HasValue)
				DrawBothCubesAt(_endPos);
			if (_nextPos.HasValue)
				DrawBothCubesAt(_nextPos);
		}
		if (PrevRail)
		{
			Gizmos.color = Color.magenta;
			if (_startPos.HasValue)
				DrawBothCubesAt(_startPos);
			if (_prevPos.HasValue)
				DrawBothCubesAt(_prevPos);
		}

		return;

		void DrawBothCubesAt ( Vector3? position ) {
			Gizmos.DrawWireCube(position.Value, Vector3.one * 9);
			Gizmos.DrawWireCube(position.Value, Vector3.one * 6);
		}
	}
#endif

}
