using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_CoreMethods 
{

	//A faster versopm of Vector3.Distance because it doesn't calculate the actual magnitude. Therefore, remember that anything you compare this to MUST be squared, as this wont give the square root.
   public static float GetDistanceOfVectors(Vector3 Vector1, Vector3 Vector2 ) {
		return (Vector1 - Vector2).sqrMagnitude;
	}
}
