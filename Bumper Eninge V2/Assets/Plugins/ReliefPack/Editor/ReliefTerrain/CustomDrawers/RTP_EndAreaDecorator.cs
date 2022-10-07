using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class RTP_EndAreaDecorator : MaterialPropertyDrawer
{
    private bool indent;

    public RTP_EndAreaDecorator() : base()
    {
        this.indent = false;
    }
    public RTP_EndAreaDecorator(string indent) : base()
    {
        this.indent = indent=="true";
    }

    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("OnGUI: " + label + " RTP_EndAreaDecorator");
    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("GetHeight: " + label + " RTP_EndAreaDecorator");
        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;
            if (customEditor.helperFlag)
            {
                customEditor.helperFlag = false;
                return 0;
            }
            customEditor.helperFlag = true;
            if (customEditor.showFlag)
            {
                EditorGUILayout.EndVertical();
                if (indent)
                {
                    // EditorGUI.indentLevel--;

                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        return 0;
    }
}
