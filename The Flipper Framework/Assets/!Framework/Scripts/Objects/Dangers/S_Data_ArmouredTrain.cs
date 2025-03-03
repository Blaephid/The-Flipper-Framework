using System;
using System.Collections;
using System.Collections.Generic;
using templates;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
[AddComponentMenu("Data Components/Armoured Train")]
public class S_Data_ArmouredTrain : S_Data_Base
{
	public bool _updateAutomatically = true;

	public StrucSegments[] _Segments;

	[SerializeField, HideInInspector]
	private List<GameObject> _ChildObjectSegments;

	[Serializable]
	public struct StrucSegments {
		public GameObject		_Source;
		public Vector3		_localOffset;
		public Vector3                _rotationOffset;
		public AnimatorController	_ControllerIfNeeded;
	}

	private void Update () {
		if(Application.isPlaying) { return; }

		if (_updateAutomatically)
		{
			RemoveExtraSegmentsIfNotWanted();
			ResetOrCreateSegmentChildren(false);
		}
	}

	public void RemoveExtraSegmentsIfNotWanted () {
		//In case the user removes an element from the array of segements, the creation would not know, so destroy extra child object segments from the scene if this happens.
		if (_Segments.Length < _ChildObjectSegments.Count)
		{
			for (int i = _Segments.Length ; i < _ChildObjectSegments.Count ; i++)
			{
				GameObject extraSegment = S_S_Editor.FindChild(gameObject, "Segment - " +(i+1));
				if (extraSegment) GameObject.DestroyImmediate(extraSegment);
			}
		}
	}

	public void ResetOrCreateSegmentChildren (bool replace) {

		_ChildObjectSegments.Clear();

		//Go through each segment and ensure there is a child object for it.
		for (int i = 0 ; i < _Segments.Length ; i++)
		{
			if (_Segments[i]._Source == null) { continue; }
			GameObject source = _Segments[i]._Source;

			//This will either find the child object by name, create it, of if replace is true, find the child but delete and recreate it (allowing all needed components to be set properly)
			GameObject thisSegment = S_S_Editor.FindOrCreateChild(gameObject, "Segment - " +(i+1),
				new System.Type[] {
				typeof(MeshCollider),
				typeof(Animator)}
				,replace,
				source);

			//Setting variables

			//Because some train meshes are skinned because they have armature, get mesh from either component.
			Mesh meshForCollider = null;
			if (source.GetComponentInChildren<SkinnedMeshRenderer>())
				meshForCollider = source.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
			else if(source.GetComponentInChildren<MeshRenderer>())
				meshForCollider = source.GetComponentInChildren<MeshFilter>().sharedMesh;
			thisSegment.GetComponent<MeshCollider>().sharedMesh = meshForCollider;
			thisSegment.GetComponent<MeshCollider>().convex = true;

			thisSegment.transform.localPosition = _Segments[i]._localOffset;
			thisSegment.transform.localEulerAngles = _Segments[i]._rotationOffset;

			if (_Segments[i]._ControllerIfNeeded) { thisSegment.GetComponent<Animator>().runtimeAnimatorController = _Segments[i]._ControllerIfNeeded; }

			_ChildObjectSegments.Add(thisSegment);
		}
	}
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(S_Data_ArmouredTrain))]
public class ArmouredTrainEditor : S_CustomInspector_Base
{
	S_Data_ArmouredTrain _OwnerScript;


	public override void OnEnable () {
		//Setting variables
		_OwnerScript = (S_Data_ArmouredTrain)target;
		_InspectorTheme = _OwnerScript._InspectorTheme;

		base.OnEnable();
	}

	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _OwnerScript._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {

		DrawDefaultInspector();

		if (S_S_CustomInspector.IsDrawnButtonPressed(serializedObject, "Reset Segment Children", _BigButtonStyle, _OwnerScript, "Reset Segment Children"))
		{
			_OwnerScript.RemoveExtraSegmentsIfNotWanted();
			_OwnerScript.ResetOrCreateSegmentChildren(true);
		}
		
	}
}
	#endif