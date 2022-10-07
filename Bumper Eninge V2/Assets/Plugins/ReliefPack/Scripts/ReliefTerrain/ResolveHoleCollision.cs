using UnityEngine;
using System.Collections;

//
// put on the object with collider attached that need to pass thru terrain hole (for example your character)
//

[AddComponentMenu("Relief Terrain/Helpers/Resolve Hole Collision")]
[RequireComponent(typeof(Collider))]
public class ResolveHoleCollision : MonoBehaviour {
	public Collider[] entranceTriggers;
	public TerrainCollider[] terrainColliders;
	public float checkOffset=1.0f;
	public bool StartBelowGroundSurface=false;
	private TerrainCollider terrainColliderForUpdate;
	
	private Collider _collider;
	private Rigidbody _rigidbody;
	
	//
	// beware that using character controller that has rigidbody attached causes FAIL when we start under terrain surface (in cavern) - your player will be exploded over terrain by some penalty impulse
	// I'll try to find some workaround when such case would seem to be necessary to be resolved
	//
	void Awake() {
		_collider = GetComponent<Collider> ();
		_rigidbody = GetComponent<Rigidbody> ();
		for(int j=0; j<entranceTriggers.Length; j++) {
			if (entranceTriggers[j]!=null) entranceTriggers[j].isTrigger=true;
		}
		if (_rigidbody!=null && StartBelowGroundSurface) {
			for(int i=0; i<terrainColliders.Length; i++) {
				// rigidbodies makes trouble...
				// if we start below terrain surface (inside "a cave") - we need to disable collisions beween our collider and terrain collider
				if (terrainColliders[i]!=null && _collider!=null) {
					Physics.IgnoreCollision(_collider, terrainColliders[i], true);
				}
			}
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (_collider==null) return;
		for(int j=0; j<entranceTriggers.Length; j++) {
			if (entranceTriggers[j]==other) {
				for(int i=0; i<terrainColliders.Length; i++) {
					// we're entering entrance trigger - disable collisions between my collider and terrain
					Physics.IgnoreCollision(_collider, terrainColliders[i], true);
				}
			}
		}
	}
	
	void FixedUpdate() {
		if (terrainColliderForUpdate) {
			RaycastHit hit=new RaycastHit();
			if (terrainColliderForUpdate.Raycast (new Ray(transform.position+Vector3.up*checkOffset, Vector3.down), out hit, Mathf.Infinity)) {
				// enable only in the case when my collider seems to be over terrain surface
				for(int i=0; i<terrainColliders.Length; i++) {
					Physics.IgnoreCollision(_collider, terrainColliders[i], false);
				}
			}
			terrainColliderForUpdate=null;
		}
	}
	
	void OnTriggerExit(Collider other) {
		if (_collider==null) return;
		for(int j=0; j<entranceTriggers.Length; j++) {
			if (entranceTriggers[j]==other) {
				
				// we're exiting entrance trigger
				if (true) {//_rigidbody==null) {
					for(int i=0; i<terrainColliders.Length; i++) {
						// no rigidbody - simply enable collisions
						Physics.IgnoreCollision(_collider, terrainColliders[i], false);
					}
//				} else {
//					// rigidbodies makes trouble...
//					TerrainCollider terrainCollider=null;
//					for(int i=0; i<terrainColliders.Length; i++) {
//						if ( (terrainColliders[i].bounds.min.x<=transform.position.x) && (terrainColliders[i].bounds.min.z<=transform.position.z) && (terrainColliders[i].bounds.max.x>=transform.position.x) && (terrainColliders[i].bounds.max.z>=transform.position.z) ) {
//							terrainCollider=terrainColliders[i];
//							break;
//						}
//					}
//					// update collisions at next fixedupdate (here RayCast fails sometimes ?)
//					terrainColliderForUpdate=terrainCollider;
				}
			}
		}
	}
}
