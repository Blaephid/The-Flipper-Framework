using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEditor;

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
	private S_Handler_Camera      _CamHandler;
	private S_CharacterTools      _Tools;

	private Transform             _Camera; // A reference to the main camera in the scene's transform

	#endregion

	// Trackers
	#region trackers
	[HideInInspector]
	public Vector3     _move;
	public Vector3      _inputWithoutCamera;
	public Vector3      _prevInputWithoutCamera;
	[HideInInspector]
	public Vector3     _camMoveInput;

	public bool	_isInputLocked { get; set; }
	float               _lockedTime;
	float               _lockedCounter = 0;
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
	float				camSensi;
	public float			mouseSensi;

	[HideInInspector] public bool		JumpPressed;
	[HideInInspector] public bool		RollPressed;
	[HideInInspector] public bool		SpecialPressed;
	[HideInInspector] public bool		LeftStepPressed;
	[HideInInspector] public bool		RightStepPressed;
	[HideInInspector] public bool		BouncePressed;
	[HideInInspector] public bool		InteractPressed;
	[HideInInspector] public bool		CamResetPressed;
	[HideInInspector] public bool		HomingPressed;
	[HideInInspector] public bool		spinChargePressed;
	[HideInInspector] public bool		killBindPressed;

	[HideInInspector] public bool		usingMouse = false;

	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {
		// Set up the reference.
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_CamHandler = GetComponent<S_Handler_Camera>();
		_Tools = GetComponent<S_CharacterTools>();

		// get the transform of the main camera
		if (Camera.main != null)
		{
			_Camera = Camera.main.transform;
		}

		//Managing Inputs
		mouseSensi = _Tools.camStats.InputStats.InputMouseSensi;
		camSensi = _Tools.camStats.InputStats.InputSensi;
	}

	// Update is called once per frame
	void Update () {
		AcquireMoveInput();
	}

	private void FixedUpdate () {
		if (!_isInputLocked)
		{
			_PlayerPhys._moveInput = _move;
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

		//Lock Input Funcion
		if (_isInputLocked)
		{
			HandleLockedInput();
		}

		//Calculate move direction
		else if (_Camera != null)
		{
			//Make movement relative to camera and character
			_inputWithoutCamera = new Vector3(moveX, 0, moveY);
			_camMoveInput = GetInputByLocalTransform(_inputWithoutCamera);
			_move = _camMoveInput;
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
			_camMoveInput = transformedInput;

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

		//Sets the camera behind if locked when input is.
		if (_isCamLocked)
		{
			_CamHandler._HedgeCam.GoBehindCharacter(3, 0, true);
		}

		if (_lockedCounter > _lockedTime)
		{
			_isInputLocked = false;
		}
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//Called by other scripts to set the input to a specific thing, unable to change for a period of time.
	public void LockInputForAWhile ( float duration, bool lockCam, Vector3 newInput ) {
		_move = newInput;

		//Sets time to count to before unlocking. If already locked, then will only change if to a higher timer.
		_lockedTime = Mathf.Max(duration, _lockedTime);

		//Will be locked until counter exceeds timer.
		_lockedCounter = 0;
		_isInputLocked = true;
		_isCamLocked = lockCam; //Also prevents camera control
	}
	#endregion

	#region inputSystem
	public void MoveInput ( InputAction.CallbackContext ctx ) {
		moveVec = ctx.ReadValue<Vector2>();
		usingMouse = false;
		moveX = moveVec.x;
		moveY = moveVec.y;
	}

	public void MoveInputKeyboard ( InputAction.CallbackContext ctx ) {
		moveVec = ctx.ReadValue<Vector2>();
		moveX = moveVec.x;
		moveY = moveVec.y;
		usingMouse = true;
	}

	public void CamInput ( InputAction.CallbackContext ctx ) {
		usingMouse = false;
		CurrentCamMovement = ctx.ReadValue<Vector2>();
		moveCamX = CurrentCamMovement.x * camSensi;
		moveCamY = CurrentCamMovement.y * camSensi;
	}

	public void CamMouseInput ( InputAction.CallbackContext ctx ) {
		usingMouse = true;
		CurrentCamMovement = ctx.ReadValue<Vector2>();
		moveCamX = CurrentCamMovement.x * mouseSensi;
		moveCamY = CurrentCamMovement.y * mouseSensi;
	}

	public void Jump ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			JumpPressed = ctx.ReadValueAsButton();
		}
	}

	public void Roll ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			RollPressed = ctx.ReadValueAsButton();
		}
	}

	public void LeftStep ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			LeftStepPressed = ctx.ReadValueAsButton();
		}
	}

	public void RightStep ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			RightStepPressed = ctx.ReadValueAsButton();
		}
	}

	public void Special ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			SpecialPressed = ctx.ReadValueAsButton();
		}
	}

	public void Homing ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			HomingPressed = ctx.ReadValueAsButton();
		}
	}

	public void Interact ( InputAction.CallbackContext ctx ) {
		if (ctx.performed || ctx.canceled)
		{
			InteractPressed = ctx.ReadValueAsButton();
		}
	}

	public void Power ( InputAction.CallbackContext ctx ) {
		if (ctx.performed)
		{
			if (!_PlayerPhys._isGrounded)
			{
				BouncePressed = ctx.ReadValueAsButton();
			}
		}

		else if (ctx.canceled)
		{
			BouncePressed = ctx.ReadValueAsButton();
		}
	}

	public void SpinCharge ( InputAction.CallbackContext ctx ) {
		if (ctx.performed)
		{
			if (_PlayerPhys._isGrounded)
				spinChargePressed = ctx.ReadValueAsButton();

		}
		else if (ctx.canceled)
		{
			spinChargePressed = ctx.ReadValueAsButton();
		}
	}

	public void KillBind ( InputAction.CallbackContext ctx ) {
		if (ctx.performed)
		{
			killBindPressed = ctx.ReadValueAsButton();

		}
		else if (ctx.canceled)
		{
			killBindPressed = ctx.ReadValueAsButton();
		}
	}

	public void CamReset ( InputAction.CallbackContext ctx ) {
		if (ctx.performed)
		{
			CamResetPressed = !CamResetPressed;
			if (_CamHandler._HedgeCam._lockCamAtSpeed_ != 20f)
				_CamHandler._HedgeCam._lockCamAtSpeed_ = 20f;
			else
				_CamHandler._HedgeCam._lockCamAtSpeed_ = _CamHandler._HedgeCam._startLockCam;
		}

	}
	#endregion


}
