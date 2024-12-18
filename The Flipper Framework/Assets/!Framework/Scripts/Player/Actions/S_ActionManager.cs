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
	public S_GeneralEnums.PrimaryPlayerStates	_whatCurrentAction = S_GeneralEnums.PrimaryPlayerStates.None;
	public S_GeneralEnums.SubPlayerStates	_whatSubAction;
	public S_GeneralEnums.PlayerAttackStates       _whatCurrentAttack;
	public S_GeneralEnums.PrimaryPlayerStates	_whatPreviousAction { get; set; }

	[HideInInspector]
	public bool         _canChangeActions = true;	//All StartActions should check this, and return if its false, unless they are set to overwrite this.

	[HideInInspector]
	public List<float>                            _listOfSpeedOnPaths = new List<float>();	//Certain actions will move the player along the spline, this will be used to track the speed for any actions that do so. It is used as a list rather than a singular as it will allow speeds to be added and removed with the action, then the most recent is the only one used.

	//Actions
	public List<S_Structs.StrucMainActionTracker>	_MainActions; //This list of structs will cover each action currently available to the player (set in inspector), along with what actions it can enter through input or situation.	
	private S_Structs.StrucMainActionTracker	_currentAction; //Which struct in the above list is currently active.

	//Inspector
#if UNITY_EDITOR
	public S_O_CustomInspectorStyle		_InspectorTheme; // Will decide the apperance in the inspector.
#endif
	public S_GeneralEnums.PrimaryPlayerStates                _addState; //Used only by the inspector in order to add states for other states to transition into.

	//Specific action trackers
	[HideInInspector]
	public bool         _isAirDashAvailables = true; //Govers whether homing attacks and jump dashes can be performed.
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
	public Vector3      _dashAngle;		//Same as above but for the jumpDash action.

	//Can perform actions

	//The bellow are all temporarily locked under certain situations, like using a spring.
	[HideInInspector]
	public bool         _areAirActionsAvailable = true;

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

		//Go through each struct and assign/add the scripts linked to that enum.
		for (int i = 0 ; i < _MainActions.Count ; i++)
		{
			S_Structs.StrucMainActionTracker action = _MainActions[i];

			//Makes lists of scripts matching what states are assigned for this state to transition to or activate.

			action.ConnectedActions = new List<IMainAction>();
			for (int a = 0 ; a < action.ConnectedStates.Count ; a++)
			{
				action.ConnectedActions.Add(AssignControlledScriptByEnum(action.ConnectedStates[a]));
			}

			action.SituationalActions = new List<IMainAction>();
			for (int a = 0 ; a < action.SituationalStates.Count ; a++)
			{
				action.SituationalActions.Add(AssignSituationalScriptByEnum(action.SituationalStates[a]));
			}


			action.SubActions = new List<ISubAction>();
			for (int a = 0 ; a < action.PerformableSubStates.Count ; a++)
			{
				action.SubActions.Add(AssignSubScript(action.PerformableSubStates[a]));
			}

			//Assigns the script related to this state
			action.Action = AssignMainActionScriptByEnum(action.State);

			//Applies the changes directly to the struct element.
			_MainActions[i] = action;
		}

		//Set player to start in default action.
		DeactivateAllActions(true);
		_currentAction = _MainActions[0];
		//ChangeAction(S_Enums.PrimaryPlayerStates.Default);
		//_ActionDefault.enabled = true;
		_ActionDefault.StartAction();
	}

	private void FixedUpdate () {


		//The counter is set and referenced outside of this script, this counts it down and must be zero or lower to allow homing attacks.
		if (_dashDelayCounter > 0)
		{
			_dashDelayCounter -= Time.deltaTime;
			if (_dashDelayCounter <= 0) { _isAirDashAvailables = true; }
		}

		//Current action is set when  handle inputs is called, this goes through each situation action and calls methods that should allow them to be checked. Meaning it can only be enetered if it's called this frame
		for (int a = 0 ; a < _currentAction.SituationalActions.Count ; a++)
		{
			_currentAction.SituationalActions[a].AttemptAction();
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
			S_Structs.StrucMainActionTracker track = _MainActions[a];
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

		_currentAction = _MainActions[currentActionInList]; //This will allow the update method to check situation actions


		//Calls the attempt methods of actions saved to the current action's struct, which handle input and situations.
		for (int a = 0 ; a < _currentAction.ConnectedActions.Count ; a++)
		{
			performAction = _currentAction.ConnectedActions[a].AttemptAction();
			if (performAction) { return; }
		}


		//Checks if any subactions attached to this action should be performed ontop. 
		//When one returns true, it is being switched to, so end the method.
		for (int a = 0 ; a < _currentAction.SubActions.Count ; a++)
		{
			performAction = _currentAction.SubActions[a].AttemptAction();
			if (performAction) { return; }
		}

	}

	//Takes a controlled or situational action, and returns true if the current active action has that set as an action is can transition into.
	//Used to check if the player can enter this given action, from the current active one.
	public bool IsActionConnectedToCurrentAction(S_GeneralEnums.PlayerControlledStates givenConAction, S_GeneralEnums.PlayerSituationalStates givenSitAction) {
		if (givenConAction != S_GeneralEnums.PlayerControlledStates.None)
		{
			for (int i = 0 ; i < _currentAction.ConnectedStates.Count ; i++)
			{
				if (_currentAction.ConnectedStates[i] == givenConAction) { return true; }
			}
		}
		if (givenSitAction != S_GeneralEnums.PlayerSituationalStates.None)
		{
			for (int i = 0 ; i < _currentAction.SituationalStates.Count ; i++)
			{
				if (_currentAction.SituationalStates[i] == givenSitAction) { return true; }
			}
		}

		return false;
	}

	//Call this function to change the action. Enabled should always be called when this is, but this disables all the others and sets the enum.
	public void ChangeAction ( S_GeneralEnums.PrimaryPlayerStates ActionToChange) {
		_whatPreviousAction = _whatCurrentAction;
		_whatCurrentAction = ActionToChange;
		DeactivateAllActions();
	}

	//Called externally to prevent certain actions from being performed until time is up.
	public IEnumerator LockAirMovesForFrames ( float frames ) {
		_areAirActionsAvailable = false;

		//Apply delay, in frames.
		for (int s = 0 ; s < frames ; s++)
		{
			yield return new WaitForFixedUpdate();
			if (_PlayerPhys._isGrounded)
				break;
		}

		_areAirActionsAvailable = true;
	}

	//Called upon successful attacks to set the counter (which will tick down when above 0)
	public void AddDashDelay (float delay) {
		_isAirDashAvailables = false;
		_dashDelayCounter = delay;
	}
	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	//Each enum of playerstate corresponds to a different script that handles its behaviour. This assigns the matching script.
	public IMainAction AssignControlledScriptByEnum ( S_GeneralEnums.PlayerControlledStates state ) {
		switch (state)
		{
			//If the enum of the struct is set to Jump, then assign the jump script to it.
			case S_GeneralEnums.PlayerControlledStates.Jump:
				_ObjectForActions.TryGetComponent(out S_Action01_Jump jump);
				return jump;
			case S_GeneralEnums.PlayerControlledStates.Homing:
				_ObjectForActions.TryGetComponent(out S_Action02_Homing home);
				return home;
			case S_GeneralEnums.PlayerControlledStates.SpinCharge:
				_ObjectForActions.TryGetComponent(out S_Action03_SpinCharge spin);
				return spin;
			case S_GeneralEnums.PlayerControlledStates.Bounce:
				_ObjectForActions.TryGetComponent(out S_Action06_Bounce bounce);
				return bounce;
			case S_GeneralEnums.PlayerControlledStates.DropCharge:
				_ObjectForActions.TryGetComponent(out S_Action08_DropCharge drop);
				return drop;
			case S_GeneralEnums.PlayerControlledStates.JumpDash:
				_ObjectForActions.TryGetComponent(out S_Action11_JumpDash dash);
				return dash;
		}
		return null;
	}
	public IMainAction AssignSituationalScriptByEnum ( S_GeneralEnums.PlayerSituationalStates state ) {
		switch (state)
		{
			//If the enum of the struct is set to Regular, then assign the jump script to it.
			case S_GeneralEnums.PlayerSituationalStates.Default:
				_ObjectForActions.TryGetComponent(out S_Action00_Default def);
				return def;
			case S_GeneralEnums.PlayerSituationalStates.Hurt:
				_ObjectForActions.TryGetComponent(out S_Action04_Hurt hurt);
				return hurt;
			case S_GeneralEnums.PlayerSituationalStates.Rail:
				_ObjectForActions.TryGetComponent(out S_Action05_Rail rail);
				return rail;
			case S_GeneralEnums.PlayerSituationalStates.RingRoad:
				_ObjectForActions.TryGetComponent(out S_Action07_RingRoad road);
				return road;
			case S_GeneralEnums.PlayerSituationalStates.Path:
				_ObjectForActions.TryGetComponent(out S_Action10_FollowAutoPath path);
				return path;
			case S_GeneralEnums.PlayerSituationalStates.WallRunning:
				_ObjectForActions.TryGetComponent(out S_Action12_WallRunning wall);
				return wall;
			case S_GeneralEnums.PlayerSituationalStates.WallClimbing:
				_ObjectForActions.TryGetComponent(out S_Action15_WallClimbing wallClimb);
				return wallClimb;
			case S_GeneralEnums.PlayerSituationalStates.Hovering:
				_ObjectForActions.TryGetComponent(out S_Action13_Hovering hov);
				return hov;
			case S_GeneralEnums.PlayerSituationalStates.Upreel:
				_ObjectForActions.TryGetComponent(out S_Action14_Upreel up);
				return up;
		}
		return null;
	}

	public IMainAction AssignMainActionScriptByEnum ( S_GeneralEnums.PrimaryPlayerStates state ) {
		switch (state)
		{
			// If the enum of the struct is set to Jump, then assign the jump script to it.
			case S_GeneralEnums.PrimaryPlayerStates.Jump:
				if (_ObjectForActions.TryGetComponent(out S_Action01_Jump jumpAction))
				{
					return jumpAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action01_Jump>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.Homing:
				if (_ObjectForActions.TryGetComponent(out S_Action02_Homing homingAction))
				{
					return homingAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action02_Homing>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.SpinCharge:
				if (_ObjectForActions.TryGetComponent(out S_Action03_SpinCharge spinChargeAction))
				{
					return spinChargeAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action03_SpinCharge>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.Bounce:
				if (_ObjectForActions.TryGetComponent(out S_Action06_Bounce bounceAction))
				{
					return bounceAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action06_Bounce>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.DropCharge:
				if (_ObjectForActions.TryGetComponent(out S_Action08_DropCharge dropChargeAction))
				{
					return dropChargeAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action08_DropCharge>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.JumpDash:
				if (_ObjectForActions.TryGetComponent(out S_Action11_JumpDash jumpDashAction))
				{
					return jumpDashAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action11_JumpDash>();
				}

			// If the enum of the struct is set to Default, then assign the jump script to it.
			case S_GeneralEnums.PrimaryPlayerStates.Default:
				if (_ObjectForActions.TryGetComponent(out S_Action00_Default defaultAction))
				{
					return defaultAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action00_Default>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.Hurt:
				if (_ObjectForActions.TryGetComponent(out S_Action04_Hurt hurtAction))
				{
					return hurtAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action04_Hurt>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.Rail:
				if (_ObjectForActions.TryGetComponent(out S_Action05_Rail railAction))
				{
					return railAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action05_Rail>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.RingRoad:
				if (_ObjectForActions.TryGetComponent(out S_Action07_RingRoad ringRoadAction))
				{
					return ringRoadAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action07_RingRoad>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.Path:
				if (_ObjectForActions.TryGetComponent(out S_Action10_FollowAutoPath pathAction))
				{
					return pathAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action10_FollowAutoPath>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.WallRunning:
				if (_ObjectForActions.TryGetComponent(out S_Action12_WallRunning wallRunningAction))
				{
					return wallRunningAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action12_WallRunning>();
				}
			case S_GeneralEnums.PrimaryPlayerStates.WallClimbing:
				if (_ObjectForActions.TryGetComponent(out S_Action15_WallClimbing wallClimbingAction))
				{
					return wallClimbingAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action15_WallClimbing>();
				}

			case S_GeneralEnums.PrimaryPlayerStates.Hovering:
				if (_ObjectForActions.TryGetComponent(out S_Action13_Hovering hoveringAction))
				{
					return hoveringAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action13_Hovering>();
				}
			case S_GeneralEnums.PrimaryPlayerStates.Upreel:
				if (_ObjectForActions.TryGetComponent(out S_Action14_Upreel upreelAction))
				{
					return upreelAction;
				}
				else
				{
					return _ObjectForActions.AddComponent<S_Action14_Upreel>();
				}
		}
		return null;
	}

	public ISubAction AssignSubScript ( S_GeneralEnums.SubPlayerStates state ) {
		switch (state)
		{
			// Case for S_SubAction_Skid
			case S_GeneralEnums.SubPlayerStates.Skidding:
				if (_ObjectForSubActions.TryGetComponent(out S_SubAction_Skid skidAction))
				{
					return skidAction;
				}
				else
				{
					return _ObjectForSubActions.AddComponent<S_SubAction_Skid>();
				}

			// Case for S_SubAction_Quickstep
			case S_GeneralEnums.SubPlayerStates.Quickstepping:
				if (_ObjectForSubActions.TryGetComponent(out S_SubAction_Quickstep quickstepAction))
				{
					return quickstepAction;
				}
				else
				{
					return _ObjectForSubActions.AddComponent<S_SubAction_Quickstep>();
				}

			// Case for S_SubAction_Roll
			case S_GeneralEnums.SubPlayerStates.Rolling:
				if (_ObjectForSubActions.TryGetComponent(out S_SubAction_Roll rollAction))
				{
					return rollAction;
				}
				else
				{
					return _ObjectForSubActions.AddComponent<S_SubAction_Roll>();
				}
			case S_GeneralEnums.SubPlayerStates.Boost:
				if (_ObjectForSubActions.TryGetComponent(out S_SubAction_Boost boostAction))
				{
					return boostAction;
				}
				else
				{
					return _ObjectForSubActions.AddComponent<S_SubAction_Boost>();
				}
		}
		return null;
	}
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

			S_S_CustomInspectorMethods.DrawListCustom(serializedObject, "_MainActions", _SmallButtonStyle, _ActionMan,
			DrawListElementName, DrawWithEachListElement);
		}

		void DrawListElementName ( int i, SerializedProperty element ) {
			EditorGUILayout.PropertyField(element, new GUIContent("Action " + i + " - " + _ActionMan._MainActions[i].State));
		}

		void DrawWithEachListElement ( int i ) {
			//Pressing this button inserts the state labled at the top to be transitined to from the current state.
			if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject, "Add Set", _SmallButtonStyle, _ActionMan, "Add Connector to State"))
			{
				AddActionToThis(_ActionMan._addState, i);
			}
		}

		//Button for adding new action
		void DrawAddAction () {
			//Each element of the list is shown in the inspector seperately, rather than under one header. Therefore we need custom add and remove buttons.

			//Add new element button.
			if (S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject,"Add New State", _BigButtonStyle, _ActionMan, "Add New State"))
			{
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.Default, true, true);
				serializedObject.Update();
			}
			serializedObject.ApplyModifiedProperties();
		}

		//Button for making sure the object has the components necessary to its actions.
		void DrawMissingScripts () {
			if(S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject, "Import Missing", _BigButtonStyle, _ActionMan, "Import Missing"))
			{
				for (int i = 0 ; i < _ActionMan._MainActions.Count ; i++)
				{
					//Go through each struct and use the AssignScript to add missing components.
					S_Structs.StrucMainActionTracker action = _ActionMan._MainActions[i];


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
					if (!action.SituationalStates.Any(item => item == S_GeneralEnums.PlayerSituationalStates.Default))
					{
						action.SituationalStates.Insert(0, S_GeneralEnums.PlayerSituationalStates.Default);
					}

					//Makes sure every action still has Hurt as as connected situation interaction.
					if (!action.SituationalStates.Any(item => item == S_GeneralEnums.PlayerSituationalStates.Hurt))
					{
						action.SituationalStates.Insert(0, S_GeneralEnums.PlayerSituationalStates.Hurt);
					}

					//Ensures the component is attached to the game objects.
					_ActionMan.AssignMainActionScriptByEnum(action.State);
					//Ensures the same with the subactions
					for (int a = 0 ; a < action.PerformableSubStates.Count ; a++)
					{
						_ActionMan.AssignSubScript(action.PerformableSubStates[a]);
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
			if(S_S_CustomInspectorMethods.IsDrawnButtonPressed(serializedObject,"Sort all actions", _BigButtonStyle, _ActionMan, "Reoder Actions"))
			{
				_ActionMan._MainActions = _ActionMan._MainActions.OrderBy(item => item.State).ToList();
				serializedObject.ApplyModifiedProperties();

				//Also order each of the subLists for each actions by how they're ordered in the Enums class.
				for (int i = 0 ; i < _ActionMan._MainActions.Count ; i++)
				{
					S_Structs.StrucMainActionTracker s = _ActionMan._MainActions[i];
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
	private void AddActionToList ( S_GeneralEnums.PrimaryPlayerStates state, bool withDefault = false, bool skip = false ) {
		if (!_ActionMan._MainActions.Any(item => item.State == state) || skip)
		{
			S_Structs.StrucMainActionTracker temp = new S_Structs.StrucMainActionTracker();
			temp.State = state;

			if (withDefault)
			{
				temp.SituationalStates = new List<S_GeneralEnums.PlayerSituationalStates>();
				temp.SituationalStates.Add(S_GeneralEnums.PlayerSituationalStates.Default);
			}

			_ActionMan._MainActions.Add(temp);
		}
	}

	//Go through all situation actions and add the equivilant main action.
	private void AddSituationalActionToList ( S_GeneralEnums.PlayerSituationalStates state ) {

		switch (state)
		{
			case S_GeneralEnums.PlayerSituationalStates.Rail:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.Rail);
				break;
			case S_GeneralEnums.PlayerSituationalStates.Path:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.Path);
				break;
			case S_GeneralEnums.PlayerSituationalStates.RingRoad:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.RingRoad);
				break;
			case S_GeneralEnums.PlayerSituationalStates.WallRunning:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.WallRunning);
				break;
			case S_GeneralEnums.PlayerSituationalStates.WallClimbing:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.WallClimbing);
				break;
			case S_GeneralEnums.PlayerSituationalStates.Default:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.Default);
				break;
			case S_GeneralEnums.PlayerSituationalStates.Hovering:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.Hovering);
				break;
			case S_GeneralEnums.PlayerSituationalStates.Hurt:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.Hurt);
				break;
			case S_GeneralEnums.PlayerSituationalStates.Upreel:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.Upreel);
				break;
		}
	}

	//Go through all controlled actions and add the equivilant main action.
	private void AddControledActionToList ( S_GeneralEnums.PlayerControlledStates state ) {
		switch (state)
		{
			case S_GeneralEnums.PlayerControlledStates.Jump:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.Jump);
				break;
			case S_GeneralEnums.PlayerControlledStates.Homing:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.Homing);
				break;
			case S_GeneralEnums.PlayerControlledStates.SpinCharge:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.SpinCharge);
				break;
			case S_GeneralEnums.PlayerControlledStates.Bounce:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.Bounce);
				break;
			case S_GeneralEnums.PlayerControlledStates.DropCharge:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.DropCharge);
				break;
			case S_GeneralEnums.PlayerControlledStates.JumpDash:
				AddActionToList(S_GeneralEnums.PrimaryPlayerStates.JumpDash);
				break;
		}
	}

	//Takes in a main action and connects it to the current state as one that can be transitioned to.
	void AddActionToThis ( S_GeneralEnums.PrimaryPlayerStates state, int target ) {
		if (_ActionMan._MainActions[target].State == state) { return; }

		switch (state)
		{
			case S_GeneralEnums.PrimaryPlayerStates.Default:
				_ActionMan._MainActions[target].SituationalStates.Add(S_GeneralEnums.PlayerSituationalStates.Default);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.Jump:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_GeneralEnums.PlayerControlledStates.Jump);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.Homing:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_GeneralEnums.PlayerControlledStates.Homing);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.SpinCharge:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_GeneralEnums.PlayerControlledStates.SpinCharge);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.Hurt:
				_ActionMan._MainActions[target].SituationalStates.Add(S_GeneralEnums.PlayerSituationalStates.Hurt);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.Rail:
				_ActionMan._MainActions[target].SituationalStates.Add(S_GeneralEnums.PlayerSituationalStates.Rail);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.Bounce:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_GeneralEnums.PlayerControlledStates.Bounce);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.RingRoad:
				_ActionMan._MainActions[target].SituationalStates.Add(S_GeneralEnums.PlayerSituationalStates.RingRoad);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.DropCharge:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_GeneralEnums.PlayerControlledStates.DropCharge);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.Path:
				_ActionMan._MainActions[target].SituationalStates.Add(S_GeneralEnums.PlayerSituationalStates.Path);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.JumpDash:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_GeneralEnums.PlayerControlledStates.JumpDash);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.WallRunning:
				_ActionMan._MainActions[target].SituationalStates.Add(S_GeneralEnums.PlayerSituationalStates.WallRunning);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.WallClimbing:
				_ActionMan._MainActions[target].SituationalStates.Add(S_GeneralEnums.PlayerSituationalStates.WallClimbing);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.Hovering:
				_ActionMan._MainActions[target].SituationalStates.Add(S_GeneralEnums.PlayerSituationalStates.Hovering);
				break;
			case S_GeneralEnums.PrimaryPlayerStates.Upreel:
				_ActionMan._MainActions[target].SituationalStates.Add(S_GeneralEnums.PlayerSituationalStates.Upreel);
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
