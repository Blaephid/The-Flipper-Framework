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
		if(!S_SpawnCharacter._SpawnedPlayer) { return; }
		if (S_S_MoreMaths.GetDistanceSqrOfVectors(S_SpawnCharacter._SpawnedPlayer.position, transform.position) < Mathf.Pow(Distance, 2))
		{
			Particle.Emit(1);
		}
	}
}
