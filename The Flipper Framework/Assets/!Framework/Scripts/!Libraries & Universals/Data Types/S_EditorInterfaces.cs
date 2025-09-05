using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface ICustomEditorLogic
{
#if UNITY_EDITOR
	public void CustomOnSceneGUI ( SceneView sceneView = null) {

	}
#endif

}

