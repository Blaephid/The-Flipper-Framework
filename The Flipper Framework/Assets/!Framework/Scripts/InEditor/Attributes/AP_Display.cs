using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Changes the label's colour
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class BaseColourAttribute : MultiPropertyAttribute
{
	public Color            _colour;

	public BaseColourAttribute ( float r, float g, float b, float a ) {
		_colour = new Color(r, g, b, a);
	}

	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		GUI.color = _colour;

		return true;
	}
}

/// <summary>
/// Changes the label's colour if the property is null
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class ColourIfNullAttribute : MultiPropertyAttribute
{
	public Color            _colour;

	public ColourIfNullAttribute ( float r, float g, float b, float a ) {
		_colour = new Color( r, g, b, a );
	}

	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
		{
			GUI.color = _colour;
		}

		return true;
	}
}

/// <summary>
/// Changes the label's colour if the property is a boolean and false
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class ColourIfEqualToAttribute : MultiPropertyAttribute
{
	public bool             _compare;
	public Color            _colour;

	public ColourIfEqualToAttribute (bool compare, float r, float g, float b, float a ) {
		_colour = new Color(r, g, b, a);
		_compare = compare;
	}

	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		if (property.propertyType == SerializedPropertyType.Boolean && property.boolValue == _compare)
		{
			GUI.color = _colour;
		}

		return true;
	}
}