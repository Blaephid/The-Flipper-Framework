using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinnerActor : MonoBehaviour
{
    Transform Player;
    public enum SpinnerType { Normal, Electric }
    public SpinnerType spinnerType;
    public float LookSmoothing;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        //Face the player
        Vector3 LookDirection = Vector3.Normalize(Player.position - transform.position);
        LookDirection.y = 0;
        Quaternion LookRot = Quaternion.LookRotation(LookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, LookRot, Time.deltaTime * LookSmoothing);
    }
}
