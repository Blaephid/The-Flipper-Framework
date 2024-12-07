using UnityEngine;
using System.Collections;

public class S_UVScroll : MonoBehaviour 
{
	[SerializeField] float scrollSpeed = 0.5F;
	[SerializeField] Material Mat;
	[SerializeField] Vector2 ScrollVector;

	void OnEnable()
	{
		Mat.SetTextureOffset("_MainTex",new Vector2(0,0));
	}

	void Update() {
		float offset = Mathf.Repeat(Time.time * scrollSpeed,100);
		Mat.SetTextureOffset("_MainTex",new Vector2(ScrollVector.x*offset,ScrollVector.y*offset));

	}
	void OnDisable()
	{
		Mat.SetTextureOffset("_MainTex",new Vector2(0,0));
	}
}