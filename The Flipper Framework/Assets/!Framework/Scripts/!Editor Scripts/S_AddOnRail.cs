using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
using System;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public class S_AddOnRail : MonoBehaviour
{
    [Header("Updating in Editor")]
    public bool UpdateAll = false;
    public bool UpdateNow = false;
    public S_AddOnRail[] AddThis;
    public S_AddOnRail[] AddBehindThese;

    [Header("Main Rails")]
    public bool setConnectedOfOtherObjects;
    public S_AddOnRail nextRail;
    public S_AddOnRail PrevRail;

    [Header("Switch Rails")]
    public S_AddOnRail altNextRail;
    public S_AddOnRail altPrevRail;

    Vector3 offset;

    [HideInInspector] public bool toUpdate = false;


    private void OnEnable()
    {
        toUpdate = true;
    }


    private void OnValidate()
    {
        if (AddThis.Length == 0) return;
        toUpdate = true;
    }

    public void switchTrigger()
    {
        if(altNextRail != null)
        {
            S_AddOnRail temp = nextRail;
            nextRail = altNextRail;
            altNextRail = temp;
        }
        if(altPrevRail != null)
        {
            S_AddOnRail temp = PrevRail;
            PrevRail = altPrevRail;
            altPrevRail = temp;
        }
    }


    public void Announce()
    {
        Debug.LogWarning("I am " +this+ " And next is " +nextRail);
    }


    private void Update()
    {
        // we can prevent the generated content to be updated during playmode to preserve baked data saved in the scene
        if (Application.isPlaying) return;

        if(UpdateAll)
        {
            S_AddOnRail[] railsMeshes = FindObjectsOfType<S_AddOnRail>();
            foreach (S_AddOnRail add in railsMeshes)
            {
                if (add.AddThis.Length != 0)
                {
                    add.toUpdate = true;
                }
            }
            UpdateAll = false;
        }

        if(setConnectedOfOtherObjects)
        {
            if (PrevRail != null)
                PrevRail.nextRail = this;

            if (nextRail != null)
                nextRail.PrevRail = this;

            if (altNextRail != null)
                altNextRail.PrevRail = this;

            if(altPrevRail != null)
                altPrevRail.nextRail = this;

            setConnectedOfOtherObjects = false;
        }

        if (toUpdate)
        {
            toUpdate = false;
            Place();
        }
    }

    // Update is called once per frame
    void Place()
    {
        offset = new Vector3 (GetComponent<S_PlaceOnSpline>().Offset3d.x, 0, 0);
        //offset = Vector3.zero;

        if (AddThis.Length > 0)
        {
            foreach (S_AddOnRail rail in AddThis)
            {
                S_Spline thisSpline = GetComponentInParent<S_Spline>();
                S_Spline otherSpline = rail.GetComponentInParent<S_Spline>();

                CurveSample sample = thisSpline.GetSampleAtDistance(thisSpline.Length);
                offset = sample.Rotation * offset;
                Debug.DrawRay(thisSpline.gameObject.transform.position + thisSpline.nodes[thisSpline.nodes.Count - 1].Position, offset, Color.red, 30f);

                otherSpline.gameObject.transform.parent.position = thisSpline.gameObject.transform.parent.position + offset;
                otherSpline.nodes[0].Position = thisSpline.nodes[thisSpline.nodes.Count - 1].Position;
                otherSpline.nodes[0].Direction = thisSpline.nodes[thisSpline.nodes.Count - 1].Direction;
            }

            foreach(S_AddOnRail rail in AddBehindThese)
            {
                S_Spline thisSpline = GetComponentInParent<S_Spline>();
                S_Spline otherSpline = rail.GetComponentInParent<S_Spline>();

                CurveSample sample = thisSpline.GetSampleAtDistance(thisSpline.Length);
                offset = sample.Rotation * offset;

                otherSpline.gameObject.transform.parent.position = thisSpline.gameObject.transform.parent.position + offset;
                otherSpline.nodes[otherSpline.nodes.Count - 1].Position = thisSpline.nodes[0].Position;
                otherSpline.nodes[otherSpline.nodes.Count - 1].Direction = thisSpline.nodes[0].Direction;
            }
            
        }
    }

}
