using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace templates
{
	public class ActionsTemplate : S_Action_Base, IMainAction
	{
		/// <summary>
		/// Properties ----------------------------------------------------------------------------------
		/// </summary>
		/// 
		#region properties

		//Unity

		//Stats - See Stats scriptable objects for tooltips explaining their purpose.

		// Trackers
	

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

		new private void FixedUpdate () {
			base.FixedUpdate();

		}

		new public  bool AttemptAction () {
			if (!base.AttemptAction()) return false;
			return false;
		}

		new public void StartAction (bool overwrite = false) {
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
		public override void AssignTools () {
			base.AssignTools();
		}

		//Reponsible for assigning stats from the stats script.
		public override void AssignStats () {
			
		}
		#endregion
	}
}

