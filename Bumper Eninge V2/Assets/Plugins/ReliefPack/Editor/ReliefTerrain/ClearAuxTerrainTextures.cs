using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

//
//
// here we celar unused textures from scene, so it won't be published
// some textures are set to not readable from scripts, too
//
//
public class ClearAuxTerrainTextures {
    [PostProcessScene]
    public static void OnPostprocessScene() { 
		if (EditorApplication.isPlayingOrWillChangePlaymode) return;

		//
		// Relief Terrain objects
		//
        ReliefTerrain[] terrains=Object.FindObjectsOfType(typeof(ReliefTerrain)) as ReliefTerrain[];
		for(int i=0; i<terrains.Length; i++) {
			terrains[i].globalSettingsHolder.BumpGlobal=null;
			terrains[i].globalSettingsHolder.SuperDetailA=null;
			terrains[i].globalSettingsHolder.SuperDetailB=null;
			terrains[i].TERRAIN_WetMask=null;
			if (terrains[i].ColorGlobal && (terrains[i].ColorGlobal.format==TextureFormat.ARGB32)) {
				try {
					terrains[i].ColorGlobal.Compress(true);
					terrains[i].ColorGlobal.Apply(true,true);
				} catch {
				}
			}			
				
			terrains[i].globalSettingsHolder.Heights=null;
			terrains[i].globalSettingsHolder.Bumps=null;
			terrains[i].source_controls_mask=null;
			terrains[i].source_controls=null;
            #if (!UNITY_WEBGL || UNITY_EDITOR) && !UNITY_2018_1_OR_NEWER
			terrains[i].globalSettingsHolder.Substances=null;
			#endif
			
			if (terrains[i].BumpGlobalCombined) {
				try {
					terrains[i].BumpGlobalCombined.Apply(true, true);
				} catch {
				}
			}
			if (terrains[i].globalSettingsHolder.Bump01) {
				try {
					terrains[i].globalSettingsHolder.Bump01.Apply(true, true);
				} catch {
				}
			}
			if (terrains[i].globalSettingsHolder.Bump23) {
				try {
					terrains[i].globalSettingsHolder.Bump23.Apply(true, true);
				} catch {
				}
			}
			if (terrains[i].globalSettingsHolder.Bump45) {
				try {
					terrains[i].globalSettingsHolder.Bump45.Apply(true, true);
				} catch {
				}
			}
			if (terrains[i].globalSettingsHolder.Bump67) {
				try {
					terrains[i].globalSettingsHolder.Bump67.Apply(true, true);
				} catch {
				}
			}
			if (terrains[i].globalSettingsHolder.Bump89) {
				try {
					terrains[i].globalSettingsHolder.Bump89.Apply(true, true);
				} catch {
				}
			}
			if (terrains[i].globalSettingsHolder.BumpAB) {
				try {
					terrains[i].globalSettingsHolder.BumpAB.Apply(true, true);
				} catch {
				}
			}
			if (terrains[i].globalSettingsHolder.SSColorCombinedA) {
				try {
					terrains[i].globalSettingsHolder.SSColorCombinedA.Apply(true, true);
				} catch {
				}
			}
			if (terrains[i].globalSettingsHolder.SSColorCombinedB) {
				try {
					terrains[i].globalSettingsHolder.SSColorCombinedB.Apply(true, true);
				} catch {
				}
			}
			
			if (terrains[i].globalSettingsHolder.HeightMap) {
				try {
					terrains[i].globalSettingsHolder.HeightMap.Apply(true, true);
				} catch {
				}
			}
			if (terrains[i].globalSettingsHolder.HeightMap2) {
				try {
					terrains[i].globalSettingsHolder.HeightMap.Apply(true, true);
				} catch {
				}
			}
			if (terrains[i].globalSettingsHolder.HeightMap3) {
				try {
					terrains[i].globalSettingsHolder.HeightMap.Apply(true, true);
				} catch {
				}
			}
            // don't break restorePreset functionality in builds...
			/*	
			if (terrains[i].presetHolders!=null) {
				for(int k=0; k<terrains[i].presetHolders.Length; k++) {
					if (terrains[i].presetHolders[k]!=null) {
						terrains[i].presetHolders[k].BumpGlobal=null;
						terrains[i].presetHolders[k].SuperDetailA=null;
						terrains[i].presetHolders[k].SuperDetailB=null;
						terrains[i].presetHolders[k].TERRAIN_WetMask=null;
						terrains[i].presetHolders[k].TERRAIN_ReflectionMap=null;
						terrains[i].presetHolders[k].Heights=null;
						terrains[i].presetHolders[k].Bumps=null;
						#if !UNITY_WEBGL
						terrains[i].presetHolders[k].Substances=null;			
						#endif
						
//						terrains[i].presetHolders[k].controlA=null;
//						terrains[i].presetHolders[k].controlB=null;
//						terrains[i].presetHolders[k].controlC=null;
					}
				}
			}
            */
				
		}
		
    }
}