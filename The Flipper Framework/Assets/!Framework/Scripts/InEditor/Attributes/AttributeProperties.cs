using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class CustomBaseAttribute : PropertyAttribute
{

}

[AttributeUsage(AttributeTargets.Field)]
public abstract class MultiPropertyAttribute : PropertyAttribute
{
	public List<object> stored = new List<object>();
	public virtual GUIContent BuildLabel ( GUIContent label ) {
		return label;
	}
	public virtual bool DrawOnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		return true;
	}

	internal virtual float? GetPropertyHeight ( SerializedProperty property, GUIContent label ) {
		return null;
	}
}


/// <summary>
/// Draws the field/property ONLY if the Property To Check has the Value To Check for. Do not use mutliple times with the same property to check, use DrawOthersIf then.
/// Based on: https://discussions.unity.com/t/650579
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class OnlyDrawIfAttribute : MultiPropertyAttribute
{

	public string _propertyToCheck { get; private set; }
	public object _valueToCheckFor { get; private set; }
	//Constructor
	public OnlyDrawIfAttribute ( string comparedPropertyName, object comparedValue ) {
		_propertyToCheck = comparedPropertyName;
		_valueToCheckFor = comparedValue;
	}


	// Field that is being compared.
	SerializedProperty fieldToCheck;

	//Custom GetProperty Height to ensure no space is taken if not drawing.
	internal override float? GetPropertyHeight ( SerializedProperty property, GUIContent label ) {

		if (!WillDraw(property))
			return -EditorGUIUtility.standardVerticalSpacing;

		// If not being stopped, use default height
		return null;
	}

	public override bool DrawOnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		SerializedProperty propertyToDraw = property;

		// If the condition is met, simply draw the field.
		if (!WillDraw(propertyToDraw))
		{
			position = new Rect(0, -10, 0, 0);
			return false;
		}
		else
		{
			return true;
		}
	}


	/// <summary>
	/// Errors default to showing the property.
	/// </summary>
	private bool WillDraw ( SerializedProperty propertyToDraw ) {

		//Find the field that determines if this property should be drawn.
		string propertyToCheck = propertyToDraw.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(propertyToDraw.propertyPath, _propertyToCheck) : _propertyToCheck;
		fieldToCheck = propertyToDraw.serializedObject.FindProperty(propertyToCheck);

		if (fieldToCheck == null)
		{
			Debug.LogError("Cannot find property with name: " + propertyToCheck);
			return true;
		}

		// get the value & compare based on types
		switch (fieldToCheck.type)
		{
			case "bool":
				return fieldToCheck.boolValue.Equals(_valueToCheckFor);
			case "Enum":
				return fieldToCheck.enumValueIndex.Equals((int)_valueToCheckFor);
			default:
				Debug.LogError("Error: " + fieldToCheck.type + " is not supported of " + propertyToCheck);
				return true;
		}
	}
}

/// <summary>
/// Draw a string of other serialized properties if THIS property matches the given value. Use this instead of OnlyDrawIf when hiding or showing large amounts, as OnlyDrawIf leaves noticeable gaps.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawOthersIfAttribute : PropertyAttribute
{
	public bool             _drawSelf;
	public string[] _otherPropertiesToDraw { get; private set; }
	public object _valueToCheckFor { get; private set; }

	/// <summary>
	/// Only draws the field only if a condition is met. Supports enum and bools.
	/// </summary>
	/// <param name="otherPropertiesToDraw">The name of the property that is being compared (case sensitive).</param>
	/// <param name="comparedValue">The value the property is being compared to.</param>

	//Constructor
	public DrawOthersIfAttribute (bool drawSelf, string[] otherPropertiesToDraw, object comparedValue ) {
		_drawSelf = drawSelf;
		_otherPropertiesToDraw = otherPropertiesToDraw;
		_valueToCheckFor = comparedValue;
	}
}


/// <summary>
/// Set this property to a boolean value if the given object matches matches a given value. Use if this property can only be true if another is.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class SetBoolIfOtherAttribute : PropertyAttribute
{
	public bool             _setThisBoolTo { get; private set; }
	public string	_valueToCompare { get; private set; }
	public object	_checkOtherBoolFor { get; private set; }

	//Constructor
	public SetBoolIfOtherAttribute ( bool setThis, string valueToCheck, object comparedValue ) {
		_setThisBoolTo = setThis;
		_valueToCompare = valueToCheck;
		_checkOtherBoolFor = comparedValue;
	}
}


/// <summary>
/// Properties with this will be greyed out and uninteractable in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class CustomReadOnlyAttribute : MultiPropertyAttribute
{
	public override bool DrawOnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		GUI.enabled = false;
		EditorGUI.PropertyField(position, property);
		GUI.enabled = true;
		return false;
	}
}

/// <summary>
/// Properties with this will not show the fields name, and injust give a place to adjust the value. Use in conjunction with others.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DontShowFieldNameAttribute : PropertyAttribute
{

}


/// <summary>
/// Provide a boolean serialize property, and draw the tick box after this property's label, but before its edit field.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawTickBoxBeforeAttribute : PropertyAttribute
{
	public string _booleanForTickBox { get; private set; }

	//Constructor
	public DrawTickBoxBeforeAttribute ( string booleanField ) {
		_booleanForTickBox = booleanField;
	}
}


/// <summary>
/// Take a list of other properties to draw on the same line as this one. Ensure those properties are HideInInspector so they are not drawn twice.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawHorizontalWithOthersAttribute : PropertyAttribute
{
	public string[] _listOfOtherFields { get; private set; }

	//Constructor
	public DrawHorizontalWithOthersAttribute ( string[] listOfOtherFields ) {
		_listOfOtherFields = listOfOtherFields;
	}
}
