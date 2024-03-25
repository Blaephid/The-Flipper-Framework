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

    GameObject Player;
    S_ActionManager Actions;

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
                Player = other.GetComponentInParent<S_PlayerPhysics>().gameObject;
                Actions = Player.GetComponent<S_ActionManager>();
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
                if ( Actions.whatAction == S_Enums.PrimaryPlayerStates.Path || (Actions.whatAction == S_Enums.PrimaryPlayerStates.Default && RegularAction) || (Actions.whatAction == S_Enums.PrimaryPlayerStates.Jump && JumpAction) 
                    || (Actions.whatAction == S_Enums.PrimaryPlayerStates.Rail && RailAction) || (Actions.whatAction == S_Enums.PrimaryPlayerStates.WallRunning && wallRunAction) || (Actions.whatAction == S_Enums.PrimaryPlayerStates.RingRoad && RingRoadAction))
                {
                    isActive = true;
                    hedgeCam = Player.GetComponent<S_Handler_Camera>()._VirtCam;
                    

                    ActivateCam(5f);

                    if (lookPlayer)
                    {
                        virCam.LookAt = Player.transform;
                    }

                    if (followPlayer)
                    {
                        virCam.Follow = Player.transform;
                    }

                }
            }
            else
            {
                if(!(
                    Actions.whatAction == S_Enums.PrimaryPlayerStates.Default && RegularAction) && !(Actions.whatAction == S_Enums.PrimaryPlayerStates.Jump && JumpAction) && 
                    !(Actions.whatAction == S_Enums.PrimaryPlayerStates.Rail && RailAction) && !(Actions.whatAction == S_Enums.PrimaryPlayerStates.WallRunning && wallRunAction) && !(Actions.whatAction == S_Enums.PrimaryPlayerStates.RingRoad && RingRoadAction) && onExit)
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
            attachedCam.transform.position = Player.GetComponent<S_Handler_Camera>()._HedgeCam.transform.position;
            attachedCam.transform.rotation = Player.GetComponent<S_Handler_Camera>()._HedgeCam.transform.rotation;
        }

        attachedCam.transform.position += startOffset;

        attachedCam.SetActive(true);
        hedgeCam = Player.GetComponent<S_Handler_Camera>()._VirtCam;
        hedgeCam.gameObject.SetActive(false);
        if(disableFor > 0)
            Player.GetComponent<S_PlayerInput>().LockInputForAWhile(disableFor, true, Player.GetComponent<S_PlayerPhysics>()._moveInput);

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
 
        if(setBehind)
        {
            Player.GetComponent<S_Handler_Camera>()._HedgeCam.SetBehind(0);
        }

        isActive = false;
        attachedCam.transform.position = camPosit;
        attachedCam.transform.rotation = camRotit;
        hedgeCam.gameObject.SetActive(true);
        attachedCam.SetActive(false);
        if(disableFor > 0)
            Player.GetComponent<S_PlayerInput>().LockInputForAWhile(disableFor, true, Player.GetComponent<S_PlayerPhysics>()._moveInput);
    }
}
