using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class RailHiding : MonoBehaviour {

    public float Distance = 2000;
    SplineMeshTiling[] Rail;
    Transform Player;
    float Distancetrckr;
    bool active = true;


    // Use this for initialization
    void Start () {
        Distance *= Distance;
        //Player = GameObject.FindWithTag("Player").transform;
        Rail = GetComponentsInChildren<SplineMeshTiling>();
    }
	
	// Update is called once per frame
	void Update () {
        Distancetrckr = (PlayerBhysics.MasterPlayer.playerPos - transform.position).sqrMagnitude;

        if (!active && Distancetrckr < Distance)
        {
            active = true;
            Toogle(true);
        }
        if (active && (Distancetrckr > Distance + 3))
        {
            active = false;
            Toogle(false);
        }


    }

    private void Toogle(bool activate)
    {
        active = activate;
        for(int s = 0; s < Rail.Length; s++)
            {
                Rail[s].gameObject.SetActive(activate);
            }
    }


 

}
