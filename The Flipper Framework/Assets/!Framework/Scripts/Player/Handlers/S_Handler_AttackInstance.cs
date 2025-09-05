using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Handler_AttackInstance : MonoBehaviour
{
	public S_Handler_CharacterAttacks	_AttacksComponent;

	private void OnTriggerEnter ( Collider other ) {

		switch ( other.tag )
		{
			case "Enemy":
				_AttacksComponent.AttackThing(other, S_GeneralEnums.PlayerAttackTypes.Rolling, S_GeneralEnums.AttackTargets.Enemy, 1);
				break;
		}
	}
}
