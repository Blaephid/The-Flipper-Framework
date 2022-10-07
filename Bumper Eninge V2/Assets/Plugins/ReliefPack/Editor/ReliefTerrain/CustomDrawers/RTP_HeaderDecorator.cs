using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class RTP_HeaderDecorator : MaterialPropertyDrawer {
    string headerLabel;
    bool active_layer_flag;

	public RTP_HeaderDecorator(string headerLabel):base() {
        this.headerLabel = RTP_MatPropStringParser.Parse(headerLabel);
        active_layer_flag = false;
    }
    public RTP_HeaderDecorator(string headerLabel, string layer_flag) : base()
    {
        this.headerLabel = RTP_MatPropStringParser.Parse(headerLabel);
        active_layer_flag = true;
    }

    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("OnGUI: " + label + " RTP_HeaderDecorator");

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;

            if (customEditor.showFlag)
            {
                EditorGUI.BeginDisabledGroup(customEditor.inactiveFlag);
                position.y += 2;
                EditorGUI.LabelField(position, headerLabel + (active_layer_flag ? " " +(customEditor.active_layer+1) : ""), EditorStyles.boldLabel);
                EditorGUI.EndDisabledGroup();
            }
        }
    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("GetHeight: " + label + " RTP_HeaderDecorator");

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;
            if (customEditor.showFlag)
            {
                return 24;
            }
            return -2;
        }
        return 0;
    }
}
