using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
#if UNITY_EDITOR
using System.IO;
#endif

[CustomEditor (typeof(ReliefTerrainVertexBlendTriplanar))]
public class ReliefTerrainVertexBlendTriplanarEditor : Editor {
	#if UNITY_EDITOR	
	
	public void OnEnable() {
		ReliefTerrainVertexBlendTriplanar _target=(ReliefTerrainVertexBlendTriplanar)target;
		
		if (_target.GetComponent<Renderer>()==null && _target.material==null) return;
		Material mat;
		if (_target.GetComponent<Renderer>()!=null) {
			mat=_target.GetComponent<Renderer>().sharedMaterial;
		} else {
			mat=_target.material;
		}
		if (mat==null) return;
		
		if (mat.HasProperty("_ColorMapGlobal")) {
			if (_target.tmp_globalColorMap!=mat.GetTexture("_ColorMapGlobal") as Texture2D) {
				_target.tmp_globalColorMap=null;
			}
		}
	}
	
	public override void OnInspectorGUI () {
		ReliefTerrainVertexBlendTriplanar _target=(ReliefTerrainVertexBlendTriplanar)target;
		if (GUILayout.Button("Setup top planar uv from bounding box")) {
			_target.SetTopPlanarUVBounds();
		}
		_target.material=EditorGUILayout.ObjectField("Material for MIP setup (optional)", _target.material, typeof(Material), false) as Material;
		
		if (_target.GetComponent<Renderer>()==null && _target.material==null) return;
		Material mat;
		if (_target.GetComponent<Renderer>()!=null) {
			mat=_target.GetComponent<Renderer>().sharedMaterial;
		} else {
			mat=_target.material;
		}
		if (mat==null) return;
		
		if (mat.HasProperty("_ColorMapGlobal")) {
		EditorGUI.BeginDisabledGroup(_target.tmp_globalColorMap==null);
			Color skin_color=GUI.color;
			if (_target.tmp_globalColorMap!=null) {
				GUI.color=new Color(1,0.4f,0.2f,1);
			}
			if (GUILayout.Button("Save modified texture")) {
			
				string directory;
				string file;
				if (_target.save_path_colormap=="") {
					directory=Application.dataPath;
					file="_output_colormap.png";
				} else {
					directory=Path.GetDirectoryName(_target.save_path_colormap);
					file=Path.GetFileNameWithoutExtension(_target.save_path_colormap)+".png";
				}
				string path = EditorUtility.SaveFilePanel("Save edited Global color map", directory, file, "png");
				if (path!="") {
					path=path.Substring(Application.dataPath.Length-6);
					_target.save_path_colormap=path;
					byte[] bytes =_target.tmp_globalColorMap.EncodeToPNG();
					System.IO.File.WriteAllBytes(path, bytes);
					AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
					if (_target.tmp_globalColorMap.format==TextureFormat.Alpha8) {
						TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
						if (textureImporter) {
							textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                            textureImporter.textureType = TextureImporterType.SingleChannel;
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
						}
					}
					if (mat.HasProperty("_ColorMapGlobal")) {
						_target.tmp_globalColorMap=(Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
						if (_target.tmp_globalColorMap!=null) {
							mat.SetTexture("_ColorMapGlobal", _target.tmp_globalColorMap);
							_target.tmp_globalColorMap=null;
						}
					}
					if (_target.painterInstance) {
						_target.painterInstance.SyncMaterialProps();
					}
				
				}										
			
			}
			GUI.color=skin_color;
		EditorGUI.EndDisabledGroup();
		}
		
//		DrawDefaultInspector();
	}
	
	#endif
}
