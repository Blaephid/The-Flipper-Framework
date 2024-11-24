using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Windows;
using System.Runtime.CompilerServices;
using UnityEditor;
using System.IO;
using templates;
using Unity.VisualScripting;

#if UNITY_EDITOR
public class S_TakeScreenShots : MonoBehaviour
{
	[SerializeField] string _fileName = "Screenshot";
	[SerializeField] string _folderPathFromAssets = "/Assets/ScreenShots/";
	[SerializeField] int _scaleValue = 2;

	[SerializeField] bool _withUI;
	[SerializeField] bool _fileNameWithTimeStamp = true;


	[ContextMenu("Take Shot")]
	public void TakeShot () {
		if(!this.enabled) {return;}

		StartCoroutine(TakeScreenShotCourotine());
	}



	//If UI is set to not be included, finds all canvases (that would be projecting UI) and enable/disable them.
	private void CheckUI ( bool enable ) {
		if (!_withUI)
		{
			Canvas[] UIObjects = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
			for (int i = 0 ; i < UIObjects.Length ; i++)
			{
				UIObjects[i].enabled = enable;
			}
		}
	}

	private IEnumerator TakeScreenShotCourotine () {
		
		CheckUI(false);
		yield return new WaitForEndOfFrame();

		var directory = new DirectoryInfo(Application.dataPath);

		string usename = _fileNameWithTimeStamp ? _fileName + "-" + DateTime.Now.ToString("yyMMdd-Hmmss") : _fileName;
		usename = _folderPathFromAssets + usename + ".png";
		string path = directory.Parent.FullName + usename;

		ScreenCapture.CaptureScreenshot(path, _scaleValue);

		Debug.LogWarning(path + " Has Been Taken");

		//To allow new image show in project files quickly
		yield return new WaitForNextFrameUnit();
		Reset();
		CheckUI(true);
	}

	private void Reset () {
		AssetDatabase.Refresh();
		Debug.Log("Refreshed Assets");
	}

	public S_O_CustomInspectorStyle _InspectorTheme;
}


[CustomEditor(typeof(S_TakeScreenShots))]
public class MainScriptEditor : Editor
{
	S_TakeScreenShots _OwnerScript;

	GUIStyle	_HeaderStyle;
	GUIStyle	_BigButtonStyle;
	GUIStyle	_SmallButtonStyle;
	float	_spaceSize;

	public override void OnInspectorGUI () {
		DrawInspector();
	}

	private void OnEnable () {
		//Setting variables
		_OwnerScript = (S_TakeScreenShots)target;

		if (_OwnerScript._InspectorTheme == null) { return; }
		ApplyStyle();
	}

	private void ApplyStyle () {
		_HeaderStyle = _OwnerScript._InspectorTheme._ReplaceNormalHeaders;
		_BigButtonStyle = _OwnerScript._InspectorTheme._GeneralButton;
		_spaceSize = _OwnerScript._InspectorTheme._spaceSize;
	}

	private void DrawInspector () {

		//The inspector needs a visual theme to use, this makes it available and only displays the rest after it is set.
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_InspectorTheme"), new GUIContent("Inspector Theme"));
		
		serializedObject.ApplyModifiedProperties();
		if (EditorGUI.EndChangeCheck())
		{
			ApplyStyle();
		}

		//Will only happen if above is attatched and has a theme.
		if (_OwnerScript == null || _OwnerScript._InspectorTheme == null) return;

		serializedObject.Update();

		//Describe what the script does
		EditorGUILayout.TextArea("Adding this script to an object allows you to take screenshots of the current game view, both in edit and play mode. \n" +
			"Make sure you've set the correct path for a file to store the image.", EditorStyles.textArea);

		DrawButton();
		EditorGUILayout.LabelField("File", _HeaderStyle);
		DrawProperty("_fileName", "File Name");
		DrawProperty("_folderPathFromAssets", "Folder Path From Assets");
		DrawProperty("_scaleValue", "Scale Value");

		EditorGUILayout.LabelField("Options", _HeaderStyle);
		DrawProperty("_withUI", "Include UI");
		DrawProperty("_fileNameWithTimeStamp", "Include Time Stamp");

		//Button for adding new action
		void DrawButton () {

			//Add new element button.
			if (GUILayout.Button("Take Shot", _BigButtonStyle))
			{
				_OwnerScript.TakeShot();
			}
		}

		//Called whenever a property needs to be shown in the editor.
		void DrawProperty ( string property, string outputName, bool isHorizontal = false ) {
			if (isHorizontal) GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(outputName));
			serializedObject.ApplyModifiedProperties();
		}
	}
}

#endif
