using UnityEngine;
using System.Collections;
using System.ComponentModel;
using UnityEditor;

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
	public CameraControlType            _whatType;
	[ColourIfNull(1,0,0,1)]
	public Transform                    _Direction;
	[HideInInspector] public Vector3                _forward;

	[Tooltip("If true, all of the above effects will be undone when the player leaves the trigger (but the rotation will not).")]
	public bool _willReleaseOnExit = false;


	[Header("Turning")]
	[Tooltip("How quickly the camera will rotate to face the trigger direction."), DrawHorizontalWithOthers(new string[] {"_duration"})]
	public float _faceSpeed = 2f;
	[Min(0), Tooltip("How long the camera will be looking in this direction. If 0, will be forever. This includes time to face that direction."), HideInInspector]
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

	public bool _willOffsetTarget;
	public bool _asLocalOffset;
	public Vector3 _newOffset;
	[Tooltip("How many frames it takes to reach this offset")]
	public float _framesToOffset;
	[Tooltip("If true, the offset will be unaffected by any HedgeCamera calculations that move the offset. If false, the offset will be affected by other offsets like input direction")]
	public bool _overWriteAllOffsets;
	[BaseColour(0.6f,0.6f,0.6f,1)]
	public Mesh _MeshToDraw;
	private Vector3 _offsetReferenceInWorld;


	private void Awake () {
		if (_Direction == null)
		{
			_Direction = transform;
		}

		_forward = _Direction.forward;
	}

#if UNITY_EDITOR

	public override void DrawAdditionalGizmos ( bool selected, Color colour ) {
		if (_hasTrigger)
		{
			S_S_DrawingMethods.DrawArrowHandle(colour, transform, 0.4f, true);
		}

		if (_willOffsetTarget)
		{
			if (_MeshToDraw) {
				Gizmos.color = selected ? new Color(0, 0, 0, 0.1f) : new Color(0, 0, 0, 0.02f);
				if (_asLocalOffset)
					Gizmos.DrawWireMesh(_MeshToDraw, transform.position, Quaternion.LookRotation(transform.forward, Vector3.up), Vector3.one * 10);
				else
					Gizmos.DrawWireMesh(_MeshToDraw, transform.position, Quaternion.identity, Vector3.one * 10);
			}

			Gizmos.color =  colour;
			Gizmos.DrawWireSphere(_offsetReferenceInWorld, 0.5f);
		}
	}

	public override void AdditionalTriggerSceneGUI () {
		if (_isSelected && _willOffsetTarget)
		{
			Vector3 currentPos = transform.position;
			currentPos += _asLocalOffset ? transform.rotation * _newOffset : _newOffset;

			_offsetReferenceInWorld = Handles.FreeMoveHandle(currentPos, 3f, Vector3.zero, Handles.RectangleHandleCap);

			_newOffset = (_asLocalOffset ? transform.rotation * _offsetReferenceInWorld : _offsetReferenceInWorld) - transform.position;
		}
	}
#endif
}
