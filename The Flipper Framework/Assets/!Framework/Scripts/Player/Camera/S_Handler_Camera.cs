using UnityEngine;
using Cinemachine;
using System.Collections;

public class S_Handler_Camera : MonoBehaviour
{

	public S_HedgeCamera _HedgeCam;
	public CinemachineVirtualCamera _VirtCam;
	[HideInInspector] public float _initialDistance;

	void Awake () {
		_initialDistance = _HedgeCam._cameraMaxDistance_;
	}


	public void OnTriggerEnter ( Collider col ) {
		if (col.tag == "CameraTrigger")
		{
			if (col.GetComponent<S_Trigger_Camera>() != null)
			{
				S_Trigger_Camera cameraData = col.GetComponent<S_Trigger_Camera>();

				switch (cameraData.Type)
				{
					case TriggerType.LockToDirection:
						setHedgeCamera(cameraData, col.transform.forward);
						LockCamera(true);
						changeDistance(cameraData);
						break;

					case TriggerType.SetFree:
						_HedgeCam._cameraMaxDistance_ = _initialDistance;
						LockCamera(false);
						_HedgeCam._isReversed = false;
						break;

					case TriggerType.justEffect:
						changeDistance(cameraData);
						if (cameraData.changeAltitude)
							_HedgeCam.SetCameraNoLook(cameraData.CameraAltitude);
						break;


					case TriggerType.SetFreeAndLookTowards:
						setHedgeCamera(cameraData, col.transform.forward);
						changeDistance(cameraData);
						LockCamera(false);
						break;

					case TriggerType.Reverse:				
						_HedgeCam._isReversed = true;
						setHedgeCamera(cameraData, -GetComponent<S_CharacterTools>().CharacterAnimator.transform.forward);
						changeDistance(cameraData);
						LockCamera(false);
						break;

					case TriggerType.ReverseAndLockControl:
						_HedgeCam._isReversed = true;
						setHedgeCamera(cameraData, -GetComponent<S_CharacterTools>().CharacterAnimator.transform.forward);
						changeDistance(cameraData);
						LockCamera(true) ;
						break;
				}


			}
		}

	}

	void changeDistance(S_Trigger_Camera cameraData) {
		if (!cameraData.changeDistance)
		{
			_HedgeCam._cameraMaxDistance_ = _initialDistance;
		}
		else
		{
			_HedgeCam._cameraMaxDistance_ = cameraData.ChangeDistance;
		}
	}

	void LockCamera(bool state) {
		_HedgeCam._isMasterLocked = state;
		_HedgeCam._isLocked = state;
		_HedgeCam._canMove = !state;
	}

	void setHedgeCamera(S_Trigger_Camera cameraData, Vector3 dir) {
		Quaternion targetRotation = Quaternion.LookRotation(cameraData.transform.forward, cameraData.transform.up);

		if (cameraData.changeAltitude)
			_HedgeCam.SetCameraToDirection(dir, 2.5f, cameraData.CameraAltitude, cameraData.FaceSpeed, targetRotation, cameraData.shouldRotateCameraUpToThis);
		else
			_HedgeCam.SetCameraNoHeight(dir, 2.5f, cameraData.FaceSpeed, targetRotation, cameraData.shouldRotateCameraUpToThis);
	}

	public void OnTriggerExit ( Collider col ) {
		if (col.tag == "CameraTrigger")
		{
			S_Trigger_Camera cameraData = col.GetComponent<S_Trigger_Camera>();
			if (cameraData != null)
			{
				if (cameraData.Type == TriggerType.LockToDirection && cameraData.ReleaseOnExit)
				{
					_HedgeCam._cameraMaxDistance_ = _initialDistance;

					_HedgeCam._isMasterLocked = false;
					_HedgeCam._isLocked = false;
					_HedgeCam._canMove = true;
					_HedgeCam._lookTimer = 0;
				}

				else if (cameraData.ReleaseOnExit)
				{
					_HedgeCam._lookTimer = 0;
					_HedgeCam._cameraMaxDistance_ = _initialDistance;

					if (cameraData.Type == TriggerType.Reverse)
					{
						_HedgeCam._isReversed = false;
					}
					else if (cameraData.Type == TriggerType.ReverseAndLockControl)
					{
						_HedgeCam._isReversed = false;
						_HedgeCam._canMove = true;
					}
				}
			}
		}
	}




}
