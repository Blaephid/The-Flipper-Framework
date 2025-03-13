using UnityEngine;
using System.Collections;
using System.ComponentModel;
using UnityEditor;
using SplineMesh;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public enum enumCameraControlType
{
	SetToDirection, SetToInfrontOfCharacter, SetToBehindCharacter, OnlyApplyEffects, RemoveEffects, SetToViewTarget
}

public class S_Trigger_Camera : S_Trigger_External
{
	public S_Trigger_Camera () {
		_isLogicInPlayerScript = true;
	}

	[Header("Functionality"), DrawHorizontalWithOthers(new string[] { "_willReleaseOnExit" }, new float[] { 2.5f, 1f })]
	public enumCameraControlType _whatType;
	[Tooltip("If true, all of the above effects will be undone when the player leaves the trigger (but the rotation will not)."), HideInInspector]
	public bool _willReleaseOnExit = false;

	[ColourIfNull(1, 0, 0, 1)]
	[OnlyDrawIfNot("_whatType", enumCameraControlType.SetToViewTarget)]
	public Transform _Direction;

	[OnlyDrawIf("_whatType", enumCameraControlType.RemoveEffects)]
	[Tooltip("If true, every possible camera effect will be removed, if not, will only remove effects currently set to be applied, as if inverted.")]
	public bool _removeAll = true;

	[ColourIfNull(1, 0, 0, 1)]
	[OnlyDrawIf("_whatType", enumCameraControlType.SetToViewTarget)]
	public Transform _lockOnTarget;

	[HideInInspector] public Vector3 _directionToSet;

	[OnlyDrawIfNot("_whatType", enumCameraControlType.RemoveEffects), Tooltip("Disables most camera interactions and automatic rotating ")]
	[DrawHorizontalWithOthers(new string[] { "_lockCameraX", "_lockCameraY", "_lockToCharacterRotation", "_lockCameraFallBack" })]
	public bool _lockCamera;
	[HideInInspector, Tooltip("Prevents manually moving camera left and right")]
	public bool _lockCameraX;
	[HideInInspector, Tooltip("Prevents manually moving camera up and down")]
	public bool _lockCameraY;
	[HideInInspector, Tooltip("If true, camera will be locked to characters movement, constantly trying to stay at the angle relative to the character. E.G. If set behind player, will lock behind player.")]
	public bool _lockToCharacterRotation;
	[HideInInspector, Tooltip("Prevents certain actions like boost or spin dash leaving the camera behind as they burst forwards")]
	public bool _lockCameraFallBack;

	[Header("Turning")]
	[Tooltip("How quickly the camera will rotate to face the trigger direction."), DrawHorizontalWithOthers(new string[] { "_duration" })]
	public float _faceSpeed = 2f;
	[Min(0), Tooltip("How long the camera will be looking in this direction. If 0, will be forever. This includes time to face that direction."), HideInInspector]
	public float _duration = 1;
	[Tooltip("If true, then the camera rotation following player (where left is down if running along a wall) will be taken control of, making the roll upwards the same as the trigger.")]
	public bool _setCameraReferenceWorldRotation;
	[Tooltip("If true, then the camera will face in the vertical direction based on the trigger transform as well.")]
	public bool _willRotateVertically;


	[Header("Effects")]
	[Tooltip("If true, the distance given will still be affected by distance changes based on player speed, handled already in HedgeCamera"), DrawHorizontalWithOthers(new string[] { "_affectNewFOVBySpeed" })]
	public bool _affectNewDistanceBySpeed = true;
	[Tooltip("If true, the FOV given will still be affected by distance changes based on player speed, handled already in HedgeCamera"), HideInInspector]
	public bool _affectNewFOVBySpeed;


	[HideInInspector] public bool _willChangeAltitude;
	[DrawTickBoxBefore("_willChangeAltitude")]
	[Tooltip("If true, ignore direction of trigger and use custom local angle for camera. From -90 to 90. Speed determined by rotation. "), Range(-90, 90)]
	public float _newAltitude;

	[HideInInspector] public bool _willChangeDistance = false;
	[DrawTickBoxBefore("_willChangeDistance")]
	[Tooltip("X is the new distance from the camera, y is how many frames to reach it. Distance will still be changed based on running speed and collisions.")]
	public Vector2 _newDistance;

	[HideInInspector] public bool _willChangeFOV = false;
	[DrawTickBoxBefore("_willChangeFOV")]
	[Tooltip("X is the new FOV for the view camera, Y is how many frames to reach it. It will still be changed based on running speed and collisions.")]
	public Vector2 _newFOV = new Vector2(100, 5);

	[HideInInspector]
	public bool _willOffsetTarget;
	[Space(2f)]
	[DrawTickBoxBefore("_willOffsetTarget")]
	public Vector3 _newOffset = Vector3.zero;

	[DrawHorizontalWithOthers(new string[] { "_asLocalOffset", "_overWriteAllOffsets" })]
	[Tooltip("How many frames it takes to reach this offset")]
	[OnlyDrawIf("_willOffsetTarget", true)]
	public int _framesToOffset;
	[OnlyDrawIf("_willOffsetTarget", true)]
	[HideInInspector]
	[Tooltip("If true, offSet will be relative to the characters look direction. So if they turn, it orbits around them.")]
	public bool _asLocalOffset = true;
	[HideInInspector]
	[Tooltip("If true, the offset will be unaffected by any HedgeCamera calculations that move the offset. If false, the offset will be affected by other offsets like input direction")]
	[OnlyDrawIf("_willOffsetTarget", true)]
	public bool _overWriteAllOffsets;

	[DrawHorizontalWithOthers(new string[] { "_meshScale" }, new float[] { 2.5f, 1f })]
	[OnlyDrawIf("_willOffsetTarget", true)]
	[BaseColour(0.8f, 0.8f, 0.8f, 1)]
	public Mesh _VisualiseWithMesh;
	[OnlyDrawIf("_willOffsetTarget", true), HideInInspector, Min(0.2f)]
	[SerializeField] private float _meshScale = 1;

	[HideInInspector, SerializeField]
	private Vector3 _offsetReferenceInWorld;

	[SerializeField, AsButton("Update Name", "UpdateNameCommand", null)]
	private bool updateNameButton;

	[SerializeField,AsButton("Update All Names", "UpdateAllNamesCommand", null)]
	private bool updateAllNamesButton;


	private void Awake () {
		if (_Direction == null)
		{
			_Direction = transform;
		}

		_directionToSet = _Direction.forward;
	}

#if UNITY_EDITOR

	public override void DrawAdditionalGizmos ( bool selected, Color colour ) {
		base.DrawAdditionalGizmos(selected, colour);
		if (_hasTrigger)
		{
			switch (_whatType)
			{
				case enumCameraControlType.SetToViewTarget:
					if (!_lockOnTarget) break;
					S_S_Drawing.DrawArrowHandle(colour, null, S_S_MoreMaths.GetAverageOfVector(transform.lossyScale) / 2, false, (_lockOnTarget.position - transform.position).normalized, transform.position); break;
				case enumCameraControlType.OnlyApplyEffects:
				case enumCameraControlType.RemoveEffects:
					S_S_Drawing.DrawCubeHandle(colour, transform, 2f, false); break;
				default:
					S_S_Drawing.DrawArrowHandle(colour, transform, 0.4f, true, Vector3.forward, Vector3.zero); break;
			}
		}

		if (_willOffsetTarget)
		{
			if (_VisualiseWithMesh)
			{
				Gizmos.color = selected ? new Color(0, 0, 0, 0.1f) : new Color(0, 0, 0, 0.02f);
				if (_asLocalOffset)
					Gizmos.DrawWireMesh(_VisualiseWithMesh, transform.position, Quaternion.LookRotation(transform.forward, Vector3.up), Vector3.one * _meshScale * 10);
				else
					Gizmos.DrawWireMesh(_VisualiseWithMesh, transform.position, Quaternion.identity, Vector3.one * _meshScale * 10);
			}

			Gizmos.color = colour;
			Gizmos.DrawWireSphere(_offsetReferenceInWorld, 0.3f * _meshScale);
		}
	}

	public override void AdditionalTriggerSceneGUI () {
		if (_isSelected && _willOffsetTarget)
		{
			Vector3 currentPos = transform.position;

			if (float.IsNaN(_newOffset.x))
			{ _newOffset = Vector3.zero; }

			_meshScale = Mathf.Max(_meshScale, 0.2f);

			Vector3 useOffset = _newOffset * _meshScale;
			Vector3 currentPos2 = currentPos + (_asLocalOffset ? transform.rotation * useOffset : useOffset);


			_offsetReferenceInWorld = Handles.FreeMoveHandle(currentPos2, _meshScale, Vector3.zero, Handles.RectangleHandleCap);
			Vector3 difference = (_offsetReferenceInWorld - currentPos2) / _meshScale;

			_newOffset += _asLocalOffset ? transform.rotation * difference : difference;
		}
	}

	public void UpdateAllNamesCommand () {
		S_Trigger_Camera[] cameraTriggers = FindObjectsByType<S_Trigger_Camera>((FindObjectsSortMode.None));
		foreach (S_Trigger_Camera Trig in cameraTriggers)
		{
			Trig.UpdateNameCommand();
		}
	}

	//Changes the game objects name to include what type of cam trig it is.
	public void UpdateNameCommand () {
		//Removes brackets, so number can be added at end, and so there aren't multiple types listed.
		List<string> input = S_S_Editor.CleanBracketsInString(gameObject.name, '(', ')', 0);
		string newName = input[0];

		string typeString = _whatType.ToString();
		//Set is used a lot in the types, but is just clutter for an object name.
		if (typeString.Contains("Set"))
		{
			typeString = typeString.Replace("Set", ""); // Remove "world"
		}
		newName += ("(" + typeString +")");
		
		//Adds any number related brackets to the end, for identifying instances.
		for (int i = 1 ; i < input.Count ; i++)
		{
			if (input[i].Any(char.IsDigit)) { newName += input[i]; }
		}

		gameObject.name = newName;
	}
#endif
}
