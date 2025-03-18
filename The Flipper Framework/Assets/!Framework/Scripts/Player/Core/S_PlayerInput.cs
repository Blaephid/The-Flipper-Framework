using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.Windows;

public class S_PlayerInput : MonoBehaviour
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Properties

	//Unity
	#region Unity Specific

	private S_PlayerPhysics       _PlayerPhys;
	private S_PlayerMovement	_PlayerMovement;
	private S_Handler_Camera      _CamHandler;
	private S_CharacterTools      _Tools;

	private Transform             _Camera; // A reference to the main camera in the scene's transform

	private Transform             _MainSkin;

	#endregion

	// Trackers
	#region trackers
	[HideInInspector]
	public Vector3     _move; //The final input acquired and passed onto PlayerPhysics. Can be locked.
	private Vector3     _lockedMoveInput;
	[HideInInspector]
	public Vector3      _inputOnController; //The input acquired just from the controller, not relevant to character or camera.
	private Vector2     _lockedControllerInput; //Because acceleration takes the magnitude of the input on the controller, when input is locked, set the above to this so there is a magnitude to the locked input.
	[HideInInspector]
	public Vector3      _prevInputWithoutCamera; //The input without the camera last frame. This is to check when the used input is changed without changing the controller input (meaning the character and camera did it).
	[HideInInspector]
	public Vector3     _camMoveInput; //The input relevant to the camera, but not in local space with the character.
	[HideInInspector]
	public Vector3     _constantInputRelevantToCharacter; //Equal to _move, but never locked (and not used in movement), called globally to check actual input for other calculations even if locked.

	private Vector3     _inputCheckedLastFrame;

	public bool	_isInputLocked { get; set; }
	float               _lockedTime;
	float               _lockedCounter = 0;
	private bool            _lockedToCharacter;
	[HideInInspector]
	public bool	_isCamLocked { get; set; }

	//input
	//NewInput system
	public PlayerNewInput		newInput;

	//NewInput inputs stored
	[HideInInspector] public float	moveX;
	[HideInInspector] public float	moveY;
	[HideInInspector] public Vector2	moveVec;

	Vector2 CurrentCamMovement;
	[HideInInspector] public float	moveCamX;
	[HideInInspector] public float	moveCamY;
	private float			camSensi;
	[HideInInspector]
	public float			mouseSensi;

	[HideInInspector] public bool		_JumpPressed;
	[HideInInspector] public bool		_RollPressed;
	[HideInInspector] public bool		_SpecialPressed;
	[HideInInspector] public bool		_BoostPressed;
	[HideInInspector] public bool		_LeftStepPressed;
	[HideInInspector] public bool		_RightStepPressed;
	[HideInInspector] public bool		_BouncePressed;
	[HideInInspector] public bool		_PowerPressed;
	[HideInInspector] public bool		_InteractPressed;
	[HideInInspector] public bool		_CamResetPressed;
	[HideInInspector] public bool		_HomingPressed;
	[HideInInspector] public bool		_SpinChargePressed;
	[HideInInspector] public bool		_KillBindPressed;

	[HideInInspector] public bool		_isUsingMouse = false;

	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Awake () {
		// Set up the reference.
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_PlayerMovement = _Tools.GetComponent<S_PlayerMovement>();
		_CamHandler = _Tools.CamHandler;
		_MainSkin = _Tools.MainSkin;

		// get the transform of the main camera
		if (Camera.main != null)
		{
			_Camera = Camera.main.transform;
		}

		//Managing Inputs
		mouseSensi = _Tools.CameraStats.InputStats.InputMouseSensi;
		camSensi = _Tools.CameraStats.InputStats.InputSensi;
	}

	// Update is called once per frame
	void Update () {
	}

	private void FixedUpdate () {
		AcquireMoveInput();

#if UNITY_EDITOR
		if (UnityEngine.Input.GetKeyDown(KeyCode.LeftControl))
		{
			if (_isInputLocked)
				UnLockInput();
			else
				LockInputForAWhile(1000, false, _move);
		}
#endif

		if (!_isInputLocked)
		{
			_PlayerMovement._moveInput = _move;
		}
		else
		{
			_PlayerMovement._moveInput = _PlayerPhys.GetRelevantVector(_lockedMoveInput, false);
		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private
	//Figures out the desired movement direction from input, camera and player transform.
	private void AcquireMoveInput () {

		//Calculate move direction
		if (_Camera != null)
		{
			//Make movement relative to camera and character
			_inputOnController = new Vector3(moveX, 0, moveY);
			_camMoveInput = GetInputByLocalTransform(_inputOnController);
			_move = _camMoveInput;
			_constantInputRelevantToCharacter = transform.TransformDirection(_camMoveInput);
		}

		//Lock Input Funcion
		if (_isInputLocked)
		{
			_inputOnController = _lockedControllerInput;
			HandleLockedInput();
		}
	}

	//Takes in a direction and returns it relative to the camera and player
	private Vector3 GetInputByLocalTransform ( Vector3 inputDirection ) {

		if (inputDirection != Vector3.zero)
		{
			Vector3 transformedInput;
			Vector3 upDirection = _PlayerPhys._isGrounded ? _PlayerPhys._groundNormal : transform.up;

			//Affect input by camera
			transformedInput = Quaternion.FromToRotation(_Camera.up, upDirection) * (_Camera.rotation * inputDirection);
			//_camMoveInput = transformedInput;

			//Makes input relevant to character.
			transformedInput = transform.InverseTransformDirection(transformedInput);
			transformedInput.y = 0.0f;
			return transformedInput;
		}
		return inputDirection;
	}

	//Prevents changing input when input is locked, but counts up the frames until timer has expired.
	private void HandleLockedInput () {
		_lockedCounter += 1;

		UpdateCharacterForwardsInput();

		//Sets the camera behind if locked when input is.
		if (_isCamLocked)
		{
			_CamHandler._HedgeCam.GoBehindCharacter(3, 0, true);
		}

		if (_lockedCounter > _lockedTime && _lockedTime != -1)
		{
			UnLockInput();
		}
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//Called by other scripts to set the input to a specific thing, unable to change for a period of time.
	public void LockInputForAWhile ( float frames, bool lockCam, Vector3 newInput, S_GeneralEnums.LockControlDirection whatLock = S_GeneralEnums.LockControlDirection.Change, bool overwrite = false) {

		if(_isInputLocked && frames < _lockedTime - _lockedCounter) { return; } //If this new lock has less frames than the amount left of the current one, then ignore it.

		_lockedToCharacter = false;

		//While the enum won't be used freqeuntly, it is short hand for removing input or setting player to forwards without having to calculate it before being called.
		switch (whatLock)
		{
			//If enum is not set in the call, move becomes the input given.
			case S_GeneralEnums.LockControlDirection.Change:
				_lockedControllerInput = Vector2.one;
				_lockedMoveInput = newInput; break;
			case S_GeneralEnums.LockControlDirection.NoInput:
				_lockedControllerInput = Vector2.zero;
				_lockedMoveInput = Vector3.zero; break;
			case S_GeneralEnums.LockControlDirection.CharacterForwards:
				_lockedControllerInput = Vector2.one;
				Debug.DrawRay(_PlayerPhys._CharacterCenterPosition, _MainSkin.forward * 5, Color.red, 20f);
				_lockedToCharacter = true;
				_lockedMoveInput = _MainSkin.forward; break;
		}

		_PlayerMovement._moveInput = _PlayerPhys.GetRelevantVector(_lockedMoveInput, false);

		//Sets time to count to before unlocking. If already locked, then will only change if to a higher timer.
		_lockedTime = frames;

		//Will be locked until counter exceeds timer.
		_lockedCounter = 0;
		_isInputLocked = true;
		_isCamLocked = lockCam; //Also prevents camera control
	}

	private void UpdateCharacterForwardsInput () {
		if (!_lockedToCharacter) { return; }

		_lockedControllerInput = Vector2.one;
		_lockedMoveInput = _MainSkin.forward;
		_PlayerMovement._moveInput = _PlayerPhys.GetRelevantVector(_lockedMoveInput, false);
	}

	public void LockInputIndefinately ( bool lockCam, Vector3 newInput, S_GeneralEnums.LockControlDirection whatLock = S_GeneralEnums.LockControlDirection.Change ) {
		LockInputForAWhile(1, lockCam, newInput, whatLock);

		_lockedTime = -1;
	}

	public void UnLockInput () {
		_lockedTime = 0;
		_isInputLocked = false;
		_isCamLocked = false; 
	}

	//Called externally once per frame to check if the input is different to last frame despite the actual controller input not being changed.
	public bool IsTurningBecauseOfCamera (Vector3 inputDirection, float threshold = 1) {
		bool isCamera = false;
		if (Vector3.Angle(_inputCheckedLastFrame, inputDirection) > threshold)                      //If move input is noticeably different to how it was last frame.
		{
			if (_prevInputWithoutCamera == _inputOnController) //But if controlled input has not changed, this means the input is only changed because of the camera.
			{
				isCamera = true;
			}
		}
		_prevInputWithoutCamera = _inputOnController;
		_inputCheckedLastFrame = inputDirection;
		return isCamera;
	}

	#endregion


	#region inputSystem
	public void MoveInput ( InputAction.CallbackContext ctx ) {
		moveVec = ctx.ReadValue<Vector2>();
		_isUsingMouse = false;
		moveX = moveVec.x;
		moveY = moveVec.y;
	}

	public void MoveInputKeyboard ( InputAction.CallbackContext ctx ) {
		moveVec = ctx.ReadValue<Vector2>();
		moveX = moveVec.x;
		moveY = moveVec.y;
		_isUsingMouse = true;
	}

	public void CamInput ( InputAction.CallbackContext ctx ) {
		_isUsingMouse = false;
		CurrentCamMovement = ctx.ReadValue<Vector2>();
		moveCamX = CurrentCamMovement.x * camSensi;
		moveCamY = CurrentCamMovement.y * camSensi;
	}

	public void CamMouseInput ( InputAction.CallbackContext ctx ) {
		_isUsingMouse = true;
		CurrentCamMovement = ctx.ReadValue<Vector2>();
		moveCamX = CurrentCamMovement.x * mouseSensi;
		moveCamY = CurrentCamMovement.y * mouseSensi;
	}

	public void Jump ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			_JumpPressed = ctx.ReadValueAsButton();
		}
	}

	public void Roll ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			_RollPressed = ctx.ReadValueAsButton();
		}
	}

	public void LeftStep ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			_LeftStepPressed = ctx.ReadValueAsButton();
		}
	}

	public void RightStep ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			_RightStepPressed = ctx.ReadValueAsButton();
		}
	}

	public void Special ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			_SpecialPressed = ctx.ReadValueAsButton();
		}
	}

	public void Boost ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			_BoostPressed = ctx.ReadValueAsButton();
		}
	}

	public void Homing ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			_HomingPressed = ctx.ReadValueAsButton();
		}
	}

	public void Interact ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			_InteractPressed = ctx.ReadValueAsButton();
		}
	}

	public void Power ( InputAction.CallbackContext ctx ) {

		if (ctx.performed)
		{
			_PowerPressed = ctx.ReadValueAsButton();
			_BouncePressed = !_PlayerPhys._isGrounded;
		}

		else if (ctx.canceled)
		{
			_PowerPressed = ctx.ReadValueAsButton();
			_BouncePressed = ctx.ReadValueAsButton();
		}
	}

	public void SpinCharge ( InputAction.CallbackContext ctx ) {
		if (ctx.performed)
		{
			if (_PlayerPhys._isGrounded)
				_SpinChargePressed = ctx.ReadValueAsButton();

		}
		else if (ctx.canceled)
		{
			_SpinChargePressed = ctx.ReadValueAsButton();
		}
	}

	public void KillBind ( InputAction.CallbackContext ctx ) {
		if (ctx.performed)
		{
			_KillBindPressed = ctx.ReadValueAsButton();

		}
		else if (ctx.canceled)
		{
			_KillBindPressed = ctx.ReadValueAsButton();
		}
	}

	public void CamReset ( InputAction.CallbackContext ctx ) {
		if (ctx.performed)
		{
			_CamResetPressed = !_CamResetPressed;
		}
	}
	#endregion


}
