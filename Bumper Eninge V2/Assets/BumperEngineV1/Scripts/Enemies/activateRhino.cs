using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class activateRhino : MonoBehaviour
{
    public RailEnemyControl Rhino;

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
