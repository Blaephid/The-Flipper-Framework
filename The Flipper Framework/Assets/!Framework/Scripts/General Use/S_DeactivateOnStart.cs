using UnityEngine;
using System.Collections;

public class S_DeactivateOnStart : MonoBehaviour {

	public bool StopRender;
	public bool Des = false;
	void Start () {

		if (StopRender)
		{
			gameObject.GetComponent<Renderer>().enabled = false;
		}
		else if (Des)
        {
			Destroy(gameObject);
        }
		else
			gameObject.SetActive(false);



	}
	
}
