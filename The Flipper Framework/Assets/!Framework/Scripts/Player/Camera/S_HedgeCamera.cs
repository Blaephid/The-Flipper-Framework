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
	[Header("Character Components")]
	public S_CharacterTools       _Tools;
	
	public S_PlayerPhysics        _PlayerPhys;
	public Transform              _Skin;
	public S_ActionManager        _Actions;
	public S_PlayerInput          _Input;

	[Header("Target Levels")]
	public Transform              _BaseTarget;
	public Transform               _FinalTarget;
	public Transform               _TargetByCollisions;
	public Transform               _TargetByInput;
	public Transform               _TargetByAngle;
	public Transform              _PlayerTransformCopy;


	[Header("Cameras")]
	public GameObject		_SecondaryCamera;
	public CinemachineBrain	_Brain;

	private Transform              _PlayerTransformReal;

	private CinemachineVirtualCamera	_VirtualCamera;
	private CinemachineFramingTransposer	_Transposer;
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
	private bool                  _shouldAffectDistanceBySpeed_;
	private Vector3               _angleThreshold_;

	private Vector3               _dampingBehind_;
	private Vector3               _dampingInFront_;

	private float                 _cameraVerticalRotationSpeed_ = 10;
	private AnimationCurve        _VerticalFollowSpeedByAngle_;

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
	private float                  _rotateToBehindSpeed_;
	private float                  _rotateCharacterBeforeCameraFollows_;
	private float                 _followFacingDirectionSpeed_;

	private float                 _inputPredictonDistance_;
	private float                 _cameraMoveToInputSpeed_;


	LayerMask                     _CollidableLayers_;
	private bool                  _shouldMoveInInputDirection_;
	private bool		_shouldMoveBasedOnAngle_ = true;
		
	private AnimationCurve	_moveUpByAngle_;
	private AnimationCurve        _moveSideByAngle_;
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
	[HideInInspector]
	public float                  _preY = 20.0f;

	private float                 _changeY;
	private float                 _changeX;

	Quaternion                    _lerpedRot;

	private float                 _moveModifier = 1;


	[HideInInspector]
	public bool		_isLocked;
	[HideInInspector]
	public bool                   _canMove;
	[HideInInspector]
	public bool                   _isMasterLocked;
	private float                 _heightToLook;
	[HideInInspector]
	public float                  _lookTimer;
	private float                 _backBehindTimer;
	private float                 _equalHeightTimer;
	[HideInInspector]
	public float                  _lockedRotationSpeed;
	private Vector3                _currentFaceDirection;
	private bool                  _isRotatingBehind;


	private float                 _lockTimer;
	[HideInInspector]
	public bool                   _isReversed;

	//Effects
	private Vector3               _lookAtDir;
	private bool		_shouldAlignToExternal;
	private Quaternion            _externalAlignment;

	private float                 _distanceModifier = 1;

	[HideInInspector]
	public float                  _invertedX;
	[HideInInspector]
	public float                  _invertedY;
	[HideInInspector]
	public float                  _sensiX;
	[HideInInspector]
	public float                  _sensiY;

	private Vector3               _hitNormal;

	private Vector3               _predictAheadPosition;
	private Vector3		_AngleOffset;
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

		SetTools();
		SetStats();

		//Deals with cursor 
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	//LateUpdate is called at the end of an update, and all camera controls are handled here.
	void LateUpdate () {

		HandleTargetPosition();
		AlignPlayerTransformCopy();
		HandleCameraMovement();	
		_isLocked = GetLocked();
		HandleCameraSituations();
		ApplyCameraEffects();

		ConfirmCameraChanges();
		ChangeBasedOnFacing();
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

		//Set to the change this frame can be tracked.
		_preY = _posY;
		_preX = _posX;

		//If LookTimer is currently below zero, then direct the camera towards the point of interest
		if (_lookTimer < 0 || _lookTimer == 1)
		{
			//Get new height or current height if it isn't to be changed.
			float heightToGo = _heightToLook != 0 ? _heightToLook : _posY;
			RotateDirection(_lookAtDir, _lockedRotationSpeed, heightToGo, true);

			//Count down timer to zero
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

	//Handles moving the target locally from the character.
	private void HandleTargetPosition() {
		_PlayerTransformCopy.position = _PlayerTransformReal.position; ;

		//Gets the players current input direction and moves the target in that direction.
		if (_shouldMoveInInputDirection_)
		{
			Vector3 inputDirection = _PlayerPhys.transform.TransformDirection(_PlayerPhys._moveInput);
			_predictAheadPosition = Vector3.MoveTowards(_predictAheadPosition, inputDirection * _inputPredictonDistance_, _cameraMoveToInputSpeed_ * Time.deltaTime);

		}

		//Moves the target slightly away from the character when looking left, right, up or down.
		if (_shouldMoveBasedOnAngle_)
		{
			//Get where to move target based on how up or down camera is currently looking.
			float y = _moveUpByAngle_.Evaluate(GetFaceDirection(transform.forward, false).y);

			//Get how far to move away from the character based on difference between camera and character direction, looping it back down if it goes inn front of the player.
			float angle = Vector3.Angle(GetFaceDirection(transform.forward), GetFaceDirection(_Skin.forward)) / 90;
			if (angle > 1) { angle = 2 - angle; }
			else if (angle < -1) { angle = -2 - angle; }

			//Get which direction (clockwise or counterclockwise) the camera has rotated from the character, and makes the angle negative or positive.
			Vector3 crossProduct = Vector3.Cross(new Vector3 (transform.forward.x, 0, transform.forward.z).normalized, _Skin.forward);
			crossProduct = _Skin.transform.InverseTransformDirection(crossProduct);
			angle *= Mathf.Sign(crossProduct.y);

			float xz = _moveSideByAngle_.Evaluate(angle);

			//Move the offset towards a place relative to the character.
			Vector3 goal = (_Skin.up * y) + (_Skin.right * xz);
			_AngleOffset = Vector3.Lerp(_AngleOffset, goal, 0.1f);
			_TargetByAngle.localPosition = _AngleOffset;
		}


		//The final target is the actual anchor point for the camera, and should not be changed, as it is a child of the other targets. 
		//_FinalTarget.localPosition = Vector3.zero;
		_TargetByCollisions.localPosition = Vector3.zero;

		Vector3 targetOffset = _FinalTarget.position - _BaseTarget.position;
		float targetOffsetDistance = Vector3.Distance(_FinalTarget.position , _BaseTarget.position) + 0.2f;
		//But to prevent the target going through surfaces (and by extent the camera) move it if there would be a collision closer to the centre.
		if (Physics.Raycast(_BaseTarget.position, targetOffset, out RaycastHit hit, targetOffsetDistance, _CollidableLayers_))
		{
			_TargetByCollisions.position = Vector3.LerpUnclamped(hit.point, _BaseTarget.position, 0.8f);
		}
		//If no wall to block the target, then apply the calculations.
		else
		{
			_TargetByInput.localPosition = _predictAheadPosition;
			_TargetByAngle.localPosition = _AngleOffset;
		}

	}

	//Handles the PlayerTransformCopy, making it match the actual player, or its own angle. This will later be used as a reference for the camera to rotate around.
	void AlignPlayerTransformCopy () {

		Quaternion newRot = _PlayerTransformCopy.rotation;
		Quaternion targetRot = Quaternion.identity;
		bool willLerp = false;

		//Sometimes the copy will be set externally in the setcamera functions, this avoids the atumomatic.
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
			//Lerp is used to make a smooth follow rather than a jittering.
			//But this means it can never reach the exact same rotation, so if the difference is small enough, then the slerp becomes 1.
			float slerpValue = Time.deltaTime * _cameraVerticalRotationSpeed_;
			float dif = Quaternion.Angle(newRot, targetRot) / 180;
			slerpValue = dif < 0.005f
				? 1 : slerpValue * _VerticalFollowSpeedByAngle_.Evaluate(dif);
			newRot = Quaternion.Lerp(newRot, targetRot, slerpValue);

			_PlayerTransformCopy.rotation = newRot;
		}
	}

	//Changes variables and elements to the camera movement depinding if the player is moving towards or away from it.
	void ChangeBasedOnFacing () {
		//Get player and camera direction without vertical since vertical damping is not used.
		Vector3 playerVelocity = _PlayerTransformCopy.InverseTransformDirection(_PlayerPhys._RB.velocity);
		playerVelocity.y = 0;
		Vector3 cameraDirectionWithoutY = _PlayerTransformCopy.InverseTransformDirection(transform.forward);
		cameraDirectionWithoutY.y = 0;

		float angle = Vector3.Angle(playerVelocity, cameraDirectionWithoutY);

		//If camera is facing same way as player, following behind.
		if(angle < 90)
		{
			_Transposer.m_XDamping = _dampingBehind_.x;
			_Transposer.m_ZDamping = _dampingBehind_.z;
			_Transposer.m_YDamping = _dampingBehind_.y;
		}
		//If player is facing the camera, and moving towards it.
		else
		{
			_Transposer.m_XDamping = _dampingInFront_.x;
			_Transposer.m_ZDamping = _dampingInFront_.z;
			_Transposer.m_YDamping = _dampingInFront_.y;
		}
	}

	//Sets the camera a distance away from the player, either by adjusting the cinemachine or placing manually.
	void HandleCameraDistance () {
		_distanceModifier = 1;

		//Pushes camera further away from character at higher speeds, allowing more control and sense of movement
		if(_shouldAffectDistanceBySpeed_)
		{
			float targetDistance = _cameraDistanceBySpeed_.Evaluate(_PlayerPhys._horizontalSpeedMagnitude / _PlayerPhys._currentMaxSpeed);
			_distanceModifier = Mathf.Lerp(_distanceModifier, targetDistance, 0.1f);
		}

		float dist = _cameraMaxDistance_ * _distanceModifier;

		//If the object has a virtual camera set to framing transposer, then that will handle placement on its own.
		if(_Transposer && _VirtualCamera.enabled)
		{
			_Transposer.m_CameraDistance = dist;
		}
		//If not, position is calculated by distance and if there is a wall in the way.
		else
		{
			//Check for a wall by moving from anchor towards camera.
			if (Physics.Raycast(_TargetByCollisions.position, -transform.forward, out RaycastHit hitWall, -dist, _CollidableLayers_))
			{
				dist = (-hitWall.distance);
				_hitNormal = hitWall.normal * 0.5f;
			}
			else
			{
				_hitNormal = Vector3.zero;
			}

			//Get position by moving from the camera anchor in a backwards direction relative to the overall rotation.
			var position = _lerpedRot * new Vector3(0, 0, dist + 0.3f);
			transform.position = _FinalTarget.position + position + _hitNormal;
		}

	}

	//See if camera should be locked and decrease counter if so.
	private bool GetLocked () {

		if (_isMasterLocked)
		{
			return true;
		}

		//Countdown lock timer
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
		bool isRightAction = _Actions._whatAction == S_Enums.PrimaryPlayerStates.Jump || _Actions._whatAction == S_Enums.PrimaryPlayerStates.Default || _Actions._whatAction == S_Enums.PrimaryPlayerStates.DropCharge;
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
		//If over a certain speed, then camera will start drifting to set height.
		else if (_shouldSetHeightWhenMoving_ && _PlayerPhys._horizontalSpeedMagnitude >= 10)
		{
			if (_equalHeightTimer < 0)
				_equalHeightTimer += Time.deltaTime;
			else
				ChangeHeight(_heightToLock_, _lockHeightSpeed_);
		}

		AutoRotateCamera();
	}

	//Handles when the camera will rotate to direction automatically, mainly rotating behind the character when running.
	private void AutoRotateCamera() {
		//Changing camera direction
		if (_backBehindTimer < 0)
			_backBehindTimer += Time.deltaTime;

		//Certain actions will have different requirements for the camera to move behind. The switch sets the requirements before the if statement checks against them.
		float minSpeed;
		bool skipDelay = false;
		switch (_Actions._whatAction)
		{
			default:
				minSpeed = _lockCamAtSpeed_;
				break;
			case S_Enums.PrimaryPlayerStates.WallRunning:
				minSpeed = 60;
				skipDelay = true;
				break;
			case S_Enums.PrimaryPlayerStates.Homing:
				minSpeed = 0;
				break;
		}

		float dif = Vector3.Angle(GetFaceDirection(transform.forward), _currentFaceDirection) / 180;

		//If moving fast enough, the delay from moving the camera has expired, and the player is facing a different enough angle to the camera, then it will move behind. MinSpeed at 0 means it won't happen.
		if (_PlayerPhys._horizontalSpeedMagnitude > minSpeed && minSpeed > 0 && ((_backBehindTimer >= 0 && (dif >= _rotateCharacterBeforeCameraFollows_ || _isRotatingBehind)) || skipDelay))
		{		
				GoBehindCharacter(_rotateToBehindSpeed_, 0, false);
		} 
		//_CurrentFaceDirection is used to add a delay to rotating before the camera starts following. It moves towards the player rotation, and GoBehindCharacter sets isRotatingBehind to true until rotation is completed, resetting the delay.
		if(!_isRotatingBehind) 
		{
			_currentFaceDirection = Vector3.RotateTowards(_currentFaceDirection, GetFaceDirection(_Skin.forward), Mathf.Deg2Rad * _followFacingDirectionSpeed_, 0);
		}
	}

	//Takes the changes to _posY and _posX, and applies the rotation and position of to the Camera.
	private void ConfirmCameraChanges () {

		//Applies the rotation and placement from the previous update. Having this applied the new data this frame causes jittering.
		transform.rotation = _lerpedRot;

		//Keeps horizontal and vertical positons always represented within 360 degrees
		_posX = ClampAngleX(_posX, -1, 361);
		_posY = ClampAngleY(_posY, _yMinLimit_, _yMaxLimit_);

		//Takes the x and y positions as euler angles around the player.
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

		dir = dir == Vector3.zero ? transform.forward : dir;

		//Get a rotation based off the look direction without compensating player's current up.	
		Vector3 newDirection = _PlayerTransformCopy.InverseTransformDirection(dir);
		Quaternion localDirection = Quaternion.LookRotation(newDirection, _PlayerTransformCopy.up);

		//Get local versions of the euler angles which are used for x and y cam positons.
		Vector3 eulers = localDirection.eulerAngles;
		int eulerY = (int)eulers.y;

		//Lerp can't compute looping where -1 is actually 359. This lies to it about having higher and lower values so it moves accurately. These are then back to within 360 later.
		float xSpeed = speed;
		if (_posX < 90 && eulerY > 270) { eulerY -= 360 ; }
		else if (_posX > 270 && eulerY < 90) { eulerY += 360; }
		_posX = Mathf.Lerp(_posX, eulerY, Time.deltaTime * xSpeed );

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


		//These are relevant to auto rotating behind player, when camera completes its rotation, rotation delay is reset.
		_isRotatingBehind = true;
		if (Vector3.Dot(GetFaceDirection(transform.forward), GetFaceDirection(_Skin.forward)) > 0.99f)
		{
			_isRotatingBehind = false;
			_currentFaceDirection = GetFaceDirection(_Skin.forward);
		}
	}

	//Called whenever a direction needs to be relevalnt to the base rotation the camera is based around
	public Vector3 GetFaceDirection(Vector3 dir, bool removeY = true) {
		dir = _PlayerTransformCopy.InverseTransformDirection(dir);
		if(removeY) dir.y = 0;
		dir = dir.normalized;
		return dir;
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
			switch(_Actions._whatAction)
			{
				case S_Enums.PrimaryPlayerStates.Rail:
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
		_lockedRotationSpeed = speed;
		_externalAlignment = target;
		_shouldAlignToExternal = align;

	}

	//Tell camera to look in direction but not change height seperately from the target.
	public void SetCameraNoHeight ( Vector3 dir, float duration, float speed, Quaternion target, bool align ) {
		_lookAtDir = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = 0;
		_lockedRotationSpeed = speed;
		_externalAlignment = target;
		_shouldAlignToExternal = align;
	}

	//Set camera to direction but with a change to movement input.
	public void SetCamera ( Vector3 dir, float duration, float heightSet, float speed, float lagSet ) {

		_lookAtDir = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = heightSet;
		_lockedRotationSpeed = speed;
		_moveModifier = lagSet;

	}

	//Only changes height
	public void SetCameraHeightOnly ( float heightSet, float speed, float duration = 1 ) {
		_lookTimer = duration > 0 ? -duration : 1;
		_lockedRotationSpeed = speed;
		_heightToLook = heightSet;
	}

	//
	//Camera Effects
	//

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

	//Called externally and temporarily creates activates the second camera at the position of the main one, before transitioning back to the primary.
	//The x value is the frames fully stationary, and the y is how long it takes to catch up again.
	public IEnumerator ApplyCameraPause (Vector2 frames) {

		if(_SecondaryCamera.active) { yield return null; } //If secondary camera is already active, don't move it, let it play out.

		//Sets the secondary camera to the position of the primary, then makes it take over display.
		_SecondaryCamera.transform.position = transform.position;
		_SecondaryCamera.transform.rotation = transform.rotation;
		_SecondaryCamera.SetActive(true);

		//Remain locked in place for x frames.
		for (int i = 0 ; i < frames.x; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		//This will tell the cinemachine brain to make the transition from secondary to hedgecamera take this many frames (converted to seconds) in this way.
		_Brain.m_DefaultBlend.m_Time = 55 / frames.y;
		_Brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;

		_SecondaryCamera.SetActive(false); //Disabling the secondary camera will cause the brain to automatically transition back to primary (assuming no other virtual cameras are at play.
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	private void SetTools () {

		_VirtualCamera = GetComponent<CinemachineVirtualCamera>();
		_PlayerTransformReal = _PlayerPhys.transform;
		_PlayerTransformCopy.rotation = _PlayerTransformReal.rotation;
		_currentFaceDirection = GetFaceDirection(_Skin.forward);
		_Transposer = _VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

		_VirtualCamera.Follow = _FinalTarget;
		_SecondaryCamera.SetActive(false);
	}

	void SetStats () {
		_isLocked = false;
		_canMove = true;

		_shouldSetHeightWhenMoving_ = _Tools.CameraStats.LockHeightStats.LockHeight;
		_lockHeightSpeed_ = _Tools.CameraStats.LockHeightStats.LockHeightSpeed;
		_shouldFaceDownWhenInAir_ = _Tools.CameraStats.AutoLookDownStats.shouldLookDownWhenInAir;
		_minHeightToLookDown_ = _Tools.CameraStats.AutoLookDownStats.minHeightToLookDown;
		_heightToLock_ = _Tools.CameraStats.LockHeightStats.HeightToLock;
		_heightFollowSpeed_ = _Tools.CameraStats.AutoLookDownStats.HeightFollowSpeed;
		_fallSpeedThreshold_ = _Tools.CameraStats.AutoLookDownStats.FallSpeedThreshold;

		_cameraMaxDistance_ = _Tools.CameraStats.DistanceStats.CameraMaxDistance;
		_cameraDistanceBySpeed_ = _Tools.CameraStats.DistanceStats.cameraDistanceBySpeed;
		_shouldAffectDistanceBySpeed_ = _Tools.CameraStats.DistanceStats.affectDistancebySpeed;
		_VirtualCamera.GetComponent<CinemachineCollider>().m_CollideAgainst = _Tools.CameraStats.DistanceStats.CollidableLayers;
		_CollidableLayers_ = _Tools.CameraStats.DistanceStats.CollidableLayers;

		_dampingBehind_ = _Tools.CameraStats.cinemachineStats.dampingBehind;
		_dampingInFront_ = _Tools.CameraStats.cinemachineStats.dampingInFront;
		_Transposer.m_SoftZoneHeight = _Tools.CameraStats.cinemachineStats.softZone.y;
		_Transposer.m_SoftZoneWidth = _Tools.CameraStats.cinemachineStats.softZone.x;
		_Transposer.m_DeadZoneHeight = _Tools.CameraStats.cinemachineStats.deadZone.y;
		_Transposer.m_DeadZoneWidth = _Tools.CameraStats.cinemachineStats.deadZone.x;

		_angleThreshold_.x = _Tools.CameraStats.AligningStats.angleThresholdUpwards;
		_angleThreshold_.y = _Tools.CameraStats.AligningStats.angleThresholdDownwards;
		_cameraVerticalRotationSpeed_ = _Tools.CameraStats.AligningStats.CameraVerticalRotationSpeed;
		_VerticalFollowSpeedByAngle_ = _Tools.CameraStats.AligningStats.vertFollowSpeedByAngle;

		_inputXSpeed_ = _Tools.CameraStats.InputStats.InputXSpeed;
		_inputYSpeed_ = _Tools.CameraStats.InputStats.InputYSpeed;
		_stationaryCamIncrease_ = _Tools.CameraStats.InputStats.stationaryCamIncrease;

		_afterMoveXDelay_ = _Tools.CameraStats.RotateBehindStats.afterMoveXDelay;
		_afterMoveYDelay_ = _Tools.CameraStats.LockHeightStats.afterMoveYDelay;

		_yMinLimit_ = _Tools.CameraStats.ClampingStats.yMinLimit;
		_yMaxLimit_ = _Tools.CameraStats.ClampingStats.yMaxLimit;

		_lockCamAtSpeed_ = _Tools.CameraStats.RotateBehindStats.LockCamAtHighSpeed;
		_rotateToBehindSpeed_ = _Tools.CameraStats.RotateBehindStats.rotateToBehindSpeed;
		_rotateCharacterBeforeCameraFollows_ = _Tools.CameraStats.RotateBehindStats.rotateCharacterBeforeCameraFollows;
		_followFacingDirectionSpeed_ = _Tools.CameraStats.RotateBehindStats.followFacingDirectionSpeed;

	         _shakeDampen_ = _Tools.CameraStats.EffectsStats.ShakeDampen;

		_inputPredictonDistance_ = _Tools.CameraStats.LookAheadStats.inputPredictonDistance;
		_cameraMoveToInputSpeed_ = _Tools.CameraStats.LookAheadStats.cameraMoveToInputSpeed;
		_shouldMoveInInputDirection_ = _Tools.CameraStats.LookAheadStats.shouldMoveInInputDirection;

		_shouldMoveBasedOnAngle_ = _Tools.CameraStats.TargetByAngleStats.shouldMoveBasedOnAngle;
		_moveUpByAngle_ = _Tools.CameraStats.TargetByAngleStats.moveUpByAngle;
		_moveSideByAngle_ = _Tools.CameraStats.TargetByAngleStats.moveSideByAngle;

		_startLockCam = _lockCamAtSpeed_;
	}
	#endregion
}