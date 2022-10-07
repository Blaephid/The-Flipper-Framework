using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class TextureChannelMixer : EditorWindow
{
	static Texture2D source_tex0;
	static Texture2D source_tex1;
	static Texture2D source_tex2;
	static Texture2D source_tex3;
	static Texture2D rendered_tex;
	static RTPColorChannels sourceChannel0=RTPColorChannels.R;
	static RTPColorChannels sourceChannel1=RTPColorChannels.R;
	static RTPColorChannels sourceChannel2=RTPColorChannels.R;
	static RTPColorChannels sourceChannel3=RTPColorChannels.R;
	static string save_path="";
	static string directory="";
	static string file="_output.png";
	static bool linearTexture=true;
	bool finalize=false;

	[MenuItem("Window/Relief Tools/4 to 1 texture channels mixer (for Terrains)")]
	public static void ShowWindow() {
		TextureChannelMixer window=EditorWindow.GetWindow(typeof(TextureChannelMixer)) as TextureChannelMixer;
		window.titleContent.text="Texture mixer";
		window.minSize=new Vector2(360,490);
		window.maxSize=new Vector2(370,492);
	}

	void OnGUI() {
		if (finalize) {
			// select created texture
			Selection.activeObject=AssetDatabase.LoadAssetAtPath(save_path, typeof(Texture2D));
			finalize=false;
		}
		
		EditorGUILayout.Space();
        source_tex0 = EditorGUILayout.ObjectField("Source Texture 0", source_tex0, typeof(Texture2D), false) as Texture2D;
		EditorGUILayout.Space();
        source_tex1 = EditorGUILayout.ObjectField("Source Texture 1", source_tex1, typeof(Texture2D), false) as Texture2D;
		EditorGUILayout.Space();
        source_tex2 = EditorGUILayout.ObjectField("Source Texture 2", source_tex2, typeof(Texture2D), false) as Texture2D;
		EditorGUILayout.Space();
        source_tex3 = EditorGUILayout.ObjectField("Source Texture 3", source_tex3, typeof(Texture2D), false) as Texture2D;
		
		EditorGUILayout.Space();
		if (source_tex0) {
			int sources_ready=0;
			if (check_texture(source_tex0, 0, source_tex0.width, source_tex0.height)) { sources_ready++; sourceChannel0 = (RTPColorChannels)EditorGUILayout.EnumPopup("Target R from source 0", sourceChannel0); }
	        if (check_texture(source_tex1, 1, source_tex0.width, source_tex0.height)) { sources_ready++; sourceChannel1 = (RTPColorChannels)EditorGUILayout.EnumPopup("Target G from source 1", sourceChannel1); }
	        if (check_texture(source_tex2, 2, source_tex0.width, source_tex0.height)) { sources_ready++; sourceChannel2 = (RTPColorChannels)EditorGUILayout.EnumPopup("Target B from source 2", sourceChannel2); }
	        if (check_texture(source_tex3, 3, source_tex0.width, source_tex0.height)) { sources_ready++; sourceChannel3 = (RTPColorChannels)EditorGUILayout.EnumPopup("Target A from source 3", sourceChannel3); }
			
			if (sources_ready==4) {
				if (GUILayout.Button("Render mixed texture")) {
					rendered_tex=new Texture2D(source_tex0.width, source_tex0.height, TextureFormat.ARGB32, true);
					byte[] colsR=get_color_channel(source_tex0, sourceChannel0);
					byte[] colsG=get_color_channel(source_tex1, sourceChannel1);
					byte[] colsB=get_color_channel(source_tex2, sourceChannel2);
					byte[] colsA=get_color_channel(source_tex3, sourceChannel3);
					Color32[] cols=rendered_tex.GetPixels32();
					for(int i=0; i<cols.Length; i++) {
						cols[i].r=colsR[i];
						cols[i].g=colsG[i];
						cols[i].b=colsB[i];
						cols[i].a=colsA[i];
					}
					rendered_tex.SetPixels32(cols);
					if (Selection.activeObject is Texture2D) {
						save_path=AssetDatabase.GetAssetPath(Selection.activeObject as Texture2D);
						directory=Path.GetDirectoryName(save_path);
						file=Path.GetFileNameWithoutExtension(save_path)+".png";
					} else {
						if (save_path=="") {
							directory=Path.GetDirectoryName(AssetDatabase.GetAssetPath(source_tex0));
							file=Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(source_tex0))+"(mixed).png";
						}
					}
				}
			}
		}
		if (rendered_tex) {
			linearTexture=GUILayout.Toggle(linearTexture, "Linear texture (Bypass sRGB Sampling)");
			if (GUILayout.Button("Save texture")) {
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
			AssetImporter _importer=AssetImporter.GetAtPath(path);
			TextureImporter tex_importer=(TextureImporter)_importer;
			tex_importer.sRGBTexture = linearTexture;
			AssetDatabase.ImportAsset(path,  ImportAssetOptions.ForceUpdate);
			finalize=true;
			rendered_tex=null;
		}							
	}
				
	bool check_texture(Texture2D tex, int num, int w, int h) {
		if (!tex) return false;
		
		AssetImporter _importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
		if (_importer) {
			TextureImporter tex_importer=(TextureImporter)_importer;
			if (!tex_importer.isReadable) {
				Debug.LogWarning("Texture ("+tex.name+") has been reimported as readable.");
				tex_importer.isReadable=true;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex),  ImportAssetOptions.ForceUpdate);
			}
		}			
		
		string path = AssetDatabase.GetAssetPath(tex);
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		if (!textureImporter.isReadable) {
			EditorGUILayout.LabelField("Mark your source texture "+num+" as readable...");
			return false;
		}
		if (tex.width!=w || tex.height!=h) {
			EditorGUILayout.LabelField("Source tex "+num+" must fit dimensions of source tex 0");
			return false;
		}
		return true;
	}
				
	byte[] get_color_channel(Texture2D source_tex, RTPColorChannels sourceChannel) {
		Color32[] cols=source_tex.GetPixels32();
		byte[] ret=new byte[cols.Length];
		if (sourceChannel==RTPColorChannels.R) for(int i=0; i<cols.Length; i++) 	ret[i]=cols[i].r;
		if (sourceChannel==RTPColorChannels.G) for(int i=0; i<cols.Length; i++) 	ret[i]=cols[i].g;
		if (sourceChannel==RTPColorChannels.B) for(int i=0; i<cols.Length; i++) 	ret[i]=cols[i].b;
		if (sourceChannel==RTPColorChannels.A) for(int i=0; i<cols.Length; i++) 	ret[i]=cols[i].a;
		return ret;
	}
}