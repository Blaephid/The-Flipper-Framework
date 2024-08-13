using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class S_Trigger_LoadLevelBeats : MonoBehaviour
{
    public GameObject[] SetSection;

    bool deSpawning = false;
    bool isSpawning = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //Debug.Log("Player Enters");
            StartCoroutine(DoSpawn());
        }
    }

    private void Awake()
    {
        StartCoroutine(deSpawn());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(deSpawn());
        }
    }

    IEnumerator deSpawn()
    {
        isSpawning = false;

        deSpawning = true;
		for (int i = 0 ; i < SetSection.Length ; i++)
		{
			GameObject section = SetSection[i];
			section.SetActive(false);
			yield return new WaitForFixedUpdate();

			if (!deSpawning) { yield break; }
		}

		deSpawning = false;
    }

    IEnumerator DoSpawn()
    {
        deSpawning = false;

        isSpawning = true;
        yield return new WaitForFixedUpdate();

		for (int i = 0 ; i < SetSection.Length ; i++)
		{
			SetSection[i].SetActive(true);
			yield return new WaitForFixedUpdate();

			if (!isSpawning) { yield break; }
		}

		isSpawning =false;
    }
}
