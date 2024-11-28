using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class S_S_CustomInspectorMethods : Editor
{
	//Called whenever a property needs to be shown in the editor.
	public static void DrawEditableProperty ( SerializedObject serializedObject, string property, string outputName, bool isHorizontal = false ) {
		if (isHorizontal) GUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
		serializedObject.ApplyModifiedProperties();
	}

	//General button to check, return true if pressed, and update object.
	public static bool IsDrawnButtonPressed ( SerializedObject serializedObject, string buttonName, GUIStyle style, Object objectToUndo, string undoDescription = "" ) {

		if (objectToUndo)
		{
			Undo.RecordObject(objectToUndo, undoDescription);
		}

		//Add new element button.
		if (GUILayout.Button(buttonName, style))
		{
			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
			return true;
		}
		return false;
	}

	public static bool IsDrawnPropertyChanged ( SerializedObject serializedObject, string property, string outputName, bool isHorizontal = false ) {

		EditorGUI.BeginChangeCheck();
		EditorGUI.BeginChangeCheck();
		DrawEditableProperty(serializedObject, property, outputName, isHorizontal);

		if (EditorGUI.EndChangeCheck())
		{
			return true;
		}
		return false;
	}
}
