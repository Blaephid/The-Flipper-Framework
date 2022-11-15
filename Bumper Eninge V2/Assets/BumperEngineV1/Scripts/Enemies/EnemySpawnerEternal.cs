using UnityEngine;
using System.Collections;
using System;

public class EnemySpawnerEternal : MonoBehaviour {

    public float Distance;
    Transform Player;

    public float RespawnTime;
    float counter;
    public bool respawnOnDeath = true;

    public GameObject Enemy;
    public bool HasSpawned { get; set; }
    public GameObject TeleportSparkle;

    GameObject EnemyClone = null;

    void Start()
    {
        HasSpawned = false;
        Player = GameObject.FindWithTag("Player").transform;
        GetComponent<MeshRenderer>().enabled = false;
        counter = RespawnTime;
    }

    void LateUpdate()
    {
        counter += Time.deltaTime;
        if (Vector3.Distance(Player.position, transform.position) < Distance)
        {
            if (!HasSpawned && EnemyClone == null)
            {
                if (counter > RespawnTime)
                {
                    SpawnInNormal();
                }
            }
        }
    }

    void SpawnInNormal()
    {
        HasSpawned = true;
        Instantiate(TeleportSparkle, transform.position, transform.rotation);
        EnemyClone = (GameObject)Instantiate(Enemy, transform.position, transform.rotation);
        EnemyClone.GetComponent<EnemyHealth>().SpawnReference = this;
        //HomingAttackControl.UpdateHomingTargets();
    }

    public void ResartSpawner()
    {
        HasSpawned = false;
        counter = 0;
    }

    private void OnEnable()
    {

        LevelProgressControl.onReset += ReturnOnDeath;

    }
    private void OnDisable()
    {

        LevelProgressControl.onReset -= ReturnOnDeath;

    }

    void ReturnOnDeath(object sender, EventArgs e)
    {
        //Debug.Log("Player fucking DIED");
        if (respawnOnDeath)
        {
            Destroy(EnemyClone);
            EnemyClone = null;
            HasSpawned = false;
            counter = RespawnTime - 1f;
        }
    }
}
