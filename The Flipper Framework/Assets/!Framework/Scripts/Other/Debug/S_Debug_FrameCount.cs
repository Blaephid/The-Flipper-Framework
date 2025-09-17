using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Debug_FrameCount : MonoBehaviour
{
	[SerializeField] private int _maxFrameCount = -1;

	private int _frameCount;

	private void FixedUpdate () {
		if(_frameCount >= _maxFrameCount && _maxFrameCount != -1) { enabled = false; return; }

		_frameCount++;

		Debug.Log("Frames = " + _frameCount + " On " +gameObject);
	}
}
