using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class SplineTrigger : MonoBehaviour
{
    public Spline spline;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SplineController>())
        {
            SplineController s = other.GetComponent<SplineController>();
            if (s.activeSpline != spline)
                s.activeSpline = spline;
            else if (s.activeSpline == spline)
                s.activeSpline = null;
        }
    }
}
