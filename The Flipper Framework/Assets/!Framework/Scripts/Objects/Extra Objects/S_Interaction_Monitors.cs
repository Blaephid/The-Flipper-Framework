using UnityEngine;
using System.Collections;
using UnityEditor;

public class S_Interaction_Monitors : MonoBehaviour {

    S_CharacterTools tools;
    S_Interaction_Objects Objects;
    S_PlayerPhysics Player;
    S_ActionManager Actions;
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
        tools = GetComponent<S_CharacterTools>();
        Objects = GetComponent<S_Interaction_Objects>();
        Player = basePlayer.GetComponent<S_PlayerPhysics>();
        Actions = basePlayer.GetComponent<S_ActionManager>();

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
        if (col.tag == "Monitor" && col.GetComponent<S_Data_Monitor>() != null)
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
            if (col.GetComponent<S_Data_Monitor>().Type == MonitorType.Ring)
            {
                if (Once == -1)
                {
                    GameObject clone = (GameObject)Instantiate(RingGiver, transform.position, transform.rotation);
                    clone.GetComponent<S_RingGiverControl>().Rings = col.GetComponent<S_Data_Monitor>().RingAmount;
                    col.GetComponent<S_Data_Monitor>().DestroyMonitor();
                    updateTgts = true;
                }
            }
            else if (col.GetComponent<S_Data_Monitor>().Type == MonitorType.Shield)
            {
                if (Once == -1)
                {
                    GameObject clone = (GameObject)Instantiate(ShieldGiver, transform.position, transform.rotation);
                    col.GetComponent<S_Data_Monitor>().DestroyMonitor();
                    updateTgts = true;
                }
            }
        }
    }

}
