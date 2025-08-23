using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_FullScreenQuad : MonoBehaviour
{
	public Camera CameraToStickTo;

	[AsButton("Fit To Full Screen", "FitFullScreenQuad", null)]
	public bool fitbutton;

	// Start is called before the first frame update
	void Start () {
		FitFullScreenQuad();
	}

	public void FitFullScreenQuad ( ) {
		if(!CameraToStickTo) { return; }

		Vector2 newScale = S_S_Objects.GetScaleToFitCameraBounds(CameraToStickTo, transform.localPosition.z, transform, true);
	}

}
