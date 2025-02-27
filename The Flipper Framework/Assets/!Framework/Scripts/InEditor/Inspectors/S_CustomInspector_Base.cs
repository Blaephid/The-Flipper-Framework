using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;


#if UNITY_EDITOR

[CustomEditor(typeof(MonoBehaviour))]
[CanEditMultipleObjects]
public class S_CustomInspector_Base : Editor
{
	[SerializeField]
	public S_O_CustomInspectorStyle _InspectorTheme;

	public GUIStyle	_HeaderStyle;
	public GUIStyle	_BigButtonStyle;
	public GUIStyle	_SmallButtonStyle;
	public GUIStyle	_NormalHeaderStyle;
	public float	_spaceSize;

	public override void OnInspectorGUI () {
		DrawInspector();
	}

	public virtual void OnEnable () {
		_InspectorTheme = GetInspectorStyleFromSerializedObject();
		ApplyStyle();
	}

	public void ApplyStyle () {
		if (_InspectorTheme == null) { return; }
		_HeaderStyle = _InspectorTheme._MainHeaders;
		_BigButtonStyle = _InspectorTheme._GeneralButton;
		_spaceSize = _InspectorTheme.__spaceSize;
		_SmallButtonStyle = _InspectorTheme._ResetButton;
		_NormalHeaderStyle = _InspectorTheme._ReplaceNormalHeaders;
	}

	public bool IsThemeNotSet () {
		//The inspector needs a visual theme to use, this makes it available and only displays the rest after it is set.
		if (S_S_CustomInspector.IsDrawnPropertyChanged(serializedObject, "_InspectorTheme", "Inspector Theme", false))
		{
			_InspectorTheme = GetInspectorStyleFromSerializedObject();
			ApplyStyle();
		}

		//Will only happen if above is attatched and has a theme.
		return (_InspectorTheme == null);
	}

	public virtual S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _InspectorTheme;
		//This is to be overridden so the class in question can return its inspector theme from the class it is presenting.
	}

	public void DrawInspector () {

		if (IsThemeNotSet()) return;

		serializedObject.Update();

		DrawInspectorNotInherited();
	}

	public virtual void DrawInspectorNotInherited () {
		//This is to be overwritten by whatever inherits this class, so they can do their own inspector after the universal things are already done.
	}
}
#endif
