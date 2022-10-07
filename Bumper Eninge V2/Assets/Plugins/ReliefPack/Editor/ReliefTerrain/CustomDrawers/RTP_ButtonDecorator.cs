using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class RTP_ButtonDecorator : MaterialPropertyDrawer {
    private string buttonLabel;
    private string functionName;

    public RTP_ButtonDecorator(string buttonLabel, string functionName):base() {
        this.buttonLabel = buttonLabel;
        this.functionName = functionName;
    }

    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("OnGUI: " + label + " Button");

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;

            if (customEditor.showFlag)
            {
                EditorGUI.BeginDisabledGroup(customEditor.inactiveFlag);
                Rect rect = position;
                if (GUI.Button(rect, buttonLabel))
                {
                    Type thisType = customEditor.GetType();
                    MethodInfo theMethod = thisType.GetMethod(functionName);
                    theMethod.Invoke(customEditor, new System.Object[] { });//, userParameters);
                }
                EditorGUI.EndDisabledGroup();
            }
        }
    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        //Debug.Log("GetHeight: " + label + " Button");

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;
            if (customEditor.showFlag)
            {
                return 30;
            }
            return -2;
        }
        return 0;
    }
}
