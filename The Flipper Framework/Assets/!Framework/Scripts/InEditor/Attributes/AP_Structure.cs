using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Properties with this will not show the fields name, and injust give a place to adjust the value. Use in conjunction with others.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DontShowFieldNameAttribute : MultiPropertyAttribute
{
	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		BaseAttribute._GUIContentOnDraw_ = GUIContent.none;

		//EditorGUI.PropertyField(position, property, GUIContent.none);
		return true;
	}
}


/// <summary>
/// Provide a boolean serialize property, and draw the tick box after this property's label, but before its edit field.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawTickBoxBeforeAttribute : MultiPropertyAttribute
{
	public string _booleanForTickBox { get; private set; }

	//Constructor
	public DrawTickBoxBeforeAttribute ( string booleanField ) {
		_booleanForTickBox = booleanField;
	}

	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {

		SettingRects(position, BaseAttribute);

		//EditorGUI.PropertyField(_fieldRect, property, GUIContent.none);

		BaseAttribute._GUIContentOnDraw_ = GUIContent.none;

		return true;
	}

	public override void DrawBeforeProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		SettingRects(position, BaseAttribute);

		EditorGUILayout.BeginHorizontal();
		EditorGUI.PrefixLabel(_labelRect, label);

		//Find the field that will be used as the checkbox between the field and name of field.
		SerializedProperty fieldToCheck = property.serializedObject.FindProperty(_booleanForTickBox);
		EditorGUI.PropertyField(_tickBoxRect, fieldToCheck, GUIContent.none);
	}

	private void SettingRects (Rect position, MultiPropertyAttribute BaseAttribute ) {
		//Get approrpriate sizes of elements, based on inspector panel.
		float fullWidth = EditorGUIUtility.currentViewWidth;
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = fullWidth - labelWidth;
		_labelRect = new Rect(position.x, position.y, labelWidth, position.height);
		_tickBoxRect = new Rect(position.x + labelWidth, position.y, fieldWidth * 0.2f, position.height);
		BaseAttribute._fieldRect_ = new Rect((position.x + labelWidth + fieldWidth * 0.2f) - 4, position.y, fieldWidth * 0.8f, position.height);
	}

	public override void DrawAfterProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		EditorGUILayout.EndHorizontal();
	}
}


/// <summary>
/// Take a list of other properties to draw on the same line as this one. Ensure those properties are HideInInspector so they are not drawn twice.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawHorizontalWithOthersAttribute : MultiPropertyAttribute
{
	public string[] _listOfOtherFields { get; private set; }

	//Constructor
	public DrawHorizontalWithOthersAttribute ( string[] listOfOtherFields ) {
		_listOfOtherFields = listOfOtherFields;
	}


	float _partWidth;

	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		Debug.Log("Draw Horizontal thanks to " + property.name);
		return true;
	}

	public override void DrawBeforeProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		//Draw all in line.
		EditorGUILayout.BeginHorizontal();
		SetPartWidth();

		BaseAttribute._fieldRect_ = new Rect(position.x, position.y, _partWidth - 5, position.height);
	}

	public override void DrawAfterProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		Rect fieldRect = BaseAttribute._fieldRect_;
		SetPartWidth();

		//Go through each string name that was added when applying this attributre, and draw properties that match it. They must be set to HideInInspector to ensure they aren't drawn twice.
		for (int i = 0 ; i < _listOfOtherFields.Length ; i++)
		{
			string newPropertyName = _listOfOtherFields[i];
			if (i != _listOfOtherFields.Length)
				fieldRect = new Rect(position.x + (_partWidth * (i + 1)), position.y, _partWidth - 5, position.height);
			else
				fieldRect = new Rect(position.x + (_partWidth * (i + 1)), position.y, _partWidth, position.height);

			SerializedProperty newProperty = property.serializedObject.FindProperty(newPropertyName);
			EditorGUI.PropertyField(fieldRect, newProperty);
		}

		EditorGUILayout.EndHorizontal();
	}

	private void SetPartWidth() {
		//Get approrpriate sizes of elements, based on inspector panel.
		float fullWidth = EditorGUIUtility.currentViewWidth;
		_partWidth = fullWidth / (_listOfOtherFields.Length + 1);
	}
}
