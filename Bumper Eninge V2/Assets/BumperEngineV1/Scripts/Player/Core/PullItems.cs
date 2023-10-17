using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullItems : MonoBehaviour
{
    CharacterTools tools;

    Animator CharacterAnimator;
    PlayerBhysics player;

    AnimationCurve radiusBySpeed;
    LayerMask ringMask;
    float basePullSpeed;

    GameObject currentMonitor;

    List<Transform> allRings = new List<Transform>();

    public void Start()
    {
        player = GetComponent<PlayerBhysics>();
        tools = GetComponent<CharacterTools>();

        CharacterAnimator = tools.CharacterAnimator;
        radiusBySpeed = tools.coreStats.radiusBySpeed;
        ringMask = tools.coreStats.ringMask;
        basePullSpeed = tools.coreStats.basePullSpeed;


        
    }

    private void FixedUpdate()
    {
        AddToList();
        pullRings();

    }

    public void AddToList()
    {

        Collider[] rings = Physics.OverlapSphere(transform.position, radiusBySpeed.Evaluate(player.HorizontalSpeedMagnitude / player.MaxSpeed), ringMask, QueryTriggerInteraction.Collide);
        foreach (Collider r in rings)
        {
            allRings.Add(r.transform.parent);
            Destroy(r);
        }
    }

    public void pullRings()
    {
        if(allRings.Count > 0)
        {

            for(int i = 0; i < allRings.Count; i++)
            {
                Transform r = allRings[i];
                if(!r)
                {
                    allRings.RemoveAt(i);
                    i--;
                }
                   
                else
                {
                    r.position = Vector3.MoveTowards(r.position, transform.position, Time.deltaTime * basePullSpeed * player.HorizontalSpeedMagnitude);
                }

            
            }
        }

    }


}
