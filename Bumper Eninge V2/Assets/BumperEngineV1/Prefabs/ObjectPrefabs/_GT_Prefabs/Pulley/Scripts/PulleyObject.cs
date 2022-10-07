using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class PulleyObject : MonoBehaviour {
    public Spline Rail;
    CapsuleCollider sphcol;
    [SerializeField]
    bool placeOnEnd;
    [SerializeField]
    float offset = 0.5f;
    HomingTarget homingtgt;
	// Use this for initialization
	void Start () {
        Rail = GetComponentInParent<Spline>();
        sphcol = GetComponent<CapsuleCollider>();
        homingtgt = GetComponent<HomingTarget>();
        CurveSample sample =(placeOnEnd) ? Rail.GetSampleAtDistance(Rail.Length - offset): Rail.GetSampleAtDistance(offset);
        transform.position = sample.location + Rail.transform.position;
        //Vector3 dir = Rail.GetSampleAtDistance(1).location - Rail.GetSampleAtDistance(0).location;
        Vector3 dir = Vector3.zero;
        if (placeOnEnd)
        {
            dir = Rail.GetSampleAtDistance(Rail.Length - offset).location - Rail.GetSampleAtDistance(Rail.Length - 1 - offset).location;
        }
        else
        {
            dir = Rail.GetSampleAtDistance(1 + offset).location - Rail.GetSampleAtDistance(offset).location;
        }
        transform.rotation = Quaternion.LookRotation(dir, transform.up);
    }
    private void Update()
    {   
        if(!sphcol.enabled)
        {
            if(homingtgt != null)
                homingtgt.enabled = false;
            this.enabled = false;            
        }
        
    }
}
