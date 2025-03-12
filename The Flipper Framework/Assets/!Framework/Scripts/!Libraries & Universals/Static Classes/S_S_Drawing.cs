using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_S_Drawing
{

	//
	//Gizmos
	//

	public static void DrawArrowHandle ( Color colour, Transform transform, float scale, bool isLocal, Vector3 direction ) {

		//An alpha under 0.1 means dont change from colour was already set to
		if (colour.a < 0.1f) { colour = Handles.color; }

		using (new Handles.DrawingScope(colour, isLocal ? transform.localToWorldMatrix : Matrix4x4.identity))
		{
			Vector3 relativeFowards = !isLocal ? transform.rotation * direction : direction;
			Vector3 relativeUp =  !isLocal ? transform.rotation * Vector3.up : Vector3.up;
			Vector3 relativeRight = Vector3.Cross(relativeUp,relativeFowards);

			//Get positions to make up arrow shape. Gizmo matrix may have been let to local or world, so respond accordingly.
			Vector3 middle = isLocal ? Vector3.zero : transform.position;
			Vector3 forwardFar = middle + relativeFowards * scale;
			Vector3 forwardSmall = middle + relativeFowards * scale * 0.3f;
			Vector3 right = middle + relativeRight * scale * 0.8f;
			Vector3 left = middle - relativeRight * scale * 0.8f;

			//Draw lines making up arrow. Remember that if in local space from a previous line, this should be called as isLocal so points are correct.
			Handles.DrawLine(middle, forwardFar);
			Handles.DrawLine(forwardSmall, right);
			Handles.DrawLine(forwardSmall, left);
			Handles.DrawLine(forwardFar, right);
			Handles.DrawLine(forwardFar, left);
		}

	}

	public static void DrawCubeHandle ( Color colour, Transform transform, float radius, bool isLocal ) {
		//An alpha under 0.1 means dont change from colour was already set to
		if (colour.a < 0.1f) { colour = Handles.color; }

		using (new Handles.DrawingScope(colour, isLocal ? transform.localToWorldMatrix : Matrix4x4.identity))
		{
			Handles.DrawWireCube(isLocal ? Vector3.zero : transform.position, Vector3.one * radius);
		}
	}



	public static void DrawSelectableHandle ( Vector3 handlePosition, GameObject targetObject, float size = 1 ) {
		Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; //Ensures handles drawn wont be visible through walls.

		// Draw the sphere handle at the specified position
		EditorGUI.BeginChangeCheck();
		Handles.SphereHandleCap(0, handlePosition, Quaternion.identity, size, EventType.Repaint);

		// Detect if the mouse is clicked on the handle
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			// Check if the mouse is over the handle (using HandleUtility.DistanceToCircle for a more precise check)
			if (HandleUtility.DistanceToCircle(handlePosition, size * 0.7f) < 0.5f)
			{
				if (IsHandleBlocked(handlePosition, targetObject)) { return; }

				// Select the object when the handle is clicked
				Selection.activeObject = targetObject;

				// Mark the event as handled
				Event.current.Use();
			}
		}
		EditorGUI.EndChangeCheck();
	}


	public static bool IsHandleBlocked ( Vector3 handlePosition, GameObject targetObject ) {
		// Convert mouse position to a ray
		Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		RaycastHit[] hits = Physics.RaycastAll(mouseRay, Vector3.Distance(mouseRay.origin, handlePosition) + 2);

		for (int hit = 0 ; hit < hits.Length ; hit++)
		{
			// If the ray hit something else. This allows hitting the collider of itself to be ignored.
			if (hits[hit].collider.gameObject != targetObject)
			{
				return true;
			}
		}
		//If nothing was in the way
		return false;
	}

}
