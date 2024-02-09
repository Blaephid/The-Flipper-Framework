using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
#if UNITY_EDITOR
using UnityEditor;


[CreateAssetMenu(fileName = "Custom Inspector Style")]
public class S_O_CustomInspectorStyle : ScriptableObject
{
	public GUIStyle _DefaultButton;
	public GUIStyle _MainHeaders;
	public GUIStyle _SubHeaders;

}

[CustomEditor(typeof(S_O_CustomInspectorStyle))]
public class CustomInpsectorEditor : Editor
{

	public override void OnInspectorGUI () {

		//DrawDefaultInspector();

		S_O_CustomInspectorStyle Details = (S_O_CustomInspectorStyle)target;

		//Default Button
		DrawLabel("_MainHeaders", "Main Headers", new GUIStyle(Details._MainHeaders));
		Undo.RecordObject(Details, "Reset");
		if (GUILayout.Button("Reset", new GUIStyle(Details._DefaultButton))) 
			{ Details._MainHeaders = new GUIStyle(GUI.skin.label); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		//Default Button
		DrawLabel("_SubHeaders", "Sub-Headers", new GUIStyle(Details._SubHeaders));
		Undo.RecordObject(Details, "Reset");
		if (GUILayout.Button("Reset", new GUIStyle(Details._DefaultButton)))
			{ Details._SubHeaders = new GUIStyle(GUI.skin.label); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		//Default Button
		DrawButton("_DefaultButton", "Default Button", new GUIStyle(Details._DefaultButton));
		Undo.RecordObject(Details, "Reset");
		if (GUILayout.Button("Reset", new GUIStyle(Details._DefaultButton)))
			{ Details._DefaultButton = new GUIStyle(GUI.skin.button); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		void DrawButton (string property, string outputName, GUIStyle inputStyle) {

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
			GUILayout.BeginHorizontal();			
			GUILayout.Button("Example", new GUIStyle(inputStyle));		
		}

		void DrawLabel ( string property, string outputName, GUIStyle inputStyle ) {

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Example", new GUIStyle(inputStyle));
		}

	}
	

	
}
#endif
