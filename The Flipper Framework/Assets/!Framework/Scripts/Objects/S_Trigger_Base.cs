using System;
using System.Collections;
using System.Collections.Generic;
using templates;
using Unity.Collections;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(S_ConstantSceneGUI))]
public class S_Trigger_Base : S_Data_Base, ICustomEditorLogic
{
	//Serialized

	[Header("Trigger")]
	public StrucGeneralTriggerObjects TriggerObjects = new StrucGeneralTriggerObjects()
	{
		_ObjectsToTriggerOn = new List<GameObject>(),
		_ObjectsToTriggerOff = new List<GameObject>(),
	};

	[Serializable]
	[Tooltip("This data related to general triggering of other objects, which certain triggers won't need. Will interact with the ITriggerable Interface")]
	public struct StrucGeneralTriggerObjects {
		public List<GameObject> _ObjectsToTriggerOn;
		public List<GameObject> _ObjectsToTriggerOff;
		public bool         _triggerSelf;
		[Tooltip("if true, the code performing the trigger effect will be in a player script, and the object below will be used as a marker for current active trigger..")]
		public bool         _isLogicInPlayScript;
	}

	[CustomReadOnly, Tooltip("When the player enters this trigger, this will be what they base the effect on. If this is set to trigger self, it will be this, if not, it will take from ObjectsToTrigger")]
	public GameObject _TriggerForPlayerToRead;
	public S_EditorEnums.ColliderTypes _whatTriggerShape = S_EditorEnums.ColliderTypes.External;

	//Trackers
	S_ConstantSceneGUI ConstantGUI;

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

		if (TriggerObjects._triggerSelf)
			if (TryGetComponent(out ITriggerable Trigger))
				Trigger.TriggerObjectOn();

		//Go through each given gameObject and trigger if possible.
		for (int i = 0 ; i < TriggerObjects._ObjectsToTriggerOn.Count ; i++)
		{
			if (TriggerObjects._ObjectsToTriggerOn[i].TryGetComponent(out ITriggerable Trigger))
				Trigger.TriggerObjectOn();
		}
	}

	private void OnTriggerExit ( Collider other ) {
		if (other.tag != "Player") { return; }

		if (TriggerObjects._triggerSelf)
			if (TryGetComponent(out ITriggerable Trigger))
				Trigger.TriggerObjectOn();

		//Go through each given gameObject and trigger if possible.
		for (int i = 0 ; i < TriggerObjects._ObjectsToTriggerOn.Count ; i++)
		{
			if (TriggerObjects._ObjectsToTriggerOn[i].TryGetComponent(out ITriggerable Trigger))
				Trigger.TriggerObjectOff();
		}
	}



	public override void OnValidate () {

		base.OnValidate();

#if UNITY_EDITOR
		SetTriggerForPlayer();
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
	private void SetTriggerForPlayer () {
		if (!TriggerObjects._isLogicInPlayScript) { _TriggerForPlayerToRead = null; return; }

		//If set to trigger self, then this is what will be returned.
		if (TriggerObjects._triggerSelf) { _TriggerForPlayerToRead = gameObject; return; }

		//Get the class derived class using this as a type, then look for other object triggers using it too. E.G. S_Trigger_Camera will look for other S_Trigger_Cameras. And the first will be what to read.
		System.Type scriptType = GetType();

		for (int i = 0 ; i < TriggerObjects._ObjectsToTriggerOn.Count ; i++)
		{
			if (TriggerObjects._ObjectsToTriggerOn[i].GetComponent(scriptType))
			{ _TriggerForPlayerToRead = TriggerObjects._ObjectsToTriggerOn[i].gameObject; return; }
		}

		_TriggerForPlayerToRead = null;
	}

	/// <summary>
	///			Gizmo Drawing
	/// </summary>
	#region Gizmo Drawing


	public override void DrawGizmosAndHandles ( bool selected ) {

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

		DrawTriggerAdditional(colour);
	}

	public virtual void DrawTriggerAdditional ( Color colour ) {

	}
	#endregion

	//Inherited from ICustomEditorLogic Interface. This will be attached to the DuringSceneGUI event, or called seperately when certain objects are selected.
	public void CustomOnSceneGUI ( SceneView sceneView ) {
		if(this == null) { return; }
		float handleRadius = 2 * Mathf.Clamp(S_S_MoreMathMethods.GetLargestOfVector(transform.lossyScale) / 40, 1, 20);
		base.VisualiseWithSelectableHandle(transform.position,handleRadius);
	}
#endif
}

#if UNITY_EDITOR

[CustomEditor(typeof(S_Trigger_Base))]
public class TriggerEditor : S_CustomInspector_Base
{
	[SerializeField]
	public S_Trigger_Base _OwnerScript;
	[SerializeField]
	public GameObject _OwnerObject;


	public override void OnEnable () {
		//Setting variables
		_OwnerScript = (S_Trigger_Base)target;
		_OwnerObject = _OwnerScript.gameObject;

		base.OnEnable();
	}


	public override S_O_CustomInspectorStyle GetInspectorStyleFromSerializedObject () {
		return _OwnerScript._InspectorTheme;
	}

	public override void DrawInspectorNotInherited () {
		//Describe what the script does
		EditorGUILayout.TextArea("Details.", EditorStyles.textArea);
		DrawDefaultInspector();
	}

}
#endif
