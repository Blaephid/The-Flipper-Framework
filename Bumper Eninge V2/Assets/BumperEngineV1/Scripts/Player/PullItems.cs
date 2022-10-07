using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullItems : MonoBehaviour
{
    public CapsuleCollider Collider;
    public Transform transform;
    public Transform Player;
    public Animator CharacterAnimator;
    [SerializeField] PlayerBhysics playerphys;
    public float StartSize;

    GameObject currentMonitor;

    public void Start()
    {
        SetSize(0);
        
    }

    private void Update()
    {
        //The parent should automatically assign itself ot the player's location


        if (currentMonitor != null)
        {
            if (CharacterAnimator.GetInteger("Action") == 1)
            {
                currentMonitor.GetComponent<BoxCollider>().enabled = false;
            }

            else
            {
                currentMonitor.GetComponent<BoxCollider>().enabled = true;
            }
        }


    }

    public void SetSize (float f)
    {
        if (f == 0)
        {
            Collider.radius = StartSize;
            Collider.height = StartSize *2;
        }
        else if (f == -1)
        {
            Collider.radius = .1f;
            Collider.height = .1f;
        }
        else if (f != 0)
        {
            Collider.radius = f;
            Collider.height = StartSize * 1.5f;
        }

    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Ring")
        {
            PulledByPlayer ring = col.GetComponent<PulledByPlayer>();
            Destroy(ring.boxcol);

            ring.Player = this.gameObject.transform;
            ring.PulledSpeed = playerphys.SpeedMagnitude / 2;
            ring.Pulled = true;

        }

        if(col.tag == "Monitor")
        {
            currentMonitor = col.gameObject;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject == currentMonitor)
        {
            currentMonitor.GetComponent<BoxCollider>().enabled = true;
            currentMonitor = null;
        }
    }


}
