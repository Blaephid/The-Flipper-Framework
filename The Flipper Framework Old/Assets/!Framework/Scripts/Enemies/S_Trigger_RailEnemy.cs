using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class S_Trigger_RailEnemy : MonoBehaviour
{
    public S_AI_RailEnemy Rhino;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && Rhino.isActiveAndEnabled)
        {

            Rhino.InitialEvents();
            if (other.GetComponentInParent<S_Action05_Rail>()) 
                Rhino.playerRail = other.GetComponentInParent<S_Action05_Rail>();
        }
    }
}
