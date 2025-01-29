using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

#if UNITY_EDITOR
public class S_Vis_Base : MonoBehaviour
{
	[Header("Visualisation")]

	[CustomReadOnly, SerializeField, Tooltip("Must be defined in code. If true, will serialize fields releveant to Visualisation, as not all classes will have the necessary implementation for this.")]
	public bool _hasVisualisationScripted = false;

	[DrawIf("_hasVisualisationScripted", true)]
	public bool _drawAtAllTimes = true;
	[DrawIf("_hasVisualisationScripted", true)]
	public bool _drawIfParentSelected = false;
	[DrawIf("_hasVisualisationScripted", true)]
	public Color _normalOutlineColour = Color.grey;
	[DrawIf("_hasVisualisationScripted", true)]
	public Color _selectedOutlineColour = Color.white;
	[DrawIf("_hasVisualisationScripted", true)]
	public Color _selectedFillColour = new Color(1,1,1,0.1f);

	private void OnDrawGizmos () {
		if (!enabled || !_hasVisualisationScripted) { return; }

		if (transform.parent != null)
		{
			if(_drawIfParentSelected)
				if (S_S_EditorMethods.IsThisOrListOrChildrenSelected(transform.parent, new GameObject[] { transform.parent.gameObject })) { DrawGizmosAndHandles(true); return; }
			if (S_S_EditorMethods.IsThisOrListOrChildrenSelected(transform, new GameObject[] { transform.parent.gameObject })) { DrawGizmosAndHandles(true); return; }
		}
		else
			if (S_S_EditorMethods.IsThisOrListOrChildrenSelected(transform, null)){ DrawGizmosAndHandles(true); return; }

		if (_drawAtAllTimes)
			DrawGizmosAndHandles(false);
	}

	//Called whenever object is selected when gizmos are enabled.
	public virtual void DrawGizmosAndHandles ( bool selected ) {

	}


	public virtual void VisualiseWithSelectableHandle (Vector3 position ,float handleRadius) {
		//Only draw select handle if not already selected.
		if (gameObject == null || transform == null || S_S_EditorMethods.IsThisOrListOrChildrenSelected(transform, null, 0))
		{ return; }

		//Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; //Ensures handles drawn wont be visible through walls.
		Color color = _selectedFillColour;
		color.a = Mathf.Max(color.a, 0.5f);

		using (new Handles.DrawingScope(color))
		{
			S_S_DrawingMethods.DrawSelectableHandle(position, gameObject, handleRadius);
		}
	}
}
#endif
