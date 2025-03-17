using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


/// <summary>
/// Draws the field/property ONLY if the Property To Check has the Value To Check for. Do not use mutliple times with the same property to check, use DrawOthersIf then.
/// Based on: https://discussions.unity.com/t/650579
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class OnlyDrawIfAttribute : MultiPropertyAttribute
{

	public string _propertyToCheck { get; set; }
	public object _valueToCheckFor { get; set; }

	//Constructor
	public OnlyDrawIfAttribute ( string comparedPropertyName, object comparedValue ) {
		_propertyToCheck = comparedPropertyName;
		_valueToCheckFor = comparedValue;
	}


	// Field that is being compared.
	public SerializedProperty _fieldToCheck;

	//Custom GetProperty Height to ensure no space is taken if not drawing.
	public override float? GetPropertyHeight (float baseHeight, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {

		if (!WillDraw(property))
			return -EditorGUIUtility.standardVerticalSpacing;

		// If not being stopped, use default height
		return null;
	}

	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
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
	public virtual bool WillDraw ( SerializedProperty propertyToDraw ) {
		_fieldToCheck = GetFieldToCheck(propertyToDraw, _propertyToCheck);
		if(_fieldToCheck == null) { return true; }

		return DrawOthersIfAttribute.IsPropertyEqualToValue(_fieldToCheck, _valueToCheckFor);
	}

	public static SerializedProperty GetFieldToCheck ( SerializedProperty propertyToEdit, string propertyToFind) {
		//Find the field that determines if this property should be drawn.
		//string propertyToCheck = propertyToEdit.propertyPath.Contains("." +propertyToEdit.name) ? System.IO.Path.ChangeExtension(propertyToEdit.propertyPath, propertyToFind) : propertyToFind;
		string propertyToCheck =  System.IO.Path.ChangeExtension(propertyToEdit.propertyPath, propertyToFind);
		SerializedProperty fieldToCheck = propertyToEdit.serializedObject.FindProperty(propertyToCheck);

		if (fieldToCheck == null)
		{
			fieldToCheck = propertyToEdit.serializedObject.FindProperty(propertyToFind);
			if(fieldToCheck == null)
			{
				Debug.LogError("Called by " + propertyToEdit.name + ". Cannot find property with name: " + propertyToCheck);
				return null;
			}
		}
		return fieldToCheck;
	}
}

/// <summary>
/// Varient on only draw if, but instead only draws the property if the value is false. Use when checking enums and only one would not draw this.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class OnlyDrawIfNotAttribute : OnlyDrawIfAttribute
{
	//Constructor
	public OnlyDrawIfNotAttribute ( string comparedPropertyName, object comparedValue ) : base(comparedPropertyName, comparedValue) {
		_propertyToCheck = comparedPropertyName;
		_valueToCheckFor = comparedValue;
	}


	public override bool WillDraw ( SerializedProperty propertyToDraw ) {
		return !base.WillDraw(propertyToDraw);
	}
}

	/// <summary>
	/// Draw a string of other serialized properties if THIS property matches the given value. Use this instead of OnlyDrawIf when hiding or showing large amounts, as OnlyDrawIf leaves noticeable gaps.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawOthersIfAttribute : MultiPropertyAttribute
{
	public bool             _drawSelf;
	public string[] _otherPropertiesToDraw { get; private set; }
	public object _valueToCheckFor { get; private set; }

	//Constructor
	public DrawOthersIfAttribute (bool drawSelf, string[] otherPropertiesToDraw, object comparedValue ) {
		_drawSelf = drawSelf;
		_otherPropertiesToDraw = otherPropertiesToDraw;
		_valueToCheckFor = comparedValue;
	}

	float _heightParts;

	//Custom GetProperty Height to ensure no space is taken if not drawing.
	public override float? GetPropertyHeight (float baseHeight, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {

		float? height = _drawSelf ? null : 0f;

		if (IsPropertyEqualToValue(property, _valueToCheckFor))
			height += _otherPropertiesToDraw.Length * baseHeight;

		// If not being stopped, use default height
		return height;
	}


	/// <summary>
	/// Errors default to showing the property.
	/// </summary>
	public static bool IsPropertyEqualToValue ( SerializedProperty propertyToCheck, object valueToCheckFor ) {
		if (propertyToCheck.isArray)
		{
			return !propertyToCheck.arraySize.Equals(valueToCheckFor);
		}

		// get the value & compare based on types
		switch (propertyToCheck.type)
		{
			case "bool":
				return propertyToCheck.boolValue.Equals(valueToCheckFor);
			case "Enum":
				return propertyToCheck.enumValueIndex.Equals((int)valueToCheckFor);
			default:
				Debug.LogError("Error: " + propertyToCheck.type + " is not supported");
				return false;
		}
	}

	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		return _drawSelf;
	}

	public override void DrawAfterProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		SerializedProperty propertyToCheck = property;

		// If the condition is met, simply draw the field.
		if (IsPropertyEqualToValue(propertyToCheck, _valueToCheckFor))
		{
			int elementsBeingDrawn = _otherPropertiesToDraw.Length;
			elementsBeingDrawn = _drawSelf ? elementsBeingDrawn + 1 : elementsBeingDrawn;
			_heightParts = position.height / elementsBeingDrawn;

			position = new Rect(position.y, position.y / elementsBeingDrawn, position.width, _heightParts);

			//Go through each string name that was added when applying this attributre, and draw properties that match it. They must be set to HideInInspector to ensure they aren't drawn twice.
			for (int i = 0 ; i < _otherPropertiesToDraw.Length ; i++)
			{
				string newPropertyName = _otherPropertiesToDraw[i];
				SerializedProperty newProperty = property.serializedObject.FindProperty(newPropertyName);
				EditorGUI.PropertyField(position, newProperty);

				position.y += _heightParts;
			}
		}
	}
}