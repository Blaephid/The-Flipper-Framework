// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// C # manual conversion work by Yun Kyu Choi
//
// tweaked for RTP package so we can export mesh with right vertex order per triangle (should be the same as on Unity's terrain mesh that is dynamically created)
//
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Text;
 
enum SaveFormatRTPtweaked { Triangles, Quads }
enum SaveResolutionRTPtweaked { Full=0, Half, Quarter, Eighth, Sixteenth }
 
class ExportTerrainRTPtweaked : EditorWindow
{
   SaveFormatRTPtweaked saveFormat = SaveFormatRTPtweaked.Triangles;
   SaveResolutionRTPtweaked saveResolution = SaveResolutionRTPtweaked.Half;
	float loAngle=0;
	float hiAngle=90;
 
   static TerrainData terrain;
   static Vector3 terrainPos;
   static GameObject go;
   static bool AnchorOffset=true;
 
   int tCount;
   int counter;
   int totalCount;
   int progressUpdateInterval = 10000;
 
   [MenuItem("Window/Relief Tools/Export Terrain To Obj by steepnes")]
   static void Init()
   {
      terrain = null;
      Terrain terrainObject = Selection.activeObject as Terrain;
      if (!terrainObject)
      {
         terrainObject = Terrain.activeTerrain;
      }
      if (terrainObject)
      {
         terrain = terrainObject.terrainData;
         terrainPos = terrainObject.transform.position;
      }
 
      EditorWindow.GetWindow<ExportTerrainRTPtweaked>().Show();
   }
 
   void OnGUI()
   {
		go=EditorGUILayout.ObjectField("Terrain game object", go, typeof(GameObject), true) as GameObject;
		if (!go) return;
		Terrain terrainObject;
		terrainObject=go.GetComponent(typeof(Terrain)) as Terrain;
		if (!terrainObject) return;
		terrain=terrainObject.terrainData;
		terrainPos = terrainObject.transform.position;
		
        saveFormat = (SaveFormatRTPtweaked) EditorGUILayout.EnumPopup("Export Format", saveFormat);
		saveResolution = (SaveResolutionRTPtweaked) EditorGUILayout.EnumPopup("Resolution", saveResolution);
 
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField("Slope range (" + Mathf.RoundToInt(loAngle) + "\u00B0 - " + Mathf.RoundToInt(hiAngle) + "\u00B0)");
		if (saveFormat==SaveFormatRTPtweaked.Triangles) {
	  		EditorGUILayout.MinMaxSlider(ref loAngle, ref hiAngle, 0, 90);
		}
		EditorGUILayout.EndHorizontal ();

		AnchorOffset=EditorGUILayout.Toggle("Anchor Offset", AnchorOffset);

		
 
      if (GUILayout.Button("Export"))
      {
         Export();
      }
   }
 
   void Export()
   {
      string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");
      int w = terrain.heightmapResolution;
	  int h = terrain.heightmapResolution;
      Vector3 meshScale = terrain.size;
      int tRes = (int)Mathf.Pow(2, (int)saveResolution );
      meshScale = new Vector3(meshScale.z / (w - 1) * tRes, meshScale.y, meshScale.x / (h - 1) * tRes);
      Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
      float[,] tData = terrain.GetHeights(0, 0, w, h);
 
      w = (w - 1) / tRes + 1;
      h = (h - 1) / tRes + 1;
      Vector3[] tVertices = new Vector3[w * h];
      Vector2[] tUV = new Vector2[w * h];
 
      int[] tPolys;
 
      if (saveFormat == SaveFormatRTPtweaked.Triangles)
      {
         tPolys = new int[(w - 1) * (h - 1) * 6];
      }
      else
      {
         tPolys = new int[(w - 1) * (h - 1) * 4];
      }

	  terrainPos = new Vector3 (terrainPos.z, terrainPos.y, terrainPos.x);

      // Build vertices and UVs
      for (int y = 0; y < h; y++)
      {
         for (int x = 0; x < w; x++)
         {
            tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(x, tData[x * tRes, y * tRes], y));
			if (AnchorOffset) tVertices[y * w + x]+=terrainPos;
            tUV[y * w + x] = Vector2.Scale( new Vector2(x * tRes, y * tRes), uvScale);
         }
      }
 
      int  index = 0;
      if (saveFormat == SaveFormatRTPtweaked.Triangles)
      {
         // Build triangle indices: 3 indices into vertex array for each triangle
         for (int y = 0; y < h - 1; y++)
         {
            for (int x = 0; x < w - 1; x++)
            {
               // For each grid cell output two triangles
//               tPolys[index++] = (y * w) + x; // C
//               tPolys[index++] = ((y + 1) * w) + x; //A
//               tPolys[index++] = (y * w) + x + 1; // B
// 
//               tPolys[index++] = ((y + 1) * w) + x; //A
//               tPolys[index++] = ((y + 1) * w) + x + 1; // D
//               tPolys[index++] = (y * w) + x + 1; //B
					
                 tPolys[index++] = (y * w) + x; // C
	             tPolys[index++] = ((y + 1) * w) + x + 1; // D
	             tPolys[index++] = (y * w) + x + 1; // B
					
                 tPolys[index++] = (y * w) + x; // C
	             tPolys[index++] = ((y + 1) * w) + x; // A
	             tPolys[index++] = ((y + 1) * w) + x + 1; // D
            }
         }
		 	ArrayList tPolysReucedByAngle=new ArrayList();
			for(int p=0; p<tPolys.Length; p+=3) {
				Vector3 dir=Vector3.Cross(tVertices[tPolys[p+1]]-tVertices[tPolys[p]], tVertices[tPolys[p+2]]-tVertices[tPolys[p]]);
				float angle=Vector3.Angle(Vector3.up, dir);
				if (angle>=loAngle && angle<=hiAngle) {
					tPolysReucedByAngle.Add(tPolys[p]);
					tPolysReucedByAngle.Add(tPolys[p+1]);
					tPolysReucedByAngle.Add(tPolys[p+2]);
				}
			}
			tPolys=(int[])tPolysReucedByAngle.ToArray(typeof(int));
      }
      else
      {
         // Build quad indices: 4 indices into vertex array for each quad
         for (int y = 0; y < h - 1; y++)
         {
            for (int x = 0; x < w - 1; x++)
            {
               // For each grid cell output one quad
               tPolys[index++] = (y * w) + x;
               tPolys[index++] = ((y + 1) * w) + x;
               tPolys[index++] = ((y + 1) * w) + x + 1;
               tPolys[index++] = (y * w) + x + 1;
            }
         }
      }
 
      // Export to .obj
      StreamWriter sw = new StreamWriter(fileName);
      try
      {
 
         sw.WriteLine("# Unity terrain OBJ File");
 
         // Write vertices
         System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
         counter = tCount = 0;
         totalCount = (tVertices.Length * 2 + (saveFormat == SaveFormatRTPtweaked.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / progressUpdateInterval;
         for (int i = 0; i < tVertices.Length; i++)
         {
            UpdateProgress();
            StringBuilder sb = new StringBuilder("v ", 20);
            // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
            // Which is important when you're exporting huge terrains.
            sb.Append((-tVertices[i].z).ToString()).Append(" ").
               Append((tVertices[i].y).ToString()).Append(" ").
               Append((tVertices[i].x).ToString());
            sw.WriteLine(sb);
         }
         // Write UVs
         for (int i = 0; i < tUV.Length; i++)
         {
            UpdateProgress();
            StringBuilder sb = new StringBuilder("vt ", 22);
            sb.Append(tUV[i].y.ToString()).Append(" ").
               Append(tUV[i].x.ToString());
            sw.WriteLine(sb);
         }
         if (saveFormat == SaveFormatRTPtweaked.Triangles)
         {
            // Write triangles
            for (int i = 0; i < tPolys.Length; i += 3)
            {
               UpdateProgress();
               StringBuilder sb = new StringBuilder("f ", 43);
               sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                  Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                  Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
               sw.WriteLine(sb);
            }
         }
         else
         {
            // Write quads
            for (int i = 0; i < tPolys.Length; i += 4)
            {
               UpdateProgress();
               StringBuilder sb = new StringBuilder("f ", 57);
               sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                  Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                  Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
                  Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
               sw.WriteLine(sb);
            }
         }
      }
      catch(Exception err)
      {
         Debug.Log("Error saving file: " + err.Message);
      }
      sw.Close();
 
      terrain = null;
      EditorUtility.ClearProgressBar();
      EditorWindow.GetWindow<ExportTerrainRTPtweaked>().Close();
   }
 
   void UpdateProgress()
   {
      if (counter++ == progressUpdateInterval)
      {
         counter = 0;
         EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, totalCount, ++tCount));
      }
   }
}