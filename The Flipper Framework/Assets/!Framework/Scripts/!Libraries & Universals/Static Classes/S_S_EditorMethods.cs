using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System;

public class S_S_EditorMethods : MonoBehaviour
{
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

	public static object FindFieldByName ( object obj, string inputName ) {
		if (obj == null || string.IsNullOrEmpty(inputName))
			return null;

		Type type = obj.GetType();

		FieldInfo field = type.GetField(inputName);
		object value = field.GetValue(obj);
		if (value == null)
			Debug.LogError("Could not find  " + inputName + "  in  " + obj);

		return value;
	}
}
