#if (UNITY_EDITOR)
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Data Components/Checkpoint")]
public class S_Data_Checkpoint :  S_Data_Base
{
	public string _checkPointName = "X1";

	public bool _IsOn { get; set; }
	public float _checkpointDistance = 15;
	private float _currentdistance;
	public float _height = 15, _length = 3, _upOffset = 0;

	public GameObject LeftCheckpoint, RightCheckpoint;
	public GameObject Laser;

	public Transform CheckPos;
	public Animator[] Animators;


	private void Update () {
#if (UNITY_EDITOR)
		if (EditorApplication.isPlaying) return;
		LeftCheckpoint.transform.localPosition = Vector3.right * (_checkpointDistance * 0.5f);
		RightCheckpoint.transform.localPosition = -Vector3.right * (_checkpointDistance * 0.5f);
		Laser.transform.localScale = new Vector3(_checkpointDistance, 1, 1);
		GetComponent<BoxCollider>().size = new Vector3(_checkpointDistance, _height, _length);
		GetComponent<BoxCollider>().center = new Vector3(0, (_height * 0.5f) - 1 + _upOffset, _length * 0.5f);
#endif
	}



}
