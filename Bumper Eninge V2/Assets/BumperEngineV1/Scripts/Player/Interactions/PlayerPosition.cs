using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPosition : MonoBehaviour
{
    public Transform player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Assigns Position, this has to be done seperately, because if the colliders were children of the player, they could not interact seperately.
        transform.position = player.position;
        transform.rotation = player.rotation;
    }
}
