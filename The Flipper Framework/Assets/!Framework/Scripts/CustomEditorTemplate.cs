using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

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
			ApplyStyle();
		}

		private void ApplyStyle () {
			_HeaderStyle = _OwnerScript._InspectorTheme._MainHeaders;
			_BigButtonStyle = _OwnerScript._InspectorTheme._GeneralButton;
			_spaceSize = _OwnerScript._InspectorTheme._spaceSize;
		}

		private bool IsThemeNotSet () {
			//The inspector needs a visual theme to use, this makes it available and only displays the rest after it is set.
			if (S_S_CustomInspectorMethods.IsDrawnPropertyChanged(serializedObject, "_InspectorTheme", "Inspector Theme", false))
			{
				ApplyStyle();
			}

			//Will only happen if above is attatched and has a theme.
			return (_OwnerScript == null || _OwnerScript._InspectorTheme == null);
		}

		private void DrawInspector () {

			if (IsThemeNotSet()) return;

			serializedObject.Update();

			//Describe what the script does
			EditorGUILayout.TextArea("Details.", EditorStyles.textArea);


			//Button for adding new action
			void DrawGeneralButton () {

				//Add new element button.
				if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject,"Button Name", _BigButtonStyle, _OwnerScript, "Undo Description"))
				{
					//Insert Action
				}
			}


			//ONLY ADD THE FOLLOWING IF THIS EDITOR IS INTENDED TO SHOW A LARGE GROUP OF VARIABLES IN YOUR OWN WAY
			//USE DRAWDEFAULTINPSECTOR INSTEAD TO GET YOUR VARIABLES NORMALLY
			//Order of Drawing
			EditorGUILayout.Space(_spaceSize);
			EditorGUILayout.LabelField("Core Movement", _HeaderStyle);
			DrawStructWithDefault();
			DrawGeneralStruct();
			DrawGeneralButton();
			S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject,"", "", false);


			//Struct With Default
			#region Struct
			void DrawStructWithDefault () {
				EditorGUILayout.Space(_spaceSize);
				S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject,"StructWithDefault", "Struct With Default", true);

				if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject, "Default", _BigButtonStyle, _OwnerScript, "set to defaults"))
				{
					//Owner.StructWithDefault = Owner.DefaultStruct;
				}
				GUILayout.EndHorizontal();
			}
			#endregion

			//GeneralStruct
			#region GeneralStruct
			void DrawGeneralStruct () {
				EditorGUILayout.Space(_spaceSize);
				S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject,"GeneralStruct", "GeneralStruct", false);
			}
			#endregion
		}
	}
#endif
}


