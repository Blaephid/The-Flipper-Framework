using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEngine.Windows;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;

public class S_Handler_Camera : MonoBehaviour
{

	public S_HedgeCamera          _HedgeCam;
	public CinemachineVirtualCamera _VirtCam;
	private S_CharacterTools      _Tools;
	private S_PlayerInput         _Input;
	private S_PlayerPhysics       _PlayerPhys;

	private Transform             _MainSkin;

	[HideInInspector] public float _initialDistance;
	[HideInInspector] public float _initialFOV;

	//This is used to check what the current dominant trigger is, as multiple triggers might be working together under one effect. These will have their read values set to the same.
	private List<S_Trigger_External> _CurrentActiveCameraTriggers = new List<S_Trigger_External>();

	void Awake () {
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_MainSkin = _Tools.MainSkin;

		_initialDistance = _Tools.CameraStats.DistanceStats.CameraDistance;
		_initialFOV = _Tools.CameraStats.FOVStats.baseFOV;
	}

	#region Trigger Interaction

	//Called when entering a trigger in the physics script (must be assigned in Unity editor)
	public void EventTriggerEnter ( Collider col ) {
		if (col.tag == "CameraTrigger")
		{
			CheckCameraTriggerEnter(col);
		}
	}

	public void CheckCameraTriggerEnter (Collider Col) {
		//This static method determines the data of the trigger entered, and returns data if its different, or null if it isn't. It also adds to the list of camera triggers if it shares data.
		List<S_Trigger_Base> cameraData = S_Interaction_Triggers.CheckTriggerEnter(Col, ref _CurrentActiveCameraTriggers, typeof(S_Trigger_Camera));

		for (int i = 0 ; i < cameraData.Count; i++){
			StartCameraEffect(cameraData[i] as S_Trigger_Camera);
		}
	}

	public void EventTriggerExit ( Collider col ) {
		if (col.tag == "CameraTrigger")
		{
			CheckCameraTriggerExit(col);
		}
	}

	public void CheckCameraTriggerExit ( Collider Col ) {
		//This static method determines the data of the trigger entered, and returns data if its different, or null if it isn't. It also adds to the list of camera triggers if it shares data.
		List<S_Trigger_Base> cameraData = S_Interaction_Triggers.CheckTriggerEnter(Col, ref _CurrentActiveCameraTriggers, typeof(S_Trigger_Camera));

		for (int i = 0 ; i < cameraData.Count ; i++)
		{
			EndCameraEffect(cameraData[i] as S_Trigger_Camera);
		}
	}


	private void StartCameraEffect ( S_Trigger_Camera cameraData ) {
		switch (cameraData._whatType)
		{
			//Rotates the camera in direction and prevents controlled rotation.
			case enumCameraControlType.SetToDirection:
				SetHedgeCamera(cameraData, cameraData._directionToSet);
				break;

			//Reneables camera control but still affects distance and other.
			case enumCameraControlType.RemoveEffects:
				RemoveAdditonalCameraEffects(cameraData, cameraData._removeAll);
				return;

			//Nothing changes in control, but distance and height may change.
			case enumCameraControlType.OnlyApplyEffects:
				if (cameraData._willChangeAltitude)
					_HedgeCam.SetCameraHeightOnly(cameraData._newAltitude, cameraData._faceSpeed, cameraData._duration);
				break;

			//Make camera face behind player.
			case enumCameraControlType.SetToInfrontOfCharacter:
				cameraData._directionToSet = -_MainSkin.forward;
				SetHedgeCamera(cameraData, cameraData._directionToSet);
				break;

			//Make camera face behind player and disable rotation.
			case enumCameraControlType.SetToBehindCharacter:
				cameraData._directionToSet = _MainSkin.forward;
				SetHedgeCamera(cameraData, cameraData._directionToSet);
				break;
			case enumCameraControlType.SetToViewTarget:
				if (!cameraData._lockOnTarget) return;
				cameraData._directionToSet = (cameraData._lockOnTarget.position - _MainSkin.position).normalized;
				SetHedgeCamera(cameraData, cameraData._directionToSet, cameraData._lockOnTarget);
				break;
		}

		ApplyAdditionalCameraEffects(cameraData);
	}


	private void EndCameraEffect ( S_Trigger_Camera cameraData ) {
		//If trigger was set to undo effects on exit, then reset all data to how they should be again.
		if (cameraData._willReleaseOnExit)
		{
			RemoveAdditonalCameraEffects(cameraData);
		}
	}

	#endregion

	#region Camera Effects

	private void ApplyAdditionalCameraEffects(S_Trigger_Camera cameraData ) {
		if (cameraData._willChangeDistance)
			StartCoroutine(LerpToNewDistance(cameraData._newDistance.y, cameraData._newDistance.x, false, cameraData._affectNewDistanceBySpeed));

		if (cameraData._willChangeFOV)
			StartCoroutine(LerpToNewFOV(cameraData._newFOV.y, cameraData._newFOV.x, false, cameraData._affectNewFOVBySpeed));

		if (cameraData._willOffsetTarget)
			HandleSecondaryTargetWithOffset(cameraData);

		SetLockCameras(cameraData, true);
		if (cameraData._lockToCharacterRotation)
			_HedgeCam.SetToStickToLocalRotation(true, cameraData._directionToSet);
	}

	private void RemoveAdditonalCameraEffects(S_Trigger_Camera cameraData, bool removeAll = false ) {

		if (removeAll || cameraData._willChangeDistance)
			StartCoroutine(LerpToNewDistance(cameraData ? cameraData._newDistance.y : 5, _initialDistance, true, cameraData ? !cameraData._affectNewDistanceBySpeed : true));
		if (removeAll || cameraData._willChangeFOV)
			StartCoroutine(LerpToNewFOV(cameraData ? cameraData._newFOV.y : 5, _initialFOV, true, cameraData ? !cameraData._affectNewFOVBySpeed : true));
		SetLockCameras(cameraData, false, !removeAll);

		if (removeAll || cameraData._lockToCharacterRotation)
			_HedgeCam.SetToStickToLocalRotation(false, Vector3.zero);

		if(removeAll || cameraData._willOffsetTarget)
			_HedgeCam.ReturnCameraTargetsToNormal(null, cameraData ? cameraData._framesToOffset : 0);

		if (removeAll || (cameraData._whatType != enumCameraControlType.OnlyApplyEffects && cameraData._Direction))
			_HedgeCam._lookTimer = 0; // To ensure the HedgeCamera script will end the look timer countdown and apply necessary changes.
	}

	private IEnumerator LerpToNewDistance(float frames, float distance, bool goalHasModifier = false, bool setAffectedBySpeed = false) {
		float startDistance = _HedgeCam._cameraMaxDistance_;
		//To ensure transition on screen is smooth, if setting speed to not adjust distance, ensure lerp from the current result, as HedgeCamera will immediately stop applying the modifier.
		if (!setAffectedBySpeed)
		{
			startDistance *= _HedgeCam._canAffectDistanceBySpeed ? _HedgeCam._distanceModifier : 1;
			_HedgeCam._canAffectDistanceBySpeed = false;
		}

		for (float f = 1f ; f <= frames ; f++)
		{
			yield return new WaitForFixedUpdate();

			float goalDistance = goalHasModifier ? distance * _HedgeCam._distanceModifier : distance;
			_HedgeCam._cameraMaxDistance_ = Mathf.Lerp(startDistance, goalDistance, f / frames);
		}
		if(setAffectedBySpeed)
		{
			_HedgeCam._canAffectDistanceBySpeed = true;
			_HedgeCam._cameraMaxDistance_ = distance;
		}
	}

	private IEnumerator LerpToNewFOV ( float frames, float newFOV, bool withModifier = false, bool setAffectedBySpeed = false ) {
		float startFOV =  _HedgeCam._baseFOV_;
		if (!setAffectedBySpeed)
		{
			startFOV *= _HedgeCam._canAffectFOVBySpeed ? _HedgeCam._FOVModifier : 1;
			_HedgeCam._canAffectFOVBySpeed = false;
		}

		for (float f = 1f; f <= frames ; f++)
		{
			yield return new WaitForFixedUpdate();

			float goalFOV = withModifier ? newFOV * _HedgeCam._FOVModifier : newFOV;
			_HedgeCam._baseFOV_ = Mathf.Lerp(startFOV, goalFOV, f / frames);
		}
		if (setAffectedBySpeed)
		{
			_HedgeCam._canAffectFOVBySpeed = true;
			_HedgeCam._baseFOV_ = newFOV;
		}
	}

	private void HandleSecondaryTargetWithOffset(S_Trigger_Camera cameraData ) {
		Vector3 newTargetPosition = _PlayerPhys._CharacterPivotPosition;
		newTargetPosition += cameraData._asLocalOffset ?  _MainSkin.rotation * cameraData._newOffset : cameraData._newOffset;

		Transform TargetToMove = cameraData._overWriteAllOffsets ? _HedgeCam._FinalTarget : _HedgeCam._BaseTarget;
		Transform NewParent = cameraData._asLocalOffset ? _HedgeCam._Skin : _HedgeCam._PlayerMainBody;

		_HedgeCam.SetCameraTargetToNewParent(TargetToMove, NewParent, newTargetPosition, cameraData._framesToOffset);	
	}

	//Will either make it so the camera can't be moved in the hedge cam script, or that it can.
	public void SetLockCameras ( S_Trigger_Camera cameraData, bool setTo, bool overWriteIfFalse = true ) {
		//For each boolean, only set if camera data is set true (meaning camera data effects this, ignore if not.
		//If overwritting everything, then just set to false.


		if (!overWriteIfFalse)
		{
			_HedgeCam._isXLocked = false;
			_HedgeCam._isYLocked = false;
			_HedgeCam._isLocked = false;
			_HedgeCam._isMasterLocked = false;
			S_S_Logic.RemoveLockFromList(ref _HedgeCam._locksForCameraFallBack, "cameraTrigger");
		}
		else
		{
			_HedgeCam._isMasterLocked = cameraData._lockCamera ? setTo : _HedgeCam._isMasterLocked;
			_HedgeCam._isLocked = cameraData._lockCamera ? setTo : _HedgeCam._isLocked;
			_HedgeCam._isYLocked = cameraData._lockCameraY ? setTo : _HedgeCam._isYLocked;
			_HedgeCam._isXLocked = cameraData._lockCameraX ? setTo : _HedgeCam._isXLocked;
			if (cameraData._lockCameraFallBack) { S_S_Logic.AddLockToList(ref _HedgeCam._locksForCameraFallBack, "cameraTrigger"); }
		}
	}

	//Calls the hedgecam to rotate towards or change height.
	void SetHedgeCamera ( S_Trigger_Camera cameraData, Vector3 direction, Transform LockOn = null ) {
		Vector3 targetUpDirection = cameraData._setCameraReferenceWorldRotation
			? cameraData.transform.up : Vector3.zero;

		if (cameraData._willChangeAltitude)
			_HedgeCam.SetCameraWithSeperateHeight(direction, cameraData._duration, cameraData._newAltitude, cameraData._faceSpeed, targetUpDirection, LockOn);
		else
			_HedgeCam.SetCameraNoSeperateHeight(direction, cameraData._duration, cameraData._faceSpeed, targetUpDirection, cameraData._willRotateVertically, LockOn);
	}


	//Certain actions will call this in input, where if button is pressed under right speed, then camera will reset to behind character's back.
	public void AttemptCameraReset () {
		//Set Camera to back
		if (_Input._CamResetPressed)
		{
			if (!_HedgeCam._isLocked)
			{
				if (_Input.moveVec == Vector2.zero && _PlayerPhys._PlayerVelocity._horizontalSpeedMagnitude < 5f)
				{
					_HedgeCam.SetCameraWithSeperateHeight(_MainSkin.forward, 0.25f, 0, 12, Vector3.zero);
					_Input._CamResetPressed = false;
				}
			}
		}
	}

	public void ResetOnDeath () {
		RemoveAdditonalCameraEffects(null, true);

		_HedgeCam._cameraMaxDistance_ = _initialDistance;
	}
	#endregion
}
