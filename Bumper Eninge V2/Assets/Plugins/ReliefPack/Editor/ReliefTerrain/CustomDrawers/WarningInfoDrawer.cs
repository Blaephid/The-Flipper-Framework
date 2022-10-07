using UnityEditor;
using UnityEngine;
using System.Collections;


public class RTP_WarningInfoDrawer : MaterialPropertyDrawer {
	private string visibilityProp1;
	private float[] visibilityPropVals1=new float[1]{1};
	private string visibilityProp2;
	private float[] visibilityPropVals2=new float[1]{1};
	private bool inactiveFlag1 = false;
	private bool inactiveFlag2 = false;
	private bool BoolOrFlag = false;
		
	public RTP_WarningInfoDrawer(string prop1):base() {
		if (inactiveFlag1 = (prop1.IndexOf("INACTIVE")==0)) prop1=prop1.Substring(8);
		ProcessVisibilityProp1(prop1);
	}
	public RTP_WarningInfoDrawer(string prop1, string prop2):base() {
		if (inactiveFlag1 = (prop1.IndexOf("INACTIVE")==0)) prop1=prop1.Substring(8);
		if (prop2.IndexOf("INACTIVE")==0) {
			inactiveFlag2=true;
			prop2=prop2.Substring(8);
		}
		if (prop2.IndexOf("LOGICOR")==0) {
			BoolOrFlag=true;
			prop2=prop2.Substring(7);
		}
		//Debug.Log (prop1 + "  " + prop2 + "  " + inactiveFlag1 + "  " + inactiveFlag2);
		ProcessVisibilityProp1(prop1);
		ProcessVisibilityProp2(prop2);
	}

	private void ProcessVisibilityProp1(string prop1) {
		string[] tmp = prop1.Split('.');
		visibilityProp1 = tmp[0];
		if (tmp.Length==1) {
			visibilityPropVals1 = new float[1]{1};
		} else {
			visibilityPropVals1 = new float[tmp.Length-1];
		}
		for(int i=0; i<tmp.Length-1; i++) {
			visibilityPropVals1[i]=float.Parse(tmp[i+1]);
		}
	}
	private void ProcessVisibilityProp2(string prop2) {
		string[] tmp = prop2.Split('.');
		visibilityProp2 = tmp[0];
		if (tmp.Length==1) {
			visibilityPropVals2 = new float[1]{1};
		} else {
			visibilityPropVals2 = new float[tmp.Length-1];
		}
		for(int i=0; i<tmp.Length-1; i++) {
			visibilityPropVals2[i]=float.Parse(tmp[i+1]);
		}
	}
		
	override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
		bool inactiveFlag = false;
		//if (!checkNormalmapsUsage(editor.target as Material, label)) return;
		//if (!checkIBLUsage(editor.target as Material, label)) return;

		if (checkVisible (editor, ref inactiveFlag)) {
			EditorGUI.BeginDisabledGroup(inactiveFlag);
			position.x+=12;
			position.width-=12;
			EditorGUI.HelpBox(position, label, MessageType.Warning);
			EditorGUI.EndDisabledGroup();
		}
	}
		
	//bool checkNormalmapsUsage(Material mat, string label) {
	//	if (label.ToLower().IndexOf("normalmap")<0) return true;
	//	if (!mat.HasProperty("_Normalmaps")) return true;
	//	if (mat.GetFloat("_Normalmaps")==0) return false;
	//	return true;
	//}
		
	//bool checkIBLUsage(Material mat, string label) {
	//	if (label.IndexOf("Diffuse Ambient Lighting")<0) return true;
	//	if (!mat.HasProperty("_IBLDiff")) return true;
	//	if (mat.GetFloat("_IBLDiff")!=1) return false;
	//	return true;
	//}


	override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {
		bool inactiveFlag=false;
		//if (!checkNormalmapsUsage(editor.target as Material, label)) return -2;
		//if (!checkIBLUsage(editor.target as Material, label)) return -2;
		if (checkVisible(editor, ref inactiveFlag)) {
			return 30;//MaterialEditor.GetDefaultPropertyHeight(prop);
		}
		return -2;
	}
		
	bool checkVisible(MaterialEditor editor, ref bool inactiveFlagOut) {
		bool visible = false;
			
		// 1st level
		if (visibilityProp1 != "") {
			Material mat=editor.target as Material;
			if (mat.HasProperty(visibilityProp1)) {
				float val = mat.GetFloat(visibilityProp1);
				//Debug.Log (visibilityProp1+" "+visibilityProp2+"  "+val+"   "+visibilityPropVals1[0]); 
				for(int i=0; i<visibilityPropVals1.Length; i++) {
					visible = visible || (val==visibilityPropVals1[i]);
				}
			}
			if (!visible && inactiveFlag1) inactiveFlagOut=true;
			visible = visible || inactiveFlag1; 
		}
		if (BoolOrFlag) {
			if (visible) return true;
		} else {
			if (!visible) return false;
		}
			
		// 2nd nested level
		if (visibilityProp2 != "") {
			Material mat=editor.target as Material;
			if (mat.HasProperty(visibilityProp2)) {
				visible=false;
				float val = mat.GetFloat(visibilityProp2);
				for(int i=0; i<visibilityPropVals2.Length; i++) {
					visible = visible || (val==visibilityPropVals2[i]);
				}
				if (!visible && inactiveFlag2) inactiveFlagOut=true;
				visible = visible || inactiveFlag2;
			}
		}
			
		return visible;
	}
}
