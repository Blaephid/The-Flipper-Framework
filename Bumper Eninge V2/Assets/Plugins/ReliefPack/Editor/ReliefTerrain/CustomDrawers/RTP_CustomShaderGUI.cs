using UnityEngine;
using System;
using System.Collections;
using UnityEditor;

public class RTP_MatPropStringParser
{
    public static string Parse(string val)
    {
        string parsed = val;
        parsed = parsed.Replace("(plus)", "+");
        parsed = parsed.Replace("(minus)", "-");
        parsed = parsed.Replace("(amp)", "&");
        parsed = parsed.Replace("(slash)", "/");
        parsed = parsed.Replace("(excl)", "!");
        parsed = parsed.Replace("(lb)", "[");
        parsed = parsed.Replace("(rb)", "]");
        parsed = parsed.Replace("(quot)", "\"");
        parsed = parsed.Replace("(br)", "\n");
        parsed = parsed.Replace("(comma)", ",");
        return parsed;
    }
}

class RTP_shaderDefineCacheItem
{
    public string define;
    public bool invertFlag;
    public bool result;
    public RTP_shaderDefineCacheItem(string define, bool invertFlag, bool result)
    {
        this.define = define;
        this.invertFlag = invertFlag;
        this.result = result;
    }
}

public class RTP_CustomShaderGUI : MaterialEditor
{


    public string shaderCode;
    public bool showFlag=true;
    public bool inactiveFlag = false;
    public bool helperFlag = false;
    public int active_layer = 0;
    public float nextLabelWidth = 0;
    public string[] features = { "COLOR_MAP", "ADV_COLOR_MAP_BLENDING", "VERTICAL_TEXTURE", "FLOWMAP", "BLENDING_HEIGHT" };
    Shader cur_shader;
    ArrayList definesCache;

    Texture2D _blankGreyTex;
    public Texture2D blankGreyTex
    {
        get {
            if (_blankGreyTex == null)
            {
                _blankGreyTex = new Texture2D(64, 64, TextureFormat.RGB24, false);
                Color[] cols = new Color[_blankGreyTex.width * _blankGreyTex.height];
                for(int i=0; i<cols.Length; i++)
                {
                    cols[i] = Color.grey;
                }
                _blankGreyTex.SetPixels(cols);
                _blankGreyTex.Apply();
            }
            return _blankGreyTex;
        }
    }

    public override void Awake()
    {
        GetShaderCode();
        base.Awake();
    }

    public void GetShaderCode()
    {
        if (target)
        {
            Shader shader = (target as Material).shader;
            if (shader)
            {
                string shader_path = AssetDatabase.GetAssetPath(shader);
                shaderCode = System.IO.File.ReadAllText(shader_path);
            }
        }
        if (definesCache == null)
            definesCache = new ArrayList();
        definesCache.Clear();
    }

    public override void OnEnable()
    {
        GetShaderCode();
        cur_shader = (target as Material).shader;
        base.OnEnable();
    }
    
    public override void OnInspectorGUI()
    {
        showFlag = true;
        inactiveFlag = false;
        helperFlag = false;

        MaterialProperty active_layerProp = MaterialEditor.GetMaterialProperty(targets, "active_layer");
        if (active_layerProp!=null)
        {
            active_layer = (int)active_layerProp.floatValue;
        }
        UnityEngine.Profiling.Profiler.BeginSample("BetterEditor OnInspectorGUI", target);
        base.OnInspectorGUI();
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public bool CheckDefine(string define, bool invertFlag)
    {
        bool result;
        if (definesCache!=null)
        {
            for(int i=0; i<definesCache.Count; i++)
            {
                RTP_shaderDefineCacheItem item = definesCache[i] as RTP_shaderDefineCacheItem;
                if (item.define==define && item.invertFlag == invertFlag)
                {
                    return item.result;
                }
            }
        }
        // calling hundred times shader_code.IndexOf() is sloooow, that's why we need to cache results
        if (CheckDefine("//#define " + define)) result = invertFlag ? true : false;
        else if (CheckDefine("#define " + define)) result = invertFlag ? false : true;
        else result = false;

        RTP_shaderDefineCacheItem new_item = new RTP_shaderDefineCacheItem(define, invertFlag, result);
        definesCache.Add(new_item);

        return result;
    }
    public bool CheckDefine(string define)
    {
        if (shaderCode == null || shaderCode == "") return false;

        int sidx = 0;
        bool flag;
        do
        {
            flag = false;
            int idx = shaderCode.IndexOf(define, sidx);
            if (idx > 0)
            {
                if (shaderCode.Substring(idx - 1, 1) != " ")
                {
                    return true;
                }
                else
                {
                    sidx += 5; flag = true;
                }
            }
        } while (flag);
        return false;
    }

    public void RecompileShader()
    {
        if (cur_shader != (target as Material).shader)
        {
            cur_shader = (target as Material).shader;
            GetShaderCode();
        }
        if (shaderCode == null || shaderCode == "") return;

        string _code = shaderCode;
        MaterialProperty[] props = GetMaterialProperties(targets);
        for (int i = 0; i < props.Length; i++)
        {
            if ((props[i].name.IndexOf("RTP_") == 0) || (Consists(features,props[i].name)))
            {
                // feature toggle
                ChangeShaderDef(ref _code, props[i].name, props[i].floatValue > 0);
            }
        }
        if (shaderCode != _code)
        {
            Shader shader = (target as Material).shader;
            if (shader)
            {
                string shader_path = AssetDatabase.GetAssetPath(shader);
                System.IO.File.WriteAllText(shader_path, _code);
                AssetDatabase.ImportAsset(shader_path, ImportAssetOptions.ForceUpdate);
                GetShaderCode();
            }
        }
    }

    private void ChangeShaderDef(ref string _code, string define_name, bool feature)
    {
        int sidx = 0;
        int idx;
        bool flag;
        do
        {
            flag = false;
            idx = _code.IndexOf("//#define " + define_name, sidx);
            if (idx > 0 && _code.Substring(idx - 1, 1) == " ")
            {
                flag = true;
                sidx = idx + 5;
                idx = -1;
            }
            if (idx < 0) idx = _code.IndexOf("#define " + define_name, sidx);
            if (idx > 0 && _code.Substring(idx - 1, 1) == " ")
            {
                flag = true;
                sidx = idx + 5;
                idx = -1;
            }
            if (idx > 0)
            {
                flag = true; sidx = idx + 5; // search next
                string _code_beg = _code.Substring(0, idx);
                string _code_end = _code.Substring(_code.IndexOfNewLine(idx + 1));
                _code = _code_beg;
                if (feature)
                {
                    _code += "#define " + define_name;
                }
                else
                {
                    _code += "//#define " + define_name;
                }
                _code += _code_end;
            }
        } while (flag);
    }

    private bool Consists(string[] arr, string val)
    {
        for(int i=0; i<arr.Length; i++) {
            if (arr[i] == val) return true;
        }
        return false;
    }

}