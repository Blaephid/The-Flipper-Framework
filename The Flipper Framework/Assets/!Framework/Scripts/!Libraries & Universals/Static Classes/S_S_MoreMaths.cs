using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_S_MoreMaths
{

	//	VECTORS

	//A faster version of Vector3.Distance because it doesn't calculate the actual magnitude. Therefore, remember that anything you compare this to MUST be squared, as this wont give the square root.
	public static float GetDistanceSqrOfVectors ( Vector3 Vector1, Vector3 Vector2 ) {
		return (Vector1 - Vector2).sqrMagnitude;
	}

	public static Vector3 TreatAxisAsZeroForVector(Vector3 vector, float clamp ) {
		if (Mathf.Abs(vector.x) < clamp) vector.x = 0;
		if (Mathf.Abs(vector.y) < clamp) vector.y = 0;
		if (Mathf.Abs(vector.z) < clamp) vector.z = 0;

		return vector;
	}

	//Because normal Clamp Magnitude uses Square roots, call this instead to just limit the vector1 magnitude through simple comparisons.
	public static Vector3 ClampMagnitudeWithSquares ( Vector3 Vector1, float minimum, float maximum ) {
		if (Vector1.sqrMagnitude < Mathf.Pow(minimum, 2))
			return Vector1.normalized * minimum;
		else if (Vector1.sqrMagnitude < Mathf.Pow(maximum, 2))
			return Vector1.normalized * maximum;

		return Vector1;
	}

	public static Vector3 GetDirection ( Vector3 fromStart, Vector3 toEnd ) {
		return (toEnd - fromStart).normalized;
	}

	//	Comparisons

	//Takes an array of numbers and returns false if any are further than the threshold apart.
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

	//	FLOATS
	#region FLOATS

	public static float GetLargestOfVector ( Vector3 vector ) {
		return Mathf.Max(vector.x, Mathf.Max(vector.y, vector.z));
	}

	public static float GetAverageOfVector ( Vector3 vector ) {
		return ((vector.x + vector.y + vector.z) / 3);
	}

	public static float GetNumberAsIncrement ( float number, float increments ) {
		number = increments * (int)(number / increments);
		return number;
	}

	public static int DivideWhileRoundingUp ( float numerator, float denominator ) {
		float result = (numerator + denominator - 1) / denominator;

		return (int)result;
	}
	#endregion

	//	Time
	#region TIME

	public static Vector3 ConvertFloatTimeToMinutesVector ( float time ) {
		float minutes = (int)time / (int)60;
		float seconds = (int)time - minutes;
		float milliseconds = time - (minutes * seconds) - seconds;

		return new Vector3(minutes, seconds, milliseconds);
	}

	public static float ConvertVectorMinutesTimeToTotalTime ( Vector3 time ) {
		float totalTime = (time.x * 60) + time.y + (time.z / 100);

		return totalTime;
	}

	public static string DisplayIn2Digits ( int value ) {
		return value >= 10 ? value.ToString() : "0" + value.ToString();
	}
	#endregion
}
