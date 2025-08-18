using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_CustomPlayerTriggers : MonoBehaviour
{
	public S_PlayerPhysics _PlayerPhys;
	public Collider Collider;

	public LayerMask LayerMask;
	public S_EditorEnums.ColliderTypes _whatTriggerShape;

	private void OnValidate () {
		//Update components based on the enum
		Collider = S_S_Editor.HandleColliderComponentsByEnum(gameObject, _whatTriggerShape);
		Collider.enabled = false;
	}
	private void FixedUpdate () {
		if(!_PlayerPhys) { return; }

		Collider[] Colliders = new Collider[0];

		//use the information on the attached collider to determine the shape of the trigger area.
		switch (_whatTriggerShape) {
			case S_EditorEnums.ColliderTypes.Box:
				Colliders = Physics.OverlapBox(
					Collider.bounds.center,
					Collider.bounds.extents,
					Collider.transform.rotation,
					LayerMask,
					QueryTriggerInteraction.Collide
				);
				break;

			case S_EditorEnums.ColliderTypes.Sphere:
				Colliders = Physics.OverlapSphere(
					Collider.bounds.center,
					Collider.bounds.extents.x, // Assuming uniform scale for sphere
					LayerMask,
					QueryTriggerInteraction.Collide
				);
				break;
		}

		//For any triggers found, add to the automatic list handled in player physic.
		for (int i = 0; i < Colliders.Length; i++) {
				_PlayerPhys._ListOfTriggersEnteredThisFrame.Add(Collider);
		}
	}
}
