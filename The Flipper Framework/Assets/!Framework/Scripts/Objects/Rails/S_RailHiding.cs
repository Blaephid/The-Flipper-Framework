using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class S_RailHiding : MonoBehaviour
{

	public float Distance = 2000;
	[CustomReadOnly]
	public S_SplineMeshTiling[] _Rail;
	[CustomReadOnly]
	public Spline _Spline;
	Transform Player;
	float _distanceTracker;
	bool active = true;


	// Use this for initialization
	void Start () {
		Distance *= Distance;

		Toggle(false);
	}

	private void OnValidate () {
		_Rail = GetComponentsInChildren<S_SplineMeshTiling>();
		if(!gameObject.TryGetComponent(out _Spline))
		{
			_Spline = GetComponentInParent<Spline>();
		}

		if(!_Spline) enabled = false;
	}

	// Update is called once per frame
	void Update () {
		if(S_SpawnCharacter._SpawnedPlayer == null) { return; }

		Vector3 thisPosition = transform.position + (transform.rotation * _Spline.nodes[0].Position);
		_distanceTracker = S_S_MoreMaths.GetDistanceSqrOfVectors(S_SpawnCharacter._SpawnedPlayer.position, thisPosition);

		if (!active && _distanceTracker < Distance * S_SpawnCharacter._spawnCheckModifier)
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
		//gameObject.SetActive(activate);
		for (int i = 0 ; i < transform.childCount ; i++)
		{
			transform.GetChild(i).gameObject.SetActive( activate );
		}

		active = activate;
		for (int s = 0 ; s < _Rail.Length ; s++)
		{
			_Rail[s].gameObject.SetActive(activate);
		}
	}




}
