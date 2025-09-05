using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class S_Vis_Directions : S_Vis_Base
{
#if UNITY_EDITOR
	public S_Vis_Directions () {
		_hasVisualisationScripted = true;
	}

	[SerializeField, DrawHorizontalWithOthers(new string[] {"_scale"})]
	private float _thickness = 1;
	[SerializeField, HideInInspector, Min(0.2f)]
	private float _scale = 1.5f;
	[SerializeField, DrawHorizontalWithOthers(new string[] {"_backwards"})]
	private bool _forwards;
	[SerializeField, HideInInspector]
	private bool _backwards;

	[SerializeField, DrawHorizontalWithOthers(new string[] {"_downwards"})]
	private bool _upwards;
	[SerializeField, HideInInspector]
	private bool _downwards;

	[SerializeField, DrawHorizontalWithOthers(new string[] {"_left"})]
	private bool _right;
	[SerializeField, HideInInspector]
	private bool _left;


	public override void DrawGizmosAndHandles ( bool selected ) {
		float baseScale = S_S_MoreMaths.GetAverageOfVector(transform.lossyScale);
		float distance = Mathf.Max(0.2f, _scale) * baseScale;
		float useThickness = selected ? _thickness * 1.2f : _thickness;

		Handles.color = _normalOutlineColour;
		if (_forwards) DrawLineInDirection(transform.forward * distance * 1.2f, useThickness * 1.2f);
		if (_backwards) DrawLineInDirection(-transform.forward * distance, useThickness);

		Handles.color = _selectedOutlineColour;
		if (_upwards) DrawLineInDirection(transform.up * distance * 1.2f, useThickness * 1.2f);
		if (_downwards) DrawLineInDirection(-transform.up * distance, useThickness);

		Handles.color = _selectedFillColour;
		if (_right) DrawLineInDirection(transform.right * distance * 1.2f, useThickness * 1.2f);
		if (_left) DrawLineInDirection(-transform.right * distance, useThickness);
	}

	private void DrawLineInDirection ( Vector3 direction, float useThickness ) {
		Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; //Ensures handles drawn wont be visible through walls.
		Handles.DrawLine(transform.position, transform.position + direction, useThickness);
	}
#endif
}
