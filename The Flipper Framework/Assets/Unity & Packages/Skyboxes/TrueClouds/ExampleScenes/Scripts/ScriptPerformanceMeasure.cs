using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TrueClouds
{
    class ScriptPerformanceMeasure: MonoBehaviour
    {
        public MonoBehaviour Target = null;

        public int BatchDurationInFrames = 10;
        public int BatchCount = 40;

        private string _testResult = "Not measured";
        private List<bool> _isScriptEnabled = new List<bool>();

        private float[] _enabledTimes;
        private float[] _disabledlTimes;

        private GUIStyle _labelStyle;
        private bool _wasMeasureLaunched = false;

        private void Start()
        {
            _labelStyle = new GUIStyle("label") {fontSize = 20};
            _enabledTimes = new float[BatchCount];
            _disabledlTimes = new float[BatchCount];

            for (int i = 0; i < BatchCount; i++)
            {
                _isScriptEnabled.Add(true);
                _isScriptEnabled.Add(false);
            }
            Shuffle(_isScriptEnabled);

        }

        private IEnumerator MeasureCoroutine()
        {            
            int enabledId = 0;
            int disabledId = 0;
            for (int i = 0; i < BatchCount * 2; i++)
            {
                float percent = 100 * i / (BatchCount * 2);
                _testResult = string.Format("Measured {0}%", percent);

                Target.enabled = _isScriptEnabled[i];

                yield return null;

                float time = Time.unscaledTime;
                yield return WaitForFrames(BatchDurationInFrames);
                time = (Time.unscaledTime - time) * 1000 / BatchDurationInFrames;

                if (_isScriptEnabled[i])
                {
                    _enabledTimes[enabledId++] = time;
                }
                else
                {
                    _disabledlTimes[disabledId++] = time;
                }
            }

            SetTimeString();

            Target.enabled = true;
        }

        private void SetTimeString()
        {
            Array.Sort(_enabledTimes);
            Array.Sort(_disabledlTimes);

            float[] times = new float[BatchCount];
            for (int i = 0; i < BatchCount; i++)
            {
                times[i] = _enabledTimes[i] - _disabledlTimes[i];
            }

            Array.Sort(times);
            float percentile50 = times[BatchCount * 50 / 100];
            float percentile90 = times[BatchCount * 90 / 100];

            _testResult = string.Format("3d Cloud rendering takes {0} ms per frame. 90% of time rendering was faster than {1} ms",
                          percentile50.ToString("F4"),
                          percentile90.ToString("F4"));
        }

        private IEnumerator WaitForFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                Thread.Sleep(16);
                yield return null;
            }
        }


        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 40, 1000, 30));
            if (_wasMeasureLaunched)
            {
                GUILayout.Label(_testResult, _labelStyle);
            }
            else
            {
                if (GUILayout.Button("Measure Performance", GUILayout.Width(150)))
                {
                    StartCoroutine(MeasureCoroutine());
                    _wasMeasureLaunched = true;
                }
            }
            GUILayout.EndArea();
        }

        private static System.Random rnd = new System.Random();
        private static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
