using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class S_CarryAcrossScenes : MonoBehaviour
{
	public static EnumGameSceneTypes whatIsCurrentSceneType = EnumGameSceneTypes.None; 

	[Tooltip("Seperates different instances of this script. There can only be one case of an instance with an ID. EG, if two instances have the ID of '10', one is deleted")]
	public string ID = "";
	[Tooltip("If true, if an instance with this ID is found, destroy that, and replace it with this.")]
	public bool willReplaceOldInstancesWithThis;
	private S_CarryAcrossScenes Instance;

	public bool CarryAcrossMenuScenes = true;
	public bool CarryAcrossCharacterScenes = false;

	public enum EnumGameSceneTypes {
		Menus,
		Overworld,
		None
	}

	//Automatically connects to everytime a new scene loads.
	private void OnEnable () {
		SceneManager.sceneLoaded += EventCheckSceneType;
	}

	private void OnDisable () {
		SceneManager.sceneLoaded -= EventCheckSceneType;
	}

	void Awake () {
		//Since awake can be called multiple times, only do this the first time its called.
		if(!Instance)
		{
			Instance = this;

			//Checks for every other instance of this script, and if one already has this ID, destroys this object as that is already in control.
			S_CarryAcrossScenes[] Others = FindObjectsByType<S_CarryAcrossScenes>(FindObjectsSortMode.None);
			for (int i = 0 ; i < Others.Length ; i++)
			{
				S_CarryAcrossScenes Other = Others[i];
				if (Other.ID == ID && Other != this)
				{
					if (willReplaceOldInstancesWithThis)
					{
						Destroy(Other.gameObject);
						break;
					}

					else
					{
						Destroy(gameObject);
						return;
					}
				}
			}
			transform.parent = null;
			DontDestroyOnLoad(gameObject); //Prevents the object from being destroyed when entering a new scene.		
		}
	}

	//Automatically connected to the scene manager event, and checks if the scene type (set externally) allows this object to exist.
	public void EventCheckSceneType ( Scene scene, LoadSceneMode mode ) {
		if(whatIsCurrentSceneType == EnumGameSceneTypes.Overworld && !CarryAcrossCharacterScenes)
		{
			Destroy(gameObject);
		}
		else if (whatIsCurrentSceneType == EnumGameSceneTypes.Menus && !CarryAcrossMenuScenes)
		{
			Destroy(gameObject);
		}
	}
}
