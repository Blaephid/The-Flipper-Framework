using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Reflection;



	/// <summary>
	/// Based on: https://discussions.unity.com/t/650579
	/// </summary>
	[CustomPropertyDrawer(typeof(OnlyDrawIfAttribute))]
public class DrawIfPropertyDrawer : PropertyDrawer
{
	#region Fields

	// Reference to the attribute on the property.
	OnlyDrawIfAttribute drawIf;

	// Field that is being compared.
	SerializedProperty fieldToCheck;

	#endregion

	//Custom GetProperty Height to ensure no space is taken if not drawing.
	public override float GetPropertyHeight ( SerializedProperty property, GUIContent label ) {

		if (!WillDraw(property))
			 return -EditorGUIUtility.standardVerticalSpacing;

		// If not being stopped, use default height
		return base.GetPropertyHeight(property, label);
	}


	/// <summary>
	/// Errors default to showing the property.
	/// </summary>
	private bool WillDraw ( SerializedProperty propertyToDraw ) {
		drawIf = attribute as OnlyDrawIfAttribute;
		
		//Find the field that determines if this property should be drawn.
		string propertyToCheck = propertyToDraw.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(propertyToDraw.propertyPath, drawIf._propertyToCheck) : drawIf._propertyToCheck;
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
				return fieldToCheck.boolValue.Equals(drawIf._valueToCheckFor);
			case "Enum":
				return fieldToCheck.enumValueIndex.Equals((int)drawIf._valueToCheckFor);
			default:
				Debug.LogError("Error: " + fieldToCheck.type + " is not supported of " + propertyToCheck);
				return true;
		}
	}

	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		SerializedProperty propertyToDraw = property;

		// If the condition is met, simply draw the field.
		if (!WillDraw(propertyToDraw))
		{
			position = new Rect(0, -10, 0, 0);
			return;
		}
		else
		{
			EditorGUI.PropertyField(position, propertyToDraw);
		}
	}
}

[CustomPropertyDrawer(typeof(DrawOthersIfAttribute))]
public class DrawOthersIfPropertyDrawer : PropertyDrawer
{
	#region Fields

	// Reference to the attribute on the property.
	DrawOthersIfAttribute drawOthersIf;


	#endregion

	//Custom GetProperty Height to ensure no space is taken if not drawing.
	public override float GetPropertyHeight ( SerializedProperty property, GUIContent label ) {
		drawOthersIf = attribute as DrawOthersIfAttribute;
		float baseHeight = drawOthersIf._drawSelf ? base.GetPropertyHeight(property, label) : 0;


		if (WillDraw(property))
			baseHeight += drawOthersIf._otherPropertiesToDraw.Length * base.GetPropertyHeight(property, label);

		// If not being stopped, use default height
		return baseHeight;
	}


	/// <summary>
	/// Errors default to showing the property.
	/// </summary>
	private bool WillDraw ( SerializedProperty propertyToCheck ) {
		// get the value & compare based on types
		switch (propertyToCheck.type)
		{
			case "bool":
				return propertyToCheck.boolValue.Equals(drawOthersIf._valueToCheckFor);
			case "Enum":
				return propertyToCheck.enumValueIndex.Equals((int)drawOthersIf._valueToCheckFor);
			default:
				Debug.LogError("Error: " + propertyToCheck.type + " is not supported");
				return false;
		}
	}

	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		SerializedProperty propertyToCheck = property;
		drawOthersIf = attribute as DrawOthersIfAttribute;

		// If the condition is met, simply draw the field.
		if (WillDraw(propertyToCheck))
		{
			int elementsBeingDrawn = drawOthersIf._otherPropertiesToDraw.Length;
			elementsBeingDrawn = drawOthersIf._drawSelf ? elementsBeingDrawn + 1 : elementsBeingDrawn;
			float heightParts = position.height / elementsBeingDrawn;

			position = new Rect(position.y, position.y / elementsBeingDrawn, position.width, heightParts);

			if (drawOthersIf._drawSelf)
				EditorGUI.PropertyField(position, propertyToCheck);

			//Go through each string name that was added when applying this attributre, and draw properties that match it. They must be set to HideInInspector to ensure they aren't drawn twice.
			for (int i = 0 ; i < drawOthersIf._otherPropertiesToDraw.Length ; i++)
			{
				position.y += heightParts;

				string newPropertyName = drawOthersIf._otherPropertiesToDraw[i];
				SerializedProperty newProperty = property.serializedObject.FindProperty(newPropertyName);
				EditorGUI.PropertyField(position, newProperty);
			}
		}
		else if (drawOthersIf._drawSelf)
			EditorGUI.PropertyField(position, propertyToCheck);
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
		string propertyToCheck = propertyToSet.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(propertyToSet.propertyPath, setBoolIf._valueToCompare) : setBoolIf._valueToCompare;
		fieldToCheck = propertyToSet.serializedObject.FindProperty(propertyToCheck);

		if (fieldToCheck == null)
		{
			Debug.LogError("Cannot find property with name: " + propertyToCheck);
			return true;
		}

		if (fieldToCheck.isArray)
		{
			return !fieldToCheck.arraySize.Equals(setBoolIf._checkOtherBoolFor);
		}

		// get the value & compare based on types
		switch (fieldToCheck.type)
		{
			case "bool":
				return fieldToCheck.boolValue.Equals(setBoolIf._checkOtherBoolFor);
			case "float":
				return fieldToCheck.floatValue.Equals(setBoolIf._checkOtherBoolFor);	
			default:
				Debug.LogError("Error: " + fieldToCheck.type + " is not supported of " + propertyToCheck);
				return false;
		}

	}

	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		EditorGUI.BeginChangeCheck();

		SerializedProperty propertyToSet = property;
		setBoolIf = attribute as SetBoolIfOtherAttribute;

		bool goalBoolean = setBoolIf._setThisBoolTo;

		EditorGUI.PropertyField(position, propertyToSet);

		if (EditorGUI.EndChangeCheck())
		{
			if (ShouldSetBoolean(propertyToSet))
			{
				//If user tried to change this property, but it would be immediately be set back, give an error message to inform them why they can't change it.
				if (!propertyToSet.boolValue.Equals(goalBoolean))
				{
					Debug.LogError(propertyToSet.name + " cannot be changed as it uses the SetIfOtherProperty Attribute, based on " + setBoolIf._valueToCompare);
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


[CustomPropertyDrawer(typeof(DontShowFieldNameAttribute))]
public class DontShowFieldNamePropertyDrawer : PropertyDrawer
{
	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		EditorGUI.PropertyField(position, property, GUIContent.none);
	}
}

[CustomPropertyDrawer(typeof(DrawTickBoxBeforeAttribute))]
public class DrawTickBoxBeforePropertyDrawer : PropertyDrawer
{

	DrawTickBoxBeforeAttribute drawTick;

	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		drawTick = attribute as DrawTickBoxBeforeAttribute;

		//Get approrpriate sizes of elements, based on inspector panel.
		float fullWidth = EditorGUIUtility.currentViewWidth;
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = fullWidth - labelWidth;
		Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
		Rect tickBoxRect = new Rect(position.x + labelWidth, position.y, fieldWidth * 0.2f, position.height);
		Rect fieldRect = new Rect((position.x + labelWidth + fieldWidth * 0.2f) -4, position.y, fieldWidth * 0.8f, position.height);

		//Draw all in line.
		EditorGUILayout.BeginHorizontal();
		EditorGUI.PrefixLabel(labelRect, label);

		//Find the field that will be used as the checkbox between the field and name of field.
		SerializedProperty fieldToCheck = property.serializedObject.FindProperty(drawTick._booleanForTickBox);
		EditorGUI.PropertyField(tickBoxRect, fieldToCheck, GUIContent.none);

		EditorGUI.PropertyField(fieldRect, property, GUIContent.none);

		EditorGUILayout.EndHorizontal();
	}
}

[CustomPropertyDrawer(typeof(DrawHorizontalWithOthersAttribute))]
public class DrawHorizontalWithOthersPropertyDrawer : PropertyDrawer
{
	DrawHorizontalWithOthersAttribute DrawHoriz;

	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		DrawHoriz = attribute as DrawHorizontalWithOthersAttribute;

		//Draw all in line.
		EditorGUILayout.BeginHorizontal();

		//Get approrpriate sizes of elements, based on inspector panel.
		float fullWidth = EditorGUIUtility.currentViewWidth;
		float partWidth = fullWidth / (DrawHoriz._listOfOtherFields.Length + 1);

		Rect fieldRect = new Rect(position.x - 2, position.y, partWidth - 5, position.height);

		EditorGUI.PropertyField(fieldRect, property);

		//Go through each string name that was added when applying this attributre, and draw properties that match it. They must be set to HideInInspector to ensure they aren't drawn twice.
		for (int i = 0 ; i < DrawHoriz._listOfOtherFields.Length ; i++)
		{
			string newPropertyName = DrawHoriz._listOfOtherFields[i];
			if(i != DrawHoriz._listOfOtherFields.Length)
				fieldRect = new Rect(position.x + (partWidth * (i+1)) -2, position.y, partWidth - 5, position.height);
			else
				fieldRect = new Rect(position.x + (partWidth * (i + 1)) - 2, position.y, partWidth, position.height);

			SerializedProperty newProperty = property.serializedObject.FindProperty(newPropertyName);
			EditorGUI.PropertyField(fieldRect, newProperty);
		}

		EditorGUILayout.EndHorizontal();
	}
}