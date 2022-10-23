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

    public Vector3 Gravity = new Vector3(0, -1.5f, 0);
    public float PlayerDecell = 1.05f;
    
    


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
                    DebugTrajectoryPoints = PreviewTrajectory(ShotCenter.position, transform.up * Force, Gravity, LineLength, PlayerDecell);
                }
                else
                {
                    Force = speedPad.Speed;
                    DebugTrajectoryPoints = PreviewTrajectory(ShotCenter.position, transform.forward * Force, Gravity, LineLength, PlayerDecell);
                }
                

                for (int i = 1; i < DebugTrajectoryPoints.Length; i++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(DebugTrajectoryPoints[i - 1], DebugTrajectoryPoints[i]);
                }
            }
        }

    }

    static Vector3[] PreviewTrajectory(Vector3 position, Vector3 velocity, Vector3 gravity, float time, float decell)
    {
        float timeStep = Time.fixedDeltaTime;
        int iterations = Mathf.CeilToInt(time / timeStep);
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
            vel = new Vector3(vel.x / decell, vel.y, vel.z / decell);
            //vel = vel + (gravity * timeStep);
            vel = vel + gravity;
            pos = pos + (vel * timeStep);
            path[i] = pos;
        }
        return path;
    }
}
