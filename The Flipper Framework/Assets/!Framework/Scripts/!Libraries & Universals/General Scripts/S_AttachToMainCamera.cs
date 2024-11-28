using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_AttachToMainCamera : MonoBehaviour
{
	void Awake () {
		Camera MainCamera = Camera.main;
		transform.parent = MainCamera.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
	}
}
