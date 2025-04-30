using SplineMesh;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Unity.Android.Gradle;

[ExecuteInEditMode]
public class S_AI_RhinoMaster : S_Vis_Base
{
	[AsButton("Set All To Spline", "SetAllToSpline", null)]
	[SerializeField] bool SetAllToSplineButton;


	public S_AI_RhinoMaster() {
		_hasVisualisationScripted = true;
		_selectedOutlineColour = Color.white;
		_selectedOutlineColour.a = 0.8f;
		_normalOutlineColour = Color.white;
		_normalOutlineColour.a = 0.3f;
	}

	[SerializeField, BaseColour(0.8f,0.8f,0.8f,1f)]
	private GameObject _RhinosToSpawn;
	[SerializeField, BaseColour(0.8f,0.8f,0.8f,1f)]
	private Mesh _VisualiseMesh;

	[SerializeField]
	private RhinoManaging[] _Rhinos;
	[SerializeField, HideInInspector]
	private RhinoManaging[] _RhinosBackup;

	private bool _rhinoArrayWasChanged;

	private void OnValidate () {
		if (!_RhinosToSpawn) { return; }

		//If the user just changed the _Rhinos list (E.G added to or removed from.
		if (_Rhinos != _RhinosBackup)
		{
			_rhinoArrayWasChanged = true; //Set a boolean to true so the code that needs doing is done in Update, rather than OnValidate.
		}
	}

	[ExecuteAlways]
	private void Update () {
		if (_rhinoArrayWasChanged)
		{
			//If added a new one, add a new rhino child object to this object
			if (_Rhinos.Length > _RhinosBackup.Length)
			{
				int index = _Rhinos.Length;
				foreach (RhinoManaging Rhino in _Rhinos)
				{
					//If this elements doesn't haven an object yet, spawn one. Remember adding to arrays in editor duplicates values so also create a new one if object used more than once.
					if (Rhino._Object == null || ArrayContainsThatRhinoMoreThanX(Rhino._Object, ref _Rhinos, 1))
					{
						Rhino._Object = PrefabUtility.InstantiatePrefab(_RhinosToSpawn, transform) as GameObject;
						Rhino._Object.name = Rhino._Object.name + (" (" +index + ")");

						Rhino._RailEnemyScript = Rhino._Object.GetComponent<S_AI_RailEnemy>();
						Rhino._RailEnemyData = Rhino._RailEnemyScript._Data; //So the rhino values can be set from here.

						index--;
					};
				}
			}
			//If removed a rhino from the array, find which one and delete the corresponding child object.
			else if (_Rhinos.Length < _RhinosBackup.Length)
			{
				foreach (RhinoManaging Rhino in _RhinosBackup)
				{
					if (!ArrayContainsThatRhinoMoreThanX(Rhino._Object, ref _Rhinos, 0))
					{
						S_S_Editor.DestroyFromOnValidate(Rhino._Object);
					}
				}
			}

			foreach (RhinoManaging Rhino in _Rhinos)
			{
				Rhino._RailEnemyScript._Data = Rhino._RailEnemyData; //Takes changes on each rhino into this array
			}

			foreach (RhinoManaging Rhino in _Rhinos)
			{
				Rhino._RailEnemyScript._Data = Rhino._RailEnemyData; //Applies changes to rhinos in this array to that rhino.
			}

			_RhinosBackup = _Rhinos; //So changes to the array can be tracked.
			_rhinoArrayWasChanged = false;
		}
	}

	//Take one of the arrays of rhinos and check if that object is in more than x of the elements. Allows removal of unused ones, and replacement of ones mused multiple times.
	public bool ArrayContainsThatRhinoMoreThanX(GameObject Rhino, ref RhinoManaging[] array, int x ) {
		int howMany = 0;
		foreach(RhinoManaging Element in array)
		{
			if(Rhino ==Element._Object) { howMany++; }
			if(howMany > x) { return true; }
		}

		return false;
	}

	public void SetAllToSpline () {
		foreach(RhinoManaging Rhino in _Rhinos)
		{
			Rhino._RailEnemyScript.SetToSpline();
		}
	}

	#region gizmos

	public override void DrawGizmosAndHandles ( bool selected ) {
		Gizmos.color = selected ? _selectedOutlineColour : _normalOutlineColour;

		Gizmos.DrawWireMesh(_VisualiseMesh, transform.position, transform.rotation, Vector3.one * 300);

		for (int i = 0 ; i < _Rhinos.Length ; i++)
		{
			Gizmos.DrawLine(transform.position, _Rhinos[i]._Object.transform.position);
		}
	}

	public override void CallCustomSceneGUI () {
		VisualiseWithSelectableHandle(transform.position, 2f);
	}
	#endregion
}

[Serializable]
public class RhinoManaging
{
	public GameObject _Object;
	[HideInInspector]
	public S_AI_RailEnemy _RailEnemyScript;
	public S_RailEnemyData _RailEnemyData;
}
