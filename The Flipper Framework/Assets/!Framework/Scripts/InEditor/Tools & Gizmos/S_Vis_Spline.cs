using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static SplineMesh.Spline;

[ExecuteInEditMode]
public class S_Vis_Spline : S_Vis_Base, ICustomEditorLogic
{
#if UNITY_EDITOR
	public S_Vis_Spline () {
		_hasVisualisationScripted = true;
	}


	[Header("Details")]
	[Tooltip ("This is how long each line that makes up the visualisation will be. The lower number, the more calculations but the more accurate the visual.")]
	[SerializeField] private int _distancePerCalculation = 10;

	[SerializeField] private Vector3 _selectPointOffset;
	[SerializeField] private float _selectHandleSize = 6;
	private Vector3 _middleOfSpline;

	public Spline _SplineToDisplay;

	private void OnValidate () {

		//Delay this because OnValidate is called on creation, before anything else, so scripts like PlaceOnSpline would set up shot before changing position.
		EditorApplication.delayCall += () =>
		{
			_debugTrajectoryPoints = SetUpSpline();
		};
	}

	[ExecuteAlways]
	void Update () {
		if (_isSelected)
			_debugTrajectoryPoints = SetUpSpline();
	}

	private Vector3[] SetUpSpline () {
		if (Application.isPlaying) { return null; }
		if(!_SplineToDisplay || ! _SplineToDisplay.transform) { return null; };

		Vector3[] points = new Vector3[(int)(_SplineToDisplay.Length / _distancePerCalculation) + 2];

		CurveSample sample =  _SplineToDisplay.GetSampleAtDistance(0);
		Spline.SampleTransforms sampleTransform = Spline.GetSampleTransformInfo(_SplineToDisplay.transform, sample);

		for (float i = 0 ; i < points.Length ; i++)
		{
			float f = Mathf.Min( i * _distancePerCalculation, _SplineToDisplay.Length);
			sample = _SplineToDisplay.GetSampleAtDistance(f);
			sampleTransform = Spline.GetSampleTransformInfo(_SplineToDisplay.transform, sample);

			points[(int)i] = sampleTransform.location;

			//Because F won't go over the length, make sure to end loop if it does after drawing the last line.
			if (_SplineToDisplay.Length == f)
			{ break; }

			//If at the halfway point of the spline. Found by checking f against the closest increment to half distance
			else if (f == S_S_MoreMaths.GetNumberAsIncrement(Mathf.Lerp(0, _SplineToDisplay.Length, 0.5f), _distancePerCalculation))
			{
				_middleOfSpline = sampleTransform.location;
			}
		}

		return points;
	}


	public override void DrawGizmosAndHandles ( bool selected ) {
		if (!_SplineToDisplay) { return; }

		DrawGizmosFromArray(selected);

		for (int i = 0 ; i < _SplineToDisplay.nodes.Count ; i++)
		{
			SplineNode thisNode = _SplineToDisplay.nodes[i];
			Vector3 position = _SplineToDisplay.transform.position + (_SplineToDisplay.transform.rotation * thisNode.Position);

			Gizmos.DrawWireCube(position, Vector3.one);
		}
	}

	public void CustomOnSceneGUI ( SceneView sceneView ) {
		VisualiseWithSelectableHandle(_middleOfSpline + (transform.rotation * _selectPointOffset), _selectHandleSize);
	}
#endif
}

