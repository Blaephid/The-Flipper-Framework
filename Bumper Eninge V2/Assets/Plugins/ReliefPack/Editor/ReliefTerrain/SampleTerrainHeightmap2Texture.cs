using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class SampleTerrainHeightmap2Texture : EditorWindow
{
	static TerrainData sourceTerrain;
	static Texture2D rendered_tex;
	static string save_path="";
	static string directory="";
	static string file="_heightmap.png";
	bool finalize=false;
	
	[MenuItem("Window/Relief Tools/Prepare Height&Normal Texture for Tessellation")]
	public static void ShowWindow() {
		SampleTerrainHeightmap2Texture window=EditorWindow.GetWindow(typeof(SampleTerrainHeightmap2Texture)) as SampleTerrainHeightmap2Texture;
		window.titleContent.text="Sample Terrain Heights 2 Texture";
		window.minSize=new Vector2(360,250);
		window.maxSize=new Vector2(370,250);
	}
	
	void OnGUI() {
		if (finalize) {
			// select created texture
			Selection.activeObject=AssetDatabase.LoadAssetAtPath(save_path, typeof(Texture2D));
			finalize=false;
		}
		
		EditorGUILayout.Space();
		sourceTerrain = EditorGUILayout.ObjectField("Source Terrain Data", sourceTerrain, typeof(TerrainData), false) as TerrainData;

		EditorGUILayout.Space();
		if (sourceTerrain) {
			bool render_flag=false;
			bool render_flag_npot=false;
			if (GUILayout.Button("Render heights")) {
				render_flag=true;
			}
			if (GUILayout.Button("Render heights (NPOT exact size 2^n+1)")) {
				render_flag_npot=true;
			}
			if (render_flag || render_flag_npot) {
				rendered_tex=new Texture2D(sourceTerrain.heightmapResolution-(render_flag_npot ? 0:1), sourceTerrain.heightmapResolution-(render_flag_npot ? 0:1), TextureFormat.RGBA32, false, true);
				Color32[] cols=new Color32[rendered_tex.width * rendered_tex.height];
                for (int y = 0; y < rendered_tex.height; y++)
                {
                    //string t = "";
                    for ( int x = 0; x < rendered_tex.width; x++ )
                    {
						float _x = 1.0f*x/(rendered_tex.width-1);
						float _y = 1.0f*y/(rendered_tex.height-1);

						int _hiP,_loP;
                        float hgt = sourceTerrain.GetInterpolatedHeight( _x,_y );
                        //float hgt = sourceTerrain.GetHeight(x,y);
                        //if (y==0 || (y==rendered_tex.height-1))
                        //{
                        //    string _hgt = "" + hgt;
                        //    _hgt = _hgt.Replace(".", ",");
                        //    t +=  _hgt + "\n";
                        //}
                        hgt /= sourceTerrain.size.y;
                        _hiP = Mathf.FloorToInt( hgt*255.0f );
						_loP = Mathf.FloorToInt( (hgt*255.0f - _hiP)*255.0f );

						int _Nx, _Nz;
						Vector3 norm = sourceTerrain.GetInterpolatedNormal( _x,_y );
						_Nx = Mathf.RoundToInt( Mathf.Clamp01(norm.x*0.5f+0.5f)*255.0f );
						_Nz = Mathf.RoundToInt( Mathf.Clamp01(norm.z*0.5f+0.5f)*255.0f );
						cols[y * rendered_tex.width + x] = new Color32( (byte)_hiP, (byte)_loP, (byte)_Nx, (byte)_Nz );
					}
                    //if (y == 0 || (y == rendered_tex.height - 1))
                    //{
                    //    Debug.Log(t);
                    //}

                }
                rendered_tex.SetPixels32(cols);
				rendered_tex.Apply(false,false);
				if (Selection.activeObject is Texture2D && AssetDatabase.GetAssetPath(Selection.activeObject as Texture2D)!="") {
					save_path=AssetDatabase.GetAssetPath(Selection.activeObject as Texture2D);
					directory=Path.GetDirectoryName(save_path);
					file=Path.GetFileNameWithoutExtension(save_path)+".png";
				} else {
					if (save_path=="") {
						directory=Path.GetDirectoryName(AssetDatabase.GetAssetPath(sourceTerrain));
						file=Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sourceTerrain))+".png";
					}
				}
			}

		}
		if (rendered_tex) {
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Height(RG) & normal(BA) texture", GUILayout.MinWidth(200));
				EditorGUILayout.ObjectField(rendered_tex, typeof(Texture2D), false, GUILayout.MinWidth(150), GUILayout.MinHeight(150), GUILayout.MaxWidth(150), GUILayout.MaxHeight(150));
			EditorGUILayout.EndHorizontal();
			if (GUILayout.Button("Save texture")) {
				if (save_path=="") {
					directory=Path.GetDirectoryName(AssetDatabase.GetAssetPath(sourceTerrain));
					file=Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sourceTerrain))+".png";
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
			AssetImporter _importer=AssetImporter.GetAtPath(path);
			if (_importer) {
				TextureImporter tex_importer=(TextureImporter)_importer;
				tex_importer.wrapMode=TextureWrapMode.Clamp;
				tex_importer.sRGBTexture = false;
				tex_importer.mipmapEnabled = false;
				tex_importer.npotScale=TextureImporterNPOTScale.None;
				tex_importer.maxTextureSize=4096;
                tex_importer.textureCompression = TextureImporterCompression.Uncompressed;
            }
			AssetDatabase.ImportAsset(path,  ImportAssetOptions.ForceUpdate);
			finalize=true;
			rendered_tex=null;
		}							
	}
}