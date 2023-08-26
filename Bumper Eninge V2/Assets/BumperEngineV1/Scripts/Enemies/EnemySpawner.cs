using UnityEngine;
using System;
using System.Collections;

public class EnemySpawner : MonoBehaviour {

    public float Distance;
    Transform Player;
    public GameObject TeleportSparkle;

    public GameObject Enemy;

    private GameObject EnemyClone = null;
    public bool respawnOnDeath;

    bool hasSpawned = false;

    void Start()
    {
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
        if(!hasSpawned)
        {
            if (Vector3.Distance(Player.position, transform.position) < Distance)
            {
                if (EnemyClone == null)
                {
                    hasSpawned = true;
                    SpawnInNormal();
                }
            }
        }
        
    }

    void SpawnInNormal()
    {
        Instantiate(TeleportSparkle, transform.position, transform.rotation);
        EnemyClone = (GameObject)Instantiate(Enemy, transform.position, transform.rotation);


        //HomingAttackControl.UpdateHomingTargets();
        //Destroy(gameObject);
    }


    void ReturnOnDeath(object sender, EventArgs e)
    {
        //Debug.Log("Player fucking DIED");
        if (respawnOnDeath)
        {
            hasSpawned = false;
            Destroy(EnemyClone);
            EnemyClone = null;
        }
    }
}
