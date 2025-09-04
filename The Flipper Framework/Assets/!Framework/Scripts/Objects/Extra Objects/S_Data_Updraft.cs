using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Data Components/Updraft")]
public class S_Data_Updraft : S_Data_Base
{
	public Transform _Direction;
	public AnimationCurve _FallOfByPercentageDistance =  new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0, 1),
				new Keyframe(1f, 0.7f),
			});

	public float    _setRange;
	[HideInInspector]
	public float        _totalRangeSquare;
	[Min(10)]
	public float    _power = 100;

	public Transform        _Trigger;


	private void Awake () {
		PlaceTrigger();
	}

#if UNITY_EDITOR
	//Affect collider object so it covers the start and reaches out to match range.
	private void Update () {
		PlaceTrigger();
	}

	public override void OnValidate () {
		base.OnValidate();

		Keyframe lastKey = _FallOfByPercentageDistance[_FallOfByPercentageDistance.length - 1];

		// Only update if it's not already at (1,0)
		if (lastKey.value == 0 && lastKey.time == 1) { return; }

		lastKey.time = 1f;
		lastKey.value = 0f;
		_FallOfByPercentageDistance.MoveKey(_FallOfByPercentageDistance.length - 1, lastKey);
	}
#endif

	private void PlaceTrigger () {
		_totalRangeSquare = _setRange * transform.parent.localScale.y;
		_Trigger.position = transform.position + _Direction.up * (_totalRangeSquare / 2); //Because the collider is centred to the object, move this halfway, then change size to match range in total.
		_Trigger.localScale = new Vector3(_Trigger.localScale.x, _setRange, _Trigger.localScale.z);
		_totalRangeSquare = Mathf.Pow(_totalRangeSquare, 2);
	}
}
