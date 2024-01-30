using UnityEngine;
using System.Collections;

public class ShieldGiverControl : MonoBehaviour {

    public float Duration = 1;
    float counter;
    bool hasadded = false;

    void Update()
    {

        counter += Time.deltaTime;
        if (counter > Duration)
        {
            if (!hasadded)
            {
                hasadded = true;
                S_Interaction_Monitors.HasShield = true;
                GetComponent<AudioSource>().Play();
            }
        }
        if (counter > Duration + 2f)
        {
            Destroy(gameObject);
        }

    }

}
