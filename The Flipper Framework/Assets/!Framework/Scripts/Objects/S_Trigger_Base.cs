using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Trigger_Base : MonoBehaviour
{

	public ITriggerable[] _ObjectsToTrigger;
	private void OnTriggerEnter ( Collider other ) {


		for (int i = 0 ; i < _ObjectsToTrigger.Length ; i++)
		{
			_ObjectsToTrigger[i].TriggerObjectOn();
		}
	}
}
