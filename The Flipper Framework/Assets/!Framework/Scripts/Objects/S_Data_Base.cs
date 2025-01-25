using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class S_Data_Base : MonoBehaviour
{
	public event        EventHandler onObjectValidate;

	public virtual void OnValidate () {
		if (onObjectValidate != null)
			onObjectValidate.Invoke(null, null);
	}

#if UNITY_EDITOR
	[HideInInspector] public S_O_CustomInspectorStyle _InspectorTheme;
#endif
}
