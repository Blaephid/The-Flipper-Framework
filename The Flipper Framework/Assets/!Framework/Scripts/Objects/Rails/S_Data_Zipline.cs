using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System;

public class S_Data_Zipline : S_Data_Base
{
	[CustomReadOnly]
	public Spline _Rail;
	[HideInInspector]

	[SerializeField]
	bool _shouldPlaceFromEnd;
	[SerializeField]
	float _offset = 0.5f;

	[Header("Components")]
	[ColourIfNull(0.8f,0,0,1)]public Transform _HandGripPoint;
	[ColourIfNull(0.8f,0,0,1)]public Transform _HandlePivot;
	[ColourIfNull(0.8f,0,0,1)]public Rigidbody _RB;
	[ColourIfNull(0.8f,0,0,1)]public GameObject _HomingTarget;
	[ColourIfNull(0.8f,0,0,1)]public CapsuleCollider _CapsuleCollider;

	// Use this for initialization
	void Start () {
		//Set all object references automatically
		_RB = GetComponent<Rigidbody>();

		//Ensure handle is at correct place along the spline
		PlaceOnRope();
	}

#if UNITY_EDITOR
	private new void OnValidate () {
		base.OnValidate();
		_Rail = GetComponentInParent<Spline>();
	}
#endif

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
		S_Manager_LevelProgress.OnReset += EventZiplineOnReset;
	}

	private void OnDisable () {
		S_Manager_LevelProgress.OnReset -= EventZiplineOnReset;
	}

	//Reset handle to how it started
	void EventZiplineOnReset ( object sender, EventArgs e ) {
		_CapsuleCollider.enabled = true;
		_HomingTarget.SetActive(true);
		_RB.isKinematic = true;
		_RB.linearVelocity = Vector3.zero;


		PlaceOnRope();
	}

	//Get correct transform in world space based on spline
	void PlaceOnRope () {
		CurveSample sample = (_shouldPlaceFromEnd) ? _Rail.GetSampleAtDistance(_Rail.Length - _offset) : _Rail.GetSampleAtDistance(1 + _offset);
		Spline.SampleTransforms sampleTransform = Spline.GetSampleTransformInfo(_Rail.transform, sample);

		transform.position = sampleTransform.location;
		transform.rotation = sampleTransform.rotation;
	}
}
