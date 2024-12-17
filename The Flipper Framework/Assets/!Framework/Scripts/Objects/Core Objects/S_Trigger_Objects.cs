using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_Trigger_Objects : S_Trigger_Base
{
 
}

[CustomEditor(typeof(S_Trigger_Objects))]
public class TriggerObjectEditor : TriggerEditor
{
	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _OwnerScript._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {

		DrawDefaultInspector();
	}
}
