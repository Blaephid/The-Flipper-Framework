#if (UNITY_EDITOR)
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class S_Data_Checkpoint :  S_Data_Base
{
	public string checkPointName = "X1";

	public bool IsOn { get; set; }
	public float CheckpointDistance = 15;
	float currentdistance;
	public float CheckpointHeight = 15, CheckpointLenght = 3, CheckPointUpOffset = 0;

	public GameObject LeftCheckpoint, RightCheckpoint;
	public GameObject Laser;

	public Transform CheckPos;
	public Animator[] Animators;



	private void Update () {
#if (UNITY_EDITOR)
		if (EditorApplication.isPlaying) return;
		LeftCheckpoint.transform.localPosition = Vector3.right * (CheckpointDistance * 0.5f);
		RightCheckpoint.transform.localPosition = -Vector3.right * (CheckpointDistance * 0.5f);
		Laser.transform.localScale = new Vector3(CheckpointDistance, 1, 1);
		GetComponent<BoxCollider>().size = new Vector3(CheckpointDistance, CheckpointHeight, CheckpointLenght);
		GetComponent<BoxCollider>().center = new Vector3(0, (CheckpointHeight * 0.5f) - 1 + CheckPointUpOffset, CheckpointLenght * 0.5f);
#endif
	}



}
