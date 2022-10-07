//
// Dynamic Snow Shader 
// Tomasz Stobierski 2013-2016
//
// simple snow coverage - no normal filtering, no snow color/normal textures
//
// you can easily put the same snow feature to any surface shader - just copy / paste "RTP specific" sections into the right places
// (we need vertex:vert in surface shader main pragma definition, note that not every built-shader got it, so you'll sometimes need to copy whole vert function)
//
//
Shader "Relief Pack - Bonus Shaders/Legacy BumpedSpecular/Snow Simple" {
Properties {
		_Color ("Color (RGBA)", Color) = (1,1,1,1)
		_SpecColor ("Specular Color (RGBA)", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0, 100)) = 80
		_MainTex ("Main texture (RGBA)", 2D) = "black" {}
		_BumpMap ("Normal map", 2D) = "bump" {}

/////////////////////////////////////////////////////////////////////
// RTP specific
//
		rtp_snow_mult("Snow multiplicator", Range(0,2)) = 1
/////////////////////////////////////////////////////////////////////
}

SubShader {
	Tags {
		"RenderType" = "Opaque"
		"Queue" = "Geometry"
	}
	
CGPROGRAM
#pragma surface surf BlinnPhong vertex:vert fullforwardshadows
#include "UnityCG.cginc"
#pragma target 3.0

/////////////////////////////////////////////////////////////////////
// RTP specific
//
float rtp_snow_strength;
float rtp_global_color_brightness_to_snow;
float rtp_snow_slope_factor;
float rtp_snow_edge_definition;
float rtp_snow_height_treshold;
float rtp_snow_height_transition;
fixed3 rtp_snow_color;
float rtp_snow_gloss;
float rtp_snow_specular;
float rtp_snow_mult;
float rtp_snow_deep_factor;
////////////////////////////////////////////////////////////////////

sampler2D _MainTex;
sampler2D _BumpMap;
float4 _Color;
float _Shininess;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
/////////////////////////////////////////////////////////////////////
// RTP specific
//	
	float4 snowDir;
/////////////////////////////////////////////////////////////////////
};

void vert (inout appdata_full v, out Input o) {
    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_PI)
		UNITY_INITIALIZE_OUTPUT(Input, o);
	#endif

/////////////////////////////////////////////////////////////////////
// RTP specific
//
	TANGENT_SPACE_ROTATION;
	o.snowDir.xyz = mul (rotation, mul(unity_WorldToObject, float4(0,1,0,0)).xyz);
	o.snowDir.w = mul(unity_ObjectToWorld, v.vertex).y;
/////////////////////////////////////////////////////////////////////	
}

void surf (Input IN, inout SurfaceOutput o) {
	float4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Gloss = tex.a * _Color.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	
/////////////////////////////////////////////////////////////////////
// RTP specific
//
	float snow_val=rtp_snow_strength*2*rtp_snow_mult;
	float snow_height_fct=saturate((rtp_snow_height_treshold - IN.snowDir.w)/rtp_snow_height_transition)*4;
	snow_val += snow_height_fct<0 ? 0 : -snow_height_fct;
	
	snow_val += rtp_snow_strength*0.5*rtp_global_color_brightness_to_snow;
	float3 norm_for_snow=float3(0,0,1);
	snow_val -= rtp_snow_slope_factor*( 1 - dot(norm_for_snow, IN.snowDir.xyz) );

	float snow_depth=snow_val-1;
	snow_depth=snow_depth<0 ? 0:snow_depth*6;		
	
	snow_val -= rtp_snow_slope_factor*( 1 - dot(o.Normal, IN.snowDir.xyz));
	snow_val=saturate(snow_val);
	snow_val=pow(abs(snow_val), rtp_snow_edge_definition);
	
	o.Albedo=lerp( o.Albedo, rtp_snow_color, snow_val );
	
	//o.Gloss=lerp(o.Gloss, rtp_snow_gloss, snow_val);
	o.Specular=lerp(o.Specular, rtp_snow_specular, snow_val);	
	
	float snow_depth_lerp=saturate(snow_depth-rtp_snow_deep_factor)*0.7;
	o.Normal=lerp(o.Normal, norm_for_snow, snow_depth_lerp);
/////////////////////////////////////////////////////////////////////
	
	o.Alpha=o.Gloss;
}

ENDCG
}


// Fallback to Diffuse
Fallback "Diffuse"
}
