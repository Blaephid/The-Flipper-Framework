using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class S_PullItems : MonoBehaviour
{
	S_CharacterTools _Tools;

	Animator CharacterAnimator;
	S_PlayerPhysics _PlayerPhys;
	S_PlayerVelocity _PlayerVel;

	AnimationCurve _RadiusBySpeed_;
	LayerMask _RingMask_;
	float _basePullSpeed_;

	GameObject currentMonitor;

	List<Transform> _allRings = new List<Transform>();

	public void Start () {
		//Tools
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_PlayerVel =	_Tools.GetComponent<S_PlayerVelocity>();	
		CharacterAnimator = _Tools.CharacterAnimator;

		//Stats
		_RadiusBySpeed_ = _Tools.Stats.ItemPulling.RadiusBySpeed;
		_RingMask_ = _Tools.Stats.ItemPulling.RingMask;
		_basePullSpeed_ = _Tools.Stats.ItemPulling.basePullSpeed;
	}

	private void FixedUpdate () {
		SearchForRingsNearby();
	}

	private void Update () {
		PullSavedRingsIn();
	}

	//Generates a sh=phere with size based on current speed, and any ring caught in it is added to be pulled towards the character for the next frames.
	public void SearchForRingsNearby () {

		Collider[] rings = Physics.OverlapSphere(transform.position, _RadiusBySpeed_.Evaluate(_PlayerVel._horizontalSpeedMagnitude / _PlayerPhys._PlayerMovement._currentMaxSpeed), _RingMask_, QueryTriggerInteraction.Collide);
		for (int i = 0 ; i < rings.Length ; i++)
		{
			Collider ring = rings[i];
			_allRings.Add(ring.transform.parent);
			ring.transform.parent = null; //This is to ensure they will remain being pulled even if their parents become inactive.
			Destroy(ring);
		}
	}

	//Go through each ring that was added to be pulled, and change their position to be closer.
	public void PullSavedRingsIn () {
		if (_allRings.Count > 0)
		{
			for (int i = 0 ; i < _allRings.Count ; i++)
			{
				Transform ring = _allRings[i];
				//If ring was picked up, this would be deleted.
				if (!ring)
				{
					_allRings.RemoveAt(i);
					i--;
				}

				else
				{
					ring.position = Vector3.MoveTowards(ring.position, _PlayerPhys._CharacterCenterPosition + (_PlayerVel._totalVelocity * Time.fixedDeltaTime), Time.deltaTime * _basePullSpeed_ * _PlayerVel._horizontalSpeedMagnitude);
				}
			}
		}
	}
}
