Shader "0_SonicGT/CharacterShader" {
	Properties {
		[HDR]_DifuseColor ("Dif Color", Color) = (1,1,1,0.0)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}


		[Space] 
		[Header(Rim Effects)]
		[Space] 
		[HDR]_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)		
		_RimPower ("Rim Power", Range(0.5,200.0)) = 3.0
		_RimModifier ("Rim Modifier", Vector) = (1,1,1,1)
		[HDR]_CenterColor ("Center Color", Color) = (0.26,0.19,0.16,0.0)		
		_CenterPower ("Center Power", Range(0.5,2000.0)) = 3.0
		_CenterModifier ("Center Modifier", Vector) = (1,1,1,1)
		_RimMap ("Rim Texture", 2D) = "white" {}
		[Space] 
		[Header(Normal Map)]
		[Space] 
		_BumpMap ("Normal Texture", 2D) = "bump" {}
		_BumpPower ("Bump Power", Range(-1,1)) = 1
		[Space] 
		_emission  ("Emission", Range(0,10)) = 0.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5

		_Cube ("Reflection Map", Cube) ="black" {}
		_Reflectivity ("Reflectivity", Range(0,1)) = 0.0
		_RefMap ("Reflectivity Texture", 2D) = "white" {}
		}
	SubShader {
		Tags {"Queue"="Geometry"	"RenderType"="Opaque"}
		LOD 200

		/*
		    // extra pass that renders to depth buffer only
		Pass {
        ZWrite On
        ColorMask 0
		}
		// paste in __forward rendering__ passes from Transparent/Diffuse
		UsePass "Transparent/Diffuse/FORWARD"
		*/

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows//alpha:fade 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _RimMap,_RefMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldNormal;
			fixed3 viewDir;					
			float3 worldPos;
			float3 worldRefl;
			INTERNAL_DATA
		};

		sampler2D _CameraDepthTexture;
		samplerCUBE _Cube;
		float4 _RimColor,
		_CenterColor,
		_DifuseColor,
		_RimModifier,
		_CenterModifier;
		float _RimPower,
		_CenterPower,
		_BumpPower,
		_emission,
		_Reflectivity;
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
			fixed4 ref = tex2D (_RefMap, IN.uv_MainTex);			
			
			float3 VD = IN.viewDir;
			fixed3 worldNormal = WorldNormalVector(IN,o.Normal);
			float3 Nrm = UnpackScaleNormal (tex2D (_BumpMap, IN.uv_BumpMap),_BumpPower );

			float Center = saturate(dot (normalize(VD), Nrm * _CenterModifier));
			float rim = 1.0 - saturate(dot (normalize(VD), Nrm * _RimModifier));

			float3 rimrgb = ((r *  _RimColor) * pow (rim, _RimPower));
			float3 Centerrgb = ((r * _CenterColor) * pow (Center, _CenterPower));

			if(worldNormal.y > 0)
			{
			rimrgb *= worldNormal.y;
			} else {
			rimrgb = float3 (0,0,0);
			}

			float3 reflectedDir = texCUBE (_Cube, WorldReflectionVector (IN, o.Normal)).rgb * _Reflectivity * ref.r;


			o.Albedo = ((c.rgb * _DifuseColor) + rimrgb + Centerrgb )+ (reflectedDir); 			
			o.Normal = UnpackScaleNormal (tex2D (_BumpMap, IN.uv_BumpMap),_BumpPower );
			o.Smoothness = _Glossiness;
			o.Emission = o.Albedo * _emission *r;

		}
		ENDCG
	}
	FallBack "Diffuse"
}
