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
        if (isRings && go.GetComponent<S_Spawn_Ring_Eternal>())
        {
            setRings(go);
        }
        else if (isBooster && go.GetComponent<S_Data_SpeedPad>())
        {
            setBoosters(go);
        }
        else if(isSpring && go.GetComponent<S_Data_Spring>())
        {
            setSprings(go);
        }
    }

    

    void setRings(GameObject go)
    {
        S_Spawn_Ring_Eternal goRing = go.GetComponent<S_Spawn_Ring_Eternal>();
        goRing.autoRespawn = respawnAuto;
        goRing.Distance = spawnDistance;
        goRing.RespawnTime = respawnTime;
        goRing.respawnOnDeath = respawnOnDeath;
    }

    void setBoosters(GameObject go)
    {
        S_Data_SpeedPad pad = go.GetComponent<S_Data_SpeedPad>();

        pad.Speed = speed;
        pad.addSpeed = addRailSpeed;
        pad.setSpeed = setRailSpeed;
        
    }

    void setSprings(GameObject go)
    {
        S_Data_Spring spring = go.GetComponent<S_Data_Spring>();
        spring.SpringForce = force;
        spring.LockControl = LockControl;
        spring.LockTime = LockTime;
        spring.lockGravity = lockGravity;
        spring.lockAirMoves = lockAirMoves;
        spring.lockAirMovesTime = lockAirMovesTime;
    }
}
