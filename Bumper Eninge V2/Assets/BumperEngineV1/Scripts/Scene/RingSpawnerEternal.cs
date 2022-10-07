using UnityEngine;
using System.Collections;

public class RingSpawnerEternal : MonoBehaviour {

    public float Distance;
    Transform Player;

    public float RespawnTime;
  

    public GameObject Ring;
	private GameObject RingClone = null;
    public bool HasSpawned { get; set; }
    public GameObject TeleportSparkle;

    bool firstTime = true;

    void Start()
    {
        HasSpawned = false;
        Player = GameObject.FindWithTag("Player").transform;
        GetComponent<MeshRenderer>().enabled = false;
    }

    void LateUpdate()
    {
        if (Vector3.Distance(Player.position, transform.position) < Distance)
        {
            if (!HasSpawned && firstTime)
            {
                firstTime = false;
                HasSpawned = true;
                StartCoroutine(SpawnInNormal(.5f));
            }

			else if (!HasSpawned && RingClone == null) 
			{
				HasSpawned = true;
				//Debug.Log ("ShouldSpawn");
				StartCoroutine(SpawnInNormal(RespawnTime));
			}

            
        }
    }

    private IEnumerator SpawnInNormal(float RespawnIn)
    {
		//Debug.Log ("SpawnRing");
		yield return new WaitForSeconds (RespawnIn);
		HasSpawned = false;
        Instantiate(TeleportSparkle, transform.position, transform.rotation);
		RingClone = (GameObject)Instantiate(Ring, transform.position, transform.rotation);
		GameObject.FindObjectOfType<LightDashControl>().UpdateHomingTargets();
    }
		
}
