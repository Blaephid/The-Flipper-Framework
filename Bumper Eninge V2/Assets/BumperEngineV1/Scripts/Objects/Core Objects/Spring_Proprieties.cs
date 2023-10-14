using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Spring_Proprieties : MonoBehaviour {

    public float SpringForce;
    public bool IsAdditive;
    public Transform BounceCenter;
    public Animator anim { get; set; }
    public bool LockControl = false;
    public float LockTime = 60;

    public Vector3 lockGravity = new Vector3(0f, -1.5f, 0f);
    public bool lockAirMoves = true;
    public float lockAirMovesTime = 30f;


    void Start()
    {
        anim = GetComponent<Animator>();
        //Gravity.y = 56.5f * PlayerGravity;
    }
}
