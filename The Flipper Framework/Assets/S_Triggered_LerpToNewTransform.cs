using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Triggered_LerpToNewTransform : S_Triggered_Base, ITriggerable
{
	[HideInInspector, SerializeField] GameObject generated;

	private TransformStructData TransformA;
	private TransformStructData TransformB;
	private TransformStructData GoalTransform;
	private TransformStructData FromTransform;

	private bool _currentlyTransformA = true;
	private bool _isLerping;
	private bool _lerpToA;
	private float _currentLerp;

	[SerializeField] private float timeToLerp = 1;

	public struct TransformStructData
	{
		public Vector3 worldPosition;
		public Quaternion worldRotation;
		public Vector3 worldScale;
	}

#if UNITY_EDITOR
	public S_Triggered_LerpToNewTransform () {
		_hasVisualisationScripted = true;
	}
#endif

	private void Start () {
		if (!generated) { enabled = false; return; }

		TransformA = new TransformStructData()
		{
			worldPosition = transform.position,
			worldRotation = transform.rotation,
			worldScale = transform.lossyScale,
		};

		TransformB = new TransformStructData()
		{
			worldPosition = generated.transform.position,
			worldRotation = generated.transform.rotation,
			worldScale = generated.transform.lossyScale
		};

		_currentlyTransformA = true;
	}

#if UNITY_EDITOR
	private void OnValidate () {
		generated = S_S_Editor.FindOrCreateChild(gameObject, "Transform Goal", null, false);
	}
#endif

	private void Update () {
		if (!_isLerping) { return; }

		_currentLerp -= Time.deltaTime;
		_currentLerp = Mathf.Max(_currentLerp, 0);
		float lerpAmount = (timeToLerp - _currentLerp) / timeToLerp;

		transform.position = Vector3.Lerp(FromTransform.worldPosition, GoalTransform.worldPosition, lerpAmount);
		transform.rotation = Quaternion.Lerp(FromTransform.worldRotation, GoalTransform.worldRotation, lerpAmount);

		Vector3 parentLossyScale = transform.parent ? transform.parent.lossyScale : Vector3.one;
		Vector3 goalScale = new Vector3(GoalTransform.worldScale.x / parentLossyScale.x, GoalTransform.worldScale.y / parentLossyScale.y, GoalTransform.worldScale.z / parentLossyScale.z);
		transform.localScale = Vector3.Lerp(FromTransform.worldScale, goalScale, lerpAmount);

		_isLerping = _currentLerp > 0;

	}

	public void TriggerObjectOn ( S_PlayerPhysics Player = null ) {
		if(!CanBeTriggeredOn(Player)) return;

		_isLerping = true;

		_isCurrentlyOn = _currentlyTransformA;
		GoalTransform = _currentlyTransformA ? TransformB : TransformA;
		FromTransform = _currentlyTransformA ? TransformA : TransformB;

		_lerpToA = !_currentlyTransformA;
		_currentlyTransformA = !_currentlyTransformA;
		_currentLerp = timeToLerp;
	}

	public override void ResetToOriginal () {
		TriggerObjectOn(null);
		_currentLerp = 0; //Makes switch instant.
	}



#if UNITY_EDITOR
	public override void DrawGizmosAndHandles ( bool selected ) {
		if (!selected || !generated) { return; }

		if (gameObject.TryGetComponent(out MeshFilter MeshFilterComponent))
		{
			if (!MeshFilterComponent.sharedMesh) { return; }

			Gizmos.color = _selectedFillColour;
			Gizmos.matrix = generated.transform.localToWorldMatrix;
			Gizmos.DrawMesh(MeshFilterComponent.sharedMesh);
		}
	}
#endif
}
