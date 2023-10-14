Shader "0_SonicGT/RingShader" {
	Properties {
		_DifuseColor ("Dif Color", Color) = (1,1,1,0.0)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}


		[Space] 
		[Header(Rim Effects)]
		[Space] 
		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)		
		_RimPower ("Rim Power", Range(0,20.0)) = 3.0
		_CenterColor ("Center Color", Color) = (0.26,0.19,0.16,0.0)		
		_CenterPower ("Center Power", Range(0,100.0)) = 3.0
		_RimMap ("Rim Texture", 2D) = "bump" {}
		[Space] 
		[Header(Normal Map)]
		[Space] 
		_BumpMap ("Normal Texture", 2D) = "bump" {}
		_BumpPower ("Bump Power", Range(0,1)) = 1
		[Space] 
		_emission  ("Emission", Range(0,2)) = 0.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		
		}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard  fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _RimMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldNormal;
			fixed3 viewDir;
			INTERNAL_DATA
		};
		
		float4 _RimColor,
		_CenterColor,
		_DifuseColor;
		float _RimPower,
		_CenterPower,
		_BumpPower,
		_emission;
		half _Glossiness;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 r = tex2D (_RimMap, IN.uv_MainTex);			
			
			float3 VD = IN.viewDir;
			fixed3 worldNormal = WorldNormalVector(IN,o.Normal);
			float3 Nrm = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));

			float Center = saturate(dot (normalize(VD), Nrm) + worldNormal.y);

			float rim = 1.0 - saturate(dot (normalize(VD), Nrm));
			
			//Center = clamp(-1,1,Center  );

			float3 rimrgb = ((r.rgb * _RimColor) * pow (rim, _RimPower));
			float3 Centerrgb = ((r.rgb * _CenterColor) * pow (Center, _CenterPower));



			o.Normal = UnpackScaleNormal (tex2D (_BumpMap, IN.uv_BumpMap),_BumpPower );
			o.Albedo = (c.rgb * _DifuseColor) + rimrgb + Centerrgb; 			
			
			o.Smoothness = _Glossiness;
			o.Emission = o.Albedo * _emission;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
