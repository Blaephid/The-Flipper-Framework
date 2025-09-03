using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace TrueClouds
{
    public class NoiseGenerator: EditorWindow
    {
        public enum OctaveLerpType
        {
            Lerp = 0,
            Slerp = 1,
            NormalizedLerp = 2
        }

        public enum InitNoiseType
        {
            Independent = 0,
            InsideUnitSphere = 1,
            OnUnitSphere = 2
        }

        public int Size = 512;
        public InitNoiseType InitialNoiseType = InitNoiseType.OnUnitSphere;
        public int Octaves = 6;
        public float OctaveDownscale = 2.3f;

        public float NoiseReapply = 1.88f;
        public OctaveLerpType LerpType = OctaveLerpType.Lerp;
        public bool NormalizeInTheEnd = false;
        public float ContrastAdjustement = 1;

        private Texture2D _texture;

        private Texture2D _textureR;
        private Texture2D _textureG;
        private Texture2D _textureB;
        private Texture2D _textureA;

        private Texture2D[] _octaveDirections;
        private Texture2D[] _octaves;
        private CloudCamera _target;

        public static void Open(CloudCamera target)
        {
            NoiseGenerator window = GetWindow<NoiseGenerator>();

            window._target = target;
            window.ShowAuxWindow();
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Noise");

            wantsMouseMove = true;

            _texture = new Texture2D(1, 1);
            _textureR = new Texture2D(1, 1);
            _textureG = new Texture2D(1, 1);
            _textureB = new Texture2D(1, 1);
            _textureA = new Texture2D(1, 1);
        }

        Texture2D SphereNoise(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Vector3 color;
                    switch (InitialNoiseType)
                    {
                        case InitNoiseType.Independent:
                            color = new Vector3(Random.value, Random.value, Random.value) * 2 - Vector3.one;
                            break;
                        case InitNoiseType.InsideUnitSphere:
                            color = Random.insideUnitSphere;
                            break;
                        case InitNoiseType.OnUnitSphere:
                            color = Random.onUnitSphere;
                            break;
                        default:
                            throw new InvalidOperationException("Unknown noise type: " + InitialNoiseType);
                    }
                    color = color * .5f + new Vector3(.5f, .5f, .5f);
                    texture.SetPixel(i, j, new Color(color.x, color.y, color.z));
                }
            }
            texture.Apply();
            return texture;
        }

        Texture2D DirectionNoize(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Vector3 color;
                    color = new Vector3(Random.value, Random.value, Random.value);
                    color = color * .5f + new Vector3(.5f, .5f, .5f);
                    texture.SetPixel(i, j, new Color(color.x, color.y, color.z));
                }
            }
            texture.Apply();
            return texture;
        }

        void GenerateOctaves()
        {
            _octaves = new Texture2D[Octaves];
            _octaveDirections = new Texture2D[Octaves];
            float divider = 1;
            for (int octave = 0; octave < Octaves; octave++)
            {
                _octaves[octave] = SphereNoise((int) (Size / divider));
                _octaveDirections[octave] = DirectionNoize((int)(Size / divider));
                divider *= OctaveDownscale;
            }
        }

        void GenerateBaseNoise()
        {
            GenerateOctaves();
            _texture = SphereNoise(Size);
        }

        void GenerateNoise()
        {
            var pixels = _texture.GetPixels();

            Vector3 half = Vector3.one * .5f;

            float noiseMultiplier = 1 / Mathf.Pow(OctaveDownscale, Octaves);
            Profiler.BeginSample("CreateNoizeNoize");
            for (int octave = 0; octave < Octaves; octave++)
            {
                var octaveTexture = _octaves[octave];
                var directionTexture = _octaveDirections[octave];

                Profiler.BeginSample("MakeStep" + octave);

                int id = 0;
                float x = 0, y = 0;
                float oneOverSize = 1.0f / Size;

                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        Color colorCurrent = pixels[id];
                        Vector3 currentVec = new Vector3(colorCurrent.r, colorCurrent.g, colorCurrent.b) - half;

                        Color directionColor = directionTexture.GetPixelBilinear(x, y);
                        Vector3 noizeDirection = new Vector3(directionColor.r, directionColor.g, directionColor.b) - half;

                        Color colorOctave = octaveTexture.GetPixelBilinear(
                            x + noizeDirection.x * currentVec.x * NoiseReapply * noiseMultiplier, 
                            y + noizeDirection.y * currentVec.y * NoiseReapply * noiseMultiplier);

                        Vector3 octaveVec = new Vector3(colorOctave.r, colorOctave.g, colorOctave.b) - half;
                        
                        Vector3 res;
                        switch (LerpType)
                        {
                            case OctaveLerpType.Lerp:
                                res = Vector3.Lerp(currentVec, octaveVec, .6f);
                                break;
                            case OctaveLerpType.Slerp:
                                res = Vector3.Slerp(currentVec, octaveVec, .6f);
                                break;
                            case OctaveLerpType.NormalizedLerp:
                                res = Vector3.Lerp(currentVec, octaveVec, .6f).normalized / 2;
                                break;
                            default:
                                throw new InvalidOperationException("unhandled lerp type " + LerpType);
                        }
                        res += half;

                        pixels[id] = new Color(res.x, res.y, res.z);
                        id++;
                        x += oneOverSize;
                    }
                    y += oneOverSize;
                }
                noiseMultiplier *= OctaveDownscale;
                Profiler.EndSample();
            }
            Profiler.EndSample();

            AdjustColor(pixels);

            _texture.SetPixels(pixels);
            _texture.Apply();

            CopyToARGBTextures();
        }

        private void AdjustColor(Color[] pixels)
        {
            Vector3 half = Vector3.one / 2;
            Profiler.BeginSample("Adjust Color");

            Vector3 center = Vector3.zero;
            for (int i = 0; i < pixels.Length; i++)
            {
                Color colorCurrent = pixels[i];
                Vector3 vec = new Vector3(colorCurrent.r, colorCurrent.g, colorCurrent.b) - half;

                if (NormalizeInTheEnd)
                {
                    vec = vec.normalized / 2 + half;
                    pixels[i] = new Color(vec.x, vec.y, vec.z);
                    vec -= half;
                }

                center += vec;
            }
            center /= pixels.Length;

            for (int i = 0; i < pixels.Length; i++)
            {
                Color colorCurrent = pixels[i];

                Vector3 vec = new Vector3(colorCurrent.r, colorCurrent.g, colorCurrent.b) - half - center;
                vec *= ContrastAdjustement;
                vec += half;

                pixels[i] = new Color(vec.x, vec.y, vec.z);
            }
            Profiler.EndSample();
        }

        private void CopyToARGBTextures()
        {
            Profiler.BeginSample("copy argb");
            Profiler.BeginSample("create argb");
            _textureR = new Texture2D(Size, Size);
            _textureG = new Texture2D(Size, Size);
            _textureB = new Texture2D(Size, Size);
            _textureA = new Texture2D(Size, Size);
            Profiler.EndSample();

            Profiler.BeginSample("perform copy");
            var pixels = _texture.GetPixels32();
            Color32[] pixelsR = new Color32[pixels.Length];
            Color32[] pixelsG = new Color32[pixels.Length];
            Color32[] pixelsB = new Color32[pixels.Length];
            Color32[] pixelsA = new Color32[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 color = pixels[i];
                pixelsR[i] = new Color32(color.r, color.r, color.r, 255);
                pixelsG[i] = new Color32(color.g, color.g, color.g, 255);
                pixelsB[i] = new Color32(color.b, color.b, color.b, 255);
                pixelsA[i] = new Color32(color.a, color.a, color.a, 255);
            }
            Profiler.EndSample();

            Profiler.BeginSample("Apply");

            _textureR.SetPixels32(pixelsR);
            _textureG.SetPixels32(pixelsG);
            _textureB.SetPixels32(pixelsB);
            _textureA.SetPixels32(pixelsA);

            _textureR.Apply();
            _textureG.Apply();
            _textureB.Apply();
            _textureA.Apply();
            Profiler.EndSample();
            Profiler.EndSample();
        }

        private bool _unappliedInitialConditions = true;

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            Size = EditorGUILayout.IntSlider(
                new GUIContent("Texture Size", "Resulting size of the noise texture"), Size, 32, 4096);
            Size = Mathf.CeilToInt(Mathf.Pow(2, Mathf.Round(Mathf.Log(Size, 2))));

            Octaves = EditorGUILayout.IntSlider(
                new GUIContent("Octaves Count", "Count of exponentialy downsampled textures stacked onto eachother"), Octaves, 1, 12);
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            InitialNoiseType = (InitNoiseType)EditorGUILayout.EnumPopup(
                new GUIContent("Initial Noize", "Type of initial content for each noise texture"), InitialNoiseType);

            OctaveDownscale = EditorGUILayout.Slider(
                new GUIContent("Octaves Downscale", "How much to downscale each next octave"), OctaveDownscale, 1, Mathf.Pow(Size, 1.0f / Octaves));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            _unappliedInitialConditions |= EditorGUI.EndChangeCheck();

            DrawPreviews();

            NoiseReapply = EditorGUILayout.Slider(
                new GUIContent("Noize Reapply", "How much displacement to apply on each octave step"), NoiseReapply, 0, Octaves - 1);
            ContrastAdjustement = EditorGUILayout.Slider(
                new GUIContent("Contrast", "Contrast of resulting texture"), ContrastAdjustement, 0, 2);

            GUILayout.BeginHorizontal();
            LerpType = (OctaveLerpType)EditorGUILayout.EnumPopup(
                new GUIContent("Lerp Type", "How to mix colors on each octave step"), LerpType);
            if (LerpType == OctaveLerpType.Lerp)
            {
                NormalizeInTheEnd = EditorGUILayout.Toggle(
                    new GUIContent("Normalise last pass", "Apply normalize() to last color vector"), NormalizeInTheEnd);
            }
            else
            {
                NormalizeInTheEnd = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Generate", "Completely regenerate noise")))
            {
                _unappliedInitialConditions = false;
                GenerateBaseNoise();
                GenerateNoise();
            }

            EditorGUI.BeginDisabledGroup(_unappliedInitialConditions);
            if (GUILayout.Button(new GUIContent("Update", "Apply new settings, but don't reset octave textures")))
            {
                GenerateNoise();
            }
            EditorGUI.EndDisabledGroup();
            
            
            if (GUILayout.Button(new GUIContent("Save", "Save texture to file and apply as Noise Texture to your CloudCamera3D")))
            {
                string path = EditorUtility.SaveFilePanelInProject("Save generated noise texture", "noise", "png", "");
                if (path.Length != 0)
                {
                    File.WriteAllBytes(path, _texture.EncodeToPNG());
                    AssetDatabase.Refresh();
                    (AssetImporter.GetAtPath(path) as TextureImporter).textureCompression = TextureImporterCompression.Uncompressed;
                    (AssetImporter.GetAtPath(path) as TextureImporter).mipmapEnabled = true;

                    _target.Noise = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
            }
            GUILayout.EndHorizontal();
            
            if (Event.current.type == EventType.MouseMove)
            {
                Repaint();
            }
        }

        private void DrawPreview(Rect rect, Texture texture, string label)
        {
            rect.size -= Vector2.one * 2;
            rect.center += Vector2.one;
            EditorGUI.DrawPreviewTexture(rect, texture);

            if (rect.Contains(Event.current.mousePosition))
            {
                var labelRect = rect;
                labelRect.width = 60 + label.Length * 12;
                labelRect.height = 70;
                labelRect.center = rect.center;
                labelRect.y += rect.height / 2 - 50;
                GUIStyle labelStyle = new GUIStyle("NotificationBackground");
                labelStyle.padding = new RectOffset(20, 20, 20, 20);
                EditorGUI.LabelField(labelRect, label, labelStyle);
            }
        }

        private void DrawPreviews()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayoutUtility.GetRect(512, 512);
            Rect rect = GUILayoutUtility.GetLastRect();

            DrawPreview(rect, _texture, "Texture");

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayoutUtility.GetRect(512, 256);
            rect = GUILayoutUtility.GetLastRect();
            rect.width = 256;
            DrawPreview(rect, _textureR, "R");
            rect.x += 256;
            DrawPreview(rect, _textureG, "G");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayoutUtility.GetRect(512, 256);
            rect = GUILayoutUtility.GetLastRect();
            rect.width = 256;
            DrawPreview(rect, _textureB, "B");
            rect.x += 256;
            DrawPreview(rect, _textureA, "A");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void OnWizardOtherButton()
        {
            _texture = new Texture2D(Size, Size);
        }
    }
}
