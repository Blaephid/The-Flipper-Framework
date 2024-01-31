using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class setRailsOnPlatform : MonoBehaviour
{
    public bool Update;

    public S_Spline rail;
    public Transform start;
    public Transform end;

    public Vector3 offset = new Vector3(0, 2, 0);

    private void OnValidate()
    {
        if(Update)
        {
            Place();
        }
    }

    void Place()
    {
        rail.transform.position = gameObject.transform.position + (gameObject.transform.rotation * offset);


        Vector3 startPos = (start.position - gameObject.transform.position).normalized * Vector3.Distance(gameObject.transform.position, start.position);
        rail.nodes[0].Position = startPos;
        rail.nodes[0].Direction = (gameObject.transform.position - start.position).normalized;
        rail.nodes[0].Up = gameObject.transform.up;

        Vector3 endPos = (end.position - gameObject.transform.position).normalized * Vector3.Distance(gameObject.transform.position, end.position);
        rail.nodes[1].Position = endPos;
        rail.nodes[1].Direction = (end.position - gameObject.transform.position).normalized * 50f;
        rail.nodes[1].Up = gameObject.transform.up;

        Update = false;
    }

}
