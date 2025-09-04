using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SetTransform : MonoBehaviour
{
	[Header("Via a parent")]
	public Transform _TransformForPosition;
	public Transform _TransformForRotation;
	[SerializeField] Space _whatSpaceForOffset = Space.Self;
	[SerializeField] Vector3 _localOffset;

	[Header("Manual")]
	[SerializeField] Space _whatSpaceForPosition;
	[HideInInspector, SerializeField] bool _applyPosition = true;
	[DrawTickBoxBefore("_applyPosition")]
	[SerializeField] Vector3 _manPosition;

	[SerializeField] Space _whatSpaceForRotation;
	[HideInInspector, SerializeField] bool _applyRotation = true;
	[DrawTickBoxBefore("_applyRotation")]
	[SerializeField] Vector3 _manEulerAngles;

	[AsButton("Apply", "LateUpdate", null)]
	public bool _button;

	// Update is called once per frame
	public void LateUpdate () {
		SetPosition();
		SetRotation();

	}

	void SetPosition () {
		//If a parent is set, place at position with offset
		if (_TransformForPosition != null)
		{
			transform.position = _TransformForPosition.position;
			switch (_whatSpaceForOffset)
			{
				case Space.Self:
					transform.position += _TransformForPosition.rotation * _localOffset;
					return;
				case Space.World:
					transform.position += _localOffset;
					return;
			}
		}

		if (!_applyPosition) return;

		switch (_whatSpaceForPosition)
		{
			case Space.World:
				transform.position = _manPosition; break;
			case Space.Self:
				transform.localPosition = _manPosition; break;
		}
	}

	void SetRotation () {
		if (_TransformForRotation != null)
		{ transform.rotation = _TransformForRotation.rotation; return; }

		if (!_applyRotation) return;

		switch (_whatSpaceForRotation)
		{
			case Space.World:
				transform.eulerAngles = _manEulerAngles; break;
			case Space.Self:
				transform.localEulerAngles = _manEulerAngles; break;
		}
	}
}
