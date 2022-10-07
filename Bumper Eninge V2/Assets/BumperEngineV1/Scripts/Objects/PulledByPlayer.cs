using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulledByPlayer : MonoBehaviour
{
    public Transform Player;
    public bool Pulled;
    public float PulledSpeed = 2f;
    public BoxCollider boxcol;
    Transform transform;

    private void Start()
    {
        transform = GetComponent<Transform>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Pulled && Player != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, Player.position, PulledSpeed);
        }
    }
}
