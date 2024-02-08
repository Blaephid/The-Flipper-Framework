using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XR;


public class S_Interaction_Objects : MonoBehaviour {

    S_CharacterTools Tools;

    [Header("For Rings, Springs and so on")]

    S_PlayerPhysics Player;
    S_HedgeCamera Cam;
    Animator CharacterAnimator;
    S_Control_SoundsPlayer Sounds;
    S_ActionManager Actions;
    S_PlayerInput Inp;
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

    private void Awake()
    {
        if (Player == null)
        {
            Tools = GetComponent<S_CharacterTools>();
            AssignTools();

            AssignStats();
        }

    }

    //Displays rings and speed on UI
    private void LateUpdate()
    {
        UpdateSpeed();

        CurrentRings = RingAmount;
        RingsCounter.text = ": " + RingAmount;
    }

    void UpdateSpeed()
    {
        if (Actions.whatAction == S_Enums.PlayerStates.Regular)
        {
            //DisplaySpeed = Player.SpeedMagnitude;
            DisplaySpeed = Player.HorizontalSpeedMagnitude;
        }
        
        else if(Actions.whatAction == S_Enums.PlayerStates.Rail)
        {
            DisplaySpeed = Actions.Action05.PlayerSpeed;
        }
        else if (Actions.whatAction == S_Enums.PlayerStates.WallRunning)
        {
            if (Actions.Action12.RunningSpeed > Actions.Action12.ClimbingSpeed)
            DisplaySpeed = Actions.Action12.RunningSpeed;
            else
                DisplaySpeed = Actions.Action12.ClimbingSpeed;

        }
        else if (Actions.whatAction == S_Enums.PlayerStates.Path)
        {
            DisplaySpeed = Actions.Action10.PlayerSpeed;
        }
        else
        {
             DisplaySpeed = Player.HorizontalSpeedMagnitude;           
        }

        if (SpeedCounter != null && Player.SpeedMagnitude > 10f) SpeedCounter.text = DisplaySpeed.ToString("F0");
        else if (SpeedCounter != null && DisplaySpeed < 10f) SpeedCounter.text = "0"; 
    }

    void Update()
    {

        if (updateTargets)
        {
            //HomingAttackControl.UpdateHomingTargets();
			if (Actions.Action02 != null) {
				if (Actions.Action02 != null) {
			Actions.Action02.HomingAvailable = true;
		}
			}
            updateTargets = false;
        }



        //Set speed pad trackpad's offset
        SpeedPadTrack.SetTextureOffset("_MainTex", new Vector2(0, -Time.time) * 3);
        DashRingMaterial.SetColor("_EmissionColor", (Mathf.Sin(Time.time * 15) * 1.3f) * DashRingLightsColor);
    }

	

    void FixedUpdate()
    {
        if(Platform != null)
        {
            transform.position += (-Platform.TranslateVector);
        }
        if (!Player.Grounded)
        {
            Platform = null;
        }


    }

    IEnumerator setRailSpeed(float speed, bool set, float addSpeed, bool backwards)
    {
        for(int i = 0; i < 3; i++)
        {
            yield return new WaitForFixedUpdate();

            if(set)
            {
                if (Actions.Action05.PlayerSpeed < speed)
                {
                    Actions.Action05.PlayerSpeed = speed;
                    Actions.Action05.Boosted = true;
                    Actions.Action05.boostTime = 0.7f;

                }
                else
                    set = false;

            }
            else if (Actions.whatAction == S_Enums.PlayerStates.Rail)
            {

                Actions.Action05.PlayerSpeed += addSpeed / 2;
                Actions.Action05.Boosted = true;
                Actions.Action05.boostTime = 0.7f;

                i = 3;
                
            }

            if (backwards)
                Actions.Action05.backwards = true;
            else
                Actions.Action05.backwards = false;
        }

    }

    public void OnTriggerEnter(Collider col)
    {
        //Speed Pads Collision
        if(col.tag == "SpeedPad")
        {
            S_Data_SpeedPad pad = col.GetComponent<S_Data_SpeedPad>();

            col.GetComponent<AudioSource>().Play();

            JumpBall.SetActive(false);
            if (Actions.Action08 != null)
            {
                if (Actions.Action08.DropEffect.isPlaying == true)
                {
                    Actions.Action08.DropEffect.Stop();
                }
            }

            if (pad.onRail)
             {

                if (Actions.whatAction != S_Enums.PlayerStates.Rail)
                {
                    transform.position = col.GetComponent<S_Data_SpeedPad>().positionToLockTo.position;
                }
                else
                {
                    StartCoroutine(setRailSpeed(pad.Speed, pad.setSpeed, pad.addSpeed, pad.railBackwards));
                }        
                return;
            }
                             

            else if(!col.GetComponent<S_Data_SpeedPad>().path)
            {
                
                Actions.Action02.HomingAvailable = true;

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
                    if (speed < Player.HorizontalSpeedMagnitude)
                        speed = Player.HorizontalSpeedMagnitude;

                    if(!pad.isDashRing)
                        StartCoroutine(applyForce(col.transform.forward * speed, lockpos, 1));
                    else
                        StartCoroutine(applyForce(col.transform.forward * col.GetComponent<S_Data_SpeedPad>().Speed, lockpos));
                }
                else
                {
                    Player.AddVelocity(col.transform.forward * col.GetComponent<S_Data_SpeedPad>().Speed);

                    if (col.GetComponent<S_Data_SpeedPad>().Snap)
                    {
                        transform.position = lockpos;
                    }
                }

               
                if (pad.isDashRing)
                {
                                  
                    Actions.Action00.cancelCoyote();
                    Actions.ChangeAction(S_Enums.PlayerStates.Regular);
                    CharacterAnimator.SetBool("Grounded", false);
                    CharacterAnimator.SetInteger("Action", 0);

                    if (pad.lockAirMoves)
                    {
                        StopCoroutine(Actions.lockAirMoves(pad.lockAirMovesTime));
                        StartCoroutine(Actions.lockAirMoves(pad.lockAirMovesTime));
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
                    Inp.LockInputForAWhile(pad.LockControlTime, true);
                    if(pad.setInputForwards)
                    {
                        Actions.moveX = 0;
                        Actions.moveY = 1;
                    }
                }
                if (pad.AffectCamera)
                {
                    Vector3 dir = col.transform.forward;
                    Cam.SetCamera(dir, 2.5f, 20, 5f, 1);
                    
                }

            }
        }

        //Rings Collision
        if (col.tag == "Ring")
        {
			Instantiate(RingCollectParticle, col.transform.position, Quaternion.identity);
			Destroy(col.gameObject);
			StartCoroutine(IncreaseRing ());
            
            
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
					StartCoroutine(IncreaseRing ());
                    Instantiate(RingCollectParticle, col.transform.position, Quaternion.identity);
                    Destroy(col.gameObject);
                }
            }
        }

		//Switch
		if(col.tag == "Switch")
		{	
			if (col.GetComponent<S_Data_Switch> () != null) {
				col.GetComponent<S_Data_Switch> ().Activate ();
			}
		}

        //Hazard
        if(col.tag == "Hazard")
        {
			JumpBall.SetActive(false);
			if (Actions.Action08 != null) {
				if (Actions.Action08.DropEffect.isPlaying == true) {
					Actions.Action08.DropEffect.Stop ();
				}
			}
            DamagePlayer();
            S_HedgeCamera.Shakeforce = _enemyDamageShakeAmmount_;
        }

        //Enemies
        if (col.tag == "Enemy")
        {
            S_HedgeCamera.Shakeforce = _enemyHitShakeAmmount_;
        //Either triggers an attack on the enemy or takes damage.
            if (Actions.whatAction == S_Enums.PlayerStates.SpinCharge || (Actions.whatAction == S_Enums.PlayerStates.Regular && Player.isRolling))
            {
                attack.AttackThing(col, "SpinDash", "Enemy"); ;
                
            }
            //If in the rolling or jumpdash animation.
            if (CharacterAnimator.GetInteger("Action") == 1 || CharacterAnimator.GetInteger("Action") == 11)
            {
                Debug.Log("Rolling");
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
            Actions.Action00.cancelCoyote();
            Player.GravityAffects = true;

            if (Actions.whatAction == S_Enums.PlayerStates.Homing || Actions.whatPreviousAction == S_Enums.PlayerStates.Homing)
                Player._homingDelay_ = Tools.Stats.HomingStats.successDelay;

            JumpBall.SetActive(false);
			if (Actions.Action08 != null) {
				if (Actions.Action08.DropEffect.isPlaying == true) {
					Actions.Action08.DropEffect.Stop ();
				}
			}


            if (col.GetComponent<S_Data_Spring>() != null)
            {

                spring = col.GetComponent<S_Data_Spring>();

                if (spring.LockControl)
                {
                    Inp.LockInputForAWhile(spring.LockTime, false);
                }

                if(spring.lockAirMoves)
                {
                    StopCoroutine(Actions.lockAirMoves(spring.lockAirMovesTime));
                    StartCoroutine(Actions.lockAirMoves(spring.lockAirMovesTime));
                }

                if(spring.lockGravity != Vector3.zero)
                {
                    StartCoroutine(lockGravity(spring.lockGravity));
                }

                Actions.ChangeAction(S_Enums.PlayerStates.Regular);
                

                if (col.GetComponent<AudioSource>()) { col.GetComponent<AudioSource>().Play(); }
                CharacterAnimator.SetInteger("Action", 0);
                CharacterAnimator.SetBool("Grounded", false);

                if (Actions.Action02 != null) {
				    Actions.Action02.HomingAvailable = true;
			    }

                if (spring.anim != null)
                     spring.anim.SetTrigger("Hit");

                if (spring.IsAdditive)
                {
                    Vector3 newVelocity = new Vector3(Player.rb.velocity.x, 0f, Player.rb.velocity.z);
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
            if (Actions.whatAction == S_Enums.PlayerStates.Homing || Actions.whatPreviousAction == S_Enums.PlayerStates.Homing)
                Player._homingDelay_ = Tools.Stats.HomingStats.successDelay;

            JumpBall.SetActive(false);
			if (Actions.Action08 != null) {
				if (Actions.Action08.DropEffect.isPlaying == true) {
					Actions.Action08.DropEffect.Stop ();
				}
			}
			
		}

		//CancelHoming
		else if (col.tag == "CancelHoming") 
		{
			if (Actions.whatAction == S_Enums.PlayerStates.Homing || Actions.whatPreviousAction == S_Enums.PlayerStates.Homing)
            {

				Vector3 newSpeed = new Vector3(1, 0, 1);

				Actions.ChangeAction (S_Enums.PlayerStates.Regular);
				newSpeed = new Vector3(0, _homingBouncingPower_, 0);
				////Debug.Log (newSpeed);
				Player.rb.velocity = newSpeed;
				Player.transform.position = col.ClosestPoint (Player.transform.position);
				if (Actions.Action02 != null) {
					Actions.Action02.HomingAvailable = true;
				}

                Player._homingDelay_ = Tools.Stats.HomingStats.successDelay;
            }
		}

        else if(col.tag == "Wind")
        {
            if(col.GetComponent<S_Trigger_Updraft>())
            {
                if (Actions.whatAction == S_Enums.PlayerStates.Hovering)
                {
                    Actions.Action13.updateHover(col.GetComponent<S_Trigger_Updraft>());
                }
                else
                {
                    Actions.Action13.InitialEvents(col.GetComponent<S_Trigger_Updraft>());
                    Actions.ChangeAction(S_Enums.PlayerStates.Hovering);
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


                if (Actions.usingMouse)
                {
                    Debug.Log("SHOWHINT with = " +hintRing.hintText[0]);
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


                if (Actions.eventMan != null)
                {
                    foreach (GameObject HR in Actions.eventMan.hintRings)
                    {
                        if (col.gameObject == HR)
                            return;
                    }
                    Actions.eventMan.hintRings.Add(col.gameObject);
                    Actions.eventMan.hintRingsHit += 1;
                }

            }

        }

    }

    private void OnTriggerExit(Collider col)
    {
        if (col.tag == "Wind")
        {
            Actions.Action13.inWind = false;
        }
    }

    public void OnTriggerStay(Collider col)
    {
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

	private IEnumerator IncreaseRing()
	{
		int ThisFramesRingCount = RingAmount;
		RingAmount++;
		yield return new WaitForEndOfFrame ();
		if (RingAmount > ThisFramesRingCount + 1) 
		{
			RingAmount--;
		}
			
	}

    private IEnumerator lockGravity(Vector3 newGrav)
    {

        Player.fallGravity = newGrav;
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            yield return new WaitForFixedUpdate();
            if (Player.Grounded)
                break;
        }

        Player.fallGravity = Player._startFallGravity_;
    }

    private IEnumerator applyForce(Vector3 force, Vector3 position, int frames = 3)
    {
        
        for(int i = 0; i < frames; i++)
        {
            transform.position = position;
            Player.rb.velocity = Vector3.zero;
            yield return new WaitForFixedUpdate();
        }

        Actions.ChangeAction(S_Enums.PlayerStates.Regular);
        transform.position = position;
        Player.rb.velocity = force;

    }
    public void DamagePlayer()
    {
        if (!Actions.Action04Control.IsHurt && Actions.whatAction != S_Enums.PlayerStates.Hurt)
        {

            if (!S_Interaction_Monitors.HasShield)
            {
                if (RingAmount > 0)
                {
                    //LoseRings
                    Sounds.RingLossSound();
                    Actions.Action04Control.GetHurt();
                    Actions.ChangeAction(S_Enums.PlayerStates.Hurt);
                    Actions.Action04.InitialEvents();
                }
                if (RingAmount <= 0)
                {
                    //Die
                    if (!Actions.Action04Control.isDead)
                    {
                        Sounds.DieSound();
                        Actions.Action04Control.isDead = true;
                        Actions.ChangeAction(S_Enums.PlayerStates.Hurt);
                        Actions.Action04.InitialEvents();
                    }
                }
            }
            if (S_Interaction_Monitors.HasShield)
            {
                //Lose Shield
                Sounds.SpikedSound();
                S_Interaction_Monitors.HasShield = false;
                Actions.ChangeAction(S_Enums.PlayerStates.Hurt);
                Actions.Action04.InitialEvents();
            }
        }
    }


    private void AssignStats()
    {
        _homingBouncingPower_ = Tools.Stats.EnemyInteraction.homingBouncingPower;
        _enemyDamageShakeAmmount_ = Tools.Stats.EnemyInteraction.enemyDamageShakeAmmount;
        _enemyHitShakeAmmount_ = Tools.Stats.EnemyInteraction.enemyHitShakeAmmount;
    }

    private void AssignTools()
    {
        Player = GetComponent<S_PlayerPhysics>();
        Cam = GetComponent<S_Handler_Camera>().Cam;
        Actions = GetComponent<S_ActionManager>();
        Inp = GetComponent<S_PlayerInput>();
        attack = GetComponent<S_Handler_CharacterAttacks>();

        CharacterAnimator = Tools.CharacterAnimator;
        Sounds = Tools.SoundControl;
        JumpBall = Tools.JumpBall;


    }
}
