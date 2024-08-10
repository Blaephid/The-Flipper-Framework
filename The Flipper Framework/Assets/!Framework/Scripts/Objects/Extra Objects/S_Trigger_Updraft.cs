using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class S_Trigger_Updraft : MonoBehaviour
{
	public Transform _Direction;
	public AnimationCurve _FallOfByPercentageDistance =  new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1),
				new Keyframe(1f, 0.7f),
			});

	public float	_setRange;
	[HideInInspector]
	public float        _getRange;
	[Min(10)]
	public float	_power = 100;

	public Transform	_Trigger;


	private void Start () {
		PlaceTrigger();
	}

#if UNITY_EDITOR
	//Affect collider object so it covers the start and reaches out to match range.
	private void Update () {
		PlaceTrigger();
	}
#endif

	private void PlaceTrigger () {
		_getRange = _setRange * transform.parent.localScale.y;
		_Trigger.position = transform.position + _Direction.up * (_getRange / 2); //Because the collider is centred to the object, move this halfway, then change size to match range in total.
		_Trigger.localScale = new Vector3(_Trigger.localScale.x, _setRange, _Trigger.localScale.z);
	}
}
