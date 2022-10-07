#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation vertex shader
struct InternalTessInterp_appdata {
  float4 vertex : INTERNALTESSPOS;
  float4 tangent : TANGENT;
  float3 normal : NORMAL;
  float2 texcoord : TEXCOORD0;
  float2 texcoord1 : TEXCOORD1;
  float2 texcoord2 : TEXCOORD2;
  half4 color : COLOR;
};
InternalTessInterp_appdata tessvert_surf (appdata v) {
  InternalTessInterp_appdata o;
  o.vertex = v.vertex;
  o.tangent = v.tangent;
  o.normal = v.normal;
  o.texcoord = v.texcoord;
  o.texcoord1 = v.texcoord1;
  o.texcoord2 = v.texcoord2;
  o.color = v.color;
  return o;
}

// tessellation hull constant shader
UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata,3> v) {
  UnityTessellationFactors o;
  float4 tf;
  appdata vi[3];
  vi[0].vertex = v[0].vertex;
  vi[0].tangent = v[0].tangent;
  vi[0].normal = v[0].normal;
  vi[0].texcoord = v[0].texcoord;
  vi[0].texcoord1 = v[0].texcoord1;
  vi[0].texcoord2 = v[0].texcoord2;
  vi[0].color = v[0].color;
  vi[1].vertex = v[1].vertex;
  vi[1].tangent = v[1].tangent;
  vi[1].normal = v[1].normal;
  vi[1].texcoord = v[1].texcoord;
  vi[1].texcoord1 = v[1].texcoord1;
  vi[1].texcoord2 = v[1].texcoord2;
  vi[1].color = v[1].color;
  vi[2].vertex = v[2].vertex;
  vi[2].tangent = v[2].tangent;
  vi[2].normal = v[2].normal;
  vi[2].texcoord = v[2].texcoord;
  vi[2].texcoord1 = v[2].texcoord1;
  vi[2].texcoord2 = v[2].texcoord2;
  vi[2].color = v[2].color;
  tf = tessEdge(vi[0], vi[1], vi[2]);
  o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
  return o;
}

// tessellation hull shader
[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
InternalTessInterp_appdata hs_surf (InputPatch<InternalTessInterp_appdata,3> v, uint id : SV_OutputControlPointID) {
  return v[id];
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float2 pack0 : TEXCOORD1; // _Control
  half3 worldNormal : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
};
float4 _Control_ST;

// vertex shader
v2f_surf vert_surf (appdata v) {
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _Control);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  o.worldNormal = worldNormal;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation domain shader
[UNITY_domain("tri")]
v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata,3> vi, float3 bary : SV_DomainLocation) {
  appdata v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  float3 pp[3];
  for (int i = 0; i < 3; ++i)
    pp[i] = v.vertex.xyz - vi[i].normal * (dot(v.vertex.xyz, vi[i].normal) - dot(vi[i].vertex.xyz, vi[i].normal));
  v.vertex.xyz = _Phong * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-_Phong) * v.vertex.xyz;
  v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
  v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
  v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
  v.texcoord2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
  v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;
  vert (v);
  v2f_surf o = vert_surf (v);
  return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_Control = IN.pack0.xy;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  surfIN.worldNormal = 0.0;
  surfIN.worldPos = worldPos;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}