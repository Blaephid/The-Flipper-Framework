using UnityEngine;
using System.Collections;
using System;

public class S_AI_Health : MonoBehaviour, IHealthSystem
{

	public int _maxHealth = 1;
	int _currentHealth;

	public GameObject Explosion;
	public S_Spawn_Enemy SpawnReference { get; set; }

	public bool _willDestroy = true;

	public event System.Action<GameObject, S_AI_Health> OnDefeated;

	void Awake () {
		_currentHealth = _maxHealth;
	}

	public bool DealDamage ( int Damage ) {
		_currentHealth -= Damage;
		if (_currentHealth <= 0)
		{

			Defeated();
			return true;
		}
		else
			return false;
	}

	void Defeated () {
		if (SpawnReference != null)
		{
			SpawnReference.RestartSpawner();
		}
		else
		{
			S_Manager_LevelProgress.OnReset += EventReturnOnDeath;
		}

		GameObject.Instantiate(Explosion, transform.position, Quaternion.identity);

		if (_willDestroy)
			Destroy(gameObject);
		else
			gameObject.SetActive(false);

		OnDefeated.Invoke(gameObject, this);
	}

	void EventReturnOnDeath ( object sender, EventArgs e ) {
		_currentHealth = _maxHealth;
		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
	}

}
