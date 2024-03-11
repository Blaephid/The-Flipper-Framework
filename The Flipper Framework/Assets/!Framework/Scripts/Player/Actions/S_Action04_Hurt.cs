using UnityEngine;
using System.Collections;

[RequireComponent(typeof(S_Handler_Hurt))]
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
	private S_Control_PlayerSound	_Sounds;
	private S_Handler_Hurt	_HurtControl;

	private CapsuleCollider       _CharacterCapsule;
	private GameObject            _JumpBall;
	private Animator		_CharacterAnimator;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	LayerMask		_RecoilFrom_;
	[HideInInspector]
	public float	_knockbackUpwardsForce_ = 10;

	[HideInInspector]
	public float	_knockbackForce_ = 10;

	private float	_bonkBackForce_;
	private float	_bonkUpForce_;

	private float	_recoilGround_ ;
	private float	_recoilAir_ ;

	private float	_bonkLock_ ;
	private float	_bonkLockAir_ ;

	private int         _stateLengthWithKnockback_;
	private int         _stateLengthWithoutKnockback_;
	private int         _bonkLength_;
	#endregion

	// Trackers
	#region trackers
	private float       _lockInStateFor;
	private int	_counter;
	[HideInInspector]
	public float        _deadCounter;

	[HideInInspector]
	public Vector3      _knockbackDirection;
	[HideInInspector]
	public bool         _wasHit;
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
		ReadyAction();
	}
	private void OnDisable () {

	}

	// Update is called once per frame
	void Update () {
		//Set Animator Parameters
		_Actions.ActionDefault.HandleAnimator(4);
	}

	private void FixedUpdate () {
		_counter += 1;

		//How long to be performing this action. When the counter is up, return to the default state.
		if (_counter > _lockInStateFor)
		{
			if (!_Actions.Action04Control._isDead)
			{
				_Actions.ActionDefault.StartAction();
			}
		}
	}

	public bool AttemptAction () {
		return false;
	}

	public void StartAction () {
		//Effects
		_JumpBall.SetActive(false);
		_Sounds.PainVoicePlay();

		//Animator
		_CharacterAnimator.SetInteger("Action", 4);
		_CharacterAnimator.SetTrigger("ChangedState");

		//Set public

		float lockControlFor;

		//For checking for a wall. 
		Vector3 boxSize = new Vector3(_CharacterCapsule.radius, _CharacterCapsule.height, _CharacterCapsule.radius); //Based on player collider size
		float checkDistance = _PlayerPhys._previousHorizontalSpeed[1] * Time.deltaTime * 3; //Direction and speed are obtained from previous frames because there has now been a collision that may have affected them this frame.
		Vector3 checkDirection = _PlayerPhys._prevTotalVelocities[1].normalized;

		Debug.DrawRay(transform.position, checkDirection * checkDistance, Color.blue, 20f);

		//Knockback direction will have been set to zero in the hurt handler if not resetting speed on hit. If there isn't a solid object infront, the dont bounce back.
		if (_knockbackDirection == Vector3.zero && !Physics.BoxCast(transform.position, boxSize, checkDirection, transform.rotation, checkDistance, _RecoilFrom_))
		{
			//Apply slight force against and upwards.
			_PlayerPhys.AddCoreVelocity(-_PlayerPhys._RB.velocity.normalized * _knockbackForce_ * 0.2f);
			_PlayerPhys.AddCoreVelocity(transform.up * _knockbackUpwardsForce_);

			lockControlFor = _PlayerPhys._isGrounded ? _recoilGround_ : _recoilAir_;
			_lockInStateFor = _stateLengthWithoutKnockback_;

			_HurtControl._wasHurtWithoutKnockback = true;
		}
		//Speed should be reset.
		else
		{
			transform.position -= checkDirection * 2; //Places character back the way they were moving to avoid weird collisions.

			//Get a new direction if this was triggered because something was blocking the previous option
			_knockbackDirection = _knockbackDirection == Vector3.zero ? -checkDirection : _knockbackDirection;
			_HurtControl._wasHurtWithoutKnockback = false;

			//Gets the values to use, then edit if was not hit by an attack.
			float force = _knockbackForce_;
			float upForce = _knockbackUpwardsForce_;
			lockControlFor = _PlayerPhys._isGrounded ? _recoilGround_ * 1.5f: _recoilAir_ * 1.5f;
			_lockInStateFor = _stateLengthWithKnockback_;


			//If was hit is false, then this was action was trigged by something not meant to be an attack, so apply bonk stats rather than damage response stats.
			if (!_wasHit)
				{
				force = _bonkBackForce_;
				upForce = _bonkUpForce_;
				lockControlFor = _PlayerPhys._isGrounded ? _bonkLock_ : _bonkLockAir_;
				_lockInStateFor = _bonkLength_;
			}
			//Increase upwards force if grounded so the player properly leaves it.
			if (_PlayerPhys._isGrounded) { upForce *= 1.25f; }

			//Make direction local to player rotation so we can change the y and xz values seperately.
			Vector3 newSpeed = _PlayerPhys.GetRelevantVel(_knockbackDirection);
			newSpeed.y = 0;
			newSpeed.Normalize(); //Get the horizontal direction local to player rotation
			newSpeed *= force;
			newSpeed.y = upForce; //Apply force towards players upwards

			//Now represent as velocity in world space and apply
			newSpeed = transform.TransformDirection(newSpeed);
			_PlayerPhys.SetTotalVelocity(newSpeed, new Vector2(1f, 0f));

			Debug.Log(newSpeed + "  -  " +newSpeed.magnitude);
			StartCoroutine(_PlayerPhys.LockFunctionForTime(_PlayerPhys._listOfCanControl, _lockInStateFor / 55));
		}

		_Input.LockInputForAWhile(lockControlFor, false);
		_Input._move = Vector3.zero; //Locks input as nothing being input, preventing skidding against the knockback until unlocked.

		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Hurt);
	}

	public void StopAction () {
		if (enabled) enabled = false;
		else return;

		_counter = 0;
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

	//This has to be set up in Editor. The invoker is in the PlayerPhysics script component, adding this event to it will mean this is called whenever the player lands.
	public void EventOnGrounded() {
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
				_Actions.ActionDefault.StartAction();
			}
			else
			{
				//On landing, greatly decrease time in state; 
				_lockInStateFor /= 2;
			}
		}
	}

	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//If not assigned already, sets the tools and stats.
	public void ReadyAction () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_Input = GetComponent<S_PlayerInput>();
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Actions = GetComponent<S_ActionManager>();
		_HurtControl = GetComponent<S_Handler_Hurt>();

		_CharacterCapsule = _Tools.characterCapsule.GetComponent<CapsuleCollider>();
		_JumpBall = _Tools.JumpBall;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_knockbackForce_ = _Tools.Stats.KnockbackStats.knockbackForce;
		_knockbackUpwardsForce_ = _Tools.Stats.KnockbackStats.knockbackUpwardsForce;
		_RecoilFrom_ = _Tools.Stats.KnockbackStats.recoilFrom;

		_bonkBackForce_ = _Tools.Stats.WhenBonked.bonkBackwardsForce;
		_bonkUpForce_ = _Tools.Stats.WhenBonked.bonkUpwardsForce;

		_recoilAir_ = _Tools.Stats.KnockbackStats.hurtControlLockAir;
		_recoilGround_ = _Tools.Stats.KnockbackStats.hurtControlLock;
		_bonkLock_ = _Tools.Stats.WhenBonked.bonkControlLock;
		_bonkLockAir_ = _Tools.Stats.WhenBonked.bonkControlLockAir;

		_stateLengthWithKnockback_ = _Tools.Stats.KnockbackStats.stateLengthWithKnockback;
		_stateLengthWithoutKnockback_ = _Tools.Stats.KnockbackStats.stateLengthWithoutKnockback;
		_bonkLength_ = _Tools.Stats.WhenBonked.bonkTime;
	}
	#endregion
}
