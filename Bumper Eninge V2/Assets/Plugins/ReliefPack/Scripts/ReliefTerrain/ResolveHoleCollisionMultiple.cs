using UnityEngine;
using System.Collections;

//
// put on the object with collider attached that need to pass thru terrain hole (for example your character)
//

[AddComponentMenu("Relief Terrain/Helpers/Resolve Hole Collision for multiple child colliders")]
public class ResolveHoleCollisionMultiple : MonoBehaviour {
    public Collider[] childColliders;
	public Collider[] entranceTriggers;
	public TerrainCollider[] terrainColliders;
	public float checkOffset=1.0f;
	public bool StartBelowGroundSurface=false;
	private TerrainCollider terrainColliderForUpdate;
	
	private Rigidbody _rigidbody;
	
	//
	// beware that using character controller that has rigidbody attached causes FAIL when we start under terrain surface (in cavern) - your player will be exploded over terrain by some penalty impulse
	// I'll try to find some workaround when such case would seem to be necessary to be resolved
	//
	void Awake() {
		_rigidbody = GetComponent<Rigidbody> ();
		for(int j=0; j<entranceTriggers.Length; j++) {
			if (entranceTriggers[j]!=null) entranceTriggers[j].isTrigger=true;
		}
		if (_rigidbody!=null && StartBelowGroundSurface) {
			for(int i=0; i<terrainColliders.Length; i++) {
				// rigidbodies makes trouble...
				// if we start below terrain surface (inside "a cave") - we need to disable collisions beween our collider and terrain collider
                for(int j=0; j<childColliders.Length; j++) { 
				    if (terrainColliders[i]!=null && childColliders[j]!=null) {
					    Physics.IgnoreCollision(childColliders[j], terrainColliders[i], true);
    				}
                }
			}
		}
	}
	
	void OnTriggerEnter(Collider other) {
        for (int j = 0; j < entranceTriggers.Length; j++)
        {
            if (entranceTriggers[j] == other)
            {
                for (int i = 0; i < terrainColliders.Length; i++)
                {
                    for (int c = 0; c < childColliders.Length; c++)
                    {
                        // we're entering entrance trigger - disable collisions between my collider and terrain
                        if (childColliders[c] != null && terrainColliders[i] != null)
                        {
                            Physics.IgnoreCollision(childColliders[c], terrainColliders[i], true);
                        }
                    }
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
                    for (int c = 0; c < childColliders.Length; c++)
                    {
                        if (childColliders[c] != null && terrainColliders[i] != null)
                        {
                            Physics.IgnoreCollision(childColliders[c], terrainColliders[i], false);
                        }
                    }
				}
			}
			terrainColliderForUpdate=null;
		}
	}
	
	void OnTriggerExit(Collider other) {
		for(int j=0; j<entranceTriggers.Length; j++) {
			if (entranceTriggers[j]==other) {
				
				// we're exiting entrance trigger
				for(int i=0; i<terrainColliders.Length; i++) {
                    // no rigidbody - simply enable collisions
                    for (int c = 0; c < childColliders.Length; c++)
                    {
                        if (childColliders[c] != null && terrainColliders[i] != null)
                        {
                            Physics.IgnoreCollision(childColliders[c], terrainColliders[i], false);
                        }
                    }
				}
			}
		}
	}
}
