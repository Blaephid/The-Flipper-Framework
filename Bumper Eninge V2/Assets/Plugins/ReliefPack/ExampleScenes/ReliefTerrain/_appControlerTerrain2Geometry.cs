using UnityEngine;
using System.Collections;

public class _appControlerTerrain2Geometry : MonoBehaviour {
	public bool shadows=false;
	public bool forward_path=true;
	//public bool water=true;
	public bool terrain_self_shadow=false;
	public bool terrain_smooth_shadows=true;
	private bool panel_enabled;
	public float light_dir=285;
	public float preset_param_interp=0;
	
	//private GameObject go_water;
	private RTP_LODmanager LODmanager;

	void Awake() {
		GetLODManager();
		panel_enabled=true;
		//go_water=GameObject.Find("Water");
	}
	
	void Update() {
		if (Input.GetKeyDown(KeyCode.P)) {
			panel_enabled=!panel_enabled;
		}
		if (Input.GetKey(KeyCode.Period)) {
			transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Min(transform.localPosition.y+0.5f,50), transform.localPosition.z);
		}
		if (Input.GetKey(KeyCode.Comma)) {
			transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Max(transform.localPosition.y-0.5f, 0.9f), transform.localPosition.z);
		}
	}
	
	void GetLODManager() {
		GameObject go=GameObject.Find("_RTP_LODmanager");
		if (go==null) return;
		LODmanager=(RTP_LODmanager)go.GetComponent(typeof(RTP_LODmanager));
	}
	
	void OnGUI () {
		if (!LODmanager) {
			GetLODManager();
			return;
		}
		
		GUILayout.Space(10);
		GUILayout.BeginVertical("box");

		GUILayout.Label (""+FPSmeter.fps);
		if (panel_enabled) {
			shadows = GUILayout.Toggle(shadows, "disable Unity's shadows");
		 	Light light=GameObject.Find("Directional light").GetComponent<Light>() as Light;
			light.shadows=shadows ?  LightShadows.None : LightShadows.Soft;
			
			forward_path = GUILayout.Toggle(forward_path, "forward rendering");
		 	Camera cam=GameObject.Find("Main Camera").GetComponent<Camera>() as Camera;
			cam.renderingPath = forward_path ?  RenderingPath.Forward : RenderingPath.DeferredShading;
			if (forward_path) RenderSettings.ambientLight=new Color32(25,25,25,0); else RenderSettings.ambientLight=new Color32(93,103,122,0);
			
			
			TerrainShaderLod pom=LODmanager.RTP_LODlevel;
			TerrainShaderLod npom=pom;
			switch(pom) {
				case TerrainShaderLod.POM:
					if (GUILayout.Button("POM shading")) npom=TerrainShaderLod.PM;
				break;
				case TerrainShaderLod.PM:
					if (GUILayout.Button("PM shading")) npom=TerrainShaderLod.SIMPLE;
				break;
				case TerrainShaderLod.SIMPLE:
					if (GUILayout.Button("SIMPLE shading")) npom = TerrainShaderLod.POM; //npom =TerrainShaderLod.CLASSIC;
				break;
				//case TerrainShaderLod.CLASSIC:
				//	if (GUILayout.Button("CLASSIC shading")) npom=TerrainShaderLod.POM;
				//break;
			}
			switch(npom) {
				case TerrainShaderLod.POM:
					if (npom!=pom) {
						GameObject terrain=GameObject.Find("terrainMesh");
						ReliefTerrain script=terrain.GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;
						script.globalSettingsHolder.Refresh();
					
						LODmanager.RTP_LODlevel=TerrainShaderLod.POM;
						LODmanager.RefreshLODlevel();
					}
				break;
				case TerrainShaderLod.PM:
					if (npom!=pom) {
						GameObject terrain=GameObject.Find("terrainMesh");
						ReliefTerrain script=terrain.GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;
						script.globalSettingsHolder.Refresh();
					
						LODmanager.RTP_LODlevel=TerrainShaderLod.PM;
						LODmanager.RefreshLODlevel();
					}
				break;
				case TerrainShaderLod.SIMPLE:
					if (npom!=pom) {
						GameObject terrain=GameObject.Find("terrainMesh");
						ReliefTerrain script=terrain.GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;
						script.globalSettingsHolder.Refresh();
					
						LODmanager.RTP_LODlevel=TerrainShaderLod.SIMPLE;
						LODmanager.RefreshLODlevel();
					}
				break;
				//case TerrainShaderLod.CLASSIC:
				//	if (npom!=pom) {
				//		GameObject terrain=GameObject.Find("terrainMesh");
				//		ReliefTerrain script=terrain.GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;
				//		script.globalSettingsHolder.Refresh();
					
				//		LODmanager.RTP_LODlevel=TerrainShaderLod.CLASSIC;
				//		LODmanager.RefreshLODlevel();
				//	}
				//break;
			}
			pom=npom;
			if (pom==TerrainShaderLod.POM) {
				terrain_self_shadow=LODmanager.RTP_SHADOWS;
				bool nterrain_self_shadow = GUILayout.Toggle(terrain_self_shadow, "self shadowing");
				if (nterrain_self_shadow!=terrain_self_shadow) {
					LODmanager.RTP_SHADOWS=nterrain_self_shadow;
					LODmanager.RefreshLODlevel();
				}
				terrain_self_shadow=nterrain_self_shadow;
				if (terrain_self_shadow) {
					terrain_smooth_shadows=LODmanager.RTP_SOFT_SHADOWS;
					bool nterrain_smooth_shadows = GUILayout.Toggle(terrain_smooth_shadows, "smooth shadows");
					if (nterrain_smooth_shadows!=terrain_smooth_shadows) {
						LODmanager.RTP_SOFT_SHADOWS=nterrain_smooth_shadows;
						LODmanager.RefreshLODlevel();
					}
					terrain_smooth_shadows=nterrain_smooth_shadows;
				}
			}
			
			if (LODmanager.RTP_SNOW_FIRST) {
				GameObject terrain=GameObject.Find("terrainMesh");
				ReliefTerrain script=terrain.GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;
				GUILayout.BeginHorizontal();
				GUILayout.Label ("Snow", GUILayout.MaxWidth(40));
				float nval = GUILayout.HorizontalSlider(script.globalSettingsHolder._snow_strength, 0,1);
				if (nval!=script.globalSettingsHolder._snow_strength) {
					script.globalSettingsHolder._snow_strength = nval;
					script.globalSettingsHolder.Refresh();
				}
				GUILayout.EndHorizontal();
			}
			
			GUILayout.Label ("Light", GUILayout.MaxWidth(40));
			light_dir = GUILayout.HorizontalSlider(light_dir, 0, 360);
			light.transform.rotation=Quaternion.Euler(40,light_dir	,0);
			
//			GUILayout.Label ("Interp", GUILayout.MaxWidth(40));
//			float n_interp = GUILayout.HorizontalSlider(preset_param_interp, 0, 1);
//			if (n_interp!=preset_param_interp) {
//				preset_param_interp=n_interp;
//				GameObject terrain=GameObject.Find("terrainMesh");
//				ReliefTerrain script=terrain.GetComponent(typeof(ReliefTerrain)) as ReliefTerrain;	
//				ReliefTerrainPresetHolder holderA=script.GetPresetByName("terrain stateA");
//				ReliefTerrainPresetHolder holderB=script.GetPresetByName("terrain stateB");
//				if (holderA!=null && holderB!=null) {
//					script.InterpolatePresets(holderA.PresetID, holderB.PresetID, preset_param_interp);
//					script.globalSettingsHolder.Refresh();
//				}
//			}
			
			//if (!Application.isWebPlayer) {
			//	if (GUILayout.Button("QUIT")) {
			//		Application.Quit();
			//	}
			//}
			GUILayout.Label ("  F (hold) - freeze camera");
			GUILayout.Label ("  ,/. - change cam position");
		} else {
			//if (!Application.isWebPlayer) {
			//	if (GUILayout.Button("QUIT")) {
			//		Application.Quit();
			//	}
			//}
		}
		GUILayout.Label ("  P - toggle panel");
		GUILayout.EndVertical();
	}
}
