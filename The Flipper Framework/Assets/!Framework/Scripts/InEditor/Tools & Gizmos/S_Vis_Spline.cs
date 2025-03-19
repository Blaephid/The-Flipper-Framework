using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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


	public override void DrawGizmosAndHandles ( bool selected ) {
		if(!_SplineToDisplay) { return; }

		CurveSample sample =  _SplineToDisplay.GetSampleAtDistance(0);
		Spline.SampleTransforms sampleTransform = Spline.GetSampleTransformInfo(_SplineToDisplay.transform, sample);

		Vector3 samplePoint1 = sampleTransform.location ;
		Vector3 samplePoint2;

		Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; //Ensures handles drawn wont be visible through walls.
		if (selected) { Handles.color = _selectedOutlineColour; }
		else { Handles.color = _normalOutlineColour; }
		
		//At each increment of the set distance, draw a line from that sample and the last sample, leading to one consistant bending line.
		for (float f = _distancePerCalculation ; f <= _SplineToDisplay.Length ; f = Mathf.Min (f + _distancePerCalculation, _SplineToDisplay.Length))
		{
			sample = _SplineToDisplay.GetSampleAtDistance(f);
			sampleTransform = Spline.GetSampleTransformInfo(_SplineToDisplay.transform, sample);

			samplePoint2 = sampleTransform.location;
			Handles.DrawLine(samplePoint1, samplePoint2, 3f);

			samplePoint1 = samplePoint2;

			//Because F won't go over the length, make sure to end loop if it does after drawing the last line.
			if (_SplineToDisplay.Length == f) 
			{ break; }

			//If at the halfway point of the spline. Found by checking f against the closest increment to half distance
			else if (f == S_S_MoreMaths.GetNumberAsIncrement(Mathf.Lerp(0, _SplineToDisplay.Length, 0.5f), _distancePerCalculation)){
				_middleOfSpline = samplePoint1;
			}
		}

		for (int i = 0 ; i < _SplineToDisplay.nodes.Count ; i++)
		{
			SplineNode thisNode = _SplineToDisplay.nodes[i];
			Vector3 position = _SplineToDisplay.transform.position + (_SplineToDisplay.transform.rotation * thisNode.Position);

			Handles.DrawWireCube(position, Vector3.one);
		}
	}

	public void CustomOnSceneGUI ( SceneView sceneView ) {
		VisualiseWithSelectableHandle(_middleOfSpline + (transform.rotation * _selectPointOffset), _selectHandleSize);
	}
#endif
}

