using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_CoreMethods 
{

	//A faster versopm of Vector3.Distance because it doesn't calculate the actual magnitude. Therefore, remember that anything you compare this to MUST be squared, as this wont give the square root.
   public static float GetDistanceOfVectors(Vector3 Vector1, Vector3 Vector2 ) {
		return (Vector1 - Vector2).sqrMagnitude;
	}

	//Because normal Clamp Magnitude uses Square roots, call this instead to just limit the vector1 magnitude through simple comparisons.
	public static Vector3 ClampMagnitudeWithSquares ( Vector3 Vector1, float minimum, float maximum ) {
		if(Vector1.sqrMagnitude < Mathf.Pow(minimum, 2)) 
			return Vector1.normalized * minimum;
		else if (Vector1.sqrMagnitude < Mathf.Pow(maximum, 2))
			return Vector1.normalized * maximum;

		return Vector1;
	}
}
