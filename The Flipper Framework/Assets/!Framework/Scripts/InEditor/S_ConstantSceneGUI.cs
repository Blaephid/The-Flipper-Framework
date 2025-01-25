using System.Collections;
using System.Collections.Generic;
using templates;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TerrainTools;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class S_ConstantSceneGUI : MonoBehaviour
{
	public MonoBehaviour	_LinkedComponent;
	public ICustomEditorLogic	_LinkedEditorLogic;

	private bool currentlyEnabled = false;

	void OnValidate () {
		
		if (_LinkedComponent == null) return;
		ConvertComponentToEditorLogicInterface();
		EnableEvents(null);
	}

	private void ConvertComponentToEditorLogicInterface () {
		//Used two different methods to get the editor logic as a usable type, from the given component.
		_LinkedEditorLogic = _LinkedEditorLogic as ICustomEditorLogic;
		if (_LinkedEditorLogic == null)
			_LinkedEditorLogic = _LinkedComponent.GetComponent<ICustomEditorLogic>();
	}


	//Attaches the given script to the builtin SceneView GUI update, so its called constaltly by that dispatcher.
	//There is a custom inspector button below to trigger this as it won't happen when object is placed in scene.
	public void OnEnable () {
		EnableEvents(null);

		//These seperate events track when entering the prefab editor. If these weren't here, then these would continue to call custom logic even when in a seperate mode.
		PrefabStage.prefabStageOpened += DisableEvents;
		PrefabStage.prefabStageClosing += EnableEvents;
	}

	private void OnDisable () {
		DisableEvents(null);

		//These seperate events track when entering the prefab editor. If these weren't here, then these would continue to call custom logic even when in a seperate mode.
		PrefabStage.prefabStageOpened -= DisableEvents;
		PrefabStage.prefabStageClosing -= EnableEvents;
	}

	public void EnableEvents ( PrefabStage prefabStage ) {
		if (currentlyEnabled) { return; }
		if (_LinkedEditorLogic == null) { return; }

		//Prevents the prefab assets from enabling this if not viewing them in prefab mode. If this wasn't here, all prefabs with this would call in every scene.
		if (PrefabUtility.IsPartOfPrefabAsset(gameObject) && PrefabStageUtility.GetCurrentPrefabStage() == null) { return; }

		currentlyEnabled = true;
		SceneView.duringSceneGui += _LinkedEditorLogic.CustomOnSceneGUI;
	}
	public void DisableEvents ( PrefabStage prefabStage ) {
		if (!currentlyEnabled) { return; }
		if (_LinkedComponent == null) return;
		currentlyEnabled = false;

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
		EditorGUILayout.TextArea("Link a component using ICustomEditorLogic CustomOnSceneGUI() to call that constantly no matter what is selected. It will use the built in SceneView.duringSceneGui event ", EditorStyles.textArea);
		DrawDefaultInspector();

		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject, "Enable", _BigButtonStyle))
		{
			_OwnerScript.OnEnable();
		}
	}
}
#endif
