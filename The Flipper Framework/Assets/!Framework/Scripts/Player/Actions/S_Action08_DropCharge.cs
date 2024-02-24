using UnityEngine;
using System.Collections;

public class S_Action08_DropCharge : MonoBehaviour
{
	S_CharacterTools Tools;
	S_PlayerInput _Input;

	Animator CharacterAnimator;

	Transform feetPoint;
	public float BallAnimationSpeedMultiplier;

	S_Handler_Camera Cam;
	S_ActionManager Actions;
	S_PlayerPhysics Player;
	S_Control_PlayerSound sounds;
	[HideInInspector] public ParticleSystem DropEffect;
	GameObject JumpBall;
	public float SpinDashChargedEffectAmm;

	public bool DropDashAvailable { get; set; }

	SkinnedMeshRenderer[] PlayerSkin;


	[HideInInspector] public float _spinDashChargingSpeed_ = 0.3f;
	[HideInInspector] public float _minimunCharge_ = 10;
	[HideInInspector] public float _maximunCharge_ = 100;
	bool Charging = true;

	[HideInInspector] public float charge;
	bool isSpinDashing;
	Vector3 RawPrevInput;
	Quaternion CharRot;
	RaycastHit floorHit;
	Vector3 newForward;

	public float ReleaseShakeAmmount;

	void Awake () {
		if (Player == null)
		{
			Tools = GetComponent<S_CharacterTools>();
			AssignTools();

			AssignStats();
		}
	}


	public void TryDropCharge () {
		if (Player._RB.velocity.y < 40f && Actions.Action08 != null)
		{
			//Debug.Log("Enter DropDash");
			Actions.ChangeAction(S_Enums.PlayerStates.DropCharge);

			Actions.Action08.InitialEvents();
		}
	}

	public void InitialEvents ( float newCharge = 15 ) {
		////Debug.Log ("startDropDash");
		CharacterAnimator.SetInteger("Action", 1);
		CharacterAnimator.SetBool("Grounded", false);

		sounds.SpinDashSound();
		charge = newCharge;
		Charging = true;
	}

	void FixedUpdate () {
		if (Charging)
		{
			charge += _spinDashChargingSpeed_;

			//Lock camera on behind
			// Cam.Cam.FollowDirection(3, 14f, -10,0);


			if (DropEffect.isPlaying == false)
			{
				DropEffect.Play();
			}

			// Player.rigidbody.velocity /= SpinDashStillForce;

			if (!_Input.RollPressed)
			{
				//if (DropEffect.isPlaying == true)
				//{
				//    DropEffect.Stop();
				//}

				sounds.Source2.Stop();


				Charging = false;
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
				Charging = true;
				StopCoroutine(exitAction());
			}
		}

		if (Physics.Raycast(feetPoint.position, -transform.up, out floorHit, 1.3f, Player._Groundmask_) || Vector3.Dot(Player._groundNormal, Vector3.up) > 0.99)
		{

			if (!_Input.JumpPressed)
				Release();
			else
			{
				Player._isRolling = true;
				JumpBall.SetActive(false);
				if (DropEffect.isPlaying == true)
				{
					DropEffect.Stop();
				}
			}

			_Input.JumpPressed = false;
			JumpBall.SetActive(false);
			Actions.ChangeAction(S_Enums.PlayerStates.Regular);
		}

		else if (_Input.SpecialPressed && charge > _minimunCharge_)
		{
			AirRelease();
		}
	}

	IEnumerator exitAction () {
		yield return new WaitForSeconds(0.7f);
		if (DropEffect.isPlaying == true)
		{
			DropEffect.Stop();
		}
		if (Actions.whatAction == S_Enums.PlayerStates.DropCharge)
		{
			JumpBall.SetActive(true);
			Actions.ChangeAction(S_Enums.PlayerStates.Jump);
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
		Charging = false;
	}

	IEnumerator airDash () {
		float time = Mathf.Round(charge / 30);
		time /= 10;

		Debug.Log(time);

		Player._isGravityOn = false;
		yield return new WaitForSeconds(time);
		Player._isGravityOn = true;
		JumpBall.SetActive(false);
		Actions.ChangeAction(S_Enums.PlayerStates.Jump);

	}

	void Release () {

		if (Actions.eventMan != null) Actions.eventMan.dropChargesPerformed += 1;

		JumpBall.SetActive(false);

		DropDashAvailable = false;

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


		if (DropEffect.isPlaying == true)
		{
			DropEffect.Stop();
		}

		StartCoroutine(delayForce(charge, 1));

	}


	void Launch ( float charge ) {
		Cam._HedgeCam.ApplyCameraShake((ReleaseShakeAmmount * charge) / 100, 40);
		sounds.SpinDashReleaseSound();

		Player.AlignToGround(Player._groundNormal, true);

		Vector3 newVec = charge *  newForward;

		Actions.Action00.Curl();
		Player._isRolling = true;
		Actions.Action00.rollCounter = 0.005f;


		Vector3 releVec = Player.GetRelevantVec(newVec);
		float newSpeedMagnitude = new Vector3(releVec.x, 0f, releVec.z).magnitude;

		Debug.DrawRay(transform.position, newVec.normalized * 30, Color.red * 2, 20f);

		if (newSpeedMagnitude > Player._horizontalSpeedMagnitude)
		{
			Player.SetCoreVelocity( newVec);

			Cam._HedgeCam.ChangeHeight(18, 25f);
		}
		else
		{
			Player.SetCoreVelocity(newVec.normalized * (Player._horizontalSpeedMagnitude + (charge * 0.45f)));
			Cam._HedgeCam.ChangeHeight(20, 15f);
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
		Cam._HedgeCam.ApplyCameraShake((ReleaseShakeAmmount * charge) / 100, 30);
		sounds.SpinDashReleaseSound();
		return charge;
	}

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
			Vector3 VelocityMod = new Vector3(Player._RB.velocity.x, 0, Player._RB.velocity.z);
			if (VelocityMod != Vector3.zero)
			{
				Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
				CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * 200);
			}
		}

		if (Player._isGrounded && DropEffect.isPlaying)
		{
			DropEffect.Stop();
		}

	}

	private void OnDisable () {
		DropEffect.Stop();
	}

	public void ResetSpinDashVariables () {
		if (DropEffect.isPlaying == true)
		{
			DropEffect.Stop();
		}
		for (int i = 0 ; i < PlayerSkin.Length ; i++)
		{
			PlayerSkin[i].enabled = true;
		}
		//SpinDashBall.SetActive(false);
		charge = 0;
	}

	private void AssignStats () {
		_spinDashChargingSpeed_ = Tools.Stats.DropChargeStats.chargingSpeed;
		_minimunCharge_ = Tools.Stats.DropChargeStats.minimunCharge;
		_maximunCharge_ = Tools.Stats.DropChargeStats.maximunCharge;
	}

	private void AssignTools () {
		Player = GetComponent<S_PlayerPhysics>();
		Actions = GetComponent<S_ActionManager>();
		Cam = GetComponent<S_Handler_Camera>();
		_Input = GetComponent<S_PlayerInput>();

		CharacterAnimator = Tools.CharacterAnimator;
		sounds = Tools.SoundControl;
		DropEffect = Tools.DropEffect;

		feetPoint = Tools.FeetPoint;
		JumpBall = Tools.JumpBall;
		PlayerSkin = Tools.PlayerSkin;


	}
}
