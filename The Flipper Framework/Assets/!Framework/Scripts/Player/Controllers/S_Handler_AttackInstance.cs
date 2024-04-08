using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_AttackInstance : MonoBehaviour
{
	public S_Handler_CharacterAttacks	_AttacksComponent;

	private void OnTriggerEnter ( Collider other ) {
		Debug.Log(other.tag);

		switch ( other.tag )
		{
			case "Enemy":
				_AttacksComponent.AttackThing(other, S_Enums.PlayerAttackTypes.Rolling, S_Enums.AttackTargets.Enemy, 1);
				break;
		}
	}
}
