Shader "Clouds/TintAdditiveTwosided" {
	Properties {
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Main", 2D) = "white" {}
		_MaxDistance("Max Distance", float) = 1
	}
    SubShader {
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Pass {
            Blend SrcAlpha One
			ColorMask RGB
			Lighting Off ZWrite Off
            Cull Off
            CGPROGRAM
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#include "CloudsTint.cginc"
			
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
	}
}
