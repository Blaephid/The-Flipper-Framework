using UnityEngine;
using System.Collections;
using System;
using System.Net.NetworkInformation;

public class S_Spawn_Ring : S_Spawners_Base
{
	public StructRingSpawner _ringSpawnerData;

	[Serializable]
	public struct StructRingSpawner {
		public Transform    _TransformToFollow;
		public Vector3      _followOffset;
	}

	//Because this is inherited from S_Spawners, this overrrides the normal SpawnObject, which is called in by LateUpdate in the inherited script. Go there for calls
	public override void SpawnObject () {

		Vector3 spawnLocation =  _ringSpawnerData._TransformToFollow ? _ringSpawnerData._TransformToFollow.transform.position + _ringSpawnerData._followOffset : transform.position;

		Instantiate(_SpawnerData._TeleportSparkle, spawnLocation, transform.rotation); //Effect
		_ObjectClone = (GameObject)Instantiate(_SpawnerData._ObjectToSpawn, spawnLocation, transform.rotation); //Object

		//Bind the spawned ring either to a set object to follow, or to this so it is more organised in editor.
		_ObjectClone.transform.parent = _ringSpawnerData._TransformToFollow ? _ringSpawnerData._TransformToFollow : transform;
	}

}
