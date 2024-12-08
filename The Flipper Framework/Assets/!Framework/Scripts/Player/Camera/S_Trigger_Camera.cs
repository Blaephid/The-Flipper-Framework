using UnityEngine;
using System.Collections;

public enum CameraControlType
{
	LockToDirection, SetFree, SetFreeAndLookTowards, Reverse, ReverseAndLockControl, justEffect
}

public class S_Trigger_Camera : S_Trigger_Base
{
	[Header("Functionality")]
	public CameraControlType	_whatType;
	public Transform		_Direction;
	[HideInInspector]
	public Vector3		_forward;

	[Header("Turning")]
	[Tooltip("How quickly the camera will rotate to face the trigger direction.")]
	public float _faceSpeed = 2f;
	[Min(0), Tooltip("How long the camera will be looking in this direction. If 0, will be forever.")]
	public float _duration = 1;
	[Tooltip("If true, then the camera rotation following player (where left is down if running along a wall) will be taken control of, making the roll upwards the same as the trigger.")]
	public bool _willRotateCameraUpToThis;
	[Tooltip("If true, then the camera will face in the vertical direction based on the trigger transform as well.")]
	public bool _willRotateVertically; 
	[Header("Effects")]
	[Tooltip("If true, the camera will look up and down to match this new angle (overwritse rotating vertically)")]
	public bool _willChangeAltitude;
	[Tooltip("If above is true, the player camera will locally rotate to this new angle, from -90 to 90 where 90 - directly above player."),Range(-90, 90)]
	public float _newAltitude;
	[Tooltip("If true, the camera distance from character will be overwritten.")]
	public bool _willChangeDistance = false;
	[Tooltip("The new distance the camera will be from the character, will still be changed based on running speed and collisions.")]
	public float _newDistance;
	[Tooltip("If true, the distance given will still be affected by distance changes based on player speed, handled already in HedgeCamera")]
	public bool _affectNewDistanceBySpeed;
	[Tooltip("If true, all of the above effects will be undone when the player leaves the trigger (but the rotation will not).")]
	public bool _willReleaseOnExit = false;


	private void Awake () {
		if (_Direction == null)
		{
			_Direction = transform;
		}

		_forward = _Direction.forward;
	}

	public override void DrawAdditional () {
		S_S_EditorMethods.DrawArrowHandle(Color.clear, transform, 0.4f, true);
	}
}
