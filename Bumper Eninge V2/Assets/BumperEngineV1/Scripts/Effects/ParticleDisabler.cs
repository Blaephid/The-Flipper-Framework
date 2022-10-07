using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDisabler : MonoBehaviour
{
    public float Lifetime = 0.8f;
    float t;
    private void OnEnable()
    {
        t = Lifetime;
    }

    // Update is called once per frame
    void Update()
    {
        t -= Time.deltaTime;
        if (t <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
