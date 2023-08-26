using UnityEngine;
using System.Collections;
using SplineMesh;

public class SpeedPadData : MonoBehaviour
{
    [Header ("Type")]
    public bool Path;
    public bool onRail;
    public bool railBackwards;
    public bool isDashRing;
    public Spline path;
    public bool setSpeed = true;
    public float addSpeed = 15;

    [Header("Effects")]
    public float Speed;
    public bool LockToDirection;
    
    public bool Snap;
    public bool AffectCamera = true;
    public bool ResetRotation = true;
    public bool setInputForwards;

    [Header("Lock?")]
    public Transform positionToLockTo;
    public bool LockControl;
    public float LockControlTime = 0.5f;
    public Vector3 lockGravity = new Vector3(0f, -1.5f, 0f);
    public bool lockAirMoves = false;
    public float lockAirMovesTime = 30f;
    
}

