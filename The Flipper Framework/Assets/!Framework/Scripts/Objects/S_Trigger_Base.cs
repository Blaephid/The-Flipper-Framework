using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_Trigger_Base : S_Data_Base
{
	[Header("Trigger")]
	public ITriggerable[] _ObjectsToTrigger;
	[Tooltip("This won't do anything, and is just used as a marker on where to look for the effects of this script.")]
	public bool         _isLogicInPlayScript;
	public S_EditorEnums.ColliderTypes _whatTriggerShape;

	[Header("Dev Visibility")]
	public bool _drawAtAllTimes = true;
	public Color _normalOutlineColour = Color.grey;
	public Color _selectedOutlineColour = Color.white;
	public Color _selectedFillColour = new Color(1,1,1,0.1f);


	private void OnTriggerEnter ( Collider other ) {

		for (int i = 0 ; i < _ObjectsToTrigger.Length ; i++)
		{
			_ObjectsToTrigger[i].TriggerObjectOn();
		}
	}

#if UNITY_EDITOR

	private void OnValidate () {
		switch (_whatTriggerShape) {
			case S_EditorEnums.ColliderTypes.Box:
				  
				break;
		}
	}

	private void OnDrawGizmos () {
		if (S_S_EditorMethods.IsThisOrReferenceSelected(transform))
			DrawTriggerVolume(true);
		else if (_drawAtAllTimes)
			DrawTriggerVolume(false);
	}

	private void DrawTriggerVolume (bool selected) {

		Vector3 size = transform.lossyScale;
		size = Vector3.one;
		Handles.matrix = transform.localToWorldMatrix;
		Gizmos.matrix = transform.localToWorldMatrix;

		Handles.color = selected ? _selectedOutlineColour : _normalOutlineColour;

		Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

		DrawAdditional();

		switch (_whatTriggerShape)
		{
			case S_EditorEnums.ColliderTypes.Box:
				BoxCollider Col = GetComponent<BoxCollider>();
				size = new Vector3 (size.x * Col.size.x, size.y * Col.size.y, size.z * Col.size.z);
				Handles.DrawWireCube(Col.center, size);
				if (selected)
				{
					Handles.DrawWireCube(Col.center, size + new Vector3(0.02f, 0.02f, 0.02f));
					Gizmos.color = _selectedFillColour;
					Gizmos.DrawCube(Col.center, size);
				}
				break;
		}

		Gizmos.matrix = transform.worldToLocalMatrix;
		Handles.matrix = transform.worldToLocalMatrix;
	}

	public virtual void DrawAdditional () {

	}

#endif
}
