using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "StageScenesObject")]
public class S_O_StageScenes : ScriptableObject
{
	[Header("Display")]
	public string _StageName;

	[Header("Handling scenes to load")]
	public int _minSecondsToLoadScenes;
	public SceneField _BaseScene;
	public SceneField[] _Scenes;

	[Header("Rank")]
	public RankingStruct _Ranks = new RankingStruct()
	{
		Time_DRank = 6001,
		Rings_DRank = 0,
	};

	[Serializable]
	public struct RankingStruct {
		[Header("Time Ranks")]
		public float Time_XRank;
		public float Time_SRank;
		public float Time_ARank;
		public float Time_BRank;
		public float Time_CRank;
		[CustomReadOnly]
		public float Time_DRank;

		[Header("Ring Ranks")]
		public float Rings_SRank;
		public float Rings_ARank;
		public float Rings_BRank;
		public float Rings_CRank;
		[CustomReadOnly]
		public float Rings_DRank;
	}


#if UNITY_EDITOR
	[HideInInspector] public S_O_CustomInspectorStyle _InspectorTheme;
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(S_O_StageScenes))]
public class S_O_StageScenesEditor : S_CustomInspector_Base
{
	S_O_StageScenes _OwnerScript;


	public override void OnEnable () {
		//Setting variables
		_OwnerScript = (S_O_StageScenes)target;
		_InspectorTheme = _OwnerScript._InspectorTheme;

		base.OnEnable();
	}

	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _OwnerScript._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {
		//Start Tite and description

		EditorGUILayout.TextArea("This object is used to allow the stage select screen to locate scenes relevant to the selected stage. \n" +
		"If your stage is contained in one level, just place that in _BaseScene. \n " +
		"If your stage required multiple scenes (maybe to avoid one big level over 100MB as that doesn't work with Github" +
		"Then you will need to make a BASE scene for the level, that holds the main data and lighting (E.G. skybox), then place any subscenes that make up the level in the array.", EditorStyles.textArea);

		DrawDefaultInspector();
	}
}
	#endif