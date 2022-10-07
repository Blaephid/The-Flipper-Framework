using System;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ReliefTerrainGlobalSettingsHolder
{

    public int numTiles = 0;

    public int numLayers;

    public TerrainLayer[] terrainLayers;

    [System.NonSerialized]
    public bool dont_check_weak_references = false;
    [System.NonSerialized]
    public bool dont_check_for_interfering_terrain_replacement_shaders = false;
    [System.NonSerialized]
    public Texture2D[] bakeJobArray;

    public Texture2D[] splats;
    public Texture2D[] splat_atlases = new Texture2D[3];
    public string save_path_atlasA = "";
    public string save_path_atlasB = "";
    public string save_path_atlasC = "";
    public string save_path_terrain_steepness = "";
    public string save_path_terrain_height = "";
    public string save_path_terrain_direction = "";
    public string save_path_Bump01 = "";
    public string save_path_Bump23 = "";
    public string save_path_Bump45 = "";
    public string save_path_Bump67 = "";
    public string save_path_Bump89 = "";
    public string save_path_BumpAB = "";
    public string save_path_HeightMap = "";
    public string save_path_HeightMap2 = "";
    public string save_path_HeightMap3 = "";
    public string save_path_SSColorCombinedA = "";
    public string save_path_SSColorCombinedB = "";

    public string newPresetName = "a preset name...";

    public Texture2D activateObject;
    private GameObject _RTP_LODmanager;

    public RTP_LODmanager _RTP_LODmanagerScript;

    //public bool super_simple_active=false;
    public float RTP_MIP_BIAS = 0;
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
    public Texture2D BumpGlobal;
    public int BumpGlobalCombinedSize = 1024;

    public Texture2D SSColorCombinedA;
    public Texture2D SSColorCombinedB;

    public Texture2D VerticalTexture;
    public float BumpMapGlobalScale;
    public Vector3 GlobalColorMapBlendValues;
    public float _GlobalColorMapNearMIP;
    public float GlobalColorMapSaturation;
    public float GlobalColorMapSaturationFar = 1;
    //public float GlobalColorMapSaturationByPerlin=0.2f;
    public float GlobalColorMapDistortByPerlin = 0.005f;
    public float GlobalColorMapBrightness;
    public float GlobalColorMapBrightnessFar = 1.0f;
    public float _FarNormalDamp;

    public float blendMultiplier;

    public Vector3 terrainTileSize;

    public Texture2D HeightMap;
    public Vector4 ReliefTransform;
    public float DIST_STEPS;
    public float WAVELENGTH;
    public float ReliefBorderBlend;

    public float ExtrudeHeight;
    public float LightmapShading;

    public float RTP_AOsharpness;
    public float RTP_AOamp;
    public float _occlusionStrength=1;

    public float SHADOW_STEPS;
    public float WAVELENGTH_SHADOWS;
    //public float SHADOW_SMOOTH_STEPS; // not used in RTP3.2d
    public float SelfShadowStrength;
    public float ShadowSmoothing;
    public float ShadowSoftnessFade = 0.8f; // new in RTP3.2d

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
    public float global_normalMap_farUsage = 0;

    public float _AmbientEmissiveMultiplier = 1;
    public float _AmbientEmissiveRelief = 0.5f;

    public Texture2D HeightMap2;
    public Texture2D HeightMap3;
    public int rtp_mipoffset_globalnorm;
    public float _SuperDetailTiling;
    public Texture2D SuperDetailA;
    public Texture2D SuperDetailB;

    // water/wet
    public float TERRAIN_GlobalWetness;

    public Texture2D TERRAIN_RippleMap;
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

    // caustics
    public float TERRAIN_CausticsAnimSpeed;
    public Color TERRAIN_CausticsColor;
    public GameObject TERRAIN_CausticsWaterLevelRefObject;
    public float TERRAIN_CausticsWaterLevel;
    public float TERRAIN_CausticsWaterLevelByAngle;
    public float TERRAIN_CausticsWaterDeepFadeLength;
    public float TERRAIN_CausticsWaterShallowFadeLength;
    public float TERRAIN_CausticsTilingScale;
    public Texture2D TERRAIN_CausticsTex;
    //
    //public Color rtp_customAmbientCorrection;
    //public Cubemap _CubemapDiff;
    //public float TERRAIN_IBL_DiffAO_Damp=0.25f;
    //public Cubemap _CubemapSpec;
    //public float TERRAIN_IBLRefl_SpecAO_Damp=0.5f;
    //	
    public Vector4 RTP_LightDefVector;
    //public Color RTP_ReflexLightDiffuseColor;
    //public Color RTP_ReflexLightDiffuseColor2;
    //public Color RTP_ReflexLightSpecColor;


    //////////////////////
    // layer_dependent arrays
    //////////////////////
    public Texture2D[] Bumps;
    //public float[] Spec; // RTP3.1 - mnoznik o.Gloss (range 0-4)
    public float[] FarSpecCorrection; // RTP3.1 zmiana nazwy z FarGlossCorrection
    public float[] MIPmult;
    public float[] MixScale;
    public float[] MixBlend;
    public float[] MixSaturation;

    ///////////////////////////////////////////////////
    // RTP3.1
    //public float[] RTP_gloss2mask;
    //public float[] RTP_gloss_mult;
    //public float[] RTP_gloss_shaping;
    //public float[] RTP_Fresnel;
    //public float[] RTP_FresnelAtten;
    public float[] RTP_DiffFresnel;
    //public float[] RTP_IBL_bump_smoothness;
    //public float[] RTP_IBL_DiffuseStrength;
    //public float[] RTP_IBL_SpecStrength;
    //public float[] _DeferredSpecDampAddPass;
    public float[] RTP_metallic;
    public float[] RTP_glossMin;
    public float[] RTP_glossMax;
    public float[] RTP_glitter;

    // adv global colormap blending
    public float[] GlobalColorBottom;
    public float[] GlobalColorTop;
    public float[] GlobalColorColormapLoSat;
    public float[] GlobalColorColormapHiSat;
    public float[] GlobalColorLayerLoSat;
    public float[] GlobalColorLayerHiSat;
    public float[] GlobalColorLoBlend;
    public float[] GlobalColorHiBlend;
    ///////////////////////////////////////////////////

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
    public float[] PER_LAYER_HEIGHT_MODIFIER;
    public float[] _SuperDetailStrengthMultA;
    public float[] _SuperDetailStrengthMultASelfMaskNear;
    public float[] _SuperDetailStrengthMultASelfMaskFar;
    public float[] _SuperDetailStrengthMultB;
    public float[] _SuperDetailStrengthMultBSelfMaskNear;
    public float[] _SuperDetailStrengthMultBSelfMaskFar;
    public float[] _SuperDetailStrengthNormal;
    public float[] _BumpMapGlobalStrength;

    public float[] AO_strength = new float[12] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

    public float[] VerticalTextureStrength;
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
    //public float[] TERRAIN_WaterSpecularity;
    public float[] TERRAIN_WaterGloss;
    public float[] TERRAIN_WaterGlossDamper;
    public float[] TERRAIN_Refraction;
    public float[] TERRAIN_WetRefraction;
    public float[] TERRAIN_Flow;
    public float[] TERRAIN_WetFlow;
    //public float[] TERRAIN_WetSpecularity;
    public float[] TERRAIN_WaterMetallic;
    public float[] TERRAIN_WetGloss;
    public Color[] TERRAIN_WaterColor;
    //public float[] TERRAIN_WaterIBL_SpecWetStrength;
    //public float[] TERRAIN_WaterIBL_SpecWaterStrength;
    public float[] TERRAIN_WaterEmission;

    ////////////////////
    public float _snow_strength;
    public float _global_color_brightness_to_snow;
    public float _snow_slope_factor;
    public float _snow_edge_definition;
    public float _snow_height_treshold;
    public float _snow_height_transition;
    public Color _snow_color;
    //public float _snow_specular;
    public float _snow_gloss;
    public float _snow_reflectivness;
    public float _snow_deep_factor;
    //public float _snow_fresnel;
    public float _snow_diff_fresnel;
    //public float _snow_IBL_DiffuseStrength;
    //public float _snow_IBL_SpecStrength;

    public float _snow_metallic;
    public float _snow_Frost=0;
    public float _snow_MicroTiling=1;
    public float _snow_BumpMicro=0.2f;
    public Color _SnowGlitterColor=new Color(1,1,1,0.1f);
    public float _snow_occlusionStrength=0.5f;
    public int _snow_TranslucencyDeferredLightIndex=0;

    // glitter
    public Color _GlitterColor;
    public float[] _GlitterStrength;
    public float _GlitterTiling;
    public float _GlitterDensity;
    public float _GlitterFilter;
    public float _GlitterColorization;
    public Texture2D _SparkleMap;



    public bool _4LAYERS_SHADER_USED = false;

    public bool flat_dir_ref = true;
    public bool flip_dir_ref = true;
    public GameObject direction_object;

    public bool show_details = false;
    public bool show_details_main = false;
    public bool show_details_atlasing = false;
    public bool show_details_layers = false;
    public bool show_details_uv_blend = false;

    public bool show_controlmaps = false;
    public bool show_controlmaps_build = false;
    public bool show_controlmaps_helpers = false;
    public bool show_controlmaps_highcost = false;
    public bool show_controlmaps_splats = false;

    public bool show_vert_texture = false;

    public bool show_global_color = false;

    public bool show_snow = false;

    public bool show_global_bump = false;
    public bool show_global_bump_normals = false;
    public bool show_global_bump_superdetail = false;

    public ReliefTerrainMenuItems submenu = (ReliefTerrainMenuItems)(0);
    public ReliefTerrainSettingsItems submenu_settings = (ReliefTerrainSettingsItems)(0);
    public ReliefTerrainDerivedTexturesItems submenu_derived_textures = (ReliefTerrainDerivedTexturesItems)(0);
    public ReliefTerrainControlTexturesItems submenu_control_textures = (ReliefTerrainControlTexturesItems)(0);
    public bool show_global_wet_settings = false;
    public bool show_global_reflection_settings = false;
    public int show_active_layer = 0;

    public bool show_derivedmaps = false;

    public bool show_settings = false;

    // paint
    public bool undo_flag = false;
    public bool paint_flag = false;
    public float paint_size = 0.5f;
    public float paint_smoothness = 0;
    public float paint_opacity = 1;
    public Color paintColor = new Color(0.5f, 0.3f, 0, 0);
    public bool preserveBrightness = true;
    public bool paint_alpha_flag = false;
    public bool paint_wetmask = false;
    //private Transform underlying_transform;
    //private MeshRenderer underlying_renderer;
    public RaycastHit paintHitInfo;
    public bool paintHitInfo_flag;

    public bool cut_holes = false;

    private Texture2D dumb_tex;

    public Color[] paintColorSwatches;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    // constructor - init arrays
    //
    public ReliefTerrainGlobalSettingsHolder()
    {
        const int cnt = 12;

        Bumps = new Texture2D[cnt];
        Heights = new Texture2D[cnt];

        //Spec=new float[cnt];
        FarSpecCorrection = new float[cnt];
        MIPmult = new float[cnt];
        MixScale = new float[cnt];
        MixBlend = new float[cnt];
        MixSaturation = new float[cnt];

        // RTP3.1
        //RTP_gloss2mask=new float[cnt];
        //RTP_gloss_mult=new float[cnt];
        //RTP_gloss_shaping=new float[cnt];
        //RTP_Fresnel=new float[cnt];
        //RTP_FresnelAtten=new float[cnt];
        RTP_DiffFresnel = new float[cnt];
        //RTP_IBL_bump_smoothness=new float[cnt];
        //RTP_IBL_DiffuseStrength=new float[cnt];
        //RTP_IBL_SpecStrength=new float[cnt];
        //_DeferredSpecDampAddPass=new float[cnt];
        RTP_metallic = new float[cnt];
        RTP_glossMin = new float[cnt];
        RTP_glossMax = new float[cnt];
        RTP_glitter = new float[cnt];

        MixBrightness = new float[cnt];
        MixReplace = new float[cnt];
        LayerBrightness = new float[cnt];
        LayerBrightness2Spec = new float[cnt];
        LayerAlbedo2SpecColor = new float[cnt];
        LayerSaturation = new float[cnt];
        LayerEmission = new float[cnt];
        LayerEmissionColor = new Color[cnt];
        LayerEmissionRefractStrength = new float[cnt];
        LayerEmissionRefractHBedge = new float[cnt];

        GlobalColorPerLayer = new float[cnt];
        GlobalColorBottom = new float[cnt];
        GlobalColorTop = new float[cnt];
        GlobalColorColormapLoSat = new float[cnt];
        GlobalColorColormapHiSat = new float[cnt];
        GlobalColorLayerLoSat = new float[cnt];
        GlobalColorLayerHiSat = new float[cnt];
        GlobalColorLoBlend = new float[cnt];
        GlobalColorHiBlend = new float[cnt];

        PER_LAYER_HEIGHT_MODIFIER = new float[cnt];
        _snow_strength_per_layer = new float[cnt];
#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
        Substances = new ProceduralMaterial[cnt];
#endif

        _SuperDetailStrengthMultA = new float[cnt];
        _SuperDetailStrengthMultASelfMaskNear = new float[cnt];
        _SuperDetailStrengthMultASelfMaskFar = new float[cnt];
        _SuperDetailStrengthMultB = new float[cnt];
        _SuperDetailStrengthMultBSelfMaskNear = new float[cnt];
        _SuperDetailStrengthMultBSelfMaskFar = new float[cnt];
        _SuperDetailStrengthNormal = new float[cnt];
        _BumpMapGlobalStrength = new float[cnt];

        AO_strength = new float[cnt];
        VerticalTextureStrength = new float[cnt];

        TERRAIN_LayerWetStrength = new float[cnt];
        TERRAIN_WaterLevel = new float[cnt];
        TERRAIN_WaterLevelSlopeDamp = new float[cnt];
        TERRAIN_WaterEdge = new float[cnt];
        TERRAIN_WaterGloss = new float[cnt];
        TERRAIN_WaterGlossDamper = new float[cnt];
        TERRAIN_Refraction = new float[cnt];
        TERRAIN_WetRefraction = new float[cnt];
        TERRAIN_Flow = new float[cnt];
        TERRAIN_WetFlow = new float[cnt];
        TERRAIN_WaterMetallic = new float[cnt];
        TERRAIN_WetGloss = new float[cnt];
        TERRAIN_WaterColor = new Color[cnt];

        TERRAIN_WaterEmission = new float[cnt];

        _GlitterStrength = new float[cnt];

#if UNITY_EDITOR
        ReturnToDefaults();
#endif
    }

    public void ReInit(Terrain terrainComp)
    {
        if (terrainComp.terrainData.terrainLayers.Length > numLayers)
        {
            Texture2D[] splats_new = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
            Texture2D[] Bumps_new = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
            for (int i = 0; i < splats.Length; i++)
            {
                splats_new[i] = splats[i];
                Bumps_new[i] = Bumps[i];
            }
            splats = splats_new;
            Bumps = Bumps_new;
            splats[terrainComp.terrainData.terrainLayers.Length - 1] = terrainComp.terrainData.terrainLayers[((terrainComp.terrainData.terrainLayers.Length - 2) >= 0) ? (terrainComp.terrainData.terrainLayers.Length - 2) : 0].diffuseTexture;
            Bumps[terrainComp.terrainData.terrainLayers.Length - 1] = terrainComp.terrainData.terrainLayers[((terrainComp.terrainData.terrainLayers.Length - 2) >= 0) ? (terrainComp.terrainData.terrainLayers.Length - 2) : 0].normalMapTexture;
        }
        else if (terrainComp.terrainData.terrainLayers.Length < numLayers)
        {
            Texture2D[] splats_new = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
            Texture2D[] Bumps_new = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
            for (int i = 0; i < splats_new.Length; i++)
            {
                splats_new[i] = splats[i];
                Bumps_new[i] = Bumps[i];
            }
            splats = splats_new;
            Bumps = Bumps_new;
        }
        numLayers = terrainComp.terrainData.terrainLayers.Length;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public Material use_mat = null;
    public void SetShaderParam(string name, Texture2D tex)
    {
        if (!tex) return;
        if (use_mat)
        {
            use_mat.SetTexture(name, tex);
        }
        else
        {
            Shader.SetGlobalTexture(name, tex);
        }
    }
    public void SetShaderParam(string name, Cubemap tex)
    {
        if (!tex) return;
        if (use_mat)
        {
            use_mat.SetTexture(name, tex);
        }
        else
        {
            Shader.SetGlobalTexture(name, tex);
        }
    }
    public void SetShaderParam(string name, Matrix4x4 mtx)
    {
        if (use_mat)
        {
            use_mat.SetMatrix(name, mtx);
        }
        else
        {
            Shader.SetGlobalMatrix(name, mtx);
        }
    }
    public void SetShaderParam(string name, Vector4 vec)
    {
        if (use_mat)
        {
            use_mat.SetVector(name, vec);
        }
        else
        {
            Shader.SetGlobalVector(name, vec);
        }
    }
    public void SetShaderParam(string name, float val)
    {
        if (use_mat)
        {
            use_mat.SetFloat(name, val);
        }
        else
        {
            Shader.SetGlobalFloat(name, val);
        }
    }
    public void SetShaderParam(string name, Color col)
    {
        if (use_mat)
        {
            use_mat.SetColor(name, col);
        }
        else
        {
            Shader.SetGlobalColor(name, col);
        }
    }

    public RTP_LODmanager Get_RTP_LODmanagerScript()
    {
        return _RTP_LODmanagerScript;
    }

    private void CheckLightScriptForDefered()
    {
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        Light aLight = null;
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i].type == LightType.Directional)
            {
                if ((lights[i].gameObject.GetComponent<ReliefShaders_applyLightForDeferred>()) == null)
                {
                    // potential light to attach the script
                    aLight = lights[i];
                }
                else
                {
                    // at least one light with the script attached
                    return;
                }
            }
        }
        if (aLight)
        {
            // at least one directional light found and none of directional lights have the script attached
            // attach component then
            ReliefShaders_applyLightForDeferred comp = aLight.gameObject.AddComponent(typeof(ReliefShaders_applyLightForDeferred)) as ReliefShaders_applyLightForDeferred;
            comp.lightForSelfShadowing = aLight;
        }
    }

    public void RefreshAll()
    {
        CheckLightScriptForDefered();

        ReliefTerrain[] rts = GameObject.FindObjectsOfType(typeof(ReliefTerrain)) as ReliefTerrain[];
        for (int i = 0; i < rts.Length; i++)
        {
            if (rts[i].globalSettingsHolder != null)
            {
                Terrain ter = rts[i].GetComponent(typeof(Terrain)) as Terrain;
                if (ter)
                {
                    rts[i].globalSettingsHolder.Refresh(ter.materialTemplate);
                }
                else
                {
                    rts[i].globalSettingsHolder.Refresh(rts[i].GetComponent<Renderer>().sharedMaterial);
                }
                rts[i].RefreshTextures();
            }
        }
        GeometryVsTerrainBlend[] blnd = GameObject.FindObjectsOfType(typeof(GeometryVsTerrainBlend)) as GeometryVsTerrainBlend[];
        for (int i = 0; i < blnd.Length; i++)
        {
            blnd[i].SetupValues();
        }
    }

    public void Refresh(Material mat = null, ReliefTerrain rt_caller = null)
    {
        if (splats == null) return;
#if UNITY_EDITOR
        if (_RTP_LODmanager == null)
        {
            if ((_RTP_LODmanager = GameObject.Find("_RTP_LODmanager")) == null)
            {
                _RTP_LODmanager = new GameObject("_RTP_LODmanager");
                _RTP_LODmanager.AddComponent(typeof(RTP_LODmanager));
                _RTP_LODmanagerScript = (RTP_LODmanager)_RTP_LODmanager.GetComponent(typeof(RTP_LODmanager));
                EditorUtility.DisplayDialog("RTP Notification", "_RTP_LODmanager object added to the scene.\nIts script handles LOD properties of RTP shaders.", "OK");
                Selection.activeObject = _RTP_LODmanager;
            }
        }
        if (_RTP_LODmanagerScript == null)
        {
            _RTP_LODmanagerScript = (RTP_LODmanager)_RTP_LODmanager.GetComponent(typeof(RTP_LODmanager));
        }
        _4LAYERS_SHADER_USED = _RTP_LODmanagerScript.RTP_4LAYERS_MODE;

        //colorSpaceLinear = (UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear);
#endif
        // switch for SetShaderParam - when use_mat defined we're injecting param into material
        if (mat == null && rt_caller != null)
        {
            if (rt_caller.globalSettingsHolder == this)
            {
                Terrain ter = rt_caller.GetComponent(typeof(Terrain)) as Terrain;
                if (ter)
                {
                    rt_caller.globalSettingsHolder.Refresh(ter.materialTemplate);
                }
                else
                {
                    if (rt_caller.GetComponent<Renderer>() != null && rt_caller.GetComponent<Renderer>().sharedMaterial != null)
                    {
                        rt_caller.globalSettingsHolder.Refresh(rt_caller.GetComponent<Renderer>().sharedMaterial);
                    }
                }
            }
        }

        use_mat = mat;

        if (mat!= null)
        {
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        for (int i = 0; i < numLayers; i++)
        {
            if (i < 4)
            {
                SetShaderParam("_SplatA" + i, splats[i]);
            }
            else if (i < 8)
            {
                if (_4LAYERS_SHADER_USED)
                {
                    SetShaderParam("_SplatC" + (i - 4), splats[i]);
                    // potrzebne przy sniegu (firstpass moze korzystac z koloru i bumpmap 4-7)
                    SetShaderParam("_SplatB" + (i - 4), splats[i]);
                }
                else
                {
                    SetShaderParam("_SplatB" + (i - 4), splats[i]);
                }
            }
            else if (i < 12)
            {
                SetShaderParam("_SplatC" + (i - 4), splats[i]);
            }
        }

        // > RTP3.1
        // update-set to default
        //if (CheckAndUpdate(ref RTP_gloss2mask, 0.5f, numLayers)) {
        //	for(int k=0; k<numLayers; k++) {
        //		Spec[k]=1; // zresetuj od razu mnoznik glossa (RTP3.1 - zmienna ma inne znaczenie)
        //	}
        //}
        //CheckAndUpdate(ref RTP_gloss_mult, 1f, numLayers);
        //CheckAndUpdate(ref RTP_gloss_shaping, 0.5f, numLayers);
        //CheckAndUpdate(ref RTP_Fresnel, 0, numLayers);
        //CheckAndUpdate(ref RTP_FresnelAtten, 0, numLayers);
        CheckAndUpdate(ref RTP_DiffFresnel, 0, numLayers);
        //CheckAndUpdate(ref RTP_IBL_bump_smoothness, 0.7f, numLayers);
        //CheckAndUpdate(ref RTP_IBL_DiffuseStrength, 0.5f, numLayers);
        //CheckAndUpdate(ref RTP_IBL_SpecStrength, 0.5f, numLayers);
        //CheckAndUpdate(ref _DeferredSpecDampAddPass, 1f, numLayers);
        CheckAndUpdate(ref RTP_metallic, 0f, numLayers);
        CheckAndUpdate(ref RTP_glossMin, 0f, numLayers);
        CheckAndUpdate(ref RTP_glossMax, 1f, numLayers);
        CheckAndUpdate(ref RTP_glitter, 0f, numLayers);

        //CheckAndUpdate(ref TERRAIN_WaterSpecularity, 0.5f, numLayers);
        CheckAndUpdate(ref TERRAIN_WaterGloss, 0.1f, numLayers);
        CheckAndUpdate(ref TERRAIN_WaterGlossDamper, 0f, numLayers);
        //CheckAndUpdate(ref TERRAIN_WetSpecularity, 0.2f, numLayers);
        CheckAndUpdate(ref TERRAIN_WaterMetallic, 0.1f, numLayers);
        CheckAndUpdate(ref TERRAIN_WetGloss, 0.05f, numLayers);
        CheckAndUpdate(ref TERRAIN_WetFlow, 0.05f, numLayers);

        CheckAndUpdate(ref MixBrightness, 2.0f, numLayers);
        CheckAndUpdate(ref MixReplace, 0.0f, numLayers);
        CheckAndUpdate(ref LayerBrightness, 1.0f, numLayers);
        CheckAndUpdate(ref LayerBrightness2Spec, 0.0f, numLayers);
        CheckAndUpdate(ref LayerAlbedo2SpecColor, 0.0f, numLayers);
        CheckAndUpdate(ref LayerSaturation, 1.0f, numLayers);
        CheckAndUpdate(ref LayerEmission, 0f, numLayers);
        CheckAndUpdate(ref FarSpecCorrection, 0f, numLayers);
        CheckAndUpdate(ref LayerEmissionColor, Color.black, numLayers);
        CheckAndUpdate(ref LayerEmissionRefractStrength, 0, numLayers);
        CheckAndUpdate(ref LayerEmissionRefractHBedge, 0, numLayers);

        //CheckAndUpdate(ref TERRAIN_WaterIBL_SpecWetStrength, 0.1f, numLayers);
        //CheckAndUpdate(ref TERRAIN_WaterIBL_SpecWaterStrength, 0.5f, numLayers);
        CheckAndUpdate(ref TERRAIN_WaterEmission, 0f, numLayers);

        CheckAndUpdate(ref _GlitterStrength, 0f, numLayers);

        /////////////////////////////////////////////////////////////////////
        //
        // layer independent
        //
        /////////////////////////////////////////////////////////////////////

        // custom fog (unity's fog doesn't work with this shader - too many texture interpolators)
        //if (RenderSettings.fog)
        //{
        //    Shader.SetGlobalFloat("_Fdensity", RenderSettings.fogDensity);
        //    if (colorSpaceLinear)
        //    {
        //        Shader.SetGlobalColor("_FColor", RenderSettings.fogColor.linear);
        //    }
        //    else
        //    {
        //        Shader.SetGlobalColor("_FColor", RenderSettings.fogColor);
        //    }
        //    Shader.SetGlobalFloat("_Fstart", RenderSettings.fogStartDistance);
        //    Shader.SetGlobalFloat("_Fend", RenderSettings.fogEndDistance);
        //}
        //else
        //{
        //    Shader.SetGlobalFloat("_Fdensity", 0);
        //    Shader.SetGlobalFloat("_Fstart", 1000000);
        //    Shader.SetGlobalFloat("_Fend", 2000000);
        //}


        SetShaderParam("terrainTileSize", terrainTileSize);

        SetShaderParam("RTP_AOamp", RTP_AOamp);
        SetShaderParam("RTP_AOsharpness", RTP_AOsharpness);
        SetShaderParam("_occlusionStrength", _occlusionStrength);

        SetShaderParam("EmissionRefractFiltering", EmissionRefractFiltering);
        SetShaderParam("EmissionRefractAnimSpeed", EmissionRefractAnimSpeed);

        // global
        SetShaderParam("_VerticalTexture", VerticalTexture);

        SetShaderParam("_GlobalColorMapBlendValues", GlobalColorMapBlendValues);
        SetShaderParam("_GlobalColorMapSaturation", GlobalColorMapSaturation);
        SetShaderParam("_GlobalColorMapSaturationFar", GlobalColorMapSaturationFar);
        //SetShaderParam("_GlobalColorMapSaturationByPerlin", GlobalColorMapSaturationByPerlin);
        SetShaderParam("_GlobalColorMapDistortByPerlin", GlobalColorMapDistortByPerlin);

        SetShaderParam("_GlobalColorMapBrightness", GlobalColorMapBrightness);
        SetShaderParam("_GlobalColorMapBrightnessFar", GlobalColorMapBrightnessFar);
        SetShaderParam("_GlobalColorMapNearMIP", _GlobalColorMapNearMIP);

        SetShaderParam("_RTP_MIP_BIAS", RTP_MIP_BIAS);

        SetShaderParam("_BumpMapGlobalScale", BumpMapGlobalScale);
        SetShaderParam("_FarNormalDamp", _FarNormalDamp);

        //SetShaderParam("_SpecColor", _SpecColor);
        //SetShaderParam("RTP_DeferredAddPassSpec", RTP_DeferredAddPassSpec);

        SetShaderParam("_blend_multiplier", blendMultiplier);
        SetShaderParam("_TERRAIN_ReliefTransform", ReliefTransform);

        SetShaderParam("_TERRAIN_ReliefTransformTriplanarZ", ReliefTransform.x);
        SetShaderParam("_TERRAIN_DIST_STEPS", DIST_STEPS);
        SetShaderParam("_TERRAIN_WAVELENGTH", WAVELENGTH);

        SetShaderParam("_TERRAIN_ExtrudeHeight", ExtrudeHeight);
        SetShaderParam("_TERRAIN_LightmapShading", LightmapShading);

        SetShaderParam("_TERRAIN_SHADOW_STEPS", SHADOW_STEPS);
        SetShaderParam("_TERRAIN_WAVELENGTH_SHADOWS", WAVELENGTH_SHADOWS);
        //SetShaderParam("_TERRAIN_SHADOW_SMOOTH_STEPS", SHADOW_SMOOTH_STEPS); // not used since RTP3.2d

        SetShaderParam("_TERRAIN_SelfShadowStrength", SelfShadowStrength);
        SetShaderParam("_TERRAIN_ShadowSmoothing", (1 - ShadowSmoothing) * 6); // changed range in RTP3.2d
        SetShaderParam("_TERRAIN_ShadowSoftnessFade", ShadowSoftnessFade); // new in RTP3.2d

        SetShaderParam("_TERRAIN_distance_start", distance_start);
        SetShaderParam("_TERRAIN_distance_transition", distance_transition);

        SetShaderParam("_TERRAIN_distance_start_bumpglobal", distance_start_bumpglobal);
        SetShaderParam("_TERRAIN_distance_transition_bumpglobal", distance_transition_bumpglobal);
        SetShaderParam("rtp_perlin_start_val", rtp_perlin_start_val);

        Shader.SetGlobalVector("_TERRAIN_trees_shadow_values", new Vector4(trees_shadow_distance_start, trees_shadow_distance_transition, trees_shadow_value, global_normalMap_multiplier));
        Shader.SetGlobalVector("_TERRAIN_trees_pixel_values", new Vector4(trees_pixel_distance_start, trees_pixel_distance_transition, trees_pixel_blend_val, global_normalMap_farUsage));

        SetShaderParam("_Phong", _Phong);
        SetShaderParam("_TessSubdivisions", _TessSubdivisions);
        SetShaderParam("_TessSubdivisionsFar", _TessSubdivisionsFar);
        SetShaderParam("_TessYOffset", _TessYOffset);

        Shader.SetGlobalFloat("_AmbientEmissiveMultiplier", _AmbientEmissiveMultiplier);
        Shader.SetGlobalFloat("_AmbientEmissiveRelief", _AmbientEmissiveRelief);

        SetShaderParam("_SuperDetailTiling", _SuperDetailTiling);

        Shader.SetGlobalFloat("rtp_snow_strength", _snow_strength);
        Shader.SetGlobalFloat("rtp_global_color_brightness_to_snow", _global_color_brightness_to_snow);
        Shader.SetGlobalFloat("rtp_snow_slope_factor", _snow_slope_factor);
        Shader.SetGlobalFloat("rtp_snow_edge_definition", _snow_edge_definition);
        Shader.SetGlobalFloat("rtp_snow_height_treshold", _snow_height_treshold);
        Shader.SetGlobalFloat("rtp_snow_height_transition", _snow_height_transition);
        Shader.SetGlobalColor("rtp_snow_color", _snow_color);
        //Shader.SetGlobalFloat("rtp_snow_specular", _snow_specular);
        Shader.SetGlobalFloat("rtp_snow_gloss", _snow_gloss);
        Shader.SetGlobalFloat("rtp_snow_reflectivness", _snow_reflectivness);
        Shader.SetGlobalFloat("rtp_snow_deep_factor", _snow_deep_factor);
        //Shader.SetGlobalFloat("rtp_snow_fresnel", _snow_fresnel);
        Shader.SetGlobalFloat("rtp_snow_diff_fresnel", _snow_diff_fresnel);

        //Shader.SetGlobalFloat("rtp_snow_IBL_DiffuseStrength", _snow_IBL_DiffuseStrength);
        //Shader.SetGlobalFloat("rtp_snow_IBL_SpecStrength", _snow_IBL_SpecStrength);

        Shader.SetGlobalFloat("rtp_snow_metallic", _snow_metallic);
        Shader.SetGlobalFloat("rtp_snow_Frost", _snow_Frost);
        Shader.SetGlobalFloat("rtp_snow_MicroTiling", _snow_MicroTiling);
        Shader.SetGlobalFloat("rtp_snow_BumpMicro", _snow_BumpMicro);
        Shader.SetGlobalFloat("rtp_snow_occlusionStrength", _snow_occlusionStrength);
        Shader.SetGlobalFloat("rtp_snow_TranslucencyDeferredLightIndex", (float)_snow_TranslucencyDeferredLightIndex);
        Shader.SetGlobalColor("_SnowGlitterColor", _SnowGlitterColor);

        // caustics
        SetShaderParam("TERRAIN_CausticsAnimSpeed", TERRAIN_CausticsAnimSpeed);
        SetShaderParam("TERRAIN_CausticsColor", TERRAIN_CausticsColor);
        if (TERRAIN_CausticsWaterLevelRefObject) TERRAIN_CausticsWaterLevel = TERRAIN_CausticsWaterLevelRefObject.transform.position.y;
        Shader.SetGlobalFloat("TERRAIN_CausticsWaterLevel", TERRAIN_CausticsWaterLevel);
        Shader.SetGlobalFloat("TERRAIN_CausticsWaterLevelByAngle", TERRAIN_CausticsWaterLevelByAngle);
        Shader.SetGlobalFloat("TERRAIN_CausticsWaterDeepFadeLength", TERRAIN_CausticsWaterDeepFadeLength);
        Shader.SetGlobalFloat("TERRAIN_CausticsWaterShallowFadeLength", TERRAIN_CausticsWaterShallowFadeLength);
        SetShaderParam("TERRAIN_CausticsTilingScale", TERRAIN_CausticsTilingScale);
        SetShaderParam("TERRAIN_CausticsTex", TERRAIN_CausticsTex);

        SetShaderParam("_GlitterColor", _GlitterColor);
        SetShaderParam("_GlitterTiling", _GlitterTiling);
        SetShaderParam("_GlitterDensity", _GlitterDensity);
        SetShaderParam("_GlitterFilter", _GlitterFilter);
        SetShaderParam("_GlitterColorization", _GlitterColorization);
        SetShaderParam("_SparkleMap", _SparkleMap);

        if (numLayers > 0)
        {
            int tex_width = 512;
            for (int i = 0; i < numLayers; i++)
            {
                if (splats[i])
                {
                    tex_width = splats[i].width;
                    break;
                }
            }
            SetShaderParam("rtp_mipoffset_color", -Mathf.Log(1024.0f / tex_width) / Mathf.Log(2));
            if (Bump01 != null)
            {
                tex_width = Bump01.width;
            }
            SetShaderParam("rtp_mipoffset_bump", -Mathf.Log(1024.0f / tex_width) / Mathf.Log(2));
            if (HeightMap)
            {
                tex_width = HeightMap.width;
            }
            else if (HeightMap2)
            {
                tex_width = HeightMap2.width;
            }
            else if (HeightMap3)
            {
                tex_width = HeightMap3.width;
            }
            SetShaderParam("rtp_mipoffset_height", -Mathf.Log(1024.0f / tex_width) / Mathf.Log(2));

            tex_width = BumpGlobalCombinedSize;
            SetShaderParam("rtp_mipoffset_globalnorm", -Mathf.Log(1024.0f / (tex_width * BumpMapGlobalScale)) / Mathf.Log(2) + rtp_mipoffset_globalnorm);
            SetShaderParam("rtp_mipoffset_superdetail", -Mathf.Log(1024.0f / (tex_width * _SuperDetailTiling)) / Mathf.Log(2));
            SetShaderParam("rtp_mipoffset_flow", -Mathf.Log(1024.0f / (tex_width * TERRAIN_FlowScale)) / Mathf.Log(2) + TERRAIN_FlowMipOffset);
            if (TERRAIN_RippleMap)
            {
                tex_width = TERRAIN_RippleMap.width;
            }
            SetShaderParam("rtp_mipoffset_ripple", -Mathf.Log(1024.0f / (tex_width * TERRAIN_RippleScale)) / Mathf.Log(2));
            if (TERRAIN_CausticsTex)
            {
                tex_width = TERRAIN_CausticsTex.width;
            }
            SetShaderParam("rtp_mipoffset_caustics", -Mathf.Log(1024.0f / (tex_width * TERRAIN_CausticsTilingScale)) / Mathf.Log(2));
        }

        //SetShaderParam("TERRAIN_GlobalWetness", TERRAIN_GlobalWetness); // (breaks setting wet level by another script somehow)
        Shader.SetGlobalFloat("TERRAIN_GlobalWetness", TERRAIN_GlobalWetness);
        SetShaderParam("TERRAIN_RippleMap", TERRAIN_RippleMap);
        SetShaderParam("TERRAIN_RippleScale", TERRAIN_RippleScale);
        SetShaderParam("TERRAIN_FlowScale", TERRAIN_FlowScale);
        SetShaderParam("TERRAIN_FlowMipOffset", TERRAIN_FlowMipOffset);
        SetShaderParam("TERRAIN_FlowSpeed", TERRAIN_FlowSpeed);
        SetShaderParam("TERRAIN_FlowCycleScale", TERRAIN_FlowCycleScale);
        Shader.SetGlobalFloat("TERRAIN_RainIntensity", TERRAIN_RainIntensity);
        SetShaderParam("TERRAIN_DropletsSpeed", TERRAIN_DropletsSpeed);
        SetShaderParam("TERRAIN_WetDropletsStrength", TERRAIN_WetDropletsStrength);
        SetShaderParam("TERRAIN_WetDarkening", TERRAIN_WetDarkening);
        SetShaderParam("TERRAIN_mipoffset_flowSpeed", TERRAIN_mipoffset_flowSpeed);
        SetShaderParam("TERRAIN_WetHeight_Treshold", TERRAIN_WetHeight_Treshold);
        SetShaderParam("TERRAIN_WetHeight_Transition", TERRAIN_WetHeight_Transition);

        //Shader.SetGlobalVector("rtp_customAmbientCorrection", new Vector4(rtp_customAmbientCorrection.r-0.2f, rtp_customAmbientCorrection.g-0.2f, rtp_customAmbientCorrection.b-0.2f, 0)*0.1f);
        //SetShaderParam("_CubemapDiff", _CubemapDiff);
        //SetShaderParam("_CubemapSpec", _CubemapSpec);

        //Shader.SetGlobalFloat("TERRAIN_IBL_DiffAO_Damp", TERRAIN_IBL_DiffAO_Damp);
        //Shader.SetGlobalFloat("TERRAIN_IBLRefl_SpecAO_Damp", TERRAIN_IBLRefl_SpecAO_Damp);

        Shader.SetGlobalVector("RTP_LightDefVector", RTP_LightDefVector);
        //Shader.SetGlobalFloat("RTP_BackLightStrength", RTP_LightDefVector.x);
        //Shader.SetGlobalFloat("RTP_ReflexLightDiffuseSoftness", RTP_LightDefVector.y);
        //Shader.SetGlobalFloat("RTP_ReflexLightSpecSoftness", RTP_LightDefVector.z);
        //Shader.SetGlobalFloat("RTP_ReflexLightSpecularity", RTP_LightDefVector.w);
        //Shader.SetGlobalColor("RTP_ReflexLightDiffuseColor1", RTP_ReflexLightDiffuseColor);
        //Shader.SetGlobalColor("RTP_ReflexLightDiffuseColor2", RTP_ReflexLightDiffuseColor2);
        //Shader.SetGlobalColor("RTP_ReflexLightSpecColor", RTP_ReflexLightSpecColor);

        SetShaderParam("_VerticalTextureGlobalBumpInfluence", VerticalTextureGlobalBumpInfluence);
        SetShaderParam("_VerticalTextureTiling", VerticalTextureTiling);

        /////////////////////////////////////////////////////////////////////
        //
        // layer dependent numeric
        //
        /////////////////////////////////////////////////////////////////////
        //float[] tmp_RTP_gloss_mult=new float[RTP_gloss_mult.Length];
        //for(int k=0; k<tmp_RTP_gloss_mult.Length; k++) {
        //	if (gloss_baked[k]!=null && gloss_baked[k].baked) {
        //		tmp_RTP_gloss_mult[k]=1;
        //	} else {
        //		tmp_RTP_gloss_mult[k]=RTP_gloss_mult[k];
        //	}
        //}
        //float[] tmp_RTP_gloss_shaping=new float[RTP_gloss_shaping.Length];
        //for(int k=0; k<tmp_RTP_gloss_shaping.Length; k++) {
        //	if (gloss_baked[k]!=null && gloss_baked[k].baked) {
        //		tmp_RTP_gloss_shaping[k]=0.5f;
        //	} else {
        //		tmp_RTP_gloss_shaping[k]=RTP_gloss_shaping[k];
        //	}
        //}
        //SetShaderParam("_Spec0123", getVector(Spec, 0,3));
        SetShaderParam("_FarSpecCorrection0123", getVector(FarSpecCorrection, 0, 3));
        SetShaderParam("_MIPmult0123", getVector(MIPmult, 0, 3));
        SetShaderParam("_MixScale0123", getVector(MixScale, 0, 3));
        SetShaderParam("_MixBlend0123", getVector(MixBlend, 0, 3));
        SetShaderParam("_MixSaturation0123", getVector(MixSaturation, 0, 3));

        // RTP3.1
        //SetShaderParam("RTP_gloss2mask0123", getVector(RTP_gloss2mask, 0,3));
        //SetShaderParam("RTP_gloss_mult0123", getVector(tmp_RTP_gloss_mult, 0,3));
        //SetShaderParam("RTP_gloss_shaping0123", getVector(tmp_RTP_gloss_shaping, 0,3));
        //SetShaderParam("RTP_Fresnel0123", getVector(RTP_Fresnel, 0,3));
        //SetShaderParam("RTP_FresnelAtten0123", getVector(RTP_FresnelAtten, 0,3));
        SetShaderParam("RTP_DiffFresnel0123", getVector(RTP_DiffFresnel, 0, 3));
        //SetShaderParam("RTP_IBL_bump_smoothness0123", getVector(RTP_IBL_bump_smoothness, 0,3));
        //SetShaderParam("RTP_IBL_DiffuseStrength0123", getVector(RTP_IBL_DiffuseStrength, 0,3));
        //SetShaderParam("RTP_IBL_SpecStrength0123", getVector(RTP_IBL_SpecStrength, 0,3));
        SetShaderParam("RTP_metallic0123", getVector(RTP_metallic, 0, 3));
        SetShaderParam("RTP_glossMin0123", getVector(RTP_glossMin, 0, 3));
        SetShaderParam("RTP_glossMax0123", getVector(RTP_glossMax, 0, 3));

        SetShaderParam("RTP_glitter0123", getVector(RTP_glitter, 0, 3));

        // (only in deferred addpass)
        //SetShaderParam("_DeferredSpecDampAddPass0123", getVector(_DeferredSpecDampAddPass, 0,3));

        SetShaderParam("_MixBrightness0123", getVector(MixBrightness, 0, 3));
        SetShaderParam("_MixReplace0123", getVector(MixReplace, 0, 3));
        SetShaderParam("_LayerBrightness0123", MasterLayerBrightness * getVector(LayerBrightness, 0, 3));
        SetShaderParam("_LayerSaturation0123", MasterLayerSaturation * getVector(LayerSaturation, 0, 3));
        SetShaderParam("_LayerEmission0123", getVector(LayerEmission, 0, 3));
        SetShaderParam("_LayerEmissionColorR0123", getColorVector(LayerEmissionColor, 0, 3, 0));
        SetShaderParam("_LayerEmissionColorG0123", getColorVector(LayerEmissionColor, 0, 3, 1));
        SetShaderParam("_LayerEmissionColorB0123", getColorVector(LayerEmissionColor, 0, 3, 2));
        SetShaderParam("_LayerEmissionColorA0123", getColorVector(LayerEmissionColor, 0, 3, 3));
        SetShaderParam("_LayerBrightness2Spec0123", getVector(LayerBrightness2Spec, 0, 3));
        SetShaderParam("_LayerAlbedo2SpecColor0123", getVector(LayerAlbedo2SpecColor, 0, 3));
        SetShaderParam("_LayerEmissionRefractStrength0123", getVector(LayerEmissionRefractStrength, 0, 3));
        SetShaderParam("_LayerEmissionRefractHBedge0123", getVector(LayerEmissionRefractHBedge, 0, 3));

        SetShaderParam("_GlobalColorPerLayer0123", getVector(GlobalColorPerLayer, 0, 3));

        SetShaderParam("_GlobalColorBottom0123", getVector(GlobalColorBottom, 0, 3));
        SetShaderParam("_GlobalColorTop0123", getVector(GlobalColorTop, 0, 3));
        SetShaderParam("_GlobalColorColormapLoSat0123", getVector(GlobalColorColormapLoSat, 0, 3));
        SetShaderParam("_GlobalColorColormapHiSat0123", getVector(GlobalColorColormapHiSat, 0, 3));
        SetShaderParam("_GlobalColorLayerLoSat0123", getVector(GlobalColorLayerLoSat, 0, 3));
        SetShaderParam("_GlobalColorLayerHiSat0123", getVector(GlobalColorLayerHiSat, 0, 3));
        SetShaderParam("_GlobalColorLoBlend0123", getVector(GlobalColorLoBlend, 0, 3));
        SetShaderParam("_GlobalColorHiBlend0123", getVector(GlobalColorHiBlend, 0, 3));

        SetShaderParam("PER_LAYER_HEIGHT_MODIFIER0123", getVector(PER_LAYER_HEIGHT_MODIFIER, 0, 3));

        SetShaderParam("rtp_snow_strength_per_layer0123", getVector(_snow_strength_per_layer, 0, 3));

        SetShaderParam("_SuperDetailStrengthMultA0123", getVector(_SuperDetailStrengthMultA, 0, 3));
        SetShaderParam("_SuperDetailStrengthMultB0123", getVector(_SuperDetailStrengthMultB, 0, 3));
        SetShaderParam("_SuperDetailStrengthNormal0123", getVector(_SuperDetailStrengthNormal, 0, 3));
        SetShaderParam("_BumpMapGlobalStrength0123", getVector(_BumpMapGlobalStrength, 0, 3));

        SetShaderParam("_SuperDetailStrengthMultASelfMaskNear0123", getVector(_SuperDetailStrengthMultASelfMaskNear, 0, 3));
        SetShaderParam("_SuperDetailStrengthMultASelfMaskFar0123", getVector(_SuperDetailStrengthMultASelfMaskFar, 0, 3));
        SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear0123", getVector(_SuperDetailStrengthMultBSelfMaskNear, 0, 3));
        SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar0123", getVector(_SuperDetailStrengthMultBSelfMaskFar, 0, 3));

        SetShaderParam("TERRAIN_LayerWetStrength0123", getVector(TERRAIN_LayerWetStrength, 0, 3));
        SetShaderParam("TERRAIN_WaterLevel0123", getVector(TERRAIN_WaterLevel, 0, 3));
        SetShaderParam("TERRAIN_WaterLevelSlopeDamp0123", getVector(TERRAIN_WaterLevelSlopeDamp, 0, 3));
        SetShaderParam("TERRAIN_WaterEdge0123", getVector(TERRAIN_WaterEdge, 0, 3));
        SetShaderParam("TERRAIN_WaterGloss0123", getVector(TERRAIN_WaterGloss, 0, 3));
        SetShaderParam("TERRAIN_WaterGlossDamper0123", getVector(TERRAIN_WaterGlossDamper, 0, 3));
        SetShaderParam("TERRAIN_Refraction0123", getVector(TERRAIN_Refraction, 0, 3));
        SetShaderParam("TERRAIN_WetRefraction0123", getVector(TERRAIN_WetRefraction, 0, 3));
        SetShaderParam("TERRAIN_Flow0123", getVector(TERRAIN_Flow, 0, 3));
        SetShaderParam("TERRAIN_WetFlow0123", getVector(TERRAIN_WetFlow, 0, 3));
        SetShaderParam("TERRAIN_WaterMetallic0123", getVector(TERRAIN_WaterMetallic, 0,3));
        SetShaderParam("TERRAIN_WetGloss0123", getVector(TERRAIN_WetGloss, 0, 3));
        SetShaderParam("TERRAIN_WaterColorR0123", getColorVector(TERRAIN_WaterColor, 0, 3, 0));
        SetShaderParam("TERRAIN_WaterColorG0123", getColorVector(TERRAIN_WaterColor, 0, 3, 1));
        SetShaderParam("TERRAIN_WaterColorB0123", getColorVector(TERRAIN_WaterColor, 0, 3, 2));
        SetShaderParam("TERRAIN_WaterColorA0123", getColorVector(TERRAIN_WaterColor, 0, 3, 3));
        SetShaderParam("TERRAIN_WaterEmission0123", getVector(TERRAIN_WaterEmission, 0, 3));

        SetShaderParam("_GlitterStrength0123", getVector(_GlitterStrength, 0, 3));

        SetShaderParam("RTP_AO_0123", getVector(AO_strength, 0, 3));
        SetShaderParam("_VerticalTexture0123", getVector(VerticalTextureStrength, 0, 3));

        if ((numLayers > 4) && _4LAYERS_SHADER_USED)
        {
            //
            // przekieruj parametry warstw 4-7 na AddPass
            //
            //SetShaderParam("_Spec89AB", getVector(Spec, 4,7));
            SetShaderParam("_FarSpecCorrection89AB", getVector(FarSpecCorrection, 4, 7));
            SetShaderParam("_MIPmult89AB", getVector(MIPmult, 4, 7));
            SetShaderParam("_MixScale89AB", getVector(MixScale, 4, 7));
            SetShaderParam("_MixBlend89AB", getVector(MixBlend, 4, 7));
            SetShaderParam("_MixSaturation89AB", getVector(MixSaturation, 4, 7));

            // RTP3.1
            //SetShaderParam("RTP_gloss2mask89AB", getVector(RTP_gloss2mask, 4, 7));
            //SetShaderParam("RTP_gloss_mult89AB", getVector(tmp_RTP_gloss_mult, 4, 7));
            //SetShaderParam("RTP_gloss_shaping89AB", getVector(tmp_RTP_gloss_shaping, 4, 7));
            //SetShaderParam("RTP_Fresnel89AB", getVector(RTP_Fresnel, 4, 7));
            //SetShaderParam("RTP_FresnelAtten89AB", getVector(RTP_FresnelAtten, 4, 7));
            SetShaderParam("RTP_DiffFresnel89AB", getVector(RTP_DiffFresnel, 4, 7));
            //SetShaderParam("RTP_IBL_bump_smoothness89AB", getVector(RTP_IBL_bump_smoothness, 4, 7));
            //SetShaderParam("RTP_IBL_DiffuseStrength89AB", getVector(RTP_IBL_DiffuseStrength, 4, 7));
            //SetShaderParam("RTP_IBL_SpecStrength89AB", getVector(RTP_IBL_SpecStrength, 4, 7));
            //SetShaderParam("_DeferredSpecDampAddPass89AB", getVector(_DeferredSpecDampAddPass, 4,7));
            SetShaderParam("RTP_metallic89AB", getVector(RTP_metallic, 4, 7));
            SetShaderParam("RTP_glossMin89AB", getVector(RTP_glossMin, 4, 7));
            SetShaderParam("RTP_glossMax89AB", getVector(RTP_glossMax, 4, 7));

            SetShaderParam("RTP_glitter89AB", getVector(RTP_glitter, 4, 7));


            SetShaderParam("_MixBrightness89AB", getVector(MixBrightness, 4, 7));
            SetShaderParam("_MixReplace89AB", getVector(MixReplace, 4, 7));
            SetShaderParam("_LayerBrightness89AB", MasterLayerBrightness * getVector(LayerBrightness, 4, 7));
            SetShaderParam("_LayerSaturation89AB", MasterLayerSaturation * getVector(LayerSaturation, 4, 7));
            SetShaderParam("_LayerEmission89AB", getVector(LayerEmission, 4, 7));
            SetShaderParam("_LayerEmissionColorR89AB", getColorVector(LayerEmissionColor, 4, 7, 0));
            SetShaderParam("_LayerEmissionColorG89AB", getColorVector(LayerEmissionColor, 4, 7, 1));
            SetShaderParam("_LayerEmissionColorB89AB", getColorVector(LayerEmissionColor, 4, 7, 2));
            SetShaderParam("_LayerEmissionColorA89AB", getColorVector(LayerEmissionColor, 4, 7, 3));
            SetShaderParam("_LayerBrightness2Spec89AB", getVector(LayerBrightness2Spec, 4, 7));
            SetShaderParam("_LayerAlbedo2SpecColor89AB", getVector(LayerAlbedo2SpecColor, 4, 7));
            SetShaderParam("_LayerEmissionRefractStrength89AB", getVector(LayerEmissionRefractStrength, 4, 7));
            SetShaderParam("_LayerEmissionRefractHBedge89AB", getVector(LayerEmissionRefractHBedge, 4, 7));

            SetShaderParam("_GlobalColorPerLayer89AB", getVector(GlobalColorPerLayer, 4, 7));

            SetShaderParam("_GlobalColorBottom89AB", getVector(GlobalColorBottom, 4, 7));
            SetShaderParam("_GlobalColorTop89AB", getVector(GlobalColorTop, 4, 7));
            SetShaderParam("_GlobalColorColormapLoSat89AB", getVector(GlobalColorColormapLoSat, 4, 7));
            SetShaderParam("_GlobalColorColormapHiSat89AB", getVector(GlobalColorColormapHiSat, 4, 7));
            SetShaderParam("_GlobalColorLayerLoSat89AB", getVector(GlobalColorLayerLoSat, 4, 7));
            SetShaderParam("_GlobalColorLayerHiSat89AB", getVector(GlobalColorLayerHiSat, 4, 7));
            SetShaderParam("_GlobalColorLoBlend89AB", getVector(GlobalColorLoBlend, 4, 7));
            SetShaderParam("_GlobalColorHiBlend89AB", getVector(GlobalColorHiBlend, 4, 7));

            SetShaderParam("PER_LAYER_HEIGHT_MODIFIER89AB", getVector(PER_LAYER_HEIGHT_MODIFIER, 4, 7));

            SetShaderParam("rtp_snow_strength_per_layer89AB", getVector(_snow_strength_per_layer, 4, 7));

            SetShaderParam("_SuperDetailStrengthMultA89AB", getVector(_SuperDetailStrengthMultA, 4, 7));
            SetShaderParam("_SuperDetailStrengthMultB89AB", getVector(_SuperDetailStrengthMultB, 4, 7));
            SetShaderParam("_SuperDetailStrengthNormal89AB", getVector(_SuperDetailStrengthNormal, 4, 7));
            SetShaderParam("_BumpMapGlobalStrength89AB", getVector(_BumpMapGlobalStrength, 4, 7));

            SetShaderParam("_SuperDetailStrengthMultASelfMaskNear89AB", getVector(_SuperDetailStrengthMultASelfMaskNear, 4, 7));
            SetShaderParam("_SuperDetailStrengthMultASelfMaskFar89AB", getVector(_SuperDetailStrengthMultASelfMaskFar, 4, 7));
            SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear89AB", getVector(_SuperDetailStrengthMultBSelfMaskNear, 4, 7));
            SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar89AB", getVector(_SuperDetailStrengthMultBSelfMaskFar, 4, 7));

            SetShaderParam("TERRAIN_LayerWetStrength89AB", getVector(TERRAIN_LayerWetStrength, 4, 7));
            SetShaderParam("TERRAIN_WaterLevel89AB", getVector(TERRAIN_WaterLevel, 4, 7));
            SetShaderParam("TERRAIN_WaterLevelSlopeDamp89AB", getVector(TERRAIN_WaterLevelSlopeDamp, 4, 7));
            SetShaderParam("TERRAIN_WaterEdge89AB", getVector(TERRAIN_WaterEdge, 4, 7));
            SetShaderParam("TERRAIN_WaterGloss89AB", getVector(TERRAIN_WaterGloss, 4, 7));
            SetShaderParam("TERRAIN_WaterGlossDamper89AB", getVector(TERRAIN_WaterGlossDamper, 4, 7));
            SetShaderParam("TERRAIN_Refraction89AB", getVector(TERRAIN_Refraction, 4, 7));
            SetShaderParam("TERRAIN_WetRefraction89AB", getVector(TERRAIN_WetRefraction, 4, 7));
            SetShaderParam("TERRAIN_Flow89AB", getVector(TERRAIN_Flow, 4, 7));
            SetShaderParam("TERRAIN_WetFlow89AB", getVector(TERRAIN_WetFlow, 4, 7));
            SetShaderParam("TERRAIN_WaterMetallic89AB", getVector(TERRAIN_WaterMetallic, 4, 7));
            SetShaderParam("TERRAIN_WetGloss89AB", getVector(TERRAIN_WetGloss, 4, 7));
            SetShaderParam("TERRAIN_WaterColorR89AB", getColorVector(TERRAIN_WaterColor, 4, 7, 0));
            SetShaderParam("TERRAIN_WaterColorG89AB", getColorVector(TERRAIN_WaterColor, 4, 7, 1));
            SetShaderParam("TERRAIN_WaterColorB89AB", getColorVector(TERRAIN_WaterColor, 4, 7, 2));
            SetShaderParam("TERRAIN_WaterColorA89AB", getColorVector(TERRAIN_WaterColor, 4, 7, 3));
            SetShaderParam("TERRAIN_WaterEmission89AB", getVector(TERRAIN_WaterEmission, 4, 7));

            SetShaderParam("_GlitterStrength89AB", getVector(_GlitterStrength, 4, 7));

            SetShaderParam("RTP_AO_89AB", getVector(AO_strength, 4, 7));
            SetShaderParam("_VerticalTexture89AB", getVector(VerticalTextureStrength, 4, 7));
        }
        else
        {
            //SetShaderParam("_Spec4567", getVector(Spec, 4,7));
            SetShaderParam("_FarSpecCorrection4567", getVector(FarSpecCorrection, 4, 7));
            SetShaderParam("_MIPmult4567", getVector(MIPmult, 4, 7));
            SetShaderParam("_MixScale4567", getVector(MixScale, 4, 7));
            SetShaderParam("_MixBlend4567", getVector(MixBlend, 4, 7));
            SetShaderParam("_MixSaturation4567", getVector(MixSaturation, 4, 7));

            // RTP3.1
            //SetShaderParam("RTP_gloss2mask4567", getVector(RTP_gloss2mask, 4, 7));
            //SetShaderParam("RTP_gloss_mult4567", getVector(tmp_RTP_gloss_mult, 4, 7));
            //SetShaderParam("RTP_gloss_shaping4567", getVector(tmp_RTP_gloss_shaping, 4, 7));
            //SetShaderParam("RTP_Fresnel4567", getVector(RTP_Fresnel, 4, 7));
            //SetShaderParam("RTP_FresnelAtten4567", getVector(RTP_FresnelAtten, 4, 7));
            SetShaderParam("RTP_DiffFresnel4567", getVector(RTP_DiffFresnel, 4, 7));
            //SetShaderParam("RTP_IBL_bump_smoothness4567", getVector(RTP_IBL_bump_smoothness, 4, 7));
            //SetShaderParam("RTP_IBL_DiffuseStrength4567", getVector(RTP_IBL_DiffuseStrength, 4, 7));
            //SetShaderParam("RTP_IBL_SpecStrength4567", getVector(RTP_IBL_SpecStrength, 4, 7));
            SetShaderParam("RTP_metallic4567", getVector(RTP_metallic, 4, 7));
            SetShaderParam("RTP_glossMin4567", getVector(RTP_glossMin, 4, 7));
            SetShaderParam("RTP_glossMax4567", getVector(RTP_glossMax, 4, 7));

            SetShaderParam("RTP_glitter4567", getVector(RTP_glitter, 4, 7));

            // only in deferred add pass
            //SetShaderParam("_DeferredSpecDampAddPass4567", getVector(_DeferredSpecDampAddPass, 4,7));

            SetShaderParam("_MixBrightness4567", getVector(MixBrightness, 4, 7));
            SetShaderParam("_MixReplace4567", getVector(MixReplace, 4, 7));
            SetShaderParam("_LayerBrightness4567", MasterLayerBrightness * getVector(LayerBrightness, 4, 7));
            SetShaderParam("_LayerSaturation4567", MasterLayerSaturation * getVector(LayerSaturation, 4, 7));
            SetShaderParam("_LayerEmission4567", getVector(LayerEmission, 4, 7));
            SetShaderParam("_LayerEmissionColorR4567", getColorVector(LayerEmissionColor, 4, 7, 0));
            SetShaderParam("_LayerEmissionColorG4567", getColorVector(LayerEmissionColor, 4, 7, 1));
            SetShaderParam("_LayerEmissionColorB4567", getColorVector(LayerEmissionColor, 4, 7, 2));
            SetShaderParam("_LayerEmissionColorA4567", getColorVector(LayerEmissionColor, 4, 7, 3));
            SetShaderParam("_LayerBrightness2Spec4567", getVector(LayerBrightness2Spec, 4, 7));
            SetShaderParam("_LayerAlbedo2SpecColor4567", getVector(LayerAlbedo2SpecColor, 4, 7));
            SetShaderParam("_LayerEmissionRefractStrength4567", getVector(LayerEmissionRefractStrength, 4, 7));
            SetShaderParam("_LayerEmissionRefractHBedge4567", getVector(LayerEmissionRefractHBedge, 4, 7));

            SetShaderParam("_GlobalColorPerLayer4567", getVector(GlobalColorPerLayer, 4, 7));

            SetShaderParam("_GlobalColorBottom4567", getVector(GlobalColorBottom, 4, 7));
            SetShaderParam("_GlobalColorTop4567", getVector(GlobalColorTop, 4, 7));
            SetShaderParam("_GlobalColorColormapLoSat4567", getVector(GlobalColorColormapLoSat, 4, 7));
            SetShaderParam("_GlobalColorColormapHiSat4567", getVector(GlobalColorColormapHiSat, 4, 7));
            SetShaderParam("_GlobalColorLayerLoSat4567", getVector(GlobalColorLayerLoSat, 4, 7));
            SetShaderParam("_GlobalColorLayerHiSat4567", getVector(GlobalColorLayerHiSat, 4, 7));
            SetShaderParam("_GlobalColorLoBlend4567", getVector(GlobalColorLoBlend, 4, 7));
            SetShaderParam("_GlobalColorHiBlend4567", getVector(GlobalColorHiBlend, 4, 7));

            SetShaderParam("PER_LAYER_HEIGHT_MODIFIER4567", getVector(PER_LAYER_HEIGHT_MODIFIER, 4, 7));

            SetShaderParam("rtp_snow_strength_per_layer4567", getVector(_snow_strength_per_layer, 4, 7));

            SetShaderParam("_SuperDetailStrengthMultA4567", getVector(_SuperDetailStrengthMultA, 4, 7));
            SetShaderParam("_SuperDetailStrengthMultB4567", getVector(_SuperDetailStrengthMultB, 4, 7));
            SetShaderParam("_SuperDetailStrengthNormal4567", getVector(_SuperDetailStrengthNormal, 4, 7));
            SetShaderParam("_BumpMapGlobalStrength4567", getVector(_BumpMapGlobalStrength, 4, 7));

            SetShaderParam("_SuperDetailStrengthMultASelfMaskNear4567", getVector(_SuperDetailStrengthMultASelfMaskNear, 4, 7));
            SetShaderParam("_SuperDetailStrengthMultASelfMaskFar4567", getVector(_SuperDetailStrengthMultASelfMaskFar, 4, 7));
            SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear4567", getVector(_SuperDetailStrengthMultBSelfMaskNear, 4, 7));
            SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar4567", getVector(_SuperDetailStrengthMultBSelfMaskFar, 4, 7));

            SetShaderParam("TERRAIN_LayerWetStrength4567", getVector(TERRAIN_LayerWetStrength, 4, 7));
            SetShaderParam("TERRAIN_WaterLevel4567", getVector(TERRAIN_WaterLevel, 4, 7));
            SetShaderParam("TERRAIN_WaterLevelSlopeDamp4567", getVector(TERRAIN_WaterLevelSlopeDamp, 4, 7));
            SetShaderParam("TERRAIN_WaterEdge4567", getVector(TERRAIN_WaterEdge, 4, 7));
            SetShaderParam("TERRAIN_WaterGloss4567", getVector(TERRAIN_WaterGloss, 4, 7));
            SetShaderParam("TERRAIN_WaterGlossDamper4567", getVector(TERRAIN_WaterGlossDamper, 4, 7));
            SetShaderParam("TERRAIN_Refraction4567", getVector(TERRAIN_Refraction, 4, 7));
            SetShaderParam("TERRAIN_WetRefraction4567", getVector(TERRAIN_WetRefraction, 4, 7));
            SetShaderParam("TERRAIN_Flow4567", getVector(TERRAIN_Flow, 4, 7));
            SetShaderParam("TERRAIN_WetFlow4567", getVector(TERRAIN_WetFlow, 4, 7));
            SetShaderParam("TERRAIN_WaterMetallic4567", getVector(TERRAIN_WaterMetallic, 4, 7));
            SetShaderParam("TERRAIN_WetGloss4567", getVector(TERRAIN_WetGloss, 4, 7));
            SetShaderParam("TERRAIN_WaterColorR4567", getColorVector(TERRAIN_WaterColor, 4, 7, 0));
            SetShaderParam("TERRAIN_WaterColorG4567", getColorVector(TERRAIN_WaterColor, 4, 7, 1));
            SetShaderParam("TERRAIN_WaterColorB4567", getColorVector(TERRAIN_WaterColor, 4, 7, 2));
            SetShaderParam("TERRAIN_WaterColorA4567", getColorVector(TERRAIN_WaterColor, 4, 7, 3));
            SetShaderParam("TERRAIN_WaterEmission4567", getVector(TERRAIN_WaterEmission, 4, 7));

            SetShaderParam("_GlitterStrength4567", getVector(_GlitterStrength, 4, 7));

            SetShaderParam("RTP_AO_4567", getVector(AO_strength, 4, 7));
            SetShaderParam("_VerticalTexture4567", getVector(VerticalTextureStrength, 4, 7));

            //
            // AddPass
            //
            //SetShaderParam("_Spec89AB", getVector(Spec, 8,11));
            SetShaderParam("_FarSpecCorrection89AB", getVector(FarSpecCorrection, 8, 11));
            SetShaderParam("_MIPmult89AB", getVector(MIPmult, 8, 11));
            SetShaderParam("_MixScale89AB", getVector(MixScale, 8, 11));
            SetShaderParam("_MixBlend89AB", getVector(MixBlend, 8, 11));
            SetShaderParam("_MixSaturation89AB", getVector(MixSaturation, 8, 11));

            // RTP3.1
            //SetShaderParam("RTP_gloss2mask89AB", getVector(RTP_gloss2mask, 8,11));
            //SetShaderParam("RTP_gloss_mult89AB", getVector(tmp_RTP_gloss_mult, 8,11));
            //SetShaderParam("RTP_gloss_shaping89AB", getVector(tmp_RTP_gloss_shaping, 8,11));
            //SetShaderParam("RTP_Fresnel89AB", getVector(RTP_Fresnel, 8,11));
            //SetShaderParam("RTP_FresnelAtten89AB", getVector(RTP_FresnelAtten, 8,11));
            SetShaderParam("RTP_DiffFresnel89AB", getVector(RTP_DiffFresnel, 8, 11));
            //SetShaderParam("RTP_IBL_bump_smoothness89AB", getVector(RTP_IBL_bump_smoothness, 8,11));
            //SetShaderParam("RTP_IBL_DiffuseStrength89AB", getVector(RTP_IBL_DiffuseStrength, 8,11));
            //SetShaderParam("RTP_IBL_SpecStrength89AB", getVector(RTP_IBL_SpecStrength, 8,11));
            //SetShaderParam("_DeferredSpecDampAddPass89AB", getVector(_DeferredSpecDampAddPass, 8,11));
            SetShaderParam("RTP_metallic89AB", getVector(RTP_metallic, 8, 11));
            SetShaderParam("RTP_glossMin89AB", getVector(RTP_glossMin, 8, 11));
            SetShaderParam("RTP_glossMax89AB", getVector(RTP_glossMax, 8, 11));

            SetShaderParam("RTP_glitter89AB", getVector(RTP_glitter, 8, 11));

            SetShaderParam("_MixBrightness89AB", getVector(MixBrightness, 8, 11));
            SetShaderParam("_MixReplace89AB", getVector(MixReplace, 8, 11));
            SetShaderParam("_LayerBrightness89AB", MasterLayerBrightness * getVector(LayerBrightness, 8, 11));
            SetShaderParam("_LayerSaturation89AB", MasterLayerSaturation * getVector(LayerSaturation, 8, 11));
            SetShaderParam("_LayerEmission89AB", getVector(LayerEmission, 8, 11));
            SetShaderParam("_LayerEmissionColorR89AB", getColorVector(LayerEmissionColor, 8, 11, 0));
            SetShaderParam("_LayerEmissionColorG89AB", getColorVector(LayerEmissionColor, 8, 11, 1));
            SetShaderParam("_LayerEmissionColorB89AB", getColorVector(LayerEmissionColor, 8, 11, 2));
            SetShaderParam("_LayerEmissionColorA89AB", getColorVector(LayerEmissionColor, 8, 11, 3));
            SetShaderParam("_LayerBrightness2Spec89AB", getVector(LayerBrightness2Spec, 8, 11));
            SetShaderParam("_LayerAlbedo2SpecColor89AB", getVector(LayerAlbedo2SpecColor, 8, 11));
            SetShaderParam("_LayerEmissionRefractStrength89AB", getVector(LayerEmissionRefractStrength, 8, 11));
            SetShaderParam("_LayerEmissionRefractHBedge89AB", getVector(LayerEmissionRefractHBedge, 8, 11));

            SetShaderParam("_GlobalColorPerLayer89AB", getVector(GlobalColorPerLayer, 8, 11));

            SetShaderParam("_GlobalColorBottom89AB", getVector(GlobalColorBottom, 8, 11));
            SetShaderParam("_GlobalColorTop89AB", getVector(GlobalColorTop, 8, 11));
            SetShaderParam("_GlobalColorColormapLoSat89AB", getVector(GlobalColorColormapLoSat, 8, 11));
            SetShaderParam("_GlobalColorColormapHiSat89AB", getVector(GlobalColorColormapHiSat, 8, 11));
            SetShaderParam("_GlobalColorLayerLoSat89AB", getVector(GlobalColorLayerLoSat, 8, 11));
            SetShaderParam("_GlobalColorLayerHiSat89AB", getVector(GlobalColorLayerHiSat, 8, 11));
            SetShaderParam("_GlobalColorLoBlend89AB", getVector(GlobalColorLoBlend, 8, 11));
            SetShaderParam("_GlobalColorHiBlend89AB", getVector(GlobalColorHiBlend, 8, 11));

            SetShaderParam("PER_LAYER_HEIGHT_MODIFIER89AB", getVector(PER_LAYER_HEIGHT_MODIFIER, 8, 11));

            SetShaderParam("rtp_snow_strength_per_layer89AB", getVector(_snow_strength_per_layer, 8, 11));

            SetShaderParam("_SuperDetailStrengthMultA89AB", getVector(_SuperDetailStrengthMultA, 8, 11));
            SetShaderParam("_SuperDetailStrengthMultB89AB", getVector(_SuperDetailStrengthMultB, 8, 11));
            SetShaderParam("_SuperDetailStrengthNormal89AB", getVector(_SuperDetailStrengthNormal, 8, 11));
            SetShaderParam("_BumpMapGlobalStrength89AB", getVector(_BumpMapGlobalStrength, 8, 11));

            SetShaderParam("_SuperDetailStrengthMultASelfMaskNear89AB", getVector(_SuperDetailStrengthMultASelfMaskNear, 8, 11));
            SetShaderParam("_SuperDetailStrengthMultASelfMaskFar89AB", getVector(_SuperDetailStrengthMultASelfMaskFar, 8, 11));
            SetShaderParam("_SuperDetailStrengthMultBSelfMaskNear89AB", getVector(_SuperDetailStrengthMultBSelfMaskNear, 8, 11));
            SetShaderParam("_SuperDetailStrengthMultBSelfMaskFar89AB", getVector(_SuperDetailStrengthMultBSelfMaskFar, 8, 11));

            SetShaderParam("TERRAIN_LayerWetStrength89AB", getVector(TERRAIN_LayerWetStrength, 8, 11));
            SetShaderParam("TERRAIN_WaterLevel89AB", getVector(TERRAIN_WaterLevel, 8, 11));
            SetShaderParam("TERRAIN_WaterLevelSlopeDamp89AB", getVector(TERRAIN_WaterLevelSlopeDamp, 8, 11));
            SetShaderParam("TERRAIN_WaterEdge89AB", getVector(TERRAIN_WaterEdge, 8, 11));
            SetShaderParam("TERRAIN_WaterGloss89AB", getVector(TERRAIN_WaterGloss, 8, 11));
            SetShaderParam("TERRAIN_WaterGlossDamper89AB", getVector(TERRAIN_WaterGlossDamper, 8, 11));
            SetShaderParam("TERRAIN_Refraction89AB", getVector(TERRAIN_Refraction, 8, 11));
            SetShaderParam("TERRAIN_WetRefraction89AB", getVector(TERRAIN_WetRefraction, 8, 11));
            SetShaderParam("TERRAIN_Flow89AB", getVector(TERRAIN_Flow, 8, 11));
            SetShaderParam("TERRAIN_WetFlow89AB", getVector(TERRAIN_WetFlow, 8, 11));
            SetShaderParam("TERRAIN_WaterMetallic89AB", getVector(TERRAIN_WaterMetallic, 8, 11));
            SetShaderParam("TERRAIN_WetGloss89AB", getVector(TERRAIN_WetGloss, 8, 11));
            SetShaderParam("TERRAIN_WaterColorR89AB", getColorVector(TERRAIN_WaterColor, 8, 11, 0));
            SetShaderParam("TERRAIN_WaterColorG89AB", getColorVector(TERRAIN_WaterColor, 8, 11, 1));
            SetShaderParam("TERRAIN_WaterColorB89AB", getColorVector(TERRAIN_WaterColor, 8, 11, 2));
            SetShaderParam("TERRAIN_WaterColorA89AB", getColorVector(TERRAIN_WaterColor, 8, 11, 3));
            SetShaderParam("TERRAIN_WaterEmission89AB", getVector(TERRAIN_WaterEmission, 8, 11));

            SetShaderParam("_GlitterStrength89AB", getVector(_GlitterStrength, 8, 11));

            SetShaderParam("RTP_AO_89AB", getVector(AO_strength, 8, 11));
            SetShaderParam("_VerticalTexture89AB", getVector(VerticalTextureStrength, 8, 11));
        }

        /////////////////////////////////////////////////////////////////////
        //
        // layer dependent textures
        //
        /////////////////////////////////////////////////////////////////////
        // update (RTP3.1)
        if (splat_atlases.Length == 2)
        {
            Texture2D _atA = splat_atlases[0];
            Texture2D _atB = splat_atlases[1];
            splat_atlases = new Texture2D[3];
            splat_atlases[0] = _atA;
            splat_atlases[1] = _atB;
        }
        SetShaderParam("_SplatAtlasA", splat_atlases[0]);
        SetShaderParam("_BumpMap01", Bump01);
        SetShaderParam("_BumpMap23", Bump23);
        SetShaderParam("_TERRAIN_HeightMap", HeightMap);
        SetShaderParam("_SSColorCombinedA", SSColorCombinedA);

        if (numLayers > 4)
        {
            SetShaderParam("_SplatAtlasB", splat_atlases[1]);
            SetShaderParam("_SplatAtlasC", splat_atlases[1]);
            SetShaderParam("_TERRAIN_HeightMap2", HeightMap2);
            SetShaderParam("_SSColorCombinedB", SSColorCombinedB);
        }
        if (numLayers > 8)
        {
            SetShaderParam("_SplatAtlasC", splat_atlases[2]);
        }
        if ((numLayers > 4) && _4LAYERS_SHADER_USED)
        {
            //
            // przekieruj parametry warstw 4-7 na AddPass
            //
            SetShaderParam("_BumpMap89", Bump45);
            SetShaderParam("_BumpMapAB", Bump67);
            SetShaderParam("_TERRAIN_HeightMap3", HeightMap2);
            // potrzebne przy sniegu (firstpass moze korzystac z koloru i bumpmap 4-7)
            SetShaderParam("_BumpMap45", Bump45);
            SetShaderParam("_BumpMap67", Bump67);
        }
        else
        {
            SetShaderParam("_BumpMap45", Bump45);
            SetShaderParam("_BumpMap67", Bump67);

            //
            // AddPass
            //
            SetShaderParam("_BumpMap89", Bump89);
            SetShaderParam("_BumpMapAB", BumpAB);
            SetShaderParam("_TERRAIN_HeightMap3", HeightMap3);
        }

        use_mat = null;
    }

    public Vector4 getVector(float[] vec, int idxA, int idxB)
    {
        if (vec == null) return Vector4.zero;
        Vector4 ret = Vector4.zero;
        for (int i = idxA; i <= idxB; i++)
        {
            if (i < vec.Length)
            {
                ret[i - idxA] = vec[i];
            }
        }
        return ret;
    }
    public Vector4 getColorVector(Color[] vec, int idxA, int idxB, int channel)
    {
        if (vec == null) return Vector4.zero;
        Vector4 ret = Vector4.zero;
        for (int i = idxA; i <= idxB; i++)
        {
            if (i < vec.Length)
            {
                ret[i - idxA] = vec[i][channel];
            }
        }
        return ret;
    }

    public Texture2D get_dumb_tex()
    {
        if (!dumb_tex)
        {
            dumb_tex = new Texture2D(32, 32, TextureFormat.RGB24, false);
            Color[] cols = dumb_tex.GetPixels();
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i] = Color.white;
            }
            dumb_tex.SetPixels(cols);
            dumb_tex.Apply();
        }
        return dumb_tex;
    }

    public void SyncGlobalPropsAcrossTerrainGroups()
    {
        ReliefTerrain[] terrainObjects = (ReliefTerrain[])(GameObject.FindObjectsOfType(typeof(ReliefTerrain)));
        ReliefTerrainGlobalSettingsHolder[] globalHolders = new ReliefTerrainGlobalSettingsHolder[terrainObjects.Length];
        int numSeparateGroups = 0;
        for (int i = 0; i < terrainObjects.Length; i++)
        {
            bool alreadyPresent = false;
            for (int j = 0; j < numSeparateGroups; j++)
            {
                if (globalHolders[j] == terrainObjects[i].globalSettingsHolder)
                {
                    alreadyPresent = true;
                    break;
                }
            }
            if (!alreadyPresent)
            {
                globalHolders[numSeparateGroups++] = terrainObjects[i].globalSettingsHolder;
            }
        }

        for (int i = 0; i < numSeparateGroups; i++)
        {
            if (globalHolders[i] != this)
            {
                globalHolders[i].trees_shadow_distance_start = trees_shadow_distance_start;
                globalHolders[i].trees_shadow_distance_transition = trees_shadow_distance_transition;
                globalHolders[i].trees_shadow_value = trees_shadow_value;
                globalHolders[i].global_normalMap_multiplier = global_normalMap_multiplier;

                globalHolders[i].trees_pixel_distance_start = trees_pixel_distance_start;
                globalHolders[i].trees_pixel_distance_transition = trees_pixel_distance_transition;
                globalHolders[i].trees_pixel_blend_val = trees_pixel_blend_val;
                globalHolders[i].global_normalMap_farUsage = global_normalMap_farUsage;

                globalHolders[i]._AmbientEmissiveMultiplier = _AmbientEmissiveMultiplier;
                globalHolders[i]._AmbientEmissiveRelief = _AmbientEmissiveRelief;

                globalHolders[i]._snow_strength = _snow_strength;
                globalHolders[i]._global_color_brightness_to_snow = _global_color_brightness_to_snow;
                globalHolders[i]._snow_slope_factor = _snow_slope_factor;
                globalHolders[i]._snow_edge_definition = _snow_edge_definition;
                globalHolders[i]._snow_height_treshold = _snow_height_treshold;
                globalHolders[i]._snow_height_transition = _snow_height_transition;
                globalHolders[i]._snow_color = _snow_color;
                //globalHolders[i]._snow_specular = _snow_specular;
                globalHolders[i]._snow_gloss = _snow_gloss;
                globalHolders[i]._snow_reflectivness = _snow_reflectivness;
                globalHolders[i]._snow_deep_factor = _snow_deep_factor;
                //globalHolders[i]._snow_fresnel = _snow_fresnel;
                globalHolders[i]._snow_diff_fresnel = _snow_diff_fresnel;

                //globalHolders[i]._snow_IBL_DiffuseStrength = _snow_IBL_DiffuseStrength;
                //globalHolders[i]._snow_IBL_SpecStrength = _snow_IBL_SpecStrength;

                globalHolders[i]._snow_metallic = _snow_metallic;
                globalHolders[i]._snow_Frost = _snow_Frost;
                globalHolders[i]._snow_MicroTiling = _snow_MicroTiling;
                globalHolders[i]._snow_BumpMicro = _snow_BumpMicro;
                globalHolders[i]._snow_occlusionStrength = _snow_occlusionStrength;
                globalHolders[i]._snow_TranslucencyDeferredLightIndex = _snow_TranslucencyDeferredLightIndex;

                globalHolders[i]._SnowGlitterColor = _SnowGlitterColor;

                globalHolders[i]._GlitterColor = _GlitterColor;
                globalHolders[i]._GlitterTiling = _GlitterTiling;
                globalHolders[i]._GlitterDensity = _GlitterDensity;
                globalHolders[i]._GlitterFilter = _GlitterFilter;

                globalHolders[i]._GlitterColorization = _GlitterColorization;
                globalHolders[i]._SparkleMap = _SparkleMap;

                globalHolders[i].TERRAIN_CausticsWaterLevel = TERRAIN_CausticsWaterLevel;
                globalHolders[i].TERRAIN_CausticsWaterLevelByAngle = TERRAIN_CausticsWaterLevelByAngle;
                globalHolders[i].TERRAIN_CausticsWaterDeepFadeLength = TERRAIN_CausticsWaterDeepFadeLength;
                globalHolders[i].TERRAIN_CausticsWaterShallowFadeLength = TERRAIN_CausticsWaterShallowFadeLength;

                globalHolders[i].TERRAIN_GlobalWetness = TERRAIN_GlobalWetness;
                globalHolders[i].TERRAIN_RainIntensity = TERRAIN_RainIntensity;

                //globalHolders[i].rtp_customAmbientCorrection=rtp_customAmbientCorrection;

                //globalHolders[i].TERRAIN_IBL_DiffAO_Damp=TERRAIN_IBL_DiffAO_Damp;
                //globalHolders[i].TERRAIN_IBLRefl_SpecAO_Damp=TERRAIN_IBLRefl_SpecAO_Damp;

                globalHolders[i].RTP_LightDefVector = RTP_LightDefVector;
                //globalHolders[i].RTP_ReflexLightDiffuseColor=RTP_ReflexLightDiffuseColor;
                //globalHolders[i].RTP_ReflexLightDiffuseColor2=RTP_ReflexLightDiffuseColor2;
                //globalHolders[i].RTP_ReflexLightSpecColor=RTP_ReflexLightSpecColor;
            }
        }


    }

    public void RestorePreset(ReliefTerrainPresetHolder holder)
    {
        terrainLayers = holder.terrainLayers;
        numLayers = holder.numLayers;
        splats = new Texture2D[holder.splats.Length];
        for (int i = 0; i < holder.splats.Length; i++)
        {
            splats[i] = holder.splats[i];
        }

        splat_atlases = new Texture2D[3];
        for (int i = 0; i < splat_atlases.Length; i++)
        {
            splat_atlases[i] = holder.splat_atlases[i];
        }

        RTP_MIP_BIAS = holder.RTP_MIP_BIAS;
        //_SpecColor=holder._SpecColor;
        //RTP_DeferredAddPassSpec=holder.RTP_DeferredAddPassSpec;

        MasterLayerBrightness = holder.MasterLayerBrightness;
        MasterLayerSaturation = holder.MasterLayerSaturation;

        SuperDetailA_channel = holder.SuperDetailA_channel;
        SuperDetailB_channel = holder.SuperDetailB_channel;

        Bump01 = holder.Bump01;
        Bump23 = holder.Bump23;
        Bump45 = holder.Bump45;
        Bump67 = holder.Bump67;
        Bump89 = holder.Bump89;
        BumpAB = holder.BumpAB;

        SSColorCombinedA = holder.SSColorCombinedA;
        SSColorCombinedB = holder.SSColorCombinedB;

        BumpGlobal = holder.BumpGlobal;

        VerticalTexture = holder.VerticalTexture;
        BumpMapGlobalScale = holder.BumpMapGlobalScale;
        GlobalColorMapBlendValues = holder.GlobalColorMapBlendValues;
        GlobalColorMapSaturation = holder.GlobalColorMapSaturation;
        GlobalColorMapSaturationFar = holder.GlobalColorMapSaturationFar;
        //GlobalColorMapSaturationByPerlin=holder.GlobalColorMapSaturationByPerlin;
        GlobalColorMapDistortByPerlin = holder.GlobalColorMapDistortByPerlin;
        GlobalColorMapBrightness = holder.GlobalColorMapBrightness;
        GlobalColorMapBrightnessFar = holder.GlobalColorMapBrightnessFar;
        _GlobalColorMapNearMIP = holder._GlobalColorMapNearMIP;
        _FarNormalDamp = holder._FarNormalDamp;

        blendMultiplier = holder.blendMultiplier;

        HeightMap = holder.HeightMap;
        HeightMap2 = holder.HeightMap2;
        HeightMap3 = holder.HeightMap3;

        ReliefTransform = holder.ReliefTransform;
        DIST_STEPS = holder.DIST_STEPS;
        WAVELENGTH = holder.WAVELENGTH;
        ReliefBorderBlend = holder.ReliefBorderBlend;

        ExtrudeHeight = holder.ExtrudeHeight;
        LightmapShading = holder.LightmapShading;

        SHADOW_STEPS = holder.SHADOW_STEPS;
        WAVELENGTH_SHADOWS = holder.WAVELENGTH_SHADOWS;
        //SHADOW_SMOOTH_STEPS=holder.SHADOW_SMOOTH_STEPS;
        SelfShadowStrength = holder.SelfShadowStrength;
        ShadowSmoothing = holder.ShadowSmoothing;
        ShadowSoftnessFade = holder.ShadowSoftnessFade;

        distance_start = holder.distance_start;
        distance_transition = holder.distance_transition;
        distance_start_bumpglobal = holder.distance_start_bumpglobal;
        distance_transition_bumpglobal = holder.distance_transition_bumpglobal;
        rtp_perlin_start_val = holder.rtp_perlin_start_val;

        _Phong = holder._Phong;
        tessHeight = holder.tessHeight;
        _TessSubdivisions = holder._TessSubdivisions;
        _TessSubdivisionsFar = holder._TessSubdivisionsFar;
        _TessYOffset = holder._TessYOffset;

        trees_shadow_distance_start = holder.trees_shadow_distance_start;
        trees_shadow_distance_transition = holder.trees_shadow_distance_transition;
        trees_shadow_value = holder.trees_shadow_value;
        trees_pixel_distance_start = holder.trees_pixel_distance_start;
        trees_pixel_distance_transition = holder.trees_pixel_distance_transition;
        trees_pixel_blend_val = holder.trees_pixel_blend_val;
        global_normalMap_multiplier = holder.global_normalMap_multiplier;
        global_normalMap_farUsage = holder.global_normalMap_farUsage;

        _AmbientEmissiveMultiplier = holder._AmbientEmissiveMultiplier;
        _AmbientEmissiveRelief = holder._AmbientEmissiveRelief;

        rtp_mipoffset_globalnorm = holder.rtp_mipoffset_globalnorm;
        _SuperDetailTiling = holder._SuperDetailTiling;
        SuperDetailA = holder.SuperDetailA;
        SuperDetailB = holder.SuperDetailB;

        // water/wet
        TERRAIN_GlobalWetness = holder.TERRAIN_GlobalWetness;

        TERRAIN_RippleMap = holder.TERRAIN_RippleMap;
        TERRAIN_RippleScale = holder.TERRAIN_RippleScale;
        TERRAIN_FlowScale = holder.TERRAIN_FlowScale;
        TERRAIN_FlowSpeed = holder.TERRAIN_FlowSpeed;
        TERRAIN_FlowCycleScale = holder.TERRAIN_FlowCycleScale;
        TERRAIN_FlowMipOffset = holder.TERRAIN_FlowMipOffset;
        TERRAIN_WetDarkening = holder.TERRAIN_WetDarkening;
        TERRAIN_WetDropletsStrength = holder.TERRAIN_WetDropletsStrength;
        TERRAIN_WetHeight_Treshold = holder.TERRAIN_WetHeight_Treshold;
        TERRAIN_WetHeight_Transition = holder.TERRAIN_WetHeight_Transition;

        TERRAIN_RainIntensity = holder.TERRAIN_RainIntensity;
        TERRAIN_DropletsSpeed = holder.TERRAIN_DropletsSpeed;

        TERRAIN_mipoffset_flowSpeed = holder.TERRAIN_mipoffset_flowSpeed;

        // caustics
        TERRAIN_CausticsAnimSpeed = holder.TERRAIN_CausticsAnimSpeed;
        TERRAIN_CausticsColor = holder.TERRAIN_CausticsColor;
        TERRAIN_CausticsWaterLevel = holder.TERRAIN_CausticsWaterLevel;
        TERRAIN_CausticsWaterLevelByAngle = holder.TERRAIN_CausticsWaterLevelByAngle;
        TERRAIN_CausticsWaterDeepFadeLength = holder.TERRAIN_CausticsWaterDeepFadeLength;
        TERRAIN_CausticsWaterShallowFadeLength = holder.TERRAIN_CausticsWaterShallowFadeLength;
        TERRAIN_CausticsTilingScale = holder.TERRAIN_CausticsTilingScale;
        TERRAIN_CausticsTex = holder.TERRAIN_CausticsTex;

        //rtp_customAmbientCorrection=holder.rtp_customAmbientCorrection;
        //TERRAIN_IBL_DiffAO_Damp=holder.TERRAIN_IBL_DiffAO_Damp;
        //TERRAIN_IBLRefl_SpecAO_Damp=holder.TERRAIN_IBLRefl_SpecAO_Damp;
        //_CubemapDiff=holder._CubemapDiff;
        //_CubemapSpec=holder._CubemapSpec;

        RTP_AOsharpness = holder.RTP_AOsharpness;
        RTP_AOamp = holder.RTP_AOamp;
        _occlusionStrength = holder._occlusionStrength;

        RTP_LightDefVector = holder.RTP_LightDefVector;
        //RTP_ReflexLightDiffuseColor=holder.RTP_ReflexLightDiffuseColor;
        //RTP_ReflexLightDiffuseColor2=holder.RTP_ReflexLightDiffuseColor2;
        //RTP_ReflexLightSpecColor=holder.RTP_ReflexLightSpecColor;

        EmissionRefractFiltering = holder.EmissionRefractFiltering;
        EmissionRefractAnimSpeed = holder.EmissionRefractAnimSpeed;

        VerticalTextureGlobalBumpInfluence = holder.VerticalTextureGlobalBumpInfluence;
        VerticalTextureTiling = holder.VerticalTextureTiling;

        // snow
        _snow_strength = holder._snow_strength;
        _global_color_brightness_to_snow = holder._global_color_brightness_to_snow;
        _snow_slope_factor = holder._snow_slope_factor;
        _snow_edge_definition = holder._snow_edge_definition;
        _snow_height_treshold = holder._snow_height_treshold;
        _snow_height_transition = holder._snow_height_transition;
        _snow_color = holder._snow_color;
        //_snow_specular = holder._snow_specular;
        _snow_gloss = holder._snow_gloss;
        _snow_reflectivness = holder._snow_reflectivness;
        _snow_deep_factor = holder._snow_deep_factor;
        //_snow_fresnel = holder._snow_fresnel;
        _snow_diff_fresnel = holder._snow_diff_fresnel;
        //_snow_IBL_DiffuseStrength = holder._snow_IBL_DiffuseStrength;
        //_snow_IBL_SpecStrength = holder._snow_IBL_SpecStrength;

        _snow_metallic = holder._snow_metallic;
        _snow_Frost = holder._snow_Frost;
        _snow_MicroTiling = holder._snow_MicroTiling;
        _snow_BumpMicro = holder._snow_BumpMicro;
        _snow_occlusionStrength = holder._snow_occlusionStrength;
        _snow_TranslucencyDeferredLightIndex = holder._snow_TranslucencyDeferredLightIndex;
        _SnowGlitterColor = holder._SnowGlitterColor;

        _GlitterColor = holder._GlitterColor;
        _GlitterTiling = holder._GlitterTiling;
        _GlitterDensity = holder._GlitterDensity;
        _GlitterFilter = holder._GlitterFilter;
        _GlitterColorization = holder._GlitterColorization;
        _SparkleMap = holder._SparkleMap;


        //////////////////////
        // layer_dependent arrays
        //////////////////////
        Bumps = new Texture2D[holder.Bumps.Length];
        //Spec=new float[holder.Bumps.Length];
        FarSpecCorrection = new float[holder.Bumps.Length];
        MixScale = new float[holder.Bumps.Length];
        MixBlend = new float[holder.Bumps.Length];
        MixSaturation = new float[holder.Bumps.Length];

        // RTP3.1
        //RTP_gloss2mask=new float[holder.Bumps.Length];
        //RTP_gloss_mult=new float[holder.Bumps.Length];
        //RTP_gloss_shaping=new float[holder.Bumps.Length];
        //RTP_Fresnel=new float[holder.Bumps.Length];
        //RTP_FresnelAtten=new float[holder.Bumps.Length];
        RTP_DiffFresnel = new float[holder.Bumps.Length];
        //RTP_IBL_bump_smoothness=new float[holder.Bumps.Length];
        //RTP_IBL_DiffuseStrength=new float[holder.Bumps.Length];
        //RTP_IBL_SpecStrength=new float[holder.Bumps.Length];
        //_DeferredSpecDampAddPass=new float[holder.Bumps.Length];
        RTP_metallic = new float[holder.Bumps.Length];
        RTP_glossMin = new float[holder.Bumps.Length];
        RTP_glossMax = new float[holder.Bumps.Length];
        RTP_glitter = new float[holder.Bumps.Length];

        MixBrightness = new float[holder.Bumps.Length];
        MixReplace = new float[holder.Bumps.Length];
        LayerBrightness = new float[holder.Bumps.Length];
        LayerBrightness2Spec = new float[holder.Bumps.Length];
        LayerAlbedo2SpecColor = new float[holder.Bumps.Length];
        LayerSaturation = new float[holder.Bumps.Length];
        LayerEmission = new float[holder.Bumps.Length];
        LayerEmissionColor = new Color[holder.Bumps.Length];
        LayerEmissionRefractStrength = new float[holder.Bumps.Length];
        LayerEmissionRefractHBedge = new float[holder.Bumps.Length];

        GlobalColorPerLayer = new float[holder.Bumps.Length];
        GlobalColorBottom = new float[holder.Bumps.Length];
        GlobalColorTop = new float[holder.Bumps.Length];
        GlobalColorColormapLoSat = new float[holder.Bumps.Length];
        GlobalColorColormapHiSat = new float[holder.Bumps.Length];
        GlobalColorLayerLoSat = new float[holder.Bumps.Length];
        GlobalColorLayerHiSat = new float[holder.Bumps.Length];
        GlobalColorLoBlend = new float[holder.Bumps.Length];
        GlobalColorHiBlend = new float[holder.Bumps.Length];

        PER_LAYER_HEIGHT_MODIFIER = new float[holder.Bumps.Length];
        _SuperDetailStrengthMultA = new float[holder.Bumps.Length];
        _SuperDetailStrengthMultASelfMaskNear = new float[holder.Bumps.Length];
        _SuperDetailStrengthMultASelfMaskFar = new float[holder.Bumps.Length];
        _SuperDetailStrengthMultB = new float[holder.Bumps.Length];
        _SuperDetailStrengthMultBSelfMaskNear = new float[holder.Bumps.Length];
        _SuperDetailStrengthMultBSelfMaskFar = new float[holder.Bumps.Length];
        _SuperDetailStrengthNormal = new float[holder.Bumps.Length];
        _BumpMapGlobalStrength = new float[holder.Bumps.Length];

        AO_strength = new float[holder.Bumps.Length];
        VerticalTextureStrength = new float[holder.Bumps.Length];

        Heights = new Texture2D[holder.Bumps.Length];

        _snow_strength_per_layer = new float[holder.Bumps.Length];
#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
        Substances = new ProceduralMaterial[holder.Bumps.Length];
#endif

        // wet
        TERRAIN_LayerWetStrength = new float[holder.Bumps.Length];
        TERRAIN_WaterLevel = new float[holder.Bumps.Length];
        TERRAIN_WaterLevelSlopeDamp = new float[holder.Bumps.Length];
        TERRAIN_WaterEdge = new float[holder.Bumps.Length];
        //TERRAIN_WaterSpecularity=new float[holder.Bumps.Length];
        TERRAIN_WaterGloss = new float[holder.Bumps.Length];
        TERRAIN_WaterGlossDamper = new float[holder.Bumps.Length];
        TERRAIN_Refraction = new float[holder.Bumps.Length];
        TERRAIN_WetRefraction = new float[holder.Bumps.Length];
        TERRAIN_Flow = new float[holder.Bumps.Length];
        TERRAIN_WetFlow = new float[holder.Bumps.Length];
        //TERRAIN_WetSpecularity=new float[holder.Bumps.Length];
        TERRAIN_WaterMetallic = new float[holder.Bumps.Length];
        TERRAIN_WetGloss = new float[holder.Bumps.Length];
        TERRAIN_WaterColor = new Color[holder.Bumps.Length];

        //TERRAIN_WaterIBL_SpecWetStrength=new float[holder.Bumps.Length];
        //TERRAIN_WaterIBL_SpecWaterStrength=new float[holder.Bumps.Length];
        TERRAIN_WaterEmission = new float[holder.Bumps.Length];

        _GlitterStrength = new float[holder.Bumps.Length];


        for (int i = 0; i < holder.Bumps.Length; i++)
        {
            Bumps[i] = holder.Bumps[i];
            //Spec[i]=holder.Spec[i];
            FarSpecCorrection[i] = holder.FarSpecCorrection[i];
            MixScale[i] = holder.MixScale[i];
            MixBlend[i] = holder.MixBlend[i];
            MixSaturation[i] = holder.MixSaturation[i];

            // RTP3.1
            // update-set to default
            //if (CheckAndUpdate(ref holder.RTP_gloss2mask, 0.5f, holder.Bumps.Length)) {
            //	for(int k=0; k<numLayers; k++) {
            //		Spec[k]=1; // zresetuj od razu mnoznik glossa (RTP3.1 - zmienna ma inne znaczenie)
            //	}
            //}
            //CheckAndUpdate(ref holder.RTP_gloss_mult, 1f, holder.Bumps.Length);
            //CheckAndUpdate(ref holder.RTP_gloss_shaping, 0.5f, holder.Bumps.Length);
            //CheckAndUpdate(ref holder.RTP_Fresnel, 0, holder.Bumps.Length);
            //CheckAndUpdate(ref holder.RTP_FresnelAtten, 0, holder.Bumps.Length);
            CheckAndUpdate(ref holder.RTP_DiffFresnel, 0, holder.Bumps.Length);
            //CheckAndUpdate(ref holder.RTP_IBL_bump_smoothness, 0.7f, holder.Bumps.Length);
            //CheckAndUpdate(ref holder.RTP_IBL_DiffuseStrength, 0.5f, holder.Bumps.Length);
            //CheckAndUpdate(ref holder.RTP_IBL_SpecStrength, 0.5f, holder.Bumps.Length);
            //CheckAndUpdate(ref holder._DeferredSpecDampAddPass, 1f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.RTP_metallic, 0, holder.Bumps.Length);
            CheckAndUpdate(ref holder.RTP_glossMin, 0, holder.Bumps.Length);
            CheckAndUpdate(ref holder.RTP_glossMax, 1, holder.Bumps.Length);
            CheckAndUpdate(ref holder.RTP_glitter, 0, holder.Bumps.Length);

            //CheckAndUpdate(ref holder.TERRAIN_WaterSpecularity, 0.5f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.TERRAIN_WaterGloss, 0.1f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.TERRAIN_WaterGlossDamper, 0f, holder.Bumps.Length);
            //CheckAndUpdate(ref holder.TERRAIN_WetSpecularity, 0.2f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.TERRAIN_WaterMetallic, 0.1f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.TERRAIN_WetGloss, 0.05f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.TERRAIN_WetFlow, 0.05f, holder.Bumps.Length);

            CheckAndUpdate(ref holder.MixBrightness, 2.0f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.MixReplace, 0.0f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.LayerBrightness, 1.0f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.LayerBrightness2Spec, 0.0f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.LayerAlbedo2SpecColor, 0.0f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.LayerSaturation, 1.0f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.LayerEmission, 1.0f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.LayerEmissionColor, Color.black, holder.Bumps.Length);
            CheckAndUpdate(ref holder.FarSpecCorrection, 0f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.LayerEmissionRefractStrength, 0f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.LayerEmissionRefractHBedge, 0f, holder.Bumps.Length);

            //CheckAndUpdate(ref holder.TERRAIN_WaterIBL_SpecWetStrength, 0.1f, holder.Bumps.Length);
            //CheckAndUpdate(ref holder.TERRAIN_WaterIBL_SpecWaterStrength, 0.5f, holder.Bumps.Length);
            CheckAndUpdate(ref holder.TERRAIN_WaterEmission, 0f, holder.Bumps.Length);

            CheckAndUpdate(ref holder._GlitterStrength, 0f, holder.Bumps.Length);

            //RTP_gloss2mask[i]=holder.RTP_gloss2mask[i];
            //RTP_gloss_mult[i]=holder.RTP_gloss_mult[i];
            //RTP_gloss_shaping[i]=holder.RTP_gloss_shaping[i];
            //RTP_Fresnel[i]=holder.RTP_Fresnel[i];
            //RTP_FresnelAtten[i]=holder.RTP_FresnelAtten[i];
            RTP_DiffFresnel[i] = holder.RTP_DiffFresnel[i];
            //RTP_IBL_bump_smoothness[i]=holder.RTP_IBL_bump_smoothness[i];
            //RTP_IBL_DiffuseStrength[i]=holder.RTP_IBL_DiffuseStrength[i];
            //RTP_IBL_SpecStrength[i]=holder.RTP_IBL_SpecStrength[i];
            //_DeferredSpecDampAddPass[i]=holder._DeferredSpecDampAddPass[i];
            RTP_metallic[i] = holder.RTP_metallic[i];
            RTP_glossMin[i] = holder.RTP_glossMin[i];
            RTP_glossMax[i] = holder.RTP_glossMax[i];
            RTP_glitter[i] = holder.RTP_glitter[i];

            MixBrightness[i] = holder.MixBrightness[i];
            MixReplace[i] = holder.MixReplace[i];
            LayerBrightness[i] = holder.LayerBrightness[i];
            LayerBrightness2Spec[i] = holder.LayerBrightness2Spec[i];
            LayerAlbedo2SpecColor[i] = holder.LayerAlbedo2SpecColor[i];
            LayerSaturation[i] = holder.LayerSaturation[i];
            LayerEmission[i] = holder.LayerEmission[i];
            LayerEmissionColor[i] = holder.LayerEmissionColor[i];
            LayerEmissionRefractStrength[i] = holder.LayerEmissionRefractStrength[i];
            LayerEmissionRefractHBedge[i] = holder.LayerEmissionRefractHBedge[i];

            GlobalColorPerLayer[i] = holder.GlobalColorPerLayer[i];
            GlobalColorBottom[i] = holder.GlobalColorBottom[i];
            GlobalColorTop[i] = holder.GlobalColorTop[i];
            GlobalColorColormapLoSat[i] = holder.GlobalColorColormapLoSat[i];
            GlobalColorColormapHiSat[i] = holder.GlobalColorColormapHiSat[i];
            GlobalColorLayerLoSat[i] = holder.GlobalColorLayerLoSat[i];
            GlobalColorLayerHiSat[i] = holder.GlobalColorLayerHiSat[i];
            GlobalColorLoBlend[i] = holder.GlobalColorLoBlend[i];
            GlobalColorHiBlend[i] = holder.GlobalColorHiBlend[i];

            PER_LAYER_HEIGHT_MODIFIER[i] = holder.PER_LAYER_HEIGHT_MODIFIER[i];
            _SuperDetailStrengthMultA[i] = holder._SuperDetailStrengthMultA[i];
            _SuperDetailStrengthMultASelfMaskNear[i] = holder._SuperDetailStrengthMultASelfMaskNear[i];
            _SuperDetailStrengthMultASelfMaskFar[i] = holder._SuperDetailStrengthMultASelfMaskFar[i];
            _SuperDetailStrengthMultB[i] = holder._SuperDetailStrengthMultB[i];
            _SuperDetailStrengthMultBSelfMaskNear[i] = holder._SuperDetailStrengthMultBSelfMaskNear[i];
            _SuperDetailStrengthMultBSelfMaskFar[i] = holder._SuperDetailStrengthMultBSelfMaskFar[i];
            _SuperDetailStrengthNormal[i] = holder._SuperDetailStrengthNormal[i];
            _BumpMapGlobalStrength[i] = holder._BumpMapGlobalStrength[i];

            VerticalTextureStrength[i] = holder.VerticalTextureStrength[i];
            AO_strength[i] = holder.AO_strength[i];

            Heights[i] = holder.Heights[i];

            _snow_strength_per_layer[i] = holder._snow_strength_per_layer[i];
#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
            Substances[i] = holder.Substances[i];
#endif

            // wet
            TERRAIN_LayerWetStrength[i] = holder.TERRAIN_LayerWetStrength[i];
            TERRAIN_WaterLevel[i] = holder.TERRAIN_WaterLevel[i];
            TERRAIN_WaterLevelSlopeDamp[i] = holder.TERRAIN_WaterLevelSlopeDamp[i];
            TERRAIN_WaterEdge[i] = holder.TERRAIN_WaterEdge[i];
            //TERRAIN_WaterSpecularity[i]=holder.TERRAIN_WaterSpecularity[i];
            TERRAIN_WaterGloss[i] = holder.TERRAIN_WaterGloss[i];
            TERRAIN_WaterGlossDamper[i] = holder.TERRAIN_WaterGlossDamper[i];
            TERRAIN_Refraction[i] = holder.TERRAIN_Refraction[i];
            TERRAIN_WetRefraction[i] = holder.TERRAIN_WetRefraction[i];
            TERRAIN_Flow[i] = holder.TERRAIN_Flow[i];
            TERRAIN_WetFlow[i] = holder.TERRAIN_WetFlow[i];
            //TERRAIN_WetSpecularity[i]=holder.TERRAIN_WetSpecularity[i];
            TERRAIN_WaterMetallic[i] = holder.TERRAIN_WaterMetallic[i];
            TERRAIN_WetGloss[i] = holder.TERRAIN_WetGloss[i];
            TERRAIN_WaterColor[i] = holder.TERRAIN_WaterColor[i];
            //TERRAIN_WaterIBL_SpecWetStrength[i]=holder.TERRAIN_WaterIBL_SpecWetStrength[i];
            //TERRAIN_WaterIBL_SpecWaterStrength[i]=holder.TERRAIN_WaterIBL_SpecWaterStrength[i];
            TERRAIN_WaterEmission[i] = holder.TERRAIN_WaterEmission[i];

            _GlitterStrength[i] = holder._GlitterStrength[i];

        }
    }

    public void GlossBakeJob(Texture2D detailTex, Texture2D normalTex)
    {
        bakeJobArray = new Texture2D[] { detailTex, normalTex };
    }

    public void SavePreset(ref ReliefTerrainPresetHolder holder)
    {
        holder.terrainLayers = terrainLayers;
        holder.numLayers = numLayers;
        holder.splats = new Texture2D[splats.Length];
        for (int i = 0; i < holder.splats.Length; i++)
        {
            holder.splats[i] = splats[i];
        }

        holder.splat_atlases = new Texture2D[3];
        for (int i = 0; i < splat_atlases.Length; i++)
        {
            holder.splat_atlases[i] = splat_atlases[i];
        }

        holder.RTP_MIP_BIAS = RTP_MIP_BIAS;
        //holder._SpecColor=_SpecColor;
        //holder.RTP_DeferredAddPassSpec=RTP_DeferredAddPassSpec;

        holder.MasterLayerBrightness = MasterLayerBrightness;
        holder.MasterLayerSaturation = MasterLayerSaturation;

        holder.SuperDetailA_channel = SuperDetailA_channel;
        holder.SuperDetailB_channel = SuperDetailB_channel;

        holder.Bump01 = Bump01;
        holder.Bump23 = Bump23;
        holder.Bump45 = Bump45;
        holder.Bump67 = Bump67;
        holder.Bump89 = Bump89;
        holder.BumpAB = BumpAB;

        holder.SSColorCombinedA = SSColorCombinedA;
        holder.SSColorCombinedB = SSColorCombinedB;

        holder.BumpGlobal = BumpGlobal;

        holder.VerticalTexture = VerticalTexture;
        holder.BumpMapGlobalScale = BumpMapGlobalScale;
        holder.GlobalColorMapBlendValues = GlobalColorMapBlendValues;
        holder.GlobalColorMapSaturation = GlobalColorMapSaturation;
        holder.GlobalColorMapSaturationFar = GlobalColorMapSaturationFar;
        //holder.GlobalColorMapSaturationByPerlin=GlobalColorMapSaturationByPerlin;
        holder.GlobalColorMapDistortByPerlin = GlobalColorMapDistortByPerlin;
        holder.GlobalColorMapBrightness = GlobalColorMapBrightness;
        holder.GlobalColorMapBrightnessFar = GlobalColorMapBrightnessFar;
        holder._GlobalColorMapNearMIP = _GlobalColorMapNearMIP;
        holder._FarNormalDamp = _FarNormalDamp;

        holder.blendMultiplier = blendMultiplier;

        holder.HeightMap = HeightMap;
        holder.HeightMap2 = HeightMap2;
        holder.HeightMap3 = HeightMap3;

        holder.ReliefTransform = ReliefTransform;
        holder.DIST_STEPS = DIST_STEPS;
        holder.WAVELENGTH = WAVELENGTH;
        holder.ReliefBorderBlend = ReliefBorderBlend;

        holder.ExtrudeHeight = ExtrudeHeight;
        holder.LightmapShading = LightmapShading;

        holder.SHADOW_STEPS = SHADOW_STEPS;
        holder.WAVELENGTH_SHADOWS = WAVELENGTH_SHADOWS;
        //holder.SHADOW_SMOOTH_STEPS=SHADOW_SMOOTH_STEPS;
        holder.SelfShadowStrength = SelfShadowStrength;
        holder.ShadowSmoothing = ShadowSmoothing;
        holder.ShadowSoftnessFade = ShadowSoftnessFade;

        holder.distance_start = distance_start;
        holder.distance_transition = distance_transition;
        holder.distance_start_bumpglobal = distance_start_bumpglobal;
        holder.distance_transition_bumpglobal = distance_transition_bumpglobal;
        holder.rtp_perlin_start_val = rtp_perlin_start_val;

        holder._Phong = _Phong;
        holder.tessHeight = tessHeight;
        holder._TessSubdivisions = _TessSubdivisions;
        holder._TessSubdivisionsFar = _TessSubdivisionsFar;
        holder._TessYOffset = _TessYOffset;

        holder.trees_shadow_distance_start = trees_shadow_distance_start;
        holder.trees_shadow_distance_transition = trees_shadow_distance_transition;
        holder.trees_shadow_value = trees_shadow_value;
        holder.trees_pixel_distance_start = trees_pixel_distance_start;
        holder.trees_pixel_distance_transition = trees_pixel_distance_transition;
        holder.trees_pixel_blend_val = trees_pixel_blend_val;
        holder.global_normalMap_multiplier = global_normalMap_multiplier;
        holder.global_normalMap_farUsage = global_normalMap_farUsage;

        holder._AmbientEmissiveMultiplier = _AmbientEmissiveMultiplier;
        holder._AmbientEmissiveRelief = _AmbientEmissiveRelief;

        holder.rtp_mipoffset_globalnorm = rtp_mipoffset_globalnorm;
        holder._SuperDetailTiling = _SuperDetailTiling;
        holder.SuperDetailA = SuperDetailA;
        holder.SuperDetailB = SuperDetailB;

        // water/wet
        holder.TERRAIN_GlobalWetness = TERRAIN_GlobalWetness;

        holder.TERRAIN_RippleMap = TERRAIN_RippleMap;
        holder.TERRAIN_RippleScale = TERRAIN_RippleScale;
        holder.TERRAIN_FlowScale = TERRAIN_FlowScale;
        holder.TERRAIN_FlowSpeed = TERRAIN_FlowSpeed;
        holder.TERRAIN_FlowCycleScale = TERRAIN_FlowCycleScale;
        holder.TERRAIN_FlowMipOffset = TERRAIN_FlowMipOffset;
        holder.TERRAIN_WetDarkening = TERRAIN_WetDarkening;
        holder.TERRAIN_WetDropletsStrength = TERRAIN_WetDropletsStrength;
        holder.TERRAIN_WetHeight_Treshold = TERRAIN_WetHeight_Treshold;
        holder.TERRAIN_WetHeight_Transition = TERRAIN_WetHeight_Transition;

        holder.TERRAIN_RainIntensity = TERRAIN_RainIntensity;
        holder.TERRAIN_DropletsSpeed = TERRAIN_DropletsSpeed;

        holder.TERRAIN_mipoffset_flowSpeed = TERRAIN_mipoffset_flowSpeed;

        // caustics
        holder.TERRAIN_CausticsAnimSpeed = TERRAIN_CausticsAnimSpeed;
        holder.TERRAIN_CausticsColor = TERRAIN_CausticsColor;
        holder.TERRAIN_CausticsWaterLevel = TERRAIN_CausticsWaterLevel;
        holder.TERRAIN_CausticsWaterLevelByAngle = TERRAIN_CausticsWaterLevelByAngle;
        holder.TERRAIN_CausticsWaterDeepFadeLength = TERRAIN_CausticsWaterDeepFadeLength;
        holder.TERRAIN_CausticsWaterShallowFadeLength = TERRAIN_CausticsWaterShallowFadeLength;
        holder.TERRAIN_CausticsTilingScale = TERRAIN_CausticsTilingScale;
        holder.TERRAIN_CausticsTex = TERRAIN_CausticsTex;

        //holder.rtp_customAmbientCorrection=rtp_customAmbientCorrection;
        //holder.TERRAIN_IBL_DiffAO_Damp=TERRAIN_IBL_DiffAO_Damp;
        //holder.TERRAIN_IBLRefl_SpecAO_Damp=TERRAIN_IBLRefl_SpecAO_Damp;
        //holder._CubemapDiff=_CubemapDiff;
        //holder._CubemapSpec=_CubemapSpec;

        holder.RTP_AOsharpness = RTP_AOsharpness;
        holder.RTP_AOamp = RTP_AOamp;
        holder._occlusionStrength = _occlusionStrength;

        holder.RTP_LightDefVector = RTP_LightDefVector;
        //holder.RTP_ReflexLightDiffuseColor=RTP_ReflexLightDiffuseColor;
        //holder.RTP_ReflexLightDiffuseColor2=RTP_ReflexLightDiffuseColor2;
        //holder.RTP_ReflexLightSpecColor=RTP_ReflexLightSpecColor;

        holder.EmissionRefractFiltering = EmissionRefractFiltering;
        holder.EmissionRefractAnimSpeed = EmissionRefractAnimSpeed;

        holder.VerticalTextureGlobalBumpInfluence = VerticalTextureGlobalBumpInfluence;
        holder.VerticalTextureTiling = VerticalTextureTiling;

        // snow
        holder._snow_strength = _snow_strength;
        holder._global_color_brightness_to_snow = _global_color_brightness_to_snow;
        holder._snow_slope_factor = _snow_slope_factor;
        holder._snow_edge_definition = _snow_edge_definition;
        holder._snow_height_treshold = _snow_height_treshold;
        holder._snow_height_transition = _snow_height_transition;
        holder._snow_color = _snow_color;
        //holder._snow_specular = _snow_specular;
        holder._snow_gloss = _snow_gloss;
        holder._snow_reflectivness = _snow_reflectivness;
        holder._snow_deep_factor = _snow_deep_factor;
        //holder._snow_fresnel = _snow_fresnel;
        holder._snow_diff_fresnel = _snow_diff_fresnel;
        //holder._snow_IBL_DiffuseStrength = _snow_IBL_DiffuseStrength;
        //holder._snow_IBL_SpecStrength = _snow_IBL_SpecStrength;


        holder._snow_metallic = _snow_metallic;
        holder._snow_Frost = _snow_Frost;
        holder._snow_MicroTiling = _snow_MicroTiling;
        holder._snow_BumpMicro = _snow_BumpMicro;
        holder._snow_occlusionStrength = _snow_occlusionStrength;
        holder._snow_TranslucencyDeferredLightIndex = _snow_TranslucencyDeferredLightIndex;
        holder._SnowGlitterColor = _SnowGlitterColor;

        holder._GlitterColor = _GlitterColor;
        holder._GlitterTiling = _GlitterTiling;
        holder._GlitterDensity = _GlitterDensity;
        holder._GlitterFilter = _GlitterFilter;
        holder._GlitterColorization = _GlitterColorization;
        holder._SparkleMap = _SparkleMap;


        //////////////////////
        // layer_dependent arrays
        //////////////////////
        holder.Bumps = new Texture2D[numLayers];
        holder.Spec = new float[numLayers];
        holder.FarSpecCorrection = new float[numLayers];
        holder.MixScale = new float[numLayers];
        holder.MixBlend = new float[numLayers];
        holder.MixSaturation = new float[numLayers];

        // RTP3.1		
        //holder.RTP_gloss2mask=new float[numLayers];
        //holder.RTP_gloss_mult=new float[numLayers];
        //holder.RTP_gloss_shaping=new float[numLayers];
        //holder.RTP_Fresnel=new float[numLayers];
        //holder.RTP_FresnelAtten=new float[numLayers];
        holder.RTP_DiffFresnel = new float[numLayers];
        //holder.RTP_IBL_bump_smoothness=new float[numLayers];
        //holder.RTP_IBL_DiffuseStrength=new float[numLayers];
        //holder.RTP_IBL_SpecStrength=new float[numLayers];
        //holder._DeferredSpecDampAddPass=new float[numLayers];
        holder.RTP_metallic = new float[numLayers];
        holder.RTP_glossMin = new float[numLayers];
        holder.RTP_glossMax = new float[numLayers];
        holder.RTP_glitter = new float[numLayers];


        holder.MixBrightness = new float[numLayers];
        holder.MixReplace = new float[numLayers];
        holder.LayerBrightness = new float[numLayers];
        holder.LayerBrightness2Spec = new float[numLayers];
        holder.LayerAlbedo2SpecColor = new float[numLayers];
        holder.LayerSaturation = new float[numLayers];
        holder.LayerEmission = new float[numLayers];
        holder.LayerEmissionColor = new Color[numLayers];
        holder.LayerEmissionRefractStrength = new float[numLayers];
        holder.LayerEmissionRefractHBedge = new float[numLayers];

        holder.GlobalColorPerLayer = new float[numLayers];
        holder.GlobalColorBottom = new float[numLayers];
        holder.GlobalColorTop = new float[numLayers];
        holder.GlobalColorColormapLoSat = new float[numLayers];
        holder.GlobalColorColormapHiSat = new float[numLayers];
        holder.GlobalColorLayerLoSat = new float[numLayers];
        holder.GlobalColorLayerHiSat = new float[numLayers];
        holder.GlobalColorLoBlend = new float[numLayers];
        holder.GlobalColorHiBlend = new float[numLayers];

        holder.PER_LAYER_HEIGHT_MODIFIER = new float[numLayers];
        holder._SuperDetailStrengthMultA = new float[numLayers];
        holder._SuperDetailStrengthMultASelfMaskNear = new float[numLayers];
        holder._SuperDetailStrengthMultASelfMaskFar = new float[numLayers];
        holder._SuperDetailStrengthMultB = new float[numLayers];
        holder._SuperDetailStrengthMultBSelfMaskNear = new float[numLayers];
        holder._SuperDetailStrengthMultBSelfMaskFar = new float[numLayers];
        holder._SuperDetailStrengthNormal = new float[numLayers];
        holder._BumpMapGlobalStrength = new float[numLayers];

        holder.VerticalTextureStrength = new float[numLayers];
        holder.AO_strength = new float[numLayers];

        holder.Heights = new Texture2D[numLayers];

        holder._snow_strength_per_layer = new float[numLayers];
#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
        holder.Substances = new ProceduralMaterial[numLayers];
#endif

        // wet
        holder.TERRAIN_LayerWetStrength = new float[numLayers];
        holder.TERRAIN_WaterLevel = new float[numLayers];
        holder.TERRAIN_WaterLevelSlopeDamp = new float[numLayers];
        holder.TERRAIN_WaterEdge = new float[numLayers];
        //holder.TERRAIN_WaterSpecularity=new float[numLayers];
        holder.TERRAIN_WaterGloss = new float[numLayers];
        holder.TERRAIN_WaterGlossDamper = new float[numLayers];
        holder.TERRAIN_Refraction = new float[numLayers];
        holder.TERRAIN_WetRefraction = new float[numLayers];
        holder.TERRAIN_Flow = new float[numLayers];
        holder.TERRAIN_WetFlow = new float[numLayers];
        //holder.TERRAIN_WetSpecularity=new float[numLayers];
        holder.TERRAIN_WaterMetallic = new float[numLayers];
        holder.TERRAIN_WetGloss = new float[numLayers];
        holder.TERRAIN_WaterColor = new Color[numLayers];
        //holder.TERRAIN_WaterIBL_SpecWetStrength=new float[numLayers];
        //holder.TERRAIN_WaterIBL_SpecWaterStrength=new float[numLayers];
        holder.TERRAIN_WaterEmission = new float[numLayers];

        holder._GlitterStrength = new float[numLayers];


        for (int i = 0; i < numLayers; i++)
        {
            holder.Bumps[i] = Bumps[i];
            //holder.Spec[i]=Spec[i];
            holder.FarSpecCorrection[i] = FarSpecCorrection[i];
            holder.MixScale[i] = MixScale[i];
            holder.MixBlend[i] = MixBlend[i];
            holder.MixSaturation[i] = MixSaturation[i];

            // >RTP3.1
            // update-set to default
            //if (CheckAndUpdate(ref RTP_gloss2mask, 0.5f, numLayers)) {
            //	for(int k=0; k<numLayers; k++) {
            //		Spec[k]=1; // zresetuj od razu mnoznik glossa (RTP3.1 - zmienna ma inne znaczenie)
            //	}
            //}
            //CheckAndUpdate(ref RTP_gloss_mult, 1f, numLayers);
            //CheckAndUpdate(ref RTP_gloss_shaping, 0.5f, numLayers);
            //CheckAndUpdate(ref RTP_Fresnel, 0, numLayers);
            //CheckAndUpdate(ref RTP_FresnelAtten, 0, numLayers);
            CheckAndUpdate(ref RTP_DiffFresnel, 0, numLayers);
            //CheckAndUpdate(ref RTP_IBL_bump_smoothness, 0.7f, numLayers);
            //CheckAndUpdate(ref RTP_IBL_DiffuseStrength, 0.5f, numLayers);
            //CheckAndUpdate(ref RTP_IBL_SpecStrength, 0.5f, numLayers);
            //CheckAndUpdate(ref _DeferredSpecDampAddPass, 1f, numLayers);
            CheckAndUpdate(ref RTP_metallic, 0, numLayers);
            CheckAndUpdate(ref RTP_glossMin, 0, numLayers);
            CheckAndUpdate(ref RTP_glossMax, 1, numLayers);
            CheckAndUpdate(ref RTP_glitter, 0, numLayers);

            //CheckAndUpdate(ref TERRAIN_WaterSpecularity, 0.5f, numLayers);
            CheckAndUpdate(ref TERRAIN_WaterGloss, 0.1f, numLayers);
            CheckAndUpdate(ref TERRAIN_WaterGlossDamper, 0f, numLayers);
            //CheckAndUpdate(ref TERRAIN_WetSpecularity, 0.2f, numLayers);
            CheckAndUpdate(ref TERRAIN_WaterMetallic, 0.1f, numLayers);
            CheckAndUpdate(ref TERRAIN_WetGloss, 0.05f, numLayers);
            CheckAndUpdate(ref TERRAIN_WetFlow, 0.05f, numLayers);

            CheckAndUpdate(ref MixBrightness, 2.0f, numLayers);
            CheckAndUpdate(ref MixReplace, 0.0f, numLayers);
            CheckAndUpdate(ref LayerBrightness, 1.0f, numLayers);
            CheckAndUpdate(ref LayerBrightness2Spec, 0.0f, numLayers);
            CheckAndUpdate(ref LayerAlbedo2SpecColor, 0.0f, numLayers);
            CheckAndUpdate(ref LayerSaturation, 1.0f, numLayers);
            CheckAndUpdate(ref LayerEmission, 0f, numLayers);
            CheckAndUpdate(ref LayerEmissionColor, Color.black, numLayers);
            CheckAndUpdate(ref LayerEmissionRefractStrength, 0f, numLayers);
            CheckAndUpdate(ref LayerEmissionRefractHBedge, 0f, numLayers);

            //CheckAndUpdate(ref TERRAIN_WaterIBL_SpecWetStrength, 0.1f, numLayers);
            //CheckAndUpdate(ref TERRAIN_WaterIBL_SpecWaterStrength, 0.5f, numLayers);
            CheckAndUpdate(ref TERRAIN_WaterEmission, 0.5f, numLayers);

            CheckAndUpdate(ref _GlitterStrength, 0.0f, numLayers);

            //holder.RTP_gloss2mask[i]=RTP_gloss2mask[i];
            //holder.RTP_gloss_mult[i]=RTP_gloss_mult[i];
            //holder.RTP_gloss_shaping[i]=RTP_gloss_shaping[i];
            //holder.RTP_Fresnel[i]=RTP_Fresnel[i];
            //holder.RTP_FresnelAtten[i]=RTP_FresnelAtten[i];
            holder.RTP_DiffFresnel[i] = RTP_DiffFresnel[i];
            //holder.RTP_IBL_bump_smoothness[i]=RTP_IBL_bump_smoothness[i];
            //holder.RTP_IBL_DiffuseStrength[i]=RTP_IBL_DiffuseStrength[i];
            //holder.RTP_IBL_SpecStrength[i]=RTP_IBL_SpecStrength[i];
            //holder._DeferredSpecDampAddPass[i]=_DeferredSpecDampAddPass[i];
            holder.RTP_metallic[i] = RTP_metallic[i];
            holder.RTP_glossMin[i] = RTP_glossMin[i];
            holder.RTP_glossMax[i] = RTP_glossMax[i];
            holder.RTP_glitter[i] = RTP_glitter[i];

            //holder.TERRAIN_WaterIBL_SpecWetStrength[i] = TERRAIN_WaterIBL_SpecWetStrength[i];
            //holder.TERRAIN_WaterIBL_SpecWaterStrength[i] = TERRAIN_WaterIBL_SpecWaterStrength[i];
            holder.TERRAIN_WaterEmission[i] = TERRAIN_WaterEmission[i];

            holder.MixBrightness[i] = MixBrightness[i];
            holder.MixReplace[i] = MixReplace[i];
            holder.LayerBrightness[i] = LayerBrightness[i];
            holder.LayerBrightness2Spec[i] = LayerBrightness2Spec[i];
            holder.LayerAlbedo2SpecColor[i] = LayerAlbedo2SpecColor[i];
            holder.LayerSaturation[i] = LayerSaturation[i];
            holder.LayerEmission[i] = LayerEmission[i];
            holder.LayerEmissionColor[i] = LayerEmissionColor[i];
            holder.LayerEmissionRefractStrength[i] = LayerEmissionRefractStrength[i];
            holder.LayerEmissionRefractHBedge[i] = LayerEmissionRefractHBedge[i];

            holder.GlobalColorPerLayer[i] = GlobalColorPerLayer[i];
            holder.GlobalColorBottom[i] = GlobalColorBottom[i];
            holder.GlobalColorTop[i] = GlobalColorTop[i];
            holder.GlobalColorColormapLoSat[i] = GlobalColorColormapLoSat[i];
            holder.GlobalColorColormapHiSat[i] = GlobalColorColormapHiSat[i];
            holder.GlobalColorLayerLoSat[i] = GlobalColorLayerLoSat[i];
            holder.GlobalColorLayerHiSat[i] = GlobalColorLayerHiSat[i];
            holder.GlobalColorLoBlend[i] = GlobalColorLoBlend[i];
            holder.GlobalColorHiBlend[i] = GlobalColorHiBlend[i];

            holder.PER_LAYER_HEIGHT_MODIFIER[i] = PER_LAYER_HEIGHT_MODIFIER[i];
            holder._SuperDetailStrengthMultA[i] = _SuperDetailStrengthMultA[i];
            holder._SuperDetailStrengthMultASelfMaskNear[i] = _SuperDetailStrengthMultASelfMaskNear[i];
            holder._SuperDetailStrengthMultASelfMaskFar[i] = _SuperDetailStrengthMultASelfMaskFar[i];
            holder._SuperDetailStrengthMultB[i] = _SuperDetailStrengthMultB[i];
            holder._SuperDetailStrengthMultBSelfMaskNear[i] = _SuperDetailStrengthMultBSelfMaskNear[i];
            holder._SuperDetailStrengthMultBSelfMaskFar[i] = _SuperDetailStrengthMultBSelfMaskFar[i];
            holder._SuperDetailStrengthNormal[i] = _SuperDetailStrengthNormal[i];
            holder._BumpMapGlobalStrength[i] = _BumpMapGlobalStrength[i];

            holder.VerticalTextureStrength[i] = VerticalTextureStrength[i];
            holder.AO_strength[i] = AO_strength[i];

            holder.Heights[i] = Heights[i];

            holder._snow_strength_per_layer[i] = _snow_strength_per_layer[i];
#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
            holder.Substances[i] = Substances[i];
#endif

            // wet
            holder.TERRAIN_LayerWetStrength[i] = TERRAIN_LayerWetStrength[i];
            holder.TERRAIN_WaterLevel[i] = TERRAIN_WaterLevel[i];
            holder.TERRAIN_WaterLevelSlopeDamp[i] = TERRAIN_WaterLevelSlopeDamp[i];
            holder.TERRAIN_WaterEdge[i] = TERRAIN_WaterEdge[i];
            //holder.TERRAIN_WaterSpecularity[i]=TERRAIN_WaterSpecularity[i];
            holder.TERRAIN_WaterGloss[i] = TERRAIN_WaterGloss[i];
            holder.TERRAIN_WaterGlossDamper[i] = TERRAIN_WaterGlossDamper[i];
            holder.TERRAIN_Refraction[i] = TERRAIN_Refraction[i];
            holder.TERRAIN_WetRefraction[i] = TERRAIN_WetRefraction[i];
            holder.TERRAIN_Flow[i] = TERRAIN_Flow[i];
            holder.TERRAIN_WetFlow[i] = TERRAIN_WetFlow[i];
            //holder.TERRAIN_WetSpecularity[i]=TERRAIN_WetSpecularity[i];
            holder.TERRAIN_WaterMetallic[i] = TERRAIN_WaterMetallic[i];
            holder.TERRAIN_WetGloss[i] = TERRAIN_WetGloss[i];
            holder.TERRAIN_WaterColor[i] = TERRAIN_WaterColor[i];

            holder._GlitterStrength[i] = _GlitterStrength[i];
        }
    }

    public void InterpolatePresets(ReliefTerrainPresetHolder holderA, ReliefTerrainPresetHolder holderB, float t)
    {
        RTP_MIP_BIAS = Mathf.Lerp(holderA.RTP_MIP_BIAS, holderB.RTP_MIP_BIAS, t);
        //_SpecColor=Color.Lerp(holderA._SpecColor, holderB._SpecColor, t);
        //RTP_DeferredAddPassSpec=Mathf.Lerp(holderA.RTP_DeferredAddPassSpec, holderB.RTP_DeferredAddPassSpec, t);

        MasterLayerBrightness = Mathf.Lerp(holderA.MasterLayerBrightness, holderB.MasterLayerBrightness, t);
        MasterLayerSaturation = Mathf.Lerp(holderA.MasterLayerSaturation, holderB.MasterLayerSaturation, t);

        BumpMapGlobalScale = Mathf.Lerp(holderA.BumpMapGlobalScale, holderB.BumpMapGlobalScale, t);
        GlobalColorMapBlendValues = Vector3.Lerp(holderA.GlobalColorMapBlendValues, holderB.GlobalColorMapBlendValues, t);
        GlobalColorMapSaturation = Mathf.Lerp(holderA.GlobalColorMapSaturation, holderB.GlobalColorMapSaturation, t);
        GlobalColorMapSaturationFar = Mathf.Lerp(holderA.GlobalColorMapSaturationFar, holderB.GlobalColorMapSaturationFar, t);
        //GlobalColorMapSaturationByPerlin=Mathf.Lerp(holderA.GlobalColorMapSaturationByPerlin, holderB.GlobalColorMapSaturationByPerlin, t);
        GlobalColorMapDistortByPerlin = Mathf.Lerp(holderA.GlobalColorMapDistortByPerlin, holderB.GlobalColorMapDistortByPerlin, t);
        GlobalColorMapBrightness = Mathf.Lerp(holderA.GlobalColorMapBrightness, holderB.GlobalColorMapBrightness, t);
        GlobalColorMapBrightnessFar = Mathf.Lerp(holderA.GlobalColorMapBrightnessFar, holderB.GlobalColorMapBrightnessFar, t);
        _GlobalColorMapNearMIP = Mathf.Lerp(holderA._GlobalColorMapNearMIP, holderB._GlobalColorMapNearMIP, t);
        _FarNormalDamp = Mathf.Lerp(holderA._FarNormalDamp, holderB._FarNormalDamp, t);

        blendMultiplier = Mathf.Lerp(holderA.blendMultiplier, holderB.blendMultiplier, t);

        ReliefTransform = Vector4.Lerp(holderA.ReliefTransform, holderB.ReliefTransform, t);
        DIST_STEPS = Mathf.Lerp(holderA.DIST_STEPS, holderB.DIST_STEPS, t);
        WAVELENGTH = Mathf.Lerp(holderA.WAVELENGTH, holderB.WAVELENGTH, t);
        ReliefBorderBlend = Mathf.Lerp(holderA.ReliefBorderBlend, holderB.ReliefBorderBlend, t);

        ExtrudeHeight = Mathf.Lerp(holderA.ExtrudeHeight, holderB.ExtrudeHeight, t);
        LightmapShading = Mathf.Lerp(holderA.LightmapShading, holderB.LightmapShading, t);

        SHADOW_STEPS = Mathf.Lerp(holderA.SHADOW_STEPS, holderB.SHADOW_STEPS, t);
        WAVELENGTH_SHADOWS = Mathf.Lerp(holderA.WAVELENGTH_SHADOWS, holderB.WAVELENGTH_SHADOWS, t);
        //SHADOW_SMOOTH_STEPS=Mathf.Lerp(holderA.SHADOW_SMOOTH_STEPS, holderB.SHADOW_SMOOTH_STEPS, t);
        SelfShadowStrength = Mathf.Lerp(holderA.SelfShadowStrength, holderB.SelfShadowStrength, t);
        ShadowSmoothing = Mathf.Lerp(holderA.ShadowSmoothing, holderB.ShadowSmoothing, t);
        ShadowSoftnessFade = Mathf.Lerp(holderA.ShadowSoftnessFade, holderB.ShadowSoftnessFade, t);

        distance_start = Mathf.Lerp(holderA.distance_start, holderB.distance_start, t);
        distance_transition = Mathf.Lerp(holderA.distance_transition, holderB.distance_transition, t);
        distance_start_bumpglobal = Mathf.Lerp(holderA.distance_start_bumpglobal, holderB.distance_start_bumpglobal, t);
        distance_transition_bumpglobal = Mathf.Lerp(holderA.distance_transition_bumpglobal, holderB.distance_transition_bumpglobal, t);
        rtp_perlin_start_val = Mathf.Lerp(holderA.rtp_perlin_start_val, holderB.rtp_perlin_start_val, t);

        // (interpolating this makes no sense)
        //_Phong=Mathf.Lerp(holderA._Phong, holderB._Phong, t);
        //tessHeight=Mathf.Lerp(holderA.tessHeight, holderB.tessHeight, t);

        trees_shadow_distance_start = Mathf.Lerp(holderA.trees_shadow_distance_start, holderB.trees_shadow_distance_start, t);
        trees_shadow_distance_transition = Mathf.Lerp(holderA.trees_shadow_distance_transition, holderB.trees_shadow_distance_transition, t);
        trees_shadow_value = Mathf.Lerp(holderA.trees_shadow_value, holderB.trees_shadow_value, t);
        trees_pixel_distance_start = Mathf.Lerp(holderA.trees_pixel_distance_start, holderB.trees_pixel_distance_start, t);
        trees_pixel_distance_transition = Mathf.Lerp(holderA.trees_pixel_distance_transition, holderB.trees_pixel_distance_transition, t);
        trees_pixel_blend_val = Mathf.Lerp(holderA.trees_pixel_blend_val, holderB.trees_pixel_blend_val, t);
        global_normalMap_multiplier = Mathf.Lerp(holderA.global_normalMap_multiplier, holderB.global_normalMap_multiplier, t);
        global_normalMap_farUsage = Mathf.Lerp(holderA.global_normalMap_farUsage, holderB.global_normalMap_farUsage, t);

        _AmbientEmissiveMultiplier = Mathf.Lerp(holderA._AmbientEmissiveMultiplier, holderB._AmbientEmissiveMultiplier, t);
        _AmbientEmissiveRelief = Mathf.Lerp(holderA._AmbientEmissiveRelief, holderB._AmbientEmissiveRelief, t);

        _SuperDetailTiling = Mathf.Lerp(holderA._SuperDetailTiling, holderB._SuperDetailTiling, t);

        // water/wet
        TERRAIN_GlobalWetness = Mathf.Lerp(holderA.TERRAIN_GlobalWetness, holderB.TERRAIN_GlobalWetness, t);

        TERRAIN_RippleScale = Mathf.Lerp(holderA.TERRAIN_RippleScale, holderB.TERRAIN_RippleScale, t);
        TERRAIN_FlowScale = Mathf.Lerp(holderA.TERRAIN_FlowScale, holderB.TERRAIN_FlowScale, t);
        TERRAIN_FlowSpeed = Mathf.Lerp(holderA.TERRAIN_FlowSpeed, holderB.TERRAIN_FlowSpeed, t);
        TERRAIN_FlowCycleScale = Mathf.Lerp(holderA.TERRAIN_FlowCycleScale, holderB.TERRAIN_FlowCycleScale, t);
        TERRAIN_FlowMipOffset = Mathf.Lerp(holderA.TERRAIN_FlowMipOffset, holderB.TERRAIN_FlowMipOffset, t);
        TERRAIN_WetDarkening = Mathf.Lerp(holderA.TERRAIN_WetDarkening, holderB.TERRAIN_WetDarkening, t);
        TERRAIN_WetDropletsStrength = Mathf.Lerp(holderA.TERRAIN_WetDropletsStrength, holderB.TERRAIN_WetDropletsStrength, t);
        TERRAIN_WetHeight_Treshold = Mathf.Lerp(holderA.TERRAIN_WetHeight_Treshold, holderB.TERRAIN_WetHeight_Treshold, t);
        TERRAIN_WetHeight_Transition = Mathf.Lerp(holderA.TERRAIN_WetHeight_Transition, holderB.TERRAIN_WetHeight_Transition, t);

        TERRAIN_RainIntensity = Mathf.Lerp(holderA.TERRAIN_RainIntensity, holderB.TERRAIN_RainIntensity, t);
        TERRAIN_DropletsSpeed = Mathf.Lerp(holderA.TERRAIN_DropletsSpeed, holderB.TERRAIN_DropletsSpeed, t);

        TERRAIN_mipoffset_flowSpeed = Mathf.Lerp(holderA.TERRAIN_mipoffset_flowSpeed, holderB.TERRAIN_mipoffset_flowSpeed, t);

        TERRAIN_CausticsAnimSpeed = Mathf.Lerp(holderA.TERRAIN_CausticsAnimSpeed, holderB.TERRAIN_CausticsAnimSpeed, t);
        TERRAIN_CausticsColor = Color.Lerp(holderA.TERRAIN_CausticsColor, holderB.TERRAIN_CausticsColor, t);
        TERRAIN_CausticsWaterLevel = Mathf.Lerp(holderA.TERRAIN_CausticsWaterLevel, holderB.TERRAIN_CausticsWaterLevel, t);
        TERRAIN_CausticsWaterLevelByAngle = Mathf.Lerp(holderA.TERRAIN_CausticsWaterLevelByAngle, holderB.TERRAIN_CausticsWaterLevelByAngle, t);
        TERRAIN_CausticsWaterDeepFadeLength = Mathf.Lerp(holderA.TERRAIN_CausticsWaterDeepFadeLength, holderB.TERRAIN_CausticsWaterDeepFadeLength, t);
        TERRAIN_CausticsWaterShallowFadeLength = Mathf.Lerp(holderA.TERRAIN_CausticsWaterShallowFadeLength, holderB.TERRAIN_CausticsWaterShallowFadeLength, t);
        TERRAIN_CausticsTilingScale = Mathf.Lerp(holderA.TERRAIN_CausticsTilingScale, holderB.TERRAIN_CausticsTilingScale, t);

        //rtp_customAmbientCorrection=Color.Lerp(holderA.rtp_customAmbientCorrection, holderB.rtp_customAmbientCorrection, t);
        //TERRAIN_IBL_DiffAO_Damp=Mathf.Lerp(holderA.TERRAIN_IBL_DiffAO_Damp, holderB.TERRAIN_IBL_DiffAO_Damp, t);
        //TERRAIN_IBLRefl_SpecAO_Damp=Mathf.Lerp(holderA.TERRAIN_IBLRefl_SpecAO_Damp, holderB.TERRAIN_IBLRefl_SpecAO_Damp, t);

        RTP_AOsharpness = Mathf.Lerp(holderA.RTP_AOsharpness, holderB.RTP_AOsharpness, t);
        RTP_AOamp = Mathf.Lerp(holderA.RTP_AOamp, holderB.RTP_AOamp, t);
        _occlusionStrength = Mathf.Lerp(holderA._occlusionStrength, holderB._occlusionStrength, t);

        RTP_LightDefVector = Vector4.Lerp(holderA.RTP_LightDefVector, holderB.RTP_LightDefVector, t);
        //RTP_ReflexLightDiffuseColor=Color.Lerp(holderA.RTP_ReflexLightDiffuseColor, holderB.RTP_ReflexLightDiffuseColor, t);
        //RTP_ReflexLightDiffuseColor2=Color.Lerp(holderA.RTP_ReflexLightDiffuseColor2, holderB.RTP_ReflexLightDiffuseColor2, t);
        //RTP_ReflexLightSpecColor=Color.Lerp(holderA.RTP_ReflexLightSpecColor, holderB.RTP_ReflexLightSpecColor, t);

        EmissionRefractFiltering = Mathf.Lerp(holderA.EmissionRefractFiltering, holderB.EmissionRefractFiltering, t);
        EmissionRefractAnimSpeed = Mathf.Lerp(holderA.EmissionRefractAnimSpeed, holderB.EmissionRefractAnimSpeed, t);

        VerticalTextureGlobalBumpInfluence = Mathf.Lerp(holderA.VerticalTextureGlobalBumpInfluence, holderB.VerticalTextureGlobalBumpInfluence, t);
        VerticalTextureTiling = Mathf.Lerp(holderA.VerticalTextureTiling, holderB.VerticalTextureTiling, t);

        // snow
        _snow_strength = Mathf.Lerp(holderA._snow_strength, holderB._snow_strength, t);
        _global_color_brightness_to_snow = Mathf.Lerp(holderA._global_color_brightness_to_snow, holderB._global_color_brightness_to_snow, t);
        _snow_slope_factor = Mathf.Lerp(holderA._snow_slope_factor, holderB._snow_slope_factor, t);
        _snow_edge_definition = Mathf.Lerp(holderA._snow_edge_definition, holderB._snow_edge_definition, t);
        _snow_height_treshold = Mathf.Lerp(holderA._snow_height_treshold, holderB._snow_height_treshold, t);
        _snow_height_transition = Mathf.Lerp(holderA._snow_height_transition, holderB._snow_height_transition, t);
        _snow_color = Color.Lerp(holderA._snow_color, holderB._snow_color, t);
        //_snow_specular = Mathf.Lerp(holderA._snow_specular, holderB._snow_specular, t);
        _snow_gloss = Mathf.Lerp(holderA._snow_gloss, holderB._snow_gloss, t);
        _snow_reflectivness = Mathf.Lerp(holderA._snow_reflectivness, holderB._snow_reflectivness, t);
        _snow_deep_factor = Mathf.Lerp(holderA._snow_deep_factor, holderB._snow_deep_factor, t);
        //_snow_fresnel = Mathf.Lerp(holderA._snow_fresnel, holderB._snow_fresnel, t);
        _snow_diff_fresnel = Mathf.Lerp(holderA._snow_diff_fresnel, holderB._snow_diff_fresnel, t);
        //_snow_IBL_DiffuseStrength = Mathf.Lerp(holderA._snow_IBL_DiffuseStrength, holderB._snow_IBL_DiffuseStrength, t);
        //_snow_IBL_SpecStrength = Mathf.Lerp(holderA._snow_IBL_SpecStrength, holderB._snow_IBL_SpecStrength, t);

        _snow_metallic = Mathf.Lerp(holderA._snow_metallic, holderB._snow_metallic, t);
        _snow_Frost = Mathf.Lerp(holderA._snow_Frost, holderB._snow_Frost, t);
        _snow_MicroTiling = Mathf.Lerp(holderA._snow_MicroTiling, holderB._snow_MicroTiling, t);
        _snow_BumpMicro = Mathf.Lerp(holderA._snow_BumpMicro, holderB._snow_BumpMicro, t);
        _snow_occlusionStrength = Mathf.Lerp(holderA._snow_occlusionStrength, holderB._snow_occlusionStrength, t);
        _SnowGlitterColor = Color.Lerp(holderA._SnowGlitterColor, holderB._SnowGlitterColor, t);

        _GlitterColor = Color.Lerp(holderA._GlitterColor, holderB._GlitterColor, t);
        _GlitterTiling = Mathf.Lerp(holderA._GlitterTiling, holderB._GlitterTiling, t);
        _GlitterDensity = Mathf.Lerp(holderA._GlitterDensity, holderB._GlitterDensity, t);
        _GlitterFilter = Mathf.Lerp(holderA._GlitterFilter, holderB._GlitterFilter, t);
        _GlitterColorization = Mathf.Lerp(holderA._GlitterColorization, holderB._GlitterColorization, t);

        //////////////////////
        // layer_dependent arrays
        //////////////////////
        for (int i = 0; i < holderA.Spec.Length; i++)
        {
            if (i < FarSpecCorrection.Length)
            {
                //Spec[i]=Mathf.Lerp(holderA.Spec[i], holderB.Spec[i], t);
                FarSpecCorrection[i] = Mathf.Lerp(holderA.FarSpecCorrection[i], holderB.FarSpecCorrection[i], t);
                MixScale[i] = Mathf.Lerp(holderA.MixScale[i], holderB.MixScale[i], t);
                MixBlend[i] = Mathf.Lerp(holderA.MixBlend[i], holderB.MixBlend[i], t);
                MixSaturation[i] = Mathf.Lerp(holderA.MixSaturation[i], holderB.MixSaturation[i], t);

                // RTP3.1
                //RTP_gloss2mask[i]=Mathf.Lerp(holderA.RTP_gloss2mask[i], holderB.RTP_gloss2mask[i], t);
                //RTP_gloss_mult[i]=Mathf.Lerp(holderA.RTP_gloss_mult[i], holderB.RTP_gloss_mult[i], t);
                //RTP_gloss_shaping[i]=Mathf.Lerp(holderA.RTP_gloss_shaping[i], holderB.RTP_gloss_shaping[i], t);
                //RTP_Fresnel[i]=Mathf.Lerp(holderA.RTP_Fresnel[i], holderB.RTP_Fresnel[i], t);
                //RTP_FresnelAtten[i]=Mathf.Lerp(holderA.RTP_FresnelAtten[i], holderB.RTP_FresnelAtten[i], t);
                RTP_DiffFresnel[i] = Mathf.Lerp(holderA.RTP_DiffFresnel[i], holderB.RTP_DiffFresnel[i], t);
                //RTP_IBL_bump_smoothness[i]=Mathf.Lerp(holderA.RTP_IBL_bump_smoothness[i], holderB.RTP_IBL_bump_smoothness[i], t);
                //RTP_IBL_DiffuseStrength[i]=Mathf.Lerp(holderA.RTP_IBL_DiffuseStrength[i], holderB.RTP_IBL_DiffuseStrength[i], t);
                //RTP_IBL_SpecStrength[i]=Mathf.Lerp(holderA.RTP_IBL_SpecStrength[i], holderB.RTP_IBL_SpecStrength[i], t);
                //_DeferredSpecDampAddPass[i]=Mathf.Lerp(holderA._DeferredSpecDampAddPass[i], holderB._DeferredSpecDampAddPass[i], t);
                RTP_metallic[i] = Mathf.Lerp(holderA.RTP_metallic[i], holderB.RTP_metallic[i], t);
                RTP_glossMin[i] = Mathf.Lerp(holderA.RTP_glossMin[i], holderB.RTP_glossMin[i], t);
                RTP_glossMax[i] = Mathf.Lerp(holderA.RTP_glossMax[i], holderB.RTP_glossMax[i], t);
                RTP_glitter[i] = Mathf.Lerp(holderA.RTP_glitter[i], holderB.RTP_glitter[i], t);

                MixBrightness[i] = Mathf.Lerp(holderA.MixBrightness[i], holderB.MixBrightness[i], t);
                MixReplace[i] = Mathf.Lerp(holderA.MixReplace[i], holderB.MixReplace[i], t);
                LayerBrightness[i] = Mathf.Lerp(holderA.LayerBrightness[i], holderB.LayerBrightness[i], t);
                LayerBrightness2Spec[i] = Mathf.Lerp(holderA.LayerBrightness2Spec[i], holderB.LayerBrightness2Spec[i], t);
                LayerAlbedo2SpecColor[i] = Mathf.Lerp(holderA.LayerAlbedo2SpecColor[i], holderB.LayerAlbedo2SpecColor[i], t);
                LayerSaturation[i] = Mathf.Lerp(holderA.LayerSaturation[i], holderB.LayerSaturation[i], t);
                LayerEmission[i] = Mathf.Lerp(holderA.LayerEmission[i], holderB.LayerEmission[i], t);
                LayerEmissionColor[i] = Color.Lerp(holderA.LayerEmissionColor[i], holderB.LayerEmissionColor[i], t);
                LayerEmissionRefractStrength[i] = Mathf.Lerp(holderA.LayerEmissionRefractStrength[i], holderB.LayerEmissionRefractStrength[i], t);
                LayerEmissionRefractHBedge[i] = Mathf.Lerp(holderA.LayerEmissionRefractHBedge[i], holderB.LayerEmissionRefractHBedge[i], t);

                GlobalColorPerLayer[i] = Mathf.Lerp(holderA.GlobalColorPerLayer[i], holderB.GlobalColorPerLayer[i], t);
                GlobalColorBottom[i] = Mathf.Lerp(holderA.GlobalColorBottom[i], holderB.GlobalColorBottom[i], t);
                GlobalColorTop[i] = Mathf.Lerp(holderA.GlobalColorTop[i], holderB.GlobalColorTop[i], t);
                GlobalColorColormapLoSat[i] = Mathf.Lerp(holderA.GlobalColorColormapLoSat[i], holderB.GlobalColorColormapLoSat[i], t);
                GlobalColorColormapHiSat[i] = Mathf.Lerp(holderA.GlobalColorColormapHiSat[i], holderB.GlobalColorColormapHiSat[i], t);
                GlobalColorLayerLoSat[i] = Mathf.Lerp(holderA.GlobalColorLayerLoSat[i], holderB.GlobalColorLayerLoSat[i], t);
                GlobalColorLayerHiSat[i] = Mathf.Lerp(holderA.GlobalColorLayerHiSat[i], holderB.GlobalColorLayerHiSat[i], t);
                GlobalColorLoBlend[i] = Mathf.Lerp(holderA.GlobalColorLoBlend[i], holderB.GlobalColorLoBlend[i], t);
                GlobalColorHiBlend[i] = Mathf.Lerp(holderA.GlobalColorHiBlend[i], holderB.GlobalColorHiBlend[i], t);

                PER_LAYER_HEIGHT_MODIFIER[i] = Mathf.Lerp(holderA.PER_LAYER_HEIGHT_MODIFIER[i], holderB.PER_LAYER_HEIGHT_MODIFIER[i], t);
                _SuperDetailStrengthMultA[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultA[i], holderB._SuperDetailStrengthMultA[i], t);
                _SuperDetailStrengthMultASelfMaskNear[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultASelfMaskNear[i], holderB._SuperDetailStrengthMultASelfMaskNear[i], t);
                _SuperDetailStrengthMultASelfMaskFar[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultASelfMaskFar[i], holderB._SuperDetailStrengthMultASelfMaskFar[i], t);
                _SuperDetailStrengthMultB[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultB[i], holderB._SuperDetailStrengthMultB[i], t);
                _SuperDetailStrengthMultBSelfMaskNear[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultBSelfMaskNear[i], holderB._SuperDetailStrengthMultBSelfMaskNear[i], t);
                _SuperDetailStrengthMultBSelfMaskFar[i] = Mathf.Lerp(holderA._SuperDetailStrengthMultBSelfMaskFar[i], holderB._SuperDetailStrengthMultBSelfMaskFar[i], t);
                _SuperDetailStrengthNormal[i] = Mathf.Lerp(holderA._SuperDetailStrengthNormal[i], holderB._SuperDetailStrengthNormal[i], t);
                _BumpMapGlobalStrength[i] = Mathf.Lerp(holderA._BumpMapGlobalStrength[i], holderB._BumpMapGlobalStrength[i], t);

                AO_strength[i] = Mathf.Lerp(holderA.AO_strength[i], holderB.AO_strength[i], t);
                VerticalTextureStrength[i] = Mathf.Lerp(holderA.VerticalTextureStrength[i], holderB.VerticalTextureStrength[i], t);

                _snow_strength_per_layer[i] = Mathf.Lerp(holderA._snow_strength_per_layer[i], holderB._snow_strength_per_layer[i], t);

                // wet
                TERRAIN_LayerWetStrength[i] = Mathf.Lerp(holderA.TERRAIN_LayerWetStrength[i], holderB.TERRAIN_LayerWetStrength[i], t);
                TERRAIN_WaterLevel[i] = Mathf.Lerp(holderA.TERRAIN_WaterLevel[i], holderB.TERRAIN_WaterLevel[i], t);
                TERRAIN_WaterLevelSlopeDamp[i] = Mathf.Lerp(holderA.TERRAIN_WaterLevelSlopeDamp[i], holderB.TERRAIN_WaterLevelSlopeDamp[i], t);
                TERRAIN_WaterEdge[i] = Mathf.Lerp(holderA.TERRAIN_WaterEdge[i], holderB.TERRAIN_WaterEdge[i], t);
                //TERRAIN_WaterSpecularity[i]=Mathf.Lerp(holderA.TERRAIN_WaterSpecularity[i], holderB.TERRAIN_WaterSpecularity[i], t);
                TERRAIN_WaterGloss[i] = Mathf.Lerp(holderA.TERRAIN_WaterGloss[i], holderB.TERRAIN_WaterGloss[i], t);
                TERRAIN_WaterGlossDamper[i] = Mathf.Lerp(holderA.TERRAIN_WaterGlossDamper[i], holderB.TERRAIN_WaterGlossDamper[i], t);
                TERRAIN_Refraction[i] = Mathf.Lerp(holderA.TERRAIN_Refraction[i], holderB.TERRAIN_Refraction[i], t);
                TERRAIN_WetRefraction[i] = Mathf.Lerp(holderA.TERRAIN_WetRefraction[i], holderB.TERRAIN_WetRefraction[i], t);
                TERRAIN_Flow[i] = Mathf.Lerp(holderA.TERRAIN_Flow[i], holderB.TERRAIN_Flow[i], t);
                TERRAIN_WetFlow[i] = Mathf.Lerp(holderA.TERRAIN_WetFlow[i], holderB.TERRAIN_WetFlow[i], t);
                //TERRAIN_WetSpecularity[i]=Mathf.Lerp(holderA.TERRAIN_WetSpecularity[i], holderB.TERRAIN_WetSpecularity[i], t);
                TERRAIN_WaterMetallic[i] = Mathf.Lerp(holderA.TERRAIN_WaterMetallic[i], holderB.TERRAIN_WaterMetallic[i], t);
                TERRAIN_WetGloss[i] = Mathf.Lerp(holderA.TERRAIN_WetGloss[i], holderB.TERRAIN_WetGloss[i], t);
                TERRAIN_WaterColor[i] = Color.Lerp(holderA.TERRAIN_WaterColor[i], holderB.TERRAIN_WaterColor[i], t);
                //TERRAIN_WaterIBL_SpecWetStrength[i]=Mathf.Lerp(holderA.TERRAIN_WaterIBL_SpecWetStrength[i], holderB.TERRAIN_WaterIBL_SpecWetStrength[i], t);
                //TERRAIN_WaterIBL_SpecWaterStrength[i]=Mathf.Lerp(holderA.TERRAIN_WaterIBL_SpecWaterStrength[i], holderB.TERRAIN_WaterIBL_SpecWaterStrength[i], t);
                TERRAIN_WaterEmission[i] = Mathf.Lerp(holderA.TERRAIN_WaterEmission[i], holderB.TERRAIN_WaterEmission[i], t);

                // glitter
                _GlitterStrength[i] = Mathf.Lerp(holderA._GlitterStrength[i], holderB._GlitterStrength[i], t);
            }
        }
    }

    public void ReturnToDefaults(string what = "", int layerIdx = -1)
    {
        // main settings
        if (what == "" || what == "main")
        {
            ReliefTransform = new Vector4(3, 3, 0, 0);
            distance_start = 5f;
            distance_transition = 20f;
            //_SpecColor=new Color(200.0f/255.0f, 200.0f/255.0f, 200.0f/255.0f, 1);
            //RTP_DeferredAddPassSpec=0.5f;
            //rtp_customAmbientCorrection=new Color(0.2f, 0.2f, 0.2f, 1);
            //TERRAIN_IBL_DiffAO_Damp=0.25f;
            //TERRAIN_IBLRefl_SpecAO_Damp=0.5f;
            RTP_LightDefVector = new Vector4(0.05f, 0.5f, 0.5f, 25.0f);
            //RTP_ReflexLightDiffuseColor=new Color(202.0f/255.0f, 240.0f/255.0f, 1, 0.2f);
            //RTP_ReflexLightDiffuseColor2=new Color(202.0f/255.0f, 240.0f/255.0f, 1, 0.2f);
            //RTP_ReflexLightSpecColor=new Color(240.0f/255.0f, 245.0f/255.0f, 1, 0.15f);

            ReliefBorderBlend = 6;
            LightmapShading = 0f;
            RTP_MIP_BIAS = 0;
            RTP_AOsharpness = 1.5f;
            RTP_AOamp = 0.1f;
            _occlusionStrength = 1.0f;

            MasterLayerBrightness = 1;
            MasterLayerSaturation = 1;

            EmissionRefractFiltering = 4;
            EmissionRefractAnimSpeed = 4;
        }

        //perlin
        if (what == "" || what == "perlin")
        {
            BumpMapGlobalScale = 0.1f;
            _FarNormalDamp = 0.2f;
            distance_start_bumpglobal = 30f;
            distance_transition_bumpglobal = 30f;
            rtp_perlin_start_val = 0;
        }

        // global color
        if (what == "" || what == "global_color")
        {
            GlobalColorMapBlendValues = new Vector3(0.2f, 0.4f, 0.5f);
            GlobalColorMapSaturation = 1;
            GlobalColorMapSaturationFar = 1;
            //GlobalColorMapSaturationByPerlin=0.2f;
            GlobalColorMapDistortByPerlin = 0.005f;
            GlobalColorMapBrightness = 1;
            GlobalColorMapBrightnessFar = 1;
            _GlobalColorMapNearMIP = 0;

            trees_shadow_distance_start = 50;
            trees_shadow_distance_transition = 10;
            trees_shadow_value = 0.5f;
            trees_pixel_distance_start = 500;
            trees_pixel_distance_transition = 10;
            trees_pixel_blend_val = 2;
            global_normalMap_multiplier = 1;
            global_normalMap_farUsage = 0;
            _Phong = 0;
            tessHeight = 300;
            _TessSubdivisions = 1;
            _TessSubdivisionsFar = 1;
            _TessYOffset = 0;

            _AmbientEmissiveMultiplier = 1;
            _AmbientEmissiveRelief = 0.5f;
        }

        // uvblend
        if (what == "" || what == "uvblend")
        {
            blendMultiplier = 1;
        }

        // POM/PM settings
        if (what == "" || what == "pom/pm")
        {
            ExtrudeHeight = 0.05f;
            DIST_STEPS = 20;
            WAVELENGTH = 2;
            SHADOW_STEPS = 20f;
            WAVELENGTH_SHADOWS = 2f;
            //SHADOW_SMOOTH_STEPS=6f;
            SelfShadowStrength = 0.8f;
            ShadowSmoothing = 1f;
            ShadowSoftnessFade = 0.8f;
        }

        // snow global
        if (what == "" || what == "snow")
        {
            _global_color_brightness_to_snow = 0.5f;
            _snow_strength = 0;
            _snow_slope_factor = 2;
            _snow_edge_definition = 2;
            _snow_height_treshold = -200;
            _snow_height_transition = 1;
            _snow_color = Color.white;
            //_snow_specular = 0.5f;
            _snow_gloss = 0.7f;
            _snow_reflectivness = 0.7f;
            _snow_deep_factor = 1.5f;
            //_snow_fresnel = 0.5f;
            _snow_diff_fresnel = 0.5f;
            _SnowGlitterColor = new Color(1, 1, 1, 0.1f);
            //_snow_IBL_DiffuseStrength = 0.5f;
            //_snow_IBL_SpecStrength = 0.5f;
        }

        // superdetail
        if (what == "" || what == "superdetail")
        {
            _SuperDetailTiling = 8;
        }

        // vertical texturing
        if (what == "" || what == "vertical")
        {
            VerticalTextureGlobalBumpInfluence = 0;
            VerticalTextureTiling = 50f;
        }

        // glitter
        if (what == "" || what == "glitter")
        {

            _GlitterColor = new Color(1, 1, 1, 0.1f);
            _GlitterTiling = 1;
            _GlitterDensity = 0.1f;
            _GlitterFilter = 0;
            _GlitterColorization = 0.5f;
        }

        // water
        if (what == "" || what == "water")
        {
            TERRAIN_GlobalWetness = 1;
            TERRAIN_RippleScale = 4;
            TERRAIN_FlowScale = 1;
            TERRAIN_FlowSpeed = 0.5f;
            TERRAIN_FlowCycleScale = 1f;
            TERRAIN_RainIntensity = 1;
            TERRAIN_DropletsSpeed = 10;

            TERRAIN_mipoffset_flowSpeed = 1;

            TERRAIN_FlowMipOffset = 0;
            TERRAIN_WetDarkening = 0.5f;
            TERRAIN_WetDropletsStrength = 0f;
            TERRAIN_WetHeight_Treshold = -200;
            TERRAIN_WetHeight_Transition = 5;
        }

        // caustics
        if (what == "" || what == "caustics")
        {
            TERRAIN_CausticsAnimSpeed = 2;
            TERRAIN_CausticsColor = Color.white;
            TERRAIN_CausticsWaterLevel = 30;
            TERRAIN_CausticsWaterLevelByAngle = 2;
            TERRAIN_CausticsWaterDeepFadeLength = 50;
            TERRAIN_CausticsWaterShallowFadeLength = 30;
            TERRAIN_CausticsTilingScale = 1;
        }

        // layer
        if (what == "" || what == "layer")
        {
            int b = 0;
            int e = numLayers < 12 ? numLayers : 12;
            if (layerIdx >= 0)
            {
                b = layerIdx;
                e = layerIdx + 1;
            }
            for (int j = b; j < e; j++)
            {
                //Spec[j]=1f; //RTP3.1 - mnoznik glossa (inne znaczenie)
                FarSpecCorrection[j] = 0;
                MIPmult[j] = 0.0f;
                MixScale[j] = 0.2f;
                MixBlend[j] = 0.5f;
                MixSaturation[j] = 0.3f;

                // RTP3.1
                //RTP_gloss2mask[j]=0.5f;
                //RTP_gloss_mult[j]=1;
                //RTP_gloss_shaping[j]=0.5f;
                //RTP_Fresnel[j]=0;
                //RTP_FresnelAtten[j]=0;
                RTP_DiffFresnel[j] = 0;
                //RTP_IBL_bump_smoothness[j]=0.7f;
                //RTP_IBL_DiffuseStrength[j]=0.5f;
                //RTP_IBL_SpecStrength[j]=0.5f;
                //_DeferredSpecDampAddPass[j]=1;
                RTP_metallic[j] = 0.0f;
                RTP_glossMin[j] = 0.0f;
                RTP_glossMax[j] = 1.0f;
                RTP_glitter[j] = 0.0f;

                MixBrightness[j] = 2.0f;
                MixReplace[j] = 0.0f;
                LayerBrightness[j] = 1.0f;
                LayerBrightness2Spec[j] = 0.0f;
                LayerAlbedo2SpecColor[j] = 0.0f;
                LayerSaturation[j] = 1.0f;
                LayerEmission[j] = 0f;
                LayerEmissionColor[j] = Color.black;
                LayerEmissionRefractStrength[j] = 0f;
                LayerEmissionRefractHBedge[j] = 0f;

                GlobalColorPerLayer[j] = 1.0f;

                GlobalColorBottom[j] = 0.0f;
                GlobalColorTop[j] = 1.0f;
                GlobalColorColormapLoSat[j] = 1.0f;
                GlobalColorColormapHiSat[j] = 1.0f;
                GlobalColorLayerLoSat[j] = 1.0f;
                GlobalColorLayerHiSat[j] = 1.0f;
                GlobalColorLoBlend[j] = 1.0f;
                GlobalColorHiBlend[j] = 1.0f;

                PER_LAYER_HEIGHT_MODIFIER[j] = 0;

                _SuperDetailStrengthMultA[j] = 0;
                _SuperDetailStrengthMultASelfMaskNear[j] = 0;
                _SuperDetailStrengthMultASelfMaskFar[j] = 0;
                _SuperDetailStrengthMultB[j] = 0;
                _SuperDetailStrengthMultBSelfMaskNear[j] = 0;
                _SuperDetailStrengthMultBSelfMaskFar[j] = 0;
                _SuperDetailStrengthNormal[j] = 0;
                _BumpMapGlobalStrength[j] = 0.3f;

                _snow_strength_per_layer[j] = 1;

                VerticalTextureStrength[j] = 0.5f;
                AO_strength[j] = 1;

                TERRAIN_LayerWetStrength[j] = 1;
                TERRAIN_WaterLevel[j] = 0.5f;
                TERRAIN_WaterLevelSlopeDamp[j] = 0.5f;
                TERRAIN_WaterEdge[j] = 2;
                //TERRAIN_WaterSpecularity[j]=0.5f; // spec boost
                TERRAIN_WaterGloss[j] = 0.1f; // gloss boost
                TERRAIN_WaterGlossDamper[j] = 0;
                TERRAIN_Refraction[j] = 0.01f;
                TERRAIN_WetRefraction[j] = 0.2f;
                TERRAIN_Flow[j] = 0.3f;
                TERRAIN_WetFlow[j] = 0.05f;
                //TERRAIN_WetSpecularity[j]=0.2f;
                TERRAIN_WaterMetallic[j] = 0.1f;
                TERRAIN_WetGloss[j] = 0.05f;
                TERRAIN_WaterColor[j] = new Color(0.9f, 0.9f, 1, 0.5f);
                //TERRAIN_WaterIBL_SpecWetStrength[j]=0.5f;
                //TERRAIN_WaterIBL_SpecWaterStrength[j]=0.5f;
                TERRAIN_WaterEmission[j] = 0f;

                _GlitterStrength[j] = 0f;
            }
        }

    }

    public bool CheckAndUpdate(ref float[] aLayerPropArray, float defVal, int len)
    {
        if (aLayerPropArray == null || aLayerPropArray.Length < len)
        {
            aLayerPropArray = new float[len];
            for (int k = 0; k < len; k++)
            {
                aLayerPropArray[k] = defVal;
            }
            return true;
        }
        return false; // no update
    }
    public bool CheckAndUpdate(ref Color[] aLayerPropArray, Color defVal, int len)
    {
        if (aLayerPropArray == null || aLayerPropArray.Length < len)
        {
            aLayerPropArray = new Color[len];
            for (int k = 0; k < len; k++)
            {
                aLayerPropArray[k] = defVal;
            }
            return true;
        }
        return false; // no update
    }

#if UNITY_EDITOR
    public bool PrepareNormals()
    {
        if (Bumps == null) return false;
        Texture2D[] bumps = new Texture2D[(numLayers > 8) ? 12 : ((numLayers > 4) ? 8 : 4)];
        int i;
        for (i = 0; i < bumps.Length; i++) bumps[i] = (i < Bumps.Length) ? Bumps[i] : null;
        for (i = 0; i < bumps.Length; i++)
        {
            if (!bumps[i])
            {
                if ((i & 1) == 0)
                {
                    if (bumps[i + 1])
                    {
                        bumps[i] = new Texture2D(bumps[i + 1].width, bumps[i + 1].width, TextureFormat.ARGB32, false, true);
                        FillTex(bumps[i], new Color32(128, 128, 128, 128));
                    }
                }
                else
                {
                    if (bumps[i - 1])
                    {
                        bumps[i] = new Texture2D(bumps[i - 1].width, bumps[i - 1].width, TextureFormat.ARGB32, false, true);
                        FillTex(bumps[i], new Color32(128, 128, 128, 128));
                    }
                }
            }
            if (bumps[i])
            {
                try
                {
                    bumps[i].GetPixels(0, 0, 4, 4, 0);
                }
                catch (Exception e)
                {
                    Debug.LogError("Normal texture " + i + " has to be marked as isReadable...");
                    Debug.LogError(e.Message);
                    activateObject = bumps[i];
                    return false;
                }
            }
            else
            {
                bumps[i] = new Texture2D(4, 4, TextureFormat.ARGB32, false, true);
                FillTex(bumps[i], new Color32(128, 128, 128, 128));
            }
        }
        if (bumps[0] && bumps[1] && bumps[0].width != bumps[1].width)
        {
            Debug.LogError("Normal textures pair 0,1 should have the same size");
            activateObject = bumps[0].width > bumps[1].width ? bumps[0] : bumps[1];
            //Time.timeScale=0; // pause
            return false;
        }
        if (bumps[2] && bumps[3] && bumps[2].width != bumps[3].width)
        {
            Debug.LogError("Normal textures pair 2,3 should have the same size");
            activateObject = bumps[2].width > bumps[3].width ? bumps[2] : bumps[3];
            //Time.timeScale=0; // pause
            return false;
        }
        Bump01 = CombineNormals(bumps[0], bumps[1]);
        Bump23 = CombineNormals(bumps[2], bumps[3]);
        if (bumps.Length > 4)
        {
            if (bumps[4] && bumps[5] && bumps[4].width != bumps[5].width)
            {
                Debug.LogError("Normal textures pair 4,5 should have the same size");
                activateObject = bumps[4].width > bumps[5].width ? bumps[4] : bumps[5];
                //Time.timeScale=0; // pause
                return false;
            }
            if (bumps[6] && bumps[7] && bumps[6].width != bumps[7].width)
            {
                Debug.LogError("Normal textures pair 6,7 should have the same size");
                activateObject = bumps[6].width > bumps[7].width ? bumps[6] : bumps[7];
                //Time.timeScale=0; // pause
                return false;
            }
            Bump45 = CombineNormals(bumps[4], bumps[5]);
            Bump67 = CombineNormals(bumps[6], bumps[7]);
        }
        if (bumps.Length > 8)
        {
            if (bumps[8] && bumps[9] && bumps[8].width != bumps[9].width)
            {
                Debug.LogError("Normal textures pair 8,9 should have the same size");
                activateObject = bumps[8].width > bumps[9].width ? bumps[8] : bumps[9];
                //Time.timeScale=0; // pause
                return false;
            }
            if (bumps[10] && bumps[11] && bumps[10].width != bumps[11].width)
            {
                Debug.LogError("Normal textures pair 10,11 should have the same size");
                activateObject = bumps[10].width > bumps[11].width ? bumps[10] : bumps[11];
                //Time.timeScale=0; // pause
                return false;
            }
            Bump89 = CombineNormals(bumps[8], bumps[9]);
            BumpAB = CombineNormals(bumps[10], bumps[11]);
        }
        return true;
    }

    private void FillTex(Texture2D tex, Color32 col)
    {
        Color32[] cols = tex.GetPixels32();
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].r = col.r;
            cols[i].g = col.g;
            cols[i].b = col.b;
            cols[i].a = col.a;
        }
        tex.SetPixels32(cols);
    }

    private Texture2D CombineNormals(Texture2D texA, Texture2D texB)
    {
        if (!texA) return null;
        Color32[] colsA = texA.GetPixels32();
        Color32[] colsB = texB.GetPixels32();
        Color32[] cols = new Color32[colsA.Length];
        for (int i = 0; i < cols.Length; i++)
        {
#if UNITY_WEBGL || UNITY_IPHONE || UNITY_ANDROID
			cols[i].r=colsA[i].r;
			cols[i].g=colsA[i].g;
			cols[i].b=colsB[i].r;
			cols[i].a=colsB[i].g;
#else
            cols[i].r = colsA[i].a;
            cols[i].g = colsA[i].g;
            cols[i].b = colsB[i].a;
            cols[i].a = colsB[i].g;
#endif
        }
        Texture2D tex = new Texture2D(texA.width, texA.width, TextureFormat.ARGB32, true, true);
        tex.SetPixels32(cols);
        tex.Apply(true, false);
        //tex.Compress(true); // may try, but quality will be bad...
        //tex.Apply(false,true); // not readable przy publishingu
        tex.filterMode = FilterMode.Trilinear;
        tex.anisoLevel = 0;
        return tex;
    }

    public void CopyWaterParams(int src, int tgt)
    {
        TERRAIN_LayerWetStrength[tgt] = TERRAIN_LayerWetStrength[src];
        TERRAIN_WaterLevel[tgt] = TERRAIN_WaterLevel[src];
        TERRAIN_WaterLevelSlopeDamp[tgt] = TERRAIN_WaterLevelSlopeDamp[src];
        TERRAIN_WaterEdge[tgt] = TERRAIN_WaterEdge[src];
        //TERRAIN_WaterSpecularity[tgt]=TERRAIN_WaterSpecularity[src];
        TERRAIN_WaterGloss[tgt] = TERRAIN_WaterGloss[src];
        TERRAIN_WaterGlossDamper[tgt] = TERRAIN_WaterGlossDamper[src];
        TERRAIN_Refraction[tgt] = TERRAIN_Refraction[src];
        TERRAIN_WetRefraction[tgt] = TERRAIN_WetRefraction[src];
        TERRAIN_Flow[tgt] = TERRAIN_Flow[src];
        TERRAIN_WetFlow[tgt] = TERRAIN_WetFlow[src];
        //TERRAIN_WetSpecularity[tgt]=TERRAIN_WetSpecularity[src];
        TERRAIN_WaterMetallic[tgt] = TERRAIN_WaterMetallic[src];
        TERRAIN_WetGloss[tgt] = TERRAIN_WetGloss[src];
        TERRAIN_WaterColor[tgt] = TERRAIN_WaterColor[src];
        //TERRAIN_WaterIBL_SpecWetStrength[tgt]=TERRAIN_WaterIBL_SpecWetStrength[src];
        //TERRAIN_WaterIBL_SpecWaterStrength[tgt]=TERRAIN_WaterIBL_SpecWaterStrength[src];
        TERRAIN_WaterEmission[tgt] = TERRAIN_WaterEmission[src];
    }

    public void RecalcControlMaps(Terrain terrainComp, ReliefTerrain rt)
    {
        float[,,] splatData = terrainComp.terrainData.GetAlphamaps(0, 0, terrainComp.terrainData.alphamapResolution, terrainComp.terrainData.alphamapResolution);
        Color[] cols_control;
        float[,] norm_array = new float[terrainComp.terrainData.alphamapResolution, terrainComp.terrainData.alphamapResolution];
        if (rt.splat_layer_ordered_mode)
        {
            // ordered mode
            for (int k = 0; k < terrainComp.terrainData.alphamapLayers; k++)
            {
                int n = rt.splat_layer_seq[k];
                // value for current layer
                if (rt.splat_layer_calc[n])
                {
                    int idx = 0;
                    if (rt.source_controls[n])
                    {
                        cols_control = rt.source_controls[n].GetPixels();
                    }
                    else
                    {
                        cols_control = new Color[terrainComp.terrainData.alphamapResolution * terrainComp.terrainData.alphamapResolution];
                        if (rt.source_controls_invert[n])
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.black;
                        }
                        else
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.white;
                        }
                    }
                    int channel_idx = (int)rt.source_controls_channels[n];
                    // apply mask
                    if (rt.splat_layer_masked[n] && rt.source_controls_mask[n])
                    {
                        Color[] cols_mask = rt.source_controls_mask[n].GetPixels();
                        idx = 0;
                        int channel_idx_mask = (int)rt.source_controls_mask_channels[n];
                        for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                        {
                            for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                            {
                                cols_control[idx][channel_idx] *= cols_mask[idx][channel_idx_mask];
                                idx++;
                            }
                        }
                        idx = 0;
                    }
                    for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                    {
                        for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                        {
                            norm_array[i, j] = cols_control[idx++][channel_idx] * rt.splat_layer_boost[n];
                            if (norm_array[i, j] > 1) norm_array[i, j] = 1;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                    {
                        for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                        {
                            norm_array[i, j] = splatData[i, j, n];
                            if (norm_array[i, j] > 1) norm_array[i, j] = 1;
                        }
                    }
                }
                // damp underlying layers
                for (int l = 0; l < k; l++)
                {
                    int m = rt.splat_layer_seq[l];
                    for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                    {
                        for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                        {
                            splatData[i, j, m] *= (1 - norm_array[i, j]);
                        }
                    }
                }
                // write current layer
                if (rt.splat_layer_calc[n])
                {
                    int idx = 0;
                    if (rt.source_controls[n])
                    {
                        cols_control = rt.source_controls[n].GetPixels();
                    }
                    else
                    {
                        cols_control = new Color[terrainComp.terrainData.alphamapResolution * terrainComp.terrainData.alphamapResolution];
                        if (rt.source_controls_invert[n])
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.black;
                        }
                        else
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.white;
                        }
                    }
                    int channel_idx = (int)rt.source_controls_channels[n];
                    // apply mask
                    if (rt.splat_layer_masked[n] && rt.source_controls_mask[n])
                    {
                        Color[] cols_mask = rt.source_controls_mask[n].GetPixels();
                        idx = 0;
                        int channel_idx_mask = (int)rt.source_controls_mask_channels[n];
                        for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                        {
                            for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                            {
                                cols_control[idx][channel_idx] *= cols_mask[idx][channel_idx_mask];
                                idx++;
                            }
                        }
                        idx = 0;
                    }
                    for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                    {
                        for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                        {
                            splatData[i, j, n] = cols_control[idx++][channel_idx] * rt.splat_layer_boost[n];
                            if (splatData[i, j, n] > 1) splatData[i, j, n] = 1;
                        }
                    }
                }
            }
        }
        else
        {
            // unordered mode
            for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
            {
                for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                {
                    norm_array[i, j] = 0;
                }
            }
            for (int n = 0; n < terrainComp.terrainData.alphamapLayers; n++)
            {
                if (rt.splat_layer_calc[n])
                {
                    int idx = 0;
                    if (rt.source_controls[n])
                    {
                        cols_control = rt.source_controls[n].GetPixels();
                    }
                    else
                    {
                        cols_control = new Color[terrainComp.terrainData.alphamapResolution * terrainComp.terrainData.alphamapResolution];
                        if (rt.source_controls_invert[n])
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.black;
                        }
                        else
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.white;
                        }
                    }
                    int channel_idx = (int)rt.source_controls_channels[n];
                    // apply mask
                    if (rt.splat_layer_masked[n] && rt.source_controls_mask[n])
                    {
                        Color[] cols_mask = rt.source_controls_mask[n].GetPixels();
                        idx = 0;
                        int channel_idx_mask = (int)rt.source_controls_mask_channels[n];
                        for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                        {
                            for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                            {
                                cols_control[idx][channel_idx] *= cols_mask[idx][channel_idx_mask];
                                idx++;
                            }
                        }
                        idx = 0;
                    }
                    for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                    {
                        for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                        {
                            norm_array[i, j] += cols_control[idx++][channel_idx] * rt.splat_layer_boost[n];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                    {
                        for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                        {
                            norm_array[i, j] += splatData[i, j, n];
                        }
                    }
                }
            }
            for (int n = 0; n < terrainComp.terrainData.alphamapLayers; n++)
            {
                if (rt.splat_layer_calc[n])
                {
                    int idx = 0;
                    if (rt.source_controls[n])
                    {
                        cols_control = rt.source_controls[n].GetPixels();
                    }
                    else
                    {
                        cols_control = new Color[terrainComp.terrainData.alphamapResolution * terrainComp.terrainData.alphamapResolution];
                        if (rt.source_controls_invert[n])
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.black;
                        }
                        else
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.white;
                        }
                    }
                    int channel_idx = (int)rt.source_controls_channels[n];
                    // apply mask
                    if (rt.splat_layer_masked[n] && rt.source_controls_mask[n])
                    {
                        Color[] cols_mask = rt.source_controls_mask[n].GetPixels();
                        idx = 0;
                        int channel_idx_mask = (int)rt.source_controls_mask_channels[n];
                        for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                        {
                            for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                            {
                                cols_control[idx][channel_idx] *= cols_mask[idx][channel_idx_mask];
                                idx++;
                            }
                        }
                        idx = 0;
                    }
                    for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                    {
                        for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                        {
                            splatData[i, j, n] = cols_control[idx++][channel_idx] * rt.splat_layer_boost[n] / norm_array[i, j];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
                    {
                        for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                        {
                            splatData[i, j, n] = splatData[i, j, n] / norm_array[i, j];
                        }
                    }
                }
            }
        }
        terrainComp.terrainData.SetAlphamaps(0, 0, splatData);

    }

    public void RecalcControlMapsForMesh(ReliefTerrain rt)
    {

        float[,] splatData;
        Color[] cols;
        if (numLayers > 4 && rt.controlA != null && rt.controlB != null)
        {
            if (rt.controlA.width != rt.controlB.width)
            {
                Debug.LogError("Control maps A&B have to be of the same size for recalculation !");
                return;
            }
            else
            {
                bool exit = false;
                for (int k = 0; k < rt.source_controls.Length; k++)
                {
                    if (rt.splat_layer_calc[k] && rt.source_controls[k] != null && rt.source_controls[k].width != rt.controlA.width)
                    {
                        Debug.LogError("Source control map " + k + " should be of the control texture size (" + rt.controlA.width + ") !");
                        exit = true;
                    }
                }
                for (int k = 0; k < rt.source_controls_mask.Length; k++)
                {
                    if (rt.splat_layer_masked[k] && rt.source_controls_mask[k] != null && rt.source_controls_mask[k].width != rt.controlA.width)
                    {
                        Debug.LogError("Source mask control map " + k + " should be of the control texture size (" + rt.controlA.width + ") !");
                        exit = true;
                    }
                }
                if (exit) return;
            }
        }
        if (rt.controlA == null)
        {
            rt.controlA = new Texture2D(1024, 1024, TextureFormat.ARGB32, true);
            cols = new Color[1024 * 1024];
            for (int i = 0; i < cols.Length; i++) cols[i] = new Color(1, 0, 0, 0);
            rt.controlA.Apply(false, false);
        }
        else
        {
            cols = rt.controlA.GetPixels(0);
        }
        splatData = new float[rt.controlA.width * rt.controlA.width, numLayers];
        for (int n = 0; n < numLayers; n++)
        {
            if (n == 4)
            {
                if (rt.controlB == null)
                {
                    rt.controlB = new Texture2D(rt.controlA.width, rt.controlA.width, TextureFormat.ARGB32, true);
                    cols = new Color[1024 * 1024];
                    for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);
                    rt.controlB.Apply(false, false);
                }
                else
                {
                    cols = rt.controlB.GetPixels(0);
                }
            }
            if (n == 8)
            {
                if (rt.controlC == null)
                {
                    rt.controlC = new Texture2D(rt.controlA.width, rt.controlA.width, TextureFormat.ARGB32, true);
                    cols = new Color[1024 * 1024];
                    for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);
                    rt.controlC.Apply(false, false);
                }
                else
                {
                    cols = rt.controlC.GetPixels(0);
                }
            }
            for (int i = 0; i < cols.Length; i++)
            {
                splatData[i, n] = cols[i][n % 4];
            }
        }

        Color[] cols_control;
        float[] norm_array = new float[rt.controlA.width * rt.controlA.width];
        if (rt.splat_layer_ordered_mode)
        {
            // ordered mode
            for (int k = 0; k < numLayers; k++)
            {
                int n = rt.splat_layer_seq[k];
                // value for current layer
                if (rt.splat_layer_calc[n])
                {
                    int idx = 0;
                    if (rt.source_controls[n])
                    {
                        cols_control = rt.source_controls[n].GetPixels();
                    }
                    else
                    {
                        cols_control = new Color[rt.controlA.width * rt.controlA.width];
                        if (rt.source_controls_invert[n])
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.black;
                        }
                        else
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.white;
                        }
                    }
                    int channel_idx = (int)rt.source_controls_channels[n];
                    // apply mask
                    if (rt.splat_layer_masked[n] && rt.source_controls_mask[n])
                    {
                        Color[] cols_mask = rt.source_controls_mask[n].GetPixels();
                        idx = 0;
                        int channel_idx_mask = (int)rt.source_controls_mask_channels[n];
                        for (int i = 0; i < rt.controlA.width; i++)
                        {
                            for (int j = 0; j < rt.controlA.width; j++)
                            {
                                cols_control[idx][channel_idx] *= cols_mask[idx][channel_idx_mask];
                                idx++;
                            }
                        }
                        idx = 0;
                    }
                    for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                    {
                        norm_array[i] = cols_control[idx++][channel_idx] * rt.splat_layer_boost[n];
                        if (norm_array[i] > 1) norm_array[i] = 1;
                    }
                }
                else
                {
                    for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                    {
                        norm_array[i] = splatData[i, n];
                        if (norm_array[i] > 1) norm_array[i] = 1;
                    }
                }
                // damp underlying layers
                for (int l = 0; l < k; l++)
                {
                    int m = rt.splat_layer_seq[l];
                    for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                    {
                        splatData[i, m] *= (1 - norm_array[i]);
                    }
                }
                // write current layer
                if (rt.splat_layer_calc[n])
                {
                    int idx = 0;
                    if (rt.source_controls[n])
                    {
                        cols_control = rt.source_controls[n].GetPixels();
                    }
                    else
                    {
                        cols_control = new Color[rt.controlA.width * rt.controlA.width];
                        if (rt.source_controls_invert[n])
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.black;
                        }
                        else
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.white;
                        }
                    }
                    int channel_idx = (int)rt.source_controls_channels[n];
                    // apply mask
                    if (rt.splat_layer_masked[n] && rt.source_controls_mask[n])
                    {
                        Color[] cols_mask = rt.source_controls_mask[n].GetPixels();
                        idx = 0;
                        int channel_idx_mask = (int)rt.source_controls_mask_channels[n];
                        for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                        {
                            cols_control[idx][channel_idx] *= cols_mask[idx][channel_idx_mask];
                            idx++;
                        }
                        idx = 0;
                    }
                    for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                    {
                        splatData[i, n] = cols_control[idx++][channel_idx] * rt.splat_layer_boost[n];
                    }
                }
            }
        }
        else
        {
            // unordered mode
            for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
            {
                norm_array[i] = 0;
            }
            for (int n = 0; n < numLayers; n++)
            {
                if (rt.splat_layer_calc[n])
                {
                    int idx = 0;
                    if (rt.source_controls[n])
                    {
                        cols_control = rt.source_controls[n].GetPixels();
                    }
                    else
                    {
                        cols_control = new Color[rt.controlA.width * rt.controlA.width];
                        if (rt.source_controls_invert[n])
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.black;
                        }
                        else
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.white;
                        }
                    }
                    int channel_idx = (int)rt.source_controls_channels[n];
                    // apply mask
                    if (rt.splat_layer_masked[n] && rt.source_controls_mask[n])
                    {
                        Color[] cols_mask = rt.source_controls_mask[n].GetPixels();
                        idx = 0;
                        int channel_idx_mask = (int)rt.source_controls_mask_channels[n];
                        for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                        {
                            cols_control[idx][channel_idx] *= cols_mask[idx][channel_idx_mask];
                            idx++;
                        }
                        idx = 0;
                    }
                    for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                    {
                        norm_array[i] += cols_control[idx++][channel_idx] * rt.splat_layer_boost[n];
                    }
                }
                else
                {
                    for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                    {
                        norm_array[i] += splatData[i, n];
                    }
                }
            }
            for (int n = 0; n < numLayers; n++)
            {
                if (rt.splat_layer_calc[n])
                {
                    int idx = 0;
                    if (rt.source_controls[n])
                    {
                        cols_control = rt.source_controls[n].GetPixels();
                    }
                    else
                    {
                        cols_control = new Color[rt.controlA.width * rt.controlA.width];
                        if (rt.source_controls_invert[n])
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.black;
                        }
                        else
                        {
                            for (int i = 0; i < cols_control.Length; i++) cols_control[i] = Color.white;
                        }
                    }
                    int channel_idx = (int)rt.source_controls_channels[n];
                    // apply mask
                    if (rt.splat_layer_masked[n] && rt.source_controls_mask[n])
                    {
                        Color[] cols_mask = rt.source_controls_mask[n].GetPixels();
                        idx = 0;
                        int channel_idx_mask = (int)rt.source_controls_mask_channels[n];
                        for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                        {
                            cols_control[idx][channel_idx] *= cols_mask[idx][channel_idx_mask];
                            idx++;
                        }
                        idx = 0;
                    }
                    for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                    {
                        splatData[i, n] = cols_control[idx++][channel_idx] * rt.splat_layer_boost[n] / norm_array[i];
                    }
                }
                else
                {
                    for (int i = 0; i < rt.controlA.width * rt.controlA.width; i++)
                    {
                        splatData[i, n] = splatData[i, n] / norm_array[i];
                    }
                }
            }
        }

        for (int n = 0; n < numLayers; n++)
        {
            if (n == 0)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    cols[i] = new Color(0, 0, 0, 0);
                }
            }
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i][n % 4] = splatData[i, n];
            }
            if (n == 3)
            {
                rt.controlA.SetPixels(cols, 0);
                rt.controlA.Apply(true, false);
            }
            else if (n == 7)
            {
                rt.controlB.SetPixels(cols, 0);
                rt.controlB.Apply(true, false);
            }
            else if (n == 11)
            {
                rt.controlC.SetPixels(cols, 0);
                rt.controlC.Apply(true, false);
            }
            else if (n == numLayers - 1)
            {
                if (n < 4)
                {
                    rt.controlA.SetPixels(cols, 0);
                    rt.controlA.Apply(true, false);
                }
                else if (n < 8)
                {
                    rt.controlB.SetPixels(cols, 0);
                    rt.controlB.Apply(true, false);
                }
                else
                {
                    rt.controlC.SetPixels(cols, 0);
                    rt.controlC.Apply(true, false);
                }
            }
        }

    }
    public void InvertChannel(Color[] cols, int channel_idx = -1)
    {
        if (channel_idx < 0)
        {
            for (int idx = 0; idx < cols.Length; idx++)
            {
                cols[idx].r = 1 - cols[idx].r;
                cols[idx].g = 1 - cols[idx].g;
                cols[idx].b = 1 - cols[idx].b;
                cols[idx].a = 1 - cols[idx].a;
            }
        }
        else
        {
            for (int idx = 0; idx < cols.Length; idx++)
            {
                cols[idx][channel_idx] = 1 - cols[idx][channel_idx];
            }
        }
    }
    public void InvertChannel(Color32[] cols, int channel_idx = -1)
    {
        if (channel_idx < 0)
        {
            for (int idx = 0; idx < cols.Length; idx++)
            {
                cols[idx].r = (byte)(255 - cols[idx].r);
                cols[idx].g = (byte)(255 - cols[idx].g);
                cols[idx].b = (byte)(255 - cols[idx].b);
                cols[idx].a = (byte)(255 - cols[idx].a);
            }
        }
        else
        {
            if (channel_idx == 0)
            {
                for (int idx = 0; idx < cols.Length; idx++)
                {
                    cols[idx].r = (byte)(255 - cols[idx].r);
                }
            }
            else if (channel_idx == 1)
            {
                for (int idx = 0; idx < cols.Length; idx++)
                {
                    cols[idx].g = (byte)(255 - cols[idx].g);
                }
            }
            else if (channel_idx == 2)
            {
                for (int idx = 0; idx < cols.Length; idx++)
                {
                    cols[idx].b = (byte)(255 - cols[idx].b);
                }
            }
            else
            {
                for (int idx = 0; idx < cols.Length; idx++)
                {
                    cols[idx].a = (byte)(255 - cols[idx].a);
                }
            }

        }
    }
#endif

}
