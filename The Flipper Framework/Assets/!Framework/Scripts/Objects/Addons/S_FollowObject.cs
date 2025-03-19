using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteAlways]
public class S_FollowObject : MonoBehaviour
{

	[SerializeField] private Transform _Parent;
	[SerializeField] private Vector3  _offset;
	[SerializeField] private bool _followInPlay;

	private Transform _RememberParent;
	private Vector3 _rememberOffset;

	private bool _isFollowing;

	// Start is called before the first frame update
	void Start () {
		_isFollowing = _followInPlay;
	}

#if UNITY_EDITOR
	private void OnValidate () {
		_offset = _offset == default(Vector3) ? Vector3.up * 0.1f : _offset;

		if (!_Parent) { return; }
		if (!Application.isPlaying) { _isFollowing = true; }

		if(_RememberParent != _Parent)
		{
			_offset = transform.position - _Parent.position;
		}

		_RememberParent = _Parent;

		Follow();
	}
#endif

	private void OnEnable () {
		if((_followInPlay && Application.isPlaying ) || !Application.isPlaying) { _isFollowing = true; Follow(); }
	}

	private void OnDisable () {
		if ((_followInPlay && Application.isPlaying) || !Application.isPlaying) { _isFollowing = true; Follow(); }
	}

	[ExecuteAlways]
	// Update is called once per frame
	void Update () {
		Follow(true);
	}

	private void Follow (bool canAdjust = false) {
		if (!_isFollowing || !_Parent) { return; }

#if UNITY_EDITOR
		if (canAdjust && Selection.activeGameObject == gameObject &&  transform.position - _Parent.position != _rememberOffset && _rememberOffset == _offset && _rememberOffset != default(Vector3))
			_offset = transform.position - _Parent.position;
#endif

		transform.position = _Parent.position + _offset;
		_rememberOffset = _offset;
	}
}
