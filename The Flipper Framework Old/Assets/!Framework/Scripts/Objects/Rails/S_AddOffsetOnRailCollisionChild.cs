using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class S_AddOffsetOnRailCollisionChild : MonoBehaviour
{

    public Vector3 offSet;

    // Start is called before the first frame update
    void Start()
    {
        
    }



    [ContextMenu("Update")]
    public void doUpdate()
    {
        S_PlaceOnSpline s = GetComponentInChildren<S_PlaceOnSpline>();
        if(s != null )
        {
            s.Offset3d = offSet;
        }

    }

}
