#include "../CloudsDepth.cginc"
#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
			
sampler2D _MainTex;
uniform sampler2D _CameraDepth;
fixed4 _TintColor;
float _MaxDistance;
 
struct appdata_t {
	float4 vertex : POSITION;
	fixed4 color : COLOR;
	float2 texcoord : TEXCOORD0;
};

struct v2f {
	fixed4 color : COLOR;
	half2 uv : TEXCOORD0;
	float4 screenPos : TEXCOORD1;
	float depth : TEXCOORD2;
    half4 pos : POSITION;
};
 
v2f vert (appdata_t v) {
    v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o); 
    o.pos = UnityObjectToClipPos(v.vertex);
	o.screenPos = ComputeScreenPos(o.pos);
	COMPUTE_EYEDEPTH(o.depth);
	o.uv = v.texcoord;
	o.color = v.color;
    return o;
}
			
half4 frag( v2f i ) : COLOR {
	float deltaDepth = abs(DecodeDepth(tex2D(_CameraDepth, i.screenPos.xy / i.screenPos.w)) * _ProjectionParams.z - i.depth);
	deltaDepth = saturate(1 - deltaDepth / _MaxDistance);

	return tex2D(_MainTex, i.uv) * _TintColor * i.color * deltaDepth;
}	