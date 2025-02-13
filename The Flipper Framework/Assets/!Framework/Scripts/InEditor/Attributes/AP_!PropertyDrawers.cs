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
using System.Reflection.Emit;


[CustomPropertyDrawer(typeof(MultiPropertyAttribute), true)]
public class MultiPropertyDrawer : PropertyDrawer
{
	private static Attribute _MainAttributeOnThisProperty;
	private static int _currentPropertyID;
	private static float _previousHeight;

	MultiPropertyAttribute _MultiAttribute;

	public override float GetPropertyHeight ( SerializedProperty property, GUIContent label ) {

		_MultiAttribute = attribute as MultiPropertyAttribute;
		_MultiAttribute._Property = property;
		if (PropertyAlreadyCalledThisDraw(property, false)) 
		{ return _previousHeight; }

		float baseHeight = base.GetPropertyHeight(property, label);
		float useHeight = baseHeight;

		_MultiAttribute._AttributesToApply = fieldInfo.GetCustomAttributes(typeof(MultiPropertyAttribute), false).ToList();

		//Go through the attributes, and try to get an altered height, if no altered height return default height.
		for (int i = 0 ; i <  _MultiAttribute._AttributesToApply.Count ; i++)
		{
			object AttributeI = _MultiAttribute._AttributesToApply[i];
			if (AttributeI as MultiPropertyAttribute != null)
			{
				var tempheight = ((MultiPropertyAttribute)AttributeI).GetPropertyHeight(useHeight, property, label, _MultiAttribute);
				if (tempheight.HasValue)
				{
					useHeight = tempheight.Value;
					break;
				}
			}
		}
		_previousHeight = useHeight;
		return useHeight;
	}

	private bool PropertyAlreadyCalledThisDraw ( SerializedProperty property, bool calledFromOnGUI ) {

		string calledFrom = calledFromOnGUI ? " ONGUI" : "PROTERTY HEIGHT";
		Debug.Log(calledFrom +  "- Drawing " + property.displayName + " from " + attribute + "  during " + Event.current);

		int propertyId = property.serializedObject.targetObject.GetInstanceID() ^ property.propertyPath.GetHashCode();
		if (!propertyId.Equals(_currentPropertyID))
		{
			if (calledFromOnGUI)
			{
				_MainAttributeOnThisProperty = attribute;
				_currentPropertyID = propertyId;
			}
			return false;
		}
		else if(_MainAttributeOnThisProperty != attribute && propertyId.Equals(_currentPropertyID))
		{
			if (calledFromOnGUI)
			{
				//The static fields are used to keep track of each important value used to draw the property field, so they don't need to be recalculated if this was already drawn.
				//Not drawing would just erase what was already drawn, so instead draw the same thing (position and style).
				DrawPropertyField(MultiPropertyAttribute._staticWillDraw, MultiPropertyAttribute._staticColor, 
					GUI.color, property, MultiPropertyAttribute._staticIsReadOnly, MultiPropertyAttribute._staticfieldRect_, MultiPropertyAttribute._staticGUIContentOnDraw_);
			}
			return true;
		}
		return false;

	}

	// Draw the property inside the given rect
	public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
		_MultiAttribute = attribute as MultiPropertyAttribute;
		_MultiAttribute._Property = property;
		_MultiAttribute._debugProperty = property.name;

		if (PropertyAlreadyCalledThisDraw(property, true)) { return; }

		//As there is no way to acquire a PropertyDrawer from a PropertyAttribute, functionality must instead be kept in the PropertyAttribute classes.
		_MultiAttribute._AttributesToApply = fieldInfo.GetCustomAttributes(typeof(MultiPropertyAttribute), false).ToList();

		_MultiAttribute._GUIContentOnDraw_ = label;
		_MultiAttribute._fieldRect_ = position;

		Color previousColor = GUI.color;
		var Label = label;
		_MultiAttribute._willDraw = true;

		EditorGUI.BeginChangeCheck();
		//Go through each attribute and call its OnGUI directly. Usually, OnGUI is called once, and in propertyDrawers, like here, but storing methods of the same name in the Attribute allows them to be called here.
		for (int i = 0 ; i < _MultiAttribute._AttributesToApply.Count ; i++)
		{
			object AttributeI = _MultiAttribute._AttributesToApply[i];

			if (AttributeI as MultiPropertyAttribute != null)
			{
				Label = ((MultiPropertyAttribute)AttributeI).BuildLabel(_MultiAttribute._GUIContentOnDraw_);
				_MultiAttribute._willDraw = ((MultiPropertyAttribute)AttributeI).WillDrawOnGUI(position, property, _MultiAttribute._GUIContentOnDraw_, _MultiAttribute);
				if(!_MultiAttribute._willDraw) { break; }
			}
		}

		HandlePropertyField(position, property, label, previousColor);
	}

	private void HandlePropertyField ( Rect position, SerializedProperty property, GUIContent label, Color previousColor ) {

		CallDrawBeforeProperty(position, property, label, _MultiAttribute);

		SaveValuesUsedToDrawInCaseONGUIIsCalledForOtherAttributes();
		DrawPropertyField(_MultiAttribute._willDraw, GUI.color, previousColor, property, _MultiAttribute._isReadOnly, _MultiAttribute._fieldRect_, _MultiAttribute._GUIContentOnDraw_);

		CallDrawAfterProperty(position, property, label, _MultiAttribute);

		if (EditorGUI.EndChangeCheck()) { CallChangeChecks(true); }
		else { CallChangeChecks(false); }

		return;
		void SaveValuesUsedToDrawInCaseONGUIIsCalledForOtherAttributes () {
			//Save the data that was used to draw this property
			MultiPropertyAttribute._staticfieldRect_ = _MultiAttribute._fieldRect_;
			MultiPropertyAttribute._staticGUIContentOnDraw_ = _MultiAttribute._GUIContentOnDraw_;
			MultiPropertyAttribute._staticIsReadOnly = _MultiAttribute._isReadOnly;
			MultiPropertyAttribute._staticColor = GUI.color;
			MultiPropertyAttribute._staticWillDraw = _MultiAttribute._willDraw;
		}
	}

	private void DrawPropertyField (bool willDraw, Color drawColor, Color previousColor, SerializedProperty property, bool isReadOnly, Rect fieldRect, GUIContent content ) {
		if (willDraw)
		{
			if (isReadOnly) { GUI.enabled = false; }
			EditorGUI.PropertyField(fieldRect, property, content);
			if (isReadOnly) { GUI.enabled = true; }
			GUI.color = previousColor;
		}
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
