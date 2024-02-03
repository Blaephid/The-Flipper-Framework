using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{

    public GameObject player;

    // Start is called before the first frame update
    public Transform getPlayer()
    {
        return(player.transform);
    }
}
