using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CineStart : MonoBehaviour
{
    public bool RegularAction = true;
    public bool JumpAction = false;
    public bool RailAction = false;
    public bool wallRunAction = false;

    public bool Active = true;
    public GameObject attachedCam;
    private Vector3 camPosit;
    private Quaternion camRotit;

    public bool lookPlayer;
    public bool followPlayer;

    public bool disableMove;

    public CinemachineVirtualCamera virCam;
    CinemachineVirtualCamera hedgeCam;

    GameObject Player;
    ActionManager Actions;

    public bool isEnabled = true;
    public bool onExit = true;
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

            Player = other.GetComponent<PlayerCollider>().player;
            Actions = Player.GetComponent<ActionManager>();
        }
    }
    private void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player" && isEnabled)
        {
            if(!isActive && Player != null)
            {
                if ((Actions.Action == 0 && RegularAction) || (Actions.Action == 1 && JumpAction) || (Actions.Action == 5 && RailAction) || (Actions.Action == 12 && wallRunAction))
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
                if(!(Actions.Action == 0 && RegularAction) && !(Actions.Action == 1 && JumpAction) && !(Actions.Action == 5 && RailAction) && !(Actions.Action == 12 && wallRunAction))
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
      
        attachedCam.SetActive(true);
        hedgeCam.gameObject.SetActive(false);
        Player.GetComponent<PlayerBinput>().LockInputForAWhile(disableFor, true);
            
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
        Player.GetComponent<PlayerBinput>().LockInputForAWhile(disableFor, true);
    }
}
