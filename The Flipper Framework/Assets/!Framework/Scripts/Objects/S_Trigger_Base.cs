using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public class S_Trigger_Base : S_Data_Base
{

	[Header("Trigger")]
	public List<GameObject> _ObjectsToTriggerOn = new List<GameObject>();
	public List<GameObject> _ObjectsToTriggerOff = new List<GameObject>();
	public bool         _triggerSelf;
	[Tooltip("if true, the code performing the trigger effect will be in a player script, and the object below will be used as a marker for current active trigger..")]
	public bool         _isLogicInPlayScript;
	[ReadOnly, Tooltip("When the player enters this trigger, this will be what they base the effect on. If this is set to trigger self, it will be this, if not, it will take from ObjectsToTrigger")]
	public GameObject _TriggerForPlayerToRead;
	public S_EditorEnums.ColliderTypes _whatTriggerShape;

	[Header("Dev Visibility")]
	public bool _drawAtAllTimes = true;
	public Color _normalOutlineColour = Color.grey;
	public Color _selectedOutlineColour = Color.white;
	public Color _selectedFillColour = new Color(1,1,1,0.1f);


	private void OnTriggerEnter ( Collider other ) {
		if(other.tag != "Player") { return; }

		if(_triggerSelf)
			if (TryGetComponent(out ITriggerable Trigger))
				Trigger.TriggerObjectOn();

		//Go through each given gameObject and trigger if possible.
		for (int i = 0 ; i < _ObjectsToTriggerOn.Count ; i++)
		{
			if (_ObjectsToTriggerOn[i].TryGetComponent(out ITriggerable Trigger))
				Trigger.TriggerObjectOn();
		}
	}

	private void OnTriggerExit ( Collider other ) {
		if (other.tag != "Player") { return; }

		if (_triggerSelf)
			if (TryGetComponent(out ITriggerable Trigger))
				Trigger.TriggerObjectOn();

		//Go through each given gameObject and trigger if possible.
		for (int i = 0 ; i < _ObjectsToTriggerOn.Count ; i++)
		{
			if (_ObjectsToTriggerOn[i].TryGetComponent(out ITriggerable Trigger))
				Trigger.TriggerObjectOn();
		}
	}

#if UNITY_EDITOR

	[ExecuteAlways]
	private void OnValidate () {
		SetTriggerForPlayer();
		switch (_whatTriggerShape) {
			case S_EditorEnums.ColliderTypes.Box:
				  
				break;
		}
	}

	private void SetTriggerForPlayer () {
		if (!_isLogicInPlayScript) { _TriggerForPlayerToRead = null; return; }

		//If set to trigger self, then this is what will be returned.
		if (_triggerSelf) { _TriggerForPlayerToRead = gameObject; return; }

		//Get the class derived class using this as a type, then look for other object triggers using it too. E.G. S_Trigger_Camera will look for other S_Trigger_Cameras. And the first will be what to read.
		System.Type scriptType = GetType();

		for (int i = 0 ; i < _ObjectsToTriggerOn.Count ; i++)
		{
			if (_ObjectsToTriggerOn[i].GetComponent(scriptType))
				{ _TriggerForPlayerToRead = _ObjectsToTriggerOn[i].gameObject; return;}
		}

		_TriggerForPlayerToRead = null;
	}

	private void OnDrawGizmos () {
		if (S_S_EditorMethods.IsThisOrReferenceSelected(transform))
			DrawTriggerVolume(true);
		else if (_drawAtAllTimes)
			DrawTriggerVolume(false);
	}

	private void DrawTriggerVolume (bool selected) {

		//To match object scale and rotation, set draws to local space.
		Gizmos.matrix = transform.localToWorldMatrix;
		Color colour = selected ? _selectedOutlineColour : _normalOutlineColour;

		//Temporarily sets handles to local space until Using is over.
		using (new Handles.DrawingScope(colour, transform.localToWorldMatrix))
		{
			Vector3 size = Vector3.one;

			Handles.color = selected ? _selectedOutlineColour : _normalOutlineColour;
			Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; //Ensures handles drawn wont be visible through walls.

			switch (_whatTriggerShape)
			{
				case S_EditorEnums.ColliderTypes.Box:
					BoxCollider Col = GetComponent<BoxCollider>();
					size = new Vector3(size.x * Col.size.x, size.y * Col.size.y, size.z * Col.size.z);
					Handles.DrawWireCube(Col.center, size);
					if (selected)
					{
						Handles.DrawWireCube(Col.center, size + new Vector3(0.02f, 0.02f, 0.02f));
						Gizmos.color = _selectedFillColour;
						Gizmos.DrawCube(Col.center, size);
					}
					break;
			}
		}

		using (new Handles.DrawingScope(colour))
		{
			for (int i = 0 ; i < _ObjectsToTriggerOn.Count ; i++)
			{
				Handles.DrawLine(transform.position, _ObjectsToTriggerOn[i].transform.position, 5f);
			}
		}

		DrawAdditional(colour);
	}

	public virtual void DrawAdditional (Color colour) {

	}

#endif
}
