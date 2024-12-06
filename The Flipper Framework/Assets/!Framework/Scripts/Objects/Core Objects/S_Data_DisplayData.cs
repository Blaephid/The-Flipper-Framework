using System.Collections;
using System.Collections.Generic;
using templates;
using UnityEditor;
using UnityEngine;
using System;
using TMPro;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;
using System.ComponentModel;

#if UNITY_EDITOR
public class S_Data_DisplayData : MonoBehaviour
{

	[SerializeField]
	private bool _onlyDisplayWhenSelected;

	[SerializeField]
	private string _displayTitle;

	[SerializeField]
	private TextMeshPro _3DTitle;
	[SerializeField]
	private TextMeshPro _3DText;

	[SerializeReference]
	public GameObject[]		_ObjectsToReference;
	private List<IObjectData>	_DataSources = new List<IObjectData>();

	[Serializable]
	public struct StrucDataToDisplay {
		public string variableName;
		public string displayName;
		public S_EditorEnums.CasingTypes casing;
		[ReadOnly(true)]
		public string value;
	}
	public List<StrucDataToDisplay> _DataToDisplay;

	//Go through each object set in editor, and search for scripts using the dataInterface, then add them to a list to use as sources later.
	public void GetDataSources () {
		_DataSources.Clear();
		for (int i = 0 ; i < _ObjectsToReference.Length ; i++)
		{
			IObjectData[] ObjectsDataComponents = _ObjectsToReference[i].GetComponents<IObjectData>();
			for (int j = 0 ; j < ObjectsDataComponents.Length ; j++) { _DataSources.Add(ObjectsDataComponents[j]); }
		}
	}

	public void UpdateData () {
		GetDataSources();

		for (int i = 0 ; i < _DataToDisplay.Count ; i++)
		{
			//Ensures the name taken in from a human matches code style, so it can find a field.
			string translatedVariableName = S_S_EditorMethods.TranslateStringToVariableName(_DataToDisplay[i].variableName, _DataToDisplay[i].casing);
			if (translatedVariableName == "") continue;

			//Goes through each data source until a field matching the given name is found, and returns that value
			object value = null;
			for (int s = 0 ; value == null & s < _DataSources.Count ; s++) 
				value = (S_S_EditorMethods.FindFieldByName(_DataSources[0], translatedVariableName));

			//Updates the data
			StrucDataToDisplay Temp = new StrucDataToDisplay
			{
				casing = _DataToDisplay[i].casing,
				displayName = _DataToDisplay[i].displayName,
				variableName = translatedVariableName,
				value = value.ToString()
			};
			_DataToDisplay [i] = Temp;
		}	

		Update3DText();
	}

	public void Update3DText () {
		_3DTitle.text = _displayTitle;
		transform.localScale = S_S_ObjectMethods.LockScale(transform); //Ensures object is never stretched.

		//Goes through each data element, and makes a new line in the text to include display and value.
		string DisplayText = "";
		for (int i = 0 ; i < _DataToDisplay.Count ; i++) {
			string newText = _DataToDisplay[i].displayName;
			DisplayText += newText + " = " + _DataToDisplay[i].value;
			DisplayText += "\n";
		}

		_3DText.text = DisplayText;
	}


	public S_O_CustomInspectorStyle _InspectorTheme;
}


[CustomEditor(typeof(S_Data_DisplayData))]
public class DisplayDataEditor : Editor
{
	S_Data_DisplayData _OwnerScript;

	GUIStyle _HeaderStyle;
	GUIStyle _BigButtonStyle;
	GUIStyle _SmallButtonStyle;
	float _spaceSize;

	public override void OnInspectorGUI () {
		DrawInspector();
	}

	private void OnEnable () {
		//Setting variables
		_OwnerScript = (S_Data_DisplayData)target;

		if (_OwnerScript._InspectorTheme == null) { return; }
		ApplyStyle();
	}

	private void ApplyStyle () {
		_HeaderStyle = _OwnerScript._InspectorTheme._MainHeaders;
		_BigButtonStyle = _OwnerScript._InspectorTheme._GeneralButton;
		_spaceSize = _OwnerScript._InspectorTheme._spaceSize;
		_SmallButtonStyle = _OwnerScript._InspectorTheme._ResetButton;
	}

	private bool IsThemeNotSet () {
		//The inspector needs a visual theme to use, this makes it available and only displays the rest after it is set.
		if (S_S_CustomInspectorMethods.IsDrawnPropertyChanged(serializedObject, "_InspectorTheme", "Inspector Theme", false))
		{
			ApplyStyle();
		}

		//Will only happen if above is attatched and has a theme.
		return (_OwnerScript == null || _OwnerScript._InspectorTheme == null);
	}

	private void DrawInspector () {

		if (IsThemeNotSet()) return;

		serializedObject.Update();

		//Describe what the script does
		EditorGUILayout.TextArea("Details.", EditorStyles.textArea);

	
		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject, "_onlyDisplayWhenSelected", "Only Display When Selected");
		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject, "_ObjectsToReference", "Objects To Reference", false, true);
		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject, "_3DTitle", "Title Object");
		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject, "_3DText", "Text Object");

		EditorGUILayout.Space(_spaceSize);

		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject, "_displayTitle", "ObjectTitle");

		//Add new element button.
		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject, "Update Data", _BigButtonStyle, _OwnerScript, "Update 3D Text"))
		{
			_OwnerScript.UpdateData();
		}

		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject, "Add New Data", _BigButtonStyle, _OwnerScript))
		{
			_OwnerScript._DataToDisplay.Add(new S_Data_DisplayData.StrucDataToDisplay());
		}

		S_S_CustomInspectorMethods.DrawListCustom(serializedObject, "_DataToDisplay", _SmallButtonStyle, _OwnerScript,
			DrawListElementName, DrawWithEachListElement);
	}

	public void DrawListElementName ( int i, SerializedProperty element ) {
		EditorGUILayout.PropertyField(element, new GUIContent("Data " + i + " - " + _OwnerScript._DataToDisplay[i].displayName));
	}

	public void DrawWithEachListElement (int i) {
		return;
	}
}
#endif
