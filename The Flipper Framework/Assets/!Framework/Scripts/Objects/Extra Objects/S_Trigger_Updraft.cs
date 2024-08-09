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
	public float _range;
	[Min(10)]
	public float _power = 100;

	public Transform _Trigger;

#if UNITY_EDITOR
	//Affect collider object so it covers the start and reaches out to match range.
	private void Update () {
		_Trigger.position = transform.position + _Direction.up * ((_range / 2) * transform.parent.localScale.y); //Because the collider is centred to the object, move this halfway, then change size to match range in total.
		_Trigger.localScale = new Vector3(_Trigger.localScale.x, _range, _Trigger.localScale.z);
	}
#endif
}
