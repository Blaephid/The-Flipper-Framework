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


	private new void OnValidate () {
		base.OnValidate();
		if (!gameObject) { return; }

		if (!GetSplineFromObject(gameObject))
		{
			if (transform.parent != null && GetSplineFromObject(transform.parent.gameObject))
				return;
			else
			{
				if (transform.parent.parent != null && GetSplineFromObject(transform.parent.parent.gameObject))
					return;
				else
					enabled = false;
			}
		}
	}

	bool GetSplineFromObject ( GameObject GO ) {
		if (GO.TryGetComponent(out S_PlaceOnSpline TempPlacer))
		{
			_Placer = TempPlacer;
			_SplineParent = _Placer._Spline;
			_ConnectedRails = _Placer.GetComponent<S_AddOnRail>();
			return true;
		}
		else
			return false;
	}

}
