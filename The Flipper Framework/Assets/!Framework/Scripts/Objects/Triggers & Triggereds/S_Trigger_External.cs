using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class S_Trigger_External : S_Trigger_Base
{
	[SerializeAs("_startTrigered")]
	public bool _triggerOnStart;
	public TriggerExternalData TriggerObjects = new TriggerExternalData();

	private float _delayBetweenMultiTriggers = 0.3f;
	private float _timeSinceTriggered = 0;
	private bool _wastriggered;


	private void Start () {
		if(_triggerOnStart) { StartCoroutine(DelayBeforeTrigger()); }
	}

	IEnumerator DelayBeforeTrigger () {
		yield return new WaitForSeconds(1);
		if (!Application.isPlaying) { yield break; }
		TriggerGivenObjects(TriggerTypes.Start, TriggerObjects._ObjectsToTriggerOn, null);
	}

	public void OnTriggerEnter ( Collider other ) {
		if (other.tag != "Player") { return; }
		other.TryGetComponent(out _Player);
		if (!_Player) { _Player = other.GetComponentInParent<S_PlayerPhysics>(); }

		if (_wastriggered) { return; } //Enforces a delay between when and when not the trigger can be activated.

		_wastriggered = true;
		_timeSinceTriggered = 0;

		TriggerGivenObjects(TriggerTypes.On, TriggerObjects._ObjectsToTriggerOn, _Player);
		TriggerGivenObjects(TriggerTypes.Either, TriggerObjects._ObjectsToTriggerOn, _Player);
	}

	public void OnTriggerStay ( Collider other ) {
		if (other.tag != "Player") { return; }

		if (_wastriggered) { return; } //Enforces a delay between when and when not the trigger can be activated.

		TriggerGivenObjects(TriggerTypes.Frame, TriggerObjects._ObjectsToTriggerOn, _Player);
	}

	public void OnTriggerExit ( Collider other ) {
		if (other.tag != "Player") { return; }

		TriggerGivenObjects(TriggerTypes.Off, TriggerObjects._ObjectsToTriggerOff, _Player);
	}

	public static void TriggerGivenObjects ( TriggerTypes triggerType, List<GameObject> gameObjects, S_PlayerPhysics Player ) {
		if(gameObjects.Count == 0) { return; }

		//Go through each given gameObject and trigger if possible.
		for (int i = 0 ; i < gameObjects.Count ; i++)
		{
			GameObject thisObject = gameObjects[i];
			if (!thisObject) { continue; }

			//In case object has multiple triggerable components.
			ITriggerable[] Triggers = thisObject.GetComponents<ITriggerable>();

			for (int triggerable = 0 ; triggerable < Triggers.Length ; triggerable++)
			{
				ITriggerable Trigger = Triggers[triggerable];
				if(Trigger == null) { continue; }

				switch (triggerType)
				{
					case TriggerTypes.On: Trigger.TriggerObjectOn(Player); break;

					case TriggerTypes.Off: Trigger.TriggerObjectOff(Player); break;

					case TriggerTypes.Either: Trigger.TriggerObjectEither(Player); break;

					case TriggerTypes.Reset: Trigger.ResetObject(Player); break;

					case TriggerTypes.Frame: Trigger.TriggerObjectEachFrame(Player); break;

					case TriggerTypes.Start: Trigger.StartTriggeredOn(Player); break;
				}
			}
		}
	}

	private void Update () {
		if (_wastriggered)
		{
			_timeSinceTriggered += Time.deltaTime;
			_wastriggered = _timeSinceTriggered >= _delayBetweenMultiTriggers;
		}
	}


#if UNITY_EDITOR
	public override void OnValidate () {
		base.OnValidate();
		UpdateExternalTriggers();

		if (TriggerObjects._triggerSelfOn)
		{
			CheckExternalTriggerDataHasTrigger(false, this);
			SetSelfToTriggerObjects(ref TriggerObjects._ObjectsToTriggerOn, true);
		}
		else
		{
			CheckExternalTriggerDataHasTrigger(true, this);
			SetSelfToTriggerObjects(ref TriggerObjects._ObjectsToTriggerOn, false);
		}

		if (TriggerObjects._triggerSelfOff)
			SetSelfToTriggerObjects(ref TriggerObjects._ObjectsToTriggerOff, true);
		else
			SetSelfToTriggerObjects(ref TriggerObjects._ObjectsToTriggerOff, false);


		CheckTriggerForPlayerToRead();
	}

	private void SetSelfToTriggerObjects ( ref List<GameObject> TriggerList, bool add ) {
		bool included = TriggerList.Contains(gameObject);

		if (included && !add) { TriggerList.Remove(gameObject); }
		else if (!included && add) { TriggerList.Add(gameObject); }
	}

	private void UpdateExternalTriggers () {

		//Goes through what is set to trigger on, and ensure it knows this is set to trigger it.
		if (TriggerObjects._ObjectsToTriggerOn.Count > 0)
		{
			for (int i = 0 ; i < TriggerObjects._ObjectsToTriggerOn.Count ; i++)
			{
				if (!TriggerObjects._ObjectsToTriggerOn[i]) { continue; }
				if (TriggerObjects._ObjectsToTriggerOn[i].TryGetComponent(out S_Trigger_External ExternalTriggerData))
				{
					CheckExternalTriggerDataHasTrigger(false, ExternalTriggerData); //Add to
				}
			}
		}

		//Finds what is not in the list that was before, and ensures that object knows it is no longer referenced by this.
		for (int i = 0 ; i < TriggerObjects._RememberObjectsToTriggerOn.Count ; i++)
		{
			if (!TriggerObjects._RememberObjectsToTriggerOn[i]) { continue; }
			if (TriggerObjects._ObjectsToTriggerOn.Count > 0)
			{
				if (!TriggerObjects._ObjectsToTriggerOn[i]) { continue; }
				if (TriggerObjects._ObjectsToTriggerOn.Contains(TriggerObjects._RememberObjectsToTriggerOn[i])) { continue; }
			}

			if (TriggerObjects._RememberObjectsToTriggerOn[i].TryGetComponent(out S_Trigger_External ExternalTriggerData))
			{
				CheckExternalTriggerDataHasTrigger(true, ExternalTriggerData); //Remove from
			}
		}

		TriggerObjects._RememberObjectsToTriggerOn = TriggerObjects._ObjectsToTriggerOn;
		TriggerObjects._RememberObjectsToTriggerOff = TriggerObjects._ObjectsToTriggerOff;
	}

	private void CheckTriggerForPlayerToRead () {

		//First, clear the triggers to read, and their references to this, so they can be readded if necessary
		for (int trig = 0 ; trig < TriggerObjects._TriggersForPlayerToRead.Count ; trig++)
		{
			//Remove each reading trigger's reference to what needs to be read now, as it may no longer have that reference soon.
			if (TriggerObjects._TriggersForPlayerToRead[trig] && TriggerObjects._TriggersForPlayerToRead[trig].TryGetComponent(out S_Trigger_External TriggerData))
			{
				CheckExternalTriggerDataHasTrigger(true, TriggerData);
			}
		}
		TriggerObjects._TriggersForPlayerToRead.Clear();

		//Go through each object to trigger on (remember, this inlcudes itself is _TriggerSelfOn, and if they have logic to read, add them to the list of scripts to read.
		for (int i = 0 ; i < TriggerObjects._ObjectsToTriggerOn.Count ; i++)
		{
			if(!TriggerObjects._ObjectsToTriggerOn[i]) { continue; }
			if (TriggerObjects._ObjectsToTriggerOn[i].TryGetComponent(out S_Trigger_External Trigger))
			{
				if (Trigger.TriggerObjects._isLogicInPlayerScript)
				{
					TriggerObjects._TriggersForPlayerToRead.Add(Trigger.gameObject);
					CheckExternalTriggerDataHasTrigger(false, Trigger);
				}
			}
		}
	}


	private void CheckExternalTriggerDataHasTrigger ( bool removeThisTrigger, S_Trigger_External TargetTriggerData ) {

		if (removeThisTrigger)
		{
			//If the old TriggerToRead was triggered by this, remove it from that list, and if that's the last one, it has no current trigger.
			if (TargetTriggerData.TriggerObjects._ObjectsThatTriggerThis.Contains(gameObject))
			{
				TargetTriggerData.TriggerObjects._ObjectsThatTriggerThis.Remove(gameObject);
			}
			if (TargetTriggerData.TriggerObjects._ObjectsThatTriggerThis.Count == 0) { TargetTriggerData.TriggerObjects._hasTrigger = false; }
		}
		else
		{
			//If the new Trigger To read doesn't have this as an object activating it, add it.
			if (!TargetTriggerData.TriggerObjects._ObjectsThatTriggerThis.Contains(gameObject))
			{
				TargetTriggerData.TriggerObjects._ObjectsThatTriggerThis.Add(gameObject);
				TargetTriggerData.TriggerObjects._hasTrigger = true;
			}
		}
	}

	public override void DrawAdditionalGizmos ( bool selected, Color colour ) {
		using (new Handles.DrawingScope(colour))
		{

			for (int i = 0 ; i < TriggerObjects._ObjectsToTriggerOn.Count ; i++)
			{
				if (TriggerObjects._ObjectsToTriggerOn[i] == null) continue;
				Handles.DrawLine(transform.position, TriggerObjects._ObjectsToTriggerOn[i].transform.position, 5f);
			}
			for (int i = 0 ; i < TriggerObjects._ObjectsToTriggerOff.Count ; i++)
			{
				if (TriggerObjects._ObjectsToTriggerOff[i] == null) continue;
				Handles.DrawDottedLine(transform.position, TriggerObjects._ObjectsToTriggerOff[i].transform.position, 5f);
			}
		}
	}
#endif
}

[Serializable]
public class TriggerExternalData
{
	public bool         _triggerSelfOn;
	public bool         _triggerSelfOff;

	public List<GameObject> _ObjectsToTriggerOn = new List<GameObject>();
	public List<GameObject> _ObjectsToTriggerOff = new List<GameObject>();

	[NonSerialized]
	public List<GameObject> _RememberObjectsToTriggerOn = new List<GameObject>(); //Only used to check when the above list is edited.
	[NonSerialized]
	public List<GameObject> _RememberObjectsToTriggerOff = new List<GameObject>(); //Only used to check when the above list is edited.

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
}

