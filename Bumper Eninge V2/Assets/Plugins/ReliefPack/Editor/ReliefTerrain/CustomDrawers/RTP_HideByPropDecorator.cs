using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class RTP_HideByPropDecorator : MaterialPropertyDrawer
{
    private string visibilityProp1;
    private float[] visibilityPropVals1 = new float[1] { 1 };
    private string visibilityProp2;
    private float[] visibilityPropVals2 = new float[1] { 1 };
    private bool inactiveFlag1 = false;
    private bool inactiveFlag2 = false;
    private bool BoolOrFlag = false;
    private bool clearFlag = false;
    private bool invertFlag = false;

    public RTP_HideByPropDecorator() : base()
    {
        clearFlag = true;
    }
    public RTP_HideByPropDecorator(string prop1) : base()
    {
        if (prop1.IndexOf("not_")>=0)
        {
            invertFlag = true;
            prop1 = prop1.Substring(4);
        }
        if (inactiveFlag1 = (prop1.IndexOf("INACTIVE_") == 0)) prop1 = prop1.Substring(9);
        ProcessVisibilityProp1(prop1);
    }
    public RTP_HideByPropDecorator(string prop1, string prop2) : base()
    {
        if (prop1.IndexOf("not_") >= 0)
        {
            invertFlag = true;
            prop1 = prop1.Substring(4);
        }
        if (inactiveFlag1 = (prop1.IndexOf("INACTIVE_") == 0)) prop1 = prop1.Substring(9);
        if (prop2.IndexOf("INACTIVE_") == 0)
        {
            inactiveFlag2 = true;
            prop2 = prop2.Substring(9);
        }
        if (prop2.IndexOf("LOGICOR_") == 0)
        {
            BoolOrFlag = true;
            prop2 = prop2.Substring(8);
        }
        ProcessVisibilityProp1(prop1);
        ProcessVisibilityProp2(prop2);
    }

    private void ProcessVisibilityProp1(string prop1)
    {
        string[] tmp = prop1.Split('.');
        visibilityProp1 = tmp[0];
        if (tmp.Length == 1)
        {
            visibilityPropVals1 = new float[1] { 1 };
        }
        else
        {
            visibilityPropVals1 = new float[tmp.Length - 1];
        }
        for (int i = 0; i < tmp.Length - 1; i++)
        {
            visibilityPropVals1[i] = float.Parse(tmp[i + 1]);
        }
    }
    private void ProcessVisibilityProp2(string prop2)
    {
        string[] tmp = prop2.Split('.');
        visibilityProp2 = tmp[0];
        if (tmp.Length == 1)
        {
            visibilityPropVals2 = new float[1] { 1 };
        }
        else
        {
            visibilityPropVals2 = new float[tmp.Length - 1];
        }
        for (int i = 0; i < tmp.Length - 1; i++)
        {
            visibilityPropVals2[i] = float.Parse(tmp[i + 1]);
        }
    }

    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("OnGUI: " + label + " RTP_HideByPropDecorator");
    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("GetHeight: " + label + " RTP_HideByPropDecorator");

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;
            if (clearFlag)
            {
                customEditor.showFlag = true;
                customEditor.inactiveFlag = false;
            }
            else
            {
                customEditor.showFlag = checkVisible(editor, ref customEditor.inactiveFlag);
            }

        }
        return 0;
    }

    protected bool checkVisible(MaterialEditor editor, ref bool inactiveFlagOut)
    {
        bool visible = false;

        // 1st level
        if (visibilityProp1 != "")
        {
            Material mat = editor.target as Material;
            string firstChar = visibilityProp1.Substring(0, 1);
            if ((firstChar.ToUpper() == firstChar) && (editor is RTP_CustomShaderGUI))
            {
                RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;
                if (customEditor.shaderCode != null)
                {
                    visible = customEditor.CheckDefine(visibilityProp1, invertFlag);
                    if (!visible && inactiveFlag1) inactiveFlagOut = true;
                    visible = visible || inactiveFlag1;
                }
            }
            else
            {
                if (mat.HasProperty(visibilityProp1))
                {
                    float val = mat.GetFloat(visibilityProp1);
                    for (int i = 0; i < visibilityPropVals1.Length; i++)
                    {
                        visible = visible || (val == visibilityPropVals1[i]);
                    }
                    if (!visible && inactiveFlag1) inactiveFlagOut = true;
                    visible = visible || inactiveFlag1;
                }
            }
        }
        if (!BoolOrFlag && !visible)
        {
            return false;
        }

        // 2nd nested level
        if (visibilityProp2 != "")
        {
            Material mat = editor.target as Material;
            if (mat.HasProperty(visibilityProp2))
            {
                visible = false;
                float val = mat.GetFloat(visibilityProp2);
                for (int i = 0; i < visibilityPropVals2.Length; i++)
                {
                    visible = visible || (val == visibilityPropVals2[i]);
                }
                if (!visible && inactiveFlag2) inactiveFlagOut = true;
                visible = visible || inactiveFlag2;
            }
        }

        return visible;
    }
}
