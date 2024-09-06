using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class S_SubAction_Roll : MonoBehaviour, ISubAction
{



	/// <summary>
	/// Properties ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region properties

	//Unity
	#region Unity Specific Properties
	private S_PlayerPhysics       _PlayerPhys;
	private S_PlayerVelocity	_PlayerVel;
	private S_CharacterTools      _Tools;
	private S_PlayerInput         _Input;
	private S_Control_SoundsPlayer _Sounds;
	private S_ActionManager       _Actions;
	private S_Action00_Default    _Action00;

	private CapsuleCollider           _StandingCapsule;
	private CapsuleCollider            _RollingCapsule;
	private Animator              _CharacterAnimator;

	#endregion

	//Stats
	#region Stats
	private float       _rollingStartSpeed_;
	private float       _minRollTime_ = 0.3f;
	#endregion

	// Trackers
	#region trackers
	private S_Enums.PrimaryPlayerStates _whatCurrentAction;

	private bool       _isRollingFromThis;
	[HideInInspector]
	public float        _rollCounter;
	#endregion

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Start is called before the first frame update
	void Awake () {
		if (!_PlayerPhys)
		{
			AssignTools();
			AssignStats();
		}
	}

	// Update is called once per frame
	void Update () {

	}

	private void FixedUpdate () {

		if (_PlayerPhys._isRolling)
		{
			//Cancels rolling if the ground is lost, or the player performs a different Action / Subaction
			if (!_PlayerPhys._isGrounded || (_isRollingFromThis && (_Actions._whatSubAction != S_Enums.SubPlayerStates.Rolling || _whatCurrentAction != _Actions._whatCurrentAction)))
			{
				UnCurl();
			}

			//While isRolling is set externally, the counter tracks when it is.
			else if (_isRollingFromThis)
				_rollCounter += Time.deltaTime;
		}
	}

	//Called when attempting to perform an action, checking and preparing inputs.
	public bool AttemptAction () {
		switch(_Actions._whatCurrentAction)
		{
			//Any action with this on
			default:
				//Must be on the ground to roll
				if (_PlayerPhys._isGrounded)
				{
					//Enter Rolling state, must be moving fast enought first.
					if (_Input._RollPressed && !_isRollingFromThis && _PlayerVel._horizontalSpeedMagnitude > _rollingStartSpeed_)
					{
						_whatCurrentAction = _Actions._whatCurrentAction; //If the current action stops matching this, then the player has switched actions while rolling
						_Actions._whatSubAction = S_Enums.SubPlayerStates.Rolling; //If what subaction changes from this, then the player has stopped rolling.
						
						Curl();
						return true;
					}

					//End rolling state
					if (_isRollingFromThis && !_Input._RollPressed && _rollCounter > _minRollTime_)
					{
						UnCurl();
					}
				}
				break;
		}
		return false;
	}
	public void StartAction ( bool overwrite = false ) {

	}
	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	//When the player starts rolling on the ground, play the sound and set rolling stats.
	public void Curl () {
		if (!_PlayerPhys._isRolling)
		{
			_Sounds.StartRollingSound();
			SetIsRolling(true);
		}
	}

	//When the player wants to stop rolling while on the ground, check if there's enough room to stand up.
	public void UnCurl () {
		if (_PlayerPhys._isRolling && !Physics.BoxCast(_StandingCapsule.transform.position, 
			new Vector3(_StandingCapsule.radius, _StandingCapsule.height / 2.1f, _StandingCapsule.radius), Vector3.zero, transform.rotation, 0))
		{
			SetIsRolling(false);
		}
	}

	//Called whenever IsRolling is to be changed, since multiple things should change whenever this does.
	public void SetIsRolling ( bool value ) {
		//If changing from true to false or vice versa
		if (value != _PlayerPhys._isRolling)
		{
			//Set to rolling from not
			if (value)
			{
				//Make shorter to slide under spaces
				_Actions._ActionDefault.OverWriteCollider(_RollingCapsule);
			}
			//Set to not rolling from was
			else
			{

				//Make taller again to slide under spaces
				_Actions._ActionDefault.OverWriteCollider(_StandingCapsule);
				_rollCounter = 0f;
			}

			_isRollingFromThis = value;
			_PlayerPhys._isRolling = value; //The physics script handles all of the movement differences while rolling.
		}
	}
	#endregion


	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	private void AssignTools () {
		_Tools = GetComponentInParent<S_CharacterTools>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_PlayerVel = _Tools.GetComponent<S_PlayerVelocity>();
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_Sounds = _Tools.SoundControl;
		_Actions = _Tools._ActionManager;
		_Action00 = _Actions._ActionDefault;
		_StandingCapsule = _Tools.StandingCapsule.GetComponent<CapsuleCollider>();
		_RollingCapsule = _Tools.CrouchCapsule.GetComponent<CapsuleCollider>();
		_CharacterAnimator = _Tools.CharacterAnimator;
	}

	private void AssignStats () {
		_minRollTime_ = _Tools.Stats.RollingStats.minRollingTime;
		_rollingStartSpeed_ = _Tools.Stats.RollingStats.rollingStartSpeed;
	}
	#endregion
}
