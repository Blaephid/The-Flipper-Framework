using UnityEngine;
using System.Collections;
using System;
using System.Net.NetworkInformation;

public class S_Spawn_Ring : S_Spawners
{
	public StructRingSpawner _ringSpawnerData;

	[Serializable]
	public struct StructRingSpawner {
		public Transform    _TransformToFollow;
		public Vector3      _followOffset;
	}

	//Because this is inherited from S_Spawners, this overrrides the normal SpawnObject, which is called in by LateUpdate in the inherited script. Go there for calls
	public override void SpawnObject () {
		//Either spawn on an object the ring should be following, or spawn stationary in place.
		if (_ringSpawnerData._TransformToFollow != null)
		{
			Instantiate(_SpawnerData._TeleportSparkle, _ringSpawnerData._TransformToFollow.transform.position + _ringSpawnerData._followOffset, transform.rotation);
			_ObjectClone = (GameObject)Instantiate(_SpawnerData._ObjectToSpawn, _ringSpawnerData._TransformToFollow.transform.position + _ringSpawnerData._followOffset, transform.rotation);
			_ObjectClone.transform.parent = _ringSpawnerData._TransformToFollow;
		}
		else
		{
			Instantiate(_SpawnerData._TeleportSparkle, transform.position, transform.rotation);
			_ObjectClone = (GameObject)Instantiate(_SpawnerData._ObjectToSpawn, transform.position, transform.rotation);
		}
	}

}
