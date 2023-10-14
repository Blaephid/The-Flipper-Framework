using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCharacter : MonoBehaviour {

	[SerializeField] private GameObject PlayerObject;

	// Use this for initialization
	void Awake () {

		StartCoroutine(Spawn());
	}
	
	IEnumerator Spawn()
    {
		if (GameObject.Find("CharacterSelector") != null)
		{
			PlayerObject = GameObject.Find("CharacterSelector").GetComponent<CharacterSelect>().DesiredCharacter;
		}
		GameObject Player = Instantiate(PlayerObject);
		Player.transform.position = transform.position;

		yield return null;

		Player.GetComponentInChildren<CharacterTools>().CharacterAnimator.transform.forward = transform.forward;
		//Player.transform.forward = transform.forward;
		if (GameObject.Find("CharacterSelector") != null)
		{
			Destroy(GameObject.Find("CharacterSelector"));
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
