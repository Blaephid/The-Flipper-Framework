using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class RTP_LabelWidthDecorator : MaterialPropertyDrawer {
    float nextLabelWidth;

	public RTP_LabelWidthDecorator(float nextLabelWidth) :base() {
        this.nextLabelWidth = nextLabelWidth;
    }

    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("OnGUI: " + label + " RTP_HeaderDecorator");

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;

            if (customEditor.showFlag)
            {
                customEditor.nextLabelWidth = nextLabelWidth;
            }
        }
    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 0;
    }
}
