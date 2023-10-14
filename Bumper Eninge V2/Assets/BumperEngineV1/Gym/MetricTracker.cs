using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetricTracker : MonoBehaviour
{
    // Start is called before the first frame update

    public bool sayStart;

    public bool trackSpeed;
    public bool trackTime;
    public bool trackDistance;
    public bool trackAccel;
    public bool trackJumpsInstead;
    public bool atEnd;

    public MetricTracker startPoint;

    float Speed;
    [HideInInspector] public bool startTrack;
    float Seconds;
    [HideInInspector] public Vector3 thisPos;
    bool followingAcc;
    bool followingJump;

    PlayerBhysics player;
    Action01_Jump jumpAction;



    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            player = other.GetComponentInParent<PlayerBhysics>();
            if(trackJumpsInstead)
            {
                jumpAction = other.GetComponentInParent<Action01_Jump>();
            }

            if (startPoint.startTrack && atEnd)
            {
                startPoint.startTrack = false;
                {
                    if(trackSpeed)
                    {
                        Debug.Log("Speed at start was = " + startPoint.Speed);
                        Debug.Log("Speed at end was = " + player.HorizontalSpeedMagnitude);
                    }

                    if(trackDistance)
                        Debug.Log("Distance between = " +Vector3.Distance(startPoint.thisPos, other.transform.position));

                    if(trackTime && !trackAccel)
                        Debug.Log("Time since = " + (startPoint.Seconds + Time.fixedDeltaTime));
                }
            }

            else
            {
                startTrack = true;

                if (sayStart)
                    Debug.Log("Entered");

                thisPos = other.transform.position;

                Speed = player.HorizontalSpeedMagnitude;

                Seconds = 0;
                followingAcc = false;

                followingJump = false;
            }



        }
    }

    private void FixedUpdate()
    {
        if(startTrack)
        {

            if (trackAccel)
            {
                if (player.HorizontalSpeedMagnitude < 0.5 && !followingAcc)
                { 
                    Debug.Log("Can track accel");
                    followingAcc = true;
                    thisPos = player.transform.position;
                }

                if (player.HorizontalSpeedMagnitude > 0.5f && followingAcc)
                    Seconds += Time.fixedDeltaTime;

                if(player.HorizontalSpeedMagnitude >= player.TopSpeed)
                {
                    Debug.Log("Time to Top Speed = " + Seconds);
                    Debug.Log("Distance to Top Speed = " + Vector3.Distance(player.transform.position, thisPos));
                    startTrack = false;
                }


            }
            else if (trackTime)
            {
                Seconds += Time.fixedDeltaTime;
                
            }

            if(trackJumpsInstead)
            {
                if((player.Action.Action != ActionManager.States.Jump && player.Action.Action != ActionManager.States.JumpDash) || jumpAction.Jumping)
                    player.Action.JumpPressed = true;

                if (!followingJump && !player.Grounded)
                {
                    followingJump = true;
                    thisPos = player.transform.position;
                }

                else if (followingJump)
                {
                    if(!player.Grounded)
                    {
                        Seconds += Time.fixedDeltaTime;

                        if(!jumpAction.Jumping)
                        {
                            if(player.rb.velocity.y < 0)
                            {
                                //Debug.Log("Peak Jump At = " + (player.transform.position.y - thisPos.y));

                                if (jumpAction.jumpCount < 2)
                                    player.Action.JumpPressed = true;
                                //else
                                //    player.Action.SpecialPressed = true;
                            }
                            else
                                player.Action.JumpPressed = false;
                        }
                    }

                    else if (player.Grounded)
                    {
                        startTrack = false;
                        Debug.Log("Distance between = " + Vector3.Distance(thisPos, player.transform.position));
                        Debug.Log("Time in air = " + Seconds);
                        Debug.Log("Speed at Start was =" + Speed);

                    }
                }
            }
        }
       
    }
}
