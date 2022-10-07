using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class RTP_BeginAreaDrawer : MaterialPropertyDrawer {
    string type;
    bool indent;
    int indentVal=20;

    public RTP_BeginAreaDrawer(string type, string indent) {
        this.type = type;
        this.indent = indent=="true";
    }
    public RTP_BeginAreaDrawer(string type, string indent, float indentVal)
    {
        this.type = type;
        this.indent = indent == "true";
        this.indentVal = (int)indentVal;
    }

    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("OnGUI: " + label + " RTP_BeginAreaDecorator");
        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;
            //if (label == "Triplanar tile size")
            //{
            //    Debug.Log("E"+ customEditor.test);
            //    customEditor.test++;
            //}
            //if (!customEditor.helperFlag)
            //{
            //    customEditor.helperFlag = true;
            //    return 0;
            //}
            //customEditor.helperFlag = false;
            if (customEditor.showFlag)
            {
                if (indent)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(indentVal);
                }
                if (type == "none")
                {
                    EditorGUILayout.BeginVertical();
                }
                else
                {
                    EditorGUILayout.BeginVertical(type);
                }
            }
        }

    }

    // called before OnGUI and after, that's why we need helperFlag
    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("GetHeight: " + label + " RTP_BeginAreaDecorator");
        return 0;
    }
}
