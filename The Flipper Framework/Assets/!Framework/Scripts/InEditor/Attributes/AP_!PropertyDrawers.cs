using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.Rendering.Universal;


[CustomPropertyDrawer(typeof(MultiPropertyAttribute), true)]
public class MultiPropertyDrawer : PropertyDrawer
{
	MultiPropertyAttribute _MultiAttribute;

	public override float GetPropertyHeight ( SerializedProperty property, GUIContent label ) {

		_MultiAttribute = attribute as MultiPropertyAttribute;
		_MultiAttribute._Property = property;

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
		_MultiAttribute._Property = property;

		_MultiAttribute._debugProperty = property.name;

		//As there is no way to acquire a PropertyDrawer from a PropertyAttribute, functionality must instead be kept in the PropertyAttribute classes.
		_MultiAttribute._AttributesToApply = fieldInfo.GetCustomAttributes(typeof(MultiPropertyAttribute), false).ToList();

		_MultiAttribute._GUIContentOnDraw_ = label;
		_MultiAttribute._fieldRect_ = position;

		Color previousColor = GUI.color;
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

		CallDrawBeforeProperty(position, property, label, _MultiAttribute);
		if (willDraw) {
			if (_MultiAttribute._isReadOnly) { GUI.enabled = false; }
			EditorGUI.PropertyField(_MultiAttribute._fieldRect_, property, _MultiAttribute._GUIContentOnDraw_);
			if (_MultiAttribute._isReadOnly) { GUI.enabled = true; }
		}
		CallDrawAfterProperty(position, property, label, _MultiAttribute);

		if (EditorGUI.EndChangeCheck()){CallChangeChecks(true);}
		else { CallChangeChecks(false); }

		GUI.color = previousColor;
	}

	private void CallChangeChecks(bool change ) {
		//Go through each attribute and call its inherited OnChangeCheck individually.
		for (int i = 0 ; i < _MultiAttribute._AttributesToApply.Count ; i++)
		{
			object AttributeI = _MultiAttribute._AttributesToApply[i];

			if (AttributeI as MultiPropertyAttribute != null)
			{
				((MultiPropertyAttribute)AttributeI).OnChangeCheck(change, _MultiAttribute, _MultiAttribute._Property);
			}
		}
	}

	private void CallDrawAfterProperty( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		for (int i = 0 ; i < _MultiAttribute._AttributesToApply.Count ; i++)
		{
			object AttributeI = _MultiAttribute._AttributesToApply[i];

			if (AttributeI as MultiPropertyAttribute != null)
			{
				((MultiPropertyAttribute)AttributeI).DrawAfterProperty(position, property, label, _MultiAttribute);
			}
		}
	}

	private void CallDrawBeforeProperty ( Rect position, SerializedProperty property, GUIContent label, MultiPropertyAttribute BaseAttribute ) {
		for (int i = 0 ; i < _MultiAttribute._AttributesToApply.Count ; i++)
		{
			object AttributeI = _MultiAttribute._AttributesToApply[i];

			if (AttributeI as MultiPropertyAttribute != null)
			{
				((MultiPropertyAttribute)AttributeI).DrawBeforeProperty(position, property, label, _MultiAttribute);
			}
		}
	}
}
