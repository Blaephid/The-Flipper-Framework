using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using Cinemachine;
//using UnityEngine.UIElements;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class S_HedgeCamera : MonoBehaviour
{
	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific properties
	public S_CharacterTools       _Tools;
	
	public S_PlayerPhysics        _PlayerPhys;
	public Transform              _Skin;
	public S_ActionManager        _Actions;
	public S_PlayerInput          _Input;

	public Transform              _PlayerTransformCopy;
	public Transform              _TargetBase;
	public Transform               _currentTarget;
	private Transform              _PlayerTransformReal;
	private CinemachineVirtualCamera _virtualCamera;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	[HideInInspector]
	public bool                   _shouldSetHeightWhenMoving_;
	private float                 _lockHeightSpeed_;
	private bool                  _shouldFaceDownWhenInAir_;
	private float                 _minHeightToLookDown_ = 50;
	private float                 _heightToLock_;
	private float                 _heightFollowSpeed_;
	private float                 _fallSpeedThreshold_;

	[HideInInspector]
	public float                  _cameraMaxDistance_ = 11;
	private AnimationCurve        _cameraDistanceBySpeed_;
	private Vector3               _angleThreshold_;

	private float                 _cameraRotationSpeed_ = 100;
	private float                 _cameraVerticalRotationSpeed_ = 10;
	private AnimationCurve        _VerticalFollowSpeedByAngle_;
	private float                 _cameraMoveSpeed_ = 100;

	private float                 _inputXSpeed_ = 1;
	private float                 _inputYSpeed_ = 0.5f;

	private float                 _afterMoveXDelay_ = 0.3f;
	private float                 _afterMoveYDelay_ = 0.5f;

	private float                 _stationaryCamIncrease_ = 1.2f;

	private float                 _yMinLimit_ = -20f;
	private float                 _yMaxLimit_ = 80f;

	[HideInInspector]
	public float                  _lockCamAtSpeed_ = 130;

	private float                 _shakeDampen_;
	public float                  _lockedRotationSpeed_;

	private Vector2               _softZone_;
	private Vector2               _deadZone_;
	private Vector2               _turnZone_;


	LayerMask                     _CollidableLayers_;
	private bool                  _shouldMoveInInputDirection_;
	#endregion

	// Trackers
	#region trackers

	[HideInInspector]
	public float                  _startLockCam;
	private bool                  _isFacingDown = false;

	private float                 _posX = 0.0f;
	private float                 _preX = 0.0f;
	[HideInInspector]
	public float                  _posY = 20.0f;
	public float                  _preY = 20.0f;

	private float                 _changeY;
	private float                 _changeX;

	private float                 _curveX;

	Quaternion                    _lerpedRot;
	Vector3                       _lerpedPos;

	private float                 _moveModifier = 1;


	public bool _isLocked { get; set; }
	public bool                   _canMove;
	public bool _isMasterLocked { get; set; }
	private float                 _heightToLook;
	[HideInInspector]
	public float                  _lookTimer;
	private float                 _backBehindTimer;
	private float                 _equalHeightTimer;


	private float                 _lockTimer;
	public bool                   _isReversed;

	//Effects
	Vector3                       _lookAtDir;
	bool			 _shouldAlignToExternal;
	Quaternion                    _externalAlignment;

	private float                 _distanceModifier = 1;
	private int                   frameCounter;

	public float _invertedX { get; set; }
	public float _invertedY { get; set; }
	public float _sensiX { get; set; }
	public float _sensiY { get; set; }

	Vector3                       _hitNormal;

	private Vector3               _targetOffset;
	private Vector3               _predictAheadPosition;
	#endregion

	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	//Start is called before the first frame update
	//Since this script is on the camera, not character, important components are assigned in the editor.
	void Start () {
		_virtualCamera = GetComponent<CinemachineVirtualCamera>();

		SetStats();

		_PlayerTransformReal = _PlayerPhys.transform;
		_targetOffset = _TargetBase.localPosition;
		_PlayerTransformCopy.rotation = _PlayerTransformReal.rotation;

		//Deals with cursor 
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	//LateUpdate is called at the end of an update, and all camera controls are handled here.
	void LateUpdate () {

		handleTargetPosition();
		AlignPlayerTransformCopy();
		HandleCameraMovement();	
		_isLocked = GetLocked();
		HandleCameraSituations();
		ApplyCameraEffects();

		ConfirmCameraChanges();
		HandleCameraDistance();
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//Handles directing the camera by player input or to a point of interest.
	private void HandleCameraMovement () {

		_preY = _posY;
		_preX = _posX;

		//If LookTimer is currently below zero, then direct the camera towards the point of interest
		if (_lookTimer < 0 || _lookTimer == 1)
		{
			float heightToGo = _heightToLook != 0 ? _heightToLook : _posY;
			RotateDirection(_lookAtDir, _lockedRotationSpeed_, heightToGo, true);

			if (_lookTimer < 0)
			{
				_lookTimer = Mathf.Clamp(_lookTimer + Time.deltaTime, _lookTimer, 0);
				if( _lookTimer == 0)
				{
					_shouldAlignToExternal = false;
				}
			}
		}

		//Changes the x and y values of the camera by input, changing speed based on a number of factors, such as if stationary and external modifiers.
		if (!_isLocked && _canMove)
		{
			float camMoveSpeed = _PlayerPhys._speedMagnitude > 10 ?     Time.deltaTime :    Time.deltaTime * _stationaryCamIncrease_;
			camMoveSpeed *= _moveModifier;
			float movementX = _Input.moveCamX * _inputXSpeed_ * _invertedX;
			float movementY = _Input.moveCamY * _inputYSpeed_ * _invertedY;
			_moveModifier = Mathf.MoveTowards(_moveModifier, 1, Time.deltaTime);

			_posX += movementX * camMoveSpeed;
			_posY -= movementY * camMoveSpeed;
		}

		//Pres were assigned in the movement script and this is used to see how much was changed this frame. 
		_changeY = Mathf.Abs(_preY - _posY);
		_changeX = Mathf.Abs(_preX - _posX);

		//If the camera was changed, then ready the delay before automoving.
		if (_changeY > 0.5f || _changeX > 0.5f)
		{
			_backBehindTimer = -_afterMoveXDelay_;
			_equalHeightTimer = -_afterMoveYDelay_;
		}
	}

	private void handleTargetPosition() {
		_PlayerTransformCopy.position = _PlayerTransformReal.position; ;

		if(_shouldMoveInInputDirection_)
		{
			Vector3 inputDirection = (_PlayerPhys._moveInput);

			Debug.DrawRay(_PlayerTransformReal.position, inputDirection * 2, Color.red, 5);
			_predictAheadPosition = Vector3.MoveTowards(_predictAheadPosition, inputDirection * 3, 0.15f);
			_currentTarget.localPosition = _predictAheadPosition;
		}	
	}

	//Handles the PlayerTransformCopy, making it match the actual player, or its own angle. This will later be used as a reference for the camera to rotate.
	void AlignPlayerTransformCopy () {

		Quaternion newRot = _PlayerTransformCopy.rotation;
		Quaternion targetRot = Quaternion.identity;
		bool willLerp = false;

		if ((_lookTimer < 0 || _lookTimer == 1) && _shouldAlignToExternal)
		{
			targetRot = _externalAlignment;
			willLerp = true;
		}
		//If the player is currently on a steeper angle than the set negative angle (E.G. on a horizontal wall), then make the transform copy relative to this ground as well.
		//Once the threshold is acceeded, the copy will keep following until it returns to upright.
		else if (_PlayerTransformReal.up.y < _angleThreshold_.x || _PlayerTransformCopy.up.y < 1f)
		{
			targetRot = _PlayerTransformReal.rotation;
			if (_PlayerTransformReal.up.y > _angleThreshold_.y) { targetRot = Quaternion.FromToRotation(_PlayerTransformReal.up, Vector3.up) * _PlayerTransformReal.rotation; }
			willLerp = true;			
		}

		if(willLerp)
		{
			//Slerp is used to make a smooth follow rather than a jittering.
			//But this means it can never reach the exact same rotation, so if the difference is small enough, then the slerp becomes 1.
			float slerpValue = Time.deltaTime * _cameraVerticalRotationSpeed_;
			float dif = Quaternion.Angle(newRot, targetRot) / 180;
			slerpValue = dif < 0.005f
				? 1 : slerpValue * _VerticalFollowSpeedByAngle_.Evaluate(dif);
			newRot = Quaternion.Slerp(newRot, targetRot, slerpValue);

			_PlayerTransformCopy.rotation = newRot;
		}
	}

	//Sets the camera a distance away from the player, based on stats and current speed.
	//Checks for any walls from the target to the camera, and if there is, decreases current distance so the camera is placed in front of it.
	void HandleCameraDistance () {
		float targetDistance = _cameraDistanceBySpeed_.Evaluate(_PlayerPhys._horizontalSpeedMagnitude / _PlayerPhys._currentMaxSpeed);
		_distanceModifier = Mathf.Lerp(_distanceModifier, targetDistance, 0.04f);

		float dist = _cameraMaxDistance_ * _distanceModifier;

		if(_virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>())
		{
			_virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = dist;
		}
		else
		{
			if (Physics.Raycast(_currentTarget.position, -transform.forward, out RaycastHit hitWall, -dist, _CollidableLayers_))
			{
				dist = (-hitWall.distance);
				_hitNormal = hitWall.normal * 0.5f;
			}
			else
			{
				_hitNormal = Vector3.zero;
			}

			var position = _lerpedRot * new Vector3(0, 0, dist + 0.3f);
			_lerpedPos = position;
		}

	}

	//See if camera should be locked and decrease counter if so.
	private bool GetLocked () {

		if (_isMasterLocked)
		{
			return true;
		}
		else if (_isLocked && _lockTimer < 0)
		{
			_lockTimer = Mathf.Clamp(_lockTimer + Time.deltaTime, _lookTimer, 0);
			if (_lockTimer == 0)
			{
				return false;
			}
		}
		return _isLocked;
	}

	//Handles the height or direction of the camera based on certain situations such as when falling for too long.
	void HandleCameraSituations () {

		if (_isLocked) { return; }

		//Changing camera height
		float verticalSpeed = _PlayerTransformReal.InverseTransformDirection(_PlayerPhys._RB.velocity).y;

		//Making the camera face down when in the air for long enough.
		bool isRightAction = _Actions.whatAction == S_Enums.PlayerStates.Jump || _Actions.whatAction == S_Enums.PlayerStates.Regular || _Actions.whatAction == S_Enums.PlayerStates.DropCharge;
		if (_shouldFaceDownWhenInAir_ && !_PlayerPhys._isGrounded && verticalSpeed < _fallSpeedThreshold_ && isRightAction)
		{
			//If isn't facing down yet, then check high enough in the air to warrent changing view.
			if (_isFacingDown)
			{
				ChangeHeight(100, _heightFollowSpeed_);
			}
			else if (!_isFacingDown && !Physics.Raycast(_PlayerTransformReal.position, Vector3.down, _minHeightToLookDown_, _PlayerPhys._Groundmask_) && _PlayerTransformCopy.up.y > 0.99)
			{
				_isFacingDown = true;
			}
		}
		//When facing down ends, quickly turn camera to face up.
		else if (_isFacingDown)
		{
			_isFacingDown = false;
			StartCoroutine(MoveFromDowntoForward(60));
		}
		//If over a certain speed, then camera will start drifting to height.
		else if (_shouldSetHeightWhenMoving_ && _PlayerPhys._horizontalSpeedMagnitude >= 10)
		{
			if (_equalHeightTimer < 0)
				_equalHeightTimer += Time.deltaTime;
			else
				ChangeHeight(_heightToLock_, _lockHeightSpeed_);
		}

		//Changing camera direction
		if (_backBehindTimer < 0)
			_backBehindTimer += Time.deltaTime;

		//Certain actions will have different requirements for the camera to move behind. The switch sets the requirements before the if statement checks against them.
		float minSpeed;
		bool skipDelay;
		switch (_Actions.whatAction)
		{
			default:
				minSpeed = _lockCamAtSpeed_;
				skipDelay = false;
				break;
			case S_Enums.PlayerStates.WallRunning:
				minSpeed = 60;
				skipDelay = true;
				break;
		}

		if (_PlayerPhys._horizontalSpeedMagnitude > minSpeed && (_backBehindTimer >= 0 || skipDelay))
		{
			GoBehindCharacter(5, 0, false);
		}
	}

	//Takes the changes to _posY and _posX, and applies the rotation and position of to the Camera.
	private void ConfirmCameraChanges () {

		//Applies the rotation and placement from the previous update. Having this applied the new data this frame causes jittering.
		//transform.position = _cameraPivotPoint + _lerpedPos + _hitNormal;
		//transform.position += _lerpedPos + _hitNormal;
		//transform.localPosition = _lerpedPos + _hitNormal;
		transform.rotation = _lerpedRot;

		//Keeps horizontal and vertical positons always represented within 360 degrees
		_posX = ClampAngleX(_posX, -1, 361);
		_posY = ClampAngleY(_posY, _yMinLimit_, _yMaxLimit_);

		//Takes the x and y positions as euler angles around the player, then bases it around the player.
		_lerpedRot = Quaternion.Euler(_posY, _posX, 0);
		_lerpedRot = _PlayerTransformCopy.rotation * _lerpedRot;		
	}

	private void ApplyCameraEffects() {
		
	}


	//For a set number of frames, the height follow speed will be increased to move from looking down to following player direction again. 
	IEnumerator MoveFromDowntoForward ( int frames ) {
		float initialFollow = _lockHeightSpeed_;
		_lockHeightSpeed_ *= 8f;
		for (int i = 0 ; i < frames ; i++)
		{
			yield return new WaitForFixedUpdate();
			//If manually moving the camera height after half of the loop, then can afford to disable the increase and return normal speed.
			if (i > frames / 1.5f && _changeY > 1)
			{
				break;
			}
			else
			{
				_equalHeightTimer = 1;
			}
		}
		_lockHeightSpeed_ = initialFollow;
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	//Causes the camera to turn around to face a designated vector direction in world space.
	public void RotateDirection ( Vector3 dir, float speed, float height, bool changeHeight ) {
		
		//Get a rotation based off the look direction without compensating player's current up.	
		Vector3 newDirection = _PlayerTransformCopy.InverseTransformDirection(dir);
		Quaternion localDirection = Quaternion.LookRotation(newDirection, _PlayerTransformCopy.up);

		//Get local versions of the euler angles which are used for x and y cam positons.
		Vector3 eulers = localDirection.eulerAngles;

		//MoveTowards can't compute looping around where -1 is actually 359. This tells it to reverse the usual way if it would be quicker to rotate through this loop.
		float xSpeed = speed;
		if((_posX < 90 && eulers.y > 270) || (_posX > 270 && eulers.y < 90)) { xSpeed = -speed; }

		_posX = Mathf.MoveTowards(_posX, eulers.y, xSpeed);

		if(changeHeight)
		{
			//Y position will be acquired either from a designated height or part of the direction.
			if (_posY != height)
			{
				_posY = Mathf.MoveTowards(_posY, height, speed);
			}
			else
			{
				float yTarget = eulers.x;
				if (yTarget > 180) { yTarget -= 360; }
				if (yTarget < -180) { yTarget += 360; }

				_posY = Mathf.MoveTowards(_posY, yTarget, speed);
			}
		}
	}

	//Called by other scripts to make the camera face the direction the character is facing.
	public void GoBehindCharacter ( float speed, float height, bool overwrite = false ) {
		bool changeHeight = height != 0;

		if (_isReversed)
		{
			RotateDirection(-_Skin.forward, speed, height, changeHeight);
		}

		else if (!_isLocked || overwrite)
		{

			RotateDirection(_Skin.forward, speed, height, changeHeight);
		}
	}

	//Called by other scripts to immediately set the camera to behind the character.
	public void SetBehind (int height) {
		bool changeHeight = height != 0;
		RotateDirection(_Skin.forward, 2000, 14, changeHeight);

	}

	//Changes only the height of the camera to look up or down.
	public void ChangeHeight ( float height, float speed ) {
		if (!_isLocked)
		{
			//A switch is used so it's less clutured than an if statement.
			switch(_Actions.whatAction)
			{
				case S_Enums.PlayerStates.Rail:
					break;
				default:
				{
					if (_PlayerPhys._isGrounded)
					{
						_posY = Mathf.MoveTowards(_posY, height, Time.deltaTime * speed);
					}
					else
					{
						_posY = Mathf.MoveTowards(_posY, height, Time.deltaTime * speed);
					}
					break;
				}
			}
		}
	}

	//Prevents eulers being numbers outside of 360 degree, then applies limits.
	public float ClampAngleY ( float angle, float min, float max ) {
		while (angle < -180)
			angle += 360F;
		while (angle > 180F)
			angle -= 360F;

		return Mathf.Clamp(angle, min, max);
	}
	public float ClampAngleX ( float angle, float min, float max ) {
		while (angle < 0)
			angle += 360F;
		while (angle > 361F)
			angle -= 360F;

		return Mathf.Clamp(angle, min, max);
	}


	//Tells the camera to look in the direction, changing eulers over the next few frames to do so. See camera movement and rotate direction for more.
	public void SetCameraToDirection ( Vector3 dir, float duration, float heightSet, float speed, Quaternion target, bool align ) {

		_lookAtDir = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = heightSet;
		_lockedRotationSpeed_ = speed;
		_externalAlignment = target;
		_shouldAlignToExternal = align;

	}

	//Tell camera to look in direction but not change height seperately from the target.
	public void SetCameraNoHeight ( Vector3 dir, float duration, float speed, Quaternion target, bool align ) {
		_lookAtDir = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = 0;
		_lockedRotationSpeed_ = speed;
		_externalAlignment = target;
		_shouldAlignToExternal = align;
	}

	//Set camera to direction but with a change to movement input.
	public void SetCamera ( Vector3 dir, float duration, float heightSet, float speed, float lagSet ) {

		_lookAtDir = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = heightSet;
		_lockedRotationSpeed_ = speed;
		_moveModifier = lagSet;

	}

	//Only changes height
	public void SetCameraNoLook ( float heightSet ) {
		_lookTimer = 1;
		_heightToLook = heightSet;
	}

	//Called by other scripts to make the camera shake with force  for a time.
	public void ApplyCameraShake ( float shakeForce, int frames ) {
		StopCoroutine(ShakeCamera(1, 1));
		StartCoroutine(ShakeCamera(shakeForce, frames));
	}
	//At the end of every fixed update will apply the shake force to move the camera slightly, but slowly decrease it across the set time.
	public IEnumerator ShakeCamera ( float shakeForce, int frames ) {

		float lerp = 1 / frames;
		float counter = 0;
		shakeForce /= _shakeDampen_;
		float force = shakeForce;
		while (true)
		{
			yield return new WaitForEndOfFrame();

			//Get change from shake force
			float noiseX = (Random.Range(-shakeForce, shakeForce));
			float noiseY = (Random.Range(-shakeForce, shakeForce));
			float shakeX = (transform.position.x + noiseX);
			float shakeY = (transform.position.y + noiseY);

			//Apply shake, then decrease it for the next.
			transform.position = new Vector3(shakeX, shakeY, transform.position.z);
			shakeForce = Mathf.Lerp(force, 0, counter);
			counter += lerp;

			//End coroutine
			if (shakeForce == 0) break;
		}
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	void SetStats () {
		_isLocked = false;
		_canMove = true;

		_startLockCam = _lockCamAtSpeed_;
		_shouldSetHeightWhenMoving_ = _Tools.camStats.LockHeight;
		_lockHeightSpeed_ = _Tools.camStats.LockHeightSpeed;
		_shouldFaceDownWhenInAir_ = _Tools.camStats.MoveHeightBasedOnSpeed;
		_minHeightToLookDown_ = _Tools.camStats.minHeightToLookDown;
		_heightToLock_ = _Tools.camStats.HeightToLock;
		_heightFollowSpeed_ = _Tools.camStats.HeightFollowSpeed;
		_fallSpeedThreshold_ = _Tools.camStats.FallSpeedThreshold;

		_cameraMaxDistance_ = _Tools.camStats.CameraMaxDistance;
		_cameraDistanceBySpeed_ = _Tools.camStats.cameraDistanceBySpeed;
	         _angleThreshold_ = _Tools.camStats.AngleThreshold;

		_cameraRotationSpeed_ = _Tools.camStats.CameraRotationSpeed;
		_cameraVerticalRotationSpeed_ = _Tools.camStats.CameraVerticalRotationSpeed;
		_VerticalFollowSpeedByAngle_ = _Tools.camStats.vertFollowSpeedByAngle;
		_cameraMoveSpeed_ = _Tools.camStats.CameraMoveSpeed;

		_inputXSpeed_ = _Tools.camStats.InputXSpeed;
		_inputYSpeed_ = _Tools.camStats.InputYSpeed;
		_stationaryCamIncrease_ = _Tools.camStats.stationaryCamIncrease;

		_afterMoveXDelay_ = _Tools.camStats.afterMoveXDelay;
		_afterMoveYDelay_ = _Tools.camStats.afterMoveYDelay;

		_yMinLimit_ = _Tools.camStats.yMinLimit;
		_yMaxLimit_ = _Tools.camStats.yMaxLimit;

		_lockCamAtSpeed_ = _Tools.camStats.LockCamAtHighSpeed;

		_lockedRotationSpeed_ = _Tools.camStats.LockedRotationSpeed;
		_shakeDampen_ = _Tools.camStats.ShakeDampen;

		_virtualCamera.GetComponent<CinemachineCollider>().m_CollideAgainst = _Tools.camStats.CollidableLayers;

		_softZone_ = _Tools.camStats.softZone;
		_deadZone_ = _Tools.camStats.deadZone;
		_turnZone_ = _Tools.camStats.turnZone;
		_shouldMoveInInputDirection_ = _Tools.camStats.shouldMoveInInputDirection;
	}
	#endregion


}




