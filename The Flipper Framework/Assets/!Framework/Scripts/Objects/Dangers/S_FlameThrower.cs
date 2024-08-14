using UnityEngine;
using System.Collections;

public class S_FlameThrower : MonoBehaviour
{


	public float Distance;
	public ParticleSystem Particle;

	void Start () {


		InvokeRepeating("Flame", 0.1f, 1.0f);
	}

	void Flame () {
		if (Vector3.Distance(S_SpawnCharacter._SpawnedPlayer.position, transform.position) < Distance)
		{
			Particle.Emit(1);
		}
	}
}
