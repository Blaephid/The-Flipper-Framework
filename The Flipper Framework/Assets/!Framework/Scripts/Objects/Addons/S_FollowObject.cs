using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteAlways]
public class S_FollowObject : MonoBehaviour
{

	[SerializeField] private Transform _Parent;
	private Vector3  _offsetToTry;
	[SerializeField] private Vector3  _currentOffset;
	[SerializeField] private bool _followInPlay;

	private Transform _RememberParent;
	private Vector3 _rememberOffset;
	private Vector3 _positionLastFrame;

	private bool _isFollowing;
	private bool _willApplyFollow;

	[AsButton("Reset Offset", "ResetOffsetCommand", null), SerializeField]
	bool resetOffsetButton;

	// Start is called before the first frame update
	void Start () {
		_isFollowing = _followInPlay;
	}

#if UNITY_EDITOR
	private void OnValidate () {
		if (!Application.isPlaying) { _isFollowing = true; }

		if(_RememberParent != _Parent && _Parent)
		{
			_currentOffset = transform.position - _Parent.position;
		}

		_RememberParent = _Parent;
		_willApplyFollow = PrepareFollow();
	}
#endif

	private void OnEnable () {
		if((_followInPlay && Application.isPlaying ) || !Application.isPlaying) { _isFollowing = true; _willApplyFollow = PrepareFollow(); }

#if UNITY_EDITOR
		Undo.undoRedoPerformed += OnUndoRedoPerformed;
#endif
	}

	private void OnDisable () {
		if ((_followInPlay && Application.isPlaying) || !Application.isPlaying) { _isFollowing = true; _willApplyFollow = PrepareFollow(); }

#if UNITY_EDITOR
		Undo.undoRedoPerformed -= OnUndoRedoPerformed;
#endif
	}

	[ExecuteAlways]
	// Update is called once per frame
	void Update () {
		_currentOffset = _currentOffset == default(Vector3) ? Vector3.up * 0.1f : _currentOffset; //Ensures offset is never zero exactly.

		if (_willApplyFollow)
		{
			ApplyFollow();
		}
		_rememberOffset = _currentOffset;

#if UNITY_EDITOR
		if (S_S_Editor.IsSelected(gameObject))
			ChangeCurrentOffset();
#endif
		_willApplyFollow = PrepareFollow();
	}

	private void OnUndoRedoPerformed () {
		ChangeCurrentOffset();

		_willApplyFollow = false;
	}

	//Move location to match the offset. This is called at the start of a frame, after being readied end of last one.
	private void ApplyFollow () {
		if(_offsetToTry == default(Vector3) || _positionLastFrame != transform.position) { _willApplyFollow = false; }

#if UNITY_EDITOR
		if (S_S_Editor.IsSelected(gameObject))
			ChangeCurrentOffset();
#endif
		if(!_willApplyFollow) { return; }

		transform.position = _offsetToTry;
	}

	private bool PrepareFollow () {
		if (!_isFollowing || !_Parent) { return false; }

		_offsetToTry = _Parent.position + _currentOffset;
		_positionLastFrame = transform.position;
		return true;
	}

	//If changing the offset, rather than moving to fit the offset.
	private void ChangeCurrentOffset () {
		if(!_Parent) { return; }

		//If this object has been moved, but its set offset hasn't been changed, then change the offset to match the new position.
		if (transform.position - _Parent.position != _rememberOffset && _rememberOffset == _currentOffset && _rememberOffset != default(Vector3))
		{
			_currentOffset = transform.position - _Parent.position;
			_offsetToTry = _Parent.position + _currentOffset;
			_willApplyFollow = true;
		}
	}

	//Linked to the button, and sets offset to 0
	public void ResetOffsetCommand () {
		_currentOffset = Vector3.up * 0.1f;
		transform.position = _Parent.position;
		_offsetToTry = Vector3.zero;
	}
}
