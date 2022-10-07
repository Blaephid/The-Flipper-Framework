using UnityEngine;
using System.Collections;

[AddComponentMenu("Relief Terrain/Helpers/Tint dielectric color")]
public class RTP_TintDielectricColor : MonoBehaviour {
    [Tooltip("You can reduce/increase reflectivity by tinting default unity_ColorSpaceDielectricSpec.rgb color")]
    [ColorUsage(false)]public Color DielectricTint = new Color(0.2f, 0.2f, 0.2f, 1);

    void Awake()
    {
        SetDielectricColorTint();
    }

    void OnValidate()
    {
        SetDielectricColorTint();
    }

    public void SetDielectricColorTint()
    {
        Shader.SetGlobalColor("RTP_ColorSpaceDielectricSpecTint", DielectricTint);
    }
}