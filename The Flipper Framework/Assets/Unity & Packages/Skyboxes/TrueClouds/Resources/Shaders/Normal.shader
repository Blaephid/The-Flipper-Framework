Shader "Hidden/Clouds/Normal" {
	Properties {
	}
	SubShader {
		Tags{ "RenderType" = "Opaque" }
		Pass{

			Lighting Off Fog{ Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"

			struct v2f {
				half4 pos : POSITION;
				float3 normal : NORMAL;
			};

			v2f vert(appdata_base v) {
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f i) : COLOR {
				return fixed4(normalize(i.normal) * 0.5 + fixed3(0.5, 0.5, 0.5), 1);
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
