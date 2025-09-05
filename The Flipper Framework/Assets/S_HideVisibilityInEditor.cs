using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public class S_HideVisibilityInEditor : MonoBehaviour
{
	private void OnValidate () {
		if(!enabled) { return; }
		var stage = UnityEditor.SceneVisibilityManager.instance;
		stage.Hide(gameObject, true); // true = children too
	}
}
#endif
