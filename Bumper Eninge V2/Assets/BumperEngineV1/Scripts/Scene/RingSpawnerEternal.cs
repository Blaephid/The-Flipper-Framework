using UnityEngine;
using System.Collections;
using System;

public class RingSpawnerEternal : MonoBehaviour {

    public float Distance;
    Transform Player;

    public bool autoRespawn = true;
    public float RespawnTime;

    public bool respawnOnDeath;
  

    public GameObject Ring;
	private GameObject RingClone = null;
    public bool HasSpawned { get; set; }
    public GameObject TeleportSparkle;

    bool firstTime = true;

    void Start()
    {
        HasSpawned = false;
        Player = GameObject.FindWithTag("Player").transform;
        //GetComponent<MeshRenderer>().enabled = false;

    }

    private void OnEnable()
    {
        
         LevelProgressControl.onReset += ReturnOnDeath;
        
    }
    private void OnDisable()
    {
        
        LevelProgressControl.onReset -= ReturnOnDeath;
        
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
                if(autoRespawn)
				    StartCoroutine(SpawnInNormal(RespawnTime));
			}
            
        }
    }

    void ReturnOnDeath(object sender, EventArgs e)
    {
        //Debug.Log("Player fucking DIED");
        if(respawnOnDeath)
        {
            if (RingClone == null)
            {
                StopCoroutine(SpawnInNormal(0f));

                firstTime = true;
                HasSpawned = false;
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
