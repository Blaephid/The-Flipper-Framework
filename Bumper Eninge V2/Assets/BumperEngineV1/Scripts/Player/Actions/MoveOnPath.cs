using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class MoveOnPath : MonoBehaviour
{

    public GameObject path;
    public float minSpeed = 20f;
    public float needSpeed = 60f;
    private float Speed;
    public bool onPath = false;
    private float distanceTraveled;

    ActionManager Actions;
    PlayerBhysics player;

    // Start is called before the first frame update
    void Start()
    {
        Actions = GetComponent<ActionManager>();
        player = GetComponent<PlayerBhysics>();
    }

    // Update is called once per frame
    void Update()
    {
        if (onPath && path != null)
        {
            if (Speed < needSpeed)
                Speed += 1f;

            distanceTraveled = 4f * Time.deltaTime;

            transform.position = path.GetComponent<PathCreator>().path.GetPointAtDistance(distanceTraveled);
        }
    }


}
