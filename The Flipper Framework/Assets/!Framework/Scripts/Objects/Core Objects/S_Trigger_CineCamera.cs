using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class S_Trigger_CineCamera : MonoBehaviour
{
	[Header ("Major settings.")]
	public bool endCine = false;
	public bool Active = true;
	public bool isEnabled = true;
	public bool onExit = true;
	public float timeDelay = 0;
	public bool startAtCameraPoint;
	public Vector3 startOffset;

	[Header("Attached Elements")]
	public CinemachineVirtualCamera virCam;
	public GameObject attachedCam;

	[Header("Works with these actions")]
	public bool RegularAction = true;
	public bool JumpAction = false;
	public bool RailAction = false;
	public bool wallRunAction = false;
	public bool RingRoadAction = false;



	private Vector3 camPosit;
	private Quaternion camRotit;

	[Header("Effects on/with Player")]
	public bool lookPlayer;
	public bool followPlayer;

	public bool disableMove;


	CinemachineVirtualCamera hedgeCam;

	S_CharacterTools _PlayerTools;
	S_ActionManager Actions;

	[Header("On Cancel")]
	public bool setBehind = true;
	public float lockTime = 5f;

	bool isActive = false;

	// Start is called before the first frame update

	void Awake () {
		camPosit = attachedCam.transform.position;
		camRotit = attachedCam.transform.rotation;
		attachedCam.SetActive(false);
	}


	private void OnTriggerEnter ( Collider other ) {
		if (other.tag == "Player" && isEnabled)
		{
			if (!endCine)
			{
				_PlayerTools = other.GetComponentInParent<S_CharacterTools>();
				Actions = _PlayerTools._ActionManager;
			}
			else
				DeactivateCam(lockTime);
		}
	}
	private void OnTriggerStay ( Collider col ) {
		if (col.tag == "Player" && isEnabled && !endCine)
		{
			if (!isActive && _PlayerTools != null)
			{
				if (Actions._whatAction == S_Enums.PrimaryPlayerStates.Path || (Actions._whatAction == S_Enums.PrimaryPlayerStates.Default && RegularAction) || (Actions._whatAction == S_Enums.PrimaryPlayerStates.Jump && JumpAction)
				    || (Actions._whatAction == S_Enums.PrimaryPlayerStates.Rail && RailAction) || (Actions._whatAction == S_Enums.PrimaryPlayerStates.WallRunning && wallRunAction) || (Actions._whatAction == S_Enums.PrimaryPlayerStates.RingRoad && RingRoadAction))
				{
					isActive = true;
					hedgeCam = _PlayerTools.CamHandler._VirtCam;


					ActivateCam(5f);

					if (lookPlayer)
					{
						virCam.LookAt = _PlayerTools.transform;
					}

					if (followPlayer)
					{
						virCam.Follow = _PlayerTools.transform;
					}

				}
			}
			else
			{
				if (!(
				    Actions._whatAction == S_Enums.PrimaryPlayerStates.Default && RegularAction) && !(Actions._whatAction == S_Enums.PrimaryPlayerStates.Jump && JumpAction) &&
				    !(Actions._whatAction == S_Enums.PrimaryPlayerStates.Rail && RailAction) && !(Actions._whatAction == S_Enums.PrimaryPlayerStates.WallRunning && wallRunAction) && !(Actions._whatAction == S_Enums.PrimaryPlayerStates.RingRoad && RingRoadAction) && onExit)
				{
					DeactivateCam(0);
				}
			}

		}
	}

	void OnTriggerExit ( Collider col ) {
		if (isActive)
		{
			if (col.tag == "Player" && onExit)
			{
				DeactivateCam(lockTime);

			}
		}
	}

	public void ActivateCam ( float disableFor ) {

		if (startAtCameraPoint)
		{
			attachedCam.transform.position = _PlayerTools.GetComponent<S_Handler_Camera>()._HedgeCam.transform.position;
			attachedCam.transform.rotation = _PlayerTools.GetComponent<S_Handler_Camera>()._HedgeCam.transform.rotation;
		}

		attachedCam.transform.position += startOffset;

		attachedCam.SetActive(true);
		hedgeCam = _PlayerTools.GetComponent<S_Handler_Camera>()._VirtCam;
		hedgeCam.gameObject.SetActive(false);
		if (disableFor > 0)
			_PlayerTools.GetComponent<S_PlayerInput>().LockInputForAWhile(disableFor, true, _PlayerTools.GetComponent<S_PlayerPhysics>()._moveInput);

		if (timeDelay != 0)
		{
			StartCoroutine(TimeLimit());
		}

	}

	IEnumerator TimeLimit () {
		isEnabled = false;
		yield return new WaitForSeconds(timeDelay);
		DeactivateCam(lockTime);
		isEnabled = true;
	}

	public void DeactivateCam ( float disableFor ) {

		if (setBehind)
		{
			_PlayerTools.GetComponent<S_CharacterTools>().CamHandler._HedgeCam.SetBehind(0);
		}

		isActive = false;
		attachedCam.transform.position = camPosit;
		attachedCam.transform.rotation = camRotit;
		hedgeCam.gameObject.SetActive(true);
		attachedCam.SetActive(false);
		if (disableFor > 0)
			_PlayerTools.GetComponent<S_PlayerInput>().LockInputForAWhile(disableFor, true, _PlayerTools.GetComponent<S_PlayerPhysics>()._moveInput);
	}
}
