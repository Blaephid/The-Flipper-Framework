using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System;

public class S_Control_Zipline : MonoBehaviour
{
	[HideInInspector]
	public Spline _Rail;
	[HideInInspector]
	public GameObject _HomingTarget;

	[SerializeField]
	bool _shouldPlaceFromEnd;
	[SerializeField]
	float _offset = 0.5f;

	CapsuleCollider _CapsuleCollider;
	Rigidbody _RB;

	// Use this for initialization
	void Start () {
		//Set all object references automatically
		_RB = GetComponent<Rigidbody>();
		_Rail = GetComponentInParent<Spline>();
		_CapsuleCollider = GetComponent<CapsuleCollider>();
		_HomingTarget = GetComponentInChildren<S_Data_HomingTarget>().gameObject;

		//Ensure handle is at correct place along the spline
		PlaceOnRope();
	}
	private void Update () {
		if (!_CapsuleCollider.enabled)
		{
			if (_HomingTarget != null)
				_HomingTarget.SetActive(false);
			//this.enabled = false;            
		}

	}


	//Attaches to, and removes from, OnReset event so will always be where it should be when player dies.
	private void OnEnable () {
		S_Manager_LevelProgress.OnReset += EventReturnToRope;
	}

	private void OnDisable () {
		S_Manager_LevelProgress.OnReset -= EventReturnToRope;
	}

	//Reset handle to how it started
	void EventReturnToRope ( object sender, EventArgs e ) {
		_CapsuleCollider.enabled = true;
		_HomingTarget.SetActive(true);
		_RB.isKinematic = true;
		_RB.velocity = Vector3.zero;


		PlaceOnRope();
	}

	//Get correct transform in world space based on spline
	void PlaceOnRope () {
		CurveSample sample = (_shouldPlaceFromEnd) ? _Rail.GetSampleAtDistance(_Rail.Length - _offset) : _Rail.GetSampleAtDistance(1 + _offset);
		transform.position =  _Rail.transform.position + (_Rail.transform.rotation * sample.location);

		Vector3 dir = _Rail.transform.rotation * sample.tangent;
		transform.rotation = Quaternion.LookRotation(dir, _Rail.transform.rotation * sample.up);
	}
}
