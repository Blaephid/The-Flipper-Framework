using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class CharacterSelect : MonoBehaviour {
	public GameObject DesiredCharacter;
	[SerializeField] TextMeshProUGUI InfoText;
	[SerializeField] GameObject GoButton;
	// Use this for initialization
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
		SceneManager.LoadScene ("StageSelect");
	}
	public void LoadPastScene()
	{
		if (GameObject.Find ("CharacterSelector") != null) {
			Destroy (GameObject.Find ("CharacterSelector"));
		}
		SceneManager.LoadScene ("LogoScreen");
	}
		



}
