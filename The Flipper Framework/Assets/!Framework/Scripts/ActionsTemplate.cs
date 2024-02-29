using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace templates
{
	[RequireComponent(typeof(S_ActionManager))]
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

		//General
		#region General Properties

		//Stats
		#region Stats
		#endregion

		// Trackers
		#region trackers
		private int         _positionInActionList;
		#endregion

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

		// Called when the script is enabled, but will only assign the tools and stats on the first time.
		private void OnEnable () {
			if (_PlayerPhys == null)
			{
				//Assign all external values needed for gameplay.
				_Tools = GetComponent<S_CharacterTools>();
				AssignTools();
				AssignStats();

				//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
				for (int i = 0 ; i < _Actions._MainActions.Count ; i++)
				{
					if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Default)
					{
						Debug.Log("This is found at " + i);
						_positionInActionList = i;
						break;
					}
				}
			}
		}
		private void OnDisable () {
			
		}

		// Update is called once per frame
		void Update () {

		}

		private void FixedUpdate () {

		}

		public bool AttemptAction () {
			bool willChangeAction = false;
			willChangeAction = true;
			return willChangeAction;
		}

		public void StartAction () {

		}

		public void StopAction () {

		}

		#endregion

		/// <summary>
		/// Private ----------------------------------------------------------------------------------
		/// </summary>
		/// 
		#region private

		public void HandleInputs () {
			if (!_Actions.isPaused)
			{
				//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
				_Actions.HandleInputs(_positionInActionList);
			}
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

		//Responsible for assigning objects and components from the tools script.
		private void AssignTools () {
			_Input = GetComponent<S_PlayerInput>();
			_PlayerPhys = GetComponent<S_PlayerPhysics>();
			_Actions = GetComponent<S_ActionManager>();
		}

		//Reponsible for assigning stats from the stats script.
		private void AssignStats () {

		}
		#endregion
	}
}

