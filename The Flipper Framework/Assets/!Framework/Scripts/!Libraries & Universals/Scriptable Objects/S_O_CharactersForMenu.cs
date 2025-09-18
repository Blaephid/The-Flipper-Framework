using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterObject")]
public class S_O_CharactersForMenu : ScriptableObject
{
	public string _displayName;
	public GameObject _Prefab;

#if UNITY_EDITOR
	[HideInInspector] public S_O_CustomInspectorStyle _InspectorTheme;
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(S_O_CharactersForMenu))]
public class S_O_CharactersForMenuEditor : S_CustomInspector_Base
{
	S_O_CharactersForMenu _OwnerScript;


	public override void OnEnable () {
		//Setting variables
		_OwnerScript = (S_O_CharactersForMenu)target;
		_InspectorTheme = _OwnerScript._InspectorTheme;

		base.OnEnable();
	}

	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _OwnerScript._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {
		//Start Tite and description

		DrawDefaultInspector();
	}
}
	#endif