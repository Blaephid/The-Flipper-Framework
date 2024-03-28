using UnityEngine;
using System.Collections;

public enum MonitorType
{
    Ring, Shield
}

public class S_Data_Monitor : MonoBehaviour {

    public MonitorType Type;
    public GameObject Bubble;
    public Texture MonitorFace;
    public int RingAmount;
    

    public GameObject MonitorExplosion;

    public void DestroyMonitor()
    {
        if (Bubble != null)
        {
            Destroy(Bubble);
        }

        GameObject.Instantiate(MonitorExplosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
