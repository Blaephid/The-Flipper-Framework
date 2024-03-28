using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class S_CharacterSelect : MonoBehaviour {
	public GameObject DesiredCharacter;
	[SerializeField] TextMeshProUGUI InfoText;
	[SerializeField] GameObject GoButton;
	// Use this for initialization

	[SerializeField] string nextScene = "Sc_StageSelect";
    [SerializeField] string lastScene = "Sc_LogoScreen";
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SwitchCharacter(GameObject Character)
	{
		DesiredCharacter = Character;
		if (GoButton.activeSelf != true) 
		{
			GoButton.SetActive (true);
		}
	}

	public void InfoTextUpdate(Text Info)
	{
		InfoText.text = Info.text;
	}
	public void LoadNextScene()
	{
		DontDestroyOnLoad (transform.root.gameObject);
		SceneManager.LoadScene (nextScene);
	}
	public void LoadPastScene()
	{
		if (GameObject.Find ("CharacterSelector") != null) {
			Destroy (GameObject.Find ("CharacterSelector"));
		}
		SceneManager.LoadScene (lastScene);
	}
		



}
