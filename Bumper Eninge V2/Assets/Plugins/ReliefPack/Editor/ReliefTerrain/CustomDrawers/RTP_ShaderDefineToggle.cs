using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class RTP_ShaderDefineToggle : MaterialPropertyDrawer {
    string toolTip="";
    string[] dependentFeatures;

    public RTP_ShaderDefineToggle() : base()
    {
    }
    public RTP_ShaderDefineToggle(string prop1) : base()
    {
        this.toolTip = prop1;
    }
    public RTP_ShaderDefineToggle(string prop1, string prop2) : base()
    {
        this.toolTip = prop1;
        dependentFeatures = new string[] { prop2 };
    }
    public RTP_ShaderDefineToggle(string prop1, string prop2, string prop3) : base()
    {
        this.toolTip = prop1;
        dependentFeatures = new string[] { prop2, prop3 };
    }
    public RTP_ShaderDefineToggle(string prop1, string prop2, string prop3, string prop4) : base()
    {
        this.toolTip = prop1;
        dependentFeatures = new string[] { prop2, prop3, prop4 };
    }

    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
        //Debug.Log("OnGUI: " + label + " RTP_MaterialProp");

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;

            if (customEditor.showFlag) {
                bool featureActive = customEditor.CheckDefine(prop.name, false);
                EditorGUI.BeginDisabledGroup(customEditor.inactiveFlag);
                EditorGUI.BeginChangeCheck();
                bool feature_doesntFit = (featureActive != prop.floatValue > 0);
                Color col = GUI.color;
                if (feature_doesntFit)
                {
                    GUI.color = new Color(1,0.6f,0.6f,1);
                }
                float nval=EditorGUILayout.ToggleLeft(new GUIContent(label, toolTip), prop.floatValue>0 ? true:false, feature_doesntFit ? EditorStyles.boldLabel : EditorStyles.label) ? 1:0;
                GUI.color = col;
                if (EditorGUI.EndChangeCheck())
                {
                    prop.floatValue = nval;
                    if (nval==0 && dependentFeatures!=null)
                    {
                        for(int i=0; i<dependentFeatures.Length; i++)
                        {
                            if ((editor.target as Material).HasProperty(dependentFeatures[i]))
                            {
                                foreach(Material mat in editor.targets)
                                {
                                    mat.SetFloat(dependentFeatures[i], 0);
                                }
                            }
                        }
                    }
                }

			    EditorGUI.EndDisabledGroup();
		    }
        }

    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {
        //Debug.Log("GetHeight: " + label+ " RTP_MaterialProp" );
        return 0;
        //if (editor is RTP_CustomShaderGUI)
        //{
        //    RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;
        //    if (customEditor.showFlag)
        //    {
        //        return MaterialEditor.GetDefaultPropertyHeight(prop);
        //    }
        //    return -2;
        //}
        //return MaterialEditor.GetDefaultPropertyHeight(prop);
    }


}
