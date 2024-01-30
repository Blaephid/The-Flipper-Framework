using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class railSwitch : MonoBehaviour
{
    MeshRenderer mesh;

    [Header("Materials")]
    public Material blueLight;
    public Material redLight;

    [Header("Rails")]
    public GameObject blueRails;
    public GameObject redRails;

    [Header("Connected Rails")]
    public AddOnRail[] connectedRails;
    public List<railSwitch> linkedSwitches;

    [Header("Stats")]
    public float delay;

    bool blue = true;


    float timer;
    bool active = true;

    public bool main;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        redRails.SetActive(false);


        foreach (railSwitch child in transform.parent.GetComponentsInChildren<railSwitch>())
        {
            linkedSwitches.Add(child);
        }
    }

    private void OnEnable()
    {
        if (main)
        {
            S_Manager_LevelProgress.onReset += Reset;
        }
    }

  

    private void OnDestroy()
    {
        if (main)
        {
            //LevelProgressControl.onReset -= Reset;
        }

    }

    void Reset(object sender, EventArgs e)
    {
        if(main)
        {

            if (!blue && connectedRails.Length > 0)
            {
                foreach (AddOnRail rail in connectedRails)
                {
                    rail.switchTrigger();
                }
            }

            setBlue();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;

            if(timer <= 0)
            {
                timer = 0;
                active = true;
            }    
        }
    }

    public void setBlue()
    {
        redRails.SetActive(false);
        blueRails.SetActive(true);

        if (linkedSwitches.Count > 0)
        {
            foreach (railSwitch switche in linkedSwitches)
            {
                switche.beBlue();
            }
        }
    }

    public void beBlue()
    {
        blue = true;
        mesh.material = blueLight;
    }

    void setRed()
    {   
        blueRails.SetActive(false);
        redRails.SetActive(true);

        if (linkedSwitches.Count > 0)
        {
            foreach (railSwitch switche in linkedSwitches)
            {
                switche.beRed();
            }
        }
    }

    public void beRed()
    {
        blue = false;
        mesh.material = redLight;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && active)
        {

            active = false;
            timer = delay;

            if(connectedRails.Length > 0)
            {
                foreach (AddOnRail rail in connectedRails)
                {
                    rail.switchTrigger();
                }
            }
            

            if (!blue)
            {
                setBlue();
            }
            else
            {
                setRed();
            }

        }
    }
}
