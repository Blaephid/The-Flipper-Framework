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


[CustomPropertyDrawer(typeof(SetBoolIfOtherAttribute))]
public class SetBoolIfOtherPropertyDrawer : PropertyDrawer
{

	// Reference to the attribute on the property.
	SetBoolIfOtherAttribute setBoolIf;

	// Field that is being compared.
	SerializedProperty fieldToCheck;


	private bool ShouldSetBoolean ( SerializedProperty propertyToSet ) {
		//Find the field that determines if this property should be drawn.
		string propertyToCheck = propertyToSet.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(propertyToSet.propertyPath, setBoolIf.valueToCompare) : setBoolIf.valueToCompare;
		fieldToCheck = propertyToSet.serializedObject.FindProperty(propertyToCheck);

		if (fieldToCheck == null)
		{
			Debug.LogError("Cannot find property with name: " + propertyToCheck);
			return true;
		}

		if (fieldToCheck.isArray)
		{
			return !fieldToCheck.arraySize.Equals(setBoolIf.checkOtherBoolFor);
		}

		// get the value & compare based on types
		switch (fieldToCheck.type)
		{
			case "bool":
				return fieldToCheck.boolValue.Equals(setBoolIf.checkOtherBoolFor);
			case "float":
				return fieldToCheck.floatValue.Equals(setBoolIf.checkOtherBoolFor);
			default:
				Debug.LogError("Error: " + fieldToCheck.type + " is not supported of " + propertyToCheck);
				return false;
		}

	}

	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		EditorGUI.BeginChangeCheck();

		SerializedProperty propertyToSet = property;
		setBoolIf = attribute as SetBoolIfOtherAttribute;

		bool goalBoolean = setBoolIf.setThisBoolTo;

		EditorGUI.PropertyField(position, propertyToSet);

		if (EditorGUI.EndChangeCheck())
		{
			if (ShouldSetBoolean(propertyToSet))
			{
				//If user tried to change this property, but it would be immediately be set back, give an error message to inform them why they can't change it.
				if (!propertyToSet.boolValue.Equals(goalBoolean))
				{
					Debug.LogError(propertyToSet.name + " cannot be changed as it uses the SetIfOtherProperty Attribute, based on " + setBoolIf.valueToCompare);
				}
				propertyToSet.boolValue = goalBoolean; //Set property
			}
		}
		//If this property should be set to something else based on another, then change it now, before drawing.
		else if (ShouldSetBoolean(propertyToSet))
		{
			propertyToSet.boolValue = goalBoolean; //Set property
		}

		propertyToSet.serializedObject.ApplyModifiedProperties();
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