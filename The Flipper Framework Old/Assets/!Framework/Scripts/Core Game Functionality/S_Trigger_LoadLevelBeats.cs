using System.Collections;
using System.Collections.Generic;
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
            StartCoroutine(doSpawn());
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
        foreach(GameObject section in SetSection)
        {
            section.SetActive(false);
            yield return new WaitForFixedUpdate();

            if (!deSpawning) { yield break; }
        }

        deSpawning = false;
    }

    IEnumerator doSpawn()
    {
        deSpawning = false;

        isSpawning = true;
        yield return new WaitForFixedUpdate();

        foreach (GameObject section in SetSection)
        {
            section.SetActive(true);
            yield return new WaitForFixedUpdate();

            if (!isSpawning) { yield break; }
        }

        isSpawning=false;
    }
}
