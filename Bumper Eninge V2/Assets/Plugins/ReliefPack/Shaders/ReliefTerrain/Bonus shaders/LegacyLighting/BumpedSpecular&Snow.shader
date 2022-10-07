//
// Dynamic Snow Shader 
// Tomasz Stobierski 2013-2016
//
//
// you can easily put the same snow feature to any surface shader - just copy / paste "RTP specific" sections into the right places
// (we need vertex:vert in surface shader main pragma definition, note that not every built-shader got it, so you'll sometimes need to copy whole vert function)
//
//
Shader "Relief Pack - Bonus Shaders/Legacy BumpedSpecular/Snow" {
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
		_ColorSnow ("Snow texture (RGBA)", 2D) = "white" {}
		_BumpMapSnow ("Snow Normalmap", 2D) = "bump" {}
/////////////////////////////////////////////////////////////////////
}

SubShader {
	Tags {
		"RenderType" = "Opaque"
		"Queue" = "Geometry"
	}
	
CGPROGRAM
#pragma surface surf BlinnPhong vertex:vert fullforwardshadows
#pragma target 3.0
#pragma glsl

#pragma exclude_renderers d3d11_9x gles

#include "UnityCG.cginc"

/////////////////////////////////////////////////////////////////////
// RTP specific
//
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
float rtp_snow_specular;
float rtp_snow_mult;
float rtp_snow_deep_factor;

sampler2D _ColorSnow;
sampler2D _BumpMapSnow;

float4 _MainTex_TexelSize;
float4 _BumpMap_TexelSize;
////////////////////////////////////////////////////////////////////

sampler2D _MainTex;
sampler2D _BumpMap;
float4 _Color;
float _Shininess;

float4 _MainTex_ST;

struct Input {
	float4 _uv_MainTex;
/////////////////////////////////////////////////////////////////////
// RTP specific
//
	float2 uv_ColorSnow;
	float4 snowDir;
/////////////////////////////////////////////////////////////////////
};

void vert (inout appdata_full v, out Input o) {
    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_PI)
		UNITY_INITIALIZE_OUTPUT(Input, o);
	#endif
	o._uv_MainTex.xy=v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
/////////////////////////////////////////////////////////////////////
// RTP specific
//
	#if (!defined(SHADER_API_OPENGL))
		o._uv_MainTex.zw=o._uv_MainTex.xy / _MainTex_TexelSize.x;
	#endif	
	
	TANGENT_SPACE_ROTATION;
	o.snowDir.xyz = mul (rotation, mul(unity_WorldToObject, float4(0,1,0,0)).xyz);
	o.snowDir.w = mul(unity_ObjectToWorld, v.vertex).y;
/////////////////////////////////////////////////////////////////////	
}

void surf (Input IN, inout SurfaceOutput o) {
	float4 tex = tex2D(_MainTex, IN._uv_MainTex.xy);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Gloss = tex.a * _Color.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN._uv_MainTex.xy));
	o.Alpha=o.Gloss;
	
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
	
	float snow_depth_lerp=saturate(snow_depth-rtp_snow_deep_factor);
	
	// by color
		half4 c=tex2D(_ColorSnow, IN.uv_ColorSnow);
		float3 rtp_snow_color_tex=c.rgb;
		rtp_snow_gloss=c.a;
	
	o.Albedo=lerp( o.Albedo, rtp_snow_color_tex, snow_val );
	
	float2 dx = ddx( IN._uv_MainTex.zw );
	float2 dy = ddy( IN._uv_MainTex.zw );
	float d = max( dot( dx, dx ), dot( dy, dy ) );
	float mip_selector=min(0.5*log2(d), 8);
	float mip_selector_bumpMap=	max(0,mip_selector-log2(_BumpMap_TexelSize.x/_MainTex_TexelSize.x));
	float3 snow_normal=UnpackNormal(tex2Dlod(_BumpMap, float4(IN._uv_MainTex.xy, mip_selector_bumpMap.xx+snow_depth.xx)));
	
	// by norm
	float3 n=UnpackNormal(tex2D(_BumpMapSnow, IN.uv_ColorSnow));
	snow_normal=lerp(snow_normal, n, snow_depth_lerp );
	snow_normal=normalize(snow_normal);
	
	o.Normal=lerp(o.Normal, snow_normal, snow_val);		
	//o.Normal=normalize(o.Normal);
	
	o.Gloss=lerp(o.Gloss, rtp_snow_gloss, snow_val);
	o.Specular=lerp(o.Specular, rtp_snow_specular, snow_val);	
/////////////////////////////////////////////////////////////////////
}

ENDCG
}


// Fallback to Diffuse
Fallback "Diffuse"
}
