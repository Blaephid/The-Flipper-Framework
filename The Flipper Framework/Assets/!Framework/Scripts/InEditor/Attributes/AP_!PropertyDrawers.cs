using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;


[CustomPropertyDrawer(typeof(MultiPropertyAttribute), true)]
public class MultiPropertyDrawer : PropertyDrawer
{
	MultiPropertyAttribute _MultiAttribute;

	public override float GetPropertyHeight ( SerializedProperty property, GUIContent label ) {
		_MultiAttribute = attribute as MultiPropertyAttribute;
		float baseHeight = base.GetPropertyHeight(property, label);
		float useHeight = baseHeight;

		_MultiAttribute._AttributesToApply = fieldInfo.GetCustomAttributes(typeof(MultiPropertyAttribute), false).ToList();

		//Go through the attributes, and try to get an altered height, if no altered height return default height.
		for (int i = 0 ; i <  _MultiAttribute._AttributesToApply.Count ; i++)
		{
			object AttributeI = _MultiAttribute._AttributesToApply[i];
			if (AttributeI as MultiPropertyAttribute != null)
			{
				var tempheight = ((MultiPropertyAttribute)AttributeI).GetPropertyHeight(baseHeight, property, label, _MultiAttribute);
				if (tempheight.HasValue)
				{
					useHeight = tempheight.Value;
					break;
				}
			}
		}
		return useHeight;
	}
	// Draw the property inside the given rect
	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		_MultiAttribute = attribute as MultiPropertyAttribute;

		//As there is no way to acquire a PropertyDrawer from a PropertyAttribute, functionality must instead be kept in the PropertyAttribute classes.
		_MultiAttribute._AttributesToApply = fieldInfo.GetCustomAttributes(typeof(MultiPropertyAttribute), false).ToList();

		_MultiAttribute._GUIContentOnDraw_ = label;
		_MultiAttribute._fieldRect_ = position;


		//if (MultiAttribute.stored == null || MultiAttribute.stored.Count == 0)
		//{
		//	MultiAttribute.stored = fieldInfo.GetCustomAttributes(typeof(MultiPropertyAttribute), false).OrderBy(s => ((PropertyAttribute)s).order).ToList();
		//}

		var Label = label;
		bool willDraw = true;

		EditorGUI.BeginChangeCheck();
		//Go through each attribute and call its OnGUI directly. Usually, OnGUI is called once, and in propertyDrawers, like here, but storing methods of the same name in the Attribute allows them to be called here.
		for (int i = 0 ; i < _MultiAttribute._AttributesToApply.Count ; i++)
		{
			object AttributeI = _MultiAttribute._AttributesToApply[i];

			if (AttributeI as MultiPropertyAttribute != null)
			{
				Label = ((MultiPropertyAttribute)AttributeI).BuildLabel(_MultiAttribute._GUIContentOnDraw_);
				willDraw = ((MultiPropertyAttribute)AttributeI).WillDrawOnGUI(position, property, _MultiAttribute._GUIContentOnDraw_, _MultiAttribute);
				if(!willDraw) { break; }
			}
		}

		if (willDraw) {
			CallMethodInAttributes(obj => _MultiAttribute.DrawBeforeProperty(position, property, label, _MultiAttribute));

			EditorGUI.PropertyField(_MultiAttribute._fieldRect_, property, _MultiAttribute._GUIContentOnDraw_);

			CallMethodInAttributes(obj => _MultiAttribute.DrawAfterProperty(position, property, label, _MultiAttribute));
		}

		if (EditorGUI.EndChangeCheck()){CallChangeChecks(true);}
		else { CallChangeChecks(false); }
	}

	private void CallChangeChecks(bool change ) {
		//Go through each attribute and call its inherited OnChangeCheck individually.
		for (int i = 0 ; i < _MultiAttribute._AttributesToApply.Count ; i++)
		{
			object AttributeI = _MultiAttribute._AttributesToApply[i];

			if (AttributeI as MultiPropertyAttribute != null)
			{
				((MultiPropertyAttribute)AttributeI).OnChangeCheck(change, _MultiAttribute);
			}
		}
	}

	private void CallMethodInAttributes(Action<MultiPropertyAttribute> Method ) {
		for (int i = 0 ; i < _MultiAttribute._AttributesToApply.Count ; i++)
		{
			object AttributeI = _MultiAttribute._AttributesToApply[i];

			if (AttributeI as MultiPropertyAttribute != null)
			{
				//((MultiPropertyAttribute)AttributeI).Method;
				Method((MultiPropertyAttribute)AttributeI);
			}
		}
	}
}


//[CustomPropertyDrawer(typeof(SetBoolIfOtherAttribute))]
//public class SetBoolIfOtherPropertyDrawer : PropertyDrawer
//{

//	// Reference to the attribute on the property.
//	SetBoolIfOtherAttribute setBoolIf;

//	// Field that is being compared.
//	SerializedProperty _fieldToCheck;


//	private bool ShouldSetBoolean ( SerializedProperty propertyToSet ) {
//		//Find the field that determines if this property should be drawn.
//		string propertyToCheck = propertyToSet.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(propertyToSet.propertyPath, setBoolIf._valueToCompare) : setBoolIf._valueToCompare;
//		_fieldToCheck = propertyToSet.serializedObject.FindProperty(propertyToCheck);

//		if (_fieldToCheck == null)
//		{
//			Debug.LogError("Cannot find property with name: " + propertyToCheck);
//			return true;
//		}

//		if (_fieldToCheck.isArray)
//		{
//			return !_fieldToCheck.arraySize.Equals(setBoolIf._checkOtherBoolFor);
//		}

//		// get the value & compare based on types
//		switch (_fieldToCheck.type)
//		{
//			case "bool":
//				return _fieldToCheck.boolValue.Equals(setBoolIf._checkOtherBoolFor);
//			case "float":
//				return _fieldToCheck.floatValue.Equals(setBoolIf._checkOtherBoolFor);	
//			default:
//				Debug.LogError("Error: " + _fieldToCheck.type + " is not supported of " + propertyToCheck);
//				return false;
//		}

//	}

//	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
//		EditorGUI.BeginChangeCheck();

//		SerializedProperty propertyToSet = property;
//		setBoolIf = attribute as SetBoolIfOtherAttribute;

//		bool goalBoolean = setBoolIf._setThisBoolTo;

//		EditorGUI.PropertyField(position, propertyToSet);

//		if (EditorGUI.EndChangeCheck())
//		{
//			if (ShouldSetBoolean(propertyToSet))
//			{
//				//If user tried to change this property, but it would be immediately be set back, give an error message to inform them why they can't change it.
//				if (!propertyToSet.boolValue.Equals(goalBoolean))
//				{
//					Debug.LogError(propertyToSet.name + " cannot be changed as it uses the SetIfOtherProperty Attribute, based on " + setBoolIf._valueToCompare);
//				}
//				propertyToSet.boolValue = goalBoolean; //Set property
//			}
//		}
//		//If this property should be set to something else based on another, then change it now, before drawing.
//		else if (ShouldSetBoolean(propertyToSet))
//		{
//			propertyToSet.boolValue = goalBoolean; //Set property
//		}

//		propertyToSet.serializedObject.ApplyModifiedProperties();
//	}
//}


//[CustomPropertyDrawer(typeof(DontShowFieldNameAttribute))]
//public class DontShowFieldNamePropertyDrawer : PropertyDrawer
//{
//	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
//		EditorGUI.PropertyField(position, property, GUIContent.none);
//	}
//}

//[CustomPropertyDrawer(typeof(DrawTickBoxBeforeAttribute))]
//public class DrawTickBoxBeforePropertyDrawer : PropertyDrawer
//{

//	DrawTickBoxBeforeAttribute drawTick;

//	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
//		drawTick = attribute as DrawTickBoxBeforeAttribute;

//		//Get approrpriate sizes of elements, based on inspector panel.
//		float fullWidth = EditorGUIUtility.currentViewWidth;
//		float labelWidth = EditorGUIUtility.labelWidth;
//		float fieldWidth = fullWidth - labelWidth;
//		Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
//		Rect tickBoxRect = new Rect(position.x + labelWidth, position.y, fieldWidth * 0.2f, position.height);
//		Rect fieldRect = new Rect((position.x + labelWidth + fieldWidth * 0.2f) -4, position.y, fieldWidth * 0.8f, position.height);

//		//Draw all in line.
//		EditorGUILayout.BeginHorizontal();
//		EditorGUI.PrefixLabel(labelRect, label);

//		//Find the field that will be used as the checkbox between the field and name of field.
//		SerializedProperty fieldToCheck = property.serializedObject.FindProperty(drawTick._booleanForTickBox);
//		EditorGUI.PropertyField(tickBoxRect, fieldToCheck, GUIContent.none);

//		EditorGUI.PropertyField(fieldRect, property, GUIContent.none);

//		EditorGUILayout.EndHorizontal();
//	}
//}

//[CustomPropertyDrawer(typeof(DrawHorizontalWithOthersAttribute))]
//public class DrawHorizontalWithOthersPropertyDrawer : PropertyDrawer
//{
//	DrawHorizontalWithOthersAttribute DrawHoriz;

//	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
//		DrawHoriz = attribute as DrawHorizontalWithOthersAttribute;

//		//Draw all in line.
//		EditorGUILayout.BeginHorizontal();

//		//Get approrpriate sizes of elements, based on inspector panel.
//		float fullWidth = EditorGUIUtility.currentViewWidth;
//		float partWidth = fullWidth / (DrawHoriz._listOfOtherFields.Length + 1);

//		Rect fieldRect = new Rect(position.x - 2, position.y, partWidth - 5, position.height);

//		EditorGUI.PropertyField(fieldRect, property);

//		//Go through each string name that was added when applying this attributre, and draw properties that match it. They must be set to HideInInspector to ensure they aren't drawn twice.
//		for (int i = 0 ; i < DrawHoriz._listOfOtherFields.Length ; i++)
//		{
//			string newPropertyName = DrawHoriz._listOfOtherFields[i];
//			if(i != DrawHoriz._listOfOtherFields.Length)
//				fieldRect = new Rect(position.x + (partWidth * (i+1)) -2, position.y, partWidth - 5, position.height);
//			else
//				fieldRect = new Rect(position.x + (partWidth * (i + 1)) - 2, position.y, partWidth, position.height);

//			SerializedProperty newProperty = property.serializedObject.FindProperty(newPropertyName);
//			EditorGUI.PropertyField(fieldRect, newProperty);
//		}

//		EditorGUILayout.EndHorizontal();
//	}
//}