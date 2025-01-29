using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Based on: https://discussions.unity.com/t/650579
/// </summary>
[CustomPropertyDrawer(typeof(DrawIfAttribute))]
public class DrawIfPropertyDrawer : PropertyDrawer
{
	#region Fields

	// Reference to the attribute on the property.
	DrawIfAttribute drawIf;

	// Field that is being compared.
	SerializedProperty fieldToCheck;

	#endregion

	//Custom GetProperty Height to ensure no space is taken if not drawing.
	public override float GetPropertyHeight ( SerializedProperty property, GUIContent label ) {
		
		if (!WillDraw(property))
			return 0f;

		// If not being stopped, use default height
		return base.GetPropertyHeight(property, label);
	}

	/// <summary>
	/// Errors default to showing the property.
	/// </summary>
	private bool WillDraw ( SerializedProperty propertyToDraw ) {
		drawIf = attribute as DrawIfAttribute;
		
		//Find the field that determines if this property should be drawn.
		string propertyToCheck = propertyToDraw.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(propertyToDraw.propertyPath, drawIf.propertyToCheck) : drawIf.propertyToCheck;
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
				return fieldToCheck.boolValue.Equals(drawIf.valueToCheckFor);
			case "Enum":
				return fieldToCheck.enumValueIndex.Equals((int)drawIf.valueToCheckFor);
			default:
				Debug.LogError("Error: " + fieldToCheck.type + " is not supported of " + propertyToCheck);
				return true;
		}
	}

	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		SerializedProperty propertyToDraw = property;

		// If the condition is met, simply draw the field.
		if (WillDraw(propertyToDraw))
		{
			EditorGUI.PropertyField(position, propertyToDraw);
		}
	}
}


[CustomPropertyDrawer(typeof(CustomReadOnlyAttribute))]
public class CustomReadOnlyPropertyDrawer : PropertyDrawer
{
	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		GUI.enabled = false;
		EditorGUI.PropertyField(position, property);
		GUI.enabled = true;	
	}
}