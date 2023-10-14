using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System;

[ExecuteInEditMode]
[SelectionBase]
[DisallowMultipleComponent]
public class objectOnSpline : MonoBehaviour
{

    [Header("Rings")]
    public bool isRings;
    public float spawnDistance = 400;
    public bool respawnAuto;
    public float respawnTime = 15;
    public bool respawnOnDeath;

    [Header("Boosters")]
    public bool isBooster = false;
    public float speed;
    public bool setRailSpeed;
    public float addRailSpeed;

    [Header("Spring")]
    public bool isSpring;
    public float force = 100;
    public bool LockControl = true;
    public float LockTime = 60;

    public Vector3 lockGravity = new Vector3(0f, -1.5f, 0f);
    public bool lockAirMoves = true;
    public float lockAirMovesTime = 30f;



    public void affectObject(GameObject go)
    {
        if (isRings && go.GetComponent<RingSpawnerEternal>())
        {
            setRings(go);
        }
        else if (isBooster && go.GetComponent<SpeedPadData>())
        {
            setBoosters(go);
        }
        else if(isSpring && go.GetComponent<Spring_Proprieties>())
        {
            setSprings(go);
        }
    }

    

    void setRings(GameObject go)
    {
        RingSpawnerEternal goRing = go.GetComponent<RingSpawnerEternal>();
        goRing.autoRespawn = respawnAuto;
        goRing.Distance = spawnDistance;
        goRing.RespawnTime = respawnTime;
        goRing.respawnOnDeath = respawnOnDeath;
    }

    void setBoosters(GameObject go)
    {
        SpeedPadData pad = go.GetComponent<SpeedPadData>();

        pad.Speed = speed;
        pad.addSpeed = addRailSpeed;
        pad.setSpeed = setRailSpeed;
        
    }

    void setSprings(GameObject go)
    {
        Spring_Proprieties spring = go.GetComponent<Spring_Proprieties>();
        spring.SpringForce = force;
        spring.LockControl = LockControl;
        spring.LockTime = LockTime;
        spring.lockGravity = lockGravity;
        spring.lockAirMoves = lockAirMoves;
        spring.lockAirMovesTime = lockAirMovesTime;
    }
}
