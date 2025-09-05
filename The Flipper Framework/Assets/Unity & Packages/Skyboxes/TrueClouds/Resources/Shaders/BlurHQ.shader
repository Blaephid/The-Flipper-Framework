Shader "Hidden/Clouds/BlurHQ" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_CameraDepth("Camera Depth Texture", 2D) = "white" {}
		_BlurSize("BlurSize", Range(0, 20)) = 0
		_LateCutThreshohld("Threshold", Range(0, 0.7)) = 0.2
		_LateCutPower("Threshold", Range(1, 3)) = 2
		_DepthFilteringPower("Depth Filtering Power", Range(0, 2)) = 0.5
	}

	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		Pass {
			CGPROGRAM
			#pragma multi_compile DEPTH_FILTERING_ON DEPTH_FILTERING_OFF
            #pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 
			#pragma vertex vert
			#pragma fragment frag 
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "CloudsDepth.cginc" 

			sampler2D _MainTex;
			sampler2D _CameraDepth;
			float _BlurSize;
			float _DepthFilteringPower;

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
				float depth = _BlurSize * 0.0007 / 2 *_ScreenParams.y / _ScreenParams.x;

				#ifdef DEPTH_FILTERING_ON
					half depthC = pow(DecodeDepth(tex2D(_CameraDepth, screenPos)), _DepthFilteringPower);
					depth /= depthC;
				#endif

				//horizontal 
				half4 sum = half4(0, 0, 0 ,0);  
				half4 color21 = tex2D( _MainTex, float2(screenPos.x-10.0 * depth, screenPos.y));
				half4 color20 = tex2D( _MainTex, float2(screenPos.x+10.0 * depth, screenPos.y));
				half4 color19 = tex2D( _MainTex, float2(screenPos.x-9.0  * depth, screenPos.y));
				half4 color18 = tex2D( _MainTex, float2(screenPos.x+9.0  * depth, screenPos.y));
				half4 color17 = tex2D( _MainTex, float2(screenPos.x-8.0  * depth, screenPos.y));
				half4 color16 = tex2D( _MainTex, float2(screenPos.x+8.0  * depth, screenPos.y));
				half4 color15 = tex2D( _MainTex, float2(screenPos.x-7.0  * depth, screenPos.y));
				half4 color14 = tex2D( _MainTex, float2(screenPos.x+7.0  * depth, screenPos.y));
				half4 color13 = tex2D( _MainTex, float2(screenPos.x-6.0  * depth, screenPos.y));
				half4 color12 = tex2D( _MainTex, float2(screenPos.x+6.0  * depth, screenPos.y));
				half4 color11 = tex2D( _MainTex, float2(screenPos.x-5.0  * depth, screenPos.y));
				half4 color10 = tex2D( _MainTex, float2(screenPos.x+5.0  * depth, screenPos.y));
				half4 color09 = tex2D( _MainTex, float2(screenPos.x-4.0  * depth, screenPos.y));
				half4 color08 = tex2D( _MainTex, float2(screenPos.x+4.0  * depth, screenPos.y));
				half4 color07 = tex2D( _MainTex, float2(screenPos.x-3.0  * depth, screenPos.y));
				half4 color06 = tex2D( _MainTex, float2(screenPos.x+3.0  * depth, screenPos.y));
				half4 color05 = tex2D( _MainTex, float2(screenPos.x-2.0  * depth, screenPos.y));
				half4 color04 = tex2D( _MainTex, float2(screenPos.x+2.0  * depth, screenPos.y));
				half4 color03 = tex2D( _MainTex, float2(screenPos.x-       depth, screenPos.y));
				half4 color02 = tex2D( _MainTex, float2(screenPos.x+       depth, screenPos.y));
				half4 color01 = tex2D( _MainTex, float2(screenPos.x             , screenPos.y));

				//formula: y^2 = 0.9; x + x*2 * (y + y ^ 2 + ... + y ^ 10) = 1
				sum.xyz += color21.xyz * 0.03658;
				sum.xyz += color20.xyz * 0.03658;
				sum.xyz += color19.xyz * 0.03856;
				sum.xyz += color18.xyz * 0.03856;
				sum.xyz += color17.xyz * 0.04064;
				sum.xyz += color16.xyz * 0.04064;
				sum.xyz += color15.xyz * 0.04284;
				sum.xyz += color14.xyz * 0.04284;
				sum.xyz += color13.xyz * 0.04516;
				sum.xyz += color12.xyz * 0.04516;
				sum.xyz += color11.xyz * 0.04760;
				sum.xyz += color10.xyz * 0.04760;
				sum.xyz += color09.xyz * 0.05018;
				sum.xyz += color08.xyz * 0.05018;
				sum.xyz += color07.xyz * 0.05289;
				sum.xyz += color06.xyz * 0.05289;
				sum.xyz += color05.xyz * 0.05575;
				sum.xyz += color04.xyz * 0.05575;
				sum.xyz += color03.xyz * 0.05877;
				sum.xyz += color02.xyz * 0.05877;
				sum.xyz += color01.xyz * 0.06195;

				//formula: y^2 = 0.75; x + x*2 * (y + y ^ 2 + ... + y ^ 10)  = 1
				sum.w += color21.w * 0.0218506969838;
				sum.w += color20.w * 0.0218506969838;
				sum.w += color19.w * 0.0252310115712;
				sum.w += color18.w * 0.0252310115712;
				sum.w += color17.w * 0.0291342626451;
				sum.w += color16.w * 0.0291342626451;
				sum.w += color15.w * 0.0336413487616;
				sum.w += color14.w * 0.0336413487616;
				sum.w += color13.w * 0.0388456835268;
				sum.w += color12.w * 0.0388456835268;
				sum.w += color11.w * 0.0448551316821;
				sum.w += color10.w * 0.0448551316821;
				sum.w += color09.w * 0.0517942447024;
				sum.w += color08.w * 0.0517942447024;
				sum.w += color07.w * 0.0598068422428;
				sum.w += color06.w * 0.0598068422428;
				sum.w += color05.w * 0.0690589929365;
				sum.w += color04.w * 0.0690589929365;
				sum.w += color03.w * 0.0797424563237;
				sum.w += color02.w * 0.0797424563237;
				sum.w += color01.w * 0.0920786572486;
				
				if (tex2D( _MainTex, float2(screenPos.x, screenPos.y)).w < 0.01) 
				{
					//sum.w = 0;
				}

				return sum;

			}
			ENDCG
		}
        
		Pass {
			CGPROGRAM
			#pragma multi_compile DEPTH_FILTERING_ON DEPTH_FILTERING_OFF
            #pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH
			#pragma vertex vert
			#pragma fragment frag 
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "CloudsDepth.cginc" 

			sampler2D _MainTex;
			sampler2D _CameraDepth;
			float _BlurSize;
			float _LateCutThreshohld;
			float _LateCutPower;
			float _DepthFilteringPower;

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

			half4 frag(v2f i) : COLOR
			{
				float2 screenPos = i.uv;
				float depth = _BlurSize * 0.0007 / 2;
				#ifdef DEPTH_FILTERING_ON
					half depthC = pow(DecodeDepth(tex2D(_CameraDepth, screenPos)), _DepthFilteringPower);
					depth /= depthC;
				#endif
    
				half4 sum = half4(0, 0, 0, 0);
  
				half4 color21 = tex2D( _MainTex, float2(screenPos.x, screenPos.y+10.0 * depth));
				half4 color20 = tex2D( _MainTex, float2(screenPos.x, screenPos.y-10.0 * depth));
				half4 color19 = tex2D( _MainTex, float2(screenPos.x, screenPos.y+9.0  * depth));
				half4 color18 = tex2D( _MainTex, float2(screenPos.x, screenPos.y-9.0  * depth));
				half4 color17 = tex2D( _MainTex, float2(screenPos.x, screenPos.y+8.0  * depth));
				half4 color16 = tex2D( _MainTex, float2(screenPos.x, screenPos.y-8.0  * depth));
				half4 color15 = tex2D( _MainTex, float2(screenPos.x, screenPos.y+7.0  * depth));
				half4 color14 = tex2D( _MainTex, float2(screenPos.x, screenPos.y-7.0  * depth));
				half4 color13 = tex2D( _MainTex, float2(screenPos.x, screenPos.y+6.0  * depth));
				half4 color12 = tex2D( _MainTex, float2(screenPos.x, screenPos.y-6.0  * depth));
				half4 color11 = tex2D( _MainTex, float2(screenPos.x, screenPos.y+5.0  * depth));
				half4 color10 = tex2D( _MainTex, float2(screenPos.x, screenPos.y-5.0  * depth));
				half4 color09 = tex2D( _MainTex, float2(screenPos.x, screenPos.y+4.0  * depth));
				half4 color08 = tex2D( _MainTex, float2(screenPos.x, screenPos.y-4.0  * depth));
				half4 color07 = tex2D( _MainTex, float2(screenPos.x, screenPos.y+3.0  * depth));
				half4 color06 = tex2D( _MainTex, float2(screenPos.x, screenPos.y-3.0  * depth));
				half4 color05 = tex2D( _MainTex, float2(screenPos.x, screenPos.y+2.0  * depth));
				half4 color04 = tex2D( _MainTex, float2(screenPos.x, screenPos.y-2.0  * depth));
				half4 color03 = tex2D( _MainTex, float2(screenPos.x, screenPos.y-       depth));
				half4 color02 = tex2D( _MainTex, float2(screenPos.x, screenPos.y+       depth));
				half4 color01 = tex2D( _MainTex, float2(screenPos.x, screenPos.y             ));

				//vertical
				sum.xyz += color21.xyz * 0.03658;
				sum.xyz += color20.xyz * 0.03658;
				sum.xyz += color19.xyz * 0.03856;
				sum.xyz += color18.xyz * 0.03856;
				sum.xyz += color17.xyz * 0.04064;
				sum.xyz += color16.xyz * 0.04064;
				sum.xyz += color15.xyz * 0.04284;
				sum.xyz += color14.xyz * 0.04284;
				sum.xyz += color13.xyz * 0.04516;
				sum.xyz += color12.xyz * 0.04516;
				sum.xyz += color11.xyz * 0.04760;
				sum.xyz += color10.xyz * 0.04760;
				sum.xyz += color09.xyz * 0.05018;
				sum.xyz += color08.xyz * 0.05018;
				sum.xyz += color07.xyz * 0.05289;
				sum.xyz += color06.xyz * 0.05289;
				sum.xyz += color05.xyz * 0.05575;
				sum.xyz += color04.xyz * 0.05575;
				sum.xyz += color03.xyz * 0.05877;
				sum.xyz += color02.xyz * 0.05877;
				sum.xyz += color01.xyz * 0.06195;
				
				sum.w += color21.w * 0.0218506969838;
				sum.w += color20.w * 0.0218506969838;
				sum.w += color19.w * 0.0252310115712;
				sum.w += color18.w * 0.0252310115712;
				sum.w += color17.w * 0.0291342626451;
				sum.w += color16.w * 0.0291342626451;
				sum.w += color15.w * 0.0336413487616;
				sum.w += color14.w * 0.0336413487616;
				sum.w += color13.w * 0.0388456835268;
				sum.w += color12.w * 0.0388456835268;
				sum.w += color11.w * 0.0448551316821;
				sum.w += color10.w * 0.0448551316821;
				sum.w += color09.w * 0.0517942447024;
				sum.w += color08.w * 0.0517942447024;
				sum.w += color07.w * 0.0598068422428;
				sum.w += color06.w * 0.0598068422428;
				sum.w += color05.w * 0.0690589929365;
				sum.w += color04.w * 0.0690589929365;
				sum.w += color03.w * 0.0797424563237;
				sum.w += color02.w * 0.0797424563237;
				sum.w += color01.w * 0.0920786572486;

				// a little trick to blur only inwards
				sum.w = clamp((sum.w - _LateCutThreshohld) / (1 - _LateCutThreshohld), 0, 1);
				sum.w = pow(sum.w, _LateCutPower);

				return sum;

			}
			ENDCG
		}
	}
}
