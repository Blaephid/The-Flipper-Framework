using SplineMesh;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;

[ExecuteInEditMode]
public class S_AI_RhinoMaster : S_Vis_Base, ITriggerable
{
	[AsButton("Set All To Spline", "SetAllToSpline", null)]
	[SerializeField] bool SetAllToSplineButton;

	#region In Editor Fields
#if UNITY_EDITOR
	public S_AI_RhinoMaster () {
		_hasVisualisationScripted = true;
		_selectedOutlineColour = Color.white;
		_selectedOutlineColour.a = 0.8f;
		_normalOutlineColour = Color.white;
		_normalOutlineColour.a = 0.3f;
	}
#endif

	[SerializeField, BaseColour(0.8f,0.8f,0.8f,1f)]
	private GameObject _RhinosToSpawn;
	[SerializeField, BaseColour(0.8f,0.8f,0.8f,1f)]
	private Mesh _VisualiseMesh;

	[SerializeField]
	private RhinoManaging[] _Rhinos;
	private RhinoManaging[] _RhinosBackup;

	private bool _rhinoArrayWasChanged;

	#endregion

	public S_AI_RhinoMaster _InheritRhinosFromThisOnActivation;
	[SerializeField] private float _distanceBetweenRails = 10;
	[SerializeField] private Vector2 _timeBetweenAttacks_ = new Vector2(2f,3f);
	[SerializeField] private Vector2 _timeBetweenHops_ = new Vector2(5f,6f);
	[SerializeField] private float  _timeBeforeFirstHop_ = 2;
	[SerializeField] private float  _timeBeforeFirstAttack_ = 2;

	private float _timeToNextAttack;
	private float _timeToNextHop;
	private float _timeSinceLastAttack;
	private float _timeSinceLasHop;

	private List<GameObject> _listOfAllRhinoObjects;
	private List<RhinoManaging> _listOfAllRhinos;
	private List<GameObject> _ListOfRhinosInfront = new List<GameObject>();
	private List<GameObject> _ListOfRhinosThatHaveShot = new List<GameObject>();
	private List<GameObject> _ListOfRhinosThatHaveHopped = new List<GameObject>();

	private Transform _PlayerCenter;
	private S_PlayerVelocity _PlayerVel;

#if UNITY_EDITOR

	private void OnValidate () {
		if (!_RhinosToSpawn || !gameObject.IsPrefabInstance()) { return; }

		if (_RhinosBackup == null)
		{ _RhinosBackup = _Rhinos; }


		//If the user just changed the _Rhinos list (E.G added to or removed from.
		if (_Rhinos != _RhinosBackup)
		{
			_rhinoArrayWasChanged = true; //Set a boolean to true so the code that needs doing is done in Update, rather than OnValidate.
		}
		else
			foreach(RhinoManaging Rhino in _Rhinos)
			{
				Rhino._RailEnemyScript = Rhino._Object.GetComponent<S_AI_RailEnemy>();
			}

	}
#endif

	private void Start () {
		if (!Application.isPlaying) { return; } //Ensure these aren't called when returning to edit mode.

		_listOfAllRhinoObjects = new List<GameObject>();
		_ListOfRhinosInfront = new List<GameObject>();
		_ListOfRhinosThatHaveShot = new List<GameObject>();
		_ListOfRhinosThatHaveHopped = new List<GameObject>();
		_listOfAllRhinos = new List<RhinoManaging>();

		SetUpNextAttack(_timeBeforeFirstAttack_);
		SetUpNextHop(_timeBeforeFirstHop_);
	}

	public void TriggerObjectOnce ( S_PlayerPhysics Player = null ) {
		_PlayerCenter = Player._CenterOfMass;
		_PlayerVel = Player._PlayerVelocity;

		S_Manager_LevelProgress.OnReset += EventReturnOnDeath;

		AddRhinosFromArrayToLists(ref _Rhinos);
		if (_InheritRhinosFromThisOnActivation)
		{
			AddRhinosFromArrayToLists(ref _InheritRhinosFromThisOnActivation._Rhinos);
		}
	}


	//Lists are easier to manage during gameplay, and if a master is getting rhinos from another master. they can be added.
	private void AddRhinosFromArrayToLists ( ref RhinoManaging[] Array ) {
		foreach (RhinoManaging RhinoManager in Array)
		{
			if (RhinoManager._Object.activeSelf && !_listOfAllRhinos.Contains(RhinoManager))
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
#if UNITY_EDITOR
		TrackingIfArrayWasChanged();
#endif
	}

	private void FixedUpdate () {
		_timeSinceLastAttack += Time.fixedDeltaTime;
		_timeSinceLasHop += Time.fixedDeltaTime;

		bool tryHop = _timeSinceLasHop >= _timeToNextHop;
		bool haveAnyHopped = false;

		bool tryShoot = _timeSinceLastAttack >= _timeToNextAttack;
		bool haveAnyShot = false;

		//Only rhinos in front will attack and jump, to prevent player being confused.
		for (int i = 0 ; i < _ListOfRhinosInfront.Count ; i++)
		{
			GameObject Rhino = _ListOfRhinosInfront[i];
			if (tryShoot && !haveAnyShot)
			{
				if (!_ListOfRhinosThatHaveShot.Contains(Rhino))
				{
					S_AI_RhinoActions Action = Rhino.GetComponent<S_AI_RhinoActions>();
					if (Action.ReadyShot(_PlayerCenter, _PlayerVel))
					{
						_ListOfRhinosThatHaveShot.Add(Rhino);
						//StartCoroutine(AddToListOfShotAfterShotDelay(Rhino, Action._timeToReadyShot));
						SetUpNextAttack();
						haveAnyShot = true;
					}
				}
			}

			if (tryHop && !haveAnyHopped)
			{
				if (!_ListOfRhinosThatHaveHopped.Contains(Rhino))
				{
					S_AI_RhinoActions Action = Rhino.GetComponent<S_AI_RhinoActions>();
					if (Action.CanSwitch(_distanceBetweenRails))
					{
						haveAnyHopped = true;
						SetUpNextHop();
					}
				}
			}
		}

		//If all rhinos have shot, or none of the ones that haven't are able to, reset.
		if (!haveAnyShot && tryShoot && _ListOfRhinosThatHaveShot.Count > 0)
		{ _ListOfRhinosThatHaveShot.Clear(); }

		//If none had the safety to hop, then allow ones that have already hopped to try again.
		if (!haveAnyHopped && tryHop && _ListOfRhinosThatHaveHopped.Count > 0)
		{ _ListOfRhinosThatHaveHopped.Clear(); }
	}


	private void SetUpNextHop ( float priority = 0 ) {
		_timeSinceLasHop = 0;
		_timeToNextHop = priority > 0 ? priority : UnityEngine.Random.Range(_timeBetweenHops_.x, _timeBetweenHops_.y);
	}

	private void SetUpNextAttack ( float priority = 0 ) {
		_timeSinceLastAttack = 0;
		_timeToNextAttack = priority > 0 ? priority : UnityEngine.Random.Range(_timeBetweenAttacks_.x, _timeBetweenAttacks_.y);
	}

	public void EventARhinoGotInFront ( GameObject Rhino ) {
		_ListOfRhinosInfront.Add(Rhino);

	}

	public void EventARhinoFellBehind ( GameObject Rhino ) {
		if (_ListOfRhinosInfront.Contains(Rhino)) { _ListOfRhinosInfront.Remove(Rhino); }
	}

	public void EventRhinoDefeated ( GameObject Rhino, S_AI_Health HealthScript ) {
		if (_listOfAllRhinoObjects.Contains(Rhino))
		{
			ClearEventConnections(Rhino, HealthScript);

			_listOfAllRhinos.RemoveAt(_listOfAllRhinoObjects.IndexOf(Rhino));
			_listOfAllRhinoObjects.Remove(Rhino);
		}
		EventARhinoFellBehind(Rhino);
	}

	private void ClearEventConnections ( GameObject Rhino, S_AI_Health HealthScript ) {
		Rhino.GetComponent<S_AI_RailEnemy>().OnFallBehindPlayer -= EventARhinoFellBehind;
		Rhino.GetComponent<S_AI_RailEnemy>().OnGetInFrontOfPlayer -= EventARhinoGotInFront;
		HealthScript.OnDefeated -= EventRhinoDefeated;
	}

	void EventReturnOnDeath ( object sender, EventArgs e ) {

		for (int i = 0 ; i < _listOfAllRhinoObjects.Count ; i++)
		{
			GameObject Rhino = _listOfAllRhinoObjects[i];
			EventRhinoDefeated(Rhino, Rhino.GetComponent<S_AI_Health>());
		}

		Start();

		S_Manager_LevelProgress.OnReset -= EventReturnOnDeath;
	}

#if UNITY_EDITOR
	#region inEditor

	//Adding or removing child rhinos if the array was changed.
	private void TrackingIfArrayWasChanged () {
		if (_rhinoArrayWasChanged && !Application.isPlaying)
		{
			//If added a new one, add a new rhino child object to this object
			if (_Rhinos.Length > _RhinosBackup.Length)
			{
				int index = _Rhinos.Length;
				for (int i = _Rhinos.Length - 1 ; i >= 0 ; i--)
				{
					RhinoManaging Rhino = _Rhinos[i];
					//If this element doesn't haven an object yet, spawn one. Remember adding to arrays in editor duplicates values so also create a new one if object used more than once.
					if (Rhino._Object == null || ArrayContainsThatRhinoMoreThanX(Rhino._Object, ref _Rhinos, 1))
					{
						Rhino._Object = PrefabUtility.InstantiatePrefab(_RhinosToSpawn, transform) as GameObject;
						Rhino._Object.name = Rhino._Object.name + (" (" + index + ")");

						Rhino._RailEnemyScript = Rhino._Object.GetComponent<S_AI_RailEnemy>();

						index--;
					}
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
			if (_Rhinos[i]._Object)
				Gizmos.DrawLine(transform.position, _Rhinos[i]._Object.transform.position);
		}
	}

	public override void CallCustomSceneGUI () {
		VisualiseWithSelectableHandle(transform.position, 2f);
	}
	#endregion

#endif
}

[Serializable]
public class RhinoManaging
{
	[Tooltip("Used to track which rhino is on which rail in a set. 0 = middle rail, -1 = 1 rail to left, 1 = one rail to right")]
	public int _whichRail;
	[Tooltip("The child object which is an instance of the rhino prefab set at the top.")]
	public GameObject _Object;

	[HideInInspector]
	public S_AI_RailEnemy _RailEnemyScript;
	[HideInInspector]
	public S_AI_RhinoActions _RhinoActions;
}
