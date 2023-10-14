using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Windows;
using System.Runtime.CompilerServices;
using UnityEditor;

public class ScreenShots : MonoBehaviour
{

    [SerializeField] string fileName = "Screenshot";
    [SerializeField] string folderPathFromAssets = "/ScreenShots/";
    [SerializeField] int scaleValue = 2;



    [ContextMenu("Take Shot")]
    public void takeShot()
    {
        string usename = Application.dataPath + folderPathFromAssets + fileName + ".png";

        ScreenCapture.CaptureScreenshot(usename, scaleValue);
        Debug.LogWarning(usename);

        StartCoroutine(waitFor());
    }

    IEnumerator waitFor()
    {
        yield return new WaitForSeconds(.3f);
        AssetDatabase.Refresh();
    }
}
