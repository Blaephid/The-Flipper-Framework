using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;

namespace TrueClouds
{
    public class FPSDisplay : MonoBehaviour
    {
        private string _text;
        private Stopwatch _stopwatch;
        private float _delta;

        private void Update()
        {
            _delta = Mathf.Lerp(_delta, Time.unscaledDeltaTime, 1.0f);
            float fps = 1.0f / _delta;
            _text = string.Format("{0:0.0} ms ({1:0.} fps)", _delta * 1000, fps);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 20));
            GUILayout.Label(_text);
            GUILayout.EndArea();
        }
    }
}