using UnityEngine;
using System.Collections;

public class S_AI_GunBeetleControl : MonoBehaviour {

    Transform Player;

    void Start () {

        Player = GameObject.FindWithTag("Player").transform;

    }
	
	void Update () {

        var lookPos = Player.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 4);

    }
}
