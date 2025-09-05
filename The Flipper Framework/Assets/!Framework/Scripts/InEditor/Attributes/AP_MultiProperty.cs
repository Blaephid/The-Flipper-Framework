using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[AttributeUsage(AttributeTargets.Field)]
public abstract class MultiPropertyAttribute : PropertyAttribute
{

#if UNITY_EDITOR
	//List of all the Attributes to be applied onto the property. Set in the Property Drawer.
	public List<object> _AttributesToApply = new List<object>();

	public SerializedProperty _Property;
	public string	_debugProperty;

	//Data for drawing the Property Field. Because an instance of this will act as the base property drawer, others need to adjust the data of it, not themselves. This is why methods pass "BaseAttribute"/
	//Using _ as a suffix so its easier to tell what needs to be adjusted at base.

	//By default, is set to label, but if overwritten, that ill be used to draw instead.
	public GUIContent _GUIContentOnDraw_;
	//These static fields are created to store the last used drawing data, so for properties with multiple attributes, this will carry over and be used to draw the same thing without calculating multiple times
	public static GUIContent _staticGUIContentOnDraw_; 

	public Rect		_fieldRect_;
	public static Rect		_staticfieldRect_;

	public Rect _labelRect;
	public Rect _tickBoxRect;

	public bool _isReadOnly;
	public static bool _staticIsReadOnly;

	public bool		_willDraw;
	public static bool _staticWillDraw;

	public static Color _staticColor;

	public virtual GUIContent BuildLabel ( GUIContent label ) {
		return label;
	}
	public virtual bool WillDrawOnGUI ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		return true;
	}

	public virtual float? GetPropertyHeight (float baseHeight, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		return null;
	}

	public virtual void OnChangeCheck(bool wasChanged, MultiPropertyAttribute BaseAttribute, SerializedProperty property ) {

	}

	public virtual void DrawBeforeProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {

	}

	public virtual void DrawAfterProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {

	}

#endif
}