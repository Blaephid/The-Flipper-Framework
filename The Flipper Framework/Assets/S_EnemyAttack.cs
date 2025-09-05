using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_EnemyAttack : MonoBehaviour
{
	public S_EnemyAttack () {
		_isHazzard = true;
	}

	[CustomReadOnly]
	public bool _isHazzard  = true;

	[CustomReadOnly]
	public bool _isInvincible;
}
