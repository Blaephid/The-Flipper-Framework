//Add following line if you inlude this file:
//#pragma multi_compile HIGH_RES_DEPTH MEDIUM_RES_DEPTH 

#ifdef MEDIUM_RES_DEPTH 
inline half3 EncodeDepth(float value)
{
	float2 enc = float2(value, value * float(256));
	enc.y = frac(enc.y);
	enc.x -= enc.y / float(256);
	return half3(enc, 0);
}

inline float DecodeDepth(float4 value) {
	return value.x + value.y / float(256);
}
#endif

#ifdef HIGH_RES_DEPTH
inline half3 EncodeDepth(float value)
{
	float3 enc = float3(value, value * 256, value * 65536);
	enc.yz = frac(enc.yz);
	enc.x -= enc.y / float(256);
	enc.y -= enc.z / float(256);
	return enc;
}

inline float DecodeDepth(float4 value) {
	return value.x + value.y / float(256) + value.z / float(65536);
}
#endif