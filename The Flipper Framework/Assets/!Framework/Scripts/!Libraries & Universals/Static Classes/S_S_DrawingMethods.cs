using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_S_DrawingMethods 
{

	//
	//Gizmos
	//

	public static void DrawArrowHandle ( Color colour, Transform transform, float scale, bool isLocal ) {

		//An alpha under 0.1 means dont change from colour was already set to
		if (colour.a < 0.1f) { colour = Handles.color; }

		using (new Handles.DrawingScope(colour, isLocal ? transform.localToWorldMatrix : transform.worldToLocalMatrix))
		{

			//Get positions to make up arrow shape. Gizmo matrix may have been let to local or world, so respond accordingly.
			Vector3 middle = isLocal ? Vector3.zero: transform.position;
			Vector3 forwardFar = isLocal ? Vector3.forward * scale: transform.position + Vector3.forward * scale;
			Vector3 forwardSmall = isLocal ? Vector3.forward * scale * 0.3f: transform.position + Vector3.forward * scale * 0.3f;
			Vector3 right = isLocal ? Vector3.right * scale * 0.8f: transform.position + Vector3.right * scale * 0.8f;
			Vector3 left = isLocal ? -Vector3.right * scale * 0.8f : transform.position - Vector3.right * scale * 0.8f ;

			//Draw lines making up arrow. Remember that if in local space from a previous line, this should be called as isLocal so points are correct.
			Handles.DrawLine(middle, forwardFar);
			Handles.DrawLine(forwardSmall, right);
			Handles.DrawLine(forwardSmall, left);
			Handles.DrawLine(forwardFar, right);
			Handles.DrawLine(forwardFar, left);
		}

	}


	public static void DrawSelectableHandle (Vector3 handlePosition, GameObject targetObject) {
		// Draw the sphere handle at the specified position
		EditorGUI.BeginChangeCheck();
		Handles.SphereHandleCap(0, handlePosition, Quaternion.identity, 0.8f, EventType.Repaint);

		// Detect if the mouse is clicked on the handle
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			// Check if the mouse is over the handle (using HandleUtility.DistanceToCircle for a more precise check)
			if (HandleUtility.DistanceToCircle(handlePosition, 1f) < 1f)
			{
				// Select the object when the handle is clicked
				Selection.activeObject = targetObject;

				// Mark the event as handled
				Event.current.Use();
			}
		}
	}

}
