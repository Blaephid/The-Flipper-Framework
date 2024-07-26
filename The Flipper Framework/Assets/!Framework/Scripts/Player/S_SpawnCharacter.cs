using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SpawnCharacter : MonoBehaviour {

	[SerializeField] private GameObject PlayerObject;

	// Use this for initialization
	void Awake () {

		StartCoroutine(Spawn());
	}
	
	IEnumerator Spawn()
    {
		if (GameObject.Find("CharacterSelector") != null)
		{
			PlayerObject = GameObject.Find("CharacterSelector").GetComponent<S_CharacterSelect>().DesiredCharacter;
		}
		GameObject Player = Instantiate(PlayerObject, transform.position, Quaternion.identity, transform);

		yield return null;
	}

	// Update is called once per frame
	void Update () {
		
	}
}
