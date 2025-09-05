Shader "Hidden/Clouds/Depth" {

	Properties {
		_MainTex ("Main", 2D) = "white" {}
        _Cutoff("Mask Clip Value", Float) = 0.5
	}
    SubShader {
		Tags{ "RenderType" = "Opaque" }
        Pass {
            Lighting Off Fog { Mode Off }
            CGPROGRAM
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "CloudsDepth.cginc" 
            #pragma vertex vert
            #pragma fragment frag
			
			sampler2D _MainTex;
 
            struct v2f {
			    half2 uv_MainTex : TEXCOORD0;
                half4 pos : POSITION;
				float depth : TEXCOORD1;
            };
 
            v2f vert (appdata_base v) {
                v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o); 
                o.pos = UnityObjectToClipPos(v.vertex);
				COMPUTE_EYEDEPTH(o.depth);
				o.depth = o.depth * _ProjectionParams.w;
				o.uv_MainTex = v.texcoord;
                return o;
            }
			
            half4 frag( v2f i ) : COLOR {
				return half4(EncodeDepth(i.depth), 1);
            }	
            ENDCG
        }
	}
	SubShader {
		Tags{ "RenderType" = "TransparentCutout" }
		Pass {
            Lighting Off Fog { Mode Off }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "CloudsDepth.cginc" 
			
			sampler2D _MainTex;
            uniform float _Cutoff;
 
            struct v2f {
			    half2 uv_MainTex : TEXCOORD0;
                half4 pos : POSITION;
				float depth : TEXCOORD1;
            };
 
            v2f vert (appdata_base v) {
                v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o); 
                o.pos = UnityObjectToClipPos(v.vertex);
				COMPUTE_EYEDEPTH(o.depth);
				o.depth = o.depth * _ProjectionParams.w;
				o.uv_MainTex = v.texcoord;
                return o;
            }
			
            half4 frag( v2f i ) : COLOR {
				if (tex2D(_MainTex, i.uv_MainTex).a > _Cutoff) {
					return half4(EncodeDepth(i.depth), 1);
				}
				return half4(EncodeDepth(1), 0);
            }	
            ENDCG
        }
    }
}