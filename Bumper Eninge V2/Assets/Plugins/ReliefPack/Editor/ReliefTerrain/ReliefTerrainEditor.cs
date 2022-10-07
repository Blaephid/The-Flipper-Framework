using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using UnityEditor.Callbacks;
#if UNITY_EDITOR	
using System.IO;
#endif

[CustomEditor(typeof(ReliefTerrain))]
public class ReliefTerrainEditor : Editor
{
#if UNITY_EDITOR
    int progress_count_max;
    int progress_count_current;
    const int progress_granulation = 1;
    string progress_description = "";

    private bool dirtyFlag;
    private double changedTime = 0;
    public static Texture2D blank_white_tex;
    public static Texture2D blank_black_tex;

    private static double lCovTim = 0;
    private static bool control_down_flag = false;
    private static RaycastHit paintHitInfo;
    private static bool paintHitInfo_flag;
    private static Tool prev_tool;

    private Texture icoPaintOn;
    private Texture icoPaintOff;
    private Texture icoMicroArrow;
    private Texture icoLayers;
    private Texture icoCoverage;
    private Texture icoCombinedTextures;
    private Texture icoSettings;

    private Texture icoPOM;
    private Texture icoLayersSmall;
    private Texture icoUVBlend;
    private Texture icoPerlinNormal;
    private Texture icoGlobalcolor;
    private Texture icoSuperdetail;
    private Texture icoWater;
    private Texture icoSnow;
    private Texture icoVerticalTexture;
    //private Texture icoReflection;
    private Texture icoGlitter;

    static ReliefTerrainEditor cur_Editor;

    void OnEnable()
    {
        if (icoPaintOn == null) icoPaintOn = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoPaintOn.png", typeof(Texture)) as Texture;
        if (icoPaintOff == null) icoPaintOff = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoPaintOff.png", typeof(Texture)) as Texture;
        if (icoMicroArrow == null) icoMicroArrow = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoMicroArrow.png", typeof(Texture)) as Texture;
        if (icoLayers == null) icoLayers = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoLayers.png", typeof(Texture)) as Texture;
        if (icoCoverage == null) icoCoverage = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoCoverage.png", typeof(Texture)) as Texture;
        if (icoCombinedTextures == null) icoCombinedTextures = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoCombinedTextures.png", typeof(Texture)) as Texture;
        if (icoSettings == null) icoSettings = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoSettings.png", typeof(Texture)) as Texture;

        if (icoPOM == null) icoPOM = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoPOM.png", typeof(Texture)) as Texture;
        if (icoLayersSmall == null) icoLayersSmall = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoLayersSmall.png", typeof(Texture)) as Texture;
        if (icoUVBlend == null) icoUVBlend = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoUVBlend.png", typeof(Texture)) as Texture;
        if (icoGlobalcolor == null) icoGlobalcolor = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoGlobalcolor.png", typeof(Texture)) as Texture;
        if (icoPerlinNormal == null) icoPerlinNormal = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoPerlinNormal.png", typeof(Texture)) as Texture;
        if (icoSuperdetail == null) icoSuperdetail = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoSuperdetail.png", typeof(Texture)) as Texture;
        if (icoWater == null) icoWater = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoWater.png", typeof(Texture)) as Texture;
        if (icoSnow == null) icoSnow = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoSnow.png", typeof(Texture)) as Texture;
        if (icoVerticalTexture == null) icoVerticalTexture = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoVerticalTexture.png", typeof(Texture)) as Texture;
        //if (icoReflection == null) icoReflection = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoReflection.png", typeof(Texture)) as Texture;
        if (icoGlitter == null) icoGlitter = AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoGlitter.png", typeof(Texture)) as Texture;

        ReliefTerrain _target = (ReliefTerrain)target;
        _target.RefreshTextures(null, true);     // check for weak references (global color and special combined not saved to disk));
        if (_target.globalSettingsHolder.BumpGlobal && _target.BumpGlobalCombined)
        {
            TextureImporter _importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_target.globalSettingsHolder.BumpGlobal));
            if (_importer)
            {
                if (_importer.maxTextureSize != _target.BumpGlobalCombined.width)
                {
                    Debug.LogWarning("Due to rescaling special combined texture superdetail textures, water mask and reflection map have been cleared (you have to reassign them again).");
                    _target.TERRAIN_WetMask = null;
                    _target.globalSettingsHolder.SuperDetailA = null;
                    _target.globalSettingsHolder.SuperDetailB = null;
                    _target.PrepareGlobalNormalsAndSuperDetails();
                }
            }
        }

        if (_target.globalSettingsHolder != null)
        {
            _target.globalSettingsHolder.SyncGlobalPropsAcrossTerrainGroups();
        }

        prev_tool = UnityEditor.Tools.current;
    }

    void OnDisable()
    {
        ReliefTerrain _target = (ReliefTerrain)target;
        if (_target)
        {
            _target.RefreshTextures();
            if (_target.globalSettingsHolder.paint_flag)
            {
                _target.globalSettingsHolder.paint_flag = false;
                RemoveSceneViewCallback();
            };
            if (_target.globalSettingsHolder != null)
            {
                _target.globalSettingsHolder.SyncGlobalPropsAcrossTerrainGroups();
            }
            EditorUtility.SetDirty(target);
        }
        UnityEditor.Tools.current = prev_tool;
    }

    void OnDestroy()
    {
        RemoveSceneViewCallback();
    }

    public override void OnInspectorGUI()
    {
        ReliefTerrain _targetRT = (ReliefTerrain)target;
        ReliefTerrainGlobalSettingsHolder _target = _targetRT.globalSettingsHolder;
        if (_target == null)
        {
            _targetRT.RefreshTextures();
            _target = _targetRT.globalSettingsHolder;
        }
        dirtyFlag = false;

        RTP_LODmanager _RTP_LODmanagerScript = _target.Get_RTP_LODmanagerScript();

        Terrain terrainComp = (Terrain)_targetRT.GetComponent(typeof(Terrain));
        if (terrainComp)
        {
            if (_target.numLayers != terrainComp.terrainData.terrainLayers.Length)
            {
                // a layer removed or added
                _target.ReInit(terrainComp);
                ReliefTerrain[] script_objs = (ReliefTerrain[])GameObject.FindObjectsOfType(typeof(ReliefTerrain));
                for (int s = 0; s < script_objs.Length; s++)
                {
                    script_objs[s].splat_layer_seq = new int[12] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
                }
            }
        }
        if (_target.splats == null || _target.numLayers != _target.splats.Length)
        {
            EditorGUILayout.HelpBox("Problem with number of layers, try to reassign ReliefTerrain component script on this terrain...", MessageType.Error, true);
            return;
        }
        int sw = 512;
        for (int i = 0; i < _target.numLayers; i++)
        {
            if (_target.splats[i])
            {
                sw = _target.splats[i].width;
                break;
            }
        }
        for (int i = 0; i < _target.numLayers; i++)
        {
            if (_target.splats[i] == null)
            {
                Debug.LogError("Splat (detail) texture " + i + " was empty ! Assigning null texture");
                _target.splats[i] = new Texture2D(sw, sw, TextureFormat.ARGB32, true, QualitySettings.activeColorSpace == ColorSpace.Linear);
            }
        }

        //if (_target.CheckAndUpdate(ref _target.RTP_gloss2mask, 0.5f, _target.numLayers)) {
        //	for(int k=0; k<_target.numLayers; k++) {
        //		_target.Spec[k]=1; // zresetuj od razu mnoznik glossa (RTP3.1 - zmienna ma inne znaczenie)
        //	}
        //}
        //_target.CheckAndUpdate(ref _target.RTP_gloss_mult, 1f, _target.numLayers);
        //_target.CheckAndUpdate(ref _target.RTP_gloss_shaping, 0.5f, _target.numLayers);
        //_target.CheckAndUpdate(ref _target.RTP_Fresnel, 0, _target.numLayers);
        //_target.CheckAndUpdate(ref _target.RTP_FresnelAtten, 0, _target.numLayers);
        _target.CheckAndUpdate(ref _target.RTP_DiffFresnel, 0, _target.numLayers);
        //_target.CheckAndUpdate(ref _target.RTP_IBL_bump_smoothness, 0.7f, _target.numLayers);
        //_target.CheckAndUpdate(ref _target.RTP_IBL_DiffuseStrength, 0.5f, _target.numLayers);
        //_target.CheckAndUpdate(ref _target.RTP_IBL_SpecStrength, 0.5f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.RTP_metallic, 0, _target.numLayers);
        _target.CheckAndUpdate(ref _target.RTP_glossMin, 0, _target.numLayers);
        _target.CheckAndUpdate(ref _target.RTP_glossMax, 1, _target.numLayers);

        _target.CheckAndUpdate(ref _target._GlitterStrength, 0, _target.numLayers);

        //_target.CheckAndUpdate(ref _target.TERRAIN_WaterSpecularity, 0.5f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.TERRAIN_WaterGloss, 0.1f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.TERRAIN_WaterGlossDamper, 0f, _target.numLayers);
        //_target.CheckAndUpdate(ref _target.TERRAIN_WetSpecularity, 0.2f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.TERRAIN_WetGloss, 0.05f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.TERRAIN_WetFlow, 0.05f, _target.numLayers);

        _target.CheckAndUpdate(ref _target.MixBrightness, 2.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.MixReplace, 0.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.LayerBrightness, 1.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.LayerBrightness2Spec, 0.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.LayerAlbedo2SpecColor, 0.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.LayerSaturation, 1.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.FarSpecCorrection, 0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.LayerEmission, 0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.LayerEmissionColor, Color.black, _target.numLayers);
        _target.CheckAndUpdate(ref _target.LayerEmissionRefractStrength, 0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.LayerEmissionRefractHBedge, 0f, _target.numLayers);

        _target.CheckAndUpdate(ref _target.GlobalColorBottom, 0.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.GlobalColorTop, 1.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.GlobalColorColormapLoSat, 1.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.GlobalColorColormapHiSat, 1.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.GlobalColorLayerLoSat, 1.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.GlobalColorLayerHiSat, 1.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.GlobalColorLoBlend, 1.0f, _target.numLayers);
        _target.CheckAndUpdate(ref _target.GlobalColorHiBlend, 1.0f, _target.numLayers);

        //_target.CheckAndUpdate(ref _target.TERRAIN_WaterIBL_SpecWetStrength, 0.1f, _target.numLayers);
        //_target.CheckAndUpdate(ref _target.TERRAIN_WaterIBL_SpecWaterStrength, 0.5f, _target.numLayers);

        // update (RTP3.1)
        if (_target.splat_atlases.Length == 2)
        {
            Texture2D _atA = _target.splat_atlases[0];
            Texture2D _atB = _target.splat_atlases[1];
            _target.splat_atlases = new Texture2D[3];
            _target.splat_atlases[0] = _atA;
            _target.splat_atlases[1] = _atB;
        }

        bool atlasA_needed = false;
        bool atlasB_needed = false;
        bool atlasC_needed = false;

        bool _4LAYERS_SHADER_USED = false;
        if (_RTP_LODmanagerScript)
        {
            _target._4LAYERS_SHADER_USED = _4LAYERS_SHADER_USED = _RTP_LODmanagerScript.RTP_4LAYERS_MODE;
        }
        if (_RTP_LODmanagerScript) _RTP_LODmanagerScript.numLayers = _target.numLayers;

        if (_RTP_LODmanagerScript)
        {
            if (_target.numLayers <= 4 && !_RTP_LODmanagerScript.RTP_4LAYERS_MODE)
            {
                EditorGUILayout.HelpBox("Shaders are set to 8 layers mode while number of layers is less than 5. Add layers or recompile shaders in RTP_LODmanager to not use 8 layers in first pass.", MessageType.Error, true);
            }
            atlasA_needed = !_4LAYERS_SHADER_USED || _RTP_LODmanagerScript.RTP_USE_COLOR_ATLAS_FIRST;
            atlasB_needed = (!_4LAYERS_SHADER_USED || _RTP_LODmanagerScript.RTP_USE_COLOR_ATLAS_ADD) && (_target.numLayers > 4);
            atlasC_needed = (!_4LAYERS_SHADER_USED && _RTP_LODmanagerScript.RTP_USE_COLOR_ATLAS_ADD) && (_target.numLayers > 8);

            bool atlas_made = false;
            if (atlasA_needed)
            {
                if (_target.splat_atlases[0] == null)
                {
                    PrepareAtlases(0);
                    atlas_made = true;
                }
            }
            else
            {
                _target.splat_atlases[0] = null;
            }
            if (atlasB_needed)
            {
                if (_target.splat_atlases[1] == null)
                {
                    PrepareAtlases(1);
                    atlas_made = true;
                }
            }
            else
            {
                _target.splat_atlases[1] = null;
            }
            if (atlasC_needed)
            {
                if (_target.splat_atlases[2] == null)
                {
                    PrepareAtlases(2);
                    atlas_made = true;
                }
            }
            else
            {
                _target.splat_atlases[2] = null;
            }
            if (atlas_made)
            {
                _target.Refresh(null, _targetRT);
            }
        }

        bool ADV_COLOR_MAP_BLENDING_ENABLED_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.ADV_COLOR_MAP_BLENDING_FIRST;
        bool ADV_COLOR_MAP_BLENDING_ENABLED_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.ADV_COLOR_MAP_BLENDING_ADD;

        bool UV_BLEND_ENABLED_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_UV_BLEND_FIRST;
        bool UV_BLEND_ENABLED_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_UV_BLEND_ADD;
        int[] UV_BLEND_ROUTE_NUM_FIRST;
        int[] UV_BLEND_ROUTE_NUM_ADD;
        if (_RTP_LODmanagerScript)
        {
            UV_BLEND_ROUTE_NUM_FIRST = _RTP_LODmanagerScript.UV_BLEND_ROUTE_NUM_FIRST;
            UV_BLEND_ROUTE_NUM_ADD = _RTP_LODmanagerScript.UV_BLEND_ROUTE_NUM_ADD;
        }
        else
        {
            UV_BLEND_ROUTE_NUM_FIRST = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 };
            UV_BLEND_ROUTE_NUM_ADD = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 };
        }

        //bool RTP_INDEPENDENT_TILING = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_INDEPENDENT_TILING;

        bool RTP_CUT_HOLES = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_CUT_HOLES;
        //bool RTP_PBL_FRESNEL = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_PBL_FRESNEL;

        bool RTP_USE_EXTRUDE_REDUCTION_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_USE_EXTRUDE_REDUCTION_FIRST;
        bool RTP_USE_EXTRUDE_REDUCTION_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_USE_EXTRUDE_REDUCTION_ADD;

        bool RTP_VERTICAL_TEXTURE_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_VERTICAL_TEXTURE_FIRST;
        bool RTP_VERTICAL_TEXTURE_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_VERTICAL_TEXTURE_ADD;

        bool RTP_NORMALGLOBAL = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_NORMALGLOBAL;
        bool RTP_TREESGLOBAL = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_TREESGLOBAL;
        //bool RTP_AMBIENT_EMISSIVE_MAP = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_AMBIENT_EMISSIVE_MAP;

        bool RTP_TESSELLATION = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_TESSELLATION;
        bool RTP_TESSELLATION_SAMPLE_TEXTURE = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_TESSELLATION_SAMPLE_TEXTURE;
        bool RTP_HEIGHTMAP_SAMPLE_BICUBIC = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_HEIGHTMAP_SAMPLE_BICUBIC;

        //bool RTP_SUPER_SIMPLE = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_SUPER_SIMPLE;
        //bool RTP_SS_GRAYSCALE_DETAIL_COLORS_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_SUPER_SIMPLE  && _RTP_LODmanagerScript.RTP_SS_GRAYSCALE_DETAIL_COLORS_FIRST;
        //bool RTP_SS_GRAYSCALE_DETAIL_COLORS_ADD = _target.numLayers>4 && _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_SUPER_SIMPLE  && _RTP_LODmanagerScript.RTP_SS_GRAYSCALE_DETAIL_COLORS_ADD;
        //bool RTP_USE_PERLIN_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_USE_PERLIN_FIRST;
        //bool RTP_USE_PERLIN_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_USE_PERLIN_ADD;

        bool SUPER_DETAIL_ENABLED_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_SUPER_DETAIL_FIRST;
        bool SUPER_DETAIL_ENABLED_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_SUPER_DETAIL_ADD;
        bool SUPER_DETAIL_MULTS_ENABLED_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_SUPER_DETAIL_MULTS_FIRST;
        bool SUPER_DETAIL_MULTS_ENABLED_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_SUPER_DETAIL_MULTS_ADD;

        bool SNOW_ENABLED_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_SNOW_FIRST;
        bool SNOW_ENABLED_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_SNOW_ADD;

        bool GLITTER_ENABLED_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_GLITTER_FIRST;
        bool GLITTER_ENABLED_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_GLITTER_ADD;

        bool RTP_HEIGHTBLEND_AO_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_HEIGHTBLEND_AO_FIRST;
        bool RTP_HEIGHTBLEND_AO_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_HEIGHTBLEND_AO_ADD;

        bool RTP_EMISSION_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_EMISSION_FIRST;
        bool RTP_EMISSION_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_EMISSION_ADD;

        bool RTP_HOTAIR_EMISSION_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_HOTAIR_EMISSION_FIRST;
        bool RTP_HOTAIR_EMISSION_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_HOTAIR_EMISSION_ADD;


        bool WATER_ENABLED_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_WETNESS_FIRST;
        bool WATER_ENABLED_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_WETNESS_ADD;
        bool RIPPLEMAP_ENABLED_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_WET_RIPPLE_TEXTURE_FIRST;
        bool RIPPLEMAP_ENABLED_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_WET_RIPPLE_TEXTURE_ADD;

        bool CAUSTICS_ENABLED_FIRST = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_CAUSTICS_FIRST;
        bool CAUSTICS_ENABLED_ADD = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_CAUSTICS_ADD;

        bool RTP_VERTALPHA_CAUSTICS = _RTP_LODmanagerScript && _RTP_LODmanagerScript.RTP_VERTALPHA_CAUSTICS && (RTP_VERTICAL_TEXTURE_FIRST || RTP_VERTICAL_TEXTURE_ADD);

        string[] toolbarStrings;
        GUIContent[] toolbarIcons = new GUIContent[4] { new GUIContent("Layers", icoLayers), new GUIContent("Coverage", icoCoverage), new GUIContent("Combined\ntextures", icoCombinedTextures), new GUIContent("Settings", icoSettings) };
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        _target.submenu = (ReliefTerrainMenuItems)(GUILayout.Toolbar((int)_target.submenu, toolbarIcons,
#if !UNITY_2019_3_OR_NEWER
            EditorStyles.miniButton,
#endif
            GUILayout.MaxWidth(370)));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        _target.show_details = _target.submenu == ReliefTerrainMenuItems.Details;
        _target.show_controlmaps = _target.submenu == ReliefTerrainMenuItems.Control;
        _target.show_derivedmaps = _target.submenu == ReliefTerrainMenuItems.CombinedTextures;
        _target.show_settings = _target.submenu == ReliefTerrainMenuItems.GeneralSettings;

        if (_target.show_active_layer >= _target.numLayers)
        {
            _target.show_active_layer = _target.numLayers - 1;
        }
        //Debug.Log (_target.show_active_layer+","+_target.numLayers);

        if (_target.show_details)
        {
        #region Detail maps unfold

            if (_target.numLayers > 0)
            {
        #region Detail maps unfold	- layers

                if (!terrainComp)
                {
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(_target.numLayers == 8);
                    if (GUILayout.Button("Add layer"))
                    {
                        _target.numLayers++;
                        int j = _target.numLayers - 1;

                        ReliefTerrain[] script_objs = (ReliefTerrain[])GameObject.FindObjectsOfType(typeof(ReliefTerrain));
                        for (int s = 0; s < script_objs.Length; s++)
                        {
                            script_objs[s].splat_layer_seq[j] = j;
                            script_objs[s].splat_layer_boost[j] = 1;
                            script_objs[s].splat_layer_calc[j] = true;
                            script_objs[s].splat_layer_masked[j] = false;
                            script_objs[s].source_controls_invert[j] = false;
                            script_objs[s].source_controls_mask_invert[j] = false;
                        }

                        _target.Bumps[j] = null;
                        _target.Heights[j] = null;
#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
                        _target.Substances[j] = null;
#endif
                        _target.ReturnToDefaults("layer", j);

                        Texture2D[] splats_new = new Texture2D[_target.numLayers];
                        for (int i = 0; i < _target.splats.Length; i++) splats_new[i] = _target.splats[i];
                        _target.splats = splats_new;
                        _target.splats[_target.numLayers - 1] = _target.splats[((_target.numLayers - 2) >= 0) ? (_target.numLayers - 2) : 0];
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(_target.numLayers == 1);
                    if (GUILayout.Button("Remove layer"))
                    {
                        _target.numLayers--;
                        Texture2D[] splats_new = new Texture2D[_target.numLayers];
                        for (int i = 0; i < splats_new.Length; i++) splats_new[i] = _target.splats[i];
                        _target.splats = splats_new;
                        ReliefTerrain[] script_objs = (ReliefTerrain[])GameObject.FindObjectsOfType(typeof(ReliefTerrain));
                        for (int s = 0; s < script_objs.Length; s++)
                        {
                            script_objs[s].splat_layer_seq = new int[12] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
                        }
                        if (_target.show_active_layer >= _target.numLayers)
                        {
                            _target.show_active_layer = _target.numLayers - 1;
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
                bool handle_substances = false;
                handle_substances = true;
#endif

                EditorGUILayout.BeginVertical("Box");
                Color skin_color = GUI.color;
                GUI.color = new Color(1, 1, 0.5f, 1);
                EditorGUILayout.LabelField("Choose layer", EditorStyles.boldLabel);
                GUI.color = skin_color;
                EditorGUILayout.HelpBox("Hint: to quickly select a layer - focus on scene view (by click) and press L holding cursor over the layer that should be selected.", MessageType.Info, true);

                GUISkin gs = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
                RectOffset ro1 = gs.label.padding;
                RectOffset ro2 = gs.label.margin;
                gs.label.padding = new RectOffset(0, 0, 0, 0);
                gs.label.margin = new RectOffset(3, 3, 3, 3);
                int thumb_size = 50;
                int per_row = Mathf.Max(4, (Screen.width) / thumb_size - 1);
                thumb_size = (Screen.width - 50 - 2 * per_row) / per_row;
                Color ccol = GUI.contentColor;
                for (int n = 0; n < _target.numLayers; n++)
                {
                    if ((n % per_row) == 0) EditorGUILayout.BeginHorizontal();
                    Color bcol = GUI.backgroundColor;
                    if (n == _target.show_active_layer)
                    {
                        GUI.contentColor = new Color(1, 1, 1, 1);
                        GUI.backgroundColor = new Color(1, 1, 0, 1);
                        EditorGUILayout.BeginHorizontal("Box");
                        if (_target.splats[n])
                        {
                            GUILayout.Label((Texture2D)AssetPreview.GetAssetPreview(_target.splats[n]), GUILayout.Width(thumb_size - 8), GUILayout.Height(thumb_size - 8));
                        }
                        else
                        {
                            GUILayout.Label(" ", GUILayout.Width(thumb_size - 8), GUILayout.Height(thumb_size - 8));
                        }
                    }
                    else
                    {
                        GUI.contentColor = new Color(1, 1, 1, 0.5f);
                        if (_target.splats[n])
                        {
                            if (GUILayout.Button((Texture2D)AssetPreview.GetAssetPreview(_target.splats[n]), "Label", GUILayout.Width(thumb_size), GUILayout.Height(thumb_size)))
                            {
                                GUI.FocusControl("");
                                _target.show_active_layer = n;
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(" ", "Label", GUILayout.Width(thumb_size), GUILayout.Height(thumb_size)))
                            {
                                GUI.FocusControl("");
                                _target.show_active_layer = n;
                            }
                        }
                    }
                    if (n == _target.show_active_layer)
                    {
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = bcol;
                    }
                    if ((n % per_row) == (per_row - 1) || n == _target.numLayers - 1) EditorGUILayout.EndHorizontal();
                }
                GUI.contentColor = ccol;
                gs.label.padding = ro1;
                gs.label.margin = ro2;
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("Box");
                skin_color = GUI.color;
                GUI.color = new Color(1, 1, 0.5f, 1);
                EditorGUILayout.LabelField("Layer properties", EditorStyles.boldLabel);
                GUI.color = skin_color;
                EditorGUILayout.HelpBox("All layers should share THE SAME size for given texture type\n(i.e. all detail textures should have the same size and so on). ", MessageType.Info, true);
                EditorGUILayout.Space();
                {
                    int thW = (Screen.width - 60) / 3;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Detail", EditorStyles.boldLabel, GUILayout.Width(thW));
                    EditorGUILayout.LabelField("Normal", EditorStyles.boldLabel, GUILayout.Width(thW));
                    EditorGUILayout.LabelField("Height (A)", EditorStyles.boldLabel, GUILayout.Width(thW));
                    EditorGUILayout.EndHorizontal();
                }

                {
                    int n = _target.show_active_layer;
                    EditorGUILayout.BeginHorizontal();

                    int thW = (Screen.width - 60) / 3;
                    //Debug.Log (_target.splats.Length+"  "+_target.numLayers);
                    Texture2D tex = _target.splats[n];
                    Texture2D ntex = tex;
                    checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(_target.splats[n], typeof(Texture2D), false, GUILayout.Width(thW), GUILayout.Height(thW)));
                    if (ntex != tex && ntex)
                    {
                        _target.splats[n] = ntex;
                        int prep_atlas = -1;
                        if (n < 4)
                        {
                            if (atlasA_needed)
                            {
                                prep_atlas = 0;
                            }
                        }
                        else if (n < 8)
                        {
                            if (atlasB_needed)
                            {
                                prep_atlas = 1;
                            }
                        }
                        else if (n < 12)
                        {
                            if (atlasC_needed)
                            {
                                prep_atlas = 2;
                            }
                        }
                        if (prep_atlas >= 0)
                        {
                            if (EditorUtility.DisplayDialog("RTP Notification", "You have to recalc atlas to see changes.", "OK, do it now", "Thanks, I'll do it later"))
                            {
                                PrepareAtlases(prep_atlas);
                            }
                        }
                        _target.Refresh(null, _targetRT);
                    }

                    tex = _target.Bumps[n];
                    ntex = tex;
                    checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(_target.Bumps[n], typeof(Texture2D), false, GUILayout.Width(thW), GUILayout.Height(thW)));
                    if (ntex != tex)
                    {
                        Texture2D tex_prev = _target.Bumps[n];
                        _target.Bumps[n] = ntex;
                        AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ntex));
                        if (_importer)
                        {
                            TextureImporter tex_importer = (TextureImporter)_importer;
                            bool changed = false;
                            if (!tex_importer.isReadable)
                            {
                                Debug.LogWarning("Normal texture " + n + " (" + ntex.name + ") has been reimported as readable.");
                                tex_importer.isReadable = true;
                                changed = true;
                            }
                            if (tex_importer.textureType != TextureImporterType.NormalMap)
                            {
                                Debug.LogWarning("Normal texture " + n + " (" + ntex.name + ") has been reimported as normal map type.");
                                tex_importer.textureType = TextureImporterType.NormalMap;
                                changed = true;
                            }
                            if (changed)
                            {
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(ntex), ImportAssetOptions.ForceUpdate);
                            }
                        }
                        if (!_target.PrepareNormals()) _target.Bumps[n] = tex_prev;
                        _target.Refresh(null, _targetRT);
                    }

                    tex = _target.Heights[n];
                    ntex = tex;
                    checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(_target.Heights[n], typeof(Texture2D), false, GUILayout.Width(thW), GUILayout.Height(thW)));
                    if (ntex != tex)
                    {
                        Texture2D tex_prev = _target.Heights[n];
                        _target.Heights[n] = ntex;
                        AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ntex));
                        if (_importer)
                        {
                            TextureImporter tex_importer = (TextureImporter)_importer;
                            bool changed = false;
                            if (!tex_importer.isReadable)
                            {
                                Debug.LogWarning("Height texture " + n + " (" + ntex.name + ") has been reimported as readable.");
                                tex_importer.isReadable = true;
                                changed = true;
                            }
                            if (tex_importer.sRGBTexture)
                            {
                                Debug.LogWarning("Height texture " + n + " (" + ntex.name + ") has been reimported as linear.");
                                tex_importer.sRGBTexture = false;
                                changed = true;
                            }
                            if (changed)
                            {
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(ntex), ImportAssetOptions.ForceUpdate);
                            }
                        }
                        if (!PrepareHeights(n)) _target.Heights[n] = tex_prev;
                        _target.Refresh(null, _targetRT);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    int thW2 = (Screen.width - 60) / 3;
                    // refresh detail (atlas)
                    {
                        int prep_atlas = -1;
                        if (n < 4 && atlasA_needed)
                        {
                            prep_atlas = 0;
                        }
                        else if (n >= 4 && n < 8 && atlasB_needed)
                        {
                            prep_atlas = 1;
                        }
                        else if (n >= 8 && n < 12 && atlasC_needed)
                        {
                            prep_atlas = 2;
                        }
                        EditorGUI.BeginDisabledGroup(prep_atlas == -1);
                        if (GUILayout.Button("Refresh", GUILayout.Width(thW2)))
                        {
                            PrepareAtlases(prep_atlas);
                            _target.Refresh(null, _targetRT);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    // refresh normal combined
                    if (GUILayout.Button("Refresh", GUILayout.Width(thW2)))
                    {
                        _target.PrepareNormals();
                        _target.Refresh(null, _targetRT);
                    }
                    // refresh height combined
                    if (GUILayout.Button("Refresh", GUILayout.Width(thW2)))
                    {
                        PrepareHeights(n);
                        _target.Refresh(null, _targetRT);
                    }

                    EditorGUILayout.EndHorizontal();
#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
                    if (handle_substances)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Substance", EditorStyles.boldLabel, GUILayout.MaxWidth(75));

                        ProceduralMaterial subs = _target.Substances[n];
                        ProceduralMaterial nsubs = subs;
                        checkChange(ref nsubs, (ProceduralMaterial)EditorGUILayout.ObjectField(_target.Substances[n], typeof(ProceduralMaterial), false));
                        if (nsubs != subs)
                        {
                            _target.Substances[n] = nsubs;
                            if (nsubs)
                            {
                                _target.Substances[n].isReadable = true;
                                _target.Substances[n].RebuildTexturesImmediately();
                                SubstanceImporter s_importer = (SubstanceImporter)SubstanceImporter.GetAtPath(AssetDatabase.GetAssetPath(_target.Substances[n]));
                                if (_target.Substances[n].shader.name != "Parallax Specular")
                                {
                                    _target.Substances[n].shader = Shader.Find("Parallax Specular");
                                    s_importer.OnShaderModified(_target.Substances[n]);
                                }
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_target.Substances[n]), ImportAssetOptions.ForceUpdate);
                                //GetSubstanceTex("_Diffuse","_MainTex", n);		
                                //GetSubstanceTex("_Normal","_BumpMap", n, true);	
                                //GetSubstanceTex("_Height","_ParallaxMap", n);

                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.HelpBox("Before you proceed with Get textures button below, make sure your substance material format is set to RAW. If substance resists to give proper output try getting texture again.", MessageType.Warning, true);
                        EditorGUI.BeginDisabledGroup(_target.Substances[n] == null || _target.Substances[n].isProcessing);
                        if (GUILayout.Button("Get textures"))
                        {

                            _target.Substances[n].isReadable = true;
                            SubstanceImporter s_importer = (SubstanceImporter)SubstanceImporter.GetAtPath(AssetDatabase.GetAssetPath(_target.Substances[n]));
                            if (_target.Substances[n].shader.name != "Parallax Specular")
                            {
                                _target.Substances[n].shader = Shader.Find("Parallax Specular");
                                s_importer.OnShaderModified(_target.Substances[n]);
                            }
                            _target.Substances[n].RebuildTexturesImmediately();

                            AssetImporter _importer;
                            string orig_path;
                            string path = "";
                            bool override_flag;
                            byte[] bytes;
                            int option;

                            _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_target.splats[n]));
                            if (_importer)
                            {
                                orig_path = AssetDatabase.GetAssetPath(_target.splats[n]);
                                orig_path = orig_path.Substring(0, orig_path.LastIndexOf("/") + 1);
                            }
                            else
                            {
                                orig_path = System.IO.Directory.GetParent(AssetDatabase.GetAssetPath(_target.Substances[n])) + "/";
                            }

                            //
                            // diffuse								
                            //
                            ntex = GetSubstanceTex("_Diffuse", "_MainTex", n);

                            path = orig_path + _target.Substances[n].name + "_diffuse_spec" + ".png";
                            override_flag = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) != null;
                            if (override_flag)
                            {
                                option = EditorUtility.DisplayDialogComplex("RTP Notification", "Override color/diffuse texture ?", "OK", "Make copy", "Skip texture");
                            }
                            else
                            {
                                option = 0;
                            }
                            if (option == 1)
                            {
                                do
                                {
                                    int suffix_pos = path.LastIndexOf(".");
                                    path = path.Substring(0, suffix_pos) + "_copy" + path.Substring(suffix_pos);
                                } while (AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) != null);
                            }
                            if (option < 2)
                            {
                                bytes = ntex.EncodeToPNG();
                                DestroyImmediate(ntex);
                                ntex = null;
                                System.IO.File.WriteAllBytes(path, bytes);
                                AssetDatabase.Refresh();
                                _importer = AssetImporter.GetAtPath(path);
                                if (_importer)
                                {
                                    TextureImporter tex_importer = (TextureImporter)_importer;
                                    tex_importer.isReadable = true;
                                    tex_importer.maxTextureSize = _target.splats[n].width;
                                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                                    ntex = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                                    _target.splats[n] = ntex;
                                    int prep_atlas = -1;
                                    if (n < 4)
                                    {
                                        if (atlasA_needed)
                                        {
                                            prep_atlas = 0;
                                        }
                                    }
                                    else if (n < 8)
                                    {
                                        if (atlasB_needed)
                                        {
                                            prep_atlas = 1;
                                        }
                                    }
                                    else if (n < 12)
                                    {
                                        if (atlasC_needed)
                                        {
                                            prep_atlas = 2;
                                        }
                                    }
                                    if (prep_atlas >= 0)
                                    {
                                        if (EditorUtility.DisplayDialog("RTP Notification", "You have to recalc atlas to see changes.", "OK, do it now", "Thanks, I'll do it later"))
                                        {
                                            PrepareAtlases(prep_atlas);
                                        }
                                    }
                                    _target.Refresh(null, _targetRT);
                                }
                            }

                            //
                            // normal						
                            //
                            ntex = GetSubstanceTex("_Normal", "_BumpMap", n, true);
                            int nwidth = ntex.width;
                            path = orig_path + _target.Substances[n].name + "_normal" + ".png";
                            override_flag = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) != null;
                            if (override_flag)
                            {
                                option = EditorUtility.DisplayDialogComplex("RTP Notification", "Override normal texture ?", "OK", "Make copy", "Skip texture");
                            }
                            else
                            {
                                option = 0;
                            }
                            if (option == 1)
                            {
                                do
                                {
                                    int suffix_pos = path.LastIndexOf(".");
                                    path = path.Substring(0, suffix_pos) + "_copy" + path.Substring(suffix_pos);
                                } while (AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) != null);
                            }
                            if (option < 2)
                            {
                                bytes = ntex.EncodeToPNG();
                                DestroyImmediate(ntex);
                                ntex = null;
                                System.IO.File.WriteAllBytes(path, bytes);
                                AssetDatabase.Refresh();
                                _importer = AssetImporter.GetAtPath(path);
                                if (_importer)
                                {
                                    Texture2D tex_prev = _target.Bumps[n];
                                    TextureImporter tex_importer = (TextureImporter)_importer;
                                    tex_importer.isReadable = true;
                                    int compare_n;
                                    if ((n & 1) == 0)
                                    {
                                        if ((n + 1) < _target.numLayers)
                                        {
                                            compare_n = n + 1;
                                        }
                                        else
                                        {
                                            compare_n = -1;
                                        }
                                    }
                                    else
                                    {
                                        compare_n = n - 1;
                                    }
                                    if (compare_n >= 0 && _target.Bumps[compare_n] != null)
                                    {
                                        if (nwidth < _target.Bumps[compare_n].width)
                                        {
                                            TextureImporter tex_importer_tmp = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_target.Bumps[compare_n].width));
                                            tex_importer_tmp.maxTextureSize = nwidth;
                                            Debug.LogWarning("Normal texture " + compare_n + " (" + _target.splats[compare_n].name + ") has been reimported with " + tex_importer_tmp.maxTextureSize + " size.");
                                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_target.Bumps[compare_n].width), ImportAssetOptions.ForceUpdate);
                                        }
                                        else if (nwidth > _target.Bumps[compare_n].width)
                                        {
                                            tex_importer.maxTextureSize = _target.Bumps[compare_n].width;
                                            Debug.LogWarning("Normal texture " + n + " (" + _target.splats[n].name + ") has been imported with " + tex_importer.maxTextureSize + " size.");
                                        }
                                    }
                                    tex_importer.textureType = TextureImporterType.NormalMap;
                                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                                    ntex = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                                    _target.Bumps[n] = ntex;
                                    if (!_target.PrepareNormals()) _target.Bumps[n] = tex_prev;
                                    _target.Refresh(null, _targetRT);
                                }
                            }

                            //
                            // height				
                            //
                            ntex = GetSubstanceTex("_Height", "_ParallaxMap", n);

                            path = orig_path + _target.Substances[n].name + "_height" + ".png";
                            override_flag = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) != null;
                            if (override_flag)
                            {
                                option = EditorUtility.DisplayDialogComplex("RTP Notification", "Override height texture ?", "OK", "Make copy", "Skip texture");
                            }
                            else
                            {
                                option = 0;
                            }
                            if (option == 1)
                            {
                                do
                                {
                                    int suffix_pos = path.LastIndexOf(".");
                                    path = path.Substring(0, suffix_pos) + "_copy" + path.Substring(suffix_pos);
                                } while (AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) != null);
                            }
                            if (option < 2)
                            {
                                bytes = ntex.EncodeToPNG();
                                DestroyImmediate(ntex);
                                ntex = null;
                                System.IO.File.WriteAllBytes(path, bytes);
                                AssetDatabase.Refresh();
                                _importer = AssetImporter.GetAtPath(path);
                                if (_importer)
                                {
                                    Texture2D tex_prev = _target.Heights[n];
                                    TextureImporter tex_importer = (TextureImporter)_importer;
                                    tex_importer.isReadable = true;
                                    tex_importer.sRGBTexture = false;
                                    tex_importer.textureCompression = TextureImporterCompression.Uncompressed;
                                    tex_importer.textureType = TextureImporterType.SingleChannel;
                                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                                    ntex = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                                    _target.Heights[n] = ntex;
                                    if (!PrepareHeights(n)) _target.Heights[n] = tex_prev;
                                    _target.Refresh(null, _targetRT);
                                }
                            }

                        }

                        EditorGUI.EndDisabledGroup();
                    } // handle substances
#endif // WebGL substances off

                    GUILayout.Space(10);

                    bool AddPass_flag = (_4LAYERS_SHADER_USED && n >= 4) || (n >= 8);

                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.LabelField("PBL / IBL properties", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("Detail texture A channel defines smoothness.", MessageType.None, true);
                    EditorGUILayout.BeginVertical("Box");
                    float nval;
                    nval = _target.RTP_metallic[n];
                    checkChange(ref nval, EditorGUILayout.Slider("Layer " + n + " metalness", _target.RTP_metallic[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                    _target.RTP_metallic[n] = nval;
                    nval = _target.RTP_glossMin[n];
                    checkChange(ref nval, EditorGUILayout.Slider("   gloss min", _target.RTP_glossMin[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                    _target.RTP_glossMin[n] = nval;
                    nval = _target.RTP_glossMax[n];
                    checkChange(ref nval, EditorGUILayout.Slider("   gloss max", _target.RTP_glossMax[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                    _target.RTP_glossMax[n] = nval;

                    //nval=_target.Spec[n];
                    //checkChange(ref nval, EditorGUILayout.Slider("Layer "+n+" spec multiplier", _target.Spec[n], 0f, 4f, GUILayout.ExpandWidth(true)));
                    //_target.Spec[n]=nval;
                    //EditorGUI.BeginDisabledGroup(_target.gloss_baked[n]!=null && _target.gloss_baked[n].baked);
                    //	nval=_target.RTP_gloss_mult[n];
                    //	checkChange(ref nval, EditorGUILayout.Slider("   gloss multiplier", _target.RTP_gloss_mult[n], 0f, 4f, GUILayout.ExpandWidth(true)));
                    //	_target.RTP_gloss_mult[n]=nval;
                    //EditorGUI.EndDisabledGroup();
                    //nval=_target.RTP_gloss2mask[n];
                    //checkChange(ref nval, EditorGUILayout.Slider("   spec mask from gloss", _target.RTP_gloss2mask[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                    //_target.RTP_gloss2mask[n]=nval;
                    //nval=_target.LayerBrightness2Spec[n];
                    //checkChange(ref nval, EditorGUILayout.Slider("   spec mask from albedo", _target.LayerBrightness2Spec[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                    //_target.LayerBrightness2Spec[n]=nval;
                    //EditorGUI.BeginDisabledGroup(_target.gloss_baked[n]!=null && _target.gloss_baked[n].baked);
                    //	nval=_target.RTP_gloss_shaping[n];
                    //	checkChange(ref nval, EditorGUILayout.Slider("   gloss shaping", _target.RTP_gloss_shaping[n]*2-1, -1f, 1f, GUILayout.ExpandWidth(true))*0.5f+0.5f);
                    //	_target.RTP_gloss_shaping[n]=nval;
                    //EditorGUI.EndDisabledGroup();
                    {//if (!RTP_SUPER_SIMPLE) {
                        nval = _target.FarSpecCorrection[n];
                        checkChange(ref nval, EditorGUILayout.Slider("   far gloss correction", _target.FarSpecCorrection[n], -1f, 1f, GUILayout.ExpandWidth(true)));
                        _target.FarSpecCorrection[n] = nval;
                        //if ( (!AddPass_flag && (REFLECTION_ENABLED_FIRST || RTP_IBL_SPEC_FIRST)) || (AddPass_flag && (REFLECTION_ENABLED_FIRST || RTP_IBL_SPEC_FIRST)) || RTP_PBL_FRESNEL) {
                        //	nval=_target.RTP_Fresnel[n];
                        //	checkChange(ref nval, EditorGUILayout.Slider("   fresnel", _target.RTP_Fresnel[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                        //	_target.RTP_Fresnel[n]=nval;
                        //	EditorGUI.BeginDisabledGroup(_target.RTP_Fresnel[n]==0);
                        //	nval=_target.RTP_FresnelAtten[n];
                        //	checkChange(ref nval, EditorGUILayout.Slider("      attenuate by gloss", _target.RTP_FresnelAtten[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                        //	_target.RTP_FresnelAtten[n]=nval;
                        //	EditorGUI.EndDisabledGroup();
                        //}

                        nval = _target.RTP_DiffFresnel[n];
                        checkChange(ref nval, EditorGUILayout.Slider("   diffuse scattering", _target.RTP_DiffFresnel[n], 0f, 2f, GUILayout.ExpandWidth(true)));
                        _target.RTP_DiffFresnel[n] = nval;

                        //nval=_target.RTP_IBL_bump_smoothness[n];
                        //checkChange(ref nval, EditorGUILayout.Slider("IBL / Refl bump smooth", _target.RTP_IBL_bump_smoothness[n], 0f, 1f));
                        //_target.RTP_IBL_bump_smoothness[n]=nval;		

                        //if ( (!AddPass_flag && RTP_IBL_DIFFUSE_FIRST) || (AddPass_flag && RTP_IBL_SPEC_ADD) ) {
                        //	nval=_target.RTP_IBL_DiffuseStrength[n];
                        //	checkChange(ref nval, EditorGUILayout.Slider("IBL diffuse exposure", _target.RTP_IBL_DiffuseStrength[n], 0f, 8f));
                        //	_target.RTP_IBL_DiffuseStrength[n]=nval;
                        //}						

                        //EditorGUI.BeginDisabledGroup(!((!AddPass_flag && (REFLECTION_ENABLED_FIRST || RTP_IBL_SPEC_FIRST)) || (AddPass_flag && (REFLECTION_ENABLED_ADD || RTP_IBL_SPEC_ADD))));
                        //nval = _target.RTP_IBL_SpecStrength[n];
                        //EditorGUILayout.LabelField("IBL spec / Refl exposure", GUILayout.Width(150));
                        //checkChange(ref nval, EditorGUILayout.Slider(_target.RTP_IBL_SpecStrength[n], 0f, 8f));
                        //_target.RTP_IBL_SpecStrength[n] = nval;
                        //EditorGUI.EndDisabledGroup();
                        //EditorGUILayout.EndHorizontal();
                        //EditorGUILayout.BeginHorizontal();
                        //EditorGUILayout.LabelField("     color from albedo (metal tint)", GUILayout.Width(200));
                        //nval = _target.LayerAlbedo2SpecColor[n];
                        //checkChange(ref nval, EditorGUILayout.Slider(_target.LayerAlbedo2SpecColor[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                        //_target.LayerAlbedo2SpecColor[n] = nval;
                        //EditorGUILayout.EndHorizontal();
                        //if (AddPass_flag)
                        //{
                        //    EditorGUILayout.BeginHorizontal();
                        //    EditorGUILayout.LabelField("Specmask for deferred AddPass", GUILayout.MinWidth(190), GUILayout.MaxWidth(190));
                        //    checkChange(ref _target._DeferredSpecDampAddPass[n], EditorGUILayout.Slider(_target._DeferredSpecDampAddPass[n], 0, 1));
                        //    EditorGUILayout.EndHorizontal();
                        //}

                        //
                        // gloss baking section
                        //
                        {
                            Texture2D glossTexToBake = _target.splats[n];
                            int prep_atlas = -1;
                            if (n < 4 && atlasA_needed)
                            {
                                prep_atlas = 0;
                                glossTexToBake = _target.splat_atlases[0];
                            }
                            else if (n >= 4 && n < 8 && atlasB_needed)
                            {
                                prep_atlas = 1;
                                glossTexToBake = _target.splat_atlases[1];
                            }
                            else if (n >= 8 && n < 12 && atlasC_needed)
                            {
                                prep_atlas = 2;
                                glossTexToBake = _target.splat_atlases[2];
                            }
                            bool canBake = glossTexToBake != null && AssetDatabase.GetAssetPath(glossTexToBake) != "";
                            if (canBake)
                            {
                                EditorGUILayout.HelpBox("You can consider baking esp. when your surface is very smooth (Detail A channel) and it containts high frequency details together with layer normalmap. Baking can reduce specular spot pixels jittering when using higher texture MIP levels.", MessageType.Warning, true);
                            }
                            EditorGUI.BeginDisabledGroup(!canBake);
                            if (GUILayout.Button("Prepare gloss in higher texture MIP levels" + ((prep_atlas >= 0) ? "\n(whole atlas - no variance)":"")))
                            {
                                AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_target.Bumps[n]));
                                if (_importer)
                                {
                                    TextureImporter tex_importer = (TextureImporter)_importer;
                                    if (!tex_importer.isReadable)
                                    {
                                        Debug.LogWarning("Texture (" + _target.Bumps[n].name + ") has been reimported as readable.");
                                        tex_importer.isReadable = true;
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_target.Bumps[n]), ImportAssetOptions.ForceUpdate);
                                    }
                                }
                                _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(glossTexToBake));
                                if (_importer)
                                {
                                    _target.GlossBakeJob(glossTexToBake, prep_atlas>=0 ? null : _target.Bumps[n]);
                                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(glossTexToBake), ImportAssetOptions.ForceUpdate);
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                        }

                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(8);

                    //if (!RTP_SUPER_SIMPLE)
                    {
                        EditorGUILayout.LabelField("Layer adjustement", EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox("Setting brightness to 0 allows for finetuning / debuging specular term alone.", MessageType.None, true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Layer Brightness", GUILayout.Width(127));
                        nval = _target.LayerBrightness[n];
                        checkChange(ref nval, EditorGUILayout.Slider(_target.LayerBrightness[n], 0f, 4f, GUILayout.ExpandWidth(true)));
                        _target.LayerBrightness[n] = nval;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Layer Saturation", GUILayout.Width(127));
                        nval = _target.LayerSaturation[n];
                        checkChange(ref nval, EditorGUILayout.Slider(_target.LayerSaturation[n], 0f, 2f, GUILayout.ExpandWidth(true)));
                        _target.LayerSaturation[n] = nval;
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.BeginDisabledGroup(!(AddPass_flag ? RTP_USE_EXTRUDE_REDUCTION_ADD : RTP_USE_EXTRUDE_REDUCTION_FIRST));
                        nval = _target.PER_LAYER_HEIGHT_MODIFIER[n];
                        checkChange(ref nval, EditorGUILayout.Slider("Extrude reduction", _target.PER_LAYER_HEIGHT_MODIFIER[n], 0.0f, 1f, GUILayout.ExpandWidth(true)));
                        _target.PER_LAYER_HEIGHT_MODIFIER[n] = nval;
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Far filtering", GUILayout.Width(127));
                        nval = _target.MIPmult[n];
                        checkChange(ref nval, EditorGUILayout.Slider(_target.MIPmult[n], 0f, 7f, GUILayout.ExpandWidth(true)));
                        _target.MIPmult[n] = nval;
                        EditorGUILayout.EndHorizontal();
                        if ((!AddPass_flag && RTP_HEIGHTBLEND_AO_FIRST) || (AddPass_flag && RTP_HEIGHTBLEND_AO_ADD))
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Heightblend AO", GUILayout.Width(127));
                            nval = _target.AO_strength[n];
                            checkChange(ref nval, EditorGUILayout.Slider(_target.AO_strength[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                            _target.AO_strength[n] = nval;
                            EditorGUILayout.EndHorizontal();
                        }
                        if ((!AddPass_flag && RTP_EMISSION_FIRST) || (AddPass_flag && RTP_EMISSION_ADD))
                        {
                            EditorGUILayout.HelpBox("Layer emission is controlled by below slider && gloss output. For high emission values layer specularity is cancelled.", MessageType.None, true);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Layer emission", GUILayout.Width(125));
                            nval = _target.LayerEmission[n];
                            checkChange(ref nval, EditorGUILayout.Slider(_target.LayerEmission[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                            _target.LayerEmission[n] = nval;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Emission glow color", GUILayout.Width(125));
                            checkChange(ref _target.LayerEmissionColor[n], EditorGUILayout.ColorField(_target.LayerEmissionColor[n]));
                            EditorGUILayout.EndHorizontal();
                            if ((!AddPass_flag && RTP_EMISSION_FIRST && RTP_HOTAIR_EMISSION_FIRST) || (AddPass_flag && RTP_EMISSION_ADD && RTP_HOTAIR_EMISSION_ADD))
                            {
                                EditorGUILayout.HelpBox("For emissive layers you can adjust simple hot air refraction effect.", MessageType.None, true);
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Hot air refraction strength", GUILayout.Width(160));
                                nval = _target.LayerEmissionRefractStrength[n];
                                checkChange(ref nval, EditorGUILayout.Slider(_target.LayerEmissionRefractStrength[n], 0f, 0.02f, GUILayout.ExpandWidth(true)));
                                _target.LayerEmissionRefractStrength[n] = nval;
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("        on layer edges only", GUILayout.Width(160));
                                nval = _target.LayerEmissionRefractHBedge[n];
                                checkChange(ref nval, EditorGUILayout.Slider(_target.LayerEmissionRefractHBedge[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                                _target.LayerEmissionRefractHBedge[n] = nval;
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    } // EOF !super-simple

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent(icoGlobalcolor, "Global maps settings"), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16)))
                    {
                        _target.submenu = ReliefTerrainMenuItems.GeneralSettings;
                        _target.submenu_settings = ReliefTerrainSettingsItems.GlobalColor;
                    }
                    EditorGUILayout.LabelField("Global colormap", GUILayout.Width(130));
                    nval = _target.GlobalColorPerLayer[n];
                    checkChange(ref nval, EditorGUILayout.Slider(_target.GlobalColorPerLayer[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                    _target.GlobalColorPerLayer[n] = nval;
                    EditorGUILayout.EndHorizontal();
                    //if (!RTP_SUPER_SIMPLE && (AddPass_flag ? ADV_COLOR_MAP_BLENDING_ENABLED_ADD : ADV_COLOR_MAP_BLENDING_ENABLED_FIRST))
                    if (AddPass_flag ? ADV_COLOR_MAP_BLENDING_ENABLED_ADD : ADV_COLOR_MAP_BLENDING_ENABLED_FIRST)
                    {
                        EditorGUILayout.BeginVertical("Box");
                        EditorGUILayout.LabelField("Advanced blending settings", EditorStyles.boldLabel);
                        EditorGUILayout.HelpBox("Below options are based on detail heightmaps. You can separately set blending for low & hi parts.", MessageType.None, true);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Height levels", GUILayout.Width(145));
                        float valMin = _target.GlobalColorBottom[n];
                        float valMax = _target.GlobalColorTop[n];
                        float nvalMin = _target.GlobalColorBottom[n];
                        float nvalMax = _target.GlobalColorTop[n];
                        EditorGUILayout.MinMaxSlider(ref valMin, ref valMax, 0f, 1f, GUILayout.ExpandWidth(true));
                        if (checkChange(ref nvalMin, valMin, ref nvalMax, valMax))
                        {
                            if (Mathf.Abs(nvalMin - nvalMax) < 0.04f)
                            {
                                nvalMin -= 0.02f;
                                if (nvalMin < 0) nvalMin = 0;
                                nvalMax += 0.02f;
                                if (nvalMax > 1) nvalMax = 1;
                            }
                        }
                        _target.GlobalColorBottom[n] = nvalMin;
                        _target.GlobalColorTop[n] = nvalMax;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("  colormap saturation LO", GUILayout.Width(145));
                        nval = _target.GlobalColorColormapLoSat[n];
                        checkChange(ref nval, EditorGUILayout.Slider(_target.GlobalColorColormapLoSat[n], 0f, 2f, GUILayout.ExpandWidth(true)));
                        _target.GlobalColorColormapLoSat[n] = nval;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("  colormap saturation HI", GUILayout.Width(145));
                        nval = _target.GlobalColorColormapHiSat[n];
                        checkChange(ref nval, EditorGUILayout.Slider(_target.GlobalColorColormapHiSat[n], 0f, 2f, GUILayout.ExpandWidth(true)));
                        _target.GlobalColorColormapHiSat[n] = nval;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("  layer saturation LO", GUILayout.Width(145));
                        nval = _target.GlobalColorLayerLoSat[n];
                        checkChange(ref nval, EditorGUILayout.Slider(_target.GlobalColorLayerLoSat[n], 0f, 2f, GUILayout.ExpandWidth(true)));
                        _target.GlobalColorLayerLoSat[n] = nval;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("  layer saturation HI", GUILayout.Width(145));
                        nval = _target.GlobalColorLayerHiSat[n];
                        checkChange(ref nval, EditorGUILayout.Slider(_target.GlobalColorLayerHiSat[n], 0f, 2f, GUILayout.ExpandWidth(true)));
                        _target.GlobalColorLayerHiSat[n] = nval;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Blending for LO", GUILayout.Width(145));
                        nval = _target.GlobalColorLoBlend[n];
                        checkChange(ref nval, EditorGUILayout.Slider(_target.GlobalColorLoBlend[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                        _target.GlobalColorLoBlend[n] = nval;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Blending for HI", GUILayout.Width(145));
                        nval = _target.GlobalColorHiBlend[n];
                        checkChange(ref nval, EditorGUILayout.Slider(_target.GlobalColorHiBlend[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                        _target.GlobalColorHiBlend[n] = nval;
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();

                    //if (!RTP_SUPER_SIMPLE)
                    {
                        EditorGUILayout.BeginVertical("Box");
                        if ((AddPass_flag ? UV_BLEND_ENABLED_ADD : UV_BLEND_ENABLED_FIRST))
                        {
                            EditorGUI.BeginDisabledGroup(!(AddPass_flag ? UV_BLEND_ENABLED_ADD : UV_BLEND_ENABLED_FIRST));
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent(icoUVBlend, "UV blend settings"), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16)))
                            {
                                _target.submenu = ReliefTerrainMenuItems.GeneralSettings;
                                _target.submenu_settings = ReliefTerrainSettingsItems.UVblend;
                            }
                            int firstAddPassLayer = AddPass_flag ? ((_target.numLayers > 8) ? 8 : 4) : 0;
                            int routed_n = AddPass_flag ? UV_BLEND_ROUTE_NUM_ADD[n - firstAddPassLayer] : UV_BLEND_ROUTE_NUM_FIRST[n];
                            if (routed_n > _target.MixScale.Length - 1) routed_n = n; // at init
                            nval = _target.MixScale[routed_n + firstAddPassLayer];
                            EditorGUILayout.LabelField("UV blend scale" + ((routed_n != n - firstAddPassLayer) ? " (from " + routed_n + ")" : ""), GUILayout.Width(130 + ((routed_n != n - firstAddPassLayer) ? 10 : 0)));
                            EditorGUILayout.BeginHorizontal();
                            checkChange(ref nval, EditorGUILayout.Slider(_target.MixScale[routed_n + firstAddPassLayer], 0.01f, 0.5f));
                            //								if (GUILayout.Button(new GUIContent("A","Align to terrain tile"), GUILayout.Width(22), GUILayout.Height(16))) {
                            //									float ratio=Mathf.Round(_target.ReliefTransform.x*nval);
                            //									nval=ratio/_target.ReliefTransform.x;
                            //									_target.RefreshAll();
                            //								}
                            EditorGUILayout.EndHorizontal();
                            _target.MixScale[routed_n + firstAddPassLayer] = nval;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            nval = _target.MixBlend[n];
                            EditorGUILayout.LabelField(" ", GUILayout.Width(16));
                            EditorGUILayout.LabelField("UV blend val", GUILayout.Width(130));
                            checkChange(ref nval, EditorGUILayout.Slider(_target.MixBlend[n], 0, 0.9f));
                            _target.MixBlend[n] = nval;
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            nval = _target.MixSaturation[n];
                            EditorGUILayout.LabelField(" ", GUILayout.Width(16));
                            EditorGUILayout.LabelField("              saturation", GUILayout.Width(130));
                            checkChange(ref nval, EditorGUILayout.Slider(_target.MixSaturation[n], 0, 1.0f));
                            _target.MixSaturation[n] = nval;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            nval = _target.MixBrightness[n];
                            EditorGUILayout.LabelField(" ", GUILayout.Width(16));
                            EditorGUILayout.LabelField("              brightness", GUILayout.Width(130));
                            checkChange(ref nval, EditorGUILayout.Slider(_target.MixBrightness[n], 0, 4.0f));
                            _target.MixBrightness[n] = nval;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            nval = _target.MixReplace[n];
                            EditorGUILayout.LabelField(" ", GUILayout.Width(16));
                            EditorGUILayout.LabelField("Far blend replace", GUILayout.Width(130));
                            checkChange(ref nval, EditorGUILayout.Slider(_target.MixReplace[n], 0, 1.0f));
                            _target.MixReplace[n] = nval;
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent(icoUVBlend, "UV blend settings"), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16)))
                            {
                                _target.submenu = ReliefTerrainMenuItems.GeneralSettings;
                                _target.submenu_settings = ReliefTerrainSettingsItems.UVblend;
                            }
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.LabelField("UV blend (disabled)");
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                    }
                    //if (!RTP_SUPER_SIMPLE || (AddPass_flag ? RTP_USE_PERLIN_ADD : RTP_USE_PERLIN_FIRST))
                    {
                        EditorGUILayout.BeginVertical("Box");
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent(icoPerlinNormal, "Perlin normal settings"), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16)))
                        {
                            _target.submenu = ReliefTerrainMenuItems.GeneralSettings;
                            _target.submenu_settings = ReliefTerrainSettingsItems.GlobalNormal;
                        }
                        nval = _target._BumpMapGlobalStrength[n];
                        EditorGUILayout.LabelField("Perlin normal", GUILayout.Width(130));
                        checkChange(ref nval, EditorGUILayout.Slider(_target._BumpMapGlobalStrength[n], 0.02f, 2f));
                        _target._BumpMapGlobalStrength[n] = nval;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }

                    //if (!RTP_SUPER_SIMPLE)
                    {
                        EditorGUILayout.BeginVertical("Box");

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent(icoSuperdetail, "Superdetail settings"), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16)))
                        {
                            _target.submenu = ReliefTerrainMenuItems.GeneralSettings;
                            _target.submenu_settings = ReliefTerrainSettingsItems.Superdetail;
                        }
                        if (AddPass_flag ? SUPER_DETAIL_ENABLED_ADD : SUPER_DETAIL_ENABLED_FIRST)
                        {
                            nval = _target._SuperDetailStrengthNormal[n];
                            EditorGUILayout.LabelField("Superdetail normal", GUILayout.Width(130));
                            if (checkChange(ref nval, EditorGUILayout.Slider(_target._SuperDetailStrengthNormal[n], 0f, 1f)))
                            {
                                _target._SuperDetailStrengthNormal[n] = nval;
                            }
                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.LabelField("Superdetail (disabled)", GUILayout.Width(130));
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUILayout.EndHorizontal();

                        if ((AddPass_flag ? SUPER_DETAIL_ENABLED_ADD : SUPER_DETAIL_ENABLED_FIRST) && (AddPass_flag ? SUPER_DETAIL_MULTS_ENABLED_ADD : SUPER_DETAIL_MULTS_ENABLED_FIRST))
                        {
                            EditorGUILayout.BeginHorizontal();
                            _target._SuperDetailStrengthNormal[n] = nval;
                            nval = _target._SuperDetailStrengthMultA[n];
                            EditorGUILayout.LabelField(" ", GUILayout.Width(16));
                            EditorGUILayout.LabelField("Superdetail mult A", GUILayout.Width(130));
                            checkChange(ref nval, EditorGUILayout.Slider(_target._SuperDetailStrengthMultA[n], 0f, 1f));
                            _target._SuperDetailStrengthMultA[n] = nval;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            nval = _target._SuperDetailStrengthMultASelfMaskNear[n];
                            EditorGUILayout.LabelField(" ", GUILayout.Width(16));
                            EditorGUILayout.LabelField("  SelfMask near", GUILayout.Width(130));
                            checkChange(ref nval, EditorGUILayout.Slider(_target._SuperDetailStrengthMultASelfMaskNear[n], 0f, 1f));
                            _target._SuperDetailStrengthMultASelfMaskNear[n] = nval;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            nval = _target._SuperDetailStrengthMultASelfMaskFar[n];
                            EditorGUILayout.LabelField(" ", GUILayout.Width(16));
                            EditorGUILayout.LabelField("  SelfMask far", GUILayout.Width(130));
                            checkChange(ref nval, EditorGUILayout.Slider(_target._SuperDetailStrengthMultASelfMaskFar[n], 0f, 1f));
                            _target._SuperDetailStrengthMultASelfMaskFar[n] = nval;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            nval = _target._SuperDetailStrengthMultB[n];
                            EditorGUILayout.LabelField(" ", GUILayout.Width(16));
                            EditorGUILayout.LabelField("Superdetail mult B", GUILayout.Width(130));
                            checkChange(ref nval, EditorGUILayout.Slider(_target._SuperDetailStrengthMultB[n], 0f, 1f));
                            _target._SuperDetailStrengthMultB[n] = nval;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            nval = _target._SuperDetailStrengthMultBSelfMaskNear[n];
                            EditorGUILayout.LabelField(" ", GUILayout.Width(16));
                            EditorGUILayout.LabelField("  SelfMask near", GUILayout.Width(130));
                            checkChange(ref nval, EditorGUILayout.Slider(_target._SuperDetailStrengthMultBSelfMaskNear[n], 0f, 1f));
                            _target._SuperDetailStrengthMultBSelfMaskNear[n] = nval;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            nval = _target._SuperDetailStrengthMultBSelfMaskFar[n];
                            EditorGUILayout.LabelField(" ", GUILayout.Width(16));
                            EditorGUILayout.LabelField("  SelfMask far", GUILayout.Width(130));
                            checkChange(ref nval, EditorGUILayout.Slider(_target._SuperDetailStrengthMultBSelfMaskFar[n], 0f, 1f));
                            _target._SuperDetailStrengthMultBSelfMaskFar[n] = nval;
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical("Box");
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent(icoVerticalTexture, "Vertical map settings"), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16)))
                        {
                            _target.submenu = ReliefTerrainMenuItems.GeneralSettings;
                            _target.submenu_settings = ReliefTerrainSettingsItems.VerticalTex;
                        }
                        if (AddPass_flag ? RTP_VERTICAL_TEXTURE_ADD : RTP_VERTICAL_TEXTURE_FIRST)
                        {
                            EditorGUI.BeginDisabledGroup(!(AddPass_flag ? RTP_VERTICAL_TEXTURE_ADD : RTP_VERTICAL_TEXTURE_FIRST));
                            nval = _target.VerticalTextureStrength[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Vertical map strength", _target.VerticalTextureStrength[n], 0f, 1f));
                            _target.VerticalTextureStrength[n] = nval;
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.LabelField("Vertical map (disabled)");
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical("Box");
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent(icoSnow, "Snow settings"), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16)))
                        {
                            _target.submenu = ReliefTerrainMenuItems.GeneralSettings;
                            _target.submenu_settings = ReliefTerrainSettingsItems.Snow;
                        }
                        if (AddPass_flag ? SNOW_ENABLED_ADD : SNOW_ENABLED_FIRST)
                        {
                            nval = _target._snow_strength_per_layer[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Snow strength", _target._snow_strength_per_layer[n], 0f, 1f));
                            _target._snow_strength_per_layer[n] = nval;
                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.LabelField("Snow (disabled)");
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical("Box");
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent(icoGlitter, "Glitter settings"), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16)))
                        {
                            _target.submenu = ReliefTerrainMenuItems.GeneralSettings;
                            _target.submenu_settings = ReliefTerrainSettingsItems.Glitter;
                        }
                        if (AddPass_flag ? GLITTER_ENABLED_ADD : GLITTER_ENABLED_FIRST)
                        {
                            nval = _target._GlitterStrength[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Glitter strength", _target._GlitterStrength[n], 0f, 1f));
                            _target._GlitterStrength[n] = nval;
                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.LabelField("Glitter (disabled)");
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical("Box");
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent(icoWater, "Water settings"), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16)))
                        {
                            _target.submenu = ReliefTerrainMenuItems.GeneralSettings;
                            _target.submenu_settings = ReliefTerrainSettingsItems.Water;
                        }
                        if (AddPass_flag ? WATER_ENABLED_ADD : WATER_ENABLED_FIRST)
                        {
                            nval = _target.TERRAIN_LayerWetStrength[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Layer wetness", _target.TERRAIN_LayerWetStrength[n], 0f, 1f));
                            _target.TERRAIN_LayerWetStrength[n] = nval;
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.HelpBox("Accumulated water color (below water level).\nRealised multiplicative (unless you set high water opacity controlled by color alpha.", MessageType.None, true);
                            checkChange(ref _target.TERRAIN_WaterColor[n], EditorGUILayout.ColorField("Water color (A - opacity)", _target.TERRAIN_WaterColor[n]));
                            if ((!AddPass_flag && RTP_EMISSION_FIRST) || (AddPass_flag && RTP_EMISSION_ADD))
                            {
                                nval = _target.TERRAIN_WaterEmission[n];
                                checkChange(ref nval, EditorGUILayout.Slider("Water emission", _target.TERRAIN_WaterEmission[n], 0f, 1f, GUILayout.ExpandWidth(true)));
                                _target.TERRAIN_WaterEmission[n] = nval;
                            }

                            GUILayout.Space(4);

                            nval = _target.TERRAIN_WaterLevel[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Water level", _target.TERRAIN_WaterLevel[n] / 2f, 0f, 1f) * 2f);
                            _target.TERRAIN_WaterLevel[n] = nval;

                            nval = _target.TERRAIN_WaterLevelSlopeDamp[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Water level slope damp", _target.TERRAIN_WaterLevelSlopeDamp[n], 0.1f, 2));
                            _target.TERRAIN_WaterLevelSlopeDamp[n] = nval;

                            nval = _target.TERRAIN_WaterEdge[n];
                            checkChange(ref nval, Mathf.Pow(2, EditorGUILayout.Slider("Water level sharpness", Mathf.Log(_target.TERRAIN_WaterEdge[n]) / Mathf.Log(2) / 2, 0, 1) * 2));
                            _target.TERRAIN_WaterEdge[n] = nval;

                            GUILayout.Space(4);

                            //nval=_target.TERRAIN_WaterSpecularity[n];
                            //checkChange(ref nval, EditorGUILayout.Slider("Water spec boost", _target.TERRAIN_WaterSpecularity[n], -1f, 1f));
                            //_target.TERRAIN_WaterSpecularity[n]=nval;		
                            nval=_target.TERRAIN_WaterMetallic[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Water metalness", _target.TERRAIN_WaterMetallic[n], 0f, 1f));
                            _target.TERRAIN_WaterMetallic[n]=nval;		

                            nval = _target.TERRAIN_WaterGloss[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Water gloss", _target.TERRAIN_WaterGloss[n], 0f, 1f));
                            _target.TERRAIN_WaterGloss[n] = nval;

                            nval = _target.TERRAIN_Flow[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Water flow", _target.TERRAIN_Flow[n], 0f, 1f));
                            _target.TERRAIN_Flow[n] = nval;
                            nval = _target.TERRAIN_Refraction[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Water refraction", _target.TERRAIN_Refraction[n], 0f, 0.04f));
                            _target.TERRAIN_Refraction[n] = nval;

                            //if ( (!AddPass_flag && (REFLECTION_ENABLED_FIRST || RTP_IBL_SPEC_FIRST)) || (AddPass_flag && (REFLECTION_ENABLED_FIRST || RTP_IBL_SPEC_FIRST)) ) {
                            //	GUILayout.Space(4);
                            //	nval=_target.TERRAIN_WaterIBL_SpecWaterStrength[n];
                            //	checkChange(ref nval, EditorGUILayout.Slider("IBL spec / Refl - water", _target.TERRAIN_WaterIBL_SpecWaterStrength[n], 0f, 8f));
                            //	_target.TERRAIN_WaterIBL_SpecWaterStrength[n]=nval;
                            //}

                            GUILayout.Space(4);
                            EditorGUILayout.HelpBox("Wet means layer surface covered by thin water layer, but above water level", MessageType.None, true);
                            //nval=_target.TERRAIN_WetSpecularity[n];
                            //checkChange(ref nval, EditorGUILayout.Slider("Wet spec boost", _target.TERRAIN_WetSpecularity[n], -1f, 1f));
                            //_target.TERRAIN_WetSpecularity[n]=nval;		

                            nval = _target.TERRAIN_WetGloss[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Wet gloss", _target.TERRAIN_WetGloss[n], 0f, 1f));
                            _target.TERRAIN_WetGloss[n] = nval;

                            nval = _target.TERRAIN_WetFlow[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Wet flow", _target.TERRAIN_WetFlow[n], 0f, 1f));
                            _target.TERRAIN_WetFlow[n] = nval;

                            //					nval=_target.TERRAIN_WetReflection[n];
                            //					checkChange(ref nval, EditorGUILayout.Slider("Wet reflection", _target.TERRAIN_WetReflection[n]/2f, 0f, 1f)*2f);
                            //					_target.TERRAIN_WetReflection[n]=nval;		

                            nval = _target.TERRAIN_WetRefraction[n];
                            checkChange(ref nval, EditorGUILayout.Slider("Wet refraction factor", _target.TERRAIN_WetRefraction[n], 0f, 1f));
                            _target.TERRAIN_WetRefraction[n] = nval;

                            //if ( (!AddPass_flag && (REFLECTION_ENABLED_FIRST || RTP_IBL_SPEC_FIRST)) || (AddPass_flag && (REFLECTION_ENABLED_FIRST || RTP_IBL_SPEC_FIRST)) ) {
                            //	nval=_target.TERRAIN_WaterIBL_SpecWetStrength[n];
                            //	checkChange(ref nval, EditorGUILayout.Slider("IBL spec / Refl - wet", _target.TERRAIN_WaterIBL_SpecWetStrength[n], 0f, 8f));
                            //	_target.TERRAIN_WaterIBL_SpecWetStrength[n]=nval;
                            //}
                            GUILayout.Space(4);

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Hi-freq / distance gloss damper", GUILayout.MinWidth(190), GUILayout.MaxWidth(190));
                            nval = _target.TERRAIN_WaterGlossDamper[n];
                            checkChange(ref nval, EditorGUILayout.Slider(_target.TERRAIN_WaterGlossDamper[n], 0f, 1f));
                            _target.TERRAIN_WaterGlossDamper[n] = nval;
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(6);

                            string[] options = new string[_target.numLayers];
                            options[0] = "Choose layer to copy water params from";
                            for (int k = 0; k < _target.numLayers; k++)
                            {
                                if (k <= n)
                                {
                                    if (k < _target.numLayers - 1) options[k + 1] = "layer " + k;
                                }
                                else
                                {
                                    options[k] = "layer " + k;
                                }
                            }
                            int idx = EditorGUILayout.Popup(0, options);
                            if (idx > 0)
                            {
                                if (idx - 1 < n)
                                {
                                    idx--;
                                }
                                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                                _target.CopyWaterParams(idx, n);
                                EditorUtility.SetDirty(_targetRT);
                                _target.Refresh(null, _targetRT);
                            }

                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.LabelField("Water (disabled)");
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();

                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                } // EOF !super-simple

                GUILayout.Space(8);
                if (GUILayout.Button("Reset layer numeric params to default"))
                {
                    Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                    _target.ReturnToDefaults("layer");
                    EditorUtility.SetDirty(_targetRT);
                    _target.Refresh(null, _targetRT);
                }

        #endregion Detail maps unfold	- layers
            }


        #endregion Detail maps unfold
        }

        //bool nshow_controlmaps=EditorGUILayout.Foldout(_target.show_controlmaps, "Control (alpha) maps");
        //if (nshow_controlmaps) {
        if (_target.show_controlmaps)
        {
        #region Control maps
            Color skin_color;
            toolbarStrings = new string[3] { "Compose", "Acquire", "Control maps" };
            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            _target.submenu_control_textures = (ReliefTerrainControlTexturesItems)(GUILayout.Toolbar((int)_target.submenu_control_textures, toolbarStrings,
#if !UNITY_2019_3_OR_NEWER
            EditorStyles.miniButton,
#endif
                GUILayout.MaxWidth(370)));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            switch (_target.submenu_control_textures)
            {
                case ReliefTerrainControlTexturesItems.Compose:
        #region Control maps - compose maps
                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Compose splats", EditorStyles.boldLabel);
                    GUI.color = skin_color;
                    EditorGUILayout.HelpBox("Here you can manipulate alpha maps that control layers coverage,\n1. Source splat is coverage texture and you can choose its channel,\n2. Value taken from coverage texture (can be inverted) is multiplied by given factor,\n3. Result is normalized,\n4. Blank source splats are considered 1 (white),\n5. In case you unset \"active\", input value is taken from current control map.", MessageType.None, true);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Layer", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
                    EditorGUILayout.LabelField("Source splats", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < _target.numLayers; i++)
                    {
                        int n = _targetRT.splat_layer_seq[i];
                        Texture2D tex = _targetRT.source_controls[n];
                        Texture2D ntex = tex;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label((Texture2D)AssetPreview.GetAssetPreview(_target.splats[n]), GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                        if (_targetRT.splat_layer_calc[n])
                        {
                            if (_targetRT.source_controls[n])
                            {
                                checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(_targetRT.source_controls[n], typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                            }
                            else
                            {
                                if (!blank_white_tex)
                                {
                                    blank_white_tex = new Texture2D(4, 4, TextureFormat.ARGB32, false);
                                    Color[] cols = new Color[16];
                                    for (int k = 0; k < 16; k++) cols[k] = Color.white;
                                    blank_white_tex.SetPixels(cols);
                                    blank_white_tex.Apply(false, true);
                                }
                                if (!blank_black_tex)
                                {
                                    blank_black_tex = new Texture2D(4, 4, TextureFormat.ARGB32, false);
                                    Color[] cols = new Color[16];
                                    for (int k = 0; k < 16; k++) cols[k] = Color.black;
                                    blank_black_tex.SetPixels(cols);
                                    blank_black_tex.Apply(false, true);
                                }
                                if (_targetRT.source_controls_invert[n])
                                {
                                    ntex = tex = blank_black_tex;
                                }
                                else
                                {
                                    ntex = tex = blank_white_tex;
                                }
                                checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(tex, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                                if (ntex == null)
                                {
                                    _targetRT.source_controls[n] = tex = null;
                                }
                            }
                            if (_targetRT.source_controls[n] == blank_white_tex || ntex == blank_white_tex || _targetRT.source_controls[n] == blank_black_tex || ntex == blank_black_tex || ntex == null) _targetRT.source_controls[n] = ntex = tex = null;
                            if (ntex != tex)
                            {
                                int wdth = 1024;
                                if (_targetRT.controlA != null)
                                {
                                    wdth = _targetRT.controlA.width;
                                    for (int j = 0; j < _targetRT.source_controls.Length; j++)
                                    {
                                        Color[] cols = new Color[wdth * wdth];
                                        bool cols_inited = false;
                                        if (_targetRT.source_controls[j])
                                        {
                                            if (_targetRT.source_controls[j].width != wdth)
                                            {
                                                if (!cols_inited)
                                                {
                                                    cols_inited = true;
                                                    for (int u = 0; u < cols.Length; u++) cols[u] = Color.white;
                                                }
                                                _targetRT.source_controls[j] = new Texture2D(wdth, wdth);
                                                _targetRT.source_controls[j].SetPixels(cols);
                                                _targetRT.source_controls[j].Apply(true);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < _targetRT.source_controls.Length; j++)
                                    {
                                        if (_targetRT.source_controls[j])
                                        {
                                            wdth = _targetRT.source_controls[j].width;
                                            break;
                                        }
                                    }
                                }
                                if (ntex && ntex.width == wdth)
                                {
                                    _targetRT.source_controls[n] = ntex;
                                    AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ntex));
                                    if (_importer)
                                    {
                                        TextureImporter tex_importer = (TextureImporter)_importer;
                                        if (!tex_importer.isReadable)
                                        {
                                            Debug.LogWarning("Source splat texture " + n + " (" + ntex.name + ") has been reimported as readable.");
                                            tex_importer.isReadable = true;
                                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(ntex), ImportAssetOptions.ForceUpdate);
                                        }
                                    }
                                    if (_targetRT.source_controls_invert[n])
                                    {
                                        Color[] cols = _targetRT.source_controls[n].GetPixels();
                                        _target.InvertChannel(cols, -1);
                                        int w = _targetRT.source_controls[n].width;
                                        _targetRT.source_controls[n] = new Texture2D(w, w);
                                        _targetRT.source_controls[n].SetPixels(cols);
                                        _targetRT.source_controls[n].Apply();
                                    }
                                }
                                else
                                {
                                    if (_targetRT.controlA)
                                    {
                                        EditorUtility.DisplayDialog("Error", "Source control alpha maps should have " + wdth + " size.", "OK");
                                    }
                                    else
                                    {
                                        ///////////////////
                                        if (EditorUtility.DisplayDialog("Error", "Should I reset all source control alpha maps to " + ntex.width + " size?", "Yes, please", "No, thanks"))
                                        {
                                            wdth = ntex.width;
                                            Color[] cols = new Color[wdth * wdth];
                                            for (int u = 0; u < cols.Length; u++) cols[u] = Color.white;
                                            for (int j = 0; j < _targetRT.source_controls.Length; j++)
                                            {
                                                if (j != n)
                                                {
                                                    if (_targetRT.source_controls[j].width != wdth)
                                                    {
                                                        _targetRT.source_controls[j] = new Texture2D(wdth, wdth);
                                                        _targetRT.source_controls[j].SetPixels(cols);
                                                        _targetRT.source_controls[j].Apply(true);
                                                    }
                                                }
                                                else
                                                {
                                                    _targetRT.source_controls[n] = ntex;
                                                }
                                            }
                                        }
                                        /////////////////										
                                    }
                                }
                            }
                        }
                        else
                        {
                            GUILayout.Label((Texture2D)null, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                        }
                        EditorGUILayout.BeginVertical(); // layer props
                        EditorGUILayout.BeginHorizontal();
                        checkChange(ref _targetRT.splat_layer_calc[n], EditorGUILayout.Toggle(_targetRT.splat_layer_calc[n], GUILayout.MinWidth(10), GUILayout.MaxWidth(10)));
                        EditorGUILayout.LabelField("active", GUILayout.MinWidth(40), GUILayout.MaxWidth(40));
                        EditorGUILayout.EndHorizontal();
                        if (_targetRT.splat_layer_calc[n])
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("channel", GUILayout.MinWidth(60), GUILayout.MaxWidth(60));
                            checkChange(ref _targetRT.source_controls_channels[n], (RTPColorChannels)EditorGUILayout.EnumPopup(_targetRT.source_controls_channels[n]));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("multiplier", GUILayout.MinWidth(60), GUILayout.MaxWidth(60));
                            checkChange(ref _targetRT.splat_layer_boost[n], EditorGUILayout.FloatField(_targetRT.splat_layer_boost[n]));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("masked", GUILayout.MinWidth(60), GUILayout.MaxWidth(60));
                            checkChange(ref _targetRT.splat_layer_masked[n], EditorGUILayout.Toggle(_targetRT.splat_layer_masked[n], GUILayout.MinWidth(10), GUILayout.MaxWidth(10)));
                            //									if (_targetRT.splat_layer_masked[n] && !_targetRT.source_controls[n]) {
                            //										_targetRT.splat_layer_masked[n]=false;
                            //										EditorUtility.DisplayDialog("Error", "Blank input can not be masked.","OK");
                            //									}
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("invert", GUILayout.MinWidth(40), GUILayout.MaxWidth(40));
                            bool nval = _targetRT.source_controls_invert[n];
                            checkChange(ref nval, EditorGUILayout.Toggle(_targetRT.source_controls_invert[n], GUILayout.MinWidth(10), GUILayout.MaxWidth(10)));
                            if ((_targetRT.source_controls_invert[n] != nval) && (_targetRT.source_controls[n]))
                            {
                                Color[] cols = _targetRT.source_controls[n].GetPixels();
                                _target.InvertChannel(cols, -1);
                                int w = _targetRT.source_controls[n].width;
                                _targetRT.source_controls[n] = new Texture2D(w, w);
                                _targetRT.source_controls[n].SetPixels(cols);
                                _targetRT.source_controls[n].Apply();
                            }
                            _targetRT.source_controls_invert[n] = nval;
                            //									if (_targetRT.source_controls_invert[n] && !_targetRT.source_controls[n]) {
                            //										_targetRT.source_controls_invert[n]=false;
                            //										EditorUtility.DisplayDialog("Error", "Blank input can not be inverted.","OK");
                            //									}
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.BeginHorizontal(); // move up/down
                        if (GUILayout.Button("Move up"))
                        {
                            if (i > 0)
                            {
                                int tmp = _targetRT.splat_layer_seq[i];
                                _targetRT.splat_layer_seq[i] = _targetRT.splat_layer_seq[i - 1];
                                _targetRT.splat_layer_seq[i - 1] = tmp;
                            }
                            //_targetRT.splat_layer_seq=new int[8]{0,1,2,3,4,5,6,7};
                        }
                        if (GUILayout.Button("Move down"))
                        {
                            if (i < _target.numLayers - 1)
                            {
                                int tmp = _targetRT.splat_layer_seq[i];
                                _targetRT.splat_layer_seq[i] = _targetRT.splat_layer_seq[i + 1];
                                _targetRT.splat_layer_seq[i + 1] = tmp;
                            }
                        }
                        EditorGUILayout.EndHorizontal(); // move up/down
                        EditorGUILayout.EndVertical(); // layer props

                        EditorGUILayout.EndHorizontal();
                        // optional mask
                        if (_targetRT.splat_layer_masked[n] && _targetRT.splat_layer_calc[n])
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("Source splat\nmask >", GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                            tex = ntex = _targetRT.source_controls_mask[n];
                            checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(tex, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                            if (ntex != tex)
                            {
                                int wdth = 1024;
                                if (_targetRT.controlA != null)
                                {
                                    wdth = _targetRT.controlA.width;
                                }
                                else
                                {
                                    for (int j = 0; j < _targetRT.source_controls.Length; j++)
                                    {
                                        if (_targetRT.source_controls[j])
                                        {
                                            wdth = _targetRT.source_controls[j].width;
                                            break;
                                        }
                                    }
                                }
                                if (ntex && ntex.width == wdth)
                                {
                                    _targetRT.source_controls_mask[n] = ntex;
                                    AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ntex));
                                    if (_importer)
                                    {
                                        TextureImporter tex_importer = (TextureImporter)_importer;
                                        if (!tex_importer.isReadable)
                                        {
                                            Debug.LogWarning("Mask texture " + n + " (" + ntex.name + ") has been reimported as readable.");
                                            tex_importer.isReadable = true;
                                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(ntex), ImportAssetOptions.ForceUpdate);
                                        }
                                    }
                                    if (_targetRT.source_controls_mask_invert[n])
                                    {
                                        Color[] cols = _targetRT.source_controls_mask[n].GetPixels();
                                        _target.InvertChannel(cols, -1);
                                        int w = _targetRT.source_controls_mask[n].width;
                                        _targetRT.source_controls_mask[n] = new Texture2D(w, w);
                                        _targetRT.source_controls_mask[n].SetPixels(cols);
                                        _targetRT.source_controls_mask[n].Apply();
                                    }
                                }
                                else if (ntex)
                                {
                                    EditorUtility.DisplayDialog("Error", "Mask texture should have " + _targetRT.controlA.width + " size.", "OK");
                                }
                                else
                                {
                                    _targetRT.source_controls_mask[n] = ntex; // null
                                    _targetRT.splat_layer_masked[n] = false;
                                }
                            }
                            EditorGUILayout.BeginVertical(); // mask props
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("channel", GUILayout.MinWidth(60), GUILayout.MaxWidth(60));
                            checkChange(ref _targetRT.source_controls_mask_channels[n], (RTPColorChannels)EditorGUILayout.EnumPopup(_targetRT.source_controls_mask_channels[n]));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("invert", GUILayout.MinWidth(60), GUILayout.MaxWidth(60));
                            bool nval = _targetRT.source_controls_mask_invert[n];
                            checkChange(ref nval, EditorGUILayout.Toggle(_targetRT.source_controls_mask_invert[n]));
                            if ((_targetRT.source_controls_mask_invert[n] != nval) && (_targetRT.source_controls_mask[n]))
                            {
                                Color[] cols = _targetRT.source_controls_mask[n].GetPixels();
                                _target.InvertChannel(cols, -1);
                                int w = _targetRT.source_controls_mask[n].width;
                                _targetRT.source_controls_mask[n] = new Texture2D(w, w);
                                _targetRT.source_controls_mask[n].SetPixels(cols);
                                _targetRT.source_controls_mask[n].Apply();
                            }
                            _targetRT.source_controls_mask_invert[n] = nval;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical(); // mask props						

                            EditorGUILayout.EndHorizontal();
                        }

                    }
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Set all \"active\" flags"))
                    {
                        for (int j = 0; j < _target.numLayers; j++)
                        {
                            _targetRT.splat_layer_calc[j] = true;
                        }
                    }
                    if (GUILayout.Button("Clear all \"active\" flags"))
                    {
                        for (int j = 0; j < _target.numLayers; j++)
                        {
                            _targetRT.splat_layer_calc[j] = false;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.HelpBox("In layered mode highest (bottom) layer \"wins\". In unlayered mode order doesn't matter.", MessageType.None, true);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Layered mode", EditorStyles.boldLabel, GUILayout.MinWidth(120), GUILayout.MaxWidth(120));
                    checkChange(ref _targetRT.splat_layer_ordered_mode, EditorGUILayout.Toggle(_targetRT.splat_layer_ordered_mode));
                    EditorGUILayout.EndHorizontal();
                    if (GUILayout.Button("Render Control maps from source splats"))
                    {
                        if (EditorUtility.DisplayDialog("Warning", "This will overwrite current splat controlmaps\n(you may backup them using save feature in \"Splatmaps...\" unfold below). Are you sure ?", "Yes", "Cancel"))
                        {
                            if (terrainComp)
                            {
                                _targetRT.RecalcControlMaps();
                            }
                            else
                            {
                                _targetRT.RecalcControlMapsForMesh();
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();
        #endregion Control maps - compose maps
                    break;
                case ReliefTerrainControlTexturesItems.Acquire:
        #region Control maps - acquire mask textures
                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Acquire mask textures", EditorStyles.boldLabel);
                    GUI.color = skin_color;
                    if (terrainComp)
                    {
                        EditorGUILayout.HelpBox("Here you can get coverage textures taken from height, steepness or normal direction (relative to reference object's Z axis and Y component of compared direction vectors may be optionally skipped, if not - resultant texture might be considered lightmap when using light as ref direction object).\n\nProduced textures are esp. useful for users that don't use external terrain tools, but only heightmap as base for their work. Textures taken here may serve as splat sources (\"Compose splats\" tab above).\n\nRealistic heightmaps themselves might be acquired from sattelite data - one may google for that, you can also consider making heightmap from scratch using photoshop (use perlin noise for example).", MessageType.None, true);
                        if (GUILayout.Button("Save steepness"))
                        {
                            Texture2D tex = _targetRT.GetSteepnessHeightDirectionTexture(0);
                            if (tex)
                            {
                                SaveTexture(ref tex, ref _target.save_path_terrain_steepness, "terrain_steepnessMap.png", 0, TextureImporterCompression.Compressed, TextureImporterType.Default, false, false);
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("?", "Can't get steepness texture...", "OK");
                            }
                        }
                        if (GUILayout.Button("Save height"))
                        {
                            Texture2D tex = _targetRT.GetSteepnessHeightDirectionTexture(1);
                            if (tex)
                            {
                                SaveTexture(ref tex, ref _target.save_path_terrain_height, "terrain_heightMap.png", 0, TextureImporterCompression.Compressed, TextureImporterType.Default, false, false);
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("?", "Can't get height texture...", "OK");
                            }
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Direction reference", GUILayout.MaxWidth(116));
                        checkChange(ref _target.direction_object, (GameObject)EditorGUILayout.ObjectField(_target.direction_object, typeof(GameObject), true));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Skip y component", GUILayout.MaxWidth(116));
                        checkChange(ref _target.flat_dir_ref, EditorGUILayout.Toggle(_target.flat_dir_ref));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Flip direction", GUILayout.MaxWidth(116));
                        checkChange(ref _target.flip_dir_ref, EditorGUILayout.Toggle(_target.flip_dir_ref));
                        EditorGUILayout.EndHorizontal();
                        if (GUILayout.Button("Save direction"))
                        {
                            Texture2D tex = _targetRT.GetSteepnessHeightDirectionTexture(2, _target.direction_object);
                            if (tex)
                            {
                                SaveTexture(ref tex, ref _target.save_path_terrain_direction, "terrain_directionMap.png", 0, TextureImporterCompression.Compressed, TextureImporterType.Default, false, false);
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("?", "Can't get direction texture...", "OK");
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Option available for terrains only.", MessageType.Warning, true);
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
        #endregion Control maps - acquire mask textures					
                    break;
                case ReliefTerrainControlTexturesItems.Controlmaps:
        #region Control maps - splats					
                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Splatmaps (control maps)", EditorStyles.boldLabel);
                    GUI.color = skin_color;
                    EditorGUILayout.HelpBox("Here you can see control maps as they are stored and used by Unity terrain system. For terrains you can backup them using save feature. You can drag&drop (select) such splatmaps (ARGB32) to override existing one(s) - for example when you need to restore them from backup or need to apply your own on terrain.", MessageType.None, true);
                    if (terrainComp)
                    {
                        if (GUILayout.Button("Refresh Controlmap(s) shown from TerrainData"))
                        {
                            _targetRT.GetControlMaps();
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Splat 0-3", GUILayout.MaxWidth(65));
                    if (terrainComp != null)
                    {
                        bool tmp_dirtyFlag = dirtyFlag;
                        checkChange(ref _targetRT.controlA, (Texture2D)EditorGUILayout.ObjectField(_targetRT.controlA, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                        if (dirtyFlag && !tmp_dirtyFlag)
                        {
                            AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_targetRT.controlA));
                            if (_importer)
                            {
                                TextureImporter tex_importer = (TextureImporter)_importer;
                                if (!tex_importer.isReadable)
                                {
                                    Debug.LogWarning("Control texture 0 (" + _targetRT.controlA.name + ") has been reimported as readable.");
                                    tex_importer.isReadable = true;
                                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_targetRT.controlA), ImportAssetOptions.ForceUpdate);
                                }
                            }
                            _targetRT.SetCustomControlMaps();
                        }
                    }
                    else
                    {
                        checkChange(ref _targetRT.controlA, (Texture2D)EditorGUILayout.ObjectField(_targetRT.controlA, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                    }
                    if (_target.numLayers > 4)
                    {
                        EditorGUILayout.LabelField("Splat 4-7", GUILayout.MaxWidth(65));
                        if (terrainComp != null)
                        {
                            bool tmp_dirtyFlag = dirtyFlag;
                            checkChange(ref _targetRT.controlB, (Texture2D)EditorGUILayout.ObjectField(_targetRT.controlB, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                            if (dirtyFlag && !tmp_dirtyFlag)
                            {
                                AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_targetRT.controlB));
                                if (_importer)
                                {
                                    TextureImporter tex_importer = (TextureImporter)_importer;
                                    if (!tex_importer.isReadable)
                                    {
                                        Debug.LogWarning("Control texture 1 (" + _targetRT.controlB.name + ") has been reimported as readable.");
                                        tex_importer.isReadable = true;
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_targetRT.controlB), ImportAssetOptions.ForceUpdate);
                                    }
                                }
                                _targetRT.SetCustomControlMaps();
                            }
                        }
                        else
                        {
                            checkChange(ref _targetRT.controlB, (Texture2D)EditorGUILayout.ObjectField(_targetRT.controlB, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Save to file", GUILayout.MinWidth(170), GUILayout.MaxWidth(170)))
                    {
                        Texture2D tex = _targetRT.controlA;
                        if (tex)
                        {
                            SaveTexture(ref tex, ref _targetRT.save_path_controlA, "terrain_splatMapA.png", 0, TextureImporterCompression.Uncompressed, TextureImporterType.Default, true, false, true);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("?", "Can't get height texture...", "OK");
                        }
                    }
                    if (_target.numLayers > 4)
                    {
                        if (GUILayout.Button("Save to file", GUILayout.MinWidth(170), GUILayout.MaxWidth(170)))
                        {
                            Texture2D tex = _targetRT.controlB;
                            if (tex)
                            {
                                SaveTexture(ref tex, ref _targetRT.save_path_controlB, "terrain_splatMapB.png", 0, TextureImporterCompression.Uncompressed, TextureImporterType.Default, true, false, true);
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("?", "Can't get height texture...", "OK");
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (_target.numLayers > 8)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Splat 8-11", GUILayout.MaxWidth(65));
                        if (terrainComp != null)
                        {
                            bool tmp_dirtyFlag = dirtyFlag;
                            checkChange(ref _targetRT.controlC, (Texture2D)EditorGUILayout.ObjectField(_targetRT.controlC, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                            if (dirtyFlag && !tmp_dirtyFlag)
                            {
                                AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_targetRT.controlC));
                                if (_importer)
                                {
                                    TextureImporter tex_importer = (TextureImporter)_importer;
                                    if (!tex_importer.isReadable)
                                    {
                                        Debug.LogWarning("Control texture 2 (" + _targetRT.controlC.name + ") has been reimported as readable.");
                                        tex_importer.isReadable = true;
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_targetRT.controlC), ImportAssetOptions.ForceUpdate);
                                    }
                                }
                                _targetRT.SetCustomControlMaps();
                            }
                        }
                        else
                        {
                            checkChange(ref _targetRT.controlC, (Texture2D)EditorGUILayout.ObjectField(_targetRT.controlC, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (_target.numLayers > 8)
                    {
                        if (GUILayout.Button("Save to file", GUILayout.MinWidth(170), GUILayout.MaxWidth(170)))
                        {
                            Texture2D tex = _targetRT.controlC;
                            if (tex)
                            {
                                SaveTexture(ref tex, ref _targetRT.save_path_controlC, "terrain_splatMapC.png", 0, TextureImporterCompression.Uncompressed, TextureImporterType.Default, true, false, true);
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("?", "Can't get height texture...", "OK");
                            }
                        }
                    }
        #endregion Control maps - splats					
                    EditorGUILayout.EndVertical();
                    break;
            }

        #endregion Control maps
        }

        //_target.show_heightmaps=EditorGUILayout.Foldout(_target.show_heightmaps, "Height maps (combined)");
        if (_target.show_derivedmaps)
        {
        #region Derived textures
            Color skin_color;
            toolbarStrings = new string[4] { "Atlasing", "Height maps", "Normal maps", "Special" };
            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            _target.submenu_derived_textures = (ReliefTerrainDerivedTexturesItems)(GUILayout.Toolbar((int)_target.submenu_derived_textures, toolbarStrings,
#if !UNITY_2019_3_OR_NEWER
            EditorStyles.miniButton,
#endif
                GUILayout.MaxWidth(370)));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);
            EditorGUILayout.HelpBox("Combined texures are what actually feeds shader. They're intended to be reused on multiple terrain tiles. You can also reuse them across scenes.", MessageType.Warning, true);
            switch (_target.submenu_derived_textures)
            {
                case ReliefTerrainDerivedTexturesItems.Atlasing:
        #region Derived textures - Atlasing features
                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Color atlases", EditorStyles.boldLabel);
                    GUI.color = skin_color;

                    EditorGUI.BeginDisabledGroup((!atlasA_needed) && (!atlasB_needed) && (!atlasC_needed));
                    if (GUILayout.Button("Prepare atlases from detail textures (may take a while)"))
                    {
                        if (atlasA_needed) PrepareAtlases(0);
                        if (atlasB_needed) PrepareAtlases(1);
                        if (atlasC_needed) PrepareAtlases(2);
                        _target.Refresh(null, _targetRT);
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.BeginHorizontal();
                    if (_target.numLayers > 0)
                    {
                        EditorGUI.BeginDisabledGroup(!atlasA_needed);
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Layers 0-3", GUILayout.Width(100));
                        Texture2D oRef = _target.splat_atlases[0];
                        if (checkChange(ref _target.splat_atlases[0], (Texture2D)EditorGUILayout.ObjectField(_target.splat_atlases[0], typeof(Texture2D), false, GUILayout.Height(100), GUILayout.Width(100))))
                        {
                            DestroyUnusedCombinedTexture(oRef);
                        }
                        if (SaveTexture(ref _target.splat_atlases[0], ref _target.save_path_atlasA, "atlas_texture_layers_0_to_3.png", 100, TextureImporterCompression.Compressed, TextureImporterType.Default, true))
                        {
                            string path = AssetDatabase.GetAssetPath(_target.splat_atlases[0]);
                            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                            textureImporter.wrapMode = TextureWrapMode.Clamp;
                            textureImporter.filterMode = FilterMode.Trilinear;
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUI.EndDisabledGroup();
                    }

                    if (_target.numLayers > 4)
                    {
                        EditorGUI.BeginDisabledGroup(!atlasB_needed);
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Layers 4-7", GUILayout.Width(100));
                        Texture2D oRef = _target.splat_atlases[1];
                        if (checkChange(ref _target.splat_atlases[1], (Texture2D)EditorGUILayout.ObjectField(_target.splat_atlases[1], typeof(Texture2D), false, GUILayout.Height(100), GUILayout.Width(100))))
                        {
                            DestroyUnusedCombinedTexture(oRef);
                        }
                        if (SaveTexture(ref _target.splat_atlases[1], ref _target.save_path_atlasB, "atlas_texture_layers_4_to_7.png", 100, TextureImporterCompression.Compressed, TextureImporterType.Default, true))
                        {
                            _target.splat_atlases[1].wrapMode = TextureWrapMode.Clamp;
                            _target.splat_atlases[1].filterMode = FilterMode.Trilinear;
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUI.EndDisabledGroup();
                    }

                    EditorGUILayout.EndHorizontal();

                    if (_target.numLayers > 8)
                    {
                        EditorGUI.BeginDisabledGroup(!atlasC_needed);
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Layers 9-11", GUILayout.Width(100));
                        Texture2D oRef = _target.splat_atlases[2];
                        if (checkChange(ref _target.splat_atlases[2], (Texture2D)EditorGUILayout.ObjectField(_target.splat_atlases[2], typeof(Texture2D), false, GUILayout.Height(100), GUILayout.Width(100))))
                        {
                            DestroyUnusedCombinedTexture(oRef);
                        }
                        if (SaveTexture(ref _target.splat_atlases[2], ref _target.save_path_atlasC, "atlas_texture_layers_8_to_11.png", 100, TextureImporterCompression.Compressed, TextureImporterType.Default, true))
                        {
                            _target.splat_atlases[2].wrapMode = TextureWrapMode.Clamp;
                            _target.splat_atlases[2].filterMode = FilterMode.Trilinear;
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUI.EndDisabledGroup();
                    }

                    EditorGUILayout.EndVertical();
        #endregion Derived textures - Atlasing features		
                    break;
                case ReliefTerrainDerivedTexturesItems.Heightmaps:
        #region Derived textures - Heightmaps
                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Combined heightmaps", EditorStyles.boldLabel);
                    GUI.color = skin_color;

                    EditorGUILayout.HelpBox("If you specified heightmaps for your layers you don't have to do anything while editing (heightmaps are refreshed as you modify any of separate heightmap). However, you might need to specify combined heightmaps by hand, so you can do it here (don't touch particular heightmaps in a layer properties then).", MessageType.None, true);
                    EditorGUILayout.HelpBox("Every channel of height textures below stores heightmap for consecutive detail map (splats 0-3 -> heightmap channels RGBA). To generate such channel combined heightmaps you can also use attached tool (Window / Relief Tools / 4 to 1 texture channels mixer).", MessageType.None, true);

                    EditorGUILayout.BeginHorizontal();

                    if (_target.numLayers > 0)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Heights 0-3", GUILayout.Width(80));
                        Texture2D oRef = _target.HeightMap;
                        if (checkChange(ref _target.HeightMap, (Texture2D)EditorGUILayout.ObjectField(_target.HeightMap, typeof(Texture2D), false, GUILayout.Height(80), GUILayout.Width(80))))
                        {
                            DestroyUnusedCombinedTexture(oRef);
                        }
                        SaveTexture(ref _target.HeightMap, ref _target.save_path_HeightMap, "heightmap_layers_0_to_3.png", 80, TextureImporterCompression.Compressed, TextureImporterType.Default, true, true, true);
                        EditorGUILayout.EndVertical();
                    }

                    if (_target.numLayers > 4)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Heights 4-7", GUILayout.Width(80));
                        Texture2D oRef = _target.HeightMap2;
                        if (checkChange(ref _target.HeightMap2, (Texture2D)EditorGUILayout.ObjectField(_target.HeightMap2, typeof(Texture2D), false, GUILayout.Height(80), GUILayout.Width(80))))
                        {
                            DestroyUnusedCombinedTexture(oRef);
                        }
                        SaveTexture(ref _target.HeightMap2, ref _target.save_path_HeightMap2, "heightmap_layers_4_to_7.png", 80, TextureImporterCompression.Compressed, TextureImporterType.Default, true, true, true);
                        EditorGUILayout.EndVertical();
                    }

                    if (_target.numLayers > 8)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Heights 8-12", GUILayout.Width(80));
                        Texture2D oRef = _target.HeightMap3;
                        if (checkChange(ref _target.HeightMap3, (Texture2D)EditorGUILayout.ObjectField(_target.HeightMap3, typeof(Texture2D), false, GUILayout.Height(80), GUILayout.Width(80))))
                        {
                            DestroyUnusedCombinedTexture(oRef);
                        }
                        SaveTexture(ref _target.HeightMap3, ref _target.save_path_HeightMap3, "heightmap_layers_8_to_11.png", 80, TextureImporterCompression.Compressed, TextureImporterType.Default, true, true, true);
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
        #endregion Derived textures - Heightmaps
                    break;
                case ReliefTerrainDerivedTexturesItems.Bumpmaps:
        #region Derived textures - Bumpmaps
                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Combined bumpmaps", EditorStyles.boldLabel);
                    GUI.color = skin_color;

                    EditorGUILayout.HelpBox("Each layer pair share the same normal texture (RG + BA).They're automatically composed/refreshed when you change bump map in a layer. However, you might need to specify combined bumpmaps by hand, so you can do it here (don't touch particular bumpmaps in a layer properties then).", MessageType.None, true);

                    EditorGUILayout.BeginHorizontal();
                    if (_target.numLayers > 0)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Bumps 0+1", GUILayout.Width(80));
                        Texture2D oRef = _target.Bump01;
                        if (checkChange(ref _target.Bump01, (Texture2D)EditorGUILayout.ObjectField(_target.Bump01, typeof(Texture2D), false, GUILayout.Height(80), GUILayout.Width(80))))
                        {
                            DestroyUnusedCombinedTexture(oRef);
                        }
                        SaveTexture(ref _target.Bump01, ref _target.save_path_Bump01, "bumpmap_layers01.png", 80, TextureImporterCompression.Uncompressed, TextureImporterType.Default, true, true, true);
                        EditorGUILayout.EndVertical();
                    }

                    if (_target.numLayers > 2)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Bumps 2+3", GUILayout.Width(80));
                        Texture2D oRef = _target.Bump23;
                        if (checkChange(ref _target.Bump23, (Texture2D)EditorGUILayout.ObjectField(_target.Bump23, typeof(Texture2D), false, GUILayout.Height(80), GUILayout.Width(80))))
                        {
                            DestroyUnusedCombinedTexture(oRef);
                        }
                        SaveTexture(ref _target.Bump23, ref _target.save_path_Bump23, "bumpmap_layers23.png", 80, TextureImporterCompression.Uncompressed, TextureImporterType.Default, true, true, true);
                        EditorGUILayout.EndVertical();
                    }

                    if (_target.numLayers > 4)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField("Bumps 4+5", GUILayout.Width(80));
                        Texture2D oRef = _target.Bump45;
                        if (checkChange(ref _target.Bump45, (Texture2D)EditorGUILayout.ObjectField(_target.Bump45, typeof(Texture2D), false, GUILayout.Height(80), GUILayout.Width(80))))
                        {
                            DestroyUnusedCombinedTexture(oRef);
                        }
                        SaveTexture(ref _target.Bump45, ref _target.save_path_Bump45, "bumpmap_layers45.png", 80, TextureImporterCompression.Uncompressed, TextureImporterType.Default, true, true, true);
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(10);

                    if (_target.numLayers > 6)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (_target.numLayers > 6)
                        {
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.LabelField("Bumps 6+7", GUILayout.Width(80));
                            Texture2D oRef = _target.Bump67;
                            if (checkChange(ref _target.Bump67, (Texture2D)EditorGUILayout.ObjectField(_target.Bump67, typeof(Texture2D), false, GUILayout.Height(80), GUILayout.Width(80))))
                            {
                                DestroyUnusedCombinedTexture(oRef);
                            }
                            SaveTexture(ref _target.Bump67, ref _target.save_path_Bump67, "bumpmap_layers67.png", 80, TextureImporterCompression.Uncompressed, TextureImporterType.Default, true, true, true);
                            EditorGUILayout.EndVertical();
                        }

                        if (_target.numLayers > 8)
                        {
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.LabelField("Bumps 8+9", GUILayout.Width(80));
                            Texture2D oRef = _target.Bump89;
                            if (checkChange(ref _target.Bump89, (Texture2D)EditorGUILayout.ObjectField(_target.Bump89, typeof(Texture2D), false, GUILayout.Height(80), GUILayout.Width(80))))
                            {
                                DestroyUnusedCombinedTexture(oRef);
                            }
                            SaveTexture(ref _target.Bump89, ref _target.save_path_Bump89, "bumpmap_layers89.png", 80, TextureImporterCompression.Uncompressed, TextureImporterType.Default, true, true, true);
                            EditorGUILayout.EndVertical();
                        }

                        if (_target.numLayers > 10)
                        {
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.LabelField("Bumps 10+11", GUILayout.Width(80));
                            Texture2D oRef = _target.BumpAB;
                            if (checkChange(ref _target.BumpAB, (Texture2D)EditorGUILayout.ObjectField(_target.BumpAB, typeof(Texture2D), false, GUILayout.Height(80), GUILayout.Width(80))))
                            {
                                DestroyUnusedCombinedTexture(oRef);
                            }
                            SaveTexture(ref _target.BumpAB, ref _target.save_path_BumpAB, "bumpmap_layersAB.png", 80, TextureImporterCompression.Uncompressed, TextureImporterType.Default, true, true, true);
                            EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(10);
                    }

                    EditorGUILayout.EndVertical();
        #endregion Derived textures - Bumpmaps
                    break;
                case ReliefTerrainDerivedTexturesItems.Globalnormal:
        #region Derived textures - Global normal
                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Perlin normal + superdetail / water&reflection", EditorStyles.boldLabel);
                    GUI.color = skin_color;

                    EditorGUILayout.HelpBox("Texture below use perlin normal map specified on channels RG.\n\nChannels BA are used for superdetail additional channels or (mutually exclusive) wet mask (B) and reflection map (A).", MessageType.None, true);

                    //EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("Texture", GUILayout.Width(80));
                    bool sync_tex = false;
                    Texture2D oRef2 = _targetRT.BumpGlobalCombined;
                    if (checkChange(ref _targetRT.BumpGlobalCombined, (Texture2D)EditorGUILayout.ObjectField(_targetRT.BumpGlobalCombined, typeof(Texture2D), false, GUILayout.Height(100), GUILayout.Width(100))))
                    {
                        DestroyUnusedCombinedTexture(oRef2);
                        sync_tex = true;
                    }
                    GUI.color = new Color(0.5f, 1, 0.5f, 1);
                    if (SaveTexture(ref _targetRT.BumpGlobalCombined, ref _targetRT.save_path_BumpGlobalCombined, "perlin_normal_aux.png", 100, TextureImporterCompression.Compressed, TextureImporterType.Default, true, true, true)) sync_tex = true;
                    GUI.color = skin_color;
                    if (sync_tex)
                    {
                        RTP_LODmanager manager = _target.Get_RTP_LODmanagerScript();
                        if (manager && !manager.RTP_WETNESS_FIRST && !manager.RTP_WETNESS_ADD)
                        {
                            ReliefTerrain[] objs = (ReliefTerrain[])GameObject.FindObjectsOfType(typeof(ReliefTerrain));
                            for (int i = 0; i < objs.Length; i++)
                            {
                                objs[i].BumpGlobalCombined = _targetRT.BumpGlobalCombined;
                                objs[i].globalCombinedModifed_flag = false;
                            }
                        }
                        _targetRT.globalCombinedModifed_flag = false;
                    }
                    //EditorGUILayout.EndVertical();

                    EditorGUILayout.EndVertical();
        #endregion Derived textures - Global normal
                    break;
            } // derived textures submenu switch
        #endregion Derived textures
        }

        if (_target.show_settings)
        {
        #region ALLSettings
            GUIContent[] toolbarIcons1 = new GUIContent[5] { new GUIContent("Main", icoLayersSmall), new GUIContent("UV blend", icoUVBlend), new GUIContent("Global\nmaps", icoGlobalcolor), new GUIContent("Perlin\nnormal", icoPerlinNormal), new GUIContent("S. detail", icoSuperdetail) };
            GUIContent[] toolbarIcons2 = new GUIContent[5] { new GUIContent("Parallax", icoPOM), new GUIContent("Vertical\ntexture", icoVerticalTexture), new GUIContent("Snow", icoSnow), new GUIContent("Water &\ncaustic", icoWater), new GUIContent("Glitter", icoGlitter) };
            ReliefTerrainSettingsItems prev_submenu_settings = _target.submenu_settings;
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            _target.submenu_settings = (ReliefTerrainSettingsItems)(GUILayout.Toolbar((int)_target.submenu_settings, toolbarIcons1,
#if !UNITY_2019_3_OR_NEWER
            EditorStyles.miniButton,
#endif
                GUILayout.MaxWidth(370)));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            _target.submenu_settings = (ReliefTerrainSettingsItems)(GUILayout.Toolbar((int)_target.submenu_settings - 5, toolbarIcons2,
#if !UNITY_2019_3_OR_NEWER
            EditorStyles.miniButton,
#endif
               GUILayout.MaxWidth(370)) + 5);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (prev_submenu_settings != _target.submenu_settings && _target.paint_flag)
            {
                _target.paint_flag = false;
                UnityEditor.Tools.current = prev_tool;
                RemoveSceneViewCallback();
                EditorUtility.SetDirty(target);
            }

            Color skin_color;
            switch (_target.submenu_settings)
            {
                case ReliefTerrainSettingsItems.MainSettings:
        #region Settings	 - main settings

                    GUILayout.Space(6);
                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoLayers), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Main settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;
                    EditorGUILayout.HelpBox("For performance reasons shader uses the same tiling for every detail map.", MessageType.None, true);
                    //if (RTP_INDEPENDENT_TILING)
                    //{
                    //    EditorGUILayout.HelpBox("You've got enabled independent tiling for every terrain object", MessageType.Warning, true);
                    //    checkChange(ref _targetRT.customTiling.x, EditorGUILayout.FloatField("Tile Size X [m]", _targetRT.customTiling.x));
                    //    checkChange(ref _targetRT.customTiling.y, EditorGUILayout.FloatField("Tile Size Y [m]", _targetRT.customTiling.y));
                    //}
                    //else
                    {
                        checkChange(ref _target.ReliefTransform.x, EditorGUILayout.FloatField("Tile Size X [m]", _target.ReliefTransform.x));
                        //						EditorGUILayout.BeginHorizontal();
                        //						checkChange(ref _target.ReliefTransform.x, _target.terrainTileSizeX/EditorGUILayout.FloatField("Tile Size X [m] ("+(Mathf.Round(_target.ReliefTransform.x*100)/100)+")", _target.terrainTileSizeX/_target.ReliefTransform.x));
                        //						if (GUILayout.Button(new GUIContent("A","Align to terrain tile"), GUILayout.Width(22), GUILayout.Height(16))) {
                        //							float ratio=_target.ReliefTransform.x/_target.ReliefTransform.y;
                        //							_target.ReliefTransform.x=Mathf.Round(_target.ReliefTransform.x);
                        //							_target.ReliefTransform.y=_target.ReliefTransform.x/ratio;
                        //							_target.ReliefTransform.y=Mathf.Round(_target.ReliefTransform.y);
                        //							_target.RefreshAll();
                        //						}
                        //						EditorGUILayout.EndHorizontal();

                        checkChange(ref _target.ReliefTransform.y, EditorGUILayout.FloatField("Tile Size Y [m]", _target.ReliefTransform.y));
                        //						EditorGUILayout.BeginHorizontal();					
                        //						checkChange(ref _target.ReliefTransform.y, _target.terrainTileSizeZ/EditorGUILayout.FloatField("Tile Size Y [m] ("+(Mathf.Round(_target.ReliefTransform.y*100)/100)+")", _target.terrainTileSizeZ/_target.ReliefTransform.y));
                        //						if (GUILayout.Button(new GUIContent("A","Align to terrain tile"), GUILayout.Width(22), GUILayout.Height(16))) {
                        //							float ratio=_target.ReliefTransform.x/_target.ReliefTransform.y;
                        //							_target.ReliefTransform.y=Mathf.Round(_target.ReliefTransform.y);
                        //							_target.ReliefTransform.x=_target.ReliefTransform.y*ratio;
                        //							_target.ReliefTransform.x=Mathf.Round(_target.ReliefTransform.x);
                        //							_target.RefreshAll();
                        //						}
                        //						EditorGUILayout.EndHorizontal();
                        float tile_sizex = _target.ReliefTransform.x;
                        float tile_sizey = _target.ReliefTransform.y;
                        checkChange(ref _target.ReliefTransform.z, EditorGUILayout.FloatField("Tile Offset X [m]", _target.ReliefTransform.z * tile_sizex) / tile_sizex);
                        checkChange(ref _target.ReliefTransform.w, EditorGUILayout.FloatField("Tile Offset Y [m]", _target.ReliefTransform.w * tile_sizey) / tile_sizey);
                        //						float tile_sizex=_target.terrainTileSizeX/_target.ReliefTransform.x;
                        //						float tile_sizey=_target.terrainTileSizeZ/_target.ReliefTransform.y;
                        //						checkChange(ref _target.ReliefTransform.z, EditorGUILayout.FloatField("Tile Offset X [m]", _target.ReliefTransform.z*tile_sizex)/tile_sizex);
                        //						checkChange(ref _target.ReliefTransform.w, EditorGUILayout.FloatField("Tile Offset Y [m]", _target.ReliefTransform.w*tile_sizey)/tile_sizey);
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Near distance values");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
                    EditorGUILayout.BeginVertical();
                    float prev_distance_start = _target.distance_start;
                    float prev_distance_transition = _target.distance_transition;
                    checkChange(ref _target.distance_start, EditorGUILayout.Slider("Distance start", _target.distance_start, 0, 50));
                    if (_target.distance_start_bumpglobal < _target.distance_start) _target.distance_start_bumpglobal = _target.distance_start;
                    checkChange(ref _target.distance_transition, EditorGUILayout.Slider("Fade length", _target.distance_transition, 0, 50));
                    //if (_target.useTerrainMaterial)
                    {
                        if (prev_distance_start != _target.distance_start || prev_distance_transition != _target.distance_transition)
                        {
                            Terrain[] terrainComps = (Terrain[])GameObject.FindObjectsOfType(typeof(Terrain));
                            for (int ter = 0; ter < terrainComps.Length; ter++)
                            {
                                terrainComps[ter].basemapDistance = _target.distance_start + _target.distance_transition;
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Note that some texture lookups are MIP bias independent (use regular tex2D call)", MessageType.None, true);
                    checkChange(ref _target.RTP_MIP_BIAS, EditorGUILayout.Slider("MIP level bias", _target.RTP_MIP_BIAS, -0.75f, 0.75f));

                    checkChange(ref _target._occlusionStrength, EditorGUILayout.Slider("Approximated occlusion", _target._occlusionStrength, 0f, 1f));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Shading color adjustements", EditorStyles.boldLabel);
                    GUI.color = skin_color;
                    EditorGUILayout.BeginVertical("Box");
                    checkChange(ref _target.MasterLayerBrightness, EditorGUILayout.Slider("Layers detail brightness", _target.MasterLayerBrightness, 0, 2));
                    checkChange(ref _target.MasterLayerSaturation, EditorGUILayout.Slider("Layers detail saturation", _target.MasterLayerSaturation, 0, 2));

                    EditorGUILayout.EndVertical();

                    bool showLightmapShading = false;
                    if (terrainComp)
                    {
                        if ((terrainComp.lightmapIndex != 65535) && (terrainComp.lightmapIndex != 65534) && LightmapSettings.lightmapsMode == LightmapsMode.NonDirectional)
                        {
                            showLightmapShading = true;
                        }
                    }
                    else
                    {
                        if ((_targetRT.GetComponent<Renderer>().lightmapIndex != 65535) && (_targetRT.GetComponent<Renderer>().lightmapIndex != 65534) && LightmapSettings.lightmapsMode != LightmapsMode.NonDirectional)
                        {
                            showLightmapShading = true;
                        }
                    }
                    if (showLightmapShading)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginVertical("Box");
                        EditorGUILayout.HelpBox("Helpful when lightmapping in single mode. Shader can mimic diffuse lighting with normal maps used. In dual lightmap mode (deferred) this is applied only beyond near distance, so you need to sync its values with realtime shadows distance/fade.", MessageType.None, true);
                        checkChange(ref _target.LightmapShading, EditorGUILayout.Slider("Shading strength for lightmaps", _target.LightmapShading, 0, 2));
                        EditorGUILayout.EndVertical();
                    }

                    if (RTP_HEIGHTBLEND_AO_FIRST || RTP_HEIGHTBLEND_AO_ADD)
                    {
                        EditorGUILayout.Space();
                        GUI.color = new Color(1, 1, 0.5f, 1);
                        EditorGUILayout.LabelField("Heightblend AO", EditorStyles.boldLabel);
                        GUI.color = skin_color;
                        EditorGUILayout.BeginVertical("Box");
                        checkChange(ref _target.RTP_AOsharpness, EditorGUILayout.Slider("Fake AO 2 HB sharpness", _target.RTP_AOsharpness, 0, 10));
                        checkChange(ref _target.RTP_AOamp, EditorGUILayout.Slider("Fake AO 2 HB value", _target.RTP_AOamp, 0, 2));
                        EditorGUILayout.EndVertical();
                    }
                    if ((RTP_HEIGHTBLEND_AO_FIRST && RTP_HOTAIR_EMISSION_FIRST) || (RTP_HEIGHTBLEND_AO_ADD && RTP_HOTAIR_EMISSION_ADD))
                    {
                        EditorGUILayout.Space();
                        GUI.color = new Color(1, 1, 0.5f, 1);
                        EditorGUILayout.LabelField("Hot air refraction on emissive layers", EditorStyles.boldLabel);
                        GUI.color = skin_color;
                        EditorGUILayout.BeginVertical("Box");
                        checkChange(ref _target.EmissionRefractFiltering, EditorGUILayout.Slider("Refraction filtering", _target.EmissionRefractFiltering, 0, 8));
                        checkChange(ref _target.EmissionRefractAnimSpeed, EditorGUILayout.Slider("Refraction anim speed", _target.EmissionRefractAnimSpeed, 0, 20));
                        EditorGUILayout.EndVertical();
                    }

                    // presets
                    EditorGUILayout.Space();
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
                    GUI.color = skin_color;
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.HelpBox("Here you can save / restore object state in preset. You can also use use function InterpolatePresets(PresetID1, PresetID2, float t) via script to blend between numeric settings of 2 presets.", MessageType.Info, true);
                    if (_targetRT.presetHolders != null)
                    {
                        for (int k = 0; k < _targetRT.presetHolders.Length; k++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            ReliefTerrainPresetHolder[] holders = null;

                            if (_targetRT.presetHolders[k] != null)
                            {
                                if (GUILayout.Button("restore", GUILayout.Width(60)))
                                {
                                    if (EditorUtility.DisplayDialog("Warning", "Restore to preset " + _targetRT.presetHolders[k].PresetName + " state ?", "Yes", "Cancel"))
                                    {
                                        Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");

                                        if (terrainComp && (_targetRT.presetHolders[k].terrainLayers == null || _targetRT.presetHolders[k].terrainLayers.Length == 0))
                                        {
                                            // U2018.3 upgrade
                                            _targetRT.presetHolders[k].terrainLayers = new TerrainLayer[terrainComp.terrainData.terrainLayers.Length];
                                            System.Array.Copy(terrainComp.terrainData.terrainLayers, _targetRT.presetHolders[k].terrainLayers, terrainComp.terrainData.terrainLayers.Length);
                                        }


                                        _targetRT.RestorePreset(_targetRT.presetHolders[k]);
                                        _targetRT.RefreshTextures();
                                        _target.Refresh(null, _targetRT);
                                        EditorUtility.SetDirty(_targetRT);
                                    }
                                }
                                GUI.color = new Color(1, 0.4f, 0.2f, 1);
                                if (GUILayout.Button("del", GUILayout.Width(35)))
                                {
                                    if (EditorUtility.DisplayDialog("Warning", "Remove preset " + _targetRT.presetHolders[k].PresetName + " ?", "Yes", "Cancel"))
                                    {
                                        Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                                        holders = new ReliefTerrainPresetHolder[_targetRT.presetHolders.Length - 1];
                                        int l = 0;
                                        for (int r = 0; r < _targetRT.presetHolders.Length; r++)
                                        {
                                            if (r != k)
                                            {
                                                holders[l++] = _targetRT.presetHolders[r];
                                            }
                                        }
                                        _targetRT.presetHolders = holders;
                                        break;
                                    }
                                }
                                GUI.color = new Color(0.5f, 1, 0.5f, 1);
                                if (GUILayout.Button("save", GUILayout.Width(45)))
                                {
                                    string path = EditorUtility.SaveFilePanel("Save preset", "Assets", "RTPpreset", "asset");
                                    if (path != "")
                                    {
                                        int idx = path.IndexOf("/Assets/") + 1;
                                        if (idx > 0)
                                        {
                                            path = path.Substring(idx);
                                            ReliefTerrainPresetHolder savedPreset = UnityEngine.Object.Instantiate(_targetRT.presetHolders[k]) as ReliefTerrainPresetHolder;
                                            Debug.Log("RTP " + _targetRT.presetHolders[k].name + " preset saved at " + path);
                                            if (AssetDatabase.LoadAssetAtPath(path, typeof(ReliefTerrainPresetHolder)) != null) AssetDatabase.DeleteAsset(path);
                                            AssetDatabase.CreateAsset(savedPreset, path);
                                            //_targetRT.presetHolders[k].CheckUnsavedTextures();
                                        }
                                        else
                                        {
                                            Debug.Log("Nothing saved...");
                                        }
                                    }
                                }
                                GUI.color = skin_color;
                                EditorGUILayout.LabelField("Preset " + k + ": ", GUILayout.Width(60));
                                EditorGUILayout.LabelField(_targetRT.presetHolders[k].PresetName, EditorStyles.boldLabel);

                                if (holders != null)
                                {
                                    _targetRT.presetHolders = holders;
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUI.BeginDisabledGroup(_targetRT.presetHolders != null && _targetRT.presetHolders.Length == 8);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("New preset name: ", GUILayout.Width(120));
                    _target.newPresetName = EditorGUILayout.TextField(_target.newPresetName);
                    if (GUILayout.Button("+ Add new preset"))
                    {
                        Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                        ReliefTerrainPresetHolder[] holders;
                        int k = 0;
                        if (_targetRT.presetHolders != null)
                        {
                            holders = new ReliefTerrainPresetHolder[_targetRT.presetHolders.Length + 1];
                            for (; k < _targetRT.presetHolders.Length; k++)
                            {
                                holders[k] = _targetRT.presetHolders[k];
                            }
                        }
                        else
                        {
                            holders = new ReliefTerrainPresetHolder[1];
                        }
                        holders[k] = ScriptableObject.CreateInstance(typeof(ReliefTerrainPresetHolder)) as ReliefTerrainPresetHolder;
                        holders[k].Init(_target.newPresetName);
                        _targetRT.SavePreset(ref holders[k]);
                        holders[k].type = (terrainComp != null) ? "TERRAIN" : "MESH";
                        _targetRT.presetHolders = holders;
                    }
                    EditorGUILayout.EndHorizontal();
                    if (GUILayout.Button("Load & restore saved preset"))
                    {
                        string path = EditorUtility.OpenFilePanel("Load preset", "Assets", "asset");
                        if (path != "")
                        {
                            int idx = path.IndexOf("/Assets/") + 1;
                            if (idx > 0)
                            {
                                path = path.Substring(idx);
                            }
                            ReliefTerrainPresetHolder loadedPreset = AssetDatabase.LoadAssetAtPath(path, typeof(ReliefTerrainPresetHolder)) as ReliefTerrainPresetHolder;
                            if (loadedPreset != null)
                            {
                                if (loadedPreset.numLayers == _target.numLayers)
                                {
                                    if ((loadedPreset.type == "TERRAIN" && terrainComp != null) || (loadedPreset.type == "MESH" && terrainComp == null))
                                    {
                                        // to not deal with assets on disk but only with instances in mem saved in scene
                                        ReliefTerrainPresetHolder loadedPresetCopy = UnityEngine.Object.Instantiate(loadedPreset) as ReliefTerrainPresetHolder;

                                        if (terrainComp && (loadedPresetCopy.terrainLayers==null || loadedPresetCopy.terrainLayers.Length==0))
                                        {
                                            // U2018.3 upgrade
                                            loadedPresetCopy.terrainLayers = new TerrainLayer[terrainComp.terrainData.terrainLayers.Length];
                                            System.Array.Copy(terrainComp.terrainData.terrainLayers, loadedPresetCopy.terrainLayers, loadedPresetCopy.terrainLayers.Length);
                                        }

                                        int dgres = EditorUtility.DisplayDialogComplex("RTP Notification", "What would you like to restore current terrain object specific textures, too ?", "Yes", "No, only globals", "Cancel");
                                        if (dgres != 2)
                                        {
                                            Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                                            if (dgres == 0)
                                            {
                                                _targetRT.RestorePreset(loadedPresetCopy);
                                            }
                                            else
                                            {
                                                _target.RestorePreset(loadedPresetCopy);
                                            }
                                            _target.RefreshAll();
                                        }
                                    }
                                    else
                                    {
                                        EditorUtility.DisplayDialog("Error", "Mismatched type - you can't load RTP preset that has been taken from mesh when you're using terrain and vice versa.", "OK");
                                    }
                                }
                                else
                                {
                                    EditorUtility.DisplayDialog("Error", "Number of layers in preset being loaded (" + loadedPreset.numLayers + ") and in current setup (" + _target.numLayers + ") differs.", "OK");
                                }
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Error", "No RTP preset available at specified location.", "OK");
                            }
                        }
                    }
                    if (GUILayout.Button("Clear all presets"))
                    {
                        if (EditorUtility.DisplayDialog("Warning", "Remove all presets (can NOT be undone) ?", "Yes", "Cancel"))
                        {
                            _targetRT.presetHolders = null;
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndVertical();
                    // EOF presets

                    GUILayout.Space(10);

                    {
                        Color c = GUI.color;
                        GUI.color = new Color(0.9f, 1, 0.9f);
                        if (GUILayout.Button(new GUIContent("Refresh all", "Refreshes all terrain object on the scene."), GUILayout.Height(40)))
                        {
                            _target.RefreshAll();
                        }
                        GUI.color = c;
                    }
                    if (GUILayout.Button(new GUIContent("Purge scene", ""), GUILayout.Height(40)))
                    {
                        EditorUtility.UnloadUnusedAssetsImmediate();
                    }

                    GUILayout.Space(15);
                    if (GUILayout.Button("Reset main settings numeric params to default"))
                    {
                        Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                        _target.ReturnToDefaults("main");
                        EditorUtility.SetDirty(_targetRT);
                        _target.Refresh(null, _targetRT);
                    }

        #endregion Settings - main settings

                    break;

                case ReliefTerrainSettingsItems.UVblend:

        #region Detail maps unfold - UV blend
                    // UV blend on/off global properties
                    GUILayout.Space(6);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoUVBlend), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("UV blending (tiling patterns reduction)", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;
                    if (UV_BLEND_ENABLED_FIRST || UV_BLEND_ENABLED_ADD)
                    {
                        EditorGUILayout.HelpBox("UV blend feature can be helpful to remove tiling patterns at mid/far distance.\nIt blends detail maps with themselves using different tiling.\n\nPer layer settings can be found in \"Layers properties\" tab.", MessageType.None, true);
                        checkChange(ref _target.blendMultiplier, EditorGUILayout.Slider("Mix Multiplier", _target.blendMultiplier, 0, 2));
                        if (GUILayout.Button("Disable UV blend (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_UV_BLEND_FIRST = false;
                            _RTP_LODmanagerScript.RTP_UV_BLEND_ADD = false;
                            if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                            //_RTP_LODmanagerScript.RefreshLODlevel();
                            //EditorUtility.SetDirty(_targetRT);
                        }

                        GUILayout.Space(15);
                        if (GUILayout.Button("Reset uv blend settings numeric params to default"))
                        {
                            Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                            _target.ReturnToDefaults("uvblend");
                            EditorUtility.SetDirty(_targetRT);
                            _target.Refresh(null, _targetRT);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("This feature is currently disabled.", MessageType.Warning, true);
                        if (GUILayout.Button("Enable UV blend (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_UV_BLEND_FIRST = true;
                            _RTP_LODmanagerScript.RTP_UV_BLEND_ADD = true;
                            if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                            //_RTP_LODmanagerScript.RefreshLODlevel();
                            //EditorUtility.SetDirty(_targetRT);
                        }
                    }
                    EditorGUILayout.EndVertical();

        #endregion Detail maps unfold - UV blend						
                    break;

                case ReliefTerrainSettingsItems.POMSettings:
        #region Settings - POM
                    GUILayout.Space(6);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoPOM), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Parallax (POM/PM) settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;
                    //checkChange(ref _target.ExtrudeHeight, EditorGUILayout.Slider("POM/PM Extrude Height", _target.ExtrudeHeight*Mathf.Max(tile_sizex, tile_sizey), 0.001f, 0.3f)/Mathf.Max(tile_sizex, tile_sizey));
                    checkChange(ref _target.ExtrudeHeight, EditorGUILayout.Slider("Extrude Height", _target.ExtrudeHeight, 0.001f, 0.3f));

                    EditorGUILayout.LabelField("Color/position solver (used in POM only)");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
                    EditorGUILayout.BeginVertical();
                    checkChange(ref _target.DIST_STEPS, EditorGUILayout.IntSlider("Max search steps", Mathf.FloorToInt(_target.DIST_STEPS), 4, 255));
                    checkChange(ref _target.WAVELENGTH, EditorGUILayout.Slider("Step length (in texels)", _target.WAVELENGTH, 0.5f, 16));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("Self-shadow solver (used in POM only)");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.HelpBox("To turn off self-shadowing in shader use RTP_LODmanager script (attach it to any scene object).", MessageType.None, true);
                    checkChange(ref _target.SHADOW_STEPS, EditorGUILayout.IntSlider("Max search steps", Mathf.FloorToInt(_target.SHADOW_STEPS), 0, 80));
                    checkChange(ref _target.WAVELENGTH_SHADOWS, EditorGUILayout.Slider("Step length (in texels)", _target.WAVELENGTH_SHADOWS, 0.5f, 16));
                    //checkChange(ref _target.SHADOW_SMOOTH_STEPS, EditorGUILayout.IntSlider("Max smoothing steps", Mathf.FloorToInt(_target.SHADOW_SMOOTH_STEPS), 3, 20));
                    checkChange(ref _target.SelfShadowStrength, EditorGUILayout.Slider("Self-shadow strength", _target.SelfShadowStrength, 0.1f, 1));
                    checkChange(ref _target.ShadowSmoothing, EditorGUILayout.Slider("Self-shadow smoothing", _target.ShadowSmoothing, 0f, 1));
                    checkChange(ref _target.ShadowSoftnessFade, EditorGUILayout.Slider("Self-shadow softness fade", _target.ShadowSoftnessFade, 0.2f, 1.5f));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(15);
                    if (GUILayout.Button("Reset POM/PM settings numeric params to default"))
                    {
                        Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                        _target.ReturnToDefaults("pom/pm");
                        EditorUtility.SetDirty(_targetRT);
                        _target.Refresh(null, _targetRT);
                    }

        #endregion Settings - POM
                    break;

                case ReliefTerrainSettingsItems.GlobalColor:
        #region Settings - Global color
                    _target.paint_wetmask = false;
                    GUILayout.Space(6);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoGlobalcolor), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Global color settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;

                    EditorGUILayout.HelpBox("Color is blended via multiplication (easy to find/change in shader code - look for #ifdef COLOR_MAP).\n\nALPHA channel is used to remove locally parallax effects - extrude height (necessary for seamless geometry blending or when objects on the surface seem to hover on at the surface due to high extrude height).\n\nIf you don't use ColorMap - set below blending values to zero (you may use Alpha8 compressing w/o RGB channels, but still need alpha for parallax removing at blend areas - terrain vs mesh).", MessageType.None, true);
                    EditorGUILayout.HelpBox("Near / Mid blend is realised at Main settings / Near distance values\n\nFar blend is realised at Perlin normal settings / start&fade distances.\n\nMIP level adjustement allows to take any MIP level below far distance (blended).", MessageType.None, true);
                    EditorGUILayout.HelpBox("Note that saturation on far distance might work different when shader doesn't use detail/splat colors there, but only global colormap (refer to LODmanager settings - \"No detail colors at far distance\").", MessageType.None, true);
                    EditorGUILayout.BeginHorizontal();
                    Texture2D prev_globalColor = _targetRT.ColorGlobal;
                    checkChange(ref _targetRT.ColorGlobal, (Texture2D)EditorGUILayout.ObjectField(_targetRT.ColorGlobal, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                    if (_targetRT.ColorGlobal && _targetRT.ColorGlobal != prev_globalColor)
                    {
                        AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_targetRT.ColorGlobal));
                        if (_importer)
                        {
                            TextureImporter tex_importer = (TextureImporter)_importer;
                            if (tex_importer)
                            {
                                tex_importer.wrapMode = TextureWrapMode.Clamp;
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_targetRT.ColorGlobal), ImportAssetOptions.ForceUpdate);
                            }
                        }
                        _targetRT.globalColorModifed_flag = false;
                        _targetRT.tmp_globalColorMap = null;
                        _targetRT.save_path_colormap = AssetDatabase.GetAssetPath(_targetRT.ColorGlobal);
                        if (_targetRT.save_path_colormap == "") _targetRT.save_path_colormap = "Assets/_output_colormap.png";
                        _targetRT.RefreshTextures();
                    }
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Near blend", GUILayout.MinWidth(80), GUILayout.MaxWidth(80));
                    checkChange(ref _target.GlobalColorMapBlendValues.x, EditorGUILayout.Slider(_target.GlobalColorMapBlendValues.x, 0, 1));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Mid blend", GUILayout.MinWidth(80), GUILayout.MaxWidth(80));
                    checkChange(ref _target.GlobalColorMapBlendValues.y, EditorGUILayout.Slider(_target.GlobalColorMapBlendValues.y, 0, 1));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Far blend", GUILayout.MinWidth(80), GUILayout.MaxWidth(80));
                    checkChange(ref _target.GlobalColorMapBlendValues.z, EditorGUILayout.Slider(_target.GlobalColorMapBlendValues.z, 0, 1));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("MIP level below far", GUILayout.MinWidth(120), GUILayout.MaxWidth(120));
                    checkChange(ref _target._GlobalColorMapNearMIP, EditorGUILayout.Slider(_target._GlobalColorMapNearMIP, 0, 10));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Saturation near", GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                    checkChange(ref _target.GlobalColorMapSaturation, EditorGUILayout.Slider(_target.GlobalColorMapSaturation, 0, 1));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Saturation far", GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                    checkChange(ref _target.GlobalColorMapSaturationFar, EditorGUILayout.Slider(_target.GlobalColorMapSaturationFar, 0, 1));
                    EditorGUILayout.EndHorizontal();
                    //if (!RTP_SUPER_SIMPLE || RTP_USE_PERLIN_ADD || RTP_USE_PERLIN_FIRST)
                    
                    {
                        //EditorGUILayout.BeginHorizontal();
                        //EditorGUILayout.LabelField("   mod by perlin", GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                        //checkChange(ref _target.GlobalColorMapSaturationByPerlin, EditorGUILayout.Slider(_target.GlobalColorMapSaturationByPerlin, 0, 1));
                        //EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Distort by perlin", GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                        checkChange(ref _target.GlobalColorMapDistortByPerlin, EditorGUILayout.Slider(_target.GlobalColorMapDistortByPerlin, 0, 0.02f));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Brightness near", GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                    checkChange(ref _target.GlobalColorMapBrightness, EditorGUILayout.Slider(_target.GlobalColorMapBrightness, 0.01f, 4));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Brightness far", GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                    checkChange(ref _target.GlobalColorMapBrightnessFar, EditorGUILayout.Slider(_target.GlobalColorMapBrightnessFar, 0.01f, 4));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    if (_targetRT.ColorGlobal)
                    {
                        if (_targetRT.globalColorModifed_flag)
                        {
                            GUILayout.Space(8);
                            GUI.color = new Color(0.5f, 1, 0.5f, 1);
                            if (GUILayout.Button("Colormap uncompressed & readable - save to file"))
                            {
                                string directory;
                                string file;
                                if (_targetRT.save_path_colormap == "")
                                {
                                    directory = Application.dataPath;
                                    file = "_output_colormap.png";
                                }
                                else
                                {
                                    directory = Path.GetDirectoryName(_targetRT.save_path_colormap);
                                    file = Path.GetFileNameWithoutExtension(_targetRT.save_path_colormap) + ".png";
                                }
                                string path = EditorUtility.SaveFilePanel("Save edited Global color map", directory, file, "png");
                                if (path != "")
                                {
                                    path = path.Substring(Application.dataPath.Length - 6);
                                    _targetRT.save_path_colormap = path;
                                    byte[] bytes = _targetRT.ColorGlobal.EncodeToPNG();
                                    System.IO.File.WriteAllBytes(path, bytes);
                                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                                    textureImporter.wrapMode = TextureWrapMode.Clamp;
                                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                                    _targetRT.ColorGlobal = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                                    _targetRT.globalColorModifed_flag = false;
                                    _targetRT.tmp_globalColorMap = null;
                                }
                            }
                            GUI.color = skin_color;
                            GUILayout.Space(4);
                        }
                    }


                    Color[] avg_cols = new Color[_target.numLayers];
                    for (int n = 0; n < _target.numLayers; n++)
                    {
                        Color[] col = new Color[1] { Color.black };
                        Texture2D tex = _target.splats[n];
                        string path = AssetDatabase.GetAssetPath(tex);
                        if (path != "")
                        {
                            AssetImporter _importer = AssetImporter.GetAtPath(path);
                            if (_importer)
                            {
                                TextureImporter tex_importer = (TextureImporter)_importer;
                                if (!tex_importer.isReadable)
                                {
                                    Debug.LogWarning("Texture (" + tex.name + ") has been reimported as readable.");
                                    tex_importer.isReadable = true;
                                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex), ImportAssetOptions.ForceUpdate);
                                }
                            }
                        }
                        col = tex.GetPixels(tex.mipmapCount - 1);
                        col[0].a = 1;
                        avg_cols[n] = col[0];
                    }
                    if (GUILayout.Button("Construct colormap from detail colors of layers (below)"))
                    {
                        if (_targetRT.controlA != null)
                        {
                            Color[] colorMap;
                            Color[] cols;
                            int u;
                            cols = _targetRT.controlA.GetPixels(0);
                            colorMap = new Color[cols.Length];
                            for (u = 0; u < cols.Length; u++)
                            {
                                colorMap[u] = new Color(0, 0, 0, 0);
                            }
                            for (int k = 0; k < _target.numLayers; k++)
                            {
                                if (k == 4)
                                {
                                    cols = _targetRT.controlB.GetPixels(0);
                                }
                                else if (k == 8)
                                {
                                    cols = _targetRT.controlC.GetPixels(0);
                                }
                                if (cols != null)
                                {
                                    for (u = 0; u < cols.Length; u++)
                                    {
                                        colorMap[u] += cols[u][k % 4] * avg_cols[k];
                                    }
                                }
                            }
                            _targetRT.ColorGlobal = new Texture2D(_targetRT.controlA.width, _targetRT.controlA.height, TextureFormat.ARGB32, true, false);
                            _targetRT.ColorGlobal.SetPixels(colorMap, 0);
                            _targetRT.ColorGlobal.Apply(true);
                            _targetRT.ColorGlobal.wrapMode = TextureWrapMode.Clamp;
                            _targetRT.globalColorModifed_flag = true;
                        }
                    }
                    EditorGUILayout.BeginHorizontal();
                    int thumb_size = 32;
                    for (int n = 0; n < _target.numLayers; n++)
                    {
                        Color ccol = GUI.contentColor;
                        GUI.contentColor = avg_cols[n];
                        if (GUILayout.Button(_target.get_dumb_tex(), "Label", GUILayout.Width(thumb_size), GUILayout.Height(thumb_size)))
                        {
                            _target.paintColor = avg_cols[n];
                        }
                        GUI.contentColor = ccol;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();

                    int copy_splat_idx = 0;
                    if (_4LAYERS_SHADER_USED && (_target.numLayers > 4))
                    {
                        copy_splat_idx = 1;
                    }
                    else if ((!_4LAYERS_SHADER_USED) && (_target.numLayers > 8))
                    {
                        copy_splat_idx = 2;
                    }
                    if (copy_splat_idx > 0)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("Due to parallax effects to get better transitions between AddPass and FirstPass areas you need to cancel extrusion there.\n\nNote, that any height painting already done by either geometry blend or this script will be overwritten.", MessageType.Warning, true);
                        if (GUILayout.Button("Overlap coverage to alpha (flatten overlapped areas)"))
                        {
                            colorAlphaFromSplat(copy_splat_idx);
                        }
                    }

                    EditorGUILayout.EndVertical();

                    //
                    // paint tools
                    //
                    GUILayout.Space(8);
                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Paint tools", EditorStyles.boldLabel);
                    GUI.color = skin_color;

                    bool prev_paint_flag = _target.paint_flag;

                    if (!_target.paint_flag)
                    {
                        Color c = GUI.color;
                        GUI.color = new Color(0.9f, 1, 0.9f);
                        if (GUILayout.Button(new GUIContent("Begin painting (M)", icoPaintOn, "Click to turn on painting")))
                        {
                            _target.paint_flag = true;
                            _target.paint_wetmask = false;
                            //							if (!_target.prepare_tmpTexture(true)) {
                            //								_target.paint_flag=false;
                            //							}
                        }
                        if (!_targetRT.GetComponent<Collider>() || !_targetRT.GetComponent<Collider>().enabled) _target.paint_flag = false;
                        GUI.color = c;
                    }
                    else if (_target.paint_flag)
                    {
                        Color c = GUI.color;
                        GUI.color = new Color(1, 0.9f, 0.9f);
                        if (GUILayout.Button(new GUIContent("End painting (M)", icoPaintOff, "Click to turn off painting")))
                        {
                            _target.paint_flag = false;
                        }
                        GUI.color = c;
                    }
                    if (!prev_paint_flag && _target.paint_flag)
                    {
                        UnityEditor.Tools.current = Tool.View;
                        AddSceneViewCallback();
                    }
                    else if (prev_paint_flag && !_target.paint_flag)
                    {
                        UnityEditor.Tools.current = prev_tool;
                        RemoveSceneViewCallback();
                    }
                    if (prev_paint_flag != _target.paint_flag) EditorUtility.SetDirty(target);
                    if (_target.paint_flag)
                    {
                        if (!_targetRT.GetComponent<Collider>() || !_targetRT.GetComponent<Collider>().enabled) EditorGUILayout.HelpBox("Object doesn't have collider (necessary for painting).", MessageType.Error, true);

                        string[] modes;
                        if (RTP_CUT_HOLES)
                        {
                            modes = new string[3] { "Color", "Extrude Height", "Cut holes" };
                        }
                        else
                        {
                            modes = new string[2] { "Color", "Extrude Height" };
                            _target.cut_holes = false;
                        }
                        int npaint_mode = GUILayout.SelectionGrid(_target.paint_alpha_flag ? (1 + (_target.cut_holes ? 1 : 0)) : 0, modes, modes.Length);
                        if (npaint_mode != (_target.paint_alpha_flag ? (1 + (_target.cut_holes ? 1 : 0)) : 0))
                        {
                            EditorUtility.SetDirty(target);
                        }
                        _target.paint_alpha_flag = (npaint_mode == 0) ? false : true;
                        _target.cut_holes = (npaint_mode == 2);

                        if (_target.paint_alpha_flag)
                        {
                            EditorGUILayout.HelpBox("Hold SHIFT while painting to apply eraser.", MessageType.Info, true);
                        }
                        else
                        {
                            EditorGUILayout.BeginVertical("Box");
                            skin_color = GUI.color;
                            GUI.color = new Color(1, 1, 0.5f, 1);
                            if (_target.paintColorSwatches != null && _target.paintColorSwatches.Length > 0) EditorGUILayout.LabelField("Color swatches", EditorStyles.boldLabel);
                            GUI.color = skin_color;
                            EditorGUILayout.BeginHorizontal();
                            thumb_size = 16;
                            for (int n = 0; _target.paintColorSwatches != null && n < _target.paintColorSwatches.Length; n++)
                            {
                                Color ccol = GUI.contentColor;
                                GUI.contentColor = _target.paintColorSwatches[n];
                                if (GUILayout.Button(_target.get_dumb_tex(), "Label", GUILayout.Width(thumb_size), GUILayout.Height(thumb_size)))
                                {
                                    _target.paintColor = _target.paintColorSwatches[n];
                                }
                                GUI.contentColor = ccol;
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUI.BeginDisabledGroup(_target.paintColorSwatches == null || _target.paintColorSwatches.Length == 12);
                            if (GUILayout.Button("Add swatch"))
                            {
                                Color[] ns = new Color[_target.paintColorSwatches.Length + 1];
                                for (int k = 0; k < _target.paintColorSwatches.Length; k++) ns[k] = _target.paintColorSwatches[k];
                                ns[ns.Length - 1] = new Color(_target.paintColor.r, _target.paintColor.g, _target.paintColor.b, 1);
                                _target.paintColorSwatches = ns;
                            }
                            EditorGUI.EndDisabledGroup();
                            EditorGUI.BeginDisabledGroup(_target.paintColorSwatches == null || _target.paintColorSwatches.Length == 0);
                            if (GUILayout.Button("Remove swatch"))
                            {
                                Color[] ns = new Color[_target.paintColorSwatches.Length - 1];
                                for (int k = 0; k < ns.Length; k++) ns[k] = _target.paintColorSwatches[k];
                                _target.paintColorSwatches = ns;
                            }
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();

                            _target.paintColor = EditorGUILayout.ColorField("Paint color", _target.paintColor);
                            _target.preserveBrightness = EditorGUILayout.Toggle("Preserve luminosity", _target.preserveBrightness);
                            EditorGUILayout.EndVertical();
                        }

                        Texture2D tmp_tex = _targetRT.tmp_globalColorMap;
                        if (tmp_tex && (tmp_tex.format != TextureFormat.Alpha8 && tmp_tex.format != TextureFormat.ARGB32))
                        {
                            EditorGUILayout.HelpBox("Global colormap need to be readable and uncompressed for painting.", MessageType.Error, true);
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Area size", EditorStyles.label);
                        _target.paint_size = EditorGUILayout.Slider(_target.paint_size, 0.1f, 6);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Area smoothness", EditorStyles.label);
                        _target.paint_smoothness = EditorGUILayout.Slider(_target.paint_smoothness, 0.001f, 1);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Opacity", EditorStyles.label);
                        _target.paint_opacity = EditorGUILayout.Slider(_target.paint_opacity, 0, 1);
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(10);
                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    if (RTP_TESSELLATION)
                    {
                        EditorGUILayout.LabelField("Tessellation height & normal map settings", EditorStyles.boldLabel);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Global normal settings", EditorStyles.boldLabel);
                    }
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;

                    if (RTP_TESSELLATION)
                    {

                        EditorGUILayout.HelpBox("To get texture needed use attached tool -\nWindow/Relief Tools/Prepare Height&Normal Texture for Tessellation\nWhen tessellating terrain you can increase pixel error (to get low poly terrain mesh handled by Unity itself) while maintaining very detailed and pop-minimized look.", MessageType.None, true);
                        if (!RTP_TESSELLATION_SAMPLE_TEXTURE)
                        {
                            _targetRT.NormalGlobal = null;
                        }
                        EditorGUILayout.BeginHorizontal();

                        EditorGUI.BeginDisabledGroup(!RTP_TESSELLATION_SAMPLE_TEXTURE);
                        if (checkChange(ref _targetRT.NormalGlobal, (Texture2D)EditorGUILayout.ObjectField(_targetRT.NormalGlobal, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100))))
                        {
                            AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_targetRT.NormalGlobal));
                            if (_importer)
                            {
                                TextureImporter tex_importer = (TextureImporter)_importer;
                                if (tex_importer.textureCompression != TextureImporterCompression.Uncompressed || tex_importer.textureType != TextureImporterType.Default)
                                {
                                    Debug.LogWarning("Texture (" + _targetRT.NormalGlobal.name + ") seems to be NOT compatible with RTP Height&normal format. Use attached tool to make the texture.");
                                }
                            }
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.BeginVertical();

                        EditorGUI.BeginDisabledGroup(!RTP_TESSELLATION_SAMPLE_TEXTURE);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Normal strength", GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                        checkChange(ref _target.global_normalMap_multiplier, EditorGUILayout.Slider(_target.global_normalMap_multiplier, 0f, 6f));
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Tess. substeps (close)", GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                        if (checkChange(ref _target._TessSubdivisions, EditorGUILayout.Slider(_target._TessSubdivisions, 1f, 63f)))
                        {
                            _target._TessSubdivisions = Mathf.Floor((_target._TessSubdivisions - 1) / 2) * 2 + 1;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Tess. substeps (far)", GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                        if (checkChange(ref _target._TessSubdivisionsFar, EditorGUILayout.Slider(_target._TessSubdivisionsFar, 1f, 63f)))
                        {
                            _target._TessSubdivisionsFar = Mathf.Floor((_target._TessSubdivisionsFar - 1) / 2) * 2 + 1;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.BeginDisabledGroup(RTP_TESSELLATION_SAMPLE_TEXTURE);
                        if (RTP_TESSELLATION_SAMPLE_TEXTURE) _target._Phong = 0;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Phong smoothening", GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                        checkChange(ref _target._Phong, EditorGUILayout.Slider(_target._Phong, 0f, 1f));
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();

                        Terrain terrain = _targetRT.gameObject.GetComponent<Terrain>();
                        if (terrain != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Terrain pixel error", GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                            float nPixelError = EditorGUILayout.Slider(terrain.heightmapPixelError, 1f, 200f);
                            if (terrain.heightmapPixelError != nPixelError)
                            {
                                terrain.heightmapPixelError = nPixelError;
                                ReliefTerrain[] rtp_objs = (ReliefTerrain[])GameObject.FindObjectsOfType(typeof(ReliefTerrain));
                                for (int i = 0; i < rtp_objs.Length; i++)
                                {
                                    Terrain ter = rtp_objs[i].gameObject.GetComponent<Terrain>();
                                    if (ter)
                                    {
                                        ter.heightmapPixelError = nPixelError;
                                    }
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUI.BeginDisabledGroup(_targetRT.NormalGlobal == null);
                            if (GUILayout.Button("Reapply height texture to terrain data"))
                            {
                                if (EditorUtility.DisplayDialog("Warning", "Are you sure to overwrite terrain height data ?", "Yes", "Cancel"))
                                {
                                    AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_targetRT.NormalGlobal));
                                    if (_importer)
                                    {
                                        TextureImporter tex_importer = (TextureImporter)_importer;
                                        if (!tex_importer.isReadable)
                                        {
                                            Debug.LogWarning("Global normal texture (" + _targetRT.NormalGlobal.name + ") has been reimported as readable.");
                                            tex_importer.isReadable = true;
                                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_targetRT.NormalGlobal), ImportAssetOptions.ForceUpdate);
                                        }
                                    }
                                    {
                                        TerrainData terrainData = terrain.terrainData;
                                        float[,] heights;
                                        float _ratio = 1.0f * _targetRT.NormalGlobal.width / terrainData.heightmapResolution;
                                        if (_ratio > 1.9f)
                                        {
                                            // sample from hi-res texture to lower res heightmap
                                            // we take max sample
                                            // this is used to conform lo-res terrain mesh with its occlusion data (useful in Unity5 where terrain surface can be rendered separately)
                                            heights = ReliefTerrain.ConformTerrain2Occlusion(terrainData, _targetRT.NormalGlobal);
                                        }
                                        else
                                        {
                                            // sample from hi-res texture to similar or higher res heightmap
                                            // we take exact sample
                                            // this is used to conform detail placement and collider fit
                                            heights = ReliefTerrain.ConformTerrain2Detail(terrainData, _targetRT.NormalGlobal, RTP_HEIGHTMAP_SAMPLE_BICUBIC);
                                        }
                                        terrainData.SetHeights(0, 0, heights);
                                    }
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else if (RTP_TESSELLATION_SAMPLE_TEXTURE)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Displacement height", GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                            if (checkChange(ref _target.tessHeight, EditorGUILayout.FloatField(_target.tessHeight)))
                            {
                                MeshFilter mf = _targetRT.GetComponent<MeshFilter>();
                                if (mf && mf.sharedMesh)
                                {
                                    mf.sharedMesh.RecalculateBounds();
                                    Bounds bounds = mf.sharedMesh.bounds;
                                    Vector3 center = bounds.center;
                                    Vector3 extents = bounds.extents;
                                    center.y = (_target.tessHeight + _target._TessYOffset * 2) / _targetRT.transform.localScale.y * 0.5f;
                                    extents.y = _target.tessHeight / _targetRT.transform.localScale.y;
                                    extents.x *= 2;
                                    extents.z *= 2;
                                    mf.sharedMesh.bounds = new Bounds(center, extents);
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Y offset", GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                            if (checkChange(ref _target._TessYOffset, EditorGUILayout.FloatField(_target._TessYOffset)))
                            {
                                MeshFilter mf = _targetRT.GetComponent<MeshFilter>();
                                if (mf && mf.sharedMesh)
                                {
                                    mf.sharedMesh.RecalculateBounds();
                                    Bounds bounds = mf.sharedMesh.bounds;
                                    Vector3 center = bounds.center;
                                    Vector3 extents = bounds.extents;
                                    center.y = (_target.tessHeight + _target._TessYOffset * 2) / _targetRT.transform.localScale.y * 0.5f;
                                    extents.y = _target.tessHeight / _targetRT.transform.localScale.y;
                                    extents.x *= 2;
                                    extents.z *= 2;
                                    mf.sharedMesh.bounds = new Bounds(center, extents);
                                }
                            }
                            EditorGUILayout.EndHorizontal();


                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();

                    }
                    else
                    {
                        Terrain terrain = _targetRT.GetComponent<Terrain>();
                        if (terrain != null && terrain.drawInstanced)
                        {
                            EditorGUILayout.HelpBox("This feature is automatically handled in instanced draw mode.", MessageType.Info, true);
                        }
                        else if (RTP_NORMALGLOBAL)
                        {
                            EditorGUILayout.HelpBox("Using global normal below we won't use mesh normals - will be treated flat (0,1,0), but you can greately improve look when lo-res heightmap or high pixelError settings are used.", MessageType.None, true);
                            if (GUILayout.Button("Disable global normal (rebuild shaders via RTP_LODmanager)"))
                            {
                                _RTP_LODmanagerScript.RTP_NORMALGLOBAL = false;
                                if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                                {
                                    Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                    _RTP_LODmanagerScript.dont_sync = true;
                                }
                                //_RTP_LODmanagerScript.RefreshLODlevel();
                                //EditorUtility.SetDirty(_targetRT);
                            }

                            EditorGUILayout.BeginHorizontal();
                            if (checkChange(ref _targetRT.NormalGlobal, (Texture2D)EditorGUILayout.ObjectField(_targetRT.NormalGlobal, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100))))
                            {
                                AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_targetRT.NormalGlobal));
                                if (_importer)
                                {
                                    TextureImporter tex_importer = (TextureImporter)_importer;
                                    if (tex_importer)
                                    {
                                        tex_importer.wrapMode = TextureWrapMode.Clamp;
                                        tex_importer.textureType = TextureImporterType.NormalMap;
                                        Debug.LogWarning("Global normal texture (" + _targetRT.NormalGlobal.name + ") has been imported as normalmap type.");
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_targetRT.NormalGlobal), ImportAssetOptions.ForceUpdate);
                                    }
                                }
                            }
                            EditorGUILayout.BeginVertical();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Normal strength", GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                            checkChange(ref _target.global_normalMap_multiplier, EditorGUILayout.Slider(_target.global_normalMap_multiplier, 0.5f, 6f));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Far distance use", GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                            checkChange(ref _target.global_normalMap_farUsage, EditorGUILayout.Slider(_target.global_normalMap_farUsage, 0f, 1f));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("This feature is currently disabled.", MessageType.Warning, true);
                            if (GUILayout.Button("Enable global normal (rebuild shaders via RTP_LODmanager)"))
                            {
                                _RTP_LODmanagerScript.RTP_NORMALGLOBAL = true;
                                if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                                {
                                    Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                    _RTP_LODmanagerScript.dont_sync = true;
                                }
                                //_RTP_LODmanagerScript.RefreshLODlevel();
                                //EditorUtility.SetDirty(_targetRT);
                            }
                        }

                    }


                    EditorGUILayout.EndVertical();

                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Global trees/shadow settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;

                    if (RTP_TREESGLOBAL)
                    {
                        EditorGUILayout.HelpBox("RGB - trees color at very far distance (billboard trees drawing distance). Blending done via brightness, so black means - no pixel tree.\n\nA channel - shadows (mean to be used when you don't use lightmaps).", MessageType.None, true);
                        if (GUILayout.Button("Disable global trees/shadows (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_TREESGLOBAL = false;
                            if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                            //_RTP_LODmanagerScript.RefreshLODlevel();
                            //EditorUtility.SetDirty(_targetRT);
                        }

                        EditorGUILayout.BeginHorizontal();
                        checkChange(ref _targetRT.TreesGlobal, (Texture2D)EditorGUILayout.ObjectField(_targetRT.TreesGlobal, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Trees distance", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                        checkChange(ref _target.trees_pixel_distance_start, EditorGUILayout.FloatField(_target.trees_pixel_distance_start));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Trees fade length", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                        checkChange(ref _target.trees_pixel_distance_transition, EditorGUILayout.Slider(_target.trees_pixel_distance_transition, 0, 100));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Trees blending", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                        checkChange(ref _target.trees_pixel_blend_val, EditorGUILayout.Slider(_target.trees_pixel_blend_val, 0, 10));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Shadows distance", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                        checkChange(ref _target.trees_shadow_distance_start, EditorGUILayout.FloatField(_target.trees_shadow_distance_start));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Shadows fade length", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                        checkChange(ref _target.trees_shadow_distance_transition, EditorGUILayout.Slider(_target.trees_shadow_distance_transition, 0, 30));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Shadows blending", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                        checkChange(ref _target.trees_shadow_value, 1 - EditorGUILayout.Slider(1 - _target.trees_shadow_value, 0, 1));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("This feature is currently disabled.", MessageType.Warning, true);
                        if (GUILayout.Button("Enable global trees/shadows (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_TREESGLOBAL = true;
                            if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                            //_RTP_LODmanagerScript.RefreshLODlevel();
                            //EditorUtility.SetDirty(_targetRT);
                        }
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(10);

                    //EditorGUILayout.BeginVertical("Box");
                    //skin_color=GUI.color;
                    //EditorGUILayout.BeginHorizontal();
                    //GUI.color=new Color(1,1,0.5f,1);
                    //EditorGUILayout.LabelField("Global ambient emissive map settings", EditorStyles.boldLabel);
                    //EditorGUILayout.EndHorizontal();
                    //GUI.color=skin_color;

                    //if (RTP_AMBIENT_EMISSIVE_MAP) {
                    //	EditorGUILayout.HelpBox("RGB - ambient emissive color.\n\nA channel - shadows (mean to be used when you don't use lightmaps).",MessageType.None, true);
                    //	if (GUILayout.Button("Disable ambient emissive map (rebuild shaders via RTP_LODmanager)")) {
                    //		_RTP_LODmanagerScript.RTP_AMBIENT_EMISSIVE_MAP=false;
                    //		if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?","Yes","No")) {
                    //			Selection.activeObject=_RTP_LODmanagerScript.gameObject;
                    //			_RTP_LODmanagerScript.dont_sync=true;
                    //		}
                    //		//_RTP_LODmanagerScript.RefreshLODlevel();
                    //		//EditorUtility.SetDirty(_targetRT);
                    //	}

                    //	EditorGUILayout.BeginHorizontal();
                    //	checkChange(ref _targetRT.AmbientEmissiveMap, (Texture2D)EditorGUILayout.ObjectField(_targetRT.AmbientEmissiveMap, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                    //	EditorGUILayout.BeginVertical();

                    //	EditorGUILayout.BeginHorizontal();
                    //	EditorGUILayout.LabelField("Emission brightness", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                    //	checkChange(ref _target._AmbientEmissiveMultiplier, EditorGUILayout.Slider(_target._AmbientEmissiveMultiplier,0,4));
                    //	EditorGUILayout.EndHorizontal();
                    //	EditorGUILayout.BeginHorizontal();
                    //	EditorGUILayout.LabelField("Normal/height mod", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                    //	checkChange(ref _target._AmbientEmissiveRelief, EditorGUILayout.Slider(_target._AmbientEmissiveRelief,0,1));
                    //	EditorGUILayout.EndHorizontal();
                    //	EditorGUILayout.BeginHorizontal();
                    //	EditorGUILayout.LabelField("Shadows distance", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                    //	checkChange(ref _target.trees_shadow_distance_start, EditorGUILayout.FloatField(_target.trees_shadow_distance_start));
                    //	EditorGUILayout.EndHorizontal();
                    //	EditorGUILayout.BeginHorizontal();
                    //	EditorGUILayout.LabelField("Shadows fade length", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                    //	checkChange(ref _target.trees_shadow_distance_transition, EditorGUILayout.Slider(_target.trees_shadow_distance_transition, 0, 30));
                    //	EditorGUILayout.EndHorizontal();
                    //	EditorGUILayout.BeginHorizontal();
                    //	EditorGUILayout.LabelField("Shadows blending", GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
                    //	checkChange(ref _target.trees_shadow_value, 1-EditorGUILayout.Slider(1-_target.trees_shadow_value, 0, 1));
                    //	EditorGUILayout.EndHorizontal();

                    //	EditorGUILayout.EndVertical();
                    //	EditorGUILayout.EndHorizontal();	
                    //} else {
                    //	EditorGUILayout.HelpBox("This feature is currently disabled.",MessageType.Warning, true);
                    //	if (GUILayout.Button("Enable ambient emissive map (rebuild shaders via RTP_LODmanager)")) {
                    //		_RTP_LODmanagerScript.RTP_AMBIENT_EMISSIVE_MAP=true;
                    //		if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?","Yes","No")) {
                    //			Selection.activeObject=_RTP_LODmanagerScript.gameObject;
                    //			_RTP_LODmanagerScript.dont_sync=true;
                    //		}
                    //		//_RTP_LODmanagerScript.RefreshLODlevel();
                    //		//EditorUtility.SetDirty(_targetRT);
                    //	}
                    //}
                    //EditorGUILayout.EndVertical();					

                    //if (RTP_SS_GRAYSCALE_DETAIL_COLORS_FIRST || RTP_SS_GRAYSCALE_DETAIL_COLORS_ADD) {
                    //	EditorGUILayout.BeginVertical("Box");
                    //	skin_color=GUI.color;
                    //	EditorGUILayout.BeginHorizontal();
                    //	GUI.color=new Color(1,1,0.5f,1);
                    //	EditorGUILayout.LabelField("Grayscale detail textures", EditorStyles.boldLabel);
                    //	EditorGUILayout.EndHorizontal();
                    //	GUI.color=skin_color;						
                    //	EditorGUILayout.HelpBox("In massive terrain mode (refer to RTP LOD manager for 4 layers mode) you can use even simplier close-distance detail colors as grayscale and global colorMap to tint them. RGBA channels refers to grayscaled detail textures for layers.",MessageType.None, true);
                    //	EditorGUILayout.BeginHorizontal();
                    //		EditorGUI.BeginDisabledGroup(!RTP_SS_GRAYSCALE_DETAIL_COLORS_FIRST);
                    //		EditorGUILayout.BeginVertical();
                    //			Texture2D oRef=_target.SSColorCombinedA;
                    //			if (checkChange(ref _target.SSColorCombinedA, (Texture2D)EditorGUILayout.ObjectField(_target.SSColorCombinedA, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)))) {
                    //				DestroyUnusedCombinedTexture(oRef);
                    //			}
                    //			SaveTexture(ref _target.SSColorCombinedA, ref _target.save_path_SSColorCombinedA, "terrain_detail_grayscale_combined.png", 100, TextureImporterFormat.DXT5, true);
                    //		EditorGUILayout.EndVertical();
                    //		EditorGUI.EndDisabledGroup();
                    //		EditorGUI.BeginDisabledGroup(!RTP_SS_GRAYSCALE_DETAIL_COLORS_ADD);
                    //		EditorGUILayout.BeginVertical();
                    //			oRef=_target.SSColorCombinedB;
                    //			if (checkChange(ref _target.SSColorCombinedB, (Texture2D)EditorGUILayout.ObjectField(_target.SSColorCombinedB, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)))) {
                    //				DestroyUnusedCombinedTexture(oRef);
                    //			}
                    //			SaveTexture(ref _target.SSColorCombinedB, ref _target.save_path_SSColorCombinedB, "terrain_detail_grayscale_combined.png", 100, TextureImporterFormat.DXT5, true);
                    //		EditorGUILayout.EndVertical();
                    //		EditorGUI.EndDisabledGroup();
                    //	EditorGUILayout.EndHorizontal();	

                    //	if (GUILayout.Button("Make combined grayscale texture(s)")) {
                    //		if (RTP_SS_GRAYSCALE_DETAIL_COLORS_FIRST) MakeCombinedGrayscale(0, ref _target.SSColorCombinedA);
                    //		if (RTP_SS_GRAYSCALE_DETAIL_COLORS_ADD) MakeCombinedGrayscale(4, ref _target.SSColorCombinedB);
                    //		_target.RefreshAll();
                    //	}

                    //	if (GUILayout.Button("Disable grayscale detail textures")) {
                    //		_RTP_LODmanagerScript.RTP_SS_GRAYSCALE_DETAIL_COLORS_FIRST=false;
                    //		_RTP_LODmanagerScript.RTP_SS_GRAYSCALE_DETAIL_COLORS_ADD=false;
                    //		if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?","Yes","No")) {
                    //			Selection.activeObject=_RTP_LODmanagerScript.gameObject;
                    //			_RTP_LODmanagerScript.dont_sync=true;
                    //		}
                    //	}	

                    //	EditorGUILayout.EndVertical();	
                    //}

                    GUILayout.Space(15);
                    if (GUILayout.Button("Reset global color settings numeric params to default"))
                    {
                        Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                        _target.ReturnToDefaults("global_color");
                        EditorUtility.SetDirty(_targetRT);
                        _target.Refresh(null, _targetRT);
                    }

        #endregion Settings - Global color
                    break;

                case ReliefTerrainSettingsItems.GlobalNormal:
        #region Settings - Global normal 
                    GUILayout.Space(6);
                    ;
                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoPerlinNormal), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Perlin normal map settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;

                    EditorGUILayout.HelpBox("Feature below are applied per layer in \"Layer properties\".", MessageType.None, true);
                    EditorGUILayout.HelpBox("REMEMBER - this map is also used for water flow animation, water/snow random coverage and superdetail normal mapping.", MessageType.None, true);

                    EditorGUILayout.BeginHorizontal();
                    Texture2D tex_bump = _target.BumpGlobal;
                    Texture2D ntex = tex_bump;
                    checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(_target.BumpGlobal, typeof(Texture2D), false, GUILayout.MinHeight(90), GUILayout.MinWidth(90), GUILayout.MaxWidth(90)));
                    if (ntex == null)
                    {
                        ntex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Textures/ReliefTerrain/perlin_noise.png", typeof(Texture2D));
                        _targetRT.TERRAIN_WetMask = null;
                        _target.SuperDetailA = null;
                        _target.SuperDetailB = null;
                    }
                    if (ntex != tex_bump)
                    {
                        Texture2D tex_prev = _target.BumpGlobal;
                        _target.BumpGlobal = ntex;
                        if (ntex)
                        {
                            AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ntex));
                            if (_importer)
                            {
                                TextureImporter tex_importer = (TextureImporter)_importer;
                                if (!tex_importer.isReadable)
                                {
                                    Debug.LogWarning("Global normal texture (" + ntex.name + ") has been reimported as readable.");
                                    tex_importer.isReadable = true;
                                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(ntex), ImportAssetOptions.ForceUpdate);
                                }
                            }
                        }
                        if (!_targetRT.PrepareGlobalNormalsAndSuperDetails())
                        {
                            Debug.LogWarning("Due to rebuilding special combined texture superdetail textures, water mask and reflection map have been cleared (you have to reassign them again).");
                            _target.BumpGlobal = tex_prev;
                            _targetRT.TERRAIN_WetMask = null;
                            _target.SuperDetailA = null;
                            _target.SuperDetailB = null;
                            if (_targetRT.PrepareGlobalNormalsAndSuperDetails())
                            {
                                _target.BumpGlobal = ntex;
                            }
                        }
                        _target.Refresh(null, _targetRT);
                    }
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Tiling scale (" + (Mathf.Round(_target.ReliefTransform.x / _target.BumpMapGlobalScale * 10) / 10) + " m)", GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                    EditorGUILayout.BeginHorizontal();
                    checkChange(ref _target.BumpMapGlobalScale, EditorGUILayout.Slider(_target.BumpMapGlobalScale, 0.01f, 0.25f));
                    //					if (GUILayout.Button(new GUIContent("A","Align to terrain tile"), GUILayout.Width(22), GUILayout.Height(16))) {
                    //						float ratio=Mathf.Round(_target.ReliefTransform.x*_target.BumpMapGlobalScale);
                    //						_target.BumpMapGlobalScale=ratio/_target.ReliefTransform.x;
                    //						_target.RefreshAll();
                    //					}
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("MIP offset", GUILayout.MinWidth(120), GUILayout.MaxWidth(120));
                    checkChange(ref _target.rtp_mipoffset_globalnorm, EditorGUILayout.IntSlider(_target.rtp_mipoffset_globalnorm, 0, 5));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Distance start", GUILayout.MinWidth(120), GUILayout.MaxWidth(120));
                    checkChange(ref _target.distance_start_bumpglobal, EditorGUILayout.Slider(_target.distance_start_bumpglobal, 0, 100));
                    if (_target.distance_start_bumpglobal < _target.distance_start) _target.distance_start_bumpglobal = _target.distance_start;
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginDisabledGroup(_target.distance_start_bumpglobal > 0);
                    if (_target.distance_start_bumpglobal > 0) _target.rtp_perlin_start_val = 0;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Starting value", GUILayout.MinWidth(120), GUILayout.MaxWidth(120));
                    checkChange(ref _target.rtp_perlin_start_val, EditorGUILayout.Slider(_target.rtp_perlin_start_val, 0, 1.0f));
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Fade length", GUILayout.MinWidth(120), GUILayout.MaxWidth(120));
                    checkChange(ref _target.distance_transition_bumpglobal, EditorGUILayout.Slider(_target.distance_transition_bumpglobal, 0, 300));
                    EditorGUILayout.EndHorizontal();
                    //					EditorGUILayout.BeginHorizontal();
                    //					EditorGUILayout.LabelField("Far normal damp", GUILayout.MinWidth(120), GUILayout.MaxWidth(120));
                    //					checkChange(ref _target._FarNormalDamp, EditorGUILayout.Slider(_target._FarNormalDamp, 0, 1));
                    //					EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();

                    GUILayout.Space(15);
                    if (GUILayout.Button("Reset perlin normal settings numeric params to default"))
                    {
                        Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                        _target.ReturnToDefaults("perlin");
                        EditorUtility.SetDirty(_targetRT);
                        _target.Refresh(null, _targetRT);
                    }

        #endregion Settings - Global normal	
                    break;

                case ReliefTerrainSettingsItems.Superdetail:
        #region Settings - superdetail					
                    GUILayout.Space(6);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoSuperdetail), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Superdetail settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;

                    if (SUPER_DETAIL_ENABLED_FIRST || SUPER_DETAIL_ENABLED_ADD)
                    {
                        EditorGUILayout.HelpBox("Superdetail is texture consisting of Perlin normalmap and optional 2 additional multiplicative channels. They're shared on combined texture with water mask and reflection map, so CAN NOT be used (with water/reflection) at the same time.\n\nSuperdetail normal is applied only on near distance.", MessageType.None, true);

                        if (GUILayout.Button("Disable superdetails (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_SUPER_DETAIL_FIRST = false;
                            _RTP_LODmanagerScript.RTP_SUPER_DETAIL_ADD = false;
                            if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                            //_RTP_LODmanagerScript.RefreshLODlevel();
                            //EditorUtility.SetDirty(_targetRT);
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Superdetail tiling scale", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._SuperDetailTiling, EditorGUILayout.IntSlider(Mathf.RoundToInt(_target._SuperDetailTiling), 1, 16));
                        EditorGUILayout.EndHorizontal();

                        if (SUPER_DETAIL_MULTS_ENABLED_FIRST || SUPER_DETAIL_MULTS_ENABLED_ADD)
                        {
                            GUILayout.Space(8);
                            EditorGUILayout.HelpBox("Superdetail multiplicative channles can be applied on both near and far distances with optional self-masking (works the way multiplicative channel masks itself using Perlin normal tiling).\n\nColor multiplication is realised classic way (color x mult_channel x 2) giving opportunity to make it lighten/darken (grey texture gives neutral effect).\n\nOn far distance selfmasking set to 0 means we've got no blending (otherwise we would see distinct superdetail patterns over large areas).", MessageType.None, true);
                            EditorGUILayout.BeginHorizontal();
                            Texture2D tmp_tex = _target.SuperDetailA;
                            ntex = tmp_tex;
                            checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(_target.SuperDetailA, typeof(Texture2D), false, GUILayout.MinHeight(90), GUILayout.MinWidth(90), GUILayout.MaxWidth(90)));
                            if (ntex != tmp_tex)
                            {
                                Texture2D tex_prev = _target.SuperDetailA;
                                _target.SuperDetailA = ntex;
                                if (ntex)
                                {
                                    AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ntex));
                                    if (_importer)
                                    {
                                        TextureImporter tex_importer = (TextureImporter)_importer;
                                        if (!tex_importer.isReadable)
                                        {
                                            Debug.LogWarning("Superdetail 1st texture (" + ntex.name + ") has been reimported as readable.");
                                            tex_importer.isReadable = true;
                                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(ntex), ImportAssetOptions.ForceUpdate);
                                        }
                                    }
                                }
                                if (!_targetRT.PrepareGlobalNormalsAndSuperDetails()) _target.SuperDetailA = tex_prev;
                                _target.Refresh(null, _targetRT);
                            }
                            RTPColorChannels nchannel = RTPColorChannels.R;
                            EditorGUILayout.LabelField("channel", GUILayout.MinWidth(60), GUILayout.MaxWidth(60));
                            checkChange(ref nchannel, (RTPColorChannels)EditorGUILayout.EnumPopup(_target.SuperDetailA_channel));
                            if (nchannel != _target.SuperDetailA_channel)
                            {
                                _target.SuperDetailA_channel = nchannel;
                                _targetRT.PrepareGlobalNormalsAndSuperDetails();
                            }
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.Space();
                            EditorGUILayout.BeginHorizontal();
                            tmp_tex = _target.SuperDetailB;
                            ntex = tmp_tex;
                            checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(_target.SuperDetailB, typeof(Texture2D), false, GUILayout.MinHeight(90), GUILayout.MinWidth(90), GUILayout.MaxWidth(90)));
                            if (ntex != tmp_tex)
                            {
                                Texture2D tex_prev = _target.SuperDetailB;
                                _target.SuperDetailB = ntex;
                                if (ntex)
                                {
                                    AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ntex));
                                    if (_importer)
                                    {
                                        TextureImporter tex_importer = (TextureImporter)_importer;
                                        if (!tex_importer.isReadable)
                                        {
                                            Debug.LogWarning("Superdetail 2st texture (" + ntex.name + ") has been reimported as readable.");
                                            tex_importer.isReadable = true;
                                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(ntex), ImportAssetOptions.ForceUpdate);
                                        }
                                    }
                                }
                                if (!_targetRT.PrepareGlobalNormalsAndSuperDetails()) _target.SuperDetailB = tex_prev;
                                _target.Refresh(null, _targetRT);
                            }
                            EditorGUILayout.LabelField("channel", GUILayout.MinWidth(60), GUILayout.MaxWidth(60));
                            checkChange(ref nchannel, (RTPColorChannels)EditorGUILayout.EnumPopup(_target.SuperDetailB_channel));
                            if (nchannel != _target.SuperDetailB_channel)
                            {
                                _target.SuperDetailB_channel = nchannel;
                                _targetRT.PrepareGlobalNormalsAndSuperDetails();
                            }
                            EditorGUILayout.EndHorizontal();
                        } // superdetail mults

                        if (SUPER_DETAIL_MULTS_ENABLED_FIRST || SUPER_DETAIL_MULTS_ENABLED_ADD)
                        {
                            if (GUILayout.Button("Disable superdetails mult (rebuild shaders via RTP_LODmanager)"))
                            {
                                _RTP_LODmanagerScript.RTP_SUPER_DETAIL_MULTS_FIRST = false;
                                _RTP_LODmanagerScript.RTP_SUPER_DETAIL_MULTS_ADD = false;
                                if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                                {
                                    Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                    _RTP_LODmanagerScript.dont_sync = true;
                                }
                                //_RTP_LODmanagerScript.RefreshLODlevel();
                                //EditorUtility.SetDirty(_targetRT);
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("This feature is currently disabled.", MessageType.Warning, true);
                            if (GUILayout.Button("Enable superdetails mult (rebuild shaders via RTP_LODmanager)"))
                            {
                                _RTP_LODmanagerScript.RTP_SUPER_DETAIL_MULTS_FIRST = true;
                                _RTP_LODmanagerScript.RTP_SUPER_DETAIL_MULTS_ADD = true;
                                if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                                {
                                    Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                    _RTP_LODmanagerScript.dont_sync = true;
                                }
                                //_RTP_LODmanagerScript.RefreshLODlevel();
                                //EditorUtility.SetDirty(_targetRT);
                            }
                        }

                        GUILayout.Space(15);
                        if (GUILayout.Button("Reset superdetail settings numeric params to default"))
                        {
                            Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                            _target.ReturnToDefaults("superdetail");
                            EditorUtility.SetDirty(_targetRT);
                            _target.Refresh(null, _targetRT);
                        }

                    }
                    else
                    {
                        EditorGUILayout.HelpBox("This feature is currently disabled.", MessageType.Warning, true);
                        if (GUILayout.Button("Enable superdetails (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_SUPER_DETAIL_FIRST = true;
                            _RTP_LODmanagerScript.RTP_SUPER_DETAIL_ADD = true;
                            if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                            //_RTP_LODmanagerScript.RefreshLODlevel();
                            //EditorUtility.SetDirty(_targetRT);
                        }
                    }

                    EditorGUILayout.EndVertical();

        #endregion Settings - superdetail					
                    break;
                case ReliefTerrainSettingsItems.VerticalTex:
        #region Settings - Vertical texture		
                    GUILayout.Space(6);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoVerticalTexture), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Vertical texturing", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;

                    if (RTP_VERTICAL_TEXTURE_FIRST || RTP_VERTICAL_TEXTURE_ADD)
                    {
                        EditorGUILayout.HelpBox("This texture is applied along world y axis using V coord. Pixel color is multiplied, controlled per layer (\"Layer properties\" - Vertical map strength).", MessageType.None, true);

                        bool vertTexUnused = false;
                        //bool AddPass_flag=(_4LAYERS_SHADER_USED && _target.numLayers>4) || (_target.numLayers>8);
                        if (RTP_VERTALPHA_CAUSTICS)
                        {
                            vertTexUnused = true;
                        }
                        if (vertTexUnused)
                        {
                            EditorGUILayout.HelpBox("Shader is configured to take vertical texture data from caustics tex (RGB).", MessageType.Warning, true);
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (!vertTexUnused)
                        {
                            checkChange(ref _target.VerticalTexture, (Texture2D)EditorGUILayout.ObjectField(_target.VerticalTexture, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                        }
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Tiling [m]", GUILayout.MinWidth(145), GUILayout.MaxWidth(145));
                        checkChange(ref _target.VerticalTextureTiling, EditorGUILayout.FloatField(_target.VerticalTextureTiling));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Offset via Perlin normal", GUILayout.MinWidth(145), GUILayout.MaxWidth(145));
                        checkChange(ref _target.VerticalTextureGlobalBumpInfluence, EditorGUILayout.Slider(_target.VerticalTextureGlobalBumpInfluence, 0, 0.1f));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    } // v-tex

                    if (RTP_VERTICAL_TEXTURE_FIRST || RTP_VERTICAL_TEXTURE_ADD)
                    {
                        if (GUILayout.Button("Disable feature (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_VERTICAL_TEXTURE_FIRST = false;
                            _RTP_LODmanagerScript.RTP_VERTICAL_TEXTURE_ADD = false;
                            if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                            //_RTP_LODmanagerScript.RefreshLODlevel();
                            //EditorUtility.SetDirty(_targetRT);
                        }

                        GUILayout.Space(15);
                        if (GUILayout.Button("Reset vertical texture settings numeric params to default"))
                        {
                            Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                            _target.ReturnToDefaults("vertical");
                            EditorUtility.SetDirty(_targetRT);
                            _target.Refresh(null, _targetRT);
                        }

                    }
                    else
                    {
                        EditorGUILayout.HelpBox("This feature is currently disabled.", MessageType.Warning, true);
                        if (GUILayout.Button("Enable feature (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_VERTICAL_TEXTURE_FIRST = true;
                            _RTP_LODmanagerScript.RTP_VERTICAL_TEXTURE_ADD = true;
                            if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                            //_RTP_LODmanagerScript.RefreshLODlevel();
                            //EditorUtility.SetDirty(_targetRT);
                        }
                    }

                    EditorGUILayout.EndVertical();

        #endregion Settings - Vertical texture
                    break;

                case ReliefTerrainSettingsItems.Snow:
        #region Settings - Snow
                    GUILayout.Space(6);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoSnow), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Snow settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;

                    if (SNOW_ENABLED_FIRST || SNOW_ENABLED_ADD)
                    {
                        EditorGUILayout.HelpBox("Snow settings are set globally and its params are visible in any shader using RTP dynamic snow feature (Bonus snow shaders). This also means that you can't use different snow settings on multiple terrains (Unity4).", MessageType.Warning, true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Strength", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_strength, EditorGUILayout.Slider(_target._snow_strength, 0, 1));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.HelpBox("Global color influence means the darker is global color the more snow is present.", MessageType.None, true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Global color influence", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._global_color_brightness_to_snow, EditorGUILayout.Slider(_target._global_color_brightness_to_snow, 0, 0.3f));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.HelpBox("Reduction by slope is factor for decreasing snow coverage on slopes.", MessageType.None, true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Reduction by slope", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_slope_factor, EditorGUILayout.Slider(_target._snow_slope_factor, 0, 10f));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.HelpBox("Height threshold/transition allows to put snow on choosen height only (i.e. high mountais).", MessageType.None, true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Height threshold [units]", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_height_treshold, EditorGUILayout.Slider(_target._snow_height_treshold, -200, 7000));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Height transition", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_height_transition, EditorGUILayout.Slider(_target._snow_height_transition, 1, 4000));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();

                        EditorGUILayout.HelpBox("Color / spec / gloss are used for far distance.\nFor near distance you can choose a layer to get its color/normalmap as snow (refer to _RTP_LODmanager game object inspector).", MessageType.None, true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Color", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_color, EditorGUILayout.ColorField(_target._snow_color));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Frost", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_Frost, EditorGUILayout.Slider(_target._snow_Frost, 0f, 1f));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.HelpBox("Glitter and snow use shared texture.", MessageType.None, true);
                        if (_target._SparkleMap == null)
                        {
                            _target._SparkleMap = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Textures/ReliefTerrain/snow_texture_512.png", typeof(Texture2D));
                        }
                        EditorGUILayout.BeginHorizontal();
                        Texture2D tmp_tex = _target._SparkleMap;
                        ntex = tmp_tex;
                        if (checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(_target._SparkleMap, typeof(Texture2D), false, GUILayout.MinHeight(90), GUILayout.MinWidth(90), GUILayout.MaxWidth(90))))
                        {
                            _target._SparkleMap = ntex;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Micro tex tiling", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_MicroTiling, EditorGUILayout.FloatField(_target._snow_MicroTiling));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Micro tex strength", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_BumpMicro, EditorGUILayout.Slider(_target._snow_BumpMicro, 0f, 0.4f));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Snow occlusion", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_occlusionStrength, EditorGUILayout.Slider(_target._snow_occlusionStrength, 0f, 1f));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("UBER transl. (deferred HDR)", GUILayout.MinWidth(200), GUILayout.MaxWidth(200));
                        string[] transl_options = new string[] { "Translucency Setup 1", "Translucency Setup 2", "Translucency Setup 3", "Translucency Setup 4" };
                        checkChange(ref _target._snow_TranslucencyDeferredLightIndex, EditorGUILayout.Popup(_target._snow_TranslucencyDeferredLightIndex, transl_options));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Metallic", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_metallic, EditorGUILayout.Slider(_target._snow_metallic, 0f, 1f));
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.BeginDisabledGroup(!GLITTER_ENABLED_FIRST && !GLITTER_ENABLED_ADD);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Glitter color", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._SnowGlitterColor, EditorGUILayout.ColorField(_target._SnowGlitterColor));
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        //EditorGUILayout.HelpBox("When you're in deferred and snow is present for AddPass you should keep below gloss value similar to \"Gloss for deferred AddPass\" in Settings/Main to avoid overbright specular problems.", MessageType.Warning, true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Gloss", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_gloss, EditorGUILayout.Slider(_target._snow_gloss, 0.01f, 1f));
                        EditorGUILayout.EndHorizontal();
                        //EditorGUILayout.BeginHorizontal();
                        //EditorGUILayout.LabelField("Fresnel", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        //checkChange(ref _target._snow_fresnel, EditorGUILayout.Slider(_target._snow_fresnel, 0f, 1f));
                        //EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Diffuse scattering", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_diff_fresnel, EditorGUILayout.Slider(_target._snow_diff_fresnel, 0f, 2f));
                        EditorGUILayout.EndHorizontal();

                        //bool AddPass_flag = (_4LAYERS_SHADER_USED && _target.numLayers > 4) || (_target.numLayers > 8);

                        //if ((!AddPass_flag && RTP_IBL_DIFFUSE_FIRST) || (AddPass_flag && RTP_IBL_SPEC_ADD))
                        //{
                        //    EditorGUILayout.BeginHorizontal();
                        //    EditorGUILayout.LabelField("IBL diffuse exposure", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        //    checkChange(ref _target._snow_IBL_DiffuseStrength, EditorGUILayout.Slider(_target._snow_IBL_DiffuseStrength, 0f, 8f));
                        //    EditorGUILayout.EndHorizontal();
                        //}
                        //if ((!AddPass_flag && (REFLECTION_ENABLED_FIRST || RTP_IBL_SPEC_FIRST)) || (AddPass_flag && (REFLECTION_ENABLED_FIRST || RTP_IBL_SPEC_FIRST)))
                        //{
                        //    EditorGUILayout.BeginHorizontal();
                        //    if (GUILayout.Button(new GUIContent(icoGlitter, "Reflection settings"), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16)))
                        //    {
                        //        _target.submenu = ReliefTerrainMenuItems.GeneralSettings;
                        //        _target.submenu_settings = ReliefTerrainSettingsItems.Glitter;
                        //    }
                        //    EditorGUILayout.LabelField("IBL spec / Refl exposure", GUILayout.MinWidth(150), GUILayout.MaxWidth(150));
                        //    checkChange(ref _target._snow_IBL_SpecStrength, EditorGUILayout.Slider(_target._snow_IBL_SpecStrength, 0f, 8f));
                        //    EditorGUILayout.EndHorizontal();
                        //}

                        EditorGUILayout.Space();

                        EditorGUILayout.HelpBox("Edges definition makes snow transition smooth (snow color gradualy blends underlying surface) or sharp.", MessageType.None, true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Edges definition", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_edge_definition, EditorGUILayout.Slider(_target._snow_edge_definition, 0f, 4f));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("Deep factor adjusts how fast covered surface will get dedicated normals (from texture) and extrusion height canceled (in POM/PM shading).", MessageType.None, true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Deep factor", GUILayout.MinWidth(140), GUILayout.MaxWidth(140));
                        checkChange(ref _target._snow_deep_factor, EditorGUILayout.Slider(_target._snow_deep_factor, 0, 6));
                        EditorGUILayout.EndHorizontal();

                    } // snow
                    EditorGUILayout.Space();

                    if (SNOW_ENABLED_FIRST || SNOW_ENABLED_ADD)
                    {
                        if (GUILayout.Button("Disable snow (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_SNOW_FIRST = false;
                            _RTP_LODmanagerScript.RTP_SNOW_ADD = false;
                            if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                        }

                        GUILayout.Space(15);
                        if (GUILayout.Button("Reset snow settings numeric params to default"))
                        {
                            Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                            _target.ReturnToDefaults("snow");
                            EditorUtility.SetDirty(_targetRT);
                            _target.Refresh(null, _targetRT);
                        }

                    }
                    else
                    {
                        _target._snow_strength = 1;
                        EditorGUILayout.HelpBox("This feature is currently disabled.", MessageType.Warning, true);
                        if (GUILayout.Button("Enable snow (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_SNOW_FIRST = true;
                            _RTP_LODmanagerScript.RTP_SNOW_ADD = true;
                            if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();

        #endregion Settings - Snow
                    break;

                case ReliefTerrainSettingsItems.Water:
        #region Settings - Water
                    _target.paint_wetmask = true;
                    GUILayout.Space(6);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoWater), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Water settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;

                    if (WATER_ENABLED_FIRST || WATER_ENABLED_ADD)
                    {
                        EditorGUILayout.HelpBox("Water has 3 states due to wetness applied - diffuse color darken for initial water, then surface becomes wet, then water may accumulate in lower parts (lower in terms of heightmap applied).\n\nReflection is taken from reflection map (the same as in Reflection settings). Most params are applied per layer.", MessageType.None, true);
                        checkChange(ref _target.TERRAIN_GlobalWetness, EditorGUILayout.Slider("Global wetness", _target.TERRAIN_GlobalWetness, 0, 1));
                        checkChange(ref _target.TERRAIN_WetHeight_Treshold, EditorGUILayout.Slider("Height threshold [units]", _target.TERRAIN_WetHeight_Treshold, -500, 8000));
                        checkChange(ref _target.TERRAIN_WetHeight_Transition, EditorGUILayout.Slider("Height transition", _target.TERRAIN_WetHeight_Transition, 1, 1000));
                        checkChange(ref _target.TERRAIN_FlowSpeed, EditorGUILayout.Slider("Flow speed", _target.TERRAIN_FlowSpeed, 0, 2f));
                        checkChange(ref _target.TERRAIN_FlowCycleScale, EditorGUILayout.Slider("Flow cycle scale", _target.TERRAIN_FlowCycleScale, 0.5f, 4));
                        EditorGUILayout.BeginHorizontal();
                        checkChange(ref _target.TERRAIN_FlowScale, EditorGUILayout.Slider("Flow tex tiling (" + (Mathf.Round(_target.ReliefTransform.x / _target.TERRAIN_FlowScale * 10) / 10) + " m)", _target.TERRAIN_FlowScale, 0.25f, 8f));
                        //						if (GUILayout.Button(new GUIContent("A","Align to terrain tile"), GUILayout.Width(22), GUILayout.Height(16))) {
                        //							float ratio=Mathf.Round(_target.ReliefTransform.x*_target.TERRAIN_FlowScale);
                        //							_target.TERRAIN_FlowScale=ratio/_target.ReliefTransform.x;
                        //							_target.RefreshAll();
                        //						}
                        EditorGUILayout.EndHorizontal();
                        checkChange(ref _target.TERRAIN_FlowMipOffset, EditorGUILayout.Slider("Flow tex filter", _target.TERRAIN_FlowMipOffset / 4f, 0, 1) * 4f);
                        checkChange(ref _target.TERRAIN_mipoffset_flowSpeed, EditorGUILayout.Slider("Filter by flow speed", _target.TERRAIN_mipoffset_flowSpeed, 0, 4));
                        checkChange(ref _target.TERRAIN_WetDarkening, EditorGUILayout.Slider("Water surface darkening", _target.TERRAIN_WetDarkening, 0.1f, 0.9f));
                        EditorGUILayout.HelpBox("You can exclude any parts from wetness - shelter mask is put on derived/combined texture channel B together with Perlin normal (RG) and reflection map (A) (see Combined textures in RTP main inspector menu tab).\n\nWhen you don't specify texture below it will be treated black(full water).\n\nWhen you start painting the mask will be automatically made for you (with size of Special combined texture).", MessageType.None, true);

                        Texture2D tmp_tex = _targetRT.TERRAIN_WetMask;
                        ntex = tmp_tex;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        Texture2D oRef = ntex;
                        if (checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(_targetRT.TERRAIN_WetMask, typeof(Texture2D), false, GUILayout.MinHeight(140), GUILayout.MinWidth(140), GUILayout.MaxWidth(140))))
                        {
                            _targetRT.RefreshTextures();
                            DestroyUnusedCombinedTexture(oRef);
                        }
                        if (SaveTexture(ref _targetRT.TERRAIN_WetMask, ref _targetRT.save_path_WetMask, "terrain_wetmask.png", 140, TextureImporterCompression.Uncompressed, TextureImporterType.SingleChannel, true))
                        {
                            _targetRT.globalWaterModifed_flag = false;
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginVertical();
                        int fill_flag = 0;
                        if (GUILayout.Button("Dry all terrain"))
                        {
                            fill_flag = 1;
                            _targetRT.RefreshTextures();
                        }
                        if (GUILayout.Button("Fill all terrain with water"))
                        {
                            fill_flag = 2;
                            _targetRT.RefreshTextures();
                        }
                        if (fill_flag > 0)
                        {
                            if (_targetRT.TERRAIN_WetMask) DestroyImmediate(_targetRT.TERRAIN_WetMask, false);
                            if (_targetRT.BumpGlobalCombined)
                            {
                                _targetRT.TERRAIN_WetMask = new Texture2D(_targetRT.BumpGlobalCombined.width, _targetRT.BumpGlobalCombined.height, TextureFormat.Alpha8, false);
                            }
                            else
                            {
                                _targetRT.TERRAIN_WetMask = new Texture2D(1024, 1024, TextureFormat.Alpha8, false);
                            }
                            Color32[] cols = new Color32[_targetRT.TERRAIN_WetMask.width * _targetRT.TERRAIN_WetMask.height];
                            if (fill_flag == 1)
                            {
                                for (int k = 0; k < cols.Length; k++) cols[k] = Color.white;
                            }
                            _targetRT.TERRAIN_WetMask.SetPixels32(cols, 0);
                            _targetRT.TERRAIN_WetMask.Apply(true, false);
                            _targetRT.globalWaterModifed_flag = true;
                            _targetRT.PrepareGlobalNormalsAndSuperDetails(true, false);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        if (ntex != tmp_tex)
                        {
                            Texture2D tex_prev = _targetRT.TERRAIN_WetMask;
                            _targetRT.TERRAIN_WetMask = ntex;
                            if (ntex)
                            {
                                AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(ntex));
                                if (_importer)
                                {
                                    TextureImporter tex_importer = (TextureImporter)_importer;
                                    bool modified_flag = false;
                                    if (!tex_importer.isReadable)
                                    {
                                        Debug.LogWarning("Wet mask texture (" + ntex.name + ") has been reimported as readable.");
                                        tex_importer.isReadable = true;
                                        modified_flag = true;
                                    }
                                    if (tex_importer.textureCompression != TextureImporterCompression.Uncompressed && tex_importer.textureType != TextureImporterType.SingleChannel)
                                    {
                                        Debug.LogWarning("Wet mask texture (" + ntex.name + ") has been reimported as Alpha8.");
                                        tex_importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                                        tex_importer.textureCompression = TextureImporterCompression.Uncompressed;
                                        tex_importer.textureType = TextureImporterType.SingleChannel;
                                        modified_flag = true;
                                    }
                                    if (modified_flag)
                                    {
                                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(ntex), ImportAssetOptions.ForceUpdate);
                                    }
                                }
                            }
                            if (!_targetRT.PrepareGlobalNormalsAndSuperDetails(true, false)) _targetRT.TERRAIN_WetMask = tex_prev;
                            _target.Refresh(null, _targetRT);
                        }

                        //
                        // paint tools
                        //						
                        GUILayout.Space(8);
                        EditorGUILayout.BeginVertical("Box");
                        skin_color = GUI.color;
                        GUI.color = new Color(1, 1, 0.5f, 1);
                        EditorGUILayout.LabelField("Wetmask paint tool", EditorStyles.boldLabel);
                        GUI.color = skin_color;

                        prev_paint_flag = _target.paint_flag;

                        if (!_target.paint_flag)
                        {
                            Color c = GUI.color;
                            GUI.color = new Color(0.9f, 1, 0.9f);
                            if (GUILayout.Button(new GUIContent("Begin painting (M)", icoPaintOn, "Click to turn on painting")))
                            {
                                _target.paint_flag = true;
                                _target.paint_wetmask = true;
                                _target.paint_alpha_flag = true;
                                if (!_targetRT.TERRAIN_WetMask)
                                {
                                    if (_targetRT.BumpGlobalCombined)
                                    {
                                        _targetRT.TERRAIN_WetMask = new Texture2D(_targetRT.BumpGlobalCombined.width, _targetRT.BumpGlobalCombined.height, TextureFormat.Alpha8, false);
                                    }
                                    else
                                    {
                                        _targetRT.TERRAIN_WetMask = new Texture2D(1024, 1024, TextureFormat.Alpha8, false);
                                    }
                                    Color32[] cols = new Color32[_targetRT.TERRAIN_WetMask.width * _targetRT.TERRAIN_WetMask.height];
                                    _targetRT.TERRAIN_WetMask.SetPixels32(cols, 0);
                                    _targetRT.TERRAIN_WetMask.Apply(true, false);
                                }
                                if (!_targetRT.BumpGlobalCombined)
                                {
                                    if (!_targetRT.PrepareGlobalNormalsAndSuperDetails(true, false))
                                    {
                                        _target.paint_flag = false;
                                    }
                                }
                            }
                            if (!_targetRT.GetComponent<Collider>() || !_targetRT.GetComponent<Collider>().enabled) _target.paint_flag = false;
                            GUI.color = c;
                        }
                        else if (_target.paint_flag)
                        {
                            Color c = GUI.color;
                            GUI.color = new Color(1, 0.9f, 0.9f);
                            if (GUILayout.Button(new GUIContent("End painting (M)", icoPaintOff, "Click to turn off painting")))
                            {
                                _target.paint_flag = false;
                            }
                            GUI.color = c;
                        }
                        if (!prev_paint_flag && _target.paint_flag)
                        {
                            UnityEditor.Tools.current = Tool.View;
                            AddSceneViewCallback();
                        }
                        else if (prev_paint_flag && !_target.paint_flag)
                        {
                            UnityEditor.Tools.current = prev_tool;
                            RemoveSceneViewCallback();
                        }
                        if (prev_paint_flag != _target.paint_flag) EditorUtility.SetDirty(target);
                        if (_target.paint_flag)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
                            EditorGUILayout.BeginVertical();

                            if (!_targetRT.GetComponent<Collider>() || !_targetRT.GetComponent<Collider>().enabled) EditorGUILayout.HelpBox("Object doesn't have collider (necessary for painting).", MessageType.Error, true);

                            EditorGUILayout.HelpBox("Hold SHIFT while painting to add water.", MessageType.Info, true);

                            if (_targetRT.tmp_WaterMap && _targetRT.tmp_WaterMap.format != TextureFormat.Alpha8)
                            {
                                EditorGUILayout.HelpBox("Wet mask need to be readable and uncompressed for painting.", MessageType.Error, true);
                            }
                            if (_targetRT.tmp_CombinedMap && _targetRT.tmp_CombinedMap.format != TextureFormat.ARGB32)
                            {
                                EditorGUILayout.HelpBox("Special combined map need to be readable and uncompressed for painting.", MessageType.Error, true);
                            }
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Area size", EditorStyles.label);
                            _target.paint_size = EditorGUILayout.Slider(_target.paint_size, 0.1f, 6);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Area smoothness", EditorStyles.label);
                            _target.paint_smoothness = EditorGUILayout.Slider(_target.paint_smoothness, 0.001f, 1);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Opacity", EditorStyles.label);
                            _target.paint_opacity = EditorGUILayout.Slider(_target.paint_opacity, 0, 1);
                            GUILayout.EndHorizontal();

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(10);
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.HelpBox("Below feature simulates drops on the water surface. Texture used for animation is predefined so you don't have to touch it. Ripples appears on accumulated water surface, for wet surface only when \"Rain on wet\" is positive. Ripples can be also damped on slopes via \"Water level slope damp\" layer param.", MessageType.None, true);
                        if (RIPPLEMAP_ENABLED_FIRST || RIPPLEMAP_ENABLED_ADD)
                        {
                            if (GUILayout.Button("Disable droplets anim (rebuild shaders via RTP_LODmanager)"))
                            {
                                _RTP_LODmanagerScript.RTP_WET_RIPPLE_TEXTURE_FIRST = false;
                                _RTP_LODmanagerScript.RTP_WET_RIPPLE_TEXTURE_ADD = false;
                                if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                                {
                                    Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                    _RTP_LODmanagerScript.dont_sync = true;
                                }
                            }
                        }
                        else
                        {
                            //EditorGUILayout.HelpBox("This feature is currently disabled.",MessageType.Warning, true);
                            if (GUILayout.Button("Enable droplets anim (rebuild shaders via RTP_LODmanager)"))
                            {
                                _RTP_LODmanagerScript.RTP_WET_RIPPLE_TEXTURE_FIRST = true;
                                _RTP_LODmanagerScript.RTP_WET_RIPPLE_TEXTURE_ADD = true;
                                if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                                {
                                    Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                    _RTP_LODmanagerScript.dont_sync = true;
                                }
                            }
                        }
                        EditorGUI.BeginDisabledGroup(!(RIPPLEMAP_ENABLED_FIRST || RIPPLEMAP_ENABLED_ADD));
                        checkChange(ref _target.TERRAIN_RainIntensity, EditorGUILayout.Slider("Rain intensity", _target.TERRAIN_RainIntensity, 0, 1f));
                        checkChange(ref _target.TERRAIN_WetDropletsStrength, EditorGUILayout.Slider("Rain on wet", _target.TERRAIN_WetDropletsStrength, 0, 1));
                        checkChange(ref _target.TERRAIN_DropletsSpeed, EditorGUILayout.Slider("Anim speed", _target.TERRAIN_DropletsSpeed, 1, 30f));
                        EditorGUILayout.BeginHorizontal();
                        checkChange(ref _target.TERRAIN_RippleScale, EditorGUILayout.Slider("Ripple tex tiling (" + (Mathf.Round(_target.ReliefTransform.x / _target.TERRAIN_RippleScale * 10) / 10) + " m)", _target.TERRAIN_RippleScale, 0.25f, 8));
                        //						if (GUILayout.Button(new GUIContent("A","Align to terrain tile"), GUILayout.Width(22), GUILayout.Height(16))) {
                        //							float ratio=Mathf.Round(_target.ReliefTransform.x*_target.TERRAIN_RippleScale);
                        //							_target.TERRAIN_RippleScale=ratio/_target.ReliefTransform.x;
                        //							_target.RefreshAll();
                        //						}
                        EditorGUILayout.EndHorizontal();
                        if ((RIPPLEMAP_ENABLED_FIRST || RIPPLEMAP_ENABLED_ADD) && _target.TERRAIN_RippleMap == null)
                        {
                            _target.TERRAIN_RippleMap = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Textures/ReliefTerrain/water_drops_anim.png", typeof(Texture2D));
                        }
                        checkChange(ref _target.TERRAIN_RippleMap, (Texture2D)EditorGUILayout.ObjectField(_target.TERRAIN_RippleMap, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        _target.TERRAIN_GlobalWetness = 1;
                    }

                    GUILayout.Space(15);
                    if (WATER_ENABLED_FIRST || WATER_ENABLED_ADD)
                    {
                        if (GUILayout.Button("Disable water (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_WETNESS_FIRST = false;
                            _RTP_LODmanagerScript.RTP_WETNESS_ADD = false;
                            if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                        }

                        GUILayout.Space(15);
                        if (GUILayout.Button("Reset water settings numeric params to default"))
                        {
                            Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                            _target.ReturnToDefaults("water");
                            EditorUtility.SetDirty(_targetRT);
                            _target.Refresh(null, _targetRT);
                        }

                    }
                    else
                    {
                        _target.TERRAIN_GlobalWetness = 1;
                        EditorGUILayout.HelpBox("This feature is currently disabled.", MessageType.Warning, true);
                        if (GUILayout.Button("Enable water (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_WETNESS_FIRST = true;
                            _RTP_LODmanagerScript.RTP_WETNESS_ADD = true;
                            if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();

        #endregion Settings - Water

        #region Settings - Caustics
                    GUILayout.Space(6);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoWater), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Caustics settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;

                    if (CAUSTICS_ENABLED_FIRST || CAUSTICS_ENABLED_ADD)
                    {
                        checkChange(ref _target.TERRAIN_CausticsAnimSpeed, EditorGUILayout.Slider("Anim speed", _target.TERRAIN_CausticsAnimSpeed, 0, 10f));
                        checkChange(ref _target.TERRAIN_CausticsColor, EditorGUILayout.ColorField("Color", _target.TERRAIN_CausticsColor));
                        checkChange(ref _target.TERRAIN_CausticsWaterLevelRefObject, (GameObject)EditorGUILayout.ObjectField("Water Level Ref", _target.TERRAIN_CausticsWaterLevelRefObject, typeof(GameObject), true));
                        EditorGUI.BeginDisabledGroup(_target.TERRAIN_CausticsWaterLevelRefObject);
                        checkChange(ref _target.TERRAIN_CausticsWaterLevel, EditorGUILayout.FloatField("Water Level", _target.TERRAIN_CausticsWaterLevel));
                        EditorGUI.EndDisabledGroup();
                        checkChange(ref _target.TERRAIN_CausticsWaterLevelByAngle, EditorGUILayout.Slider("Water level by slope", _target.TERRAIN_CausticsWaterLevelByAngle, 0f, 8f));
                        checkChange(ref _target.TERRAIN_CausticsWaterShallowFadeLength, EditorGUILayout.Slider("Shallow fade length", _target.TERRAIN_CausticsWaterShallowFadeLength, 0.1f, 10f));
                        checkChange(ref _target.TERRAIN_CausticsWaterDeepFadeLength, EditorGUILayout.Slider("Deep fade length", _target.TERRAIN_CausticsWaterDeepFadeLength, 1f, 100f));
                        EditorGUILayout.BeginHorizontal();
                        checkChange(ref _target.TERRAIN_CausticsTilingScale, EditorGUILayout.Slider("Texture tiling  (" + (Mathf.Round(_target.ReliefTransform.x / _target.TERRAIN_CausticsTilingScale * 10) / 10) + " m)", _target.TERRAIN_CausticsTilingScale, 0.5f, 4f));
                        //						if (GUILayout.Button(new GUIContent("A","Align to terrain tile"), GUILayout.Width(22), GUILayout.Height(16))) {
                        //							float ratio=Mathf.Round(_target.ReliefTransform.x*_target.TERRAIN_CausticsTilingScale);
                        //							_target.TERRAIN_CausticsTilingScale=ratio/_target.ReliefTransform.x;
                        //							_target.RefreshAll();
                        //						}
                        EditorGUILayout.EndHorizontal();
                        //bool AddPass_flag=(_4LAYERS_SHADER_USED && _target.numLayers>4) || (_target.numLayers>8);
                        if (RTP_VERTALPHA_CAUSTICS)
                        {
                            EditorGUILayout.HelpBox("Shader is configured to take vertical texture data from caustics tex (RGB). Caustics color will be taken from caustics tex A channel as grayscale.", MessageType.Warning, true);
                        }
                        EditorGUILayout.LabelField("Caustics texture", EditorStyles.boldLabel);
                        checkChange(ref _target.TERRAIN_CausticsTex, (Texture2D)EditorGUILayout.ObjectField(_target.TERRAIN_CausticsTex, typeof(Texture2D), false, GUILayout.MinHeight(100), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)));
                    }

                    GUILayout.Space(15);
                    if (CAUSTICS_ENABLED_FIRST || CAUSTICS_ENABLED_ADD)
                    {
                        if (GUILayout.Button("Disable caustics (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_CAUSTICS_FIRST = false;
                            _RTP_LODmanagerScript.RTP_CAUSTICS_ADD = false;
                            if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                        }

                        GUILayout.Space(15);
                        if (GUILayout.Button("Reset caustics settings numeric params to default"))
                        {
                            Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                            _target.ReturnToDefaults("caustics");
                            EditorUtility.SetDirty(_targetRT);
                            _target.Refresh(null, _targetRT);
                        }

                    }
                    else
                    {
                        EditorGUILayout.HelpBox("This feature is currently disabled.", MessageType.Warning, true);
                        if (GUILayout.Button("Enable caustics (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_CAUSTICS_FIRST = true;
                            _RTP_LODmanagerScript.RTP_CAUSTICS_ADD = true;
                            if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();

        #endregion Settings - Caustics
                    break;

                case ReliefTerrainSettingsItems.Glitter:
        #region Settings - Glitter
                    GUILayout.Space(6);

                    EditorGUILayout.BeginVertical("Box");
                    skin_color = GUI.color;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(icoGlitter), EditorStyles.miniLabel, GUILayout.Width(16), GUILayout.Height(16));
                    GUI.color = new Color(1, 1, 0.5f, 1);
                    EditorGUILayout.LabelField("Glitter settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = skin_color;

                    if (GLITTER_ENABLED_FIRST || GLITTER_ENABLED_ADD)
                    {
                        EditorGUILayout.HelpBox("Glitter and snow use shared texture.", MessageType.None, true);
                        if (_target._SparkleMap == null)
                        {
                            _target._SparkleMap = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Textures/ReliefTerrain/snow_texture_512.png", typeof(Texture2D));
                        }
                        EditorGUILayout.BeginHorizontal();
                        Texture2D tmp_tex = _target._SparkleMap;
                        ntex = tmp_tex;
                        if (checkChange(ref ntex, (Texture2D)EditorGUILayout.ObjectField(_target._SparkleMap, typeof(Texture2D), false, GUILayout.MinHeight(90), GUILayout.MinWidth(90), GUILayout.MaxWidth(90))))
                        {
                            _target._SparkleMap = ntex;
                        }


                        EditorGUILayout.EndHorizontal();

                        checkChange(ref _target._GlitterColor, EditorGUILayout.ColorField("Glitter color", _target._GlitterColor));
                        checkChange(ref _target._GlitterTiling, EditorGUILayout.FloatField("Tiling", _target._GlitterTiling));
                        checkChange(ref _target._GlitterDensity, EditorGUILayout.Slider("Density", _target._GlitterDensity, 0f, 1f));
                        checkChange(ref _target._GlitterFilter, EditorGUILayout.Slider("Filtering", _target._GlitterFilter, -4f, 4f));
                        checkChange(ref _target._GlitterColorization, EditorGUILayout.Slider("Colorization", _target._GlitterColorization, 0f, 1f));
                    }
                    GUILayout.Space(15);
                    if (GLITTER_ENABLED_FIRST || GLITTER_ENABLED_ADD)
                    {
                        if (GUILayout.Button("Disable glitter (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_GLITTER_FIRST = false;
                            _RTP_LODmanagerScript.RTP_GLITTER_ADD = false;
                            if (EditorUtility.DisplayDialog("", "Go to RTP manager now (to recompile shaders) ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                        }

                        GUILayout.Space(15);
                        if (GUILayout.Button("Reset glitter settings numeric params to default"))
                        {
                            Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                            _target.ReturnToDefaults("glitter");
                            EditorUtility.SetDirty(_targetRT);
                            _target.Refresh(null, _targetRT);
                        }

                    }
                    else
                    {
                        EditorGUILayout.HelpBox("This feature is currently disabled.", MessageType.Warning, true);
                        if (GUILayout.Button("Enable glitter (rebuild shaders via RTP_LODmanager)"))
                        {
                            _RTP_LODmanagerScript.RTP_GLITTER_FIRST = true;
                            _RTP_LODmanagerScript.RTP_GLITTER_ADD = true;
                            if (EditorUtility.DisplayDialog("", "Feature is DISABLED in shader until you recompile it.\nGo to RTP manager to do it now ?", "Yes", "No"))
                            {
                                Selection.activeObject = _RTP_LODmanagerScript.gameObject;
                                _RTP_LODmanagerScript.dont_sync = true;
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();

        #endregion Settings - Glitter
                    break;

            }

            GUILayout.Space(8);
            //Begin_Global_Indent();
            if (GUILayout.Button("Reset all numeric params to default"))
            {
                if (EditorUtility.DisplayDialog("Warning", "Are you sure to reset all RTP numeric values to their defaults ?", "Yes", "Cancel"))
                {
                    Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
                    _target.ReturnToDefaults();
                    EditorUtility.SetDirty(_targetRT);
                    _target.Refresh(null, _targetRT);
                }
            }

        #endregion ALLSettings
        }

        if (dirtyFlag)
        {
            _target.SyncGlobalPropsAcrossTerrainGroups();
            EditorUtility.SetDirty(_targetRT);
            _targetRT.RefreshTextures();
            _target.Refresh(null, _targetRT);
        }

        GUILayout.Space(10);
        //DrawDefaultInspector();
        if (_target.activateObject)
        {
            Selection.activeObject = _target.activateObject;
            _target.activateObject = null;
        }

        if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "UndoRedoPerformed")
        {
            //Debug.Log("" + Event.current.commandName);
            _target.RefreshAll();
        }

        Event current = Event.current;
        switch (current.type)
        {
            case EventType.KeyDown:
                if (current.keyCode == KeyCode.M)
                {
                    _target.paint_flag = !_target.paint_flag;
                    if (!_targetRT.GetComponent<Collider>() || !_targetRT.GetComponent<Collider>().enabled) _target.paint_flag = false;
                    if (_target.paint_flag)
                    {
                        if (_target.submenu != ReliefTerrainMenuItems.GeneralSettings || (_target.submenu_settings != ReliefTerrainSettingsItems.GlobalColor && _target.submenu_settings != ReliefTerrainSettingsItems.Water))
                        {
                            _target.paint_flag = false;
                        }
                        if (_target.submenu == ReliefTerrainMenuItems.GeneralSettings)
                        {
                            if (_target.submenu_settings == ReliefTerrainSettingsItems.Water)
                            {
                                if (!_targetRT.PrepareGlobalNormalsAndSuperDetails(true, true))
                                {
                                    _target.paint_flag = false;
                                }
                                _target.paint_wetmask = true;
                                _target.paint_alpha_flag = true;
                            }
                            //							if (_target.submenu_settings==ReliefTerrainSettingsItems.GlobalColor) {
                            //								if (!_target.prepare_tmpTexture(true)) {
                            //									_target.paint_flag=false;
                            //								}
                            //							}							
                        }
                    }
                    if (_target.paint_flag)
                    {
                        UnityEditor.Tools.current = Tool.View;
                        AddSceneViewCallback();
                    }
                    else
                    {
                        UnityEditor.Tools.current = prev_tool;
                        RemoveSceneViewCallback();
                    }
                    EditorUtility.SetDirty(target);
                }
                break;
        }
    }

    private void PrepareAtlases(int which)
    {
        ReliefTerrain _targetRT = (ReliefTerrain)target;
        ReliefTerrainGlobalSettingsHolder _target = _targetRT.globalSettingsHolder;

        int min_size = 9999;
        bool any_splat = false;
        for (int n = which * 4; (n < which * 4 + 4) && (n < _target.numLayers); n++)
        {
            if (_target.splats[n])
            { // czasem moze byc bdnie wprowadzony jako null
                if (_target.splats[n].width < min_size) min_size = _target.splats[n].width;
                any_splat = true;
            }
        }
        if (!any_splat) min_size = 512;
        for (int n = which * 4; (n < which * 4 + 4) && (n < _target.numLayers); n++)
        {
            AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_target.splats[n]));
            if (_importer)
            {
                TextureImporter tex_importer = (TextureImporter)_importer;
                bool reimport_flag = false;
                if (!tex_importer.isReadable)
                {
                    Debug.LogWarning("Detail texture " + n + " (" + _target.splats[n].name + ") has been reimported as readable.");
                    tex_importer.isReadable = true;
                    reimport_flag = true;
                }
                if (_target.splats[n] && _target.splats[n].width > min_size)
                {
                    Debug.LogWarning("Detail texture " + n + " (" + _target.splats[n].name + ") has been reimported with " + min_size + " size.");
                    tex_importer.maxTextureSize = min_size;
                    reimport_flag = true;
                }
                if (reimport_flag)
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_target.splats[n]), ImportAssetOptions.ForceUpdate);
                }
            }
        }

        int i;
        for (i = 0; i < _target.numLayers; i++)
        {
            try
            {
                _target.splats[i].GetPixels(0, 0, 4, 4, 0);
            }
            catch (Exception e)
            {
                Debug.LogError("Splat texture " + i + " has to be marked as isReadable...");
                Debug.LogError(e.Message);
                return;
            }
        }
        int w = _target.splats[0].width;
        for (i = 1; i < _target.numLayers; i++)
        {
            if (_target.splats[i].width != w)
            {
                Debug.LogError("For performance reasons - all splats (detail textures) should have the same size");
                Debug.LogError("Detail tex 0 size=" + w + " while detail tex " + i + " size=" + _target.splats[i].width);
                return;
            }
        }

        ResetProgress((4 + 8), "Padding textures");

        int num = which * 4;
        Texture2D[] splats = new Texture2D[4];
        for (i = 0; i < splats.Length; i++)
        {
            if (num < _target.numLayers && _target.splats[num] != null)
            {
                splats[i] = _target.splats[num];
                splats[i] = PadTex(splats[i], 16);
                CheckProgress();
            }
            else
            {
                splats[i] = new Texture2D(min_size, min_size);
            }
            num++;
        }
        progress_description = "Packing textures";

        if (_target.splat_atlases[which] && AssetDatabase.GetAssetPath(_target.splat_atlases[which]) == "")
        {
            DestroyImmediate(_target.splat_atlases[which]);
        }

        _target.splat_atlases[which] = new Texture2D(splats[0].width * 2, splats[0].width * 2);
        _target.splat_atlases[which].wrapMode = TextureWrapMode.Clamp;
        _target.splat_atlases[which].PackTextures(new Texture2D[4] { splats[0], splats[1], splats[2], splats[3] }, 0, _target.splat_atlases[which].width, false);
        progress_description = "Blending corners";
        CheckProgress();
        //if (splats[0].width<=128) BlendMip(_target.splat_atlases[0], 3, 0.5f);
        //if (splats[0].width<=256) BlendMip(_target.splat_atlases[0], 4, 0.5f);
        //if (splats[0].width<=512) BlendMip(_target.splat_atlases[0], 5, 0.5f);
        BlendMip(_target.splat_atlases[which], 0, 0.5f, 16); BlendMip(_target.splat_atlases[which], 0, 0.5f, 15);
        CheckProgress();
        BlendMip(_target.splat_atlases[which], 1, 0.5f, 8); BlendMip(_target.splat_atlases[which], 1, 0.5f, 7);
        CheckProgress();
        BlendMip(_target.splat_atlases[which], 2, 0.5f, 4); BlendMip(_target.splat_atlases[which], 2, 0.5f, 3);
        CheckProgress();
        BlendMip(_target.splat_atlases[which], 3, 0.5f, 2); BlendMip(_target.splat_atlases[which], 3, 0.5f, 1);
        CheckProgress();
        BlendMip(_target.splat_atlases[which], 4, 0.5f, 2); BlendMip(_target.splat_atlases[which], 4, 0.5f, 1);
        CheckProgress();
        BlendMip(_target.splat_atlases[which], 5, 0.5f, 1); BlendMip(_target.splat_atlases[which], 5, 0.5f, 0);
        CheckProgress();
        for (i = 6; i < _target.splat_atlases[which].mipmapCount - 2; i++) BlendMip(_target.splat_atlases[which], i, 0.5f);
        progress_description = "Packing textures";
        CheckProgress();
        _target.splat_atlases[which].Compress(true);
        _target.splat_atlases[which].Apply(false, false);
        _target.splat_atlases[which].filterMode = FilterMode.Trilinear;
        _target.splat_atlases[which].anisoLevel = 0;

        for (i = 0; i < splats.Length; i++)
        {
            DestroyImmediate(splats[i]);
        }
        EditorUtility.ClearProgressBar();

    }

    private void MakeCombinedGrayscale(int beg, ref Texture2D SSColorCombined)
    {
        ReliefTerrain _targetRT = (ReliefTerrain)target;
        ReliefTerrainGlobalSettingsHolder _target = _targetRT.globalSettingsHolder;

        for (int n = beg; n < _target.numLayers; n++)
        {
            int min_size = 9999;
            for (int m = (n / 4) * 4; (m < ((n / 4) * 4 + 4)) && (m < _target.numLayers); m++)
            {
                if (_target.splats[m])
                { // czasem moze byc bdnie wprowadzony jako null
                    if (_target.splats[m].width < min_size) min_size = _target.splats[m].width;
                }
            }
            AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_target.splats[n]));
            if (_importer)
            {
                TextureImporter tex_importer = (TextureImporter)_importer;
                bool reimport_flag = false;
                if (!tex_importer.isReadable)
                {
                    Debug.LogWarning("Detail texture " + n + " (" + _target.splats[n].name + ") has been reimported as readable.");
                    tex_importer.isReadable = true;
                    reimport_flag = true;
                }
                if (_target.splats[n] && _target.splats[n].width > min_size)
                {
                    Debug.LogWarning("Detail texture " + n + " (" + _target.splats[n].name + ") has been reimported with " + min_size + " size.");
                    tex_importer.maxTextureSize = min_size;
                    reimport_flag = true;
                }
                if (reimport_flag)
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_target.splats[n]), ImportAssetOptions.ForceUpdate);
                }
            }
        }

        int i;
        for (i = 0; i < _target.numLayers; i++)
        {
            try
            {
                _target.splats[i].GetPixels(0, 0, 4, 4, 0);
            }
            catch (Exception e)
            {
                Debug.LogError("Splat texture " + i + " has to be marked as isReadable...");
                Debug.LogError(e.Message);
                return;
            }
        }
        int w = _target.splats[0].width;
        for (i = 1; i < _target.numLayers; i++)
        {
            if (_target.splats[i].width != w)
            {
                Debug.LogError("For performance reasons - all splats (detail textures) should have the same size");
                Debug.LogError("Detail tex 0 size=" + w + " while detail tex " + i + " size=" + _target.splats[i].width);
                return;
            }
        }

        int num = _target.numLayers <= 8 ? _target.numLayers : 8;
        Texture2D[] splats = new Texture2D[4];
        for (i = 0; i < splats.Length; i++)
        {
            if (i < 4)
            {
                if (i < num)
                {
                    splats[i] = _target.splats[i + beg];
                }
                else
                {
                    splats[i] = new Texture2D(_target.splats[num - 1].width, _target.splats[num - 1].width);
                }
            }
        }
        Color[] tex0 = splats[0].GetPixels(0);
        Color[] tex1 = splats[1].GetPixels(0);
        Color[] tex2 = splats[2].GetPixels(0);
        Color[] tex3 = splats[3].GetPixels(0);
        Color[] result = new Color[tex0.Length];
        for (i = 0; i < tex0.Length; i++)
        {
            result[i] = new Color(tex0[i].grayscale, tex1[i].grayscale, tex2[i].grayscale, tex3[i].grayscale);
        }
        SSColorCombined = new Texture2D(splats[0].width, splats[0].height, TextureFormat.ARGB32, true, true);
        SSColorCombined.SetPixels(result);
        SSColorCombined.Apply(true, false);
        SSColorCombined.Compress(true);
    }

    private ushort[][,] ConvertTextureToArray(Texture2D tex)
    {
        ushort[][,] tab = new ushort[4][,];
        for (int i = 0; i < 4; i++)
            tab[i] = new ushort[tex.width, tex.height];

        Color32[] cols = tex.GetPixels32();
        for (int j = 0; j < tex.height; j++)
        {
            int idx = j * tex.width;
            for (int i = 0; i < tex.width; i++)
            {
                tab[0][i, j] = cols[idx].r;
                tab[1][i, j] = cols[idx].g;
                tab[2][i, j] = cols[idx].b;
                tab[3][i, j] = cols[idx].a;
                idx++;
            }
        }
        return tab;
    }

    private Texture2D PadTex(Texture2D splat, int padding, bool updateMips = true)
    {
        int size = splat.width;
        int size_padded = size - padding * 2;

        ResamplingService resamplingService = new ResamplingService();
        resamplingService.Filter = ResamplingFilters.Lanczos3;
        ushort[][,] input = ConvertTextureToArray((Texture2D)splat);
        ushort[][,] output = resamplingService.Resample(input, size_padded, size_padded);
        for (int c = 0; c < 4; c++)
        {
            for (int i = 0; i < size_padded; i++)
            {
                for (int j = 0; j < size_padded; j++)
                {
                    if (output[c][i, j] > 255) output[c][i, j] = 255;
                }
            }
        }

        Color32[] cols = new Color32[size * size];
        for (int j = 0; j < size; j++)
        {
            int j_idx1 = j * size;
            int j_idx2 = ((j + size_padded - padding) % size_padded);
            for (int i = 0; i < size; i++)
            {
                int i2 = (i + size_padded - padding) % size_padded;
                cols[j_idx1].r = (byte)output[0][i2, j_idx2];
                cols[j_idx1].g = (byte)output[1][i2, j_idx2];
                cols[j_idx1].b = (byte)output[2][i2, j_idx2];
                cols[j_idx1].a = (byte)output[3][i2, j_idx2];
                j_idx1++;
            }
        }
        if (updateMips)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, true);
            tex.SetPixels32(cols);
            tex.Apply(true, false);
            return tex;
        }
        else
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            tex.SetPixels32(cols);
            tex.Apply(false, false);
            return tex;
        }
    }

    private void BlendMip(Texture2D atlas, int mip, float blend, int pad_offset = 0)
    {
        Color32[] cols = atlas.GetPixels32(mip);
        int size = Mathf.FloorToInt(Mathf.Sqrt((float)cols.Length));
        int half_size = size / 2;
        int idx_offA, idx_offB;
        idx_offA = pad_offset * size;
        idx_offB = (half_size - 1 - pad_offset) * size;
        for (int i = 0; i < size; i++)
        {
            Color32 colA = cols[idx_offA];
            Color32 colB = cols[idx_offB];
            cols[idx_offA] = Color32.Lerp(colA, colB, blend);
            cols[idx_offB] = Color32.Lerp(colB, colA, blend);
            idx_offA++;
            idx_offB++;
        }
        idx_offA = (half_size + pad_offset) * size;
        idx_offB = (size - 1 - pad_offset) * size;
        for (int i = 0; i < size; i++)
        {
            Color32 colA = cols[idx_offA];
            Color32 colB = cols[idx_offB];
            cols[idx_offA] = Color32.Lerp(colA, colB, blend);
            cols[idx_offB] = Color32.Lerp(colB, colA, blend);
            idx_offA++;
            idx_offB++;
        }
        idx_offA = pad_offset;
        idx_offB = half_size - 1 - pad_offset;
        for (int i = 0; i < size; i++)
        {
            Color32 colA = cols[idx_offA];
            Color32 colB = cols[idx_offB];
            cols[idx_offA] = Color32.Lerp(colA, colB, blend);
            cols[idx_offB] = Color32.Lerp(colB, colA, blend);
            idx_offA += size;
            idx_offB += size;
        }
        idx_offA = half_size + pad_offset;
        idx_offB = size - 1 - pad_offset;
        for (int i = 0; i < size; i++)
        {
            Color32 colA = cols[idx_offA];
            Color32 colB = cols[idx_offB];
            cols[idx_offA] = Color32.Lerp(colA, colB, blend);
            cols[idx_offB] = Color32.Lerp(colB, colA, blend);
            idx_offA += size;
            idx_offB += size;
        }
        atlas.SetPixels32(cols, mip);
    }

    public void colorAlphaFromSplat(int splat_control_tex_idx)
    {
        ReliefTerrain _target = (ReliefTerrain)target;
        Color[] cols;

        if (!_target.ColorGlobal) return;
        _target.prepare_tmpTexture(true);
        if (!_target.ColorGlobal) return;
        if ((_target.ColorGlobal.format != TextureFormat.Alpha8) && (_target.ColorGlobal.format != TextureFormat.ARGB32))
        {
            try
            {
                _target.ColorGlobal.GetPixels(0, 0, 4, 4, 0);
            }
            catch (Exception e)
            {
                Debug.LogError("Global ColorMap has to be marked as isReadable...");
                Debug.LogError(e.Message);
            }
            Texture2D tmp_globalColorMap;
            if (_target.ColorGlobal.format == TextureFormat.Alpha8)
            {
                tmp_globalColorMap = new Texture2D(_target.ColorGlobal.width, _target.ColorGlobal.height, TextureFormat.Alpha8, true);
            }
            else
            {
                tmp_globalColorMap = new Texture2D(_target.ColorGlobal.width, _target.ColorGlobal.height, TextureFormat.ARGB32, true, false);
            }
            cols = _target.ColorGlobal.GetPixels();
            _target.ColorGlobal = tmp_globalColorMap;
        }
        else
        {
            cols = _target.ColorGlobal.GetPixels();
        }

        if (splat_control_tex_idx < _target.globalSettingsHolder.numLayers)
        {

            Texture2D splatA = _target.controlA;
            Texture2D splatB = (_target.globalSettingsHolder.numLayers <= 8) ? _target.controlB : _target.controlC;
            float[,] blured_sum = new float[_target.ColorGlobal.width, _target.ColorGlobal.height];
            int idx;

            if (splatA.width != _target.ColorGlobal.width || splatA.height != _target.ColorGlobal.height)
            {
                ResamplingService resamplingService = new ResamplingService();
                resamplingService.Filter = ResamplingFilters.Quadratic;
                ushort[][,] input = ConvertTextureToArray((Texture2D)splatA);
                ushort[][,] output = resamplingService.Resample(input, _target.ColorGlobal.width, _target.ColorGlobal.height);
                for (int i = 0; i < _target.ColorGlobal.width; i++)
                {
                    for (int j = 0; j < _target.ColorGlobal.height; j++)
                    {
                        blured_sum[i, j] = (output[0][i, j] + output[1][i, j] + output[2][i, j] + output[3][i, j]) / 255.0f;
                    }
                }
            }
            else
            {
                Color32[] cols_splat = splatA.GetPixels32();
                idx = 0;
                for (int i = 0; i < _target.ColorGlobal.width; i++)
                {
                    for (int j = 0; j < _target.ColorGlobal.height; j++)
                    {
                        blured_sum[i, j] = (cols_splat[idx].r + cols_splat[idx].g + cols_splat[idx].b + cols_splat[idx].a) / 255.0f;
                        idx++;
                    }
                }
            }
            if (splatB.width != _target.ColorGlobal.width || splatB.height != _target.ColorGlobal.height)
            {
                ResamplingService resamplingService = new ResamplingService();
                resamplingService.Filter = ResamplingFilters.Quadratic;
                ushort[][,] input = ConvertTextureToArray((Texture2D)splatB);
                ushort[][,] output = resamplingService.Resample(input, _target.ColorGlobal.width, _target.ColorGlobal.height);
                for (int i = 0; i < _target.ColorGlobal.width; i++)
                {
                    for (int j = 0; j < _target.ColorGlobal.height; j++)
                    {
                        blured_sum[i, j] = Mathf.Max(blured_sum[i, j], (output[0][i, j] + output[1][i, j] + output[2][i, j] + output[3][i, j]) / 255.0f);
                    }
                }
            }
            else
            {
                Color32[] cols_splat = splatB.GetPixels32();
                idx = 0;
                for (int i = 0; i < _target.ColorGlobal.width; i++)
                {
                    for (int j = 0; j < _target.ColorGlobal.height; j++)
                    {
                        blured_sum[i, j] = Mathf.Max(blured_sum[i, j], (cols_splat[idx].r + cols_splat[idx].g + cols_splat[idx].b + cols_splat[idx].a) / 255.0f);
                        idx++;
                    }
                }
            }
            for (int i = 0; i < _target.ColorGlobal.width; i++)
            {
                for (int j = 0; j < _target.ColorGlobal.height; j++)
                {
                    if (blured_sum[i, j] > 1) blured_sum[i, j] = 1; else blured_sum[i, j] = blured_sum[i, j];
                }
            }

            float[,] blured_sum2 = new float[_target.ColorGlobal.width, _target.ColorGlobal.height];
            float[,] tmp_blured_sum;
            for (int blur_cnt = 0; blur_cnt < 4; blur_cnt++)
            {
                for (int i = 1; i < _target.ColorGlobal.width - 1; i++)
                {
                    for (int j = 1; j < _target.ColorGlobal.height - 1; j++)
                    {
                        blured_sum2[i, j] = 0.7f * (blured_sum[i + 1, j] + blured_sum[i - 1, j] + blured_sum[i, j + 1] + blured_sum[i, j - 1]);
                        blured_sum2[i, j] += 0.3f * (blured_sum[i + 1, j + 1] + blured_sum[i + 1, j - 1] + blured_sum[i - 1, j + 1] + blured_sum[i - 1, j - 1]);
                        blured_sum2[i, j] *= 0.25f;
                    }
                }
                tmp_blured_sum = blured_sum;
                blured_sum = blured_sum2;
                blured_sum2 = tmp_blured_sum;
            }
            idx = 0;
            for (int i = 0; i < _target.ColorGlobal.width; i++)
            {
                for (int j = 0; j < _target.ColorGlobal.height; j++)
                {
                    cols[idx].a = blured_sum[i, j];
                    cols[idx].a *= cols[idx].a;
                    cols[idx].a *= cols[idx].a;
                    if (cols[idx].a < 0.008f) cols[idx].a = 0.008f;
                    idx++;
                }
            }

            _target.ColorGlobal.SetPixels(cols);
            _target.ColorGlobal.Apply(true, false);
        }
    }
#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
    private bool checkChange(ref ProceduralMaterial val, ProceduralMaterial nval)
    {
        bool changed = (nval != val);
        if (changed)
        {
            if ((EditorApplication.timeSinceStartup - changedTime) > 0.5)
            {
                changedTime = EditorApplication.timeSinceStartup;
                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
            }
        }
        dirtyFlag = dirtyFlag || changed;
        val = nval;
        return changed;
    }
#endif
    private bool checkChange(ref float val, float nval)
    {
        bool changed = (nval != val);
        if (changed)
        {
            if ((EditorApplication.timeSinceStartup - changedTime) > 0.5)
            {
                changedTime = EditorApplication.timeSinceStartup;
                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
            }
        }
        dirtyFlag = dirtyFlag || changed;
        val = nval;
        return changed;
    }
    private bool checkChange(ref int val, int nval)
    {
        bool changed = (nval != val);
        if (changed)
        {
            if ((EditorApplication.timeSinceStartup - changedTime) > 0.5)
            {
                changedTime = EditorApplication.timeSinceStartup;
                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
            }
        }
        dirtyFlag = dirtyFlag || changed;
        val = nval;
        return changed;
    }
    private bool checkChange(ref Texture2D val, Texture2D nval)
    {
        bool changed = (nval != val);
        if (changed)
        {
            if ((EditorApplication.timeSinceStartup - changedTime) > 0.5)
            {
                changedTime = EditorApplication.timeSinceStartup;
                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
            }
        }
        dirtyFlag = dirtyFlag || changed;
        val = nval;
        return changed;
    }

    private bool checkChange(ref Cubemap val, Cubemap nval)
    {
        bool changed = (nval != val);
        if (changed)
        {
            if ((EditorApplication.timeSinceStartup - changedTime) > 0.5)
            {
                changedTime = EditorApplication.timeSinceStartup;
                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
            }
        }
        dirtyFlag = dirtyFlag || changed;
        val = nval;
        return changed;
    }
    private bool checkChange(ref Color val, Color nval)
    {
        bool changed = (nval != val);
        if (changed)
        {
            if ((EditorApplication.timeSinceStartup - changedTime) > 0.5)
            {
                changedTime = EditorApplication.timeSinceStartup;
                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
            }
        }
        dirtyFlag = dirtyFlag || changed;
        val = nval;
        return changed;
    }
    private bool checkChange(ref RTPColorChannels val, RTPColorChannels nval)
    {
        bool changed = (nval != val);
        if (changed)
        {
            if ((EditorApplication.timeSinceStartup - changedTime) > 0.5)
            {
                changedTime = EditorApplication.timeSinceStartup;
                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
            }
        }
        dirtyFlag = dirtyFlag || changed;
        val = nval;
        return changed;
    }
    private bool checkChange(ref bool val, bool nval)
    {
        bool changed = (nval != val);
        if (changed)
        {
            if ((EditorApplication.timeSinceStartup - changedTime) > 0.5)
            {
                changedTime = EditorApplication.timeSinceStartup;
                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
            }
        }
        dirtyFlag = dirtyFlag || changed;
        val = nval;
        return changed;
    }
    private bool checkChange(ref GameObject val, GameObject nval)
    {
        bool changed = (nval != val);
        if (changed)
        {
            if ((EditorApplication.timeSinceStartup - changedTime) > 0.5)
            {
                changedTime = EditorApplication.timeSinceStartup;
                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
            }
        }
        dirtyFlag = dirtyFlag || changed;
        val = nval;
        return changed;
    }
    private bool checkChange(ref float valA, float nvalA, ref float valB, float nvalB)
    {
        bool changed = (nvalA != valA) || (nvalB != valB);
        if (changed)
        {
            if ((EditorApplication.timeSinceStartup - changedTime) > 0.5)
            {
                changedTime = EditorApplication.timeSinceStartup;
                Undo.RecordObject((ReliefTerrain)target, "Undo relief terrain edit");
            }
        }
        dirtyFlag = dirtyFlag || changed;
        valA = nvalA;
        valB = nvalB;
        return changed;
    }

#if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
    private Texture2D GetSubstanceTex(string gen_name, string shad_name, int n, bool normal_flag = false)
    {
        Texture2D ntex;
        ReliefTerrain _targetRT = (ReliefTerrain)target;
        ReliefTerrainGlobalSettingsHolder _target = _targetRT.globalSettingsHolder;

        Texture[] gen_texs = _target.Substances[n].GetGeneratedTextures();
        ProceduralTexture ptex = (ProceduralTexture)gen_texs[0];
        for (int tidx = 1; tidx < gen_texs.Length; tidx++)
        {
            if (gen_texs[tidx].name.IndexOf(gen_name) >= 0)
            {
                ptex = (ProceduralTexture)gen_texs[tidx];
                break;
            }
        }
        Color32[] cols = ptex.GetPixels32(0, 0, ptex.width, ptex.height);
        if (normal_flag)
        {
            ntex = new Texture2D(ptex.width, ptex.height, TextureFormat.RGB24, false);
            for (int i = 0; i < cols.Length; i++)
            {
                int cz = Mathf.FloorToInt(255 - Mathf.Sqrt(cols[i].g * cols[i].g + cols[i].a * cols[i].a));
                if (cz < 0) cz = 0; else if (cz > 255) cz = 255;
                cols[i] = new Color32(cols[i].a, cols[i].g, (byte)cz, 0);
            }
        }
        else
        {
            ntex = new Texture2D(ptex.width, ptex.height, TextureFormat.ARGB32, false);
        }
        ntex.SetPixels32(cols);
        ntex.Apply(true, false);


        return ntex;
    }
#endif

    bool PrepareHeights(int num)
    {
        ReliefTerrain _targetRT = (ReliefTerrain)target;
        ReliefTerrainGlobalSettingsHolder _target = _targetRT.globalSettingsHolder;

        Texture2D[] Heights = _target.Heights;
        if (Heights == null) return false;

        for (int n = 0; n < ((Heights.Length < _target.numLayers) ? Heights.Length : _target.numLayers); n++)
        {
            int min_size = 9999;
            for (int m = (n / 4) * 4; (m < ((n / 4) * 4 + 4)) && (m < Heights.Length); m++)
            {
                if (Heights[m])
                {
                    if (Heights[m].width < min_size) min_size = Heights[m].width;
                }
            }
            AssetImporter _importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Heights[n]));
            if (_importer)
            {
                TextureImporter tex_importer = (TextureImporter)_importer;
                bool reimport_flag = false;
                if (!tex_importer.isReadable)
                {
                    Debug.LogWarning("Height texture " + n + " (" + Heights[n].name + ") has been reimported as readable.");
                    tex_importer.isReadable = true;
                    reimport_flag = true;
                }
                if (!tex_importer.DoesSourceTextureHaveAlpha() && tex_importer.alphaSource!=TextureImporterAlphaSource.FromGrayScale)
                {
                    Debug.LogWarning("Height texture " + n + " (" + Heights[n].name + ") has been reimported to have alpha channel.");
                    tex_importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                    tex_importer.textureType = TextureImporterType.SingleChannel;
                    tex_importer.textureCompression = TextureImporterCompression.Uncompressed;
                    reimport_flag = true;
                }
                if (Heights[n] && Heights[n].width > min_size)
                {
                    Debug.LogWarning("Height texture " + n + " (" + Heights[n].name + ") has been reimported with " + min_size + " size.");
                    tex_importer.maxTextureSize = min_size;
                    reimport_flag = true;
                }
                if (reimport_flag)
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Heights[n]), ImportAssetOptions.ForceUpdate);
                }
            }
        }

        Texture2D[] heights = new Texture2D[4];
        int i;
        int _w = 256;
        int _w_idx = 0;
        int len = _target.numLayers < 12 ? _target.numLayers : 12;
        if (num >= len) return false;
        num = (num >> 2) * 4;
        for (i = num; i < num + 4; i++)
        {
            if (num < 4)
            {
                heights[i] = i < Heights.Length ? Heights[i] : null;
                if (heights[i])
                {
                    _w = heights[i].width;
                    _w_idx = i;
                }
            }
            else if (num < 8)
            {
                heights[i - 4] = i < Heights.Length ? Heights[i] : null;
                if (heights[i - 4])
                {
                    _w = heights[i - 4].width;
                    _w_idx = i;
                }
            }
            else
            {
                heights[i - 8] = i < Heights.Length ? Heights[i] : null;
                if (heights[i - 8])
                {
                    _w = heights[i - 8].width;
                    _w_idx = i;
                }
            }
        }
        for (i = 0; i < 4; i++)
        {
            if (!heights[i])
            {
                heights[i] = new Texture2D(_w, _w);
                FillTex(heights[i], new Color32(255, 255, 255, 255));
            }
            if (heights[i])
            {
                try
                {
                    heights[i].GetPixels(0, 0, 4, 4, 0);
                }
                catch (Exception e)
                {
                    Debug.LogError("Height texture " + i + " has to be marked as isReadable...");
                    Debug.LogError(e.Message);
                    _target.activateObject = heights[i];
                    return false;
                }
                if (heights[i].width != _w)
                {
                    Debug.LogError("Height textures should have the same size ! (check layer " + _w_idx + " and " + (num + i) + ")");
                    _target.activateObject = heights[i];
                    return false;
                }
            }
            else
            {
                heights[i] = new Texture2D(_w, _w);
                FillTex(heights[i], new Color32(255, 255, 255, 255));
            }
        }

        if (num < 4)
        {
            _target.HeightMap = CombineHeights(heights[0], heights[1], heights[2], heights[3]);
        }
        else if (num < 8)
        {
            _target.HeightMap2 = CombineHeights(heights[0], heights[1], heights[2], heights[3]);
        }
        else if (num < 12)
        {
            _target.HeightMap3 = CombineHeights(heights[0], heights[1], heights[2], heights[3]);
        }
        return true;
    }

    private Texture2D CombineHeights(Texture2D source_tex0, Texture2D source_tex1, Texture2D source_tex2, Texture2D source_tex3)
    {
        Texture2D rendered_tex = new Texture2D(source_tex0.width, source_tex0.height, TextureFormat.ARGB32, true, true);
        byte[] colsR = get_alpha_channel(source_tex0);
        byte[] colsG = get_alpha_channel(source_tex1);
        byte[] colsB = get_alpha_channel(source_tex2);
        byte[] colsA = get_alpha_channel(source_tex3);
        int len = colsR.Length;
        if ((colsG.Length != len) || (colsB.Length != len) || (colsA.Length != len))
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("Error", "All detail heightmaps should have THE SAME size. Check it and try again.", "OK");
#endif
        }
        Color32[] cols = rendered_tex.GetPixels32();
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].r = colsR[i];
            cols[i].g = colsG[i];
            cols[i].b = colsB[i];
            cols[i].a = colsA[i];
        }
        rendered_tex.SetPixels32(cols);
        rendered_tex.Apply(true, false);
        //rendered_tex.Compress(true);
        //rendered_tex.Apply(true, true); // (non readable robione przy publishingu)
        rendered_tex.filterMode = FilterMode.Trilinear;
        return rendered_tex;
    }

    private byte[] get_alpha_channel(Texture2D source_tex)
    {
        Color32[] cols = source_tex.GetPixels32();
        byte[] ret = new byte[cols.Length];
        for (int i = 0; i < cols.Length; i++) ret[i] = cols[i].a;
        return ret;
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

    private bool SaveTexture(ref Texture2D tex, ref string save_path, string default_name, int buttonwidth, TextureImporterCompression textureCompression, TextureImporterType textureType, bool mipmapEnabled, bool button_triggered = true, bool sRGB_flag = false)
    {
        //		if (tex==null) return;			
        //		if (AssetDatabase.GetAssetPath(tex)!="") return;
        EditorGUI.BeginDisabledGroup(tex == null || AssetDatabase.GetAssetPath(tex) != "");
        bool saved = false;
        bool cond;
        if (button_triggered)
        {
            cond = GUILayout.Button("Save to file", GUILayout.MaxWidth(buttonwidth));
        }
        else
        {
            cond = true;
        }
        if (cond)
        {
            string directory;
            string file;
            if (save_path == "")
            {
                directory = Application.dataPath;
                file = default_name;
            }
            else
            {
                directory = Path.GetDirectoryName(save_path);
                file = Path.GetFileNameWithoutExtension(save_path) + ".png";
            }
            string path = EditorUtility.SaveFilePanel("Save texture", directory, file, "png");
            if (path != "")
            {
                path = path.Substring(Application.dataPath.Length - 6);
                save_path = path;
                // kopia, bo mozemy miec skompresowana texture
                Texture2D ntex = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, true);
                // (we can simply skip higher mips when creating PNG)
                //for (int mip = 0; mip < tex.mipmapCount; mip++)
                {
                    int mip = 0;
                    Color32[] cols = tex.GetPixels32(mip);
                    ntex.SetPixels32(cols, mip);
                    ntex.Apply(false, false);
                }
                byte[] bytes = ntex.EncodeToPNG();
                ntex = null;
                System.IO.File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                textureImporter.textureType = textureType;
                textureImporter.textureCompression = textureCompression;
                textureImporter.mipmapEnabled = mipmapEnabled;
                textureImporter.sRGBTexture = !sRGB_flag;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                if (tex != null)
                {
                    if (AssetDatabase.GetAssetPath(tex) == "")
                    {
                        DestroyImmediate(tex);
                        tex = null;
                    }
                }
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                saved = true;
                EditorUtility.UnloadUnusedAssetsImmediate();
            }
        }
        EditorGUI.EndDisabledGroup();
        return saved;
    }

    public void OnSceneGUI()
    {
        ReliefTerrain _targetRT = (ReliefTerrain)target;
        ReliefTerrainGlobalSettingsHolder _target = _targetRT.globalSettingsHolder;

        Event current = Event.current;

        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.M)
            {
                _target.paint_flag = !_target.paint_flag;
                if (!_targetRT.GetComponent<Collider>() || !_targetRT.GetComponent<Collider>().enabled) _target.paint_flag = false;
                if (_target.paint_flag)
                {
                    if (_target.submenu != ReliefTerrainMenuItems.GeneralSettings || (_target.submenu_settings != ReliefTerrainSettingsItems.GlobalColor && _target.submenu_settings != ReliefTerrainSettingsItems.Water))
                    {
                        _target.paint_flag = false;
                    }
                    if (_target.submenu == ReliefTerrainMenuItems.GeneralSettings)
                    {
                        if (_target.submenu_settings == ReliefTerrainSettingsItems.Water)
                        {
                            if (!_targetRT.PrepareGlobalNormalsAndSuperDetails(true, true))
                            {
                                _target.paint_flag = false;
                            }
                            _target.paint_wetmask = true;
                            _target.paint_alpha_flag = true;
                        }
                        //						if (_target.submenu_settings==ReliefTerrainSettingsItems.GlobalColor) {
                        //							if (!_target.prepare_tmpTexture(true)) {
                        //								_target.paint_flag=false;
                        //							}
                        //						}							
                    }
                }
                if (_target.paint_flag)
                {
                    UnityEditor.Tools.current = Tool.View;
                    AddSceneViewCallback();
                }
                else
                {
                    UnityEditor.Tools.current = prev_tool;
                    RemoveSceneViewCallback();
                }
                EditorUtility.SetDirty(target);
            }
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.L)
        {
            current.Use();
            if (_targetRT.GetComponent<Collider>())
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                paintHitInfo_flag = _targetRT.GetComponent<Collider>().Raycast(ray, out paintHitInfo, Mathf.Infinity);
                if (paintHitInfo_flag)
                {
                    int max_idx = 0;
                    float max_val = 0;
                    if (_targetRT.controlA)
                    {
                        Color c = _targetRT.controlA.GetPixelBilinear(paintHitInfo.textureCoord.x, paintHitInfo.textureCoord.y);
                        if (c.r > max_val) { max_val = c.r; max_idx = 0; }
                        if (c.g > max_val) { max_val = c.g; max_idx = 1; }
                        if (c.b > max_val) { max_val = c.b; max_idx = 2; }
                        if (c.a > max_val) { max_val = c.a; max_idx = 3; }
                    }
                    if (_target.numLayers > 4 && _targetRT.controlB)
                    {
                        Color c = _targetRT.controlB.GetPixelBilinear(paintHitInfo.textureCoord.x, paintHitInfo.textureCoord.y);
                        if (c.r > max_val) { max_val = c.r; max_idx = 4; }
                        if (c.g > max_val) { max_val = c.g; max_idx = 5; }
                        if (c.b > max_val) { max_val = c.b; max_idx = 6; }
                        if (c.a > max_val) { max_val = c.a; max_idx = 7; }
                    }
                    if (_target.numLayers > 8 && _targetRT.controlC)
                    {
                        Color c = _targetRT.controlC.GetPixelBilinear(paintHitInfo.textureCoord.x, paintHitInfo.textureCoord.y);
                        if (c.r > max_val) { max_val = c.r; max_idx = 8; }
                        if (c.g > max_val) { max_val = c.g; max_idx = 9; }
                        if (c.b > max_val) { max_val = c.b; max_idx = 10; }
                        if (c.a > max_val) { max_val = c.a; max_idx = 11; }
                    }

                    _target.submenu = ReliefTerrainMenuItems.Details;
                    _target.show_active_layer = max_idx;
                }
            }
        }
    }

    void AddSceneViewCallback()
    {
        ReliefTerrain.cur_EditorTarget = (ReliefTerrain)target;
        ReliefTerrainEditor.cur_Editor = this;
#if UNITY_2019_1_OR_NEWER
		SceneView.duringSceneGui += CustomOnSceneGUI_S;
#else
        ReliefTerrain._SceneGUI = new SceneView.OnSceneFunc(CustomOnSceneGUI);
		SceneView.onSceneGUIDelegate += ReliefTerrain._SceneGUI;
#endif
    }
    static void RemoveSceneViewCallback()
    {
#if UNITY_2019_1_OR_NEWER
		SceneView.duringSceneGui -= CustomOnSceneGUI_S;
#else
		SceneView.onSceneGUIDelegate -= ReliefTerrain._SceneGUI;
#endif
    }

    // obsolete handler (left for compatibility)
    public void CustomOnSceneGUI(SceneView sceneview)
    {
        ReliefTerrainEditor.CustomOnSceneGUI_Internal(sceneview, (ReliefTerrain)target);
    }

    // new handler used via SceneView.duringSceneGUI
    public static void CustomOnSceneGUI_S(SceneView sceneview)
    {
        if (ReliefTerrain.cur_EditorTarget)
        {
            ReliefTerrainEditor.CustomOnSceneGUI_Internal(sceneview, ReliefTerrain.cur_EditorTarget);
        }
    }

    // new wrapper to handle deprecated and new SceneView handling
    public static void CustomOnSceneGUI_Internal(SceneView sceneview, ReliefTerrain target)
    {
        if (target == null) return;
        ReliefTerrain _targetRT = target;
        ReliefTerrainGlobalSettingsHolder _target = _targetRT.globalSettingsHolder;

        EditorWindow currentWindow = EditorWindow.mouseOverWindow;
        if (!currentWindow) return;

        Event current = Event.current;

        if (current.alt)
        {
            return;
        }
        if (Event.current.button == 1)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        bool paintOff_flag = (UnityEditor.Tools.current != Tool.View);
        if (_target.paint_wetmask)
        {
            paintOff_flag = paintOff_flag || _target.submenu != ReliefTerrainMenuItems.GeneralSettings || _target.submenu_settings != ReliefTerrainSettingsItems.Water;
        }
        else
        {
            paintOff_flag = paintOff_flag || _target.submenu != ReliefTerrainMenuItems.GeneralSettings || _target.submenu_settings != ReliefTerrainSettingsItems.GlobalColor;
        }
        if (paintOff_flag)
        {
            _target.paint_flag = false;
            RemoveSceneViewCallback();
            EditorUtility.SetDirty(target);
            return;
        }

        if (current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        switch (current.type)
        {
            case EventType.KeyDown:
                if (current.keyCode == KeyCode.Escape)
                {
                    _target.paint_flag = false;
                    UnityEditor.Tools.current = prev_tool;
                    RemoveSceneViewCallback();
                    EditorUtility.SetDirty(target);
                }
                break;
        }

        if (current.control)
        {
            if (current.type == EventType.MouseMove)
            {
                if (control_down_flag)
                {
                    control_down_flag = false;
                    EditorUtility.SetDirty(target);
                }
            }
            return;
        }
        control_down_flag = true;

        switch (current.type)
        {
            case EventType.MouseDown:
                get_paint_coverage();
                // Debug.Log(""+cover_verts_num + "  "+ paintHitInfo_flag + _target.prepare_tmpColorMap());
                if (paintHitInfo_flag)
                {
                    if (_targetRT.prepare_tmpTexture(!_target.paint_wetmask))
                    {
                        Undo.RegisterCompleteObjectUndo(_target.paint_wetmask ? _targetRT.tmp_CombinedMap : _targetRT.tmp_globalColorMap, "RTP Paint");
                        _targetRT.modify_blend(!current.shift);
                    }
                }
                else
                {
                    _target.undo_flag = true;
                }
                current.Use();
                break;
            case EventType.MouseDrag:
                get_paint_coverage();
                if (paintHitInfo_flag)
                {
                    if (_target.undo_flag)
                    {
                        if (_targetRT.prepare_tmpTexture(!_target.paint_wetmask))
                        {
                            Undo.RegisterCompleteObjectUndo(_target.paint_wetmask ? _targetRT.tmp_CombinedMap : _targetRT.tmp_globalColorMap, "RTP Paint");
                        }
                        _target.undo_flag = false;
                    }
                }
                if (paintHitInfo_flag)
                {
                    if (_targetRT.prepare_tmpTexture(!_target.paint_wetmask))
                    {
                        _targetRT.modify_blend(!current.shift);
                        //current.Use();
                    }
                }
                break;
            case EventType.MouseMove:
                get_paint_coverage();
                break;
        }

        if (paintHitInfo_flag)
        {
            bool upflag = _target.paint_wetmask ? !current.shift : current.shift;
            if (upflag)
            {
                Handles.color = new Color(1, 0, 0, Mathf.Max(0.1f, _target.paint_opacity * 0.5f));
                Handles.DrawWireDisc(paintHitInfo.point, paintHitInfo.normal, _target.paint_size);
                Handles.color = new Color(1, 0, 0, Mathf.Max(0.6f, _target.paint_opacity));
                Handles.DrawWireDisc(paintHitInfo.point, paintHitInfo.normal, _target.paint_size * Mathf.Max(0.3f, 1 - _target.paint_smoothness));
            }
            else
            {
                Handles.color = new Color(0, 1, 0, Mathf.Max(0.1f, _target.paint_opacity * 0.5f));
                Handles.DrawWireDisc(paintHitInfo.point, paintHitInfo.normal, _target.paint_size);
                Handles.color = new Color(0, 1, 0, Mathf.Max(0.6f, _target.paint_opacity));
                Handles.DrawWireDisc(paintHitInfo.point, paintHitInfo.normal, _target.paint_size * Mathf.Max(0.3f, 1 - _target.paint_smoothness));
            }
        }
        //if (current.shift) current.Use();
    }

    private static void get_paint_coverage()
    {
        if (EditorApplication.timeSinceStartup < lCovTim) return;
        lCovTim = EditorApplication.timeSinceStartup + 0.02;
        ReliefTerrain target = ReliefTerrain.cur_EditorTarget;
        ReliefTerrain _targetRT = target;
        ReliefTerrainGlobalSettingsHolder _target = _targetRT.globalSettingsHolder;
        if (!_targetRT.GetComponent<Collider>()) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        paintHitInfo_flag = _targetRT.GetComponent<Collider>().Raycast(ray, out paintHitInfo, Mathf.Infinity);
        _target.paintHitInfo = paintHitInfo;
        _target.paintHitInfo_flag = paintHitInfo_flag;

        EditorUtility.SetDirty(target);
    }

    private void ResetProgress(int progress_count, string _progress_description = "")
    {
        progress_count_max = progress_count;
        progress_count_current = 0;
        progress_description = _progress_description;
    }

    private void CheckProgress()
    {
        if (((progress_count_current++) % progress_granulation) == (progress_granulation - 1))
        {
            EditorUtility.DisplayProgressBar("Processing...", progress_description, 1.0f * progress_count_current / progress_count_max);
        }

    }

    private void DestroyUnusedCombinedTexture(Texture2D oRef)
    {
        if (oRef != null)
        {
            if (AssetDatabase.GetAssetPath(oRef) == "")
            {
                DestroyImmediate(oRef);
                oRef = null;
            }
        }
    }

#endif
}
