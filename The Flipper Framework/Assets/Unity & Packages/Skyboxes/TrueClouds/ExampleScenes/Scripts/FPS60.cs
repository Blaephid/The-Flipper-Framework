using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS60 : MonoBehaviour
{
    void Awake()
    {
        if (Application.isMobilePlatform)
        {
            Application.targetFrameRate = 300;
        }
    }
}
