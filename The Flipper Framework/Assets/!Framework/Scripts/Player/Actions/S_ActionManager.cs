using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using Unity.VisualScripting;
using templates;

public class S_ActionManager : MonoBehaviour
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_PlayerPhysics		_PlayerPhys;
	private S_CharacterTools		_Tools;
	private S_Handler_HealthAndHurt	_HealthAndHurt;

	//Action Scrips, These ones are required, any others will be handled through the interface list.
	[Header("Actions")]
	public S_Action00_Default _ActionDefault;
	public S_Action04_Hurt _ActionHurt;

	//Keeps track of the generated objects that handle different components. This is to prevent having all components on one object.
	public GameObject _ObjectForActions;
	public GameObject _ObjectForSubActions;
	public GameObject _ObjectForInteractions;
	#endregion

	// Trackers
	#region trackers
	//Tracking states in game
	public S_S_ActionHandling.PrimaryPlayerStates	_whatCurrentAction = S_S_ActionHandling.PrimaryPlayerStates.None;
	public S_S_ActionHandling.SubPlayerStates	_whatSubAction;
	public S_GeneralEnums.PlayerAttackStates       _whatCurrentAttack;
	public S_S_ActionHandling.PrimaryPlayerStates	_whatPreviousAction { get; set; }

	[HideInInspector]
	public bool         _canChangeActions = true;	//All StartActions should check this, and return if its false, unless they are set to overwrite this.

	[HideInInspector]
	public List<float>                            _listOfSpeedOnPaths = new List<float>();	//Certain actions will move the player along the spline, this will be used to track the speed for any actions that do so. It is used as a list rather than a singular as it will allow speeds to be added and removed with the action, then the most recent is the only one used.

	//Actions
	public List<S_Structs.MainActionTracker>	_MainActions; //This list of structs will cover each action currently available to the player (set in inspector), along with what actions it can enter through input or situation.	
	private S_Structs.MainActionTracker	_currentAction; //Which struct in the above list is currently active.
	private List<IAction> _AllActions = new List<IAction>();


	//Inspector
#if UNITY_EDITOR
	public S_O_CustomInspectorStyle		_InspectorTheme; // Will decide the apperance in the inspector.
#endif
	public S_S_ActionHandling.PrimaryPlayerStates                _addState; //Used only by the inspector in order to add states for other states to transition into.

	//Specific action trackers. Mainly used for external scripts to find (like enemies).
	[HideInInspector]
	public float        _charge;                                //Used by SpinCharge and DropCharge.
	[HideInInspector]
	public bool         _isAirDashAvailable = true; //Govers whether homing attacks and jump dashes can be performed.
	[HideInInspector]
	public int          _bounceCount;		//Tracks the number of bounces before landing are performed.
	[HideInInspector]
	public float        _actionTimeCounter;		//Used by multiple different actions to track how long has been in it.
	[HideInInspector]
	public int          _jumpCount;                   //Tracks how many jumps have been performed before landing. Will be used in handling multi jumps.
	[HideInInspector]
	public float	_dashDelayCounter;            //Used by homing attacks and jump dashes to set as able or not.
	[HideInInspector]
	public Vector3      _jumpAngle;		//Set externally, and in the jump action, may be used to choose the angle (E.G., Wall Climbing sets this, and if Jump detects that's the current state (enum), uses this.
	[HideInInspector]
	public Vector3      _dashAngle;         //Same as above but for the jumpDash action.
	[HideInInspector]
	public float       _speedBeforeAction; //Used by some actions like homing for previous speed to be accessible. Set to 0 on switch.
	[HideInInspector]
	public Vector3 _currentTargetPosition; //Used for actions like the Homing Attack that focus in on a specfic point. Can be used to find if an object is currently the target by comparing its own position to this.

	//Can perform actions

	//The bellow are all temporarily locked under certain situations, like using a spring.
	[HideInInspector]
	public bool         _areAirActionsAvailable = true;
	private int             _framesAirActionsLockedFor;

	[HideInInspector]
	public bool         _isPaused;
	#endregion

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Awake () {

		if (_Tools != null) return;

		//Assigning
		_Tools =		GetComponentInParent<S_CharacterTools>();
		_ActionDefault =	GetComponentInChildren<S_Action00_Default>();
		_ActionHurt =	GetComponentInChildren<S_Action04_Hurt>();
		_PlayerPhys =	_Tools.GetComponent<S_PlayerPhysics>();
		_HealthAndHurt =	_Tools.GetComponent<S_Handler_HealthAndHurt>();

		_AllActions.Clear();

		//Go through each struct and assign/add the scripts linked to that enum.
		for (int i = 0 ; i < _MainActions.Count ; i++)
		{
			S_Structs.MainActionTracker action = _MainActions[i];

				//Makes lists of scripts matching what states are assigned for this state to transition to or activate.

				action.ConnectedActions = new List<IMainAction>();
			for (int a = 0 ; a < action.ConnectedStates.Count ; a++)
			{
				action.ConnectedActions.Add(S_S_ActionHandling.GetControlledActionFromEnum(action.ConnectedStates[a], _ObjectForActions));
			}

			action.SituationalActions = new List<IMainAction>();
			for (int a = 0 ; a < action.SituationalStates.Count ; a++)
			{
				action.SituationalActions.Add(S_S_ActionHandling.GetSituationalActionFromEnum(action.SituationalStates[a], _ObjectForActions));
			}


			action.SubActions = new List<ISubAction>();
			for (int a = 0 ; a < action.PerformableSubStates.Count ; a++)
			{
				action.SubActions.Add(S_S_ActionHandling.AddOrFindSubActionComponent(action.PerformableSubStates[a], _ObjectForSubActions));
			}

			//Assigns the script related to this state
			action.Action = S_S_ActionHandling.AddOrFindMainActionComponent(action.State, _ObjectForActions);

			//Add this action to a list of all actions this character has, so they can all be called during FixedUpdate().
			if (!_AllActions.Contains(action.Action)) { _AllActions.Add(action.Action); }
			for(int subA = 0 ; subA < action.SubActions.Count ; subA++)
			{
				if (!_AllActions.Contains(action.SubActions[subA])) { _AllActions.Add(action.SubActions[subA]); }
			}

				//Applies the changes directly to the struct element.
				_MainActions[i] = action;
		}

		//Set player to start in default action.
		DeactivateAllActions(true);
		_currentAction = _MainActions[0];

		_ActionDefault.StartAction();
	}

	private void FixedUpdate () {


		//The counter is set and referenced outside of this script, this counts it down and must be zero or lower to allow homing attacks.
		if (_dashDelayCounter > 0)
		{
			_dashDelayCounter -= Time.deltaTime;
			if (_dashDelayCounter <= 0) { _isAirDashAvailable = true; }
		}

		if(!_areAirActionsAvailable && _framesAirActionsLockedFor > 0)
		{
			_framesAirActionsLockedFor--;
			if(_framesAirActionsLockedFor == 0) { _areAirActionsAvailable = true; }
		}

		//For actions that continue calculations even when no active, use ActionEveryFrame
		for (int a = 0 ; a < _AllActions.Count ; a++)
		{
			_AllActions[a].ActionEveryFixedUpdate();
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	//Go through every action attached to this manager and call the inherited StopAction method (because interface).
	public void DeactivateAllActions (bool firstTime = false) {

		for (int a = 0 ; a < _MainActions.Count ; a++)
		{
			S_Structs.MainActionTracker track = _MainActions[a];
			if (track.State != _whatCurrentAction)
			{
				track.Action.StopAction(firstTime); //The stop action methods should all contain the same check if enabled and then disable the script if so.
			}
		}
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//Called by action scripts to go through all of the actions they can possibly transition to. Each primary action should call this on Update
	public void HandleInputs ( int currentActionInList ) {
		if (_isPaused || _HealthAndHurt._isDead) { return; } //Can only change state if game isn't paused.

		bool performAction; //This will be set to true if an action attempt succeeds, stopping the checks after one does so.

		//_currentAction = _MainActions[currentActionInList]; //This will allow the update method to check situation actions
		IMainAction thisAction = _currentAction.Action;

		//Calls the attempt methods of actions saved to the current action's struct, which handle input and situations.
		for (int a = 0 ; a < _currentAction.ConnectedActions.Count ; a++)
		{
			IMainAction Action = _currentAction.ConnectedActions[a];
			performAction = Action.AttemptAction();
			if (performAction) { return; }
		}


		//Checks if any subactions attached to this action should be performed ontop. 
		//When one returns true, it is being switched to, so end the method.
		for (int a = 0 ; a < _currentAction.SubActions.Count ; a++)
		{
			ISubAction SubAction = _currentAction.SubActions[a];
			performAction = SubAction.AttemptAction();
			if (performAction) { return; }
		}

	}

	//Similar to HandleInputs, but performed every frame no matter what, and goes through every action to notify them that they can possiblly be performed.
	//This is so they are aware they are possible without holding direct references to them.
	//E.G. Boost will end itself if !_inAStateConnectedToThis, which is set in S_Action_Base based on CheckAction().
	public void CheckConnectedActions(int currentActionInList ) {
		if (_isPaused || _HealthAndHurt._isDead) { return; }

		_currentAction = _MainActions[currentActionInList]; //This will allow the update method to check situation actions
		IMainAction thisAction = _currentAction.Action;

		//Main Actions
		for (int a = 0 ; a < _currentAction.ConnectedActions.Count ; a++)
		{
			IMainAction Action = _currentAction.ConnectedActions[a];
			Action.CheckAction(); //Check action is found only in S_Action_Base. Inherited but not overwritten.
		}
		//Current action is set when  handle inputs is called, this goes through each situation action and calls methods that should allow them to be checked. Meaning it can only be enetered if it's called this frame
		for (int a = 0 ; a < _currentAction.SituationalActions.Count ; a++)
		{
			IMainAction Action = _currentAction.SituationalActions[a];
			Action.AttemptAction();
			Action.CheckAction();
		}
		//Sub Actions
		for (int a = 0 ; a < _currentAction.SubActions.Count ; a++)
		{
			ISubAction SubAction = _currentAction.SubActions[a];
			SubAction.CheckAction();
		}
	}

	//Call this function to change the action. Enabled should always be called when this is, but this disables all the others and sets the enum.
	public void ChangeAction ( S_S_ActionHandling.PrimaryPlayerStates ActionToChange) {
		//Resetting values
		_speedBeforeAction = 0;

		//Preparing change
		_whatPreviousAction = _whatCurrentAction;
		_whatCurrentAction = ActionToChange;
		DeactivateAllActions();
	}

	//Called externally to prevent certain actions from being performed until time is up.
	public void LockAirMovesForFrames(int frames ) {
		_areAirActionsAvailable = false;
		if(frames > _framesAirActionsLockedFor)
			_framesAirActionsLockedFor = frames;
	}

	//Called upon successful attacks to set the counter (which will tick down when above 0)
	public void AddDashDelay (float delay) {
		_isAirDashAvailable = false;
		_dashDelayCounter = delay;
	}

	//Takes an action enum, searches if the corresponding action is available, then either deactivates or reactivats.
	public void DisableOrEnableActionOfType(S_S_ActionHandling.PrimaryPlayerStates actionEnum, bool enable ) {
		for (int i = 0 ; i < _MainActions.Count ; i++)
		{
			IMainAction Action = S_S_ActionHandling.GetActionFromEnum(actionEnum, _ObjectForActions);
			if(Action != null)
			{
				if (enable) Action.ReactivateAction();
				else Action.DeactivateAction();
			}
		}
	}

	//Takes an action enum, searches if the corresponding action is available, then either deactivates or reactivats.
	public void DisableOrEnableSubActionOfType ( S_S_ActionHandling.SubPlayerStates subActionEnum, bool enable ) {
		for (int i = 0 ; i < _MainActions.Count ; i++)
		{
			ISubAction Action = S_S_ActionHandling.GetSubActionFromEnum(subActionEnum, _ObjectForSubActions);
			if (Action != null)
			{
				if (enable) Action.ReactivateAction();
				else Action.DeactivateAction();
			}
		}
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	#endregion

}

#if UNITY_EDITOR
[CustomEditor(typeof(S_ActionManager))]
public class ActionManagerEditor : S_CustomInspector_Base
{
	S_ActionManager _ActionMan;

	public override void OnEnable () {
		//Setting variables
		_ActionMan = (S_ActionManager)target;
		_InspectorTheme = _ActionMan._InspectorTheme;

		base.OnEnable();
	}

	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _ActionMan._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {

		//Describe what the script does
		EditorGUILayout.TextArea("This is the action manager, and it defines what actions the character can perform, and their connections to each other \n" +
			"Here, each state is reprsented by drop down menus, ordered into a list. Each element will have itself and the states it can perform / enter. \n" +
			"ADD NEW STATE will add a new action to the list of ones this character can perform. \n" +
			"IMPORT MISSING will add any state listed as a connection by another, and ensure the scripts themeslves are components in the children. \n" +
			"SORT ALL ACTIONS will organize the list into the normal order. \n" +
			"If you want to easily add specific actions as connections to others, set one in ADD THIS TO ACTIONS, then " +
			"whenever you press ADD SET, that action will be set as a conenction to the one you pressed on.", EditorStyles.textArea);

		SerializedProperty ActionList = serializedObject.FindProperty("_MainActions");

		//Order of Drawing
		EditorGUILayout.Space(_spaceSize);
		DrawAddAction();
		DrawMissingScripts();
		DrawReorderActions();
		DrawActions();

		//List of current actions
		#region Actions
		void DrawActions () {
			EditorGUILayout.Space(_spaceSize);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_addState"), new GUIContent("Add This To Actions"));

			EditorGUI.BeginChangeCheck();

			S_S_CustomInspector.DrawListCustom(serializedObject, "_MainActions", _SmallButtonStyle, _ActionMan,
			DrawListElementName, DrawWithEachListElement);
		}

		void DrawListElementName ( int i, SerializedProperty element ) {
			EditorGUILayout.PropertyField(element, new GUIContent("Action " + i + " - " + _ActionMan._MainActions[i].State));
		}

		void DrawWithEachListElement ( int i ) {
			//Pressing this button inserts the state labled at the top to be transitined to from the current state.
			if (S_S_CustomInspector.IsDrawnButtonPressed(serializedObject, "Add Set", _SmallButtonStyle, _ActionMan, "Add Connector to State"))
			{
				AddActionToThis(_ActionMan._addState, i);
			}
		}

		//Button for adding new action
		void DrawAddAction () {
			//Each element of the list is shown in the inspector seperately, rather than under one header. Therefore we need custom add and remove buttons.

			//Add new element button.
			if (S_S_CustomInspector.IsDrawnButtonPressed(serializedObject,"Add New State", _BigButtonStyle, _ActionMan, "Add New State"))
			{
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.Default, true, true);
				serializedObject.Update();
			}
			serializedObject.ApplyModifiedProperties();
		}

		//Button for making sure the object has the components necessary to its actions.
		void DrawMissingScripts () {
			if(S_S_CustomInspector.IsDrawnButtonPressed(serializedObject, "Import Missing", _BigButtonStyle, _ActionMan, "Import Missing"))
			{
				for (int i = 0 ; i < _ActionMan._MainActions.Count ; i++)
				{
					//Go through each struct and use the AssignScript to add missing components.
					S_Structs.MainActionTracker action = _ActionMan._MainActions[i];


					//Go through all of the connected states for each state, and make sure those states are also in the list.
					if(action.SituationalStates.Count > 0)
					{
						for (int a = 0 ; a < action.SituationalStates.Count ; a++)
						{
							AddSituationalActionToList(action.SituationalStates[a]);
						}
					}
					if(action.ConnectedStates.Count > 0)
					{
						for (int a = 0 ; a < action.ConnectedStates.Count ; a++)
						{
							AddControledActionToList(action.ConnectedStates[a]);
						}
					}

					//Makes sure every action still has default as as connected situation interaction.
					if (!action.SituationalStates.Any(item => item == S_S_ActionHandling.PlayerSituationalStates.Default))
					{
						action.SituationalStates.Insert(0, S_S_ActionHandling.PlayerSituationalStates.Default);
					}

					//Makes sure every action still has Hurt as as connected situation interaction.
					if (!action.SituationalStates.Any(item => item == S_S_ActionHandling.PlayerSituationalStates.Hurt))
					{
						action.SituationalStates.Insert(0, S_S_ActionHandling.PlayerSituationalStates.Hurt);
					}

					//Ensures the component is attached to the game objects.
					S_S_ActionHandling.AddOrFindMainActionComponent(action.State, _ActionMan._ObjectForActions);
					//Ensures the same with the subactions
					for (int a = 0 ; a < action.PerformableSubStates.Count ; a++)
					{
						S_S_ActionHandling.AddOrFindSubActionComponent(action.PerformableSubStates[a], _ActionMan._ObjectForSubActions);
					}

					//To apply this, the action has to be removed from the list, and this added in its place.
					_ActionMan._MainActions.RemoveAt(i);
					_ActionMan._MainActions.Insert(i, action);
				}
				// Apply changes to the serialized object
				serializedObject.ApplyModifiedProperties();


			}
		}

		//Button for making the list of actions ordered by the playerState enums they're set as.
		void DrawReorderActions () {
			if(S_S_CustomInspector.IsDrawnButtonPressed(serializedObject,"Sort all actions", _BigButtonStyle, _ActionMan, "Reoder Actions"))
			{
				_ActionMan._MainActions = _ActionMan._MainActions.OrderBy(item => item.State).ToList();
				serializedObject.ApplyModifiedProperties();

				//Also order each of the subLists for each actions by how they're ordered in the Enums class.
				for (int i = 0 ; i < _ActionMan._MainActions.Count ; i++)
				{
					S_Structs.MainActionTracker s = _ActionMan._MainActions[i];
					s.ConnectedStates = s.ConnectedStates.OrderBy(d => (int)d).ToList();
					s.SituationalStates = s.SituationalStates.OrderBy(d => (int)d).ToList();
					s.PerformableSubStates = s.PerformableSubStates.OrderBy(d => (int)d).ToList();

					_ActionMan._MainActions[i] = s;
					serializedObject.ApplyModifiedProperties();
				}
			}
		}
		#endregion

		_ActionMan._ObjectForActions = GenerateOrCheckObjectToHoldOthers(_ActionMan.transform, _ActionMan._ObjectForActions, "Main Actions");
		_ActionMan._ObjectForSubActions = GenerateOrCheckObjectToHoldOthers(_ActionMan.transform, _ActionMan._ObjectForSubActions, "Sub Actions");
		_ActionMan._ObjectForInteractions = GenerateOrCheckObjectToHoldOthers(_ActionMan.transform, _ActionMan._ObjectForInteractions, "Interactions");
	}

	//Take in a primary state and add it to the list if it can be found.
	private void AddActionToList ( S_S_ActionHandling.PrimaryPlayerStates state, bool withDefault = false, bool skip = false ) {
		if (!_ActionMan._MainActions.Any(item => item.State == state) || skip)
		{
			S_Structs.MainActionTracker temp = new S_Structs.MainActionTracker();
			temp.State = state;

			if (withDefault)
			{
				temp.SituationalStates = new List<S_S_ActionHandling.PlayerSituationalStates>();
				temp.SituationalStates.Add(S_S_ActionHandling.PlayerSituationalStates.Default);
			}

			_ActionMan._MainActions.Add(temp);
		}
	}

	//Go through all situation actions and add the equivilant main action.
	private void AddSituationalActionToList ( S_S_ActionHandling.PlayerSituationalStates state ) {

		switch (state)
		{
			case S_S_ActionHandling.PlayerSituationalStates.Rail:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.Rail);
				break;
			case S_S_ActionHandling.PlayerSituationalStates.Path:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.Path);
				break;
			case S_S_ActionHandling.PlayerSituationalStates.RingRoad:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.RingRoad);
				break;
			case S_S_ActionHandling.PlayerSituationalStates.WallRunning:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.WallRunning);
				break;
			case S_S_ActionHandling.PlayerSituationalStates.WallClimbing:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.WallClimbing);
				break;
			case S_S_ActionHandling.PlayerSituationalStates.Default:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.Default);
				break;
			case S_S_ActionHandling.PlayerSituationalStates.Hovering:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.Hovering);
				break;
			case S_S_ActionHandling.PlayerSituationalStates.Hurt:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.Hurt);
				break;
			case S_S_ActionHandling.PlayerSituationalStates.Upreel:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.Upreel);
				break;
		}
	}

	//Go through all controlled actions and add the equivilant main action.
	private void AddControledActionToList ( S_S_ActionHandling.PlayerControlledStates state ) {
		switch (state)
		{
			case S_S_ActionHandling.PlayerControlledStates.Jump:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.Jump);
				break;
			case S_S_ActionHandling.PlayerControlledStates.Homing:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.Homing);
				break;
			case S_S_ActionHandling.PlayerControlledStates.SpinCharge:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.SpinCharge);
				break;
			case S_S_ActionHandling.PlayerControlledStates.Bounce:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.Bounce);
				break;
			case S_S_ActionHandling.PlayerControlledStates.DropCharge:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.DropCharge);
				break;
			case S_S_ActionHandling.PlayerControlledStates.JumpDash:
				AddActionToList(S_S_ActionHandling.PrimaryPlayerStates.JumpDash);
				break;
		}
	}

	//Takes in a main action and connects it to the current state as one that can be transitioned to.
	void AddActionToThis ( S_S_ActionHandling.PrimaryPlayerStates state, int target ) {
		if (_ActionMan._MainActions[target].State == state) { return; }

		switch (state)
		{
			case S_S_ActionHandling.PrimaryPlayerStates.Default:
				_ActionMan._MainActions[target].SituationalStates.Add(S_S_ActionHandling.PlayerSituationalStates.Default);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.Jump:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_S_ActionHandling.PlayerControlledStates.Jump);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.Homing:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_S_ActionHandling.PlayerControlledStates.Homing);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.SpinCharge:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_S_ActionHandling.PlayerControlledStates.SpinCharge);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.Hurt:
				_ActionMan._MainActions[target].SituationalStates.Add(S_S_ActionHandling.PlayerSituationalStates.Hurt);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.Rail:
				_ActionMan._MainActions[target].SituationalStates.Add(S_S_ActionHandling.PlayerSituationalStates.Rail);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.Bounce:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_S_ActionHandling.PlayerControlledStates.Bounce);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.RingRoad:
				_ActionMan._MainActions[target].SituationalStates.Add(S_S_ActionHandling.PlayerSituationalStates.RingRoad);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.DropCharge:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_S_ActionHandling.PlayerControlledStates.DropCharge);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.Path:
				_ActionMan._MainActions[target].SituationalStates.Add(S_S_ActionHandling.PlayerSituationalStates.Path);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.JumpDash:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_S_ActionHandling.PlayerControlledStates.JumpDash);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.WallRunning:
				_ActionMan._MainActions[target].SituationalStates.Add(S_S_ActionHandling.PlayerSituationalStates.WallRunning);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.WallClimbing:
				_ActionMan._MainActions[target].SituationalStates.Add(S_S_ActionHandling.PlayerSituationalStates.WallClimbing);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.Hovering:
				_ActionMan._MainActions[target].SituationalStates.Add(S_S_ActionHandling.PlayerSituationalStates.Hovering);
				break;
			case S_S_ActionHandling.PrimaryPlayerStates.Upreel:
				_ActionMan._MainActions[target].SituationalStates.Add(S_S_ActionHandling.PlayerSituationalStates.Upreel);
				break;

		}
	}

	//Called whenever an object should exist, that's the child of another. Used to manage the seperate objects holding the script components.
	private GameObject GenerateOrCheckObjectToHoldOthers (Transform ObjectParent, GameObject ThisObject, string objectName) {
		//If there isn't one yet, find it or create it.
		if(ThisObject == null)
		{
			return CreateNewObject();
		}
		//If it is no longer a child of what it should be (so was moved in the editor), replace it.
		else if(ThisObject.transform.parent != ObjectParent)
		{
			ThisObject.transform.parent = ObjectParent;
		}
		return ThisObject;

		//Searched through the prefab to find the object, and if it can't be found, creates a new one in its place.
		GameObject CreateNewObject(){
			string generatedName = objectName;
			var generatedTranform = _ActionMan.transform.Find(generatedName);
			ThisObject = generatedTranform != null ? generatedTranform.gameObject : new GameObject(generatedName);
			ThisObject.transform.parent = ObjectParent;
			return ThisObject;
		}
	}
}
#endif
