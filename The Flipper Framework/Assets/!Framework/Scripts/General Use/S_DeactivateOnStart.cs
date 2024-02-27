using UnityEngine;
using System.Collections;

public class S_DeactivateOnStart : MonoBehaviour
{

	public enumDeactivate _whatAction = enumDeactivate.Deactivate;

	public enum enumDeactivate
	{
		stopRendering,
		Destroy,
		Deactivate
	}

	void Start () {

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
