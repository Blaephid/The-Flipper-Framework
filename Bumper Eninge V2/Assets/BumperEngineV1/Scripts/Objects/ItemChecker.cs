using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemChecker : MonoBehaviour
{
    public Transform Player;
    public Objects_Interaction obj_Int;
    public Pathers_Interaction path_Int;

    private void Update()
    {
        transform.position = Player.position;
        transform.rotation = Player.rotation;
    }
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);

        if (other.gameObject.CompareTag("Rail"))
        {
            //path_Int.AttachToRail(other);

        }
    }

}
