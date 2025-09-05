using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_MatchMainCamera : MonoBehaviour
{
	Camera _ThisCamera;
	public Camera _MainCamera;

	private void Awake () {
		_ThisCamera = GetComponent<Camera>();
	}

	private void LateUpdate () {
		if(!_ThisCamera) { enabled = false; }

		_ThisCamera.fieldOfView = _MainCamera.fieldOfView;
		_ThisCamera.transform.position = _MainCamera.transform.position;
		_ThisCamera.transform.rotation = _MainCamera.transform.rotation;
	}
}
