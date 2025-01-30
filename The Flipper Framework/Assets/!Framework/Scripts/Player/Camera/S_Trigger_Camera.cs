using UnityEngine;
using System.Collections;
using System.ComponentModel;

public enum CameraControlType
{
	LockToDirection, SetFree, SetFreeAndLookTowards, Reverse, ReverseAndLockControl, justEffect
}

public class S_Trigger_Camera : S_Trigger_Base
{
	public S_Trigger_Camera () {
		_TriggerObjects._isLogicInPlayScript = true;
	}

	[Header("Functionality")]
	public CameraControlType	_whatType;
	public Transform		_Direction;
	[HideInInspector]
	public Vector3		_forward;


	[Header("Turning")]
	[Tooltip("How quickly the camera will rotate to face the trigger direction."), DrawHorizontalWithOthers(new string[] {"_duration"})]
	public float _faceSpeed = 2f;
	[Min(0), Tooltip("How long the camera will be looking in this direction. If 0, will be forever."), HideInInspector]
	public float _duration = 1;
	[Tooltip("If true, then the camera rotation following player (where left is down if running along a wall) will be taken control of, making the roll upwards the same as the trigger.")]
	public bool _setCameraReferenceWorldRotation;
	[Tooltip("If true, then the camera will face in the vertical direction based on the trigger transform as well.")]
	public bool _willRotateVertically;


	[Header("Effects")]
	[Tooltip("If true, the distance given will still be affected by distance changes based on player speed, handled already in HedgeCamera"), DrawHorizontalWithOthers(new string[] {"_affectNewFOVBySpeed"})]
	public bool _affectNewDistanceBySpeed = true;
	[Tooltip("If true, the FOV given will still be affected by distance changes based on player speed, handled already in HedgeCamera"), HideInInspector] 
	public bool _affectNewFOVBySpeed;


	[HideInInspector] public bool _willChangeAltitude;
	[DrawTickBoxBefore("_willChangeAltitude")]
	[Tooltip("If true, ignore direction of trigger and use custom local angle for camera. From -90 to 90. Speed determined by rotation. "),Range(-90, 90)]
	public float _newAltitude;

	[HideInInspector] public bool _willChangeDistance = false;
	[DrawTickBoxBefore("_willChangeDistance")]
	[Tooltip("X is the new distance from the camera, y is how many frames to reach it. Distance will still be changed based on running speed and collisions.")]
	public Vector2 _newDistance;

	[HideInInspector] public bool _willChangeFOV = false;
	[DrawTickBoxBefore("_willChangeFOV")]
	[Tooltip("X is the new FOV for the view camera, Y is how many frames to reach it. It will still be changed based on running speed and collisions.")]
	public Vector2 _newFOV = new Vector2 (100, 5);

	[Tooltip("If true, all of the above effects will be undone when the player leaves the trigger (but the rotation will not).")]
	public bool _willReleaseOnExit = false;

	private void Awake () {
		if (_Direction == null)
		{
			_Direction = transform;
		}

		_forward = _Direction.forward;
	}

#if UNITY_EDITOR
	public override void DrawTriggerAdditional (Color colour) {
		if(_TriggerObjects._triggerSelf)
			S_S_DrawingMethods.DrawArrowHandle(colour, transform, 0.4f, true);
	}
#endif
}
