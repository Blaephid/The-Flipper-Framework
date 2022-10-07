//
// Material / shader customised for usage on planets
//
// (C) Tomasz Stobierski 2013-2016
//
//
Shader "Relief Pack - Bonus Shaders/Planet" {
	//
	// as RTP has so many features, number of material properties are huge (and not managed via script but default material property inspector)
	// for this reason commented out are those unused (disabled in shader code)
	// Uncomment properties needed and look for #defines section below to add/remove features manually
	// Sometimes they depends on each other and can't be used together, so beware (as you adjust them on your own, not using RTP_LODmanager)
	//
	Properties {
		//
		// analog to RTP Settings / Main
		//
		_TERRAIN_ReliefTransformTriplanarZ ("Triplanar tile size", Float) = 3
		// near distance
		_TERRAIN_distance_start ("Near distance fade start", Float) = 0
		_TERRAIN_distance_transition ("       Near distance fade length", Float) = 20
		_RTP_MIP_BIAS ("MIP Bias", Range(-1,1)) = 0
		// PM/POM (by defult we define RTP_SIMPLE_SHADING below, so it's not used)
		//_TERRAIN_ExtrudeHeight ("Parallax Mapping extrude height", Range(0.001,0.5)) = 0.06
		
/*
		RTP_AOsharpness ("Fake AO 2 HB sharpness", Range(0, 10)) = 1
		RTP_AOamp ("Fake AO 2 HB value", Range(0,2)) = 0.5
		RTP_AO_0123 ("       Per layer AO (XYZW - layers 0-3)", Vector) = (0.2, 0.2, 0.2, 0.2)	
*/	
		_occlusionStrength("Approximated occlusion", Range(0,1)) = 1
/*
		EmissionRefractFiltering ("Emission refraction filtering", Range(0, 8)) = 2
		EmissionRefractAnimSpeed ("Emission refraction anim speed", Range(0,20)) = 2
*/	
		
		_FColor ("Fog color day (RGB)", Color) = (1,1,1,1)
		_FColor2 ("Fog color sunset (RGB)", Color) = (1,0.8,0.4,1)
		_FColor3 ("Fog color night (RGB+A density)", Color) = (0.2,0.3,0.6,1)
		_Fdensity ("Fog density", Float) = 0.002
		_Fstart ("Fog linear start", Float) = 10
		_Fend ("Fog linear end", Float) = 200
		

		
		// detail textures / normal maps (combined), heightmap (combined)
		// you can use either atlas (to save 3 tex samplers)
//		[NoTilingOffset] _SplatAtlasA ("Detail atlas (RGB+A spec)", 2D) = "black" {}
		// or up to 4 textures (look for RTP_USE_COLOR_ATLAS define below)
		[NoScaleOffset] _SplatA0 ("Detailmap 0 (RGB+A spec)", 2D) = "black" {}
		[NoScaleOffset] _SplatA1 ("Detailmap 1 (RGB+A spec)", 2D) = "black" {}
		[NoScaleOffset] _SplatA2 ("Detailmap 2 (RGB+A spec)", 2D) = "black" {}
		[NoScaleOffset] _SplatA3 ("Detailmap 3 (RGB+A spec)", 2D) = "black" {}
		[NoScaleOffset] _BumpMap01 ("Bumpmap combined 0+1 (RG+BA)", 2D) = "grey" {}
		[NoScaleOffset] _BumpMap23 ("Bumpmap combined 2+3 (RG+BA)", 2D) = "grey" {}
		[NoScaleOffset] _TERRAIN_HeightMap ("Heightmap combined (RGBA - layers 0-3)", 2D) = "white" {}
		
		
		// per layer settings + PBL
		RTP_metallic0123 ("Layer metallic (XYZW - layers 0-3)", Vector) = (0, 0, 0, 0)
		RTP_glossMin0123 ("       gloss min", Vector) = (0, 0, 0, 0)
		RTP_glossMax0123 ("       gloss max", Vector) = (1, 1, 1, 1)
		
		_FarSpecCorrection0123 ("       far spec correction", Vector) = (0, 0, 0, 0)
		RTP_DiffFresnel0123 ("       diffuse scattering", Vector) = (0, 0, 0, 0)
		
		_LayerBrightness0123 ("          layer brightness", Vector) = (1, 1, 1, 1)		
		_LayerSaturation0123 ("          layer saturation", Vector) = (1, 1, 1, 1)		
		_MIPmult0123 ("       MIP offset at far distance", Vector) = (0,0,0,0)
		
/*
		_LayerEmission0123 ("Layer emission", Vector) = (0,0,0,0)
		_LayerEmissionColor0 ("       glow color0", Color) = (0.5,0.5,0.5,0)
		_LayerEmissionColor1 ("       glow color1", Color) = (0.5,0.5,0.5,0)
		_LayerEmissionColor2 ("       glow color2", Color) = (0.5,0.5,0.5,0)
		_LayerEmissionColor3 ("       glow color3", Color) = (0.5,0.5,0.5,0)
		_LayerEmissionRefractStrength0123 ("       hot air refract strength", Vector) = (0,0,0,0)
		_LayerEmissionRefractHBedge0123 ("          on layer edges only", Vector) = (0,0,0,0)
*/
	
		// UV blend
		_blend_multiplier ("UV blend multiplier", Range(0,1)) = 1
		// (0.2 means that one blended tile is 5 detail tiles)
		_MixScale0123 ("       UV blend tiling (XYZW - layers 0-3)", Vector) = (0.2, 0.2, 0.2, 0.2)
		_MixBlend0123 ("       UV blend value (XYZW - layers 0-3)", Vector) = (0.5, 0.5, 0.5, 0.5)
		_MixSaturation0123 ("       UV blend saturation (XYZW - layers 0-3)", Vector) = (1.0, 1.0, 1.0, 1.0)
		_MixBrightness0123 ("       UV blend brightness (XYZW - layers 0-3)", Vector) = (2.0, 2.0, 2.0, 2.0)
		_MixReplace0123 ("       UV blend replace (XYZW - layers 0-3)", Vector) = (0, 0, 0, 0)

		// global colormap
		[NoScaleOffset] _ColorMapGlobal ("Global colormap (RGBA)", 2D) = "white" {}
		_GlobalColorMapBlendValues ("       blending near/mid/far (XYZ)", Vector) = (0.3,0.6,0.8,0)
		_GlobalColorMapSaturation ("       saturation near", Range(0,1)) = 1
		_GlobalColorMapSaturationFar ("       saturation far", Range(0,1)) = 1
		_GlobalColorMapBrightness ("       brightness near", Range(0,2)) = 1
		_GlobalColorMapBrightnessFar ("       brightness far", Range(0,2)) = 1
		_GlobalColorMapNearMIP ("       near MIP level", Range(0,8)) = 0
		_GlobalColorMapDistortByPerlin("       distort by perlin (near)", Range(0,0.02)) = 0.005
		//_GlobalColorMapSaturationByPerlin ("       saturation by perlin", Range(0,1)) = 0.1
		// global colormap - per layer
		_GlobalColorPerLayer0123 ("Global colormap per layer (XYZW - layers 0-3)", Vector) = (1.0, 1.0, 1.0, 1.0)
/*
		// adv global colormap blending
		_GlobalColorBottom0123 ("       Height level - bottom (XYZW - layers 0-3)", Vector) = (0, 0, 0, 0)
		_GlobalColorTop0123 ("       Height level - top (XYZW - layers 0-3)", Vector) = (1.0, 1.0, 1.0, 1.0)
		_GlobalColorColormapLoSat0123 ("       colormap saturation LO", Vector) = (1.0, 1.0, 1.0, 1.0)
		_GlobalColorColormapHiSat0123 ("       colormap saturation HI", Vector) = (1.0, 1.0, 1.0, 1.0)
		_GlobalColorLayerLoSat0123 ("       layer saturation LO", Vector) = (1.0, 1.0, 1.0, 1.0)
		_GlobalColorLayerHiSat0123 ("       layer saturation HI", Vector) = (1.0, 1.0, 1.0, 1.0)
		_GlobalColorLoBlend0123 ("       Blending for LO", Vector) = (1.0, 1.0, 1.0, 1.0)
		_GlobalColorHiBlend0123 ("       Blending for HI", Vector) = (1.0, 1.0, 1.0, 1.0)	
*/

		// global normalmap
		[NoScaleOffset] _NormalMapGlobal ("Global normalmap", 2D) = "grey" {}
		_NormalMapGlobalStrength ("       strength", Range(0,2)) = 1
		_NormalMapGlobalFarOnly ("       distance fade", Float) = 30
						
		// perlin normal mapping (channels RG) with wetmask (G) and reflection map (A)
		[NoScaleOffset] _BumpMapGlobal ("Perlin normal combined w. water & reflection map (RG+B+A)", 2D) = "black" {}
		// mid / far distance definitions
		_TERRAIN_distance_start_bumpglobal ("       Far distance fade start (perlin)", Float) = 24
		rtp_perlin_start_val ("       Starting value (for start=0 only !)", Float) = 1
		_TERRAIN_distance_transition_bumpglobal ("       Far distance fade length", Float) = 50
		// (0.1 means that one perlin tile is 10 detail tiles)
		_BumpMapGlobalScale ("       Perlin normal tiling", Float) = 0.1
		rtp_mipoffset_globalnorm_offset ("       MIP offset", Range(0,5)) = 0
		//_FarNormalDamp ("       Far normal damp", Range(0,1)) = 0
		_BumpMapGlobalStrength0123 ("       Perlin normal strength per layer (XYZW - layers 0-3)", Vector) = (0.3, 0.3, 0.3, 0.3)
		
/*		
		// superdetail
		_SuperDetailTiling ("Superdetail tiling", Float) = 2
		_SuperDetailStrengthNormal0123 ("       Superdetail strength per layer (XYZW - layers 0-3)", Vector) = (0.5, 0.5, 0.5, 0.5)
*/


/*	
		// water/wet
		//
		TERRAIN_GlobalWetness ("Global wetness", Range(0,1)) = 1
		TERRAIN_WetHeight_Treshold ("       Height threshold [units]", Float) = -500
		TERRAIN_WetHeight_Transition ("       Height transition", Float) = 20
		TERRAIN_FlowSpeed ("       Flow speed", Range(0,4)) = 0.5
		TERRAIN_FlowCycleScale ("       Flow cycle scale", Range(0.5,4)) = 1
		TERRAIN_FlowScale ("       Flow tex tiling", Range(0.25,8)) = 1
		TERRAIN_FlowMipOffset ("       Flow tex filter", Range(0,4)) = 1
		TERRAIN_mipoffset_flowSpeed ("       Filter by flow speed", Range(0,0.25)) = 0.1
		TERRAIN_WetDarkening ("       Water surface darkening", Range(0.1,0.9)) = 0.5
		
		[NoTilingOffset] TERRAIN_RippleMap ("Ripplemap (RGB)", 2D) = "white" {}
		TERRAIN_RainIntensity ("Rain intensity", Range(0,1)) = 1
		TERRAIN_WetDropletsStrength ("       Rain on wet", Range(0,1)) = 0.1
		TERRAIN_DropletsSpeed ("       Anim Speed", Float) = 15
		TERRAIN_RippleScale ("       Ripple tex tiling", Range(0.25,8)) = 1
		

		// water - per layer
		TERRAIN_LayerWetStrength0123 ("Water strengh per layer (XYZW - 0123)", Vector) = (1,1,1,1)
		TERRAIN_WaterColor0 ("       Color layer 0 (A - opacity)", Color) = (0.5, 0.7, 1, 0.5)
		TERRAIN_WaterColor1 ("       Color layer 1 (A - opacity)", Color) = (0.5, 0.7, 1, 0.5)
		TERRAIN_WaterColor2 ("       Color layer 2 (A - opacity)", Color) = (0.5, 0.7, 1, 0.5)
		TERRAIN_WaterColor3 ("       Color layer 3 (A - opacity)", Color) = (0.5, 0.7, 1, 0.5)
		TERRAIN_WaterEmission0123 ("       Water emission (XYZW - 0123)", Vector) = (0,0,0,0)
		
		TERRAIN_WaterLevel0123 ("       Water level (XYZW - 0123)", Vector) = (0.5,0.5,0.5,0.5)
		TERRAIN_WaterLevelSlopeDamp0123 ("       Water level slope damp (XYZW - 0123)", Vector) = (0.1,0.1,0.1,0.1)
		TERRAIN_WaterEdge0123 ("       Water level sharpness (XYZW - 0123)", Vector) = (1,1,1,1)
		
		TERRAIN_WaterMetallic0123 ("       Water metallic (XYZW - 0123)", Vector) = (0.1,0.1,0.1,0.1)
		TERRAIN_WaterGloss0123 ("       Water gloss boost (XYZW - 0123)", Vector) = (0.2,0.2,0.2,0.2)
		
		TERRAIN_Flow0123 ("       Flow strength (XYZW - 0123)", Vector) = (1, 1, 1, 1)
		TERRAIN_Refraction0123 ("       Water refraction (XYZW - 0123)", Vector) = (0.02, 0.02, 0.02, 0.02)
		
		TERRAIN_WetGloss0123 ("       Wet gloss boost (XYZW - 0123)", Vector) = (0.2,0.2,0.2,0.2)
		
		TERRAIN_WetFlow0123 ("       Wet flow (XYZW - 0123)", Vector) = (1, 1, 1, 1)
		TERRAIN_WetRefraction0123 ("       Wet refraction factor (XYZW - 0123)", Vector) = (0.5, 0.5, 0.5, 0.5)
		
		TERRAIN_WaterGlossDamper0123 ("       Hi-freq / distance gloss damper", Vector) = (0, 0, 0, 0)
*/

		// vertical texturing
		//
		[NoScaleOffset] _VerticalTexture ("Vertical texture (RGB)", 2D) = "grey" {}
		_VerticalTextureTiling ("       Texture tiling", Float) = 50
		_VerticalTextureGlobalBumpInfluence ("       Perlin distortion", Range(0,0.3)) = 0.01
		_VerticalTexture0123 ("       Strength per layer (XYZW - 0123)", Vector) = (0.5, 0.5, 0.5, 0.5)

		// snow
		//
		rtp_snow_strength ("Snow strength", Range(0,1)) = 1
		rtp_snow_strength_per_layer0123 ("       Strength per layer (XYZW - 0123)", Vector) = (1, 1, 1, 1)
		rtp_global_color_brightness_to_snow ("       Global color brightness to snow", Range(0,1)) = 1
		rtp_snow_slope_factor ("       Slope damp factor", Range(0,15)) = 2
		// in [m] (where snow start to appear)
		rtp_snow_height_treshold ("       Coverage height theshold", Float) = -100
		rtp_snow_height_transition ("       Coverage height length", Float) = 300
		rtp_snow_color("       Color", Color) = (0.9,0.9,1,1)
		rtp_snow_gloss("       Smoothness", Range(0.01,1)) = 0.2
		
		rtp_snow_diff_fresnel("       Diffuse scattering", Range(0,2)) = 0.5
		
		rtp_snow_edge_definition ("       Edges definition", Range(0,4)) = 2
		//rtp_snow_deep_factor("       Deep factor", Range(0,6)) = 2

		rtp_snow_metallic("       Metallic", Range(0, 1)) = 0.1
		rtp_snow_Frost("       Frost", Range(0, 1)) = 0
		// WATCH this texture - it's shared with glitter !
		[NoScaleOffset] _SparkleMap("       Snow micro tex (also used for glitter)", 2D) = "black" {}
		rtp_snow_MicroTiling("       Micro tiling", Float) = 1
		rtp_snow_BumpMicro("       Micro tiling", Range(0,0.4)) = 0.08
		rtp_snow_occlusionStrength("       Occlusion strength", Range(0,1)) = 0.5
		[Enum(Translucency Setup 1, 0, Translucency Setup 2, 1, Translucency Setup 3, 2, Translucency Setup 4, 3)] rtp_snow_TranslucencyDeferredLightIndex("       Translucency deferred light index", Float) = 0

//*
		// Glitter
		//
		_GlitterColor ("Glitter color", Color) = (1,1,1,0.1)
		_GlitterStrength0123 ("       Strength per layer (XYZW - 0123)", Vector) = (0, 0, 0, 0)
		_SnowGlitterColor ("       Snow glitter color", Color) = (1,1,1,0.1)
		_GlitterTiling("       Tiling", Float) = 1
		_GlitterDensity("       Density", Range(0,1)) = 0.1
		_GlitterFilter("       Filtering", Range(-4, 4)) = 0
		_GlitterColorization("       Colorization", Range(0, 1)) = 0.5
//*/

/*		
		// caustics
		//
		TERRAIN_CausticsAnimSpeed(" Caustics anim speed", Range(0, 10)) = 2
		TERRAIN_CausticsColor("       Color (RGB)", Color) = (1,1,1,0)
		TERRAIN_CausticsWaterLevel("       Water Level", Float) = 0
		TERRAIN_CausticsWaterLevelByAngle("       Water level by slope", Range(0,8)) = 4
		TERRAIN_CausticsWaterShallowFadeLength("       Shallow fade length", Range(0.1, 10)) = 1
		TERRAIN_CausticsWaterDeepFadeLength("       Deep fade length", Range(1, 100)) = 20
		TERRAIN_CausticsTilingScale("       Texture tiling", Range(0.5, 4)) = 2
		[NoTilingOffset] TERRAIN_CausticsTex("       Caustics texture", 2D) = "black" {}
*/

		// used with VERTEX_COLOR_BLEND define switch below
		//TERRAIN_VertexColorBlend("Color blended by per vertex", Color) = (0.5,0.5,0.5,0)
		
		// used to set uv coords for global maps (top planar projection)
//		_TERRAIN_PosSize ("Top planar rect min(xz to XY) & size(xz to ZW) for global maps", Vector) = (0,0,1000,1000)

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		// planet specific
		//
		_PlanetRadius ("Planet radius", Float) = 100
		_PlanetAtmosphereLength ("   Atmosphere depth", Float) = 5
		
 		// coverage formula taken from planetary terrain shader (basing on height)
		_Value1 ("          Threshold 1", Range(0,1)) = 0
		_Value2 ("          Threshold 2", Range(0,1)) = 0.33
		_Value3 ("          Threshold 3", Range(0,1)) = 0.66
		
		// we can modify coverage basing on slopes
		_SlopeValue1 ("          Slope Boost 1", Float) = 0
		_SlopeValue2 ("          Slope Boost 2", Float) = 0
		_SlopeValue3 ("          Slope Boost 3", Float) = 0
		_SlopeValue4 ("          Slope Boost 4", Float) = 0
		
		// params used with RTP_COLORMAP_COVERAGE define switch (look for this below)
		// a bit expensive, but we can synchronize detail textures with global colormap
		_CoverageColor1Strength ("          Coverage 1 from colormap", Range(0,10)) = 0
		_CoverageColor1 ("             color sample", Color) = (1,1,1,1)
		_CoverageColor1Comp ("             matching strictness", Range(0,1)) = 0
		_CoverageColor2Strength ("          Coverage 2 from colormap", Range(0,10)) = 0
		_CoverageColor2 ("             color sample", Color) = (1,1,1,1)
		_CoverageColor2Comp ("             matching strictness", Range(0,1)) = 0
		_CoverageColor3Strength ("          Coverage 3 from colormap", Range(0,10)) = 0
		_CoverageColor3 ("             color sample", Color) = (1,1,1,1)
		_CoverageColor3Comp ("             matching strictness", Range(0,1)) = 0
		_CoverageColor4Strength ("          Coverage 4 from colormap", Range(0,10)) = 0
		_CoverageColor4 ("             color sample", Color) = (1,1,1,1)
		_CoverageColor4Comp ("             matching strictness", Range(0,1)) = 0
			
		_Color1 ("          Layer 1 color", Color) = (1,1,1,1)
		_Color2 ("          Layer 2 color", Color) = (1,1,1,1)
		_Color3 ("          Layer 3 color", Color) = (1,1,1,1)
		_Color4 ("          Layer 4 color", Color) = (1,1,1,1)
		_EquatorSize ("          Equator size", Range(0,1)) = 0.5
		_EquatorBias ("          Equator bias", Range(0,1)) = 0.5
		_EquatorColor ("          Equator color", Color) = (1,1,1,1)
		_EquatorSnowDamp ("          Equator snow damp", Range(0,4)) = 0
		_PolarSize ("          Polar size", Range(0,1)) = 0.5
		_PolarBias ("          Polar bias", Range(0,1)) = 0.5
		_PolarSnowBoost ("          Polar snow boost", Range(0,4)) = 0
		
		_GlossFromColormapAlphaStrength ("Gloss from global alpha", Range(0,1)) = 0
		_GlossFromColormapAlphaGloss ("     gloss value", Range(0,1)) = 0.25
		_GlossFromColormapAlphaNear ("     near application", Range(0,1)) = 0.2
				
		_EmissionFromColormapAlphaColor ("Emission by colormap alpha", Color) = (0,0,0,1)
		_EmissionFromColormapAlphaColormap ("     colormap application (masked by alpha)", Range(0,2)) = 0
		_EmissionFromColormapAlphaLightDependent ("     direct lighting dependent", Range(0,5)) = 0
		_EmissionFromColormapAlphaNear ("     near application", Range(0,1)) = 0.2
	}

	SubShader {
		Tags {
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}
		LOD 700
		Fog { Mode Off }
		ZTest LEqual
		CGPROGRAM
		// add "noforwardadd" below if you agree to compromise additional lighting quality (but with multiple lights in forward we'll have to render in many passes, too)
		#pragma surface surf Standard vertex:vert finalcolor:customFog fullforwardshadows nolightmap
		#pragma target 3.0
		// target platform renderers (refer to surface shader in Unity docs)
		#pragma exclude_renderers d3d11_9x gles
		#pragma glsl

		// switch between shader level of detail using Shader.SetKeyword or using material keyword handling in Unity4.////2+
		//#pragma multi_compile RTP_PM_SHADING RTP_SIMPLE_SHADING
		// for planet we don't use PM shading by default
		#define RTP_SIMPLE_SHADING
		#include "UnityCG.cginc"
		
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// defines section below allows for shader configuration. These are specific to this triplanar standalone shader
// for shader used on planet it's NECESSARY to know exactly how RTP shader works when you'd like to tweak defines below !!!
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// RTP on a planet - custom shading scenario
		#define RTP_PLANET

		// self explainable - detail colors 2,3 and bumpmap 23 not used then (shader will run a bit faster)
		// R vertex color used only (1st layer), 2nd layer coverage taken as (1-R)
		//#define USE_2_LAYERS_ONLY
		// as above, but we/re using RGB vertex color channels, A is free for other usage like water/snow coverage below
		//#define USE_3_LAYERS_ONLY
		
		// when water or snow is used you can use below defines to specify vertex channel that handles coverage (by default A channel)
		// NOTE that vertex color channel specified interferes with one of the layer splat control (4th by default), so it' only does make sense using with USE_2_LAYERS_ONLY or USE_3_LAYERS_ONLY defines
		//#define VERTEX_COLOR_TO_WATER_COVERAGE IN.color.a
		//#define VERTEX_COLOR_TO_SNOW_COVERAGE IN.color.a
		
		// ambient AO can be taken from arbitrary vertex color channel (you need to use only 3 or 2 usable layers then)
		//#define VERTEX_COLOR_AO_DAMP IN.color.a
		// diffuse color can be also affected by constant color (TERRAIN_VertexColorBlend shader property variable).
		// Level of multiplicative blending is driven by arbitrary vertex color channel defined below
		//#define VERTEX_COLOR_BLEND IN.color.a
		
		// we're texturing in local space
		#define LOCAL_SPACE_UV
		
		//
		// coverage variants (define only one at time !) taken from normals
		// using one of below variants shader will derive vertex colors (coverage) from mesh normals (local or global)
		// so - you don't have to provide mesh with any mapping or vertex colors, normals are enough
		//
		// forces 3 layers only, side is first layer, top (floor) is 2nd layer, bottom (ceil) is 3rd layer
//		#define WNORMAL_COVERAGE_XZ_Ypos_Yneg
		// forces 3 layers only, X side is first layer, Z side is 2nd, top (floor) + bottom (ceil) is 3rd layer
//		#define WNORMAL_COVERAGE_X_Z_YposYneg
		// forces 4 layers X side is first layer, Z side is 2nd, top (floor) is 3rd, bottom (ceil) is 4th layer
//		#define WNORMAL_COVERAGE_X_Z_Ypos_Yneg
		// forces 2 layers X side is first layer,  top (floor) + bottom (ceil) is 2nd layer
//		#define WNORMAL_COVERAGE_XZ_YposYneg

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// defines section below allows for  shader configuration. These are regular RTP shader specific defines (the same you'll find in RTP_Base.cginc which are configured by LODmanager)
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ATLASING to save 3 texture samplers
//#define RTP_USE_COLOR_ATLAS

// uv blending
// IMPORTANT - for UV blend routing look for "UV blend routing defines section" below
#define RTP_UV_BLEND
// blending at far distance only
//#define RTP_DISTANCE_ONLY_UV_BLEND
// usage of normals from blended layer at far distance
//#define RTP_NORMALS_FOR_REPLACE_UV_BLEND

// comment below detail when not needed
//#define RTP_SUPER_DETAIL
//#define RTP_SUPER_DTL_MULTS
// comment below if you don't use snow features
#define RTP_SNOW
// layer number taken as snow normal for near distance (for deep snow cover)
//#define RTP_SNW_CHOOSEN_LAYER_NORM_3
// layer number taken as snow color/gloss for near distance
//#define RTP_SNW_CHOOSEN_LAYER_COLOR_3
#define RTP_GLITTER

// heightblend fake AO
//#define RTP_HEIGHTBLEND_AO

//  layer emissiveness
//#define RTP_EMISSION
// when wetness is defined and fuild on surface is emissive we can mod its emisiveness by output normal (wrinkles of flowing "water")
// below define change the way we treat output normals (works fine for "lava" like emissive fuilds)
//#define RTP_FUILD_EMISSION_WRAP
// with optional reafractive distortion to emulate hot air turbulence
//#define RTP_HOTAIR_EMISSION

// define for harder heightblend edges
//#define SHARPEN_HEIGHTBLEND_EDGES_PASS1
//#define SHARPEN_HEIGHTBLEND_EDGES_PASS2

// vertical texture
#define RTP_VERTICAL_TEXTURE

// we use wet (can't be used with superdetail as globalnormal texture BA channels are shared)
// (KNOWN ISSUE: on planet flow direction is screwed)
//#define RTP_WETNESS
// water droplets
//#define RTP_WET_RIPPLE_TEXTURE
// if defined water won't handle flow nor refractions
//#define SIMPLE_WATER

//#define RTP_CAUSTICS
// when we use caustics and vertical texture - with below defined we will store vertical texture and caustics together (RGB - vertical texture, A - caustics) to save texture sampler
//#define RTP_VERTALPHA_CAUSTICS

// fog type
// RTP_FOG_EXP2, RTP_FOG_EXPONENTIAL, RTP_FOG_LINEAR, RTP_FOG_NONE
#define RTP_FOG_EXP2

// helper for cross layer specularity / IBL / Refl bleeding
//#define NOSPEC_BLEED


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//

// comment if you don't need global color map
#define COLOR_MAP
// if not defined global color map will be blended (lerp)
#define RTP_COLOR_MAP_BLEND_MULTIPLY
// advanced colormap blending per layer (used when COLOR_MAP is defined)
//#define ADV_COLOR_MAP_BLENDING
// with switch below we can derive coverage from global colormap
#define RTP_COLORMAP_COVERAGE

//
// you can use it to control snow coverage from wet mask (special combined texture channel B)
//#define RTP_SNW_COVERAGE_FROM_WETNESS

// cutting holes functionality (make global colormap alpha channel completely black to cut)
//#define RTP_CUT_HOLES

// to compute far color basing only on global colormap
#define SIMPLE_FAR
// global normal map (and we will treat normals from mesh as flat (0,1,0))
// (at this moment won't work due to different detail and global mapping)
#define RTP_NORMALGLOBAL
// global trees/shadow map - used with Terrain Composer / World Composer by Nathaniel Doldersum
//#define RTP_TREESGLOBAL
// global ambient emissive map
//#define RTP_AMBIENT_EMISSIVE_MAP

//
// DON'T touch defines below !
//
#define OVERWRITE_RTPBASE_DEFINES
// these are must for standalone shader
#define _4LAYERS
#define VERTEX_COLOR_CONTROL
#define RTP_TRIPLANAR
#define RTP_STANDALONE

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// missing defines for LOD/GRAD access
#if UNITY_VERSION >= 560
    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL)
        #define UNITY_USING_SPLIT_SAMPLERS
    #endif
#else
    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_COMPILER_HLSLCC)
        #define UNITY_USING_SPLIT_SAMPLERS
    #endif
#endif
#if !defined(UNITY_USING_SPLIT_SAMPLERS) && (defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER) && UNITY_VERSION >= 201810))
   #define UNITY_USING_SPLIT_SAMPLERS
#endif

//#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_PSSL)
#if defined(UNITY_USING_SPLIT_SAMPLERS)
	#if !defined(UNITY_SAMPLE_TEX2D_GRAD)
		#define UNITY_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy) tex.SampleGrad(sampler##tex,coord,dx,dy)
	#endif
	#if !defined(UNITY_SAMPLE_TEX2D_GRAD_SAMPLER)
		#define UNITY_SAMPLE_TEX2D_GRAD_SAMPLER(tex,samplertex,coord,dx,dy) tex.SampleGrad(sampler##samplertex,coord,dx,dy)
	#endif
    #if defined(UNITY_SAMPLE_TEX2D_LOD)
        #undef UNITY_SAMPLE_TEX2D_LOD
    #endif
    #define UNITY_SAMPLE_TEX2D_LOD(tex,coord) tex.SampleLevel (sampler##tex,(coord).xy,(coord).w)
	#if !defined(UNITY_SAMPLE_TEX2D_LOD_SAMPLER)
		#define UNITY_SAMPLE_TEX2D_LOD_SAMPLER(tex,samplertex,coord) tex.SampleLevel (sampler##samplertex,(coord).xy,(coord).w)
	#endif
#else
	#define UNITY_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy) tex2Dgrad(tex,coord,dx,dy)
    #if defined(UNITY_SAMPLE_TEX2D_LOD)
        #undef UNITY_SAMPLE_TEX2D_LOD
    #endif
    #define UNITY_SAMPLE_TEX2D_LOD(tex,coord) tex2Dlod(tex,coord)
	#if !defined(UNITY_SAMPLE_TEX2D_LOD_SAMPLER)
		#define UNITY_SAMPLE_TEX2D_LOD_SAMPLER(tex,samplertex,coord) tex2Dlod(tex,coord)
	#endif
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// UV blend routing defines section
//
// DON'T touch defines below... (unless you know exactly what you're doing) - lines 473-488
#if !defined(_4LAYERS) || defined(RTP_USE_COLOR_ATLAS)
	#define UV_BLEND_SRC_0 (UNITY_SAMPLE_TEX2D_LOD(_SplatAtlasA, float4(uvSplat01M.xy, _MixMipActual.xx)).rgba)
	#define UV_BLEND_SRC_1 (UNITY_SAMPLE_TEX2D_LOD(_SplatAtlasA, float4(uvSplat01M.zw, _MixMipActual.yy)).rgba)
	#define UV_BLEND_SRC_2 (UNITY_SAMPLE_TEX2D_LOD(_SplatAtlasA, float4(uvSplat23M.xy, _MixMipActual.zz)).rgba)
	#define UV_BLEND_SRC_3 (UNITY_SAMPLE_TEX2D_LOD(_SplatAtlasA, float4(uvSplat23M.zw, _MixMipActual.ww)).rgba)
#else
	#define UV_BLEND_SRC_0 (UNITY_SAMPLE_TEX2D_LOD(_SplatA0, float4(uvSplat01M.xy, _MixMipActual.xx)).rgba)
	#define UV_BLEND_SRC_1 (UNITY_SAMPLE_TEX2D_LOD_SAMPLER(_SplatA1, _SplatA0, float4(uvSplat01M.zw, _MixMipActual.yy)).rgba)
	#define UV_BLEND_SRC_2 (UNITY_SAMPLE_TEX2D_LOD_SAMPLER(_SplatA2, _SplatA0, float4(uvSplat23M.xy, _MixMipActual.zz)).rgba)
	#define UV_BLEND_SRC_3 (UNITY_SAMPLE_TEX2D_LOD_SAMPLER(_SplatA3, _SplatA0, float4(uvSplat23M.zw, _MixMipActual.ww)).rgba)
#endif
#define UV_BLENDMIX_SRC_0 (_MixScale0123.x)
#define UV_BLENDMIX_SRC_1 (_MixScale0123.y)
#define UV_BLENDMIX_SRC_2 (_MixScale0123.z)
#define UV_BLENDMIX_SRC_3 (_MixScale0123.w)

// As we've got defined some shader parts, you can tweak things in following lines
////////////////////////////////////////////////////////////////////////


//
// for example, when you'd like layer 3 to be source for uv blend on layer 0 you'd set it like this:
//   #define UV_BLEND_ROUTE_LAYER_0 UV_BLEND_SRC_3
// HINT: routing one layer into all will boost performance as only 1 additional texture fetch will be performed in shader (instead of up to 8 texture fetches in default setup)
//
#define UV_BLEND_ROUTE_LAYER_0 UV_BLEND_SRC_1
#define UV_BLEND_ROUTE_LAYER_1 UV_BLEND_SRC_1
#define UV_BLEND_ROUTE_LAYER_2 UV_BLEND_SRC_2
#define UV_BLEND_ROUTE_LAYER_3 UV_BLEND_SRC_3
// below routing should be exactly the same as above
#define UV_BLENDMIX_ROUTE_LAYER_0 UV_BLENDMIX_SRC_1
#define UV_BLENDMIX_ROUTE_LAYER_1 UV_BLENDMIX_SRC_1
#define UV_BLENDMIX_ROUTE_LAYER_2 UV_BLENDMIX_SRC_2
#define UV_BLENDMIX_ROUTE_LAYER_3 UV_BLENDMIX_SRC_3
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// we're using the same base code for standalone shader as for RTP terrain / mesh
#include "../RTP_Base.cginc"

		ENDCG
	} 
	
	FallBack "Diffuse"
}
