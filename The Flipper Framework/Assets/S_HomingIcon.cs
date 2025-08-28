using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_HomingIcon : MonoBehaviour
{
	[NonSerialized] public bool _inPerfectTiming;

	public void AnimStartPerfectTiming () { _inPerfectTiming = true; }
	public void AnimEndPerfectTiming() { _inPerfectTiming = false; }
}
