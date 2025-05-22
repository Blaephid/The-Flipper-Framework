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

		//Check for spline on this object.
		if (GetSplineFromObject(gameObject))
			return;

		//If no spline, see if there's a parent.
		if (transform.parent != null)
		{
			//Check parent for spline
			if (GetSplineFromObject(transform.parent.gameObject))
				return;
			else
			{
				//if still no spline, check if parent has a parent.
				if (transform.parent.parent != null)
					if (GetSplineFromObject(transform.parent.parent.gameObject))
						return;
			}
		}

		//If no spline was found (any of the above was false), disable. 
		enabled = false;

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
