using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;


[ExecuteAlways]
public class S_FollowObject : MonoBehaviour
{

	[SerializeField] private Transform _Parent;
	[SerializeAs("_currentOffsetSet")]
	[SerializeField] private Vector3  _currentOffsetSet;
	[SerializeAs("_currentRotationSet")]
	[SerializeField] private Vector3  _currentEulerOffsetSet;
	[SerializeField] private bool _followInPlay;

	[SerializeField, HideInInspector]
	private Transform _RememberParent;

	private Vector3  _offsetToTry;
	private Quaternion  _rotationToTry;

	private Vector3 _rememberOffset;
	private Vector3 _positionLastFrame;

	private Vector3 _rememberEuler;
	private Quaternion _rotationLastFrame;

	private bool _isFollowing;
	private bool _willApplyFollow;

	private bool _isRotating;
	private bool _willApplyRotate;

	[AsButton("Reset Offset", "ResetOffsetCommand", null), SerializeField]
	bool resetOffsetButton;

	[AsButton("Reset Rotation", "ResetRotateCommand", null), SerializeField]
	bool resetRotateButton;

	// Start is called before the first frame update
	void Start () {
		_isFollowing = _followInPlay;
	}

#if UNITY_EDITOR
	private void OnValidate () {
		if (!Application.isPlaying)
		{
			_isFollowing = true;
			_isRotating = true;
		}

		if (_RememberParent != _Parent && _Parent)
		{
			_currentOffsetSet = transform.position - _Parent.position;
			_currentEulerOffsetSet = _Parent.eulerAngles + transform.eulerAngles;
		}

		_RememberParent = _Parent;
		_willApplyFollow = PrepareFollow();
	}
#endif

	private void OnEnable () {
		if ((_followInPlay && Application.isPlaying) || !Application.isPlaying)
		{
			_isFollowing = true;
			_isRotating = true;
			_willApplyFollow = PrepareFollow();
		}

#if UNITY_EDITOR
		Undo.undoRedoPerformed += OnUndoRedoPerformed;
#endif
	}

	private void OnDisable () {
		if ((_followInPlay && Application.isPlaying) || !Application.isPlaying)
		{
			_isFollowing = true;
			_isRotating = true;
			_willApplyFollow = PrepareFollow();
		}

#if UNITY_EDITOR
		Undo.undoRedoPerformed -= OnUndoRedoPerformed;
#endif
	}

	[ExecuteAlways]
	// Update is called once per frame
	void Update () {
		//Ensures offset is never zero exactly. If it was, we wouldn't be able to check if anything was applied (or not) using default()
		_currentOffsetSet = _currentOffsetSet == default(Vector3) ? Vector3.up * 0.1f : _currentOffsetSet;
		_currentEulerOffsetSet = _currentEulerOffsetSet == Vector3.zero ? Vector3.up * 0.01f: _currentEulerOffsetSet;

		if (_willApplyFollow)
		{
			ApplyFollow();
		}
		_rememberOffset = _currentOffsetSet;

		if (_willApplyRotate)
		{
			ApplyRotate();
		}
		_rememberEuler = _currentEulerOffsetSet;

#if UNITY_EDITOR
		if (S_S_Editor.IsSelected(gameObject))
			ChangeCurrentOffset();
#endif
		_willApplyFollow = PrepareFollow();
	}

	private void OnUndoRedoPerformed () {
		ChangeCurrentOffset();

		_willApplyFollow = false;
		_willApplyRotate = false;
	}

	//Move location to match the offset. This is called at the start of a frame, after being readied end of last one.
	private void ApplyFollow () {
		if (_offsetToTry == default(Vector3) || _positionLastFrame != transform.position) { _willApplyFollow = false; }

#if UNITY_EDITOR
		if (S_S_Editor.IsSelected(gameObject))
			ChangeCurrentOffset();
#endif
		if (!_willApplyFollow) { return; }

		transform.position = _offsetToTry;
	}

	private void ApplyRotate () {
		if (_rotationToTry == default(Quaternion) || _rotationLastFrame != transform.rotation) { _willApplyRotate = false; }

		if (!_willApplyRotate) { return; }

		transform.rotation = _rotationToTry;
	}

	private bool PrepareFollow () {
		_willApplyRotate = PrepareRotate();

		if (!_isFollowing || !_Parent) { return false; }

		_offsetToTry = _Parent.position + _currentOffsetSet;
		_positionLastFrame = transform.position;
		return true;
	}

	private bool PrepareRotate () {
		if (!_isRotating || !_Parent) { return false; }

		//_rotationToTry = _Parent.rotation * _currentRotationSet;
		_rotationToTry = Quaternion.Euler(_Parent.eulerAngles + _currentEulerOffsetSet);
		_rotationLastFrame = transform.rotation;
		return true;
	}

	//If changing the offset, rather than moving to fit the offset.
	private void ChangeCurrentOffset () {
		if (!_Parent) { return; }

		//If this object has been moved, but its set offset hasn't been changed, then change the offset to match the new position.
		if (transform.position - _Parent.position != _rememberOffset && _rememberOffset == _currentOffsetSet && _rememberOffset != default(Vector3))
		{
			_currentOffsetSet = transform.position - _Parent.position;
			_offsetToTry = _Parent.position + _currentOffsetSet;
			_willApplyFollow = true;
		}

		//If this object has been rotated, but its offset rotation hasn't changed, then it's being rotated manually. So apply.
		if (transform.rotation != Quaternion.Euler(_rememberEuler + _Parent.eulerAngles) 
			&& _rememberEuler == _currentEulerOffsetSet && _rememberEuler != default(Vector3))
		{
			Debug.Log(gameObject);
			_currentEulerOffsetSet = transform.eulerAngles - _Parent.eulerAngles;
			Debug.Log("Set To" + _currentEulerOffsetSet);
			_rotationToTry = Quaternion.Euler(_Parent.eulerAngles + _currentEulerOffsetSet);
			_willApplyRotate = true;
		}
	}

#if UNITY_EDITOR

	//Linked to the button, and sets offset to 0
	public void ResetOffsetCommand () {
		Undo.RecordObject(transform, "Reset Offset");
		_currentOffsetSet = Vector3.up * 0.1f;
		transform.position = _Parent.position;
		_offsetToTry = Vector3.zero;
	}

	public void ResetRotateCommand () {
		Undo.RecordObject(transform, "Reset Rotation");
		_currentEulerOffsetSet = Vector3.up * 0.1f;
		transform.rotation = _Parent.rotation;
		_rotationToTry = Quaternion.Euler(_Parent.eulerAngles + _currentEulerOffsetSet);
	}
#endif
}
