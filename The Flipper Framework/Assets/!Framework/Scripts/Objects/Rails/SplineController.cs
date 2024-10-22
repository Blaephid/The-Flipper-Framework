﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class SplineController : MonoBehaviour
{
    S_PlayerPhysics PlayerPhys;
    public GameObject basePlayer;
    S_PlayerInput PlayerInput;
    public Spline activeSpline;
    // Start is called before the first frame update
    void Start()
    {
        PlayerPhys = basePlayer.GetComponent<S_PlayerPhysics>();
        PlayerInput = basePlayer.GetComponent<S_PlayerInput>();
    }

    private void FixedUpdate()
    {
        //Debug.Log(activeSpline);

        if (activeSpline != null)
        {
            

            //Get Sonic's current position along the spline
            CurveSample cur = activeSpline.GetSampleAtDistance(GetClosestPos(PlayerPhys._RB.position));
            //Get the Right vector of the current spline position so we can accurately adjust Sonic's velocity
            Vector3 SplinePlane = Vector3.Cross(cur.tangent, cur.up);

            //Project the vector onto the plane
            PlayerPhys._RB.velocity = Vector3.ProjectOnPlane(PlayerPhys._RB.velocity, SplinePlane);

            //Project the input too
            //PlayerInput.InputDir = Vector3.ProjectOnPlane(PlayerInput.InputDir, SplinePlane);
            //PlayerPhys.RawInput = Vector3.ProjectOnPlane(PlayerPhys.RawInput, SplinePlane);w

            //Set the Player's position along the spline plane
            Vector3 NewPos = activeSpline.transform.TransformPoint(cur.location);
            NewPos.y = PlayerPhys._RB.position.y;
            Debug.DrawLine(transform.position, NewPos);
            PlayerPhys._RB.position = Vector3.MoveTowards(PlayerPhys._RB.position, NewPos, 1f);
        }
    }

    /// <summary>
    /// Returns the Spline Position closest to the given Transform's position
    /// </summary>
    public float GetClosestPos(Vector3 ColPos)
    {
        float ClosestSample = 0;
        float CurrentDist = 9999999f;
        for (float n = 0; n < activeSpline.Length; n += Time.deltaTime * 10f)
        {
            float dist = ((activeSpline.GetSampleAtDistance(n).location + activeSpline.transform.position) - ColPos).sqrMagnitude;
            if (dist < CurrentDist)
            {
                CurrentDist = dist;
                ClosestSample = n;
            }

        }
        return ClosestSample;
    }
}
