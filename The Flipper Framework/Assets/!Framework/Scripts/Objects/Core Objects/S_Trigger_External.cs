using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_Trigger_External : S_Trigger_Base
{
	public StrucGeneralTriggerObjects _TriggerObjects = new StrucGeneralTriggerObjects()
	{
		_ObjectsToTriggerOn = new List<GameObject>(),
		_ObjectsToTriggerOff = new List<GameObject>(),
	};

	private List<GameObject> _RememberObjectsToTriggerOn = new List<GameObject>(); //Only used to check when the above list is edited.
	private List<GameObject> _RememberObjectsToTriggerOff = new List<GameObject>(); //Only used to check when the above list is edited.

	[Serializable]
	[Tooltip("This data related to general triggering of other objects, which certain triggers won't need. Will interact with the ITriggerable Interface")]
	public struct StrucGeneralTriggerObjects
	{
		public bool         _triggerSelfOn;
		public bool         _triggerSelfOff;

		public List<GameObject> _ObjectsToTriggerOn;
		public List<GameObject> _ObjectsToTriggerOff;
	}

	[CustomReadOnly, Tooltip("This will be true if any trigger will activate this one. That includes itself or another.")]
	[ColourIfEqualTo(false, 0.9f,0.5f,0.5f,1f)]
	public bool _hasTrigger;
	[CustomReadOnly] public List<GameObject> _ObjectsThatTriggerThis = new List<GameObject>();

	[CustomReadOnly]
	[Tooltip("Set true in code for triggers where their effects are scripted in a player script. _TriggerForPlayerToRead provides the data for said script.")]
	public bool         _isLogicInPlayerScript;
	[OnlyDrawIf("_isLogicInPlayerScript", true)]
	[CustomReadOnly, Tooltip("When the player enters this trigger, this will be what they base the effect on. If this is set to trigger self, it will be this, if not, it will take from ObjectsToTrigger")]
	public List<GameObject> _TriggersForPlayerToRead = new List<GameObject>();

	public void OnTriggerEnter ( Collider other ) {
		if (other.tag != "Player") { return; }
		other.TryGetComponent(out _Player);
		if (!_Player) { _Player = other.GetComponentInParent<S_PlayerPhysics>(); }

		TriggerGivenObjects(TriggerTypes.On, _TriggerObjects._ObjectsToTriggerOn);
		TriggerGivenObjects(TriggerTypes.Either, _TriggerObjects._ObjectsToTriggerOn);
	}

	public void OnTriggerStay ( Collider other ) {
		if (other.tag != "Player") { return; }


		TriggerGivenObjects(TriggerTypes.Frame, _TriggerObjects._ObjectsToTriggerOn);
	}

	public void OnTriggerExit ( Collider other ) {
		if (other.tag != "Player") { return; }

		TriggerGivenObjects(TriggerTypes.Off, _TriggerObjects._ObjectsToTriggerOff);
	}

	public virtual void TriggerGivenObjects ( TriggerTypes triggerType, List<GameObject> gameObjects ) {
		//Go through each given gameObject and trigger if possible.
		for (int i = 0 ; i < gameObjects.Count ; i++)
		{
			GameObject thisObject = gameObjects[i];
			if (!thisObject) { continue; }
			if (thisObject.TryGetComponent(out ITriggerable Trigger))
			{
				switch (triggerType)
				{
					case TriggerTypes.On: Trigger.TriggerObjectOn(_Player); break;

					case TriggerTypes.Off: Trigger.TriggerObjectOff(_Player); break;

					case TriggerTypes.Either: Trigger.TriggerObjectEither(_Player); break;

					case TriggerTypes.Reset: Trigger.ResetObject(_Player); break;

					case TriggerTypes.Frame: Trigger.TriggerObjectEachFrame(_Player); break;
				}
			}
		}
	}


#if UNITY_EDITOR
	public override void OnValidate () {
		base.OnValidate();
		UpdateExternalTriggers();

		if (_TriggerObjects._triggerSelfOn)
		{
			CheckExternalTriggerDataHasTrigger(false, this);
			SetSelfToTriggerObjects(ref _TriggerObjects._ObjectsToTriggerOn, true);
		}
		else
		{
			CheckExternalTriggerDataHasTrigger(true, this);
			SetSelfToTriggerObjects(ref _TriggerObjects._ObjectsToTriggerOn, false);
		}

		if (_TriggerObjects._triggerSelfOff)
			SetSelfToTriggerObjects(ref _TriggerObjects._ObjectsToTriggerOff, true);
		else
			SetSelfToTriggerObjects(ref _TriggerObjects._ObjectsToTriggerOff, false);


		CheckTriggerForPlayerToRead();
	}

	private void SetSelfToTriggerObjects ( ref List<GameObject> TriggerList, bool add ) {
		bool included = TriggerList.Contains(gameObject);

		if (included && !add) { TriggerList.Remove(gameObject); }
		else if (!included && add) { TriggerList.Add(gameObject); }
	}

#endif

#if UNITY_EDITOR
	private void UpdateExternalTriggers () {

		//Goes through what is set to trigger on, and ensure it knows this is set to trigger it.
		if (_TriggerObjects._ObjectsToTriggerOn.Count > 0)
		{
			for (int i = 0 ; i < _TriggerObjects._ObjectsToTriggerOn.Count ; i++)
			{
				if (!_TriggerObjects._ObjectsToTriggerOn[i]) { continue; }
				if (_TriggerObjects._ObjectsToTriggerOn[i].TryGetComponent(out S_Trigger_External ExternalTriggerData))
				{
					CheckExternalTriggerDataHasTrigger(false, ExternalTriggerData); //Add to
				}
			}
		}

		//Finds what is not in the list that was before, and ensures that object knows it is no longer referenced by this.
		for (int i = 0 ; i < _RememberObjectsToTriggerOn.Count ; i++)
		{
			if (!_RememberObjectsToTriggerOn[i]) { continue; }
			if (_TriggerObjects._ObjectsToTriggerOn.Count > 0)
			{
				if (!_TriggerObjects._ObjectsToTriggerOn[i]) { continue; }
				if (_TriggerObjects._ObjectsToTriggerOn.Contains(_RememberObjectsToTriggerOn[i])) { continue; }
			}

			if (_RememberObjectsToTriggerOn[i].TryGetComponent(out S_Trigger_External ExternalTriggerData))
			{
				CheckExternalTriggerDataHasTrigger(true, ExternalTriggerData); //Remove from
			}
		}

		_RememberObjectsToTriggerOn = _TriggerObjects._ObjectsToTriggerOn;
		_RememberObjectsToTriggerOff = _TriggerObjects._ObjectsToTriggerOff;
	}

	private void CheckTriggerForPlayerToRead () {

		//First, clear the triggers to read, and their references to this, so they can be readded if necessary
		for (int trig = 0 ; trig < _TriggersForPlayerToRead.Count ; trig++)
		{
			//Remove each reading trigger's reference to what needs to be read now, as it may no longer have that reference soon.
			if (_TriggersForPlayerToRead[trig] && _TriggersForPlayerToRead[trig].TryGetComponent(out S_Trigger_External TriggerData))
			{
				CheckExternalTriggerDataHasTrigger(true, TriggerData);
			}
		}
		_TriggersForPlayerToRead.Clear();

		//Go through each object to trigger on (remember, this inlcudes itself is _TriggerSelfOn, and if they have logic to read, add them to the list of scripts to read.
		for (int i = 0 ; i < _TriggerObjects._ObjectsToTriggerOn.Count ; i++)
		{
			if (_TriggerObjects._ObjectsToTriggerOn[i].TryGetComponent(out S_Trigger_External Trigger))
			{
				if (Trigger._isLogicInPlayerScript)
				{
					_TriggersForPlayerToRead.Add(Trigger.gameObject);
					CheckExternalTriggerDataHasTrigger(false, Trigger);
				}
			}
		}
	}


	private void CheckExternalTriggerDataHasTrigger ( bool removeThisTrigger, S_Trigger_External TargetTriggerData ) {

		if (removeThisTrigger)
		{
			//If the old TriggerToRead was triggered by this, remove it from that list, and if that's the last one, it has no current trigger.
			if (TargetTriggerData._ObjectsThatTriggerThis.Contains(gameObject))
			{
				TargetTriggerData._ObjectsThatTriggerThis.Remove(gameObject);
			}
			if (TargetTriggerData._ObjectsThatTriggerThis.Count == 0) { TargetTriggerData._hasTrigger = false; }
		}
		else
		{
			//If the new Trigger To read doesn't have this as an object activating it, add it.
			if (!TargetTriggerData._ObjectsThatTriggerThis.Contains(gameObject))
			{
				TargetTriggerData._ObjectsThatTriggerThis.Add(gameObject);
				TargetTriggerData._hasTrigger = true;
			}
		}
	}

	public override void DrawAdditionalGizmos ( bool selected, Color colour ) {
		using (new Handles.DrawingScope(colour))
		{

			for (int i = 0 ; i < _TriggerObjects._ObjectsToTriggerOn.Count ; i++)
			{
				if (_TriggerObjects._ObjectsToTriggerOn[i] == null) continue;
				Handles.DrawLine(transform.position, _TriggerObjects._ObjectsToTriggerOn[i].transform.position, 5f);
			}
			for (int i = 0 ; i < _TriggerObjects._ObjectsToTriggerOff.Count ; i++)
			{
				if (_TriggerObjects._ObjectsToTriggerOff[i] == null) continue;
				Handles.DrawDottedLine(transform.position, _TriggerObjects._ObjectsToTriggerOff[i].transform.position, 5f);
			}
		}
	}
#endif
}

