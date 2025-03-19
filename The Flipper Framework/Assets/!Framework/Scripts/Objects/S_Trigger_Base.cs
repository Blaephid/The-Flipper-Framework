using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using templates;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(S_ConstantSceneGUI))]
public class S_Trigger_Base : S_Data_Base, ICustomEditorLogic
{
	//Serialized

	[Space]
	public S_EditorEnums.ColliderTypes _whatTriggerShape = S_EditorEnums.ColliderTypes.External;

	//Trackers
	S_ConstantSceneGUI ConstantGUI;
	[HideInInspector] public S_PlayerPhysics _Player;

#if UNITY_EDITOR
	/// 
	///			Inherited
	///
	#region inherited

	private void OnEnable () {
		ConstantGUI = GetComponent<S_ConstantSceneGUI>();
		if (ConstantGUI == null) return;
		ConstantGUI._LinkedComponent = this;
	}

	public override void OnValidate () {
		base.OnValidate();
		_hasVisualisationScripted = true;

	}

	private void Update () {
		if (Selection.activeGameObject != gameObject) { return; }

		switch (_whatTriggerShape)
		{
			case S_EditorEnums.ColliderTypes.Box:
				S_S_Editor.AddComponentIfMissing(gameObject, typeof(BoxCollider));
				S_S_Editor.FindAndRemoveComponent(gameObject, typeof(SphereCollider));

				BoxCollider Box = GetComponent<BoxCollider>();
				Box.isTrigger = true;
				Box.size = Vector3.one;
				break;
			case S_EditorEnums.ColliderTypes.Sphere:
				S_S_Editor.AddComponentIfMissing(gameObject, typeof(SphereCollider));
				S_S_Editor.FindAndRemoveComponent(gameObject, typeof(BoxCollider));

				SphereCollider Sphere = GetComponent<SphereCollider>();
				Sphere.isTrigger = true;
				Sphere.radius = 0.5f;
				break;
			case S_EditorEnums.ColliderTypes.External:
				S_S_Editor.FindAndRemoveComponent(gameObject, typeof(BoxCollider));
				S_S_Editor.FindAndRemoveComponent(gameObject, typeof(SphereCollider));
				break;
		}

	}

	#endregion

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
				float radius =  Sphere.radius * S_S_MoreMaths.GetLargestOfVector(transform.lossyScale);
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

		Gizmos.matrix = Matrix4x4.identity; //Reset gizmos to world space.
		DrawAdditionalGizmos(selected, colour);
	}

	public virtual void DrawAdditionalGizmos ( bool selected, Color colour ) {

	}
	#endregion

	//Inherited from ICustomEditorLogic Interface. This will be attached to the DuringSceneGUI event, or called seperately when certain objects are selected.
	public void CustomOnSceneGUI ( SceneView sceneView ) {
		if (this == null) { return; }
		float handleRadius = 2 * Mathf.Clamp(S_S_MoreMaths.GetLargestOfVector(transform.lossyScale) / 40, 1, 20);
		base.VisualiseWithSelectableHandle(transform.position, handleRadius);
		AdditionalTriggerSceneGUI();
	}

	public virtual void AdditionalTriggerSceneGUI () {

	}
#endif
}

