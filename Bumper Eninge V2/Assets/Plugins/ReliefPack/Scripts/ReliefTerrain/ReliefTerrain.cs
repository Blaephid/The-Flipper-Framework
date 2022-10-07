using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
#if UNITY_EDITOR	
using UnityEditor;
#endif

public enum RTPColorChannels {
	R, G, B, A
}

public enum ReliefTerrainMenuItems {
	Details, Control, CombinedTextures, GeneralSettings
}
public enum ReliefTerrainSettingsItems {
	MainSettings, UVblend, GlobalColor, GlobalNormal, Superdetail, POMSettings, VerticalTex, Snow, Water, Glitter
}
public enum ReliefTerrainDerivedTexturesItems {
	Atlasing, Heightmaps, Bumpmaps, Globalnormal
}
public enum ReliefTerrainControlTexturesItems {
	Compose, Acquire, Controlmaps
}

[AddComponentMenu("Relief Terrain/Engine - Terrain or Mesh")]
[ExecuteInEditMode]
public class ReliefTerrain : MonoBehaviour {
	public Texture2D controlA;
	public Texture2D controlB;
	public Texture2D controlC;
	public string save_path_controlA="";
	public string save_path_controlB="";
	public string save_path_controlC="";
	public string save_path_colormap="";
	public string save_path_BumpGlobalCombined="";
	public string save_path_WetMask="";
	
	public Texture2D NormalGlobal;
	public Texture2D TreesGlobal;
	public Texture2D ColorGlobal;
	public Texture2D AmbientEmissiveMap;
	public Texture2D BumpGlobalCombined;
	
	public Texture2D TERRAIN_WetMask;

	public Texture2D tmp_globalColorMap;
	public Texture2D tmp_CombinedMap;
	public Texture2D tmp_WaterMap;
	public bool globalColorModifed_flag=false;
	public bool globalCombinedModifed_flag=false;
	public bool globalWaterModifed_flag=false;	
	
	public bool splat_layer_ordered_mode;
	public RTPColorChannels[] source_controls_channels;
	public int[] splat_layer_seq;
	public float[] splat_layer_boost;
	public bool[] splat_layer_calc;
	public bool[] splat_layer_masked;
	public RTPColorChannels[] source_controls_mask_channels;
	
	public Texture2D[] source_controls;
	public bool[] source_controls_invert;
	public Texture2D[] source_controls_mask;
	public bool[] source_controls_mask_invert;		
	
	public Vector2 customTiling=new Vector2(3,3);
	
	[SerializeField] public ReliefTerrainPresetHolder[] presetHolders;
	
	[SerializeField] public ReliefTerrainGlobalSettingsHolder globalSettingsHolder;

#if UNITY_EDITOR
    public static ReliefTerrain cur_EditorTarget;
    #if !UNITY_2019_1_OR_NEWER
	    public static SceneView.OnSceneFunc _SceneGUI;
    #endif
#endif

    public void GetGlobalSettingsHolder() {
#if UNITY_EDITOR
		if (globalSettingsHolder!=null) {
			// refresh num tiles in case we've just removed all except for this one
			bool IamTerrain=GetComponent(typeof(Terrain));
			if (IamTerrain) {
				ReliefTerrain[] script_objs=(ReliefTerrain[])GameObject.FindObjectsOfType(typeof(ReliefTerrain));
				globalSettingsHolder.numTiles=0;
				for(int p=0; p<script_objs.Length; p++) {
					if ((script_objs[p].transform.parent==transform.parent) && script_objs[p].globalSettingsHolder!=null) {
						if (script_objs[p].globalSettingsHolder!=globalSettingsHolder && script_objs[p].GetComponent(typeof(Terrain))!=null) {
							//Debug.Log("RTP assert - leaving one globalSettingsHolder...");
							globalSettingsHolder=script_objs[p].globalSettingsHolder;
						}
						if (IamTerrain && script_objs[p].GetComponent(typeof(Terrain))!=null) {
							globalSettingsHolder.numTiles++;
						}
					}
				}	
				if (globalSettingsHolder.numTiles==1) {
					// we don't have to use texture redefinitions
					GetSplatsFromGlobalSettingsHolder();
				}
			}
		}
		//Debug.Log (""+globalSettingsHolder.numTiles+" "+globalSettingsHolder.useTerrainMaterial);
#endif
		if (globalSettingsHolder==null) {
			//Debug.Log("E"+name);
			ReliefTerrain[] script_objs=(ReliefTerrain[])GameObject.FindObjectsOfType(typeof(ReliefTerrain));
			bool IamTerrain=GetComponent(typeof(Terrain));
			for(int p=0; p<script_objs.Length; p++) {
				if ((script_objs[p].transform.parent==transform.parent) && script_objs[p].globalSettingsHolder!=null && ((IamTerrain && script_objs[p].GetComponent(typeof(Terrain))!=null) || (!IamTerrain && script_objs[p].GetComponent(typeof(Terrain))==null))) {
					//Debug.Log ("E2 "+script_objs[p].name);
					globalSettingsHolder=script_objs[p].globalSettingsHolder;
					if (globalSettingsHolder.Get_RTP_LODmanagerScript() && !globalSettingsHolder.Get_RTP_LODmanagerScript().RTP_WETNESS_FIRST && !globalSettingsHolder.Get_RTP_LODmanagerScript().RTP_WETNESS_ADD) {
						BumpGlobalCombined=script_objs[p].BumpGlobalCombined;
						globalCombinedModifed_flag=false;
					}
					break;
				}
			}
			if (globalSettingsHolder==null) {
				// there is no globalSettingsHolder object of my type (terrain/mesh) in the hierarchy - I'm first object
				globalSettingsHolder=new ReliefTerrainGlobalSettingsHolder();
				
				if (IamTerrain) {
					globalSettingsHolder.numTiles=0; // will be set to 1 with incrementation below
                    Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
                    globalSettingsHolder.terrainLayers = new TerrainLayer[terrainComp.terrainData.terrainLayers.Length];
                    System.Array.Copy(terrainComp.terrainData.terrainLayers, globalSettingsHolder.terrainLayers, globalSettingsHolder.terrainLayers.Length);
                    globalSettingsHolder.splats = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
                    globalSettingsHolder.Bumps = new Texture2D[terrainComp.terrainData.terrainLayers.Length];
                    globalSettingsHolder.terrainLayers = terrainComp.terrainData.terrainLayers;
                    for (int i = 0; i < terrainComp.terrainData.terrainLayers.Length; i++)
                    {
                        globalSettingsHolder.splats[i] = terrainComp.terrainData.terrainLayers[i].diffuseTexture;
                        globalSettingsHolder.Bumps[i] = terrainComp.terrainData.terrainLayers[i].normalMapTexture;
                    }
#if UNITY_EDITOR
                    globalSettingsHolder.numLayers = terrainComp.terrainData.terrainLayers.Length;
                    globalSettingsHolder.PrepareNormals();
#endif
                }
                else {				
					globalSettingsHolder.splats=new Texture2D[4];
				}
				globalSettingsHolder.numLayers=globalSettingsHolder.splats.Length;
				globalSettingsHolder.ReturnToDefaults();
			} else {
				if (IamTerrain) {
					GetSplatsFromGlobalSettingsHolder();
				}
			}
			
			source_controls_mask=new Texture2D[12];
			source_controls=new Texture2D[12];
			source_controls_channels=new RTPColorChannels[12];
			source_controls_mask_channels=new RTPColorChannels[12];
			
			splat_layer_seq=new int[12] {0,1,2,3,4,5,6,7,8,9,10,11};	
			splat_layer_boost=new float[12] {1,1,1,1,1,1,1,1,1,1,1,1};
			splat_layer_calc=new bool[12];
			splat_layer_masked=new bool[12];
			source_controls_invert=new bool[12];
			source_controls_mask_invert=new bool[12];		
			
			if (IamTerrain) globalSettingsHolder.numTiles++;
		}
	}
	
	private void GetSplatsFromGlobalSettingsHolder() {
        Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
        if (globalSettingsHolder.terrainLayers != null && globalSettingsHolder.terrainLayers.Length == globalSettingsHolder.numLayers && globalSettingsHolder.terrainLayers.Length>0 && globalSettingsHolder.terrainLayers[0] != null)
        {
            if (terrainComp.terrainData.terrainLayers.Length > 0 && terrainComp.terrainData.terrainLayers[0] == null)
            {
                // reassign terrain layer if seems to be needed
                TerrainLayer[] terrainLayers = new TerrainLayer[globalSettingsHolder.numLayers];
                System.Array.Copy(globalSettingsHolder.terrainLayers, terrainLayers, globalSettingsHolder.terrainLayers.Length);
                terrainComp.terrainData.terrainLayers = terrainLayers;
            }
        }
        else
        {
            // reinit/upgrade to U2018.3 situation, find another terrain that uses this settings holder and take its TerrainLayer[] array
            TerrainLayer[] terrainLayers = new TerrainLayer[globalSettingsHolder.numLayers];
            ReliefTerrain[] script_objs = (ReliefTerrain[])GameObject.FindObjectsOfType(typeof(ReliefTerrain));
            bool found = false;
            for (int i = 0; i < script_objs.Length; i++)
            {
                Terrain terrain = script_objs[i].GetComponent<Terrain>();
                if (terrain != null && (script_objs.Length == 1 || script_objs[i] != this) && terrain.terrainData.terrainLayers.Length == terrainLayers.Length)
                {
                    // a candidate to copy layers found
                    globalSettingsHolder.terrainLayers = new TerrainLayer[terrain.terrainData.terrainLayers.Length];
                    System.Array.Copy(terrain.terrainData.terrainLayers, globalSettingsHolder.terrainLayers, globalSettingsHolder.terrainLayers.Length);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                // let's redo layers, but they will be scene objects...
                Debug.LogWarning("TerrainLayers from GlobalSettingsHolder can't be found. Create a set of layers and setup first terrain before adding RTP script to it!");

                for (int i = 0; i < globalSettingsHolder.numLayers; i++)
                {
                    //Debug.Log(""+globalSettingsHolder.splats[i]);
                    terrainLayers[i] = new TerrainLayer();
                    terrainLayers[i].tileSize = Vector2.one;
                    terrainLayers[i].tileOffset = new Vector2(1.0f / customTiling.x, 1.0f / customTiling.y);
                    terrainLayers[i].diffuseTexture = globalSettingsHolder.splats[i];
                    terrainLayers[i].normalMapTexture = globalSettingsHolder.Bumps[i];
                }
            }
            terrainComp.terrainData.terrainLayers = terrainLayers;
        }
    }
	
	public void InitTerrainTileSizes() {
		Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
		if (terrainComp) {
			globalSettingsHolder.terrainTileSize=terrainComp.terrainData.size;
		} else {
			globalSettingsHolder.terrainTileSize=GetComponent<Renderer>().bounds.size;
			globalSettingsHolder.terrainTileSize.y=globalSettingsHolder.tessHeight;
		}
	}

//	void OnDrawGizmos() {
//		MeshRenderer mr = GetComponent<MeshRenderer>();
//		if (mr) {
//			Gizmos.color = Color.yellow;
//			Gizmos.DrawWireCube (mr.bounds.center, mr.bounds.extents*2);
//		}
//	}

	void Awake () {
		UpdateBasemapDistance(false);
		RefreshTextures();
	}
	
	public void InitArrays() {
		RefreshTextures();
	}

	private void UpdateBasemapDistance(bool apply_material_if_applicable) {
		Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
		if (terrainComp)  {
			if (globalSettingsHolder!=null) {
                {
					//
					// material
					//
					terrainComp.basemapDistance=globalSettingsHolder.distance_start+globalSettingsHolder.distance_transition;
					if (apply_material_if_applicable) {
#if UNITY_2019_2_OR_NEWER
                        bool builtInUnityTerrainMaterial = false;
                        #if UNITY_EDITOR
                            builtInUnityTerrainMaterial = AssetDatabase.GetAssetPath(terrainComp.materialTemplate).Contains("unity_builtin_extra");
                        #endif
                        if (terrainComp.materialTemplate == null || builtInUnityTerrainMaterial) // init null or Unity's built-in material template
                        {
#else
                        if (terrainComp.materialTemplate==null)
                        {
                            terrainComp.materialType=Terrain.MaterialType.Custom;
#endif
                            Material ter_mat;
							Shader ter_shad=Shader.Find("Relief Pack/ReliefTerrain-FirstPass");
							if (ter_shad) {
								ter_mat=new Material(ter_shad);
								ter_mat.name=gameObject.name+" material";
								terrainComp.materialTemplate=ter_mat;
							}
						} else {
							Material ter_mat=terrainComp.materialTemplate;
							terrainComp.materialTemplate=null;
							terrainComp.materialTemplate=ter_mat;
						}
					}
					// far shader setup might not fit (might be for multiple scenes with different scenarios)
					if (globalSettingsHolder!=null && globalSettingsHolder._RTP_LODmanagerScript!=null && globalSettingsHolder._RTP_LODmanagerScript.numLayersProcessedByFarShader!=globalSettingsHolder.numLayers) {
						// so - don't use it
						terrainComp.basemapDistance=500000;
					}
				}
				globalSettingsHolder.Refresh(terrainComp.materialTemplate);
			}
		}
	}
	
	public void RefreshTextures(Material mat=null, bool check_weak_references=false) { // mat used by geom blend to setup underlying mesh
		GetGlobalSettingsHolder();
		InitTerrainTileSizes();
		if (globalSettingsHolder!=null && BumpGlobalCombined!=null) globalSettingsHolder.BumpGlobalCombinedSize=BumpGlobalCombined.width;
		//Debug.Log ("E"+mat);
		
		// refresh distances & apply material if needed
		UpdateBasemapDistance(true);

		Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
		#if UNITY_EDITOR	
		if (terrainComp) {
			GetControlMaps();
		}		
		#endif
		
        {
			globalSettingsHolder.use_mat=mat; // mat==null - 1 tile case (all global) or shader is on mesh
			if (!terrainComp && !mat) {
				// RTP shader on mesh (former ReliefTerrain2Geometry)
				if (GetComponent<Renderer>().sharedMaterial==null || GetComponent<Renderer>().sharedMaterial.name!="RTPMaterial") {
					GetComponent<Renderer>().sharedMaterial=new Material(Shader.Find("Relief Pack/Terrain2Geometry"));
					GetComponent<Renderer>().sharedMaterial.name="RTPMaterial";
				}
				globalSettingsHolder.use_mat=GetComponent<Renderer>().sharedMaterial; // local params to mesh material
			}

			if (terrainComp) {
				if (terrainComp.materialTemplate!=null) {
					globalSettingsHolder.use_mat=terrainComp.materialTemplate;
					terrainComp.materialTemplate.SetVector("RTP_CustomTiling", new Vector4(1.0f / customTiling.x, 1.0f / customTiling.y, 0 ,0));
				}
			}

//			globalSettingsHolder.SetShaderParam("_ColorMapGlobal", ColorGlobal);
//			globalSettingsHolder.SetShaderParam("_NormalMapGlobal", NormalGlobal);
//			globalSettingsHolder.SetShaderParam("_TreesMapGlobal", TreesGlobal);
//			globalSettingsHolder.SetShaderParam("_BumpMapGlobal", BumpGlobalCombined);
			
			globalSettingsHolder.use_mat=null;
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		// control maps
		//
		RefreshControlMaps(mat);	
		if (mat) {
			mat.SetVector("RTP_CustomTiling", new Vector4(1.0f / customTiling.x, 1.0f / customTiling.y, 0 ,0));
		}
		//
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	}
	
	public void RefreshControlMaps(Material mat=null) {
		globalSettingsHolder.use_mat=mat; // mat==null - 1 tile case (all global) or shader is on mesh
		Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));		
		if (!terrainComp && !mat) {
			globalSettingsHolder.use_mat=GetComponent<Renderer>().sharedMaterial; // local params to mesh material (sharedMaterial made above if needed)
		}
		if (terrainComp && !mat) {
			if (terrainComp.materialTemplate!=null) {
				globalSettingsHolder.use_mat=terrainComp.materialTemplate;
			}
		}
		
		globalSettingsHolder.SetShaderParam("_Control1", controlA);
		if (globalSettingsHolder.numLayers>4) {
			globalSettingsHolder.SetShaderParam("_Control3", controlB);
			globalSettingsHolder.SetShaderParam("_Control2", controlB); // for FarOnly to work in 4+4 classic mode
//			if (globalSettingsHolder.Get_RTP_LODmanagerScript() && globalSettingsHolder.Get_RTP_LODmanagerScript().RTP_4LAYERS_MODE && terrainComp && !mat) {
//				globalSettingsHolder.SetShaderParam("_Control3", controlB);
//				globalSettingsHolder.SetShaderParam("_Control2", controlB); // for FarOnly to work in 4+4 classic mode
//			} else {
//				globalSettingsHolder.SetShaderParam("_Control2", controlB);
//			}
		}
		if (globalSettingsHolder.numLayers>8) {
			globalSettingsHolder.SetShaderParam("_Control3", controlC);
		}
		//if (!terrainComp || globalSettingsHolder.numTiles<=1 || mat) {
			globalSettingsHolder.SetShaderParam("_ColorMapGlobal", ColorGlobal);
			globalSettingsHolder.SetShaderParam("_NormalMapGlobal", NormalGlobal);
			globalSettingsHolder.SetShaderParam("_TreesMapGlobal", TreesGlobal);
			globalSettingsHolder.SetShaderParam("_AmbientEmissiveMapGlobal", AmbientEmissiveMap);
			globalSettingsHolder.SetShaderParam("_BumpMapGlobal", BumpGlobalCombined);		
		//}
		globalSettingsHolder.use_mat=null;		
	}
	
	public void GetControlMaps() {
		Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
		if (!terrainComp) {
			Debug.Log("Can't fint terrain component !!!");
			return;
		}
		Type terrainDataType = terrainComp.terrainData.GetType();
		PropertyInfo info = terrainDataType.GetProperty("alphamapTextures", BindingFlags.Instance | BindingFlags.Public);
		if (info!=null) {
			Texture2D[] alphamapTextures=(Texture2D[])info.GetValue(terrainComp.terrainData, null);
			if (alphamapTextures.Length>0) controlA=alphamapTextures[0]; else controlA=null;
			if (alphamapTextures.Length>1) controlB=alphamapTextures[1]; else controlB=null;
			if (alphamapTextures.Length>2) controlC=alphamapTextures[2]; else controlC=null;
		} else{
			Debug.LogError("Can't access alphamapTexture directly...");
		}
	}

    public void SetCustomControlMaps()
    {
        Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
        if (!terrainComp)
        {
            Debug.Log("Can't fint terrain component !!!");
            return;
        }

        if (controlA == null)
        {
            return;
        }

        if (terrainComp.terrainData.alphamapResolution != controlA.width)
        {
            Debug.LogError("Terrain controlmap resolution differs fromrequested control texture...");
            return;
        }
        if (!controlA) return;
        float[,,] splatData = terrainComp.terrainData.GetAlphamaps(0, 0, terrainComp.terrainData.alphamapResolution, terrainComp.terrainData.alphamapResolution);
        Color[] cols_control = controlA.GetPixels();
        for (int n = 0; n < terrainComp.terrainData.alphamapLayers; n++)
        {
            int idx = 0;
            if (n == 4)
            {
                if (!controlB) return;
                cols_control = controlB.GetPixels();
            }
            else if (n == 8)
            {
                if (!controlC) return;
                cols_control = controlC.GetPixels();
            }
            int channel_idx = n & 3;
            for (int i = 0; i < terrainComp.terrainData.alphamapResolution; i++)
            {
                for (int j = 0; j < terrainComp.terrainData.alphamapResolution; j++)
                {
                    splatData[i, j, n] = cols_control[idx++][channel_idx];
                }
            }
        }
        terrainComp.terrainData.SetAlphamaps(0, 0, splatData);
    }

#if UNITY_EDITOR
    public void Update() {
		//EditorApplication.playmodeStateChanged=cl;
		if (!Application.isPlaying) {
			if (controlA) {
				RefreshControlMaps();
			}
		}
	}
	
//	void cl() {
//		if (controlA) {
//			RefreshControlMaps(); 
//		}
//	}
	
    void OnApplicationPause(bool pauseStatus) {
		if (controlA) {
			RefreshControlMaps(); 
		}
    }	
	
	public void RecalcControlMaps() {
		Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
		if (!terrainComp) {
			Debug.Log("Can't fint terrain component !!!");
			return;
		}
		
		globalSettingsHolder.RecalcControlMaps(terrainComp, this);
		RefreshTextures();
		globalSettingsHolder.Refresh();
	}
	
	public void RecalcControlMapsForMesh() {
		globalSettingsHolder.RecalcControlMapsForMesh(this);
		RefreshTextures();
		globalSettingsHolder.Refresh();
	}
	
	public void InvertChannel(Color[] cols, int channel_idx=-1) {
		if (channel_idx<0) {
			for(int idx=0; idx<cols.Length; idx++) {
				cols[idx].r = 1-cols[idx].r;
				cols[idx].g = 1-cols[idx].g;
				cols[idx].b = 1-cols[idx].b;
				cols[idx].a = 1-cols[idx].a;
			}		
		} else {
			for(int idx=0; idx<cols.Length; idx++) {
				cols[idx][channel_idx] = 1-cols[idx][channel_idx];
			}		
		}
	}
	
	public Texture2D GetSteepnessHeightDirectionTexture(int what=0, GameObject ref_obj=null) {
		Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
		if (!terrainComp) {
			Debug.Log("Can't fint terrain component !!!");
			return null;
		}
		Texture2D tex=new Texture2D(terrainComp.terrainData.alphamapResolution, terrainComp.terrainData.alphamapResolution, TextureFormat.RGB24, false);
		int size=terrainComp.terrainData.alphamapResolution;
		Color32[] cols=new Color32[size*size];
		int idx=0;
		float sizef=1.0f/size;
		float[] val_raw_array=new float[size*size];
		if (what==0) {
			// steepness
			for(int j=0; j<size; j++) {
				for(int i=0; i<size; i++) {
					val_raw_array[idx++]=terrainComp.terrainData.GetSteepness(i*sizef, j*sizef);
				}
			}
		} else if (what==1) {
			// height
			float alpha2heightRatio=1.0f*(terrainComp.terrainData.heightmapResolution-1)/size;
			for(int j=0; j<size; j++) {
				for(int i=0; i<size; i++) {
					val_raw_array[idx++]=terrainComp.terrainData.GetHeight( Mathf.RoundToInt(alpha2heightRatio*i), Mathf.RoundToInt(alpha2heightRatio*j) );
				}
			}
		} else {
			// direction
			Vector3 ref_dir=Vector3.forward;
			if (ref_obj) {
				ref_dir=ref_obj.transform.forward;
				if (globalSettingsHolder.flat_dir_ref) {
					ref_dir.y=0;
					if (ref_dir.magnitude<0.0000001f) {
						ref_dir=Vector3.forward;
					} else {
						ref_dir.Normalize();
					}
				}
			}
			if (globalSettingsHolder.flip_dir_ref) ref_dir=-ref_dir;
			for(int j=0; j<size; j++) {
				for(int i=0; i<size; i++) {
					Vector3 dir=terrainComp.terrainData.GetInterpolatedNormal(i*sizef, j*sizef);
					if (globalSettingsHolder.flat_dir_ref) {
						dir.y=0;
						if (dir.magnitude<0.0000001f) {
							val_raw_array[idx++]=0;
						} else {
							dir.Normalize();
							val_raw_array[idx++]=Mathf.Clamp01(Vector3.Dot(dir, ref_dir));
						}
					} else {
						val_raw_array[idx++]=Mathf.Clamp01(Vector3.Dot(dir, ref_dir));
					}
				}
			}
		}
		float min=99999;
		float max=0;
		for(idx=0; idx<val_raw_array.Length; idx++) {
			if (val_raw_array[idx]<min) min=val_raw_array[idx];
			if (val_raw_array[idx]>max) max=val_raw_array[idx];
		}
		float norm_val=1.0f/(max-min);
		for(idx=0; idx<val_raw_array.Length; idx++) {
			float val_raw=(val_raw_array[idx]-min)*norm_val;
			byte val=(byte)Mathf.RoundToInt(255*val_raw);
			cols[idx]=new Color32(val,val,val,1);
		}
		tex.SetPixels32(cols);
		tex.Apply(false);
		return tex;
	}
	
	public bool PrepareGlobalNormalsAndSuperDetails(bool wet=false, bool reflection=false) {
		int[] size=new int[3]{0,0,0};
		if (globalSettingsHolder.BumpGlobal)	size[0]=globalSettingsHolder.BumpGlobal.width;
		Texture2D AddChannelA=globalSettingsHolder.SuperDetailA;
		Texture2D AddChannelB=globalSettingsHolder.SuperDetailB;
		if (wet || reflection) {
			AddChannelA=TERRAIN_WetMask;
            AddChannelB = null;// globalSettingsHolder.TERRAIN_ReflectionMap;
		}
		if (AddChannelA) size[1]=AddChannelA.width;
		if (AddChannelB) size[2]=AddChannelB.width;
		for(int i=0; i<3; i++) {
			for(int j=i+1; j<3; j++) {
				if ( (size[i]!=0) && (size[j]!=0) && (size[i]!=size[j]) ) {
					if (wet) {
						EditorUtility.DisplayDialog("Error", "Special combined texture and wet mask texture need the same size (check perlin normal texture size).","OK");
					} else if (reflection) {
						EditorUtility.DisplayDialog("Error", "Special combined texture and reflection map texture need the same size (check perlin normal texture size).","OK");
					} else {
						EditorUtility.DisplayDialog("Error", "Special combined texture and superdetail textures need the same size (check perlin normal texture size).","OK");
					}
					return false;
				}
			}
		}
		if (globalSettingsHolder.BumpGlobal) {
			try { 
				globalSettingsHolder.BumpGlobal.GetPixels(0,0,4,4,0);
			} catch (Exception e) {
				Debug.LogError("Global normalMap has to be marked as isReadable...");
				Debug.LogError(e.Message);
				globalSettingsHolder.activateObject=globalSettingsHolder.BumpGlobal;
				return false;
			}		
		}
		if (AddChannelA) {
			try { 
				AddChannelA.GetPixels(0,0,4,4,0);
			} catch (Exception e) {
				Debug.LogError("Superdetail texture 1 has to be marked as isReadable...");
				Debug.LogError(e.Message);
				globalSettingsHolder.activateObject=AddChannelA;
				return false;
			}		
		}
		if (AddChannelB) {
			try { 
				AddChannelB.GetPixels(0,0,4,4,0);
			} catch (Exception e) {
				Debug.LogError("Superdetail texture 2 has to be marked as isReadable...");
				Debug.LogError(e.Message);
				globalSettingsHolder.activateObject=AddChannelB;
				return false;
			}
		}
		int _size=size[0];
		if (_size==0) _size=size[1];
		if (_size==0) _size=size[2];
		if (_size==0) _size=wet ? 1024 : 256;
		if (BumpGlobalCombined && AssetDatabase.GetAssetPath(BumpGlobalCombined)=="") { DestroyImmediate(BumpGlobalCombined); BumpGlobalCombined=null; };
		BumpGlobalCombined=new Texture2D(_size,_size,TextureFormat.ARGB32,true,true);
		Color32[] norm_cols;
		if (globalSettingsHolder.BumpGlobal) {
			norm_cols=globalSettingsHolder.BumpGlobal.GetPixels32();
		} else {
			norm_cols=new Color32[_size*_size];
			for(int i=0; i<norm_cols.Length; i++) {
				norm_cols[i]=new Color32(128,128,128,128);
			}
		}
		Color32[] det1_cols;
		if (AddChannelA) {
			det1_cols=AddChannelA.GetPixels32();
		} else {
			det1_cols=new Color32[_size*_size];
			Color default_col=wet ? new Color32(0, 0, 0, 0) : new Color32(128,128,128,128);
			for(int i=0; i<norm_cols.Length; i++) {
				det1_cols[i]=default_col;
			}
		}
		Color32[] det2_cols;
		if (AddChannelB) {
			det2_cols=AddChannelB.GetPixels32();
		} else {
			det2_cols=new Color32[_size*_size];
			for(int i=0; i<norm_cols.Length; i++) {
				det2_cols[i]=new Color32(128,128,128,128);
			}
		}
		Color32[] cols=new Color32[_size*_size];
		int det1_channel;
		if (wet || reflection) {
			det1_channel=3;
		} else {
			det1_channel=(int)globalSettingsHolder.SuperDetailA_channel;
		}
		int det2_channel;
		if (reflection) {
            det2_channel = 0;// (int)globalSettingsHolder.TERRAIN_ReflectionMap_channel;
		} else {
			det2_channel=(int)globalSettingsHolder.SuperDetailB_channel;
		}
		for(int i=0; i<cols.Length; i++) {
			#if UNITY_WEBGL || UNITY_IPHONE || UNITY_ANDROID
			cols[i]=new Color32(norm_cols[i].r, norm_cols[i].g, 0, 0);
			#else
			cols[i]=new Color32(norm_cols[i].a, norm_cols[i].g, 0, 0);
			#endif
		}
		switch(det1_channel) {
			case 0:
				for(int i=0; i<cols.Length; i++) cols[i].b=det1_cols[i].r;
				break;
			case 1:
				for(int i=0; i<cols.Length; i++) cols[i].b=det1_cols[i].g;
				break;
			case 2:
				for(int i=0; i<cols.Length; i++) cols[i].b=det1_cols[i].b;
				break;
			case 3:
				for(int i=0; i<cols.Length; i++) cols[i].b=det1_cols[i].a;
				break;
		}
		switch(det2_channel) {
			case 0:
				for(int i=0; i<cols.Length; i++) cols[i].a=det2_cols[i].r;
				break;
			case 1:
				for(int i=0; i<cols.Length; i++) cols[i].a=det2_cols[i].g;
				break;
			case 2:
				for(int i=0; i<cols.Length; i++) cols[i].a=det2_cols[i].b;
				break;
			case 3:
				for(int i=0; i<cols.Length; i++) cols[i].a=det2_cols[i].a;
				break;
		}
		BumpGlobalCombined.SetPixels32(cols);
		BumpGlobalCombined.Apply(true, false); // not readable przy publishingu
		BumpGlobalCombined.filterMode=FilterMode.Trilinear;
		BumpGlobalCombined.wrapMode=TextureWrapMode.Repeat;
			
		globalCombinedModifed_flag=true;
		RefreshTextures();

		
		RTP_LODmanager manager=globalSettingsHolder.Get_RTP_LODmanagerScript();
		if (manager && !manager.RTP_WETNESS_FIRST && !manager.RTP_WETNESS_ADD) {
			ReliefTerrain[] objs=(ReliefTerrain[])GameObject.FindObjectsOfType(typeof(ReliefTerrain));
			for(int i=0; i<objs.Length; i++) {
				objs[i].BumpGlobalCombined=BumpGlobalCombined;
				objs[i].globalCombinedModifed_flag=true;
				objs[i].RefreshTextures();
			}
		}
		return true;
	}
	
	public int modify_blend(bool upflag) {
		if (globalSettingsHolder.paintHitInfo_flag) {
			if (prepare_tmpTexture(!globalSettingsHolder.paint_wetmask)) {
				int w;
				int h;
				Texture2D tex;
				tex=globalSettingsHolder.paint_wetmask ? tmp_CombinedMap : tmp_globalColorMap;
				w=Mathf.RoundToInt(globalSettingsHolder.paint_size/globalSettingsHolder.terrainTileSize.x * tex.width);
				h=Mathf.RoundToInt(globalSettingsHolder.paint_size/globalSettingsHolder.terrainTileSize.z * tex.height);
				if (w<1) w=1;
				if (h<1) h=1;
				int _left = Mathf.RoundToInt(globalSettingsHolder.paintHitInfo.textureCoord.x*(tex.width)-w);
				if (_left<0) _left=0;
				w*=2;
				if (_left+w>=tex.width) _left=tex.width - w;
				int _top = Mathf.RoundToInt(globalSettingsHolder.paintHitInfo.textureCoord.y*(tex.height)-h);
				if (_top<0) _top=0;
				h*=2;
				if (_top+h>=tex.height) _top=tex.height - h;
				Color[] cols=tex.GetPixels(_left, _top, w, h);
				Color[] cols2=new Color[1];
				if (globalSettingsHolder.paint_wetmask) {
					cols2=tmp_WaterMap.GetPixels(_left, _top, w, h);
				}
				int idx=0;
				float d=upflag ? -1f : 1f;
				float targetBrightness=(globalSettingsHolder.paintColor.r + globalSettingsHolder.paintColor.g + globalSettingsHolder.paintColor.b);
				for(int j=0; j<h; j++) {
					idx=j*w;
					float disty=(2.0f*j/(h-1)-1.0f)*((h-1.0f)/h);
					for(int i=0; i<w; i++) {
						float distx=(2.0f*i/(w-1)-1.0f)*((w-1.0f)/w);
						float dist=1.0f-Mathf.Sqrt(distx*distx+disty*disty);
						if (dist<0) dist=0;
						dist=dist > globalSettingsHolder.paint_smoothness ? 1 : dist/globalSettingsHolder.paint_smoothness;
						if (globalSettingsHolder.paint_wetmask) {
							cols[idx].b += -d*globalSettingsHolder.paint_opacity*dist;
							cols2[idx].a = cols[idx].b;
						} else if (globalSettingsHolder.paint_alpha_flag) {
							cols[idx].a+=d*globalSettingsHolder.paint_opacity*dist;
							if (globalSettingsHolder.cut_holes) {
								if (cols[idx].a<0) cols[idx].a=0;
							} else{
								if (cols[idx].a<0.008f) cols[idx].a=0.008f;
							}
						} else {
							if (globalSettingsHolder.preserveBrightness) {
								float sourceBrightness=cols[idx].r+cols[idx].g+cols[idx].b;
								float brightnessRatio=sourceBrightness/targetBrightness;
								if (upflag) {
									cols[idx]=Color.Lerp(cols[idx], new Color(brightnessRatio*globalSettingsHolder.paintColor.r, brightnessRatio*globalSettingsHolder.paintColor.g, brightnessRatio*globalSettingsHolder.paintColor.b, cols[idx].a), globalSettingsHolder.paint_opacity*dist);
								} else {
									cols[idx]=Color.Lerp(cols[idx], new Color(brightnessRatio*0.5f, brightnessRatio*0.5f, brightnessRatio*0.5f, cols[idx].a), globalSettingsHolder.paint_opacity*dist);
								}
							} else {
								if (upflag) {
									cols[idx]=Color.Lerp(cols[idx], new Color(globalSettingsHolder.paintColor.r, globalSettingsHolder.paintColor.g, globalSettingsHolder.paintColor.b, cols[idx].a), globalSettingsHolder.paint_opacity*dist);
								} else {
									cols[idx]=Color.Lerp(cols[idx], new Color(0.5f, 0.5f, 0.5f, cols[idx].a), globalSettingsHolder.paint_opacity*dist);
								}
							}
						}
						idx++;
					}
				}
				//Debug.Log (_left+" , "+_top+" , "+w+"  "+h);
				//for(int i=0; i<cols.Length; i++) cols[i]=Color.white;
				tex.SetPixels(_left, _top, w, h, cols);
				tex.Apply(true,false);
				if (globalSettingsHolder.paint_wetmask) {
					tmp_WaterMap.SetPixels(_left, _top, w, h, cols2);
					tmp_WaterMap.Apply(true,false);
				}
			} else {
				return -2;
			}
		}
		return 0;
	}
	
	public bool prepare_tmpTexture(bool color_flag=true) {
		if (color_flag) {
			if (ColorGlobal) {
				Texture2D colorMap=ColorGlobal;
				if (tmp_globalColorMap!=colorMap) {
					AssetImporter _importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(colorMap));
					bool sRGBflag=false;
					if (_importer) {
						TextureImporter tex_importer=(TextureImporter)_importer;
						sRGBflag=tex_importer.sRGBTexture;
						bool reimport_flag=false;
						if (!tex_importer.isReadable) {
							Debug.LogWarning("Texture ("+colorMap.name+") has been reimported as readable.");
							tex_importer.isReadable=true;
							reimport_flag=true;
						}
						if (tex_importer.textureCompression!=TextureImporterCompression.Uncompressed) {
							Debug.LogWarning("Texture ("+colorMap.name+") has been reimported as as ARGB32.");
                            tex_importer.textureCompression = TextureImporterCompression.Uncompressed;
                            reimport_flag =true;
						}
						if (reimport_flag) {
							AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(colorMap),  ImportAssetOptions.ForceUpdate);
							ColorGlobal=colorMap;
						}
					}
					try { 
						colorMap.GetPixels(0,0,4,4,0);
					} catch (Exception e) {
						Debug.LogError("Global ColorMap has to be marked as isReadable...");
						Debug.LogError(e.Message);
						return false;
					}
					if (colorMap.format==TextureFormat.Alpha8) {
						tmp_globalColorMap=new Texture2D(colorMap.width, colorMap.height, TextureFormat.Alpha8, true, false); 
					} else {
						tmp_globalColorMap=new Texture2D(colorMap.width, colorMap.height, TextureFormat.ARGB32, true, !sRGBflag); 
					}
					Color[] cols=colorMap.GetPixels();
					tmp_globalColorMap.SetPixels(cols);
					tmp_globalColorMap.Apply(true,false);
					tmp_globalColorMap.wrapMode=TextureWrapMode.Clamp;
					ColorGlobal=tmp_globalColorMap;
					globalColorModifed_flag=true;
					RefreshTextures();
				}
				return true;
			}
			return false;
		} else {
			if (TERRAIN_WetMask) {
				Texture2D colorMap=TERRAIN_WetMask;
				if (tmp_WaterMap!=colorMap) {
					AssetImporter _importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(colorMap));
					if (_importer) {
						TextureImporter tex_importer=(TextureImporter)_importer;
						bool reimport_flag=false;
						if (!tex_importer.isReadable) {
							Debug.LogWarning("Texture ("+colorMap.name+") has been reimported as readable.");
							tex_importer.isReadable=true;
							reimport_flag=true;
						}
						if (tex_importer.textureCompression!=TextureImporterCompression.Uncompressed || tex_importer.textureType != TextureImporterType.SingleChannel) {
							Debug.LogWarning("Texture ("+colorMap.name+") has been reimported as as Alpha8.");
							tex_importer.alphaSource= TextureImporterAlphaSource.FromGrayScale;
                            tex_importer.textureCompression = TextureImporterCompression.Uncompressed;
                            tex_importer.textureType = TextureImporterType.SingleChannel;
							reimport_flag=true;
						}
						if (reimport_flag) {
							AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(colorMap),  ImportAssetOptions.ForceUpdate);
							TERRAIN_WetMask=colorMap;
						}
					}
					try { 
						colorMap.GetPixels(0,0,4,4,0);
					} catch (Exception e) {
						Debug.LogError("Wetmask texture has to be marked as isReadable...");
						Debug.LogError(e.Message);
						return false;
					}
					tmp_WaterMap=new Texture2D(colorMap.width, colorMap.height, TextureFormat.Alpha8, true, true);
					Color[] cols=colorMap.GetPixels();
					tmp_WaterMap.SetPixels(cols);
					tmp_WaterMap.Apply(true,false);
					TERRAIN_WetMask=tmp_WaterMap;
					globalWaterModifed_flag=true;
					RefreshTextures();
				}
			} else {
				if (BumpGlobalCombined) {
					TERRAIN_WetMask=new Texture2D(BumpGlobalCombined.width, BumpGlobalCombined.height, TextureFormat.Alpha8, false, true);
				} else {
					TERRAIN_WetMask=new Texture2D(1024, 1024, TextureFormat.Alpha8, false, true);
				}
				Color32[] cols=new Color32[TERRAIN_WetMask.width*TERRAIN_WetMask.height];
				TERRAIN_WetMask.SetPixels32(cols,0);
				TERRAIN_WetMask.Apply(true,false);
			}
			if (BumpGlobalCombined) {
				Texture2D colorMap=BumpGlobalCombined;
				if (tmp_CombinedMap!=colorMap) {
					AssetImporter _importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(colorMap));
					if (_importer) {
						TextureImporter tex_importer=(TextureImporter)_importer;
						bool reimport_flag=false;
						if (!tex_importer.isReadable) {
							Debug.LogWarning("Texture ("+colorMap.name+") has been reimported as readable.");
							tex_importer.isReadable=true;
							reimport_flag=true;
						}
						if (tex_importer.textureCompression != TextureImporterCompression.Uncompressed) {
							Debug.LogWarning("Texture ("+colorMap.name+") has been reimported as as ARGB32.");
                            tex_importer.textureCompression = TextureImporterCompression.Uncompressed;
							reimport_flag=true;
						}
						if (reimport_flag) {
							AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(colorMap),  ImportAssetOptions.ForceUpdate);
							BumpGlobalCombined=colorMap;
						}
					}
					try { 
						colorMap.GetPixels(0,0,4,4,0);
					} catch (Exception e) {
						Debug.LogError("Special texture (perlin+water/reflection) has to be marked as isReadable...");
						Debug.LogError(e.Message);
						return false;
					}
					tmp_CombinedMap=new Texture2D(colorMap.width, colorMap.height, TextureFormat.ARGB32, true, true);
					Color[] cols=colorMap.GetPixels();
					tmp_CombinedMap.SetPixels(cols);
					tmp_CombinedMap.Apply(true,false);
					tmp_CombinedMap.wrapMode=TextureWrapMode.Repeat;
					BumpGlobalCombined=tmp_CombinedMap;
					globalCombinedModifed_flag=true;
					RefreshTextures();
				}
				return true;
			}
			return false;			
		}
	}
	
	public void SavePreset(ref ReliefTerrainPresetHolder holder) {
		Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));
		if (terrainComp) {
			if (controlA) holder.controlA=UnityEngine.Object.Instantiate(controlA) as Texture2D;
			if (controlB) holder.controlB=UnityEngine.Object.Instantiate(controlB) as Texture2D;
			if (controlC) holder.controlC=UnityEngine.Object.Instantiate(controlC) as Texture2D;
		} else {
			holder.controlA=controlA;
			holder.controlB=controlB;
			holder.controlC=controlC;
		}
		
		holder.ColorGlobal=ColorGlobal;
		holder.NormalGlobal=NormalGlobal;
		holder.TreesGlobal=TreesGlobal;
		holder.AmbientEmissiveMap=AmbientEmissiveMap;
		holder.BumpGlobalCombined=BumpGlobalCombined;
		holder.TERRAIN_WetMask=TERRAIN_WetMask;
		
		holder.globalColorModifed_flag=globalColorModifed_flag;
		holder.globalCombinedModifed_flag=globalCombinedModifed_flag;
		holder.globalWaterModifed_flag=globalWaterModifed_flag;	
		
		// store global settigns		
		globalSettingsHolder.SavePreset(ref holder);
	}	

	public static float[,] ConformTerrain2Detail(TerrainData terrainData, Texture2D hn_tex, bool bicubic_flag) {
		float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
		float ntexel_size_u = 1.0f / hn_tex.width;
		float ntexel_size_v = 1.0f / hn_tex.height;
		float mult_u;
		float mult_v;
        if (bicubic_flag)
        {
            mult_u = (1.0f / (terrainData.heightmapResolution - 1));
            mult_v = (1.0f / (terrainData.heightmapResolution - 1));
        }
        else
        {
            mult_u = (1.0f / (terrainData.heightmapResolution - 1)) * (1 - ntexel_size_u);
            mult_v = (1.0f / (terrainData.heightmapResolution - 1)) * (1 - ntexel_size_v);
        }
        float off_u=ntexel_size_u*0.5f;
		float off_v=ntexel_size_v*0.5f;
		for(int _x = 0; _x<terrainData.heightmapResolution; _x++) {
			float u=_x*mult_u+off_u;
			for(int _z = 0; _z<terrainData.heightmapResolution; _z++) {
				float v=_z*mult_v+off_v;
				if (bicubic_flag) {
					Vector3 norm=Vector3.zero; // not used
					heights[_z,_x] = GeometryVsTerrainBlend.interpolate_bicubic(u,v, hn_tex, ref norm);
				} else {
					Color col = GeometryVsTerrainBlend.CustGetPixelBilinear(hn_tex, u,v);
					heights[_z,_x] = ((1.0f/255)*col.g + col.r);
				}
			}
		}
		return heights;
	}

	public static float[,] ConformTerrain2Occlusion(TerrainData terrainData, Texture2D hn_tex) {
		float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

		float _ratio = 1.0f*hn_tex.width/terrainData.heightmapResolution;
		int sampleSize = hn_tex.width/terrainData.heightmapResolution;
		if (sampleSize<1) sampleSize=1;
		for(int _x = 0; _x<terrainData.heightmapResolution; _x++) {
			int _xB=Mathf.FloorToInt(_x*_ratio);
			for(int _z = 0; _z<terrainData.heightmapResolution; _z++) {
				float gVal=0;
				float rVal=0;
				int _zB=Mathf.FloorToInt(_z*_ratio);
				for(int _xS = 0; _xS<sampleSize; _xS++) {
					for(int _zS = 0; _zS<sampleSize; _zS++) {
						Color col = hn_tex.GetPixel(_xB+_xS, _zB+_zS);
						if (gVal<col.g) gVal=col.g;
						if (rVal<col.r) rVal=col.r;
					}
				}
                heights[_z, _x] = ((1.0f / 255) * gVal + rVal);// +0.02f; // little offset to be sure about occlusion (actually it turned out to break culling in some cases)
			}
		}
		return heights;
	}

#endif

    public void RestorePreset(ReliefTerrainPresetHolder holder)
    {
        controlA = holder.controlA;
        controlB = holder.controlB;
        controlC = holder.controlC;

        SetCustomControlMaps();

        ColorGlobal = holder.ColorGlobal;
        NormalGlobal = holder.NormalGlobal;
        TreesGlobal = holder.TreesGlobal;
        AmbientEmissiveMap = holder.AmbientEmissiveMap;
        BumpGlobalCombined = holder.BumpGlobalCombined;
        TERRAIN_WetMask = holder.TERRAIN_WetMask;

        globalColorModifed_flag = holder.globalColorModifed_flag;
        globalCombinedModifed_flag = holder.globalCombinedModifed_flag;
        globalWaterModifed_flag = holder.globalWaterModifed_flag;

        // local textures to splat textures
        RefreshTextures();

        // restore global settigns		
        globalSettingsHolder.RestorePreset(holder);
    }

    public ReliefTerrainPresetHolder GetPresetByID(string PresetID) {
		if (presetHolders!=null) {
			for(int i=0; i<presetHolders.Length; i++) {
				if (presetHolders[i].PresetID==PresetID) {
					return presetHolders[i];
				}
			}
		}
		return null;
	}
	
	public ReliefTerrainPresetHolder GetPresetByName(string PresetName) {
		if (presetHolders!=null) {
			for(int i=0; i<presetHolders.Length; i++) {
				if (presetHolders[i].PresetName==PresetName) {
					return presetHolders[i];
				}
			}
		}
		return null;
	}	
	
	public bool InterpolatePresets(string PresetID1, string PresetID2, float t) {
		ReliefTerrainPresetHolder holderA=GetPresetByID(PresetID1);
		ReliefTerrainPresetHolder holderB=GetPresetByID(PresetID2);
		if (holderA==null || holderB==null || holderA.Spec==null || holderB.Spec==null || holderA.Spec.Length!=holderB.Spec.Length) {
			return false;
		}
		globalSettingsHolder.InterpolatePresets(holderA, holderB, t);
		return true;
	}	
}
