using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

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
	public GameObject _TriggerForPlayerToRead;

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
					case TriggerTypes.On: Trigger.TriggerObjectOn(_Player); return;

					case TriggerTypes.Off: Trigger.TriggerObjectOff(_Player); return;

					case TriggerTypes.Either: Trigger.TriggerObjectEither(_Player); return;

					case TriggerTypes.Reset: Trigger.ResetObject(_Player); return;

					case TriggerTypes.Frame: Trigger.TriggerObjectEachFrame(_Player); return;
				}
			}
		}
	}
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
		//If this trigger doesn't perform its logic in a player script, then this isn't needed, so return null.
		if (!_isLogicInPlayerScript)
		{ SetTriggerForPlayerToRead(null); return; }

		//If set to trigger self, then this is the logic the player will need to reference.
		if (_TriggerObjects._triggerSelfOn)

		{ SetTriggerForPlayerToRead(gameObject); return; }

		//Otherwise Get the class derived class using this as a type, then look through objects that will be triggered and see if they match.
		//E.G. S_Trigger_Camera will look for other S_Trigger_Cameras. And the first will be what to read.
		System.Type scriptType = GetType();

		for (int i = 0 ; i < _TriggerObjects._ObjectsToTriggerOn.Count ; i++)
		{
			if (_TriggerObjects._ObjectsToTriggerOn[i].GetComponent(scriptType))
			{ SetTriggerForPlayerToRead(_TriggerObjects._ObjectsToTriggerOn[i].gameObject); return; }
		}

		//If none, then this trigger has no trigger data for the player.
		SetTriggerForPlayerToRead(null);
	}

	//Ensures the GameObject that acts as the source of data knows this, and will display in its own inspector.
	private void SetTriggerForPlayerToRead ( GameObject SetTo ) {

		//If not changing, none of this is needed.
		if ((!_TriggerForPlayerToRead && !SetTo) || (SetTo && _TriggerForPlayerToRead == SetTo)) { return; }

		//Remove this trigger's reference to what needs to be read now, as it may no longer have that reference soon.
		if (_TriggerForPlayerToRead && _TriggerForPlayerToRead.TryGetComponent(out S_Trigger_External TriggerData))
		{ CheckExternalTriggerDataHasTrigger(true, TriggerData); }

		_TriggerForPlayerToRead = SetTo;

		if (!(_TriggerForPlayerToRead && _TriggerForPlayerToRead.TryGetComponent(out S_Trigger_External TriggerData2))) { return; }
		//If valid TriggerToRead, update it as being triggered by this, even if its itself.
		CheckExternalTriggerDataHasTrigger(false, TriggerData2);
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
}

