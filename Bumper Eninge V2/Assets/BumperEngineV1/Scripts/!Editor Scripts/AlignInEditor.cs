using UnityEngine;
using System.Collections;
/* Written by Elmar Hanlhofer  http://www.plop.at  2015 06 10*/

[ExecuteInEditMode]
public class AlignInEditor : MonoBehaviour
{
    public enum Direction
    {
        Down,ObjectRotation,Up
    }

    public bool SurfaceAlignment = true;
    public Direction SnapDirection = Direction.Down;
    public bool LineToTargetPos = false;
    [Space]
    public bool Align = false;
    public bool RandomRotation = false;
    [Space]
    public bool RemoveScript = false;



    private void Update()
    {
        if (LineToTargetPos)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position + (Vector3.up * 0.02f), Vector3.down);

            switch (SnapDirection)
            {
                case Direction.Down:
                    break;
                case Direction.Up:
                    ray = new Ray(transform.position + (Vector3.down * 0.02f), Vector3.up);
                    break;
                case Direction.ObjectRotation:
                    ray = new Ray(transform.position + (transform.up * 0.02f), -transform.up);
                    break;
            }
            if (Physics.Raycast(ray, out hit))
            {
                Debug.DrawLine(transform.position, hit.point);
            }
        }

        if (Align) { SetAlign(); Align = false; }
        if (RandomRotation) { SetRandomRotation(); RandomRotation = false; }

        if (RemoveScript) { SetRemoveScript(); RemoveScript = false; }

    }

    public void SetAlign()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + (Vector3.up * 0.02f), Vector3.down);

        switch (SnapDirection)
        {
            case Direction.Down:
                break;
            case Direction.Up:
                ray = new Ray(transform.position + (Vector3.down * 0.02f), Vector3.up);
                break;
            case Direction.ObjectRotation:
                ray = new Ray(transform.position + (transform.up * 0.02f), -transform.up);
                break;
        }
        if (Physics.Raycast(ray, out hit))
        {
            transform.position = hit.point;
            if (SurfaceAlignment) transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            Debug.Log(transform.name + " aligned.",this);
        }
        else
        {
            Debug.Log("No surface found for " + transform.name,this);
        }
    }

    public void SetRandomRotation()
    {
        transform.rotation = transform.rotation * Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    public void SetRemoveScript()
    {
        DestroyImmediate(this); //KIS
    }
}
