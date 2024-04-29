using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

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
	private S_Control_SoundsPlayer _Sounds;
	private S_VolumeTrailRenderer _HomingTrailScript;
	private S_Handler_Camera      _CamHandler;

	private Animator    _BallAnimator;
	private Transform   _MainSkin;
	private CapsuleCollider _CharacterCapsule;
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private float _startDropSpeed_;
	private float _maxDropSpeed_;

	[HideInInspector]
	public List<float>	_BounceUpSpeeds_;
	private float	_minimumPushForce_;
	private float       _lerpTowardsInput_;

	private float	_bounceHaltFactor_;
	private Vector2	_horizontalSpeedDecay_;

	private float	_bounceCoolDown_;
	private float	_cooldownModifierBySpeed_;

	private Vector2               _cameraPauseEffect_ = new Vector2(3, 35);
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;         //In every action script, takes note of where in the Action Managers Main action list this script is. 

	private float       _counter;


	[HideInInspector] 
	private bool	_isBounceAvailable;
	private bool	_hasBounced;		//Set to false on start, and true when bounce upwards is performed. Can't exit action unless true.

	private float	_currentBounceForce;

	private float	_memorisedSpeed;		//The speed the player was moving at before starting the bounce.
	private float	_nextSpeed;		//Starts at memorised speed, but will slowly decrease over time, and return the horizontal speed to it when the bounce upwards is performed.

	private float       _trackedVerticalSpeed;	//Used only for the animator. Lerps towards fall speed so transiton isn't instant.

	private RaycastHit	_HitGround;
	#endregion

	#endregion

	/// <summary>
	/// Inherited ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region Inherited

	// Called when the script is enabled, but will only assign the tools and stats on the first time.
	private void OnEnable () {
		ReadyAction();
	}

	// Update is called once per frame
	void Update () {
	}

	private void FixedUpdate () {
		CheckSpeed(); //Called first to make sure state will be changed after any physics changes.
		CheckForGround();

		HandleInputs();
	}
	
	public bool AttemptAction () {

		//Can only bounce if it isn't locked in the actionManager, and not moving too fast up.
		if (_Input.BouncePressed && _PlayerPhys._RB.velocity.y < 35f && _Actions._areAirActionsAvailable && _isBounceAvailable)
		{
			StartAction();
			return true;
		}
		return false;
	}

	public void StartAction () {
		_hasBounced = false; //Tracks when to end the action.

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Bounce); //Called first so stopAction methods in other actions happen before this.
		this.enabled = true;

		_memorisedSpeed = _PlayerPhys._currentRunningSpeed; //Stores the running speed the player was before bouncing.
		_nextSpeed = _memorisedSpeed; //This will decrease as the action goes on, then resetting the player's movement to it after completing the bounce.

		_isBounceAvailable = false; //Can't perform a bounce while in a bounce.
		_trackedVerticalSpeed = _PlayerPhys._coreVelocity.y;

		//Physics
		_PlayerPhys._isGravityOn = false; //Moving down will be handled here rather than through the premade gravity in physics script.
		_PlayerPhys.SetCoreVelocity(new Vector3(_PlayerPhys._RB.velocity.x * _bounceHaltFactor_, 0f, _PlayerPhys._RB.velocity.z * _bounceHaltFactor_), false); //Immediately slows down player movement and removes vertical movement.
		float thisDropSpeed = Mathf.Min(_startDropSpeed_, _PlayerPhys._coreVelocity.y - 20);
		_PlayerPhys.AddCoreVelocity(new Vector3(0,  thisDropSpeed , 0), false); // Apply downward force, this is instant rather than  ramp up like gravity.

		_PlayerPhys._canStickToGround = false; //Prevents the  bounce following the ground direction.

		//Effects
		_Actions._ActionDefault.SwitchSkin(false); //Ball animation rather than character ones.
		_BallAnimator.SetInteger("Action", 1); //Ensures it is set to jump first, because it will then transition from that to bounce.

		StartCoroutine(_CamHandler._HedgeCam.ApplyCameraPause(_cameraPauseEffect_, new Vector2 ( -_PlayerPhys._coreVelocity.y, -thisDropSpeed), 1f)); //The camera will fall back before catching up.

		_Sounds.BounceStartSound();

		_HomingTrailScript.emitTime = -1f; //Makes the trail follow along behind the player
		_HomingTrailScript.emit = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.

		//Apply a cooldown so this can't be performed again immediately.
		float coolDown = _bounceCoolDown_;
		coolDown = Mathf.Clamp(coolDown - (_PlayerPhys._horizontalSpeedMagnitude * _cooldownModifierBySpeed_), 0.05f, coolDown);
		StartCoroutine(AddDelay(coolDown));

		//Incase this was disabled by changing action, rather than a bounce.
		_PlayerPhys._isGravityOn = true;

		_HomingTrailScript.emitTime = 0.2f;
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	public void HandleInputs () {
			//Action Manager goes through all of the potential action this action can enter and checks if they are to be entered
			_Actions.HandleInputs(_positionInActionList);	
	}

	//Searched for ground below the player as they move down, calling any bounces or calculations post bounce.
	private void CheckForGround () {

		//Check if on the ground, either by using the ground check or physics, or a different one based on fall speed with a capsule (to ensure not hitting a corner and not bouncing).
		bool isRaycasthit = Physics.SphereCast(transform.position,_CharacterCapsule.radius, -transform.up, out _HitGround, (_PlayerPhys._coreVelocity.y * Time.deltaTime * 0.8f), _PlayerPhys._Groundmask_);
		bool isGroundHit = _PlayerPhys._isGrounded || isRaycasthit;

		RaycastHit UseHit = isRaycasthit ? _HitGround : _PlayerPhys._HitGround; //Get which one to use

		//If no longer hitting the ground but did earlier, then has bounced and won't be grounded immediately, so end action after a delay.
		if (!isGroundHit && _hasBounced)
		{
			_counter += Time.deltaTime;
			if(_counter > 0.12f)
			{
				_Actions._ActionDefault.StartAction();
			}
		}

		//If there is ground and hasn't bounced yet
		else if (isGroundHit && !_hasBounced)
		{
			Bounce(UseHit.normal);			
		}
	}

	private void CheckSpeed () {

		//Decreases the speed the player will keep when the action ends. Can not lose more than half the original speed.
		if (_nextSpeed > _memorisedSpeed / 2)
			_nextSpeed -= Mathf.Max(_horizontalSpeedDecay_.x, _nextSpeed * _horizontalSpeedDecay_.y);

		if (!_hasBounced)
		{
			//Increases or decreases speed of animation (see ball animator "Bounce").
			_trackedVerticalSpeed = Mathf.Lerp(_trackedVerticalSpeed, _PlayerPhys._coreVelocity.y, 0.05f);
			_BallAnimator.SetFloat("VerticalSpeed", _trackedVerticalSpeed);
			_BallAnimator.SetInteger("Action", 6); //Causes a transition through the jump animation

			float vertSpeed = _PlayerPhys._RB.velocity.y;
			//If fall speed has been decreased this much, then something must be in the way so end the action.
			 if (vertSpeed > 1)
			{
				_Actions._ActionDefault.StartAction();
			}
			 else if(vertSpeed > _maxDropSpeed_)
			{
				_PlayerPhys.AddCoreVelocity(Vector3.down * 2.5f);
			}
		}
	}

	private IEnumerator AddDelay (float s) {
		_isBounceAvailable = false;
		yield return new WaitForSeconds(s);
		_isBounceAvailable = true;
	}

	private void Bounce ( Vector3 normal ) {

		_counter = 0; //Starts the process of waiting after bounce before exiting action.

		_Input.BouncePressed = false; //Prevents the action from being hold spammed.

		_PlayerPhys.SetIsGrounded(true); //Resets air actions like homing attacks, air dashes, etc.

		_hasBounced = true; //Prevents speed being adjusted and allows the action to be exited.

		//Get force of the bounce from how many bounces have been done without resetting, not exceeidng the maxmium.
		_currentBounceForce = Mathf.Clamp(_currentBounceForce, _BounceUpSpeeds_[_Actions._bounceCount], _currentBounceForce);

		//Effects
		_HomingTrailScript.emitTime = _currentBounceForce / 60f;
		_HomingTrailScript.emit = true;
		_Sounds.BounceImpactSound();

		//Set animations back to jump shape, ensuring the player is still in a ball since they'd be set to normal when grounded.
		_BallAnimator.SetInteger("Action", 1);
		_Actions._ActionDefault._animationAction = 1;
		_Actions._ActionDefault.SwitchSkin(false);

		//Physics
		Vector3 newDir= _PlayerPhys._coreVelocity.normalized;
		float newSpeed = _PlayerPhys._horizontalSpeedMagnitude;
		//Player will always be push forwards at least slightly.
		if (_PlayerPhys._horizontalSpeedMagnitude < _minimumPushForce_)
		{
			newDir = _MainSkin.forward;
			newSpeed = _minimumPushForce_;
		}
		//Next speed has been decreasing across the action, but if higher than current speed, set to it.
		else if (_nextSpeed > newSpeed)
		{
			//Gets the local to remove players relevant upwards velocity, then converts back to world for calculations
			newDir = _PlayerPhys.GetRelevantVel(_PlayerPhys._coreVelocity, false).normalized;
			newDir = transform.TransformDirection(newDir);
			newSpeed = _nextSpeed;
		}

		//Starts applying normal force downwards again, even before exiting action.
		_PlayerPhys._isGravityOn = true;

		Vector3 input = transform.TransformDirection(_PlayerPhys._moveInput);

		newDir = Vector3.Lerp(newDir, input, _lerpTowardsInput_);

		//Makes the player's movement relevant to the surface and removes vertical speed.
		Vector3 setVel = _PlayerPhys.AlignWithNormal(newDir, normal, newSpeed);
		_PlayerPhys.SetCoreVelocity(setVel, false);

		//Since vertical speed is removed, add it here, but more facing upwards rather than directly along normal.
		float dif = Vector3.Angle(normal, Vector3.up) * 0.5f;
		normal = Vector3.Lerp(normal, Vector3.up, dif * Mathf.Deg2Rad);
		_PlayerPhys.AddCoreVelocity(normal * _currentBounceForce, false);

		Debug.DrawRay(transform.position, normal * _currentBounceForce, Color.magenta, 20f);

		//If not at the last index, increase the bounce count.
		if (_Actions._bounceCount < _BounceUpSpeeds_.Count - 1)
		{
			_Actions._bounceCount++;
		}
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 
	//This has to be set up in Editor. The invoker is in the PlayerPhysics script component, adding this event to it will mean this is called whenever the player lands.
	public void EventOnGrounded () {
		_isBounceAvailable = true;

		if(!enabled)_Actions._bounceCount = 0; //Only reset bouncing if hit the ground in a different state, since every bounce grounds the player.
	}
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
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Bounce)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Input =	_Tools.GetComponent<S_PlayerInput>();
		_PlayerPhys =	_Tools.GetComponent<S_PlayerPhysics>();
		_Actions =	_Tools._ActionManager;

		_MainSkin =		_Tools.MainSkin;
		_Sounds =			_Tools.SoundControl;
		_HomingTrailScript =	_Tools.HomingTrailScript;
		_BallAnimator =		_Tools.BallAnimator;
		_CharacterCapsule =		_Tools.CharacterCapsule.GetComponent<CapsuleCollider>();
		_CamHandler =		_Tools.CamHandler;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_startDropSpeed_ = _Tools.Stats.BounceStats.startDropSpeed;
		_maxDropSpeed_ = _Tools.Stats.BounceStats.maxDropSpeed;

		for (int i = 0 ; i < _Tools.Stats.BounceStats.listOfBounceSpeeds.Count ; i++)
		{
			_BounceUpSpeeds_.Add(_Tools.Stats.BounceStats.listOfBounceSpeeds[i]);
		}
		_minimumPushForce_ =	_Tools.Stats.BounceStats.minimumPushForce;
		_lerpTowardsInput_ =	_Tools.Stats.BounceStats.lerpTowardsInput;

		_bounceCoolDown_ =		_Tools.Stats.BounceStats.bounceCoolDown;
		_cooldownModifierBySpeed_ =	_Tools.Stats.BounceStats.coolDownModiferBySpeed;

		_bounceHaltFactor_ =	_Tools.Stats.BounceStats.bounceHaltFactor;
		_horizontalSpeedDecay_ =	_Tools.Stats.BounceStats.horizontalSpeedDecay;

		_cameraPauseEffect_ =	_Tools.Stats.BounceStats.cameraPauseEffect;
	}
	#endregion

}
