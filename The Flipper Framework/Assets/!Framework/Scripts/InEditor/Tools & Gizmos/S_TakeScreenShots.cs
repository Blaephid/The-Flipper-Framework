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

		StartCoroutine(TakeScreenShotCoroutine());
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

	private IEnumerator TakeScreenShotCoroutine () {
		
		CheckUI(false);
		yield return new WaitForNextFrameUnit();

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
public class TakeScreenShotEditor : S_CustomInspector_Base
{
	S_TakeScreenShots _OwnerScript;


	public override void OnEnable () {
		//Setting variables
		_OwnerScript = (S_TakeScreenShots)target;
		_InspectorTheme = _OwnerScript._InspectorTheme;

		base.OnEnable();
	}

	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _OwnerScript._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {

		//Describe what the script does
		EditorGUILayout.TextArea("Adding this script to an object allows you to take screenshots of the current game view, both in edit and play mode. \n" +
			"Make sure you've set the correct path for a file to store the image.", EditorStyles.textArea);

		if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject,"Take Shot", _BigButtonStyle, null))
		{
			_OwnerScript.TakeShot();
		}

		EditorGUILayout.LabelField("File", _HeaderStyle);
		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject,"_fileName", "File Name");
		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject,"_folderPathFromAssets", "Folder Path From Assets");
		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject,"_scaleValue", "Scale Value");

		EditorGUILayout.LabelField("Options", _HeaderStyle);
		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject,"_withUI", "Include UI");
		S_S_CustomInspectorMethods.DrawEditableProperty(serializedObject,"_fileNameWithTimeStamp", "Include Time Stamp");
	}
}

#endif
