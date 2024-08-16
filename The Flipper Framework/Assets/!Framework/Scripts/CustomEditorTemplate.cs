using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace templates
{

	public class MainScript : ScriptableObject
	{
#if UNITY_EDITOR
		public S_O_CustomInspectorStyle InspectorTheme;
#endif
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(MainScript))]
	public class MainScriptEditor : Editor
	{
		MainScript Owner;
		GUIStyle headerStyle;
		GUIStyle ResetToDefaultButton;
		float spaceSize;

		public override void OnInspectorGUI () {
			DrawInspector();
		}
		private void OnEnable () {
			//Setting variables
			Owner = (MainScript)target;

			if (Owner.InspectorTheme == null) { return; }
			headerStyle = Owner.InspectorTheme._MainHeaders;
			ResetToDefaultButton = Owner.InspectorTheme._ResetButton;
			spaceSize = Owner.InspectorTheme._spaceSize;
		}

		private void DrawInspector () {

			//The inspector needs a visual theme to use, this makes it available and only displays the rest after it is set.
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("InspectorTheme"), new GUIContent("Inspector Theme"));
			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				headerStyle = Owner.InspectorTheme._MainHeaders;
				ResetToDefaultButton = Owner.InspectorTheme._ResetButton;
				spaceSize = Owner.InspectorTheme._spaceSize;
			}

			//Will only happen if above is attatched and has a theme.
			if (Owner == null || Owner.InspectorTheme == null) return;

			serializedObject.Update();

			//Describe what the script does
			EditorGUILayout.TextArea("Details.", EditorStyles.textArea);

			//Order of Drawing
			EditorGUILayout.Space(spaceSize);
			EditorGUILayout.LabelField("Core Movement", headerStyle);
			DrawStructWithDefault();
			DrawGeneralStruct();



			//Called whenever a property needs to be shown in the editor.
			void DrawProperty ( string property, string outputName, bool isHorizontal ) {
				if (isHorizontal) GUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
			}


			//Struct With Default
			#region Struct
			void DrawStructWithDefault () {
				EditorGUILayout.Space(spaceSize);
				DrawProperty("StructWithDefault", "Struct With Default", true);

				Undo.RecordObject(Owner, "set to defaults");
				if (GUILayout.Button("Default", ResetToDefaultButton))
				{
					//Owner.StructWithDefault = Owner.DefaultStruct;
				}
				serializedObject.ApplyModifiedProperties();
				GUILayout.EndHorizontal();
			}
			#endregion

			//GeneralStruct
			#region GeneralStruct
			void DrawGeneralStruct () {
				EditorGUILayout.Space(spaceSize);
				DrawProperty("GeneralStruct", "GeneralStruct", false);
			
				serializedObject.ApplyModifiedProperties();
			}
			#endregion
		}
	}
#endif
}


