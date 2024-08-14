using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class S_Trigger_LoadLevelBeats : MonoBehaviour
{
	//Tracking objects to enable
	private S_Trigger_LoadLevelBeats[]	_ListOfAllTriggers;
	public GameObject[]			_ListOfSectionToControl;

	//
	[HideInInspector] 
	public bool	_isActive;
	private bool	_isCurrentlyDespawning = false;
	private bool	_isCurrentlySpawning = false;

	//When player enters, start activating all required items.
	private void OnTriggerEnter ( Collider other ) {
		if (other.CompareTag("Player"))
		{
			StartCoroutine(EnableOrDisableObjects(true, GetAvailableObjects(_ListOfSectionToControl)));
		}
	}

	private void Awake () {
		_ListOfAllTriggers = FindObjectsByType<S_Trigger_LoadLevelBeats>(FindObjectsSortMode.None);
		
		_isActive = true; //Sets this to true so GetAvailableObjects wont check every trigger more than once. Set to False after the below coroutine.
		StartCoroutine(EnableOrDisableObjects(false, GetAvailableObjects(_ListOfSectionToControl), false));
	}

	private void OnTriggerExit ( Collider other ) {
		if (other.CompareTag("Player"))
		{
			StartCoroutine(EnableOrDisableObjects(false, GetAvailableObjects(_ListOfSectionToControl)));
		}
	}

	List<GameObject> GetAvailableObjects ( GameObject[] SetObjects ) {

		List<GameObject> List = new List<GameObject>();
		for (int i = 0 ; i < SetObjects.Length ; i++)
		{
			GameObject SectionObject = SetObjects[i];

			//Detects of this current section is part of another trigger that is currently active.
			bool isObjectActiveByAnotherTrigger = false;
			for (int t = 0 ; t < _ListOfAllTriggers.Length && !isObjectActiveByAnotherTrigger ; t++)
			{
				S_Trigger_LoadLevelBeats TriggerToCheck = _ListOfAllTriggers[t];
				if (TriggerToCheck != null & TriggerToCheck != this && TriggerToCheck._isActive)
				{
					if (TriggerToCheck._ListOfSectionToControl.Contains(SectionObject))
					{
						isObjectActiveByAnotherTrigger = true;
					}
				}
			}

			//If there isnt another active trigger handling this section, set it to be enabled or disabled now.
			if (!isObjectActiveByAnotherTrigger)
			{
				List.Add(SectionObject);
			}
		}
		return List;
	}

	//Goes through each section object and 
	IEnumerator EnableOrDisableObjects (bool enable, List<GameObject> EnableThese, bool delay = true) {

		yield return new WaitForFixedUpdate();

		_isActive = enable;

		_isCurrentlyDespawning = !enable;
		_isCurrentlySpawning = enable;

		//Once per frame checks a section object and enables or disables it.
		for (int i = 0 ; i < EnableThese.Count ; i++)
		{
			EnableThese[i].SetActive(enable);
			if (delay)
			{
				yield return new WaitForFixedUpdate();
			}

			//If this coroutine was called again to do the opposite (E.G. deactivating objects when this is still activating, or vice versa), then end this coroutine run.
			if (!_isCurrentlySpawning && enable) { yield return null; }
			else if (!_isCurrentlyDespawning && !enable) { yield return null; }
		}

		_isCurrentlySpawning = false;
		_isCurrentlyDespawning = false;

		////Only set this trigger to inactive after despawning all objects.
		//if (!enable) _isActive = false;
	}
}
