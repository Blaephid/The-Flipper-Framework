using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsPlayer : MonoBehaviour {

	[SerializeField] float Distance;
	[SerializeField] float RotationSpeed;
	private Transform Target;

	// Use this for initialization
	void Start () {
		Target = GameObject.FindGameObjectWithTag ("Player").transform;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		Vector3 targetDir = Target.position - transform.position;
		float step = RotationSpeed * Time.deltaTime;
		Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);

		// Move our position a step closer to the target.
		if (Vector3.Distance (transform.position, Target.position) < Distance) {
			transform.rotation = Quaternion.LookRotation (newDir);
		}
	}
}
