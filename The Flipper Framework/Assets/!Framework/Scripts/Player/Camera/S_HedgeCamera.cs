using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

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
	public Transform              _Target;
	public S_PlayerPhysics        _PlayerPhys;
	public Transform              _Skin;
	public S_ActionManager        _Actions;
	public S_PlayerInput          _Input;

	public Transform              _PlayerTransformCopy;
	private Transform              _PlayerTransformReal;
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
	public float                  _cameraMaxDistance_ = -11;
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

	private float                 _moveLerpingSpeed_;
	private float                 _rotationLerpingSpeed_;

	LayerMask                     _CollidableLayers_;
	#endregion

	// Trackers
	#region trackers

	[HideInInspector]
	public float                  _startLockCam;
	private bool                  _isFacingDown = false;
	private bool                  _isAligningToPlayer;

	//The countdown prevents constant jittering when alternating between the lock cam speed
	private bool                  _shouldCheckforBehind = false;
	private bool                  _isAboveSpeedLock;

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

	public float                  _lockedRotationSpeed_;

	//Cached variables
	private float                 _initialLockedRotationSpeed;

	private float                 _lockTimer;
	public bool                   _isReversed;

	//Effects
	Vector3                       _lookAtDir;

	public float _invertedX { get; set; }
	public float _invertedY { get; set; }
	public float _sensiX { get; set; }
	public float _sensiY { get; set; }

	Vector3                       _hitNormal;
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

		SetStats();

		_PlayerTransformReal = _PlayerPhys.transform;
		_PlayerTransformCopy.rotation = _PlayerTransformReal.rotation;

		//Deals with cursor 
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	//LateUpdate is called at the end of an update, and all camera controls are handled here.
	void LateUpdate () {
		HandleCameraMovement();
		AlignPlayerTransformCopy();
		HandleCameraCollision();
		_isLocked = GetLocked();
		HandleCameraSituations();

		transform.position = _lerpedPos + _hitNormal;
		transform.rotation = _lerpedRot;

		ApplyCameraEffects();
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
			RotateDirection(_lookAtDir, _lockedRotationSpeed_, heightToGo);

			if (_lookTimer < 0) _lookTimer = Mathf.Clamp(_lookTimer + Time.deltaTime, _lookTimer, 0);
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
	}

	//Handles the PlayerTransformCopy, making it match the actual player, or its own angle. This will later be used as a reference for the camera to rotate.
	void AlignPlayerTransformCopy () {
		_PlayerTransformCopy.position = _Target.position;

		//If the player is currently on an steeper angle than the set negative angle (E.G. on a horizontal wall), then make the transform copy relative to this ground as well.
		//Once the threshold is acceeded, the copy will keep following until it returns to upright.
		if (_PlayerTransformReal.up.y < _angleThreshold_.x || _PlayerTransformCopy.up.y < 1f)
		{
			Quaternion newRot = _PlayerTransformCopy.rotation;
			Quaternion targetRot = _PlayerTransformReal.rotation;
			if (_PlayerTransformReal.up.y > _angleThreshold_.y) { targetRot = Quaternion.FromToRotation(_PlayerTransformReal.up, Vector3.up) * _PlayerTransformReal.rotation; }

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

	//Checks for any walls from the target to the camera, and if there is, decreases current distance so the camera is placed in front of it.
	void HandleCameraCollision () {
		float dist;
		if (Physics.Raycast(_Target.position, -transform.forward, out RaycastHit hitWall, -_cameraMaxDistance_, _CollidableLayers_))
		{
			dist = (-hitWall.distance);
			_hitNormal = hitWall.normal;
		}
		else
		{
			_hitNormal = Vector3.zero;
			dist = _cameraMaxDistance_;
		}

		var position = _lerpedRot * new Vector3(0, 0, dist + 0.3f) + _Target.position;
		_lerpedPos = position;
	}

	//See if camera should be locked and decrease counter if so.
	private bool GetLocked () {

		if (_isMasterLocked)
		{
			return true;
		}
		else if (_isLocked && _lookTimer < 0)
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
				FollowHeightDirection(100, _heightFollowSpeed_);
			}
			else if (!_isFacingDown && !Physics.Raycast(transform.position, Vector3.down, _minHeightToLookDown_, _PlayerPhys._Groundmask_) && _PlayerTransformCopy.up.y > 0.99)
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
				FollowHeightDirection(_heightToLock_, _lockHeightSpeed_);
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
			FollowDirection(3, 14, -10, 0);
		}
	}
	private void ApplyCameraEffects () {

		//Applies y and x changes. Outside of the if statement so whenever the x and y are changed it will be tracked.	
		_posY = ClampAngle(_posY, _yMinLimit_, _yMaxLimit_);

		_changeY = Mathf.Abs(_preY - _posY);
		_changeX = Mathf.Abs(_preX - _posX);

		if (_changeY > 0.5f || _changeX > 0.5f)
		{
			_backBehindTimer = -_afterMoveXDelay_;
			_equalHeightTimer = -_afterMoveYDelay_;
		}
		else
		{
			//Keeps horizontal positon always represented within 360 degrees
			//if (_posX > 360) { _posX -= 360; }
			//else if (_posX < 0) { _posX += 360; }
		}

		_lerpedRot = Quaternion.Euler(_posY, _posX, 0);
		_lerpedRot = _PlayerTransformCopy.rotation * _lerpedRot;
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
	public void RotateDirection ( Vector3 dir, float speed, float height ) {
		//float dot = Vector3.Dot(dir, transform.right);
		//_posX += (dot * speed);

		//Because posY and posX represent the euler angles angles of rotation around the player, they can lerp towards euler angles obtained f
		Quaternion localDirection = Quaternion.LookRotation(dir, _PlayerTransformCopy.up) * _PlayerTransformCopy.rotation;
		Vector3 eulers = _PlayerTransformCopy.InverseTransformDirection(localDirection.eulerAngles);
		_posX = Mathf.MoveTowards(_posX, eulers.y, speed);

		//Y position will be acquired either from a designated height or part of the direction.
		if (_posY != height)
		{
			_posY = Mathf.Lerp(_posY, height, Time.deltaTime * 5);
		}
		else
		{
			_posY = Mathf.MoveTowards(_posY, eulers.x, speed);
		}
	}

	//Constnaly moves camera to behind player
	public void FollowDirection ( float speed, float height, float distance, float Yspeed, bool Skip = false ) {
		if (_isReversed)
		{
			FollowDirectionBehind(speed, height, distance, Yspeed, Skip);
		}

		else if (!_isLocked || Skip)
		{
			float dot = Vector3.Dot(_Skin.forward, transform.right);
			_posX += (dot * speed) * (Time.deltaTime * 100);
			// x = Mathf.MoveTowards(x, Skin.eulerAngles.y, (dot * speed) * (Time.deltaTime * 60));

			_posY = Mathf.Lerp(_posY, height, Time.deltaTime * Yspeed);
		}
	}

	public void FollowDirectionBehind ( float speed, float height, float distance, float Yspeed, bool Skip = false ) {
		if (true)
		{
			float dot = Vector3.Dot(-_Skin.forward, transform.right);
			_posX += (dot * speed) * (Time.deltaTime * 100);

			//Quaternion inverse = Quaternion.LookRotation(-Skin.forward, Skin.up);
			//x = Mathf.MoveTowards(x, inverse.eulerAngles.y, (dot * speed) * (Time.deltaTime * 60));

			_posY = Mathf.Lerp(_posY, height, Time.deltaTime * Yspeed);
		}
	}

	public void setBehind () {
		_posX = _Skin.eulerAngles.y;

	}

	public void setBehindWithHeight () {
		_posX = _Skin.eulerAngles.y;
		_posY = 14;
	}

	public void FollowHeightDirection ( float height, float speed ) {
		if (!_isLocked)
		{
			if (_Actions.whatAction != S_Enums.PlayerStates.Rail)
			{
				if (_PlayerPhys._isGrounded)
				{
					_posY = Mathf.Lerp(_posY, height, Time.deltaTime * speed);
				}
				else
				{
					_posY = Mathf.Lerp(_posY, height, Time.deltaTime * speed);
				}
			}
		}
	}

	public float ClampAngle ( float angle, float min, float max ) {
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;

		return Mathf.Clamp(angle, min, max);
	}

	public void lockCamFor ( float time ) {
		_isLocked = true;
		_lockTimer = -time;
	}

	//Set camera and overloads (function with the same name are different options)

	public void SetCamera ( Vector3 dir, float duration, float heightSet ) {

		_lookAtDir = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = heightSet;
		_lockedRotationSpeed_ = _initialLockedRotationSpeed * 0.01f;

	}
	public void SetCamera ( Vector3 dir, float duration, float heightSet, float speed ) {

		_lookAtDir = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = heightSet;
		_lockedRotationSpeed_ = speed;

	}
	public void SetCameraNoHeight ( Vector3 dir, float duration, float speed ) {
		_lookAtDir = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = 0;
		_lockedRotationSpeed_ = speed;
	}

	public void SetCamera ( Vector3 dir, float duration, float heightSet, float speed, float lagSet ) {

		_lookAtDir = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = heightSet;
		_lockedRotationSpeed_ = speed;
		_moveModifier = lagSet;

	}

	public void SetCameraNoLook ( float heightSet ) {
		_heightToLook = heightSet;
	}

	public void SetCamera ( Vector3 dir, bool instant ) {
		float dot = Vector3.Angle(dir, transform.forward);
		_posX += dot;

	}
	public void SetCamera ( float lagSet ) {

		_moveModifier = lagSet;

	}

	//Called by other scripts to make the the camera shake with force  for a time.
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
			yield return new WaitForFixedUpdate();
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
		_initialLockedRotationSpeed = _lockedRotationSpeed_;

		_startLockCam = _lockCamAtSpeed_;
		_shouldSetHeightWhenMoving_ = _Tools.camStats.LockHeight;
		_lockHeightSpeed_ = _Tools.camStats.LockHeightSpeed;
		_shouldFaceDownWhenInAir_ = _Tools.camStats.MoveHeightBasedOnSpeed;
		_minHeightToLookDown_ = _Tools.camStats.minHeightToLookDown;
		_heightToLock_ = _Tools.camStats.HeightToLock;
		_heightFollowSpeed_ = _Tools.camStats.HeightFollowSpeed;
		_fallSpeedThreshold_ = _Tools.camStats.FallSpeedThreshold;

		_cameraMaxDistance_ = _Tools.camStats.CameraMaxDistance;
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

		_moveLerpingSpeed_ = _Tools.camStats.MoveLerpingSpeed;
		_rotationLerpingSpeed_ = _Tools.camStats.RotationLerpingSpeed;

		_lockedRotationSpeed_ = _Tools.camStats.LockedRotationSpeed;
		_shakeDampen_ = _Tools.camStats.ShakeDampen;

		_CollidableLayers_ = _Tools.camStats.CollidableLayers;
	}
	#endregion


}




