using UnityEngine;
using System.Collections;
using UnityEditor;

public class Monitors_Interactions : MonoBehaviour {

    CharacterTools tools;
    Objects_Interaction Objects;
    PlayerBhysics Player;
    ActionManager Actions;
    public GameObject basePlayer;

    public GameObject RingGiver;
    public GameObject ShieldGiver;
    int Once = 0;

    public GameObject ShieldObject;
    public Material NormalShieldMaterial;
    public Vector3 ShieldOffset;
    public static bool HasShield = false;
    bool updateTgts;

    bool firstTime = false;

    void Start () {
        tools = GetComponent<CharacterTools>();
        Objects = GetComponent<Objects_Interaction>();
        Player = basePlayer.GetComponent<PlayerBhysics>();
        Actions = basePlayer.GetComponent<ActionManager>();

    }
    void FixedUpdate()
    {
        Once = 0;
    }

    void Update()
    {
        if (!firstTime)
        {
            ShieldObject.SetActive(false);
            firstTime = true;
        }

        if (HasShield)
        {
            ShieldObject.SetActive(true);
           // ShieldObject.transform.position = transform.position + ShieldOffset;
            ShieldObject.transform.rotation = transform.rotation;
        }
        else
        {
            if (ShieldObject)
            {
                ShieldObject.SetActive(false);
            }
        }

        if (updateTgts)
        {
            //HomingAttackControl.UpdateHomingTargets();
            updateTgts = false;
        }

        NormalShieldMaterial.SetTextureOffset("_MainTex", new Vector2(0, -Time.time) * 3);

    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Monitor" && col.GetComponent<MonitorData>() != null)
        {
            if (tools.CharacterAnimator.GetInteger("Action") == 1)
            {
                //Debug.Log("Monitor");
                TriggerMonitor(col, true);
            }
        }


    }

    public void TriggerMonitor(Collider col, bool fromAttack = false)
    {
        Once -= 1;
        if (fromAttack)
        {

            //Monitors data
            if (col.GetComponent<MonitorData>().Type == MonitorType.Ring)
            {
                if (Once == -1)
                {
                    GameObject clone = (GameObject)Instantiate(RingGiver, transform.position, transform.rotation);
                    clone.GetComponent<RingGiverControl>().Rings = col.GetComponent<MonitorData>().RingAmount;
                    col.GetComponent<MonitorData>().DestroyMonitor();
                    updateTgts = true;
                }
            }
            else if (col.GetComponent<MonitorData>().Type == MonitorType.Shield)
            {
                if (Once == -1)
                {
                    GameObject clone = (GameObject)Instantiate(ShieldGiver, transform.position, transform.rotation);
                    col.GetComponent<MonitorData>().DestroyMonitor();
                    updateTgts = true;
                }
            }
        }
    }

}
