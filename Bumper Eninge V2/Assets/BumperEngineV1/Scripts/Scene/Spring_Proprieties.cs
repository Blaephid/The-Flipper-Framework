using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Spring_Proprieties : MonoBehaviour {

    public float SpringForce;
    public bool IsAdditive;
    public Transform BounceCenter;
    public Animator anim { get; set; }
    public bool LockControl = false;
    public float LockTime = 60;
    public bool LockGravity;
    Vector3 Gravity = new Vector3(0, 56.5f, 0);
    public float PlayerGravity = -1.5f;
    public bool DebugForce;
    public float LineLength = 30f;

    void Start()
    {
        anim = GetComponent<Animator>();
        Gravity.y = 56.5f * PlayerGravity;
    }



    private void OnDrawGizmosSelected()
    {
            if (DebugForce)
            {
                if (LineLength > 0)
                {
                    Gravity.y = 56.5f * PlayerGravity;
                    Vector3[] DebugTrajectoryPoints = PreviewTrajectory(BounceCenter.position, transform.up * SpringForce, Gravity, LineLength);

                    for (int i = 1; i < DebugTrajectoryPoints.Length; i++)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(DebugTrajectoryPoints[i - 1], DebugTrajectoryPoints[i]);
                    }
                }
            }

    }

    static Vector3[] PreviewTrajectory(Vector3 position, Vector3 velocity, Vector3 gravity, float time)
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
            vel = vel + (gravity * timeStep);
            pos = pos + (vel * timeStep);
            path[i] = pos;
        }
        return path;
    }

}
