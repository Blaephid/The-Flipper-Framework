using UnityEngine;
using System.Collections;
using UnityEditor;

[RequireComponent(typeof(S_ActionManager))]
public class S_Action08_DropCharge : MonoBehaviour, IMainAction
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
	private S_Handler_Camera _CamHandler;
	private S_Control_PlayerSound _Sounds;

	private Animator CharacterAnimator;
	private Transform feetPoint;
	[HideInInspector]
	public ParticleSystem _DropEffect;
	private GameObject JumpBall;
	#endregion

	//General
	#region General Properties

	//Stats
	#region Stats
	[HideInInspector]
	public float        _spinDashChargingSpeed_ = 0.3f;
	[HideInInspector]
	public float        _minimunCharge_ = 10;
	[HideInInspector]
	public float        _maximunCharge_ = 100;
	#endregion

	// Trackers
	#region trackers

	SkinnedMeshRenderer[] PlayerSkin;
	private bool        _isCharging = true;

	[HideInInspector]
	public float charge;
	public float BallAnimationSpeedMultiplier;
	public float SpinDashChargedEffectAmm;
	bool isSpinDashing;
	private Vector3 RawPrevInput;
	private Quaternion CharRot;
	private RaycastHit floorHit;
	private Vector3 newForward;

	public float ReleaseShakeAmmount;
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
		_DropEffect.Stop();
	}

	// Update is called once per frame
	void Update () {
		//Set Animator Parameters
		CharacterAnimator.SetInteger("Action", 1);
		CharacterAnimator.SetFloat("GroundSpeed", 100);

		//Check if rolling
		//if (Player.Grounded && Player.isRolling) { CharacterAnimator.SetInteger("Action", 1); }
		//CharacterAnimator.SetBool("isRolling", Player.isRolling);

		//Rotation

		//Set Animation Angle
		//if (!Player.Grounded)
		{
			Vector3 VelocityMod = new Vector3(_PlayerPhys._RB.velocity.x, 0, _PlayerPhys._RB.velocity.z);
			if (VelocityMod != Vector3.zero)
			{
				Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
				CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * 200);
			}
		}

		if (_PlayerPhys._isGrounded && _DropEffect.isPlaying)
		{
			_DropEffect.Stop();
		}
	}

	private void FixedUpdate () {
		if (_isCharging)
		{
			charge += _spinDashChargingSpeed_;

			//Lock camera on behind
			// Cam.Cam.FollowDirection(3, 14f, -10,0);


			if (_DropEffect.isPlaying == false)
			{
				_DropEffect.Play();
			}

			// Player.rigidbody.velocity /= SpinDashStillForce;

			if (!_Input.RollPressed)
			{
				//if (DropEffect.isPlaying == true)
				//{
				//    DropEffect.Stop();
				//}

				_Sounds.Source2.Stop();


				_isCharging = false;
				StartCoroutine(exitAction());
			}

			if (charge > _maximunCharge_)
			{
				charge = _maximunCharge_;
			}
		}
		else
		{
			if (_Input.RollPressed)
			{
				_isCharging = true;
				StopCoroutine(exitAction());
			}
		}

		if (Physics.Raycast(feetPoint.position, -transform.up, out floorHit, 1.3f, _PlayerPhys._Groundmask_) || Vector3.Dot(_PlayerPhys._groundNormal, Vector3.up) > 0.99)
		{

			if (!_Input.JumpPressed)
				Release();
			else
			{
				_Actions.Action00.SetIsRolling(true);
				JumpBall.SetActive(false);
				if (_DropEffect.isPlaying == true)
				{
					_DropEffect.Stop();
				}
			}

			_Input.JumpPressed = false;
			JumpBall.SetActive(false);
			_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Default);
		}

		else if (_Input.SpecialPressed && charge > _minimunCharge_)
		{
			AirRelease();
		}
	}

	public bool AttemptAction () {
		bool willChangeAction = false;
		if (!_PlayerPhys._isGrounded && _Input.RollPressed)
		{
			_Actions.Action08.TryDropCharge();
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
		_CamHandler = GetComponent<S_Handler_Camera>();

		CharacterAnimator = _Tools.CharacterAnimator;
		_Sounds = _Tools.SoundControl;
		_DropEffect = _Tools.DropEffect;

		feetPoint = _Tools.FeetPoint;
		JumpBall = _Tools.JumpBall;
		PlayerSkin = _Tools.PlayerSkin;
	}

	//Reponsible for assigning stats from the stats script.
	private void AssignStats () {
		_spinDashChargingSpeed_ = _Tools.Stats.DropChargeStats.chargingSpeed;
		_minimunCharge_ = _Tools.Stats.DropChargeStats.minimunCharge;
		_maximunCharge_ = _Tools.Stats.DropChargeStats.maximunCharge;
	}
	#endregion


	public void TryDropCharge () {
		if (_PlayerPhys._RB.velocity.y < 40f && _Actions.Action08 != null)
		{
			//Debug.Log("Enter DropDash");
			_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.DropCharge);

			_Actions.Action08.InitialEvents();
		}
	}

	public void InitialEvents ( float newCharge = 15 ) {
		////Debug.Log ("startDropDash");
		CharacterAnimator.SetInteger("Action", 1);
		CharacterAnimator.SetBool("Grounded", false);

		_Sounds.SpinDashSound();
		charge = newCharge;
		_isCharging = true;
	}

	IEnumerator exitAction () {
		yield return new WaitForSeconds(0.7f);
		if (_DropEffect.isPlaying == true)
		{
			_DropEffect.Stop();
		}
		if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.DropCharge)
		{
			JumpBall.SetActive(true);
			_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Jump);
		}
	}

	void AirRelease () {

		_Input.JumpPressed = false;
		_Input.SpecialPressed = false;
		_Input.HomingPressed = false;
		charge *= 0.6f;

		StartCoroutine(airDash());

		Release();
		if (GetComponent<S_Action11_JumpDash>() != null)
			GetComponent<S_Action11_JumpDash>().AirDashParticle();
		_isCharging = false;
	}

	IEnumerator airDash () {
		float time = Mathf.Round(charge / 30);
		time /= 10;

		Debug.Log(time);

		_PlayerPhys._isGravityOn = false;
		yield return new WaitForSeconds(time);
		_PlayerPhys._isGravityOn = true;
		JumpBall.SetActive(false);
		_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Jump);

	}

	void Release () {

		if (_Actions.eventMan != null) _Actions.eventMan.dropChargesPerformed += 1;

		JumpBall.SetActive(false);

		//Vector3 newForward = Player.rb.velocity - transform.up * Vector3.Dot(Player.rb.velocity, transform.up);

		//if (newForward.magnitude < 0.1f)
		//{
		//    newForward = CharacterAnimator.transform.forward;
		//}

		//CharRot = Quaternion.LookRotation(newForward, transform.up);
		//CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * 200);

		newForward = Vector3.ProjectOnPlane(CharacterAnimator.transform.forward, floorHit.normal);

		if (charge < _minimunCharge_)
		{
			charge = _minimunCharge_;
		}


		if (_DropEffect.isPlaying == true)
		{
			_DropEffect.Stop();
		}

		StartCoroutine(delayForce(charge, 1));

	}


	void Launch ( float charge ) {
		_CamHandler._HedgeCam.ApplyCameraShake((ReleaseShakeAmmount * charge) / 100, 40);
		_Sounds.SpinDashReleaseSound();

		_PlayerPhys.AlignToGround(_PlayerPhys._groundNormal, true);

		Vector3 newVec = charge *  newForward;

		_Actions.Action00.Curl();
		_Actions.Action00.SetIsRolling(true);
		_Actions.Action00._rollCounter = 0.005f;


		Vector3 releVec = _PlayerPhys.GetRelevantVel(newVec);
		float newSpeedMagnitude = new Vector3(releVec.x, 0f, releVec.z).magnitude;

		Debug.DrawRay(transform.position, newVec.normalized * 30, Color.red * 2, 20f);

		if (newSpeedMagnitude > _PlayerPhys._horizontalSpeedMagnitude)
		{
			_PlayerPhys.SetCoreVelocity( newVec);

			_CamHandler._HedgeCam.ChangeHeight(18, 25f);
		}
		else
		{
			_PlayerPhys.SetCoreVelocity(newVec.normalized * (_PlayerPhys._horizontalSpeedMagnitude + (charge * 0.45f)));
			_CamHandler._HedgeCam.ChangeHeight(20, 15f);
		}
	}

	IEnumerator delayForce ( float charge, int delay ) {
		for (int i = 1 ; i <= delay ; i++)
		{
			yield return new WaitForFixedUpdate();
		}

		Launch(charge);
	}

	public float externalDash () {
		_CamHandler._HedgeCam.ApplyCameraShake((ReleaseShakeAmmount * charge) / 100, 30);
		_Sounds.SpinDashReleaseSound();
		return charge;
	}

	public void ResetSpinDashVariables () {
		if (_DropEffect.isPlaying == true)
		{
			_DropEffect.Stop();
		}
		for (int i = 0 ; i < PlayerSkin.Length ; i++)
		{
			PlayerSkin[i].enabled = true;
		}
		//SpinDashBall.SetActive(false);
		charge = 0;
	}
}
