using System.Collections;
using System.Collections.Generic;
using templates;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class S_LinkSceneGUI : MonoBehaviour
{
	public MonoBehaviour	_LinkedComponent;
	public ICustomEditorLogic	_LinkedEditorLogic;

	void OnValidate () {
		if (_LinkedComponent == null) return;

		_LinkedEditorLogic = _LinkedEditorLogic as ICustomEditorLogic;
		if (_LinkedEditorLogic == null) 
			_LinkedEditorLogic = _LinkedComponent.GetComponent<ICustomEditorLogic>();
		if (_LinkedEditorLogic == null) return;
	}
}

[CustomEditor(typeof(S_LinkSceneGUI))]
public class EditorLinkEditor : Editor
{
	S_LinkSceneGUI _OwnerScript;

	private void OnEnable () {
		_OwnerScript = (S_LinkSceneGUI)target;
	}

	public override void OnInspectorGUI () {
		EditorGUILayout.TextArea("Link a component using ICustomEditorLogic CustomOnSceneGUI() to call that when this is selected as well.", EditorStyles.textArea);
		DrawDefaultInspector();
	}

	private void OnSceneGUI () {
		if (_OwnerScript != null && _OwnerScript._LinkedEditorLogic != null) { _OwnerScript._LinkedEditorLogic.CustomOnSceneGUI(); }
	}
}
#endif
