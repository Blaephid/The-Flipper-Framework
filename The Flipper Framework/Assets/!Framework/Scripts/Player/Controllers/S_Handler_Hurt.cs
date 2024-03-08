using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;

[RequireComponent(typeof(S_Action04_Hurt))]
public class S_Handler_Hurt : MonoBehaviour
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
	private S_Manager_LevelProgress _LevelHandler;
	private S_Interaction_Objects _Objects;
	private S_Handler_Camera	_CamHandler;
	private S_Control_PlayerSound _Sounds;
	private S_Handler_CharacterAttacks _Attacks;

	private GameObject		_JumpBall;
	private Animator		_CharacterAnimator;
	private SkinnedMeshRenderer[] _SonicSkins;

	private Transform	_FaceHitCollider;
	private GameObject	_WallToBonk;

	[HideInInspector]
	public Image        _FadeOutImage;

	private GameObject	_MovingRing;
	private GameObject	_ReleaseDirection;
	#endregion

	//General
	#region General Properties

	//Stats - See Stats scriptable objects for tooltips explaining their purpose.
	#region Stats
	private int	_maxRingLoss_;
	private float	_ringReleaseSpeed_;
	private float	_ringArcSpeed_;
	private int	_invincibilityTime_;
	private float	_flickerSpeed_;
	[HideInInspector] public float _damageShakeAmmount_;
	[HideInInspector] public float _enemyHitShakeAmmount_;

	private LayerMask	_BonkWall_;
	#endregion
	// Trackers
	#region trackers

	//Health
	[HideInInspector]
	public int RingAmount;

	//States
	public bool	_isHurt;
	public bool         _isInvincible ;
	public bool	_isDead;
	public bool	HasShield = false;

	//Counters
	private int         _counter;
	private float	_flickerCounter;
	private int	_deadCounter = 0;

	//Release rings on hurt
	private bool	_isReleasingRings = false;
	private int	_ringsToRelease;

	private float       _previousSpeed;
	private Vector3	_previousDirection;
	private Vector3	_initialDirection;
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
		ReadyScript();
	}
	private void OnDisable () {

	}

	// Update is called once per frame
	void Update () {

	}

	private void FixedUpdate () {
		HandleDamaged();

		HandleDeath();

		HandleBonk();
	}

	public void OnTriggerEnter(Collider other) {

		switch (other.tag)
		{
			case "Hazard":
				_JumpBall.SetActive(false);
				if (_Actions.Action08 != null)
				{
					if (_Actions.Action08._DropEffect.isPlaying == true)
					{
						_Actions.Action08._DropEffect.Stop();
					}
				}
				DamagePlayer();
				_CamHandler._HedgeCam.ApplyCameraShake(_damageShakeAmmount_, 60);
				return;

			case "Enemy":
				_CamHandler._HedgeCam.ApplyCameraShake(_enemyHitShakeAmmount_, 30);

				if (!_Attacks.AttemptAttack(other, S_Enums.AttackTargets.Enemy))
				{
					DamagePlayer();
				}
				return;
			case "Pit":
				_Sounds.DieSound();
				_isDead = true;
				return;
		}

		//Debug.Log(Player.HorizontalSpeedMagnitude);
		//Debug.Log(WallToBonk);
		if (other.gameObject == _WallToBonk)
		{

			//Debug.Log("Attempt Bonk");
			if (!Physics.Raycast(transform.position + (_CharacterAnimator.transform.up * 1.5f), _previousDirection, 10f, _BonkWall_) && !_PlayerPhys._isGrounded)
			{
				transform.position = transform.position + (_CharacterAnimator.transform.up * 1.5f);
			}
			else if (!Physics.Raycast(transform.position + (-_CharacterAnimator.transform.up * 1.5f), _previousDirection, 10f, _BonkWall_) && !_PlayerPhys._isGrounded)
			{
				transform.position = transform.position + (-_CharacterAnimator.transform.up * 1.5f);
			}
			else if (_previousSpeed / 1.6f > _PlayerPhys._RB.velocity.sqrMagnitude || !_PlayerPhys._isGrounded)
			{
				StartCoroutine(giveChanceToWallClimb());
			}


		}
	}

	#endregion

	/// <summary>
	/// Private ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region private

	public void DamagePlayer () {
		if (!_isHurt && !_Actions.ActionHurt.enabled)
		{

			if (!HasShield)
			{
				if (RingAmount > 0)
				{
					//LoseRings
					_Sounds.RingLossSound();
					_Actions.Action04Control.GetHurt();
					_Actions.ActionHurt.AttemptAction();
				}
				if (RingAmount <= 0)
				{
					//Die
					if (!_Actions.Action04Control._isDead)
					{
						_Sounds.DieSound();
						//_Actions.Action04Control.isDead = true;
						_Actions.ActionHurt.AttemptAction();
					}
				}
			}
			if (HasShield)
			{
				//Lose Shield
				_Sounds.SpikedSound();
				SetShield(false);
				_Actions.ActionHurt.AttemptAction();
			}
		}
	}

	private void HandleDamaged() {
		_counter += 1;
		if (_counter < _invincibilityTime_)
		{
			_isInvincible = true;
			SkinFlicker();
		}
		else
		{
			_isInvincible = false;
			_isHurt = false;
			//ToggleSkin(true);
		}

		if (_isReleasingRings)
		{
			if (_ringsToRelease > 30) { _ringsToRelease = 30; }
			RingLoss();
		}
	}

	private void HandleDeath() {
		if (_Input.killBindPressed)
		{
			if (_Actions.whatAction != S_Enums.PrimaryPlayerStates.Hurt)
				_CharacterAnimator.SetTrigger("Damaged");
			_isDead = true;
		}

		//IsDead things
		if (_isDead == true)
		{
			Die();
		}
		else if (_counter > 30)
		{
			Color alpha = Color.black;
			alpha.a = 0;
			_FadeOutImage.color = Color.Lerp(_FadeOutImage.color, alpha, 0.5f);
		}
	}

	private void HandleBonk () {
		_FaceHitCollider.transform.rotation = Quaternion.LookRotation(_CharacterAnimator.transform.forward, transform.up); ;

		if ((_Actions.whatAction == 0 && _PlayerPhys._horizontalSpeedMagnitude > 50) || (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Jump && _PlayerPhys._horizontalSpeedMagnitude > 40) || (_Actions.whatAction == S_Enums.PrimaryPlayerStates.JumpDash
		    && _PlayerPhys._horizontalSpeedMagnitude > 30) || (_Actions.whatAction == S_Enums.PrimaryPlayerStates.WallRunning && _Actions.Action12._runningSpeed > 5))
		{
			if (Physics.SphereCast(transform.position, 0.3f, _CharacterAnimator.transform.forward, out RaycastHit tempHit, 10f, _BonkWall_))
			{

				if (Vector3.Dot(_CharacterAnimator.transform.forward, tempHit.normal) < -0.7f)
				{
					_WallToBonk = tempHit.collider.gameObject;
					_previousDirection = _CharacterAnimator.transform.forward;
					return;
				}
			}
		}
		_WallToBonk = null;
		_previousSpeed = _PlayerPhys._RB.velocity.sqrMagnitude;
	}

	IEnumerator giveChanceToWallClimb () {
		Vector3 newDir = _CharacterAnimator.transform.forward;
		if (_Actions.whatAction != S_Enums.PrimaryPlayerStates.WallRunning)
		{
			if (!_PlayerPhys._isGrounded)
			{
				for (int i = 0 ; i < 3 ; i++)
				{
					yield return new WaitForFixedUpdate();
					_PlayerPhys.SetTotalVelocity(Vector3.zero, new Vector2(1, 0));
					_CharacterAnimator.transform.forward = newDir;
				}
			}

			if (_Actions.whatAction != S_Enums.PrimaryPlayerStates.WallRunning)
			{
				_Actions.ActionHurt.InitialEvents(true);
				_Actions.ActionHurt.AttemptAction();
			}
		}
		else if (_Actions.Action12._runningSpeed > 0)
		{
			_Actions.ActionHurt.InitialEvents(true);
			_Actions.ActionHurt.AttemptAction();
		}
	}

	private void Die () {

		RingAmount = 0;

		_JumpBall.SetActive(false);


		_Input.enabled = false;
		_Actions.ActionHurt.AttemptAction();
		_Input._move = Vector3.zero;
		_deadCounter += 1;
		//Debug.Log("DeathGroup");

		if (_deadCounter > 70)
		{
			Color alpha = Color.black;
			alpha.a = 1;
			_FadeOutImage.color = Color.Lerp(_FadeOutImage.color, alpha, 0.51f);
		}
		if (_deadCounter == 120)
		{
			_LevelHandler.RespawnObjects();
		}
		else if (_deadCounter == 170)
		{
			_CharacterAnimator.SetBool("Dead", false);

			if (_LevelHandler.CurrentCheckPoint)
			{
				//Cam.Cam.SetCamera(Level.CurrentCheckPoint.transform.forward, 2,10,10);
			}
			else
			{
				//Cam.Cam.SetCamera(InitialDir, 5);
			}

			_Input.enabled = true;
			_LevelHandler.ResetToCheckPoint();
			//Debug.Log("CallingReset");
			_isDead = false;
			_deadCounter = 0;
			_counter = 0;

			if (_Actions.eventMan != null) _Actions.eventMan.Death();
		}
	}

	private void SkinFlicker () {
		_flickerCounter += _flickerSpeed_;
		if (_flickerCounter < 0)
		{
			
		}
		else
		{
			
		}
		if (_flickerCounter > 10)
		{
			_flickerCounter = -10;
		}
	}

	private void RingLoss () {
		RingAmount = 0;

		if (_ringsToRelease > 0)
		{
			Vector3 pos = transform.position;
			pos.y += 1;
			GameObject movingRing;
			movingRing = Instantiate(_MovingRing, pos, Quaternion.identity);
			movingRing.transform.parent = null;
			movingRing.GetComponent<Rigidbody>().velocity = Vector3.zero;
			movingRing.GetComponent<Rigidbody>().AddForce((_ReleaseDirection.transform.forward * _ringReleaseSpeed_), ForceMode.Acceleration);
			_ReleaseDirection.transform.Rotate(0, _ringArcSpeed_, 0);
			_ringsToRelease -= 1;

			//		Player.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

		}
		else
		{
			_isReleasingRings = false;
			//	Player.GetComponent<Rigidbody> ().freezeRotation = false;
		}
	}

	#endregion

	/// <summary>
	/// Public ----------------------------------------------------------------------------------
	/// </summary>
	/// 
	#region public 

	public void GetHurt () {
		_isHurt = true;
		_counter = 0;

		if (RingAmount > 0 && !_isReleasingRings)
		{
			_ringsToRelease = RingAmount;
			_isReleasingRings = true;
		}
	}

	public void OnTriggerStay ( Collider col ) {
		if (col.tag == "Pit")
		{
			_CamHandler._HedgeCam.SetCameraNoLook(100);
		}
	}

	public void SetShield(bool isOn) {
		HasShield = isOn;
		_Objects.ShieldObject.SetActive(isOn);
	}
	#endregion

	/// <summary>
	/// Assigning ----------------------------------------------------------------------------------
	/// </summary>
	#region Assigning

	//If not assigned already, sets the tools and stats and gets placement in Action Manager's action list.
	public void ReadyScript () {
		if (_PlayerPhys == null)
		{
			//Assign all external values needed for gameplay.
			_Tools = GetComponent<S_CharacterTools>();
			AssignTools();
			AssignStats();

			_initialDirection = transform.forward;
			_counter = _invincibilityTime_;
			_ReleaseDirection = new GameObject();
			_previousDirection = transform.forward;
		}
	}

	//Responsible for assigning objects and components from the tools script.
	private void AssignTools () {
		_PlayerPhys = GetComponent<S_PlayerPhysics>();
		_LevelHandler = GetComponent<S_Manager_LevelProgress>();
		_Actions = GetComponent<S_ActionManager>();
		_Objects = GetComponent<S_Interaction_Objects>();
		_CamHandler = GetComponent<S_Handler_Camera>();
		_Input = GetComponent<S_PlayerInput>();
		_Attacks = GetComponent<S_Handler_CharacterAttacks>();

		_FaceHitCollider = _Tools.faceHit.transform;
		_JumpBall = _Tools.JumpBall;
		_Sounds = _Tools.SoundControl;
		_CharacterAnimator = _Tools.CharacterAnimator;
		_SonicSkins = _Tools.PlayerSkin;
		_MovingRing = _Tools.movingRing;
		_FadeOutImage = _Tools.FadeOutImage;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_invincibilityTime_ = _Tools.Stats.WhenHurt.invincibilityTime;
		_maxRingLoss_ = _Tools.Stats.WhenHurt.maxRingLoss;
		_ringReleaseSpeed_ = _Tools.Stats.WhenHurt.ringReleaseSpeed;
		_ringArcSpeed_ = _Tools.Stats.WhenHurt.ringArcSpeed;
		_flickerSpeed_ = _Tools.Stats.WhenHurt.flickerSpeed;
		_BonkWall_ = _Tools.Stats.WhenBonked.BonkOnWalls;
		_damageShakeAmmount_ = _Tools.Stats.EnemyInteraction.damageShakeAmmount;
		_enemyHitShakeAmmount_ = _Tools.Stats.EnemyInteraction.hitShakeAmmount;
	}
	#endregion
}
