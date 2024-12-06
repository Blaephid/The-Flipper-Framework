using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class S_S_CustomInspectorMethods : Editor
{
	//Called whenever a property needs to be shown in the editor.
	public static void DrawEditableProperty ( SerializedObject serializedObject, string property, string outputName, bool isHorizontal = false, bool includeChildren = false ) {
		if (isHorizontal) GUILayout.BeginHorizontal();
		if(outputName != "")
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName), includeChildren);
		else
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), includeChildren);
		serializedObject.ApplyModifiedProperties();
	}

	//General button to check, return true if pressed, and update object.
	public static bool IsDrawnButtonPressed ( SerializedObject serializedObject, string buttonName, GUIStyle style, 
		UnityEngine.Object objectToUndo = null, string undoDescription = "" ) {

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

	//Called to set up lists in custom inspectors, using delegates to call methods that get the name right, and add on additional elements.
	//If wanting to draw the same as default inspectors, use DrawEditableProperty with includeChildren.
	public static void DrawListCustom ( SerializedObject serializedObject, string propertyName,
		GUIStyle _SmallButtonStyle, UnityEngine.Object objectToUndo,
		Action<int, SerializedProperty> MethodForPropertyNames, Action<int> MethodForDrawingInElementLine = null ) {

		//To display array elements seperately, need to get the array property first.
		SerializedProperty ListProperty = serializedObject.FindProperty(propertyName);

		if (ListProperty.arraySize == 0) { return; }

		for (int i = 0 ; i < ListProperty.arraySize ; i++)
		{
			//Get element in list
			SerializedProperty element = ListProperty.GetArrayElementAtIndex(i);
			GUILayout.BeginHorizontal();

			//Call back to the given method to ensure name is correct in list
			MethodForPropertyNames(i, element);

			//Button allowing a list element to be removed
			if (S_S_CustomInspectorMethods.IsDrawnButtonPressed
				(serializedObject, "Remove", _SmallButtonStyle, objectToUndo, "Remove State"))
			{
				ListProperty.DeleteArrayElementAtIndex(i);
				serializedObject.ApplyModifiedProperties();
			}

			//Call back to given method to add on any aditional Inspector Objects with the list element.
			MethodForDrawingInElementLine(i);

			serializedObject.ApplyModifiedProperties();
			GUILayout.EndHorizontal();
		}
	}
}
