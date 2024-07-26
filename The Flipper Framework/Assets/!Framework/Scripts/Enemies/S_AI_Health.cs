using UnityEngine;
using System.Collections;

public class S_AI_Health : MonoBehaviour, IHealthSystem
{

	public int _maxHealth = 1;
	int _currentHealth;

	public GameObject Explosion;
	public S_Spawn_Enemy SpawnReference { get; set; }

	public bool _willDestroy = true;

	void Awake () {
		_currentHealth = _maxHealth;
	}

	public bool DealDamage ( int Damage ) {
		_currentHealth -= Damage;
		if (_currentHealth <= 0)
		{
			if (SpawnReference != null)
			{
				SpawnReference.RestartSpawner();
			}
			GameObject.Instantiate(Explosion, transform.position, Quaternion.identity);

			if (_willDestroy)
				Destroy(gameObject);
			else
				gameObject.SetActive(false);
			return true;
		}
		else
			return false;
	}

}
