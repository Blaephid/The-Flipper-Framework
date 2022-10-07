using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFrameRate : MonoBehaviour {
	[SerializeField] private int FrameRate;
	// Use this for initialization
	void Awake () {
		Application.targetFrameRate = FrameRate;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
