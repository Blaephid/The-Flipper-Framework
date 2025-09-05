void MainLight_half(float3 WorldPos, out half3 Dir, out half3 Color, out half DistanceAtten, out half ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
   Dir = half3(0.5, 0.5, 0);
   Color = 1;
   DistanceAtten = 1;
   ShadowAtten = 1;
#else
   half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
   Light mainLight = GetMainLight(shadowCoord);
   Dir = mainLight.direction;
   Color = mainLight.color;
   DistanceAtten = mainLight.distanceAttenuation;
   ShadowAtten = mainLight.shadowAttenuation;
#endif
}