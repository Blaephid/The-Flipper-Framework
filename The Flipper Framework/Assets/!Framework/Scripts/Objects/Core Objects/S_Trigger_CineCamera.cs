using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using UnityEditor;

public class S_Trigger_CineCamera : S_Trigger_Base, ITriggerable
{

	//Stats
	[Header("Main")]
	[Tooltip("Will only activates its camera if this is true.")]
	public bool _willActivateCamera = true;
	[Tooltip("If true then the cinematic camera will start each scene looking towards the trigger. Helps align it, especially if using LookAtTarget.")]
	public bool _defaultCameraToFaceTrigger = true;

	[Header("Attached Elements")]
	[Tooltip("Controls the properties of the cinemachine component, by setting it to follow or look at the player.")]
	public CinemachineVirtualCamera         _CinematicCamComponent;
	[Tooltip("The gameObject that will be set to active or inactive. Likely the same as the above.")]
	public GameObject                       _CinematicCamObject;
	[Tooltip("If not empty, anytime this trigger ends its camera, it will instead end the camera of a different trigger, using the properties FROM THAT ONE. Use if you don't want to just have one large trigger.")]
	public S_Trigger_CineCamera             _EndThisTriggerInstead;

	[Header("Works with these actions")]
	[Tooltip("Waits until the player is in one of the following actions before triggering the camera. Allow more control, as large triggers can sometimes be cumbersome.")]
	public S_S_ActionHandling.PrimaryPlayerStates[]    _ListOfActionsThisWorksOn;
	[Tooltip("If true, the camera will deactivate if the player enters an action not in the above list.")]
	public bool                             _willDeactivateCameraIfActonChanges = false;

	[Header ("Starting")]
	[Tooltip("If true, when activated the cinematic camera will be at the point the main camera already is")]
	public bool         _moveCinematicCamToMainCamPoint;
	[Tooltip("Adds this to the position of wherever the camera is on start (position resets on deactivate)")]
	public Vector3      _startOffset;

	[Header ("Ending")]
	public bool         _endOnEnterTrigger = false;
	public bool         _endOnExitTrigger = true;
	public float        _delayEndForFrames = 0;

	[Header("Effects on/with Player")]
	[Tooltip("See Cinemachine Virtual Camera component. If this is true, the players transform becomes what the virtual camera looks at.")]
	public bool lookPlayer;
	[Tooltip("See Cinemachine Virtual Camera component. If this is true, the players transform becomes what the virtual camera follows, using its own parameters.")]
	public bool followPlayer;

	[Header("On End")]
	[Tooltip("If true, when the main camera returns to the player, it will be behind them.")]
	public bool         _willSetCameraBehind = true;
	public int          _lockPlayerInputFor = 5;
	public S_GeneralEnums.LockControlDirection _LockInputTo_;

	//Private
	private Vector3 cameraOriginalPosition;
	private Quaternion cameraOriginalRotation;

	private bool _isCurrentlyActive = false;

	//Player
	private S_CharacterTools      _PlayerTools;
	private S_ActionManager       _PlayerActions;
	private S_Handler_Camera      _PlayerCameraHandler;

	// Start is called before the first frame update

	void Awake () {
		FaceCinematicCameraIn();

		//Ensure cinematic camera isn't on and save its starting transform.
		cameraOriginalPosition = _CinematicCamObject.transform.position;
		cameraOriginalRotation = _CinematicCamObject.transform.rotation;
		_CinematicCamObject.SetActive(false);

	}

#if UNITY_EDITOR
	public override void OnValidate () {
		base.OnValidate();

		if (!Application.isPlaying)
			FaceCinematicCameraIn();
	}

	public override void DrawAdditionalGizmos ( bool selected, Color colour) {
		if (_TriggerObjects._triggerSelf)
		{
			using (new Handles.DrawingScope(colour))
			{
				S_S_DrawingMethods.DrawArrowHandle(colour, _CinematicCamObject.transform, 0.4f, true, Vector3.forward);
				Handles.DrawLine(transform.position, _CinematicCamObject.transform.position, 1.5f);
			}
		}
		
	}
#endif

	private void FaceCinematicCameraIn () {
		if (_willActivateCamera && _defaultCameraToFaceTrigger)
		{
			Vector3 direction = (transform.position - _CinematicCamObject.transform.position).normalized;
			_CinematicCamObject.transform.forward = direction;
		}
	}

	private void OnTriggerEnter ( Collider other ) {
		if (other.tag == "Player")
		{
			if (_endOnEnterTrigger)
			{
				EndThisOrAnotherCamera();
			}

			//Set this camera up to detect player. Cameras are activated in Trigger Stay so it waits for the player to be in the right actions.
			else if (_willActivateCamera)
			{
				_PlayerTools = other.GetComponentInParent<S_CharacterTools>();
				_PlayerActions = _PlayerTools._ActionManager;
				_PlayerCameraHandler = _PlayerTools.CamHandler;
			}
		}
	}
	private void OnTriggerStay ( Collider col ) {
		if (col.tag == "Player")
		{
			//Only check if the player has already been saved to check its actions.
			if (_PlayerActions != null && _willActivateCamera)
			{
				bool isPlayerIsRightAction = false;

				//Check if players current action is one this cinematic is set to work with.
				for (int i = 0 ; i < _ListOfActionsThisWorksOn.Length ; i++)
				{
					if (_PlayerActions._whatCurrentAction == _ListOfActionsThisWorksOn[i])
						isPlayerIsRightAction = true;
				}

				if (!isPlayerIsRightAction)
				{
					if (_willDeactivateCameraIfActonChanges)
					{
						EndThisOrAnotherCamera();
					}
				}
				else
				{
					ActivateCam();
				}
			}
		}
	}

	void OnTriggerExit ( Collider col ) {
		if (col.tag == "Player")
		{
			if (_isCurrentlyActive && _endOnExitTrigger)
			{
				EndThisOrAnotherCamera();
			}
		}
	}

	//End this or the cinematic camera input in its place.
	private void EndThisOrAnotherCamera () {
		if (_EndThisTriggerInstead)
			StartCoroutine(_EndThisTriggerInstead.DeactivateCam());
		else
			StartCoroutine(DeactivateCam());
	}

	public void ActivateCam () {

		if (_isCurrentlyActive) return;

		_isCurrentlyActive = true;

		//If the cinematic is supposed to start from the player cameras current position.
		if (_moveCinematicCamToMainCamPoint)
		{
			_CinematicCamObject.transform.position = _PlayerCameraHandler._HedgeCam.transform.position;
			_CinematicCamObject.transform.rotation = _PlayerCameraHandler._HedgeCam.transform.rotation;
		}

		//Apply requirements onto cinemachine
		if (lookPlayer)
			_CinematicCamComponent.LookAt = _PlayerTools.transform;

		if (followPlayer)
			_CinematicCamComponent.Follow = _PlayerTools.transform;

		_CinematicCamObject.transform.position += _startOffset;

		_CinematicCamObject.SetActive(true); //Blending is handled by the blend object attached to the main camera cinemachine brain.

		S_Manager_LevelProgress.OnReset += ResetCamera; //Ensures camera will end if player dies when its active.
	}

	public void ResetCamera ( object sender, EventArgs e ) {
		//Deactivate
		_isCurrentlyActive = false;
		_PlayerActions = null;

		_CinematicCamObject.transform.position = cameraOriginalPosition;
		_CinematicCamObject.transform.rotation = cameraOriginalRotation;

		_CinematicCamObject.SetActive(false); //Blending is handled by the blend object attached to the main camera cinemachine brain.
	}

	public IEnumerator DeactivateCam () {
		if (!_isCurrentlyActive) { yield break; }

		//Wait a number of frames before deactivating cinema camera.
		for (int i = 0 ; i < _delayEndForFrames ; i++)
		{
			yield return new WaitForFixedUpdate();
		}

		//Player
		if (_willSetCameraBehind)
			_PlayerCameraHandler._HedgeCam.SetBehind(0);
		if (_lockPlayerInputFor > 0)
			_PlayerTools.GetComponent<S_PlayerInput>().LockInputForAWhile(_lockPlayerInputFor, true, Vector3.zero, _LockInputTo_);

		ResetCamera(null, null);
	}
}