using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class S_State_CameraMode : MonoBehaviour
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	public S_CharacterTools		_Tools;
	private S_ActionManager		_ActionMan;
	private PlayerInput                     _PlayerInput;
	private S_PlayerInput                   _InputScript;

	public CinemachineVirtualCamera	_VirtualCamera;
	private Transform                       _CamTransform;
	#endregion

	// Trackers
	#region trackers

	//Inputs
	private Vector2      _move;
	private Vector2      _rotate;
	private float       _ascend;

	private Vector2     _saveInput;

	//Time
	public float        _timeScaleWhenActive = 0.00001f;
	private float       _timeValuePerFrame;

	//Speeds
	public float        _moveSpeed = 5;
	public float        _ascendSpeed = 5;
	public Vector2       _rotateSpeed = new Vector2 (10, 12f);

	//Tracking Player
	private Vector3     _playerPosition;
	public float        _maxDistanceFromPlayer = 300;
	#endregion

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {
		_ActionMan = _Tools._ActionManager;
		_CamTransform = _VirtualCamera.transform;
		_PlayerInput = _Tools.GetComponent<PlayerInput>();
		_InputScript= _Tools.GetComponent<S_PlayerInput>();

		SetModeOn(false);
	}

	// Update is called once per frame
	void Update () {
		_timeValuePerFrame = Time.deltaTime / _timeScaleWhenActive;

		Move();
		Rotate();
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	private void SetModeOn (bool newState) {
		//If entering mode
		if (newState)
		{
			_playerPosition = transform.position; //Since this is a child of player, save the player position during camera mode as its position at the start.

			//Places the camera mode camera at the same position as the current camera, then go from there.
			_CamTransform.position = Camera.main.transform.position;
			_CamTransform.rotation = Camera.main.transform.rotation;

			_saveInput = new Vector2(_InputScript.moveX, _InputScript.moveY); //Saves input so they can be set again as input are called as negative when switching action maps.

			_PlayerInput.SwitchCurrentActionMap("Camera Mode"); //Changes inputs to ones relevant to controlling the camera
		}
		//If exiting mode
		else
		{
			//Return move inputs to what they were before entering this mode.
			_InputScript.moveX = _saveInput.x;
			_InputScript.moveY = _saveInput.y;

			_PlayerInput.SwitchCurrentActionMap("Character Controls"); //Changes Inputs to ones relevant to controlling the character
		}

		enabled = newState; //Allows script to call Update function.
		_ActionMan._isPaused = newState;
		_VirtualCamera.gameObject.SetActive(newState);

		Time.timeScale = newState ? _timeScaleWhenActive : 1; //Either set time scale to normal, or to a very low value. This will either make things move normally, or barely move at all/
	}

	//Check the 3 directions the camera can move in, one by one. Applying inputs.
	private void Move () {
		Vector3 direction = _CamTransform.forward * _move.y;
		TryMove(direction);
		direction = _CamTransform.right * _move.x;
		TryMove(direction);
		direction = _CamTransform.up * _ascend;
		TryMove(direction);
	}

	//Rather than using physics, check the movement direction for a blockage, and move in that direction if there isn't one.
	private void TryMove (Vector3 direction) {
		Vector3 previousPosition = _CamTransform.position;
		if (!Physics.Raycast(_CamTransform.position, direction, 2, _Tools.CameraStats.DistanceStats.CollidableLayers))
		{
			_CamTransform.Translate(direction * _moveSpeed * _timeValuePerFrame, Space.World);

			//If this new position is too far away from the character.
			float newDistanceSquared = S_S_CoreMethods.GetDistanceOfVectors(_CamTransform.position, _playerPosition);
			if (newDistanceSquared > Mathf.Pow(_maxDistanceFromPlayer, 2))
			{
				//If moved further away from character, set back to previous position. This prevents moving too far from character, but if already too far, can still move back at least.
				if(newDistanceSquared > S_S_CoreMethods.GetDistanceOfVectors(_playerPosition, previousPosition))
				{
					_CamTransform.position = previousPosition;
				}
			}
		}
	}

	//Rotates by changing the euler angles by corresponding dimensions in the input Vector.
	private void Rotate () {
		//Because changing euler angles leads Unity to make automatic changes in its own scale, suggest a change here, get a change first, then ensure eulers are in the right scale.
		float xEuler = _CamTransform.eulerAngles.x - (_rotate.y * _rotateSpeed.y * _timeValuePerFrame);
		if(_CamTransform.eulerAngles.x > 180) { xEuler -= 360; }

		//Apply changes to eulers, ensuring up and down can't go too far.
		_CamTransform.eulerAngles = new Vector3 (Mathf.Clamp(xEuler, -85,+85), _CamTransform.eulerAngles.y + (_rotate.x * _rotateSpeed.x * _timeValuePerFrame), 0);
	}

	#endregion

	//New input system
	#region InputSystem
	public void InputEnterMode ( InputAction.CallbackContext ctx ) {
		if (ctx.performed)
		{
			SetModeOn(!enabled);
		}
	}

	public void InputMoveCam ( InputAction.CallbackContext ctx ) {
		_move = ctx.ReadValue<Vector2>();
	}

	public void InputRotateCam ( InputAction.CallbackContext ctx ) {
		_rotate = ctx.ReadValue<Vector2>();
	}

	public void InputElevate( InputAction.CallbackContext ctx ) {
		_ascend = ctx.ReadValue<Vector2>().y;
	}

	public void TakeShot () {
#if UNITY_EDITOR
		_VirtualCamera.GetComponent<S_TakeScreenShots>();
#endif
	}
	#endregion
}
