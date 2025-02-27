using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

	private void OnValidate () {
		if (!_Parent) { return; }

		if(_RememberParent != _Parent)
		{
			_offset = transform.position - _Parent.position;
		}

		_RememberParent = _Parent;
	}

	private void OnEnable () {
		if((_followInPlay && Application.isPlaying ) || !Application.isPlaying) { _isFollowing = true; }
	}

	private void OnDisable () {
		if ((_followInPlay && Application.isPlaying) || !Application.isPlaying) { _isFollowing = true; }
	}

	[ExecuteInEditMode]
	// Update is called once per frame
	void Update () {
		if(!_isFollowing || !_Parent) { return; }

		if (transform.position - _Parent.position != _rememberOffset && _rememberOffset == _offset)
			_offset = transform.position - _Parent.position;

		transform.position = _Parent.position + _offset;
		_rememberOffset = _offset;
	}
}
