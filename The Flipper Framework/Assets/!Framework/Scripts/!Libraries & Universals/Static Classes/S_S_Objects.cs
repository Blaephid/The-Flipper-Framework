using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_S_Objects
{
	//Takes a transform and returns returns a local transform that is equal in world space.
	//NOTE - This only works if the transform is the same rotation as its parent. If you want to rotate, only rotate children of this, as their scaling will be set back to a factor of Vector3.one
	public static Vector3 LockScale ( Transform transform, float lockTo = 0 ) {
		Vector3 parentScale = transform.parent ? transform.parent.lossyScale : Vector3.one;

		Vector3 worldScale = transform.lossyScale;
		Vector3 localScale = transform.localScale;
		float averageScale = (Mathf.Abs(worldScale.x) + Mathf.Abs(worldScale.y) + Mathf.Abs(worldScale.z)) / 3;
		averageScale = lockTo > 0 ? lockTo : averageScale;

		//Applies inverse scale to parents world scale, esentially resetting scale to one for its children. This only works if the object has no local rotation.
		Vector3 newLocalScale = new Vector3(averageScale  / parentScale.x
			,averageScale / parentScale.y
			,averageScale / parentScale.z);
		return newLocalScale;
	}
}
