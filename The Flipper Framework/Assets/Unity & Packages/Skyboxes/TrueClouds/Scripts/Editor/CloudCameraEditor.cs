using System;
using System.Collections.Generic;
using TrueClouds;
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace TrueClouds
{
    [CustomEditor(typeof (CloudCamera), true)]
    public class CloudCameraEditor : Editor
    {
        private CloudCamera _cloudCamera;
        private GUIStyle _lockedStyle;
        private GUIStyle _unlockedStyle;
        
        private static EditTool _editTool = EditTool.General;
        private bool _areNoizePowersLocked;
        private bool _areNoizeScalesLocked;

        private void OnEnable()
        {
            _cloudCamera = (CloudCamera)target;
            _editTool = (EditTool)EditorPrefs.GetInt("CloudCameraEditor._editTool");
            _areNoizePowersLocked = EditorPrefs.GetBool("CloudCameraEditor._areNoizePowersLocked");
            _areNoizeScalesLocked = EditorPrefs.GetBool("CloudCameraEditor._areNoizeScalesLocked");

            SetupShader(ref _cloudCamera.blurFastShader, "Hidden/Clouds/BlurFast");
            SetupShader(ref _cloudCamera.blurShader, "Hidden/Clouds/Blur");
            SetupShader(ref _cloudCamera.blurHQShader, "Hidden/Clouds/BlurHQ");

            SetupShader(ref _cloudCamera.depthBlurShader, "Hidden/Clouds/DepthBlur");
            SetupShader(ref _cloudCamera.depthShader, "Hidden/Clouds/Depth");
            SetupShader(ref _cloudCamera.cloudShader, "Hidden/Clouds/Clouds");
            SetupShader(ref _cloudCamera.clearColorShader, "Hidden/Clouds/ClearColor");
        }

        private void OnDisable()
        {
            _cloudCamera = (CloudCamera)target;
            EditorPrefs.SetInt("CloudCameraEditor._editTool", (int)_editTool);
            EditorPrefs.SetBool("CloudCameraEditor._areNoizePowersLocked", _areNoizePowersLocked);
            EditorPrefs.SetBool("CloudCameraEditor._areNoizeScalesLocked", _areNoizeScalesLocked);
        }

        public override void OnInspectorGUI()
        {
            _lockedStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).GetStyle("IN LockButton");

            _cloudCamera = (CloudCamera)target;
            Undo.RecordObject(_cloudCamera, "cloud camera");

            _editTool = EditTypeToolbar(_editTool);

            switch (_editTool)
            {
                case EditTool.General:
                    GeneralFoldout();
                    break;
                case EditTool.Light:
                    LightFoldout();
                    break;
                case EditTool.Blur:
                    BlurFoldout();
                    break;
                case EditTool.Noise:
                    NoiseFoldout();
                    break;
            }

            EditorUtility.SetDirty(_cloudCamera);

            if (!Application.isPlaying)
            {
                // This is done to update the game view smoothly when cloud camera is selected but don't break editor's behaviour:)
                Repaint();
            }
        }

        private enum EditTool
        {
            General = 0,
            Light = 1,
            Blur = 2,
            Noise = 3
        }

        private EditTool EditTypeToolbar(EditTool selected)
        {
            string[] toolbar = new string[] { "General", "Light", "Blur", "Noise" };
            return (EditTool)GUILayout.Toolbar((int)selected, toolbar);
        }

        private void GeneralFoldout()
        {
            _cloudCamera.CloudsMask = LayerMaskField(
                new GUIContent("Clouds Mask", "Layers that should be rendered as clouds"), _cloudCamera.CloudsMask);
            
            _cloudCamera.WorldBlockingMask = LayerMaskField(
                new GUIContent("Blocking Mask", "Layers that should block clouds from being seen"), _cloudCamera.WorldBlockingMask);
            _cloudCamera.FallbackDistance = EditorGUILayout.FloatField(
                new GUIContent("Fallback Distance", "Distance at which object is completely invisible inside the cloud"), 
                _cloudCamera.FallbackDistance);

            _cloudCamera.LightMask = LayerMaskField(
                new GUIContent("Light Mask", "post process lights for clouds"), _cloudCamera.LightMask);

            if (_cloudCamera.FallbackDistance < 0.01f)
            {
                _cloudCamera.FallbackDistance = 0.01f;
            }

            _cloudCamera.LateCut = EditorGUILayout.Toggle(
                new GUIContent("Late Cut", "Perform the depth test AFTER the blurring step"), _cloudCamera.LateCut);
            _cloudCamera.DistanceToClouds = EditorGUILayout.FloatField(
                new GUIContent("Approximate Distance", "Approximate distance to clouds.\n Used just to scale other values so that they have proper min/max values"),
                _cloudCamera.DistanceToClouds);

            _cloudCamera.DepthPrecision = (DepthPrecision)EditorGUILayout.EnumPopup(
                    new GUIContent("Depth Precision", "Keep low for best performance. If FarPlane is too large, increase"), _cloudCamera.DepthPrecision);
        }

        private void NoiseFoldout()
        {
            _cloudCamera.UseNoise = EditorGUILayout.Toggle(
                new GUIContent("UseNoise", "Add noise to normals, shape and depth"), _cloudCamera.UseNoise);


            using (new EditorGUI.DisabledGroupScope(!_cloudCamera.UseNoise))
            {
                DrawNoisePowerSliders();

                _cloudCamera.Wind = EditorGUILayout.Vector3Field(
                    new GUIContent("Wind", "Wind that is applied to noise."), _cloudCamera.Wind);
                _cloudCamera.NoiseSinTimeScale = EditorGUILayout.Slider(
                    new GUIContent("Sinus Time Scale", "Time scale for waves of depth"), _cloudCamera.NoiseSinTimeScale, 0, 0.3f);
                _cloudCamera.Noise = (Texture2D)EditorGUILayout.ObjectField(
                    new GUIContent("Texture", "Noise that will be projected on the clouds"), _cloudCamera.Noise, typeof(Texture2D), false);
                DrawNoiseScales();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("Generate noise", "Open noise generator"), GUILayout.Width(200)))
                    {
                        NoiseGenerator.Open(_cloudCamera);
                    }
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawNoiseScales()
        {
            float noiseScale = _cloudCamera.NoiseScale;
            float depthNoiseScale = _cloudCamera.DepthNoiseScale;

            using (new IndentLevelScope())
            {
                _cloudCamera.NoiseScale = EditorGUILayout.FloatField(
                    new GUIContent("Noise Scale", "Noise scale for normals and displacement"), _cloudCamera.NoiseScale);
                _cloudCamera.DepthNoiseScale = EditorGUILayout.FloatField(
                    new GUIContent("Depth Noise Scale", "Noise scale for noise for depth"), _cloudCamera.DepthNoiseScale);

                if (_cloudCamera.NoiseScale < 0.01f)
                {
                    _cloudCamera.NoiseScale = 0.01f;
                }
                if (_cloudCamera.DepthNoiseScale < 0.01f)
                {
                    _cloudCamera.DepthNoiseScale = 0.01f;
                }
            }
            DrawDepthLock(GUILayoutUtility.GetLastRect());

            if (_areNoizeScalesLocked)
            {
                float mul = 1;
                mul *= _cloudCamera.NoiseScale / noiseScale;
                mul *= _cloudCamera.DepthNoiseScale / depthNoiseScale;
                _cloudCamera.NoiseScale = noiseScale * mul;
                _cloudCamera.DepthNoiseScale = depthNoiseScale * mul;
            }
        }

        private void DrawNoisePowerSliders()
        {
            using (new IndentLevelScope())
            {
                float normal = _cloudCamera.NormalNoisePower;
                float displacement = _cloudCamera.DisplacementNoisePower;
                float depth = _cloudCamera.DepthNoisePower;
                _cloudCamera.NormalNoisePower = EditorGUILayout.Slider(
                    new GUIContent("Normal", "Power of noise for normals"), _cloudCamera.NormalNoisePower, 0.01f, 5);
                _cloudCamera.DisplacementNoisePower = EditorGUILayout.Slider(
                    new GUIContent("Displacement", "Power of noise for displacement"), _cloudCamera.DisplacementNoisePower, 0.01f, 5);
                DrawSliderLock(GUILayoutUtility.GetLastRect());
                _cloudCamera.DepthNoisePower = EditorGUILayout.Slider(
                    new GUIContent("Depth", "Power of noise for depth"), _cloudCamera.DepthNoisePower, 0.01f, 5);

                if (_areNoizePowersLocked)
                {
                    float mul = 1;
                    mul *= _cloudCamera.NormalNoisePower / normal;
                    mul *= _cloudCamera.DisplacementNoisePower / displacement;
                    mul *= _cloudCamera.DepthNoisePower / depth;
                    _cloudCamera.NormalNoisePower = normal * mul;
                    _cloudCamera.DisplacementNoisePower = displacement * mul;
                    _cloudCamera.DepthNoisePower = depth * mul;
                }
            }
        }

        private void DrawSliderLock(Rect lockRect)
        {
            lockRect.width = 10;
            lockRect.x -= 10;
            _areNoizePowersLocked = EditorGUI.Toggle(lockRect, _areNoizePowersLocked, _lockedStyle);

            Handles.color = _areNoizePowersLocked ? new Color(0.3f, 0.3f, 1f) : new Color(0.3f, 0.3f, 0.3f);
            Vector2 center = lockRect.center + Vector2.right * 10;
            Vector2 top = center - Vector2.up * 17;
            Vector2 bottom = center - Vector2.down * 17;
            Vector2 right = Vector2.right * 10;

            Handles.DrawLine(top - Vector2.up * 2, bottom);
            Handles.DrawLine(top - Vector2.up * 2+ Vector2.right, bottom + Vector2.right);

            Handles.DrawLine(top, top + right);
            Handles.DrawLine(top + Vector2.down, top + right + Vector2.down);

            Handles.DrawLine(center, center + right);
            Handles.DrawLine(center + Vector2.down, center + right + Vector2.down);

            Handles.DrawLine(bottom, bottom + right);
            Handles.DrawLine(bottom + Vector2.down, bottom + right + Vector2.down);
        }

        private void DrawDepthLock(Rect lockRect)
        {
            lockRect.width = 10;
            lockRect.x -= 10;
            lockRect.y -= 17 / 2;
            _areNoizeScalesLocked = EditorGUI.Toggle(lockRect, _areNoizeScalesLocked, _lockedStyle);

            Handles.color = _areNoizeScalesLocked ? new Color(0.3f, 0.3f, 1f) : new Color(0.3f, 0.3f, 0.3f);
            Vector2 center = lockRect.center + Vector2.right * 10;
            Vector2 top = center - Vector2.up * (17 / 2 + 2);
            Vector2 bottom = center - Vector2.down * (17 / 2 + 1);
            Vector2 right = Vector2.right * 10;

            Handles.DrawLine(top - Vector2.up * 2, bottom);
            Handles.DrawLine(top - Vector2.up * 2 + Vector2.right, bottom + Vector2.right);

            Handles.DrawLine(top, top + right);
            Handles.DrawLine(top + Vector2.down, top + right + Vector2.down);

            Handles.DrawLine(bottom, bottom + right);
            Handles.DrawLine(bottom + Vector2.down, bottom + right + Vector2.down);
        }

        private void BlurFoldout()
        {
            EditorGUILayout.LabelField("Blur", EditorStyles.boldLabel);
            using (new IndentLevelScope())
            {
                _cloudCamera.BlurRadius = EditorGUILayout.Slider(
                    new GUIContent("Radius", "Radius of blur applied to normals, alpha, and depth"), _cloudCamera.BlurRadius, 0, 35);
                _cloudCamera.LateCutThreshohld = EditorGUILayout.Slider(
                    new GUIContent("Threshohld", "Blur linear fadeout"), _cloudCamera.LateCutThreshohld, 0, 0.7f);
                _cloudCamera.LateCutPower = EditorGUILayout.Slider(
                    new GUIContent("Power", "Blur exponential fadeout"), _cloudCamera.LateCutPower, 1, 3);
                _cloudCamera.BlurQuality = (BlurQuality)EditorGUILayout.EnumPopup(
                    new GUIContent("Quality", "Gaussian blur quality for normals and alpha"), _cloudCamera.BlurQuality);

                _cloudCamera.DepthFilteringPower = EditorGUILayout.Slider(
                    new GUIContent("Depth Filtering Power", "How much does blur radius depend on distance"), 
                    _cloudCamera.DepthFilteringPower, 0, 2);

                _cloudCamera.UseDepthFiltering = _cloudCamera.DepthFilteringPower > 0.05f;
                if (!_cloudCamera.UseDepthFiltering)
                {
                    _cloudCamera.DepthFilteringPower = 0;
                }
            }

            EditorGUILayout.LabelField("Downsample", EditorStyles.boldLabel);
            using (new IndentLevelScope())
            {
                _cloudCamera.ResolutionDivider = EditorGUILayout.IntSlider(
                    new GUIContent("Clouds", "Downsample for most of the effect"),
                    _cloudCamera.ResolutionDivider, 1, 20);

                _cloudCamera.WorldDepthResolutionDivider = EditorGUILayout.IntSlider(
                    new GUIContent("World Depth", "Downsample for world depth rendering and for final blit"),
                    _cloudCamera.WorldDepthResolutionDivider, 1, 4);
            }
        }

        private void LightFoldout()
        {
            _cloudCamera.Light = (Transform)EditorGUILayout.ObjectField(
                new GUIContent("Sun", "Directional light for clouds"), _cloudCamera.Light, typeof(Transform), true);

            _cloudCamera.UseRamp = EditorGUILayout.Toggle(
                new GUIContent("Use ramp for coloring", "Use more flexible approach for light"), _cloudCamera.UseRamp);

            if (_cloudCamera.UseRamp)
            {
                _cloudCamera.Ramp = (Texture)EditorGUILayout.ObjectField(
                    new GUIContent("Ramp", "Texture that controlls the light.\n The most enlighten part is on the left.\n The most shadowed part is on the right"), 
                    _cloudCamera.Ramp, typeof(Texture), true, GUILayout.Height(60));
            }
            _cloudCamera.LightColor = EditorGUILayout.ColorField(
                new GUIContent("Light color", "Color of light source"), _cloudCamera.LightColor);
            if (!_cloudCamera.UseRamp)
            {
                _cloudCamera.LightEnd = EditorGUILayout.Slider(
                    new GUIContent("Light End", "Angle to light after which there is only shadow color"), _cloudCamera.LightEnd, 0, 1);
                _cloudCamera.ShadowColor = EditorGUILayout.ColorField(
                    new GUIContent("Shadow Color", "Color of the darkest part of the cloud"), _cloudCamera.ShadowColor);
            }

            _cloudCamera.HaloPower = EditorGUILayout.Slider(
                new GUIContent("Silver Lining Power", "Power of shinig through thin parts of the cloud"), _cloudCamera.HaloPower, 0, 10);
            if (_cloudCamera.HaloPower > 0.0005f)
            {
                _cloudCamera.HaloDistance = EditorGUILayout.Slider(
                    new GUIContent("Silver Lining Distance", "Distance from the light source in screen space"), _cloudCamera.HaloDistance, 0, 2);
            }
        }

        public static List<string> layers;
        public static List<int> layerNumbers;
        public static string[] layerNames;
        public static long lastUpdateTick;

        /** Displays a LayerMask field.
         * \param showSpecial Use the Nothing and Everything selections
         * \param selected Current LayerMask
         * \version Unity 3.5 and up will use the EditorGUILayout.MaskField instead of a custom written one.
         */

        private static LayerMask LayerMaskField(GUIContent label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
                else if ((layerMask & 1 << i) != 0)
                {
                    layers.Add("Unknown Layer" + i);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }

        private void SetupShader(ref Shader shader, string shaderName)
        {
            if (shader == null)
            {
                Debug.Log("shader" + shaderName + " is null. Fixing...");
                shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    Debug.LogError("shader " + shaderName + " Not found");
                }
                else
                {
                    Debug.Log("shader " + shaderName + " is fixed");
                }
            }
        }

        private class Foldout : IDisposable
        {
            public Foldout(string name, ref bool foldout, Action contentsDrawer)
            {
                foldout = EditorGUILayout.Foldout(foldout, name);
                EditorGUI.indentLevel++;
                if (foldout)
                {
                    contentsDrawer();
                }
            }

            public void Dispose()
            {
                EditorGUI.indentLevel--;
            }
        }
    }
    class IndentLevelScope : IDisposable
    {
        public IndentLevelScope()
        {
            EditorGUI.indentLevel++;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel--;
        }
    }
}