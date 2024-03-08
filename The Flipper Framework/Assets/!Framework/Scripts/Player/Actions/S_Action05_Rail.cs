using UnityEngine;
using System.Collections;
using UnityEngine.Windows;
using SplineMesh;

[RequireComponent(typeof(S_ActionManager))]
public class S_Action05_Rail : MonoBehaviour, IMainAction
{

	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_CharacterTools      _Tools;
	private S_PlayerPhysics       _PlayerPhys;
	private S_PlayerInput         _Input;
	private S_ActionManager       _Actions;
	private S_Control_PlayerSound _Sounds;
	[HideInInspector]
	public S_Interaction_Pathers  _Rail_int;
	public S_AddOnRail		_ConnectedRails;
	private S_HedgeCamera         _CamHandler;
	private Transform             _RailTransform;
	public CurveSample		_Sample;

	[HideInInspector]
	public Transform		_ZipHandle;
	[HideInInspector]
	public Rigidbody		_ZipBody;
	private GameObject            _JumpBall;
	private Quaternion            _CharRot;
	private Animator              _CharacterAnimator;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	[Header("Skin Rail Params")]

	public float                  _skinRotationSpeed;
	private float                 _offsetRail_ = 2.05f;
	private float                 _offsetZip_ = -2.05f;

	[HideInInspector] 
	public float		_railmaxSpeed_;
	private float		_railTopSpeed_;
	private float		_decaySpeedLow_;
	private float		_decaySpeedHigh_;
	private float		_minStartSpeed_ = 60f;
	private float		_pushFowardmaxSpeed_ = 80f;
	private float		_pushFowardIncrements_ = 15f;
	private float		_pushFowardDelay_ = 0.5f;
	private float		_slopePower_ = 2.5f;
	private float		_upHillMultiplier_ = 0.25f;
	private float		_downHillMultiplier_ = 0.35f;
	private float		_upHillMultiplierCrouching_ = 0.4f;
	private float		_downHillMultiplierCrouching_ = 0.6f;
	private float		_dragVal_ = 0.0001f;
	private float		_playerBrakePower_ = 0.95f;
	private float		_hopDelay_;
	private float		_hopDistance_ = 12;
	private float		_decayTime_;
	private AnimationCurve	_accelBySpeed_;
	private float		_decaySpeed_;
	#endregion

	// Trackers
	#region trackers
	public LayerMask		_RailMask;
	[HideInInspector]
	public bool                   _isOnZipLine;
	private float                 _pulleyRotate;
	[HideInInspector]
	public float		_curvePosSlope;

	// Setting up Values
	private float		_timer = 0f;
	[HideInInspector] 
	public float		_range = 0f;
	[HideInInspector]
	public bool                   _isOnRail ;
	[HideInInspector] 
	public float		_playerSpeed;
	[HideInInspector] 
	public bool		_isGoingBackwards;
	private bool                  _isCrouching;

	//Sounds
	private int		_railSound = 1;
	private bool		_playingRailContactSound, isBraking, isSwitching;

	//Quaternion rot;
	private Quaternion		_initialRot;
	private Vector3		_setOffSet;

	//Camera testing
	public float		_targetDistance = 10;
	public float		_cameraLerp = 10;

	//Stepping
	private bool	_canInput = true;
	private bool	_canHop = false;
	private float	_distanceToStep;
	private float	_stepSpeed_ = 3.5f;
	private bool	_isSteppingRight;
	private bool	_isFacingRight = true;

	//Boosters
	[HideInInspector] 
	public bool	_isBoosted;
	[HideInInspector] 
	public float	_boostTime;
	#endregion
	#endregion
	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Start () {

	}

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		if (_PlayerPhys == null)
		{
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();
		}
		_playingRailContactSound = false;
	}
	private void OnDisable () {
		if (!transform.parent.gameObject.activeSelf)
			return;

		_PlayerPhys._isGravityOn = true;

		_isOnRail = false;
		_isOnZipLine = false;
		_ZipBody = null;
		isBraking = false;
		_playingRailContactSound = false;
		StartCoroutine(DelayCollision());

		////////Sounds.RailSoundStop();
		///

		_Input.RollPressed = false;
		_Input.SpecialPressed = false;
		_Input.BouncePressed = false;

		transform.rotation = Quaternion.identity;

		if (_PlayerPhys._RB.velocity != Vector3.zero)
			_CharacterAnimator.transform.rotation = Quaternion.LookRotation(_PlayerPhys._RB.velocity, Vector3.up);

		//if (Skin != null)
		//{
		//    Skin.transform.localPosition = OGSkinLocPos;
		//    Skin.localRotation = Quaternion.identity;
		//}
	}

	// Update is called once per frame
	void Update () {
		SoundControl();
		//CameraFocus();
		//Set Animator Parameters
		//CharacterAnimator.SetFloat("YSpeed", Player.rb.velocity.y);
		_CharacterAnimator.SetFloat("GroundSpeed", _playerSpeed / _PlayerPhys._currentMaxSpeed);
		_CharacterAnimator.SetBool("Grounded", false);

		// Actions Go Here
		if (!_Actions.isPaused && _canInput)
		{
			InputHandling();
		}
	}

	private void FixedUpdate () {
		if (_isOnRail)
		{
			_CharacterAnimator.SetBool("GrindRight", _isFacingRight);
			RailGrind();
		}
		else
		{
			_Actions.ActionDefault.ReadyCoyote();
			_CharacterAnimator.SetInteger("Action", 0);
			_CharacterAnimator.SetBool("Grounded", _PlayerPhys._isGrounded);

			_Actions.ActionDefault.StartAction();
		}
	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		willChangeAction = true;
		return willChangeAction;
	}

	public void StartAction () {

	}

	public void StopAction () {

	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	public void HandleInputs () {

	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Actions = GetComponent<S_ActionManager>();
		_Input = GetComponent<S_PlayerInput>();
		_Rail_int = GetComponent<S_Interaction_Pathers>();
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_CamHandler = GetComponent<S_Handler_Camera>()._HedgeCam;

		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;

		_JumpBall = _Tools.JumpBall;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_railTopSpeed_ = _Tools.Stats.RailStats.railTopSpeed;
		_railmaxSpeed_ = _Tools.Stats.RailStats.railMaxSpeed;
		_decaySpeedHigh_ = _Tools.Stats.RailStats.railDecaySpeedHigh;
		_decaySpeedLow_ = _Tools.Stats.RailStats.railDecaySpeedLow;
		_minStartSpeed_ = _Tools.Stats.RailStats.MinStartSpeed;
		_pushFowardmaxSpeed_ = _Tools.Stats.RailStats.RailPushFowardmaxSpeed;
		_pushFowardIncrements_ = _Tools.Stats.RailStats.RailPushFowardIncrements;
		_pushFowardDelay_ = _Tools.Stats.RailStats.RailPushFowardDelay;
		_slopePower_ = _Tools.Stats.SlopeStats.generalHillMultiplier;
		_upHillMultiplier_ = _Tools.Stats.RailStats.RailUpHillMultiplier;
		_downHillMultiplier_ = _Tools.Stats.RailStats.RailDownHillMultiplier;
		_upHillMultiplierCrouching_ = _Tools.Stats.RailStats.RailUpHillMultiplierCrouching;
		_downHillMultiplierCrouching_ = _Tools.Stats.RailStats.RailDownHillMultiplierCrouching;
		_dragVal_ = _Tools.Stats.RailStats.RailDragVal;
		_playerBrakePower_ = _Tools.Stats.RailStats.RailPlayerBrakePower;
		_hopDelay_ = _Tools.Stats.RailStats.hopDelay;
		_stepSpeed_ = _Tools.Stats.RailStats.hopSpeed;
		_hopDistance_ = _Tools.Stats.RailStats.hopDistance;
		_accelBySpeed_ = _Tools.Stats.RailStats.RailAccelerationBySpeed;

		_offsetRail_ = _Tools.Stats.RailPosition.offsetRail;
		_offsetZip_ = _Tools.Stats.RailPosition.offsetZip;
		_decaySpeed_ = _Tools.Stats.RailStats.railBoostDecaySpeed;
		_decayTime_ = _Tools.Stats.RailStats.railBoostDecayTime;
	}
	#endregion

	IEnumerator DelayCollision () {
		yield return new WaitForSeconds(0.1f);
		Physics.IgnoreLayerCollision(8, 23, false);
	}

	public void InitialEvents ( float Range, Transform RailPos, bool isZip, Vector3 thisOffset, S_AddOnRail addOn ) {
		StartCoroutine(allowHop());

		//ignore further railcollisions
		Physics.IgnoreLayerCollision(this.gameObject.layer, 23, true);

		_canInput = true;
		_setOffSet = -thisOffset;

		_ConnectedRails = addOn;
		_JumpBall.SetActive(false);
		_Input.JumpPressed = false;

		_isOnZipLine = isZip;
		_timer = _pushFowardDelay_;
		_playingRailContactSound = false;

		_isBoosted = false;
		_boostTime = 0;

		_PlayerPhys._isGravityOn = false;
		// Player.p_rigidbody.useGravity = false;

		//Animations and Skin Changes
		//CharacterAnimator.SetTrigger("GenericT");

		if (!_isOnZipLine)
		{
			_CharacterAnimator.SetBool("GrindRight", _isFacingRight);
			_CharacterAnimator.SetInteger("Action", 10);


		}
		else
		{
			_ZipHandle.GetComponentInChildren<MeshCollider>().enabled = false;
			_CharacterAnimator.SetInteger("Action", 9);

		}

		_CharacterAnimator.SetTrigger("HitRail");

		//fix for camera jumping
		//rotYFix = transform.rotation.eulerAngles.y;
		//transform.rotation = Quaternion.identity;
		if (transform.eulerAngles.y < -89)
		{
			_PlayerPhys.transform.eulerAngles = new Vector3(0, -89, 0);
		}


		//Setting up Rails
		_range = Range;
		_RailTransform = RailPos;
		_isOnRail = true;

		if (_distanceToStep <= 0)
			_playerSpeed = _PlayerPhys._speedMagnitude;



		CurveSample sample = _Rail_int.RailSpline.GetSampleAtDistance(_range);
		float dotdir = Vector3.Dot(_PlayerPhys._RB.velocity.normalized, sample.tangent);
		_isCrouching = false;
		_pulleyRotate = 0f;

		_initialRot = transform.rotation;

		//Vector3 dir = sample.tangent;
		//Cam.SetCamera(dir, 2.5f, 20f, 1f);
		//Cam.Locked = false;


		// Check if was Homingattack
		if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Homing)
		{
			_playerSpeed = _PlayerPhys._speedMagnitude;
			dotdir = Vector3.Dot(_Actions.Action02._targetDirection.normalized, sample.tangent);
		}
		else if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.DropCharge)
		{
			//dotdir = Vector3.Dot(Player.rb.velocity.normalized, sample.tangent);

			float charge = _Actions.Action08.externalDash();


			_playerSpeed = Mathf.Clamp(charge, _playerSpeed + (charge / 6), 160);
		}

		//Cam.CamLagSet(4);
		////////Cam.OverrideTarget.position = Skin.position;

		// make sure that Player wont have a shitty Speed...
		if ((dotdir > 0.85f || dotdir < -.85f) && _distanceToStep > 0)
		{
			_playerSpeed = Mathf.Abs(_playerSpeed * 1);
		}
		else if (dotdir < 0.5 && dotdir > -.5f)
		{
			_playerSpeed = Mathf.Abs(_playerSpeed * 0.8f);
		}
		_playerSpeed = Mathf.Max(_playerSpeed, _minStartSpeed_);

		// Get Direction for the Rail
		if (dotdir > 0)
		{
			_isGoingBackwards = false;

			if (_isOnZipLine && _range > _Rail_int.RailSpline.Length - 5)
				_isGoingBackwards = true;
		}
		else
		{
			_isGoingBackwards = true;
			if (_isOnZipLine && _range < 5)
				_isGoingBackwards = false;
		}


		_PlayerPhys._RB.velocity = Vector3.zero;
		_PlayerPhys.SetTotalVelocity(Vector3.zero, new Vector2(1, 0));

	}


	IEnumerator allowHop () {
		_canHop = false;
		yield return new WaitForSeconds(_hopDelay_);
		_canHop = true;
	}

	void InputHandling () {
		_timer += Time.deltaTime;

		if (_Input.JumpPressed)
		{

			Vector3 jumpCorrectedOffset = (_CharacterAnimator.transform.up * 1.5f); //Quaternion.LookRotation(Player.p_rigidbody.velocity, transform.up) * (transform.forward * 3.5f);


			if (_isOnZipLine)
			{
				if (!_isGoingBackwards)
					_PlayerPhys._RB.velocity = _Sample.tangent * _playerSpeed;
				else
					_PlayerPhys._RB.velocity = -_Sample.tangent * _playerSpeed;

				jumpCorrectedOffset = -jumpCorrectedOffset;
				_ZipBody.isKinematic = true;
				_PlayerPhys._groundNormal = Vector3.up;

				StartCoroutine(_Rail_int.JumpFromZipLine(_ZipHandle, 1));

			}

			transform.position += jumpCorrectedOffset;

			_isOnRail = false;

			_isOnZipLine = false;


		}


		if (_Input.RollPressed && !_isOnZipLine)
		{
			//Crouch
			_isCrouching = true;
			_CharacterAnimator.SetBool("isRolling", true);
		}
		else
		{
			_isCrouching = false;
			_CharacterAnimator.SetBool("isRolling", false);
		}

		if (_Input.SpecialPressed && !_isOnZipLine)
		{
			//ChangeSide

			if (_timer > _pushFowardDelay_)
			{
				//Sounds.RailSoundStop();
				isSwitching = true;
				if (_playerSpeed < _pushFowardmaxSpeed_)
				{
					_playerSpeed += _pushFowardIncrements_ + _accelBySpeed_.Evaluate(_playerSpeed / _pushFowardmaxSpeed_);
				}
				_isFacingRight = !_isFacingRight;
				_timer = 0f;
				_Input.SpecialPressed = false;
			}
		}
		isSwitching = (_timer < _pushFowardDelay_);

		//If above a certain speed, the player breaks depending it they're presseing the skid button.
		if (Time.timeScale != 0 && !_isOnZipLine)
		{
			isBraking = _Input.BouncePressed;

		}
		else
		{
			isBraking = false;
		}

	}

	public void RailGrind () {

		//Increase the Amount of distance trought the Spline by DeltaTime
		float ammount = (Time.deltaTime * _playerSpeed);
		// Increase/Decrease Range depending on direction

		SlopePhys();

		if (!_isGoingBackwards)
		{
			//range += ammount / dist;
			_range += ammount;
		}
		else
		{
			//range -= ammount / dist;
			_range -= ammount;
		}

		//Check so for the size of the Spline
		if (_range < _Rail_int.RailSpline.Length && _range > 0)
		{
			//Get Sample of the Rail to put player
			_Sample = _Rail_int.RailSpline.GetSampleAtDistance(_range);

			//Set player Position and rotation on Rail
			if (!_isOnZipLine)
			{
				if (_isGoingBackwards)
				{
					_CharacterAnimator.transform.rotation = Quaternion.LookRotation(-_Sample.tangent, _Sample.up);
				}
				else
				{
					_CharacterAnimator.transform.rotation = Quaternion.LookRotation(_Sample.tangent, _Sample.up);
				}

				Vector3 binormal = Vector3.zero;

				if (_setOffSet != Vector3.zero)
				{
					//binormal = sample.tangent;
					//binormal = Quaternion.LookRotation(Vector3.right, Vector3.up) * binormal;
					binormal += _Sample.Rotation * -_setOffSet;
				}
				transform.position = (_Sample.location + _RailTransform.position + (_Sample.up * _offsetRail_)) + binormal;

				if (_canHop)
				{
					railHopping();
				}

			}
			else
			{
				float rotatePoint = 0;
				if (_Input.RightStepPressed)
				{
					_Input.LeftStepPressed = false;
					rotatePoint = 1;
				}
				else if (_Input.LeftStepPressed)
				{
					_Input.RightStepPressed = false;
					rotatePoint = -1;
				}

				if (!_isGoingBackwards)
				{
					_pulleyRotate = Mathf.MoveTowards(_pulleyRotate, rotatePoint, 3.5f * Time.deltaTime);
					_CharacterAnimator.transform.rotation = Quaternion.LookRotation(_Sample.tangent, _Sample.up);
				}
				else
				{
					_pulleyRotate = Mathf.MoveTowards(_pulleyRotate, rotatePoint, 3.5f * Time.deltaTime);
					_CharacterAnimator.transform.rotation = Quaternion.LookRotation(-_Sample.tangent, _Sample.up);
				}

				_ZipHandle.rotation = _Sample.Rotation;
				_ZipHandle.eulerAngles = new Vector3(_ZipHandle.eulerAngles.x, _ZipHandle.eulerAngles.y, _ZipHandle.eulerAngles.z + _pulleyRotate * 70f);

				_CharacterAnimator.transform.eulerAngles = new Vector3(_CharacterAnimator.transform.eulerAngles.x, _CharacterAnimator.transform.eulerAngles.y, _CharacterAnimator.transform.eulerAngles.z + _pulleyRotate * 70f);




				// Cam.FollowDirection(0.8f, 14, -5, 0.1f, true);
				//CameraTarget.position = sample.location + RailTransform.position;
				// CameraTarget.localRotation = Quaternion.LookRotation(CharacterAnimator.transform.forward, Vector3.up);



				_ZipHandle.transform.position = (_Sample.location + _RailTransform.position) + _setOffSet;
				transform.position = _ZipHandle.transform.position + (_ZipHandle.transform.up * _offsetZip_);


			}

			if (isBraking && _playerSpeed > _minStartSpeed_) _playerSpeed *= _playerBrakePower_;

			//Set Player Speed correctly so that it becomes smooth grinding
			if (!_isGoingBackwards)
			{

				if (_isOnZipLine && _ZipBody != null)
				{
					_ZipBody.velocity = _Sample.tangent * (_playerSpeed);
					_PlayerPhys._RB.velocity = _Sample.tangent;
				}
				else
					_PlayerPhys._RB.velocity = _Sample.tangent * (_playerSpeed);

				//remove camera tracking at the end of the rail to be safe from strange turns
				//if (range > Rail_int.RailSpline.Length * 0.9f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f;}
				if (_range > _Rail_int.RailSpline.Length * 0.9f)
				{
					// Cam.lockCamFor(0.5f);
				}
			}
			else
			{

				if (_isOnZipLine && _ZipBody != null)
				{
					_ZipBody.velocity = -_Sample.tangent * (_playerSpeed);
					_PlayerPhys._RB.velocity = -_Sample.tangent;
				}
				else
					_PlayerPhys._RB.velocity = -_Sample.tangent * (_playerSpeed);
				//remove camera tracking at the end of the rail to be safe from strange turns
				//if (range < 0.1f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f; }
				if (_range > _Rail_int.RailSpline.Length * 0.9f)
				{
					//  Cam.lockCamFor(0.5f);
				}
			}

		}
		else
		{
			if (!_isGoingBackwards)
				_Sample = _Rail_int.RailSpline.GetSampleAtDistance(_Rail_int.RailSpline.Length - 1);
			else
				_Sample = _Rail_int.RailSpline.GetSampleAtDistance(0);

			LoseRail();
		}

	}

	void railHopping () {
		if (_canInput)
		{
			//Takes in quickstep and makes it relevant to the camera (e.g. if player is facing that camera, step left becomes step right)
			if (_Input.RightStepPressed)
			{
				Vector3 Direction = _CharacterAnimator.transform.position - _CamHandler.transform.position;
				bool Facing = Vector3.Dot(_CharacterAnimator.transform.forward, Direction.normalized) < -0.5f;
				if (Facing)
				{
					_Input.RightStepPressed = false;
					_Input.LeftStepPressed = true;
				}
			}
			else if (_Input.LeftStepPressed)
			{
				Vector3 Direction = _CharacterAnimator.transform.position - _CamHandler.transform.position;
				bool Facing = Vector3.Dot(_CharacterAnimator.transform.forward, Direction.normalized) < -0.5f;
				if (Facing)
				{
					_Input.RightStepPressed = true;
					_Input.LeftStepPressed = false;
				}
			}

			Debug.DrawRay(transform.position - (_Sample.up * 2) + (_CharacterAnimator.transform.right * 3), _CharacterAnimator.transform.right * 10, Color.red);
			Debug.DrawRay(transform.position - (_Sample.up * 2) + (_CharacterAnimator.transform.right * 3), -_CharacterAnimator.transform.right * 10, Color.red);

			if (_Input.RightStepPressed)
			{

				_distanceToStep = _hopDistance_;
				_canInput = false;
				_isSteppingRight = true;
				_Input.RightStepPressed = false;
				performStep();
				return;

			}
			else if (_Input.LeftStepPressed)
			{


				_distanceToStep = _hopDistance_;
				_canInput = false;
				_isSteppingRight = false;
				_Input.LeftStepPressed = false;
				performStep();
				return;
			}
		}

		performStep();
	}

	void performStep () {
		if (_distanceToStep > 0)
		{
			float move = _stepSpeed_;

			if (_isSteppingRight)
				move = -move;
			if (_isGoingBackwards)
				move = -move;

			move = Mathf.Clamp(move, -_distanceToStep, _distanceToStep);

			_setOffSet.Set(_setOffSet.x + move, _setOffSet.y, _setOffSet.z);

			if (move < 0)
				if (Physics.BoxCast(_CharacterAnimator.transform.position, new Vector3(1.3f, 3f, 1.3f), -_CharacterAnimator.transform.right, Quaternion.identity, 4, _Tools.Stats.QuickstepStats.StepLayerMask))
				{
					_Actions.ActionDefault.StartAction();
				}
				else
				if (Physics.BoxCast(_CharacterAnimator.transform.position, new Vector3(1.3f, 3f, 1.3f), _CharacterAnimator.transform.right, Quaternion.identity, 4, _Tools.Stats.QuickstepStats.StepLayerMask))
				{

					_Actions.ActionDefault.StartAction();
				}

			_distanceToStep -= _stepSpeed_;

			if (_distanceToStep < 6)
			{
				Physics.IgnoreLayerCollision(8, 23, false);

				if (_distanceToStep <= 0)
				{
					_isOnRail = false;
					_Actions.ActionDefault.StartAction();
				}

			}
		}
	}

	void LoseRail () {
		_distanceToStep = 0;
		Physics.IgnoreLayerCollision(8, 23, true);

		Debug.Log("The Rail Is Over");

		//Check if the Spline is loop and resets position
		if (_Rail_int.RailSpline.IsLoop)
		{
			if (!_isGoingBackwards)
			{
				_range = _range - _Rail_int.RailSpline.Length;
				RailGrind();
			}
			else
			{
				_range = _range + _Rail_int.RailSpline.Length;
				RailGrind();
			}
		}
		else if (_ConnectedRails != null && ((!_isGoingBackwards && _ConnectedRails.nextRail != null && _ConnectedRails.nextRail.isActiveAndEnabled) || (_isGoingBackwards && _ConnectedRails.PrevRail != null && _ConnectedRails.PrevRail.isActiveAndEnabled)))
		{
			if (!_isGoingBackwards && _ConnectedRails.nextRail != null)
			{
				Debug.Log("On to Next Rail With = " + _range);
				//Debug.Log("Set by " + ConnectedRails.nextRail);

				_range = _range - _Rail_int.RailSpline.Length;
				_range = 0;

				_ConnectedRails.Announce();

				_ConnectedRails = _ConnectedRails.nextRail;
				_setOffSet.Set(-_ConnectedRails.GetComponent<S_PlaceOnSpline>().Offset3d.x, 0, 0);

				_Rail_int.RailSpline = _ConnectedRails.GetComponentInParent<Spline>();
				_RailTransform = _Rail_int.RailSpline.transform.parent;

				Debug.Log("Then With = " + _range);
			}
			else if (_isGoingBackwards && _ConnectedRails.PrevRail != null)
			{

				Debug.Log("Back To Previous Rail");

				S_AddOnRail temp = _ConnectedRails;
				_ConnectedRails = _ConnectedRails.PrevRail;
				_setOffSet.Set(-_ConnectedRails.GetComponent<S_PlaceOnSpline>().Offset3d.x, 0, 0);

				_Rail_int.RailSpline = _ConnectedRails.GetComponentInParent<Spline>();
				_RailTransform = _Rail_int.RailSpline.transform.parent;

				_range = _range + _Rail_int.RailSpline.Length;
				_range = _Rail_int.RailSpline.Length;

			}


			//RailGrind();
		}
		else
		{
			_Input.LockInputForAWhile(5f, false);

			if (_isOnZipLine)
			{
				_ZipHandle.GetComponent<CapsuleCollider>().enabled = false;
				GameObject target = _ZipHandle.transform.GetComponent<S_Control_PulleyObject>().homingtgt;
				target.SetActive(false);

				if (_isGoingBackwards)
				{
					_PlayerPhys._RB.velocity = _ZipBody.velocity;
				}
				else
				{
					_PlayerPhys._RB.velocity = _ZipBody.velocity;
				}

				Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
				if (VelocityMod != Vector3.zero)
				{
					_CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, transform.up);
				}

			}
			else
			{

				if (_isGoingBackwards)
					_PlayerPhys._RB.velocity = -_Sample.tangent * _playerSpeed;
				else
					_PlayerPhys._RB.velocity = _Sample.tangent * _playerSpeed;

				Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
				if (VelocityMod != Vector3.zero)
				{
					_CharacterAnimator.transform.rotation = Quaternion.LookRotation(VelocityMod, transform.up);
				}
			}

			_Input.LeftStepPressed = false;
			_Input.RightStepPressed = false;

			_isOnRail = false;
		}
	}
	void SlopePhys () {
		if (_isBoosted)
		{
			if (_playerSpeed > 60)
			{
				_boostTime -= Time.fixedDeltaTime;
				if (_boostTime < 0)
				{
					_playerSpeed -= _decaySpeed_;
					if (_boostTime < -_decayTime_)
					{
						_isBoosted = false;
						_boostTime = 0;
					}
				}
			}
			else
				_isBoosted = false;
		}

		//slope curve from Bhys
		_curvePosSlope = _PlayerPhys._curvePosSlopePower;
		//use player vertical speed to find if player is going up or down
		//Debug.Log(Player.p_rigidbody.velocity.normalized.y);

		//if (Player.rb.velocity.y >= -3f)
		if (_PlayerPhys._RB.velocity.y > 0.05f)
		{
			//uphill and straight
			float lean = _upHillMultiplier_;
			if (_isCrouching) { lean = _upHillMultiplierCrouching_; }
			//Debug.Log("UpHill : *" + lean);
			float force = (_slopePower_ * _curvePosSlope) * lean;
			//Debug.Log(Mathf.Abs(Player.p_rigidbody.velocity.normalized.y - 1));
			float AbsYPow = Mathf.Abs(_PlayerPhys._RB.velocity.normalized.y * _PlayerPhys._RB.velocity.normalized.y);
			//Debug.Log( "Val" + Player.p_rigidbody.velocity.normalized.y + "Pow" + AbsYPow);
			force = (AbsYPow * force) + (_dragVal_ * _playerSpeed);
			//Debug.Log(force);
			force = Mathf.Clamp(force, -0.3f, 0.3f);
			_playerSpeed += force;

			//Enforce max Speed
			if (_playerSpeed > _PlayerPhys._currentMaxSpeed)
			{
				_playerSpeed = _PlayerPhys._currentMaxSpeed;
			}
		}
		else if (_PlayerPhys._RB.velocity.y < -0.05f)
		{
			//Downhill
			float lean = _downHillMultiplier_;
			if (_isCrouching) { lean = _downHillMultiplierCrouching_; }
			//Debug.Log("DownHill : *" + lean);
			float force = (_slopePower_ * _curvePosSlope) * lean;
			//Debug.Log(Mathf.Abs(Player.p_rigidbody.velocity.normalized.y));
			float AbsYPow = Mathf.Abs(_PlayerPhys._RB.velocity.normalized.y * _PlayerPhys._RB.velocity.normalized.y);
			//Debug.Log("Val" + Player.p_rigidbody.velocity.normalized.y + "Pow" + AbsYPow);
			force = (AbsYPow * force) - (_dragVal_ * _playerSpeed);
			//Debug.Log(force);
			_playerSpeed -= force;

			//Enforce max Speed
			if (_playerSpeed > _PlayerPhys._currentMaxSpeed)
			{
				_playerSpeed = _PlayerPhys._currentMaxSpeed;
			}
		}
		else
		{
			//Decay
			if (_playerSpeed > _railmaxSpeed_)
				_playerSpeed -= _decaySpeedHigh_;

			else if (_playerSpeed > _railTopSpeed_)
				_playerSpeed -= _decaySpeedLow_;
		}



	}

	void SoundControl () {
		//Player Rail Sound

		//If Entring Rail
		if (!_playingRailContactSound)
		{
			_railSound = !(_isOnZipLine) ? 0 : 10;
			_playingRailContactSound = true;
		}
		else
		{
			_railSound = !(_isOnZipLine) ? 1 : 11;
		}

		if (!isSwitching)
		{
			//Sounds.RailSound(RailSound);
		}
	}

}


