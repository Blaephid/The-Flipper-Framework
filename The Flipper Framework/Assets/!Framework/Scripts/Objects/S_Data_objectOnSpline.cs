using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System;

[ExecuteInEditMode]
[SelectionBase]
[DisallowMultipleComponent]
public class S_Data_objectOnSpline : MonoBehaviour
{

	public enum WhatCanBeSpawned {
		Unlabelled = 0,
		Rings,
		Boosters,
		Springs,
	}

	public WhatCanBeSpawned whatToSpawn;

	[Header("Rings")]
	public S_Spawn_Enemy.StructSpawnerData _GeneralSpawner;
	public S_Spawn_Ring.StructRingSpawner _RingSpawner;

	[Header("Boosters")]
	public bool isBooster = false;
	public float speed;
	public bool setRailSpeed;
	public float addRailSpeed;

	[Header("Spring")]
	public bool isSpring;
	public float force = 100;
	public bool LockControl = true;
	public int  LockTime = 60;

	public Vector3 lockGravity = new Vector3(0f, -1.5f, 0f);
	public bool lockAirMoves = true;
	public float lockAirMovesTime = 30f;



	public void affectObject ( GameObject go ) {
		switch (whatToSpawn)
		{
			case WhatCanBeSpawned.Rings:
				setRings(go); break;
			case WhatCanBeSpawned.Boosters: 
				setBoosters(go); break;
			case WhatCanBeSpawned.Springs: 
				setSprings(go); break;
		}
	}



	void setRings ( GameObject go ) {
		S_Spawn_Ring goRing = go.GetComponent<S_Spawn_Ring>();
		goRing._ringSpawnerData = _RingSpawner;
		goRing._SpawnerData = _GeneralSpawner;
	}

	void setBoosters ( GameObject go ) {
		S_Data_SpeedPad pad = go.GetComponent<S_Data_SpeedPad>();

		pad._speedToSet_ = speed;
		pad._addSpeed_ = addRailSpeed;
		pad._willSetSpeed_ = setRailSpeed;

	}

	void setSprings ( GameObject go ) {
		S_Data_Spring spring = go.GetComponent<S_Data_Spring>();
		spring._springForce_ = force;
		spring._willLockControl_ = LockControl;
		spring._lockForFrames_ = LockTime;
		spring._overwriteGravity_ = lockGravity;
		spring._lockAirMovesTime_ = lockAirMovesTime;
	}
}
