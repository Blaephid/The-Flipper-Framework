//
// Relief Terrain Geometry Blend shader
// Tomasz Stobierski 2013-2016
//
Shader "Relief Pack - GeometryBlend/ Bumped Detail Snow" {
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
		//_DetailColor ("Detail Color (RGBA)", Color) = (1, 1, 1, 1)
		_DetailBumpTex ("Detail Normalmap", 2D) = "bump" {}
		_DetailScale ("Detail Normal Scale", Float) = 1

		rtp_snow_mult("Snow multiplicator", Range(0,2)) = 1
		_ColorSnow ("Snow texture (RGBA)", 2D) = "white" {}
		_BumpMapSnow ("Snow Normalmap", 2D) = "bump" {}
		_distance_start("Snow near distance", Float) = 10
		_distance_transition("Snow distance transition length", Range(0,100)) = 20
}


SubShader {
	Tags {
		"Queue"="Geometry+12"
		"RenderType" = "Opaque"
	}
	LOD 700

//Offset -1,-1
ZTest LEqual
CGPROGRAM
#pragma surface surf Standard vertex:vert fullforwardshadows decal:blend
#pragma target 3.0
#pragma glsl
#pragma exclude_renderers d3d11_9x gles
#pragma multi_compile RTP_PM_SHADING RTP_SIMPLE_SHADING

#include "UnityCG.cginc"

#define detail_map_enabled

/////////////////////////////////////////////////////////////////////
// RTP specific
//
	#define RTP_SNOW
	//#define RTP_SNW_CHOOSEN_LAYER_NORM
	//#define RTP_SNW_CHOOSEN_LAYER_COLOR
/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
// RTP specific
//
#ifdef RTP_SNOW
float rtp_snow_strength;
float rtp_global_color_brightness_to_snow;
float rtp_snow_slope_factor;
float rtp_snow_edge_definition;
float4 rtp_snow_strength_per_layer0123;
float4 rtp_snow_strength_per_layer4567;
float rtp_snow_height_treshold;
float rtp_snow_height_transition;
fixed3 rtp_snow_color;
float rtp_snow_gloss;
float rtp_snow_metallic;
float rtp_snow_mult;
float rtp_snow_deep_factor;

sampler2D _ColorSnow;
sampler2D _BumpMapSnow;
float4 _MainTex_TexelSize;
float4 _BumpMap_TexelSize;
#endif
////////////////////////////////////////////////////////////////////

sampler2D _MainTex;
sampler2D _BumpMap;
float4 _Color;
float _GlossMin, _GlossMax, _Metalness;

half _distance_start;
half _distance_transition;

fixed4 _DetailColor;
float _DetailScale;
sampler2D _DetailBumpTex;

float4 _MainTex_ST;

struct Input {
	float2 _uv_MainTex;
	float2 uv_ColorSnow;
	float4 snowDir;
	
	float3 worldPos;
	float4 color:COLOR;
};

void vert (inout appdata_full v, out Input o) {
    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_PI)
		UNITY_INITIALIZE_OUTPUT(Input, o);
	#endif
	o._uv_MainTex.xy=v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;

/////////////////////////////////////////////////////////////////////
// RTP specific
//
	#ifdef RTP_SNOW
		TANGENT_SPACE_ROTATION;
		o.snowDir.xyz = mul (rotation, mul(unity_WorldToObject, float4(0,1,0,0)).xyz);
		o.snowDir.w = mul(unity_ObjectToWorld, v.vertex).y;
	#endif	
/////////////////////////////////////////////////////////////////////	
}

void surf (Input IN, inout SurfaceOutputStandard o) {
//	o.Emission.rg=frac(IN._uv_MainTex.xy);
//	o.Alpha=1;
//	return;
	
	float4 tex = tex2D(_MainTex, IN._uv_MainTex.xy);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Smoothness = lerp(_GlossMin, _GlossMax, tex.a);
	o.Metallic = _Metalness;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN._uv_MainTex.xy));
	
	#ifndef RTP_SIMPLE_SHADING
	#ifdef detail_map_enabled
		float3 norm_det=UnpackNormal(tex2D(_DetailBumpTex, IN._uv_MainTex.xy*_DetailScale));
		o.Normal+=2*norm_det;//*_DetailColor.a;
		o.Normal=normalize(o.Normal);
	#endif
	#endif
	
/////////////////////////////////////////////////////////////////////
// RTP specific
//
	#ifdef RTP_SNOW
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
		
		float snow_depth_lerp=saturate(snow_depth-rtp_snow_deep_factor);
		
		#ifdef RTP_SNW_CHOOSEN_LAYER_COLOR
		#ifndef RTP_SIMPLE_SHADING
			half4 c=tex2D(_ColorSnow, IN.uv_ColorSnow);
			float3 rtp_snow_color_tex=c.rgb;
			rtp_snow_gloss=c.a;
			
			float _dist=saturate((distance(_WorldSpaceCameraPos, IN.worldPos) - _distance_start) / _distance_transition);
			
			rtp_snow_color=lerp(rtp_snow_color_tex, rtp_snow_color, _dist);
		#endif
		#endif
		
		o.Albedo=lerp( o.Albedo, rtp_snow_color, snow_val );
		
		float2 dx = ddx( IN._uv_MainTex.xy * _MainTex_TexelSize.z );
		float2 dy = ddy( IN._uv_MainTex.xy * _MainTex_TexelSize.w );
		float d = max( dot( dx, dx ), dot( dy, dy ) );
		float mip_selector=min(0.5*log2(d), 8);
		float mip_selector_bumpMap=	max(0,mip_selector-log2(_BumpMap_TexelSize.x/_MainTex_TexelSize.x));
		float3 snow_normal=UnpackNormal(tex2Dlod(_BumpMap, float4(IN._uv_MainTex.xy, mip_selector_bumpMap.xx+snow_depth.xx)));
		
		#ifdef RTP_SNW_CHOOSEN_LAYER_NORM
			float3 n=UnpackNormal(tex2D(_BumpMapSnow, IN.uv_ColorSnow));
			snow_normal=lerp(snow_normal, n, snow_depth_lerp );
			snow_normal=normalize(snow_normal);
		#endif
		
		o.Normal=lerp(o.Normal, snow_normal, snow_val);		
		//o.Normal=normalize(o.Normal);
		
		o.Smoothness=lerp(o.Smoothness, rtp_snow_gloss, snow_val);
		o.Metallic=lerp(o.Metallic, rtp_snow_metallic, snow_val);	
	#endif
/////////////////////////////////////////////////////////////////////
	
	o.Alpha=1-IN.color.a;
	#if defined(UNITY_PASS_PREPASSFINAL)
		o.Smoothness*=o.Alpha;
	#endif
}

ENDCG
}

	//FallBack "Diffuse"
}
