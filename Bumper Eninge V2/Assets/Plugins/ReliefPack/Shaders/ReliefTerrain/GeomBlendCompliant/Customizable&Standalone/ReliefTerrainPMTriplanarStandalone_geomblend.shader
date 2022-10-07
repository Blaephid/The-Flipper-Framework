//
// Vertex paint / 4 layers version of RTP shader (optionally 2 or 3),
// in triplanar mode (enabled below) can be easily adopted for voxel terrains (I believe)
// most features are disabled by default, but if you need then (for example dynamic snow or so)
// - go on and look around to quickly customize it
//
// geom blend version - good for complex caverns inside terrain
// with 4 layers, coverage is realised using color RGB vertex color channels (layers 1-3), 4th layer coverage is defined as (1-(R+G+B))
// you can also use automatic coverage defined by mesh normals (local or global - LOCAL_SPACE_UV define keyword), for example WNORMAL_COVERAGE_X_Z_Ypos_Yneg
// vertex color A channel is used for blending
//
// (C) Tomasz Stobierski 2013-2016
//
//
Shader "Relief Pack - GeometryBlend/ Standalone Triplanar" {
	Properties {
		//
		// features
		//
		[RTP_BlockInfo(1.0,1.0,1.0,1.0, 1)] block_features("Features", Float) = 1 
			[RTP_HideByProp(block_features)]
				//[RTP_Button(Check features enabled in shader code,GetShaderCode)]
				[RTP_Button(Recompile shader,RecompileShader)]
					[RTP_ShaderDefineToggle(Height blending)] BLENDING_HEIGHT(" Height blending", Float) = 1
					[RTP_ShaderDefineToggle(UV blend,RTP_NORMALS_FOR_REPLACE_UV_BLEND)] RTP_UV_BLEND(" UV blend", Float) = 1
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features,RTP_UV_BLEND)]
				[RTP_BeginArea(none,true)] dummy_features2("", Float) = 0
					[RTP_ShaderDefineToggle(Normal replace at far distance(br)(together with color replacement))] RTP_NORMALS_FOR_REPLACE_UV_BLEND(" Normal replace", Float) = 0
				[RTP_EndArea(true)]
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features)]
					[RTP_ShaderDefineToggle(Top planar global colormap,ADV_COLOR_MAP_BLENDING)] COLOR_MAP(" Global colormap", Float) = 0
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features,COLOR_MAP)]
				[RTP_BeginArea(none,true)] dummy_features3("", Float) = 0
					[RTP_ShaderDefineToggle(Advanced global colormap blending per layer)] ADV_COLOR_MAP_BLENDING(" Advanced blending", Float) = 0
				[RTP_EndArea(true)]
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features)]
					[RTP_ShaderDefineToggle(Water(slash)wet feature,RTP_WET_RIPPLE_TEXTURE,SIMPLE_WATER)] RTP_WETNESS(" Wetness", Float) = 0
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
			[RTP_HideByProp(block_features)]
					[RTP_ShaderDefineToggle(Snow feature)] RTP_SNOW(" Snow", Float) = 0
					[RTP_ShaderDefineToggle(Glitter feature)] RTP_GLITTER (" Glitter", Float) = 0
					[RTP_ShaderDefineToggle(Vertical texturing feature)] RTP_VERTICAL_TEXTURE(" Vertical texturing", Float) = 0
					[RTP_ShaderDefineToggle(Superdetail feature (micro perlin normal))] RTP_SUPER_DETAIL(" Superdetail", Float) = 0
					[RTP_ShaderDefineToggle(Caustics feature)] RTP_CAUSTICS (" Caustics", Float) = 0
					[RTP_ShaderDefineToggle(Emission feature,RTP_HOTAIR_EMISSION,RTP_FUILD_EMISSION_WRAP)] RTP_EMISSION (" Emission", Float) = 0
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features,RTP_EMISSION)]
				[RTP_BeginArea(none,true)] dummy_features6("", Float) = 0
					[RTP_ShaderDefineToggle(Switch to change the way we treat output normals(br)(works fine for (quot)lava(quot) like emissive fuilds))] RTP_HOTAIR_EMISSION(" Emissive refraction", Float) = 0
					[RTP_ShaderDefineToggle(Reafractive distortion to emulate hot air turbulence)] RTP_FUILD_EMISSION_WRAP(" Emissive water normal wrap", Float) = 0
				[RTP_EndArea(true)]
			[RTP_HideByProp()]
			[RTP_HideByProp(block_features)]
					[RTP_ShaderDefineToggle(Heightblend fake AO feature)] RTP_HEIGHTBLEND_AO (" Fake AO", Float) = 0
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
					[RTP_MaterialProp(miniThumb)] _TERRAIN_HeightMapBase("Terrain HeightMap (combined)", 2D) = "white" {}
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
					[RTP_Header(Tiling and distances)]
					[RTP_MaterialProp] _TERRAIN_ReliefTransformTriplanarZ ("Triplanar tile size", Float) = 3
					[RTP_MaterialProp] _TERRAIN_distance_start ("Near distance fade start", Float) = 0
					[RTP_MaterialProp] _TERRAIN_distance_transition ("                     fade length", Float) = 20
				[RTP_EndArea(true)]
				[RTP_BeginArea(Box,true)] dummy_misc("", Float) = 0
					[RTP_Header(Misc)]
					[RTP_MaterialProp] _RTP_MIP_BIAS ("MIP Bias", Range(-1,1)) = 0
					[RTP_MaterialProp] _TERRAIN_ExtrudeHeight ("Parallax extrude height", Range(0.001,0.3)) = 0.06
					[RTP_MaterialProp] _occlusionStrength("Approximated occlusion", Range(0,1)) = 1
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(RTP_HEIGHTBLEND_AO,block_main)]
				[RTP_BeginArea(Box,true)] dummy_fakeAO("", Float) = 0
					[RTP_Header(Fake AO)]
					[RTP_MaterialProp] RTP_AOsharpness ("Fake AO 2 HB sharpness", Range(0, 10)) = 1
					[RTP_MaterialProp] RTP_AOamp ("Fake AO 2 HB value", Range(0,2)) = 0.5
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(RTP_HOTAIR_EMISSION,block_main)]
				[RTP_BeginArea(Box,true)] dummy_emissionRefr("", Float) = 0
					[RTP_Header(Emissive refraction)]
					[RTP_MaterialProp] EmissionRefractFiltering ("Emission refraction filtering", Range(0, 8)) = 2
					[RTP_MaterialProp] EmissionRefractAnimSpeed ("   refraction anim speed", Range(0,20)) = 2
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(RTP_UV_BLEND,block_main)]
				[RTP_BeginArea(Box,true)] dummy_UVblend("", Float) = 0
					[RTP_Header(UV blend master)]
					[RTP_MaterialProp] _blend_multiplier("UV blend multiplier", Range(0,1)) = 1
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			// used by TerraVol vertex interpolation		
			//_TERRAIN_distance_start_vertexinterpolation ("Vertex interpolation distance fade start (TerraVol LOD)", Float) = 100
			//_TERRAIN_distance_transition_vertexinterpolation ("         distance fade length", Float) = 30

		//
		// splat textures
		//		
		[RTP_BlockInfo(0.5,1.0,0.5,0.8, 1)] block_textures("Splat textures", Float) = 1
			[RTP_BeginArea(none,true)] dummy_textures("", Float) = 0
			[RTP_HideByProp(block_textures)]
				[RTP_ShaderDefineToggle(Color atlas instead of 4 splat detail textures)] RTP_USE_COLOR_ATLAS(" Use splat textures atlas", Float) = 0
			[RTP_HideByProp()]
			[RTP_HideByProp(RTP_USE_COLOR_ATLAS,block_textures)]
				// detail textures / normal maps (combined), heightmap (combined)
				// you can use either atlas (to save 3 tex samplers)
				[RTP_MaterialProp(miniThumb)] _SplatAtlasA ("Detail atlas (RGB+A gloss)", 2D) = "black" {}
			[RTP_HideByProp()]
			[RTP_HideByProp(not_RTP_USE_COLOR_ATLAS,block_textures)] 
				// or up to 4 textures (look for RTP_USE_COLOR_ATLAS define below)
				[RTP_Tooltip( RGB (plus) A (smoothness) )][RTP_MaterialProp(miniThumb)] _SplatA0 ("Detailmap 0", 2D) = "black" {}
				[RTP_Tooltip( RGB (plus) A (smoothness) )][RTP_MaterialProp(miniThumb)] _SplatA1 ("Detailmap 1", 2D) = "black" {}
				[RTP_Tooltip( RGB (plus) A (smoothness) )][RTP_MaterialProp(miniThumb)] _SplatA2 ("Detailmap 2", 2D) = "black" {}
				[RTP_Tooltip( RGB (plus) A (smoothness) )][RTP_MaterialProp(miniThumb)] _SplatA3 ("Detailmap 3", 2D) = "black" {}
			[RTP_HideByProp()]
			[RTP_HideByProp(block_textures)]
				[RTP_MaterialProp(miniThumb)] _BumpMap01 ("Bumpmap combined 0+1 (RG+BA)", 2D) = "grey" {}
				[RTP_MaterialProp(miniThumb)] _BumpMap23 ("Bumpmap combined 2+3 (RG+BA)", 2D) = "grey" {}
				[RTP_MaterialProp(miniThumb)] _TERRAIN_HeightMap ("Heightmap combined (RGBA - layers 0-3)", 2D) = "white" {}
			[RTP_HideByProp()]
			[RTP_EndArea(true)]



		//
		// per layer settings + PBL
		//
		[RTP_BlockInfo(1.0,1.0,0.5,0.8, 1)] block_layerprops("Per layer properties", Float) = 1
			[RTP_HideByProp(block_layerprops)]
			[RTP_LayerSelector(_SplatA0,_SplatA1,_SplatA2,_SplatA3)] active_layer("", Float) = 0
				[RTP_BeginArea(Box,true)] dummy_layerPBR("", Float) = 0
				[RTP_Header(PBR props (minus) layer,layer)]
					[RTP_MaterialProp(layer,0,1)] RTP_metallic0123 ("metalness", Vector) = (0, 0, 0, 0)
					[RTP_MaterialProp(layer,0,1)] RTP_glossMin0123 ("gloss min", Vector) = (0, 0, 0, 0)
					[RTP_MaterialProp(layer,0,1)] RTP_glossMax0123 ("gloss max", Vector) = (1, 1, 1, 1)
					[RTP_MaterialProp(layer,0,2,1)] _FarSpecCorrection0123 ("far spec correction", Vector) = (0, 0, 0, 0) // third param is neg offset as drawer parameters cant start with - sign...
					[RTP_MaterialProp(layer,0,2)] RTP_DiffFresnel0123 ("diffuse scattering", Vector) = (0, 0, 0, 0)
				[RTP_EndArea(true)]
				[RTP_BeginArea(Box,true)] dummy_layerCorr("", Float) = 0
					[RTP_Header(Aux props)]
					[RTP_MaterialProp(layer,0,4)] _LayerBrightness0123 ("brightness", Vector) = (1, 1, 1, 1)
					[RTP_MaterialProp(layer,0,2)] _LayerSaturation0123 ("saturation", Vector) = (1, 1, 1, 1)
					[RTP_MaterialProp(layer,0,7)] _MIPmult0123 ("MIP offset at far distance", Vector) = (0,0,0,0)
					[RTP_MaterialProp(layer,0,1)] _BumpMapGlobalStrength0123("Perlin normal strength", Vector) = (0.3, 0.3, 0.3, 0.3)
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(RTP_HEIGHTBLEND_AO,block_layerprops)]
				[RTP_BeginArea(Box,true)] dummy_layerFakeAO("", Float) = 0
					[RTP_MaterialProp(layer,0,2)] RTP_AO_0123("AO strength", Vector) = (0.2, 0.2, 0.2, 0.2)
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(RTP_GLITTER,block_layerprops)]
				[RTP_BeginArea(Box,true)] dummy_layerGlitter("", Float) = 0
					[RTP_MaterialProp(layer,0,1)] _GlitterStrength0123 ("Glitter strength", Vector) = (1, 1, 1, 1)
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(RTP_EMISSION,block_layerprops)]
				[RTP_BeginArea(Box,true)] dummy_layerEmission("", Float) = 0
					[RTP_Header(Emission)]
					[RTP_MaterialProp(layer,0,1)] _LayerEmission0123("Strength", Vector) = (1, 1, 1, 1)
					[RTP_MaterialProp(active_layer0)] _LayerEmissionColor0 ("  glow color", Color) = (0.5,0.5,0.5,0)
					[RTP_MaterialProp(active_layer1)] _LayerEmissionColor1 ("  glow color", Color) = (0.5,0.5,0.5,0)
					[RTP_MaterialProp(active_layer2)] _LayerEmissionColor2 ("  glow color", Color) = (0.5,0.5,0.5,0)
					[RTP_MaterialProp(active_layer3)] _LayerEmissionColor3 ("  glow color", Color) = (0.5,0.5,0.5,0)
				[RTP_EndArea(true)]
			[RTP_HideByProp()]
			[RTP_HideByProp(RTP_HOTAIR_EMISSION, block_layerprops)]
				[RTP_BeginArea(Box,true)] dummy_layerRefrEmission("", Float) = 0
					[RTP_MaterialProp(layer,0,0.02)] _LayerEmissionRefractStrength0123("  hot air refract strength", Vector) = (0, 0, 0, 0)
					[RTP_MaterialProp(layer,0,1)] _LayerEmissionRefractHBedge0123("  on layer edges only", Vector) = (0, 0, 0, 0)
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(RTP_UV_BLEND, block_layerprops)]
				[RTP_BeginArea(Box, true)] dummy_layerUVBlend("", Float) = 0
					[RTP_Header(UV Blend)]
					[RTP_MaterialProp(layer)] _MixScale0123("tiling", Vector) = (0.2, 0.2, 0.2, 0.2)
					[RTP_MaterialProp(layer,0,1)] _MixBlend0123 ("value", Vector) = (0.5, 0.5, 0.5, 0.5)
					[RTP_MaterialProp(layer,0,2)] _MixSaturation0123 ("saturation", Vector) = (1.0, 1.0, 1.0, 1.0)
					[RTP_MaterialProp(layer,0,4)] _MixBrightness0123 ("brightness", Vector) = (2.0, 2.0, 2.0, 2.0)
					[RTP_MaterialProp(layer,0,1)] _MixReplace0123 ("replace", Vector) = (0, 0, 0, 0)
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(COLOR_MAP, block_layerprops)]
				[RTP_BeginArea(Box, true)] dummy_layerColormap("", Float) = 0
					[RTP_MaterialProp(layer, 0, 1)] _GlobalColorPerLayer0123("Strength", Vector) = (1.0, 1.0, 1.0, 1.0)
				[RTP_EndArea(true)]
			[RTP_HideByProp()]
			[RTP_HideByProp(ADV_COLOR_MAP_BLENDING, block_layerprops)]
				[RTP_BeginArea(Box, true)] dummy_layerAdvColormap("", Float) = 0
					[RTP_Header(Advanced global colomap blending (per layer))]
					[RTP_MaterialProp(layer,0,1)] _GlobalColorBottom0123 ("  Height level - bottom ", Vector) = (0, 0, 0, 0)
					[RTP_MaterialProp(layer,0,1)] _GlobalColorTop0123 ("  Height level - top", Vector) = (1.0, 1.0, 1.0, 1.0)
					[RTP_MaterialProp(layer,0,1)] _GlobalColorColormapLoSat0123 ("  colormap saturation LO", Vector) = (1.0, 1.0, 1.0, 1.0)
					[RTP_MaterialProp(layer,0,1)] _GlobalColorColormapHiSat0123 ("  colormap saturation HI", Vector) = (1.0, 1.0, 1.0, 1.0)
					[RTP_MaterialProp(layer,0,2)] _GlobalColorLayerLoSat0123 ("  layer saturation LO", Vector) = (1.0, 1.0, 1.0, 1.0)
					[RTP_MaterialProp(layer,0,2)] _GlobalColorLayerHiSat0123 ("  layer saturation HI", Vector) = (1.0, 1.0, 1.0, 1.0)
					[RTP_MaterialProp(layer,0,1)] _GlobalColorLoBlend0123 ("  Blending for LO", Vector) = (1.0, 1.0, 1.0, 1.0)
					[RTP_MaterialProp(layer,0,1)] _GlobalColorHiBlend0123 ("  Blending for HI", Vector) = (1.0, 1.0, 1.0, 1.0)
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

			[RTP_HideByProp(RTP_SUPER_DETAIL, block_layerprops)]
				[RTP_BeginArea(Box,true)] dummy_layersuperdetail("", Float) = 0
					[RTP_MaterialProp(layer,0,1)] _SuperDetailStrengthNormal0123 ("Superdetail strength", Vector) = (1.0, 1.0, 1.0, 1.0)
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
					[RTP_MaterialProp] _GlobalColorMapSaturation ("saturation", Range(0,1)) = 0.8
					[RTP_MaterialProp] _GlobalColorMapBrightness ("brightness", Range(0,1)) = 0.8
					[RTP_MaterialProp] _GlobalColorMapNearMIP ("near MIP level", Range(0,1)) = 0.8
					[RTP_MaterialProp] _GlobalColorMapDistortByPerlin ("distort by perlin", Range(0,0.02)) = 0.005
				[RTP_EndArea(true)]
			[RTP_HideByProp()]

		//
		// perlin
		//
		[RTP_BlockInfo(0.9,0.95,1.0,1.0, 1)] block_perlin("Perlin (amp) far distance properties", Float) = 1
			[RTP_HideByProp(block_perlin)]
				[RTP_BeginArea(Box,true)] dummy_perlin("", Float) = 0
					[RTP_MaterialProp(miniThumb)] _BumpMapGlobal ("Perlin normal combined w. water map (RG+B)", 2D) = "black" {}
					// mid / far distance definitions
					[RTP_MaterialProp] _TERRAIN_distance_start_bumpglobal ("Far distance fade start", Float) = 24
					[RTP_MaterialProp] rtp_perlin_start_val ("Beg.value(for start=0 only !)", Range(0,1)) = 1 
					[RTP_MaterialProp] _TERRAIN_distance_transition_bumpglobal ("Far distance fade length", Float) = 50
					// (0.1 means that one perlin tile is 10 detail tiles)
					[RTP_MaterialProp] _BumpMapGlobalScale ("Perlin normal tiling", Float) = 0.1
					[RTP_MaterialProp] rtp_mipoffset_globalnorm_offset ("MIP offset", Range(0,5)) = 0
					[RTP_MaterialProp] _FarNormalDamp ("Far normal damp", Range(0,1)) = 0
				[RTP_EndArea(true)]
			[RTP_HideByProp()]


		//
		// superdetail
		//
		[RTP_BlockInfo(0.5,0.5,0.5,1.0, 1)] block_superdetail("Superdetail settings", Float) = 1 
			[RTP_HideByProp(not_RTP_SUPER_DETAIL, block_superdetail)]
				[RTP_Header((inactive))]
			[RTP_HideByProp()]
			[RTP_HideByProp(RTP_SUPER_DETAIL, block_superdetail)]
				[RTP_BeginArea(Box,true)] dummy_superdetail("", Float) = 0
					[RTP_MaterialProp] _SuperDetailTiling("Superdetail tiling", Float) = 4
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
					[RTP_MaterialProp] TERRAIN_GlobalWetness("Global wetness", Range(0, 1)) = 1
					[RTP_MaterialProp] TERRAIN_WetHeight_Treshold("  Height threshold [units]", Float) = -500
					[RTP_MaterialProp] TERRAIN_WetHeight_Transition("  Height transition", Float) = 20
					[RTP_MaterialProp] TERRAIN_FlowSpeed("  Flow speed", Range(0, 4)) = 0.5
					[RTP_MaterialProp] TERRAIN_FlowCycleScale("  Flow cycle scale", Range(0.5, 4)) = 1
					[RTP_MaterialProp] TERRAIN_FlowScale("  Flow tex tiling", Range(0.25, 8)) = 1
					[RTP_MaterialProp] TERRAIN_FlowMipOffset("  Flow tex filter", Range(0, 4)) = 1
					[RTP_MaterialProp] TERRAIN_mipoffset_flowSpeed("  Filter by flow speed", Range(0, 0.25)) = 0.1
					[RTP_MaterialProp] TERRAIN_WetDarkening("  Water surface darkening", Range(0.1, 0.9)) = 0.5

					// water - per layer
					[RTP_Header(Water(slash)wet per layer settings)]
					[RTP_MaterialProp(layer,0,1)] TERRAIN_LayerWetStrength0123("  Water strengh", Vector) = (1, 1, 1, 1)
					[RTP_MaterialProp(active_layer0)] TERRAIN_WaterColor0("  Color (A - opacity)", Color) = (0.5, 0.7, 1, 0.5)
					[RTP_MaterialProp(active_layer1)] TERRAIN_WaterColor1("  Color (A - opacity)", Color) = (0.5, 0.7, 1, 0.5)
					[RTP_MaterialProp(active_layer2)] TERRAIN_WaterColor2("  Color (A - opacity)", Color) = (0.5, 0.7, 1, 0.5)
					[RTP_MaterialProp(active_layer3)] TERRAIN_WaterColor3("  Color (A - opacity)", Color) = (0.5, 0.7, 1, 0.5)
					[RTP_MaterialProp(layer,0,8)] TERRAIN_WaterEmission0123("  Emissiveness (if enabled)", Vector) = (0, 0, 0, 0)

					[RTP_MaterialProp(layer,0,1.25)] TERRAIN_WaterLevel0123("  Water level", Vector) = (0.5, 0.5, 0.5, 0.5)
					[RTP_MaterialProp(layer,0,2)] TERRAIN_WaterLevelSlopeDamp0123("     slope damp", Vector) = (0.1, 0.1, 0.1, 0.1)
					[RTP_MaterialProp(layer,0,2)] TERRAIN_WaterEdge0123("     level sharpness", Vector) = (1, 1, 1, 1)

					[RTP_MaterialProp(layer,0,1)] TERRAIN_WaterMetallic0123("  Water metallic", Vector) = (0.1, 0.1, 0.1, 0.1)
					[RTP_MaterialProp(layer,0,1)] TERRAIN_WaterGloss0123("  Water gloss", Vector) = (0.2, 0.2, 0.2, 0.2)

					[RTP_MaterialProp(layer,0,1)] TERRAIN_Flow0123("  Flow strength", Vector) = (1, 1, 1, 1)
					[RTP_MaterialProp(layer,0,0.5)] TERRAIN_Refraction0123("  Water refraction", Vector) = (0.02, 0.02, 0.02, 0.02)

					[RTP_MaterialProp(layer,0,1)] TERRAIN_WetGloss0123("  Wet gloss", Vector) = (0.2, 0.2, 0.2, 0.2)

					[RTP_MaterialProp(layer,0,1)] TERRAIN_WetFlow0123("  Wet flow", Vector) = (1, 1, 1, 1)
					[RTP_MaterialProp(layer,0,0.5)] TERRAIN_WetRefraction0123("  Wet refraction factor", Vector) = (0.5, 0.5, 0.5, 0.5)

					[RTP_MaterialProp(layer,0,2,1)] TERRAIN_WaterGlossDamper0123("  Distance gloss damper", Vector) = (0, 0, 0, 0) // third param is neg offset as drawer parameters cant start with - sign...
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
					//[RTP_MaterialProp] rtp_snow_strength("Snow strength", Range(0, 1)) = 1
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
			[RTP_HideByProp(not_RTP_VERTICAL_TEXTURE, block_vert_tex)]
				[RTP_Header((inactive))]
			[RTP_HideByProp()]
			[RTP_HideByProp(RTP_VERTICAL_TEXTURE, block_vert_tex)]
				[RTP_BeginArea(Box,true)] dummy_vert_tex("", Float) = 0
					[RTP_MaterialProp(miniThumb)] _VerticalTexture("Vertical texture (RGB)", 2D) = "grey" {}
					[RTP_MaterialProp] _VerticalTextureTiling("  Texture tiling", Float) = 50
					[RTP_MaterialProp] _VerticalTextureGlobalBumpInfluence("  Perlin distortion", Range(0, 0.3)) = 0.01
					[RTP_MaterialProp(layer,0,1)] _VerticalTexture0123("  Strength per layer", Vector) = (0.5, 0.5, 0.5, 0.5)
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
		LOD 700
		Fog { Mode Off }
		CGPROGRAM
		// add "noforwardadd" below if you agree to compromise additional lighting quality (but with multiple lights in forward we'll have to render in many passes, too)
		#pragma surface surf Standard vertex:vert decal:blend finalcolor:customFog exclude_path:prepass // fullforwardshadows 

		#pragma exclude_renderers d3d11_9x gles
		#pragma glsl
		#pragma target 3.0

		#define RTP_PM_SHADING

		#include "UnityCG.cginc"
		
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// defines section below allows for shader configuration. These are specific to this triplanar standalone shader
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		// self explainable - detail colors 2,3 and bumpmap 23 not used then (shader will run a bit faster)
		// R vertex color used only (1st layer), 2nd layer coverage taken as (1-R)
		//#define USE_2_LAYERS_ONLY
		// as above, but we/re using RGB vertex color channels, A is used for blending
		//#define USE_3_LAYERS_ONLY
		
		// edges can be heightblended with underlying terrain (with one set of four layers - using one splat control map only)
		#define BLENDING_HEIGHT
		
		// when water or snow is used you can use below defines to specify vertex channel that handles coverage (by default A channel)
		// NOTE that vertex color channel specified interferes with one of the layer splat control (4th by default), so it' only does make sense using with USE_2_LAYERS_ONLY or USE_3_LAYERS_ONLY defines
		//#define VERTEX_COLOR_TO_WATER_COVERAGE IN.color.a
		//#define VERTEX_COLOR_TO_SNOW_COVERAGE IN.color.a
		
		// direct light atten can be taken from arbitrary vertex color channel
		//#define VERTEX_COLOR_AO_DAMP IN.color.a
		// diffuse color can be also affected by constant color (TERRAIN_VertexColorBlend shader property variable).
		// Level of multiplicative blending is driven from arbitrary vertex color channel defined below
		//#define VERTEX_COLOR_BLEND IN.color.a
		
		// we're texturing in local space
		//#define LOCAL_SPACE_UV
		
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
#define SHARPEN_HEIGHTBLEND_EDGES_PASS1
//#define SHARPEN_HEIGHTBLEND_EDGES_PASS2

// vertical texture
//#define RTP_VERTICAL_TEXTURE

// we use wet (can't be used with superdetail as globalnormal texture BA channels are shared)
//#define RTP_WETNESS
// water droplets
//#define RTP_WET_RIPPLE_TEXTURE
// if defined water won't handle flow nor refractions
//#define SIMPLE_WATER

//#define RTP_CAUSTICS
// when we use caustics and vertical texture - with below defined we will store vertical texture and caustics together (RGB - vertical texture, A - caustics) to save texture sampler
//#define RTP_VERTALPHA_CAUSTICS

// helper for cross layer specularity / IBL / Refl bleeding
//#define NOSPEC_BLEED

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//

// comment if you don't need global color map
//#define COLOR_MAP
// if not defined global color map will be blended (lerp)
#define RTP_COLOR_MAP_BLEND_MULTIPLY
// advanced colormap blending per layer (used when COLOR_MAP is defined)
//#define ADV_COLOR_MAP_BLENDING

//
// you can use it to control snow coverage from wet mask (special combined texture channel B)
//#define RTP_SNW_COVERAGE_FROM_WETNESS

// to compute far color basing only on global colormap
//#define SIMPLE_FAR
// global normal map (and we will treat normals from mesh as flat (0,1,0))
//#define RTP_NORMALGLOBAL
// global trees/shadow map - used with Terrain Composer / World Composer by Nathaniel Doldersum
//#define RTP_TREESGLOBAL

//
// DON'T touch defines below !
//
#define OVERWRITE_RTPBASE_DEFINES
// these are must for standalone shader
#define _4LAYERS
#define VERTEX_COLOR_CONTROL
#define RTP_TRIPLANAR
#define RTP_STANDALONE
#define GEOM_BLEND

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
#define UV_BLEND_ROUTE_LAYER_0 UV_BLEND_SRC_0
#define UV_BLEND_ROUTE_LAYER_1 UV_BLEND_SRC_1
#define UV_BLEND_ROUTE_LAYER_2 UV_BLEND_SRC_3
#define UV_BLEND_ROUTE_LAYER_3 UV_BLEND_SRC_2
// below routing should be exactly the same as above
#define UV_BLENDMIX_ROUTE_LAYER_0 UV_BLENDMIX_SRC_0
#define UV_BLENDMIX_ROUTE_LAYER_1 UV_BLENDMIX_SRC_1
#define UV_BLENDMIX_ROUTE_LAYER_2 UV_BLENDMIX_SRC_3
#define UV_BLENDMIX_ROUTE_LAYER_3 UV_BLENDMIX_SRC_2
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// we're using the same base code for standalone shader as for RTP terrain / mesh
#include "../../RTP_Base.cginc"

		ENDCG
	} 

	CustomEditor "RTP_CustomShaderGUI"
//	FallBack "Diffuse"
}
