using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

namespace templates
{

	public class _TestEditorScript_ : ScriptableObject
	{
#if UNITY_EDITOR
		public S_O_CustomInspectorStyle _InspectorTheme;
#endif
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(_TestEditorScript_))]
	public class MainScriptEditor : S_CustomInspector_Base
	{
		_TestEditorScript_ _OwnerScript;


		public override void OnEnable () {
			//Setting variables
			_OwnerScript = (_TestEditorScript_)target;

			base.OnEnable();
		}

		public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
			return _OwnerScript._InspectorTheme;
		}

		public override void DrawInspectorNotInherited () {
			//Describe what the script does
			EditorGUILayout.TextArea("Details.", EditorStyles.textArea);


			//Button for adding new action
			void DrawGeneralButton () {

				//Add new element button.
				if (S_S_CustomInspector.IsDrawnButtonPressed(serializedObject,"Button Name", _BigButtonStyle, _OwnerScript, "Undo Description"))
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
			S_S_CustomInspector.DrawEditableProperty(serializedObject,"", "", false);


			//Struct With Default
			#region Struct
			void DrawStructWithDefault () {
				EditorGUILayout.Space(_spaceSize);
				S_S_CustomInspector.DrawEditableProperty(serializedObject,"StructWithDefault", "Struct With Default", true);

				if (S_S_CustomInspector.IsDrawnButtonPressed(serializedObject, "Default", _BigButtonStyle, _OwnerScript, "set to defaults"))
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
				S_S_CustomInspector.DrawEditableProperty(serializedObject,"GeneralStruct", "GeneralStruct", false);
			}
			#endregion
		}
	}
#endif
}


