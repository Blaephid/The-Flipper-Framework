using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class S_RailHiding : MonoBehaviour
{

	public float Distance = 2000;
	S_SplineMeshTiling[] Rail;
	Transform Player;
	float _distanceTracker;
	bool active = true;


	// Use this for initialization
	void Start () {
		Distance *= Distance;
		//Player = GameObject.FindWithTag("Player").transform;
		Rail = GetComponentsInChildren<S_SplineMeshTiling>();

		Toggle(false);
	}

	// Update is called once per frame
	void Update () {
		if(S_SpawnCharacter._SpawnedPlayer == null) { return; }

		_distanceTracker = (S_SpawnCharacter._SpawnedPlayer.position - transform.position).sqrMagnitude;

		if (!active && _distanceTracker < Distance)
		{
			active = true;
			Toggle(true);
		}
		if (active && (_distanceTracker > Distance + 3))
		{
			active = false;
			Toggle(false);
		}


	}

	private void Toggle ( bool activate ) {
		active = activate;
		for (int s = 0 ; s < Rail.Length ; s++)
		{
			Rail[s].gameObject.SetActive(activate);
		}
	}




}
