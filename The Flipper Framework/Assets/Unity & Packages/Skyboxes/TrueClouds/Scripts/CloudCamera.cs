using UnityEngine;
using UnityEngine.Profiling;

namespace TrueClouds
{
    public enum BlurQuality
    {
        Low = 0, Medium = 5, High = 10
    }

    public enum DepthPrecision
    {
        Medium = 5, High = 10
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public abstract class CloudCamera : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #region EDITOR_VARIABLES
        
        public LayerMask CloudsMask;
        public LayerMask LightMask;
        public LayerMask WorldBlockingMask;
        
        public int ResolutionDivider = 3;
        public int WorldDepthResolutionDivider = 2;

        public DepthPrecision DepthPrecision = DepthPrecision.Medium;

        public bool LateCut = true;
        
        public float BlurRadius = 10;
        public BlurQuality BlurQuality = BlurQuality.High;
        public float LateCutThreshohld = 0.0f;
        public float LateCutPower = 1.5f;

        public bool UseDepthFiltering = false;
        public float DepthFilteringPower = 0f;

        public bool UseNoise = false;
        public Texture2D Noise;
        public Vector3 Wind = new Vector3(2, 1, 3);
        public float NoiseScale = 1;
        public float DepthNoiseScale = 1;
        public float NormalNoisePower = 1;
        public float DepthNoisePower = 1;
        public float DisplacementNoisePower = 1;

        public float NoiseSinTimeScale = .2f;

        public float DistanceToClouds = 10;

        public Transform Light;

        public bool UseRamp = false;

        public Texture Ramp;

        public Color LightColor = Color.white;
        public Color ShadowColor = new Color(0.6f, 0.72f, 0.84f);
        public float LightEnd = 0.75f;

        public float HaloPower = 3;
        public float HaloDistance = 0.5f;
        
        public float FallbackDistance = 1;
        
        #endregion

        #region DEFAULT_VALUES
        public Shader blurFastShader;
        public Shader blurShader;
        public Shader blurHQShader;

        public Shader depthBlurShader;
        public Shader depthShader;
        public Shader cloudShader;

        public Shader clearColorShader;
        #endregion
        #endregion

        #region PRIVATE_VARIABLES
        private RenderTexture _worldDepth, _cloudDepth;
        private RenderTexture _fromRT, _toRT, _cloudMain, _worldBlit;

        private Material _renderMaterial;
        private Material _blurMaterial;
        private Material _depthBlurMaterial;
        private Material _clearColorMaterial;

        private Camera _camera;
        private Camera _tempCamera;

        private static int LIGHT_DIR_ID;
        private static int LIGHT_POS_ID;
        private static int MAIN_COLOR_ID;
        private static int SHADOW_COLOR_ID;
        private static int LIGHT_END_ID;
        private static int WORLD_DEPTH_ID;
        private static int CAMERA_DEPTH_ID;
        private static int NORMALS_ID;
        private static int RAMP_ID;
        private static int NOISE_ID;
        private static int NORMAL_NOISE_POWER_ID;
        private static int DEPTH_NOISE_POWER_ID;
        private static int DISPLACEMENT_NOISE_POWER_ID;
        private static int NOISE_SIN_TIME_ID;
        private static int NOISE_PARAMS_ID;
        private static int FALLBACK_DIST_ID;
        private static int CAMERA_ROTATION_ID;
        private static int NEAR_PLANE_ID;
        private static int FAR_PLANE_ID;
        private static int CAMERA_DIR_LD;
        private static int CAMERA_DIR_RD;
        private static int CAMERA_DIR_LU;
        private static int CAMERA_DIR_RU;
        private static int HALO_POWER_ID;
        private static int HALO_DISTANCE_ID;
        private static int BLUR_SIZE_ID;
        private static int LATE_CUT_THRESHOLD;
        private static int LATE_CUT_POWER;
        private static int DEPTH_FILTERING_POWER;

        #endregion

        protected virtual void Awake()
        {
            _camera = GetComponent<Camera>();
            GameObject child = new GameObject("cloud camera");
            child.hideFlags = HideFlags.HideAndDontSave;

            child.transform.parent = transform;
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            _tempCamera = child.AddComponent<Camera>();
            _tempCamera.CopyFrom(_camera);
            _tempCamera.enabled = false;
        }

        private void OnEnable()
        {
            // Cleaning old render textures to support script reloading
            // don't worry, they will be set up again in UpdateChangedSettings
            CleanupRenderTextures();
            SetupShaderIDs();
        }
        private void OnDisable()
        {
            CleanupRenderTextures();
        }

        private void SetupShaderIDs()
        {
            LIGHT_DIR_ID = Shader.PropertyToID("_LightDir");
            LIGHT_POS_ID = Shader.PropertyToID("_LightPos");
            MAIN_COLOR_ID = Shader.PropertyToID("_MainColor");
            SHADOW_COLOR_ID = Shader.PropertyToID("_ShadowColor");
            LIGHT_END_ID = Shader.PropertyToID("_LightEnd");
            WORLD_DEPTH_ID = Shader.PropertyToID("_WorldDepth");
            CAMERA_DEPTH_ID = Shader.PropertyToID("_CameraDepth");
            NORMALS_ID = Shader.PropertyToID("_NormalTex");
            RAMP_ID = Shader.PropertyToID("_Ramp");
            NOISE_ID = Shader.PropertyToID("_Noise");
            NOISE_PARAMS_ID = Shader.PropertyToID("_NoiseParams");
            NORMAL_NOISE_POWER_ID = Shader.PropertyToID("_NormalNoisePower");
            NOISE_SIN_TIME_ID = Shader.PropertyToID("_NoiseSinTime");
            DEPTH_NOISE_POWER_ID = Shader.PropertyToID("_DepthNoisePower");
            DISPLACEMENT_NOISE_POWER_ID = Shader.PropertyToID("_DisplacementNoisePower");
            FALLBACK_DIST_ID = Shader.PropertyToID("_FallbackDist");
            CAMERA_ROTATION_ID = Shader.PropertyToID("_CameraRotation");
            NEAR_PLANE_ID = Shader.PropertyToID("_NearPlane");
            FAR_PLANE_ID = Shader.PropertyToID("_FarPlane");
            CAMERA_DIR_LD = Shader.PropertyToID("_CameraDirLD");
            CAMERA_DIR_RD = Shader.PropertyToID("_CameraDirRD");
            CAMERA_DIR_LU = Shader.PropertyToID("_CameraDirLU");
            CAMERA_DIR_RU = Shader.PropertyToID("_CameraDirRU");
            HALO_POWER_ID = Shader.PropertyToID("_HaloPower");
            HALO_DISTANCE_ID = Shader.PropertyToID("_HaloDistance");
            BLUR_SIZE_ID = Shader.PropertyToID("_BlurSize");
            LATE_CUT_THRESHOLD = Shader.PropertyToID("_LateCutThreshohld");
            LATE_CUT_POWER = Shader.PropertyToID("_LateCutPower");
            DEPTH_FILTERING_POWER = Shader.PropertyToID("_DepthFilteringPower");
        }

        private void SetupRenderTextures()
        {
            _cloudMain = GetTemporaryTexture(ResolutionDivider, FilterMode.Bilinear);

            _worldDepth = GetTemporaryTexture(WorldDepthResolutionDivider, FilterMode.Bilinear);
            if (WorldDepthResolutionDivider != 1)
            {
                _worldBlit = GetTemporaryTexture(WorldDepthResolutionDivider, FilterMode.Bilinear);
            }

            _cloudDepth = GetTemporaryTexture(ResolutionDivider, FilterMode.Bilinear);

            _fromRT = GetTemporaryTexture(ResolutionDivider, FilterMode.Bilinear);
            _toRT = GetTemporaryTexture(ResolutionDivider, FilterMode.Bilinear);

            _lastScreenRect = _camera.rect;
        }

        private void CleanupRenderTextures()
        {
            ReleaseTemporaryTexture(ref _cloudMain);

            ReleaseTemporaryTexture(ref _worldDepth);
            ReleaseTemporaryTexture(ref _worldBlit);

            ReleaseTemporaryTexture(ref _cloudDepth);

            ReleaseTemporaryTexture(ref _fromRT);
            ReleaseTemporaryTexture(ref _toRT);

            _lastScreenRect = Rect.zero;
        }

        private void Start()
        {
            _camera.cullingMask &= ~CloudsMask;
            _camera.cullingMask &= ~LightMask;

            _renderMaterial = new Material(cloudShader);
            UpdateBlurMaterial();
            _depthBlurMaterial = new Material(depthBlurShader);

            _clearColorMaterial = new Material(clearColorShader);
        }

        private void UpdateBlurMaterial()
        {
            switch (BlurQuality)
            {
                case BlurQuality.Low:
                    _blurMaterial = new Material(blurFastShader);
                    break;
                case BlurQuality.Medium:
                    _blurMaterial = new Material(blurShader);
                    break;
                case BlurQuality.High:
                    _blurMaterial = new Material(blurHQShader);
                    break;
            }
            _lastBlurQuality = (int)BlurQuality;
        }

        private RenderTexture GetTemporaryTexture(int divider, FilterMode mode)
        {
            RenderTexture res = RenderTexture.GetTemporary(
                (int)_camera.pixelRect.size.x / divider, 
                (int)_camera.pixelRect.size.y / divider,
                16, 
                RenderTextureFormat.ARGB32, 
                RenderTextureReadWrite.Linear);

            return res;
        }

        private void ReleaseTemporaryTexture(ref RenderTexture texture)
        {
            if (texture != null)
            {
                RenderTexture.ReleaseTemporary(texture);
                texture = null;
            }
        }

        protected void RenderClouds(RenderTexture source, RenderTexture destination)
        {
            UpdateChangedSettings();
            
            _tempCamera.CopyFrom(_camera);
            _tempCamera.allowMSAA = false;
            _tempCamera.allowHDR = false;
            _tempCamera.renderingPath = RenderingPath.Forward;
            _tempCamera.depthTextureMode = DepthTextureMode.Depth;
            _tempCamera.enabled = false;

            ApplyBlits(source, destination);
        }

        private void ApplyBlits(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);

            _tempCamera.clearFlags = CameraClearFlags.Color;
            _tempCamera.backgroundColor = Color.white;

            _tempCamera.rect = new Rect(Vector2.zero, Vector2.one);

            _worldDepth.DiscardContents();
            _tempCamera.targetTexture = _worldDepth;
            _tempCamera.cullingMask = WorldBlockingMask;
            _tempCamera.RenderWithShader(depthShader, "RenderType");

            _cloudDepth.DiscardContents();
            _tempCamera.targetTexture = _cloudDepth;
            _tempCamera.cullingMask = CloudsMask;
            _tempCamera.RenderWithShader(depthShader, "RenderType");
            
            _tempCamera.clearFlags = CameraClearFlags.Color;
            _tempCamera.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0);
            _tempCamera.cullingMask = CloudsMask;

            _cloudMain.DiscardContents();
            _tempCamera.targetTexture = _cloudMain;
            _tempCamera.Render();

            DrawGreyBorder(_cloudMain);

            _tempCamera.enabled = false;
            Profiler.BeginSample("Update Shader Values");
            UpdateShaderValues();

            if (LateCut)
            {
                SwapTextures(ref _toRT, ref _cloudMain);
            }
            else
            {
                SwapTextures();
                Graphics.Blit(_cloudMain, _toRT, _renderMaterial, 0);
            }


            if ((LateCut || UseNoise))
            {
                Profiler.BeginSample("Blur Depth");
                _depthBlurMaterial.SetTexture(NORMALS_ID, _toRT);

                SwapTextures(ref _fromRT, ref _cloudDepth);
                Graphics.Blit(_fromRT, _cloudDepth, _depthBlurMaterial, 1);

                SwapTextures(ref _fromRT, ref _cloudDepth);
                Graphics.Blit(_fromRT, _cloudDepth, _depthBlurMaterial, 2);
                _renderMaterial.SetTexture(CAMERA_DEPTH_ID, _cloudDepth);
                _blurMaterial.SetTexture(CAMERA_DEPTH_ID, _cloudDepth);
            }

            Profiler.BeginSample("Blur");
            // Blur x
            SwapTextures();
            Graphics.Blit(_fromRT, _toRT, _blurMaterial, 0);

            // Blur y
            SwapTextures();
            Graphics.Blit(_fromRT, _toRT, _blurMaterial, 1);

            if ((LateCut || UseNoise))
            {
                if (UseNoise)
                { // Apply noise to depth
                    SwapTextures(ref _fromRT, ref _cloudDepth);
                    Graphics.Blit(_fromRT, _cloudDepth, _depthBlurMaterial, 0);
                    _renderMaterial.SetTexture(CAMERA_DEPTH_ID, _cloudDepth);
                }
            }
            Profiler.BeginSample("Calculate Color");
            // Calculate color

            SwapTextures();
            Graphics.Blit(_fromRT, _toRT, _renderMaterial, 1);
            
            Profiler.BeginSample("Additional Lightning");
            Shader.SetGlobalTexture(CAMERA_DEPTH_ID, _cloudDepth);
            Shader.SetGlobalTexture(NORMALS_ID, _fromRT);
            _tempCamera.clearFlags = CameraClearFlags.Depth;
            _tempCamera.targetTexture = _toRT;
            _tempCamera.cullingMask = LightMask;
            _tempCamera.Render();
            Profiler.EndSample();

            // Divide main function and blitting to the screen since it will run for each pixel of the screen.
            Profiler.BeginSample("Final Blit");

            if (!LateCut)
            {
                // Blit to screen using a simple alpha-blend shader
                SwapTextures();
                Graphics.Blit(_fromRT, destination, _renderMaterial, 4);
            }
            else if (WorldDepthResolutionDivider != 1)
            {
                // Blit to temporary buffer with lower resolution using depth cutting shader without alpha blending
                _worldBlit.DiscardContents();
                SwapTextures();
                Graphics.Blit(_fromRT, _worldBlit, _renderMaterial, 3);
                // Then blit to screen using a simple alpha-blend shader
                Graphics.Blit(_worldBlit, destination, _renderMaterial, 4);
            }
            else
            {
                // Blit directly to screen
                SwapTextures();
                Graphics.Blit(_fromRT, destination, _renderMaterial, 2);
            }
        }

        private void DrawGreyBorder(RenderTexture texture)
        {
            Graphics.SetRenderTarget(texture);
            _clearColorMaterial.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Color(Color.black);
            GL.Begin(GL.LINE_STRIP);
            GL.Vertex(new Vector3(1, 1));
            GL.Vertex(new Vector3(1, Screen.height));
            GL.Vertex(new Vector3(Screen.width, Screen.height));
            GL.Vertex(new Vector3(Screen.width, 1));
            GL.Vertex(new Vector3(1, 1));
            GL.End();
        }

        private void SwapTextures()
        {
            SwapTextures(ref _fromRT, ref _toRT);
        }

        private void SwapTextures(ref RenderTexture a, ref RenderTexture b)
        {
            a.DiscardContents();

            RenderTexture tmp = a;
            a = b;
            b = tmp;
        }

        private void SetFeature(string on, string off, bool enable)
        {
            if (enable)
            {
                Shader.DisableKeyword(off);
                Shader.EnableKeyword(on);
            }
            else
            {
                Shader.DisableKeyword(on);
                Shader.EnableKeyword(off);
            }
        }

        private void UpdateShaderValues()
        {
            if (Light != null)
            {
                Vector4 lightDir = -Light.transform.forward;
                lightDir.w = Mathf.Max(0, Vector3.Dot(transform.forward, -Light.transform.forward));
                _renderMaterial.SetVector(LIGHT_DIR_ID, lightDir);

                Vector3 lightPoint = -(transform.worldToLocalMatrix * Light.transform.forward);
                lightPoint.z = -lightPoint.z;
                Vector2 lightPos = _camera.projectionMatrix.MultiplyPoint(lightPoint);
                lightPos = lightPos * 0.5f + new Vector2(.5f, .5f);
                _renderMaterial.SetVector(LIGHT_POS_ID, lightPos);
            }

            bool preciseDepth = DepthPrecision == DepthPrecision.High;
            SetFeature("HIGH_RES_DEPTH", "MEDIUM_RES_DEPTH", preciseDepth);

            bool haloOn = HaloDistance > 0.01f && HaloPower > 0.01f;
            SetFeature("HALO_ON", "HALO_OFF", haloOn);

            SetFeature("LATE_CUT", "EARLY_CUT", LateCut);

            SetFeature("DEPTH_FILTERING_ON", "DEPTH_FILTERING_OFF", UseDepthFiltering);

            SetFeature("NOISE_ON", "NOISE_OFF", UseNoise);

            SetFeature("RAMP_ON", "RAMP_OFF", UseRamp);
            
            _renderMaterial.SetColor(MAIN_COLOR_ID, LightColor);
            if (UseRamp)
            {
                _renderMaterial.SetTexture(RAMP_ID, Ramp);
            }
            else
            {
                _renderMaterial.SetColor(SHADOW_COLOR_ID, ShadowColor);
                _renderMaterial.SetFloat(LIGHT_END_ID, LightEnd);
            }

            _renderMaterial.SetTexture(WORLD_DEPTH_ID, _worldDepth);
            _renderMaterial.SetTexture(CAMERA_DEPTH_ID, _cloudDepth);

            if (UseNoise)
            {
                _renderMaterial.SetTexture(NOISE_ID, Noise);
                _depthBlurMaterial.SetTexture(NOISE_ID, Noise);

                Vector4 noiseParams = new Vector4(-Wind.x, -Wind.y, -Wind.z, 1 / (NoiseScale * DistanceToClouds));
                Vector4 depthNoiseParams = new Vector4(-Wind.x, -Wind.y, -Wind.z, 1 / (DepthNoiseScale * DistanceToClouds));

                _renderMaterial.SetVector(NOISE_PARAMS_ID, noiseParams);
                _depthBlurMaterial.SetVector(NOISE_PARAMS_ID, depthNoiseParams);

                _renderMaterial.SetFloat(NORMAL_NOISE_POWER_ID, NormalNoisePower * 0.3f);
                _renderMaterial.SetFloat(DISPLACEMENT_NOISE_POWER_ID, DisplacementNoisePower * 0.07f * DistanceToClouds);
                _depthBlurMaterial.SetFloat(DEPTH_NOISE_POWER_ID, DepthNoisePower * DistanceToClouds);

                Vector3 sinTime = new Vector3(
                    Mathf.Sin((Time.time * NoiseSinTimeScale          ) * 2 * Mathf.PI),
                    Mathf.Sin((Time.time * NoiseSinTimeScale + 0.3333f) * 2 * Mathf.PI),
                    Mathf.Sin((Time.time * NoiseSinTimeScale + 0.6666f) * 2 * Mathf.PI));

                _depthBlurMaterial.SetVector(NOISE_SIN_TIME_ID, sinTime);
            }

            _renderMaterial.SetFloat(FALLBACK_DIST_ID, FallbackDistance);
            _depthBlurMaterial.SetFloat(FALLBACK_DIST_ID, FallbackDistance);

            _renderMaterial.SetMatrix(CAMERA_ROTATION_ID,    transform.localToWorldMatrix);
            _depthBlurMaterial.SetMatrix(CAMERA_ROTATION_ID, transform.localToWorldMatrix);

            _renderMaterial.SetFloat(NEAR_PLANE_ID, _camera.nearClipPlane);
            _depthBlurMaterial.SetFloat(NEAR_PLANE_ID, _camera.nearClipPlane);
            _renderMaterial.SetFloat(FAR_PLANE_ID, _camera.farClipPlane);
            _blurMaterial.SetFloat(FAR_PLANE_ID, _camera.farClipPlane);
            _depthBlurMaterial.SetFloat(FAR_PLANE_ID, _camera.farClipPlane);

            _blurMaterial.SetFloat(LATE_CUT_THRESHOLD, LateCutThreshohld);
            _blurMaterial.SetFloat(LATE_CUT_POWER, LateCutPower);

            float blurRadiusScaled = BlurRadius;
            if (UseDepthFiltering)
            {
                blurRadiusScaled *= Mathf.Pow(DistanceToClouds / _camera.farClipPlane, DepthFilteringPower);
                _blurMaterial.SetFloat(DEPTH_FILTERING_POWER, DepthFilteringPower);
                _depthBlurMaterial.SetFloat(DEPTH_FILTERING_POWER, DepthFilteringPower);
            }
            _depthBlurMaterial.SetFloat(BLUR_SIZE_ID, blurRadiusScaled);
            _blurMaterial.SetFloat(BLUR_SIZE_ID, blurRadiusScaled);

            Matrix4x4 world2local = transform.worldToLocalMatrix;
            Vector4 
                _CameraDirLD = world2local * Point(_camera.ScreenToWorldPoint(new Vector3(0, 0, 1))),
                _CameraDirRD = world2local * Point(_camera.ScreenToWorldPoint(new Vector3(_camera.pixelWidth, 0, 1))),
                _CameraDirLU = world2local * Point(_camera.ScreenToWorldPoint(new Vector3(0, _camera.pixelHeight, 1))),
                _CameraDirRU = world2local * Point(_camera.ScreenToWorldPoint(new Vector3(_camera.pixelWidth, _camera.pixelHeight, 1)));

            _renderMaterial.SetVector(CAMERA_DIR_LD, _CameraDirLD);
            _depthBlurMaterial.SetVector(CAMERA_DIR_LD, _CameraDirLD);
            _renderMaterial.SetVector(CAMERA_DIR_RD, _CameraDirRD);
            _depthBlurMaterial.SetVector(CAMERA_DIR_RD, _CameraDirRD);
            _renderMaterial.SetVector(CAMERA_DIR_LU, _CameraDirLU);
            _depthBlurMaterial.SetVector(CAMERA_DIR_LU, _CameraDirLU);
            _renderMaterial.SetVector(CAMERA_DIR_RU, _CameraDirRU);
            _depthBlurMaterial.SetVector(CAMERA_DIR_RU, _CameraDirRU);

            _renderMaterial.SetFloat(HALO_POWER_ID, HaloPower);
            _renderMaterial.SetFloat(HALO_DISTANCE_ID, HaloDistance / 2);
        }

        private static Vector4 Point(Vector3 v)
        {
            return new Vector4(v.x, v.y, v.z, 1);
        }

        private int _lastBlurQuality = -1;
        private int _lastResolutionDivider = -1;
        private int _lastWorldResolutionDivider = -1;
        private Rect _lastScreenRect = Rect.zero;
        private void UpdateChangedSettings()
        {
            if (_lastBlurQuality != (int)BlurQuality)
            {
                UpdateBlurMaterial();
            }

            if (_lastResolutionDivider != ResolutionDivider || 
                _lastWorldResolutionDivider != WorldDepthResolutionDivider || 
                _lastScreenRect != _camera.rect)
            {
                CleanupRenderTextures();

                _lastResolutionDivider = ResolutionDivider;
                _lastWorldResolutionDivider = WorldDepthResolutionDivider;

                SetupRenderTextures();
            }
        }
    }

}
