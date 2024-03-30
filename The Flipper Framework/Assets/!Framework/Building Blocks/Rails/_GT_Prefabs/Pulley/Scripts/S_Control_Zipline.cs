using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System;

public class S_Control_Zipline : MonoBehaviour
{
	public Spline Rail;
	CapsuleCollider sphcol;
	[SerializeField]
	bool placeFromEnd;
	[SerializeField]
	float offset = 0.5f;
	public GameObject homingtgt;

	Rigidbody rb;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		Rail = GetComponentInParent<Spline>();
		sphcol = GetComponent<CapsuleCollider>();
		homingtgt = GetComponentInChildren<S_Data_HomingTarget>().gameObject;
		PlaceOnRope();
	}
	private void Update () {
		if (!sphcol.enabled)
		{
			if (homingtgt != null)
				homingtgt.SetActive(false);
			//this.enabled = false;            
		}

	}

	private void OnEnable () {
		S_Manager_LevelProgress.onReset += EventReturnToRope;
	}

	private void OnDisable () {
		S_Manager_LevelProgress.onReset -= EventReturnToRope;
	}


	void EventReturnToRope ( object sender, EventArgs e ) {
		//Debug.Log("Invoke Pulley");
		sphcol.enabled = true;
		homingtgt.SetActive(true);
		rb.isKinematic = true;
		rb.velocity = Vector3.zero;


		PlaceOnRope();
	}

	void PlaceOnRope () {
		CurveSample sample = (placeFromEnd) ? Rail.GetSampleAtDistance(Rail.Length - offset) : Rail.GetSampleAtDistance(1 + offset);
		transform.position =  Rail.transform.position + (Rail.transform.rotation * sample.location);

		Vector3 dir = Rail.transform.rotation * sample.tangent;
		transform.rotation = Quaternion.LookRotation(dir, Rail.transform.rotation * sample.up);
	}
}
