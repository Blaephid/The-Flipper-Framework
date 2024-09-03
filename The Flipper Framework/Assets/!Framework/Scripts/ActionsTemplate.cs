using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace templates
{
	public class ActionsTemplate : MonoBehaviour, IMainAction
	{
		/// <summary>
		/// Properties ----------------------------------------------------------------------------------
		/// </summary>
		/// 
		#region properties

		//Unity
		#region Unity Specific Properties
		private S_CharacterTools      _Tools;
		private S_PlayerPhysics       _PlayerPhys;
		private S_PlayerInput         _Input;
		private S_ActionManager       _Actions;
		#endregion



		//Stats - See Stats scriptable objects for tooltips explaining their purpose.
		#region Stats
		#endregion

		// Trackers
		#region trackers
		private int         _positionInActionList;
		#endregion
		#endregion

		/// <summary>
		/// Inherited ----------------------------------------------------------------------------------
		/// </summary>
		/// 
		#region Inherited

		// Start is called before the first frame update
		void Start () {

		}

		// Update is called once per frame
		void Update () {

		}

		private void FixedUpdate () {

		}

		public bool AttemptAction () {
			return false;
		}

		public void StartAction (bool overwrite = false) {
			if (enabled || (!_Actions._canChangeActions && !overwrite)) { return; }
		}

		public void StopAction ( bool isFirstTime = false ) {
			if (!enabled) { return; } //If already disabled, return as nothing needs to change.
			enabled = false; 
			if (isFirstTime) { ReadyAction(); return; } //First time is called on ActionManager Awake() to ensure this starts disabled and has a single opportunity to assign tools and stats.
		}

		#endregion

		/// <summary>
		/// Private ----------------------------------------------------------------------------------
		/// </summary>
		/// 
		#region private

		public void HandleInputs () {
			//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
			_Actions.HandleInputs(_positionInActionList);
		}

		#endregion

		/// <summary>
		/// Public ----------------------------------------------------------------------------------
		/// </summary>
		/// 
		#region public 

		#endregion

		/// <summary>
		/// Assigning ----------------------------------------------------------------------------------
		/// </summary>
		#region Assigning

		//If not assigned already, sets the tools and stats and gets placement in Action Manager's action list.
		public void ReadyAction () {
			if (_PlayerPhys == null)
			{

				//Assign all external values needed for gameplay.
				_Tools = GetComponentInParent<S_CharacterTools>();
				AssignTools();
				AssignStats();

				//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
				for (int i = 0 ; i < _Actions._MainActions.Count ; i++)
				{
					if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Homing)
					{
						_positionInActionList = i;
						break;
					}
				}
			}
		}

		//Responsible for assigning objects and components from the tools script.
		private void AssignTools () {
			_Input = _Tools.GetComponent<S_PlayerInput>();
			_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
			_Actions = _Tools._ActionManager;
		}

		//Reponsible for assigning stats from the stats script.
		private void AssignStats () {

		}
		#endregion
	}
}

