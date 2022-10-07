using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attackCol : MonoBehaviour
{
    public Monitors_Interactions mon_Int;

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Monitor"))
        {
            mon_Int.TriggerMonitor(col, true);
        }
    }
}
