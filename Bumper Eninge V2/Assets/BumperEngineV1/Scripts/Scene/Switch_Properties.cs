using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch_Properties : MonoBehaviour {

	[SerializeField] private string TriggerString;
	[SerializeField] private Animator AnimatorToSendTriggerTo;

	[SerializeField] private Material ActiveMaterial;
	[SerializeField] private MeshRenderer MeshToSwapMaterial;
	[SerializeField] private AudioSource SoundEffect;

	// Use this for initialization
	public void Activate () {

			if (AnimatorToSendTriggerTo != null) {
				AnimatorToSendTriggerTo.SetTrigger (TriggerString);
			}
			if (MeshToSwapMaterial != null) {
				MeshToSwapMaterial.GetComponent<Renderer> ().material = ActiveMaterial;
			}
			if (SoundEffect != null) {
				SoundEffect.PlayOneShot (SoundEffect.clip);
			}

		this.gameObject.SetActive (false);

	}

}
