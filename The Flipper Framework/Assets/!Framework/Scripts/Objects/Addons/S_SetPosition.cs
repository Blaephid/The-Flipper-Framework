using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SetPosition : MonoBehaviour
{
	public Transform _TransformForPosition;
	public Transform _TransformForRotation;
	[SerializeField] Vector3 _localOffset;

	// Update is called once per frame
	void LateUpdate () {
		if (_TransformForPosition != null)
			transform.position = _TransformForPosition.position + _TransformForPosition.rotation * _localOffset;
		if (_TransformForRotation != null)
			transform.rotation = _TransformForRotation.rotation;
	}
}
