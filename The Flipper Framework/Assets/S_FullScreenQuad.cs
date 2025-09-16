using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_FullScreenQuad : MonoBehaviour
{
	public Camera CameraToStickTo;

	[SerializeField] bool _useMainCamera;
	[SerializeField] bool _updateInGameTime;
	[SerializeField] bool _affectChildren;
	[SerializeField] Vector3 childrenOffset;
	[SerializeField] float _customZOffset;

	[AsButton("Fit To Full Screen", "FitFullScreenQuad", null)]
	public bool fitbutton;

	// Start is called before the first frame update
	void Start () {
		if (_useMainCamera) CameraToStickTo = Camera.main;
		FitFullScreenQuad();
	}

	private void Update () {
		if (_updateInGameTime) FitFullScreenQuad();
	}

	private void LateUpdate () {
		if (_updateInGameTime) FitFullScreenQuad();
	}

	public void FitFullScreenQuad () {
		if (!CameraToStickTo) { return; }

		Vector2 newScale = S_S_Objects.GetScaleToFitCameraBounds(CameraToStickTo, _customZOffset == 0 ? transform.localPosition.z : _customZOffset, transform, true);

		for (int i = 0 ; i < transform.childCount ; i++)
		{
			transform.GetChild(i).localScale = Vector3.one;
			transform.GetChild(i).localPosition = childrenOffset;
		}
	}

}
