using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CineStart : MonoBehaviour
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

    GameObject Player;
    ActionManager Actions;

    [Header("On Cancel")]
    public bool setBehind = true;
    public float lockTime = 5f;

    bool isActive = false;

    // Start is called before the first frame update

    void Awake()
    {
        camPosit = attachedCam.transform.position;
        camRotit = attachedCam.transform.rotation;
        attachedCam.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && isEnabled)
        {
            if (!endCine)
            {
                Player = other.GetComponentInParent<PlayerBhysics>().gameObject;
                Actions = Player.GetComponent<ActionManager>();
            }
            else
                DeactivateCam(lockTime);
        }
    }
    private void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player" && isEnabled && !endCine)
        {
            if(!isActive && Player != null)
            {
                if ( Actions.Action == ActionManager.States.Path || (Actions.Action == ActionManager.States.Regular && RegularAction) || (Actions.Action == ActionManager.States.Jump && JumpAction) 
                    || (Actions.Action == ActionManager.States.Rail && RailAction) || (Actions.Action == ActionManager.States.WallRunning && wallRunAction) || (Actions.Action == ActionManager.States.RingRoad && RingRoadAction))
                {
                    isActive = true;
                    hedgeCam = Player.GetComponent<CameraControl>().virtCam;
                    

                    ActivateCam(5f);

                    if (lookPlayer)
                    {
                        virCam.LookAt = Player.transform;
                    }

                    if (followPlayer)
                    {
                        virCam.Follow = Player.transform;
                    }

                    if (disableMove)
                    {
                        Player.GetComponent<ActionManager>().actionDisable();
                    }

                }
            }
            else
            {
                if(!(
                    Actions.Action == ActionManager.States.Regular && RegularAction) && !(Actions.Action == ActionManager.States.Jump && JumpAction) && 
                    !(Actions.Action == ActionManager.States.Rail && RailAction) && !(Actions.Action == ActionManager.States.WallRunning && wallRunAction) && !(Actions.Action == ActionManager.States.RingRoad && RingRoadAction) && onExit)
                {
                    DeactivateCam(0);
                }
            }

        }
    }

    void OnTriggerExit(Collider col)
    {
        if(isActive)
        {
            if (col.tag == "Player" && onExit)
            {
                DeactivateCam(lockTime);

            }
        }
    }

    public void ActivateCam(float disableFor)
    {
      
        if(startAtCameraPoint)
        {
            attachedCam.transform.position = Player.GetComponent<CameraControl>().Cam.transform.position;
            attachedCam.transform.rotation = Player.GetComponent<CameraControl>().Cam.transform.rotation;
        }

        attachedCam.transform.position += startOffset;

        attachedCam.SetActive(true);
        hedgeCam = Player.GetComponent<CameraControl>().virtCam;
        hedgeCam.gameObject.SetActive(false);
        if(disableFor > 0)
            Player.GetComponent<PlayerBinput>().LockInputForAWhile(disableFor, true);

        if(timeDelay != 0)
        {
            StartCoroutine(TimeLimit());
        }
            
    }

    IEnumerator TimeLimit()
    {
        isEnabled = false;
        yield return new WaitForSeconds(timeDelay);
        DeactivateCam(lockTime);
        isEnabled = true;
    }

    public void DeactivateCam(float disableFor)
    {
        if (disableMove)
        {
            Player.GetComponent<ActionManager>().actionEnable();
        }
        if(setBehind)
        {
            Player.GetComponent<CameraControl>().Cam.setBehind();
        }

        isActive = false;
        attachedCam.transform.position = camPosit;
        attachedCam.transform.rotation = camRotit;
        hedgeCam.gameObject.SetActive(true);
        attachedCam.SetActive(false);
        if(disableFor > 0)
            Player.GetComponent<PlayerBinput>().LockInputForAWhile(disableFor, true);
    }
}
