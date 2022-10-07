//
// Relief Terrain Shader
// Tomasz Stobierski 2013-2016
//
// this is optimizing pass for sticked blend geometries
//
Shader "Hidden/Relief Pack/ReliefTerrainBlendBaseCutout" {
Properties {
	RTP_DeferredAddPassSpec ("Gloss for deferred", Float) = 0.5
}
SubShader {
	Tags {
		"Queue" = "Geometry-101"
		"RenderType"="Transparent"
		"IgnoreProjector"="True"
	}
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }	
		Offset 2,2
		ColorMask 0
		Blend Zero Zero
		ZWrite On Cull Back
		Fog { Mode Off }
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		struct v2f {
			float4 pos : POSITION;
			fixed4 color : COLOR0;
		};

		v2f vert (appdata_full v) {
			v2f o;
			
			o.pos = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			return o;
		}

		fixed4 frag(v2f input) : COLOR
		{
			clip(-input.color.a);
			return 0;
		}
		ENDCG
	}

// (uncommented & used in U5)
		
	Pass {
		Name "DEFERRED"
		Tags { "LightMode" = "Deferred" }
		Offset 2,2
		ColorMask 0
		Blend Zero Zero
		ZWrite On Cull Back
		Fog { Mode Off }
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		struct v2f {
			float4 pos : POSITION;
			fixed4 color : COLOR0;
		};

		v2f vert (appdata_full v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			return o;
		}

		fixed4 frag(v2f input) : COLOR
		{
			clip(-input.color.a);
			return 0;
		}
		// chyba nie trzeba tego robic - i tak wycina dziure wiec pewnie dziala...
//		void frag (v2f input,
//					    out half4 outDiffuse : SV_Target0,
//					    out half4 outSpecSmoothness : SV_Target1,
//					    out half4 outNormal : SV_Target2,
//					    out half4 outEmission : SV_Target3) {		
//							clip(-input.color.a);
//							outDiffuse=1;
//							outSpecSmoothness=1;
//							outNormal=float4(0,1,0,0);
//							outEmission=1;
//		}
		ENDCG
	}
	

	Pass {
		Name "PREPASS"
		Tags { "LightMode" = "PrePassBase" }
		Blend Zero One, One Zero
		ZWrite On Cull Back
		Fog { Mode Off }
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		
		float RTP_DeferredAddPassSpec;
		
		struct v2f {
			float4 pos : POSITION;
			fixed4 color : COLOR0;
		};

		v2f vert (appdata_full v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			return o;
		}

		fixed4 frag(v2f input) : COLOR
		{
			clip(-input.color.a);
			return fixed4(0,0,0,RTP_DeferredAddPassSpec);
		}
		ENDCG			
	}	
}	

Fallback Off
}
