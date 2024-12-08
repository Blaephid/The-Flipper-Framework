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
	private List<S_Trigger_Camera> _CurrentActiveCameraTriggers = new List<S_Trigger_Camera>();

	void Awake () {
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_MainSkin = _Tools.MainSkin;

		_initialDistance = _Tools.CameraStats.DistanceStats.CameraDistance;
	}

	//Called when entering a trigger in the physics script (must be assigned in Unity editor)
	public void EventTriggerEnter ( Collider col ) {
		if (col.tag == "CameraTrigger")
		{
			//What happens depends on the data set to the camera trigger in its script.
			if (!col.TryGetComponent(out S_Trigger_Camera cameraData)) { return; }

			cameraData = cameraData._TriggerForPlayerToRead.GetComponent<S_Trigger_Camera>();

			//If no logic is found, ignore.
			if (cameraData == null) return;

			//If either there isn't any camera logic already in effect, or this is a new trigger unlike the already active one, set this as the first active.
			if (_CurrentActiveCameraTriggers.Count == 0) { _CurrentActiveCameraTriggers = new List<S_Trigger_Camera>() { cameraData }; }

			//If the new trigger is set to trigger the logic already in effect, add it to list for tracking how long until out of every trigger, then don't restart the logic.
			else if (cameraData == _CurrentActiveCameraTriggers[0]) { _CurrentActiveCameraTriggers.Add(cameraData); return; }

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

	}

	//Makes it so the camera will be further out from the player.
	void ChangeCameraDistance ( S_Trigger_Camera cameraData ) {
		if (!cameraData._willChangeDistance)
		{
			_HedgeCam._cameraMaxDistance_ = _initialDistance;
		}
		else
		{
			_HedgeCam._cameraMaxDistance_ = cameraData._newDistance;
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

	public void EventTriggerExit ( Collider col ) {
		if (col.tag == "CameraTrigger")
		{
			//What happens depends on the data set to the camera trigger in its script.
			if (!col.TryGetComponent(out S_Trigger_Camera cameraData)) { return; }

			cameraData = cameraData._TriggerForPlayerToRead.GetComponent<S_Trigger_Camera>();
			if (cameraData == null) return;

			//If the trigger exited is NOT set to the same logic as currently active, then don't do anything.
			if (_CurrentActiveCameraTriggers.Count > 0 & cameraData != _CurrentActiveCameraTriggers[0]) { return ; }
			//If it is, then remove one from the list to track how many triggers under the same logic have been left. This allows the effect to not end until not in any triggers under the same logic.
			_CurrentActiveCameraTriggers.RemoveAt(_CurrentActiveCameraTriggers.Count - 1);

			if(_CurrentActiveCameraTriggers.Count > 0) { return; } //Only perform exit logic when out of all triggers using that logic.

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
}
