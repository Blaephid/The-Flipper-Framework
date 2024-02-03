using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

[ExecuteInEditMode]
public class S_PulleyBasePlacement : MonoBehaviour
{
    public S_Spline Rail;
    public bool isEnd = false,isAlign = false;
    public float Range = 0f;
    public bool ChangeRotation = true;
    public float Rotate = 0f;
    public Vector2 XZ;

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (Rail == null) GetComponentInParent<S_Spline>();
            CurveSample sample = (!isEnd) ? Rail.GetSampleAtDistance(Range) : Rail.GetSampleAtDistance(Rail.Length - Range);
            transform.position = sample.location + Rail.transform.position;
            Vector3 dir = Vector3.zero;
            if (isEnd)
            {
                dir = Rail.GetSampleAtDistance(Rail.Length - Range).location - Rail.GetSampleAtDistance(Rail.Length - 1 - Range).location;
            }
            else
            {
                dir = Rail.GetSampleAtDistance(Range + 1).location - Rail.GetSampleAtDistance(Range).location;
            }
            dir.y = 0;
            if (ChangeRotation)transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            if (isAlign) transform.rotation = sample.Rotation;
            if(ChangeRotation)transform.rotation = Quaternion.Euler(XZ.x, Rotate, XZ.y) * transform.rotation;
        }
#endif
    }
}
