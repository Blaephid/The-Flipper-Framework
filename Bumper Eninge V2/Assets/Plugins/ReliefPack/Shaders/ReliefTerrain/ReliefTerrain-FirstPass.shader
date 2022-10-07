//
// Relief Terrain Shader
// Tomasz Stobierski 2013-2016
//
Shader "Relief Pack/ReliefTerrain-FirstPass" {
Properties {
	_Control ("Control (RGBA)", 2D) = "red" {}
	_Splat3 ("Layer 3 (A)", 2D) = "white" {}
	_Splat2 ("Layer 2 (B)", 2D) = "white" {}
	_Splat1 ("Layer 1 (G)", 2D) = "white" {}
	_Splat0 ("Layer 0 (R)", 2D) = "white" {}
	// used in fallback on old cards
	_MainTex ("BaseMap (RGB)", 2D) = "white" {}
	_Color ("Main Color", Color) = (1,1,1,1)

	// #RTP props
	[HideInInspector] _Control1 ("Control1 (RGBA)", 2D) = "red" {}  
	[HideInInspector] _Control2 ("Control2 (RGBA)", 2D) = "red" {}
	[HideInInspector] _Control3 ("Control3 (RGBA)", 2D) = "red" {} 
	
	[HideInInspector] _SplatAtlasA  ("atlas", 2D) = "black" {}
	[HideInInspector] _SplatAtlasB  ("atlas", 2D) = "black" {}
	[HideInInspector] _SplatAtlasC  ("atlas", 2D) = "black" {}

	[HideInInspector] _SplatA0 ("Detailmap 0 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatA1 ("Detailmap 1 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatA2 ("Detailmap 2 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatA3 ("Detailmap 3 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatB0 ("Detailmap 4 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatB1 ("Detailmap 5 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatB2 ("Detailmap 6 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatB3 ("Detailmap 7 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatC0 ("Detailmap 8 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatC1 ("Detailmap 9 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatC2 ("Detailmap 10 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _SplatC3 ("Detailmap 11 (RGB+A spec)", 2D) = "black" {}
	[HideInInspector] _BumpMap01 ("Bumpmap combined 0+1 (RG+BA)", 2D) = "grey" {}
	[HideInInspector] _BumpMap23 ("Bumpmap combined 2+3 (RG+BA)", 2D) = "grey" {}
	[HideInInspector] _BumpMap45 ("Bumpmap combined 4+5 (RG+BA)", 2D) = "grey" {}
	[HideInInspector] _BumpMap67 ("Bumpmap combined 6+7 (RG+BA)", 2D) = "grey" {}
	[HideInInspector] _BumpMap89 ("Bumpmap combined 8+9 (RG+BA)", 2D) = "grey" {}
	[HideInInspector] _BumpMapAB ("Bumpmap combined 10+11 (RG+BA)", 2D) = "grey" {}
	[HideInInspector] _TERRAIN_HeightMap ("Heightmap combined (RGBA - layers 0-3)", 2D) = "white" {}	
	[HideInInspector] _TERRAIN_HeightMap2 ("Heightmap combined (RGBA - layers 4-7)", 2D) = "white" {}	
	[HideInInspector] _TERRAIN_HeightMap3 ("Heightmap combined (RGBA - layers 8-11)", 2D) = "white" {}	
	
	[HideInInspector] _ColorMapGlobal ("Global colormap (RGBA)", 2D) = "white" {}
	[HideInInspector] _NormalMapGlobal ("Global normalmap (RGBA)", 2D) = "white" {}
	[HideInInspector] _TreesMapGlobal ("Global pixel treesmap (RGBA)", 2D) = "white" {}
	[HideInInspector] _AmbientEmissiveMapGlobal ("Global  ambient emissive map (RGBA)", 2D) = "white" {}
	[HideInInspector] _BumpMapGlobal ("Perlin normal combined w. water & reflection map (RG+B+A)", 2D) = "black" {}

	[HideInInspector] _VerticalTexture ("Vertical texture", 2D) = "grey" {}
	[HideInInspector] TERRAIN_RippleMap ("Water ripplemap", 2D) = "black" {}
	
	[HideInInspector] terrainTileSize ("terrainTileSize", Vector) = (600,200,600,1)

	[HideInInspector] _BumpMapGlobalScale ("", Float) = 1
	[HideInInspector] _GlobalColorMapBlendValues ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorMapSaturation ("", Float) = 1
	[HideInInspector] _GlobalColorMapSaturationFar ("", Float) = 1
	[HideInInspector] _GlobalColorMapBrightness ("", Float) = 1
	[HideInInspector] _GlobalColorMapBrightnessFar ("", Float) = 1
	[HideInInspector] _GlobalColorMapNearMIP ("", Float) = 1
	[HideInInspector] _GlobalColorMapDistortByPerlin ("", Float) = 1
	[HideInInspector] EmissionRefractFiltering ("", Float) = 1
	[HideInInspector] EmissionRefractAnimSpeed ("", Float) = 1
	
	[HideInInspector] _TERRAIN_ReliefTransform ("", Vector) = (1,1,1,1)
	[HideInInspector] _TERRAIN_ReliefTransformTriplanarZ ("", Float) = 1
	[HideInInspector] _TERRAIN_DIST_STEPS ("", Float) = 1
	[HideInInspector] _TERRAIN_WAVELENGTH ("", Float) = 1
	
	[HideInInspector] _blend_multiplier ("", Float) = 1
	
	[HideInInspector] _TERRAIN_ExtrudeHeight ("", Float) = 1
	[HideInInspector] _TERRAIN_LightmapShading ("", Float) = 1
	
	[HideInInspector] _TERRAIN_SHADOW_STEPS ("", Float) = 1
	[HideInInspector] _TERRAIN_WAVELENGTH_SHADOWS ("", Float) = 1
	[HideInInspector] _TERRAIN_SelfShadowStrength ("", Float) = 1
	[HideInInspector] _TERRAIN_ShadowSmoothing ("", Float) = 1
	
	[HideInInspector] rtp_mipoffset_color ("", Float) = 1
	[HideInInspector] rtp_mipoffset_bump ("", Float) = 1
	[HideInInspector] rtp_mipoffset_height ("", Float) = 1
	[HideInInspector] rtp_mipoffset_superdetail ("", Float) = 1
	[HideInInspector] rtp_mipoffset_flow ("", Float) = 1
	[HideInInspector] rtp_mipoffset_ripple ("", Float) = 1
	[HideInInspector] rtp_mipoffset_globalnorm ("", Float) = 1
	[HideInInspector] rtp_mipoffset_caustics ("", Float) = 1
	
	// caustics
	[HideInInspector] TERRAIN_CausticsAnimSpeed ("", Float) = 1
	[HideInInspector] TERRAIN_CausticsColor ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_CausticsTilingScale ("", Float) = 1
	
	//
	// water/wet
	//
	// global
	[HideInInspector] TERRAIN_RippleScale ("", Float) = 1
	[HideInInspector] TERRAIN_FlowScale ("", Float) = 1
	[HideInInspector] TERRAIN_FlowSpeed ("", Float) = 1
	[HideInInspector] TERRAIN_FlowCycleScale ("", Float) = 1
	[HideInInspector] TERRAIN_FlowMipOffset ("", Float) = 1
	
	[HideInInspector] TERRAIN_DropletsSpeed ("", Float) = 1
	[HideInInspector] TERRAIN_WetDarkening ("", Float) = 1
	[HideInInspector] TERRAIN_WetDropletsStrength ("", Float) = 1
	[HideInInspector] TERRAIN_WetHeight_Treshold ("", Float) = 1
	[HideInInspector] TERRAIN_WetHeight_Transition ("", Float) = 1
	
	[HideInInspector] TERRAIN_mipoffset_flowSpeed ("", Float) = 1
	
	[HideInInspector] _TERRAIN_distance_start ("", Float) = 1
	[HideInInspector] _TERRAIN_distance_transition ("", Float) = 1
	
	[HideInInspector] _TERRAIN_distance_start_bumpglobal ("", Float) = 1
	[HideInInspector] _TERRAIN_distance_transition_bumpglobal ("", Float) = 1
	[HideInInspector] rtp_perlin_start_val ("", Float) = 1
	[HideInInspector] _FarNormalDamp ("", Float) = 1
	
	[HideInInspector] _RTP_MIP_BIAS ("", Float) = 1
	
	[HideInInspector] _SuperDetailTiling ("", Float) = 1
	
	[HideInInspector] _VerticalTextureTiling ("", Float) = 1
	[HideInInspector] _VerticalTextureGlobalBumpInfluence ("", Float) = 1
	
	[HideInInspector] RTP_AOamp ("", Float) = 1
	
	[HideInInspector] RTP_AOsharpness ("", Float) = 1
		
		
	// per layer 0-3
	[HideInInspector] _MixScale0123  ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixBlend0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorPerLayer0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerBrightness0123  ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerSaturation0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerBrightness2Spec0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerAlbedo2SpecColor0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixSaturation0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixBrightness0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixReplace0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmission0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionRefractStrength0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionRefractHBedge0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorR0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorG0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorB0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorA0123 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _GlobalColorBottom0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorTop0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorColormapLoSat0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorColormapHiSat0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorLayerLoSat0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorLayerHiSat0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorLoBlend0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorHiBlend0123 ("", Vector) = (1,1,1,1)
	
	//[HideInInspector] _Spec0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _FarSpecCorrection0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _MIPmult0123 ("", Vector) = (1,1,1,1)
	
	// water per layer
	[HideInInspector] TERRAIN_LayerWetStrength0123 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] TERRAIN_WaterLevel0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterLevelSlopeDamp0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterEdge0123 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] TERRAIN_Refraction0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WetRefraction0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_Flow0123 ("", Vector) = (1,1,1,1)
	//[HideInInspector] TERRAIN_WetSpecularity0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WetFlow0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WetGloss0123 ("", Vector) = (1,1,1,1)
	//[HideInInspector] TERRAIN_WaterSpecularity0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterMetallic0123("", Vector) = (1, 1, 1, 1)
	[HideInInspector] TERRAIN_WaterGloss0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterGlossDamper0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterEmission0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorR0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorG0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorB0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorA0123 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _BumpMapGlobalStrength0123 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] PER_LAYER_HEIGHT_MODIFIER0123 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _SuperDetailStrengthMultA0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultB0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthNormal0123 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _SuperDetailStrengthMultASelfMaskNear0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultASelfMaskFar0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultBSelfMaskNear0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultBSelfMaskFar0123 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _VerticalTexture0123 ("", Vector) = (1,1,1,1)
	
	// PBL / IBL
	[HideInInspector] RTP_metallic0123("", Vector) = (1, 1, 1, 1)
	[HideInInspector] RTP_glossMin0123("", Vector) = (1, 1, 1, 1)
	[HideInInspector] RTP_glossMax0123("", Vector) = (1, 1, 1, 1)
	//[HideInInspector] RTP_gloss2mask0123 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_gloss_mult0123 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_gloss_shaping0123 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_Fresnel0123 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_FresnelAtten0123 ("", Vector) = (1,1,1,1)
	[HideInInspector] RTP_DiffFresnel0123 ("", Vector) = (1,1,1,1)
	// IBL
	//[HideInInspector] RTP_IBL_bump_smoothness0123 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_IBL_DiffuseStrength0123 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_IBL_SpecStrength0123 ("", Vector) = (1,1,1,1)
	
	//[HideInInspector] TERRAIN_WaterIBL_SpecWetStrength0123 ("", Vector) = (1,1,1,1)
	//[HideInInspector] TERRAIN_WaterIBL_SpecWaterStrength0123 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] RTP_AO_0123 ("", Vector) = (1,1,1,1)
	
	// per layer  4-7
	[HideInInspector] _MixScale4567  ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixBlend4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorPerLayer4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerBrightness4567  ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerSaturation4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerBrightness2Spec4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerAlbedo2SpecColor4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixSaturation4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixBrightness4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixReplace4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmission4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionRefractStrength4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionRefractHBedge4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorR4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorG4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorB4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorA4567 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _GlobalColorBottom4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorTop4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorColormapLoSat4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorColormapHiSat4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorLayerLoSat4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorLayerHiSat4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorLoBlend4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorHiBlend4567 ("", Vector) = (1,1,1,1)
	
	//[HideInInspector] _Spec4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _FarSpecCorrection4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _MIPmult4567 ("", Vector) = (1,1,1,1)
	
	// water per layer
	[HideInInspector] TERRAIN_LayerWetStrength4567 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] TERRAIN_WaterLevel4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterLevelSlopeDamp4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterEdge4567 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] TERRAIN_Refraction4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WetRefraction4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_Flow4567 ("", Vector) = (1,1,1,1)
	//[HideInInspector] TERRAIN_WetSpecularity4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WetFlow4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WetGloss4567 ("", Vector) = (1,1,1,1)
	//[HideInInspector] TERRAIN_WaterSpecularity4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterMetallic4567("", Vector) = (1, 1, 1, 1)
	[HideInInspector] TERRAIN_WaterGloss4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterGlossDamper4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterEmission4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorR4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorG4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorB4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorA4567 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _BumpMapGlobalStrength4567 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] PER_LAYER_HEIGHT_MODIFIER4567 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _SuperDetailStrengthMultA4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultB4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthNormal4567 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _SuperDetailStrengthMultASelfMaskNear4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultASelfMaskFar4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultBSelfMaskNear4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultBSelfMaskFar4567 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _VerticalTexture4567 ("", Vector) = (1,1,1,1)
	
	//// PBL / IBL
	[HideInInspector] RTP_metallic4567("", Vector) = (1, 1, 1, 1)
	[HideInInspector] RTP_glossMin4567("", Vector) = (1, 1, 1, 1)
	[HideInInspector] RTP_glossMax4567("", Vector) = (1, 1, 1, 1)
	//[HideInInspector] RTP_gloss2mask4567 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_gloss_mult4567 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_gloss_shaping4567 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_Fresnel4567 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_FresnelAtten4567 ("", Vector) = (1,1,1,1)
	[HideInInspector] RTP_DiffFresnel4567 ("", Vector) = (1,1,1,1)
	//// IBL
	//[HideInInspector] RTP_IBL_bump_smoothness4567 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_IBL_DiffuseStrength4567 ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_IBL_SpecStrength4567 ("", Vector) = (1,1,1,1)
	//
	//[HideInInspector] TERRAIN_WaterIBL_SpecWetStrength4567 ("", Vector) = (1,1,1,1)
	//[HideInInspector] TERRAIN_WaterIBL_SpecWaterStrength4567 ("", Vector) = (1,1,1,1)
	
	[HideInInspector] RTP_AO_4567 ("", Vector) = (1,1,1,1)
	
	// per layer 0-3
	[HideInInspector] _MixScale89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixBlend89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorPerLayer89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerBrightness89AB  ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerSaturation89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerBrightness2Spec89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerAlbedo2SpecColor89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixSaturation89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixBrightness89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _MixReplace89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmission89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionRefractStrength89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionRefractHBedge89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorR89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorG89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorB89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _LayerEmissionColorA89AB ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _GlobalColorBottom89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorTop89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorColormapLoSat89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorColormapHiSat89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorLayerLoSat89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorLayerHiSat89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorLoBlend89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _GlobalColorHiBlend89AB ("", Vector) = (1,1,1,1)
	
	//[HideInInspector] _Spec89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _FarSpecCorrection89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _MIPmult89AB ("", Vector) = (1,1,1,1)
	
	// water per layer
	[HideInInspector] TERRAIN_LayerWetStrength89AB ("", Vector) = (1,1,1,1)
	
	[HideInInspector] TERRAIN_WaterLevel89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterLevelSlopeDamp89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterEdge89AB ("", Vector) = (1,1,1,1)
	
	[HideInInspector] TERRAIN_Refraction89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WetRefraction89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_Flow89AB ("", Vector) = (1,1,1,1)
	//[HideInInspector] TERRAIN_WetSpecularity89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WetFlow89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WetGloss89AB ("", Vector) = (1,1,1,1)
	//[HideInInspector] TERRAIN_WaterSpecularity89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterMetallic89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterGloss89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterGlossDamper89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterEmission89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorR89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorG89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorB89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] TERRAIN_WaterColorA89AB ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _BumpMapGlobalStrength89AB ("", Vector) = (1,1,1,1)
	
	[HideInInspector] PER_LAYER_HEIGHT_MODIFIER89AB ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _SuperDetailStrengthMultA89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultB89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthNormal89AB ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _SuperDetailStrengthMultASelfMaskNear89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultASelfMaskFar89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultBSelfMaskNear89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] _SuperDetailStrengthMultBSelfMaskFar89AB ("", Vector) = (1,1,1,1)
	
	[HideInInspector] _VerticalTexture89AB ("", Vector) = (1,1,1,1)
	
	//// PBL / IBL
	[HideInInspector] RTP_metallic89AB("", Vector) = (1, 1, 1, 1)
	[HideInInspector] RTP_glossMin89AB("", Vector) = (1, 1, 1, 1)
	[HideInInspector] RTP_glossMax89AB("", Vector) = (1, 1, 1, 1)
	//[HideInInspector] RTP_gloss2mask89AB ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_gloss_mult89AB ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_gloss_shaping89AB ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_Fresnel89AB ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_FresnelAtten89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] RTP_DiffFresnel89AB ("", Vector) = (1,1,1,1)
	//// IBL
	//[HideInInspector] RTP_IBL_bump_smoothness89AB ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_IBL_DiffuseStrength89AB ("", Vector) = (1,1,1,1)
	//[HideInInspector] RTP_IBL_SpecStrength89AB ("", Vector) = (1,1,1,1)
	//
	//[HideInInspector] TERRAIN_WaterIBL_SpecWetStrength89AB ("", Vector) = (1,1,1,1)
	//[HideInInspector] TERRAIN_WaterIBL_SpecWaterStrength89AB ("", Vector) = (1,1,1,1)
	[HideInInspector] RTP_AO_89AB ("", Vector) = (1,1,1,1)

	// Glitter
	[HideInInspector] _GlitterColor ("", Color) = (1,1,1,1)
	[HideInInspector] _GlitterStrength0123 ("", Vector) = (0, 0, 0, 0)
	[HideInInspector] _GlitterStrength4567 ("", Vector) = (0, 0, 0, 0)
	[HideInInspector] _GlitterStrength89AB ("", Vector) = (0, 0, 0, 0)
	//[HideInInspector] _SnowGlitterColor ("", Color) = (1,1,1,1)
	[HideInInspector] _GlitterTiling("", Float) = 1
	[HideInInspector] _GlitterDensity("", Float) = 0.5
	[HideInInspector] _GlitterFilter("", Float) = 0
	[HideInInspector] _GlitterColorization("", Float) = 0.5
	[HideInInspector] _SparkleMap("", 2D) = "black" {}

	[HideInInspector] _occlusionStrength("", Range(0,1)) = 1


	[HideInInspector] _Phong ("_Phong", Float) = 0.5
	[HideInInspector] _TessSubdivisions ("", Float) = 1
	[HideInInspector] _TessSubdivisionsFar ("", Float) = 1
	[HideInInspector] _TessYOffset ("", Float) = 0


}

CGINCLUDE
#if defined(UNITY_SPECCUBE_BLENDING)
	#undef UNITY_SPECCUBE_BLENDING
	#define UNITY_SPECCUBE_BLENDING 0
#endif
ENDCG

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//
//
// POM / PM / SIMPLE shading
//
//
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
SubShader {
	Tags {
		//"SplatCount" = "4"
		"Queue" = "Geometry-100"
		"RenderType" = "Opaque"
	}
	//LOD 700
	//Fog { Mode Off }
	CGPROGRAM
	#pragma surface surf Standard vertex:vert finalcolor:customFog exclude_path:prepass
	#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
	// U5 fog handling
	#pragma multi_compile_fog	

	#include "UnityCG.cginc"

	#pragma target 3.0
	#pragma glsl
	#pragma exclude_renderers d3d11_9x gles
	#pragma multi_compile RTP_PM_SHADING RTP_SIMPLE_SHADING
	//#define RTP_SIMPLE_SHADING

	// for geom blend (early exit from surf function)
	//#define COLOR_EARLY_EXIT
	// tangents approximation
	#define APPROX_TANGENTS

	#include "RTP_Base.cginc"

	ENDCG

// (used with tessellation)		
/* TESS SHADOW PASS
	Pass {
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		ZWrite On ZTest LEqual

		CGPROGRAM
		// compile directives
		#pragma vertex tessvert_surf
		#pragma fragment frag_surf
		#pragma hull hs_surf
		#pragma domain ds_surf
		#pragma multi_compile_fog	
		#pragma target 5.0
		#pragma glsl
		#pragma multi_compile_shadowcaster
		#include "UnityPBSLighting.cginc"
		#include "HLSLSupport.cginc"
		#include "UnityShaderVariables.cginc"

		#define RTP_PM_SHADING
		#ifndef UNITY_PASS_SHADOWCASTER
			#define UNITY_PASS_SHADOWCASTER
		#endif
		#include "UnityCG.cginc"
		#include "Lighting.cginc"

		#define INTERNAL_DATA
		#define WorldReflectionVector(data,normal) data.worldRefl
		#define WorldNormalVector(data,normal) normal

		#define APPROX_TANGENTS
		#include "RTP_Base.cginc"

			
		#include "RTP_TessShadowCaster.cginc"

		ENDCG

	}
*/ // TESS SHADOW PASS

// (used w/o tessellation)		
///* SHADOW PASSES
	// Pass to render object as a shadow caster
	Pass {
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		Offset 1, 1
		
		Fog {Mode Off}
		ZWrite On ZTest LEqual Cull Off

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing
#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap
#pragma multi_compile_shadowcaster
#include "UnityCG.cginc"
#ifndef UNITY_PASS_SHADOWCASTER
	#define UNITY_PASS_SHADOWCASTER
#endif


#define RTP_CUT_HOLES

#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)
    sampler2D _TerrainHeightmapTexture;
    sampler2D _TerrainNormalmapTexture;
    float4    _TerrainHeightmapRecipSize;   // float4(1.0f/width, 1.0f/height, 1.0f/(width-1), 1.0f/(height-1))
    float4    _TerrainHeightmapScale;       // float4(hmScale.x, hmScale.y / (float)(kMaxHeight), hmScale.z, 0.0f)
#endif

UNITY_INSTANCING_BUFFER_START(Terrain)
    UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData) // float4(xBase, yBase, skipScale, ~)
UNITY_INSTANCING_BUFFER_END(Terrain)

struct v2f { 
	V2F_SHADOW_CASTER;
	#ifdef RTP_CUT_HOLES
		float2  uv : TEXCOORD1;
	#endif
	//UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

uniform float4 _MainTex_ST;

v2f vert( appdata_base v )
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);

#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)

    float2 patchVertex = v.vertex.xy;
    float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);

    float4 uvscale = instanceData.z * _TerrainHeightmapRecipSize;
    float4 uvoffset = instanceData.xyxy * uvscale;
    uvoffset.xy += 0.5f * _TerrainHeightmapRecipSize.xy;
    float2 sampleCoords = (patchVertex.xy * uvscale.xy + uvoffset.xy);

    float hm = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(sampleCoords, 0, 0)));
    v.vertex.xz = (patchVertex.xy + instanceData.xy) * _TerrainHeightmapScale.xz * instanceData.z;  //(x + xBase) * hmScale.x * skipScale;
    v.vertex.y = hm * _TerrainHeightmapScale.y;
    v.vertex.w = 1.0f;

    v.texcoord.xy = (patchVertex.xy * uvscale.zw + uvoffset.zw);

//    #ifdef TERRAIN_INSTANCED_PERPIXEL_NORMAL
//        v.normal = float3(0, 1, 0); // TODO: reconstruct the tangent space in the pixel shader. Seems to be hard with surface shader especially when other attributes are packed together with tSpace.
//        data.tc.zw = sampleCoords;
//    #else
        float3 nor = tex2Dlod(_TerrainNormalmapTexture, float4(sampleCoords, 0, 0)).xyz;
        v.normal = 2.0f * nor - 1.0f;
//    #endif
#endif
	
	//UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
	#ifdef RTP_CUT_HOLES
		o.uv = v.texcoord.xy;
	#endif
	return o;
}

#ifdef RTP_CUT_HOLES
uniform sampler2D _ColorMapGlobal;
#endif

float4 frag( v2f i ) : COLOR
{
	#ifdef RTP_CUT_HOLES
	float4 global_color_value = tex2D(_ColorMapGlobal, i.uv);
	clip(global_color_value.a - 0.001f);
	#endif	
	SHADOW_CASTER_FRAGMENT(i)
}
ENDCG

}

//*/ // SHADOW PASSES

UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"

}
// EOF POM / PM / SIMPLE shading



Dependency "AddPassShader" = "Hidden/Relief Pack/ReliefTerrain-AddPass"
Dependency "BaseMapShader" = "Hidden/Relief Pack/ReliefTerrain-FarOnly"
//Dependency "BaseMapGenShader" = "Hidden/TerrainEngine/Splatmap/Standard-BaseGen"

// Fallback to Unity Standard
Fallback "Nature/Terrain/Standard"
}