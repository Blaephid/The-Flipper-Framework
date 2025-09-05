Shader "Hidden/Clouds/DepthBlur" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalTex("Albedo (RGB)", 2D) = "white" {}
		_BlurSize("BlurSizeXY", Range(0,20)) = 0
		_Noise("Noise", 2D) = "white" {}
		_NoiseSinTime("NoiseSinTime", Vector) = (2, 1, 3, 1)
		_NoiseParams("NoiseParams", Vector) = (2, 1, 3, 1)
		_FallbackDist("FallbackDistance", Float) = 1
	}

	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }

		Pass{
			CGPROGRAM
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#pragma vertex vert
			#pragma fragment frag 
			#include "UnityCG.cginc"
			#include "CloudsDepth.cginc" 
			#pragma target 3.0

			sampler2D _MainTex; 
			sampler2D _NormalTex;
			float _BlurSize;

			sampler2D _Noise;
			half4 _NoiseParams;
			float _DepthNoisePower;
			half3 _NoiseSinTime;

			uniform float4x4 _CameraRotation;
			float4 _CameraDirLD; // left-down
			float4 _CameraDirRD; // right-down
			float4 _CameraDirLU; // left-up
			float4 _CameraDirRU; // right-up
			float _NearPlane;
			float _FarPlane;

			inline half3 noise(half3 pos, half3 normal)
			{
				pos += _Time.r * _NoiseParams.xyz;
				pos *= _NoiseParams.w;
				return (
					tex2D(_Noise, pos.xy) * normal.z * normal.z +
					tex2D(_Noise, pos.yz) * normal.x * normal.x +
					tex2D(_Noise, pos.xz) * normal.y * normal.y -
					.5
					) * _DepthNoisePower;
			}

			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				float4 viewDir : NORMAL;
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);

				float4 down = _CameraDirRD * o.uv.x + _CameraDirLD * (1 - o.uv.x);
				float4 up = _CameraDirRU * o.uv.x + _CameraDirLU * (1 - o.uv.x);
				o.viewDir = up * o.uv.y + down * (1 - o.uv.y);;
				return o;
			}

			half4 frag(v2f i) : COLOR
			{ 
				float2 screenPos = i.uv;
				half4 undecodedDepth = tex2D(_MainTex, screenPos);
				float depth = DecodeDepth(undecodedDepth);
				if (depth < 0.99)
				{
					depth *= _FarPlane;
					float4 pos1 = i.viewDir * depth;
					pos1.w = 1;
					pos1 = mul(_CameraRotation, pos1);

					half3 normal = normalize(tex2D(_NormalTex, screenPos).xyz - .5);
					float3 noiseForPoint = noise(pos1.xyz, normal).xyz;

					depth +=
						noiseForPoint.x * _NoiseSinTime.r + 
						noiseForPoint.y * _NoiseSinTime.g +
						noiseForPoint.z * _NoiseSinTime.b;

					depth /= _FarPlane;
					depth = clamp(depth, 0, 1);
					return half4(EncodeDepth(depth), 1);
				}
				return undecodedDepth;
			}
			ENDCG
		}

		Pass {

			CGPROGRAM
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#pragma vertex vert
			#pragma fragment frag 
			#include "UnityCG.cginc" 
			#include "CloudsDepth.cginc" 
			#pragma target 3.0

			sampler2D _MainTex;
			float _BlurSize;
			float _FallbackDist;

			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o;
			}

			half4 frag( v2f i ) : COLOR
			{
				float2 screenPos = i.uv;
				float depth= _BlurSize * 0.0007 * _ScreenParams.y / _ScreenParams.x;

				//horizontal 

				float sum = 1;

				//formula: y = 0.9; x + x*2 * (y + y ^ 2 + ... + y ^ 5)  = 1
				sum =            DecodeDepth(tex2D(_MainTex, float2(screenPos.x              , screenPos.y)));
				//sum = min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x - 1.0 * depth, screenPos.y))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x + 1.0 * depth, screenPos.y))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x - 2.0 * depth, screenPos.y))));
				//sum = min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x + 2.0 * depth, screenPos.y))));
				//sum = min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x - 3.0 * depth, screenPos.y))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x + 3.0 * depth, screenPos.y))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x - 4.0 * depth, screenPos.y))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x + 4.0 * depth, screenPos.y))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x - 5.0 * depth, screenPos.y))));
				//sum = min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x + 5.0 * depth, screenPos.y))));

				return fixed4(EncodeDepth(sum), 1);

			}
			ENDCG
        }
        
        Pass {
			CGPROGRAM
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#pragma vertex vert
			#pragma fragment frag 
			#include "UnityCG.cginc"
			#include "CloudsDepth.cginc" 
			#pragma target 3.0

			sampler2D _MainTex;
			float _BlurSize;
			half _FallbackDist;

			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o;
			}

			half4 frag( v2f i ) : COLOR
			{

				float2 screenPos = i.uv;
				float depth= _BlurSize*0.0007;

				//vertical

				float sum = 1;

				//vertical
				
				sum =            DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y              )));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y + 1.0 * depth))));
				//sum = min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y - 1.0 * depth))));
				//sum = min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y + 2.0 * depth))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y - 2.0 * depth))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y + 3.0 * depth))));
				//sum = min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y - 3.0 * depth))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y + 4.0 * depth))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y - 4.0 * depth))));
				//sum = min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y + 5.0 * depth))));
				sum =   min(sum, DecodeDepth(tex2D(_MainTex, float2(screenPos.x, screenPos.y - 5.0 * depth))));
				
				return fixed4(EncodeDepth(sum), 1);

			}
			ENDCG
        }
	} 
}
