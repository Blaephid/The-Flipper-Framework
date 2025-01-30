using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEngine.Windows;
using System;
using System.Collections.Generic;
using Unity.Collections;

public class S_Handler_Camera : MonoBehaviour
{

	public S_HedgeCamera          _HedgeCam;
	public CinemachineVirtualCamera _VirtCam;
	private S_CharacterTools      _Tools;
	private S_PlayerInput         _Input;
	private S_PlayerPhysics       _PlayerPhys;

	private Transform             _MainSkin;

	[HideInInspector] public float _initialDistance;

	//This is used to check what the current dominant trigger is, as multiple triggers might be working together under one effect. These will have their read values set to the same.
	private List<S_Trigger_Base> _CurrentActiveCameraTriggers = new List<S_Trigger_Base>();

	void Awake () {
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_MainSkin = _Tools.MainSkin;

		_initialDistance = _Tools.CameraStats.DistanceStats.CameraDistance;
	}

	#region Trigger Interaction

	//Called when entering a trigger in the physics script (must be assigned in Unity editor)
	public void EventTriggerEnter ( Collider col ) {
		if (col.tag == "CameraTrigger")
		{
			CheckCameraTriggerEnter(col);
		}
	}

	public void CheckCameraTriggerEnter (Collider Col) {
		//This static method determines the data of the trigger entered, and returns data if its different, or null if it isn't. It also adds to the list of camera triggers if it shares data.
		S_Trigger_Camera cameraData = S_Interaction_Triggers.CheckTriggerEnter(Col, ref _CurrentActiveCameraTriggers) as S_Trigger_Camera;
		if(cameraData) { StartCameraEffect(cameraData); }
	}

	public void EventTriggerExit ( Collider col ) {
		if (col.tag == "CameraTrigger")
		{
			CheckCameraTriggerExit(col);
		}
	}

	public void CheckCameraTriggerExit ( Collider Col ) {
		//This static method determines the data of the trigger entered, and returns data if its different, or null if it isn't. It also adds to the list of camera triggers if it shares data.
		S_Trigger_Camera cameraData = S_Interaction_Triggers.CheckTriggerExit(Col, ref _CurrentActiveCameraTriggers) as S_Trigger_Camera;
		if(cameraData) EndCameraEffect(cameraData);
	}


	private void StartCameraEffect ( S_Trigger_Camera cameraData ) {
		switch (cameraData._whatType)
		{
			//Rotates the camera in direction and prevents controlled rotation.
			case CameraControlType.LockToDirection:
				SetHedgeCamera(cameraData, cameraData._forward);
				LockCamera(true);
				ChangeCameraDistance(cameraData);
				break;

			//Reneables camera control but still affects distance and other.
			case CameraControlType.SetFree:
				_HedgeCam._cameraMaxDistance_ = _initialDistance;
				LockCamera(false);
				_HedgeCam._isReversed = false;
				break;

			//Nothing changes in control, but distance and height may change.
			case CameraControlType.justEffect:
				ChangeCameraDistance(cameraData);
				if (cameraData._willChangeAltitude)
					_HedgeCam.SetCameraHeightOnly(cameraData._newAltitude, cameraData._faceSpeed, cameraData._duration);
				break;

			//Allow controlled rotation but manually rotate in direction.
			case CameraControlType.SetFreeAndLookTowards:
				SetHedgeCamera(cameraData, cameraData._forward);
				ChangeCameraDistance(cameraData);
				LockCamera(false);
				break;


			//Make camera face behind player.
			case CameraControlType.Reverse:
				_HedgeCam._isReversed = true;
				SetHedgeCamera(cameraData, -_MainSkin.forward);
				ChangeCameraDistance(cameraData);
				LockCamera(false);
				break;

			//Make camera face behind player and disable rotation.
			case CameraControlType.ReverseAndLockControl:
				_HedgeCam._isReversed = true;
				SetHedgeCamera(cameraData, -_MainSkin.forward);
				ChangeCameraDistance(cameraData);
				LockCamera(true);
				break;
		}
	}


	private void EndCameraEffect ( S_Trigger_Camera cameraData ) {
		//If trigger was set to undo effects on exit, then reset all data to how they should be again.
		if (cameraData._willReleaseOnExit)
		{
			_HedgeCam._cameraMaxDistance_ = _initialDistance;
			_HedgeCam._lookTimer = -Time.fixedDeltaTime; // To ensure the HedgeCamera script will end the look timer countdown and apply necessary changes.

			switch (cameraData._whatType)
			{
				case CameraControlType.LockToDirection:
					LockCamera(false);
					return;
				case CameraControlType.Reverse:
					_HedgeCam._isReversed = false;
					break;
				case CameraControlType.ReverseAndLockControl:
					_HedgeCam._isReversed = false;
					_HedgeCam._canMove = true;
					break;
			}
		}
	}

	#endregion

	#region Camera Effects

	//Makes it so the camera will be further out from the player.
	void ChangeCameraDistance ( S_Trigger_Camera cameraData ) {

		if (cameraData._willChangeDistance)
			StartCoroutine(LerpToNewDistance(10, cameraData._newDistance));
		//if (!cameraData._willChangeDistance)
		//{
		//	_HedgeCam._cameraMaxDistance_ = _initialDistance;
		//}
		//else
		//{
		//	_HedgeCam._cameraMaxDistance_ = cameraData._newDistance;
		//}
	}

	private IEnumerator LerpToNewDistance(int frames, float distance) {
		float startDistance = _HedgeCam._cameraMaxDistance_;
		for (float f = 1f / frames ; f <= 1 ; f =+ 1f/frames)
		{
			yield return new WaitForFixedUpdate();
			_HedgeCam._cameraMaxDistance_ = Mathf.Lerp(startDistance, distance, f);
		}
	}

	//Will either make it so the camera can't be moved in the hedge cam script, or that it can.
	public void LockCamera ( bool state ) {
		_HedgeCam._isMasterLocked = state;
		_HedgeCam._isLocked = state;
		_HedgeCam._canMove = !state;
	}

	//Calls the hedgecam to rotate towards or change height.
	void SetHedgeCamera ( S_Trigger_Camera cameraData, Vector3 direction ) {
		Vector3 targetUpDirection = cameraData._willRotateCameraUpToThis
			? cameraData.transform.up : Vector3.zero;


		if (cameraData._willChangeAltitude)
			_HedgeCam.SetCameraWithSeperateHeight(direction, cameraData._duration, cameraData._newAltitude, cameraData._faceSpeed, targetUpDirection);
		else
			_HedgeCam.SetCameraNoSeperateHeight(direction, cameraData._duration, cameraData._faceSpeed, targetUpDirection, cameraData._willRotateVertically);
	}


	//Certain actions will call this in input, where if button is pressed under right speed, then camera will reset to behind character's back.
	public void AttemptCameraReset () {
		//Set Camera to back
		if (_Input._CamResetPressed)
		{
			if (!_HedgeCam._isLocked)
			{
				if (_Input.moveVec == Vector2.zero && _PlayerPhys._PlayerVelocity._horizontalSpeedMagnitude < 5f)
				{
					_HedgeCam.SetCameraWithSeperateHeight(_MainSkin.forward, 0.25f, 0, 12, Vector3.zero);
					_Input._CamResetPressed = false;
				}
			}
		}
	}

	public void ResetOnDeath () {
		LockCamera(false);

		_HedgeCam._cameraMaxDistance_ = _initialDistance;
		_HedgeCam._lookTimer = -Time.fixedDeltaTime; // To ensure the HedgeCamera script will end the look timer countdown and apply necessary changes.

	}
	#endregion
}
