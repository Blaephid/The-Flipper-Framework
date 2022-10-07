using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[System.Serializable]
public class ReliefTerrainPresetHolder : ScriptableObject
{
    public string PresetID;
    public string PresetName;
    public string type;

    public int numLayers;

    public TerrainLayer[] terrainLayers;
    public Texture2D[] splats;
    public Texture2D[] splat_atlases;

    public Texture2D controlA;
    public Texture2D controlB;
    public Texture2D controlC;

    public float RTP_MIP_BIAS;
    //public Color _SpecColor;
    //public float RTP_DeferredAddPassSpec=0.5f;

    public float MasterLayerBrightness = 1;
    public float MasterLayerSaturation = 1;
    public float EmissionRefractFiltering = 4;
    public float EmissionRefractAnimSpeed = 4;

    public RTPColorChannels SuperDetailA_channel;
    public RTPColorChannels SuperDetailB_channel;

    public Texture2D Bump01;
    public Texture2D Bump23;
    public Texture2D Bump45;
    public Texture2D Bump67;
    public Texture2D Bump89;
    public Texture2D BumpAB;

    // per terrain object - not part of preset (preset stores only global, shared params)
    public Texture2D ColorGlobal;
    public Texture2D NormalGlobal;
    public Texture2D TreesGlobal;
    public Texture2D AmbientEmissiveMap;
    public Texture2D SSColorCombinedA;
    public Texture2D SSColorCombinedB;

    public bool globalColorModifed_flag;
    public bool globalCombinedModifed_flag;
    public bool globalWaterModifed_flag;

    public Texture2D BumpGlobal;

    public Texture2D BumpGlobalCombined;
    public Texture2D VerticalTexture;
    public float BumpMapGlobalScale;
    public Vector3 GlobalColorMapBlendValues;
    public float GlobalColorMapSaturation;
    public float GlobalColorMapSaturationFar;
    //public float GlobalColorMapSaturationByPerlin;
    public float GlobalColorMapDistortByPerlin = 0.005f;
    public float GlobalColorMapBrightness;
    public float GlobalColorMapBrightnessFar = 1.0f;
    public float _GlobalColorMapNearMIP;
    public float _FarNormalDamp;

    public float blendMultiplier;

    public Texture2D HeightMap;
    public Texture2D HeightMap2;
    public Texture2D HeightMap3;

    public Vector4 ReliefTransform;
    public float DIST_STEPS;
    public float WAVELENGTH;
    public float ReliefBorderBlend;

    public float ExtrudeHeight;
    public float LightmapShading;

    public float SHADOW_STEPS;
    public float WAVELENGTH_SHADOWS;
    //public float SHADOW_SMOOTH_STEPS; // not used since RTP3.2d
    public float SelfShadowStrength;
    public float ShadowSmoothing;
    public float ShadowSoftnessFade = 0.7f; // sice RTP3.2d

    public float distance_start;
    public float distance_transition;
    public float distance_start_bumpglobal;
    public float distance_transition_bumpglobal;
    public float rtp_perlin_start_val;

    public float _Phong = 0;
    public float tessHeight = 300;
    public float _TessSubdivisions = 1;
    public float _TessSubdivisionsFar = 1;
    public float _TessYOffset = 0;

    public float trees_shadow_distance_start;
    public float trees_shadow_distance_transition;
    public float trees_shadow_value;

    public float trees_pixel_distance_start;
    public float trees_pixel_distance_transition;
    public float trees_pixel_blend_val;

    public float global_normalMap_multiplier;
    public float global_normalMap_farUsage;

    public float _AmbientEmissiveMultiplier;
    public float _AmbientEmissiveRelief;

    public int rtp_mipoffset_globalnorm;
    public float _SuperDetailTiling;
    public Texture2D SuperDetailA;
    public Texture2D SuperDetailB;

    // reflection
    public Texture2D TERRAIN_ReflectionMap;
    public RTPColorChannels TERRAIN_ReflectionMap_channel;
    public Color TERRAIN_ReflColorA;
    public Color TERRAIN_ReflColorB;
    public Color TERRAIN_ReflColorC;
    public float TERRAIN_ReflColorCenter;
    public float TERRAIN_ReflGlossAttenuation;
    public float TERRAIN_ReflectionRotSpeed;

    // water/wet
    public float TERRAIN_GlobalWetness;

    public Texture2D TERRAIN_RippleMap;
    public Texture2D TERRAIN_WetMask;
    public float TERRAIN_RippleScale;
    public float TERRAIN_FlowScale;
    public float TERRAIN_FlowSpeed;
    public float TERRAIN_FlowCycleScale;
    public float TERRAIN_FlowMipOffset;
    public float TERRAIN_WetDarkening;
    public float TERRAIN_WetDropletsStrength;
    public float TERRAIN_WetHeight_Treshold;
    public float TERRAIN_WetHeight_Transition;

    public float TERRAIN_RainIntensity;
    public float TERRAIN_DropletsSpeed;

    public float TERRAIN_mipoffset_flowSpeed;

    public float TERRAIN_CausticsAnimSpeed;
    public Color TERRAIN_CausticsColor;
    public float TERRAIN_CausticsWaterLevel;
    public float TERRAIN_CausticsWaterLevelByAngle;
    public float TERRAIN_CausticsWaterDeepFadeLength;
    public float TERRAIN_CausticsWaterShallowFadeLength;
    public float TERRAIN_CausticsTilingScale;
    public Texture2D TERRAIN_CausticsTex;

    public Vector4 RTP_LightDefVector;

    public float RTP_AOamp;
    public float RTP_AOsharpness;

    public float _occlusionStrength;

    //////////////////////
    // layer_dependent arrays
    //////////////////////
    public Texture2D[] Bumps;
    public float[] Spec;
    public float[] FarSpecCorrection;
    public float[] MixScale;
    public float[] MixBlend;
    public float[] MixSaturation;

    public float[] RTP_DiffFresnel;
    public float[] RTP_metallic;
    public float[] RTP_glossMin;
    public float[] RTP_glossMax;
    public float[] RTP_glitter;

    public float[] MixBrightness;
    public float[] MixReplace;
    public float[] LayerBrightness;
    public float[] LayerBrightness2Spec;
    public float[] LayerAlbedo2SpecColor;
    public float[] LayerSaturation;
    public float[] LayerEmission;
    public Color[] LayerEmissionColor;
    public float[] LayerEmissionRefractStrength;
    public float[] LayerEmissionRefractHBedge;

    public float[] GlobalColorPerLayer;
    public float[] GlobalColorBottom;
    public float[] GlobalColorTop;
    public float[] GlobalColorColormapLoSat;
    public float[] GlobalColorColormapHiSat;
    public float[] GlobalColorLayerLoSat;
    public float[] GlobalColorLayerHiSat;
    public float[] GlobalColorLoBlend;
    public float[] GlobalColorHiBlend;

    public float[] PER_LAYER_HEIGHT_MODIFIER;
    public float[] _SuperDetailStrengthMultA;
    public float[] _SuperDetailStrengthMultASelfMaskNear;
    public float[] _SuperDetailStrengthMultASelfMaskFar;
    public float[] _SuperDetailStrengthMultB;
    public float[] _SuperDetailStrengthMultBSelfMaskNear;
    public float[] _SuperDetailStrengthMultBSelfMaskFar;
    public float[] _SuperDetailStrengthNormal;
    public float[] _BumpMapGlobalStrength;

    public float[] VerticalTextureStrength;
    public float[] AO_strength;

    public float VerticalTextureGlobalBumpInfluence;
    public float VerticalTextureTiling;

    public Texture2D[] Heights;

    public float[] _snow_strength_per_layer;
#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
    public ProceduralMaterial[] Substances;
#endif

    // wet
    public float[] TERRAIN_LayerWetStrength;
    public float[] TERRAIN_WaterLevel;
    public float[] TERRAIN_WaterLevelSlopeDamp;
    public float[] TERRAIN_WaterEdge;
    public float[] TERRAIN_WaterGloss;
    public float[] TERRAIN_WaterGlossDamper;
    public float[] TERRAIN_Refraction;
    public float[] TERRAIN_WetRefraction;
    public float[] TERRAIN_Flow;
    public float[] TERRAIN_WetFlow;
    public float[] TERRAIN_WaterMetallic;
    public float[] TERRAIN_WetGloss;
    public Color[] TERRAIN_WaterColor;
    public float[] TERRAIN_WaterEmission;

    // snow
    public float _snow_strength;
    public float _global_color_brightness_to_snow;
    public float _snow_slope_factor;
    public float _snow_edge_definition;
    public float _snow_height_treshold;
    public float _snow_height_transition;
    public Color _snow_color;
    public float _snow_gloss;
    public float _snow_reflectivness;
    public float _snow_deep_factor;
    public float _snow_fresnel;
    public float _snow_diff_fresnel;

    public float _snow_metallic;
    public float _snow_Frost;
    public float _snow_MicroTiling;
    public float _snow_BumpMicro;
    public float _snow_occlusionStrength;
    public int _snow_TranslucencyDeferredLightIndex;
    public Color _SnowGlitterColor;

    // glitter
    public Color _GlitterColor;
    public float[] _GlitterStrength;
    public float _GlitterTiling;
    public float _GlitterDensity;
    public float _GlitterFilter;
    public float _GlitterColorization;
    public Texture2D _SparkleMap;

    public void Init(string name)
    {
        PresetID = "" + Random.value + Time.realtimeSinceStartup;
        PresetName = name;
    }

#if UNITY_EDITOR
    //	private void CheckAssetRef(Texture2D tex, string[] warning_msgs, ref int cnt, string msg) {
    //		if (tex && AssetDatabase.GetAssetPath(tex)=="") {
    //			warning_msgs[cnt]=msg;
    //			cnt++;
    //		}
    //	}
    //
    //	public void CheckUnsavedTextures() {
    //		string[] warning_msgs=new string[100];
    //		int cnt=0;
    //		if (splats!=null) {
    //			for(int i=0; i<splats.Length; i++) {
    //				CheckAssetRef(splats[i], warning_msgs, ref cnt, "Detail diffuse/spec texture for layer "+i);
    //			}
    //		}
    //		if (splat_atlases!=null) {
    //			for(int i=0; i<splat_atlases.Length; i++) {
    //				CheckAssetRef(splat_atlases[i], warning_msgs, ref cnt, "Detail atlas texture "+((i==0) ? "A":"B"));
    //			}
    //		}
    //		if (Bumps!=null) {
    //			for(int i=0; i<Bumps.Length; i++) {
    //				CheckAssetRef(Bumps[i], warning_msgs, ref cnt, "Normals texture for layer "+i);
    //			}
    //		}
    //		if (Heights!=null) {
    //			for(int i=0; i<Heights.Length; i++) {
    //				CheckAssetRef(Heights[i], warning_msgs, ref cnt, "Heights texture for layer "+i);
    //			}
    //		}
    //	
    //		CheckAssetRef(controlA, warning_msgs, ref cnt, "Control (coverage) map 0");
    //		CheckAssetRef(controlB, warning_msgs, ref cnt, "Control (coverage) map 1");
    //		CheckAssetRef(controlC, warning_msgs, ref cnt, "Control (coverage) map 2");
    //		
    //		CheckAssetRef(Bump01, warning_msgs, ref cnt, "Combined normalmaps 0+1");
    //		CheckAssetRef(Bump23, warning_msgs, ref cnt, "Combined normalmaps 2+3");
    //		CheckAssetRef(Bump45, warning_msgs, ref cnt, "Combined normalmaps 4+5");
    //		CheckAssetRef(Bump67, warning_msgs, ref cnt, "Combined normalmaps 6+7");
    //		CheckAssetRef(Bump89, warning_msgs, ref cnt, "Combined normalmaps 8+9");
    //		CheckAssetRef(BumpAB, warning_msgs, ref cnt, "Combined normalmaps 10+11");
    //		
    //		CheckAssetRef(ColorGlobal, warning_msgs, ref cnt, "Global colormap");
    //		CheckAssetRef(NormalGlobal, warning_msgs, ref cnt, "Global normalmap");
    //		CheckAssetRef(TreesGlobal, warning_msgs, ref cnt, "Global pixel trees/shadow map");
    //	
    //		CheckAssetRef(SSColorCombined, warning_msgs, ref cnt, "Simple mode combined greyscale details");
    //	
    //		CheckAssetRef(BumpGlobal, warning_msgs, ref cnt, "Perlin normals texture");
    //		CheckAssetRef(BumpGlobalCombined, warning_msgs, ref cnt, "Special perlin combined texture (with optional water mask&reflections).");
    //	
    //		CheckAssetRef(VerticalTexture, warning_msgs, ref cnt, "Vertical texture");
    //		
    //		CheckAssetRef(HeightMap, warning_msgs, ref cnt, "Combined heightmaps 0");
    //		CheckAssetRef(HeightMap2, warning_msgs, ref cnt, "Combined heightmaps 1");
    //		CheckAssetRef(HeightMap3, warning_msgs, ref cnt, "Combined heightmaps 2");
    //	
    //		CheckAssetRef(SuperDetailA, warning_msgs, ref cnt, "Superdetail mult mask A");
    //		CheckAssetRef(SuperDetailB, warning_msgs, ref cnt, "Superdetail mult mask B");
    //	
    //		CheckAssetRef(TERRAIN_ReflectionMap, warning_msgs, ref cnt, "Reflection map");
    //		CheckAssetRef(TERRAIN_RippleMap, warning_msgs, ref cnt, "Water ripple animation texture");
    //		CheckAssetRef(TERRAIN_WetMask, warning_msgs, ref cnt, "Water wetmask");
    //				
    //		CheckAssetRef(TERRAIN_CausticsTex, warning_msgs, ref cnt, "Caustics texture");
    //
    //		if (cnt>0) {
    //			Debug.LogWarning("List of textures that are not saved as assets in project - they'll be missing in saved preset:");
    //		}
    //		for(int i=0; i<cnt; i++) {
    //			Debug.LogWarning(warning_msgs[i]);
    //		}
    //	}
#endif
}
