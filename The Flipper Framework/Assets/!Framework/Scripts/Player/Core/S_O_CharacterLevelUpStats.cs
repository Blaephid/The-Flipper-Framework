using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "SO_Character LevelUp Stats")]
public class S_O_CharacterLevelUpStats : ScriptableObject
{
	[HideInInspector] public string Title = "Title";

	[Header("Main Control")]
	public bool _canLevelUp = true;
	public bool _arePointsEnabled = true;
	public bool _isActionChainEnabled = true;

	[Header("Gaining Points")]
	public float pointsFromSpheres = 5;
	public AnimationCurve pointsPerActionChainLevel = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 0.1f),
				new Keyframe(5, 0),
				new Keyframe(6f, 10f),
				new Keyframe(15f, 300f),
			});
	public AnimationCurve chainCountDownPerLevel = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 5),
				new Keyframe(10, 3),
				new Keyframe(15, 2f),
			});

	[Tooltip("How many points to gain per second of rolling with energy.")]
	public float pointsFromRolling = 200;
	[Tooltip("")]
	public float pointsFromPerfectHomingAttack = 25;

	public List<LevelUpStats> _Levels = new List<LevelUpStats>();

	private void OnValidate () {
		for (int i = 0 ; i < _Levels.Count ; i++)
		{
			if (_Levels[i].level != i + 1)
			{
				_Levels[i] = new LevelUpStats()
				{
					level = i + 1,
					requiredPoints = _Levels[i].requiredPoints,
					ringsMaxMultiplier = _Levels[i].ringsMaxMultiplier,
					energyMaxMultiplier = _Levels[i].energyMaxMultiplier,
					speedMaxMultiplier = _Levels[i].speedMaxMultiplier
				};
			}
		}
	}

	[Serializable]
	public struct LevelUpStats
	{
		public int level;
		public int requiredPoints;
		public float ringsMaxMultiplier;
		public float energyMaxMultiplier;
		public float speedMaxMultiplier;
		public LevelUpStats ( int lvl, int xp ) {
			level = lvl;
			requiredPoints = xp;
			ringsMaxMultiplier = 1;
			energyMaxMultiplier = 1;
			speedMaxMultiplier = 1;
		}
	}

#if UNITY_EDITOR
	[HideInInspector] public S_O_CustomInspectorStyle _InspectorTheme;
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(S_O_CharacterLevelUpStats))]
public class S_O_CharacterLevelUpStatsEditor : S_CustomInspector_Base
{
	S_O_CharacterLevelUpStats _OwnerScript;

	public override void OnEnable () {
		//Setting variables
		_OwnerScript = (S_O_CharacterLevelUpStats)target;
		_InspectorTheme = _OwnerScript._InspectorTheme;

		base.OnEnable();
	}

	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _OwnerScript._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {
		//Start Tite and description
		_OwnerScript.Title = EditorGUILayout.TextField(_OwnerScript.Title);

		EditorGUILayout.TextArea("This list of structs contains the requirements and effects of levelling up. \n", EditorStyles.textArea);

		DrawDefaultInspector();
	}

}
#endif
