using UnityEditor;
using UnityEngine;
using System.Collections;


public class RTP_PropBlockKeywordDrawer : MaterialPropertyDrawer {
	private string myKeyword="";
	private string myMode="";
	private string visibilityProp="";
	private MaterialEditor myEditor=null;

	public RTP_PropBlockKeywordDrawer(string keyword):base() {
		myKeyword = keyword;
	}
	public RTP_PropBlockKeywordDrawer(string keyword, string mode):base() {
		myKeyword = keyword;
		myMode = mode;
	}
	public RTP_PropBlockKeywordDrawer(string keyword, string mode, string visibilityProp):base() {
		myKeyword = keyword;
		myMode = mode;
		this.visibilityProp = visibilityProp;
	}
	public RTP_PropBlockKeywordDrawer():base() {
			
	}

	override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
		if (!checkVisible(editor)) return;
		myEditor = editor;
		float nval;
		//label += "#" + myKeyword + "#";
		if (myMode=="RIGHT") {
			nval = EditorGUI.Toggle(position, label, prop.floatValue==1) ? 1:0;
		} else {
			if (label.Substring(0,1)==" ") {
				position.x+=12;
				position.width-=12;
				nval = EditorGUI.ToggleLeft(position, label.Substring(3), prop.floatValue==1, EditorStyles.boldLabel) ? 1:0;
			} else {
				nval = EditorGUI.ToggleLeft(position, label, prop.floatValue==1, EditorStyles.boldLabel) ? 1:0;
			}
		}
		if (nval!=prop.floatValue) {
			prop.floatValue = nval;
			if (myKeyword!="") Apply(prop);
		}
	}
	override public void Apply(MaterialProperty prop) {
		if (myEditor!=null) {
			Material mat=myEditor.target as Material;
			if (prop.floatValue==0) {
				mat.DisableKeyword(myKeyword+"_ON");
				mat.EnableKeyword(myKeyword+"_OFF");
			} else {
				mat.DisableKeyword(myKeyword+"_OFF");
				mat.EnableKeyword(myKeyword+"_ON");
			}
		}

	}
	override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {
		if (checkVisible(editor)) {
			return MaterialEditor.GetDefaultPropertyHeight(prop);
		}
		return -2;
	}
	bool checkVisible(MaterialEditor editor) {
		if (visibilityProp != "") {
			Material mat=editor.target as Material;
			if (mat.HasProperty(visibilityProp)) {
				return mat.GetFloat(visibilityProp)==1 ? true:false;
			}
		}
		return true;
	}
}
