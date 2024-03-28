using UnityEngine;
using System.Collections;
using SplineMesh;

[RequireComponent(typeof(S_ActionManager))]
public class S_Action10_FollowAutoPath : MonoBehaviour, IMainAction
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
	private S_Control_SoundsPlayer _Sounds;
	private S_Interaction_Pathers _Path_Int;
	private S_HedgeCamera	_CamHandler;

	private CurveSample _Sample;
	private Animator	_CharacterAnimator;
	private Transform   _MainSkin;
	[HideInInspector]
	public Collider	_PatherStarter;
	private Transform	_Skin;
	private Transform	_PathTransform;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	#endregion

	// Trackers
	#region trackers
	private Vector3	_OGSkinLocPos;

	public float	_skinRotationSpeed;
	public Vector3	_skinOffsetPos = new Vector3(0, -0.4f, 0);
	public float	_offset = 2.05f;

	private Quaternion	_charRot;

	// Setting up Values
	private float	_range = 0f;
	[HideInInspector]
	public float	_playerSpeed;
	private float	_pathTopSpeed;
	private bool	_isGoingBackwards;

	//Camera testing
	public float	_targetDistance = 10;
	public float	_cameraLerp = 10;

	[HideInInspector] 
	public float	_originalRayToGroundRot;
	[HideInInspector] 
	public float	_originalRayToGround;
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
		_OGSkinLocPos = _Skin.transform.localPosition;
	}
	private void OnDisable () {
		if (_Skin != null)
		{
			_Skin.transform.localPosition = _OGSkinLocPos;
			_Skin.localRotation = Quaternion.identity;
		}
	}

	// Update is called once per frame
	void Update () {
		//CameraFocus();

		//Set Animator Parameters as player is always running

		_CharacterAnimator.SetInteger("Action", 0);
		_CharacterAnimator.SetFloat("YSpeed", _PlayerPhys._RB.velocity.y);
		_CharacterAnimator.SetFloat("GroundSpeed", _PlayerPhys._RB.velocity.magnitude);
		_CharacterAnimator.SetBool("Grounded", _PlayerPhys._isGrounded);


		//Set Animation Angle
		Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, _PlayerPhys._RB.velocity.y, _PlayerPhys._RB.velocity.z);
		Quaternion CharRot = Quaternion.LookRotation(VelocityMod, _Sample.up);
		_MainSkin.rotation = Quaternion.Lerp(_MainSkin.rotation, CharRot, Time.deltaTime * _skinRotationSpeed);
	}

	private void FixedUpdate () {
		_Input.LockInputForAWhile(1f, true, Vector3.zero);
		PathMove();
	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		willChangeAction = true;
		return willChangeAction;
	}

	public void StartAction () {

	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.

		enabled = false;

		if (isFirstTime) { return; } //If first time, then return after setting to disabled.
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
		_Path_Int = GetComponent<S_Interaction_Pathers>();
		_PlayerPhys = GetComponent<S_PlayerPhysics>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.mainSkin;
		_Sounds = _Tools.SoundControl;
		_CamHandler = GetComponent<S_Handler_Camera>()._HedgeCam;
		_Skin = _Tools.mainSkin;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {

	}
	#endregion


	public void InitialEvents ( float Range, Transform PathPos, bool back, float speed, float pathspeed = 0f ) {

		//Disable colliders to prevent jankiness
		//Path_Int.playerCol.enabled = false;


		_Skin.transform.localPosition = _Skin.transform.localPosition;


		//fix for camera jumping
		//rotYFix = transform.rotation.eulerAngles.y;
		//transform.rotation = Quaternion.identity;

		if (transform.eulerAngles.y < -89)
		{
			_PlayerPhys.transform.eulerAngles = new Vector3(0, -89, 0);
		}


		//Setting up the path to follow
		_range = Range;
		_PathTransform = PathPos;

		_pathTopSpeed = pathspeed;

		_playerSpeed = _PlayerPhys._RB.velocity.magnitude;

		if (_playerSpeed < speed)
			_playerSpeed = speed;

		_isGoingBackwards = back;
		_CharacterAnimator.SetBool("Grounded", true);

	}


	public void PathMove () {
		//Increase the Amount of distance through the Spline by DeltaTime
		float ammount = (Time.deltaTime * _playerSpeed);

		_PlayerPhys.AlignToGround(_PlayerPhys._groundNormal, _PlayerPhys._isGrounded);

		//Slowly increases player speed.
		if (_playerSpeed < _PlayerPhys._currentTopSpeed || _playerSpeed < _pathTopSpeed)
		{
			if (_playerSpeed < _PlayerPhys._currentTopSpeed * 0.7f)
				_playerSpeed += .14f;
			else
				_playerSpeed += .07f;

		}

		//Simple Slope effects
		if (_PlayerPhys._groundNormal.y < 1 && _PlayerPhys._groundNormal != Vector3.zero)
		{
			//UpHill
			if (_PlayerPhys._RB.velocity.y > 0f)
			{
				_playerSpeed -= (1f - _PlayerPhys._groundNormal.y) * 0.1f;
			}

			//DownHill
			if (_PlayerPhys._RB.velocity.y < 0f)
			{
				_playerSpeed += (1f - _PlayerPhys._groundNormal.y) * 0.1f;
			}
		}

		//Leave path at low speed
		if (_playerSpeed < 10f)
		{
			ExitPath();
		}

		// Increase/Decrease Range depending on direction

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
		if (_range < _Path_Int._PathSpline.Length && _range > 0)
		{
			//Get Sample of the Path to put player
			_Sample = _Path_Int._PathSpline.GetSampleAtDistance(_range);

			//Set player Position and rotation on Path
			//Quaternion rot = (Quaternion.FromToRotation(Skin.transform.up, sample.Rotation * Vector3.up) * Skin.rotation);
			//Skin.rotation = rot;
			//CharacterAnimator.transform.rotation = rot;


			if ((Physics.Raycast(_Sample.location + (transform.up * 2), -transform.up, out RaycastHit hitRot, 2.2f + _Tools.Stats.FindingGround.rayToGroundDistance, _PlayerPhys._Groundmask_)))
			{
				//Vector3 FootPos = transform.position - Path_Int.feetPoint.position;
				//transform.position = (hitRot.point + PathTransform.position) + FootPos;

				Vector3 FootPos = transform.position - _Path_Int._FeetTransform.position;
				transform.position = ((_Sample.location) + _PathTransform.position) + FootPos;
			}
			else
			{
				Vector3 FootPos = transform.position - _Path_Int._FeetTransform.position;
				transform.position = ((_Sample.location) + _PathTransform.position) + FootPos;
			}



			//Moves the player to the position of the Upreel
			//Vector3 HandPos = transform.position - HandGripPoint.position;
			//transform.position = currentUpreel.HandleGripPos.position + HandPos;


			//Set Player Speed correctly for smoothness
			if (!_isGoingBackwards)
			{
				_PlayerPhys._RB.velocity = _Sample.tangent * (_playerSpeed);


				//remove camera tracking at the end of the path to be safe from strange turns
				//if (range > Rail_int.RailSpline.Length * 0.9f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f;}
			}
			else
			{
				_PlayerPhys._RB.velocity = -_Sample.tangent * (_playerSpeed);

				//remove camera tracking at the end of the path to be safe from strange turns
				//if (range < 0.1f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f; }
			}

		}
		else
		{
			//Check if the Spline is loop and resets position
			if (_Path_Int._PathSpline.IsLoop)
			{
				if (!_isGoingBackwards)
				{
					_range = _range - _Path_Int._PathSpline.Length;
					PathMove();
				}
				else
				{
					_range = _range + _Path_Int._PathSpline.Length;
					PathMove();
				}
			}
			else
			{
				ExitPath();
			}
		}

	}

	public void ExitPath () {


		_PlayerPhys.AlignToGround(_PlayerPhys._groundNormal, _PlayerPhys._isGrounded);

		//Set Player Speed correctly for smoothness
		if (!_isGoingBackwards)
		{
			_Sample = _Path_Int._PathSpline.GetSampleAtDistance(_Path_Int._PathSpline.Length);
			_PlayerPhys._RB.velocity = _Sample.tangent * (_playerSpeed);

		}
		else
		{
			_Sample = _Path_Int._PathSpline.GetSampleAtDistance(0);
			_PlayerPhys._RB.velocity = -_Sample.tangent * (_playerSpeed);

		}

		_MainSkin.rotation = Quaternion.LookRotation(_PlayerPhys._RB.velocity, _PlayerPhys._HitGround.normal);

		_PlayerPhys.GetComponent<S_Handler_Camera>()._HedgeCam.SetBehind(0);
		_Input.LockInputForAWhile(30f, true, _MainSkin.forward);

		//Reenables Colliders
		//Path_Int.playerCol.enabled = true;


		//Deactivates any cinemachine that might be attached.
		if (_Path_Int._currentExternalCamera != null)
		{
			_Path_Int._currentExternalCamera.DeactivateCam(18);
			_Path_Int._currentExternalCamera = null;
		}

		_Actions._ActionDefault.StartAction();
	}
}


