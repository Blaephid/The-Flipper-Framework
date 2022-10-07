using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class RTP_MaterialProp : MaterialPropertyDrawer {
    bool noAlphaFlag = false;
    bool miniThumbFlag = false;
    bool noTileOffsetFlag = false;
    bool hdrFlag = false;
    bool byLayerFlag = false;
    string[] sharedTextures;

    bool parsed = false;
    string parsedLabel;
    float minVal = 0;
    float maxVal = 0;
    int show_for_active_layer = -1;

    public RTP_MaterialProp() : base()
    {
    }
    public RTP_MaterialProp(string prop1) : base()
    {
        string[] props = new string[] { prop1 };
        CheckFlags(props);
    }
    // by layer min/max
    public RTP_MaterialProp(string prop1, float minVal, float maxVal) : base()
    {
        // (we assume prop1 is "layer" - this is only for readibility of shader properties block part)
        string[] props = new string[] { prop1 };
        CheckFlags(props);
        this.minVal = minVal;
        this.maxVal = maxVal;
    }
    public RTP_MaterialProp(string prop1, float minVal, float maxVal, float negOffset) : base()
    {
        // (we assume prop1 is "layer" - this is only for readibility of shader properties block part)
        string[] props = new string[] { prop1 };
        CheckFlags(props);
        this.minVal = minVal - negOffset;
        this.maxVal = maxVal - negOffset;
    }

    public RTP_MaterialProp(string prop1, string prop2) : base()
    {
        string[] props = new string[] { prop1, prop2 };
        CheckFlags(props);
    }
    public RTP_MaterialProp(string prop1, string prop2, string prop3) : base()
    {
        string[] props = new string[] { prop1, prop2, prop3 };
        CheckFlags(props);
    }
    public RTP_MaterialProp(string prop1, string prop2, string prop3, string prop4) : base()
    {
        string[] props = new string[] { prop1, prop2, prop3, prop4 };
        CheckFlags(props);
    }
    public RTP_MaterialProp(string prop1, string prop2, string prop3, string prop4, string prop5) : base()
    {
        string[] props = new string[] { prop1, prop2, prop3, prop4, prop5 };
        CheckFlags(props);
    }

    private void CheckFlags(string[] props)
    {
        for (int i = 0; i < props.Length; i++)
        {
            noAlphaFlag |= (props[i].ToLower() == "noalpha");
            miniThumbFlag |= (props[i].ToLower() == "minithumb");
            noTileOffsetFlag |= (props[i].ToLower() == "notileoffset");
            hdrFlag |= (props[i].ToLower() == "hdr");
            byLayerFlag |= (props[i].ToLower() == "layer");
            if (props[i].Length>=13 && (props[i].ToLower().Substring(0, 12) == "active_layer")) {
                if (!int.TryParse(props[i].ToLower().Substring(12), out show_for_active_layer))
                {
                    show_for_active_layer = -1;
                    Debug.LogWarning("Error parsing active layer for drawer parameter: "+props[i]);
                }
            }
        }
        ArrayList sharedTex = new ArrayList();
        for (int i = 0; i < props.Length; i++)
        {
            if (props[i].Length >= 8 && (props[i].ToLower().Substring(0, 7) == "shared_"))
            {
                sharedTex.Add(props[i].Substring(6));
            }
        }
        if (sharedTex.Count>0)
        {
            sharedTextures = sharedTex.ToArray(typeof(string)) as string[];
        }
    }

    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
        //Debug.Log("OnGUI: " + label + " RTP_MaterialProp");

        if (!parsed)
        {
            parsed = true;
            parsedLabel = RTP_MatPropStringParser.Parse(label);
        }
        label = parsedLabel;

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;

            if (customEditor.showFlag && (show_for_active_layer == -1 || customEditor.active_layer == show_for_active_layer) && prop.name!= "dummy_end") {

                EditorGUI.BeginDisabledGroup(customEditor.inactiveFlag);

                switch (prop.type)
                {
                    //case MaterialProperty.PropType.Range: // float ranges
                    //    {
                    //        editor.RangeProperty(position, prop, label);
                    //        break;
                    //    }
                    //case MaterialProperty.PropType.Float: // floats
                    //    {
                    //        editor.FloatProperty(position, prop, label);
                    //        break;
                    //    }
                    case MaterialProperty.PropType.Color: // colors
                        {
                            EditorGUIUtility.labelWidth -= 30;
                            if (noAlphaFlag)
                            {
#if !UNITY_2018_1_OR_NEWER
                                prop.colorValue = EditorGUI.ColorField(position, new GUIContent(label, ""), prop.colorValue, true, false, false, null);
#else
                                prop.colorValue = EditorGUI.ColorField(position, new GUIContent(label, ""), prop.colorValue, true, false, false);
#endif
                            }
                            else
                            {
                                editor.ColorProperty(position, prop, label);
                            }
                            break;
                        }
                    case MaterialProperty.PropType.Texture: // textures
                        {
                            EditorGUI.BeginChangeCheck();
                            if (miniThumbFlag)
                            {
                                editor.TexturePropertyMiniThumbnail(position, prop, label,"");
                            } else
                            {
                                editor.TextureProperty(position, prop, label, !noTileOffsetFlag);
                            }
                            if (EditorGUI.EndChangeCheck() && prop.textureValue!=null && sharedTextures!=null)
                            {
                                for(int j=0; j<sharedTextures.Length; j++)
                                {
                                    foreach(Material mat in editor.targets)
                                    {
                                        if (mat.HasProperty(sharedTextures[j]))
                                        {
                                            mat.SetTexture(sharedTextures[j], prop.textureValue);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case MaterialProperty.PropType.Vector: // vectors
                        {
                            if (byLayerFlag)
                            {
                                //
                                // affect single vector component depending on active layer
                                //
                                int layerNum = customEditor.active_layer;
                                float pval = prop.vectorValue[layerNum];
                                float nval;
                                if (minVal == maxVal)
                                {
                                    // float
                                    EditorGUIUtility.labelWidth -= 23;
                                    nval = EditorGUI.FloatField(position, label, pval);
                                }
                                else
                                {
                                    // slider
                                    EditorGUIUtility.labelWidth = 160;
                                    nval = EditorGUI.Slider(position, label, pval, minVal, maxVal);
                                }
                                if (pval!=nval)
                                {
                                    for(int i=0; i< prop.targets.Length; i++)
                                    {
                                        Material mat = (prop.targets[i] as Material);
                                        Vector4 vec = mat.GetVector(prop.name);
                                        vec[layerNum] = nval;
                                        mat.SetVector(prop.name, vec);
                                    }
                                }
                            }
                            else
                            {
                                position.x += 12;
                                position.width -= 12;
                                editor.VectorProperty(position, prop, label);
                            }
                            break;
                        }
                    default:
                        {
                            if (customEditor.nextLabelWidth>0)
                            {
                                EditorGUIUtility.labelWidth = customEditor.nextLabelWidth;
                                customEditor.nextLabelWidth = 0;
                            } else
                            {
                                EditorGUIUtility.labelWidth -= 30;
                            }
                            editor.DefaultShaderProperty(position, prop, label);
                            break;
                        }
                }

			    EditorGUI.EndDisabledGroup();
		    }
        }

    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) {
        //Debug.Log("GetHeight: " + label+ " RTP_MaterialProp" );

        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;
            if ( customEditor.showFlag && (show_for_active_layer==-1 || customEditor.active_layer==show_for_active_layer) && prop.name!="dummy_end")
            {
                if (miniThumbFlag)
                {
                    return 20;
                } else
                {
                    if ( (prop.type == MaterialProperty.PropType.Vector) && byLayerFlag)
                    {
                        return 17;
                    } else
                    {
                        return MaterialEditor.GetDefaultPropertyHeight(prop);
                    }
                }
            }
            return -2;
        }
        return MaterialEditor.GetDefaultPropertyHeight(prop);
    }



}
