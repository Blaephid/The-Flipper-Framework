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
using UnityEngine.InputSystem.HID;
using System.Diagnostics;
using System.Xml.Linq;

public enum DataTypes{
	Float,
	Boolean,
	Vector3,
	Vector2,
	String,
	Int,
}

public struct FieldAndValue
{
	public FieldInfo field;
	public object value;
}

public class S_S_Editor : MonoBehaviour
{

	//Takes a string and converts it to align with variable naming conventions, allowing the human's input to not have to be 100%.
	//
	public static string TranslateStringToVariableName ( string input, S_EditorEnums.CasingTypes casing = S_EditorEnums.CasingTypes.Either ) {

		if (input == "") { return ""; }

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
		while (!(char.IsLetter(firstChar) || char.IsNumber(firstChar)) & firstCharIndex < noSpaces.Length - 1)
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
			UnityEngine.Debug.LogWarning("Converted  " + input + "  to  " + noSpaces);

		return noSpaces;
	}


	//Depending on the the type of value, some values as strings will contain unnecesary information. E.G, gameObjects will add on (UnityEngine.GameObject). This returns the string without brackets, and each of the brackets removed.
	public static List<string> CleanBracketsInString ( string input, char bracket1 = '(', char bracket2 = ')', int sizeOfBracket = 6, bool onlyRemoveNumberedBrackets = false ) {

		int bracketStart = -1;
		int lastBracketStart = -1;
		int bracketEnd = 0;

		List<string> returnStrings = new List<string>() {input};

		for (int i = 0 ; i < input.Length ; i++)
		{
			//Checks for different types of brackets, (, [, {
			if (input[i] == bracket1) 
			{ 
				if (bracketStart != -1) { lastBracketStart = bracketStart; }
				bracketStart = i;
			}
			else if (input[i] == bracket2) { bracketEnd = i; }

			//If there are brackets containing more than how many character specified (so can vary on dominance of brackets)
			if (bracketStart >= 0 && bracketEnd > 0 & Mathf.Abs(bracketEnd - bracketStart) > sizeOfBracket)
			{
				//Remove brackets
				string newInput = input.Substring(0, bracketStart) + input.Substring(bracketEnd + 1);
				string bracketInput = input.Substring(bracketStart, bracketEnd - bracketStart + 1);

				//If only removing brackets with numbers, and this doesn't, then keek going.
				if(onlyRemoveNumberedBrackets && !bracketInput.Any(char.IsDigit))
				{
					bracketStart = lastBracketStart; bracketEnd = 0;
					continue;
				}

				//Call again without this bracket, to clean again.
				List<string> additionalBrackets = CleanBracketsInString(newInput, bracket1, bracket2, sizeOfBracket, onlyRemoveNumberedBrackets);

				//This returns the backets, and passes up to the previous call, which then adds theirs on too, rinse and repeat.
				if (bracketInput != "") { returnStrings.Add(bracketInput); }
				for (int sub = 1 ; sub < additionalBrackets.Count ; sub++) { returnStrings.Add(additionalBrackets[sub]); }

				input = additionalBrackets[0];
			}
		}
		returnStrings[0] = input;
		return returnStrings;
	}

	//Takes an object and a string, then finds the value of a variable/field with that name, in said object.
	public static FieldAndValue FindFieldByName ( object obj, string inputName, string structName = "" ) {
		if (obj == null || string.IsNullOrEmpty(inputName))
			return new FieldAndValue();

		string temp = inputName;
		//If given  a struct name, find that struct first.
		if (structName != "") inputName = structName;

		Type type = obj.GetType();

		FieldInfo field = type.GetField(inputName);
		if (field == null) return new FieldAndValue();

		object value = field.GetValue(obj);

		//If given a struct name, search the obtained struct for the required field.
		if (structName != "")
		{
			value = FindFieldByName(value, temp, "").value;
		}

		return new FieldAndValue() {field = field, value = value};
	}

#if UNITY_EDITOR
	// Check if a given GameObject is part of the current selection
	public static bool IsSelected ( GameObject gameObject ) {
		return Array.Exists(Selection.gameObjects, obj => obj == gameObject);
	}

	public static bool IsHidden (GameObject gameObject) {
		return SceneVisibilityManager.instance.IsHidden(gameObject);
	}

	//Checks if a gameObject is seleted, or one of its children or any from an aditional list
	public static bool IsThisOrListOrChildrenSelected ( Transform transform, GameObject[] ObjectList = null, int childrenLevels = 1 ) {

		if (S_S_Editor.IsSelected(transform.gameObject)) { return true; }

		//Goes through list to see if any are selected.
		if (ObjectList != null) for (int i = 0 ; i < ObjectList.Length ; i++) { if (IsSelected(ObjectList[i])) { return true; } }

		//Checks if any immediate children are selected
		if (childrenLevels > 0 && transform.childCount > 0)
			if (IsChildSelected(transform)) return true;

		//Goes through multiple levels to see if any are selected (E.g. Children, Children's children, Children's Children's children.)
		if (AreChildrenSelected(transform, childrenLevels)) { return true; }

		//If nothing found, then false.
		return false;
	}

	public static bool AreChildrenSelected ( Transform parent, int depth ) {
		// Base case: Stop if depth reaches 0
		if (depth <= 0 || parent == null)
			return false;

		// Iterate through each child
		for (int i = 0 ; i < parent.childCount ; i++)
		{
			Transform child = parent.GetChild(i);
			if (IsChildSelected(child)) return true;
			else
			{
				// Recursively traverse the child's children, reducing depth
				if (AreChildrenSelected(child, depth - 1))
				{ return true; }
			}
		}
		return false;
	}

	public static bool IsChildSelected ( Transform transform ) {
		//Goes through children to see if any are selected
		for (int i = 0 ; i < transform.childCount ; i++) { if (IsSelected(transform.GetChild(i).gameObject)) { return true; } }

		//If nothing found, then false.
		return false;
	}

	public static void FaceSceneViewCamera ( Transform transform, float offset ) {

		if (Application.isPlaying) return;

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
#endif

	public static GameObject FindChild ( GameObject parentObject, string newObjectName ) {
		//Searches for a child of this name
		var childTransform = parentObject.transform.Find(newObjectName);

		if (childTransform == null) return null;
		return childTransform.gameObject;
	}


	public static GameObject FindOrCreateChild ( GameObject ParentObject, string newObjectName, Type[] AddComponents, bool replace = false, GameObject InstantiateFrom = null ) {

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
			//if (!childObject.GetComponent(AddComponents[i])) { childObject.AddComponent(AddComponents[i]); }
			AddComponentIfMissing(childObject, AddComponents[i]);
		}

		return childObject;
	}


	public static void AddComponentIfMissing ( GameObject Target, Type Component ) {
		if (!Target.GetComponent(Component)) { Target.AddComponent(Component); }
	}

	public static void FindAndRemoveComponent ( GameObject Target, Type Component ) {
		if (!Target.GetComponent(Component)) { return; }

		DestroyImmediate(Target.GetComponent(Component));

	}

	public static bool CheckCallerMethodsFor ( string callerMethodName ) {

		StackTrace stackTrace = new StackTrace();
		for (int i = 0 ; i < stackTrace.FrameCount ; i++)
		{
			var method = stackTrace.GetFrame(i).GetMethod();
			if (method.Name == callerMethodName)
			{
				return true;
			}
		}
		return false;
	}

#if UNITY_EDITOR
	public static void FindObjectAndSetActive ( string name, bool set, Transform Parent = null ) {
		GameObject Target;
		if (Parent)
		{
			Transform TargetT = Parent.Find(name);
			Target = TargetT ? TargetT.gameObject : null;
		}
		else
			Target = GameObject.Find(name);

		EditorApplication.delayCall += () =>
		{
			if (!Target) { return; }
			Target.SetActive(set);
		};
	}
#endif
}
