Shader "Hidden/Clouds/PointLight" {
	Properties {
        _MainTex("Main", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_Start("Start Distance", float) = 0
		_MaxDistance("Max Distance", float) = 1
		_ShadowIntensity("Shadow Intensity", float) = 1
	}
    SubShader {
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Pass {

			Blend SrcAlpha One
			ColorMask RGB
			Cull Front
			Lighting Off ZWrite Off
            CGPROGRAM
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#include "../CloudsDepth.cginc"
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
            #pragma vertex vert
            #pragma fragment frag
			
			sampler2D _MainTex;
			uniform sampler2D _CameraDepth;
			uniform sampler2D _NormalTex;
			fixed4 _TintColor;
			float _MaxDistance;
			float _ShadowIntensity;
			float _Start;
 
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

            struct v2f {
				fixed4 color : COLOR;
				half4 pos : POSITION;
			    half2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
				float depth : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				float4 lightPoint : TEXCOORD4;
            };
 
            v2f vert (appdata_t v) {
                v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o); 
                o.pos = UnityObjectToClipPos(v.vertex);
				o.lightPoint = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)); 
				o.screenPos = ComputeScreenPos(o.pos);
				COMPUTE_EYEDEPTH(o.depth);
				o.uv = v.texcoord;
				o.color = v.color;
				o.viewDir = -WorldSpaceViewDir(v.vertex);
                return o;
            }
			
            half4 frag( v2f i ) : COLOR {
				float depth = DecodeDepth(tex2D(_CameraDepth, i.screenPos.xy / i.screenPos.w)) * _ProjectionParams.z;
				float3 pos = depth * normalize(i.viewDir) + _WorldSpaceCameraPos;
				float3 delta = (i.lightPoint.xyz - pos);
				half distance = length(delta);
				
				float factor = 1 - saturate((distance - _Start) / (_MaxDistance - _Start));
				half normal = normalize(tex2D(_NormalTex, i.screenPos.xy / i.screenPos.w) - 0.5);
				half angleCos = dot(normalize(delta), normal);
				factor = factor * lerp(0, 1, (angleCos + _ShadowIntensity) / (1 + _ShadowIntensity));

				return tex2D(_MainTex, i.uv) * _TintColor * i.color * factor * 2;
            }	
            ENDCG
        }
	}
}
