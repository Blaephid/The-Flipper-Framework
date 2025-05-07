using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public class S_HideVisibilityInEditor : MonoBehaviour
{
	private void OnValidate () {
		var stage = UnityEditor.SceneVisibilityManager.instance;
		stage.Hide(gameObject, true); // true = children too
	}
}
#endif
