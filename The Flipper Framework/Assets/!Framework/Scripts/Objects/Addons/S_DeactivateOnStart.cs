using UnityEngine;
using System.Collections;

public class S_DeactivateOnStart : MonoBehaviour
{

	public enumDeactivate _whatAction = enumDeactivate.Deactivate;
	public float _delayInSeconds;

	public enum enumDeactivate
	{
		stopRendering,
		Destroy,
		Deactivate
	}

	void Start () {
		StartCoroutine(Delay());
	}

	IEnumerator Delay() {
		yield return new WaitForSeconds(_delayInSeconds);
		Deactivate();
	}

	void Deactivate () {

		switch (_whatAction)
		{
			case enumDeactivate.Deactivate:
				gameObject.SetActive(false);
				break;
			case enumDeactivate.stopRendering:
				gameObject.GetComponent<Renderer>().enabled = false;
				break;
			case enumDeactivate.Destroy:
				Destroy(gameObject);
				break;
		}
	}

}
