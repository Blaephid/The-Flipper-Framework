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
	public event        EventHandler onObjectValidate;

	public virtual void OnValidate () {
		if (onObjectValidate != null)
			onObjectValidate.Invoke(null, null);
		OnValidateOverride();
	}

	public virtual void OnValidateOverride () {

	}

#if UNITY_EDITOR
	[HideInInspector] public S_O_CustomInspectorStyle _InspectorTheme;
#endif
}
