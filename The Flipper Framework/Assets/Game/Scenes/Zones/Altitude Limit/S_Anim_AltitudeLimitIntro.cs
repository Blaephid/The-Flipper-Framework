using UnityEngine;

public class S_Anim_AltitudeLimitIntro : MonoBehaviour
{
	[SerializeField] ParticleSystem[] Explosions;

	public void AnimExplosion ( int i ) {
		if (i > Explosions.Length || Explosions[i] == null) return;

		Explosions[i].Play();

		if (Explosions[i].TryGetComponent(out AudioSource Source))
			Source.Play();

	}
}
