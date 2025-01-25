using Cinemachine;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SpawnCharacter : MonoBehaviour {

	[SerializeField]
	[Tooltip("If false, won't spawn player, but will stick check custom spawn reference.")]
	private bool        _isActive;

	[SerializeField] 
	private GameObject	_DefaultCharacter;
	public CinemachineBrain _CameraBrain;
	public int	_spawnDelay = 5;


	//Spawning
	private GameObject            _CharacterToSpawn;
	public static Transform	_SpawnedPlayer;
	public static float           _spawnCheckModifier = 1;
	public Vector3                _velocityOnSpawn;

	public S_DeactivateOnStart[] _ListOfDeactivationsToDelay;

	public CustomSpawnReference    _ReplaceReferenceForSpawners = new CustomSpawnReference
	{
		_shouldReplacePlayerSourceOfSpawn = false,
		_Replacement = null,
		_spawnDistanceModifier = 1,
	};

	[Serializable]
	public struct CustomSpawnReference {
		[Tooltip("If this is true, then the inputted transform will be checled against when spawning or hiding elements. E.G. Rings will spawn when this object gets close, not the player.")]
		public bool	_shouldReplacePlayerSourceOfSpawn;
		[Tooltip("See above. This is mainly used for specific sequences where the camera moves on its own without the player character.")]
		public Transform	_Replacement;
		[Tooltip("If above is true, all spawners will multiply their check distance by this.")]
		public float        _spawnDistanceModifier;
	}



	// Use this for initialization
	void Awake () {
		//Some object shouldn't deactivate until the player is spawned in (like the start camera).
		for(int i  = 0; i < _ListOfDeactivationsToDelay.Length; i++)
		{
			_ListOfDeactivationsToDelay[i]._delayInSeconds = (_spawnDelay + 1) * Time.fixedDeltaTime;
		}
		
		StartCoroutine(Spawn(_spawnDelay));
	}
	
	IEnumerator Spawn(int delay)
    {
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
}
