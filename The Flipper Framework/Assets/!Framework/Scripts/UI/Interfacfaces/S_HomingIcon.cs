using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_HomingIcon : MonoBehaviour
{
	[NonSerialized] public bool _inPerfectTiming;

	public void AnimResetPerfectTiming () { _inPerfectTiming = false; }
	public void AnimStartPerfectTiming () { _inPerfectTiming = true; }
	public void AnimEndPerfectTiming() { _inPerfectTiming = false; }
}
