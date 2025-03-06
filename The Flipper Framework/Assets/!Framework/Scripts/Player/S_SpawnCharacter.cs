using Cinemachine;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SpawnCharacter : S_Vis_Base
{
	[OnlyDrawIf("_viewVisualisationData", true)]
	[DrawHorizontalWithOthers(new string[] { "_meshScale" }, new float[] { 2.5f, 1f })]
	[BaseColour(0.8f, 0.8f, 0.8f, 1)]
	public Mesh _VisualiseWithMesh;
	[OnlyDrawIf("_viewVisualisationData", true)]
	[HideInInspector, Min(1)]
	[SerializeField] private float _meshScale = 1;

	[Space]
	[SerializeField]
	[Tooltip("If false, won't spawn player, but will stick check custom spawn reference.")]
	[DrawHorizontalWithOthers(new string[] { "_DefaultCharacter" }, new float[] {1f, 2.5f })]
	private bool        _isActive;
	[SerializeField]
	[HideInInspector]
	private GameObject      _DefaultCharacter;

	public CinemachineBrain _CameraBrain;

	[Header("On Spawn")]
	public int      _spawnDelay = 5;
	public bool	_launch;
	[Tooltip("If data is provided, player will start with a velocity, rather than just dropping. This is applied in S_CharacterTools.")]
	public S_Structs.LaunchPlayerData _launchOnSpawnData_;


	//Spawning
	private GameObject            _CharacterToSpawn;
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

	[ExecuteInEditMode]
	private void Update () {
		if(Application.isPlaying) { return; }
		_hasVisualisationScripted = true;
		UpdateLaunchDataToDirection();
	}

	private void OnEnable () {
		if (Application.isPlaying) { return; }
		UpdateLaunchDataToDirection();
	}

	[ExecuteInEditMode]
	private void UpdateLaunchDataToDirection () {
		_launchOnSpawnData_ = new S_Structs.LaunchPlayerData()
		{
			_force_ = _launchOnSpawnData_._force_,
			_direction_ = _launchOnSpawnData_._direction_,
			_directionToUse_ = (transform.rotation * _launchOnSpawnData_._direction_).normalized,
			_lockInputFrames_ = _launchOnSpawnData_._lockInputFrames_,
			_lockAirMovesFrames_ = _launchOnSpawnData_._lockAirMovesFrames_,
			_overwriteGravity_ = _launchOnSpawnData_._overwriteGravity_,
			_lockInputTo_ = _launchOnSpawnData_._lockInputTo_
		};
	}

	// Use this for initialization
	void Awake () {
		//Some object shouldn't deactivate until the player is spawned in (like the start camera).
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

			GameObject Player = Instantiate(_CharacterToSpawn, transform.position, Quaternion.identity, transform);
			//Check S_CharacterTools Awake For assigning references to this. It's there because the Awakes of Player happen before any more code in this method.
		}
		yield return new WaitForFixedUpdate();

		CheckReplace();

		yield return null;
	}

	private void CheckReplace () {
		if (_ReplaceReferenceForSpawners._shouldReplacePlayerSourceOfSpawn)
		{
			_SpawnedPlayer = _ReplaceReferenceForSpawners._Replacement;
			_spawnCheckModifier = _ReplaceReferenceForSpawners._spawnDistanceModifier;
		}
	}

	public override void DrawGizmosAndHandles ( bool selected ) {
		if(selected) { return;}

		Gizmos.color = selected ? _selectedOutlineColour : _normalOutlineColour;
		
		Gizmos.DrawWireMesh(_VisualiseWithMesh, transform.position, transform.rotation, Vector3.one * _meshScale * 10);
	}
}
