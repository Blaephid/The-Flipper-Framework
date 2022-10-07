//
// Relief Terrain  - vertex color blend - 2 Parallax mapped material with height blending
// (note that shader uses A heightmap channel, so your heightmaps can't be just DXT1 RGB textures !)
// Tomasz Stobierski 2013-2016
//
// (we're using R vertex channel to blend between 1st and 2nd material)
//
Shader "Relief Pack - Bonus Shaders/Legacy BumpedSpecular/2 Layers VertexPaint" {
    Properties {
		_SpecColor ("Specular Color (RGBA)", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.5
		_ExtrudeHeight ("Extrude Height", Range(0.001,0.1)) = 0.04
		
		_MainTex ("Texture A", 2D) = "white" {}
		_BumpMap ("Bumpmap A", 2D) = "bump" {}
		_HeightMap ("Heightmap A", 2D) = "black" {}
		
		_MainTex2 ("Texture B", 2D) = "white" {}
		_BumpMap2 ("Bumpmap B", 2D) = "bump" {}
		_HeightMap2 ("Heightmap B", 2D) = "black" {}
    }
    SubShader {
	Tags { "RenderType" = "Opaque" }
	CGPROGRAM
	#pragma surface surf BlinnPhong
	#pragma exclude_renderers d3d11_9x gles
	#pragma target 3.0
	
	struct Input {
		float2 uv_MainTex;
		float3 viewDir;
		
		float4 color:COLOR;
	};
	
	float _Shininess;
	float _ExtrudeHeight;
	
	sampler2D _MainTex;
	sampler2D _BumpMap;
	sampler2D _HeightMap;
	sampler2D _MainTex2;
	sampler2D _BumpMap2;
	sampler2D _HeightMap2;
	
	void surf (Input IN, inout SurfaceOutput o) {
      	float2 tH;
      	tH.x=tex2D(_HeightMap, IN.uv_MainTex).a;
      	tH.y=tex2D(_HeightMap2, IN.uv_MainTex).a;
      	float2 uv=IN.uv_MainTex + ParallaxOffset(tH.x, _ExtrudeHeight, IN.viewDir.xyz);
      	float2 uv2=IN.uv_MainTex + ParallaxOffset(tH.y, _ExtrudeHeight, IN.viewDir.xyz);
      	float2 control=float2(IN.color.r, 1-IN.color.r);
      	control*=(tH+0.01);
      	control*=control;
      	control*=control;
      	control*=control;
      	control/=dot(control, 1);
      	
      	fixed4 col=tex2D(_MainTex, uv);
      	fixed4 col2=tex2D(_MainTex2, uv2);
      	o.Albedo=control.x*col.rgb + control.y*col2.rgb;
      	o.Gloss=control.x*col.a + control.y*col2.a;
      	o.Normal=control.x*UnpackNormal(tex2D (_BumpMap, uv));
      	o.Normal+=control.y*UnpackNormal(tex2D (_BumpMap2, uv2));
      	//o.Normal=normalize(o.Normal);
      	o.Specular=_Shininess;
	}
	ENDCG
      
    } 
    Fallback "Parallax Specular"
}
