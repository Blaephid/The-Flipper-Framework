using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor (typeof(SelectorHelperClass))]
public class SelectorHelperClassEditor : Editor {
#if UNITY_EDITOR	
//	void OnEnable() {
//		SelectorHelperClass _target=(SelectorHelperClass)target;
//		
//		GeometryVsTerrainBlend script=(GeometryVsTerrainBlend)_target.transform.parent.GetComponent(typeof(GeometryVsTerrainBlend));
//		if (script && script.dont_select_aux_object) {
//			if (Selection.activeGameObject==_target.gameObject) {
//				Selection.activeGameObject=_target.transform.parent.gameObject;
//			}
//		}		
//	}
#endif
}