using UnityEngine;
using System.Collections;

public enum TriggerType
{
    LockToDirection, SetFree, SetFreeAndLookTowards
}

public class CameraTriggerData : MonoBehaviour {

    public TriggerType Type;
    public float FaceSpeed = 1f;
    public float CameraAltitude;
    public float ChangeDistance;
    public bool changeDistance = false;
	public bool ReleaseOnExit = false;

}
