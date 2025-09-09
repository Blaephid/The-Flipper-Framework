using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class S_Debug_TrackPosition : MonoBehaviour
{
	public bool _inEditor;
	public bool _log;
	public bool _drawLine;

	public Color _lineColour = Color.white;
	public float _lineTime = 10;

	private Vector3 _previousPosition;
	
	[ExecuteAlways]

	private void LateUpdate () {
		if(!_inEditor && !Application.isPlaying) { return; }

		if(_previousPosition != default(Vector3))
		{
			if (_log) Debug.Log(gameObject + " is at " + transform.position);

			if (_drawLine) Debug.DrawLine(_previousPosition, transform.position, _lineColour, _lineTime);
		}

		_previousPosition = transform.position;
	}
}
#endif
