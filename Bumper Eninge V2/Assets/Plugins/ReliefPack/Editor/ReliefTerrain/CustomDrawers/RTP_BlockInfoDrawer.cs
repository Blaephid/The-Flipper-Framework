using UnityEditor;
using UnityEngine;
using System.Collections;


public class RTP_BlockInfoDrawer : MaterialPropertyDrawer {
	Color backgroundColor;
	bool foldoutFlag;
    bool parsed = false;
    string parsedLabel;

    public RTP_BlockInfoDrawer():base() {
		backgroundColor = Color.white;
		foldoutFlag = false;
	}
	public RTP_BlockInfoDrawer(float R, float G, float B, float A):base() {
		backgroundColor = new Color(R, G, B, A);
		foldoutFlag = false;
	}
	public RTP_BlockInfoDrawer(float R, float G, float B, float A, float foldout):base() {
		backgroundColor = new Color(R, G, B, A);
		foldoutFlag = foldout==1;
	}

	override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;

            if (!customEditor.showFlag) return;

            if (!parsed)
            {
                parsed = true;
                parsedLabel = RTP_MatPropStringParser.Parse(label);
            }
            label = parsedLabel;

            Color col = GUI.contentColor;
            Color bcol = GUI.backgroundColor;
            GUI.contentColor = new Color(1f, 1f, 0.8f, 1f);
            GUI.backgroundColor = backgroundColor;

            Rect pos = new Rect(position);
            pos.y += 3;
            pos.height -= 3;

            //if (visibilityProp1 != null)
            //{
            //    pos.x += 12;
            //    pos.width -= 12;
            //}

            EditorGUI.HelpBox(pos, (foldoutFlag ? "     " : "") + label, MessageType.None);

            if (foldoutFlag)
            {
                Rect fpos = new Rect(pos);
                fpos.x += 15;
                fpos.y += 1;
                bool state = EditorGUI.Foldout(fpos, prop.floatValue == 1, "", true);
                prop.floatValue = state ? 1 : 0;
            }

            GUI.contentColor = col;
            GUI.backgroundColor = bcol;
        }
    }

    override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        if (editor is RTP_CustomShaderGUI)
        {
            RTP_CustomShaderGUI customEditor = editor as RTP_CustomShaderGUI;
            if (customEditor.showFlag)
            {
                return 20;
            }
            return -2;
        }
        return 0;
    }

}
