#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Take a string name of a method in this class, and a object array of its parameters, then draw a button instead of the parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class AsButtonAttribute : MultiPropertyAttribute
{
	public string _buttonName;
	public string _methodName;
	public object[] _Attributes;

	public AsButtonAttribute (string buttonName, string methodName, object[] attributes ) {
		_methodName = methodName;
		_Attributes = attributes;
		_buttonName = buttonName;
	}

	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {

		if (GUI.Button(position, _buttonName))
		{
			var target = property.serializedObject.targetObject;
			var method = target.GetType().GetMethod(_methodName);
			if (method != null)
			{
				method.Invoke(target, _Attributes);
			}
			else
			{
				Debug.LogError($"Method '{_methodName}' not found on {target.GetType()}.");
			}
		}

		return false;
	}
}

#endif
