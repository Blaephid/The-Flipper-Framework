using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_S_MoreMathMethods 
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

	//Takes an array of numbers are returns false if any are further than the threshold apart.
	public static bool AreNumberCloseTogether ( float[] numbers, float threshold ) {

		for (int elementA = 0 ; elementA < numbers.Length ; elementA++)
		{
			//Go through every element after this element, and check the difference.
			for (int elementB = elementA + 1 ; elementB < numbers.Length ; elementB++)
			{
				if (Mathf.Abs(numbers[elementA] - numbers[elementB]) > threshold) return false;
			}
		}
		//If not a single comparison between every number was more than threshold, return true.
		return true;
	}

	public static float GetLargestOfVector (Vector3 vector) {
		return Mathf.Max(vector.x, Mathf.Max(vector.y, vector.z));
	}

	public static float GetNumberAsIncrement (float number, float increments) {
		number = increments * (int)(number / increments);
		return number;
	}
}
