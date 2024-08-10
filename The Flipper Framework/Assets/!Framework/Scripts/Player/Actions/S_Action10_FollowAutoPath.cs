using UnityEngine;
using System.Collections;
using SplineMesh;

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
	private S_Interaction_Pathers _Pathers;
	private S_HedgeCamera	_CamHandler;

	private CurveSample _Sample;
	private Animator	_CharacterAnimator;
	private Transform   _MainSkin;
	[HideInInspector]
	public Collider	_PatherStarter;
	private Transform	_Skin;
	private Transform	_PathTransform;
	#endregion



	//Stats
	#region Stats
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;

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
		ReadyAction();
		_OGSkinLocPos = _Skin.transform.localPosition;
	}

	// Update is called once per frame
	void Update () {

	}

	private void FixedUpdate () {
		_Input.LockInputForAWhile(1f, true, Vector3.zero);
		PathMove();
	}

	public bool AttemptAction () {
		
		_Pathers._canEnterAutoPath = true;
		return false;
	}

	public void StartAction () {

	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		if (_Skin != null)
		{
			_Skin.transform.localPosition = _OGSkinLocPos;
			_Skin.localRotation = Quaternion.identity;
		}
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

	//Assigns all external elements of the action.
	public void ReadyAction () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponentInParent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			//Get this actions placement in the action manager list, so it can be referenced to acquire its connected actions.
			for (int i = 0 ; i < _Actions._MainActions.Count ; i++)
			{
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Path)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Actions = _Tools._ActionManager;
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_Pathers = _Tools.PathInteraction;
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_MainSkin = _Tools.MainSkin;
		_Sounds = _Tools.SoundControl;
		_CamHandler = _Tools.CamHandler._HedgeCam;
		_Skin = _Tools.MainSkin;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {

	}
	#endregion


	public void AssignForThisAutoPath ( float Range, Transform PathPos, bool back, float speed, float pathspeed = 0f ) {

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
		if (_range < _Pathers._PathSpline.Length && _range > 0)
		{
			//Get Sample of the Path to put player
			_Sample = _Pathers._PathSpline.GetSampleAtDistance(_range);

			//Set player Position and rotation on Path
			//Quaternion rot = (Quaternion.FromToRotation(Skin.transform.up, sample.Rotation * Vector3.up) * Skin.rotation);
			//Skin.rotation = rot;
			//CharacterAnimator.transform.rotation = rot;


			if ((Physics.Raycast(_Sample.location + (transform.up * 2), -transform.up, out RaycastHit hitRot, 2.2f + _Tools.Stats.FindingGround.rayToGroundDistance, _PlayerPhys._Groundmask_)))
			{
				//Vector3 FootPos = transform.position - Path_Int.feetPoint.position;
				//transform.position = (hitRot.point + PathTransform.position) + FootPos;

				Vector3 FootPos = transform.position - _Pathers._FeetTransform.position;
				_PlayerPhys.SetPlayerPosition(((_Sample.location) + _PathTransform.position) + FootPos);
			}
			else
			{
				Vector3 FootPos = transform.position - _Pathers._FeetTransform.position;
				_PlayerPhys.SetPlayerPosition(((_Sample.location) + _PathTransform.position) + FootPos)	;
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
			if (_Pathers._PathSpline.IsLoop)
			{
				if (!_isGoingBackwards)
				{
					_range = _range - _Pathers._PathSpline.Length;
					PathMove();
				}
				else
				{
					_range = _range + _Pathers._PathSpline.Length;
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
			_Sample = _Pathers._PathSpline.GetSampleAtDistance(_Pathers._PathSpline.Length);
			_PlayerPhys._RB.velocity = _Sample.tangent * (_playerSpeed);

		}
		else
		{
			_Sample = _Pathers._PathSpline.GetSampleAtDistance(0);
			_PlayerPhys._RB.velocity = -_Sample.tangent * (_playerSpeed);

		}

		_MainSkin.rotation = Quaternion.LookRotation(_PlayerPhys._RB.velocity, _PlayerPhys._HitGround.normal);

		_CamHandler.SetBehind(0);
		_Input.LockInputForAWhile(30f, true, _MainSkin.forward);

		//Reenables Colliders
		//Path_Int.playerCol.enabled = true;


		//Deactivates any cinemachine that might be attached.
		if (_Pathers._currentExternalCamera != null)
		{
			_Pathers._currentExternalCamera.DeactivateCam(18);
			_Pathers._currentExternalCamera = null;
		}

		_Actions._ActionDefault.StartAction();
	}
}


