using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[DisallowMultipleComponent]
[CanEditMultipleObjects]
#endif
public class S_Data_Base : S_Vis_Base
{
#if UNITY_EDITOR

	[HideInInspector] public bool _hasDataChanged;

	public event        EventHandler onObjectValidate;

	public virtual void OnValidate () {
		if (onObjectValidate != null)
			onObjectValidate.Invoke(null, null);
		_hasDataChanged = true;
	}
#endif
}
