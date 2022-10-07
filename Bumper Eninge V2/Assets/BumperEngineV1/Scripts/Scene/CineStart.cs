using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CineStart : MonoBehaviour
{
    public bool Active = true;
    public GameObject camera;
    private Vector3 camPosit;
    private Quaternion camRotit;

    public bool lookPlayer;
    public bool followPlayer;

    public bool disableMove;

    public CinemachineVirtualCamera virCam;
    CinemachineVirtualCamera hedgeCam;

    Transform Player;

    public bool enabled = true;
    public bool onExit = true;

    // Start is called before the first frame update

    void Awake()
    {
        camPosit = camera.transform.position;
        camRotit = camera.transform.rotation;
        camera.SetActive(false);
    }

  

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player" && enabled)
        {

            Player = col.GetComponent<PlayerCollider>().getPlayer();

            if (hedgeCam == null)
            {
                hedgeCam = Player.GetComponent<CameraControl>().virtCam;
            }

            ActivateCam();

            if (lookPlayer)
            {
                virCam.LookAt = Player;
            }

            if (followPlayer)
            {
                virCam.Follow = Player;
            }

            if (disableMove)
            {
                Player.GetComponent<ActionManager>().actionDisable();
            }

            
            
        }
        
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player" && onExit)
        {
            DeactivateCam();

            if (disableMove)
            {
                Player.GetComponent<ActionManager>().actionEnable();
            }
        }
    }

    public void ActivateCam()
    {
        if (Active)
        {
            hedgeCam.gameObject.SetActive(false);
            camera.SetActive(true);
            Player.GetComponent<PlayerBinput>().LockInputForAWhile(20f, true);
        }
        
    }

    public void DeactivateCam()
    {
        camera.transform.position = camPosit;
        camera.transform.rotation = camRotit;
        camera.SetActive(false);
        hedgeCam.gameObject.SetActive(true);
        Player.GetComponent<PlayerBinput>().LockInputForAWhile(20f, true);
    }
}
