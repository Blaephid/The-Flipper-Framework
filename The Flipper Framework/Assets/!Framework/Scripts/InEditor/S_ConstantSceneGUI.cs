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

		//Because OnEnable isn't called when prefab is placed in scene. Use this.
		OnEnable();
	}



	private void OnEnable () {
		if (!firstValidated)
		{
			if (_LinkedEditorLogic == null) { return; }
			SceneView.duringSceneGui += _LinkedEditorLogic.CustomOnSceneGUI;
			firstValidated = true;
		}
	}

	private void OnDisable () {
		firstValidated = false;
		SceneView.duringSceneGui -= _LinkedEditorLogic.CustomOnSceneGUI;
	}
}

[CustomEditor(typeof(S_ConstantSceneGUI))]
public class SceneGuiConstantEditor : Editor
{
	S_ConstantSceneGUI _OwnerScript;

	private void OnEnable () {
		_OwnerScript = (S_ConstantSceneGUI)target;
	}

	public override void OnInspectorGUI () {
		EditorGUILayout.TextArea("Link a component using ICustomEditorLogic CustomOnSceneGUI() to call that constantly no matter what is selected. ", EditorStyles.textArea);
		DrawDefaultInspector();
	}
}
#endif
