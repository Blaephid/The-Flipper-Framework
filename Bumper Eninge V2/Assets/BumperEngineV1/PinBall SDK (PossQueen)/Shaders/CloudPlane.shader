// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CloudPlane"
{
	Properties
	{
		_EdgeLength ( "Edge length", Range( 2, 50 ) ) = 47.6
		_Noise("Noise", 2D) = "white" {}
		_Noise2Scale("Noise 2 Scale", Float) = 0.1
		_Noise1Scale("Noise 1 Scale", Float) = 0.1
		_Noise2Speed("Noise 2 Speed", Vector) = (0,0,0,0)
		_Noise1Speed("Noise 1 Speed", Vector) = (0,0,0,0)
		_CloudCoverage("Cloud Coverage", Float) = 0.15
		_CloudColor("Cloud Color", Color) = (0,0,0,0)
		_DistanceFade("Distance Fade", Float) = 0
		_DistanceFalloff("Distance Falloff", Float) = 0
		_Depth("Depth", Float) = 0
		_Falloff("Falloff", Float) = 0
		_CloudHeight("Cloud Height", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
		};

		uniform sampler2D _Noise;
		uniform float2 _Noise1Speed;
		uniform float _Noise1Scale;
		uniform float _Noise2Scale;
		uniform float2 _Noise2Speed;
		uniform float _CloudCoverage;
		uniform float _CloudHeight;
		uniform float4 _CloudColor;
		uniform float _DistanceFalloff;
		uniform float _DistanceFade;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Depth;
		uniform float _Falloff;
		uniform float _EdgeLength;

		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess (v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult5 = (float2(ase_worldPos.x , ase_worldPos.z));
			float temp_output_28_0 = saturate( (_CloudCoverage + ((( tex2Dlod( _Noise, float4( ( ( _Noise1Speed * _Time.y ) + ( _Noise1Scale * appendResult5 ) ), 0, 0.0) ) + tex2Dlod( _Noise, float4( ( ( appendResult5 * _Noise2Scale ) + ( _Time.y * _Noise2Speed ) ), 0, 0.0) ) )).r - 0.0) * (1.0 - _CloudCoverage) / (1.0 - 0.0)) );
			v.vertex.xyz += ( ase_vertex3Pos + ( temp_output_28_0 * _CloudHeight ) );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _CloudColor.rgb;
			float3 ase_worldPos = i.worldPos;
			float2 appendResult5 = (float2(ase_worldPos.x , ase_worldPos.z));
			float temp_output_28_0 = saturate( (_CloudCoverage + ((( tex2D( _Noise, ( ( _Noise1Speed * _Time.y ) + ( _Noise1Scale * appendResult5 ) ) ) + tex2D( _Noise, ( ( appendResult5 * _Noise2Scale ) + ( _Time.y * _Noise2Speed ) ) ) )).r - 0.0) * (1.0 - _CloudCoverage) / (1.0 - 0.0)) );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV38 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode38 = ( 0.0 + _DistanceFalloff * pow( 1.0 - fresnelNdotV38, _DistanceFade ) );
			float lerpResult40 = lerp( temp_output_28_0 , 0.0 , saturate( fresnelNode38 ));
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float eyeDepth49 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			o.Alpha = ( lerpResult40 * saturate( ( ( abs( ( eyeDepth49 - ase_screenPos.w ) ) + _Depth ) / _Falloff ) ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
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
				vertexDataFunc( v );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
0;6;1920;1013;185.7893;-272.8362;1;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;4;-1943.439,87.57693;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;11;-1385.532,357.0668;Inherit;False;Property;_Noise2Scale;Noise 2 Scale;7;0;Create;True;0;0;0;False;0;False;0.1;0.002;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-1455.16,-74.00906;Inherit;False;Property;_Noise1Scale;Noise 1 Scale;8;0;Create;True;0;0;0;False;0;False;0.1;0.005;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;5;-1713.834,126.5212;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;15;-1224.895,511.2438;Inherit;False;Property;_Noise2Speed;Noise 2 Speed;9;0;Create;True;0;0;0;False;0;False;0,0;0.01,0.01;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;16;-1137.945,-99.74617;Inherit;False;Property;_Noise1Speed;Noise 1 Speed;10;0;Create;True;0;0;0;False;0;False;0,0;-0.04,-0.01;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;19;-1206.398,153.5226;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1041.398,344.5226;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-1206.344,257.7451;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-885.3977,-48.47739;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-1218.631,29.40817;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1240.207,-305.2593;Inherit;True;Property;_Noise;Noise;6;0;Create;True;0;0;0;False;0;False;None;3f2fbe9df6bb12742a1bfd859d7f4945;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.ScreenPosInputsNode;47;471.0249,888.6002;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-550.4924,29.27702;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-921.3977,289.5226;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;48;484.7592,1080.128;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-736.3298,275.4573;Inherit;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-285.9218,-57.75277;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenDepthNode;49;700.169,909.7609;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;50;907.5689,955.0612;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;13;150.9322,91.93734;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;23;408.6023,245.5226;Inherit;False;True;False;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;51;1092.766,952.8773;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;309.6023,344.5226;Inherit;False;Property;_CloudCoverage;Cloud Coverage;11;0;Create;True;0;0;0;False;0;False;0.15;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;667.0155,764.723;Inherit;False;Property;_DistanceFade;Distance Fade;13;0;Create;True;0;0;0;False;0;False;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;1139.822,1047.929;Inherit;False;Property;_Depth;Depth;15;0;Create;True;0;0;0;False;0;False;0;-170.93;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;650.2107,590.8362;Inherit;False;Property;_DistanceFalloff;Distance Falloff;14;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;34;768.5456,376.8373;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;38;915.5156,641.5231;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;1387.696,1059.609;Inherit;False;Property;_Falloff;Falloff;16;0;Create;True;0;0;0;False;0;False;0;25.29;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;52;1309.822,927.9294;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;55;1598.972,937.2029;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;42;1192.215,616.5231;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;1218.916,322.7668;Inherit;False;Property;_CloudHeight;Cloud Height;17;0;Create;True;0;0;0;False;0;False;0;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;1191.515,511.3231;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;28;1003.602,382.5226;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;58;1434.505,251.8495;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;1491.239,416.3779;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;40;1522.068,556.18;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;54;1774.797,857.5541;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;29;1830.46,132.4321;Inherit;False;Property;_CloudColor;Cloud Color;12;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.7924528,0.7924528,0.7924528,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;1955.817,729.2242;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;1703.991,399.3577;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2467.479,230.5251;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;CloudPlane;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;47.6;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;5;-1;-1;0;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;4;1
WireConnection;5;1;4;3
WireConnection;18;0;19;0
WireConnection;18;1;15;0
WireConnection;7;0;5;0
WireConnection;7;1;11;0
WireConnection;17;0;16;0
WireConnection;17;1;19;0
WireConnection;6;0;10;0
WireConnection;6;1;5;0
WireConnection;20;0;17;0
WireConnection;20;1;6;0
WireConnection;21;0;7;0
WireConnection;21;1;18;0
WireConnection;3;0;1;0
WireConnection;3;1;21;0
WireConnection;2;0;1;0
WireConnection;2;1;20;0
WireConnection;49;0;47;0
WireConnection;50;0;49;0
WireConnection;50;1;48;4
WireConnection;13;0;2;0
WireConnection;13;1;3;0
WireConnection;23;0;13;0
WireConnection;51;0;50;0
WireConnection;34;0;23;0
WireConnection;34;3;27;0
WireConnection;38;2;62;0
WireConnection;38;3;39;0
WireConnection;52;0;51;0
WireConnection;52;1;53;0
WireConnection;55;0;52;0
WireConnection;55;1;56;0
WireConnection;42;0;38;0
WireConnection;28;0;34;0
WireConnection;60;0;28;0
WireConnection;60;1;61;0
WireConnection;40;0;28;0
WireConnection;40;1;41;0
WireConnection;40;2;42;0
WireConnection;54;0;55;0
WireConnection;57;0;40;0
WireConnection;57;1;54;0
WireConnection;59;0;58;0
WireConnection;59;1;60;0
WireConnection;0;0;29;0
WireConnection;0;9;57;0
WireConnection;0;11;59;0
ASEEND*/
//CHKSM=B24AA3F8801093E507E677B5A5A1F073AB138376