using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_S_ObjectMethods
{
	//Takes a transform and returns returns a local transform that is equal in world space.
	public static Vector3 LockScale ( Transform transform ) {
		Vector3 parentScale = transform.parent.lossyScale;

		Vector3 worldScale = transform.lossyScale;
		float averageScale = (Mathf.Abs(worldScale.x) + Mathf.Abs(worldScale.y) + Mathf.Abs(worldScale.z)) / 3;

		Vector3 localScale = new Vector3(averageScale / parentScale.x,averageScale / parentScale.y,averageScale / parentScale.z);
		return localScale;
	}
}
