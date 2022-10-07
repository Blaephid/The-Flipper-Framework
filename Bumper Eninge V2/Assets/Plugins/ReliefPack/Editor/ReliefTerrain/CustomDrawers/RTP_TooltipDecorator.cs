using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class RTP_TooltipDecorator : MaterialPropertyDrawer {
    string toolTip;

	public RTP_TooltipDecorator(string toolTip) : base() {
        this.toolTip = RTP_MatPropStringParser.Parse(toolTip);
	}

    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;

            if (customEditor.showFlag)
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.width = Mathf.Min(rect.width, EditorGUIUtility.labelWidth);
                EditorGUI.LabelField(rect, new GUIContent("", toolTip));
            }
        }
    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 0;
    }
}
