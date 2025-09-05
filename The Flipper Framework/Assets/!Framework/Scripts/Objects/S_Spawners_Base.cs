using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Spawners_Base : S_Data_Base
{
	public StructSpawnerData _SpawnerData;

	[Serializable]
	public struct StructSpawnerData
	{
		[Tooltip("The effect to temporarily spawn when spawning the main object. The prefab should be set to delete itself afterwards.")]
		public GameObject   _TeleportSparkle;

		[Tooltip("The object that will be spawned")]
		public GameObject   _ObjectToSpawn;

		[Tooltip("Is true, this will spawn its object again when the player dies, resetting one it already did.")]
		public bool         _willRespawnOnDeath;
		[Tooltip("Will only spawn its object when player gets within this distance.")]
		public float        _distanceFromPlayerToSpawn;
		[Tooltip("If true, the object will be spawned as soon as player enters range.")]
		public bool         _willAutoSpawn;

		[Tooltip("If true, the object will be spawned again whenever destroyed.")]
		public bool         _willRespawnWhenDestroyed;
		[Tooltip("The time in seconds between automatic respawns.")]
		public float        _respawnDelay;
	}

	[HideInInspector]
	public GameObject   _ObjectClone;

	[HideInInspector]
	public float _counter = 1;

	private bool isSet = false;

	void Awake () {
		if (!isSet)
		{
			_counter = Mathf.Max(_SpawnerData._respawnDelay, 0.01f);
			S_Manager_LevelProgress.OnReset += EventReturnOnDeath;
			isSet = true;
		}
	}

	////These attach the return methods, so they perform when onReset is invoked in the Level Progress script
	//private void OnEnable () {
	//	S_Manager_LevelProgress.OnReset += EventReturnOnDeath;
	//}
	//private void OnDisable () {
	//	S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
	//}
	//

	//Checks player distance and if not spawned already, then will do so if player is close enough
	private void LateUpdate () {
		if(S_SpawnCharacter._SpawnedPlayer == null ) { return; }

		if (_ObjectClone == null && _SpawnerData._willAutoSpawn)
		{
			if (S_S_MoreMaths.GetDistanceSqrOfVectors(S_SpawnCharacter._SpawnedPlayer.position, transform.position) < 
				Mathf.Pow(_SpawnerData._distanceFromPlayerToSpawn * S_SpawnCharacter._spawnCheckModifier, 2))
			{
				//Will only spawn if enough time has passed.
				if (_SpawnerData._willRespawnWhenDestroyed) { _counter += Time.deltaTime; }

				//Counter will either increase, or be set directly to respawn delay if needed to respawn immediately. The max is to prevent this from going constantly if the respawn data has a 0 respawn delay.
				if (_counter >= Mathf.Max(_SpawnerData._respawnDelay, 0.01f))
				{
					_counter = 0;
					SpawnObject();
				}
			}
		}
	
	}

	//Is virtual because scripts that inhereit this will override it.
	public virtual void SpawnObject () {

	}

	//Destroys any enemy spawned, so a new one can be created again in the above methods.
	private void EventReturnOnDeath ( object sender, EventArgs e ) {
		//Since enemies can move, this removes that one and gets ready to spawn a new one in the correct place
		if (_SpawnerData._willRespawnOnDeath && _ObjectClone != null)
		{
			Destroy(_ObjectClone);
			_ObjectClone = null;
		}
		_counter = Mathf.Max(_SpawnerData._respawnDelay, 0.01f); //Ensures respawn will be immediate, even if already destroyed
	}
}
