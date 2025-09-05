using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TrueClouds
{
    class CloudCamera3D: CloudCamera
    {
        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            RenderClouds(source, destination);
        }
    }
}
