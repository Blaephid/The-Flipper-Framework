using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class S_Data_RedStarRing : S_Data_Base
{
	public Animator _Animator;
	public AudioSource _AudioSource;
	public VisualEffect _VisualEffect;

	[CustomReadOnly]
	public int _ringsGained = 10;
	[CustomReadOnly]
	public int _energyGained = 50;
	[CustomReadOnly]
	public int _powerGained = 20;

	private void Awake () {
		_VisualEffect.Stop();
	}

	public override void OnGet ( Transform Player ) {

		_Animator.SetTrigger("Get");
		_Animator.SetTrigger("Get"); //Done twice to ensure idle animation stops as it's on a different layer to ensure get animation doesn't reset rotation.
		_AudioSource.Play();

		_VisualEffect.Play();

		transform.parent = Player;
		StartCoroutine(LerpToAbovePlayer());
	}

	IEnumerator LerpToAbovePlayer () {

		Vector3 goalLocalPosition = new Vector3 (0, 7, 0);
		Vector3 startLocalPositoin = transform.localPosition;
		float timeToTake = 0.2f;
		float timeTaken  = 0;

		while (transform.localPosition != goalLocalPosition)
		{
			yield return new WaitForEndOfFrame();
			timeTaken += Time.deltaTime;
			transform.localPosition = Vector3.Lerp(startLocalPositoin, goalLocalPosition, timeTaken / timeToTake);
		}
	}

	//Called by animation
	public void EventGetAnimationEnd () {
		Destroy(gameObject);
	}
}
