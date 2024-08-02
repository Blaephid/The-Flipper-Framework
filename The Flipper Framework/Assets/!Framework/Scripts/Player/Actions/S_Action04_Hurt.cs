using UnityEngine;
using System.Collections;
using UnityEditor;

public class S_Action04_Hurt : MonoBehaviour, IMainAction
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
	private S_Control_SoundsPlayer	_Sounds;
	private S_Handler_HealthAndHurt	_HurtControl;

	private Transform             _MainSkin;
	private CapsuleCollider       _CharacterCapsule;
	private GameObject            _JumpBall;
	private Animator		_CharacterAnimator;
	#endregion


	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	LayerMask           _RecoilFrom_;
	[HideInInspector]
	public float	_knockbackUpwardsForce_ = 10;

	[HideInInspector]
	public float	_knockbackForce_ = 10;

	private float	_bonkBackForce_;
	private float	_bonkUpForce_;

	private Vector2	_controlLockGround_ ;
	private Vector2	_controlLockAir_ ;

	private float	_bonkLock_ ;
	private float	_bonkLockAir_ ;

	private int         _stateLengthWithKnockback_;
	private int         _stateLengthWithoutKnockback_;
	private int         _bonkLength_;
	#endregion

	// Trackers
	#region trackers
	private int         _positionInActionList;        //In every action script, takes note of where in the Action Managers Main action list this script is. 

	private float       _lockInStateFor;		//When the action starts, set how long should be in it for.
	private int	_counter;			//Tracks how long the state has been active for.
	private int         _keepLockingControlUntil;     //How long to lose control for, a lot of overlap with lock in state for
	private Vector3     _lockInputToThis;		//When control is lost, input will be stuck as this. What it is depends on the knockback.

	[HideInInspector]
	public Vector3      _knockbackDirection;          //Set externally when the action starts. The direction to be flung, if it's zero it means there should be no knockback.
	private Vector3      _faceDirection;		//Set on start action, locks the character to face this direction.
	[HideInInspector]
	public bool         _wasHit;			//Set externally when the action starts, determines if this is a harmless bonk or damage was taken.
	private bool        _isEndingAction;		//Set false at start, set true when it ends, allowing a one frame delay to check this the next frame before actually ending action.
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
		if(_faceDirection == Vector3.zero)
		{
			_Actions._ActionDefault.SetSkinRotationToVelocity(5);
		}
	}

	private void FixedUpdate () {
		_counter += 1;

		LockControl();
		TrackEndAction();
		AffectMovement();
		HandleInputs();
	}

	public bool AttemptAction () {
		return false;
	}

	public void StartAction () {
		//Effects
		_JumpBall.SetActive(false);
		_Sounds.PainVoicePlay();

		//Animator
		_Actions._ActionDefault.SwitchSkin(true);
		_CharacterAnimator.SetTrigger("ChangedState");
		_CharacterAnimator.SetInteger("Action", 4);

		//Set private
		_isEndingAction = false;
		float lockControlFor;

		//For checking for a wall. 
		Vector3 boxSize = new Vector3(_CharacterCapsule.radius, _CharacterCapsule.height, _CharacterCapsule.radius); //Based on player collider size
		float checkDistance = _PlayerPhys._previousHorizontalSpeeds[3] * Time.deltaTime * 3; //Direction and speed are obtained from previous frames because there has now been a collision that may have affected them this frame.
		Vector3 checkDirection = _PlayerPhys._previousVelocities[3].normalized;
		
		//If going to keep moving in direction
		//Knockback direction will have been set to zero in the hurt handler if not resetting speed on hit. If there isn't a solid object infront, then dont bounce back.
		if (_knockbackDirection == Vector3.zero && !Physics.BoxCast(transform.position, boxSize, checkDirection, transform.rotation, checkDistance, _RecoilFrom_))
		{
			//Apply slight force against and upwards.
			_PlayerPhys.AddCoreVelocity(-_PlayerPhys._RB.velocity.normalized * _knockbackForce_ * 0.2f);
			_PlayerPhys.AddCoreVelocity(transform.up * _knockbackUpwardsForce_);

			lockControlFor = _PlayerPhys._isGrounded ? _controlLockGround_.x : _controlLockAir_.x;
			_lockInStateFor = _stateLengthWithoutKnockback_;

			_HurtControl._wasHurtWithoutKnockback = true;

			_faceDirection = Vector3.zero;

			_lockInputToThis = _MainSkin.forward;
		}
		//If being knocked back and Speed should be reset.
		else
		{
			Vector3 movePlacement = -_PlayerPhys._previousVelocities[3] * Time.deltaTime * 0.5f;
			movePlacement += transform.up;
			_PlayerPhys.SetPlayerPosition(transform.position + movePlacement); //Places character back the way they were moving to avoid weird collisions.

			//Get a new direction if this was triggered because something was blocking the previous option
			_knockbackDirection = _knockbackDirection == Vector3.zero ? -checkDirection : _knockbackDirection;
			_HurtControl._wasHurtWithoutKnockback = false;

			//Gets the values to use, then edit if was not hit by an attack.
			float force = _knockbackForce_;
			float upForce = _knockbackUpwardsForce_;
			lockControlFor = _PlayerPhys._isGrounded ? _controlLockGround_.y: _controlLockAir_.y;
			_lockInStateFor = Mathf.Max(_stateLengthWithKnockback_, lockControlFor);

			//If was hit is false, then this was action was trigged by something not meant to be an attack, so apply bonk stats rather than damage response stats.
			if (!_wasHit)
			{
				force = _bonkBackForce_;
				upForce = _bonkUpForce_;
				lockControlFor = _PlayerPhys._isGrounded ? _bonkLock_ : _bonkLockAir_;
				_lockInStateFor = Mathf.Max(_bonkLength_, lockControlFor);
			}

			//Increase upwards force if grounded so the player properly leaves it.
			if (_PlayerPhys._isGrounded) { upForce *= 1.25f; }

			//Make direction local to player rotation so we can change the y and xz values seperately.
			Vector3 newSpeed = _PlayerPhys.GetRelevantVel(_knockbackDirection);
			newSpeed.y = 0;
			newSpeed.Normalize(); //Get the horizontal direction local to player rotation
			newSpeed *= force;

			//If back is facing upwards, then player was hit while running down, so extra velocity should be added upwards to properly bounce off slope was running on.
			if (Vector3.Angle(-_MainSkin.forward, Vector3.up) < 30)
			{
				upForce *= 2;
				//Ensures the player will face away from the wall
				if (_PlayerPhys._isGrounded) { _MainSkin.forward = _PlayerPhys._groundNormal; }
				else { _MainSkin.forward = transform.up; }
			}
			newSpeed.y = upForce; //Apply force towards players upwards

			//Now represent as velocity in world space and apply
			newSpeed = transform.TransformDirection(newSpeed);

			_faceDirection = -newSpeed;

			_PlayerPhys.SetCoreVelocity(Vector3.zero, "Overwrite");
			_PlayerPhys.SetEnvironmentalVelocity(newSpeed, true, true);

			_lockInputToThis = Vector3.zero; //Locks input as nothing being input, preventing skidding against the knockback until unlocked.
		}
		_keepLockingControlUntil = (int)lockControlFor;

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Hurt);
		this.enabled = true;
	}

	public void StopAction ( bool isFirstTime = false ) {
		if (!enabled) { return; } //If already disabled, return as nothing needs to change.
		enabled = false;
		if (isFirstTime) { return; } //If first time, then return after setting to disabled.
		_counter = 0;
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

	private void LockControl() {
		//Since the lock input here may be interupted, keep setting to lock for one frame until this is up.
		if (_counter <= _keepLockingControlUntil)
		{
			_Input.LockInputForAWhile(1, false,  _lockInputToThis);
			StartCoroutine(_PlayerPhys.LockFunctionForTime(S_PlayerPhysics.EnumControlLimitations.canControl, 0, 1));
		}
	}

	private void AffectMovement() {
		//If given feedback and doesn't have control right now.
		if ((!_HurtControl._wasHurtWithoutKnockback || _HurtControl._isDead) && _PlayerPhys._listOfCanControl.Count > 0)
		{
			//If on the ground, use the decelerate method (which is currently disabled normally) to decrease horizontal movement.
			if (_PlayerPhys._isGrounded && _counter > 10)
			{
				//Get local horizontal vector
				Vector3 newVelocity = _PlayerPhys.GetRelevantVel(_PlayerPhys._coreVelocity);
				float keepY = newVelocity.y;
				newVelocity.y = 0;

				//Decrease speed
				newVelocity = _PlayerPhys.Decelerate(newVelocity, Vector3.zero, 1.2f);

				//Return vertical velocity and interpret as world space again
				newVelocity.y = keepY;
				newVelocity = transform.TransformDirection(newVelocity);

				_PlayerPhys.SetCoreVelocity(newVelocity);
			}
		}
	}

	private void TrackEndAction () {
		//This will happen on the frame after the next if statement happens. This is to add a one frame delay for the animator to properly switch.
		if (_isEndingAction)
		{
			_Actions._ActionDefault.StartAction();
		}
		//How long to be performing this action. When the counter is up, return to the default state. But if input is still locked or player is dead, don't change.
		else if (_counter > _lockInStateFor && !_Input._isInputLocked && !_HurtControl._isDead)
		{
			_isEndingAction = true;
			_CharacterAnimator.SetInteger("Action", 0);
			_CharacterAnimator.SetBool("Dead", false);
			_CharacterAnimator.SetFloat("GroundSpeed", 0);
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
		if (enabled)
		{
			//The frontiers response element means health isn't checked until hitting the ground after being hit.
			if (_HurtControl._inHurtStateBeforeDamage)
			{
				_HurtControl._inHurtStateBeforeDamage = false;
				_HurtControl.CheckHealth();
			}
			//The normal response ends the action as soon as landed to get back into the fray
			if (_HurtControl._wasHurtWithoutKnockback && !_HurtControl._isDead)
			{
				_Actions._ActionDefault.StartAction();
			}
			//If meant to hit the ground heavily and fall over
			else
			{
				if (_wasHit)
				{
					//On landing, if took damage greatly decrease time in state, with a minimum time to allow grounded animation. 
					_lockInStateFor = Mathf.Max(_lockInStateFor / 1.8f, 70);
					_keepLockingControlUntil = (int) Mathf.Max(_keepLockingControlUntil / 1.8f, 80);
					_CharacterAnimator.SetBool("Dead", true);
				}
				else
				{
					//If from a bonk, decrease lock control all the way.
					_lockInStateFor = 0;
					_keepLockingControlUntil = 0;
				}
			}
		}
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
				if (_Actions._MainActions[i].State == S_Enums.PrimaryPlayerStates.Hurt)
				{
					_positionInActionList = i;
					break;
				}
			}
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Input = _Tools.GetComponent<S_PlayerInput>();
		_PlayerPhys = _Tools.GetComponent<S_PlayerPhysics>();
		_Actions = _Tools._ActionManager;
		_HurtControl = GetComponentInParent<S_Handler_HealthAndHurt>();

		_CharacterCapsule = _Tools.CharacterCapsule.GetComponent<CapsuleCollider>();
		_JumpBall = _Tools.JumpBall;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
		_MainSkin = _Tools.MainSkin;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_knockbackForce_ = _Tools.Stats.KnockbackStats.knockbackForce;
		_knockbackUpwardsForce_ = _Tools.Stats.KnockbackStats.knockbackUpwardsForce;
		_RecoilFrom_ = _Tools.Stats.KnockbackStats.recoilFrom;

		_bonkBackForce_ = _Tools.Stats.WhenBonked.bonkBackwardsForce;
		_bonkUpForce_ = _Tools.Stats.WhenBonked.bonkUpwardsForce;

		_controlLockAir_ = _Tools.Stats.KnockbackStats.hurtControlLockAir;
		_controlLockGround_ = _Tools.Stats.KnockbackStats.hurtControlLock;
		_bonkLock_ = _Tools.Stats.WhenBonked.bonkControlLock;
		_bonkLockAir_ = _Tools.Stats.WhenBonked.bonkControlLockAir;

		_stateLengthWithKnockback_ = _Tools.Stats.KnockbackStats.stateLengthWithKnockback;
		_stateLengthWithoutKnockback_ = _Tools.Stats.KnockbackStats.stateLengthWithoutKnockback;
		_bonkLength_ = _Tools.Stats.WhenBonked.bonkTime;
	}
	#endregion
}
