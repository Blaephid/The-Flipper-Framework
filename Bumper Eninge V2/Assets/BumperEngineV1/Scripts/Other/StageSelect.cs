using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class StageSelect : MonoBehaviour {

	[SerializeField] private string CurrentStage;
	[SerializeField] TextMeshProUGUI InfoText;
	[SerializeField] List<GameObject> ObjectsToActivateOnSelect;
	[SerializeField] List<GameObject> ObjectsToActivateOnGo;

	public void SetStage(string StageName)
	{
		CurrentStage = StageName;
		StartCoroutine (SetStageRoutine());
	}

	private IEnumerator SetStageRoutine()
	{

		for (int i = 0; i < ObjectsToActivateOnSelect.Count; i++) 
		{
			ObjectsToActivateOnSelect[i].SetActive (true);
		}

		yield return null;
	}

	public void InfoTextUpdate(Text Info)
	{
		InfoText.text = Info.text;
	}

	public void LoadStage()
	{
		StartCoroutine (BeginLoad());

	}

	public void LoadPastScene()
	{
		if (GameObject.Find ("CharacterSelector") != null) {
			Destroy (GameObject.Find ("CharacterSelector"));
		}
		SceneManager.LoadScene ("CharacterSelect");
	}

	private IEnumerator BeginLoad()
	{

		for (int i = 0; i < ObjectsToActivateOnGo.Count; i++) 
		{
			ObjectsToActivateOnGo[i].SetActive (true);
		}
		SceneManager.LoadSceneAsync (CurrentStage);

		yield return null;
	}
}
