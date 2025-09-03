using UnityEditor;
using UnityEngine;

namespace TrueClouds
{
    [CustomPropertyDrawer(typeof(CloudsReadOnlyAttribute))]
    public class TrueCloudsReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
