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

	List<Rigidbody> _itemsWithRB = new List<Rigidbody>();
	List<Transform> _itemsWithout = new List<Transform>();

	public void Start () {
		//Tools
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_PlayerVel = _Tools.GetComponent<S_PlayerVelocity>();
		CharacterAnimator = _Tools.CharacterAnimator;

		//Stats
		_RadiusBySpeed_ = _Tools.Stats.ItemPulling.RadiusBySpeed;
		_RingMask_ = _Tools.Stats.ItemPulling.RingMask;
		_basePullSpeed_ = _Tools.Stats.ItemPulling.basePullSpeed;
	}

	private void FixedUpdate () {
		SearchForRingsNearby();
		PullSavedObjectsByRB();	
	}

	private void Update () {
		PullSavedObjectsByTransform();
	}

	//Generates a sh=phere with size based on current speed, and any ring caught in it is added to be pulled towards the character for the next frames.
	public void SearchForRingsNearby () {

		Collider[] rings = Physics.OverlapSphere(transform.position, _RadiusBySpeed_.Evaluate(_PlayerVel._horizontalSpeedMagnitude / _PlayerPhys._PlayerMovement._currentMaxSpeed), _RingMask_, QueryTriggerInteraction.Collide);
		for (int i = 0 ; i < rings.Length ; i++)
		{
			//The Pullcol should be a child of the object, not its main collider, so get the parent.
			Transform ring = rings[i].transform.parent;

			if (ring.TryGetComponent(out Rigidbody rb))
			{
				if (!_itemsWithRB.Contains(rb))
				{
					AddToList();
					_itemsWithRB.Add(rb);
				}
			}
			else
			{
				if (!_itemsWithout.Contains(ring.transform))
				{
					AddToList();
					_itemsWithout.Add(ring.transform);
				}
			}
			continue;

			void AddToList () {
				ring.parent = null;//This is to ensure they will remain being pulled even if their parents become inactive.
				Destroy(rings[i]);
			}
		}
	}

	private void PullSavedObjectsByRB () {
		float pullSpeed = _basePullSpeed_ * _PlayerVel._horizontalSpeedMagnitude;
		Vector3 destination = _PlayerPhys._CharacterCenterPosition + (_PlayerVel._totalVelocity * Time.fixedDeltaTime);

		for (int i = 0 ; i < _itemsWithRB.Count ; i++)
		{
			Rigidbody ring = _itemsWithRB[i];
			if (!ring)
			{
				_itemsWithRB.RemoveAt(i);
				i--;
				continue;
			}

			ring.velocity = (S_S_MoreMaths.GetDirection(ring.transform.position, destination)) * pullSpeed;
		}
	}

	//Go through each ring that was added to be pulled, and change their position to be closer.
	private void PullSavedObjectsByTransform () {
		float pullSpeed = _basePullSpeed_ * _PlayerVel._horizontalSpeedMagnitude;
		Vector3 destination = _PlayerPhys._CharacterCenterPosition + (_PlayerVel._totalVelocity * Time.fixedDeltaTime);


		for (int i = 0 ; i < _itemsWithout.Count ; i++)
		{
			Transform ring = _itemsWithout[i];
			//If ring was picked up.
			if (!ring)
			{
				_itemsWithout.RemoveAt(i);
				i--;
				continue;
			}

			//Move ring closer, and use direction to apply an offset to children
			Vector3 prevPosition = ring.position;
			ring.position = Vector3.MoveTowards(ring.position, destination, Time.deltaTime * pullSpeed);
			Vector3 directionItMoved = S_S_MoreMaths.GetDirection(prevPosition, ring.position);
			Vector3 childrenOffset = ring.position - directionItMoved * pullSpeed * Time.fixedDeltaTime;

			//The ring visuals will appear behind the collider, otherwise they look like they're collected a little late.
			for (int child = 0 ; child < ring.childCount ; child++)
			{
				ring.GetChild(child).position = childrenOffset;
			}

		}

	}
}
