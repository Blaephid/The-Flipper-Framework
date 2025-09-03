using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TrueClouds
{
    [ExecuteInEditMode]
    class CloudPointLight : MonoBehaviour
    {
        public float Start = 0;
        public float Range = 10;
        public Color Color = Color.white;
        public float ShadowIntensity = 0.2f;

        private static Shader SHADER = null;

        private static int START_ID = -1;
        private static int RANGE_ID = -1;
        private static int COLOR_ID = -1;
        private static int SHADOW_INTENSITY_ID = -1;

        private Material _material;
        private Transform _transform;
        private GameObject _light;
        private Transform _lightTransform;

        private void OnValidate()
        {
            ValidateHasGoodLayer();
            ValidateDistances();
        }

        private void ValidateHasGoodLayer()
        {
            CloudCamera[] cameras = GetComponents<CloudCamera>();
            if (cameras.Length == 0)
            {
                return;
            }
            if (cameras.All(camera => (camera.LightMask & gameObject.layer) == 0))
            {
                Debug.LogWarning("This light has a layer that is not rendered by any of the Cloud Cameras", gameObject);
            }
        }

        private void ValidateDistances()
        {
            Start = Mathf.Max(0, Start);
            Range = Mathf.Max(Range, Start);
        }

        private void Awake()
        {
            if (SHADER == null)
            {
                InitShaderAndIDs();
            }

            _transform = transform;

            _light = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _light.layer = gameObject.layer;
            _light.hideFlags = HideFlags.HideAndDontSave;
            _material = new Material(SHADER);
            _light.GetComponent<Renderer>().sharedMaterial = _material;
            _lightTransform = _light.transform;
        }

        private void OnDisable()
        {
            _light.SetActive(false);
        }

        private void OnEnable()
        {
            _light.SetActive(true);
        }

        private void OnDestroy()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(_light);
            }
            else
            {
                Destroy(_light);
            }
        }

        private void Update()
        {
            if (SHADER == null)
            {
                InitShaderAndIDs();
            }

            _material.SetFloat(START_ID, Start);
            _material.SetFloat(RANGE_ID, Range);
            _material.SetColor(COLOR_ID, Color);
            _material.SetFloat(SHADOW_INTENSITY_ID, ShadowIntensity);

            float scale = Range * 2 * 1.1f; // scale up a bit to account for the fact that we don't have a perfect sphere
            _lightTransform.localScale = new Vector3(scale, scale, scale); 
            _lightTransform.position = _transform.position;
        }

        private void InitShaderAndIDs()
        {
            SHADER = Shader.Find("Hidden/Clouds/PointLight");

            START_ID = Shader.PropertyToID("_Start");
            RANGE_ID = Shader.PropertyToID("_MaxDistance");
            COLOR_ID = Shader.PropertyToID("_TintColor");
            SHADOW_INTENSITY_ID = Shader.PropertyToID("_ShadowIntensity");
        }

        private void OnDrawGizmosSelected()
        {
            Color gizmo = Color.yellow;
            gizmo.a = 0.7f;
            Gizmos.color = gizmo;
            Gizmos.DrawSphere(transform.position, Start);

            gizmo.a = 0.3f;
            Gizmos.color = gizmo;
            Gizmos.DrawSphere(transform.position, Range);
        }
    }
}
