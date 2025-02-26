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

	private void SettingRects ( Rect position, MultiPropertyAttribute BaseAttribute ) {
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
	public string[] _listOfOtherFields_ { get; private set; }
	public float[] _fieldPrioritySizes_ { get; private set; }

	float _labelPriority_ = 1.4f;
	float _fieldPriority_ = 0.6f;

	//Constructor
	public DrawHorizontalWithOthersAttribute ( string[] listOfOtherFields, float[] fieldBalances = null ) {
		_listOfOtherFields_ = listOfOtherFields;
		if (fieldBalances == null) fieldBalances = new float[_listOfOtherFields_.Length + 1];
		_fieldPrioritySizes_ = fieldBalances;
	}


	int _linesToDraw;
	float _propertiesPerLine;
	float _propertiesInTotal;

	public override float? GetPropertyHeight ( float baseHeight, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		SetPartWidth();
		if (_propertiesPerLine >= _propertiesInTotal) { return null; }
		else
		{
			int numberOfLines = _linesToDraw;
			return baseHeight * numberOfLines;
		}
	}

	public override void DrawBeforeProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		if (!BaseAttribute._willDraw) { return; }

		if (Event.current.type == EventType.Layout) { return; }

		SetPartWidth();

		//Rather than all properties taking an equal portion, more important ones may have higher priority. -1 is used for the main property as it's not included in _listOfOtherFields_
		float widthPercentage = GetWidthPercentageFromPriorityValues(-1);

		SetLabelAndFieldWidthOnPropertyType(property);
		if (widthPercentage < 0.5f || _fieldPriority_ > 1)
			DrawLabelAndFieldSeperately();
		else
			DrawLabelAndFieldTogether();

		void DrawLabelAndFieldTogether () {
			//BaseAttribute._fieldRect_ = new Rect(position.x, position.y, position.width / _propertiesPerLine, position.height / _linesToDraw);
			BaseAttribute._fieldRect_ = new Rect(position.x, position.y, position.width * widthPercentage, position.height / _linesToDraw);
		}

		void DrawLabelAndFieldSeperately () {
			float elementWidth = widthPercentage / 2f;

			//Draw Label of Property
			BaseAttribute._fieldRect_ = new Rect(position.x, position.y, elementWidth * 1.4f, position.height / _linesToDraw);
			EditorGUI.PrefixLabel(BaseAttribute._fieldRect_, label);

			BaseAttribute._GUIContentOnDraw_ = GUIContent.none; //Ensures when field is drawn in PropertyDrawer, it won't do so with a label.
			BaseAttribute._fieldRect_ = new Rect(elementWidth * 1.4f * EditorGUIUtility.currentViewWidth, 0, elementWidth * 0.6f * EditorGUIUtility.currentViewWidth, position.height / _linesToDraw);
		}
	}

	public override void DrawAfterProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		if (!BaseAttribute._willDraw) { return; }

		if (Event.current.type == EventType.Layout) { return; }

		Rect fieldRect = BaseAttribute._fieldRect_;
		fieldRect.x = 0;
		SetPartWidth();

		float elementHeight = position.height / _linesToDraw;
		int line = 0;

		float thisPropertyWidth = 0 ;
		float lastPropertyWidth = position.width * GetWidthPercentageFromPriorityValues(-1);
		lastPropertyWidth *= _propertiesPerLine > 2 ? 0.5f * _fieldPriority_ : 1;

		//Go through each string name that was added when applying this attribute, and draw properties that match it. They must be set to HideInInspector to ensure they aren't drawn twice.
		for (int i = 0 ; i < _listOfOtherFields_.Length ; i++)
		{
			string newPropertyName = _listOfOtherFields_[i];
			SerializedProperty newProperty = property.serializedObject.FindProperty(newPropertyName);

			thisPropertyWidth = position.width * GetWidthPercentageFromPriorityValues(i);

			bool isNewLine = false;
			if (((i + 1) % _propertiesPerLine == 0 && i > 0) || (i == 0 && _propertiesPerLine == 1))
			{
				line = (i / (int)_propertiesPerLine) + 1;
				lastPropertyWidth = 0;
				isNewLine = true;
			}

			SetLabelAndFieldWidthOnPropertyType(newProperty);
			if (thisPropertyWidth < EditorGUIUtility.currentViewWidth / 2 || _fieldPriority_ > 1)
				DrawLabelAndFieldSeperately();
			else
				DrawLabelAndFieldTogether();

			continue;

			void DrawLabelAndFieldTogether () {
				float evenOrOdd = (i + 1) % _propertiesPerLine;
				fieldRect = new Rect(evenOrOdd * (lastPropertyWidth), elementHeight * line, thisPropertyWidth, elementHeight);
				EditorGUI.PropertyField(fieldRect, newProperty);

				lastPropertyWidth = thisPropertyWidth;
			}

			void DrawLabelAndFieldSeperately () {

				thisPropertyWidth /= 2;

				//If this property is the first after a multiple of how many can be on one line, then move where it should be drawn to the next line.
				if (isNewLine)
				{
					fieldRect = new Rect(0, elementHeight * line, thisPropertyWidth * _labelPriority_, elementHeight);
				}
				else
					//Draw Label of Property
					GetNewRectFromOldRect(lastPropertyWidth, thisPropertyWidth * _labelPriority_);

				label = EditorGUIUtility.TrTextContent(newProperty.displayName);
				EditorGUI.PrefixLabel(fieldRect, label);

				//Draw editable field of Property
				GetNewRectFromOldRect(thisPropertyWidth * _labelPriority_, thisPropertyWidth * _fieldPriority_);
				EditorGUI.PropertyField(fieldRect, newProperty, GUIContent.none);

				lastPropertyWidth = thisPropertyWidth * _fieldPriority_;
			}

			void GetNewRectFromOldRect ( float move, float width ) {
				fieldRect = new Rect(fieldRect.x + move, fieldRect.y, width, elementHeight);
			}
		}
	}

	//Takes all priorities for this properties line, and returns the percentage of the total this property gives.
	private float GetWidthPercentageFromPriorityValues ( int n ) {
		if (_propertiesPerLine == 1) { return 1f; }

		float percentage = 1f;
		float totalPriority = 0;
		float currentPriority = _fieldPrioritySizes_[n + 1]; //Must be +1, because _fieldPropertySizes_ includes the base property, when _listOfOtherProperties does not.
		currentPriority = currentPriority <= 0 ? 1 : currentPriority;

		int startOfThisLine =  (int)S_S_MoreMathMethods.GetNumberAsIncrement(n + 1, _propertiesPerLine); //Gets the index for the property at the start of the current line.
														 //Goes through each value on this line, and adds up their priorities. If its 0, it means none were set, so use 1.
		for (int i = 0 ; i < _propertiesPerLine ; i++)
		{
			float thisPriority;
			if (startOfThisLine + i > _fieldPrioritySizes_.Length - 1)
				thisPriority = totalPriority / i; //If no other properties left to draw, then use the average so far. Means the size will stay as it should.
			else
				thisPriority = _fieldPrioritySizes_[startOfThisLine + i];
			totalPriority += thisPriority <= 0 ? 1 : thisPriority;
		}
		//Percentage of width is how much of the total priority this property is.
		percentage = currentPriority / totalPriority;
		return percentage;
	}

	private void SetPartWidth () {
		_propertiesInTotal = _listOfOtherFields_.Length + 1;
		_propertiesPerLine = _propertiesInTotal;

		//Get approrpriate sizes of elements, based on inspector panel.
		float fullWidth = EditorGUIUtility.currentViewWidth;
		float propertyWidth = fullWidth / _propertiesInTotal;

		//If there isn't enough space for a property field, decrease how many properties can be on one line to create more space until possible.
		while (propertyWidth < 220 && _propertiesInTotal > 1 && _propertiesPerLine > 1)
		{
			float newPropertiesPerLine = Mathf.Max(1, _propertiesPerLine - 1);
			propertyWidth *= _propertiesPerLine / newPropertiesPerLine;
			_propertiesPerLine = newPropertiesPerLine;
		}

		_linesToDraw = Mathf.Max(1, S_S_MoreMathMethods.DivideWhileRoundingUp(_propertiesInTotal, _propertiesPerLine));
	}

	private void SetLabelAndFieldWidthOnPropertyType ( SerializedProperty property ) {
		switch (property.propertyType)
		{
			default: _labelPriority_ = 1.4f; _fieldPriority_ = 0.6f; break;

			case SerializedPropertyType.Float:
				_labelPriority_ = 1.5f; _fieldPriority_ = 0.5f; break;

			case SerializedPropertyType.Vector3:
				_labelPriority_ = 0.8f; _fieldPriority_ = 1.2f; break;

			case SerializedPropertyType.ObjectReference:
				_labelPriority_ = 0.8f; _fieldPriority_ = 1.2f; break;
		}
	}

}
