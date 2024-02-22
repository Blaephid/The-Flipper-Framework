using UnityEngine;
using System.Collections;

public enum TriggerType
{
    LockToDirection, SetFree, SetFreeAndLookTowards, Reverse, ReverseAndLockControl, justEffect
}

public class S_Trigger_Camera : MonoBehaviour {

    public TriggerType Type;
    public float FaceSpeed = 2f;
    public float CameraAltitude;
    public float ChangeDistance;
    public bool changeAltitude;
    public bool changeDistance = false;
	public bool ReleaseOnExit = false;
	public float duration = 1;

}
