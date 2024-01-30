using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System;

public class S_Control_PulleyObject : MonoBehaviour {
    public Spline Rail;
    CapsuleCollider sphcol;
    [SerializeField]
    bool placeOnEnd;
    [SerializeField]
    float offset = 0.5f;
    public GameObject homingtgt;

    Rigidbody rb;
	// Use this for initialization
	void Start () 
    {
        rb = GetComponent<Rigidbody>();
        Rail = GetComponentInParent<Spline>();
        sphcol = GetComponent<CapsuleCollider>();
        homingtgt = GetComponentInChildren<S_Data_HomingTarget>().gameObject;
        PlaceOnRope();        
    }
    private void Update()
    {   
        if(!sphcol.enabled)
        {
            if (homingtgt != null)
                homingtgt.SetActive(false);
            //this.enabled = false;            
        }
        
    }

    private void OnEnable()
    {
        S_Manager_LevelProgress.onReset += ReturnToRope;
    }

    private void OnDisable()
    {
        S_Manager_LevelProgress.onReset -= ReturnToRope;
    }


    void ReturnToRope(object sender, EventArgs e)
    {
        //Debug.Log("Invoke Pulley");
        sphcol.enabled = true;
        homingtgt.SetActive(true);
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;

        PlaceOnRope();
    }

    void PlaceOnRope()
    {
        CurveSample sample = (placeOnEnd) ? Rail.GetSampleAtDistance(Rail.Length - offset) : Rail.GetSampleAtDistance(offset);
        transform.position = sample.location + Rail.transform.position;
        
        //Vector3 dir = Rail.GetSampleAtDistance(1).location - Rail.GetSampleAtDistance(0).location;
        Vector3 dir;
        if (placeOnEnd)
        {
            dir = Rail.GetSampleAtDistance(Rail.Length - offset).location - Rail.GetSampleAtDistance(Rail.Length - 1 - offset).location;
        }
        else
        {
            dir = Rail.GetSampleAtDistance(1 + offset).location - Rail.GetSampleAtDistance(offset).location;
        }
        transform.rotation = Quaternion.LookRotation(dir, sample.up);
    }
}
