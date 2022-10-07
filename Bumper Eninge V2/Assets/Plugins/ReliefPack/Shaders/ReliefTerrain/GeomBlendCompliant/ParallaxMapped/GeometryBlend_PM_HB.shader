//
// Relief Terrain  -  Parallax mapped material with height blending
// Tomasz Stobierski 2013-2016
//
// distances have the same meaning like in RTP inspector
//
// we're using position (x,z), size (x,z) and tiling of underlying terrain to get right coords of global perlin map and global colormap
// 
Shader "Relief Pack - GeometryBlend/ HeightBlend" {
    Properties {
		_Color("Color (RGB)", Color) = (0.5, 0.5, 0.5, 1)
		_MainTex("Texture (A - smoothness)", 2D) = "white" {}
		_GlossMin("Gloss Min", Range(0.0, 1.0)) = 0
		_GlossMax("Gloss Max", Range(0.0, 1.0)) = 1
		_Metalness("Metalness", Range(0,1)) = 0

		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_HeightMap ("Heightmap (A)", 2D) = "black" {}
		_ExtrudeHeight("Extrude Height", Range(0.001,0.1)) = 0.04

		_BumpMapPerlin ("Perlin global (special combined RG)", 2D) = "black" {}
		_BumpMapPerlinBlend ("Perlin normal", Range(0,2)) = 0.3
		_BumpMapGlobalScale ("Tiling scale", Range( 0.01,0.25) ) = 0.1
		_TERRAIN_distance_start ("Near distance start", Float) = 2
		_TERRAIN_distance_transition ("Near fade length", Float) = 20
		_TERRAIN_distance_start_bumpglobal ("Far distance start", Float) = 22
		_TERRAIN_distance_transition_bumpglobal ("Far fade length", Float) = 40
		rtp_perlin_start_val ("Perlin start val", Range(0,1)) = 0.3
				
		_ColorMapGlobal ("Global colormap", 2D) = "black" {}
		// uncomment when used w/o ReliefTerrain script component (for example with VoxelTerrains)
		//_GlobalColorMapBlendValues ("Global colormap blending (XYZ)", Vector) = (0.3,0.6,0.8,0)
		//_GlobalColorMapSaturation ("Global colormap saturation", Range(0,1)) = 0.8
		
		_TERRAIN_PosSize ("Terrain pos (xz to XY) & size(xz to ZW)", Vector) = (0,0,1000,1000)
		_TERRAIN_Tiling ("Terrain tiling (XY) & offset(ZW)", Vector) = (3,3,0,0)
		
		_TERRAIN_HeightMap ("Terrain HeightMap (combined)", 2D) = "white" {}
		_TERRAIN_Control ("Terrain splat controlMap", 2D) = "white" {}
    }
    SubShader {
	Tags {
		"Queue"="Geometry+12"
		"RenderType" = "Opaque"
	}
	
	Offset -1,-1
	ZTest LEqual	
	CGPROGRAM
	#pragma surface surf Standard vertex:vert decal:blend
	#pragma exclude_renderers d3d11_9x gles
	#pragma multi_compile RTP_PM_SHADING RTP_SIMPLE_SHADING
	#pragma target 3.0
	
	// edges can be heightblended
	#define BLENDING_HEIGHT
	
	// we'll use global colormap
	#define COLOR_MAP
	#define RTP_COLOR_MAP_BLEND_MULTIPLY
	// we'll use global perlin
	#define GLOBAL_PERLIN
	// we'll show only global colormap at far distance (no detail)
	//#define SIMPLE_FAR
		
	float4 _MainTex_ST;
	struct Input {
		float2 tc_MainTex;
		float3 aux;
		float2 globalUV;
		float3 viewDir;
		
		float4 color:COLOR;
	};
	
	half3 _Color;		
	float _GlossMin, _GlossMax, _Metalness;
	float _ExtrudeHeight;
	
	sampler2D _MainTex;
	sampler2D _BumpMap;
	sampler2D _HeightMap;
	
	sampler2D _ColorMapGlobal;
	sampler2D _BumpMapPerlin;
	float _BumpMapGlobalScale;
	float _BumpMapPerlinBlend;
	
	float rtp_perlin_start_val;		
	float _TERRAIN_distance_start;
	float _TERRAIN_distance_transition;
	float _TERRAIN_distance_start_bumpglobal;
	float _TERRAIN_distance_transition_bumpglobal;
	
	float4 _TERRAIN_PosSize;
	float4 _TERRAIN_Tiling;
	
	// set globaly by ReliefTerrain script
	float4 _GlobalColorMapBlendValues;
	float _GlobalColorMapSaturation;
	
	sampler2D _TERRAIN_HeightMap;
	sampler2D _TERRAIN_Control;
	
	// Custom&reflexLighting
	//#include "Assets/ReliefPack/Shaders/CustomLightingLegacy.cginc"
	
	void vert (inout appdata_full v, out Input o) {
	    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_PI)
			UNITY_INITIALIZE_OUTPUT(Input, o);
		#endif
		
		o.tc_MainTex.xy=TRANSFORM_TEX(v.texcoord, _MainTex);
		
		float3 Wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
		o.aux.xy=Wpos.xz-_TERRAIN_PosSize.xy+_TERRAIN_Tiling.zw;
		o.aux.xy/=_TERRAIN_Tiling.xy;
		o.aux.z=length(_WorldSpaceCameraPos.xyz-Wpos);
		
		o.globalUV=Wpos.xz-_TERRAIN_PosSize.xy;
		o.globalUV/=_TERRAIN_PosSize.zw;
		
		//float far=saturate((o._uv_Relief.w - _TERRAIN_distance_start_bumpglobal) / _TERRAIN_distance_transition_bumpglobal);
		//v.normal.xyz=lerp(v.normal.xyz, float3(0,1,0), far*_FarNormalDamp);
	}
	
	void surf (Input IN, inout SurfaceOutputStandard o) {
		#if defined(COLOR_MAP) || defined(GLOBAL_PERLIN)
		float _uv_Relief_z=saturate((IN.aux.z - _TERRAIN_distance_start) / _TERRAIN_distance_transition);
		_uv_Relief_z=1-_uv_Relief_z;
		float _uv_Relief_w=saturate((IN.aux.z - _TERRAIN_distance_start_bumpglobal) / _TERRAIN_distance_transition_bumpglobal);
		#endif
		
		#if defined(COLOR_MAP) || defined(GLOBAL_PERLIN)
		float global_color_blend=lerp( lerp(_GlobalColorMapBlendValues.y, _GlobalColorMapBlendValues.x, _uv_Relief_z*_uv_Relief_z), _GlobalColorMapBlendValues.z, _uv_Relief_w);
		float4 global_color_value=tex2D(_ColorMapGlobal, IN.globalUV);
		#ifdef SIMPLE_FAR	
			global_color_value.rgb=lerp(dot(global_color_value.rgb,0.35).xxx, global_color_value.rgb, lerp(_GlobalColorMapSaturation,saturate(_GlobalColorMapSaturation*1.3+0.2),_uv_Relief_w));
		#else
			global_color_value.rgb=lerp(dot(global_color_value.rgb,0.35).xxx, global_color_value.rgb, _GlobalColorMapSaturation);
		#endif
		#endif

      	float tH=tex2D(_HeightMap, IN.tc_MainTex.xy).a;
		#if !defined(RTP_SIMPLE_SHADING) 
	      	float2 uv=IN.tc_MainTex.xy + ParallaxOffset(tH, _ExtrudeHeight, IN.viewDir.xyz)*(1-IN.color.a);
      	#endif
      	float control=(tH+0.01);
		#if !defined(RTP_SIMPLE_SHADING) 
	      	float2 uvMixed=uv;
		#else
	      	float2 uvMixed=IN.tc_MainTex.xy;
		#endif
      	
      	fixed4 col=tex2D(_MainTex, uvMixed);
      	o.Albedo=col.rgb*_Color * 2;
		#ifdef SIMPLE_FAR      	
		o.Albedo=lerp(o.Albedo, global_color_value.rgb, _uv_Relief_w);
		#endif

		o.Smoothness = lerp(_GlossMin, _GlossMax, col.a);
		o.Metallic = _Metalness;

      	#ifdef COLOR_MAP
		#ifdef RTP_COLOR_MAP_BLEND_MULTIPLY
			o.Albedo=lerp(o.Albedo, o.Albedo*global_color_value.rgb*2, global_color_blend);
		#else
			o.Albedo=lerp(o.Albedo, global_color_value.rgb, global_color_blend);
		#endif      	
		#endif
      	
		o.Normal=UnpackNormal(tex2D(_BumpMap,uvMixed));
      	#ifdef GLOBAL_PERLIN
	      	//float perlinmask=tex2D(_BumpMapGlobal, IN.aux.xy/8).r;
			//float4 global_bump_val=tex2D(_BumpMapPerlin, IN.aux.xy*_BumpMapGlobalScale);
			float4 global_bump_val=tex2D(_BumpMapPerlin, IN.tc_MainTex.xy*_BumpMapGlobalScale);
	
			float3 norm_far;
			norm_far.xy = global_bump_val.rg*2-1;
			norm_far.z = sqrt(1 - saturate(dot(norm_far.xy, norm_far.xy)));      	
			
			o.Normal+=norm_far*lerp(rtp_perlin_start_val,1, _uv_Relief_w)*_BumpMapPerlinBlend;	
			o.Normal=normalize(o.Normal);
		#endif	      	

      	
		#if defined(BLENDING_HEIGHT)
			float4 terrain_coverage=tex2D(_TERRAIN_Control, IN.globalUV);
			float4 splat_control1=terrain_coverage * tex2D(_TERRAIN_HeightMap, IN.aux.xy) * IN.color.a;
			float4 splat_control2=float4(control, 0, 0, 0) * (1-IN.color.a);
			
			float blend_coverage=dot(terrain_coverage, 1);
			if (blend_coverage>0.1) {
			
				splat_control1*=splat_control1;
				splat_control1*=splat_control1;
				splat_control2*=splat_control2;
				splat_control2*=splat_control2;
				
				float normalize_sum=dot(splat_control1, 1)+dot(splat_control2, 1);
				splat_control1 /= normalize_sum;
				splat_control2 /= normalize_sum;		
				
				o.Alpha=dot(splat_control2,1);
				o.Alpha=lerp(1-IN.color.a, o.Alpha, saturate((blend_coverage-0.1)*4) );
			} else {
				o.Alpha=1-IN.color.a;
			}
		#else
			o.Alpha=1-IN.color.a;
		#endif
		#if defined(UNITY_PASS_PREPASSFINAL)
			o.Smoothness*=o.Alpha;
		#endif			
	}
	ENDCG
      
    } 
	//FallBack "Diffuse"
}
