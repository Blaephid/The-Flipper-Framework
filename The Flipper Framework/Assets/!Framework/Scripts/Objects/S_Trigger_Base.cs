using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using templates;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(S_ConstantSceneGUI))]
public class S_Trigger_Base : S_Data_Base, ICustomEditorLogic
{
	//Serialized

	[Space]
	public S_EditorEnums.ColliderTypes _whatTriggerShape = S_EditorEnums.ColliderTypes.External;
	public StrucGeneralTriggerObjects _TriggerObjects = new StrucGeneralTriggerObjects()
	{
		_ObjectsToTriggerOn = new List<GameObject>(),
		_ObjectsToTriggerOff = new List<GameObject>(),
	};

	private List<GameObject> _RememberObjectsToTriggerOn = new List<GameObject>(); //Only used to check when the above list is edited.
	private List<GameObject> _RememberObjectsToTriggerOff = new List<GameObject>(); //Only used to check when the above list is edited.

	[Serializable]
	[Tooltip("This data related to general triggering of other objects, which certain triggers won't need. Will interact with the ITriggerable Interface")]
	public struct StrucGeneralTriggerObjects {
		public bool         _triggerSelf;

		public List<GameObject> _ObjectsToTriggerOn;
		public List<GameObject> _ObjectsToTriggerOff;
	}


	[CustomReadOnly]
	[Tooltip("Set true in code for triggers where their effects are scripted in a player script. _TriggerForPlayerToRead provides the data for said script.")]
	public bool         _isLogicInPlayerScript;
	[OnlyDrawIf("_isLogicInPlayerScript", true)]
	[CustomReadOnly, Tooltip("When the player enters this trigger, this will be what they base the effect on. If this is set to trigger self, it will be this, if not, it will take from ObjectsToTrigger")]
	public GameObject _TriggerForPlayerToRead;

	[CustomReadOnly, Tooltip("This will be true if any trigger will activate this one. That includes itself or another.")]
	[ColourIfEqualTo(false, 0.9f,0.5f,0.5f,1f)]
	public bool _hasTrigger;
	[CustomReadOnly] public List<GameObject> _ObjectsThatTriggerThis = new List<GameObject>();

	//Trackers
	S_ConstantSceneGUI ConstantGUI;
	S_PlayerPhysics _Player;

	/// <summary>
	///			Inherited
	/// </summary>
	#region inherited

	private void OnEnable () {
		ConstantGUI = GetComponent<S_ConstantSceneGUI>();
		if (ConstantGUI == null) return;
		ConstantGUI._LinkedComponent = this;
	}


	private void OnTriggerEnter ( Collider other ) {
		if (other.tag != "Player") { return; }

		if (_TriggerObjects._triggerSelf)
			if (TryGetComponent(out ITriggerable Trigger))
				Trigger.TriggerObjectOn();

		other.TryGetComponent(out _Player);
		if(!_Player) { _Player = other.GetComponentInParent<S_PlayerPhysics>(); }

		TriggerGivenObjects(true, _TriggerObjects._ObjectsToTriggerOn);
		TriggerGivenObjects(false, _TriggerObjects._ObjectsToTriggerOff);
	}

	private void OnTriggerExit ( Collider other ) {
		if (other.tag != "Player") { return; }

		if (_TriggerObjects._triggerSelf)
			if (TryGetComponent(out ITriggerable Trigger))
				Trigger.TriggerObjectOn();
	}

	public virtual void TriggerGivenObjects (bool on, List<GameObject> gameObjects) {
		//Go through each given gameObject and trigger if possible.
		for (int i = 0 ; i < gameObjects.Count ; i++)
		{
			if (gameObjects[i].TryGetComponent(out ITriggerable Trigger))
				if (on) Trigger.TriggerObjectOn(_Player); else Trigger.TriggerObjectOff(_Player);
		}
	}



	public override void OnValidate () {

		base.OnValidate();

#if UNITY_EDITOR
		UpdateExternalTriggers();

		if ( _TriggerObjects._triggerSelf ) 
		{ CheckExternalTriggerDataHasTrigger(false, this) ; }
		else 
		{ CheckExternalTriggerDataHasTrigger(true, this); }


		CheckTriggerForPlayerToRead();
		_hasVisualisationScripted = true;
#endif
	}


#if UNITY_EDITOR
	private void Update () {
		if (Selection.activeGameObject != gameObject) { return; }

		switch (_whatTriggerShape)
		{
			case S_EditorEnums.ColliderTypes.Box:
				S_S_EditorMethods.AddComponentIfMissing(gameObject, typeof(BoxCollider));
				S_S_EditorMethods.FindAndRemoveComponent(gameObject, typeof(SphereCollider));

				BoxCollider Box = GetComponent<BoxCollider>();
				Box.isTrigger = true;
				Box.size = Vector3.one;
				break;
			case S_EditorEnums.ColliderTypes.Sphere:
				S_S_EditorMethods.AddComponentIfMissing(gameObject, typeof(SphereCollider));
				S_S_EditorMethods.FindAndRemoveComponent(gameObject, typeof(BoxCollider));

				SphereCollider Sphere = GetComponent<SphereCollider>();
				Sphere.isTrigger = true;
				Sphere.radius = 0.5f;
				break;
			case S_EditorEnums.ColliderTypes.External:
				S_S_EditorMethods.FindAndRemoveComponent(gameObject, typeof(BoxCollider));
				S_S_EditorMethods.FindAndRemoveComponent(gameObject, typeof(SphereCollider));
				break;
		}

	}
#endif

	#endregion

#if UNITY_EDITOR

	private void UpdateExternalTriggers () {

		//Goes through what is set to trigger on, and ensure it knows this is set to trigger it.
		if (_TriggerObjects._ObjectsToTriggerOn.Count > 0)
		{
			for (int i = 0 ; i < _TriggerObjects._ObjectsToTriggerOn.Count ; i++)
			{
				if (!_TriggerObjects._ObjectsToTriggerOn[i]) { continue; }
				if (_TriggerObjects._ObjectsToTriggerOn[i].TryGetComponent(out S_Trigger_Base ExternalTriggerData))
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

			if (_RememberObjectsToTriggerOn[i].TryGetComponent(out S_Trigger_Base ExternalTriggerData))
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
		{ SetTriggerForPlayerToRead( null); return; }

		//If set to trigger self, then this is the logic the player will need to reference.
		if (_TriggerObjects._triggerSelf) 

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
	private void SetTriggerForPlayerToRead ( GameObject SetTo) {

		//If not changing, none of this is needed.
		if((!_TriggerForPlayerToRead && !SetTo) || (SetTo && _TriggerForPlayerToRead == SetTo)) { return; }

		//Remove this trigger's reference to what needs to be read now, as it may no longer have that reference soon.
		if (_TriggerForPlayerToRead && _TriggerForPlayerToRead.TryGetComponent(out S_Trigger_Base TriggerData)) 
		{ CheckExternalTriggerDataHasTrigger(true, TriggerData); }
		
		_TriggerForPlayerToRead = SetTo;

		if (!(_TriggerForPlayerToRead && _TriggerForPlayerToRead.TryGetComponent(out S_Trigger_Base TriggerData2))) { return; }
		//If valid TriggerToRead, update it as being triggered by this, even if its itself.
		CheckExternalTriggerDataHasTrigger(false, TriggerData2);
	}

	private void CheckExternalTriggerDataHasTrigger ( bool removeThisTrigger, S_Trigger_Base TargetTriggerData ) {

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

	/// <summary>
	///			Gizmo Drawing
	/// </summary>
	#region Gizmo Drawing


	public override void DrawGizmosAndHandles ( bool selected ) {

		//_isSelected will also be enabled externally in play mode, so if not playing, ensure _isSelected is based on actual selecting the gameObject.
		if (!Application.isPlaying) { _isSelected = selected; }
		selected = _isSelected;

		Color colour = selected ? _selectedOutlineColour : _normalOutlineColour;

		Vector3 size = Vector3.one;

		Handles.color = selected ? _selectedOutlineColour : _normalOutlineColour;
		Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; //Ensures handles drawn wont be visible through walls.

		//Draw representations of trigger areas.
		switch (_whatTriggerShape)
		{
			case S_EditorEnums.ColliderTypes.Box:
				//Temporarily sets handles to local space until Using is over.
				using (new Handles.DrawingScope(colour, transform.localToWorldMatrix))
				{
					//Constant outline
					BoxCollider Col = GetComponent<BoxCollider>();
					size = new Vector3(size.x * Col.size.x, size.y * Col.size.y, size.z * Col.size.z);
					Handles.DrawWireCube(Col.center, size);

					//Extra outline and fill
					if (selected)
					{
						Handles.DrawWireCube(Col.center, size + new Vector3(0.02f, 0.02f, 0.02f));
						//To match object scale and rotation, set draws to local space.
						Gizmos.matrix = transform.localToWorldMatrix;
						Gizmos.color = _selectedFillColour;
						Gizmos.DrawCube(Col.center, size);
					}
				}
				break;

			case S_EditorEnums.ColliderTypes.Sphere:
				//Constant outline
				SphereCollider Sphere = GetComponent<SphereCollider>();
				//Because sphere triggers are always perfectly spherical and only expand to match the largest scale dimension
				float radius =  Sphere.radius * S_S_MoreMathMethods.GetLargestOfVector(transform.lossyScale);
				Vector3 centre = Sphere.center + transform.position;

				using (new Handles.DrawingScope(colour))
				{
					Handles.DrawWireDisc(centre, transform.up, radius);
					Handles.DrawWireDisc(centre, transform.right, radius);

					//Extra outline and fill
					if (selected)
					{
						Handles.DrawWireCube(centre, size * radius * 2);
						Gizmos.color = _selectedFillColour;
						Gizmos.DrawSphere(centre, radius);
					}
					break;
				}

			case S_EditorEnums.ColliderTypes.External:
				break;

		}

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

		Gizmos.matrix = Matrix4x4.identity; //Reset gizmos to world space.
		DrawAdditionalGizmos(selected,colour);
	}

	public virtual void DrawAdditionalGizmos (bool selected ,Color colour ) {

	}
	#endregion

	//Inherited from ICustomEditorLogic Interface. This will be attached to the DuringSceneGUI event, or called seperately when certain objects are selected.
	public void CustomOnSceneGUI ( SceneView sceneView ) {
		if(this == null) { return; }
		float handleRadius = 2 * Mathf.Clamp(S_S_MoreMathMethods.GetLargestOfVector(transform.lossyScale) / 40, 1, 20);
		base.VisualiseWithSelectableHandle(transform.position,handleRadius);
		AdditionalTriggerSceneGUI();
	}

	public virtual void AdditionalTriggerSceneGUI () {

	}
#endif
}

