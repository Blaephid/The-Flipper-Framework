using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class RGBM2RGB : EditorWindow
{
	static Texture2D source_tex;
	static Texture2D rendered_tex;
	static float exposure=8;
	static string save_path="";
	static string directory="";
	static string file="_output.png";
	bool finalize=false;
	
	[MenuItem("Window/Relief Tools/RGBM 2 RGB")]
	public static void ShowWindow() {
		RGBM2RGB window=EditorWindow.GetWindow(typeof(RGBM2RGB)) as RGBM2RGB;
		window.titleContent.text="RGBM 2 RGB converter";
		window.minSize=new Vector2(360,250);
		window.maxSize=new Vector2(370,250);
		if ((Selection.activeObject is Texture2D) && source_tex==null) {
			source_tex=Selection.activeObject as Texture2D;
		}
	}
	
	void OnGUI() {
		if (finalize) {
			// select created texture
			Selection.activeObject=AssetDatabase.LoadAssetAtPath(save_path, typeof(Texture2D));
			finalize=false;
		}
		
		EditorGUILayout.Space();
		source_tex = EditorGUILayout.ObjectField("Source Texture", source_tex, typeof(Texture2D), false) as Texture2D;
		exposure = EditorGUILayout.Slider("Exposure", exposure, 0, 32);
		
		EditorGUILayout.Space();
		if (source_tex) {
			AssetImporter _importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(source_tex));
			if (_importer) {
				TextureImporter tex_importer=(TextureImporter)_importer;
				if (!tex_importer.isReadable) {
					Debug.LogWarning("RGBM texture ("+source_tex.name+") has been reimported as readable.");
					tex_importer.isReadable=true;
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(source_tex),  ImportAssetOptions.ForceUpdate);
				}
			}							
			
			if (GUILayout.Button("Render RGB texture")) {
				rendered_tex=new Texture2D(source_tex.width, source_tex.height, TextureFormat.RGB24, false);
				Color32[] src_cols=source_tex.GetPixels32();
				Color32[] cols=new Color32[src_cols.Length];
				for(int i=0; i<cols.Length; i++) {
					int _r,_g,_b;
					_r=Mathf.RoundToInt(exposure*src_cols[i].r*src_cols[i].a/255);
					if (_r>255) _r=255;
					_g=Mathf.RoundToInt(exposure*src_cols[i].g*src_cols[i].a/255);
					if (_g>255) _g=255;
					_b=Mathf.RoundToInt(exposure*src_cols[i].b*src_cols[i].a/255);
					if (_b>255) _b=255;
					cols[i].r=(byte)_r;
					cols[i].g=(byte)_g;
					cols[i].b=(byte)_b;
				}
				rendered_tex.SetPixels32(cols);
				rendered_tex.Apply(false,false);
				if (Selection.activeObject is Texture2D && AssetDatabase.GetAssetPath(Selection.activeObject as Texture2D)!="") {
					save_path=AssetDatabase.GetAssetPath(Selection.activeObject as Texture2D);
					directory=Path.GetDirectoryName(save_path);
					file=Path.GetFileNameWithoutExtension(save_path)+"(LDR).png";
				} else {
					if (save_path=="") {
						directory=Path.GetDirectoryName(AssetDatabase.GetAssetPath(source_tex));
						file=Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(source_tex))+"(LDR).png";
					}
				}
			}

		}
		if (rendered_tex) {
			EditorGUILayout.ObjectField("RGB texture", rendered_tex, typeof(Texture2D), false);
			if (GUILayout.Button("Save texture")) {
				if (save_path=="") {
					directory=Path.GetDirectoryName(AssetDatabase.GetAssetPath(source_tex));
					file=Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(source_tex))+"(LDR).png";
				}
				SaveTexture(directory, file);
			}
		}
	}
	
	void SaveTexture(string directory, string file) {
		string path = EditorUtility.SaveFilePanel("Save texture", directory, file, "png");
		if (path!="") {
			save_path=path=path.Substring(Application.dataPath.Length-6);
			byte[] bytes = rendered_tex.EncodeToPNG();
			System.IO.File.WriteAllBytes(path, bytes);
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			AssetDatabase.ImportAsset(path,  ImportAssetOptions.ForceUpdate);
			finalize=true;
			rendered_tex=null;
		}							
	}
}