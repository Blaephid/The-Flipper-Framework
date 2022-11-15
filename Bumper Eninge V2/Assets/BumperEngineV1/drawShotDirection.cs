using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawShotDirection : MonoBehaviour
{
    public bool DebugForce;
    public float LineLength = 30f;

    public bool isSpring;
    public bool isBoostRing;
    public Spring_Proprieties spring;
    public SpeedPadData speedPad;



    public float Force;
    public Transform ShotCenter;

    public Vector3 lockGravity = Vector3.zero;
    public Vector3 FallGravity = new Vector3(0, -1.5f, 0);
    public Vector3 UpGravity = new Vector3(0, -1.7f, 0);
    public float PlayerDecell = 1.005f;
    
    


    // Start is called before the first frame update
    void Start()
    {
        Destroy(this);
    }

    private void OnDrawGizmosSelected()
    {
        if (DebugForce)
        { 

            if (LineLength > 0)
            {
                Vector3[] DebugTrajectoryPoints;
                //Gravity.y = 56.5f * PlayerGravity;
                if (isSpring)
                {
                    Force = spring.SpringForce;
                    lockGravity = spring.lockGravity;
                    DebugTrajectoryPoints = PreviewTrajectory(ShotCenter.position, transform.up * Force, FallGravity, UpGravity, lockGravity, LineLength, PlayerDecell);
                }
                else
                {
                    Force = speedPad.Speed;
                    lockGravity = speedPad.lockGravity;
                    DebugTrajectoryPoints = PreviewTrajectory(ShotCenter.position, transform.forward * Force, FallGravity, UpGravity, lockGravity, LineLength, PlayerDecell) ;
                }
                

                for (int i = 1; i < DebugTrajectoryPoints.Length; i++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(DebugTrajectoryPoints[i - 1], DebugTrajectoryPoints[i]);
                }
            }
        }

    }

    static Vector3[] PreviewTrajectory(Vector3 position, Vector3 velocity, Vector3 fallGravity, Vector3 upGravity, Vector3 lockGravity, float time, float decell)
    {
        float timeStep = Time.fixedDeltaTime;
        int iterations = Mathf.CeilToInt(time / timeStep);

        if(lockGravity != null)
        {
            fallGravity = lockGravity;
        }

        if (iterations < 2)
        {
            Debug.LogError("PreviewTrajectory (Vector3, Vector3, Vector3, float, float): Unable to preview trajectory shorter than Time.fixedDeltaTime * 2");
            return new Vector3[0];
        }
        Vector3[] path = new Vector3[iterations];
        Vector3 pos = position;
        Vector3 vel = velocity;
        path[0] = pos;
        for (int i = 1; i < iterations; i++)
        {
            //    if(new Vector3 (vel.x, 0f, vel.x).sqrMagnitude > 196)
            //         vel = new Vector3(vel.x / decell, vel.y, vel.z / decell);

            vel = new Vector3(vel.x / decell, vel.y, vel.z / decell);
            vel = vel + ApplyGravity((int)vel.y, fallGravity, upGravity);
            pos = pos + (vel * timeStep);
            path[i] = pos;
        }
        return path;
    }

    static Vector3 ApplyGravity(int vertSpeed, Vector3 fallGravity, Vector3 upGravity)
    {
        if (vertSpeed < 5)
        {
            return fallGravity;
        }
        else
        {
            int gravMod;
            if (vertSpeed > 70)
                gravMod = vertSpeed / 15;
            else
                gravMod = vertSpeed / 12;
            float applyMod = 1 + (gravMod * 0.1f);

            Vector3 newGrav = new Vector3(0f, upGravity.y * applyMod, 0f);

            return newGrav;
        }
    }

}
