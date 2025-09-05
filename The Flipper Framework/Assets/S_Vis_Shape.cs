using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
public class S_Vis_Shape : S_Vis_Base
{
	public S_Vis_Shape () {
		_hasVisualisationScripted = true;
	}

	public float size = 3f;
	public S_EditorEnums.ShapeTypes Shape = S_EditorEnums.ShapeTypes.Sphere;

	public override void DrawGizmosAndHandles ( bool selected ) {
		Gizmos.color = selected ? _selectedOutlineColour : _normalOutlineColour;

		switch (Shape)
		{
			case S_EditorEnums.ShapeTypes.Sphere:
				Gizmos.DrawWireSphere(transform.position, size);
				break;
			case S_EditorEnums.ShapeTypes.Box:
				Gizmos.DrawWireCube(transform.position, Vector3.one * size);
				break;
		}
	}
}
#endif
