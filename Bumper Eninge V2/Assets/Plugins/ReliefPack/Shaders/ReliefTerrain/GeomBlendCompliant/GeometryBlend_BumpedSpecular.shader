//
// Relief Terrain Geometry Blend shader
// Tomasz Stobierski 2013-2016
//
Shader "Relief Pack - GeometryBlend/ Bumped" {
Properties {
		//
		// keep in mind that not all of properties are used, depending on shader configuration in defines section below
		//
		_Color ("Color (RGB)", Color) = (1,1,1,1)
		_MainTex ("Main texture (A - smoothness)", 2D) = "black" {}
		_GlossMin ("Gloss Min", Range(0,1)) = 0
		_GlossMax("Gloss Max", Range(0,1)) = 1
		_Metalness("Metalness", Range(0,1)) = 0
		_BumpMap ("Normal map", 2D) = "bump" {}
}


SubShader {
	Tags {
		"Queue"="Geometry+12"
		"RenderType" = "Opaque"
	}
	LOD 700

Offset -1,-1
ZTest LEqual
CGPROGRAM
#pragma surface surf Standard fullforwardshadows decal:blend
#pragma target 3.0

#include "UnityCG.cginc"

sampler2D _MainTex;
sampler2D _BumpMap;
float4 _Color;
float _GlossMin, _GlossMax, _Metalness;


struct Input {
	float2 uv_MainTex;
	float4 color:COLOR;
};

void surf (Input IN, inout SurfaceOutputStandard o) {
	float4 tex = tex2D(_MainTex, IN.uv_MainTex.xy);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Smoothness = lerp(_GlossMin, _GlossMax, tex.a);
	o.Metallic = _Metalness;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex.xy));
	
	o.Alpha=1-IN.color.a;
	#if defined(UNITY_PASS_PREPASSFINAL)
		o.Smoothness*=o.Alpha;
	#endif	
}

ENDCG
}

	//FallBack "Diffuse"
}
