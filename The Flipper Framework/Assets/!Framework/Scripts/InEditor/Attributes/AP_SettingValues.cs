using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


/// <summary>
/// Set this property to a boolean value if the given object matches matches a given value. Use if this property can only be true if another is.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class SetBoolIfOtherAttribute : MultiPropertyAttribute
{
	//Fields to set when constructed
	public bool             _setThisBoolTo { get; private set; }
	public string	_propertyToCheck { get; private set; }
	public object	_valueToCheckFor { get; private set; }

	//Constructor
	public SetBoolIfOtherAttribute ( bool setThis, string propertyToCheck, object comparedValue ) {
		_setThisBoolTo = setThis;
		_propertyToCheck = propertyToCheck;
		_valueToCheckFor = comparedValue;
	}

	// Tracking Fields for the Drawer
	SerializedProperty _fieldToCheck;
	SerializedProperty _propertyToSet;

	private bool ShouldSetBoolean ( SerializedProperty propertyToSet ) {
		//Find the field that determines if this property should be drawn.

		_fieldToCheck = OnlyDrawIfAttribute.GetFieldToCheck(propertyToSet, _propertyToCheck);
		if (_fieldToCheck == null) { return false; }

		return DrawOthersIfAttribute.IsPropertyEqualToValue(_fieldToCheck, _valueToCheckFor);

	}

	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {

		_propertyToSet = property;

		//If this property should be set to something else based on another, then change it now, before drawing.
		if (ShouldSetBoolean(_propertyToSet))
		{
			_propertyToSet.boolValue = _setThisBoolTo; //Set property
		}

		return true;
	}

	public override void OnChangeCheck ( bool wasChanged, MultiPropertyAttribute BaseAttribute, SerializedProperty property ) {
		_propertyToSet = property;
		bool goalBoolean = _setThisBoolTo;

		if (wasChanged)
		{
			if (ShouldSetBoolean(_propertyToSet))
			{
				//If user tried to change this property, but it would be immediately be set back, give an error message to inform them why they can't change it.
				if (!_propertyToSet.boolValue.Equals(goalBoolean))
				{
					Debug.LogError(_propertyToSet.name + " cannot be changed as it uses the SetIfOtherProperty Attribute, based on " + _propertyToCheck);
				}
				_propertyToSet.boolValue = goalBoolean; //Set property
			}
		}

		_propertyToSet.serializedObject.ApplyModifiedProperties();
	}
}


/// <summary>
/// Properties with this will be greyed out and uninteractable in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class CustomReadOnlyAttribute : MultiPropertyAttribute
{
	public override bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		BaseAttribute._isReadOnly = true;
		return true;
	}
}