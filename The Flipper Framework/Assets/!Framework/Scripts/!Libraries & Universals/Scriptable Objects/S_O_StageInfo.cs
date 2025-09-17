using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "StageScenesObject")]
public class S_O_StageInfo : ScriptableObject
{
	[Header("Display")]
	public string _StageName;

	[Header("Handling scenes to load")]
	public float _minSecondsToLoadScenes;
	public SceneField _BaseScene;
	public SceneField[] _AdditionalScenes;


	[Header("Rank")]
	public RankingStruct _Ranks = new RankingStruct()
	{
		TimeTotal_DRank = 5999,
		Time_DRank = new Vector3 (99,59,0),
		Rings_DRank = 0,
	};

	[Serializable]
	public struct RankingStruct {
		[Header("Time Ranks")]
		public Vector3 Time_XRank;
		[CustomReadOnly] public float TimeTotal_XRank;
		public Vector3 Time_SRank;
		[CustomReadOnly] public float TimeTotal_SRank;
		public Vector3 Time_ARank;
		[CustomReadOnly] public float TimeTotal_ARank;
		public Vector3 Time_BRank;
		[CustomReadOnly] public float TimeTotal_BRank;
		public Vector3 Time_CRank;
		[CustomReadOnly] public float TimeTotal_CRank;
		[CustomReadOnly]
		public Vector3 Time_DRank;
		[CustomReadOnly] public float TimeTotal_DRank;

		[Header("Ring Ranks")]
		public float Rings_SRank;
		public float Rings_ARank;
		public float Rings_BRank;
		public float Rings_CRank;
		[CustomReadOnly]
		public float Rings_DRank;
	}

	private void OnValidate () {
		_Ranks.Time_XRank = LimitVectorToTimeValues(_Ranks.Time_XRank);
		_Ranks.Time_SRank = LimitVectorToTimeValues(_Ranks.Time_SRank);
		_Ranks.Time_ARank = LimitVectorToTimeValues(_Ranks.Time_ARank);
		_Ranks.Time_BRank = LimitVectorToTimeValues(_Ranks.Time_BRank);
		_Ranks.Time_CRank = LimitVectorToTimeValues(_Ranks.Time_CRank);

		_Ranks.TimeTotal_XRank = S_S_MoreMaths.ConvertVectorMinutesTimeToTotalTime(_Ranks.Time_XRank);
		_Ranks.TimeTotal_SRank = S_S_MoreMaths.ConvertVectorMinutesTimeToTotalTime(_Ranks.Time_SRank);
		_Ranks.TimeTotal_ARank = S_S_MoreMaths.ConvertVectorMinutesTimeToTotalTime(_Ranks.Time_ARank);
		_Ranks.TimeTotal_BRank = S_S_MoreMaths.ConvertVectorMinutesTimeToTotalTime(_Ranks.Time_BRank);
		_Ranks.TimeTotal_CRank = S_S_MoreMaths.ConvertVectorMinutesTimeToTotalTime(_Ranks.Time_CRank);
		_Ranks.TimeTotal_DRank = S_S_MoreMaths.ConvertVectorMinutesTimeToTotalTime(_Ranks.Time_DRank);
		if (_Ranks.Rings_DRank != 0) { _Ranks.Rings_DRank = 0; }
	}

	private Vector3 LimitVectorToTimeValues (Vector3 time) {
		time.x = Mathf.Clamp(time.x, 0, 99);
		time.y = Mathf.Clamp(time.y, 0, 59);
		time.z = Mathf.Clamp(time.z, 0, 99);
		return time;
	}


#if UNITY_EDITOR
	[HideInInspector] public S_O_CustomInspectorStyle _InspectorTheme;
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(S_O_StageInfo))]
public class S_O_StageScenesEditor : S_CustomInspector_Base
{
	S_O_StageInfo _OwnerScript;


	public override void OnEnable () {
		//Setting variables
		_OwnerScript = (S_O_StageInfo)target;
		_InspectorTheme = _OwnerScript._InspectorTheme;

		base.OnEnable();
	}

	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _OwnerScript._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {
		//Start Tite and description

		EditorGUILayout.TextArea("This object is used to set ranks and load in the correct scenes for this stage. \n" +
		"If your stage is contained in one level, just place that in _BaseScene. \n " +
		"If your stage has multiple scenes (maybe to avoid one big level over 100MB as that doesn't work with Github)" +
		"Then you will need to make a BASE scene for the level that holds ONLY the main data and lighting (E.G. skybox), then place any subscenes that make up the level in the array.", EditorStyles.textArea);

		DrawDefaultInspector();
	}
}
	#endif