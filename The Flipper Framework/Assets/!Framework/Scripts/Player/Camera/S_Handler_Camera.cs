using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEngine.Windows;

public class S_Handler_Camera : MonoBehaviour
{

	public S_HedgeCamera	_HedgeCam;
	public CinemachineVirtualCamera _VirtCam;
	private S_CharacterTools	_Tools;
	private S_PlayerInput	_Input;
	private S_PlayerPhysics	_PlayerPhys;

	private Transform             _MainSkin;

	[HideInInspector] public float _initialDistance;

	void Start () {
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
			if (col.TryGetComponent(out S_Trigger_Camera cameraData))
			{
				switch (cameraData.Type)
				{
					//Rotates the camera in direction and prevents controlled rotation.
					case TriggerType.LockToDirection:
						SetHedgeCamera(cameraData, col.transform.forward);
						LockCamera(true);
						changeDistance(cameraData);
						break;

						//Reneables camera control but still affects distance and other.
					case TriggerType.SetFree:
						_HedgeCam._cameraMaxDistance_ = _initialDistance;
						LockCamera(false);
						_HedgeCam._isReversed = false;
						break;

						//Nothing changes in control, but distance and height may change.
					case TriggerType.justEffect:
						changeDistance(cameraData);
						if (cameraData.willChangeAltitude)
							_HedgeCam.SetCameraHeightOnly(cameraData.newAltitude, cameraData.faceSpeed, cameraData.duration);
						break;

					//Allow controlled rotation but manually rotate in direction.
					case TriggerType.SetFreeAndLookTowards:
						SetHedgeCamera(cameraData, col.transform.forward);
						changeDistance(cameraData);
						LockCamera(false);
						break;

					//Make camera face behind player.
					case TriggerType.Reverse:				
						_HedgeCam._isReversed = true;
						SetHedgeCamera(cameraData, -GetComponent<S_CharacterTools>().CharacterAnimator.transform.forward);
						changeDistance(cameraData);
						LockCamera(false);
						break;

						//Make camera face behind player and disable rotation.
					case TriggerType.ReverseAndLockControl:
						_HedgeCam._isReversed = true;
						SetHedgeCamera(cameraData, -GetComponent<S_CharacterTools>().CharacterAnimator.transform.forward);
						changeDistance(cameraData);
						LockCamera(true) ;
						break;
				}


			}
		}

	}

	//Makes it so the camera will be further out from the player.
	void changeDistance(S_Trigger_Camera cameraData) {
		if (!cameraData.willChangeDistance)
		{
			_HedgeCam._cameraMaxDistance_ = _initialDistance;
		}
		else
		{
			_HedgeCam._cameraMaxDistance_ = cameraData.newDistance;
		}
	}

	//Will either make it so the camera can't be moved in the hedge cam script, or that it can.
	void LockCamera(bool state) {
		_HedgeCam._isMasterLocked = state;
		_HedgeCam._isLocked = state;
		_HedgeCam._canMove = !state;
	}

	//Calls the hedgecam to rotate towards or change height.
	void SetHedgeCamera(S_Trigger_Camera cameraData, Vector3 dir) {
		Quaternion targetRotation = Quaternion.LookRotation(cameraData.transform.forward, cameraData.transform.up);

		if (cameraData.willChangeAltitude)
			_HedgeCam.SetCameraToDirection(dir, cameraData.duration, cameraData.newAltitude, cameraData.faceSpeed, targetRotation, cameraData.willRotateCameraUpToThis);
		else
			_HedgeCam.SetCameraNoHeight(dir, cameraData.duration, cameraData.faceSpeed, targetRotation, cameraData.willRotateCameraUpToThis, cameraData.willRotateVertically);
	}

	public void EventTriggerExit ( Collider col ) {
		if (col.tag == "CameraTrigger")
		{
			//What happens depends on the data set to the camera trigger in its script.
			if (col.TryGetComponent(out S_Trigger_Camera cameraData))
			{
				//If trigger was set to undo effects on exit, then reset all data to how they should be again.
				if (cameraData.ReleaseOnExit)
				{
					_HedgeCam._cameraMaxDistance_ = _initialDistance;

					switch (cameraData.Type)
					{
						case TriggerType.LockToDirection:
							LockCamera(false);
							return;
						case TriggerType.Reverse:
							_HedgeCam._isReversed = false;
							break;
						case TriggerType.ReverseAndLockControl:
							_HedgeCam._isReversed = false;
							_HedgeCam._canMove = true;
							break;
					}
				}
			}
		}
	}

	//Certain actions will call this in input, where if button is pressed under right speed, then camera will reset to behind character's back.
	public void AttemptCameraReset () {
		//Set Camera to back
		if (_Input.CamResetPressed)
		{
			if(!_HedgeCam._isLocked) {
				if (_Input.moveVec == Vector2.zero && _PlayerPhys._horizontalSpeedMagnitude < 5f)
				{
					_HedgeCam.SetCameraToDirection(_MainSkin.forward, 0.25f, 0, 12, Quaternion.identity, false);
					_Input.CamResetPressed = false;
				}
			}
		}
	}



}
