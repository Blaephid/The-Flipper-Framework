//
// 2 Parallax mapped materials with adjustable features
// (C) Tomasz Stobierski 2013-2016
//
//
// material blend vertex color R
// with water present, water mask taken from vertex color B by default
// vertex color A is used for blend
//
Shader "Relief Pack - GeometryBlend/ Customizable Shader 2 Layers" {
    Properties {
		//
		// features
		//
		[RTP_BlockInfo(1.0,1.0,1.0,1.0, 1)] block_features("Features", Float) = 1 
			[RTP_HideByProp(block_features)]
				//[RTP_Button(Check features enabled in shader code,GetShaderCode)]
				[RTP_Button(Recompile shader,RecompileShader)]
				[RTP_ShaderDefineToggle(Height blending)] BLENDING_HEIGHT(" Height blending", Float) = 1
				[RTP_ShaderDefineToggle(UV blend)] RTP_UV_BLEND(" UV blend", Float) = 1
				[RTP_ShaderDefineToggle(Top planar perlin normals)] GLOBAL_PERLIN(" Perlin normals", Float) = 0
				[RTP_ShaderDefineToggle(Top planar global colormap)] COLOR_MAP(" Global colormap", Float) = 0
				[RTP_ShaderDefineToggle(Water(slash)wet feature,RTP_WET_RIPPLE_TEXTURE,SIMPLE_WATER,FLOWMAP)] RTP_WETNESS(" Wetness", Float) = 0
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features,RTP_WETNESS)]
				[RTP_BeginArea(none,true)] dummy_features4("", Float) = 0
					[RTP_ShaderDefineToggle(Water droplets feature)] RTP_WET_RIPPLE_TEXTURE(" Rain", Float) = 0
				[RTP_EndArea(true)]
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features,RTP_WETNESS)]
				[RTP_BeginArea(none,true)] dummy_features5("", Float) = 0
					[RTP_ShaderDefineToggle(Switch to disable flow and rain)] SIMPLE_WATER(" Simple wetness", Float) = 0
				[RTP_EndArea(true)]
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features,RTP_WETNESS)]
				[RTP_BeginArea(none,true)] dummy_features5("", Float) = 0
					[RTP_ShaderDefineToggle(Flow direction from flomap texture)] FLOWMAP(" Flowmap", Float) = 0
				[RTP_EndArea(true)]
			[RTP_HideByProp()]
			
			[RTP_HideByProp(block_features)]
					[RTP_ShaderDefineToggle(Snow feature)] RTP_SNOW(" Snow", Float) = 0
					[RTP_ShaderDefineToggle(Glitter feature)] RTP_GLITTER (" Glitter", Float) = 0
					[RTP_ShaderDefineToggle(Vertical texturing feature)] VERTICAL_TEXTURE(" Vertical texturing", Float) = 0
					[RTP_ShaderDefineToggle(Caustics feature)] RTP_CAUSTICS (" Caustics", Float) = 0
					[RTP_ShaderDefineToggle(Emission feature,RTP_FUILD_EMISSION_WRAP)] RTP_EMISSION (" Emission", Float) = 0
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features,RTP_EMISSION)]
				[RTP_BeginArea(none,true)] dummy_features6("", Float) = 0
					[RTP_ShaderDefineToggle(Reafractive distortion to emulate hot air turbulence)] RTP_FUILD_EMISSION_WRAP(" Emissive water normal wrap", Float) = 0
				[RTP_EndArea(true)]
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features)]
				[RTP_BeginArea(Box,true)] dummy_globaltextiling("", Float) = 0
				[RTP_Header(Geom blend properties)]
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features)]
					[RTP_MaterialProp] _TERRAIN_PosSize("Top planar rect min(xz to XY) & size(xz to ZW) for global maps", Vector) = (0,0,1000,1000)
			[RTP_HideByProp()]
			[RTP_HideByProp(BLENDING_HEIGHT,block_features)]
					[RTP_MaterialProp] _TERRAIN_Tiling("Terrain tiling (XY) & offset(ZW)", Vector) = (3,3,0,0)
					[RTP_MaterialProp(miniThumb)] _TERRAIN_HeightMap("Terrain HeightMap (combined)", 2D) = "white" {}
					[RTP_MaterialProp(miniThumb)] _TERRAIN_Control("Terrain splat controlMap", 2D) = "black" {}
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features)]
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

		//
		// main settings
		//
		[RTP_BlockInfo(1.0,0.5,0.5,0.8, 1)] block_main("Main settings", Float) = 1 
			[RTP_HideByProp(block_main)]
				[RTP_BeginArea(Box,true)] dummy_tiling("", Float) = 0
					[RTP_Header(Near distance)]
					[RTP_Toolip(Near distance values (used with global colormap(comma)(br)perlin normals or uv blend feature))]
					[RTP_MaterialProp] _TERRAIN_distance_start ("Near distance fade start", Float) = 0
					[RTP_MaterialProp] _TERRAIN_distance_transition ("                     fade length", Float) = 20
				[RTP_EndArea(true)]
				[RTP_BeginArea(Box,true)] dummy_misc("", Float) = 0
					[RTP_Header(Misc)]
					[RTP_MaterialProp] _ExtrudeHeight("Extrude Height", Range(0.001,0.3)) = 0.04
					[RTP_MaterialProp] _occlusionStrength("Approximated occlusion", Range(0,1)) = 1
				[RTP_EndArea(true)]
			[RTP_HideByProp()]


		//
		// per layer settings 
		//
		[RTP_BlockInfo(1.0,1.0,0.5,0.8, 1)] block_layerprops("Layer properties", Float) = 1
			[RTP_LayerSelector(_MainTex,_MainTex2)] active_layer("", Float) = 0
			[RTP_HideByProp(block_layerprops)]
				[RTP_MaterialProp(active_layer0)] _MainTex ("Texture (A - smoothness)", 2D) = "white" {}
				[RTP_MaterialProp(active_layer0,miniThumb)] _BumpMap ("Bumpmap", 2D) = "bump" {}
				[RTP_MaterialProp(active_layer0,miniThumb)] _HeightMap ("Heightmap (A)", 2D) = "black" {}
				[RTP_MaterialProp(active_layer1)] _MainTex2("Texture (A - smoothness)", 2D) = "white" {}
				[RTP_MaterialProp(active_layer1,miniThumb)] _BumpMap2("Bumpmap", 2D) = "bump" {}
				[RTP_MaterialProp(active_layer1,miniThumb)] _HeightMap2("Heightmap (A)", 2D) = "black" {}
				[RTP_BeginArea(Box,true)] dummy_layerPBR("", Float) = 0
					[RTP_Header(PBR props)]
					[RTP_MaterialProp(active_layer0)] _Metalness0("  Metalness", Range(0, 1)) = 0
					[RTP_MaterialProp(active_layer0)] _GlossMin0("  Gloss Min", Range(0, 1)) = 0
					[RTP_MaterialProp(active_layer0)] _GlossMax0("  Gloss Max", Range(0, 1)) = 1
					[RTP_MaterialProp(active_layer0)] _FarSpecCorrection0("  far spec correction", Range(-1, 1)) = 0
					[RTP_MaterialProp(active_layer0)] RTP_DiffFresnel0("  diffuse scattering", Range(0, 1)) = 0
					[RTP_MaterialProp(active_layer1)] _Metalness1("  Metalness", Range(0, 1)) = 0
					[RTP_MaterialProp(active_layer1)] _GlossMin1("  Gloss Min", Range(0, 1)) = 0
					[RTP_MaterialProp(active_layer1)] _GlossMax1("  Gloss Max", Range(0, 1)) = 1
					[RTP_MaterialProp(active_layer1)] _FarSpecCorrection1("  far spec correction", Range(-1, 1)) = 0
					[RTP_MaterialProp(active_layer1)] RTP_DiffFresnel1("  diffuse scattering", Range(0, 1)) = 0
				[RTP_EndArea(true)]
				[RTP_BeginArea(Box,true)] dummy_layerCorr("", Float) = 0
					[RTP_Header(Aux props)]
						[RTP_MaterialProp(active_layer0,noalpha)] _LayerColor0("Color tint", Color) = (0.5, 0.5, 0.5, 1)
						[RTP_MaterialProp(active_layer1,noalpha)] _LayerColor1("Color tint", Color) = (0.5, 0.5, 0.5, 1)
						[RTP_MaterialProp(active_layer0)] _LayerSaturation0("Saturation", Range(0, 2)) = 1
						[RTP_MaterialProp(active_layer1)] _LayerSaturation1("Saturation", Range(0, 2)) = 1
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(RTP_UV_BLEND, block_layerprops)]
				[RTP_BeginArea(Box, true)] dummy_layerUVBlend("", Float) = 0
					[RTP_Header(UV Blend)]
					[RTP_MaterialProp(active_layer0)] _MixBlend0("UV blend strength", Range(0, 1)) = 0.5
					[RTP_MaterialProp(active_layer0)] _MixScale0("  tiling", Range(0.02, 0.25)) = 0.125
					[RTP_MaterialProp(active_layer0)] _MixSaturation0("  saturation", Range(0, 2)) = 1
					[RTP_MaterialProp(active_layer0)] _MixBrightness0("  brightness", Range(0.5, 3.5)) = 2
					[RTP_MaterialProp(active_layer0)] _MixReplace0("  replace at far", Range(0, 1)) = 0.25
					[RTP_MaterialProp(active_layer1)] _MixBlend1("UV blend strength", Range(0, 1)) = 0.5
					[RTP_MaterialProp(active_layer1)] _MixScale1("  tiling", Range(0.02, 0.25)) = 0.125
					[RTP_MaterialProp(active_layer1)] _MixSaturation1("  saturation", Range(0, 2)) = 1
					[RTP_MaterialProp(active_layer1)] _MixBrightness1("  brightness", Range(0.5, 3.5)) = 2
					[RTP_MaterialProp(active_layer1)] _MixReplace1("  replace at far", Range(0, 1)) = 0.25
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(RTP_EMISSION,block_layerprops)]
				[RTP_BeginArea(Box,true)] dummy_layerEmission("", Float) = 0
					[RTP_Header(Emission)]
					[RTP_MaterialProp(active_layer0)] _LayerEmission0("Emission", Range(0, 1)) = 0
					[RTP_MaterialProp(active_layer0)] _LayerEmissionColor0("  glow color", Color) = (0.5, 0.5, 0.5, 0)
					[RTP_MaterialProp(active_layer1)] _LayerEmission1("Emission", Range(0, 1)) = 0
					[RTP_MaterialProp(active_layer1)] _LayerEmissionColor1("  glow color", Color) = (0.5, 0.5, 0.5, 0)
				[RTP_EndArea(true)]
			[RTP_HideByProp()]


		//
		// global color
		//
		[RTP_BlockInfo(1.0, 0.8, 0.0, 1.0, 1)] block_globalcolormap("Global colormap", Float) = 1
			[RTP_HideByProp(not_COLOR_MAP, block_globalcolormap)]
				[RTP_Header((inactive))]
			[RTP_HideByProp()]
			[RTP_HideByProp(COLOR_MAP, block_globalcolormap)]
				[RTP_BeginArea(Box, true)] dummy_globalcolormap("", Float) = 0
					[RTP_MaterialProp(miniThumb)] _ColorMapGlobal ("Global colormap (RGBA)", 2D) = "white" {}
					[RTP_MaterialProp] _GlobalColorMapBlendValues ("blending near/mid/far (XYZ)", Vector) = (0.3,0.6,0.8,0)
					[RTP_MaterialProp] _GlobalColorMapSaturation ("saturation near", Range(0,1)) = 0.8
					[RTP_MaterialProp] _GlobalColorMapSaturationFar ("saturation far", Range(0,1)) = 0.8
					[RTP_MaterialProp] _GlobalColorMapBrightness ("brightness near", Range(0,1)) = 0.8
					[RTP_MaterialProp] _GlobalColorMapBrightnessFar ("brightness far", Range(0,1)) = 0.8
					[RTP_MaterialProp] _GlobalColorMapNearMIP ("near MIP level", Range(0,1)) = 0.8
					[RTP_MaterialProp] _GlobalColorMapDistortByPerlin ("distort by perlin", Range(0,0.02)) = 0.005
				[RTP_EndArea(true)]
			[RTP_HideByProp()]


		//
		// perlin
		//
		[RTP_BlockInfo(0.9,0.95,1.0,1.0, 1)] block_perlin("Perlin (amp) far distance properties", Float) = 1
			[RTP_HideByProp(not_GLOBAL_PERLIN, block_perlin)]
				[RTP_Header((inactive))]
			[RTP_HideByProp()]
			[RTP_HideByProp(GLOBAL_PERLIN,block_perlin)]
				[RTP_BeginArea(Box,true)] dummy_perlin("", Float) = 0
					[RTP_MaterialProp(miniThumb)] _BumpMapGlobal ("Perlin normal combined w. water map (RG+B)", 2D) = "black" {}
					[RTP_MaterialProp] _BumpMapGlobalScale("Perlin normal tiling", Float) = 0.1
					[RTP_MaterialProp(active_layer0)] _BumpMapGlobalStrength0("Perlin normal strength", Range(0,2)) = 0.3
					[RTP_MaterialProp(active_layer1)] _BumpMapGlobalStrength1("Perlin normal strength", Range(0,2)) = 0.3
					[RTP_MaterialProp] _TERRAIN_distance_start_bumpglobal ("Far distance fade start", Float) = 24
					[RTP_MaterialProp] rtp_perlin_start_val ("Beg.value(for start=0 only !)", Range(0,1)) = 1 
					[RTP_MaterialProp] _TERRAIN_distance_transition_bumpglobal ("Far distance fade length", Float) = 50
				[RTP_EndArea(true)]
			[RTP_HideByProp()]


		//
		// water/wetness
		//
		[RTP_BlockInfo(0.0,0.5,1.0,1.0, 1)] block_wetness("Water settings", Float) = 1 
			[RTP_HideByProp(not_RTP_WETNESS, block_wetness)]
				[RTP_Header((inactive))]
			[RTP_HideByProp()]
			[RTP_HideByProp(RTP_WETNESS, block_wetness)]
				[RTP_BeginArea(Box,true)] dummy_wetness("", Float) = 0
					[RTP_MaterialProp(miniThumb)] TERRAIN_FlowingMap("Flowingmap (water bumps - UNUSED with global perlin)", 2D) = "gray" {}
					[RTP_MaterialProp] TERRAIN_GlobalWetness("Wetness", Range(0, 1)) = 1
					[RTP_MaterialProp] TERRAIN_FlowSpeed("  Flow speed", Range(0, 4)) = 0.5
			[RTP_HideByProp()]
			[RTP_HideByProp(FLOWMAP, block_wetness)]
					[RTP_BeginArea(none,true,10)] dummy_flowmap("", Float) = 0
						[RTP_MaterialProp(miniThumb)]_FlowMap ("FlowMap (RG+BA)", 2D) = "grey" {}
						[RTP_MaterialProp]TERRAIN_FlowSpeedMap ("        Flow Speed (map)", Range(0, 1)) = 0.1
					[RTP_EndArea(true)]
			[RTP_HideByProp()]
			[RTP_HideByProp(RTP_WETNESS, block_wetness)]
					[RTP_MaterialProp] TERRAIN_FlowCycleScale("  Flow cycle scale", Range(0.5, 4)) = 1
					[RTP_MaterialProp] TERRAIN_FlowScale("  Flow tex tiling", Range(0.25, 8)) = 1
					[RTP_MaterialProp] TERRAIN_FlowMipOffset("  Flow tex filter", Range(0, 4)) = 1
					[RTP_MaterialProp] TERRAIN_mipoffset_flowSpeed("  Filter by flow speed", Range(0, 0.25)) = 0.1
					[RTP_MaterialProp] TERRAIN_WetDarkening("  Water surface darkening", Range(0.1, 0.9)) = 0.5

					[RTP_MaterialProp] TERRAIN_WaterColor("  Color (A - opacity)", Color) = (0.5, 0.7, 1, 0.5)
					[RTP_MaterialProp] TERRAIN_WaterEmission("  Emissiveness (if enabled)", Range(0, 8)) = 0

					[RTP_MaterialProp] TERRAIN_WaterLevel("  Water level", Range(0, 1.25)) = 0.7
					[RTP_MaterialProp] TERRAIN_WaterLevelSlopeDamp("     slope damp", Range(0, 2)) = 0.5
					[RTP_MaterialProp] TERRAIN_WaterEdge("     level sharpness", Range(0, 2)) = 1

					[RTP_MaterialProp] TERRAIN_WaterMetallic("  Water metallic", Range(0, 1)) = 0
					[RTP_MaterialProp] TERRAIN_WaterGloss("  Water gloss", Range(0, 1)) = 0.9

					[RTP_MaterialProp] TERRAIN_Flow("  Flow strength", Range(0, 1)) = 0.1
					[RTP_MaterialProp] TERRAIN_Refraction("  Water refraction", Range(0, 0.5)) = 0.02

					[RTP_MaterialProp] TERRAIN_WetGloss("  Wet gloss", Range(0, 1)) = 0.2

					[RTP_MaterialProp] TERRAIN_WetFlow("  Wet flow", Range(0,1)) = 1
					[RTP_MaterialProp] TERRAIN_WetRefraction("  Wet refraction factor", Range(0, 0.5)) = 0.1

					[RTP_MaterialProp] TERRAIN_WaterGlossDamper("  Distance gloss damper", Range(-1,1)) = 0
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

	
		//
		// rain
		//
		[RTP_BlockInfo(0.5, 0.5, 1.0, 1.0, 1)] block_rain("Rain settings", Float) = 1
			[RTP_HideByProp(not_RTP_WET_RIPPLE_TEXTURE, block_rain)]
				[RTP_Header((inactive))]
			[RTP_HideByProp()]
			[RTP_HideByProp(RTP_WET_RIPPLE_TEXTURE, block_rain)]
				[RTP_BeginArea(Box, true)] dummy_rain("", Float) = 0
					[RTP_MaterialProp(miniThumb)] TERRAIN_RippleMap("Ripplemap (RGB)", 2D) = "white" {}
					[RTP_MaterialProp] TERRAIN_RainIntensity("  Rain intensity", Range(0, 1)) = 1
					[RTP_MaterialProp] TERRAIN_WetDropletsStrength("  Rain on wet", Range(0, 1)) = 0.1
					[RTP_MaterialProp] TERRAIN_DropletsSpeed("Anim Speed", Float) = 15
					[RTP_MaterialProp] TERRAIN_RippleScale("Ripple tex tiling", Range(0.25, 8)) = 1
				[RTP_EndArea(true)]
			[RTP_HideByProp()]


		//
		// snow
		// 
		[RTP_BlockInfo(1.0,1.0,1.0,1.0, 1)] block_snow("Snow settings", Float) = 1 
			[RTP_HideByProp(not_RTP_SNOW, block_snow)]
				[RTP_Header((inactive))]
			[RTP_HideByProp()]
			[RTP_HideByProp(RTP_SNOW, block_snow)]
				[RTP_BeginArea(Box,true)] dummy_snow("", Float) = 0
					[RTP_MaterialProp(miniThumb,shared_SparkleMap,shared_SparkleMapUI)] _SparkleMapSnowUI("Sparklemap (shared with glitter)", 2D) = "black" {}
					// commented out below properties, so they work globally together with values set in RTP ReliefTerrain inspector
					//[RTP_MaterialProp] rtp_snow_strength("Snow strength", Range(0, 2)) = 1
					[RTP_MaterialProp(layer,0,1)] rtp_snow_strength_per_layer0123("  strength per layer", Vector) = (1, 1, 1, 1)

					[RTP_MaterialProp] rtp_global_color_brightness_to_snow("Global color brightness to snow", Range(0, 1)) = 1
					[RTP_MaterialProp] rtp_snow_slope_factor("Slope damp factor", Range(0, 4)) = 2
					//[RTP_MaterialProp] rtp_snow_height_treshold("Coverage height theshold", Float) = -100
					//[RTP_MaterialProp] rtp_snow_height_transition("Coverage height length", Float) = 300
					[RTP_MaterialProp(NoAlpha)]rtp_snow_color("Color", Color) = (0.9, 0.9, 1, 1)
					[RTP_MaterialProp] rtp_snow_metallic("Metallic", Range(0, 1)) = 0.1
					[RTP_MaterialProp] rtp_snow_gloss("Gloss", Range(0, 1)) = 0.2

					[RTP_MaterialProp] rtp_snow_diff_fresnel("Diffuse scattering", Range(0, 2)) = 0.5

					[RTP_MaterialProp] rtp_snow_edge_definition("  Edges definition", Range(0, 4)) = 2
					//[RTP_MaterialProp] rtp_snow_deep_factor("  Deep factor", Range(0, 6)) = 2 // used only when manually select snow routing by define

					[RTP_MaterialProp] rtp_snow_Frost("  Frost", Range(0, 1)) = 0
					[RTP_MaterialProp] rtp_snow_MicroTiling("  Micro tiling (tex shared with glitter sparkle map)", Float) = 1
					[RTP_MaterialProp] rtp_snow_BumpMicro("  Micro strength", Range(0.001, 0.3)) = 0.1
					[RTP_MaterialProp] rtp_snow_occlusionStrength("  Occlusion strength", Range(0, 1)) = 0.5
					[RTP_Enum(Translucency Setup 1,Translucency Setup 2,Translucency Setup 3,Translucency Setup 4)] rtp_snow_TranslucencyDeferredLightIndex("UBER translucency index (deferred HDR)", Float) = 0

				[RTP_EndArea(true)]
			[RTP_HideByProp()]


		//
		// vertical texturing
		//
		[RTP_BlockInfo(1.0,0.4,0.2,1.0, 1)] block_vert_tex("Vertical texturing", Float) = 1 
			[RTP_HideByProp(not_VERTICAL_TEXTURE, block_vert_tex)]
				[RTP_Header((inactive))]
			[RTP_HideByProp()]
			[RTP_HideByProp(VERTICAL_TEXTURE, block_vert_tex)]
				[RTP_BeginArea(Box,true)] dummy_vert_tex("", Float) = 0
					[RTP_MaterialProp(miniThumb)] _VerticalTexture("Vertical texture (RGB)", 2D) = "grey" {}
					[RTP_MaterialProp] _VerticalTextureTiling("  Texture tiling", Float) = 50
					[RTP_MaterialProp] _VerticalTextureGlobalBumpInfluence("  Perlin distortion", Range(0, 0.3)) = 0.01
					[RTP_MaterialProp(active_layer0)] _VerticalTextureStrength0("  Strength", Range(0,1)) = 0.5
					[RTP_MaterialProp(active_layer1)] _VerticalTextureStrength1("  Strength", Range(0, 1)) = 0.5
				[RTP_EndArea(true)]
			[RTP_HideByProp()]


		//
		// Caustics
		//
		[RTP_BlockInfo(1.0, 1.0, 0.92, 1.0, 1)] block_caustics("Caustics settings", Float) = 1
			[RTP_HideByProp(not_RTP_CAUSTICS, block_caustics)]
				[RTP_Header((inactive))]
			[RTP_HideByProp()]
			[RTP_HideByProp(RTP_CAUSTICS, block_caustics)]
				[RTP_BeginArea(Box, true)] dummy_caustics("", Float) = 0
					[RTP_MaterialProp(miniThumb)] TERRAIN_CausticsTex("Caustics texture", 2D) = "black" {}
					[RTP_MaterialProp] TERRAIN_CausticsTilingScale("  Texture tiling", Range(0.5, 4)) = 2
					[RTP_MaterialProp] TERRAIN_CausticsAnimSpeed("Caustics anim speed", Range(0, 10)) = 2
					[RTP_MaterialProp(noalpha,hdr)] TERRAIN_CausticsColor("Color", Color) = (1, 1, 1, 0)
					[RTP_MaterialProp] TERRAIN_CausticsWaterLevel("Water Level", Float) = 0
					[RTP_MaterialProp] TERRAIN_CausticsWaterLevelByAngle("Water level by slope", Range(0, 8)) = 4
					[RTP_MaterialProp] TERRAIN_CausticsWaterShallowFadeLength("Shallow fade length", Range(0.1, 10)) = 1
					[RTP_MaterialProp] TERRAIN_CausticsWaterDeepFadeLength("Deep fade length", Range(1, 100)) = 20
				[RTP_EndArea(true)]
			[RTP_HideByProp()]


		//
		// Glitter
		//
		[RTP_BlockInfo(0.5,1.0,1.0,1.0, 1)] block_glitter("Glitter settings", Float) = 1 
			[RTP_HideByProp(not_RTP_GLITTER,block_glitter)]
				[RTP_Header((inactive))]
			[RTP_HideByProp()]
			[RTP_HideByProp(RTP_GLITTER, block_glitter)]
				[RTP_BeginArea(Box,true)] dummy_glitter("", Float) = 0
					[RTP_MaterialProp(miniThumb,shared_SparkleMap,shared_SparkleMapSnowUI)] _SparkleMapUI("Sparklemap (shared with snow)", 2D) = "black" {}
					[RTP_MaterialProp] _GlitterColor ("Glitter color", Color) = (1,1,1,0.1)
					[RTP_MaterialProp(active_layer0)] _GlitterStrength0("Glitter strength", Float) = 1
					[RTP_MaterialProp(active_layer1)] _GlitterStrength1("Glitter strength", Float) = 1
					[RTP_MaterialProp] _SnowGlitterColor ("snow glitter color", Color) = (1,1,1,0.1)
					[RTP_MaterialProp] _GlitterTiling("tiling", Float) = 1
					[RTP_MaterialProp] _GlitterDensity("density", Range(0,1)) = 0.1
					[RTP_MaterialProp] _GlitterFilter("filtering", Range(-4, 4)) = 0
					[RTP_MaterialProp] _GlitterColorization("colorization", Range(0, 1)) = 0.5
				[RTP_EndArea(true)]
			[RTP_HideByProp()]



		[RTP_MaterialProp] dummy_end("",Float) = 0
		[HideInInspector] _SparkleMap("Sparklemap", 2D) = "black" {}
    }
    SubShader {
	Tags {
		"Queue"="Geometry+12"
		"RenderType" = "Opaque"
	}
	Offset -1,-1
	ZTest LEqual	
	CGPROGRAM

	#pragma surface surf Standard vertex:vert decal:blend exclude_path:prepass exclude_path:deferred

	#pragma exclude_renderers d3d11_9x gles
	#pragma glsl
	#pragma target 3.0

	// edges can be heightblended with underlying terrain (with one set of four layers - using one splat control map only)
	#define BLENDING_HEIGHT

	// define either PM or simple shading mode. You can also compile 2 versions to be switched runtime (globally by RTP LOD manager script)
	#define RTP_PM_SHADING
	//#pragma multi_compile RTP_PM_SHADING RTP_SIMPLE_SHADING

	// direct light atten can be taken from arbitrary vertex color channel
	//#define VERTEX_COLOR_AO_DAMP IN.color.g

	// comment if you don't need global color map
	//#define COLOR_MAP
	// if not defined global color map will be blended (lerp)
	//#define RTP_COLOR_MAP_BLEND_MULTIPLY

	// makes usage of _BumpMapGlobal texture (RG - perlin bumpmap, B watermask)
	// practical when used on larger areas of geom blend, this switch makes also perlin global texture to be used as water bumpmaps
	//#define GLOBAL_PERLIN

	// uv blending
	//#define RTP_UV_BLEND

	// water features	
	//#define RTP_WETNESS
	// enable below if you don't want to use water flow
	//#define SIMPLE_WATER
	// rain droplets
	//#define RTP_WET_RIPPLE_TEXTURE
	// if defined we don't use terrain wetmask (_BumpMapGlobal B channel), but B channel of vertex color
	// (to get it from combined texture B channel you need to define GLOBAL_PERLIN)
	#define VERTEX_COLOR_TO_WATER_COVERAGE IN.color.b	
	// you can ask shader to use flowmap to control direction of flow (can be dependent on this flowmap along uv coords defined there)
	//#define FLOWMAP

	//  layer emissiveness
	//#define RTP_EMISSION
	// when wetness is defined and fuild on surface is emissive we can mod its emisiveness by output normal (wrinkles of flowing "water")
	// below define change the way we treat output normals (works fine for "lava" like emissive fuilds)
	//#define RTP_FUILD_EMISSION_WRAP

	// vertical texturing
	//#define VERTICAL_TEXTURE

	// dynamic snow
	//#define RTP_SNOW
	// you can optionally define source vertex color to control snow coverage (useful when you need to mask snow under any kind of shelters)
	//#define VERTEX_COLOR_TO_SNOW_COVERAGE IN.color.a

	// glitter
	//#define RTP_GLITTER

	// caustics	
	//#define RTP_CAUSTICS
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////	

	//#define NOSPEC_BLEED
	#define TWO_LAYERS
	#define GEOM_BLEND
	
	struct Input {
		float4 texCoords_FlatRef;

		float3 worldPos;
		float3 viewDir;
		float3 worldNormal;
		float3 worldRefl;
		INTERNAL_DATA
		fixed4 color:COLOR;
	};
	
	float _ExtrudeHeight;
	float _occlusionStrength;

	//
	// main section
	//    
	UNITY_DECLARE_TEX2D(_MainTex);
	UNITY_DECLARE_TEX2D(_BumpMap);
	UNITY_DECLARE_TEX2D(_HeightMap);
	UNITY_DECLARE_TEX2D(_MainTex2);
	UNITY_DECLARE_TEX2D(_BumpMap2);
	UNITY_DECLARE_TEX2D(_HeightMap2);

	//
	// per layer adjustement PBL
	//
	// Layer 1
	fixed3 _LayerColor0;
	float _LayerSaturation0;
	float _Metalness0;
	float _GlossMin0;
	float _GlossMax0;
	float _FarSpecCorrection0;
	float RTP_DiffFresnel0;

	// Layer 2
	fixed3 _LayerColor1;
	float _LayerSaturation1;
	float _Metalness1;
	float _GlossMin1;
	float _GlossMax1;
	float _FarSpecCorrection1;
	float RTP_DiffFresnel1;

	//
	// EOF per layer adjustement PBL
	//		

	//
	// emissive properties
	//
	// Layer 1
	float _LayerEmission0;
	fixed3 _LayerEmissionColor0;
	// Layer 2
	float _LayerEmission1;
	fixed3 _LayerEmissionColor1;

	//
	// water/wet
	//
	float TERRAIN_GlobalWetness;
	// general flow direction
	UNITY_DECLARE_TEX2D(_FlowMap);
	UNITY_DECLARE_TEX2D(TERRAIN_FlowingMap);
	float TERRAIN_FlowSpeed;
	float TERRAIN_FlowSpeedMap;
	float TERRAIN_FlowCycleScale;
	float TERRAIN_FlowScale;
	float TERRAIN_FlowMipOffset;
	float TERRAIN_mipoffset_flowSpeed;
	float TERRAIN_WetDarkening;

	fixed4 TERRAIN_WaterColor;
	float TERRAIN_WaterEmission;

	float TERRAIN_WaterLevel;
	float TERRAIN_WaterLevelSlopeDamp;
	float TERRAIN_WaterEdge;

	float TERRAIN_WaterMetallic;
	float TERRAIN_WaterGloss;
	float TERRAIN_WaterGlossDamper;

	float TERRAIN_Flow;
	float TERRAIN_Refraction;

	float TERRAIN_WetGloss;

	float TERRAIN_WetFlow;
	float TERRAIN_WetRefraction;
	//
	// EOF water/wet
	// 	

	//
	// rain feature
	//
	UNITY_DECLARE_TEX2D(TERRAIN_RippleMap);
	float TERRAIN_RainIntensity;
	float TERRAIN_WetDropletsStrength;
	float TERRAIN_DropletsSpeed;
	float TERRAIN_RippleScale;

	//
	// colormap global
	//
	UNITY_DECLARE_TEX2D(_ColorMapGlobal);
	// can be set globaly by ReliefTerrain script
	float4 _GlobalColorMapBlendValues;
	float _GlobalColorMapNearMIP;
	float _GlobalColorMapDistortByPerlin;
	float _GlobalColorMapSaturation;
	float _GlobalColorMapSaturationFar;
	float _GlobalColorMapBrightness;
	float _GlobalColorMapBrightnessFar;

	//
	// perlin ( + watermask)
	//
	UNITY_DECLARE_TEX2D(_BumpMapGlobal);
	float4 _BumpMapGlobal_TexelSize;
	float _BumpMapGlobalScale;
	float _BumpMapGlobalStrength0;
	float _BumpMapGlobalStrength1;
	float rtp_perlin_start_val;

	//
	// UV blend
	//
	float _MixBlend0;
	float _MixBlend1;
	float _MixScale0;
	float _MixScale1;
	float _MixSaturation0;
	float _MixSaturation1;
	float _MixBrightness0;
	float _MixBrightness1;
	float _MixReplace0;
	float _MixReplace1;

	//
	// Vertical texturing
	//
	UNITY_DECLARE_TEX2D(_VerticalTexture);
	float _VerticalTextureTiling;
	float _VerticalTextureGlobalBumpInfluence;
	float _VerticalTextureStrength0;
	float _VerticalTextureStrength1;

	//
	// snow
	//
	float rtp_global_color_brightness_to_snow;
	float rtp_snow_strength;
	float rtp_snow_slope_factor;
	// in [m] (where snow start to appear)
	float rtp_snow_height_treshold;
	float rtp_snow_height_transition;
	fixed4 rtp_snow_color;
	float rtp_snow_gloss;
	float rtp_snow_diff_fresnel;
	float rtp_snow_edge_definition;
	float rtp_snow_deep_factor;
	float rtp_snow_metallic;
	float rtp_snow_Frost;
	float rtp_snow_MicroTiling;
	float rtp_snow_BumpMicro;
	float rtp_snow_occlusionStrength;
	//float rtp_snow_TranslucencyDeferredLightIndex; // declared in ReplacementPBSLighting.cginc

	//
	// glitter
	//
	half4 _GlitterColor;
	float _GlitterStrength0;
	float _GlitterStrength1;
	half4 _SnowGlitterColor;
	float _GlitterTiling;
	float _GlitterDensity;
	float _GlitterFilter;
	float _GlitterColorization;
	UNITY_DECLARE_TEX2D(_SparkleMap);

	//
	// caustics
	//
	float TERRAIN_CausticsAnimSpeed;
	fixed4 TERRAIN_CausticsColor;
	float TERRAIN_CausticsWaterLevel;
	float TERRAIN_CausticsWaterLevelByAngle;
	float TERRAIN_CausticsWaterShallowFadeLength;
	float TERRAIN_CausticsWaterDeepFadeLength;
	float TERRAIN_CausticsTilingScale;
	UNITY_DECLARE_TEX2D(TERRAIN_CausticsTex);


	// RTP terrain specific
	float _TERRAIN_distance_start;
	float _TERRAIN_distance_transition;
	float _TERRAIN_distance_start_bumpglobal;
	float _TERRAIN_distance_transition_bumpglobal;

	// used for global maps and height blend	
	float4 _TERRAIN_PosSize;
	float4 _TERRAIN_Tiling;
	UNITY_DECLARE_TEX2D(_TERRAIN_HeightMap);
	UNITY_DECLARE_TEX2D(_TERRAIN_Control);

	#include "Assets/ReliefPack/Shaders/CustomLighting.cginc"
	#include "Assets/ReliefPack/Shaders/ConfigurableCore.cginc"
	ENDCG
      
    } 

	CustomEditor "RTP_CustomShaderGUI"
	//FallBack "Diffuse"
}
