using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingAttractor : MonoBehaviour
{
    PlayerBhysics Player;
    public float AttractionRadius; //Maximum distance for rings to be attracted
    public float AttractionSpeed; //How fast the rings are drawn to Sonic
    public AnimationCurve SpeedOverVelocity; //Adjusts the attraction speed over Sonic's speed
    List<GameObject> closeRings = new List<GameObject>();

    private void Start()
    {
        Player = GetComponent<PlayerBhysics>();
    }
    // Update is called once per frame
    void Update()
    {
        GetClosestObjects();

        float SpeedMod = SpeedOverVelocity.Evaluate(Player.SpeedMagnitude / Player.TopSpeed);
        for (int i = 0; i < closeRings.Count; i++)
        {
            if (closeRings[i].gameObject.activeSelf)
            {
                closeRings[i].transform.position = Vector3.MoveTowards(closeRings[i].transform.position, transform.position, AttractionSpeed * SpeedMod);
            } else
            {
                closeRings.RemoveAt(i);
            }
        }
    }

    void GetClosestObjects()
    {
        GameObject[] Rings = GameObject.FindGameObjectsWithTag("Ring");
        GameObject tMin = null;
        float Distance = Mathf.Pow(AttractionRadius, 2);
        foreach (GameObject r in Rings)
        {
            if (!r.gameObject.activeSelf)
                return;
            float currentDistance = (transform.position - r.transform.position).sqrMagnitude;
            if (currentDistance <= Distance)
            {
                tMin = r;
                if (!closeRings.Contains(tMin))
                    closeRings.Add(tMin);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, AttractionRadius);
    }
}
