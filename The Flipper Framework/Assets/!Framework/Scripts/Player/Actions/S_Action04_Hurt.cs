using UnityEngine;
using System.Collections;

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
	S_Control_PlayerSound	_Sounds;

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
	public bool	_resetSpeedOnHit_ = false;
	[HideInInspector]
	public float	_knockbackForce_ = 10;

	private float	_bonkBackForce_;
	private float	_bonkUpForce_;

	private float	_recoilGround_ ;
	private float	_recoilAir_ ;

	private float	_bonkLock_ ;
	private float	_bonkLockAir_ ;
	#endregion

	// Trackers
	#region trackers
	private float	_lockedForInGround;
	private float	_lockedForInAir;
	private int	_counter;
	[HideInInspector]
	public float        _deadCounter;
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
		//Set Animator Parameters
		_CharacterAnimator.SetInteger("Action", 4);

		//Dead
		if (_Actions.Action04Control.isDead)
		{
			_deadCounter += Time.deltaTime;
			if (_PlayerPhys._isGrounded && _deadCounter > 0.3f)
			{
				_CharacterAnimator.SetBool("Dead", true);
			}
		}
	}

	private void FixedUpdate () {
		//Get out of Action
		_counter += 1;

		if ((_PlayerPhys._isGrounded && _counter > _lockedForInGround) || _counter > _lockedForInAir)
		{
			if (!_Actions.Action04Control.isDead)
			{

				_Actions.Action02Control._isHomingAvailable = true;
				_Actions.Action01._jumpCount = 0;

				_CharacterAnimator.SetInteger("Action", 0);
				_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Default);
				//Debug.Log("What");
				_counter = 0;
			}
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
		_Input = GetComponent<S_PlayerInput>();
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_Actions = GetComponent<S_ActionManager>();
		_CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_knockbackForce_ = _Tools.Stats.WhenHurt.knockbackForce;
		_knockbackUpwardsForce_ = _Tools.Stats.WhenHurt.knockbackUpwardsForce;
		_resetSpeedOnHit_ = _Tools.Stats.WhenHurt.shouldResetSpeedOnHit;
		_RecoilFrom_ = _Tools.Stats.WhenHurt.recoilFrom;

		_bonkBackForce_ = _Tools.Stats.WhenBonked.bonkBackwardsForce;
		_bonkUpForce_ = _Tools.Stats.WhenBonked.bonkUpwardsForce;

		_recoilAir_ = _Tools.Stats.WhenHurt.hurtControlLockAir;
		_recoilGround_ = _Tools.Stats.WhenHurt.hurtControlLock;
		_bonkLock_ = _Tools.Stats.WhenBonked.bonkControlLock;
		_bonkLockAir_ = _Tools.Stats.WhenBonked.bonkControlLockAir;
	}
	#endregion

	void Awake () {
		if (_PlayerPhys == null)
		{
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();
		}

	}

	public void InitialEvents ( bool bonk = false ) {
		_Tools.JumpBall.SetActive(false);
		_CharacterAnimator.SetInteger("Action", 4);
		_CharacterAnimator.SetTrigger("Damaged");

		//Change Velocity
		if (bonk)
		{
			Vector3 newSpeed = -_CharacterAnimator.transform.forward * _bonkBackForce_;
			newSpeed.y = _bonkUpForce_;
			if (_PlayerPhys._isGrounded)
				newSpeed.y *= 2;
			//Player._RB.velocity = newSpeed;
			_PlayerPhys.SetTotalVelocity(newSpeed);

			_lockedForInAir = _bonkLockAir_;
			_lockedForInGround = _bonkLock_;
			_Sounds.PainVoicePlay();
		}
		else if (!_resetSpeedOnHit_ && !Physics.Raycast(transform.position, _CharacterAnimator.transform.forward, 6, _RecoilFrom_))
		{
			Vector3 newSpeed = new Vector3((_PlayerPhys._RB.velocity.x / 2), _knockbackUpwardsForce_, (_PlayerPhys._RB.velocity.z / 2));
			newSpeed.y = _knockbackUpwardsForce_;
			//Player._RB.velocity = newSpeed;
			_PlayerPhys.SetTotalVelocity(newSpeed);
			_lockedForInAir = _recoilAir_;
			_lockedForInGround = _recoilGround_;
		}
		else
		{
			Vector3 newSpeed = -_CharacterAnimator.transform.forward * _knockbackForce_;
			newSpeed.y = _knockbackUpwardsForce_;
			//Player._RB.velocity = newSpeed;
			_PlayerPhys.SetTotalVelocity(newSpeed);
			_lockedForInAir = _recoilAir_ * 1.4f;
			_lockedForInGround = _recoilGround_ * 1.4f;
		}

		_Input.LockInputForAWhile(_lockedForInGround * 0.85f, false);

	}
}
