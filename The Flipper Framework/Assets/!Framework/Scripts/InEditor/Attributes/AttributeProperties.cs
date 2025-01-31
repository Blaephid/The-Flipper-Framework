using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class CustomBaseAttribute : PropertyAttribute
{

}


	/// <summary>
	/// Draws the field/property ONLY if the Property To Check has the Value To Check for. Do not use mutliple times with the same property to check, use DrawOthersIf then.
	/// Based on: https://discussions.unity.com/t/650579
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class OnlyDrawIfAttribute : PropertyAttribute
{

	public string _propertyToCheck { get; private set; }
	public object _valueToCheckFor { get; private set; }

	/// <summary>
	/// Only draws the field only if a condition is met. Supports enum and bools.
	/// </summary>
	/// <param name="comparedPropertyName">The name of the property that is being compared (case sensitive).</param>
	/// <param name="comparedValue">The value the property is being compared to.</param>
	
	//Constructor
	public OnlyDrawIfAttribute ( string comparedPropertyName, object comparedValue ) {
		_propertyToCheck = comparedPropertyName;
		_valueToCheckFor = comparedValue;
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
public class CustomReadOnlyAttribute : PropertyAttribute
{

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
