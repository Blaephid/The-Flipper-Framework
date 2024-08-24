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
	public Transform    Direction;
	[HideInInspector]
	public Vector3     forward;

	[Header("Turning")]
	[Tooltip("How quickly the camera will rotate to face the trigger direction.")]
	public float faceSpeed = 2f;
	[Min(0), Tooltip("How long the camera will be looking in this direction. If 0, will be forever.")]
	public float duration = 1;
	[Tooltip("If true, then the camera rotation following player (where left is down if running along a wall) will be taken control of, making the roll upwards the same as the trigger.")]
	public bool willRotateCameraUpToThis;
	[Tooltip("If true, then the camera will face in the vertical direction based on the trigger transform as well.")]
	public bool willRotateVertically; 
	[Header("Effects")]
	[Tooltip("If true, the camera will look up and down to match this new angle (overwritse rotating vertically)")]
	public bool willChangeAltitude;
	[Tooltip("If above is true, the player camera will locally rotate to this new angle, from -90 to 90 where 90 - directly above player."),Range(-90, 90)]
	public float newAltitude;
	[Tooltip("If true, the camera distance from character will be overwritten.")]
	public bool willChangeDistance = false;
	[Tooltip("The new distance the camera will be from the character, will still be changed based on running speed and collisions.")]
	public float newDistance;
	[Tooltip("If true, the distance given will still be affected by distance changes based on player speed, handled already in HedgeCamera")]
	public bool affectNewDistanceBySpeed;
	[Tooltip("If true, all of the above effects will be undone when the player leaves the trigger (but the rotation will not).")]
	public bool ReleaseOnExit = false;


	private void Awake () {
		if (Direction == null)
		{
			Direction = transform;
		}

		forward = Direction.forward;
	}
}
