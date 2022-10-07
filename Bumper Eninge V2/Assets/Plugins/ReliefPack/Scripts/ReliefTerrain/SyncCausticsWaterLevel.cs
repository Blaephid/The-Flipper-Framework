using UnityEngine;
using System.Collections;

//
// put this script on the reference object that will define water level for caustics on terrain
// this object should be also defined in ReliefTerrain script inspector (Settings/Caustics)
//
// water level is adjusted globaly OR per material if you specify gameObject (then water level shader property will be set in renderer.sharedMaterial of the object)
//

[AddComponentMenu("Relief Terrain/Helpers/Sync Caustics Water Level")]
[ExecuteInEditMode]
public class SyncCausticsWaterLevel : MonoBehaviour {
	public GameObject refGameObject;
	public float yOffset;
	
	void Update () {
		if (refGameObject && refGameObject.GetComponent<Renderer>()) {
			refGameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("TERRAIN_CausticsWaterLevel", transform.position.y+yOffset);
		} else {
			Shader.SetGlobalFloat("TERRAIN_CausticsWaterLevel", transform.position.y+yOffset);
		}
	}
}
