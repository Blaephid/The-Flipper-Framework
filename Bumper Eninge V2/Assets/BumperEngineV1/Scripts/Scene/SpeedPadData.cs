using UnityEngine;
using System.Collections;
using SplineMesh;

public class SpeedPadData : MonoBehaviour
{
    public bool Path;
    public float Speed;
    public bool LockToDirection;
    public bool LockControl;
    public float LockControlTime = 0.5f;
    public bool ChangeCameraDirection = true;
    public bool Snap;
    public bool isDashRing;
    public Vector3 lockGravity = new Vector3(0f, -1.5f, 0f);
    public bool lockAirMoves = false;
    public float lockAirMovesTime = 30f;
    public bool ResetRotation = true;
    public bool AffectCamera = true;
    public Spline path;
    public bool setInputForwards;

}

