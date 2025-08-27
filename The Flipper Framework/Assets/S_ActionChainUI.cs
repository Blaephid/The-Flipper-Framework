using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S_ActionChainUI : MonoBehaviour
{
	public Transform parent;
	public GameObject ListObjectToSpawn;
	private List<Animator> ListAnimators = new List<Animator>();

	public void SpawnNewText (string source, int value) {

		//Move all other list objects down.
		foreach (Animator animator in ListAnimators)
		{
			animator.SetInteger("PlaceInStack", animator.GetInteger("PlaceInStack") + 1);

			if (animator.GetInteger("PlaceInStack") > 4) 
			{
				animator.SetBool("End", true);
				StartCoroutine(DestoryAfterTime(animator, 2)); 
			}
		}

		//Spawn a new element to go in the list.
		GameObject GO = Instantiate(ListObjectToSpawn, parent);

		Animator newAnim = GO.GetComponent<Animator>();
		newAnim.SetInteger("PlaceInStack", 1);
		newAnim.SetBool("Valid", value > 0);

		ListAnimators.Add(newAnim);

		StartCoroutine(DestoryAfterTime(newAnim)); //To prevent the spawned objects from just spawning infinitely.

		//Ensure it displays what action was performed.
		TextMeshProUGUI Text = GO.GetComponentInChildren<TextMeshProUGUI>();

		if(value > 0)
			Text.text = source + "  +" + value;
		else
			Text.text = source;
	}

	public void EndChain () {
		//Clear list
		foreach (Animator animator in ListAnimators)
		{
			//animator.transform.SetParent(null);
			animator.SetBool("End", true);
			StartCoroutine(DestoryAfterTime(animator, 4));
		}
	}

	private IEnumerator DestoryAfterTime (Animator anim, float time = 10) {

		yield return new WaitForSeconds(time);

		if (anim != null)
			DestroyAnimator(anim);
	}

	private void DestroyAnimator ( Animator anim) {

		ListAnimators.Remove(anim);
		GameObject.Destroy(anim.gameObject);
	}
}
