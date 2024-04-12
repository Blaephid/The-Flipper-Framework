using UnityEngine;
using System.Collections;

public enum TriggerType
{
	LockToDirection, SetFree, SetFreeAndLookTowards, Reverse, ReverseAndLockControl, justEffect
}

public class S_Trigger_Camera : MonoBehaviour
{
	[Header("Functionality")]
	public TriggerType Type;

	[Header("Turning")]
	[Tooltip("How quickly the camera will rotate to face the trigger direction.")]
	public float faceSpeed = 2f;
	[Min(0), Tooltip("How long the camera will be looking in this direction. If 0, will be forever.")]
	public float duration = 1;
	[Tooltip("If this is true, then the camera rotation following player (where left is down if running along a wall) will be taken control of, making the roll upwards the same as the trigger.")]
	public bool willRotateCameraUpToThis;
	public bool willRotateVertically;
	[Header("Effects")]
	public bool willChangeAltitude;
	public bool willChangeDistance = false;
	public float newAltitude;
	public float newDistance;
	public bool ReleaseOnExit = false;

}
