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


	float _elementWidth;
	int _linesToDraw;
	float _propertiesPerLine;
	float _propertiesInTotal;

	public override float? GetPropertyHeight ( float baseHeight, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		SetPartWidth();
		if (_propertiesPerLine >= _propertiesInTotal) { return null; }
		else {
			int numberOfLines = _linesToDraw;
			return baseHeight * numberOfLines;
		}
	}

	public override void DrawBeforeProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		if(!BaseAttribute._willDraw) { return; }

		//Draw all in line.
		if (Event.current.type == EventType.Layout)
			EditorGUILayout.BeginHorizontal();
		SetPartWidth();

		//Draw Label of Property
		BaseAttribute._fieldRect_ = new Rect(position.x-2, position.y, _elementWidth *1.4f, position.height);
		EditorGUI.PrefixLabel(BaseAttribute._fieldRect_, label);

		//Draw editable field of Property
		BaseAttribute._fieldRect_ = new Rect(position.x - 2 + _elementWidth *1.4f , position.y, _elementWidth *0.6f, position.height);
		BaseAttribute._GUIContentOnDraw_ = GUIContent.none;
	}

	public override void DrawAfterProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		if (!BaseAttribute._willDraw) { return; }
		Rect fieldRect = BaseAttribute._fieldRect_;
		SetPartWidth();

		float elementHeight = position.height / _linesToDraw;

		//Go through each string name that was added when applying this attributre, and draw properties that match it. They must be set to HideInInspector to ensure they aren't drawn twice.
		for (int i = 0 ; i < _listOfOtherFields.Length ; i++)
		{
			string newPropertyName = _listOfOtherFields[i];
			SerializedProperty newProperty = property.serializedObject.FindProperty(newPropertyName);
			Debug.Log(newPropertyName);

			switch (_propertiesPerLine)
			{
				case 1:
					DrawNormally();
					break;
				default:
					DrawLabelAndFieldSeperately();
					break;
			}

			continue;

			void DrawNormally () {
				fieldRect = new Rect(position.x - 2, elementHeight * i, position.width, elementHeight);
				EditorGUI.PropertyField(fieldRect, newProperty);
			}

			void DrawLabelAndFieldSeperately () {
				//If this property is the first after a multiple of how many can be on one line, then move where it should be drawn to the next line.
				if (((i + 1) % _propertiesPerLine == 0 && i > 0) || (i == 0 && _propertiesPerLine == 1))
				{
					float line = (i / _propertiesPerLine) + 1;
					fieldRect = new Rect(position.x - 2, position.y + elementHeight * line, _elementWidth * 1.4f, elementHeight);
				}
				else
					//Draw Label of Property
					MoveAlong(0.6f, 1.4f);

				label = EditorGUIUtility.TrTextContent(newProperty.displayName);
				EditorGUI.PrefixLabel(fieldRect, label);

				Debug.Log(fieldRect +" for " + newProperty.displayName);

				//Draw editable field of Property
				MoveAlong(1.4f, 0.6f);
				EditorGUI.PropertyField(fieldRect, newProperty, GUIContent.none);

			}

			void MoveAlong (float moveModi, float widthModi) {
				fieldRect = new Rect(fieldRect.x + _elementWidth * moveModi, fieldRect.y, _elementWidth * widthModi, elementHeight);
			}
		}

		if (Event.current.type == EventType.Layout)
			EditorGUILayout.EndHorizontal();
	}

	private void SetPartWidth() {
		_propertiesInTotal = _listOfOtherFields.Length + 1;
		_propertiesPerLine = _propertiesInTotal;

		//Get approrpriate sizes of elements, based on inspector panel.
		float fullWidth = EditorGUIUtility.currentViewWidth;
		_elementWidth = fullWidth / _propertiesInTotal;
		_elementWidth /= 2;

		//If there isn't enough space for a property field, decrease how many properties can be on one line to create more space until possible.
		while(_elementWidth * 2 < 320 && _propertiesInTotal > 1 && _propertiesPerLine > 1)
		{
			int newPropertiesPerLine = Mathf.Max(1, S_S_MoreMathMethods.DivideWhileRoundingUp(_propertiesPerLine, 2));
			_elementWidth *= _propertiesPerLine / newPropertiesPerLine;
			_propertiesPerLine = newPropertiesPerLine;
		}

		_linesToDraw = S_S_MoreMathMethods.DivideWhileRoundingUp(_propertiesInTotal, _propertiesPerLine);
	}
}
