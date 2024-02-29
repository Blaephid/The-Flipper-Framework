using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
#if UNITY_EDITOR
using UnityEditor;


[CreateAssetMenu(fileName = "Custom Inspector Style")]
public class S_O_CustomInspectorStyle : ScriptableObject
{
	public float _spaceSize = 1;
	public GUIStyle _ResetButton;
	public GUIStyle _GeneralButton;
	public GUIStyle _MainHeaders;
	public GUIStyle _SubHeaders;

}

[CustomEditor(typeof(S_O_CustomInspectorStyle))]
public class CustomInpsectorEditor : Editor
{

	public override void OnInspectorGUI () {

		

		S_O_CustomInspectorStyle Details = (S_O_CustomInspectorStyle)target;

		EditorGUILayout.PropertyField(serializedObject.FindProperty("_spaceSize"), new GUIContent("Size of Spaces"));
		serializedObject.ApplyModifiedProperties();

		//Default Button
		DrawLabel("_MainHeaders", "Main Headers", new GUIStyle(Details._MainHeaders));
		Undo.RecordObject(Details, "Reset");
		if (GUILayout.Button("Reset", new GUIStyle(Details._ResetButton))) 
			{ Details._MainHeaders = new GUIStyle(GUI.skin.label); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		//Default Button
		DrawLabel("_SubHeaders", "Sub-Headers", new GUIStyle(Details._SubHeaders));
		Undo.RecordObject(Details, "Reset");
		if (GUILayout.Button("Reset", new GUIStyle(Details._ResetButton)))
			{ Details._SubHeaders = new GUIStyle(GUI.skin.label); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		//General Button
		DrawButton("_GeneralButton", "General Buton", new GUIStyle(Details._GeneralButton));
		Undo.RecordObject(Details, "Reset");
		if (GUILayout.Button("Reset", new GUIStyle(Details._ResetButton)))
		{ Details._GeneralButton = new GUIStyle(GUI.skin.button); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		//reset Button
		DrawButton("_ResetButton", "Reset Button", new GUIStyle(Details._ResetButton));
		Undo.RecordObject(Details, "Reset");
		if (GUILayout.Button("Reset", new GUIStyle(Details._ResetButton)))
			{ Details._ResetButton = new GUIStyle(GUI.skin.button); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		void DrawButton (string property, string outputName, GUIStyle inputStyle) {

			EditorGUILayout.Space(Details._spaceSize);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
			GUILayout.BeginHorizontal();			
			GUILayout.Button("Example", new GUIStyle(inputStyle));		
		}

		void DrawLabel ( string property, string outputName, GUIStyle inputStyle ) {

			EditorGUILayout.Space(Details._spaceSize);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Example", new GUIStyle(inputStyle));
		}

	}
	

	
}
#endif
