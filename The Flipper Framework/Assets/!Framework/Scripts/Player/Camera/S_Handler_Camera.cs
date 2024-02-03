using UnityEngine;
using Cinemachine;
using System.Collections;

public class S_Handler_Camera : MonoBehaviour {

    public S_HedgeCamera Cam;
    public CinemachineVirtualCamera virtCam;
    [HideInInspector] public float InitialDistance;

    void Awake()
    {
        InitialDistance = Cam.CameraMaxDistance;
    }


    public void OnTriggerEnter(Collider col)
    {
        if(col.tag == "CameraTrigger")
        {
            if(col.GetComponent<S_Trigger_Camera>() != null)
            {

                switch (col.GetComponent<S_Trigger_Camera>().Type)
                {
                    case TriggerType.LockToDirection:
                        Vector3 dir = col.transform.forward;
                        if (col.GetComponent<S_Trigger_Camera>().changeAltitude)
                            Cam.SetCamera(dir, 2f, col.GetComponent<S_Trigger_Camera>().CameraAltitude, col.GetComponent<S_Trigger_Camera>().FaceSpeed);
                        else
                            Cam.SetCameraNoHeight(dir, 2f, col.GetComponent<S_Trigger_Camera>().FaceSpeed);
                        Cam.MasterLocked = true;
                        Cam.canMove = false;
                        Cam.Locked = true;
                        if (col.GetComponent<S_Trigger_Camera>().changeDistance)
                        {
                            Cam.CameraMaxDistance = col.GetComponent<S_Trigger_Camera>().ChangeDistance;
                        }
                        else
                        {
                            Cam.CameraMaxDistance = InitialDistance;
                        }
                        break;

                    case TriggerType.SetFree:
                        Cam.CameraMaxDistance = InitialDistance;
                        Cam.MasterLocked = false;
                        Cam.Reversed = false;
                        Cam.Locked = false;
                        Cam.canMove = true;
                        break;

                    case TriggerType.justEffect:
                        if (!col.GetComponent<S_Trigger_Camera>().changeDistance)
                        {
                            Cam.CameraMaxDistance = InitialDistance;
                        }
                        else
                        {
                            Cam.CameraMaxDistance = col.GetComponent<S_Trigger_Camera>().ChangeDistance;
                        }
                        if (col.GetComponent<S_Trigger_Camera>().changeAltitude)
                            Cam.SetCameraNoLook(col.GetComponent<S_Trigger_Camera>().CameraAltitude);
  
                        break;


                    case TriggerType.SetFreeAndLookTowards:
                        dir = col.transform.forward;
                        if (col.GetComponent<S_Trigger_Camera>().changeAltitude)
                            Cam.SetCamera(dir, 2.5f, col.GetComponent<S_Trigger_Camera>().CameraAltitude, col.GetComponent<S_Trigger_Camera>().FaceSpeed);
                        else
                            Cam.SetCameraNoHeight(dir, 2.5f, col.GetComponent<S_Trigger_Camera>().FaceSpeed);
                        if (!col.GetComponent<S_Trigger_Camera>().changeDistance)
                        {
                            Cam.CameraMaxDistance = InitialDistance;
                        }
                        else
                        {
                            Cam.CameraMaxDistance = col.GetComponent<S_Trigger_Camera>().ChangeDistance;
                        }
                        Cam.MasterLocked = false;
                        Cam.Locked = false;
                        break;

                    case TriggerType.Reverse:
                        dir = -GetComponent<S_CharacterTools>().CharacterAnimator.transform.forward;
                        Cam.Reversed = true;
                        if (col.GetComponent<S_Trigger_Camera>().changeAltitude)
                            Cam.SetCamera(dir, 2.5f, col.GetComponent<S_Trigger_Camera>().CameraAltitude, col.GetComponent<S_Trigger_Camera>().FaceSpeed);
                        else
                            Cam.SetCameraNoHeight(dir, 2.5f, col.GetComponent<S_Trigger_Camera>().FaceSpeed);
                        if (!col.GetComponent<S_Trigger_Camera>().changeDistance)
                        {
                            Cam.CameraMaxDistance = InitialDistance;
                        }
                        else
                        {
                            Cam.CameraMaxDistance = col.GetComponent<S_Trigger_Camera>().ChangeDistance;
                        }
                        Cam.MasterLocked = false;
                        Cam.Locked = false;
                        break;

                    case TriggerType.ReverseAndLockControl:
                        dir = -GetComponent<S_CharacterTools>().CharacterAnimator.transform.forward;
                        Cam.Reversed = true;
                        if (col.GetComponent<S_Trigger_Camera>().changeAltitude)
                            Cam.SetCamera(dir, 2.5f, col.GetComponent<S_Trigger_Camera>().CameraAltitude, col.GetComponent<S_Trigger_Camera>().FaceSpeed);
                        else
                            Cam.SetCameraNoHeight(dir, 2.5f, col.GetComponent<S_Trigger_Camera>().FaceSpeed);
                        if (!col.GetComponent<S_Trigger_Camera>().changeDistance)
                        {
                            Cam.CameraMaxDistance = InitialDistance;
                        }
                        else
                        {
                            Cam.CameraMaxDistance = col.GetComponent<S_Trigger_Camera>().ChangeDistance;
                        }
                        Cam.canMove = false;
                        break;
                }

                   
            }
        }

    }

	public void OnTriggerExit(Collider col)
	{
		if (col.tag == "CameraTrigger") {
			if (col.GetComponent<S_Trigger_Camera> () != null) {
				if (col.GetComponent<S_Trigger_Camera> ().Type == TriggerType.LockToDirection && col.GetComponent<S_Trigger_Camera> ().ReleaseOnExit) {
					Cam.CameraMaxDistance = InitialDistance;

                    Cam.MasterLocked = false;
                    Cam.Locked = false;
                    Cam.canMove = true;

                    Vector3 dir = col.transform.forward;
					Cam.SetCamera(dir, 2.5f, col.GetComponent<S_Trigger_Camera>().CameraAltitude);
				}

                else if(col.GetComponent<S_Trigger_Camera>().ReleaseOnExit)
                {
                    Cam.CameraMaxDistance = InitialDistance;

                    if(col.GetComponent<S_Trigger_Camera>().Type == TriggerType.Reverse)
                    {
                        Cam.Reversed = false;
                    }
                    else if(col.GetComponent<S_Trigger_Camera>().Type == TriggerType.ReverseAndLockControl)
                    {
                        Cam.Reversed = false;
                        Cam.canMove = true;
                    }
                }
			}
		}
	}


    

}
