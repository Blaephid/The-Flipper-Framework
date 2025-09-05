using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Data Components/Homing Target")]
public class S_Data_HomingTarget : S_Data_Base
{
#if UNITY_EDITOR
	S_Data_HomingTarget () {
		_hasVisualisationScripted = true;
		_selectedOutlineColour = Color.red;
		_normalOutlineColour = Color.red;
		_normalOutlineColour.a = 0.5f;
		_distanceModifier = 1;
	}
#endif

	public Vector3 _offset = Vector3.up;
	public TargetType type = TargetType.destroy;
	public EffectOnHoming OnHit = EffectOnHoming.normal;
	public EffectOnHoming OnDestroy = EffectOnHoming.normal;

	[Tooltip("When checking if this is a valid target, must be within the homing actions distance * this. Used to make some homing targets more difficult.")]
	[Range(0.1f,1)]
	public float _distanceModifier = 1;

	//These are used to track how fast the target is moving to adjust homing attack speed.
	[HideInInspector] public Vector3 _positionLastFixedUpdate;
	[HideInInspector] public Vector3 _positionThisFixedUpdate = Vector3.one;

	public enum TargetType { normal, destroy}
	public enum EffectOnHoming { normal, shootdownWithCarry, shootdownStraight}

	private void Start () {
		_offset = transform.rotation * _offset;
	}

	private void OnEnable () {
		gameObject.layer = 17;
	}

	private void FixedUpdate () {
		_positionLastFixedUpdate = _positionThisFixedUpdate;
		_positionThisFixedUpdate = transform.position;
	}

#if UNITY_EDITOR

	public override void DrawGizmosAndHandles ( bool selected ) {
		if(selected) {Gizmos.color = _selectedFillColour; }
		else { Gizmos.color = _normalOutlineColour; }

		float size = selected ? 4 : 3;

		Gizmos.DrawWireSphere(transform.position + (transform.rotation * _offset), size);
	}

#endif
}
