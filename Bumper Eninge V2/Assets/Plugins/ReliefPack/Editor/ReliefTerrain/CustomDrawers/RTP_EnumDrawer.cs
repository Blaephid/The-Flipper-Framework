using UnityEditor;
using UnityEngine;


public class RTP_Enum : MaterialPropertyDrawer {
    string[] props;

    bool parsed = false;
    string parsedLabel;

    public RTP_Enum(string prop1) : base()
    {
        props = new string[] { prop1 };
    }
    public RTP_Enum(string prop1, string prop2) : base()
    {
        props = new string[] { prop1, prop2 };
    }
    public RTP_Enum(string prop1, string prop2, string prop3) : base()
    {
        props = new string[] { prop1, prop2, prop3};
    }
    public RTP_Enum(string prop1, string prop2, string prop3, string prop4) : base()
    {
        props = new string[] { prop1, prop2, prop3, prop4 };
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

            if (customEditor.showFlag) {

                EditorGUI.BeginDisabledGroup(customEditor.inactiveFlag);
                                
                EditorGUIUtility.labelWidth = 300;
                EditorGUI.BeginChangeCheck();
                    float pval = prop.floatValue;
                    float nval = EditorGUI.Popup(position, label, (int)pval, props);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.floatValue = nval;
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
            if (customEditor.showFlag)
            {
                return 20;
            }
            return -2;
        }
        return MaterialEditor.GetDefaultPropertyHeight(prop);
    }



}
