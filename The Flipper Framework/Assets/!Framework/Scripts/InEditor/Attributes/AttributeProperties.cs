using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws the field/property ONLY if the compared property compared by the comparison type with the value of comparedValue returns true.
/// Based on: https://discussions.unity.com/t/650579
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfAttribute : PropertyAttribute
{

	public string propertyToCheck { get; private set; }
	public object valueToCheckFor { get; private set; }

	/// <summary>
	/// Only draws the field only if a condition is met. Supports enum and bools.
	/// </summary>
	/// <param name="comparedPropertyName">The name of the property that is being compared (case sensitive).</param>
	/// <param name="comparedValue">The value the property is being compared to.</param>
	
	//Constructor
	public DrawIfAttribute ( string comparedPropertyName, object comparedValue ) {
		propertyToCheck = comparedPropertyName;
		valueToCheckFor = comparedValue;
	}
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class SetBoolIfOtherAttribute : PropertyAttribute
{
	public bool             setThisBoolTo { get; private set; }
	public string	valueToCompare { get; private set; }
	public object	checkOtherBoolFor { get; private set; }

	//Constructor
	public SetBoolIfOtherAttribute ( bool setThis, string valueToCheck, object comparedValue ) {
		setThisBoolTo = setThis;
		valueToCompare = valueToCheck;
		checkOtherBoolFor = comparedValue;
	}
}

//This exists only to be used with the CustomReadOnlyDrawer
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class CustomReadOnlyAttribute : PropertyAttribute
{

}
