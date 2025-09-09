using Cinemachine;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SpawnCharacter : S_Vis_Base
{
	[NonSerialized] public static bool s_CanSpawnNow = true;


#if UNITY_EDITOR
	[OnlyDrawIf("_viewVisualisationData", true)]
	[DrawHorizontalWithOthers(new string[] { "_meshScale" }, new float[] { 2.5f, 1f })]
	[BaseColour(0.8f, 0.8f, 0.8f, 1)]
	public Mesh _VisualiseWithMesh;
	[OnlyDrawIf("_viewVisualisationData", true)]
	[HideInInspector, Min(1)]
	[SerializeField] private float _meshScale = 1;
#endif

	[Space]
	[SerializeField]
	[Tooltip("If false, won't spawn player, but will stick check custom spawn reference.")]
	private bool        _isActive;
	[SerializeField]

	[Header("Stage Info")]
	public S_O_CharactersForMenu      _DefaultCharacter;
	public CinemachineBrain _CameraBrain;
	public S_O_StageScenes _StageInfo;
	public AudioSource _MusicPlayer;
	public GameObject _PostProcessing;


	[Header("On Spawn")]
	public int      _spawnDelay = 5;
	public Animator _AnimatorOnSpawn;
	public bool	_launch;
	[Tooltip("If data is provided, player will start with a velocity, rather than just dropping. This is applied in S_CharacterTools.")]
	public LaunchPlayerData _launchOnSpawnData_;


	//Spawning
	[NonSerialized] public S_O_CharactersForMenu            _CharacterToSpawn;
	public static Transform _SpawnedPlayer;
	public static float           _spawnCheckModifier = 1;


	[Header("External References")]
	public S_DeactivateOnStart[] _ListOfDeactivationsToDelay;

	public CustomSpawnReference    _ReplaceReferenceForSpawners = new CustomSpawnReference
	{
		_shouldReplacePlayerSourceOfSpawn = false,
		_Replacement = null,
		_spawnDistanceModifier = 1,
	};

	[Serializable]
	public struct CustomSpawnReference
	{
		[Tooltip("If this is true, then the inputted transform will be checled against when spawning or hiding elements. E.G. Rings will spawn when this object gets close, not the player.")]
		public bool     _shouldReplacePlayerSourceOfSpawn;
		[Tooltip("See above. This is mainly used for specific sequences where the camera moves on its own without the player character.")]
		public Transform        _Replacement;
		[Tooltip("If above is true, all spawners will multiply their check distance by this.")]
		public float        _spawnDistanceModifier;
	}

#if UNITY_EDITOR
	//[ExecuteInEditMode]
	//[ExecuteAlways]
	//private void Update () {
	//	if(Application.isPlaying) { return; }
	//	_hasVisualisationScripted = true;
	//	UpdateLaunchDataToDirection();
	//}

	[ExecuteAlways]
	private void OnEnable () {
		if (Application.isPlaying) { return; }
		UpdateLaunchDataToDirection();
	}

	[ExecuteInEditMode]
	private void UpdateLaunchDataToDirection () {
		_launchOnSpawnData_ = LaunchPlayerData.SetLaunchDataToDirection(transform, _launchOnSpawnData_);
	}
#endif

	// Use this for initialization
	void Awake () {
		StartCoroutine(WaitUntilCanSpawn());
	}

	//If loading a scene directly in editor, s_CanSpawn will be true, but if loading in from a loading screen, it will not be true until every main scene is loaded in.
	IEnumerator WaitUntilCanSpawn () {
		while (!s_CanSpawnNow)
		{
			yield return new WaitForEndOfFrame();
		}

		//Some objects shouldn't deactivate until the player is spawned in (like the start camera).
		for (int i = 0 ; i < _ListOfDeactivationsToDelay.Length ; i++)
		{
			_ListOfDeactivationsToDelay[i]._delayInSeconds = (_spawnDelay + 1) * Time.fixedDeltaTime;
		}

		StartCoroutine(Spawn(_spawnDelay));
	}

	IEnumerator Spawn ( int delay ) {
		//Dont spawn until enough frames have passed.
		for (int i = 0 ; i < _spawnDelay ; i++)
		{
			yield return new WaitForFixedUpdate();
		}

		if (_isActive)
		{
			S_SelectMenu ExternalCharacterSelected = FindFirstObjectByType<S_SelectMenu>();
			if (ExternalCharacterSelected != null)
			{
				_CharacterToSpawn = ExternalCharacterSelected._SelectedCharacter;
			}
			else
			{
				_CharacterToSpawn = _DefaultCharacter;
			}

			GameObject Player = Instantiate(_CharacterToSpawn._Prefab, transform.position, Quaternion.identity, transform);

			SetPlayerValuesOnStart(Player);
			//Check S_CharacterTools Awake For assigning references to this. It's there because the Awakes of Player happen before any more code in this method.
		}
		yield return new WaitForFixedUpdate();

		CheckReplace();

		yield return null;
	}

	private void SetPlayerValuesOnStart (GameObject Player) {
		Player.GetComponentInChildren<S_PlayerScore>()._StageInfo = _StageInfo;
		Player.GetComponentInChildren<S_PlayerCoreValues>()._Music = _MusicPlayer;
	}

	private void CheckReplace () {
		if (_ReplaceReferenceForSpawners._shouldReplacePlayerSourceOfSpawn)
		{
			_SpawnedPlayer = _ReplaceReferenceForSpawners._Replacement;
			_spawnCheckModifier = _ReplaceReferenceForSpawners._spawnDistanceModifier;
		}
	}

	public void OnStageEnd () {
		_PostProcessing.SetActive(false);
	}


#if UNITY_EDITOR
	public override void DrawGizmosAndHandles ( bool selected ) {
		if(selected) { return;}

		Gizmos.color = selected ? _selectedOutlineColour : _normalOutlineColour;
		
		Gizmos.DrawWireMesh(_VisualiseWithMesh, transform.position, transform.rotation, Vector3.one * _meshScale * 10);
	}
#endif
}
