using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ComponentModel;
using UnityEditor;
using SplineMesh;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class S_S_EditorMethods : MonoBehaviour
{

#if UNITY_EDITOR
	//Takes a string and converts it to align with variable naming conventions, allowing the human's input to not have to be 100%.
	//
	public static string TranslateStringToVariableName ( string input, S_EditorEnums.CasingTypes casing ) {

		if(input == "") { return ""; }

		//Uses System.Linq to remove any potential spaces from input, as variables can't have these.
		string noSpaces =  new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());


		//Adds an underscore to the start in case it was missed.
		if (noSpaces[0] != '_')
		{
			noSpaces = "_" + noSpaces.Substring(0);
		}


		//Gets the first letter or number in the string, as this would be when casing starts.
		char firstChar = noSpaces[0];
		int firstCharIndex = 0;
		while (!(char.IsLetter(firstChar) || char.IsNumber(firstChar)) & firstCharIndex < noSpaces.Length-1)
		{
			firstCharIndex++;
			firstChar = noSpaces[firstCharIndex];
		}

		//If set to a specific casing, aligns the input to this casing by replacing the first character.
		string start = noSpaces.Substring(0, firstCharIndex);
		string end = noSpaces.Substring(firstCharIndex + 1);
		switch (casing)
		{
			case S_EditorEnums.CasingTypes.PascalCase:
				noSpaces = start + char.ToUpper(firstChar) + end;
				break;
			case S_EditorEnums.CasingTypes.camelCase:
				noSpaces = start + char.ToLower(firstChar) + end;
				break;
		}

		if (input != noSpaces)
			Debug.LogWarning("Converted  " +input+  "  to  " +noSpaces);

		return noSpaces;
	}


	//Depending on the the type of value, some values as strings will contain unnecesary information. E.G, gameObjects will add on (UnityEngine.GameObject). This removes brackets and their contents.
	public static string CleanBracketsInString ( string input ) {

		int bracketStart = 0;
		int bracketEnd = 0;

		for (int i = 0 ; i < input.Length ; i++)
		{
			if (input[i] == '(') { bracketStart = i; }
			else if (input[i] == ')') { bracketEnd = i; }
		}

		if(bracketStart > 0 && bracketEnd > 0)
			input = input.Substring(0, bracketStart) + input.Substring(bracketEnd+1);

		return input;
	}

	//Takes an object and a string, then finds the value of a variable/field with that name, in said object.
	public static object FindFieldByName ( object obj, string inputName, string structName = "" ) {
		if (obj == null || string.IsNullOrEmpty(inputName))
			return null;

		string temp = inputName;
		//If given  a struct name, find that struct first.
		if(structName != "") inputName = structName;

		Type type = obj.GetType();

		FieldInfo field = type.GetField(inputName);
		if (field == null) return null;

		object value = field.GetValue(obj);

		//If given a struct name, search the obtained struct for the required field.
		if (structName != "")
		{
			value = FindFieldByName(value, temp, "");
		}

		return value;
	}

	// Check if a given GameObject is part of the current selection
	public static bool IsSelected ( GameObject gameObject ) {
		return Array.Exists(Selection.gameObjects, obj => obj == gameObject);
	}

	//Checks if a gameObject is seleted, or one of its children or any from an aditional list
	public static bool IsThisOrReferenceSelected ( Transform transform ,GameObject[] ObjectList = null) {
	
		if (S_S_EditorMethods.IsSelected(transform.gameObject)) { return true; }
	
		//Goes through list to see if any are selected.
		if(ObjectList != null) for (int i = 0 ; i < ObjectList.Length ; i++) {if (IsSelected(ObjectList[i])){ return true;}}
		//Goes through children to see if any are selected
		for (int i = 0 ; i < transform.childCount ; i++){ if (IsSelected(transform.GetChild(i).gameObject)){return true;}}

		//If nothing found, then false.
		return false;
	}

	public static void FaceSceneViewCamera (Transform transform, float offset) {
		if(Application.isPlaying) return;

		SceneView sceneView = SceneView.lastActiveSceneView;
		if (sceneView != null && sceneView.camera != null)
		{
			// Get the position of the Scene View camera
			Vector3 cameraPosition = sceneView.camera.transform.position;

			// Make the object face the camera
			//transform.LookAt(cameraPosition);
			Vector3 camDirection = cameraPosition - transform.position;
			camDirection = Vector3.RotateTowards(camDirection, -camDirection, offset * Mathf.Deg2Rad, 0);
			transform.rotation = Quaternion.LookRotation(camDirection);
		}
	}


	//
	//Gizmos
	//

	public static void DrawArrowHandle (Color colour, Transform transform, float scale, bool isLocal) {

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

	public static GameObject FindChild ( GameObject parentObject, string newObjectName ) {
		//Searches for a child of this name
		var childTransform = parentObject.transform.Find(newObjectName);
		
		if (childTransform == null) return null;
		return childTransform.gameObject;
	}


	public static GameObject FindOrCreateChild (GameObject ParentObject ,string newObjectName, Type[] AddComponents, bool replace = false, GameObject InstantiateFrom = null ) {

		//Searches for a child of this name
		var childTransform = ParentObject.transform.Find(newObjectName);
		GameObject childObject;

		//Creates a GameObject with the given components
		if (childTransform == null)
		{
			childObject = CreateChild();
		}
		else
		{
			if (!replace) { childObject = childTransform.gameObject; }
			else
			{
				GameObject.DestroyImmediate(childTransform.gameObject);

				childObject = CreateChild();
			}
		}

		childObject.tag = ParentObject.tag;
		childObject.layer = ParentObject.layer;

		return childObject;

		GameObject CreateChild () {

			if (InstantiateFrom != null) { return CreateChildFromBase(ParentObject, InstantiateFrom, newObjectName, AddComponents); }

			else { return CreateChildFromScratch(ParentObject, newObjectName, AddComponents); }
		}
	}

	public static GameObject CreateChildFromScratch ( GameObject ParentObject, string newObjectName, Type[] AddComponents ) {

		GameObject childObject = UOUtility.Create(newObjectName, ParentObject, AddComponents);
		childObject.isStatic = true;

		return childObject;
	}

	public static GameObject CreateChildFromBase ( GameObject ParentObject, GameObject InstantiateFrom, string newObjectName, Type[] AddComponents ) {
		
		//Create a clone of the given base object and set it as a child.
		GameObject childObject = Instantiate(InstantiateFrom, ParentObject.transform);
		childObject.name = newObjectName;
		childObject.transform.parent = ParentObject.transform;
		childObject.isStatic = true;

		//Add the wanted components as long as the object doesn't already have them.
		for (int i = 0 ; i < AddComponents.Length ; i++)
		{
			if (!childObject.GetComponent(AddComponents[i])) { childObject.AddComponent(AddComponents[i]); }
		}

		return childObject;
	}
#endif
}
