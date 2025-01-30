using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_Vis_Spline : S_Vis_Base, ICustomEditorLogic
{
	public S_Vis_Spline () {
		_hasVisualisationScripted = true;
	}


	[Header("Details")]
	[Tooltip ("This is how long each line that makes up the visualisation will be. The lower number, the more calculations but the more accurate the visual.")]
	[SerializeField] private int _distancePerCalculation = 10;

	[SerializeField] private Vector3 _selectPointOffset;
	private Vector3 _middleOfSpline;

	public Spline _SplineToDisplay;


	public override void DrawGizmosAndHandles ( bool selected ) {
		if(!_SplineToDisplay) { return; }

		Vector3 sample1 = (_SplineToDisplay.transform.rotation * _SplineToDisplay.GetSampleAtDistance(0).location) + _SplineToDisplay.transform.position;
		Vector3 sample2;

		Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; //Ensures handles drawn wont be visible through walls.
		if (selected) { Handles.color = _selectedOutlineColour; }
		else { Handles.color = _normalOutlineColour; }
		
		//At each increment of the set distance, draw a line from that sample and the last sample, leading to one consistant bending line.
		for (float f = _distancePerCalculation ; f < _SplineToDisplay.Length ; f = Mathf.Min (f + _distancePerCalculation, _SplineToDisplay.Length))
		{
			sample2 = (_SplineToDisplay.transform.rotation * _SplineToDisplay.GetSampleAtDistance(f).location) + _SplineToDisplay.transform.position;
			Handles.DrawLine(sample1, sample2, 3f);

			sample1 = sample2;

			//Because F won't go over the length, make sure to end loop if it does after drawing the last line.
			if (_SplineToDisplay.Length == f) { return; }

			//If at the halfway point of the spline. Found by checking f against the closest increment to half distance
			else if (f == S_S_MoreMathMethods.GetNumberAsIncrement(Mathf.Lerp(0, _SplineToDisplay.Length, 0.5f), _distancePerCalculation)){
				_middleOfSpline = sample1;
			}
		}
	}


	public void CustomOnSceneGUI ( SceneView sceneView ) {
		VisualiseWithSelectableHandle(_middleOfSpline + (transform.rotation * _selectPointOffset), 6);
	}
}

