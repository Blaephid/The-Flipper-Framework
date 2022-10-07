using UnityEngine;
using System.Collections;
//
// you have to just put this script on object that uses ReliefTerrainVertexBlendTriplanar shader in it's renderer
// or optionally you can specify material that should be updated and proper mip maps used (look at Planet GO in example planet scene)
//
[AddComponentMenu("Relief Terrain/Helpers/MIP solver for standalone shader")]
[ExecuteInEditMode]
public class ReliefTerrainVertexBlendTriplanar : MonoBehaviour {

	public Texture2D tmp_globalColorMap;
	public string save_path_colormap="";
	public GeometryVsTerrainBlend painterInstance;
	public Material material;
	
	public void SetupMIPOffsets() {
		if (GetComponent<Renderer>()==null && material==null) return;
		Material mat;
		if (GetComponent<Renderer>()!=null) {
			mat=GetComponent<Renderer>().sharedMaterial;
		} else {
			mat=material;
		}
		if (mat==null) return;
		
		if (mat.HasProperty("_SplatAtlasA")) {
			SetupTex("_SplatAtlasA", "rtp_mipoffset_color",1,-1);
		}
		if (mat.HasProperty("_SplatA0")) {
			SetupTex("_SplatA0", "rtp_mipoffset_color");
		}
		SetupTex("_BumpMap01", "rtp_mipoffset_bump");
		SetupTex("_TERRAIN_HeightMap", "rtp_mipoffset_height");
		if (mat.HasProperty("_BumpMapGlobal")) {
			SetupTex("_BumpMapGlobal", "rtp_mipoffset_globalnorm", mat.GetFloat("_BumpMapGlobalScale"), mat.GetFloat("rtp_mipoffset_globalnorm_offset"));
			if (mat.HasProperty("_SuperDetailTiling")) {
				SetupTex("_BumpMapGlobal", "rtp_mipoffset_superdetail", mat.GetFloat("_SuperDetailTiling"));
			}
			if (mat.HasProperty("TERRAIN_FlowScale")) {
				SetupTex("_BumpMapGlobal", "rtp_mipoffset_flow", mat.GetFloat("TERRAIN_FlowScale"), mat.GetFloat("TERRAIN_FlowMipOffset"));
			}
		}
		if (mat.HasProperty("TERRAIN_RippleMap")) {
			SetupTex("TERRAIN_RippleMap", "rtp_mipoffset_ripple", mat.GetFloat("TERRAIN_RippleScale"));
		}
		if (mat.HasProperty("TERRAIN_CausticsTex")) {
			SetupTex("TERRAIN_CausticsTex", "rtp_mipoffset_caustics", mat.GetFloat("TERRAIN_CausticsTilingScale"));
		}
	}

	private void SetupTex(string tex_name, string param_name, float _mult=1, float _add=0) {
		if (GetComponent<Renderer>()==null && material==null) return;
		Material mat;
		if (GetComponent<Renderer>()!=null) {
			mat=GetComponent<Renderer>().sharedMaterial;
		} else {
			mat=material;
		}
		if (mat==null) return;
		if (mat.GetTexture(tex_name)!=null) {
			int tex_width=mat.GetTexture(tex_name).width;
			mat.SetFloat(param_name, -Mathf.Log(1024.0f/(tex_width*_mult))/Mathf.Log(2) + _add );
		}
	}
	
	public void SetTopPlanarUVBounds() {
		Vector4 bounds;
		MeshFilter mf=GetComponent(typeof(MeshFilter)) as MeshFilter;
		if (mf==null) return;
		Mesh mesh=mf.sharedMesh;
		Vector3[] vertices=mesh.vertices;
		if (vertices.Length==0) return;
		Vector3 tPoint=transform.TransformPoint(vertices[0]);
		bounds.x=tPoint.x; // min
		bounds.y=tPoint.z;
		bounds.z=tPoint.x; // max
		bounds.w=tPoint.z;
		for(int i=1; i<vertices.Length; i++) {
			tPoint=transform.TransformPoint(vertices[i]);
			if (tPoint.x<bounds.x) bounds.x=tPoint.x;
			if (tPoint.z<bounds.y) bounds.y=tPoint.z;
			if (tPoint.x>bounds.z) bounds.z=tPoint.x;
			if (tPoint.z>bounds.w) bounds.w=tPoint.z;
		}
		bounds.z-=bounds.x;
		bounds.w-=bounds.y;
		
		if (GetComponent<Renderer>()==null && material==null) return;
		Material mat;
		if (GetComponent<Renderer>()!=null) {
			mat=GetComponent<Renderer>().sharedMaterial;
		} else {
			mat=material;
		}
		if (mat==null) return;
		mat.SetVector("_TERRAIN_PosSize", bounds);
	}

	void Awake() {
		SetupMIPOffsets();
		if (Application.isPlaying) {
			enabled=false;
		}		
	}
	void Update () {
		if (!Application.isPlaying) {
			SetupMIPOffsets();
		}
	}
	
}
