Shader "Hidden/Clouds/ClearColor"
{
	Properties {
		_Color("Main Color", Color) = (.5,.5,.5,0.8)
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
			};

			half4 _Color;

			v2f vert(appdata_base v) {
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : COLOR {
				return fixed4(0.5, 0.5, 1, 0);
			}
			ENDCG
		}
	} 
}
