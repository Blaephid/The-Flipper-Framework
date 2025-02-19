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

	[HideInInspector]
	bool _currentlyAddedToDuringSceneGUI = false;
	[HideInInspector]
	bool _currentlyAddedToDuringPrefabChange = false;

	private void Awake () {
		if (!Application.isPlaying)
			AddToPrefabStageChange();
		else
			OnDisable();
	}


	void OnValidate () {	
		OnEnable();
	}

	private void ConvertComponentToEditorLogicInterface () {
		if (_LinkedComponent == null) return;

		//Used two different methods to get the editor logic as a usable type, from the given component.
		_LinkedEditorLogic = _LinkedEditorLogic as ICustomEditorLogic;
		if (_LinkedEditorLogic == null)
			_LinkedEditorLogic = _LinkedComponent.GetComponent<ICustomEditorLogic>();
	}


	//Attaches the given script to the builtin SceneView GUI update, so its called constaltly by that dispatcher.
	//There is a custom inspector button below to trigger this as it won't happen when object is placed in scene.
	public void OnEnable () {
		if (!Application.isPlaying)
		{
			//PrefabStageUtility.GetCurrentPrefabStage();
			AddToDuringSceneGUI(PrefabStageUtility.GetCurrentPrefabStage());
			AddToPrefabStageChange();
		}
	}

	private void OnDisable () {
		RemoveFromDuringSceneGUI(null);
		RemoveFromPrefabStageChange();
	}

	private void AddToPrefabStageChange () {
		if(_currentlyAddedToDuringPrefabChange) { return; }

		_currentlyAddedToDuringPrefabChange = true;

		//These seperate events track when entering the prefab editor. If these weren't here, then these would continue to call custom logic even when in a seperate mode.
		PrefabStage.prefabStageOpened += RemoveFromDuringSceneGUI;
		PrefabStage.prefabStageClosing += EventPrefabStageClosed;
	}

	private void RemoveFromPrefabStageChange () {
		if (!_currentlyAddedToDuringPrefabChange) { return; }

		_currentlyAddedToDuringPrefabChange = false;

		//These seperate events track when entering the prefab editor. If these weren't here, then these would continue to call custom logic even when in a seperate mode.
		PrefabStage.prefabStageOpened -= RemoveFromDuringSceneGUI;
		PrefabStage.prefabStageClosing -= EventPrefabStageClosed;
	}

	//Because prefabStageClosing returns the prefabStage just exited, it messes with the checks if currently in a prefabStage,
	//So the event instead calls this method, which calls AddToSceneGUIWithout passing the prefabstage.
	public void EventPrefabStageClosed(PrefabStage prefabStage ) {
		AddToDuringSceneGUI(null);
	}


	//DuringSceneGUI Event. This event is called every frame in editor, so add and remove from this.
	public void AddToDuringSceneGUI ( PrefabStage prefabStage) {
		if (!this || _currentlyAddedToDuringSceneGUI || !gameObject) { return; }
		else if (_LinkedEditorLogic == null)
		{
			ConvertComponentToEditorLogicInterface();
			if (_LinkedEditorLogic == null)
			{
				return;
			}
		}

		//prefabStage = prefabStage == null ? PrefabStageUtility.GetCurrentPrefabStage() : prefabStage;

		//Prevents the PREFAB ASSETS from enabling this. If this wasn't here, all prefabs with this would call in every scene, as well as their instances.
		if (prefabStage == null && PrefabUtility.IsPartOfPrefabAsset(gameObject)) { return; }

		//Prevents the scene objects from enabling if currently in prefab view. This prevents a bunch of things being drawn when this object is not relevant.
		else if (prefabStage != null)
		{
			if(S_S_EditorMethods.CheckCallerMethodsFor("OnEnable")) {return;} //Because prefabContentsRoot cannot be used when called during an OnEnable, if this was called from that, do nothing and continue.
			else if (prefabStage.prefabContentsRoot != gameObject.transform.root.gameObject) { return; }
		}

		_currentlyAddedToDuringSceneGUI = true;
		SceneView.duringSceneGui += _LinkedEditorLogic.CustomOnSceneGUI;
	}
	public void RemoveFromDuringSceneGUI ( PrefabStage prefabStage ) {
		if (!this || !_currentlyAddedToDuringSceneGUI) { return; }
		else if (_LinkedEditorLogic == null)
		{
			ConvertComponentToEditorLogicInterface();
			if (_LinkedEditorLogic == null)
			{
				return;
			}
		}

		_currentlyAddedToDuringSceneGUI = false;
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
