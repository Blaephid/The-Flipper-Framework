using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_AttachToMainCamera : MonoBehaviour
{
	[SerializeField] Vector3 _localOffset;
	[SerializeField] GameObject ActAsCameraObject;
	[AsButton("Go To Object","GoToFakeCameraObject", null)]
	[SerializeField] bool GoToObject;

	void Awake () {
		Camera MainCamera = Camera.main;
		transform.parent = MainCamera.transform;
		transform.localPosition = _localOffset;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}

#if UNITY_EDITOR
	[ExecuteInEditMode]
	public void GoToFakeCameraObject () {
		if (!Application.isPlaying && ActAsCameraObject)
		{
			transform.position = ActAsCameraObject.transform.position + (ActAsCameraObject.transform.rotation * _localOffset);
			transform.rotation = ActAsCameraObject.transform.rotation;
		}
	}
#endif
}
