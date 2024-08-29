using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class S_PullItems : MonoBehaviour
{
    S_CharacterTools _Tools;

    Animator CharacterAnimator;
    S_PlayerPhysics _PlayerPhys;

    AnimationCurve _RadiusBySpeed_;
    LayerMask _RingMask_;
    float _basePullSpeed_;

    GameObject currentMonitor;

    List<Transform> allRings = new List<Transform>();

    public void Start()
    {
        _PlayerPhys = GetComponentInParent<S_PlayerPhysics>();
        _Tools = GetComponentInParent<S_CharacterTools>();

        CharacterAnimator = _Tools.CharacterAnimator;
        _RadiusBySpeed_ = _Tools.Stats.ItemPulling.RadiusBySpeed;
        _RingMask_ = _Tools.Stats.ItemPulling.RingMask;
        _basePullSpeed_ = _Tools.Stats.ItemPulling.basePullSpeed;


        
    }

    private void FixedUpdate()
    {
        AddToList();
        pullRings();

    }

    public void AddToList()
    {

        Collider[] rings = Physics.OverlapSphere(transform.position, _RadiusBySpeed_.Evaluate(_PlayerPhys._horizontalSpeedMagnitude / _PlayerPhys._PlayerMovement._currentMaxSpeed), _RingMask_, QueryTriggerInteraction.Collide);
		for (int i = 0 ; i < rings.Length ; i++)
		{
			Collider r = rings[i];
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
                    r.position = Vector3.MoveTowards(r.position, transform.position, Time.deltaTime * _basePullSpeed_ * _PlayerPhys._horizontalSpeedMagnitude);
                }
            
            }
        }

    }


}
