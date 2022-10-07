In a basic situation to make shader GeomBlend compliant follow steps below:

1. find line "#pragma surface..."
2. put "decal:blend" at the end of line above 
3. find "struct Input {"
4. add "fixed4 color:COLOR;" at the end of struct ( before "}" )
5. find "void surf" function
6. put

	o.Alpha=1-IN.color.a;
	#if defined(UNITY_PASS_PREPASSFINAL)
		o.Smoothness*=o.Alpha;
	#endif
		
	at the end of it

Refer to example GeometryBlend_BumpedDiffuseOverlay.shader (made from BumpedDiffuseOverlay.shader) - I put comments there that reflect above instructions.

As one may see - we're using vertex color A component to make blending, so you can't make any surface shader GeomBlend compliant, but only these which doesn't use vertex color A.

To know more about making GeomBlend shaders (for people with at least basic shader coding knownledge) - refer to the code of all shaders starting with "GeomBlend_"

NOTE:
One of shaders in this folder (GeometryBlend_BumpedDiffuseOverlay.shader) is GeomBlend compliant version of a shader I've made for Michael O. aka Killstar aka Manufactura 4K - so - you might use it to blend his cool models with RTP Terrains/Arbitrary mesh objects.

