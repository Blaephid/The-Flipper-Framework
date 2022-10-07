//
// Relief Terrain  - Parallax mapped material + water (flowMap)
// Tomasz Stobierski 2013-2016
//
// (water mask taken from vertex color B)
//
Shader "Relief Pack - Bonus Shaders/Water/ FlowMap" {
    Properties {
		_Color("Color", Color) = (0.5,0.5,0.5,1)	
		_MainTex("Albedo (A - Smoothness)", 2D) = "white" {}
		_GlossMin("Gloss Min", Range(0.0, 1.0)) = 0
		_GlossMax("Gloss Max", Range(0.0, 1.0)) = 1
		_Metalness("Metalness", Range(0,1)) = 0
		_BumpMap("Normal Map", 2D) = "bump" {}
		_HeightMap ("Heightmap", 2D) = "black" {}
		
		TERRAIN_ExtrudeHeight ("Extrude Height", Range(0.001,0.08)) = 0.02 
		_FlowMap ("FlowMap (RG)", 2D) = "grey" {}

		TERRAIN_FlowingMap ("Flowingmap (water bumps)", 2D) = "gray" {}
		TERRAIN_RippleMap ("Ripplemap (droplets)", 2D) = "gray" {}
		TERRAIN_RippleScale ("Ripple scale", Float) = 1

		TERRAIN_LayerWetStrength ("Layer wetness", Range(0,1)) = 1
		TERRAIN_WaterLevelSlopeDamp ("Water level slope damp", Range(0.25,8)) = 4
		TERRAIN_WaterLevel ("Water Level", Range(0,2)) = 0.5
		TERRAIN_WaterColor ("Water Color (A - opacity)", Color) = (1,1,1,0)
      	TERRAIN_WaterGloss ("Water Gloss", Range(0,1)) = 0.95
		TERRAIN_WaterEdge ("Water Edge", Range(1, 4)) = 1

		TERRAIN_DropletsSpeed ("Droplets speed", Float) = 15
		TERRAIN_RainIntensity ("Rain intensity", Range(0,1)) = 1
		TERRAIN_WetDropletsStrength ("Rain on wet", Range(0,1)) = 0
		TERRAIN_Refraction("Refraction", Range(0,0.04)) = 0.02
		TERRAIN_WetRefraction ("Wet refraction", Range(0,1)) = 1
		TERRAIN_WetGloss ("Wet Gloss", Range(0, 1)) = 0.1
		TERRAIN_Flow ("Flow", Range(0, 1)) = 0.1
		TERRAIN_FlowScale ("Flow Scale", Float) = 1
		TERRAIN_FlowSpeed ("Flow Speed", Range(0, 3)) = 0.25
		TERRAIN_FlowSpeedMap ("Flow Speed (map)", Range(0, 0.2)) = 0.1

    }
    
    SubShader {
	Tags { "RenderType" = "Opaque" }
	CGPROGRAM

	#pragma surface surf Standard vertex:vert
	#pragma exclude_renderers d3d11_9x gles
	#pragma glsl
	#pragma target 3.0

	#pragma multi_compile RTP_PM_SHADING RTP_SIMPLE_SHADING
	
	#define RTP_WETNESS
	// enabled below if you don't want to use water flow
	//#define SIMPLE_WATER
	#define RTP_WET_RIPPLE_TEXTURE	  
	
	struct Input {
		float2 uv_MainTex;
		
		float3 viewDir;
		float4 _auxDir;
		
		float4 color:COLOR;
	};

	half4 _Color;
	half _GlossMin, _GlossMax, _Metalness;

	sampler2D _MainTex;
	sampler2D _BumpMap;
	
	sampler2D _HeightMap;
	float TERRAIN_ExtrudeHeight;

	sampler2D _FlowMap;
	
	sampler2D TERRAIN_RippleMap;
	sampler2D TERRAIN_FlowingMap;
	float TERRAIN_LayerWetStrength;
	float TERRAIN_WaterLevelSlopeDamp;
	float TERRAIN_DropletsSpeed;
	float TERRAIN_RainIntensity;
	float TERRAIN_WetDropletsStrength;
	float TERRAIN_Refraction;
	float TERRAIN_WetRefraction;
	float TERRAIN_Flow;
	float TERRAIN_FlowScale;
	float TERRAIN_RippleScale;
	float TERRAIN_FlowSpeed;
	float TERRAIN_FlowSpeedMap;
	float TERRAIN_WaterLevel;
	half4 TERRAIN_WaterColor;
	float TERRAIN_WaterEdge;
	float TERRAIN_WaterSpecularity;
	float TERRAIN_WaterGloss;
	float TERRAIN_WetGloss;

	inline float2 GetRipple(float2 UV, float Intensity)
	{
	    float4 Ripple = tex2D(TERRAIN_RippleMap, UV);
	    Ripple.xy = Ripple.xy * 2 - 1;
	
	    float DropFrac = frac(Ripple.w + _Time.x*TERRAIN_DropletsSpeed);
	    float TimeFrac = DropFrac - 1.0f + Ripple.z;
	    float DropFactor = saturate(0.2f + Intensity * 0.8f - DropFrac);
	    float FinalFactor = DropFactor * Ripple.z * sin( clamp(TimeFrac * 9.0f, 0.0f, 3.0f) * 3.1415);
	    
	    return Ripple.xy * FinalFactor * 0.35f;
	}
	
	void vert (inout appdata_full v, out Input o) {
	    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_PI)
			UNITY_INITIALIZE_OUTPUT(Input, o);
		#endif
		#if defined(RTP_REFLECTION) || defined(RTP_WETNESS)
			float3 binormal = cross( v.normal, v.tangent.xyz ) * v.tangent.w;
			float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal.xyz );		
			
			float3 viewDir = -ObjSpaceViewDir(v.vertex);
			float3 viewRefl = reflect (viewDir, v.normal);
			float2 refl_vec = normalize(mul((float3x3)unity_ObjectToWorld, viewRefl)).xz;
			#ifdef RTP_ROTATE_REFLECTION
				float3 refl_rot;
				refl_rot.x=sin(_Time.x*TERRAIN_ReflectionRotSpeed);
				refl_rot.y=cos(_Time.x*TERRAIN_ReflectionRotSpeed);
				refl_rot.z=-refl_rot.x;
				o._auxDir.x=dot(refl_vec, refl_rot.yz);
				o._auxDir.y=dot(refl_vec, refl_rot.xy);
			#else
				o._auxDir.xy=refl_vec;
			#endif
			o._auxDir.xy=o._auxDir.xy*0.5+0.5;
		#endif
		#if defined(RTP_WETNESS)
		o._auxDir.zw = ( mul (rotation, mul(unity_WorldToObject, float4(0,1,0,0)).xyz) ).xy;		
		#endif
	}
	
	void surf (Input IN, inout SurfaceOutputStandard o) {
      	float3 rayPos;
      	rayPos.z=tex2D(_HeightMap, IN.uv_MainTex).a;
      	#if defined(RTP_SIMPLE_SHADING)
      	rayPos.xy=IN.uv_MainTex;
      	#else
      	rayPos.xy=IN.uv_MainTex + ParallaxOffset(rayPos.z, TERRAIN_ExtrudeHeight, IN.viewDir.xyz);
      	#endif
      	
		float3 flat_dir;
		flat_dir.xy=IN._auxDir.zw;
		flat_dir.z=sqrt(1 - saturate(dot(flat_dir.xy, flat_dir.xy)));
		float wetSlope=1-flat_dir.z;

		float perlinmask=tex2D(TERRAIN_FlowingMap, IN.uv_MainTex/8).a;
		TERRAIN_LayerWetStrength*=saturate(IN.color.b*2 - perlinmask*(1-TERRAIN_LayerWetStrength)*2);
		float2 roff=0;
		wetSlope=saturate(wetSlope*TERRAIN_WaterLevelSlopeDamp);
		float _RippleDamp=saturate(TERRAIN_LayerWetStrength*2-1)*saturate(1-wetSlope*4);
		TERRAIN_RainIntensity*=_RippleDamp;
		TERRAIN_LayerWetStrength=saturate(TERRAIN_LayerWetStrength*2);
		TERRAIN_WaterLevel=clamp(TERRAIN_WaterLevel + ((TERRAIN_LayerWetStrength - 1) - wetSlope)*2, 0, 2);
		TERRAIN_LayerWetStrength=saturate(TERRAIN_LayerWetStrength - (1-TERRAIN_LayerWetStrength)*rayPos.z);
		TERRAIN_Flow*=TERRAIN_LayerWetStrength*TERRAIN_LayerWetStrength;
		
		float p = saturate((TERRAIN_WaterLevel-rayPos.z)*TERRAIN_WaterEdge);
		p*=p;
		#if !defined(RTP_SIMPLE_SHADING) && !defined(SIMPLE_WATER)
			float2 flowUV=lerp(IN.uv_MainTex, rayPos.xy, 1-p*0.5)*TERRAIN_FlowScale;
			float _Tim=frac(_Time.x*4)*2;
			float ft=abs(frac(_Tim)*2 - 1);
			float2 flowSpeed=clamp((IN._auxDir.zw)*4,-1,1)/4;
			flowSpeed=(tex2D(_FlowMap, flowUV).rg*2-1)*float2(-1,1)*TERRAIN_FlowSpeedMap+flowSpeed;
			flowUV*=TERRAIN_FlowScale;
			flowSpeed*=TERRAIN_FlowSpeed*TERRAIN_FlowScale;
			float2 flowOffset=tex2D(TERRAIN_FlowingMap, flowUV+frac(_Tim.xx)*flowSpeed).ag*2-1;
			flowOffset=lerp(flowOffset, tex2D(TERRAIN_FlowingMap, flowUV+frac(_Tim.xx+0.5)*flowSpeed*1.1).ag*2-1, ft);
			flowOffset*=TERRAIN_Flow*p*TERRAIN_LayerWetStrength; // TTT
		#else
			float2 flowOffset=0;
			float2 flowSpeed=0;
		#endif
		
		#if defined(RTP_WET_RIPPLE_TEXTURE) && !defined(RTP_SIMPLE_SHADING)
			float2 rippleUV = IN.uv_MainTex*TERRAIN_RippleScale + flowOffset*0.1*flowSpeed/TERRAIN_FlowScale;
		  	roff = GetRipple( rippleUV, TERRAIN_RainIntensity);
			roff += GetRipple( rippleUV+float2(0.25,0.25), TERRAIN_RainIntensity);
		  	roff*=4*_RippleDamp*lerp(TERRAIN_WetDropletsStrength, 1, p);
		  	roff+=flowOffset;
		#else
			roff = flowOffset;
		#endif
		
		#if !defined(RTP_SIMPLE_SHADING)
			rayPos.xy+=TERRAIN_Refraction*roff*max(p, TERRAIN_WetRefraction);
		#endif
		
		fixed4 col = tex2D(_MainTex, rayPos.xy);
		col.rgb *= _Color.rgb * 2;

		col.a = lerp(_GlossMin, _GlossMax, col.a);
		o.Smoothness = col.a;
		o.Metallic = _Metalness;

		o.Smoothness = lerp(lerp(o.Smoothness, TERRAIN_WetGloss, TERRAIN_LayerWetStrength), TERRAIN_WaterGloss, p);
		float _WaterOpacity = TERRAIN_WaterColor.a*p;

        col.rgb = lerp(col.rgb, TERRAIN_WaterColor.rgb, _WaterOpacity);
        
        o.Normal = lerp(UnpackNormal (tex2D (_BumpMap, rayPos.xy)), float3(0,0,1), p);
        o.Normal.xy+=roff;
        o.Normal=normalize(o.Normal);
  		
		col.rgb*=1-saturate(TERRAIN_LayerWetStrength*2)*0.3;
                
        o.Albedo = col.rgb;
	}
	ENDCG
      
    } 
    Fallback "Diffuse"
}
