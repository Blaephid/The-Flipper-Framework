using UnityEngine;

[ExecuteAlways]
public class S_LockScale : MonoBehaviour
{
	[SerializeField] float setScale = 1;

#if UNITY_EDITOR
	private void Awake () {
		if (Application.isPlaying) { enabled = false; }
	}
#endif
	// Update is called once per frame
	void Update () {
		if(transform.parent == null) { enabled = false; return; }

		transform.rotation = transform.parent.rotation;

		transform.localScale = S_S_Objects.LockScale(transform, setScale);
	}
}
