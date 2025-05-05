using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SplineInParent : S_Data_Base
{
	[CustomReadOnly]
	public Spline _SplineParent;
	[CustomReadOnly]
	public S_PlaceOnSpline _Placer;
	[CustomReadOnly]
	public S_AddOnRail _ConnectedRails;

#if UNITY_EDITOR
	private new void OnValidate () {

		if (TryGetComponent(out S_PlaceOnSpline TempPlacer))
		{
			_Placer = TempPlacer;
			_SplineParent = _Placer._Spline;
			_ConnectedRails = _Placer.GetComponent<S_AddOnRail>();
		}
		else
		{
			enabled = false;
		}
	}
#endif
}
