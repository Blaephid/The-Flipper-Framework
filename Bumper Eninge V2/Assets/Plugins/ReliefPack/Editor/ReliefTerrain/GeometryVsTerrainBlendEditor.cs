using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
#if UNITY_EDITOR
using System.IO;
#endif

[CustomEditor (typeof(GeometryVsTerrainBlend))]
[CanEditMultipleObjects]
public class GeometryVsTerrainBlendEditor : Editor {
#if UNITY_EDITOR	
	private static double UpdTim=0;

	private static Vector3[] cover_verts=new Vector3[2000];
	private static float[] cover_strength=new float[2000];
	private static int[] cover_indices=new int[2000];
	private static int[] cover_tris=new int[600];
	private static int cover_num =0;
	private static int cover_num_start_drag =0;
	private static double lCovTim =0;
	private static bool control_down_flag =false;

	private static RaycastHit paintHitInfo;
	private static bool paintHitInfo_flag;
	private static Tool prev_tool;

	// icons
	private Texture paintButTexOn;
	private Texture paintButTexOff;

    private static GeometryVsTerrainBlendEditor cur_Editor;

    void OnEnable() {
		if (paintButTexOn==null) paintButTexOn=AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoPaintOn.png", typeof(Texture)) as Texture;
		if (paintButTexOff==null) paintButTexOff=AssetDatabase.LoadAssetAtPath("Assets/ReliefPack/Editor/ReliefTerrain/icons/icoPaintOff.png", typeof(Texture)) as Texture;
		
		prev_tool=UnityEditor.Tools.current;
		
		GeometryVsTerrainBlend _target=(GeometryVsTerrainBlend)target;
		if (_target.orig_mesh==null) {
			MeshFilter mf=_target.GetComponent(typeof(MeshFilter)) as MeshFilter;
			//if (AssetDatabase.GetAssetPath(mf.sharedMesh)!="") {
				_target.orig_mesh = mf.sharedMesh;	
			//}
		}
		if (!_target.blendedObject) {
			MeshFilter mf=_target.GetComponent(typeof(MeshFilter)) as MeshFilter;
		  	RaycastHit[] hits;
			hits=Physics.RaycastAll(_target.transform.position+_target.transform.up*20, -_target.transform.up, 100);
			int o;
			for(o=0; o<hits.Length; o++) {
				bool  ReliefTerrainBlended=hits[o].collider.gameObject.GetComponent(typeof(ReliefTerrain))!=null;
				bool  VoxelTerrainBlended=hits[o].collider.gameObject.GetComponent(typeof(ReliefTerrainVertexBlendTriplanar))!=null;
				if (ReliefTerrainBlended || VoxelTerrainBlended) {
					_target.VoxelBlendedObject=VoxelTerrainBlended;
					_target.blendedObject=hits[o].collider.gameObject;
					CheckShaderForBlendCapability();
					_target.MakeMeshCopy();
					_target.pmesh=mf.sharedMesh;
					EditorUtility.SetDirty(target);
					SceneView.RepaintAll();
					break;
				}
			}
		}		
	}
	
	void OnDisable() {
		GeometryVsTerrainBlend _target=(GeometryVsTerrainBlend)target;
		if (_target) {
			if (_target.paint_flag) {
                _target.paint_flag=false;
                RemoveSceneViewCallback();
            };
			EditorUtility.SetDirty(target);
		}
		UnityEditor.Tools.current=prev_tool;
	}

    void OnDestroy()
    {
        RemoveSceneViewCallback();
    }

    public override void OnInspectorGUI () {
		GeometryVsTerrainBlend _target=(GeometryVsTerrainBlend)target;
		MeshFilter mf=_target.GetComponent(typeof(MeshFilter)) as MeshFilter;
		if (_target.orig_mesh==null) {
			//if (AssetDatabase.GetAssetPath(mf.sharedMesh)!="") {
				_target.orig_mesh = mf.sharedMesh;	
				MeshCollider mc=_target.GetComponent(typeof(MeshCollider)) as MeshCollider;
				if (mc) {
					mc.sharedMesh=null;
					mc.sharedMesh=_target.orig_mesh;
				}
				if (_target.blendedObject) {
					CheckShaderForBlendCapability();
					_target.MakeMeshCopy();
					EditorUtility.SetDirty(target);
					SceneView.RepaintAll();
				}
			//}
		} else {
			if (mf.sharedMesh!=_target.pmesh) {
				if (AssetDatabase.GetAssetPath(mf.sharedMesh)!="") {
					_target.orig_mesh = mf.sharedMesh;		
					MeshCollider mc=_target.GetComponent(typeof(MeshCollider)) as MeshCollider;
					if (mc) {
						mc.sharedMesh=null;
						mc.sharedMesh=_target.orig_mesh;
					}
					CheckShaderForBlendCapability();
					_target.MakeMeshCopy();
					EditorUtility.SetDirty(target);
					SceneView.RepaintAll();
				}
				_target.pmesh=mf.sharedMesh;
			}
		}
		if (!_target.blendedObject) {
		  	RaycastHit[] hits;
			hits=Physics.RaycastAll(_target.transform.position+_target.transform.up*20, -_target.transform.up, 100);
			int o;
			for(o=0; o<hits.Length; o++) {
				bool  ReliefTerrainBlended=hits[o].collider.gameObject.GetComponent(typeof(ReliefTerrain))!=null;
				bool  VoxelTerrainBlended=hits[o].collider.gameObject.GetComponent(typeof(ReliefTerrainVertexBlendTriplanar))!=null;
				if (ReliefTerrainBlended || VoxelTerrainBlended) {
					_target.VoxelBlendedObject=VoxelTerrainBlended;
					_target.blendedObject=hits[o].collider.gameObject;
					CheckShaderForBlendCapability();
					_target.MakeMeshCopy();
					_target.pmesh=mf.sharedMesh;
					EditorUtility.SetDirty(target);
					SceneView.RepaintAll();
					break;
				}
			}
		}
		
		EditorGUILayout.HelpBox("This script allows you to prepare info for geometry blending shader (encoded in alpha component of vertex color) and modify alpha channel of global ColorMap (extrude height).",MessageType.Info, true);
		
		GameObject go=(GameObject)EditorGUILayout.ObjectField("Blended Object", _target.blendedObject, typeof(GameObject), true);
		GUILayout.Space(5);		
		if (go!=_target.blendedObject) {
			_target.blendedObject=go;
			_target.VoxelBlendedObject=go.GetComponent(typeof(ReliefTerrainVertexBlendTriplanar))!=null;
			if (go) {
				CheckShaderForBlendCapability();
				_target.MakeMeshCopy();
			}
			EditorUtility.SetDirty(target);
		}

		EditorGUILayout.BeginVertical("Box");
		//if (AssetDatabase.GetAssetPath( (_target.gameObject.GetComponent(typeof(MeshRenderer)) as MeshRenderer).sharedMaterial)!="") {
			if (GUILayout.Button(new GUIContent("Make material copy", "You can make independent clone\nof material used on object."))) {
				for(int i=0; i<targets.Length; i++) {
						GeometryVsTerrainBlend _atarget=(GeometryVsTerrainBlend)targets[i];
						Undo.RecordObject( _atarget.gameObject.GetComponent (typeof(MeshRenderer)), "Geometry Blend Edit");
						_atarget.MakeMaterialCopy();
				}
			}
			bool allUnbatched = !_target.isBatched;
			if (targets.Length > 1) {
				for (int i=0; i<targets.Length; i++) {
					GeometryVsTerrainBlend _atarget = (GeometryVsTerrainBlend)targets[i];
					if (_atarget.isBatched) {
						allUnbatched = false;
					}
				}
			}

			if (_target.blendedObject) {
						if (_target.shader_global_blend_capabilities) {
								if (GUILayout.Button (new GUIContent ("Sync material properties", "Will automatically setup terrain/mesh props used in blending material"))) {
										for (int i=0; i<targets.Length; i++) {
												GeometryVsTerrainBlend _atarget = (GeometryVsTerrainBlend)targets [i];
												if (_atarget.blendedObject && _atarget.shader_global_blend_capabilities) {
														_atarget.SyncMaterialProps ();
														_atarget.SetupValues ();		
												}
										}
								}
						}

						if (targets.Length > 1) {
								EditorGUILayout.BeginHorizontal ();
								Color bcol = GUI.backgroundColor;
								GUI.backgroundColor = new Color (0.8f, 0.99f, 1, 0.7f);
								if (GUILayout.Button (new GUIContent ("Unify materials used", "Will use the same material on all selected objects (good for mesh batching)"))) {
										if (EditorUtility.DisplayDialog ("RTP Warning", "Are you sure to overwrite all materials\n(that use the same shader) ?", "Yes, go on", "Cancel")) {
												////
												Material mat = null;
												Shader shader = null;
												for (int i=0; i<targets.Length; i++) {
														GeometryVsTerrainBlend _atarget = (GeometryVsTerrainBlend)targets [i];
														MeshRenderer mr = _atarget.GetComponent (typeof(MeshRenderer)) as MeshRenderer;
														// actual object
														if (mr != null && mr.sharedMaterial) {
																if (mat) {
																		// apply to other objects
																		if (shader == mr.sharedMaterial.shader) {
																				mr.sharedMaterial = mat;
																		}
																} else {
																		// get material from the first object 
																		mat = mr.sharedMaterial;
																		shader = mat.shader;
																}
														}
												}
												////
										}
								}
								GUI.backgroundColor = new Color (1.0f, 0.7f, 0.5f, 0.9f);
								if (allUnbatched) { // && PlayerSettings.advancedLicense) { // batching only in Pro (was in U4, in Unity5 it doesn't apply)
										if (GUILayout.Button (new GUIContent ("Prepare static batch", "Will combine all objects/underlying objects into combined batched mesh."))) {
												if (EditorUtility.DisplayDialog ("RTP Warning", "Are you sure to do batching ?\n\nDo it the the VERY END of your authoring\n(because you'll have to use reset to original mesh\nto restore separate meshes back).", "Yes, go on", "Cancel")) {

														// check everything and prapre arrays with objects to batch
														bool process_flag = true;
														ArrayList gos = new ArrayList ();
														ArrayList gos_underlying = new ArrayList ();
														int num_vertices = 0;

														for (int i=0; i<targets.Length; i++) {
																GeometryVsTerrainBlend _atarget = (GeometryVsTerrainBlend)targets [i];

																if (_atarget.transform.childCount != 1) {
																		EditorUtility.DisplayDialog ("RTP Error", "All of selected objects need to have underlying object. Check\"" + _atarget.name + "\" game object.", "OK");
																		process_flag = false;
																		break;
																}

																if (_atarget.blendedObject == null) {
																		EditorUtility.DisplayDialog ("RTP Error", "All of selected objects need to have blended object (terrain) defined. Check\"" + _atarget.name + "\" game object.", "OK");
																		process_flag = false;
																		break;
																}

																MeshFilter mfilter = _atarget.GetComponent (typeof(MeshFilter)) as MeshFilter;
																if (mfilter && mfilter.sharedMesh) {
																		num_vertices += mf.sharedMesh.vertexCount;
																		if (num_vertices > 65536) {
																				EditorUtility.DisplayDialog ("RTP Error", "Selected meshes got more than 65536 vertices so they won't be batched.\n\nUnselect some of the objects and try again.", "OK");
																				process_flag = false;
																				break;
																		}
																		gos.Add (_atarget.gameObject);
																} else {
																		EditorUtility.DisplayDialog ("RTP Error", "One of selected objects has no mesh ? Check\"" + _atarget.name + "\" game object.", "OK");
																		process_flag = false;
																		break;
																}
																MeshFilter mf_underlying = _atarget.GetComponent (typeof(MeshFilter)) as MeshFilter;
																if (mf_underlying && mf_underlying.sharedMesh) {
																		gos_underlying.Add (_atarget.transform.GetChild (0).gameObject);
																} else {
																		EditorUtility.DisplayDialog ("RTP Error", "One of selected objects has no mesh on underlying object ? Check\"" + _atarget.name + "\" game object.", "OK");
																		process_flag = false;
																		break;
																}
																// unify underlying objects materials (will be shared for the same blendedObject and the same shader used)
																MeshRenderer mr_underlying = _atarget.transform.GetChild (0).GetComponent (typeof(MeshRenderer)) as MeshRenderer;
																if (!mr_underlying || !mr_underlying.sharedMaterial) {
																		EditorUtility.DisplayDialog ("RTP Error", "One of selected objects has no renderer on underlying object ? Check\"" + _atarget.name + "\" game object.", "OK");
																		process_flag = false;
																		break;
																}
																for (int j=i+1; j<targets.Length; j++) {
																		GeometryVsTerrainBlend _atarget2 = (GeometryVsTerrainBlend)targets [j];
																		if (_atarget2.transform.childCount != 1) {
																				EditorUtility.DisplayDialog ("RTP Error", "All of selected objects need to have underlying object. Check\"" + _atarget2.name + "\" game object.", "OK");
																				process_flag = false;
																				break;
																		}
																		if (_atarget2.blendedObject == null) {
																				EditorUtility.DisplayDialog ("RTP Error", "All of selected objects need to have blended object (terrain) defined. Check\"" + _atarget2.name + "\" game object.", "OK");
																				process_flag = false;
																				break;
																		}
																		MeshRenderer mr2_underlying = _atarget2.transform.GetChild (0).GetComponent (typeof(MeshRenderer)) as MeshRenderer;
																		if (!mr2_underlying || !mr2_underlying.sharedMaterial) {
																				EditorUtility.DisplayDialog ("RTP Error", "One of selected objects has no renderer on underlying object ? Check\"" + _atarget2.name + "\" game object.", "OK");
																				process_flag = false;
																				break;
																		}
																		if ((_atarget.blendedObject == _atarget2.blendedObject) && (mr_underlying.sharedMaterial != mr2_underlying.sharedMaterial)) {
																				// unify material to make static batch actually working
																				mr2_underlying.sharedMaterial = mr_underlying.sharedMaterial;
																				//Debug.Log("Material copied from "+_atarget.name+" to "+_atarget2.name);
																		}
																}
																if (!process_flag) {
																		break;
																}
																////

														}

														if (process_flag) {
																if (gos.Count > 1) {
																		GameObject[] gos_array = (GameObject[])gos.ToArray (typeof(GameObject));
																		for( int j=0; j<gos_array.Length; j++) {
																			GeometryVsTerrainBlend blendScript = gos_array[j].GetComponent (typeof(GeometryVsTerrainBlend)) as GeometryVsTerrainBlend;
																			if (blendScript && !blendScript.isBatched) {
																				blendScript.isBatched=true;
																				allUnbatched=false;
																			}
																		}
																		GameObject[] gos_underlying_array = (GameObject[])gos_underlying.ToArray (typeof(GameObject));
																		GameObject batch_root = ((GeometryVsTerrainBlend)targets [0]).blendedObject;
																		StaticBatchingUtility.Combine (gos_array, batch_root);
																		StaticBatchingUtility.Combine (gos_underlying_array, batch_root);
																} else {
																		EditorUtility.DisplayDialog ("RTP Warning", "None prepared for batching.", "OK");
																}
														}

												}
										}
								} // show batching button
								GUI.backgroundColor = bcol;
								EditorGUILayout.EndHorizontal ();
						}
						GUILayout.Space (5);

						if (allUnbatched) {

						if (GUILayout.Button (new GUIContent ("Rebuild mesh", "(operation needed after modification\nof mesh transform)"))) {
								for (int i=0; i<targets.Length; i++) {
										GeometryVsTerrainBlend _atarget = (GeometryVsTerrainBlend)targets [i];
										RegisterUndoForMeshes (_atarget); 
										CheckShaderForBlendCapability (_atarget);
										_atarget.MakeMeshCopy ();
										_atarget.SetupValues ();						
										EditorUtility.SetDirty (targets [i]);
								}
						}

						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.BeginVertical ();
						bool stick_along_normals_flag=false;
						bool stick_downward_flag=false;
						if (GUILayout.Button (new GUIContent ("Stick to Blended Object along normals"))) {
							stick_along_normals_flag=true;
						}
						if (GUILayout.Button (new GUIContent ("Stick to Blended Object downward"))) {
							stick_downward_flag=true;
						}
						if (stick_along_normals_flag || stick_downward_flag) {
							for (int i=0; i<targets.Length; i++) {
								GeometryVsTerrainBlend _atarget = (GeometryVsTerrainBlend)targets [i];					
								MeshFilter mft = _atarget.GetComponent (typeof(MeshFilter)) as MeshFilter;
								if (mft != null) {						
									RegisterUndoForMeshes (_atarget);
									if (mft.sharedMesh == _atarget.orig_mesh) _atarget.MakeMeshCopy();
									_atarget.StickToBlendedObject(stick_along_normals_flag);
									Transform _underlying=_atarget.transform.Find("RTP_blend_underlying");
									if (_underlying) {
										_underlying.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
									}
									_atarget.SetupValues ();
									EditorUtility.SetDirty (targets [i]);
								}
							}
						}
						EditorGUILayout.EndVertical ();
						EditorGUILayout.BeginVertical ();
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Distance", GUILayout.Width (60));
						{
								float nval = EditorGUILayout.FloatField (_target.StickOffset);
								if (nval != _target.StickOffset) {
										_target.StickOffset = nval;
										for (int i=0; i<targets.Length; i++) {
												GeometryVsTerrainBlend _atarget = (GeometryVsTerrainBlend)targets [i];
												_atarget.StickOffset = _target.StickOffset;
										}
								} }			
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Optimize sticked child", GUILayout.Width (150));
						{
								bool nval = EditorGUILayout.Toggle (_target.StickedOptimized);
								if (nval != _target.StickedOptimized) {
										_target.StickedOptimized = nval;
										for (int i=0; i<targets.Length; i++) {
												GeometryVsTerrainBlend _atarget = (GeometryVsTerrainBlend)targets [i];
												_atarget.StickedOptimized = _target.StickedOptimized;
										}
								} }					
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.EndVertical ();
						EditorGUILayout.EndHorizontal ();

			
						if (_target.Sticked) {
								if (GUILayout.Button ("Export OBJ")) {
										string directory;
										string file;
										if (_target.save_path == "") {
												directory = Application.dataPath;
												file = "geom_blend_mesh.obj";
										} else {
												directory = Path.GetDirectoryName (_target.save_path);
												file = Path.GetFileNameWithoutExtension (_target.save_path) + ".obj";
										}
										string path = EditorUtility.SaveFilePanel ("Save mesh as OBJ file", directory, file, "obj");
										if (path != "") {
												path = path.Substring (Application.dataPath.Length - 6);
												_target.save_path = path;
												if (_target.GetComponent (typeof(MeshFilter)))
														RTPObjExporter.MeshToFile ((MeshFilter)_target.GetComponent (typeof(MeshFilter)), path);
										}										
								}
						}
			
//				if (_target.Sticked && _target.blendedObject && _target.blendedObject.GetComponent(typeof(Terrain)) && _target.blendedObject.GetComponent(typeof(TerrainCollider)) && _target.blendedObject.GetComponent(typeof(ReliefTerrain))) {
//					if (GUILayout.Button(new GUIContent("Unstretch UV"))) {
//						RegisterUndoForMeshes();
//						if (mf.sharedMesh==_target.orig_mesh) _target.MakeMeshCopy();
//						_target.unstretchUV();
//						EditorUtility.SetDirty(target);
//					}
//				}

						} // hide when batched
				
			}
		//}
		EditorGUILayout.EndVertical();
		GUILayout.Space(5);

		if (!allUnbatched) {
			EditorGUILayout.HelpBox("Selected object(s) has been batched, further manipulations are disabled.", MessageType.Warning, true);
		}
		GUILayout.Space(5);
		
		if (_target.blendedObject && _target.blendedObject.GetComponent(typeof(Collider))!=null  && ((_target.blendedObject.GetComponent(typeof(MeshRenderer))!=null) || (_target.blendedObject.GetComponent(typeof(Terrain))!=null))) {
			EditorGUILayout.BeginVertical("Box");
			//GUILayout.Space(10);
			if (GUILayout.Button(new GUIContent("Reset mesh to its original asset model", "Re-apply original model (below)"))) {
				for(int i=0; i<targets.Length; i++) {
					GeometryVsTerrainBlend _atarget=(GeometryVsTerrainBlend)targets[i];
					RegisterUndoForMeshes(_atarget);
					MeshFilter mft=_atarget.GetComponent(typeof(MeshFilter)) as MeshFilter;
					if (mft!=null) {
						mft.sharedMesh=_atarget.orig_mesh;
					}
					_atarget.pmesh=_atarget.orig_mesh;
					MeshCollider mc=_atarget.GetComponent(typeof(MeshCollider)) as MeshCollider;
					if (mc) {
						mc.sharedMesh=null;
						mc.sharedMesh=_atarget.orig_mesh;
					}
					_atarget.isBatched=false;
					_atarget.MakeMeshCopy();
					_atarget.Sticked=false;
					CheckShaderForBlendCapability(_atarget);
					_atarget.SetupValues();	
					EditorUtility.SetDirty(targets[i]);
				}				
			}
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Original (asset) model", GUILayout.MaxWidth(140));
			_target.orig_mesh=(Mesh)EditorGUILayout.ObjectField(_target.orig_mesh, typeof(Mesh), false);
			GUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			GUILayout.Space(10);
			
//			if (GUILayout.Button("Clear blending")) {
//				RegisterUndoForMeshes();
//				if (mf.sharedMesh==_target.orig_mesh) _target.MakeMeshCopy();
//				_target.ClearBlend();
//				EditorUtility.SetDirty(target);
//			}
			

			EditorGUILayout.BeginVertical("Box");

			if (allUnbatched) {
			EditorGUI.BeginDisabledGroup(!_target.Sticked);
			if (GUILayout.Button(new GUIContent("Autoblend at edges", "Autoblend painting for vertices\non the edges"))) {
				for(int i=0; i<targets.Length; i++) {
					GeometryVsTerrainBlend _atarget=(GeometryVsTerrainBlend)targets[i];	
					if (_atarget.Sticked) {
						MeshFilter mft=_atarget.GetComponent(typeof(MeshFilter)) as MeshFilter;
						if (mft!=null) {						
							RegisterUndoForMeshes(_atarget);
							if (mft.sharedMesh==_atarget.orig_mesh) _atarget.MakeMeshCopy();
							_atarget.PrepareMeshAtEdges();
							_atarget.SetupValues();	
							EditorUtility.SetDirty(targets[i]);
						}
					}
				}
			}
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(_target.Sticked);
			//GUILayout.Space(10);"
			if (GUILayout.Button(new GUIContent("Flatten mesh near the ground", "Blended vertices near given distance (below) will be smoothly punched out"))) {
				for(int i=0; i<targets.Length; i++) {
					GeometryVsTerrainBlend _atarget=(GeometryVsTerrainBlend)targets[i];					
					MeshFilter mft=_atarget.GetComponent(typeof(MeshFilter)) as MeshFilter;
					if (mft!=null) {						
						RegisterUndoForMeshes(_atarget);
						if (mft.sharedMesh==_atarget.orig_mesh) _atarget.MakeMeshCopy();
						_atarget.FlattenMesh();
						MeshCollider mc=_atarget.GetComponent(typeof(MeshCollider)) as MeshCollider;
						if (mc) {
							mc.sharedMesh=null;
							mc.sharedMesh=mft.sharedMesh;
						}
						EditorUtility.SetDirty(targets[i]);
					}
				}
			}
			if (GUILayout.Button(new GUIContent("Tessellate mesh on ground intersections","This will introduce additional mesh edges at its ground intersections"))) {
				for(int i=0; i<targets.Length; i++) {
					GeometryVsTerrainBlend _atarget=(GeometryVsTerrainBlend)targets[i];					
					MeshFilter mft=_atarget.GetComponent(typeof(MeshFilter)) as MeshFilter;
					if (mft!=null) {						
						RegisterUndoForMeshes(_atarget);
						if (mft.sharedMesh==_atarget.orig_mesh) _atarget.MakeMeshCopy();
						_atarget.TessellateMesh();
						EditorUtility.SetDirty(targets[i]);
					}
				}
			}
			if (GUILayout.Button(new GUIContent("Remove tris below ground level","This will remove triangles which vertices are placed below ground"))) {
				for(int i=0; i<targets.Length; i++) {
					GeometryVsTerrainBlend _atarget=(GeometryVsTerrainBlend)targets[i];					
					MeshFilter mft=_atarget.GetComponent(typeof(MeshFilter)) as MeshFilter;
					if (mft!=null) {							
						RegisterUndoForMeshes(_atarget);
						if (mft.sharedMesh==_atarget.orig_mesh) _atarget.MakeMeshCopy();
						_atarget.GetTrisBelowGroundLevel(ref cover_num, cover_tris);
						_atarget.remove_tris(cover_num, cover_tris);
						CleanUpAfterRemovingTris(_atarget);
						_atarget.ClearPaintMesh();
						EditorUtility.SetDirty(targets[i]);
					}
				}
				cover_num=0;								
			}
			if (GUILayout.Button(new GUIContent("Autoblend from vertices distance", "Autoblend painting for vertices\non distance given below"))) {
				for(int i=0; i<targets.Length; i++) {
					GeometryVsTerrainBlend _atarget=(GeometryVsTerrainBlend)targets[i];					
					MeshFilter mft=_atarget.GetComponent(typeof(MeshFilter)) as MeshFilter;
					if (mft!=null) {					
						RegisterUndoForMeshes(_atarget);
						if (mft.sharedMesh==_atarget.orig_mesh) _atarget.MakeMeshCopy();
						_atarget.PrepareMesh();
						_atarget.SetupValues();	
						EditorUtility.SetDirty(targets[i]);
					}
				}
			}
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Autoblend / flatten distance", GUILayout.MinWidth(165));
			{ float nval=EditorGUILayout.Slider(_target.blend_distance, 0.02f, 1f);
			if (nval!=_target.blend_distance) {
				_target.blend_distance=nval;
				for(int i=0; i<targets.Length; i++) {
					GeometryVsTerrainBlend _atarget=(GeometryVsTerrainBlend)targets[i];
					_atarget.blend_distance=_target.blend_distance;
				}
			} }
			EditorGUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();
			
			} // hide when batched

			//EditorGUILayout.BeginHorizontal();
			//	EditorGUILayout.LabelField("Constant gloss for deferred", GUILayout.MinWidth(165));
			//	{ float nval=EditorGUILayout.Slider(_target._DeferredBlendGloss,0,1);
			//	if (nval!=_target._DeferredBlendGloss) {
			//		_target._DeferredBlendGloss=nval;
			//		for(int i=0; i<targets.Length; i++) {
			//			GeometryVsTerrainBlend _atarget=(GeometryVsTerrainBlend)targets[i];
			//			_atarget._DeferredBlendGloss=_target._DeferredBlendGloss;
			//		}
			//	} }
			//EditorGUILayout.EndHorizontal();			
			EditorGUILayout.EndVertical();
			
			if (allUnbatched) {

			GUILayout.Space(5);
			EditorGUILayout.BeginVertical("Box");			
			bool prev_paint_flag=_target.paint_flag;
			
			if (!_target.paint_flag) {
				Color c=GUI.color;
				GUI.color=new Color(0.9f,1, 0.9f);
				if (GUILayout.Button(new GUIContent("Begin painting (M)",paintButTexOn, "Click to turn on painting"))) {
					_target.paint_flag=true;
				}
				GUI.color=c;
			} else if (_target.paint_flag) {
				Color c=GUI.color;
				GUI.color=new Color(1,0.9f,0.9f);
				if (GUILayout.Button(new GUIContent("End painting (M)",paintButTexOff, "Click to turn off painting"))) {
					_target.paint_flag=false;
				}
				GUI.color=c;
			}
			if (!prev_paint_flag && _target.paint_flag) {
				UnityEditor.Tools.current=Tool.View;
                AddSceneViewCallback();
            } else if (prev_paint_flag && !_target.paint_flag) {
				UnityEditor.Tools.current=prev_tool;
                RemoveSceneViewCallback();
			}
			if (prev_paint_flag!=_target.paint_flag) EditorUtility.SetDirty(target);
			if (_target.paint_flag) {
				_target.ClearPaintMesh();
				cover_num=0;
				
				if (_target.paint_mode<1 && (!_target.GetComponent<Collider>() || !_target.GetComponent<Collider>().enabled)) {
					EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(420));
					EditorGUILayout.HelpBox("Object doesn't have collider\n(necessary for manual painting).",MessageType.Error, true);
					Color bcol=GUI.backgroundColor;
					GUI.backgroundColor=new Color(0.8f, 0.99f, 1, 0.7f);
					if (GUILayout.Button("Add mesh collider", GUILayout.MinHeight(38))) {
						_target.gameObject.AddComponent(typeof(MeshCollider));
					}
					GUI.backgroundColor=bcol;
					EditorGUILayout.EndHorizontal();
				}
				if (!_target.ModifyTris) {
					EditorGUILayout.HelpBox("Hold SHIFT while painting to apply eraser.",MessageType.Info, true);
				}
					
				string[] modes;
				int npaint_mode;
				if (_target.Sticked) {
					if (_target.blendedObject && _target.blendedObject.GetComponent<Collider>()) {// && (_target.blendedObject.collider is TerrainCollider)) {
						modes=new string[4] {"Blend", "Extrude Height", "Add tris", "Del tris"};
						int cur_paint_mode=_target.paint_mode;
						if (_target.ModifyTris) {
							if (_target.BuildMeshFlag) {
								cur_paint_mode=2;
							} else {
								cur_paint_mode=3;
							}
						}
						npaint_mode=GUILayout.SelectionGrid(cur_paint_mode, modes, 4);
						if (npaint_mode==3) {
							npaint_mode=0;
							_target.ModifyTris=true;
							_target.BuildMeshFlag=false;
						} else if (npaint_mode==2) {
							npaint_mode=0;
							_target.ModifyTris=true;
							_target.BuildMeshFlag=true;
						} else {
							_target.ModifyTris=false;
							_target.BuildMeshFlag=false;
						}
					} else {
						modes=new string[3] {"Blend", "Extrude Height", "Del tris"};
						npaint_mode=GUILayout.SelectionGrid(_target.ModifyTris ? 2:_target.paint_mode, modes, 3);
						if (npaint_mode==2) {
							npaint_mode=0;
							_target.ModifyTris=true;
							_target.BuildMeshFlag=false;
						} else {
							_target.ModifyTris=false;
							_target.BuildMeshFlag=false;
						}
					}
				} else {
					modes=new string[2] {"Blend", "Extrude Height"};
					npaint_mode=GUILayout.SelectionGrid(_target.paint_mode, modes, 2);
					_target.ModifyTris=false;
				}
				
				if (npaint_mode!=_target.paint_mode) {
					EditorUtility.SetDirty(target);
				}
				_target.paint_mode=npaint_mode;
				if (_target.paint_mode==1) {
					Texture2D tmp_tex=_target.get_tmpColorMap();
					if (tmp_tex && (tmp_tex.format!=TextureFormat.Alpha8 && tmp_tex.format!=TextureFormat.ARGB32)) {
						EditorGUILayout.HelpBox("Global colormap need to be readable and uncompressed for painting.",MessageType.Error, true);
					}
				}
				if (_target.ModifyTris) {
					EditorGUI.BeginDisabledGroup(!_target.BuildMeshFlag);
					GUILayout.BeginHorizontal();
						GUILayout.Label ("Subdivision", EditorStyles.label );
						_target.addTrisSubdivision = EditorGUILayout.IntSlider(_target.addTrisSubdivision, -3, 3);
					GUILayout.EndHorizontal();	
					GUILayout.BeginHorizontal();
						GUILayout.Label ("Slope range (" + Mathf.RoundToInt(_target.addTrisMinAngle) + "\u00B0 - " + Mathf.RoundToInt(_target.addTrisMaxAngle) + "\u00B0)", EditorStyles.label );
						EditorGUILayout.MinMaxSlider(ref _target.addTrisMinAngle, ref _target.addTrisMaxAngle, 0, 90);
					GUILayout.EndHorizontal();	
					EditorGUI.EndDisabledGroup();
				}
				GUILayout.BeginHorizontal();
					GUILayout.Label ("Area size", EditorStyles.label );
					_target.paint_size = EditorGUILayout.Slider(_target.paint_size, 0.01f, 6);
				GUILayout.EndHorizontal();	
				EditorGUI.BeginDisabledGroup(_target.ModifyTris);
				GUILayout.BeginHorizontal();
					GUILayout.Label ("Area smoothness", EditorStyles.label );
					_target.paint_smoothness = EditorGUILayout.Slider (_target.paint_smoothness, 0.001f, 1);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
					GUILayout.Label ("Opacity", EditorStyles.label );
					_target.paint_opacity = EditorGUILayout.Slider (_target.paint_opacity, 0, 1);
				GUILayout.EndHorizontal();	
				EditorGUILayout.HelpBox("Mesh blend uses A channel. You might use different if you'd like to paint another material properties.", MessageType.Info, true);
				EditorGUILayout.BeginHorizontal();
					GUILayout.Label ("vertex channel", EditorStyles.label );
					_target.vertex_paint_channel = (RTPColorChannels)EditorGUILayout.EnumPopup(_target.vertex_paint_channel);
				EditorGUILayout.EndHorizontal();
				if (GUILayout.Button(new GUIContent("Clear channel"))) {
					_target.ClearBlend(_target.vertex_paint_channel, 0);	
				}
				if (GUILayout.Button(new GUIContent("Set channel"))) {
					_target.ClearBlend(_target.vertex_paint_channel, 1);	
				}
				EditorGUI.EndDisabledGroup();
	
				GUILayout.Space(10);
			}
			EditorGUILayout.EndVertical();
			
			} // hide when batched

		} else {
			EditorGUILayout.HelpBox("Select an object (terrain or GameObject with mesh) to be blended with this object.",MessageType.Warning, true);
		}
		
//		GUILayout.Space(5);
//		EditorGUILayout.HelpBox("Underlying child object renders blended underlying terrain. You might need to select it for debugging purposes.",MessageType.Info, true);
//		{ bool nval=!EditorGUILayout.Toggle("Enable child selection", !_target.dont_select_aux_object);
//		if (nval!=_target.dont_select_aux_object) {
//			_target.dont_select_aux_object=nval;
//			for(int i=0; i<targets.Length; i++) {
//				GeometryVsTerrainBlend _atarget=(GeometryVsTerrainBlend)targets[i];
//				_atarget.dont_select_aux_object=_target.dont_select_aux_object;
//			}
//		} }		
		GUILayout.Space(5);
		
		Event current = Event.current;
		switch(current.type) {
			case EventType.KeyDown:
				if (current.keyCode==KeyCode.M) {
					_target.paint_flag=!_target.paint_flag;
					if (_target.paint_flag) {
						UnityEditor.Tools.current=Tool.View;
                        AddSceneViewCallback();
					} else {
						UnityEditor.Tools.current=prev_tool;
                        RemoveSceneViewCallback();
                    }
                    EditorUtility.SetDirty(target);
				}
			break;
		}
		
		if (EditorApplication.timeSinceStartup>UpdTim) {
			UpdTim=EditorApplication.timeSinceStartup+1;	
			_target.SetupValues();		
		}
		//DrawDefaultInspector();
	}
	
    void AddSceneViewCallback()
    {
        GeometryVsTerrainBlend.cur_EditorTarget = (GeometryVsTerrainBlend)target;
        GeometryVsTerrainBlendEditor.cur_Editor = this;
#if UNITY_2019_1_OR_NEWER
        SceneView.duringSceneGui += CustomOnSceneGUI_S;
#else
        GeometryVsTerrainBlend._SceneGUI = new SceneView.OnSceneFunc(CustomOnSceneGUI);
		SceneView.onSceneGUIDelegate += GeometryVsTerrainBlend._SceneGUI;
#endif
    }
    static void RemoveSceneViewCallback()
    {
#if UNITY_2019_1_OR_NEWER
		SceneView.duringSceneGui -= CustomOnSceneGUI_S;
#else
		SceneView.onSceneGUIDelegate -= GeometryVsTerrainBlend._SceneGUI;
#endif
    }

	public void OnSceneGUI() {
		GeometryVsTerrainBlend _target=(GeometryVsTerrainBlend)target;
		
		if (Event.current.type==EventType.KeyDown) {
			if (Event.current.keyCode==KeyCode.M) {
				_target.paint_flag=!_target.paint_flag;
				if (_target.paint_flag) {
					_target.ClearPaintMesh();
					cover_num=0;
					UnityEditor.Tools.current=Tool.View;
                    AddSceneViewCallback();
                    RemoveSceneViewCallback();
                }
                else
                {
					UnityEditor.Tools.current=prev_tool;
                    RemoveSceneViewCallback();
                }
				EditorUtility.SetDirty(target);
			}
		}
	}

    // obsolete handler (left for compatibility)
    public void CustomOnSceneGUI(SceneView sceneview)
    {
        GeometryVsTerrainBlendEditor.CustomOnSceneGUI_Internal(sceneview, (GeometryVsTerrainBlend)target);
    }

    // new handler used via SceneView.duringSceneGUI
    public static void CustomOnSceneGUI_S(SceneView sceneview)
    {
        if (GeometryVsTerrainBlend.cur_EditorTarget) {
            GeometryVsTerrainBlendEditor.CustomOnSceneGUI_Internal(sceneview, GeometryVsTerrainBlend.cur_EditorTarget);
        }
    }

    // new wrapper to handle deprecated and new SceneView handling
    public static void CustomOnSceneGUI_Internal(SceneView sceneview, GeometryVsTerrainBlend target) {
        if (target == null) return;
        GeometryVsTerrainBlend _target = target;

        EditorWindow currentWindow = EditorWindow.mouseOverWindow;
		if(!currentWindow) return;
		
		//Rect winRect = currentWindow.position;
		Event current = Event.current;
		
		if (current.alt) {
			return;
		}		
		if (Event.current.button == 1) {
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			return;
		}
		
		if (UnityEditor.Tools.current!=Tool.View) {
			 _target.paint_flag=false;
            RemoveSceneViewCallback();
            EditorUtility.SetDirty(target);
			return;
		}		
		
		if (current.type==EventType.Layout) {
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			return;
		}
		
		switch(current.type) {
			case EventType.KeyDown:
				if (current.keyCode==KeyCode.Escape) {
					_target.ClearPaintMesh();
					cover_num=0;
					_target.paint_flag=false;
					UnityEditor.Tools.current=prev_tool;
                    RemoveSceneViewCallback();
                    EditorUtility.SetDirty(target);
				}
			break;
		}

		if (current.control) {
				if (current.type==EventType.MouseMove) {
					if (control_down_flag) {
						control_down_flag=false;
						EditorUtility.SetDirty(target);
					}
				}
				return;
		}
		control_down_flag=true;
		
		switch(current.type) {
			case EventType.MouseDown:
				get_paint_coverage(current.shift);
				cover_num_start_drag=cover_num;
				// Debug.Log(""+cover_num + "  "+ paintHitInfo_flag + _target.prepare_tmpColorMap());
				if (cover_num>0 || paintHitInfo_flag) {
					if (paintHitInfo_flag) {
						if (!_target.ModifyTris && (!paintHitInfo_flag || _target.prepare_tmpColorMap())) {
							Undo.RegisterCompleteObjectUndo(_target.get_tmpColorMap(), "Geometry Blend Edit");
							_target.modify_blend(cover_num, cover_indices, cover_strength, !current.shift);
							//current.Use();
						}
					} else {
						if (cover_num_start_drag>0) {
							RegisterUndoForMeshes();
							_target.undo_flag=false;
							if (_target.ModifyTris) {
								if (_target.BuildMeshFlag) {
									_target.add_tris(cover_num, cover_verts);
								} else {
									_target.remove_tris(cover_num, cover_tris);
								}
								cover_num=0;
							} else {
								_target.modify_blend(cover_num, cover_indices, cover_strength, !current.shift);
							}
							EditorUtility.SetDirty(target);
						}
					}
				} else {
					_target.undo_flag=true;
				}
				current.Use();
				_target.RealizePaint_Flag=true;
			break;
			case EventType.MouseDrag:
				get_paint_coverage(current.shift);
				if (cover_num>0 || paintHitInfo_flag) {
					if (_target.undo_flag) {
						if (paintHitInfo_flag &&_target.prepare_tmpColorMap()) {
							if (paintHitInfo_flag) {
								Undo.RegisterCompleteObjectUndo(_target.get_tmpColorMap(), "Geometry Blend Edit");
							}
						} else {
							RegisterUndoForMeshes();
						}
						_target.undo_flag=false;
					}
				}
				if (cover_num_start_drag>0 || paintHitInfo_flag) {
					if (!_target.ModifyTris && (!paintHitInfo_flag || _target.prepare_tmpColorMap())) {
						_target.modify_blend(cover_num, cover_indices, cover_strength, !current.shift);
						//current.Use();
					}
					if (_target.ModifyTris) {
						if (_target.BuildMeshFlag) {
							_target.add_tris(cover_num, cover_verts);
						} else {
							_target.remove_tris(cover_num, cover_tris);
						}
						cover_num=0;
					}
				}
				_target.RealizePaint_Flag=true;
			break;
			case EventType.MouseMove:
				get_paint_coverage(current.shift);
				if (_target.RealizePaint_Flag && _target.ModifyTris) {
					if (_target.BuildMeshFlag) {
						// optimize mesh after add tris
						_target.RealizePaint_Flag=false;
						MeshFilter mf=_target.gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
						RemoveUnusedVerts(mf);
						if (_target.GetComponent<Collider>() && _target.GetComponent<Collider>() is MeshCollider) {
							if (mf) {
								(_target.GetComponent<Collider>() as MeshCollider).sharedMesh=mf.sharedMesh;
							}
						}				
						Transform tr=_target.transform.Find("RTP_blend_underlying");
						if (tr!=null) {
							GameObject go=tr.gameObject;
							MeshFilter mf_under=(MeshFilter)go.GetComponent(typeof(MeshFilter));
							Color[] prev_cols=null;
							if (mf_under) {
								prev_cols=mf_under.sharedMesh.colors;
							}
							_target.MakeMeshCopy();
							if (mf_under) {
								Color[] cur_cols=mf_under.sharedMesh.colors;
								for(int j=0; j<prev_cols.Length; j++) {
									cur_cols[j].a=prev_cols[j].a;
								}
								mf_under.sharedMesh.colors=cur_cols;
								if (mf) {
									Color[] colors=mf.sharedMesh.colors;
									for(int j=0; j<prev_cols.Length; j++) {
										colors[j].a=prev_cols[j].a;
									}
									mf.sharedMesh.colors=colors;
								}
							}						
						}
						_target.ClearPaintMesh();
						cover_num=0;
					} else {
						// optimize mesh after remove tris
						_target.RealizePaint_Flag=false;
						CleanUpAfterRemovingTris(_target);
						_target.ClearPaintMesh();
						cover_num=0;	
					}
				}
			break;
		}
		
		int i;
		if (_target.ModifyTris) {
			if (_target.BuildMeshFlag) {
				// add
				Handles.color=Color.green;
				for(i=0; i<cover_num; i++) {
					if (cover_verts[i].x!=0 && cover_verts[i].y!=0 && cover_verts[i].z!=0) {
						Handles.DrawSolidDisc(cover_verts[i], Camera.current.transform.position-cover_verts[i], HandleUtility.GetHandleSize(cover_verts[i])*0.03f);
					}
				}
			} else {
				// remove
				Vector3[] verts=_target.get_vertices();
				int[] tris=_target.get_tris();
				if (verts!=null) {
					Handles.color=Color.red;
					for(i=0; i<cover_num; i+=3) {
						Vector3 pntA=_target.transform.TransformPoint(verts[tris[cover_tris[i]]]);
						Vector3 pntB=_target.transform.TransformPoint(verts[tris[cover_tris[i]+1]]);
						Vector3 pntC=_target.transform.TransformPoint(verts[tris[cover_tris[i]+2]]);
						Handles.DrawLine(pntA, pntB);
						Handles.DrawLine(pntB, pntC);
						Handles.DrawLine(pntA, pntC);
					}
				}
			}
		} else {
			if (current.shift) {
				if (_target.paint_mode<1) {
					for(i=0; i<cover_num; i++) {
						Handles.color=new Color(1,0,0, Mathf.Max(0.1f,cover_strength[i]*_target.paint_opacity));
						Handles.DrawSolidDisc(cover_verts[i], Camera.current.transform.position-cover_verts[i], HandleUtility.GetHandleSize(cover_verts[i])*0.03f);
					}					
				} else {
					if (paintHitInfo_flag) {
						Handles.color=new Color(1,0,0, Mathf.Max(0.1f, _target.paint_opacity*0.5f));
						Handles.DrawWireDisc(paintHitInfo.point, paintHitInfo.normal, _target.paint_size);
						Handles.color=new Color(1,0,0, Mathf.Max(0.6f, _target.paint_opacity));
						Handles.DrawWireDisc(paintHitInfo.point, paintHitInfo.normal, _target.paint_size*Mathf.Max(0.3f, 1 -_target.paint_smoothness));
					}
				}
			} else {
				if (_target.paint_mode<1) {
					for(i=0; i<cover_num; i++) {
						Handles.color=new Color(0,1,0,Mathf.Max(0.1f, cover_strength[i]*_target.paint_opacity));
						Handles.DrawSolidDisc(cover_verts[i], Camera.current.transform.position-cover_verts[i], HandleUtility.GetHandleSize(cover_verts[i])*0.03f);
					}
				} else {
					if (paintHitInfo_flag) {
						Handles.color=new Color(0,1,0, Mathf.Max(0.1f, _target.paint_opacity*0.5f));
						Handles.DrawWireDisc(paintHitInfo.point, paintHitInfo.normal, _target.paint_size);
						Handles.color=new Color(0,1,0, Mathf.Max(0.6f, _target.paint_opacity));
						Handles.DrawWireDisc(paintHitInfo.point, paintHitInfo.normal, _target.paint_size*Mathf.Max(0.3f, 1 -_target.paint_smoothness));
					}
				}
			}		
		}
		
		//if (current.shift) current.Use();
	}
	
	private static void CleanUpAfterRemovingTris(GeometryVsTerrainBlend _target) {
		MeshFilter mf=_target.gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
		RemoveUnusedVerts(mf);
		if (_target.GetComponent<Collider>() && _target.GetComponent<Collider>() is MeshCollider) {
			if (mf) {
				(_target.GetComponent<Collider>() as MeshCollider).sharedMesh=mf.sharedMesh;
			}
		}
		Transform tr=_target.transform.Find("RTP_blend_underlying");
		if (tr!=null) {
			GameObject go=tr.gameObject;
			mf=(MeshFilter)go.GetComponent(typeof(MeshFilter));
			RemoveUnusedVerts(mf);
		}
	}

	private static void get_paint_coverage(bool shift_flag) {
		if (EditorApplication.timeSinceStartup<lCovTim) return;
		lCovTim=EditorApplication.timeSinceStartup+0.02;
        UnityEngine.Object target = GeometryVsTerrainBlend.cur_EditorTarget;
        GeometryVsTerrainBlend _target = GeometryVsTerrainBlend.cur_EditorTarget;
		if (_target.Sticked) {
			if (!_target.blendedObject || !_target.blendedObject.GetComponent<Collider>()) return;
		} else {
			if (!_target.GetComponent<Collider>()) return;
		}
		if (_target.paint_mode<1) {
			Vector3[] vertices=_target.get_vertices();
			Vector3[] normals=_target.get_normals();
			int[] tris=_target.get_tris();
			Ray gui_ray=HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			Vector3 gui_ray_dir=_target.transform.InverseTransformDirection(gui_ray.direction);
			if (_target.ModifyTris) {
				Terrain terrainComp=(Terrain)_target.blendedObject.GetComponent(typeof(Terrain));
				if (_target.BuildMeshFlag) {
					// add tris
					if (terrainComp) {
						float cellW=terrainComp.terrainData.size.x/(terrainComp.terrainData.heightmapResolution-1)*Mathf.Pow(2.0f, -1.0f*_target.addTrisSubdivision);
						float cellH=terrainComp.terrainData.size.z/(terrainComp.terrainData.heightmapResolution-1)*Mathf.Pow(2.0f, -1.0f*_target.addTrisSubdivision);
						int _w=Mathf.RoundToInt((_target.paint_size*2)/cellW+1);
						int _h=Mathf.RoundToInt((_target.paint_size*2)/cellH+1);
						_w=_w>44 ? 44:_w;
						_h=_h>44 ? 44:_h;
						cover_num=0;
						TerrainCollider terrainCollider=(TerrainCollider)_target.blendedObject.GetComponent(typeof(TerrainCollider));
						Vector3 pnt=GetWorldPointFromMouse(terrainCollider);
						Vector3 terrainPosition = terrainComp.GetPosition();
						int centerX = Mathf.RoundToInt( (pnt.x-terrainPosition.x)/cellW );
						int centerZ = Mathf.RoundToInt( (pnt.z-terrainPosition.z)/cellH );
						bool bicubic_flag=false;
						Texture2D GlobalHNMap=_target.GetGlobalHNMap(ref bicubic_flag); // not null means we've got tessellation turned on and its texture here
						if (GlobalHNMap) {
							float xf=cellW/terrainComp.terrainData.size.x;
							float zf=cellH/terrainComp.terrainData.size.z;
							float terrain_height=terrainComp.terrainData.size.y;
							Vector3 norm=new Vector3(); // not used here
							for(int i=0; i<_w; i++) {
								for(int j=0; j<_h; j++) {
									int idx_i=i-_w/2+centerX;
									int idx_j=j-_h/2+centerZ;
									float _u=idx_i*xf;
									float _v=idx_j*zf;
									float tess_height;
									if (bicubic_flag) {
										tess_height=GeometryVsTerrainBlend.interpolate_bicubic(_u,_v, GlobalHNMap, ref norm)*terrain_height;
									} else {
										Color col=GeometryVsTerrainBlend.CustGetPixelBilinear(GlobalHNMap, _u,_v);
										tess_height=((1.0f/255)*col.g + col.r)*terrain_height;
									}
									cover_verts[cover_num]=new Vector3(terrainPosition.x+idx_i*cellW, tess_height+0.02f, terrainPosition.z+idx_j*cellH);
									cover_num++;
								}
							}
						} else {
							for(int i=0; i<_w; i++) {
								for(int j=0; j<_h; j++) {
									int idx_i=i-_w/2+centerX;
									int idx_j=j-_h/2+centerZ;
									cover_verts[cover_num]=new Vector3(terrainPosition.x+idx_i*cellW, terrainComp.terrainData.GetInterpolatedHeight(idx_i*cellW/terrainComp.terrainData.size.x, idx_j*cellH/terrainComp.terrainData.size.z)+terrainPosition.y+0.02f, terrainPosition.z+idx_j*cellH);
									cover_num++;
								}
							}
						}
					} else {
						ReliefTerrain geomScript=(ReliefTerrain)_target.blendedObject.GetComponent(typeof(ReliefTerrain));
						if ((geomScript==null) || geomScript.controlA!=null) {
							float cellW;
							float cellH;
							Vector3 paintSurfaceNormal=Vector3.down;
							if (geomScript!=null) {
								cellW=_target.blendedObject.GetComponent<Renderer>().bounds.size.x/(geomScript.controlA.width)*Mathf.Pow(2.0f, -1.0f*_target.addTrisSubdivision);
								cellH=_target.blendedObject.GetComponent<Renderer>().bounds.size.z/(geomScript.controlA.height)*Mathf.Pow(2.0f, -1.0f*_target.addTrisSubdivision);
							} else {
								cellW=0.5f*Mathf.Pow(2.0f, -1.0f*_target.addTrisSubdivision);
								cellH=0.5f*Mathf.Pow(2.0f, -1.0f*_target.addTrisSubdivision);
//								Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
//								RaycastHit paintHitInfoTmp=new RaycastHit();
//								if (_target.blendedObject && _target.blendedObject.collider) {
//									if (_target.blendedObject.collider.Raycast(ray, out paintHitInfoTmp, Mathf.Infinity)) {
//										paintSurfaceNormal=-paintHitInfoTmp.normal;
//									}
//								}								
							}
							int _w=Mathf.RoundToInt((_target.paint_size*2)/cellW+1);
							int _h=Mathf.RoundToInt((_target.paint_size*2)/cellH+1);
							_w=_w>44 ? 44:_w;
							_h=_h>44 ? 44:_h;
							cover_num=0;
							Collider terrainCollider=(Collider)_target.blendedObject.GetComponent(typeof(Collider));
							Vector3 pnt=GetWorldPointFromMouse(terrainCollider);
							Vector3 terrainPosition=_target.blendedObject.GetComponent<Renderer>().bounds.min;
							int centerX=Mathf.RoundToInt( (pnt.x-terrainPosition.x)/cellW );
							int centerZ=Mathf.RoundToInt( (pnt.z-terrainPosition.z)/cellH );

							// needed for tessellation below
							bool bicubic_flag=false;
							Texture2D GlobalHNMap=_target.GetGlobalHNMap(ref bicubic_flag); // not null means we've got tessellation turned on and its texture here
							float tess_height = 0;
							float _TessYOffset = 0;
							if (GlobalHNMap!=null) {
								ReliefTerrain rt=_target.blendedObject.GetComponent<ReliefTerrain>();
								if (rt) {
									ReliefTerrainGlobalSettingsHolder rtg=rt.globalSettingsHolder;
									tess_height=rtg.tessHeight;
									_TessYOffset=rtg._TessYOffset;
								} else {
									GlobalHNMap=null; // paint on standalone mesh (voxel)
								}
							}

							for(int i=0; i<_w; i++) {
								for(int j=0; j<_h; j++) {
									int idx_i=i-_w/2+centerX;
									int idx_j=j-_h/2+centerZ;
									cover_verts[cover_num]=new Vector3(terrainPosition.x+idx_i*cellW, 0, terrainPosition.z+idx_j*cellH);
									RaycastHit hit;
									if (terrainCollider.Raycast(new Ray(new Vector3(cover_verts[cover_num].x, _target.blendedObject.GetComponent<Renderer>().bounds.max.y, cover_verts[cover_num].z), paintSurfaceNormal), out hit, Mathf.Infinity)) {
										if (GlobalHNMap!=null) {
											// tessellated solution based on height&normal texture
											Vector3 ground_norm=Vector3.zero; // not used
											float height=0;
											_target.GetTessHeightAndNorm(tess_height, GlobalHNMap, bicubic_flag, ref height, ref ground_norm, hit.textureCoord.x, hit.textureCoord.y);
											height+=_target.blendedObject.transform.position.y+_TessYOffset;
											cover_verts[cover_num].y=height+0.03f;
										} else {
											cover_verts[cover_num].y=hit.point.y+0.03f;
										}
									}
									cover_num++;
								}
							}
						}
					}
				} else {
					// remove tris
					if (tris!=null) {
						Vector3 pnt;
						if (_target.Sticked) {
							pnt=GetWorldPointFromMouse(_target.blendedObject.GetComponent<Collider>());
						} else {
							pnt=GetWorldPointFromMouse(_target.GetComponent<Collider>());
						}
						cover_num=0;
						for(int i=0; i<tris.Length; i+=3) {
							Vector3 wPos=T_lw(0.3333333f*(vertices[tris[i]]+vertices[tris[i+1]]+vertices[tris[i+2]]));
							float dist=Vector3.Distance(pnt, wPos);
							if ((cover_num<cover_tris.Length) && (dist<_target.paint_size)) {
								cover_tris[cover_num]=i;
								cover_num++;
							}
						}
					}
				}
			} else {
				if (vertices!=null) {
					Vector3 pnt;
					if (_target.Sticked) {
						pnt=GetWorldPointFromMouse(_target.blendedObject.GetComponent<Collider>());
					} else {
						pnt=GetWorldPointFromMouse(_target.GetComponent<Collider>());
					}
					cover_num=0;
					for(int i=0; i<vertices.Length; i++) {
						Vector3 wPos=T_lw(vertices[i]);
						float dist=Vector3.Distance(pnt, wPos);
						if ((cover_num<cover_verts.Length) && (dist<_target.paint_size) && (Vector3.Dot(gui_ray_dir, normals[i])<=0)) {
							cover_verts[cover_num]=wPos;
							cover_strength[cover_num]=(_target.paint_size-dist*_target.paint_smoothness)/_target.paint_size;
							cover_indices[cover_num]=i;
							cover_num++;
						}
					}
				}
			}
			paintHitInfo_flag=false;
		} else {
			cover_num=0;
	        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			if (_target.blendedObject && _target.blendedObject.GetComponent<Collider>()) {
				paintHitInfo_flag=_target.blendedObject.GetComponent<Collider>().Raycast(ray, out paintHitInfo, Mathf.Infinity);
				_target.paintHitInfo=paintHitInfo;
				_target.paintHitInfo_flag=paintHitInfo_flag;
			}
		}
		EditorUtility.SetDirty(target);
	}
	
    private static Vector3 GetWorldPointFromMouse(Collider obj_collider) {
		float planeLevel = 0;
        var groundPlane = new Plane(Vector3.up, new Vector3(0, planeLevel, 0));

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Vector3 hit = new Vector3(0,0,0);
        float dist;
		
		RaycastHit rayHit;
        if (obj_collider.Raycast(ray, out rayHit, Mathf.Infinity)) {
			return rayHit.point;
		} else if (groundPlane.Raycast(ray, out dist)) {
            hit = ray.origin + ray.direction.normalized * dist;
		}
        return hit;
    }

	static Vector3 T_lw(Vector3 input) {
		GeometryVsTerrainBlend _target = GeometryVsTerrainBlend.cur_EditorTarget;
		return _target.transform.TransformPoint(input);
	}
    static Vector3 T_wl(Vector3 input) {
		GeometryVsTerrainBlend _target = GeometryVsTerrainBlend.cur_EditorTarget;
		return _target.transform.InverseTransformPoint(input);
	}	
	
	static void RegisterUndoForMeshes(GeometryVsTerrainBlend _target=null) {
		if (_target==null) _target= GeometryVsTerrainBlend.cur_EditorTarget;		
		UnityEngine.Object[] objs=new UnityEngine.Object[4];
		objs[0]=(_target.gameObject.GetComponent (typeof(MeshFilter)) as MeshFilter);
		objs[1]=(_target.gameObject.GetComponent (typeof(Collider)) as Collider);
		if (objs[1]==null) objs[1]=_target; // no collider would crash U4.3
		if (_target.GetUnderlyingGameObject()) {
			objs[2]=(_target.GetUnderlyingGameObject().GetComponent (typeof(MeshFilter)) as MeshFilter);
		} else {
			// voxel sticked nie ma underlying
			objs[2]=_target;
		}
		objs[3]=_target;
		Undo.RecordObjects( objs, "Geometry Blend Edit");
		string nam=(_target.gameObject.GetComponent (typeof(MeshFilter)) as MeshFilter).sharedMesh.name;
		_target.pmesh=(_target.gameObject.GetComponent (typeof(MeshFilter)) as MeshFilter).sharedMesh=Instantiate((_target.gameObject.GetComponent (typeof(MeshFilter)) as MeshFilter).sharedMesh) as Mesh;
		_target.pmesh.name=nam;
	}

	private static void RemoveUnusedVerts(MeshFilter mf) {
		if (!mf) return;
		Mesh sh=mf.sharedMesh;
		Vector3[] vertices=sh.vertices;
		Vector3[] normals=sh.normals;
		Vector4[] tangents=sh.tangents;
		Color[] colors=sh.colors;
		Vector2[] uv=sh.uv;
		Vector2[] uv2=sh.uv2;
		int[] tris=sh.triangles;
		int[] tris_mod=sh.triangles;
		
		ArrayList new_vertices=null;
		if (vertices!=null && vertices.Length>0) new_vertices=new ArrayList();
		ArrayList new_normals=null;
		if (normals!=null && normals.Length>0) new_normals=new ArrayList();
		ArrayList new_tangents=null;
		if (tangents!=null && tangents.Length>0) new_tangents=new ArrayList();
		ArrayList new_colors=null;
		if (colors!=null && colors.Length>0) new_colors=new ArrayList();
		ArrayList new_uv=null;
		if (uv!=null && uv.Length>0) new_uv=new ArrayList();
		ArrayList new_uv2=null;
		if (uv2!=null && uv2.Length>0) new_uv2=new ArrayList();

		for(int i=0; i<vertices.Length; i++) {
			bool unused=true;
			for(int j=0; j<tris.Length; j++) {
				if (tris[j]==i) {
					unused=false;
					break;
				}
			}
			if (unused) {
				for(int j=0; j<tris.Length; j++) {
					if (tris[j]>i)	tris_mod[j]--;
				}
			} else {
				if (new_vertices!=null) new_vertices.Add(vertices[i]);
				if (new_normals!=null) new_normals.Add(normals[i]);
				if (new_tangents!=null) new_tangents.Add(tangents[i]);
				if (new_colors!=null) new_colors.Add(colors[i]);
				if (new_uv!=null) new_uv.Add(uv[i]);
				if (new_uv2!=null) new_uv2.Add(uv2[i]);
			}
		}
		sh.triangles=null;
		if (new_vertices!=null) sh.vertices=(Vector3[])new_vertices.ToArray(typeof(Vector3));
		if (new_normals!=null) sh.normals=(Vector3[])new_normals.ToArray(typeof(Vector3));
		if (new_tangents!=null) sh.tangents=(Vector4[])new_tangents.ToArray(typeof(Vector4));
		if (new_colors!=null) sh.colors=(Color[])new_colors.ToArray(typeof(Color));
		if (new_uv!=null) sh.uv=(Vector2[])new_uv.ToArray(typeof(Vector2));
		if (new_uv2!=null) sh.uv2=(Vector2[])new_uv2.ToArray(typeof(Vector2));
		sh.triangles=tris_mod;
		sh.RecalculateBounds();
		mf.sharedMesh=sh;
	}
	
	private void CheckShaderForBlendCapability(GeometryVsTerrainBlend _target=null) {
		if (_target==null) _target=(GeometryVsTerrainBlend)target;
		
		_target.shader_global_blend_capabilities=false;
		if (_target.GetComponent<Renderer>().sharedMaterial) {
			Shader shad=_target.GetComponent<Renderer>().sharedMaterial.shader;
			string path=AssetDatabase.GetAssetPath(shad);
			if (path!="" && (path.IndexOf("builtin")<0)) {
				string _code = System.IO.File.ReadAllText(path);
				if (_code.IndexOf("_TERRAIN_PosSize")>0) {
					_target.shader_global_blend_capabilities=true;
					_target.SyncMaterialProps();
				}
			}
		}
	}
	
#endif
}
