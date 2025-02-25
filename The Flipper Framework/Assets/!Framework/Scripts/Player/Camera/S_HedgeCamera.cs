using UnityEngine;
using System.Collections;
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
	private S_PlayerVelocity       _PlayerVel;
	private S_PlayerMovement            _PlayerMovement;
	private S_ActionManager        _Actions;
	public S_PlayerInput          _Input;

	[Header("Objects")]
	public Transform              _Skin;
	public Transform              _PlayerMainBody;
	[Header("Target Levels")]
	public Transform              _BaseTarget;
	public Transform               _FinalTarget;
	public Transform              _TemporaryCameraTarget;
	public Transform               _TargetByCollisions;
	public Transform               _TargetByInput;
	public Transform               _TargetByAngle;
	public Transform              _PlayerTransformCopy;

	//Handling targets being reparented
	private Vector3                     _BaseTargetStart;
	private Transform       _CurrentReparentedTarget;
	private Transform                   _movedTargetsPreviousParent;

	[Header("Cameras")]

	public CinemachineVirtualCamera         _SecondaryCamera;
	private CinemachineBrain                        _MainCameraBrain;

	private Transform              _PlayerTransformReal;

	private CinemachineVirtualCamera                  _VirtualCamera;
	private CinemachineBasicMultiChannelPerlin        _Noise;
	private CinemachineFramingTransposer              _Transposer;
	#endregion


	//Stats - See camera stats scriptable objects for details
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

	private bool                  _shouldAffectFOVBySpeed_;
	[HideInInspector]
	public float                 _baseFOV_;
	private AnimationCurve        _cameraFOVBySpeed_;

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
	private bool                  _shouldMoveBasedOnAngle_ = true;

	private AnimationCurve        _moveUpByAngle_;
	private AnimationCurve        _moveSideByAngle_;
	#endregion

	// Trackers
	#region trackers

	[HideInInspector]
	public float                  _startLockCam;
	private bool                  _isFacingDown = false;

	private float                 _xPositionOfCamera = 0.0f;
	private float                 _xPrevious = 0.0f;

	[HideInInspector]
	public float                  _yPositionOfCamera = 20.0f;
	[HideInInspector]
	public float                  _yPrevious = 20.0f;

	private float                 _changeY;
	private float                 _changeX;

	Quaternion                    _lerpedRot;

	private float                 _moveModifier = 1;


	[HideInInspector]
	public bool                   _isLocked;
	[HideInInspector]
	public bool             _isYLocked;
	[HideInInspector]
	public bool             _isXLocked;
	[HideInInspector]
	public bool                   _isMasterLocked;
	private float                 _heightToLook;
	[HideInInspector]
	public float                  _lookTimer;
	private float                 _backBehindTimer;
	private float                 _equalHeightTimer;

	public float _currentDistance { get; private set; }
	public float _currentFOV { get; private set; }

	[HideInInspector]
	public float                  _lockedRotationSpeed;
	private Vector3                _currentFaceDirection;
	private bool                  _isRotatingBehind;


	[HideInInspector]
	public bool                         _canAffectDistanceBySpeed = true;
	public bool                         _canAffectFOVBySpeed = true;

	private float                 _lockTimer;

	//Effects
	private Vector3               _lookAtDirection; //A direction in world space the camera will aim to look towards. Usually set by Camera Triggers
	private Transform              _lookAtLockOn;  //If not null, will update the above every frame to be the direction of this object.
	private bool                  _willChangeHeight;
	private bool                  _isPlayerTransformCopyControlledExternally;

	private bool                        _rotateToLocalDirection;
	private Vector3                     _localLookAtDirection;
	private Vector3                      _previousTranslatedLocalLookAtDirection;
	private bool                     _isCurrentlyRotationTowardsLocalDirection = true;

	[HideInInspector]
	public float                 _distanceModifier = 1;
	[HideInInspector]
	public float                 _FOVModifier = 1;

	[HideInInspector]
	public float                  _invertedX = 1;
	[HideInInspector]
	public float                  _invertedY = 1;
	[HideInInspector]
	public float                  _sensiX;
	[HideInInspector]
	public float                  _sensiY;

	private Vector3               _hitNormal;

	private Vector3               _predictAheadPosition;
	private Vector3               _AngleOffset;

	[HideInInspector]
	public bool             _cameraPausedLocked;
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

		_BaseTargetStart = _BaseTarget.transform.position - _Skin.position;

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
		_yPrevious = _yPositionOfCamera;
		_xPrevious = _xPositionOfCamera;

		//If LookTimer is currently below zero, then direct the camera towards the point of interest
		if (_lookTimer < 0 || _lookTimer == 1)
		{
			if(_lookAtLockOn) 
			{ _lookAtDirection = (_lookAtLockOn.position - _Skin.position).normalized; }

			RotateDirection(_lookAtDirection, _lockedRotationSpeed, _heightToLook, _willChangeHeight);

			//Count down timer to zero
			if (_lookTimer < 0)
			{
				_lookTimer = Mathf.Clamp(_lookTimer + Time.deltaTime, _lookTimer, 0);
				if (_lookTimer == 0)
				{
					_isPlayerTransformCopyControlledExternally = false;
				}
			}
		}

		//Changes the x and y values of the camera by input, changing speed based on a number of factors, such as if stationary and external modifiers.

		float camMoveSpeed = _PlayerVel._speedMagnitudeSquared > 100 ?     Time.deltaTime :    Time.deltaTime * _stationaryCamIncrease_;
		camMoveSpeed *= _moveModifier;
		float movementX = _Input.moveCamX * _inputXSpeed_ * _invertedX;
		float movementY = _Input.moveCamY * _inputYSpeed_ * _invertedY;
		_moveModifier = Mathf.MoveTowards(_moveModifier, 1, Time.deltaTime);

		if (!_isXLocked)
			_xPositionOfCamera += movementX * camMoveSpeed;
		if (!_isYLocked)
			_yPositionOfCamera -= movementY * camMoveSpeed;


		//Pres were assigned in the movement script and this is used to see how much was changed this frame. 
		_changeY = Mathf.Abs(_yPrevious - _yPositionOfCamera);
		_changeX = Mathf.Abs(_xPrevious - _xPositionOfCamera);

		//If the camera was changed, then ready the delay before automoving.
		if (_changeY > 0.5f || _changeX > 0.5f)
		{
			_backBehindTimer = -_afterMoveXDelay_;
			_equalHeightTimer = -_afterMoveYDelay_;
		}
	}

	//Handles moving the target locally from the character.
	private void HandleTargetPosition () {
		_PlayerTransformCopy.position = _PlayerTransformReal.position; ;

		//Gets the players current input direction and moves the target in that direction.
		if (_shouldMoveInInputDirection_)
		{
			Vector3 inputDirection = _PlayerVel._horizontalSpeedMagnitude > 10 ? _Input._constantInputRelevantToCharacter : Vector3.zero;
			_predictAheadPosition = Vector3.MoveTowards(_predictAheadPosition, inputDirection * _inputPredictonDistance_, _cameraMoveToInputSpeed_ * Time.deltaTime);
		}

		//Moves the target slightly away from the character when looking left, right, up or down.
		if (_shouldMoveBasedOnAngle_)
		{
			//Get where to move target based on how up or down camera is currently looking.
			float y = _moveUpByAngle_.Evaluate(GetFaceDirection(transform.forward, false).y);

			//Get how far to move away from the character based on difference between camera and character direction,
			//looping it back down if it goes in front of the player.
			float angle = Vector3.Angle(GetFaceDirection(transform.forward), GetFaceDirection(_Skin.forward)) / 90;
			if (angle > 1) { angle = 2 - angle; }
			else if (angle < -1) { angle = -2 - angle; }

			//Get which direction (clockwise or counterclockwise) the camera has rotated from the character,
			//and makes the angle negative or positive.
			Vector3 crossProduct = Vector3.Cross(new Vector3 (transform.forward.x, 0, transform.forward.z).normalized, _Skin.forward);
			crossProduct = _Skin.transform.InverseTransformDirection(crossProduct);
			angle *= Mathf.Sign(crossProduct.y);

			float xz = _moveSideByAngle_.Evaluate(angle);

			//Move the offset towards a place relative to the character.
			Vector3 goal = (_Skin.up * y) + (_Skin.right * xz);
			_AngleOffset = Vector3.Lerp(_AngleOffset, goal, 0.1f);
		}

		//Apply, ensuring matches player rotation.
		_TargetByInput.localPosition = _PlayerPhys.GetRelevantVector(_predictAheadPosition);
		_TargetByAngle.localPosition = Vector3.zero;
		_TargetByAngle.position += _AngleOffset;

		//To ensure distance is only slighlty beyond offset, use a linecast to avoid getting the actual distance value.
		Vector3 targetOffset = _TargetByCollisions.parent.position - _BaseTarget.position;
		targetOffset += targetOffset.normalized * 0.2f;

		//But to prevent the target going through surfaces (and by extent the camera) move it if there would be a collision closer to the centre.
		if (Physics.Linecast(_BaseTarget.position, _BaseTarget.position + targetOffset, out RaycastHit hit, _CollidableLayers_))
		{
			//This is the lowest in the hierachy, so move world position, not local.
			_TargetByCollisions.position = hit.point + hit.normal * 0.3f;
		}
		else
		{
			bool isCollisionTargetCloserThanParent =
			(_BaseTarget.position - _TargetByCollisions.position).sqrMagnitude < (_BaseTarget.position - _TargetByCollisions.parent.position).sqrMagnitude;
			if (isCollisionTargetCloserThanParent)
				_TargetByCollisions.localPosition = Vector3.Lerp(_TargetByCollisions.localPosition, Vector3.zero, 0.2f);
		}

		//The final target is the actual anchor point for the camera, and should not be changed, as it is a child of the other targets. 
	}

	//Handles the PlayerTransformCopy, making it match the actual player, or its own angle. This will later be used as a reference for the camera to rotate around.
	void AlignPlayerTransformCopy () {
		if (_isPlayerTransformCopyControlledExternally) { return; }
		Quaternion newRot = _PlayerTransformCopy.rotation;
		Quaternion targetRot = Quaternion.identity;
		bool willLerp = false;

		//If the player is currently on a steeper angle than the set negative angle (E.G. on a horizontal wall), then make the transform copy relative to this ground as well.
		//Once the threshold is acceeded, then keep doing this until copy returns to normal.
		if (_PlayerTransformReal.up.y < _angleThreshold_.x || _PlayerTransformCopy.up.y < 1f)
		{
			//If exited angle limit, new target is facing up again, otherwise, only aim to change up direction
			if (_PlayerTransformReal.up.y > _angleThreshold_.y)
				targetRot = Quaternion.FromToRotation(_PlayerTransformCopy.up, Vector3.up) * _PlayerTransformCopy.rotation;
			else
				targetRot = Quaternion.FromToRotation(_PlayerTransformCopy.up, _PlayerTransformReal.up) * _PlayerTransformCopy.rotation;

			willLerp = true;
		}

		if (willLerp)
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

	//Changes variables and elements to the camera movement depending if the player is moving towards or away from it.
	void ChangeBasedOnFacing () {
		//Get player and camera direction without vertical since vertical damping is not used.
		Vector3 playerVelocity = _PlayerTransformCopy.InverseTransformDirection(_PlayerVel._worldVelocity);
		playerVelocity.y = 0;
		Vector3 cameraDirectionWithoutY = _PlayerTransformCopy.InverseTransformDirection(transform.forward);
		cameraDirectionWithoutY.y = 0;

		float angle = Vector3.Angle(playerVelocity, cameraDirectionWithoutY);

		//If camera is facing same way as player, following behind.
		if (angle < 90)
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

		//Set up variables to limit the view changes.
		Vector3 actionModifier = GetDistanceModifiedByAction();
		float minValue = actionModifier.x;
		float maxValue = actionModifier.z;
		float speedPercentage = Mathf.Clamp((_PlayerVel._currentRunningSpeed / _PlayerMovement._currentMaxSpeed) * actionModifier.y, minValue, maxValue);

		//Pushes camera further away from character at higher speeds, allowing more control and sense of movement
		if (_shouldAffectDistanceBySpeed_)
		{
			float targetDistanceModi = _cameraDistanceBySpeed_.Evaluate(speedPercentage);
			_distanceModifier = Mathf.Lerp(_distanceModifier, targetDistanceModi, 0.1f);
		}

		//To make the player feel faster than they are, changes camera Field Of View based on speed.
		if (_shouldAffectFOVBySpeed_)
		{
			float targetFOVModi = _cameraFOVBySpeed_.Evaluate(speedPercentage);
			_FOVModifier = Mathf.Lerp(_FOVModifier, targetFOVModi, 0.2f);
		}

		//If the _can values are false, modifiers will still be calculated, just not applied. This ensures smooth transitions when toggling these.
		float useDistanceModifier = _canAffectDistanceBySpeed ? _distanceModifier : 1;
		float useFOVModifier = _canAffectFOVBySpeed ? _FOVModifier : 1;

		float dist = _cameraMaxDistance_ * useDistanceModifier;
		_VirtualCamera.m_Lens.FieldOfView = _baseFOV_ * useFOVModifier;
		_SecondaryCamera.m_Lens.FieldOfView = _baseFOV_ * useFOVModifier;
		_currentFOV = _baseFOV_ * useFOVModifier;

		//If the object has a virtual camera set to framing transposer, then that will handle placement on its own.
		if (_Transposer && _VirtualCamera.enabled)
		{
			_Transposer.m_CameraDistance = dist;
			_currentDistance = S_S_MoreMathMethods.GetDistanceOfVectors(transform.position, _FinalTarget.position);
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

			_currentDistance = dist;
		}

	}

	//The distance and FOV the camera changes can depend on the action (where some require greater zoom out). This returns the modifier, and min and max values.
	private Vector3 GetDistanceModifiedByAction () {
		switch (_Actions._whatCurrentAction)
		{
			default:
				return new Vector3(0, 1, 1);
			case S_S_ActionHandling.PrimaryPlayerStates.WallClimbing:
				return new Vector3(0.6f, 1.3f, 1);
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
		AutoRotateCamera();

		if (_isLocked) { return; }

		//Changing camera height
		float verticalSpeed = _PlayerTransformReal.InverseTransformDirection(_PlayerPhys._RB.velocity).y;

		//Making the camera face down when in the air for long enough.
		bool isRightAction = _Actions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.Jump || _Actions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.Default || _Actions._whatCurrentAction == S_S_ActionHandling.PrimaryPlayerStates.DropCharge;

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
			StartCoroutine(KeepGoingToHeightForFrames(10, 10, 200));
			//StartCoroutine(MoveFromDowntoForward(60));
		}
		//If over a certain speed, then camera will start drifting to set height.
		else if (_shouldSetHeightWhenMoving_ && _PlayerVel._horizontalSpeedMagnitude >= 10)
		{
			if (_equalHeightTimer < 0)
				_equalHeightTimer += Time.deltaTime;
			else
				ChangeHeight(_heightToLock_, _lockHeightSpeed_);
		}

	}

	//Handles when the camera will rotate to direction automatically, mainly rotating behind the character when running.
	private void AutoRotateCamera () {

		HandleRotatingBehindCharacter();
		HandleSetLocalRotation();

		return;
		void HandleRotatingBehindCharacter () {
			//Changing camera direction
			if (_backBehindTimer < 0)
				_backBehindTimer += Time.deltaTime;

			//Certain actions will have different requirements for the camera to move behind. The switch sets the requirements before the if statement checks against them.
			float minSpeed;
			bool skipDelay = false;
			switch (_Actions._whatCurrentAction)
			{
				default:
					minSpeed = _lockCamAtSpeed_;
					break;
				case S_S_ActionHandling.PrimaryPlayerStates.WallRunning:
					minSpeed = 60;
					skipDelay = true;
					break;
				case S_S_ActionHandling.PrimaryPlayerStates.Homing:
					minSpeed = 0;
					break;
			}

			float dif = Vector3.Angle(GetFaceDirection(transform.forward), _currentFaceDirection) / 180;

			//If moving fast enough, the delay from moving the camera has expired, and the player is facing a different enough angle to the camera, then it will move behind. MinSpeed at 0 means it won't happen.
			if (_PlayerVel._horizontalSpeedMagnitude > minSpeed && minSpeed > 0 && ((_backBehindTimer >= 0 && (dif >= _rotateCharacterBeforeCameraFollows_ || _isRotatingBehind)) || skipDelay))
			{
				GoBehindCharacter(_rotateToBehindSpeed_, 0, false);
			}
			//_CurrentFaceDirection is used to add a delay to rotating before the camera starts following. It moves towards the player rotation, and GoBehindCharacter sets isRotatingBehind to true until rotation is completed, resetting the delay.
			if (!_isRotatingBehind)
			{
				_currentFaceDirection = Vector3.RotateTowards(_currentFaceDirection, GetFaceDirection(_Skin.forward), Mathf.Deg2Rad * _followFacingDirectionSpeed_, 0);
			}
		}

		//This is set externally, and every frame will rotate towards the intended local rotation. This means rather than looking at a world space direction, the camera stays at the same point relative to the 
		//characters rotation. Similar to rotating behind the character, but for any angle, not just directly behind.
		void HandleSetLocalRotation () {
			if (_isRotatingBehind || !_rotateToLocalDirection) { return; }

			Vector3 directionInWorldSpace = _Skin.transform.rotation * _localLookAtDirection;
			float turnThisFrame = Vector3.Angle(directionInWorldSpace, _previousTranslatedLocalLookAtDirection);

			//To prevent the camera doing a huge turn if the character justs turns around. If they start facing the other way, change the location to look at to match on the other side
			if (_previousTranslatedLocalLookAtDirection != Vector3.zero && turnThisFrame > 26 && _isCurrentlyRotationTowardsLocalDirection)
			{
				_localLookAtDirection *= -1;
				directionInWorldSpace *= -1;
				_isCurrentlyRotationTowardsLocalDirection = false;
			}
			//After the player slows their turn, return to rotating to the new direction.
			else if (!_isCurrentlyRotationTowardsLocalDirection && turnThisFrame < 0.03f)
			{
				_isCurrentlyRotationTowardsLocalDirection = true;
			}

			_previousTranslatedLocalLookAtDirection = directionInWorldSpace;

			if (turnThisFrame < 25 && _isCurrentlyRotationTowardsLocalDirection)
				RotateDirection(directionInWorldSpace, 6, 0, false);
		}
	}

	//Takes the changes to _posY and _posX, and applies the rotation and position of to the Camera.
	private void ConfirmCameraChanges () {

		//Applies the rotation and placement from the previous update. Having this applied the new data this frame causes jittering.
		transform.rotation = _lerpedRot;

		//Keeps horizontal and vertical positons always represented within 360 degrees
		_xPositionOfCamera = ClampAngleX(_xPositionOfCamera, -1, 361);
		_yPositionOfCamera = ClampAngleY(_yPositionOfCamera, _yMinLimit_, _yMaxLimit_);

		//Takes the x and y positions as euler angles around the player.
		_lerpedRot = Quaternion.Euler(_yPositionOfCamera, _xPositionOfCamera, 0);
		_lerpedRot = _PlayerTransformCopy.rotation * _lerpedRot;
	}

	private void ApplyCameraEffects () {

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

	//Causes the camera to turn around to face a designated vector direction in world space.
	private void RotateDirection ( Vector3 dir, float speed, float height, bool changeHeight ) {

		dir = dir == Vector3.zero ? transform.forward : dir;

		//Get a rotation based off the look direction without compensating player's current up.	
		Vector3 newDirection = _PlayerTransformCopy.InverseTransformDirection(dir);
		Quaternion localDirection = Quaternion.LookRotation(newDirection, _PlayerTransformCopy.up);

		Debug.DrawRay(_FinalTarget.position, newDirection * 2, Color.black);

		//Get local versions of the euler angles which are used for x and y cam positons.
		Vector3 eulers = localDirection.eulerAngles;
		int eulerY = (int)eulers.y;

		//Lerp can't compute looping where -1 is actually 359. This lies to it about having higher and lower values so it moves accurately. These are then back to within 360 later.
		float xSpeed = speed;

		if (_xPositionOfCamera - eulerY < -180) { eulerY -= 360; }
		else if (eulerY - _xPositionOfCamera < -180) { eulerY += 360; }

		xSpeed = xSpeed < 0 ? 1 : xSpeed * Time.deltaTime;

		_xPositionOfCamera = Mathf.Lerp(_xPositionOfCamera, eulerY, xSpeed);
		//True either if hieght is set manually, or told to include vertical in look direction.
		if (changeHeight)
		{
			//Get y height to look accurately in set direction
			float yTarget = eulers.x;
			if (yTarget > 180) { yTarget -= 360; }
			if (yTarget < -180) { yTarget += 360; }

			//If height was not preset, then use this.
			height = height != 0 ? height : yTarget;

			float ySpeed = speed < 0 ? 10000 : speed;
			_yPositionOfCamera = Mathf.MoveTowards(_yPositionOfCamera, height, ySpeed);
		}
	}
	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	public IEnumerator KeepGoingBehindCharacterForFrames ( int frames, float speed, float height, bool overwrite ) {
		for (int i = 0 ; i < frames ; i++)
		{
			GoBehindCharacter(speed, height, overwrite);
			yield return new WaitForFixedUpdate();
		}
	}

	public void SetToStickToLocalRotation ( bool set, Vector3 rotationInWorld ) {
		_rotateToLocalDirection = set;
		_isCurrentlyRotationTowardsLocalDirection = set;
		if (!set) { return; }

		_localLookAtDirection = _Skin.transform.InverseTransformDirection(rotationInWorld);
		_previousTranslatedLocalLookAtDirection = _Skin.rotation * _localLookAtDirection;
		_lookTimer = 0;

	}

	//Called by other scripts to make the camera face the direction the character is facing.
	public void GoBehindCharacter ( float speed, float height, bool evenIfLocked = false ) {
		bool changeHeight = height != 0;


		if (!_isLocked || evenIfLocked)
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
	public Vector3 GetFaceDirection ( Vector3 dir, bool removeY = true ) {
		dir = _PlayerTransformCopy.InverseTransformDirection(dir);
		if (removeY) dir.y = 0;
		dir = dir.normalized;
		return dir;
	}

	//Called by other scripts to immediately set the camera to behind the character.
	public void SetBehind ( int height ) {
		bool changeHeight = height != 0;
		RotateDirection(_Skin.forward, -1, 14, changeHeight);
	}

	public IEnumerator KeepGoingToHeightForFrames ( int frames, float height, float speed ) {
		for (int i = 0 ; i < frames ; i++)
		{
			ChangeHeight(height, speed);
			yield return new WaitForFixedUpdate();
		}
	}

	//Changes only the height of the camera to look up or down.
	public void ChangeHeight ( float height, float speed ) {
		if (!_isLocked)
		{
			//A switch is used so it's less clutured than an if statement.
			switch (_Actions._whatCurrentAction)
			{
				case S_S_ActionHandling.PrimaryPlayerStates.Rail:
					break;
				default:
				{
					_yPositionOfCamera = Mathf.MoveTowards(_yPositionOfCamera, height, Time.deltaTime * speed);
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
	public void SetCameraWithSeperateHeight ( Vector3 dir, float duration, float heightSet, float speed, Vector3 externalAlignment, Transform TargetLockOn = null ) {

		_lookAtDirection = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = heightSet;
		_lockedRotationSpeed = speed;
		_willChangeHeight = true;
		_lookAtLockOn = TargetLockOn;

		AlignPlayerTransformExternally(externalAlignment);
	}

	//Tell camera to look in direction but not change height seperately from the target.
	public void SetCameraNoSeperateHeight ( Vector3 dir, float duration, float speed, Vector3 externalAlignment, bool directionIncludesHeight, Transform TargetLockOn = null ) {
		_lookAtDirection = dir;
		_lookTimer = duration > 0 ? -duration : 1;
		_heightToLook = 0;
		_lockedRotationSpeed = speed;
		_willChangeHeight = directionIncludesHeight;
		_lookAtLockOn = TargetLockOn;

		AlignPlayerTransformExternally(externalAlignment);
	}

	void AlignPlayerTransformExternally ( Vector3 externalAlignmentUp ) {
		if (externalAlignmentUp != Vector3.zero)
		{
			_PlayerTransformCopy.rotation = Quaternion.LookRotation(_PlayerTransformCopy.forward, externalAlignmentUp);
			_isPlayerTransformCopyControlledExternally = true;
		}
	}

	//Only changes height
	public void SetCameraHeightOnly ( float heightSet, float speed, float duration = 1 ) {
		_lookTimer = duration > 0 ? -duration : 1;
		_lockedRotationSpeed = speed;
		_heightToLook = heightSet;
		_willChangeHeight = true;
	}

	//
	//Camera Effects
	//

	//Called by other scripts to make the camera shake with force  for a time. This is done through the built in noise feature of CInemachine Virtual Cameras.
	public IEnumerator ApplyCameraShake ( float shakeForce, int frames ) {
		_Noise.m_AmplitudeGain = shakeForce / _shakeDampen_;
		_Noise.m_FrequencyGain = 10;
		_SecondaryCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = _Noise.m_FrequencyGain;
		float segments = _Noise.m_AmplitudeGain / frames;

		//This will repeat until noise has reached 0 again.
		for (int i = 0 ; _Noise.m_AmplitudeGain > 0 ; i++)
		{
			yield return new WaitForFixedUpdate();

			//Once through 50% of the shake time, start slowing it down until it reaches 0.
			if (i > frames * 0.5f)
			{
				_Noise.m_AmplitudeGain = Mathf.Max(_Noise.m_AmplitudeGain - (segments / 0.5f), 0);
			}

			//If the secondary camera is in affect, ensure it has the same shake. (An exmaple of this situation is when launching a spin charge.)
			_SecondaryCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = _Noise.m_AmplitudeGain;
		}
	}

	//Called externally and temporarily creates activates the second camera at the position of the main one, before transitioning back to the primary.
	//The x value is the frames fully stationary, and the y is how long it takes to catch up again.
	public IEnumerator ApplyCameraPause ( Vector2 frames, Vector2 speedBeforeAndAfter, float minDifference = 0 ) {
		if(_cameraPausedLocked) { yield break; }

		if (_SecondaryCamera.gameObject.activeSelf) { yield break; } //If secondary camera is already active, don't move it, let it play out.

		//If the caller has input a speed the player is suddenly moving at, affect the lerp time by the speed difference.
		if (speedBeforeAndAfter.y > 0 && speedBeforeAndAfter.y >= speedBeforeAndAfter.x)
		{
			//Get a percentage difference as 0->1+
			float speedDifference = speedBeforeAndAfter.y - speedBeforeAndAfter.x;
			speedDifference = speedDifference / speedBeforeAndAfter.x;
			speedDifference = Mathf.Lerp(minDifference, 1, speedDifference);

			//The smaller the difference in speed, the less time the lerp from camera to camera will take.
			frames *= speedDifference;
		}

		//This will tell the cinemachine brain to make the transition from secondary to hedgecamera take this many frames (converted to seconds) in this way.
		_MainCameraBrain.m_DefaultBlend.m_Time = frames.y / 55; //Convert to seconds

		//Sets the secondary camera to the position of the primary, then makes it take over display.
		_SecondaryCamera.transform.position = transform.position;
		_SecondaryCamera.transform.rotation = transform.rotation;
		_SecondaryCamera.gameObject.SetActive(true);

		//Remain locked in place for x frames. At least 1
		for (int i = 1 ; i <= Mathf.Max(frames.x, 2) ; i++)
		{
			yield return new WaitForFixedUpdate();
		}

		_SecondaryCamera.gameObject.SetActive(false); //Disabling the secondary camera will cause the brain to automatically transition back to primary (assuming no other virtual cameras are at play.
	}

	//Called to make the camera target at a new position, attached to the secondary target, which is set to follow another transform (usually the player or the player skin).
	public void SetCameraTargetToNewParent ( Transform TargetToMove, Transform newParent, Vector3 position, int frames ) {
		if(!(position.sqrMagnitude >= 0)) { return; }
		if (_CurrentReparentedTarget != null) { ReturnCameraTargetsToNormal(_CurrentReparentedTarget, frames); }

		_TemporaryCameraTarget.parent = newParent;
		_TemporaryCameraTarget.position = position;

		_movedTargetsPreviousParent = TargetToMove.parent;
		_CurrentReparentedTarget = TargetToMove;

		TargetToMove.parent = _TemporaryCameraTarget;

		StartCoroutine(LerpTargetPosition(TargetToMove, TargetToMove.localPosition, Vector3.zero, frames));
	}

	private IEnumerator LerpTargetPosition ( Transform Target, Vector3 localPositionA, Vector3 localPositionB, float frames ) {
		frames = Mathf.Max(frames, 1 );
		for (float f = 1 ; f <= frames ; f++)
		{
			yield return new WaitForFixedUpdate();
			Target.localPosition = Vector3.Lerp(localPositionA, localPositionB, f / frames);
		}
	}

	//Undoes the above method, returning the camera target to its normal place.
	public void ReturnCameraTargetsToNormal ( Transform TargetToReturn, float frames ) {
		if(!_movedTargetsPreviousParent) { return; }
		TargetToReturn = TargetToReturn ? TargetToReturn : _CurrentReparentedTarget;
		TargetToReturn.parent = _movedTargetsPreviousParent;

		StartCoroutine(LerpTargetPosition(TargetToReturn, TargetToReturn.localPosition, Vector3.zero, frames));
		_CurrentReparentedTarget = null;
		_movedTargetsPreviousParent = null;
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning
	private void SetTools () {

		_VirtualCamera = GetComponent<CinemachineVirtualCamera>();
		_Noise = _VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		_PlayerTransformReal = _PlayerPhys.transform;
		_PlayerTransformCopy.rotation = _PlayerTransformReal.rotation;
		_currentFaceDirection = GetFaceDirection(_Skin.forward);
		_Transposer = _VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

		_VirtualCamera.Follow = _FinalTarget;

		_SecondaryCamera.gameObject.SetActive(false);
		_SecondaryCamera.transform.parent = null;

		_MainCameraBrain = _Tools.MainCamera;

		_Actions = _Tools._ActionManager;
		_PlayerMovement = _PlayerPhys.GetComponent<S_PlayerMovement>();
		_PlayerVel = _PlayerPhys.GetComponent<S_PlayerVelocity>();
	}

	void SetStats () {
		_isLocked = false;

		_invertedX = 1;
		_invertedY = 1;

		_shouldSetHeightWhenMoving_ = _Tools.CameraStats.LockHeightStats.LockHeight;
		_lockHeightSpeed_ = _Tools.CameraStats.LockHeightStats.LockHeightSpeed;
		_shouldFaceDownWhenInAir_ = _Tools.CameraStats.AutoLookDownStats.shouldLookDownWhenInAir;
		_minHeightToLookDown_ = _Tools.CameraStats.AutoLookDownStats.minHeightToLookDown;
		_heightToLock_ = _Tools.CameraStats.LockHeightStats.HeightToLock;
		_heightFollowSpeed_ = _Tools.CameraStats.AutoLookDownStats.HeightFollowSpeed;
		_fallSpeedThreshold_ = _Tools.CameraStats.AutoLookDownStats.FallSpeedThreshold;

		_cameraMaxDistance_ = _Tools.CameraStats.DistanceStats.CameraDistance;
		_cameraDistanceBySpeed_ = _Tools.CameraStats.DistanceStats.cameraDistanceBySpeed;
		_shouldAffectDistanceBySpeed_ = _Tools.CameraStats.DistanceStats.shouldAffectDistancebySpeed;
		_VirtualCamera.GetComponent<CinemachineCollider>().m_CollideAgainst = _Tools.CameraStats.DistanceStats.CollidableLayers;
		_CollidableLayers_ = _Tools.CameraStats.DistanceStats.CollidableLayers;

		_shouldAffectFOVBySpeed_ = _Tools.CameraStats.FOVStats.shouldAffectFOVbySpeed;
		_baseFOV_ = _Tools.CameraStats.FOVStats.baseFOV;
		_cameraFOVBySpeed_ = _Tools.CameraStats.FOVStats.cameraFOVBySpeed;

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