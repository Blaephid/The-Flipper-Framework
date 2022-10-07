//
// Relief Terrain  -  Parallax mapped material with caustics
// Tomasz Stobierski 2013-2016
//
Shader "Relief Pack - Bonus Shaders/Legacy BumpedSpecular/Caustics" {
    Properties {
		_MainColor ("Main Color (RGB)", Color) = (0.5, 0.5, 0.5, 1)		
		_SpecColor ("Specular Color (RGBA)", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.5
		
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		
		// caustics
		TERRAIN_CausticsTex ("Caustics", 2D) = "black" {}
		TERRAIN_CausticsColor ("Caustics color", Color) = (1,1,1,1)
		TERRAIN_CausticsAnimSpeed ("Caustics anim speed", Float) = 1
		TERRAIN_CausticsWaterLevel ("Caustics water level", Float) = 0
		TERRAIN_CausticsWaterDeepFadeLength ("Caustics deep fade", Float) = 15
		TERRAIN_CausticsWaterShallowFadeLength ("Caustics shallow fade", Float) = 4		
    }
    SubShader {
	Tags {
		"Queue" = "Geometry"
		"RenderType" = "Opaque"
	}
	
	CGPROGRAM
	#pragma surface surf BlinnPhong vertex:vert
	#pragma exclude_renderers d3d11_9x gles
	#pragma target 3.0
	
	struct Input {
		float2 uv_MainTex;
		float2 _uvCaustics;
		float level;
	};
	
	half3 _MainColor;		
	float _Shininess;
	
	sampler2D _MainTex;
	sampler2D _BumpMap;
	
	// caustics
	float TERRAIN_CausticsAnimSpeed;
	half4 TERRAIN_CausticsColor;
	sampler2D TERRAIN_CausticsTex;
	float4 TERRAIN_CausticsTex_ST;
	float TERRAIN_CausticsWaterLevel;
	float TERRAIN_CausticsWaterDeepFadeLength;
	float TERRAIN_CausticsWaterShallowFadeLength;	
	float TERRAIN_WetnessDark;
	float TERRAIN_WetnessGloss;
	float TERRAIN_WetnessSpecularity;
	
	void vert (inout appdata_full v, out Input o) {
	    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_PI)
			UNITY_INITIALIZE_OUTPUT(Input, o);
		#endif
		
		o._uvCaustics=mul(unity_ObjectToWorld, v.vertex).xz*TERRAIN_CausticsTex_ST.xy+TERRAIN_CausticsTex_ST.zw;
		o.level=mul(unity_ObjectToWorld, v.vertex).y;
	}	
	
	void surf (Input IN, inout SurfaceOutput o) {
      	float2 uvMixed=IN.uv_MainTex;
      	
      	fixed4 col=tex2D(_MainTex, uvMixed);
      	o.Normal=UnpackNormal(tex2D(_BumpMap, uvMixed));
      	o.Albedo=col.rgb;
      	o.Gloss=col.a;
      	
      	o.Specular=_Shininess;
		
		o.Albedo*=_MainColor*2;	

		
		{
			float damp_fct_caustics=saturate((IN.level-TERRAIN_CausticsWaterLevel+TERRAIN_CausticsWaterDeepFadeLength)/TERRAIN_CausticsWaterDeepFadeLength);
			float overwater=saturate(-(IN.level-TERRAIN_CausticsWaterLevel-TERRAIN_CausticsWaterShallowFadeLength)/TERRAIN_CausticsWaterShallowFadeLength);
			damp_fct_caustics*=overwater;		
		
			float tim=_Time.x*TERRAIN_CausticsAnimSpeed;
			uvMixed=IN._uvCaustics; // topplanar uv
			float3 _Emission=tex2D(TERRAIN_CausticsTex, uvMixed+float2(tim, tim) ).rgb;
			_Emission+=tex2D(TERRAIN_CausticsTex, uvMixed+float2(-tim, -tim*0.873) ).rgb;
			_Emission+=tex2D(TERRAIN_CausticsTex, uvMixed*1.1+float2(tim, 0) ).rgb;
			_Emission+=tex2D(TERRAIN_CausticsTex, uvMixed*0.5+float2(0, tim*0.83) ).rgb;
			_Emission=saturate(_Emission-1.6);
			_Emission*=_Emission;
			_Emission*=_Emission;
			_Emission*=damp_fct_caustics;			
			_Emission*=TERRAIN_CausticsColor.rgb*3;
			o.Emission+=_Emission;
		} 
	}
	ENDCG
      
    } 
    Fallback Off
}
