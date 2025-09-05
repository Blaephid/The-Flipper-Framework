Shader "Hidden/Clouds/Clouds" {
	Properties {
		_MainTex ("Render Input", 2D) = "white" {}
		_WorldDepth ("Depth Texture", 2D) = "white" {}
		_CameraDepth ("Camera Depth Texture", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_NoiseParams("NoiseParams", Vector) = (2, 1, 3, 1)
		_NormalNoisePower("NormalNoisePower", Float) = 1
		_DisplacementNoisePower("DisplacementNoisePower", Float) = 1
		_Ramp ("Ramp", 2D) = "white" {}
		_BlurSize("BlurSizeXY", Range(0,20)) = 0
		_MainColor("main color", COLOR) = (1, 1, 1, 1)
		_ShadowColor("shadow color", COLOR) = (1, 1, 1, 1)
		_LightEnd("light end", Range(0, 1)) = 0.75
		_LightDir("Light Direction", Vector) = (1, 1, 1, 1)
		_LightPos("Light PositionOnScreen", Vector) = (1, 1, 1, 1)
		_FallbackDist("Fallback distance", Range(0, 10)) = 0.5
		_HaloDistance("Halo Distance", Range(0, 10)) = 1
		_HaloPower("Halo Power", Range(0, 10)) = 1
	}

	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }

		//EARLY CUT
		Pass {
			CGPROGRAM
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "CloudsDepth.cginc" 

			sampler2D _MainTex;
			sampler2D _CameraDepth;
			sampler2D _WorldDepth;

			half _FallbackDist;

			float _FarPlane;

			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				float2 fallback : TEXCOORD1;
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				o.fallback = float2(_FarPlane, 1 / _FallbackDist);
				return o;
			}

			half4 frag(v2f IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.uv);

				half depthC = DecodeDepth(tex2D(_CameraDepth, IN.uv)) * IN.fallback.x;
				half depthW = DecodeDepth(tex2D(_WorldDepth, IN.uv)) * IN.fallback.x;

				c.a *= (1 - saturate((depthC + _FallbackDist - depthW) * IN.fallback.y));

				return c;
			}
			ENDCG
		}

		//COLORING WITH INTERPOLATION
		Pass {
			CGPROGRAM
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#pragma multi_compile EARLY_CUT LATE_CUT
			#pragma multi_compile NOISE_ON NOISE_OFF
			#pragma multi_compile HALO_ON HALO_OFF
			#pragma multi_compile RAMP_ON RAMP_OFF
			#include "CloudsDepth.cginc" 
			#include "UnityCG.cginc"
			#pragma vertex vert 
			#pragma fragment frag
			
			sampler2D _MainTex;
			sampler2D _Ramp;
			sampler2D _CameraDepth;
			sampler2D _WorldDepth;
			sampler2D _Noise;

			half4 _NoiseParams;
			float _NormalNoisePower;
			float _DisplacementNoisePower;

			float4 _MainColor;
			float4 _ShadowColor;
			float _LightEnd;
			float4 _LightDir;
			float4 _LightPos;
			float _HaloPower;
			float _HaloDistance;
			half _FallbackDist;
				
			uniform float4x4 _CameraRotation;
			float4 _CameraDirLD; // left-down
			float4 _CameraDirRD; // right-down
			float4 _CameraDirLU; // left-up
			float4 _CameraDirRU; // right-up

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
					);
			} 

			struct v2f { 
				float4 pos : SV_POSITION; 
				half2 uv : TEXCOORD0; 
				float4 viewDir : NORMAL;
			};
								
			v2f vert( appdata_img v ) { 
				v2f o; 
				o.pos = UnityObjectToClipPos (v.vertex); 
				o.uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord ); 
					
				float4 down = _CameraDirRD * o.uv.x + _CameraDirLD * (1 - o.uv.x); 
				float4 up   = _CameraDirRU * o.uv.x + _CameraDirLU * (1 - o.uv.x);
				o.viewDir = up * o.uv.y + down * (1 - o.uv.y);
				return o; 
			}
			
			half4 frag(v2f IN) : COLOR {
				half4 c1 = tex2D(_MainTex, IN.uv);
				
				half3 normal;
				#ifdef NOISE_ON
					float depthCRange1 = DecodeDepth(tex2D(_CameraDepth, IN.uv)) * _FarPlane;
					float4 pos1 = IN.viewDir * depthCRange1;
					pos1.w = 1;
					pos1 = mul(_CameraRotation, pos1);
					normal = normalize(c1.xyz - .5);

					half3 noiseForPoint = noise(pos1.xyz, normal).xyz;
					
					//DEBUG: world position
					//return float4((float3(pos1.x - X, pos1.y - Y, pos1.z + Z) / R + 1) / 2, 1);

					//DEBUG: triplanar coefficients
					//return float4(normal.z * normal.z).xxx, 1);
					//return float4(normal.x * normal.x).xxx, 1);
					//return float4(normal.y * normal.y).xxx, 1);
					
					//DEBUG: noize
					//return float4((noiseForPoint + .5), c1.a);

					half2 dUV = noiseForPoint.xy / depthCRange1 * _DisplacementNoisePower * sqrt(c1.a);

					dUV.x *= _ScreenParams.y / _ScreenParams.x;
					float2 uv = IN.uv + dUV;

					c1 = tex2D(_MainTex, uv);
					normal = normalize(c1.xyz - .5);
					normal = normalize(normal + noiseForPoint * _NormalNoisePower);
				#else 
					normal = normalize(c1.xyz - .5);
				#endif

				half intensity = (dot(normal, normalize(_LightDir.xyz)) + _LightEnd) / (1 + _LightEnd);
				intensity = saturate(intensity);

				#ifdef RAMP_OFF
					half4 resColor = lerp(_ShadowColor, _MainColor, intensity);
				#else
					half4 resColor = tex2D(_Ramp, half2(intensity, 0));
				#endif

				#ifdef HALO_ON
					float depthWUnscaled = DecodeDepth(tex2D(_WorldDepth, IN.uv));

					if (depthWUnscaled > 0.99) {
						half2 dist = _LightPos;
						dist -= IN.uv.xy;
						dist.x *= _ScreenParams.x / _ScreenParams.y;
						dist.y *= _ProjectionParams.x;
						float haloDistance = _HaloDistance * 6;
						half tmp = clamp((haloDistance - length(dist)) / haloDistance, 0, 1);
						tmp *= tmp;
						tmp *= tmp * _HaloPower * _LightDir.w * (1 - pow(c1.a, 2));
						resColor = lerp(resColor, _MainColor, tmp);
						c1.a += c1.a * tmp;
					}
				#endif

				resColor.a = c1.a; 

				return resColor;
			}
			ENDCG
		}

		// Late Cut with alpha blending
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#pragma multi_compile EARLY_CUT LATE_CUT
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "CloudsDepth.cginc" 
			
			sampler2D _MainTex;
			sampler2D _CameraDepth;
			sampler2D _WorldDepth;

			half _FallbackDist;
				
			uniform float4x4 _CameraRotation;

			float _FarPlane;

			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				float2 fallback : TEXCOORD1;
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				o.fallback = float2(_FarPlane, 1 / _FallbackDist);
				return o;
			}

			half4 frag(v2f IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.uv);

				#ifdef LATE_CUT
					half depthC = DecodeDepth(tex2D(_CameraDepth, IN.uv)) * IN.fallback.x;
					half depthW = DecodeDepth(tex2D(_WorldDepth, IN.uv)) * IN.fallback.x;

					c.a *= (1 - saturate((depthC + _FallbackDist - depthW) * IN.fallback.y));
				#endif

				return c;
			}
			ENDCG
		}

		// Late Cut without alpha blending
		Pass {
			CGPROGRAM
			#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#pragma multi_compile EARLY_CUT LATE_CUT
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "CloudsDepth.cginc" 
			
			sampler2D _MainTex;
			sampler2D _CameraDepth;
			sampler2D _WorldDepth;

			half _FallbackDist;
				
			uniform float4x4 _CameraRotation;

			float _FarPlane;

			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				float2 fallback : TEXCOORD1;
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				o.fallback = float2(_FarPlane, 1 / _FallbackDist);
				return o;
			}

			half4 frag(v2f IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.uv);

				#ifdef LATE_CUT
					half depthC = DecodeDepth(tex2D(_CameraDepth, IN.uv)) * IN.fallback.x;
					half depthW = DecodeDepth(tex2D(_WorldDepth, IN.uv)) * IN.fallback.x;

					c.a *= (1 - saturate((depthC + _FallbackDist - depthW) * IN.fallback.y));
				#endif

				return c;
			}
			ENDCG
		}
		
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;

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

			half4 frag(v2f IN) : COLOR
			{
				return tex2D(_MainTex, IN.uv);
			}
			ENDCG
		}

	}
    CustomEditor "CloudMaterialEditor"
}
