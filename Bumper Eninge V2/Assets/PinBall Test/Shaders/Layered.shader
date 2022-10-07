// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hedge+/Clearcoat"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_InnerNormal("Inner Normal", 2D) = "bump" {}
		[HDR]_InnerEmission("Inner Emission", 2D) = "white" {}
		[HDR]_OuterEmission("Outer Emission", 2D) = "white" {}
		_OuterNormal("Outer Normal", 2D) = "bump" {}
		_InnerSpecular("Inner Specular", 2D) = "white" {}
		_InnerEmissionStrength("Inner Emission Strength", Float) = 0
		_InnerNormalStrength("Inner Normal Strength", Range( 0 , 1)) = 0
		_OuterEmissionStrength("Outer Emission Strength", Float) = 0
		_OuterNormalStrength("Outer Normal Strength", Range( 0 , 1)) = 0
		_ClearCoatSmoothness("Clear Coat Smoothness", Range( 0 , 1)) = 1
		_CoatAmount("Coat Amount", Range( 0 , 1)) = 1
		_Occlusion("Occlusion", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityStandardUtils.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float3 worldPos;
			float4 vertexColor : COLOR;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _InnerNormalStrength;
		uniform sampler2D _InnerNormal;
		uniform float4 _InnerNormal_ST;
		uniform sampler2D _InnerEmission;
		uniform float4 _InnerEmission_ST;
		uniform float _InnerEmissionStrength;
		uniform sampler2D _InnerSpecular;
		uniform float4 _InnerSpecular_ST;
		uniform float _Occlusion;
		uniform float _OuterNormalStrength;
		uniform sampler2D _OuterNormal;
		uniform float4 _OuterNormal_ST;
		uniform sampler2D _OuterEmission;
		uniform float4 _OuterEmission_ST;
		uniform float _OuterEmissionStrength;
		uniform float _ClearCoatSmoothness;
		uniform float _CoatAmount;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			SurfaceOutputStandard s1 = (SurfaceOutputStandard ) 0;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			s1.Albedo = tex2D( _Albedo, uv_Albedo ).rgb;
			float2 uv_InnerNormal = i.uv_texcoord * _InnerNormal_ST.xy + _InnerNormal_ST.zw;
			s1.Normal = WorldNormalVector( i , UnpackScaleNormal( tex2D( _InnerNormal, uv_InnerNormal ), _InnerNormalStrength ) );
			float2 uv_InnerEmission = i.uv_texcoord * _InnerEmission_ST.xy + _InnerEmission_ST.zw;
			s1.Emission = ( tex2D( _InnerEmission, uv_InnerEmission ) * _InnerEmissionStrength ).rgb;
			s1.Metallic = 0.0;
			float2 uv_InnerSpecular = i.uv_texcoord * _InnerSpecular_ST.xy + _InnerSpecular_ST.zw;
			s1.Smoothness = tex2D( _InnerSpecular, uv_InnerSpecular ).r;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV20 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode20 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV20, 5.0 ) );
			float lerpResult33 = lerp( i.vertexColor.r , 1.0 , _Occlusion);
			float Occlusion25 = ( saturate( ( 1.0 - fresnelNode20 ) ) * lerpResult33 );
			s1.Occlusion = Occlusion25;

			data.light = gi.light;

			UnityGI gi1 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g1 = UnityGlossyEnvironmentSetup( s1.Smoothness, data.worldViewDir, s1.Normal, float3(0,0,0));
			gi1 = UnityGlobalIllumination( data, s1.Occlusion, s1.Normal, g1 );
			#endif

			float3 surfResult1 = LightingStandard ( s1, viewDir, gi1 ).rgb;
			surfResult1 += s1.Emission;

			#ifdef UNITY_PASS_FORWARDADD//1
			surfResult1 -= s1.Emission;
			#endif//1
			SurfaceOutputStandardSpecular s2 = (SurfaceOutputStandardSpecular ) 0;
			s2.Albedo = float3( 0,0,0 );
			float2 uv_OuterNormal = i.uv_texcoord * _OuterNormal_ST.xy + _OuterNormal_ST.zw;
			s2.Normal = WorldNormalVector( i , UnpackScaleNormal( tex2D( _OuterNormal, uv_OuterNormal ), _OuterNormalStrength ) );
			float2 uv_OuterEmission = i.uv_texcoord * _OuterEmission_ST.xy + _OuterEmission_ST.zw;
			float4 tex2DNode11 = tex2D( _OuterEmission, uv_OuterEmission );
			s2.Emission = ( tex2DNode11 * _OuterEmissionStrength ).rgb;
			float3 temp_cast_3 = (1.0).xxx;
			s2.Specular = temp_cast_3;
			s2.Smoothness = _ClearCoatSmoothness;
			s2.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi2 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g2 = UnityGlossyEnvironmentSetup( s2.Smoothness, data.worldViewDir, s2.Normal, float3(0,0,0));
			gi2 = UnityGlobalIllumination( data, s2.Occlusion, s2.Normal, g2 );
			#endif

			float3 surfResult2 = LightingStandardSpecular ( s2, viewDir, gi2 ).rgb;
			surfResult2 += s2.Emission;

			#ifdef UNITY_PASS_FORWARDADD//2
			surfResult2 -= s2.Emission;
			#endif//2
			float Alpha26 = ( ( fresnelNode20 * _CoatAmount ) * lerpResult33 );
			float3 lerpResult17 = lerp( surfResult1 , surfResult2 , ( Alpha26 + tex2DNode11.r ));
			c.rgb = lerpResult17;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17500
1920;1;1586;831;2646.08;501.9438;1.708789;True;True
Node;AmplifyShaderEditor.FresnelNode;20;-2508.205,588.721;Inherit;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;37;-2415.222,845.6551;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;34;-2331.523,1033.668;Inherit;False;Property;_Occlusion;Occlusion;12;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-2232.523,950.6682;Inherit;False;Constant;_Float1;Float 1;12;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;23;-2197.22,608.7416;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-2541.572,779.5829;Inherit;False;Property;_CoatAmount;Coat Amount;11;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;33;-2006.523,910.6682;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-2211.901,699.501;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;24;-1972.99,627.4274;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-1744.523,666.6682;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-1735.523,797.6682;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;19;-2516.512,-1206.206;Inherit;False;1261.175;994.2521;Inner;10;3;7;6;8;9;5;10;1;27;28;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;18;-2509.069,-169.3972;Inherit;False;1277.911;684.1702;Outer;8;13;14;15;11;12;16;2;38;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-2120.557,312.6028;Inherit;False;Property;_OuterEmissionStrength;Outer Emission Strength;8;0;Create;True;0;0;False;0;0;2.98;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-2128,-752;Inherit;True;Property;_InnerEmission;Inner Emission;2;1;[HDR];Create;True;0;0;False;0;-1;None;04b16d7b973f05c41b8c7c33837271ee;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-2466.512,-900.8538;Inherit;False;Property;_InnerNormalStrength;Inner Normal Strength;7;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-1590.761,668.8071;Inherit;False;Occlusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-2459.069,-60.25098;Inherit;False;Property;_OuterNormalStrength;Outer Normal Strength;9;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;11;-2120.557,88.60288;Inherit;True;Property;_OuterEmission;Outer Emission;3;1;[HDR];Create;True;0;0;False;0;-1;None;a2b4b22ac492ec943a4fe7571112260b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-2128,-528;Inherit;False;Property;_InnerEmissionStrength;Inner Emission Strength;6;0;Create;True;0;0;False;0;0;6.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-1536.378,784.9014;Inherit;False;Alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-1755.243,172.5087;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1905.473,399.7729;Inherit;False;Property;_ClearCoatSmoothness;Clear Coat Smoothness;10;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-2089.511,-441.9536;Inherit;True;Property;_InnerSpecular;Inner Specular;5;0;Create;True;0;0;False;0;-1;None;a9ea1a51a91c9aa4f91758a150f53ac9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;28;-1577.92,-282.8372;Inherit;False;26;Alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-1752.383,284.099;Inherit;False;Constant;_Float0;Float 0;13;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;27;-1735.414,-356.2457;Inherit;False;25;Occlusion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-1744,-576;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-2130.108,-1156.206;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;-1;None;d7485d456488dc342b85ce45c8321e9f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-2120.557,-119.3971;Inherit;True;Property;_OuterNormal;Outer Normal;4;0;Create;True;0;0;False;0;-1;None;411fbb210e9849b4e82bd49a9e5af123;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;5;-2128,-960;Inherit;True;Property;_InnerNormal;Inner Normal;1;0;Create;True;0;0;False;0;-1;None;8bf309d34d8674b408d8601b14338e81;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomStandardSurface;1;-1515.337,-655.3341;Inherit;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;29;-1192.191,-214.7676;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomStandardSurface;2;-1475.443,75.61816;Inherit;False;Specular;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;17;-992.4628,-288.9601;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-664.2873,-387.6692;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;Hedge+/Clearcoat;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;23;0;20;0
WireConnection;33;0;37;1
WireConnection;33;1;32;0
WireConnection;33;2;34;0
WireConnection;22;0;20;0
WireConnection;22;1;21;0
WireConnection;24;0;23;0
WireConnection;35;0;24;0
WireConnection;35;1;33;0
WireConnection;36;0;22;0
WireConnection;36;1;33;0
WireConnection;25;0;35;0
WireConnection;26;0;36;0
WireConnection;13;0;11;0
WireConnection;13;1;12;0
WireConnection;9;0;6;0
WireConnection;9;1;8;0
WireConnection;14;5;15;0
WireConnection;5;5;10;0
WireConnection;1;0;3;0
WireConnection;1;1;5;0
WireConnection;1;2;9;0
WireConnection;1;4;7;1
WireConnection;1;5;27;0
WireConnection;29;0;28;0
WireConnection;29;1;11;1
WireConnection;2;1;14;0
WireConnection;2;2;13;0
WireConnection;2;3;38;0
WireConnection;2;4;16;0
WireConnection;17;0;1;0
WireConnection;17;1;2;0
WireConnection;17;2;29;0
WireConnection;0;13;17;0
ASEEND*/
//CHKSM=0515D9D4CC0754F6682CACCE10F6ECF0C54BFBAC