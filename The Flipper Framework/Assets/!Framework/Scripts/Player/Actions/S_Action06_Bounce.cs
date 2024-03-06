using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(S_ActionManager))]
public class S_Action06_Bounce : MonoBehaviour, IMainAction
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
	private S_VolumeTrailRenderer HomingTrailScript;

	private Animator _CharacterAnimator;
	private GameObject jumpBall;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	private float _dropSpeed_;
	[HideInInspector]
	public List<float> _BounceUpSpeeds_;
	private float _bounceUpMaxSpeed_;
	private float _bounceHaltFactor_;
	private float _bounceCoolDown_;
	#endregion

	// Trackers
	#region trackers

	[HideInInspector] public bool BounceAvailable;
	private bool HasBounced;

	private float CurrentBounceAmount;

	private float memoriseSpeed;
	private float nextSpeed;

	private RaycastHit hit;
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
	}
	private void OnDisable () {

	}

	// Update is called once per frame
	void Update () {

	}

	private void FixedUpdate () {
		bool raycasthit = Physics.SphereCast(transform.position, 0.5f, -transform.up, out hit, (_PlayerPhys._speedMagnitude * Time.deltaTime * 0.75f) + _PlayerPhys._negativeGHoverHeight_, _PlayerPhys._Groundmask_);
		//bool raycasthit = Physics.Raycast(transform.position, Vector3.down, out hit, (Player.SpeedMagnitude * Time.deltaTime * 0.95f) + Player.negativeGHoverHeight, Player.Playermask);
		bool groundhit = _PlayerPhys._isGrounded || raycasthit;

		if (nextSpeed > memoriseSpeed / 2)
			nextSpeed /= 1.0005f;


		//End Action
		if (!raycasthit && HasBounced && _PlayerPhys._RB.velocity.y > 4f)
		{

			HasBounced = false;

			float coolDown = _bounceCoolDown_;
			//coolDown -= 0.75f * (int)(Player.HorizontalSpeedMagnitude / 20);
			//coolDown = Mathf.Clamp(coolDown, 3, 6);

			//StartCoroutine(Action.lockBounceOnly(coolDown));
			_Actions.Action00.StartAction();
		}

		else if ((groundhit && !HasBounced) || (!groundhit && _PlayerPhys._RB.velocity.y > _dropSpeed_ * 0.4f && !HasBounced))
		{


			if (true)
			{
				if (_PlayerPhys._isGrounded)
				{
					//Debug.Log("Ground Bounce " + Player.GroundNormal);
					Bounce(_PlayerPhys._groundNormal);
				}
				else if (raycasthit)
				{
					//Debug.Log("RaycastHitBounce " + hit.normal);
					//transform.position = hit.point;
					Bounce(hit.normal);
				}
				else
				{
					Bounce(Vector3.up);
				}
			}
		}
		else if (_PlayerPhys._RB.velocity.y > _dropSpeed_ * 0.8f)
		{
			_PlayerPhys._RB.velocity = new Vector3(_PlayerPhys._RB.velocity.x, -_dropSpeed_, _PlayerPhys._RB.velocity.z);
		}

	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		if (_Input.BouncePressed && _PlayerPhys._RB.velocity.y < 35f)
		{
			InitialEvents();
			_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Bounce);
			willChangeAction = true;
		}
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
		_Input = GetComponent<S_PlayerInput>();
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Actions = GetComponent<S_ActionManager>();

		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
		HomingTrailScript = _Tools.HomingTrailScript;
		jumpBall = _Tools.JumpBall;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_dropSpeed_ = _Tools.Stats.BounceStats.dropSpeed;
		for (int i = 0 ; i < _Tools.Stats.BounceStats.listOfBounceSpeeds.Count ; i++)
		{
			_BounceUpSpeeds_.Add(_Tools.Stats.BounceStats.listOfBounceSpeeds[i]);
		}
		_bounceUpMaxSpeed_ = _Tools.Stats.BounceStats.bounceUpMaxSpeed;
		_bounceCoolDown_ = _Tools.Stats.BounceStats.bounceCoolDown;
		_bounceHaltFactor_ = _Tools.Stats.BounceStats.bounceHaltFactor;
	}
	#endregion



	public void InitialEvents () {
		if (!_Actions.lockBounce)
		{

			HasBounced = false;
			memoriseSpeed = _PlayerPhys._horizontalSpeedMagnitude;
			nextSpeed = memoriseSpeed;


			_Sounds.BounceStartSound();
			BounceAvailable = false;
			_PlayerPhys._RB.velocity = new Vector3(_PlayerPhys._RB.velocity.x * _bounceHaltFactor_, 0f, _PlayerPhys._RB.velocity.z * _bounceHaltFactor_);
			_PlayerPhys.AddCoreVelocity(new Vector3(0, -_dropSpeed_, 0));

			HomingTrailScript.emitTime = -1f;
			HomingTrailScript.emit = true;

			//Set Animator Parameters
			_CharacterAnimator.SetInteger("Action", 1);
			_CharacterAnimator.SetBool("isRolling", false);
			jumpBall.SetActive(true);
		}
	}


	private void Bounce ( Vector3 normal ) {

		_Input.BouncePressed = false;
		_PlayerPhys.SetIsGrounded(false);
		_Actions._isHomingAvailable = true;

		HasBounced = true;
		CurrentBounceAmount = _BounceUpSpeeds_[_Actions._bounceCount];


		CurrentBounceAmount = Mathf.Clamp(CurrentBounceAmount, _BounceUpSpeeds_[_Actions._bounceCount], _bounceUpMaxSpeed_);

		//HomingTrailScript.emitTime = (BounceCount +1) * 0.65f;
		HomingTrailScript.emitTime = CurrentBounceAmount / 60f;

		HomingTrailScript.emit = true;

		Vector3 newVec;


		if (_PlayerPhys._horizontalSpeedMagnitude < 20)
		{
			newVec = _CharacterAnimator.transform.forward;
			newVec *= 20;
		}
		else if (nextSpeed > _PlayerPhys._horizontalSpeedMagnitude)
		{
			newVec = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z).normalized;
			newVec *= nextSpeed;
		}
		else
			newVec = _PlayerPhys._RB.velocity;

		_PlayerPhys._RB.velocity = new Vector3(newVec.x, CurrentBounceAmount, newVec.z);
		_PlayerPhys.AddCoreVelocity(_PlayerPhys._groundNormal);

		_Sounds.BounceImpactSound();

		//Set Animator Parameters
		_CharacterAnimator.SetInteger("Action", 1);
		_CharacterAnimator.SetBool("isRolling", false);
		jumpBall.SetActive(false);

		if (_Actions._bounceCount < _BounceUpSpeeds_.Count - 1)
		{
			_Actions._bounceCount++;
		}

	}


}
