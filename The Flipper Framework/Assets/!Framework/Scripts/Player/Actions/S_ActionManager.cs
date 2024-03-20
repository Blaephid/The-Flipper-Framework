using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine.UI;
using static UnityEngine.GridBrushBase;

[RequireComponent(typeof(S_Action00_Default))]
public class S_ActionManager : MonoBehaviour
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	S_PlayerPhysics _PlayerPhys;
	public S_LevelEventHandler eventMan;

	private S_CharacterTools      _Tools;

	//Action Scrips, Always leave them in the correct order;
	[Header("Actions")]
	public S_Action00_Default ActionDefault;
	public S_Action01_Jump Action01;
	public S_Action02_Homing Action02;
	public S_Action03_SpinCharge Action03;
	public S_Handler_HomingAttack Action02Control;
	public S_Action04_Hurt ActionHurt;
	public S_Handler_HealthAndHurt Action04Control;
	public S_Action05_Rail Action05;
	public S_Action06_Bounce Action06;
	public S_Action07_RingRoad Action07;
	public S_Action08_DropCharge Action08;
	public S_Action10_FollowAutoPath Action10;
	public S_Action11_JumpDash Action11;
	public S_Action12_WallRunning Action12;
	public S_Action13_Hovering Action13;
	public S_SubAction_Skid skid;

	private Animator _CharacterAnimator;
	public S_O_CustomInspectorStyle InspectorTheme;
	#endregion

	// Trackers
	#region trackers
	//Tracking action in game
	public S_Enums.PrimaryPlayerStates whatAction;
	public S_Enums.SubPlayerStates whatSubAction;
	public S_Enums.PrimaryPlayerStates whatPreviousAction { get; set; }

	//Actions
	public S_Enums.PrimaryPlayerStates _addState;
	public List<S_Structs.StrucMainActionTracker> _MainActions;
	public List<ISubAction> _SubActions;
	private S_Structs.StrucMainActionTracker _currentAction;


	//Specific action trackers
	[HideInInspector]
	public bool         _isAirDashAvailables = true;
	[HideInInspector]
	public int          _bounceCount;
	[HideInInspector]
	public float        _actionTimeCounter;
	[HideInInspector]
	public int          _jumpCount;

	public float _dashDelayCounter;

	//Can perform actions
	[HideInInspector]
	public bool         lockBounce;
	[HideInInspector]
	public bool         lockHoming;
	[HideInInspector]
	public bool         lockJumpDash;
	[HideInInspector]
	public bool         lockDoubleJump;
	public bool         _isTrackingEvents;
	[HideInInspector]
	public bool         isPaused;
	#endregion

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {

		_Tools = GetComponent<S_CharacterTools>();
		ActionDefault = GetComponent<S_Action00_Default>();
		ActionHurt = GetComponent<S_Action04_Hurt>();

		if (_isTrackingEvents)
		{
			eventMan = FindObjectOfType<S_LevelEventHandler>();
		}
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_CharacterAnimator = _Tools.CharacterAnimator;

		//Go through each struct and assign/add the scripts linked to that enum.
		for (int i = 0 ; i < _MainActions.Count ; i++)
		{
			S_Structs.StrucMainActionTracker action = _MainActions[i];

			//Makes lists of scripts matching what states are assigned for this state to transition to or activate.

			action.ConnectedActions = new List<IMainAction>();
			foreach (S_Enums.PlayerControlledStates connectedState in action.ConnectedStates)
			{
				action.ConnectedActions.Add(AssignControlledScriptByEnum(connectedState));
			}

			action.SituationalActions = new List<IMainAction>();
			foreach (S_Enums.PlayerSituationalStates situationalState in action.SituationalStates)
			{
				action.SituationalActions.Add(AssignSituationalScriptByEnum(situationalState));
			}

			action.SubActions = new List<ISubAction>();
			foreach (S_Enums.SubPlayerStates subState in action.PerformableSubStates)
			{
				action.SubActions.Add(AssignSubScript(subState));
			}

			//Assigns the script related to this state
			action.Action = AssignMainActionScriptByEnum(action.State);

			//Applies the changes directly to the struct element.
			_MainActions[i] = action;
		}

		_currentAction = _MainActions[0];
		DeactivateAllActions(true);
		ChangeAction(S_Enums.PrimaryPlayerStates.Default);
	}

	private void FixedUpdate () {

		//The counter is set and referenced outside of this script, this counts it down and must be zero or lower to allow homing attacks.
		if (_dashDelayCounter > 0)
		{
			_dashDelayCounter -= Time.deltaTime;
			if (_dashDelayCounter <= 0) { _isAirDashAvailables = true; }
		}

		foreach (IMainAction situationAction in _currentAction.SituationalActions)
		{
			situationAction.AttemptAction();
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

		foreach (S_Structs.StrucMainActionTracker track in _MainActions)
		{
			if(track.State != whatAction)
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

	//Called by action scripts to go through all of the actions they can possibly transition to.
	public void HandleInputs ( int currentActionInList ) {
		if (isPaused) { return; }

		bool performAction;

		_currentAction = _MainActions[currentActionInList];

		//Calls the attempt methods of actions saved to the current action's struct, which handle input and situations.
		//When one returns true, it is being switched to, so end the loop.
		foreach (IMainAction mainAction in _currentAction.ConnectedActions)
		{
			performAction = mainAction.AttemptAction();
			if (performAction) { break; }
		}

		performAction = false;
		//Checks if the subaction should be performed ontop of the current action.
		foreach (ISubAction subAction in _currentAction.SubActions)
		{
			performAction = subAction.AttemptAction();
			if (performAction) { break; }
		}
	}

	//Call this function to change the action
	public void ChangeAction ( S_Enums.PrimaryPlayerStates ActionToChange ) {


		//Put an case for all your actions here
		switch (ActionToChange)
		{
			case S_Enums.PrimaryPlayerStates.Default:
				IsChangePossible(ActionToChange);
				ActionDefault.enabled = true;
				break;
			case S_Enums.PrimaryPlayerStates.Jump:
				if (!lockDoubleJump)
				{
					IsChangePossible(ActionToChange);
					Action01.enabled = true;
				}
				break;
			case S_Enums.PrimaryPlayerStates.Homing:
				if (!lockHoming)
				{
					if (eventMan != null) eventMan.homingAttacksPerformed += 1;
					IsChangePossible(ActionToChange);
					Action02.enabled = true;
				}
				break;
			case S_Enums.PrimaryPlayerStates.JumpDash:
				if (!lockJumpDash)
				{
					if (eventMan != null) eventMan.jumpDashesPerformed += 1;
					IsChangePossible(ActionToChange);
					Action11.enabled = true;
				}
				break;
			case S_Enums.PrimaryPlayerStates.SpinCharge:
				IsChangePossible(ActionToChange);
				Action03.enabled = true;
				break;
			case S_Enums.PrimaryPlayerStates.Hurt:
				IsChangePossible(ActionToChange);
				ActionHurt.enabled = true;
				break;
			case S_Enums.PrimaryPlayerStates.Rail:
				if (eventMan != null) eventMan.RailsGrinded += 1;
				IsChangePossible(ActionToChange);
				Action05.enabled = true;
				break;
			case S_Enums.PrimaryPlayerStates.Bounce:
				IsChangePossible(ActionToChange);
				if (eventMan != null) eventMan.BouncesPerformed += 1;
				Action06.enabled = true;
				break;
			case S_Enums.PrimaryPlayerStates.RingRoad:
				if (eventMan != null) eventMan.ringRoadsPerformed += 1;
				IsChangePossible(ActionToChange);
				Action07.enabled = true;
				break;
			case S_Enums.PrimaryPlayerStates.DropCharge:
				IsChangePossible(ActionToChange);
				Action08.enabled = true;
				break;
			case S_Enums.PrimaryPlayerStates.Path:
				IsChangePossible(ActionToChange);
				Action10.enabled = true;
				break;
			case S_Enums.PrimaryPlayerStates.WallRunning:
				IsChangePossible(ActionToChange);
				Action12.enabled = true;
				break;
			case S_Enums.PrimaryPlayerStates.Hovering:
				IsChangePossible(ActionToChange);
				Action13.enabled = true;
				break;

		}

	}

	private void IsChangePossible ( S_Enums.PrimaryPlayerStates newAction ) {
		whatPreviousAction = whatAction;

		whatAction = newAction;
		DeactivateAllActions();
	}

	public IEnumerator lockAirMoves ( float time ) {
		lockBounce = true;
		lockJumpDash = true;
		lockHoming = true;
		lockDoubleJump = true;

		for (int s = 0 ; s < time ; s++)
		{
			yield return new WaitForFixedUpdate();
			if (_PlayerPhys._isGrounded)
				break;
		}

		lockBounce = false;
		lockJumpDash = false;
		lockHoming = false;
		lockDoubleJump = false;

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
	public IMainAction AssignControlledScriptByEnum ( S_Enums.PlayerControlledStates state ) {
		switch (state)
		{
			//If the enum of the struct is set to Jump, then assign the jump script to it.
			case S_Enums.PlayerControlledStates.Jump:
				TryGetComponent(out S_Action01_Jump jump);
				return jump;
			case S_Enums.PlayerControlledStates.Homing:
				TryGetComponent(out S_Action02_Homing home);
				return home;
			case S_Enums.PlayerControlledStates.SpinCharge:
				TryGetComponent(out S_Action03_SpinCharge spin);
				return spin;
			case S_Enums.PlayerControlledStates.Bounce:
				TryGetComponent(out S_Action06_Bounce bounce);
				return bounce;
			case S_Enums.PlayerControlledStates.DropCharge:
				TryGetComponent(out S_Action08_DropCharge drop);
				return drop;
			case S_Enums.PlayerControlledStates.JumpDash:
				TryGetComponent(out S_Action11_JumpDash dash);
				return dash;
		}
		return null;
	}
	public IMainAction AssignSituationalScriptByEnum ( S_Enums.PlayerSituationalStates state ) {
		switch (state)
		{
			//If the enum of the struct is set to Regular, then assign the jump script to it.
			case S_Enums.PlayerSituationalStates.Default:
				TryGetComponent(out S_Action00_Default def);
				return def;
			case S_Enums.PlayerSituationalStates.Hurt:
				TryGetComponent(out S_Action04_Hurt hurt);
				return hurt;
			case S_Enums.PlayerSituationalStates.Rail:
				TryGetComponent(out S_Action05_Rail rail);
				return rail;
			case S_Enums.PlayerSituationalStates.RingRoad:
				TryGetComponent(out S_Action07_RingRoad road);
				return road;
			case S_Enums.PlayerSituationalStates.Path:
				TryGetComponent(out S_Action10_FollowAutoPath path);
				return path;
			case S_Enums.PlayerSituationalStates.WallRunning:
				TryGetComponent(out S_Action12_WallRunning wall);
				return wall;
			case S_Enums.PlayerSituationalStates.Hovering:
				TryGetComponent(out S_Action13_Hovering hov);
				return hov;
		}
		return null;
	}

	public IMainAction AssignMainActionScriptByEnum ( S_Enums.PrimaryPlayerStates state ) {
		switch (state)
		{
			// If the enum of the struct is set to Jump, then assign the jump script to it.
			case S_Enums.PrimaryPlayerStates.Jump:
				if (TryGetComponent(out S_Action01_Jump jumpAction))
				{
					return jumpAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action01_Jump>();
				}

			case S_Enums.PrimaryPlayerStates.Homing:
				if (TryGetComponent(out S_Action02_Homing homingAction))
				{
					return homingAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action02_Homing>();
				}

			case S_Enums.PrimaryPlayerStates.SpinCharge:
				if (TryGetComponent(out S_Action03_SpinCharge spinChargeAction))
				{
					return spinChargeAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action03_SpinCharge>();
				}

			case S_Enums.PrimaryPlayerStates.Bounce:
				if (TryGetComponent(out S_Action06_Bounce bounceAction))
				{
					return bounceAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action06_Bounce>();
				}

			case S_Enums.PrimaryPlayerStates.DropCharge:
				if (TryGetComponent(out S_Action08_DropCharge dropChargeAction))
				{
					return dropChargeAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action08_DropCharge>();
				}

			case S_Enums.PrimaryPlayerStates.JumpDash:
				if (TryGetComponent(out S_Action11_JumpDash jumpDashAction))
				{
					return jumpDashAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action11_JumpDash>();
				}

			// If the enum of the struct is set to Default, then assign the jump script to it.
			case S_Enums.PrimaryPlayerStates.Default:
				if (TryGetComponent(out S_Action00_Default defaultAction))
				{
					return defaultAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action00_Default>();
				}

			case S_Enums.PrimaryPlayerStates.Hurt:
				if (TryGetComponent(out S_Action04_Hurt hurtAction))
				{
					return hurtAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action04_Hurt>();
				}

			case S_Enums.PrimaryPlayerStates.Rail:
				if (TryGetComponent(out S_Action05_Rail railAction))
				{
					return railAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action05_Rail>();
				}

			case S_Enums.PrimaryPlayerStates.RingRoad:
				if (TryGetComponent(out S_Action07_RingRoad ringRoadAction))
				{
					return ringRoadAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action07_RingRoad>();
				}

			case S_Enums.PrimaryPlayerStates.Path:
				if (TryGetComponent(out S_Action10_FollowAutoPath pathAction))
				{
					return pathAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action10_FollowAutoPath>();
				}

			case S_Enums.PrimaryPlayerStates.WallRunning:
				if (TryGetComponent(out S_Action12_WallRunning wallRunningAction))
				{
					return wallRunningAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action12_WallRunning>();
				}

			case S_Enums.PrimaryPlayerStates.Hovering:
				if (TryGetComponent(out S_Action13_Hovering hoveringAction))
				{
					return hoveringAction;
				}
				else
				{
					return gameObject.AddComponent<S_Action13_Hovering>();
				}
		}
		return null;
	}

	public ISubAction AssignSubScript ( S_Enums.SubPlayerStates state ) {
		switch (state)
		{
			// Case for S_SubAction_Skid
			case S_Enums.SubPlayerStates.Skidding:
				if (TryGetComponent(out S_SubAction_Skid skidAction))
				{
					return skidAction;
				}
				else
				{
					return gameObject.AddComponent<S_SubAction_Skid>();
				}

			// Case for S_SubAction_Quickstep
			case S_Enums.SubPlayerStates.Quickstepping:
				if (TryGetComponent(out S_SubAction_Quickstep quickstepAction))
				{
					return quickstepAction;
				}
				else
				{
					return gameObject.AddComponent<S_SubAction_Quickstep>();
				}

			// Case for S_SubAction_Roll
			case S_Enums.SubPlayerStates.Rolling:
				if (TryGetComponent(out S_SubAction_Roll rollAction))
				{
					return rollAction;
				}
				else
				{
					return gameObject.AddComponent<S_SubAction_Roll>();
				}
		}
		return null;
	}
	#endregion

}

#if UNITY_EDITOR
[CustomEditor(typeof(S_ActionManager))]
public class ActionManagerEditor : Editor
{
	S_ActionManager _ActionMan;
	GUIStyle headerStyle;
	GUIStyle BigButtonStyle;
	GUIStyle SmallButtonStyle;
	float spaceSize = 1;

	public override void OnInspectorGUI () {
		DrawInspector();
	}

	private void OnEnable () {
		//Setting variables
		_ActionMan = (S_ActionManager)target;

		if (_ActionMan.InspectorTheme == null) { return; }
		headerStyle = _ActionMan.InspectorTheme._MainHeaders;
		BigButtonStyle = _ActionMan.InspectorTheme._GeneralButton;
		SmallButtonStyle = _ActionMan.InspectorTheme._ResetButton;
		spaceSize = _ActionMan.InspectorTheme._spaceSize;
	}

	private void DrawInspector () {

		//The inspector needs a visual theme to use, this makes it available and only displays the rest after it is set.
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("InspectorTheme"), new GUIContent("Inspector Theme"));
		serializedObject.ApplyModifiedProperties();
		if (EditorGUI.EndChangeCheck())
		{
			headerStyle = _ActionMan.InspectorTheme._MainHeaders;
			BigButtonStyle = _ActionMan.InspectorTheme._GeneralButton;
			SmallButtonStyle = _ActionMan.InspectorTheme._ResetButton;
			spaceSize = _ActionMan.InspectorTheme._spaceSize;
		}

		//Will only happen if above is attatched.
		if (_ActionMan == null || _ActionMan.InspectorTheme == null) return;

		serializedObject.Update();

		//Describe what the script does
		EditorGUILayout.TextArea("Details.", EditorStyles.textArea);

		SerializedProperty ActionList = serializedObject.FindProperty("_MainActions");

		//Order of Drawing
		EditorGUILayout.Space(spaceSize);
		DrawAddAction();
		DrawMissingScripts();
		DrawReorderActions();
		DrawActions();

		//List of current actions
		#region Actions
		void DrawActions () {
			EditorGUILayout.Space(spaceSize);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_addState"), new GUIContent("Add This To Actions"));

			EditorGUI.BeginChangeCheck();

			//Draw each element in the list.
			for (int i = 0 ; i < ActionList.arraySize ; i++)
			{
				EditorGUILayout.Space(spaceSize / 1.5f);

				//Draw the list element.
				GUILayout.BeginHorizontal();
				SerializedProperty element = ActionList.GetArrayElementAtIndex(i);
				EditorGUILayout.PropertyField(element, new GUIContent("Action " + i + " - " + _ActionMan._MainActions[i].State));

				//Pressing this button inserts the state labled at the top to be transitined to from the current state.
				if (GUILayout.Button("Add Set", SmallButtonStyle))
				{
					AddActionToThis(_ActionMan._addState, i);

				}


				//Remove this element button.
				if (GUILayout.Button("Remove", SmallButtonStyle))
				{
					ActionList.DeleteArrayElementAtIndex(i);
					serializedObject.ApplyModifiedProperties();
				}
				GUILayout.EndHorizontal();
			}
			serializedObject.ApplyModifiedProperties();
		}

		//Button for adding new action
		void DrawAddAction () {
			//Each element of the list is shown in the inspector seperately, rather than under one header. Therefore we need custom add and remove buttons.

			//Add new element button.
			Undo.RecordObject(_ActionMan, "Add New State");
			if (GUILayout.Button("Add New State", BigButtonStyle))
			{
				AddActionToList(S_Enums.PrimaryPlayerStates.Default, true, true);
				serializedObject.Update();
			}
			serializedObject.ApplyModifiedProperties();
		}

		//Button for making sure the object has the components necessary to its actions.
		void DrawMissingScripts () {
			Undo.RecordObject(_ActionMan, "Import Missing");
			if (GUILayout.Button("Import Missing", BigButtonStyle))
			{

				for (int i = 0 ; i < _ActionMan._MainActions.Count ; i++)
				{
					//Go through each struct and use the AssignScript to add missing components.
					S_Structs.StrucMainActionTracker action = _ActionMan._MainActions[i];


					//Go through all of the connected states for each state, and make sure those states are also in the list.
					foreach (S_Enums.PlayerSituationalStates state in action.SituationalStates)
					{
						AddSituationalActionToList(state);
					}
					foreach (S_Enums.PlayerControlledStates state in action.ConnectedStates)
					{
						AddControledActionToList(state);
					}

					//Makes sure every action still has default as as connected situation interaction.
					if (!action.SituationalStates.Any(item => item == S_Enums.PlayerSituationalStates.Default))
					{
						action.SituationalStates.Insert(0, S_Enums.PlayerSituationalStates.Default);
					}

					//Makes sure every action still has Hurt as as connected situation interaction.
					if (!action.SituationalStates.Any(item => item == S_Enums.PlayerSituationalStates.Hurt))
					{
						action.SituationalStates.Insert(0, S_Enums.PlayerSituationalStates.Hurt);
					}

					_ActionMan.AssignMainActionScriptByEnum(action.State);

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
			Undo.RecordObject(_ActionMan, "Reorder Actions");
			if (GUILayout.Button("Sort all actions", BigButtonStyle))
			{
				_ActionMan._MainActions = _ActionMan._MainActions.OrderBy(item => item.State).ToList();
				serializedObject.ApplyModifiedProperties();
			}
		}
		#endregion
	}

	//Take in a primary state and add it to the list if it cant be found.
	private void AddActionToList ( S_Enums.PrimaryPlayerStates state, bool withDefault = false, bool skip = false ) {
		if (!_ActionMan._MainActions.Any(item => item.State == state) || skip)
		{
			S_Structs.StrucMainActionTracker temp = new S_Structs.StrucMainActionTracker();
			temp.State = state;

			if (withDefault)
			{
				temp.SituationalStates = new List<S_Enums.PlayerSituationalStates>();
				temp.SituationalStates.Add(S_Enums.PlayerSituationalStates.Default);
			}

			_ActionMan._MainActions.Add(temp);
		}
	}

	//Go through all situation actions and add the equivilant main action.
	private void AddSituationalActionToList ( S_Enums.PlayerSituationalStates state ) {

		switch (state)
		{
			case S_Enums.PlayerSituationalStates.Rail:
				AddActionToList(S_Enums.PrimaryPlayerStates.Rail);
				break;
			case S_Enums.PlayerSituationalStates.Path:
				AddActionToList(S_Enums.PrimaryPlayerStates.Path);
				break;
			case S_Enums.PlayerSituationalStates.RingRoad:
				AddActionToList(S_Enums.PrimaryPlayerStates.RingRoad);
				break;
			case S_Enums.PlayerSituationalStates.WallRunning:
				AddActionToList(S_Enums.PrimaryPlayerStates.WallRunning);
				break;
			case S_Enums.PlayerSituationalStates.Default:
				AddActionToList(S_Enums.PrimaryPlayerStates.Default);
				break;
			case S_Enums.PlayerSituationalStates.Hovering:
				AddActionToList(S_Enums.PrimaryPlayerStates.Hovering);
				break;
			case S_Enums.PlayerSituationalStates.Hurt:
				AddActionToList(S_Enums.PrimaryPlayerStates.Hurt);
				break;
		}
	}

	//Go through all controlled actions and add the equivilant main action.
	private void AddControledActionToList ( S_Enums.PlayerControlledStates state ) {
		switch (state)
		{
			case S_Enums.PlayerControlledStates.Jump:
				AddActionToList(S_Enums.PrimaryPlayerStates.Jump);
				break;
			case S_Enums.PlayerControlledStates.Homing:
				AddActionToList(S_Enums.PrimaryPlayerStates.Homing);
				break;
			case S_Enums.PlayerControlledStates.SpinCharge:
				AddActionToList(S_Enums.PrimaryPlayerStates.SpinCharge);
				break;
			case S_Enums.PlayerControlledStates.Bounce:
				AddActionToList(S_Enums.PrimaryPlayerStates.Bounce);
				break;
			case S_Enums.PlayerControlledStates.DropCharge:
				AddActionToList(S_Enums.PrimaryPlayerStates.DropCharge);
				break;
			case S_Enums.PlayerControlledStates.JumpDash:
				AddActionToList(S_Enums.PrimaryPlayerStates.JumpDash);
				break;
		}
	}

	//Takes in a main action and connects it to the current state as one that can be transitioned to.
	void AddActionToThis ( S_Enums.PrimaryPlayerStates state, int target ) {
		if (_ActionMan._MainActions[target].State == state) { return; }

		switch (state)
		{
			case S_Enums.PrimaryPlayerStates.Default:
				_ActionMan._MainActions[target].SituationalStates.Add(S_Enums.PlayerSituationalStates.Default);
				break;
			case S_Enums.PrimaryPlayerStates.Jump:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_Enums.PlayerControlledStates.Jump);
				break;
			case S_Enums.PrimaryPlayerStates.Homing:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_Enums.PlayerControlledStates.Homing);
				break;
			case S_Enums.PrimaryPlayerStates.SpinCharge:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_Enums.PlayerControlledStates.SpinCharge);
				break;
			case S_Enums.PrimaryPlayerStates.Hurt:
				_ActionMan._MainActions[target].SituationalStates.Add(S_Enums.PlayerSituationalStates.Hurt);
				break;
			case S_Enums.PrimaryPlayerStates.Rail:
				_ActionMan._MainActions[target].SituationalStates.Add(S_Enums.PlayerSituationalStates.Rail);
				break;
			case S_Enums.PrimaryPlayerStates.Bounce:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_Enums.PlayerControlledStates.Bounce);
				break;
			case S_Enums.PrimaryPlayerStates.RingRoad:
				_ActionMan._MainActions[target].SituationalStates.Add(S_Enums.PlayerSituationalStates.RingRoad);
				break;
			case S_Enums.PrimaryPlayerStates.DropCharge:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_Enums.PlayerControlledStates.DropCharge);
				break;
			case S_Enums.PrimaryPlayerStates.Path:
				_ActionMan._MainActions[target].SituationalStates.Add(S_Enums.PlayerSituationalStates.Path);
				break;
			case S_Enums.PrimaryPlayerStates.JumpDash:
				_ActionMan._MainActions[target].ConnectedStates.Add(S_Enums.PlayerControlledStates.JumpDash);
				break;
			case S_Enums.PrimaryPlayerStates.WallRunning:
				_ActionMan._MainActions[target].SituationalStates.Add(S_Enums.PlayerSituationalStates.WallRunning);
				break;
			case S_Enums.PrimaryPlayerStates.Hovering:
				_ActionMan._MainActions[target].SituationalStates.Add(S_Enums.PlayerSituationalStates.Hovering);
				break;

		}
	}
}
#endif
