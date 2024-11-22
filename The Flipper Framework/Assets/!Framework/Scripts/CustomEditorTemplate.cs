using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace templates
{

	public class _MainScript_ : ScriptableObject
	{
#if UNITY_EDITOR
		public S_O_CustomInspectorStyle _InspectorTheme;
#endif
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(_MainScript_))]
	public class MainScriptEditor : Editor
	{
		_MainScript_ _OwnerScript;

		GUIStyle _HeaderStyle;
		GUIStyle _BigButtonStyle;
		GUIStyle _SmallButtonStyle;
		float _spaceSize;

		public override void OnInspectorGUI () {
			DrawInspector();
		}

		private void OnEnable () {
			//Setting variables
			_OwnerScript = (_MainScript_)target;

			if (_OwnerScript._InspectorTheme == null) { return; }
			_HeaderStyle = _OwnerScript._InspectorTheme._MainHeaders;
			_BigButtonStyle = _OwnerScript._InspectorTheme._GeneralButton;
			_spaceSize = _OwnerScript._InspectorTheme._spaceSize;
		}

		private void DrawInspector () {

			//The inspector needs a visual theme to use, this makes it available and only displays the rest after it is set.
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_InspectorTheme"), new GUIContent("Inspector Theme"));
			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				_HeaderStyle = _OwnerScript._InspectorTheme._MainHeaders;
				_BigButtonStyle = _OwnerScript._InspectorTheme._GeneralButton;
				_spaceSize = _OwnerScript._InspectorTheme._spaceSize;
			}

			//Will only happen if above is attatched and has a theme.
			if (_OwnerScript == null || _OwnerScript._InspectorTheme == null) return;

			serializedObject.Update();

			//Describe what the script does
			EditorGUILayout.TextArea("Details.", EditorStyles.textArea);


			//Called whenever a property needs to be shown in the editor.
			void DrawProperty ( string property, string outputName, bool isHorizontal ) {
				if (isHorizontal) GUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
			}


			//Button for adding new action
			void DrawButton () {

				//Add new element button.
				Undo.RecordObject(_OwnerScript, "What Button Does");
				if (GUILayout.Button("Button Name", _BigButtonStyle))
				{
					//Insert Action
					serializedObject.Update();
				}
				serializedObject.ApplyModifiedProperties();
			}


			//ONLY ADD THE FOLLOWING IF THIS EDITOR IS INTENDED TO SHOW A LARGE GROUP OF VARIABLES IN YOUR OWN WAY
			//USE DRAWDEFAULTINPSECTOR INSTEAD TO GET YOUR VARIABLES NORMALLY
			//Order of Drawing
			EditorGUILayout.Space(_spaceSize);
			EditorGUILayout.LabelField("Core Movement", _HeaderStyle);
			DrawStructWithDefault();
			DrawGeneralStruct();
			DrawButton();


			//Struct With Default
			#region Struct
			void DrawStructWithDefault () {
				EditorGUILayout.Space(_spaceSize);
				DrawProperty("StructWithDefault", "Struct With Default", true);

				Undo.RecordObject(_OwnerScript, "set to defaults");
				if (GUILayout.Button("Default", _BigButtonStyle))
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
				EditorGUILayout.Space(_spaceSize);
				DrawProperty("GeneralStruct", "GeneralStruct", false);
			
				serializedObject.ApplyModifiedProperties();
			}
			#endregion
		}
	}
#endif
}


