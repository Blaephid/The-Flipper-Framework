using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class RTP_LayerSelector : MaterialPropertyDrawer {
    int numLayers;
    string[] thumbPropNames;
    MaterialProperty[] thumbs;

    public RTP_LayerSelector(string prop1, string prop2) : base()
    {
        numLayers = 2;
        thumbs = new MaterialProperty[numLayers];
        thumbPropNames = new string[numLayers];
        thumbPropNames[0] = prop1;
        thumbPropNames[1] = prop2;
    }
    public RTP_LayerSelector(string prop1, string prop2, string prop3) : base()
    {
        numLayers = 3;
        thumbs = new MaterialProperty[numLayers];
        thumbPropNames = new string[numLayers];
        thumbPropNames[0] = prop1;
        thumbPropNames[1] = prop2;
        thumbPropNames[2] = prop3;
    }
    public RTP_LayerSelector(string prop1, string prop2, string prop3, string prop4) : base()
    {
        numLayers = 4;
        thumbs = new MaterialProperty[numLayers];
        thumbPropNames = new string[numLayers];
        thumbPropNames[0] = prop1;
        thumbPropNames[1] = prop2;
        thumbPropNames[2] = prop3;
        thumbPropNames[3] = prop4;
    }

    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
        //Debug.Log("OnGUI: " + label + " RTP_MaterialProp");

        for(int i=0; i< numLayers; i++)
        {
            thumbs[i] = MaterialEditor.GetMaterialProperty(editor.targets, thumbPropNames[i]);
        }

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;

            if (customEditor.showFlag) {

                EditorGUI.BeginDisabledGroup(customEditor.inactiveFlag);

                EditorGUILayout.BeginVertical("Box");
                Color skin_color = GUI.color;
                GUI.color = new Color(1, 1, 0.5f, 1);
                EditorGUILayout.LabelField("Choose layer", EditorStyles.boldLabel);
                GUI.color = skin_color;
                //EditorGUILayout.HelpBox("Hint: to quickly select a layer - focus on scene view (by click) and press L holding cursor over the layer that should be selected.", MessageType.Info, true);

                GUISkin gs = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
                RectOffset ro1 = gs.label.padding;
                RectOffset ro2 = gs.label.margin;
                gs.label.padding = new RectOffset(0, 0, 0, 0);
                gs.label.margin = new RectOffset(3, 3, 3, 3);
                int thumb_size = 50;
                int per_row = Mathf.Max(4, (Screen.width) / thumb_size - 1);
                thumb_size = (Screen.width - thumb_size - 2 * per_row) / per_row;
                Color ccol = GUI.contentColor;
                for (int n = 0; n < numLayers; n++)
                {
                    if ((n % per_row) == 0) EditorGUILayout.BeginHorizontal();
                    Color bcol = GUI.backgroundColor;
                    if (n == prop.floatValue)
                    {
                        GUI.contentColor = new Color(1, 1, 1, 1);
                        GUI.backgroundColor = new Color(1, 1, 0, 1);
                        EditorGUILayout.BeginHorizontal("Box");
                        if (thumbs[n].textureValue)
                        {
                            GUILayout.Label((Texture2D)AssetPreview.GetAssetPreview(thumbs[n].textureValue), GUILayout.Width(thumb_size - 8), GUILayout.Height(thumb_size - 8));
                        }
                        else
                        {
                            GUILayout.Label(customEditor.blankGreyTex, GUILayout.Width(thumb_size - 8), GUILayout.Height(thumb_size - 8));
                        }
                    }
                    else
                    {
                        GUI.contentColor = new Color(1, 1, 1, 0.5f);
                        if (thumbs[n].textureValue)
                        {
                            if (GUILayout.Button((Texture2D)AssetPreview.GetAssetPreview(thumbs[n].textureValue), "Label", GUILayout.Width(thumb_size), GUILayout.Height(thumb_size)))
                            {
                                GUI.FocusControl("");
                                prop.floatValue = n;
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(customEditor.blankGreyTex, "Label", GUILayout.Width(thumb_size), GUILayout.Height(thumb_size)))
                            {
                                GUI.FocusControl("");
                                prop.floatValue = n;
                            }
                        }
                    }
                    if (n == prop.floatValue)
                    {
                        EditorGUILayout.EndHorizontal();
                        GUI.backgroundColor = bcol;
                    }
                    if ((n % per_row) == (per_row - 1) || n == numLayers - 1) EditorGUILayout.EndHorizontal();
                }
                GUI.contentColor = ccol;
                gs.label.padding = ro1;
                gs.label.margin = ro2;
                EditorGUILayout.EndVertical();

                EditorGUI.EndDisabledGroup();
		    }
        }

    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {
        //Debug.Log("GetHeight: " + label+ " RTP_MaterialProp" );

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;
            if (customEditor.showFlag)
            {
                return 0;
            }
            return -2;
        }
        return 0;
    }



}
