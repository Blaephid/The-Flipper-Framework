using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;

[ExecuteAlways]
public class FastSky_Sun_Color : MonoBehaviour
{
    Light _light;
    [SerializeField] private float sunIntensity = 5;
    [SerializeField] private Color dayColour;
    [SerializeField] private Color eveningColour;

    //[SerializeField] private Camera reflectionCamera;
    //[SerializeField] private Cubemap cubeMap;


    void Update()
    {
        if(_light == null)
        {
            _light = GetComponent<Light>();
        }

        float dotProduct = Vector3.Dot(-transform.forward, Vector3.up);
        float clampedDot = Mathf.Clamp((dotProduct + 0.9f), 0, 1);
        float topDot = (1 - Mathf.Clamp01(dotProduct)) * Mathf.Clamp01(Mathf.Sign(dotProduct));
        float bottomDot = (1 - Mathf.Clamp01(-dotProduct)) * Mathf.Clamp01(Mathf.Sign(-dotProduct));
        topDot = Mathf.Pow(math.smoothstep(0f, 0.9f, topDot), 5);
        bottomDot = Mathf.Pow(bottomDot, 5);

        _light.intensity = Mathf.Lerp(0.1f, sunIntensity, Mathf.Pow(clampedDot, 5));
        _light.color = Color.Lerp(dayColour, eveningColour, topDot + bottomDot);

        //if(transform.localEulerAngles.x == -90)
        //{
        //    transform.localEulerAngles = new Vector3(-89.9f, transform.eulerAngles.y, transform.eulerAngles.z);
        //}

        //RenderSettings.sun = GetComponent<Light>();
        //RenderSettings.ambientMode = AmbientMode.Custom;
        RenderSettings.ambientIntensity = Mathf.Lerp(2.5f, 1f, Mathf.Pow(clampedDot, 5));
        //reflectionCamera.RenderToCubemap(cubeMap);
        //cubeMap.Apply();
        //RenderSettings.customReflection = cubeMap;
    }

    //[ContextMenu("RenderToCubeMap")]
    //public void RenderToCubemap()
    //{
    //    reflectionCamera.RenderToCubemap(cubeMap);
    //    cubeMap.Apply();
    //    RenderSettings.customReflection = cubeMap;
    //}
}
