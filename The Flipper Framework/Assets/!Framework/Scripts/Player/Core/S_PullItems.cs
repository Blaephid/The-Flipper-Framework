using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PullItems : MonoBehaviour
{
    S_CharacterTools tools;

    Animator CharacterAnimator;
    S_PlayerPhysics player;

    AnimationCurve _RadiusBySpeed_;
    LayerMask _RingMask_;
    float _basePullSpeed_;

    GameObject currentMonitor;

    List<Transform> allRings = new List<Transform>();

    public void Start()
    {
        player = GetComponent<S_PlayerPhysics>();
        tools = GetComponent<S_CharacterTools>();

        CharacterAnimator = tools.CharacterAnimator;
        _RadiusBySpeed_ = tools.Stats.ItemPulling.RadiusBySpeed;
        _RingMask_ = tools.Stats.ItemPulling.RingMask;
        _basePullSpeed_ = tools.Stats.ItemPulling.basePullSpeed;


        
    }

    private void FixedUpdate()
    {
        AddToList();
        pullRings();

    }

    public void AddToList()
    {

        Collider[] rings = Physics.OverlapSphere(transform.position, _RadiusBySpeed_.Evaluate(player.HorizontalSpeedMagnitude / player.MaxSpeed), _RingMask_, QueryTriggerInteraction.Collide);
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
                    r.position = Vector3.MoveTowards(r.position, transform.position, Time.deltaTime * _basePullSpeed_ * player.HorizontalSpeedMagnitude);
                }
            
            }
        }

    }


}
