using SplineMesh;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Unity.Android.Gradle;

[ExecuteInEditMode]
public class S_AI_RhinoMaster : S_Vis_Base, ITriggerable
{
	[AsButton("Set All To Spline", "SetAllToSpline", null)]
	[SerializeField] bool SetAllToSplineButton;

	#region In Editor Fields
	public S_AI_RhinoMaster () {
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

	#endregion

	public S_AI_RhinoMaster _InheritRhinosFromThisOnActivation;
	[SerializeField] private float _distanceBetweenRails = 10;
	[SerializeField] private Vector2 _timeBetweenAttacks_ = new Vector2(2f,3f);
	[SerializeField] private Vector2 _timeBetweenJumps_ = new Vector2(5f,6f);

	private float _timeToNextAttack;
	private float _timeToNextJump;
	private float _timeSinceLastAttack;
	private float _timeSinceLastJump;

	private List<GameObject> _listOfAllRhinoObjects;
	private List<RhinoManaging> _listOfAllRhinos;
	private List<GameObject> _ListOfRhinosInfront = new List<GameObject>();
	private List<GameObject> _ListOfRhinosThatHaveShot = new List<GameObject>();
	private List<GameObject> _ListOfRhinosThatHaveJumped = new List<GameObject>();
	private bool _allInFrontOfPlayer;

	private GameObject _Player;
	private S_PlayerVelocity _PlayerVel;

	private void OnValidate () {
		if (!_RhinosToSpawn) { return; }

		//If the user just changed the _Rhinos list (E.G added to or removed from.
		if (_Rhinos != _RhinosBackup)
		{
			_rhinoArrayWasChanged = true; //Set a boolean to true so the code that needs doing is done in Update, rather than OnValidate.
		}
	}

	private void Start () {
		_listOfAllRhinoObjects = new List<GameObject>();
		_ListOfRhinosInfront = new List<GameObject>();
		_ListOfRhinosThatHaveShot = new List<GameObject>();
		_ListOfRhinosThatHaveJumped = new List<GameObject>();
		_listOfAllRhinos = new List<RhinoManaging>();

		AddRhinosFromArrayToLists(ref _Rhinos);

		SetUpNextAttack();
		SetUpNextJump();
	}

	public void TriggerObjectOn ( S_PlayerPhysics Player = null ) {
		_Player = Player.gameObject;
		_PlayerVel = Player._PlayerVelocity;

		S_Manager_LevelProgress.OnReset += EventReturnOnDeath;

		if (_InheritRhinosFromThisOnActivation)
		{
			AddRhinosFromArrayToLists(ref _InheritRhinosFromThisOnActivation._Rhinos);
		}
	}


	//Lists are easier to manage during gameplay, and if a master is getting rhinos from another master. they can be added.
	private void AddRhinosFromArrayToLists ( ref RhinoManaging[] Array ) {
		foreach (RhinoManaging RhinoManager in Array)
		{
			if (RhinoManager._Object.activeSelf)
			{
				_listOfAllRhinos.Add(RhinoManager);
				_listOfAllRhinoObjects.Add(RhinoManager._Object);
				RhinoManager._RailEnemyScript.OnFallBehindPlayer += EventARhinoFellBehind;
				RhinoManager._RailEnemyScript.OnGetInFrontOfPlayer += EventARhinoGotInFront;
				RhinoManager._Object.GetComponent<S_AI_Health>().OnDefeated += EventRhinoDefeated;
			}
		}
	}

	[ExecuteAlways]
	private void Update () {
		TrackingIfArrayWasChanged();
	}

	private void FixedUpdate () {
		_timeSinceLastAttack += Time.fixedDeltaTime;
		_timeSinceLastJump += Time.fixedDeltaTime;

		if(_ListOfRhinosThatHaveShot.Count >= _ListOfRhinosInfront.Count) { _ListOfRhinosThatHaveShot.Clear(); }

		//Only rhinos in front will attack and jump, to prevent player being confused.
		for (int i = 0 ; i < _ListOfRhinosInfront.Count ; i++)
		{
			if (_timeSinceLastAttack >= _timeToNextAttack)
			{
				if (!_ListOfRhinosThatHaveShot.Contains(_ListOfRhinosInfront[i]))
				{
					_ListOfRhinosThatHaveShot.Add(_ListOfRhinosInfront[i]);
					_ListOfRhinosInfront[i].GetComponent<S_AI_RhinoActions>().ReadyShot(_Player.transform, _PlayerVel);
					SetUpNextAttack();
				}
			}
		}
	}


	private void SetUpNextJump () {
		_timeSinceLastJump = 0;
		_timeToNextJump = UnityEngine.Random.Range(_timeBetweenJumps_.x, _timeBetweenJumps_.y);
	}

	private void SetUpNextAttack () {
		_timeSinceLastAttack = 0;
		_timeToNextAttack = UnityEngine.Random.Range(_timeBetweenAttacks_.x, _timeBetweenAttacks_.y);
	}

	public void EventARhinoGotInFront ( GameObject Rhino ) {
		_ListOfRhinosInfront.Add(Rhino);
		_allInFrontOfPlayer = false;
	}

	public void EventARhinoFellBehind ( GameObject Rhino ) {
		if (_ListOfRhinosInfront.Contains(Rhino)) { _ListOfRhinosInfront.Remove(Rhino); }

		_allInFrontOfPlayer = _ListOfRhinosInfront.Count == _listOfAllRhinoObjects.Count;
	}

	public void EventRhinoDefeated ( GameObject Rhino ) {
		if (_listOfAllRhinoObjects.Contains(Rhino))
		{
			_listOfAllRhinos.RemoveAt(_listOfAllRhinoObjects.IndexOf(Rhino));
			_listOfAllRhinoObjects.Remove(Rhino);
		}
		EventARhinoFellBehind(Rhino);
	}

	void EventReturnOnDeath ( object sender, EventArgs e ) {
		if (!gameObject) { return; }
		Start();

		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
	}

	#region inEditor

	//Adding or removing child rhinos if the array was changed.
	private void TrackingIfArrayWasChanged () {
		if (_rhinoArrayWasChanged && Application.isPlaying)
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
						Rhino._Object.name = Rhino._Object.name + (" (" + index + ")");

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
	public bool ArrayContainsThatRhinoMoreThanX ( GameObject Rhino, ref RhinoManaging[] array, int x ) {
		int howMany = 0;
		foreach (RhinoManaging Element in array)
		{
			if (Rhino == Element._Object) { howMany++; }
			if (howMany > x) { return true; }
		}

		return false;
	}

	//Called by a button field in editor.
	public void SetAllToSpline () {
		foreach (RhinoManaging Rhino in _Rhinos)
		{
			Rhino._RailEnemyScript.SetToSpline();
		}
	}

	#endregion

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
	[Tooltip("Used to track which rhino is on which rail in a set. 0 = middle rail, -1 = 1 rail to left, 1 = one rail to right")]
	public int _whichRail;
	[Tooltip("The child object which is an instance of the rhino prefab set at the top.")]
	public GameObject _Object;
	public S_RailEnemyData _RailEnemyData;

	[HideInInspector]
	public S_AI_RailEnemy _RailEnemyScript;
	[HideInInspector]
	public S_AI_RhinoActions _RhinoActions;
}
