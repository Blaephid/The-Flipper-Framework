using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Data_HomingTarget : MonoBehaviour {

    public Transform center;
    public Vector3 offset = Vector3.up;

    private void Start()
    {
        offset = transform.rotation * offset;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (center == null)
        {
            Gizmos.DrawWireSphere(transform.position + (transform.rotation * offset), 2);
        } else
        {
            Gizmos.DrawWireSphere(transform.position + (center.position - transform.position), 2);
        }
    }

}
