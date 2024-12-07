using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
#if UNITY_EDITOR
using UnityEditor;


[CreateAssetMenu(fileName = "Custom Inspector Style")]
public class S_O_CustomInspectorStyle : ScriptableObject
{
	public float __spaceSize = 1;
	public GUIStyle _ResetButton;
	public GUIStyle _GeneralButton;
	public GUIStyle _MainHeaders;
	public GUIStyle _SubHeaders;
	public GUIStyle _ReplaceNormalHeaders;

}

[CustomEditor(typeof(S_O_CustomInspectorStyle))]
public class CustomInpsectorEditor : Editor
{
	//Draw and include resets
	public override void OnInspectorGUI () {

		

		S_O_CustomInspectorStyle Details = (S_O_CustomInspectorStyle)target;

		EditorGUILayout.PropertyField(serializedObject.FindProperty("__spaceSize"), new GUIContent("Size of Spaces"));
		serializedObject.ApplyModifiedProperties();

		//Default Main Header
		DrawLabel("_MainHeaders", "Main Headers", new GUIStyle(Details._MainHeaders));
		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject,"Reset", new GUIStyle(Details._ResetButton), Details, "Reset")) 
			{ Details._MainHeaders = new GUIStyle(GUI.skin.label); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		//Default Sub Header
		DrawLabel("_SubHeaders", "Sub-Headers", new GUIStyle(Details._SubHeaders));
		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject,"Reset", new GUIStyle(Details._ResetButton), Details, "Reset"))
			{ Details._SubHeaders = new GUIStyle(GUI.skin.label); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		//Default Normal Header
		DrawLabel("_ReplaceNormalHeaders", "Normal Headers", new GUIStyle(Details._ReplaceNormalHeaders));
		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject,"Reset", new GUIStyle(Details._ResetButton), Details, "Reset"))
			{ Details._ReplaceNormalHeaders = new GUIStyle(GUI.skin.label); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		//General Button
		DrawButton("_GeneralButton", "General Buton", new GUIStyle(Details._GeneralButton));
		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject,"Reset", new GUIStyle(Details._ResetButton), Details, "Reset"))
		{ Details._GeneralButton = new GUIStyle(GUI.skin.button); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		//reset Button
		DrawButton("_ResetButton", "Reset Button", new GUIStyle(Details._ResetButton));
		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject,"Reset", new GUIStyle(Details._ResetButton), Details, "Reset"))
			{ Details._ResetButton = new GUIStyle(GUI.skin.button); }
		serializedObject.ApplyModifiedProperties();
		GUILayout.EndHorizontal();
		//

		void DrawButton (string property, string outputName, GUIStyle inputStyle) {

			EditorGUILayout.Space(Details.__spaceSize);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
			GUILayout.BeginHorizontal();			
			S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject,"Example", new GUIStyle(inputStyle), null);		
		}

		void DrawLabel ( string property, string outputName, GUIStyle inputStyle ) {

			EditorGUILayout.Space(Details.__spaceSize);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Example", new GUIStyle(inputStyle));
		}

	}
	

	
}
#endif
