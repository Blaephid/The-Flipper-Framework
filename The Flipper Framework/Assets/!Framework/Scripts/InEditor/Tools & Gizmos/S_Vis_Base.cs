using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System;
using static UnityEngine.Rendering.DebugUI;

public class S_Vis_Base : MonoBehaviour, ICustomEditorLogic
{
	[HideInInspector] public bool _isSelected;

#if UNITY_EDITOR
	[CustomReadOnly]
	[Tooltip("Must be defined in code. If true, will serialize fields releveant to Visualisation, as not all classes will have the necessary implementation for this.")]
	[DrawHorizontalWithOthers(new string[] { "_viewVisualisationData"})]
	[BaseColour(0.6f,0.6f,0.6f,1f)]
	public bool _hasVisualisationScripted = false;

	[OnlyDrawIf("_hasVisualisationScripted", true)]
	[SetBoolIfOther(false, "_hasVisualisationScripted", false)]
	[HideInInspector]
	public bool _viewVisualisationData;

	//The above property cannot be drawn horizontally if it uses SetBoolIfOthers, so instead, bind a different boolean to it (this one), that will draw the other data, but not itself.
	[SetBoolIfOther(false, "_viewVisualisationData", false)]
	[SetBoolIfOther(true, "_viewVisualisationData", true)]
	[DrawOthersIf(false, new string[] {"_drawAtAllTimes", "_drawIfParentSelected", "_normalOutlineColour", "_selectedOutlineColour", "_selectedFillColour" }, true)]
	[SerializeField]
	private bool _viewVisualisationDataHidden;

	[HideInInspector ]
	public bool _drawAtAllTimes = true;
	[HideInInspector]
	public bool _drawIfParentSelected = false;
	[HideInInspector]
	public Color _normalOutlineColour = Color.grey;
	[ HideInInspector]
	public Color _selectedOutlineColour = Color.white;
	[HideInInspector]
	public Color _selectedFillColour = new Color(1,1,1,0.1f);

	[NonSerialized]
	public Vector3[] _debugTrajectoryPoints;

	private void OnEnable () {
		_isSelected = false;
	}

	private void OnDrawGizmos () {
		if (!enabled || !_hasVisualisationScripted) { return; }

		if (S_S_Editor.IsTooFarFromEditorCamera(transform, 900)) { return; };

		_isSelected = false;
		if (!Application.isPlaying)
		{
			if (transform.parent != null)
			{
				if (_drawIfParentSelected)
					if (S_S_Editor.IsThisOrListOrChildrenSelected(transform.parent, new GameObject[] { transform.parent.gameObject }))
					{ _isSelected = true; }
				if (S_S_Editor.IsThisOrListOrChildrenSelected(transform, new GameObject[] { transform.parent.gameObject }))
				{ _isSelected = true; }
			}
			else
			{
				if (S_S_Editor.IsThisOrListOrChildrenSelected(transform, null))
				{ _isSelected = true; }
			}
		}

		if (_drawAtAllTimes || _isSelected)
		{
			DrawGizmosAndHandles(_isSelected);
		}
	}

	//Called whenever object is selected when gizmos are enabled.
	public virtual void DrawGizmosAndHandles ( bool selected ) {
	}


	public virtual void VisualiseWithSelectableHandle ( Vector3 position, float handleRadius ) {
		if (Application.isPlaying) { return; };

		if (S_S_Editor.IsTooFarFromEditorCamera(transform, 500)) { return; };

		//Only draw select handle if not already selected.
		if (gameObject == null || transform == null || S_S_Editor.IsThisOrListOrChildrenSelected(transform, null, 0))
		{ return; }

		//Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; //Ensures handles drawn wont be visible through walls.
		Color color = _selectedFillColour;
		color.a = Mathf.Max(color.a, 0.5f);

		using (new Handles.DrawingScope(color))
		{
			S_S_Drawing.DrawSelectableHandle(position, gameObject, handleRadius);
		}
	}

	public virtual void DrawGizmosFromArray ( bool selected ) {

		if (_debugTrajectoryPoints == null || _debugTrajectoryPoints.Length == 0) { return; }

		//Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

		if (selected) { Gizmos.color = _selectedOutlineColour; }
		else { Gizmos.color = _normalOutlineColour; }

		//Create a series of line Gizmos representing a path along the points.
		for (int i = 1 ; i < _debugTrajectoryPoints.Length ; i++)
		{
			Gizmos.DrawLine(_debugTrajectoryPoints[i - 1], _debugTrajectoryPoints[i]);

			DrawAdditionalAtPointOnArray(selected, i, _debugTrajectoryPoints[i]);
		}
	}

	public virtual void DrawAdditionalAtPointOnArray ( bool selected, int f, Vector3 point ) {

	}


	//Inherited from ICustomEditorLogic Interface. This will be attached to the DuringSceneGUI event, or called seperately when certain objects are selected.
	public void CustomOnSceneGUI ( SceneView sceneView ) {
		if (this == null) { return; }
		if (S_S_Editor.IsHidden(gameObject)) { return; }

		CallCustomSceneGUI();
	}

	public virtual void CallCustomSceneGUI () {

	}
#endif
}
