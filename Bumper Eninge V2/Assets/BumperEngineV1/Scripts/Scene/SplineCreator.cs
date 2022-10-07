using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BezierSolution;

[ExecuteInEditMode]
public class SplineCreator : MonoBehaviour {
	[SerializeField]
	private BezierSpline Spline;
	[SerializeField]
	private GameObject Prefab;
	[SerializeField]
	float SpacingFactor = 1.5f;
	private float ObjectSize = 8f;
	[SerializeField]
	private List<GameObject> Objects;
	[SerializeField]
	private GameObject CloneHolder;
	private float LastLength = 0f;

	#if UNITY_EDITOR

	void Start () {
		// ~
	}

	void Update () {
		if (Objects == null || Spline == null)
			return;
		float NewLength = Spline.GetLengthApproximately (0, 1, 10);
		if (NewLength == LastLength)
			return;
		LastLength = NewLength;
		UpdateRings ();
	}

	private void UpdateRings () {
		float Step = ObjectSize * SpacingFactor;
		int NumberOfRings = 0;
		float RingProgress = -Step;
		Vector3 LastPosition = Vector3.positiveInfinity;
		while (RingProgress < 1f) {
			Vector3 NewPosition = Spline.MoveAlongSpline(ref RingProgress, Step);
			if ((LastPosition - NewPosition).magnitude < Step)
				continue;
			GameObject Ring;
			if (NumberOfRings < Objects.Count) {
				Ring = Objects [NumberOfRings];
			} else {
				Ring = Instantiate (Prefab);
				Ring.transform.SetParent (CloneHolder.transform);
				Objects.Add (Ring);
			}
			Ring.transform.position = NewPosition;
			LastPosition = NewPosition;
			NumberOfRings++;
		}
		for (int ii = NumberOfRings; ii < Objects.Count; ii++) {
			DestroyImmediate (Objects [ii]);
			Objects.RemoveAt (ii);
		}
	}

	#endif

}