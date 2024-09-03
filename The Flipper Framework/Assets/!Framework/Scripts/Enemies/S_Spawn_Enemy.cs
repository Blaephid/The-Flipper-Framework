using UnityEngine;
using System;
using System.Collections;

public class S_Spawn_Enemy : S_Spawners
{

	//Instantiates the prefab in the scene, also spawning the effect temporarily.
	//Because this is inherited from S_Spawners, this overrrides the normal SpawnInNormal, which is called in by LateUpdate in the inherited script. Go there for changes.
	public override void SpawnObject () {
		Instantiate(_SpawnerData._TeleportSparkle, transform.position, transform.rotation);
		_ObjectClone = (GameObject)Instantiate(_SpawnerData._ObjectToSpawn, transform.position, transform.rotation);
		_ObjectClone.transform.parent = transform; //Set as a child of this object so the hierarchy is cleaner.

		if (_SpawnerData._willRespawnWhenDestroyed && _ObjectClone.TryGetComponent(out S_AI_Health AIHealth))
		{
			AIHealth.SpawnReference = this; //This makes the spawned object able to call methods in this, such as RespawntSpawner when it dies.
		}
	}

	//Called by the spawned onject when it is destroyed, allowing this to respawn it if set to.
	public void RestartSpawner () {
		_ObjectClone = null;
		_counter = 0;
	}
}
