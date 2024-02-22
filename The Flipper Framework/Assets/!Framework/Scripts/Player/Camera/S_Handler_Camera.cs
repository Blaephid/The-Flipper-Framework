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
						Vector3 dir = col.transform.forward;
						if (cameraData.changeAltitude)
							_HedgeCam.SetCamera(dir, cameraData.duration, cameraData.CameraAltitude, cameraData.FaceSpeed);
						else
							_HedgeCam.SetCameraNoHeight(dir, cameraData.duration, cameraData.FaceSpeed);
						_HedgeCam._isMasterLocked = true;
						_HedgeCam._canMove = false;
						_HedgeCam._isLocked = true;
						if (cameraData.changeDistance)
						{
							_HedgeCam._cameraMaxDistance_ = cameraData.ChangeDistance;
						}
						else
						{
							_HedgeCam._cameraMaxDistance_ = _initialDistance;
						}
						break;

					case TriggerType.SetFree:
						_HedgeCam._cameraMaxDistance_ = _initialDistance;
						_HedgeCam._isMasterLocked = false;
						_HedgeCam._isReversed = false;
						_HedgeCam._isLocked = false;
						_HedgeCam._canMove = true;
						break;

					case TriggerType.justEffect:
						if (!cameraData.changeDistance)
						{
							_HedgeCam._cameraMaxDistance_ = _initialDistance;
						}
						else
						{
							_HedgeCam._cameraMaxDistance_ = cameraData.ChangeDistance;
						}
						if (cameraData.changeAltitude)
							_HedgeCam.SetCameraNoLook(cameraData.CameraAltitude);

						break;


					case TriggerType.SetFreeAndLookTowards:
						dir = col.transform.forward;
						if (cameraData.changeAltitude)
							_HedgeCam.SetCamera(dir, 2.5f, cameraData.CameraAltitude, cameraData.FaceSpeed);
						else
							_HedgeCam.SetCameraNoHeight(dir, 2.5f, cameraData.FaceSpeed);
						if (!cameraData.changeDistance)
						{
							_HedgeCam._cameraMaxDistance_ = _initialDistance;
						}
						else
						{
							_HedgeCam._cameraMaxDistance_ = cameraData.ChangeDistance;
						}
						_HedgeCam._isMasterLocked = false;
						_HedgeCam._isLocked = false;
						break;

					case TriggerType.Reverse:
						dir = -GetComponent<S_CharacterTools>().CharacterAnimator.transform.forward;
						_HedgeCam._isReversed = true;
						if (cameraData.changeAltitude)
							_HedgeCam.SetCamera(dir, 2.5f, cameraData.CameraAltitude, cameraData.FaceSpeed);
						else
							_HedgeCam.SetCameraNoHeight(dir, 2.5f, cameraData.FaceSpeed);
						if (!cameraData.changeDistance)
						{
							_HedgeCam._cameraMaxDistance_ = _initialDistance;
						}
						else
						{
							_HedgeCam._cameraMaxDistance_ = cameraData.ChangeDistance;
						}
						_HedgeCam._isMasterLocked = false;
						_HedgeCam._isLocked = false;
						break;

					case TriggerType.ReverseAndLockControl:
						dir = -GetComponent<S_CharacterTools>().CharacterAnimator.transform.forward;
						_HedgeCam._isReversed = true;
						if (cameraData.changeAltitude)
							_HedgeCam.SetCamera(dir, 2.5f, cameraData.CameraAltitude, cameraData.FaceSpeed);
						else
							_HedgeCam.SetCameraNoHeight(dir, 2.5f, cameraData.FaceSpeed);
						if (!cameraData.changeDistance)
						{
							_HedgeCam._cameraMaxDistance_ = _initialDistance;
						}
						else
						{
							_HedgeCam._cameraMaxDistance_ = cameraData.ChangeDistance;
						}
						_HedgeCam._canMove = false;
						break;
				}


			}
		}

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

					Vector3 dir = col.transform.forward;
					_HedgeCam.SetCamera(dir, 2.5f, cameraData.CameraAltitude);
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
