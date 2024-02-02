Shader "Tri-Planar World Normal" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_Side("Side", 2D) = "gray" {}
		_Top("Top", 2D) = "gray" {}
		_Bottom("Bottom", 2D) = "gray" {}
		_SideScale("Side Scale", Float) = 2
		_TopScale("Top Scale", Float) = 2
		_BottomScale("Bottom Scale", Float) = 2
		_BumpMapSide("Side Normal Map", 2D) = "bump" {}
		_BumpMapTop("Top Normal Map", 2D) = "bump" {}
		_BumpMapBottom("Bottom Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Scale", Range(0,1)) = 1.0
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}

	SubShader{
		Tags{
			"Queue" = "Geometry"
			"IgnoreProjector" = "False"
			"RenderType" = "Opaque"
		}


		Cull Back
		ZWrite On

		CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma exclude_renderers flash

	struct Input {
		float3 worldPos;
		float3 worldNormal;

		float2 uv_Side;
		float2 uv_BumpMapSide;

		INTERNAL_DATA

	};

	sampler2D _Side, _Top, _Bottom;
	sampler2D _BumpMapSide, _BumpMapTop, _BumpMapBottom;

	float _SideScale, _TopScale, _BottomScale;
	half _Glossiness;
	half _Metallic;
	half _BumpScale;
	fixed4 _Color;

	sampler2D _NormalMap;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		o.Normal = float3(0, 0, 1);
		float3 n = WorldNormalVector(IN, o.Normal);
		float3 projNormal = saturate(pow(n * 1.4, 4));
		float2 invertY = float2(1, -1);

		//Adjustments//
		fixed3 dY;
		fixed3 dZ;
		fixed3 dX;
		fixed4 cZ;
		fixed4 cX;
		fixed4 cY;


		// SIDE X
		float3 x = tex2D(_Side, frac(IN.worldPos.zy * _SideScale)) * abs(n.x);
		//NORMAL//
		dX = UnpackNormal(tex2D(_BumpMapSide, frac(IN.worldPos.zy * _SideScale)) * abs(n.x));

		// TOP / BOTTOM
		float3 y = 0;
		if (n.y > 0) {
			y = tex2D(_Top, frac(IN.worldPos.zx * _TopScale)) * abs(n.y);
			//NORMAL//
			dY = UnpackNormal(tex2D(_BumpMapTop, frac(IN.worldPos.zx * _TopScale)) * abs(n.y));
		}
		else {
			y = tex2D(_Bottom, frac(IN.worldPos.zx * _BottomScale)) * abs(n.y);
			//NORMAL//
			dY = UnpackNormal(tex2D(_BumpMapBottom, frac(IN.worldPos.zx * _BottomScale)) * abs(n.y));
		}

		// SIDE Z    
		float3 z = tex2D(_Side, frac(IN.worldPos.xy * _SideScale)) * abs(n.z);
		//NORMAL//
		dZ = UnpackNormal(tex2D(_BumpMapSide, frac(IN.worldPos.xy * _SideScale)) * abs(n.z));


		// Add Normal//
		half3 tempBump = dZ;
		tempBump = lerp(tempBump, dX, projNormal.x);
		tempBump = lerp(tempBump, dY, projNormal.y);
		half3 resultBump = normalize(tempBump);

		//Add Color//
		cZ.rgb = z * _Color.rgb;
		cX.rgb = x * _Color.rgb;
		cY.rgb = y * _Color.rgb;

		o.Albedo = cZ.rgb;
		o.Albedo = lerp(o.Albedo, cX.rgb, projNormal.x); 
		o.Albedo = lerp(o.Albedo, cY.rgb, projNormal.y);

		o.Normal = lerp(o.Normal, resultBump, _BumpScale);

		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
	}
	ENDCG
	}
	Fallback "Diffuse"
}