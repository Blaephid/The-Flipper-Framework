using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;


public class S_Interaction_Objects : MonoBehaviour
{

	S_CharacterTools Tools;

	[Header("For Rings, Springs and so on")]

	S_PlayerPhysics Player;
	S_HedgeCamera _CamHandler;
	Animator CharacterAnimator;
	S_Control_PlayerSound Sounds;
	S_ActionManager _Actions;
	S_PlayerInput _Input;
	S_Handler_CharacterAttacks attack;

	GameObject JumpBall;

	S_Data_Spring spring;
	int springAmm;


	public GameObject RingCollectParticle;
	public Material SpeedPadTrack;
	public Material DashRingMaterial;

	[Header("Enemies")]


	[HideInInspector] public float _homingBouncingPower_;

	public bool updateTargets { get; set; }

	[HideInInspector] public float _enemyDamageShakeAmmount_;
	[HideInInspector] public float _enemyHitShakeAmmount_;

	[Header("UI objects")]

	public TextMeshProUGUI RingsCounter;
	public TextMeshProUGUI SpeedCounter;
	public S_HintBox HintBox;


	public static int RingAmount { get; set; }
	[HideInInspector] public int CurrentRings;
	[HideInInspector] public float DisplaySpeed;

	S_Control_MovingPlatform Platform;
	Vector3 TranslateOnPlatform;
	public Color DashRingLightsColor;

	private void Awake () {
		if (Player == null)
		{
			Tools = GetComponent<S_CharacterTools>();
			AssignTools();

			AssignStats();
		}

	}

	//Displays rings and speed on UI
	private void LateUpdate () {
		UpdateSpeed();

		CurrentRings = RingAmount;
		RingsCounter.text = ": " + RingAmount;
	}

	void UpdateSpeed () {
		if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Default)
		{
			//DisplaySpeed = Player.SpeedMagnitude;
			DisplaySpeed = Player._horizontalSpeedMagnitude;
		}

		else if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Rail)
		{
			DisplaySpeed = _Actions.Action05._playerSpeed;
		}
		else if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.WallRunning)
		{
			if (_Actions.Action12._runningSpeed > _Actions.Action12._climbingSpeed)
				DisplaySpeed = _Actions.Action12._runningSpeed;
			else
				DisplaySpeed = _Actions.Action12._climbingSpeed;

		}
		else if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Path)
		{
			DisplaySpeed = _Actions.Action10._playerSpeed;
		}
		else
		{
			DisplaySpeed = Player._horizontalSpeedMagnitude;
		}

		if (SpeedCounter != null && Player._speedMagnitude > 10f) SpeedCounter.text = DisplaySpeed.ToString("F0");
		else if (SpeedCounter != null && DisplaySpeed < 10f) SpeedCounter.text = "0";
	}

	void Update () {

		//if (updateTargets)
		//{
		//	//HomingAttackControl.UpdateHomingTargets();
		//	if (Actions.Action02 != null)
		//	{
		//		if (Actions.Action02 != null)
		//		{
		//			Actions._isHomingAvailable = true;
		//		}
		//	}
		//	updateTargets = false;
		//}



		//Set speed pad trackpad's offset
		SpeedPadTrack.SetTextureOffset("_MainTex", new Vector2(0, -Time.time) * 3);
		DashRingMaterial.SetColor("_EmissionColor", (Mathf.Sin(Time.time * 15) * 1.3f) * DashRingLightsColor);
	}



	void FixedUpdate () {
		if (Platform != null)
		{
			transform.position += (-Platform.TranslateVector);
		}
		if (!Player._isGrounded)
		{
			Platform = null;
		}


	}

	IEnumerator setRailSpeed ( float speed, bool set, float addSpeed, bool backwards ) {
		for (int i = 0 ; i < 3 ; i++)
		{
			yield return new WaitForFixedUpdate();

			if (set)
			{
				if (_Actions.Action05._playerSpeed < speed)
				{
					_Actions.Action05._playerSpeed = speed;
					_Actions.Action05._isBoosted = true;
					_Actions.Action05._boostTime = 0.7f;

				}
				else
					set = false;

			}
			else if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Rail)
			{

				_Actions.Action05._playerSpeed += addSpeed / 2;
				_Actions.Action05._isBoosted = true;
				_Actions.Action05._boostTime = 0.7f;

				i = 3;

			}

			if (backwards)
				_Actions.Action05._isGoingBackwards = true;
			else
				_Actions.Action05._isGoingBackwards = false;
		}

	}

	public void OnTriggerEnter ( Collider col ) {
		//Speed Pads Collision
		if (col.tag == "SpeedPad")
		{
			S_Data_SpeedPad pad = col.GetComponent<S_Data_SpeedPad>();

			col.GetComponent<AudioSource>().Play();

			JumpBall.SetActive(false);
			if (_Actions.Action08 != null)
			{
				if (_Actions.Action08._DropEffect.isPlaying == true)
				{
					_Actions.Action08._DropEffect.Stop();
				}
			}

			if (pad.onRail)
			{

				if (_Actions.whatAction != S_Enums.PrimaryPlayerStates.Rail)
				{
					transform.position = col.GetComponent<S_Data_SpeedPad>().positionToLockTo.position;
				}
				else
				{
					StartCoroutine(setRailSpeed(pad.Speed, pad.setSpeed, pad.addSpeed, pad.railBackwards));
				}
				return;
			}


			else if (!col.GetComponent<S_Data_SpeedPad>().path)
			{

				_Actions._isAirDashAvailables = true;

				transform.rotation = Quaternion.identity;
				//ResetPlayerRotation

				Vector3 lockpos;
				if (col.GetComponent<S_Data_SpeedPad>().positionToLockTo != null)
					lockpos = col.GetComponent<S_Data_SpeedPad>().positionToLockTo.position;
				else
					lockpos = col.transform.position;


				if (pad.LockToDirection)
				{
					float speed = col.GetComponent<S_Data_SpeedPad>().Speed;
					if (speed < Player._horizontalSpeedMagnitude)
						speed = Player._horizontalSpeedMagnitude;

					if (!pad.isDashRing)
						StartCoroutine(applyForce(col.transform.forward * speed, lockpos, 1));
					else
						StartCoroutine(applyForce(col.transform.forward * col.GetComponent<S_Data_SpeedPad>().Speed, lockpos));
				}
				else
				{
					Player.AddCoreVelocity(col.transform.forward * col.GetComponent<S_Data_SpeedPad>().Speed);

					if (col.GetComponent<S_Data_SpeedPad>().Snap)
					{
						transform.position = lockpos;
					}
				}


				if (pad.isDashRing)
				{

					_Actions.Action00.CancelCoyote();
					_Actions.Action00.StartAction();
					CharacterAnimator.SetBool("Grounded", false);

					if (pad.lockAirMoves)
					{
						StopCoroutine(_Actions.lockAirMoves(pad.lockAirMovesTime));
						StartCoroutine(_Actions.lockAirMoves(pad.lockAirMovesTime));
					}

					if (pad.lockGravity != Vector3.zero)
					{
						StartCoroutine(lockGravity(pad.lockGravity));
					}



				}
				else
				{
					transform.up = col.transform.up;
					CharacterAnimator.transform.forward = col.transform.forward;
				}

				if (pad.LockControl)
				{
					_Input.LockInputForAWhile(pad.LockControlTime, true);
					if (pad.setInputForwards)
					{
						_Input.moveX = 0;
						_Input.moveY = 1;
					}
				}
				if (pad.AffectCamera)
				{
					Vector3 dir = col.transform.forward;
					_CamHandler.SetCamera(dir, 2.5f, 20, 5f, 1);

				}

			}
		}

		//Rings Collision
		if (col.tag == "Ring")
		{
			Instantiate(RingCollectParticle, col.transform.position, Quaternion.identity);
			Destroy(col.gameObject);
			StartCoroutine(IncreaseRing());


		}
		else if (col.tag == "Ring Road")
		{
			//Actions.Action07Control.UpdateHomingTargets();
			Instantiate(RingCollectParticle, col.transform.position, Quaternion.identity);
			Destroy(col.gameObject);
			StartCoroutine(IncreaseRing());
		}
		else if (col.tag == "MovingRing")
		{
			if (col.GetComponent<S_MovingRing>() != null)
			{
				if (col.GetComponent<S_MovingRing>().colectable)
				{
					StartCoroutine(IncreaseRing());
					Instantiate(RingCollectParticle, col.transform.position, Quaternion.identity);
					Destroy(col.gameObject);
				}
			}
		}

		//Switch
		if (col.tag == "Switch")
		{
			if (col.GetComponent<S_Data_Switch>() != null)
			{
				col.GetComponent<S_Data_Switch>().Activate();
			}
		}

		//Hazard
		if (col.tag == "Hazard")
		{
			JumpBall.SetActive(false);
			if (_Actions.Action08 != null)
			{
				if (_Actions.Action08._DropEffect.isPlaying == true)
				{
					_Actions.Action08._DropEffect.Stop();
				}
			}
			DamagePlayer();
			_CamHandler.ApplyCameraShake(_enemyDamageShakeAmmount_, 60);
		}

		//Enemies
		if (col.tag == "Enemy")
		{
			_CamHandler.ApplyCameraShake(_enemyHitShakeAmmount_, 30);
			//Either triggers an attack on the enemy or takes damage.
			if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.SpinCharge || (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Default && Player._isRolling))
			{
				attack.AttackThing(col, "SpinDash", "Enemy"); ;

			}
			//If in the rolling or jumpdash animation.
			if (CharacterAnimator.GetInteger("Action") == 1 || CharacterAnimator.GetInteger("Action") == 11)
			{
				attack.AttackThing(col, "SpinJump", "Enemy");
			}
			else
			{
				DamagePlayer();
			}
		}

		////Monitors
		if (col.tag == "Monitor")
		{
			if (CharacterAnimator.GetInteger("Action") == 1)
			{
				col.GetComponentInChildren<BoxCollider>().enabled = false;

				attack.AttackThing(col, "SpinJump", "Monitor");
			}


		}


		//Spring Collision

		if (col.tag == "Spring")
		{
			_Actions.Action00.CancelCoyote();
			Player._isGravityOn = true;

			JumpBall.SetActive(false);
			if (_Actions.Action08 != null)
			{
				if (_Actions.Action08._DropEffect.isPlaying == true)
				{
					_Actions.Action08._DropEffect.Stop();
				}
			}


			if (col.GetComponent<S_Data_Spring>() != null)
			{

				spring = col.GetComponent<S_Data_Spring>();

				if (spring.LockControl)
				{
					_Input.LockInputForAWhile(spring.LockTime, false);
				}

				if (spring.lockAirMoves)
				{
					StopCoroutine(_Actions.lockAirMoves(spring.lockAirMovesTime));
					StartCoroutine(_Actions.lockAirMoves(spring.lockAirMovesTime));
				}

				if (spring.lockGravity != Vector3.zero)
				{
					StartCoroutine(lockGravity(spring.lockGravity));
				}

				_Actions.Action00.StartAction();


				if (col.GetComponent<AudioSource>()) { col.GetComponent<AudioSource>().Play(); }
				CharacterAnimator.SetBool("Grounded", false);

				if (_Actions.Action02 != null)
				{
					_Actions._isAirDashAvailables = true;
				}

				if (spring.anim != null)
					spring.anim.SetTrigger("Hit");

				if (spring.IsAdditive)
				{
					Vector3 newVelocity = new Vector3(Player._RB.velocity.x, 0f, Player._RB.velocity.z);
					newVelocity = (newVelocity * 0.8f) + (spring.transform.up * spring.SpringForce);
					StartCoroutine(applyForce(newVelocity, spring.BounceCenter.position));
				}


				else
				{
					StartCoroutine(applyForce(spring.transform.up * spring.SpringForce, spring.BounceCenter.position));
				}

				transform.position = spring.BounceCenter.position;

			}
		}

		else if (col.tag == "Bumper")
		{
			if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Homing || _Actions.whatPreviousAction == S_Enums.PrimaryPlayerStates.Homing)

			JumpBall.SetActive(false);
			if (_Actions.Action08 != null)
			{
				if (_Actions.Action08._DropEffect.isPlaying == true)
				{
					_Actions.Action08._DropEffect.Stop();
				}
			}

		}

		//CancelHoming
		else if (col.tag == "CancelHoming")
		{
			if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Homing || _Actions.whatPreviousAction == S_Enums.PrimaryPlayerStates.Homing)
			{

				Vector3 newSpeed = new Vector3(1, 0, 1);

				_Actions.Action00.StartAction();
				newSpeed = new Vector3(0, _homingBouncingPower_, 0);
				Player.SetTotalVelocity(newSpeed);
				Player.transform.position = col.ClosestPoint(Player.transform.position);
				if (_Actions.Action02 != null)
				{
					_Actions._isAirDashAvailables = true;
				}
			}
		}

		else if (col.tag == "Wind")
		{
			if (col.GetComponent<S_Trigger_Updraft>())
			{
				if (_Actions.whatAction == S_Enums.PrimaryPlayerStates.Hovering)
				{
					_Actions.Action13.updateHover(col.GetComponent<S_Trigger_Updraft>());
				}
				else
				{
					_Actions.Action13.InitialEvents(col.GetComponent<S_Trigger_Updraft>());
					_Actions.ChangeAction(S_Enums.PrimaryPlayerStates.Hovering);
				}
			}

		}

		else if (col.tag == "HintRing")
		{
			S_Data_HintRing hintRing = col.GetComponent<S_Data_HintRing>();
			//if (!HintBox.IsShowing)
			//{
			//    HintBox.ShowHint(hintRing.hintText, hintRing.hintDuration);
			//    hintRing.hintSound.Play();
			//}

			if (col.gameObject != HintBox.currentHint)
			{
				HintBox.currentHint = col.gameObject;
				hintRing.hintSound.Play();


				if (_Input.usingMouse)
				{
					Debug.Log("SHOWHINT with = " + hintRing.hintText[0]);
					HintBox.ShowHint(hintRing.hintText, hintRing.hintDuration, col.gameObject);
				}

				else
				{
					Gamepad input = Gamepad.current;
					Debug.Log(input);

					switch (input)
					{
						case (null):
							HintBox.ShowHint(hintRing.hintText, hintRing.hintDuration, col.gameObject);
							break;
						case (SwitchProControllerHID):
							HintBox.ShowHint(hintRing.hintTextGamePad, hintRing.hintDuration, col.gameObject);
							break;
						case (DualSenseGamepadHID):
							HintBox.ShowHint(hintRing.hintTextPS4, hintRing.hintDuration, col.gameObject);
							break;
						case (DualShock3GamepadHID):
							HintBox.ShowHint(hintRing.hintTextPS4, hintRing.hintDuration, col.gameObject);
							break;
						case (DualShock4GamepadHID):
							HintBox.ShowHint(hintRing.hintTextPS4, hintRing.hintDuration, col.gameObject);
							break;
						case (DualShockGamepad):
							HintBox.ShowHint(hintRing.hintTextPS4, hintRing.hintDuration, col.gameObject);
							break;
						case (XInputController):
							HintBox.ShowHint(hintRing.hintTextXbox, hintRing.hintDuration, col.gameObject);
							break;

					}
				}


				if (_Actions.eventMan != null)
				{
					foreach (GameObject HR in _Actions.eventMan.hintRings)
					{
						if (col.gameObject == HR)
							return;
					}
					_Actions.eventMan.hintRings.Add(col.gameObject);
					_Actions.eventMan.hintRingsHit += 1;
				}

			}

		}

	}

	private void OnTriggerExit ( Collider col ) {
		if (col.tag == "Wind")
		{
			_Actions.Action13.inWind = false;
		}
	}

	public void OnTriggerStay ( Collider col ) {
		//Hazard
		if (col.tag == "Hazard")
		{
			DamagePlayer();
		}

		if (col.gameObject.tag == "MovingPlatform")
		{
			Platform = col.gameObject.GetComponent<S_Control_MovingPlatform>();
		}
		else
		{
			Platform = null;
		}



	}

	private IEnumerator IncreaseRing () {
		int ThisFramesRingCount = RingAmount;
		RingAmount++;
		yield return new WaitForEndOfFrame();
		if (RingAmount > ThisFramesRingCount + 1)
		{
			RingAmount--;
		}

	}

	private IEnumerator lockGravity ( Vector3 newGrav ) {

		Player._currentFallGravity = newGrav;
		yield return new WaitForSeconds(0.2f);
		while (true)
		{
			yield return new WaitForFixedUpdate();
			if (Player._isGrounded)
				break;
		}

		Player._currentFallGravity = Player._startFallGravity_;
	}

	private IEnumerator applyForce ( Vector3 force, Vector3 position, int frames = 3 ) {

		for (int i = 0 ; i < frames ; i++)
		{
			transform.position = position;
			Player._RB.velocity = Vector3.zero;
			yield return new WaitForFixedUpdate();
		}

		_Actions.Action00.StartAction();
		transform.position = position;
		Player._RB.velocity = force;

	}
	public void DamagePlayer () {
		if (!_Actions.Action04Control.IsHurt && _Actions.whatAction != S_Enums.PrimaryPlayerStates.Hurt)
		{

			if (!S_Interaction_Monitors.HasShield)
			{
				if (RingAmount > 0)
				{
					//LoseRings
					Sounds.RingLossSound();
					_Actions.Action04Control.GetHurt();
					_Actions.Action04.AttemptAction();
				}
				if (RingAmount <= 0)
				{
					//Die
					if (!_Actions.Action04Control.isDead)
					{
						Sounds.DieSound();
						//_Actions.Action04Control.isDead = true;
						_Actions.Action04.AttemptAction();
					}
				}
			}
			if (S_Interaction_Monitors.HasShield)
			{
				//Lose Shield
				Sounds.SpikedSound();
				S_Interaction_Monitors.HasShield = false;
				_Actions.Action04.AttemptAction();
			}
		}
	}


	private void AssignStats () {
		_homingBouncingPower_ = Tools.Stats.EnemyInteraction.homingBouncingPower;
		_enemyDamageShakeAmmount_ = Tools.Stats.EnemyInteraction.enemyDamageShakeAmmount;
		_enemyHitShakeAmmount_ = Tools.Stats.EnemyInteraction.enemyHitShakeAmmount;
	}

	private void AssignTools () {
		Player = GetComponent<S_PlayerPhysics>();
		_CamHandler = GetComponent<S_Handler_Camera>()._HedgeCam;
		_Actions = GetComponent<S_ActionManager>();
		_Input = GetComponent<S_PlayerInput>();
		attack = GetComponent<S_Handler_CharacterAttacks>();

		CharacterAnimator = Tools.CharacterAnimator;
		Sounds = Tools.SoundControl;
		JumpBall = Tools.JumpBall;


	}
}
