using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_S_ObjectMethods
{
	//Takes a transform and returns returns a local transform that is equal in world space.
	public static Vector3 LockScale ( Transform transform, float lockTo = 0 ) {
		Vector3 parentScale = transform.parent ? transform.parent.lossyScale : Vector3.one;

		Vector3 worldScale = transform.lossyScale;
		float averageScale = (Mathf.Abs(worldScale.x) + Mathf.Abs(worldScale.y) + Mathf.Abs(worldScale.z)) / 3;
		averageScale = lockTo > 0 ? lockTo : averageScale;

		Vector3 localScale = new Vector3(averageScale / parentScale.x,averageScale / parentScale.y,averageScale / parentScale.z);
		//localScale *= modifier;
		return localScale;
	}
}
