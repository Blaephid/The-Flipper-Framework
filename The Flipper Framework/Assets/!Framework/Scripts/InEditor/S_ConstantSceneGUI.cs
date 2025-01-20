using System.Collections;
using System.Collections.Generic;
using templates;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class S_ConstantSceneGUI : MonoBehaviour
{
	public MonoBehaviour	_LinkedComponent;
	public ICustomEditorLogic	_LinkedEditorLogic;

	private bool firstValidated = false;

	void OnValidate () {

		if (_LinkedComponent == null) return;

		_LinkedEditorLogic = _LinkedEditorLogic as ICustomEditorLogic;
		if (_LinkedEditorLogic == null) 
			_LinkedEditorLogic = _LinkedComponent.GetComponent<ICustomEditorLogic>();
		if (_LinkedEditorLogic == null) return;
	}


	//Attaches the given script to the builtin SceneView GUI update, so its called constaltly by that dispatcher.
	//There is a custom inspector button below to trigger this as it won't happen when object is placed in scene.
	public void OnEnable () {
		if (firstValidated) { return; }
		
		if (_LinkedEditorLogic == null) { return; }
		SceneView.duringSceneGui += _LinkedEditorLogic.CustomOnSceneGUI;
		firstValidated = true;
		
	}

	private void OnDisable () {
		firstValidated = false;
		SceneView.duringSceneGui -= _LinkedEditorLogic.CustomOnSceneGUI;
	}

	[HideInInspector]
	public S_O_CustomInspectorStyle _InspectorTheme;
}

[CustomEditor(typeof(S_ConstantSceneGUI))]
public class SceneGuiConstantEditor : S_CustomInspector_Base
{
	S_ConstantSceneGUI _OwnerScript;

	public override void OnEnable () {
		_OwnerScript = (S_ConstantSceneGUI)target;

		base.OnEnable();
	}

	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _OwnerScript._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {
		EditorGUILayout.TextArea("Link a component using ICustomEditorLogic CustomOnSceneGUI() to call that constantly no matter what is selected. ", EditorStyles.textArea);
		DrawDefaultInspector();

		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject, "Enable", _BigButtonStyle))
		{
			_OwnerScript.OnEnable();
		}
	}
}
#endif
