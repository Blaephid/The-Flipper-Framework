using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_UpdateUIToNewCamera : MonoBehaviour
{
	public Canvas CanvasBoundToCamera;

	//Automatically connects to everytime a new scene loads.
	private void OnEnable () {
		SceneManager.sceneLoaded += EventAssignNewCamera;
	}

	private void OnDisable () {
		SceneManager.sceneLoaded -= EventAssignNewCamera;
	}

	public void EventAssignNewCamera (Scene scene, LoadSceneMode mode) {
		if (CanvasBoundToCamera != null)
		{
			//Using instanceID to find the lastest spawned camera, and assinging that to the overlay.
			Camera[] newCams = FindObjectsByType<Camera>(FindObjectsSortMode.InstanceID);
			CanvasBoundToCamera.worldCamera = newCams[newCams.Length - 1];

		}
	}
}
