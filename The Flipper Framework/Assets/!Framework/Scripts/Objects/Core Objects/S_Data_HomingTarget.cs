using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Data_HomingTarget : MonoBehaviour, IObjectData
{

	public Vector3 _offset = Vector3.up;
	public Color drawColour = Color.red;
	public TargetType type = TargetType.destroy;

	public enum TargetType { normal, destroy}

	private void Start () {
		_offset = transform.rotation * _offset;
	}


	private void OnDrawGizmosSelected () {
		Gizmos.color = drawColour;
		Gizmos.DrawWireSphere(transform.position + (transform.rotation * _offset), 2);
	}

}
