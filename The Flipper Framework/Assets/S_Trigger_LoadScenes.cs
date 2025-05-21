using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_Trigger_LoadScenes : S_Trigger_Base
{
	[SerializeField] SceneField SceneToLoad;
	[SerializeField] SceneField SceneToUnLoad;
	[SerializeField] bool _immediate = false;

	//When player enters, start activating all required items.
	private void OnTriggerEnter ( Collider other ) {
		if (!other.CompareTag("Player")) { return; }

		Load();
		Unload();
	}

	private void Load () {
		if (SceneToLoad != null)
		{
			var a = SceneManager.GetSceneByName(SceneToLoad);
			if (a.IsValid()) { return; }
			if (!_immediate)
				SceneManager.LoadSceneAsync(SceneToLoad, LoadSceneMode.Additive);
			else
				SceneManager.LoadScene(SceneToLoad, LoadSceneMode.Additive);
		}
	}

	private void Unload () {
		if (SceneToUnLoad != null)
		{
			for (int currentScenes = 0 ; currentScenes < SceneManager.sceneCount ; currentScenes++)
			{
				Scene loadedScene = SceneManager.GetSceneAt(currentScenes);
				if (loadedScene.name == SceneToUnLoad.SceneName)
				{
					SceneManager.UnloadSceneAsync(SceneToUnLoad);
				}
			}
		}
	}
}
