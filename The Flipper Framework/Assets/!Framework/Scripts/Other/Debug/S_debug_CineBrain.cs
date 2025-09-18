using Unity.Cinemachine;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class S_debug_CineBrain : MonoBehaviour
{

	// Update is called once per frame
	void Update () {

		if(!Camera.main) { return; }

		if(Camera.main.TryGetComponent(out CinemachineBrain brain))
		{
			if (brain.ActiveVirtualCamera != null)
			{
				// ActiveVirtualCamera is an ICinemachineCamera, not necessarily a CinemachineCamera
				ICinemachineCamera activeCam = brain.ActiveVirtualCamera;
				Debug.Log("Active VCam: " + activeCam.Name);
			}

			var blend = brain.ActiveBlend;
			if (blend != null) // a blend is happening
			{
				float progress = blend.Duration > 0f ? blend.TimeInBlend / blend.Duration : 1f;
				Debug.Log(
				    $"Blending {blend.CamA?.Name} → {blend.CamB?.Name} | " +
				    $"Style: {blend.BlendCurve} | " +
				    $"Progress: {progress:P0}"
				);
			}
		}
	}
}
#endif
